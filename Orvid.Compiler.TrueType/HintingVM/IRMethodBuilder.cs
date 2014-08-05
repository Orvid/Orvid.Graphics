#define Debugging

using System;
using System.IO;
using System.Text;
using Orvid.TrueType;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;
using System.Diagnostics.SymbolStore;
using Orvid.Compiler.TrueType.Utils;
using Orvid.Compiler.TrueType.HintingVM.Instructions;

namespace Orvid.Compiler.TrueType.HintingVM
{
	public class IRMethodBuilder
	{
		private const MethodAttributes PublicStaticMethodAttributes = MethodAttributes.Public | MethodAttributes.Static | MethodAttributes.HideBySig;
		private const MethodAttributes PublicVirtualMethodAttributes = MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Virtual | MethodAttributes.ReuseSlot;

		public string MethodName;
		public bool IsGlyph;
		public MethodBuilder mBldr;
		public TypeBuilder tBldr;
		public bool Emittable = true;
		public int StackConsumed;
		public int StackReturned;
		public bool StackAnalysisValid = false;
		public bool Returns;
		public bool ReturnIsF26Dot6;

		public IRAssembly ParentAssembly;
		/// <summary>
		/// True if the emitting of this method
		/// needs to be delayed.
		/// </summary>
		public bool Delayed;
		public int DelayedAtInstruction = 0;
		public StreamWriter DelayedtOut;
		public ISymbolDocumentWriter DelayedSDocWriter;


		public List<IRParameter> Parameters = new List<IRParameter>();

#if Debugging
		public int curLineNumber = 1;
		public int prevLineNumber = 1;
		public string curIdent = "";
		public int curInstructionLength = 0;
		
		private static readonly string DebugTextOutPrefix = System.IO.Path.GetFullPath("./decomp/");
		private static readonly Guid TrueTypeByteCodeGUID = new Guid("7ADF8988-18A1-404D-A764-692D32225A5B");
		private static readonly Guid OrvidVendorGUID = new Guid("A6079D2E-9E0B-4545-BD68-CCF2C9E9B575");
#endif


		public LinkedStack<Label> IfEndConditionBranches = new LinkedStack<Label>();
		public List<IRInstruction> Instructions = new List<IRInstruction>();

		public IRMethodBuilder(string name, bool isGlyph, IRAssembly parentAssembly)
		{
			this.MethodName = name;
			this.IsGlyph = isGlyph;
			this.ParentAssembly = parentAssembly;
		}

		public void EmitMethod(ModuleBuilder modBldr, TypeBuilder tBldr)
		{
			// This way we can delay
			// emitting if needed.
			if (mBldr == null)
			{
				this.tBldr = tBldr;
				if (this.IsGlyph)
					mBldr = tBldr.DefineMethod(MethodName, PublicVirtualMethodAttributes, typeof(void), new Type[] { typeof(GraphicsState) });
				else
				{
					Type[] tarr = new Type[Parameters.Count + 1];
					for (int i = 0; i < Parameters.Count; i++)
					{
						if (Parameters[i].Type == ParameterType.Integer)
						{
							tarr[i] = typeof(int);
						}
						else
						{
							tarr[i] = typeof(F26Dot6);
						}
					}
					tarr[Parameters.Count] = typeof(GraphicsState);
					mBldr = tBldr.DefineMethod(MethodName, PublicStaticMethodAttributes, typeof(void), tarr);
				}
#if Debugging
				mBldr.DefineParameter(Parameters.Count + 1, ParameterAttributes.None, "gState");
#endif
				ILGenerator gen = mBldr.GetILGenerator();			
	
				#region Declare Locals
				// This might look like caos,
				// but it's good caos.

				// 0
				gen.DeclareLocal(typeof(int))
#if Debugging
.SetLocalSymInfo("tmpIntA")
#endif
;
				// 1
				gen.DeclareLocal(typeof(int))
#if Debugging
.SetLocalSymInfo("tmpIntB")
#endif
;
				// 2
				gen.DeclareLocal(typeof(F26Dot6))
#if Debugging
.SetLocalSymInfo("tmpF26Dot6A")
#endif
;
				// 3
				gen.DeclareLocal(typeof(F26Dot6))
#if Debugging
.SetLocalSymInfo("tmpF26Dot6B")
#endif
;
				// 4
				gen.DeclareLocal(typeof(int)) // This is the iterator for loops.
#if Debugging
				.SetLocalSymInfo("i")
#endif
;
				// 5
				gen.DeclareLocal(typeof(int)) // This is used for the number of times to loop.
#if Debugging
				.SetLocalSymInfo("LoopCount")
#endif
;
				// 6
				gen.DeclareLocal(typeof(HintingMethod))
#if Debugging
.SetLocalSymInfo("MethodToCall")
#endif
;
				// 7
				gen.DeclareLocal(typeof(double))
#if Debugging
.SetLocalSymInfo("tmpDoubleA")
#endif
;
				#endregion

			}
			for (int i = 0; i < Instructions.Count; i++)
			{
				IRInstruction ins = Instructions[i];
				if (  (ins.OpCode == IROpCode.JmpR && ins.Args[0].Source != SourceType.Constant)
				   || (ins.OpCode == IROpCode.JROF && ins.Args[1].Source != SourceType.Constant)
				   || (ins.OpCode == IROpCode.JROT && ins.Args[1].Source != SourceType.Constant)
					)
				{
					Emittable = false;
					break;
				}
			}
			if (!Emittable)
			{
				EmitInterpreterCall(this);
				return;
			}
			else
			{
				if (DelayedtOut == null)
				{
					DelayedtOut = new StreamWriter(DebugTextOutPrefix + tBldr.Namespace + "." + tBldr.Name + "." + mBldr.Name + ".ttf");
				}
				if (DelayedSDocWriter == null)
				{
					DelayedSDocWriter = modBldr.GetSymWriter().DefineDocument(DebugTextOutPrefix + tBldr.Namespace + "." + tBldr.Name + "." + mBldr.Name + ".ttf", TrueTypeByteCodeGUID, OrvidVendorGUID, SymDocumentType.Text);
				}
#if Debugging
				ISymbolDocumentWriter sDocWrtr = DelayedSDocWriter;
				modBldr.GetSymWriter().OpenMethod(new SymbolToken(mBldr.GetToken().Token));
				StreamWriter tOut = DelayedtOut;
#endif
				ILGenerator gen = mBldr.GetILGenerator();
				
				for (int i = DelayedAtInstruction; i < Instructions.Count; i++)
				{
					Instructions[i].Emit(this, gen, mBldr, tBldr, this.IsGlyph);
					if (Delayed)
					{
						DelayedAtInstruction = i;
						goto Exit;
					}
#if Debugging
					Instructions[i].WriteText(this, tOut);
					gen.MarkSequencePoint(sDocWrtr, prevLineNumber, curIdent.Length + 1, curLineNumber, curIdent.Length + curInstructionLength + 1);
					curLineNumber++;
					prevLineNumber = curLineNumber;
#endif
				}

#if Debugging
				tOut.Flush();
				tOut.Close();
				tOut.Dispose();
#endif
			Exit:
#if Debugging
				modBldr.GetSymWriter().CloseMethod();
#endif
			}
		}

		#region EmitInterpreterCall
		private static void EmitInterpreterCall(IRMethodBuilder mBldr)
		{
			Stream strm = null;
			int i = -1;
			while (strm == null)
			{
				i++;
				strm = mBldr.Instructions[i].ParentStream;
			}
			byte[] data;
			long len;
			if (mBldr.Instructions[0] is FDef)
			{
				strm.Position = mBldr.Instructions[i + 1].PositionInStream;
				len = mBldr.Instructions[mBldr.Instructions.Count - 2].PositionInStream - strm.Position;
			}
			else
			{
				// Otherwise it was it's own stream.
				strm.Position = 0;
				len = strm.Length;
			}
			data = new byte[len];
			strm.Read(data, 0, (int)len);
			FieldBuilder dFld_SData = mBldr.tBldr.DefineInitializedData("InterpreterData_" + mBldr.MethodName + "_SData", data, FieldAttributes.Public | FieldAttributes.Static);
			FieldBuilder dFld = mBldr.tBldr.DefineField("InterpreterData_" + mBldr.MethodName, typeof(byte[]), FieldAttributes.Public | FieldAttributes.Static);
			// We now have the original data.
			ILGenerator gen = mBldr.mBldr.GetILGenerator();
			gen.Emit(OpCodes.Ldsfld, dFld);
			Label dFldInited = gen.DefineLabel();
			gen.Emit(OpCodes.Brtrue_S, dFldInited);
			IRInstruction.LoadInt(gen, (int)len);
			gen.Emit(OpCodes.Newarr, typeof(byte));
			gen.Emit(OpCodes.Dup);
			gen.Emit(OpCodes.Ldtoken, dFld_SData);
			gen.Emit(OpCodes.Call, typeof(System.Runtime.CompilerServices.RuntimeHelpers).GetMethod("InitializeArray"));
			gen.Emit(OpCodes.Stsfld, dFld);
			gen.MarkLabel(dFldInited);
			gen.Emit(OpCodes.Ldsfld, dFld);
			IRInstruction.LoadGraphicsState(gen, mBldr);
			gen.Emit(OpCodes.Call, typeof(TrueTypeInterpreter).GetMethod("ExecuteMethod"));
			gen.Emit(OpCodes.Ret);
		}
		#endregion

		public void TWriteLine(StreamWriter tOut, string val)
		{
			tOut.WriteLine(curIdent + val);
		}

		public void ReadMethod(Stream strm)
		{
			BigEndianBinaryReader rdr = new BigEndianBinaryReader(strm);
			while (strm.Position < strm.Length)
			{
				long startPos = strm.Position;
				byte b = rdr.ReadByte();
				switch (b)
				{
					#region NPushB[]
					case 0x40: // NPushB[]
					{
						byte cnt = rdr.ReadByte();
						uint[] vals = new uint[cnt];
						for (uint i = 0; i < cnt; i++)
						{
							vals[i] = rdr.ReadByte();
						}
						Instructions.Add(new NPushB(vals));
						break;
					}
					#endregion
						
					#region NPushW[]
					case 0x41: // NPushW[]
					{
						byte cnt = rdr.ReadByte();
						uint[] vals = new uint[cnt];
						for (uint i = 0; i < cnt; i++)
						{
							vals[i] = rdr.ReadUInt16();
						}
						Instructions.Add(new NPushW(vals));
						break;
					}
					#endregion

					#region PushB[abc]
					case 0xB0:
					case 0xB1:
					case 0xB2:
					case 0xB3:
					case 0xB4:
					case 0xB5:
					case 0xB6:
					case 0xB7: // PushB[abc]
					{
						byte count = (byte)((b & 0x07) + 1);
						uint[] vals = new uint[count];
						for (uint i = 0; i < count; i++)
						{
							vals[i] = rdr.ReadByte();
						}
						Instructions.Add(new PushB(vals));
						break;
					}
					#endregion

					#region PushW[abc]
					case 0xB8:
					case 0xB9:
					case 0xBA:
					case 0xBB:
					case 0xBC:
					case 0xBD:
					case 0xBE:
					case 0xBF: // PushW[abc]
					{
						byte count = (byte)((b & 0x07) + 1);
						uint[] vals = new uint[count];
						for (uint i = 0; i < count; i++)
						{
							vals[i] = rdr.ReadUInt16();
						}
						Instructions.Add(new PushW(vals));
						break;
					}
					#endregion

					#region Graphics State Management

					#region RS[]
					case 0x43: // RS[]
					{
						Instructions.Add(new RS());
						break;
					}
					#endregion

					#region WS[]
					case 0x42: // WS[]
					{
						Instructions.Add(new WS());
						break;
					}
					#endregion

					#region WCvtP[]
					case 0x44: // WCvtP[]
					{
						Instructions.Add(new WCvtP());
						break;
					}
					#endregion

					#region WCvtF[]
					case 0x70: // WCvtF[]
					{
						Instructions.Add(new WCvtF());
						break;
					}
					#endregion

					#region RCvt[]
					case 0x45: // RCvt[]
					{
						Instructions.Add(new RCvt());
						break;
					}
					#endregion

					#region SVTCA[a]
					case 0x00:
					case 0x01: // SVTCA[a]
					{
						Instructions.Add(new SVTCA(b));
						break;
					}
					#endregion

					#region SPVTCA[a]
					case 0x02:
					case 0x03: // SPVTCA[a]
					{
						Instructions.Add(new SPVTCA(b));
						break;
					}
					#endregion

					#region SFVTCA[a]
					case 0x04:
					case 0x05: // SFVTCA[a]
					{
						Instructions.Add(new SFVTCA(b));
						break;
					}
					#endregion

					#region SRP0[]
					case 0x10: // SRP0[]
					{
						Instructions.Add(new SRP0());
						break;
					}
					#endregion

					#region SRP1[]
					case 0x11: // SRP1[]
					{
						Instructions.Add(new SRP1());
						break;
					}
					#endregion

					#region SRP2[]
					case 0x12: // SRP2[]
					{
						Instructions.Add(new SRP2());
						break;
					}
					#endregion

					#region SZP0[]
					case 0x13: // SZP0[]
					{
						Instructions.Add(new SZP0());
						break;
					}
					#endregion

					#region SZP1[]
					case 0x14: // SZP1[]
					{
						Instructions.Add(new SZP1());
						break;
					}
					#endregion

					#region SZP2[]
					case 0x15: // SZP2[]
					{
						Instructions.Add(new SZP2());
						break;
					}
					#endregion

                    #region SPVFS[]
                    case 0x0A: // SPVFS[]
                    {
						Instructions.Add(new SPVFS());
                        break;
                    }
                    #endregion

                    #region SFVFS[]
                    case 0x0B: // SPVFS[]
                    {
						Instructions.Add(new SFVFS());
                        break;
                    }
                    #endregion

                    #region SFVTL[a]
                    case 0x08:
                    case 0x09: // SFVTL[a]
                    {
						Instructions.Add(new SFVTL(b));
                        break;
                    }
                    #endregion

                    #region SFVTPV[]
                    case 0x0E: // SFVTPV
                    {
						Instructions.Add(new SFVTPV());
                        break;
                    }
                    #endregion

                    #region SPVTL[a]
                    case 0x06:
                    case 0x07: // SPVTL[a]
                    {
						Instructions.Add(new SPVTL(b));
                        break;
                    }
                    #endregion

                    #region SDPVTL[a]
                    case 0x86:
                    case 0x87: // SDPVTL[a]
                    {
						Instructions.Add(new SDPVTL(b));
                        break;
                    }
                    #endregion

                    #region SZPS[]
                    case 0x16: //SZPS[]
                    {
						Instructions.Add(new SZPS());
                        break;
                    }
                    #endregion

                    #region SLoop[]
                    case 0x17: // SLoop[]
                    {
						Instructions.Add(new SLoop());
                        break;
                    }
                    #endregion

                    #region GFV[]
                    case 0x0D: // GFV[]
                    {
						Instructions.Add(new GFV());
                        break;
                    }
                    #endregion

                    #region GPV[]
                    case 0x0C: // GPV[]
                    {
						Instructions.Add(new GPV());
                        break;
                    }
                    #endregion

                    #region MPPEM[]
                    case 0x4B: // MPPEM[]
                    {
						Instructions.Add(new MPPEM());
                        break;
                    }
                    #endregion

                    #region MPS[]
                    case 0x4C: // MPS[]
                    {
						Instructions.Add(new MPS());
                        break;
                    }
                    #endregion

                    #region ScanCtrl[]
                    case 0x85: // ScanCtrl[]
                    {
						Instructions.Add(new ScanCtrl());
                        break;
                    }
                    #endregion

                    #region ScanType[]
                    case 0x8D: // ScanType[]
                    {
						Instructions.Add(new ScanType());
                        break;
                    }
                    #endregion

                    #region GetInfo[]
                    case 0x88: // GetInfo[]
                    {
						Instructions.Add(new GetInfo());
                        break;
                    }
                    #endregion

                    #region InstCtrl[]
                    case 0x8E: // InstCtrl[]
                    {
						Instructions.Add(new InstCtrl());
                        break;
                    }
                    #endregion

                    #region SMD[]
                    case 0x1A: // SMD[]
                    {
						Instructions.Add(new SMD());
                        break;
                    }
                    #endregion

                    #region SCVTCI[]
                    case 0x1D: // SCVTCI[]
                    {
						Instructions.Add(new SCVTCI());
                        break;
                    }
                    #endregion

                    #region FlipOff[]
                    case 0x4E: // FlipOff[]
                    {
						Instructions.Add(new FlipOff());
                        break;
                    }
                    #endregion

                    #region FlipOn[]
                    case 0x4D: // FlipOn[]
                    {
						Instructions.Add(new FlipOn());
                        break;
                    }
                    #endregion

                    #region SDB[]
                    case 0x5E: // SDB[]
                    {
						Instructions.Add(new SDB());
                        break;
                    }
                    #endregion
						
                    #region SDS[]
                    case 0x5F: // SDS[]
                    {
						Instructions.Add(new SDS());
                        break;
                    }
                    #endregion

                    #endregion

                    #region GC[a]
                    case 0x46:
                    case 0x47: // GC[a]
                    {
						Instructions.Add(new Instructions.GC(b));
                        break;
                    }
                    #endregion

                    #region SCFS[]
                    case 0x48: // SCFS[]
                    {
						Instructions.Add(new SCFS());
                        break;
                    }
                    #endregion

                    #region AlignRP[]
                    case 0x3C: // AlignRP[]
                    {
						Instructions.Add(new AlignRP());
                        break;
                    }
                    #endregion

                    #region MIAP[a]
                    case 0x3E:
                    case 0x3F: // MIAP[a]
                    {
						Instructions.Add(new MIAP(b));
                        break;
                    }
                    #endregion

                    #region MSIRP[a]
                    case 0x3A:
                    case 0x3B: // MSIRP[a]
                    {
						Instructions.Add(new MSIRP(b));
                        break;
                    }
                    #endregion

                    #region MIRP[abcde]
                    case 0xE0:
                    case 0xE1:
                    case 0xE2:
                    case 0xE3:
                    case 0xE4:
                    case 0xE5:
                    case 0xE6:
                    case 0xE7:
                    case 0xE8:
                    case 0xE9:
                    case 0xEA:
                    case 0xEB:
                    case 0xEC:
                    case 0xED:
                    case 0xEE:
                    case 0xEF:
                    case 0xF0:
                    case 0xF1:
                    case 0xF2:
                    case 0xF3:
                    case 0xF4:
                    case 0xF5:
                    case 0xF6:
                    case 0xF7:
                    case 0xF8:
                    case 0xF9:
                    case 0xFA:
                    case 0xFB:
                    case 0xFC:
                    case 0xFD:
                    case 0xFE:
                    case 0xFF: // MIRP[abcde]
                    {
						Instructions.Add(new MIRP(b));
                        break;
                    }
                    #endregion

                    #region MDRP[abcde]
                    case 0xC0:
                    case 0xC1:
                    case 0xC2:
                    case 0xC3:
                    case 0xC4:
                    case 0xC5:
                    case 0xC6:
                    case 0xC7:
                    case 0xC8:
                    case 0xC9:
                    case 0xCA:
                    case 0xCB:
                    case 0xCC:
                    case 0xCD:
                    case 0xCE:
                    case 0xCF:
                    case 0xD0:
                    case 0xD1:
                    case 0xD2:
                    case 0xD3:
                    case 0xD4:
                    case 0xD5:
                    case 0xD6:
                    case 0xD7:
                    case 0xD8:
                    case 0xD9:
                    case 0xDA:
                    case 0xDB:
                    case 0xDC:
                    case 0xDD:
                    case 0xDE:
                    case 0xDF: // MDRP[abcde]
                    {
						Instructions.Add(new MDRP(b));
                        break;
                    }
                    #endregion
						
                    #region MDAP[a]
                    case 0x2E:
                    case 0x2F: // MDAP[a]
                    {
						Instructions.Add(new MDAP(b));
                        break;
                    }
                    #endregion

                    #region ISect[]
                    case 0x0F: // ISect[]
                    {
						Instructions.Add(new ISect());
                        break;
                    }
                    #endregion

                    #region ShPix[]
                    case 0x38: // ShPix[]
                    {
						Instructions.Add(new ShPix());
                        break;
                    }
                    #endregion

                    #region ShC[a]
                    case 0x34:
                    case 0x35: // ShC[a]
                    {
						Instructions.Add(new ShC(b));
                        break;
                    }
                    #endregion

                    #region ShP[a]
                    case 0x32:
                    case 0x33: // ShP[a]
                    {
						Instructions.Add(new ShP(b));
                        break;
                    }
                    #endregion

                    #region IP[]
                    case 0x39: // IP[]
                    {
						Instructions.Add(new IP());
                        break;
                    }
                    #endregion

                    #region IUP[a]
                    case 0x30:
                    case 0x31: // IUP[a]
                    {
						Instructions.Add(new IUP(b));
                        break;
                    }
                    #endregion

                    #region MD[a]
                    case 0x49:
                    case 0x4A: // MD[a]
                    {
						Instructions.Add(new MD(b));
                        break;
                    }
                    #endregion

                    #region DeltaP1[]
                    case 0x5D: // DeltaP1[]
                    {
						Instructions.Add(new DeltaP1());
                        break;
                    }
                    #endregion

                    #region DeltaP2[]
                    case 0x71: // DeltaP2[]
                    {
						Instructions.Add(new DeltaP2());
                        break;
                    }
                    #endregion

                    #region DeltaP3[]
                    case 0x72: // DeltaP3[]
                    {
						Instructions.Add(new DeltaP3());
                        break;
                    }
                    #endregion

                    #region DeltaC1[]
                    case 0x73: // DeltaC1[]
                    {
						Instructions.Add(new DeltaC1());
                        break;
                    }
                    #endregion

                    #region DeltaC2[]
                    case 0x74: // DeltaC2[]
                    {
						Instructions.Add(new DeltaC2());
                        break;
                    }
                    #endregion

                    #region DeltaC3[]
                    case 0x75: // DeltaC3[]
                    {
						Instructions.Add(new DeltaC3());
                        break;
                    }
                    #endregion

                    #region FlipRgOn[]
                    case 0x81: // FlipRgOn[]
                    {
						Instructions.Add(new FlipRgOn());
                        break;
                    }
                    #endregion

                    #region FlipRgOff[]
                    case 0x82: // FlipRgOff[]
                    {
						Instructions.Add(new FlipRgOff());
                        break;
                    }
                    #endregion

                    #region FlipPt[]
                    case 0x80: // FlipPt[]
                    {
						Instructions.Add(new FlipPt());
                        break;
                    }
                    #endregion


                    #region Rounding
					
                    #region RTG[]
                    case 0x18: // RTG[]
                    {
						Instructions.Add(new RTG());
                        break;
                    }
                    #endregion

                    #region RDTG[]
                    case 0x7D: // RDTG[]
                    {
						Instructions.Add(new RDTG());
                        break;
                    }
                    #endregion

                    #region RTHG[]
                    case 0x19: // RTHG[]
                    {
						Instructions.Add(new RTHG());
                        break;
                    }
                    #endregion

                    #region RTDG[]
                    case 0x3D: // RTDG[]
                    {
						Instructions.Add(new RTDG());
                        break;
                    }
                    #endregion

                    #region RUTG[]
                    case 0x7C: // RUTG[]
                    {
						Instructions.Add(new RUTG());
                        break;
                    }
                    #endregion

                    #region S45Round[]
                    case 0x77: // S45Round[]
                    {
						Instructions.Add(new S45Round());
                        break;
                    }
                    #endregion

                    #region SRound[]
                    case 0x76: // SRound[]
                    {
						Instructions.Add(new SRound());
                        break;
                    }
                    #endregion

                    #region ROff[]
                    case 0x7A: // ROff[]
                    {
						Instructions.Add(new ROff());
                        break;
                    }
                    #endregion

                    #region Round[ab]
                    case 0x68:
                    case 0x69:
                    case 0x6A:
                    case 0x6B: // Round[ab]
                    {
						Instructions.Add(new Round(b));
                        break;
                    }
                    #endregion

                    #endregion


                    #region Function Definitions

                    #region EndF[]
                    case 0x2D: // EndF[]
                    {
						Instructions.Add(new EndF());
						break;
//#if EnableInstructionConsoleWrite
//                        gen.EmitWriteLine("Instruction #" + curInstructionNumber.ToString());
//                        curInstructionNumber++;
//#endif
//#if Debugging
//                        curIdent = curIdent.Substring(0, curIdent.Length - 4);
//                        SWriteLine(tOut, "EndF");
//                        SRefVal(ref curInstructionLength, 4);
//                        gen.MarkSequencePoint(Swrtr, prevLineNumber, curIdent.Length + 1, curLineNumber, curIdent.Length + curInstructionLength + 1);
//                        curLineNumber++;
//                        prevLineNumber = curLineNumber;
//#endif
                    }
                    #endregion

                    #region FDef[]
                    case 0x2C: // FDef[]
                    {
						Instructions.Add(new FDef());
//                        MethodBuilder mBldr = tBldr.DefineMethod("Function_" + CurFunctionNumber.ToString(), PublicStaticMethodAttributes, typeof(void), new Type[] { typeof(GraphicsState) });
//                        CurFunctionNumber++;
//                        ILGenerator gen2 = mBldr.GetILGenerator();
//#if Debugging
//                        mBldr.DefineParameter(1, ParameterAttributes.None, "gState");
//                        SWriteLine(tOut, "FDef");
//                        SRefVal(ref curInstructionLength, 4);
//                        curIdent += "".PadLeft(4, ' ');
//                        gen.MarkSequencePoint(Swrtr, prevLineNumber, curIdent.Length + 1, curLineNumber, curIdent.Length + curInstructionLength + 1);
//                        curLineNumber++;
//                        prevLineNumber = curLineNumber;
//                        WasFDef = true;

//                        ReadMethod(gen2, strm, tBldr, false, Swrtr, tOut);
						
//#else
//                        ReadMethod(gen2, strm, tBldr, false);
//#endif
//                        gen2.Emit(OpCodes.Ret);

//                        // Method read & created, now load it.
//                        LoadGraphicsState(gen, IRbldr);
//                        gen.Emit(OpCodes.Ldfld, GraphicsState_Functions);
//                        LoadGraphicsState(gen, IRbldr);
//                        gen.Emit(OpCodes.Ldfld, GraphicsState_Stack);
//                        gen.Emit(OpCodes.Call, LinkedStack_Pop);
//                        gen.Emit(OpCodes.Ldnull);
//                        gen.Emit(OpCodes.Ldftn, mBldr);
//                        gen.Emit(OpCodes.Newobj, typeof(HintingMethod).GetConstructor(new Type[] { typeof(object), typeof(IntPtr) }));
//                        gen.Emit(OpCodes.Callvirt, typeof(Dictionary<int, HintingMethod>).GetMethod("Add"));
                        break;
                    }
                    #endregion

                    #region Call[]
                    case 0x2B: // Call[]
                    {
						Instructions.Add(new Call());
                        break;
                    }
                    #endregion

                    #region LoopCall[]
                    case 0x2A: // LoopCall[]
                    {
						Instructions.Add(new LoopCall());
                        break;
                    }
                    #endregion

                    #endregion


                    #region Math Operators

                    #region Abs[]
                    case 0x64: // Abs[]
                    {
						Instructions.Add(new Abs());
                        break;
                    }
                    #endregion

                    #region Add[]
                    case 0x60: // Add[]
                    {
						Instructions.Add(new Add());
                        break;
                    }
                    #endregion

                    #region Ceiling[]
                    case 0x67: // Ceiling[]
                    {
						Instructions.Add(new Ceiling());
                        break;
                    }
                    #endregion

                    #region Div[]
                    case 0x62: // Div[]
                    {
						Instructions.Add(new Div());
                        break;
                    }
                    #endregion
					
                    #region Even[]
                    case 0x57: // Even[]
                    {
						Instructions.Add(new Even());
                        break;
                    }
                    #endregion

                    #region Floor[]
                    case 0x66: // Floor[]
                    {
						Instructions.Add(new Floor());
                        break;
                    }
                    #endregion
						
                    #region Max[]
                    case 0x8B: // Max[]
                    {
						Instructions.Add(new Max());
                        break;
                    }
                    #endregion
						
                    #region Min[]
                    case 0x8C: // Min[]
                    {
						Instructions.Add(new Min());
                        break;
                    }
                    #endregion
						
                    #region Mul[]
                    case 0x63: // Mul[]
                    {
						Instructions.Add(new Mul());
                        break;
                    }
                    #endregion

                    #region Neg[]
                    case 0x65: // Neg[]
                    {
						Instructions.Add(new Neg());
                        break;
                    }
                    #endregion
						
                    #region Odd[]
                    case 0x56: // Odd[]
                    {
						Instructions.Add(new Odd());
                        break;
                    }
                    #endregion
						
                    #region Sub[]
                    case 0x61: // Sub[]
                    {
						Instructions.Add(new Sub());
                        break;
                    }
                    #endregion
					
                    #endregion


                    #region Comparisons

                    #region EQ[]
                    case 0x54: // EQ[]
                    {
						Instructions.Add(new EQ());
                        break;
                    }
                    #endregion

                    #region NEQ[]
                    case 0x55: // NEQ[]
                    {
						Instructions.Add(new NEQ());
                        break;
                    }
                    #endregion

                    #region GT[]
                    case 0x52: // GT[]
                    {
						Instructions.Add(new GT());
                        break;
                    }
                    #endregion

                    #region GTEQ[]
                    case 0x53: // GTEQ[]
                    {
						Instructions.Add(new GTEQ());
                        break;
                    }
                    #endregion

                    #region LT[]
                    case 0x50: // LT[]
                    {
						Instructions.Add(new LT());
                        break;
                    }
                    #endregion
						
                    #region LTEQ[]
                    case 0x51: // LTEQ[]
                    {
						Instructions.Add(new LTEQ());
                        break;
                    }
                    #endregion

                    #endregion
						

                    #region Bitwise Operations

                    #region And[]
                    case 0x5A: // And[]
                    {
						Instructions.Add(new And());
                        break;
                    }
                    #endregion

                    #region Or[]
                    case 0x5B: // Or[]
                    {
						Instructions.Add(new Or());
                        break;
                    }
                    #endregion
						
                    #region Not[]
                    case 0x5C: // Not[]
                    {
						Instructions.Add(new Not());
                        break;
                    }
                    #endregion

                    #endregion


                    #region Branching Conditionals
						
                    #region If[]
                    case 0x58: // If[]
                    {
						Instructions.Add(new If());
                        break;
                    }
                    #endregion

                    #region Else[]
                    case 0x1B: // Else[]
                    {
						Instructions.Add(new Else());
                        break;
                    }
                    #endregion

                    #region EIf[]
                    case 0x59: // EIf[]
                    {
						Instructions.Add(new EIf());
                        break;
                    }
                    #endregion
						
                    #region JmpR[]
                    case 0x1C: // JmpR[]
                    {
						Instructions.Add(new JmpR());
                        break;
                    }
                    #endregion

                    #region JROT[]
                    case 0x78: // JROT[]
                    {
						Instructions.Add(new JROT());
                        break;
                    }
                    #endregion

                    #region JROF[]
                    case 0x79: // JROF[]
                    {
						Instructions.Add(new JROF());
                        break;
                    }
                    #endregion

                    #endregion


                    #region Stack Management
						
                    #region CIndex[]
                    case 0x25: // CIndex[]
                    {
						Instructions.Add(new CIndex());
                        break;
                    }
                    #endregion

                    #region Clear[]
                    case 0x22: // Clear[]
                    {
						Instructions.Add(new Clear());
                        break;
                    }
                    #endregion

                    #region Depth[]
                    case 0x24: // Depth[]
                    {
						Instructions.Add(new Depth());
                        break;
                    }
                    #endregion

                    #region Dup[]
                    case 0x20: // Dup[]
                    {
						Instructions.Add(new Dup());
                        break;
                    }
                    #endregion

                    #region MIndex[]
                    case 0x26: // MIndex[]
                    {
						Instructions.Add(new MIndex());
                        break;
                    }
                    #endregion

                    #region Pop[]
                    case 0x21: // Pop[]
                    {
						Instructions.Add(new Pop());
                        break;
                    }
                    #endregion

                    #region Roll[]
                    case 0x8A: // Roll[]
                    {
						Instructions.Add(new Roll());
                        break;
                    }
                    #endregion

                    #region Swap[]
                    case 0x23: // Swap[]
                    {
						Instructions.Add(new Swap());
                        break;
                    }
                    #endregion

                    #endregion


					case 0x89: // IDef[]
						throw new Exception("Custom instruction definitions aren't currently supported!");
					default: 
						//break;
						throw new Exception("Unknown op-code '0x" + b.ToString("X") + "'!");
				}

				Instructions[Instructions.Count - 1].PositionInStream = startPos;
				Instructions[Instructions.Count - 1].ParentStream = strm;
			}
		}
	}
}
