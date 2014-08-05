//#define EmitAllGlyphs
using System;
using System.IO;
using System.Threading;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;

namespace Orvid.Compiler.TrueType.HintingVM
{
	public class IRAssembly
	{
		public readonly string AssemblyName;
		public readonly string ModuleName;
		public readonly bool EmitDebugInfo;
		public readonly TrueTypeFont ParentFont;
		public Dictionary<int, IRMethodBuilder> FpgmFunctions = new Dictionary<int, IRMethodBuilder>();
		public List<IRMethodBuilder> all_FpgmFunctions;
		public IRMethodBuilder FpgmMain;
		public IRMethodBuilder PrepProgram;
		public Dictionary<KeyValuePair<uint, Glyph>, IRMethodBuilder> GlyphHintingPrograms = new Dictionary<KeyValuePair<uint, Glyph>, IRMethodBuilder>();
		public List<ConstructorBuilder> GlyphConstructors = new List<ConstructorBuilder>();
		public Queue<IRMethodBuilder> DelayedFpgmFunctions = new Queue<IRMethodBuilder>();

		public IRAssembly(string outputAssemblyName, bool emitDebugInfo, TrueTypeFont parentFont)
			: this(outputAssemblyName.Substring(0, outputAssemblyName.IndexOf('.')), outputAssemblyName, emitDebugInfo, parentFont)
		{
		}

		public IRAssembly(string assemblyName, string moduleName, bool emitDebugInfo, TrueTypeFont parentFont)
		{
			this.AssemblyName = assemblyName;
			this.ModuleName = moduleName;
			this.EmitDebugInfo = emitDebugInfo;
			this.ParentFont = parentFont;
		}

		#region Read
		public void Read()
		{
			if (ParentFont.TableRead_Fpgm)
			{
				ReadFpgm();
			}
			if (ParentFont.TableRead_Prep)
			{
				ReadPrep();
			}
			else
			{
				throw new Exception("Expected a Prep table!");
			}

			ReadGlyphs();
		}

		#region Read Fpgm
		private void ReadFpgm()
		{
			IRMethodBuilder IRmBldr = new IRMethodBuilder("Function_0", false, this);
			IRmBldr.ReadMethod(new MemoryStream(ParentFont.FpgmProgram));
			this.FpgmMain = IRmBldr;
			List<IRMethodBuilder> funcs = IRAssemblyHelper.SeperateFunctions(IRmBldr);
			// The main Fpgm has it's own optimizations,
			// as there is only 1 that actually does
			// anything for it.
			// This ends up saving about 1kb of IL code
			// (and increasing the speed quite a bit)
			IRAssemblyHelper.FoldFPGMConstants(IRmBldr);
			IRmBldr.Instructions.Add(new Orvid.Compiler.TrueType.HintingVM.Instructions.Return());

			for (int i = 0; i < funcs.Count; i++)
			{
				IROptimizer.Run(funcs[i], true);
			}

			all_FpgmFunctions = funcs;
		}
		#endregion

		#region Read Prep
		private void ReadPrep()
		{
			PrepProgram = new IRMethodBuilder("InitializeState", false, this);
			PrepProgram.ReadMethod(new MemoryStream(ParentFont.PrepProgram));
			IROptimizer.Run(PrepProgram, false);
		}
		#endregion

		#region Read Glyphs
		private void ReadGlyphs()
		{
#if EmitAllGlyphs
			for (uint i = 0; i < ParentFont.Glyphs.Length; i++)
#else
			for (uint i = 0; i < 64; i++)
#endif
			{
				Glyph g = ParentFont.Glyphs[i];
				if (g is SimpleGlyph)
				{
					SimpleGlyph g2 = (SimpleGlyph)g;
					IRMethodBuilder gHintMthd = new IRMethodBuilder("Hint", true, this);
					gHintMthd.ReadMethod(new MemoryStream(g2.Instructions));
					IROptimizer.Run(gHintMthd, false);
					GlyphHintingPrograms.Add(new KeyValuePair<uint, Glyph>(i, g2), gHintMthd);
				}
				else
				{
#warning Need to support Composite Glyphs at some point.
					Console.WriteLine("WARNING: Not reading glyph " + i.ToString() + " because it's not a simple glyph!");
				}
			}
		}
		#endregion

		#endregion

		#region Emit

		private const MethodAttributes PublicStaticMethodAttributes = MethodAttributes.Public | MethodAttributes.Static | MethodAttributes.HideBySig;
		private const MethodAttributes PublicVirtualMethodAttributes = MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Virtual | MethodAttributes.ReuseSlot;
		private const FieldAttributes PublicStaticReadonlyFieldAttributes = FieldAttributes.Public | FieldAttributes.Static | FieldAttributes.InitOnly;
		private const TypeAttributes PublicStaticTypeAttributes = TypeAttributes.Public | TypeAttributes.Sealed | TypeAttributes.Abstract;
		private const TypeAttributes PublicSealedClassTypeAttributes = TypeAttributes.Public | TypeAttributes.Sealed | TypeAttributes.Class;
		private const BindingFlags PrivateInstanceBindingFlags = BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly;

		public void Emit()
		{
			AssemblyBuilder bldr = Thread.GetDomain().DefineDynamicAssembly(new AssemblyName(AssemblyName), AssemblyBuilderAccess.RunAndSave);
			ModuleBuilder modBldr = bldr.DefineDynamicModule(ModuleName, ModuleName, true);

			EmitFpgm(modBldr);
			EmitPrep(modBldr);
			EmitGlyphs(modBldr);

			EmitFontDescriptor(modBldr);
			modBldr.CreateGlobalFunctions();
			bldr.Save(ModuleName);
		}

		#region Emit Fpgm
		private void EmitFpgm(ModuleBuilder bldr)
		{
			if (FpgmMain != null)
			{
				TypeBuilder tBldr = bldr.DefineType("TrueType.Fpgm", PublicStaticTypeAttributes);
				for (int i = 0; i < all_FpgmFunctions.Count; i++)
				{
					all_FpgmFunctions[i].EmitMethod(bldr, tBldr);
				}
				while (DelayedFpgmFunctions.Count > 0)
				{
					IRMethodBuilder mBldr = DelayedFpgmFunctions.Dequeue();
					mBldr.Delayed = false;
					mBldr.EmitMethod(bldr, tBldr);
				}
				FpgmMain.EmitMethod(bldr, tBldr);
				tBldr.CreateType();
			}
		}
		#endregion

		#region Emit Prep
		private void EmitPrep(ModuleBuilder bldr)
		{
			TypeBuilder tBldr = bldr.DefineType("TrueType.Prep", PublicStaticTypeAttributes);
			PrepProgram.EmitMethod(bldr, tBldr);
			tBldr.CreateType();
		}
		#endregion

		#region Emit Glyphs

		private void EmitGlyphs(ModuleBuilder bldr)
		{
			foreach (KeyValuePair<KeyValuePair<uint, Glyph>, IRMethodBuilder> mPair in GlyphHintingPrograms)
			{
				Glyph g = mPair.Key.Value;
				if (g is SimpleGlyph)
				{
					SimpleGlyph g2 = (SimpleGlyph)g;
					TypeBuilder tBldr = bldr.DefineType("TrueType.Glyphs.Glyph_" + mPair.Key.Key.ToString(), PublicSealedClassTypeAttributes, typeof(Orvid.TrueType.Glyph));
					//tBldr.SetParent(typeof(Orvid.TrueType.Glyph));
					GlyphConstructors.Add(EmitSimpleGlyphInit(g2, tBldr, mPair.Key.Key));
					mPair.Value.EmitMethod(bldr, tBldr);
					tBldr.CreateType();
				}
			}
		}

		#region Emit SimpleGlyph Init
		private const MethodAttributes PublicSpecialRTMethodAttributes = MethodAttributes.Public | MethodAttributes.RTSpecialName | MethodAttributes.SpecialName;
		private const CallingConventions StandardHasThisCallingConvention = CallingConventions.HasThis | CallingConventions.Standard;

		private static readonly ConstructorInfo TrueType_Vec2_ctor_int_int = typeof(Orvid.TrueType.Vec2).GetConstructor(new Type[] { typeof(int), typeof(int) });
		private static readonly ConstructorInfo BaseGlyph_Ctor = typeof(Orvid.TrueType.Glyph).GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic)[0];
		private static readonly FieldInfo BaseGlyph_Contour = typeof(Orvid.TrueType.Glyph).GetField("GlyphContour");
		private static readonly FieldInfo BaseGlyph_PointsOnCurve = typeof(Orvid.TrueType.Glyph).GetField("PointsOnCurve");
		private static readonly FieldInfo BaseGlyph_EndPointsOfCountours = typeof(Orvid.TrueType.Glyph).GetField("EndPointsOfCountours");

		private static readonly FieldInfo BaseGlyph_Ascender = typeof(Orvid.TrueType.Glyph).GetField("Ascender");
		private static readonly FieldInfo BaseGlyph_Descender = typeof(Orvid.TrueType.Glyph).GetField("Descender");
		private static readonly FieldInfo BaseGlyph_LeftSideBearing = typeof(Orvid.TrueType.Glyph).GetField("LeftSideBearing");
		private static readonly FieldInfo BaseGlyph_RightSideBearing = typeof(Orvid.TrueType.Glyph).GetField("RightSideBearing");
		private static readonly FieldInfo BaseGlyph_AdvanceWidth = typeof(Orvid.TrueType.Glyph).GetField("AdvanceWidth");
		private static readonly FieldInfo BaseGlyph_PhantomPoint1 = typeof(Orvid.TrueType.Glyph).GetField("PhantomPoint1");
		private static readonly FieldInfo BaseGlyph_PhantomPoint2 = typeof(Orvid.TrueType.Glyph).GetField("PhantomPoint2");
		private ConstructorBuilder EmitSimpleGlyphInit(SimpleGlyph g, TypeBuilder tBldr, uint glyphIndex)
		{
			FieldBuilder Contour = tBldr.DefineField("Glyph_Contour", typeof(Orvid.TrueType.Vec2[]), PublicStaticReadonlyFieldAttributes);
			FieldBuilder OnCurve = tBldr.DefineField("Glyph_OnCurve", typeof(bool[]), PublicStaticReadonlyFieldAttributes);
			FieldBuilder EndPointsOfCountours = tBldr.DefineField("Glyph_EndPointsOfContours", typeof(int[]), PublicStaticReadonlyFieldAttributes);
			FieldBuilder ContourCount = tBldr.DefineField("Glyph_ContourCount", typeof(int), PublicStaticReadonlyFieldAttributes);

			FieldBuilder Ascender = tBldr.DefineField("Glyph_Ascender", typeof(int), PublicStaticReadonlyFieldAttributes);
			FieldBuilder Descender = tBldr.DefineField("Glyph_Descender", typeof(int), PublicStaticReadonlyFieldAttributes);
			FieldBuilder LeftSideBearing = tBldr.DefineField("Glyph_LeftSideBearing", typeof(int), PublicStaticReadonlyFieldAttributes);
			FieldBuilder RightSideBearing = tBldr.DefineField("Glyph_RightSideBearing", typeof(int), PublicStaticReadonlyFieldAttributes);
			FieldBuilder AdvanceWidth = tBldr.DefineField("Glyph_AdvanceWidth", typeof(int), PublicStaticReadonlyFieldAttributes);
			FieldBuilder PhantomPoint1 = tBldr.DefineField("Glyph_PhantomPoint1", typeof(int), PublicStaticReadonlyFieldAttributes);
			FieldBuilder PhantomPoint2 = tBldr.DefineField("Glyph_PhantomPoint2", typeof(int), PublicStaticReadonlyFieldAttributes);

			#region Static Constructor
			ConstructorBuilder mBldr = tBldr.DefineTypeInitializer();
			ILGenerator gen = mBldr.GetILGenerator();

			#region Contour Info
			gen.DeclareLocal(typeof(Orvid.TrueType.Vec2).MakeByRefType(), true);	// The pinned Contour array
			gen.DeclareLocal(typeof(Orvid.TrueType.Vec2).MakeByRefType());		// The pointer we'll increment
			gen.DeclareLocal(typeof(bool).MakeByRefType(), true);	// The pinned OnCurve array
			gen.DeclareLocal(typeof(bool).MakeByRefType());		// The pointer we'll increment
			gen.DeclareLocal(typeof(int).MakeByRefType(), true);	// The pinned EndPointsOfCountours array
			gen.DeclareLocal(typeof(int).MakeByRefType());		// The pointer we'll increment

			LoadInt(gen, g.Points.Length);
			gen.Emit(OpCodes.Dup);
			gen.Emit(OpCodes.Newarr, typeof(Orvid.TrueType.Vec2));
			gen.Emit(OpCodes.Dup);
			gen.Emit(OpCodes.Ldc_I4_0);
			gen.Emit(OpCodes.Ldelema, typeof(Orvid.TrueType.Vec2));
			gen.Emit(OpCodes.Dup);
			gen.Emit(OpCodes.Stloc_0);
			gen.Emit(OpCodes.Stloc_1);
			gen.Emit(OpCodes.Stsfld, Contour);

			gen.Emit(OpCodes.Newarr, typeof(bool));
			gen.Emit(OpCodes.Dup);
			gen.Emit(OpCodes.Ldc_I4_0);
			gen.Emit(OpCodes.Ldelema, typeof(bool));
			gen.Emit(OpCodes.Dup);
			gen.Emit(OpCodes.Stloc_2);
			gen.Emit(OpCodes.Stloc_3);
			gen.Emit(OpCodes.Stsfld, OnCurve);

			// The Contour and OnCurve arrays are now created,
			// and pointers to them are stored in locals 0, 1,
			// 2, and 3
			for (uint i = 0; i < g.Points.Length; i++)
			{
				gen.Emit(OpCodes.Ldsfld, Contour);
				LoadInt(gen, (int)i);
				gen.Emit(OpCodes.Ldelema, typeof(Orvid.TrueType.Vec2));

				LoadInt(gen, g.Points[i].X);
				LoadInt(gen, g.Points[i].Y);
				gen.Emit(OpCodes.Newobj, TrueType_Vec2_ctor_int_int);
				gen.Emit(OpCodes.Stobj, typeof(Orvid.TrueType.Vec2));
			}

			for (uint i = 0; i < g.OnCurve.Length; i++)
			{
				gen.Emit(OpCodes.Ldsfld, OnCurve);
				LoadInt(gen, (int)i);
				gen.Emit(OpCodes.Ldelema, typeof(bool));

				LoadInt(gen, (g.OnCurve[i] ? 1 : 0));
				gen.Emit(OpCodes.Stind_I1);
			}

			// point arrays & on curve arrays are done
			// Just need to do the countour end points array
			LoadInt(gen, g.NumberOfContours);
			gen.Emit(OpCodes.Dup);
			gen.Emit(OpCodes.Stsfld, ContourCount);

			gen.Emit(OpCodes.Newarr, typeof(int));
			gen.Emit(OpCodes.Dup);
			gen.Emit(OpCodes.Ldc_I4_0);
			gen.Emit(OpCodes.Ldelema, typeof(int));
			gen.Emit(OpCodes.Dup);
			gen.Emit(OpCodes.Stloc_S, (byte)4);
			gen.Emit(OpCodes.Stloc_S, (byte)5);
			gen.Emit(OpCodes.Stsfld, EndPointsOfCountours);

			for (uint i = 0; i < g.NumberOfContours; i++)
			{
				gen.Emit(OpCodes.Ldsfld, EndPointsOfCountours);
				LoadInt(gen, (int)i);
				gen.Emit(OpCodes.Ldelema, typeof(int));

				LoadInt(gen, g.EndPointsOfContours[i]);
				gen.Emit(OpCodes.Stind_I4);
			}
			#endregion

			#region Positioning Data
			int leftSideBearing = g.ParentFont.GetLeftSideBearing((int)glyphIndex);
			int advanceWidth = g.ParentFont.GetAdvanceWidth((int)glyphIndex);
			int rightSideBearing = advanceWidth - (leftSideBearing + g.MaxX - g.MinX);
			int phantomPoint1 = g.MinX - leftSideBearing;
			int phantomPoint2 = phantomPoint1 + advanceWidth;

			LoadInt(gen, rightSideBearing);
			gen.Emit(OpCodes.Stsfld, RightSideBearing);
			LoadInt(gen, leftSideBearing);
			gen.Emit(OpCodes.Stsfld, LeftSideBearing);
			LoadInt(gen, advanceWidth);
			gen.Emit(OpCodes.Stsfld, AdvanceWidth);
			LoadInt(gen, phantomPoint1);
			gen.Emit(OpCodes.Stsfld, PhantomPoint1);
			LoadInt(gen, phantomPoint2);
			gen.Emit(OpCodes.Stsfld, PhantomPoint2);
			#endregion

			gen.Emit(OpCodes.Ret);
			#endregion


			#region Instance Constructor
			mBldr = tBldr.DefineConstructor(PublicSpecialRTMethodAttributes, StandardHasThisCallingConvention, new Type[] { });
			gen = mBldr.GetILGenerator();

			gen.Emit(OpCodes.Ldarg_0);
			gen.Emit(OpCodes.Call, BaseGlyph_Ctor);

			// This ends up with enough 'this'
			// params on the stack to be able
			// to store all the fields we need.
			gen.Emit(OpCodes.Ldarg_0);
			gen.Emit(OpCodes.Dup);
			gen.Emit(OpCodes.Dup);
			gen.Emit(OpCodes.Dup);
			gen.Emit(OpCodes.Dup);

			gen.Emit(OpCodes.Ldsfld, Contour);
			gen.Emit(OpCodes.Stfld, BaseGlyph_Contour);

			gen.Emit(OpCodes.Ldsfld, OnCurve);
			gen.Emit(OpCodes.Stfld, BaseGlyph_PointsOnCurve);

			gen.Emit(OpCodes.Ldsfld, EndPointsOfCountours);
			gen.Emit(OpCodes.Stfld, BaseGlyph_EndPointsOfCountours);

			gen.Emit(OpCodes.Ldsfld, PhantomPoint1);
			gen.Emit(OpCodes.Stfld, BaseGlyph_PhantomPoint1);

			gen.Emit(OpCodes.Ldsfld, PhantomPoint2);
			gen.Emit(OpCodes.Stfld, BaseGlyph_PhantomPoint2);

			gen.Emit(OpCodes.Ret);
			#endregion

			return mBldr;
		}
		#endregion

		#endregion

		#region Emit Font Descriptor
		private void EmitFontDescriptor(ModuleBuilder bldr)
		{
			TypeBuilder tBldr = bldr.DefineType("TrueType.FontDescriptor", PublicSealedClassTypeAttributes, typeof(Orvid.TrueType.FontDescriptor));
			FieldBuilder charMapTable = tBldr.DefineField("CharacterMapTable", typeof(Orvid.TrueType.Glyph[]), PublicStaticReadonlyFieldAttributes);
			MethodBuilder mBldr;
			ILGenerator gen;

			#region GetMaxNumberOfPoints
			mBldr = tBldr.DefineMethod("GetMaxNumberOfPoints", PublicVirtualMethodAttributes, typeof(int), new Type[] { });
			gen = mBldr.GetILGenerator();
			LoadInt(gen, ParentFont.MaxPoints);
			gen.Emit(OpCodes.Ret);
			#endregion

			#region GetMaxNumberOfTwighlightPoints
			mBldr = tBldr.DefineMethod("GetMaxNumberOfTwighlightPoints", PublicVirtualMethodAttributes, typeof(int), new Type[] { });
			gen = mBldr.GetILGenerator();
			LoadInt(gen, ParentFont.MaxTwilightPoints);
			gen.Emit(OpCodes.Ret);
			#endregion

			#region GetMaxNumberOfContours
			mBldr = tBldr.DefineMethod("GetMaxNumberOfContours", PublicVirtualMethodAttributes, typeof(int), new Type[] { });
			gen = mBldr.GetILGenerator();
			LoadInt(gen, ParentFont.MaxContours);
			gen.Emit(OpCodes.Ret);
			#endregion

			#region GetMaxStorage
			mBldr = tBldr.DefineMethod("GetMaxStorage", PublicVirtualMethodAttributes, typeof(int), new Type[] { });
			gen = mBldr.GetILGenerator();
			LoadInt(gen, ParentFont.MaxStorage);
			gen.Emit(OpCodes.Ret);
			#endregion

			FieldBuilder Font_CvtValues = tBldr.DefineField("Font_CvtValues", typeof(int[]), PublicStaticReadonlyFieldAttributes);
			
			#region Static Constructor
			{
				ConstructorBuilder mBldr2 = tBldr.DefineTypeInitializer();
				gen = mBldr2.GetILGenerator();
				//gen.DeclareLocal(typeof(int).MakeByRefType(), true);	// 0
				//gen.DeclareLocal(typeof(int).MakeByRefType());			// 1

				#region Glyph Mapping
				LoadInt(gen, ushort.MaxValue + 1);
				gen.Emit(OpCodes.Newarr, typeof(Orvid.TrueType.Glyph));
				gen.Emit(OpCodes.Dup);
				gen.Emit(OpCodes.Stsfld, charMapTable);


				int[] mIdxs = ParentFont.ActiveCharMapTable.GetMappedGlyphIndexes();
				Dictionary<int, ConstructorBuilder> GlyphMapping = new Dictionary<int, ConstructorBuilder>();
				foreach (ConstructorBuilder cBldr in GlyphConstructors)
				{
					GlyphMapping.Add(int.Parse(cBldr.DeclaringType.Name.Substring(6)), cBldr);
				}

				if (GlyphMapping.ContainsKey(0))
				{
					gen.Emit(OpCodes.Dup);
					gen.Emit(OpCodes.Newobj, GlyphMapping[0]);
					gen.Emit(OpCodes.Call, typeof(Orvid.TrueType.ArrayUtils).GetMethod("AssignValueToGlyphArray"));
				}
				else
				{
					throw new Exception("Expected a default glyph!");
				}

				for (int i = 0; i < mIdxs.Length; i++)
				{
					int gIdx = mIdxs[i];
					if (gIdx != 0 && GlyphMapping.ContainsKey(gIdx))
					{
						gen.Emit(OpCodes.Dup);
						LoadInt(gen, i);
						gen.Emit(OpCodes.Newobj, GlyphMapping[gIdx]);
						gen.Emit(OpCodes.Stelem_Ref);
					}
				}
				gen.Emit(OpCodes.Pop);
				#endregion


				#region Cvt Values
				{
					uint[] vals = ParentFont.CvtValues;
					int len = vals.Length;
					LoadInt(gen, len);
					gen.Emit(OpCodes.Newarr, typeof(int));
					//gen.Emit(OpCodes.Dup);
					//gen.Emit(OpCodes.Ldc_I4_0);
					//gen.Emit(OpCodes.Ldelema, typeof(int));
					//gen.Emit(OpCodes.Dup);
					//gen.Emit(OpCodes.Stloc_0);
					//gen.Emit(OpCodes.Stloc_1);
					gen.Emit(OpCodes.Stsfld, Font_CvtValues);

					gen.Emit(OpCodes.Ldsfld, Font_CvtValues);
					for (int i = 0; i < len; i++)
					{
						gen.Emit(OpCodes.Dup);
						LoadInt(gen, (int)i);
						gen.Emit(OpCodes.Ldelema, typeof(int));

						LoadInt(gen, (int)vals[i]);
						gen.Emit(OpCodes.Stind_I4);
					}
					gen.Emit(OpCodes.Pop);

				}
				#endregion

				gen.Emit(OpCodes.Ret);
			}
			#endregion

			#region Instance Constructor
			{
				ConstructorBuilder mBldr2 = tBldr.DefineConstructor(PublicSpecialRTMethodAttributes, StandardHasThisCallingConvention, new Type[] { });
				gen = mBldr2.GetILGenerator();

				gen.Emit(OpCodes.Ldarg_0);
				gen.Emit(OpCodes.Call, typeof(Orvid.TrueType.FontDescriptor).GetConstructor(new Type[] { }));


				gen.Emit(OpCodes.Ldarg_0);
				gen.Emit(OpCodes.Ldsfld, Font_CvtValues);
				gen.Emit(OpCodes.Stfld, typeof(Orvid.TrueType.FontDescriptor).GetField("CvtValues"));

				gen.Emit(OpCodes.Ret);
			}
			#endregion

			#region GetGlyph
			mBldr = tBldr.DefineMethod("GetGlyph", PublicVirtualMethodAttributes, typeof(Orvid.TrueType.Glyph), new Type[] { typeof(int) });
			// It just produces ugly names otherwise.
			mBldr.DefineParameter(1, ParameterAttributes.None, "charCode");
			gen = mBldr.GetILGenerator();

			gen.Emit(OpCodes.Ldsfld, charMapTable);
			gen.Emit(OpCodes.Ldarg_1);
			gen.Emit(OpCodes.Ldelem_Ref);

			gen.Emit(OpCodes.Ret);
			#endregion

			tBldr.CreateType();
		}
		#endregion

		#endregion

		#region Helper Methods

		#region LoadInt
		private void LoadInt(ILGenerator gen, int val)
		{
			switch (val)
			{
				case -1: gen.Emit(OpCodes.Ldc_I4_M1); break;
				case 0: gen.Emit(OpCodes.Ldc_I4_0); break;
				case 1: gen.Emit(OpCodes.Ldc_I4_1); break;
				case 2: gen.Emit(OpCodes.Ldc_I4_2); break;
				case 3: gen.Emit(OpCodes.Ldc_I4_3); break;
				case 4: gen.Emit(OpCodes.Ldc_I4_4); break;
				case 5: gen.Emit(OpCodes.Ldc_I4_5); break;
				case 6: gen.Emit(OpCodes.Ldc_I4_6); break;
				case 7: gen.Emit(OpCodes.Ldc_I4_7); break;
				case 8: gen.Emit(OpCodes.Ldc_I4_8); break;
				default:
					if (val < SByte.MaxValue && val > SByte.MinValue)
						gen.Emit(OpCodes.Ldc_I4_S, (byte)(sbyte)val);
					else
						gen.Emit(OpCodes.Ldc_I4, val);
					break;
			}
		}
		#endregion

		#endregion

	}
}
