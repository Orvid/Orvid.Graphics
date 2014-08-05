//#define Trace
using System;
using System.IO;
using System.Text;
using Orvid.TrueType.Utils;
using System.Collections.Generic;

namespace Orvid.TrueType
{
	public class TrueTypeInterpreter
	{
		public static void ExecuteMethod(byte[] instructions, GraphicsState gState)
		{
			TrueTypeInterpreter ttintp = new TrueTypeInterpreter();
			ttintp.stack = gState.Stack;
			//StreamWriter strm = new StreamWriter("interp_" + new Random().Next(40).ToString());
			//for (int i = 0; i < instructions.Length; i++)
			//{
			//    strm.WriteLine(instructions[i].ToString().PadRight(4, ' ') + " (0x" + instructions[i].ToString("X") + ")");
			//}
			//strm.Close();
			//strm.Dispose();
			ttintp.Execute(new BigEndianBinaryReader(new MemoryStream(instructions)), gState);
		}

		public LinkedStack<int> stack;
		public bool Executing = true;
		public int ifDepth = 0;

		public void Execute(BigEndianBinaryReader rdr, GraphicsState gState)
		{
#if Trace
			StreamWriter tOut = new StreamWriter("InterpreterTrace.txt");
#endif
			while (rdr.BaseStream.Position < rdr.BaseStream.Length)
			{
				byte b = rdr.ReadByte();
				if (!Executing)
				{
#if Trace
					tOut.WriteLine("Not executing at 0x" + rdr.BaseStream.Position.ToString() + " the instruction byte 0x" + b.ToString("X"));
#endif
					switch (b)
					{
						#region Multi-Byte instructions
						case 0x40: // NPushB[]
						{
							byte cnt = rdr.ReadByte();
							rdr.BaseStream.Position += cnt;
							break;
						}
						case 0x41: // NPushW[]
						{
							byte cnt = rdr.ReadByte();
							rdr.BaseStream.Position += cnt << 1;
							break;
						}
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
							rdr.BaseStream.Position += count;
							break;
						}
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
							rdr.BaseStream.Position += count << 1;
							break;
						}
						#endregion
							
						#region If[]
						case 0x58: // If[]
						{
							ifDepth++;
							break;
						}
						#endregion

						#region Else[]
						case 0x1B: // Else[]
						{
							if (ifDepth == 1)
							{
								Executing = true;
							}
							break;
						}
						#endregion

						#region EIf[]
						case 0x59: // EIf[]
						{
							ifDepth--;
							if (ifDepth == 0)
							{
								Executing = true;
							}
							break;
						}
						#endregion
							
						default:
							break;
					}
				}
				else
				{
#if Trace
					tOut.WriteLine("Executing at 0x" + rdr.BaseStream.Position.ToString() + " the instruction byte 0x" + b.ToString("X"));
#endif
					switch (b)
					{
						#region NPushB[]
						case 0x40: // NPushB[]
						{
							byte cnt = rdr.ReadByte();
							for (uint i = 0; i < cnt; i++)
							{
								stack.Push((int)rdr.ReadByte());
							}
							break;
						}
						#endregion
							
						#region NPushW[]
						case 0x41: // NPushW[]
						{
							byte cnt = rdr.ReadByte();
							for (uint i = 0; i < cnt; i++)
							{
								stack.Push((int)(short)rdr.ReadUInt16());
							}
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
							for (uint i = 0; i < count; i++)
							{
								stack.Push(rdr.ReadByte());
							}
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
							for (uint i = 0; i < count; i++)
							{
								stack.Push((int)(short)rdr.ReadUInt16());
							}
							break;
						}
						#endregion

						#region Graphics State Management

						#region RS[]
						case 0x43: // RS[]
						{
							stack.Push(gState.Storage[stack.Pop()]);
							break;
						}
						#endregion

						#region WS[]
						case 0x42: // WS[]
						{
							int val = stack.Pop();
							int idx = stack.Pop();
							//if (idx == 9)
							//    throw new Exception();
							gState.Storage[idx] = val;
							break;
						}
						#endregion

						#region WCvtP[]
						case 0x44: // WCvtP[]
						{
							int val = stack.Pop();
							int idx = stack.Pop();
							GraphicsState.WriteCvtEntry(idx, F26Dot6.FromLiteral(val), gState);
							break;
						}
						#endregion

						#region WCvtF[]
						case 0x70: // WCvtF[]
						{
							int val = stack.Pop();
							int idx = stack.Pop();
							GraphicsState.WriteCvtEntryF(val, idx, gState);
							break;
						}
						#endregion

						#region RCvt[]
						case 0x45: // RCvt[]
						{
							int entryNum = stack.Pop();
							F26Dot6 val = gState.GetCvtEntry(entryNum);
							stack.Push(F26Dot6.AsLiteral(val));
							break;
						}
						#endregion

						#region SVTCA[a]
						case 0x00:
						case 0x01: // SVTCA[a]
						{
							if ((b & 0x01) != 0)
							{
								gState.Projection_Vector = VecF2Dot14.Axis_X;
								gState.Freedom_Vector = VecF2Dot14.Axis_X;
								gState.Dual_Projection_Vector = VecF2Dot14.Axis_X;
							}
							else
							{
								gState.Projection_Vector = VecF2Dot14.Axis_Y;
								gState.Freedom_Vector = VecF2Dot14.Axis_Y;
								gState.Dual_Projection_Vector = VecF2Dot14.Axis_Y;
							}
							gState.RecalcProjFreedomDotProduct();
							break;
						}
						#endregion

						#region SPVTCA[a]
						case 0x02:
						case 0x03: // SPVTCA[a]
						{
							if ((b & 0x01) != 0)
							{
								gState.Dual_Projection_Vector = VecF2Dot14.Axis_X;
								gState.Projection_Vector = VecF2Dot14.Axis_X;
							}
							else
							{
								gState.Dual_Projection_Vector = VecF2Dot14.Axis_Y;
								gState.Projection_Vector = VecF2Dot14.Axis_Y;
							}
							gState.RecalcProjFreedomDotProduct();
							break;
						}
						#endregion

						#region SFVTCA[a]
						case 0x04:
						case 0x05: // SFVTCA[a]
						{
							if ((b & 0x01) != 0)
							{
								gState.Freedom_Vector = VecF2Dot14.Axis_X;
							}
							else
							{
								gState.Freedom_Vector = VecF2Dot14.Axis_Y;
							}
							gState.RecalcProjFreedomDotProduct();
							break;
						}
						#endregion

						#region SRP0[]
						case 0x10: // SRP0[]
						{
							gState.rp0 = stack.Pop();
							break;
						}
						#endregion

						#region SRP1[]
						case 0x11: // SRP1[]
						{
							gState.rp1 = stack.Pop();
							break;
						}
						#endregion

						#region SRP2[]
						case 0x12: // SRP2[]
						{
							gState.rp2 = stack.Pop();
							break;
						}
						#endregion

						#region SZP0[]
						case 0x13: // SZP0[]
						{
							gState.gep0 = stack.Pop();
							gState.SetZonePtrs();
							break;
						}
						#endregion

						#region SZP1[]
						case 0x14: // SZP1[]
						{
							gState.gep1 = stack.Pop();
							gState.SetZonePtrs();
							break;
						}
						#endregion

						#region SZP2[]
						case 0x15: // SZP2[]
						{
							gState.gep2 = stack.Pop();
							gState.SetZonePtrs();
							break;
						}
						#endregion

						#region SPVFS[]
						case 0x0A: // SPVFS[]
						{
							F2Dot14 y = F2Dot14.FromLiteral(stack.Pop());
							F2Dot14 x = F2Dot14.FromLiteral(stack.Pop());
							gState.SetProjectionVector(y, x);
							break;
						}
						#endregion

						#region SFVFS[]
						case 0x0B: // SPVFS[]
						{
							F2Dot14 y = F2Dot14.FromLiteral(stack.Pop());
							F2Dot14 x = F2Dot14.FromLiteral(stack.Pop());
							gState.SetFreedomVector(y, x);
							break;
						}
						#endregion

						#region SFVTL[a]
						case 0x08:
						case 0x09: // SFVTL[a]
						{
							int p2 = stack.Pop();
							int p1 = stack.Pop();
							GraphicsState.SetFreedomVectorToLine(p2, p1, ((b & 0x01) != 0), gState);
							break;
						}
						#endregion

						#region SFVTPV[]
						case 0x0E: // SFVTPV
						{
							gState.Freedom_Vector = gState.Projection_Vector;
							gState.RecalcProjFreedomDotProduct();
							break;
						}
						#endregion

						#region SPVTL[a]
						case 0x06:
						case 0x07: // SPVTL[a]
						{
							int p2 = stack.Pop();
							int p1 = stack.Pop();
							GraphicsState.SetProjectionVectorToLine(p2, p1, ((b & 0x01) != 0), gState);
							break;
						}
						#endregion

						#region SDPVTL[a]
						case 0x86:
						case 0x87: // SDPVTL[a]
						{
							int p2 = stack.Pop();
							int p1 = stack.Pop();
							GraphicsState.SetDualProjectionVectorToLine(p2, p1, ((b & 0x01) != 0), gState);
							break;
						}
						#endregion

						#region SZPS[]
						case 0x16: //SZPS[]
						{
							int val = stack.Pop();
							gState.gep0 = val;
							gState.gep1 = val;
							gState.gep2 = val;
							gState.SetZonePtrs();
							break;
						}
						#endregion

						#region SLoop[]
						case 0x17: // SLoop[]
						{
							gState.Loop = stack.Pop();
							break;
						}
						#endregion

						#region GFV[]
						case 0x0D: // GFV[]
						{
							stack.Push(F2Dot14.AsLiteral(gState.Freedom_Vector.local_x));
							stack.Push(F2Dot14.AsLiteral(gState.Freedom_Vector.local_y));
							break;
						}
						#endregion

						#region GPV[]
						case 0x0C: // GPV[]
						{
							stack.Push(F2Dot14.AsLiteral(gState.Projection_Vector.local_x));
							stack.Push(F2Dot14.AsLiteral(gState.Projection_Vector.local_y));
							break;
						}
						#endregion

						#region MPPEM[]
						case 0x4B: // MPPEM[]
						{
							stack.Push((int)(short)gState.GetPixelsPerEM());
							break;
						}
						#endregion

						#region MPS[]
						case 0x4C: // MPS[]
						{
							stack.Push((int)gState.SizeInPoints);
							break;
						}
						#endregion

						#region ScanCtrl[]
						case 0x85: // ScanCtrl[]
						{
							stack.Pop();
							Console.WriteLine("WARNING: ScanCtrl doesn't do anything yet!");
							break;
						}
						#endregion

						#region ScanType[]
						case 0x8D: // ScanType[]
						{
							gState.ScanControl = stack.Pop();
							break;
						}
						#endregion

						#region GetInfo[]
						case 0x88: // GetInfo[]
						{
							stack.Push(gState.GetInfo(stack.Pop()));
							break;
						}
						#endregion

						#region InstCtrl[]
						case 0x8E: // InstCtrl[]
						{
							stack.Pop();
							stack.Pop();
							Console.WriteLine("WARNING: InstCtrl[] doesn't currently do anything!");
							break;
						}
						#endregion

						#region SMD[]
						case 0x1A: // SMD[]
						{
							gState.Minimum_Distance = F26Dot6.FromLiteral(stack.Pop());
							break;
						}
						#endregion

						#region SCVTCI[]
						case 0x1D: // SCVTCI[]
						{
							gState.Control_Value_Cut_In = F26Dot6.FromLiteral(stack.Pop());
							break;
						}
						#endregion

						#region FlipOff[]
						case 0x4E: // FlipOff[]
						{
							gState.Auto_Flip = false;
							break;
						}
						#endregion

						#region FlipOn[]
						case 0x4D: // FlipOn[]
						{
							gState.Auto_Flip = true;
							break;
						}
						#endregion

						#region SDB[]
						case 0x5E: // SDB[]
						{
							gState.Delta_Base = stack.Pop();
							break;
						}
						#endregion
							
						#region SDS[]
						case 0x5F: // SDS[]
						{
							gState.Delta_Shift = stack.Pop();
							break;
						}
						#endregion

						#endregion

						#region GC[a]
						case 0x46:
						case 0x47: // GC[a]
						{
							stack.Push(F26Dot6.AsLiteral(GraphicsState.GetCoords(stack.Pop(), ((b & 0x01) != 0) ? true : false, gState)));
							break;
						}
						#endregion

						#region SCFS[]
						case 0x48: // SCFS[]
						{
							F26Dot6 dist = F26Dot6.FromLiteral(stack.Pop());
							int pNum = stack.Pop();
							gState.SetCoords(dist, pNum);
							break;
						}
						#endregion

						#region AlignRP[]
						case 0x3C: // AlignRP[]
						{
							for (int i = 0; i < gState.Loop; i++)
							{
								gState.AlignToReferencePoint(stack.Pop());
							}
							gState.Loop = 1;
							break;
						}
						#endregion

						#region MIAP[a]
						case 0x3E:
						case 0x3F: // MIAP[a]
						{
							int cNum = stack.Pop();
							int pNum = stack.Pop();
							GraphicsState.MoveIndirectAbsolutePoint(cNum, pNum, ((b & 0x01) != 0) ? true : false, gState);
							break;
						}
						#endregion

						#region MSIRP[a]
						case 0x3A:
						case 0x3B: // MSIRP[a]
						{
							F26Dot6 dist = F26Dot6.FromLiteral(stack.Pop());
							int pNum = stack.Pop();
							GraphicsState.MoveStackIndirectRelativePoint(dist, pNum, ((b & 0x01) != 0) ? true : false, gState);
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
							int cvtVal = stack.Pop();
							int pNum = stack.Pop();
							GraphicsState.MoveIndirectRelativePoint(cvtVal, pNum, ((b & (1 << 2)) != 0), ((b & (1 << 3)) != 0), ((b & (1 << 4)) != 0), (DistanceType)(b & 0x03), gState);
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
							int pNum = stack.Pop();
							GraphicsState.MoveDirectRelativePoint(pNum, ((b & (1 << 2)) != 0), ((b & (1 << 3)) != 0), ((b & (1 << 4)) != 0), (DistanceType)(b & 0x03), gState);
							break;
						}
						#endregion
							
						#region MDAP[a]
						case 0x2E:
						case 0x2F: // MDAP[a]
						{
							GraphicsState.MoveDirectAbsolutePoint(stack.Pop(), ((b & 0x01) != 0) ? true : false, gState);
							break;
						}
						#endregion

						#region ISect[]
						case 0x0F: // ISect[]
						{
							GraphicsState.ISect(gState);
							break;
						}
						#endregion

						#region ShPix[]
						case 0x38: // ShPix[]
						{
							GraphicsState.LoopedShiftPixel(F26Dot6.FromLiteral(stack.Pop()), gState);
							break;
						}
						#endregion

						#region ShC[a]
						case 0x34:
						case 0x35: // ShC[a]
						{
							GraphicsState.ShiftContour(stack.Pop(), ((b & 0x01) != 0), gState);
							break;
						}
						#endregion

						#region ShP[a]
						case 0x32:
						case 0x33: // ShP[a]
						{
							GraphicsState.LoopedShiftPoint(((b & 0x01) != 0), gState);
							break;
						}
						#endregion

						#region IP[]
						case 0x39: // IP[]
						{
							GraphicsState.InterpolatePoints(gState);
							break;
						}
						#endregion

						#region IUP[a]
						case 0x30:
						case 0x31: // IUP[a]
						{
							GraphicsState.InterpolateUntouchedPoints(((b & 0x01) != 0), gState);
							break;
						}
						#endregion

						#region MD[a]
						case 0x49:
						case 0x4A: // MD[a]
						{
							int pA = stack.Pop();
							int pB = stack.Pop();
							stack.Push(F26Dot6.AsLiteral(GraphicsState.MeasureDistance(pA, pB, ((b & 0x01) == 0), gState)));
							break;
						}
						#endregion

						#region DeltaP1[]
						case 0x5D: // DeltaP1[]
						{
							gState.AddPointExceptions(0);
							break;
						}
						#endregion

						#region DeltaP2[]
						case 0x71: // DeltaP2[]
						{
							gState.AddPointExceptions(1);
							break;
						}
						#endregion

						#region DeltaP3[]
						case 0x72: // DeltaP3[]
						{
							gState.AddPointExceptions(2);
							break;
						}
						#endregion

						#region DeltaC1[]
						case 0x73: // DeltaC1[]
						{
							gState.AddCvtExceptions(0);
							break;
						}
						#endregion

						#region DeltaC2[]
						case 0x74: // DeltaC2[]
						{
							gState.AddCvtExceptions(1);
							break;
						}
						#endregion

						#region DeltaC3[]
						case 0x75: // DeltaC3[]
						{
							gState.AddCvtExceptions(2);
							break;
						}
						#endregion

						#region FlipRgOn[]
						case 0x81: // FlipRgOn[]
						{
							int end = stack.Pop();
							int start = stack.Pop();
							gState.SetRangeOnCurve(end, start);
							break;
						}
						#endregion

						#region FlipRgOff[]
						case 0x82: // FlipRgOff[]
						{
							int end = stack.Pop();
							int start = stack.Pop();
							gState.SetRangeOffCurve(end, start);
							break;
						}
						#endregion

						#region FlipPt[]
						case 0x80: // FlipPt[]
						{
							gState.LoopedFlipOnCurve();
							break;
						}
						#endregion


						#region Rounding
						
						#region RTG[]
						case 0x18: // RTG[]
						{
							gState.RoundMode = RoundingMode.Grid;
							break;
						}
						#endregion

						#region RDTG[]
						case 0x7D: // RDTG[]
						{
							gState.RoundMode = RoundingMode.Down_To_Grid;
							break;
						}
						#endregion

						#region RTHG[]
						case 0x19: // RTHG[]
						{
							gState.RoundMode = RoundingMode.Half_Grid;
							break;
						}
						#endregion

						#region RTDG[]
						case 0x3D: // RTDG[]
						{
							gState.RoundMode = RoundingMode.Double_Grid;
							break;
						}
						#endregion

						#region RUTG[]
						case 0x7C: // RUTG[]
						{
							gState.RoundMode = RoundingMode.Up_To_Grid;
							break;
						}
						#endregion

						#region S45Round[]
						case 0x77: // S45Round[]
						{
							gState.RoundMode = RoundingMode.Super45;
							gState.SetSuperRoundMode(stack.Pop());
							Console.WriteLine("WARNING: S45Round isn't fully supported!");
							break;
						}
						#endregion

						#region SRound[]
						case 0x76: // SRound[]
						{
							gState.RoundMode = RoundingMode.Super;
							gState.SetSuperRoundMode(stack.Pop());
							break;
						}
						#endregion

						#region ROff[]
						case 0x7A: // ROff[]
						{
							gState.RoundMode = RoundingMode.Off;
							break;
						}
						#endregion

						#region Round[ab]
						case 0x68:
						case 0x69:
						case 0x6A:
						case 0x6B: // Round[ab]
						{
							DistanceType dTp = (DistanceType)(b & 0x03);
							F26Dot6 val = F26Dot6.FromLiteral(stack.Pop());
							stack.Push(F26Dot6.AsLiteral(gState.Round(val, dTp)));
							break;
						}
						#endregion

						#endregion


						#region Function Definitions

						#region EndF[]
						case 0x2D: // EndF[]
						{
							throw new Exception("EndF encountered in an interpreted method! This should never occur!");
						}
						#endregion

						#region FDef[]
						case 0x2C: // FDef[]
						{
							throw new Exception("FDef encountered in an interpreted method! This should never occur!");
						}
						#endregion

						#region Call[]
						case 0x2B: // Call[]
						{
							gState.CallFunction(stack.Pop());
							break;
						}
						#endregion

						#region LoopCall[]
						case 0x2A: // LoopCall[]
						{
							// For speed reasons, this doesn't use
							// GraphicsState.CallFunction
							int mNum = stack.Pop();
							if (!gState.Functions.ContainsKey(mNum))
							{
								throw new Exception("Tried to LoopCall a function that doesn't exist!");
							}
							HintingMethod hMth = gState.Functions[mNum];
							for (int i = 0; i < gState.Loop; i++)
							{
								hMth(gState);
							}
							gState.Loop = 1;
							break;
						}
						#endregion

						#endregion


						#region Math Operators

						#region Abs[]
						case 0x64: // Abs[]
						{
							int v = stack.Pop();
							if (v < 0)
								stack.Push(-v);
							else
								stack.Push(v);
							break;
						}
						#endregion

						#region Add[]
						case 0x60: // Add[]
						{
							stack.Push(stack.Pop() + stack.Pop());
							break;
						}
						#endregion

						#region Ceiling[]
						case 0x67: // Ceiling[]
						{
							stack.Push(F26Dot6.AsLiteral(F26Dot6.Ceiling(F26Dot6.FromLiteral(stack.Pop()))));
							break;
						}
						#endregion

						#region Div[]
						case 0x62: // Div[]
						{
							int n2 = stack.Pop();
							int n1 = stack.Pop();
							stack.Push((n1 << 6) / n2);
							break;
						}
						#endregion
						
						#region Even[]
						case 0x57: // Even[]
						{
							F26Dot6 val = F26Dot6.FromLiteral(stack.Pop());
							val = gState.Round(val, DistanceType.Grey);
							double d = F26Dot6.ToDouble(val);
							if ((d % 2) == 0)
								stack.Push(1);
							else
								stack.Push(0);
							break;
						}
						#endregion

						#region Floor[]
						case 0x66: // Floor[]
						{
							stack.Push(F26Dot6.AsLiteral(F26Dot6.Floor(F26Dot6.FromLiteral(stack.Pop()))));
							break;
						}
						#endregion
							
						#region Max[]
						case 0x8B: // Max[]
						{
							int vA = stack.Pop();
							int vB = stack.Pop();
							stack.Push((vA > vB) ? vA : vB);
							break;
						}
						#endregion
							
						#region Min[]
						case 0x8C: // Min[]
						{
							int vA = stack.Pop();
							int vB = stack.Pop();
							stack.Push((vA < vB) ? vA : vB);
							break;
						}
						#endregion
							
						#region Mul[]
						case 0x63: // Mul[]
						{
							stack.Push((stack.Pop() * stack.Pop()) >> 6);
							break;
						}
						#endregion

						#region Neg[]
						case 0x65: // Neg[]
						{
							stack.Push(-stack.Pop());
							break;
						}
						#endregion
							
						#region Odd[]
						case 0x56: // Odd[]
						{
							F26Dot6 val = F26Dot6.FromLiteral(stack.Pop());
							val = gState.Round(val, DistanceType.Grey);
							double d = F26Dot6.ToDouble(val);
							if ((d % 2) == 0)
								stack.Push(0);
							else
								stack.Push(1);
							break;
						}
						#endregion
							
						#region Sub[]
						case 0x61: // Sub[]
						{
							int vB = stack.Pop();
							int vA = stack.Pop();
							stack.Push(vA - vB);
							break;
						}
						#endregion
						
						#endregion


						#region Comparisons

						#region EQ[]
						case 0x54: // EQ[]
						{
							stack.Push((stack.Pop() == stack.Pop()) ? 1 : 0);
							break;
						}
						#endregion

						#region NEQ[]
						case 0x55: // NEQ[]
						{
							stack.Push((stack.Pop() != stack.Pop()) ? 1 : 0);
							break;
						}
						#endregion

						#region GT[]
						case 0x52: // GT[]
						{
							int vB = stack.Pop();
							int vA = stack.Pop();
							stack.Push((vA > vB) ? 1 : 0);
							break;
						}
						#endregion

						#region GTEQ[]
						case 0x53: // GTEQ[]
						{
							int vB = stack.Pop();
							int vA = stack.Pop();
							stack.Push((vA >= vB) ? 1 : 0);
							break;
						}
						#endregion

						#region LT[]
						case 0x50: // LT[]
						{
							int vB = stack.Pop();
							int vA = stack.Pop();
							stack.Push((vA < vB) ? 1 : 0);
							break;
						}
						#endregion
							
						#region LTEQ[]
						case 0x51: // LTEQ[]
						{
							int vB = stack.Pop();
							int vA = stack.Pop();
							stack.Push((vA <= vB) ? 1 : 0);
							break;
						}
						#endregion

						#endregion
							

						#region Bitwise Operations

						#region And[]
						case 0x5A: // And[]
						{
							int vB = stack.Pop();
							int vA = stack.Pop();
							if (vA != 0 && vB != 0)
								stack.Push(1);
							else
								stack.Push(0);
							break;
						}
						#endregion

						#region Or[]
						case 0x5B: // Or[]
						{
							int vB = stack.Pop();
							int vA = stack.Pop();
							if (vA != 0 || vB != 0)
								stack.Push(1);
							else
								stack.Push(0);
							break;
						}
						#endregion
							
						#region Not[]
						case 0x5C: // Not[]
						{
							int vA = stack.Pop();
							if (vA == 0)
								stack.Push(1);
							else
								stack.Push(0);
							break;
						}
						#endregion

						#endregion


						#region Branching Conditionals
							
						#region If[]
						case 0x58: // If[]
						{
							ifDepth = 1;
							int t = stack.Pop();
							if (t == 0)
							{
								Executing = false;
							}
							break;
						}
						#endregion

						#region Else[]
						case 0x1B: // Else[]
						{
							Executing = false;
							break;
						}
						#endregion

						#region EIf[]
						case 0x59: // EIf[]
						{
							// This doesn't do anything
							break;
						}
						#endregion
							
						#region JmpR[]
						case 0x1C: // JmpR[]
						{
							int dest = stack.Pop();
							rdr.BaseStream.Position += dest - 1;
							break;
						}
						#endregion

						#region JROT[]
						case 0x78: // JROT[]
						{
							int t = stack.Pop();
							int dest = stack.Pop();
							if (t != 0)
							{
								rdr.BaseStream.Position += dest - 1;
							}
							break;
						}
						#endregion

						#region JROF[]
						case 0x79: // JROF[]
						{
							int t = stack.Pop();
							int dest = stack.Pop();
							if (t == 0)
							{
								rdr.BaseStream.Position += dest - 1;
							}
							break;
						}
						#endregion

						#endregion


						#region Stack Management
							
						#region CIndex[]
						case 0x25: // CIndex[]
						{
							stack.CopyToTop(stack.Pop());
							break;
						}
						#endregion

						#region Clear[]
						case 0x22: // Clear[]
						{
							stack.Clear();
							break;
						}
						#endregion

						#region Depth[]
						case 0x24: // Depth[]
						{
							stack.Push(stack.Depth);
							break;
						}
						#endregion

						#region Dup[]
						case 0x20: // Dup[]
						{
							int val = stack.Pop();
							stack.Push(val);
							stack.Push(val);
							break;
						}
						#endregion

						#region MIndex[]
						case 0x26: // MIndex[]
						{
							gState.BringToTopOfStack(stack.Pop());
							break;
						}
						#endregion

						#region Pop[]
						case 0x21: // Pop[]
						{
							stack.Pop();
							break;
						}
						#endregion

						#region Roll[]
						case 0x8A: // Roll[]
						{
							int a = stack.Pop();
							int b2 = stack.Pop();
							int c = stack.Pop();
							stack.Push(b2);
							stack.Push(a);
							stack.Push(c);
							break;
						}
						#endregion

						#region Swap[]
						case 0x23: // Swap[]
						{
							int a = stack.Pop();
							int b2 = stack.Pop();
							stack.Push(a);
							stack.Push(b2);
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
				}
#if Trace
				tOut.Flush();
#endif
			}
#if Trace
			tOut.Flush();
			tOut.Close();
			tOut.Dispose();
#endif
		}

	}
}	
