#define ThrowNotImplAtRuntime

using System;
using System.IO;
using System.Text;
using Orvid.TrueType;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;
using Orvid.Compiler.TrueType.Utils;

namespace Orvid.Compiler.TrueType.HintingVM
{
	public enum SourceType
	{
		SyntheticStack = 0,
		ILStack,
		Constant,
		Parameter,
	}

	public struct SourceData
	{
		public SourceType Source;
		public int Constant;
		public bool IsF26Dot6;
		public int ParameterIndex;

		public override string ToString()
		{
			return ((Source == SourceType.Constant) ? Constant.ToString() : (Source == SourceType.Parameter ? Source.ToString() + "(" + ParameterIndex.ToString() + ")" : Source.ToString()));
		}
	}

	/// <summary>
	/// Represents a single instruction.
	/// </summary>
	public abstract class IRInstruction
	{
		private const BindingFlags PrivateInstanceBindingFlags = BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly;

		#region Field Info's

		protected static readonly FieldInfo F26Dot6_One = typeof(F26Dot6).GetField("One");
		protected static readonly FieldInfo F26Dot6_Zero = typeof(F26Dot6).GetField("Zero");

		protected static readonly FieldInfo GraphicsState_Auto_Flip = typeof(GraphicsState).GetField("Auto_Flip");
		protected static readonly FieldInfo GraphicsState_Cvt = typeof(GraphicsState).GetField("Cvt");
		protected static readonly FieldInfo GraphicsState_Cvt_CutIn = typeof(GraphicsState).GetField("Control_Value_Cut_In");
		protected static readonly FieldInfo GraphicsState_Delta_Base = typeof(GraphicsState).GetField("Delta_Base");
		protected static readonly FieldInfo GraphicsState_Delta_Shift = typeof(GraphicsState).GetField("Delta_Shift");
		protected static readonly FieldInfo GraphicsState_Dual_Projection_Vector = typeof(GraphicsState).GetField("Dual_Projection_Vector");
		protected static readonly FieldInfo GraphicsState_Freedom_Vector = typeof(GraphicsState).GetField("Freedom_Vector");
		protected static readonly FieldInfo GraphicsState_Functions = typeof(GraphicsState).GetField("Functions");
		protected static readonly FieldInfo GraphicsState_InstructControlFlags = typeof(GraphicsState).GetField("InstructControlFlags");
		protected static readonly FieldInfo GraphicsState_Loop = typeof(GraphicsState).GetField("Loop");
		protected static readonly FieldInfo GraphicsState_Minimum_Distance = typeof(GraphicsState).GetField("Minimum_Distance");
		protected static readonly FieldInfo GraphicsState_PointSize = typeof(GraphicsState).GetField("SizeInPoints");
		protected static readonly FieldInfo GraphicsState_Projection_Vector = typeof(GraphicsState).GetField("Projection_Vector");
		protected static readonly FieldInfo GraphicsState_RoundMode = typeof(GraphicsState).GetField("RoundMode");
		protected static readonly FieldInfo GraphicsState_rp0 = typeof(GraphicsState).GetField("rp0");
		protected static readonly FieldInfo GraphicsState_rp1 = typeof(GraphicsState).GetField("rp1");
		protected static readonly FieldInfo GraphicsState_rp2 = typeof(GraphicsState).GetField("rp2");
		protected static readonly FieldInfo GraphicsState_ScanControl = typeof(GraphicsState).GetField("ScanControl");
		protected static readonly FieldInfo GraphicsState_Stack = typeof(GraphicsState).GetField("Stack");
		protected static readonly FieldInfo GraphicsState_Storage = typeof(GraphicsState).GetField("Storage");
		protected static readonly FieldInfo GraphicsState_zp0 = typeof(GraphicsState).GetField("zp0");
		protected static readonly FieldInfo GraphicsState_zp1 = typeof(GraphicsState).GetField("zp1");
		protected static readonly FieldInfo GraphicsState_zp2 = typeof(GraphicsState).GetField("zp2");

		protected static readonly FieldInfo VecF2Dot14_X = typeof(VecF2Dot14).GetField("local_x");
		protected static readonly FieldInfo VecF2Dot14_Y = typeof(VecF2Dot14).GetField("local_y");
		protected static readonly FieldInfo VecF2Dot14_Axis_X = typeof(VecF2Dot14).GetField("Axis_X");
		protected static readonly FieldInfo VecF2Dot14_Axis_Y = typeof(VecF2Dot14).GetField("Axis_Y");

		protected static readonly FieldInfo VecF26Dot6_X = typeof(VecF26Dot6).GetField("local_x");
		protected static readonly FieldInfo VecF26Dot6_Y = typeof(VecF26Dot6).GetField("local_y");
		protected static readonly FieldInfo VecF26Dot6_Axis_X = typeof(VecF26Dot6).GetField("Axis_X");
		protected static readonly FieldInfo VecF26Dot6_Axis_Y = typeof(VecF26Dot6).GetField("Axis_Y");

		protected static readonly FieldInfo LinkedStack_Depth = typeof(LinkedStack<int>).GetField("depth", PrivateInstanceBindingFlags);
		#endregion

		#region Method Info's
		protected static readonly MethodInfo LinkedStack_Clear = typeof(LinkedStack<int>).GetMethod("Clear");
		protected static readonly MethodInfo LinkedStack_CopyToTop = typeof(LinkedStack<int>).GetMethod("CopyToTop");
		protected static readonly MethodInfo LinkedStack_Pop = typeof(LinkedStack<int>).GetMethod("Pop");
		protected static readonly MethodInfo LinkedStack_Push = typeof(LinkedStack<int>).GetMethod("Push");

		protected static readonly MethodInfo GraphicsState_AddCvtExceptions = typeof(GraphicsState).GetMethod("AddCvtExceptions");
		protected static readonly MethodInfo GraphicsState_AddLoneCvtException = typeof(GraphicsState).GetMethod("AddCvtException");
		protected static readonly MethodInfo GraphicsState_AddPointExceptions = typeof(GraphicsState).GetMethod("AddPointExceptions");
		protected static readonly MethodInfo GraphicsState_AlignToReferencePoint = typeof(GraphicsState).GetMethod("AlignToReferencePoint");
		protected static readonly MethodInfo GraphicsState_BringToTopOfStack = typeof(GraphicsState).GetMethod("BringToTopOfStack");
		protected static readonly MethodInfo GraphicsState_GetCvtEntry = typeof(GraphicsState).GetMethod("GetCvtEntry");
		protected static readonly MethodInfo GraphicsState_GetInfo = typeof(GraphicsState).GetMethod("GetInfo");
		protected static readonly MethodInfo GraphicsState_GetProjectedCoord = typeof(GraphicsState).GetMethod("GetCoords");
		protected static readonly MethodInfo GraphicsState_GetPixelsPerEM = typeof(GraphicsState).GetMethod("GetPixelsPerEM");
		protected static readonly MethodInfo GraphicsState_InterpolatePoints = typeof(GraphicsState).GetMethod("InterpolatePoints");
		protected static readonly MethodInfo GraphicsState_InterpolateUntouchedPoints = typeof(GraphicsState).GetMethod("InterpolateUntouchedPoints");
		protected static readonly MethodInfo GraphicsState_ISect = typeof(GraphicsState).GetMethod("ISect");
		protected static readonly MethodInfo GraphicsState_LoopedFlipOnCurve = typeof(GraphicsState).GetMethod("LoopedFlipOnCurve");
		protected static readonly MethodInfo GraphicsState_LoopedShiftPixel = typeof(GraphicsState).GetMethod("LoopedShiftPixel");
		protected static readonly MethodInfo GraphicsState_LoopedShiftPoint = typeof(GraphicsState).GetMethod("LoopedShiftPoint");
		protected static readonly MethodInfo GraphicsState_MeasureDistance = typeof(GraphicsState).GetMethod("MeasureDistance");
		protected static readonly MethodInfo GraphicsState_MoveDirectAbsolutePoint = typeof(GraphicsState).GetMethod("MoveDirectAbsolutePoint");
		protected static readonly MethodInfo GraphicsState_MoveDirectRelativePoint = typeof(GraphicsState).GetMethod("MoveDirectRelativePoint");
		protected static readonly MethodInfo GraphicsState_MoveIndirectAbsolutePoint = typeof(GraphicsState).GetMethod("MoveIndirectAbsolutePoint");
		protected static readonly MethodInfo GraphicsState_MoveIndirectRelativePoint = typeof(GraphicsState).GetMethod("MoveIndirectRelativePoint");
		protected static readonly MethodInfo GraphicsState_MoveStackIndirectRelativePoint = typeof(GraphicsState).GetMethod("MoveStackIndirectRelativePoint");
		protected static readonly MethodInfo GraphicsState_RecalcProjFreedomDotProduct = typeof(GraphicsState).GetMethod("RecalcProjFreedomDotProduct");
		protected static readonly MethodInfo GraphicsState_Round = typeof(GraphicsState).GetMethod("Round");
		protected static readonly MethodInfo GraphicsState_SetCoords = typeof(GraphicsState).GetMethod("SetCoords");
		protected static readonly MethodInfo GraphicsState_SetDualProjectionVectorToLine = typeof(GraphicsState).GetMethod("SetDualProjectionVectorToLine");
		protected static readonly MethodInfo GraphicsState_SetFreedomVector = typeof(GraphicsState).GetMethod("SetFreedomVector");
		protected static readonly MethodInfo GraphicsState_SetFreedomVectorToLine = typeof(GraphicsState).GetMethod("SetFreedomVectorToLine");
		protected static readonly MethodInfo GraphicsState_SetRangeOffCurve = typeof(GraphicsState).GetMethod("SetRangeOffCurve");
		protected static readonly MethodInfo GraphicsState_SetRangeOnCurve = typeof(GraphicsState).GetMethod("SetRangeOnCurve");
		protected static readonly MethodInfo GraphicsState_SetSuperRoundMode = typeof(GraphicsState).GetMethod("SetSuperRoundMode");
		protected static readonly MethodInfo GraphicsState_SetProjectionVector = typeof(GraphicsState).GetMethod("SetProjectionVector");
		protected static readonly MethodInfo GraphicsState_SetProjectionVectorToLine = typeof(GraphicsState).GetMethod("SetProjectionVectorToLine");
		protected static readonly MethodInfo GraphicsState_SetZonePointer = typeof(GraphicsState).GetMethod("SetZonePointer");
		protected static readonly MethodInfo GraphicsState_ShiftContour = typeof(GraphicsState).GetMethod("ShiftContour");
		protected static readonly MethodInfo GraphicsState_StaticRound = typeof(GraphicsState).GetMethod("StaticRound");
		protected static readonly MethodInfo GraphicsState_UnitsToPixel = typeof(GraphicsState).GetMethod("UnitsToPixel", new Type[] { typeof(double) });
		protected static readonly MethodInfo GraphicsState_WriteCvtEntry = typeof(GraphicsState).GetMethod("WriteCvtEntry");
		protected static readonly MethodInfo GraphicsState_WriteCvtEntryF = typeof(GraphicsState).GetMethod("WriteCvtEntryF");

		protected static readonly MethodInfo F2Dot14_AsLiteral = typeof(F2Dot14).GetMethod("AsLiteral");
		protected static readonly MethodInfo F2Dot14_FromLiteral = typeof(F2Dot14).GetMethod("FromLiteral");

		protected static readonly MethodInfo F26Dot6_Abs = typeof(F26Dot6).GetMethod("Abs");
		protected static readonly MethodInfo F26Dot6_Add = typeof(F26Dot6).GetMethod("op_Addition");
		protected static readonly MethodInfo F26Dot6_AsLiteral = typeof(F26Dot6).GetMethod("AsLiteral");
		protected static readonly MethodInfo F26Dot6_Ceiling = typeof(F26Dot6).GetMethod("Ceiling");
		protected static readonly MethodInfo F26Dot6_Divide = typeof(F26Dot6).GetMethod("op_Division", new Type[] { typeof(F26Dot6), typeof(F26Dot6) });
		protected static readonly MethodInfo F26Dot6_Floor = typeof(F26Dot6).GetMethod("Floor");
		protected static readonly MethodInfo F26Dot6_FromLiteral = typeof(F26Dot6).GetMethod("FromLiteral");
		protected static readonly MethodInfo F26Dot6_FromDouble = typeof(F26Dot6).GetMethod("FromDouble");
		protected static readonly MethodInfo F26Dot6_Max = typeof(F26Dot6).GetMethod("Max");
		protected static readonly MethodInfo F26Dot6_Min = typeof(F26Dot6).GetMethod("Min");
		protected static readonly MethodInfo F26Dot6_Multiply = typeof(F26Dot6).GetMethod("op_Multiply");
		protected static readonly MethodInfo F26Dot6_Negate = typeof(F26Dot6).GetMethod("op_UnaryNegation");
		protected static readonly MethodInfo F26Dot6_Subtract = typeof(F26Dot6).GetMethod("op_Subtraction", new Type[] { typeof(F26Dot6), typeof(F26Dot6) });
		protected static readonly MethodInfo F26Dot6_ToDouble = typeof(F26Dot6).GetMethod("ToDouble");
		#endregion

		/// <summary>
		/// The IROpCode that this instruction
		/// represents.
		/// </summary>
		public abstract IROpCode OpCode { get; }

		/// <summary>
		/// Emit this op-code as IL.
		/// </summary>
		public abstract void Emit(IRMethodBuilder IRbldr, ILGenerator gen, MethodBuilder mBldr, TypeBuilder tBldr, bool isGlyph);

		/// <summary>
		/// Writes the textual representation
		/// of this op-code to the specified
		/// <see cref="StreamWriter"/>.
		/// </summary>
		/// <param name="tOut">The <see cref="StreamWriter"/> to write to.</param>
		public abstract void WriteText(IRMethodBuilder IRbldr, StreamWriter tOut);

		public virtual bool ExpectsArg1F26Dot6 { get { return false; } }
		public virtual bool ExpectsArg2F26Dot6 { get { return false; } }
		public virtual bool ExpectsArg3F26Dot6 { get { return false; } }

		public SourceData[] Args = new SourceData[3];
		
		// If this is not true, Destination1IsF26Dot6 cannot be true.
		public bool Destination1IsILStack;
		public bool Destination1IsF26Dot6;

		// If this is not true, Destination2IsF26Dot6 cannot be true.
		public bool Destination2IsILStack;
		public bool Destination2IsF26Dot6;
		public bool Exists = true;

		public Stream ParentStream = null;
		public long PositionInStream = 0;

		/// <summary>
		/// Gets the argument string for this
		/// instruction.
		/// </summary>
		/// <param name="argCount">The number of arguments.</param>
		/// <param name="HasDestination">True if this instruction has a destination.</param>
		/// <returns>The generated string.</returns>
		protected string GetArgString(int argCount, bool HasDestination)
		{
			string str;
			switch (argCount)
			{
				case 0:
					str = "";
					break;
				case 1:
					str = "(" + (ExpectsArg1F26Dot6 ? (Args[0].IsF26Dot6 ? "" : "(F26Dot6)") : "") + Args[0].ToString() + ")";
					break;
				case 2:
					str = "(" + (ExpectsArg1F26Dot6 ? (Args[0].IsF26Dot6 ? "" : "(F26Dot6)") : "") + Args[0].ToString() + ", " + (ExpectsArg2F26Dot6 ? (Args[1].IsF26Dot6 ? "" : "(F26Dot6)") : "") + Args[1].ToString() + ")";
					break;
				case 3:
					str = "(" + (ExpectsArg1F26Dot6 ? (Args[0].IsF26Dot6 ? "" : "(F26Dot6)") : "") + Args[0].ToString() + ", " + (ExpectsArg2F26Dot6 ? (Args[1].IsF26Dot6 ? "" : "(F26Dot6)") : "") + Args[1].ToString() + ", " + (ExpectsArg3F26Dot6 ? (Args[2].IsF26Dot6 ? "" : "(F26Dot6)") : "") + Args[2].ToString() + ")";
					break;
				default:
					throw new Exception("Unknown argument count!");
			}
			if (HasDestination)
				str += "->" + ((Destination1IsILStack) ? "IL" : "Synth") + (Destination1IsF26Dot6 ? "(As F26Dot6)" : "");
			return str;
		}
		
		/// <summary>
		/// Gets the argument string for this
		/// instruction.
		/// </summary>
		/// <param name="argCount">The number of arguments.</param>
		/// <param name="HasDestination">True if this instruction has a destination.</param>
		/// <returns>The generated string.</returns>
		protected string GetArgString(int argCount, bool HasDestination, int destCount)
		{
			string str;
			switch (argCount)
			{
				case 0:
					str = "";
					break;
				case 1:
					str = "(" + (ExpectsArg1F26Dot6 ? (Args[0].IsF26Dot6 ? "" : "(F26Dot6)") : "") + Args[0].ToString() + ")";
					break;
				case 2:
					str = "(" + (ExpectsArg1F26Dot6 ? (Args[0].IsF26Dot6 ? "" : "(F26Dot6)") : "") + Args[0].ToString() + ", " + (ExpectsArg2F26Dot6 ? (Args[1].IsF26Dot6 ? "" : "(F26Dot6)") : "") + Args[1].ToString() + ")";
					break;
				case 3:
					str = "(" + (ExpectsArg1F26Dot6 ? (Args[0].IsF26Dot6 ? "" : "(F26Dot6)") : "") + Args[0].ToString() + ", " + (ExpectsArg2F26Dot6 ? (Args[1].IsF26Dot6 ? "" : "(F26Dot6)") : "") + Args[1].ToString() + ", " + (ExpectsArg3F26Dot6 ? (Args[2].IsF26Dot6 ? "" : "(F26Dot6)") : "") + Args[2].ToString() + ")";
					break;
				default:
					throw new Exception("Unknown argument count!");
			}
			if (destCount == 2)
			{
				str += "->(" + ((Destination1IsILStack) ? "IL" : "Synth") + (Destination1IsF26Dot6 ? "(As F26Dot6)" : "");
				str += ", " + ((Destination2IsILStack) ? "IL" : "Synth") + (Destination2IsF26Dot6 ? "(As F26Dot6)" : "") + ")";
			}
			return str;
		}


		/// <summary>
		/// Loads an argument to the top of
		/// the IL stack.
		/// </summary>
		/// <param name="gen">Where to emit the code.</param>
		/// <param name="argIndx">The arg to load.</param>
		/// <param name="isGlyph">True if this is being emitted for a glyph's hint method.</param>
		protected void LoadArgument(ILGenerator gen, int argIndx, IRMethodBuilder mBldr)
		{
			LoadArgument(gen, argIndx, mBldr, true);
		}

		/// <summary>
		/// Loads an argument to the top of
		/// the IL stack.
		/// </summary>
		/// <param name="gen">Where to emit the code.</param>
		/// <param name="argIndx">The arg to load.</param>
		/// <param name="mBldr">The method this is being emitted for.</param>
		/// <param name="autoConv">True if the parameter will be auto-converted to the destination type.</param>
		protected void LoadArgument(ILGenerator gen, int argIndx, IRMethodBuilder mBldr, bool autoConv)
		{
			bool needConvCheck = true;
			SourceType argTp;
			switch (argIndx)
			{
				case 1:
					argTp = Args[0].Source;
					break;
				case 2:
					argTp = Args[1].Source;
					break;
				case 3:
					argTp = Args[2].Source;
					break;
				default:
					throw new Exception("Unkown argument to load!");
			}
			switch (argTp)
			{
				case SourceType.ILStack: break;
				case SourceType.SyntheticStack:
					LoadGraphicsState(gen, mBldr);
					gen.Emit(OpCodes.Ldfld, GraphicsState_Stack);
					gen.Emit(OpCodes.Call, LinkedStack_Pop);
					break;
				case SourceType.Parameter:
					LoadParameter(gen, Args[0].ParameterIndex + (mBldr.IsGlyph ? 1 : 0));
					break;
				case SourceType.Constant:
					switch(argIndx)
					{
						case 1:
							{
								if (ExpectsArg1F26Dot6 && !Args[0].IsF26Dot6 && autoConv)
								{
									if (Args[0].Constant == 64)
									{
										gen.Emit(OpCodes.Ldsfld, F26Dot6_One);
										needConvCheck = false;
									}
									else if (Args[0].Constant == 0)
									{
										gen.Emit(OpCodes.Ldsfld, F26Dot6_Zero);
										needConvCheck = false;
									}
									else
									{
										LoadInt(gen, Args[0].Constant);
									}
								}
								else
								{
									LoadInt(gen, Args[0].Constant);
								}
							}
							break;
						case 2:
							{
								if (ExpectsArg2F26Dot6 && !Args[1].IsF26Dot6 && autoConv)
								{
									if (Args[1].Constant == 64)
									{
										gen.Emit(OpCodes.Ldsfld, F26Dot6_One);
										needConvCheck = false;
									}
									else if (Args[1].Constant == 0)
									{
										gen.Emit(OpCodes.Ldsfld, F26Dot6_Zero);
										needConvCheck = false;
									}
									else
									{
										LoadInt(gen, Args[1].Constant);
									}
								}
								else
								{
									LoadInt(gen, Args[1].Constant);
								}
							}
							break;
						case 3:
							{
								if (ExpectsArg3F26Dot6 && !Args[2].IsF26Dot6 && autoConv)
								{
									if (Args[2].Constant == 64)
									{
										gen.Emit(OpCodes.Ldsfld, F26Dot6_One);
										needConvCheck = false;
									}
									else if (Args[2].Constant == 0)
									{
										gen.Emit(OpCodes.Ldsfld, F26Dot6_Zero);
										needConvCheck = false;
									}
									else
									{
										LoadInt(gen, Args[2].Constant);
									}
								}
								else
								{
									LoadInt(gen, Args[2].Constant);
								}
							}
							break;
						default:
							throw new Exception("Unknown arg index!");
					}
					break;
				default:
					throw new Exception("Unknown argument type!");
			}

			if (needConvCheck && autoConv)
			{
				switch (argIndx)
				{
					case 1:
						if (ExpectsArg1F26Dot6 && !Args[0].IsF26Dot6)
							gen.Emit(OpCodes.Call, F26Dot6_FromLiteral);
						break;
					case 2:
						if (ExpectsArg2F26Dot6 && !Args[1].IsF26Dot6)
							gen.Emit(OpCodes.Call, F26Dot6_FromLiteral);
						break;
					case 3:
						if (ExpectsArg3F26Dot6 && !Args[2].IsF26Dot6)
							gen.Emit(OpCodes.Call, F26Dot6_FromLiteral);
						break;
					default:
						throw new Exception("Unknown arg index!");
				}
			}
		}

		/// <summary>
		/// Checks if the specified bit
		/// is set. 
		/// </summary>
		/// <param name="val">The value to check.</param>
		/// <param name="bitIndex">The bit to check.</param>
		/// <returns>True if the bit is set, otherwise false.</returns>
		public static bool IsBitSet(int val, byte bitIndex)
		{
			return ((val & (1 << bitIndex)) == (1 << bitIndex)) ? true : false;
		}

		/// <summary>
		/// Gets an integer representation
		/// of the specified bool.
		/// </summary>
		/// <param name="b">The bool to convert.</param>
		/// <returns>The integer representation.</returns>
		public static int BoolToInt(bool b)
		{
			return b ? 1 : 0;
		}

		/// <summary>
		/// Emits the load of a bool
		/// value.
		/// </summary>
		/// <param name="gen">Where to emit to.</param>
		/// <param name="b">Value to emit.</param>
		public static void LoadBool(ILGenerator gen, bool b)
		{
			gen.Emit(b ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0);
		}

		private static void LoadParameter(ILGenerator gen, int paramIdx)
		{
			switch (paramIdx)
			{
				case -1:
				case 0:
					gen.Emit(OpCodes.Ldarg_0);
					break;
				case 1:
					gen.Emit(OpCodes.Ldarg_1);
					break;
				case 2:
					gen.Emit(OpCodes.Ldarg_2);
					break;
				case 3:
					gen.Emit(OpCodes.Ldarg_3);
					break;
				default:
				{
					if (paramIdx < 255)
					{
						gen.Emit(OpCodes.Ldarg_S, (byte)paramIdx);
					}
					else
					{
						gen.Emit(OpCodes.Ldarg, paramIdx);
					}
					break;
				}
			}
		}

		/// <summary>
		/// Loads the graphic state to the
		/// top of the IL stack.
		/// </summary>
		public static void LoadGraphicsState(ILGenerator gen, IRMethodBuilder mBldr)
		{
			int gStateIdx = mBldr.Parameters.Count;
			if (mBldr.IsGlyph)
				gStateIdx++;
			LoadParameter(gen, gStateIdx);
		}

		/// <summary>
		/// A helper method to emit the
		/// smallest possible IL to load
		/// the specified int to the top
		/// of the IL stack.
		/// </summary>
		public static void LoadInt(ILGenerator gen, int val)
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

		protected static void EmitWarning(ILGenerator gen, string msg)
		{
			gen.Emit(OpCodes.Ldstr, msg);
			gen.Emit(OpCodes.Call, typeof(Console).GetMethod("WriteLine", new Type[] { typeof(string) }));
		}
	}
}
