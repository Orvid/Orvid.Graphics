//#define Profiling

using System;
using System.Text;
using Orvid.TrueType;
using System.Collections.Generic;
using Orvid.Compiler.TrueType.HintingVM.Instructions;
#if Profiling
using System.Diagnostics;
#endif

namespace Orvid.Compiler.TrueType.HintingVM
{
	public static class IROptimizer
	{
		public static void Run(IRMethodBuilder mBldr, bool doStackAnalysis)
		{
#if Profiling
			Console.WriteLine("Applying optimizations on \"" + mBldr.MethodName + "\"");
			Stopwatch st = new Stopwatch();
			st.Start();
#endif
			bool emittable = true;
			for (int i = 0; i < mBldr.Instructions.Count; i++)
			{
				IRInstruction ins = mBldr.Instructions[i];
				if (ins.OpCode == IROpCode.JROF || ins.OpCode == IROpCode.JmpR || ins.OpCode == IROpCode.JROT)
				{
					emittable = false;
					break;
				}
			}
#if Profiling
            st.Stop();
            Console.WriteLine("Ensuring method is emittable took " + st.ElapsedMilliseconds.ToString() + " Milliseconds.");
            st.Reset();
#endif
			if (!emittable)
			{
				return;
			}
			mBldr.Instructions.Add(new Return());


#if Profiling
			st.Start();
#endif

			UniformIntegerLoad(mBldr);

#if Profiling
			st.Stop();
			Console.WriteLine("Applying the UniformIntegerLoad transform took " + st.ElapsedMilliseconds.ToString() + " Milliseconds.");
			st.Reset();
			st.Start();
#endif

			IntegerLoadExpansion(mBldr);

#if Profiling
			st.Stop();
			Console.WriteLine("Applying the IntegerLoadExpansion transform took " + st.ElapsedMilliseconds.ToString() + " Milliseconds.");
			st.Reset();
			st.Start();
#endif

			ImmediateConstantFolding(mBldr);
			ImmediateConstantFolding(mBldr);
			ImmediateConstantFolding(mBldr); // Fold a maximum of 3 deep.

#if Profiling
			st.Stop();
			Console.WriteLine("Applying the ImmediateConstantFolding transform took " + st.ElapsedMilliseconds.ToString() + " Milliseconds.");
			st.Reset();
			st.Start();
#endif

			if (doStackAnalysis)
			{
				//DoStackAnalysis(mBldr);
#if Profiling
				st.Stop();
				Console.WriteLine("Performing stack analysis took " + st.ElapsedMilliseconds.ToString() + " Milliseconds.");
				st.Reset();
				st.Start();
#endif
			}

			RealizeStack(mBldr);

#if Profiling
			st.Stop();
			Console.WriteLine("Applying the StackRealization optimization took " + st.ElapsedMilliseconds.ToString() + " Milliseconds.");
			st.Reset();
			st.Start();
#endif

			SynthToILRemoval(mBldr);

#if Profiling
			st.Stop();
			Console.WriteLine("Applying the SynthToILRemoval optimization took " + st.ElapsedMilliseconds.ToString() + " Milliseconds.");
			st.Reset();
			st.Start();
#endif

			ComplexIfCreation(mBldr);

#if Profiling
			st.Stop();
			Console.WriteLine("Applying the ComplexIfCreation optimization took " + st.ElapsedMilliseconds.ToString() + " Milliseconds.");
			st.Reset();
			st.Start();
#endif

			CompactIntegerLoads(mBldr);

#if Profiling
			st.Stop();
			Console.WriteLine("Compacting the integer loads took " + st.ElapsedMilliseconds.ToString() + " Milliseconds.");
#endif

		}

		#region UniformIntegerLoad
		/// <summary>
		/// This tranforms all NPushB, NPushW, PushB,
		/// and PushW into a single (synthetic) op-code,
		/// PushInt.
		/// </summary>
		private static void UniformIntegerLoad(IRMethodBuilder mBldr)
		{
			for (int i = 0; i < mBldr.Instructions.Count; i++)
			{
				IRInstruction ins = mBldr.Instructions[i];
				switch (ins.OpCode)
				{
					case IROpCode.PushB:
					{
						PushB b = (PushB)ins;
						int[] convdArray = new int[b.ValuesToLoad.Length];
						for (int i2 = 0; i2 < b.ValuesToLoad.Length; i2++)
						{
							convdArray[i2] = (int)(byte)b.ValuesToLoad[i2];
						}
						mBldr.Instructions[i] = new LoadInt(convdArray);
						break;
					}
					case IROpCode.PushW:
					{
						PushW b = (PushW)ins;
						int[] convdArray = new int[b.ValuesToLoad.Length];
						for (int i2 = 0; i2 < b.ValuesToLoad.Length; i2++)
						{
							convdArray[i2] = (int)(short)(ushort)b.ValuesToLoad[i2];
						}
						mBldr.Instructions[i] = new LoadInt(convdArray);
						break;
					}
					case IROpCode.NPushB:
					{
						NPushB b = (NPushB)ins;
						int[] convdArray = new int[b.ValuesToLoad.Length];
						for (int i2 = 0; i2 < b.ValuesToLoad.Length; i2++)
						{
							convdArray[i2] = (int)(byte)b.ValuesToLoad[i2];
						}
						mBldr.Instructions[i] = new LoadInt(convdArray);
						break;
					}
					case IROpCode.NPushW:
					{
						NPushW b = (NPushW)ins;
						int[] convdArray = new int[b.ValuesToLoad.Length];
						for (int i2 = 0; i2 < b.ValuesToLoad.Length; i2++)
						{
							convdArray[i2] = (int)(short)(ushort)b.ValuesToLoad[i2];
						}
						mBldr.Instructions[i] = new LoadInt(convdArray);
						break;
					}
					default: break;
				}
			}
		}
		#endregion

		#region IntegerLoadExpansion
		/// <summary>
		/// This expands the integer
		/// load operations into an 
		/// indivual instruction for
		/// loading each integer. 
		/// This makes things such
		/// as constant folding much
		/// easier to do.
		/// </summary>
		private static void IntegerLoadExpansion(IRMethodBuilder mBldr)
		{
			// This comparison happens every loop,
			// meaning what it's comparing against
			// will change as we add more instructions.
			for (int i = 0; i < mBldr.Instructions.Count; i++)
			{
				IRInstruction ins = mBldr.Instructions[i];
				if (ins.OpCode == IROpCode.LoadInt)
				{
					LoadInt b = (LoadInt)ins;
					if (b.ValuesToLoad.Length > 1)
					{
						IRInstruction[] narr = new IRInstruction[b.ValuesToLoad.Length];
						for (int i2 = 0; i2 < narr.Length; i2++)
						{
							narr[i2] = new LoadInt(new int[] { b.ValuesToLoad[i2] });
						}
						mBldr.Instructions.RemoveAt(i);
						mBldr.Instructions.InsertRange(i, narr);
					}
				}
			}
		}
		#endregion

		#region CompactIntegerLoads
		/// <summary>
		/// This merges multiple sequential LoadInt
		/// instructions into single LoadInt 
		/// instructions.
		/// </summary>
		private static void CompactIntegerLoads(IRMethodBuilder mBldr)
		{
			int i = 0;
			int lIntSIndx;
			int lIntEIndx;
			bool ReadingLInt;
			bool ReadSecondLInt;
			bool destIsIL;
			bool destIsSet;
		Restart:
			lIntSIndx = 0;
			lIntEIndx = 0;
			ReadingLInt = false;
			ReadSecondLInt = false;
			destIsIL = false;
			destIsSet = false;
			for (; i < mBldr.Instructions.Count; i++)
			{
				IRInstruction ins = mBldr.Instructions[i];
				if (ins.OpCode == IROpCode.LoadInt && (!destIsSet || (ins.Destination1IsILStack == destIsIL)))
				{
					if (!ReadingLInt)
					{
						destIsIL = ins.Destination1IsILStack;
						destIsSet = true;
						ReadingLInt = true;
						lIntSIndx = i;
					}
					else if (!ReadSecondLInt)
					{
						ReadSecondLInt = true;
					}
				}
				else if (ReadingLInt && ReadSecondLInt)
				{
					lIntEIndx = i;
					LoadInt li;
					int[] its = new int[lIntEIndx - lIntSIndx];
					for (int i2 = lIntSIndx, i3 = 0; i2 < lIntEIndx; i2++, i3++)
					{
						li = (LoadInt)mBldr.Instructions[i2];
						its[i3] = li.ValuesToLoad[0];
					}
					mBldr.Instructions[lIntSIndx] = new LoadInt(its);
					mBldr.Instructions.RemoveRange(lIntSIndx + 1, lIntEIndx - lIntSIndx - 1);
					i = lIntSIndx + 2;
					goto Restart;
				}
				else if (ReadingLInt && !ReadSecondLInt)
				{
					ReadingLInt = false;
				}
			}
		}
		#endregion

		#region RealizeStack
		private struct ArgTarget
		{
		    public int ArgNum;
		    public IRInstruction Instr;

			/// <summary>
			/// Sets the source type of the instruction
			/// this represents.
			/// </summary>
			/// <param name="tp">The type of source.</param>
			/// <param name="constVal">The constant value. Only valid if tp is <see cref="SourceType.Constant"/>.</param>
			public void SetSourceType(SourceType tp, int constVal)
			{
				bool t = false;
				SetSourceType(tp, constVal, false, ref t);
			}

			/// <summary>
			/// Sets the source type of the instruction
			/// this represents.
			/// </summary>
			/// <param name="tp">The type of source.</param>
			/// <param name="constVal">The constant value. Only valid if tp is <see cref="SourceType.Constant"/>.</param>
			public void SetSourceType(SourceType tp, int constVal, bool SourceIsF26Dot6, ref bool destIsF26Dot6)
		    {
		        switch (ArgNum)
		        {
		            case 1:
		                if (tp == SourceType.Constant)
		                {
		                    Instr.Args[0].Source = SourceType.Constant;
		                    Instr.Args[0].Constant = constVal;
		                }
						else
						{
							Instr.Args[0].Source = tp;
						}
						if (SourceIsF26Dot6 && Instr.ExpectsArg1F26Dot6)
						{
							destIsF26Dot6 = true;
							Instr.Args[0].IsF26Dot6 = true;
						}
		                break;
		            case 2:
		                if (tp == SourceType.Constant)
		                {
		                    Instr.Args[1].Source = SourceType.Constant;
		                    Instr.Args[1].Constant = constVal;
		                }
		                else
		                {
		                    Instr.Args[1].Source = tp;
						}
						if (SourceIsF26Dot6 && Instr.ExpectsArg2F26Dot6)
						{
							destIsF26Dot6 = true;
							Instr.Args[1].IsF26Dot6 = true;
						}
		                break;
		            case 3:
		                if (tp == SourceType.Constant)
		                {
		                    Instr.Args[2].Source = SourceType.Constant;
		                    Instr.Args[2].Constant = constVal;
		                }
		                else
		                {
		                    Instr.Args[2].Source = tp;
						}
						if (SourceIsF26Dot6 && Instr.ExpectsArg3F26Dot6)
						{
							destIsF26Dot6 = true;
							Instr.Args[2].IsF26Dot6 = true;
						}
		                break;
		            default: throw new Exception("Unkown arg number!");
		        }
		    }

		    public ArgTarget(IRInstruction ins, int argNum)
		    {
		        this.Instr = ins;
		        this.ArgNum = argNum;
		    }
		}
		/// <summary>
		/// This does what it can to make operations
		/// occur via the IL stack rather than via
		/// the synthetic stack.
		/// 
		/// This is one of the few optimizations in
		/// existance that are actually run from the
		/// end of the method to the start, and it has
		/// to run like this because we don't know
		/// how many parameters a call (and even some
		/// instructions) might use.
		/// </summary>
		private static void RealizeStack(IRMethodBuilder mBldr)
		{
		    LinkedStack<ArgTarget> stack;
		    IRInstruction ins;
		    int i2 = 0;
		    int i = mBldr.Instructions.Count - 1;
		    bool inIf;
		Resume:
		    inIf = false;
		    for (; i >= 0; i--)
		    {
		        ins = mBldr.Instructions[i];
		        switch (ins.OpCode)
		        {
		            // 1 arg
		            case IROpCode.Abs:
		            case IROpCode.Ceiling:
		            case IROpCode.Dup:
		            case IROpCode.Even:
		            case IROpCode.Floor:
		            case IROpCode.GetInfo:
		            case IROpCode.If:
					case IROpCode.MDRP:
		            case IROpCode.Neg:
		            case IROpCode.Not:
		            case IROpCode.Odd:
		            case IROpCode.RCvt:
		            case IROpCode.Round:
		            case IROpCode.RS:
		            case IROpCode.S45Round:
		            case IROpCode.ScanCtrl:
		            case IROpCode.SCVTCI:
		            case IROpCode.SDB:
		            case IROpCode.SDS:
					case IROpCode.ShC:
		            case IROpCode.SLoop:
		            case IROpCode.SMD:
		            case IROpCode.SRound:
		            case IROpCode.SRP0:
		            case IROpCode.SRP1:
		            case IROpCode.SRP2:
		            case IROpCode.SZP0:
		            case IROpCode.SZP1:
		            case IROpCode.SZP2:
		            case IROpCode.SZPS:
		            {
		                stack = new LinkedStack<ArgTarget>();
						if (ins.Args[0].Source == SourceType.SyntheticStack)
							stack.Push(new ArgTarget(ins, 1));
						else
							break;
		                goto EvalLoop;
		            }


		            // These op-codes can only
		            // be the start of the loop.
		            // Encountering them during
		            // the loop stops the loop.
		            case IROpCode.Call:
		            case IROpCode.CIndex:
					case IROpCode.JmpR:
		            case IROpCode.MIndex:
					case IROpCode.ShPix:
		            {
		                stack = new LinkedStack<ArgTarget>();
						if (ins.Args[0].Source == SourceType.SyntheticStack)
							stack.Push(new ArgTarget(ins, 1));
						else
							break;
		                goto EvalLoop;
		            }

		            // 2 arg
		            case IROpCode.Add:
		            case IROpCode.And:
		            case IROpCode.Div:
		            case IROpCode.EQ:
		            case IROpCode.GT:
		            case IROpCode.GTEQ:
		            case IROpCode.InstCtrl:
		            case IROpCode.LT:
					case IROpCode.LTEQ:
					case IROpCode.Max:
					case IROpCode.MD:
					case IROpCode.MIAP:
					case IROpCode.Min:
					case IROpCode.MIRP:
					case IROpCode.MSIRP:
		            case IROpCode.Mul:
		            case IROpCode.NEQ:
					case IROpCode.Or:
					case IROpCode.SDPVTL:
					case IROpCode.SFVTL:
					case IROpCode.SPVTL:
		            case IROpCode.Sub:
		            case IROpCode.Swap:
		            case IROpCode.WCvtF:
		            case IROpCode.WCvtP:
		            case IROpCode.WS:
		            {
		                stack = new LinkedStack<ArgTarget>();
						if (ins.Args[1].Source == SourceType.SyntheticStack)
							stack.Push(new ArgTarget(ins, 2));
						if (ins.Args[0].Source == SourceType.SyntheticStack)
							stack.Push(new ArgTarget(ins, 1));
		                goto EvalLoop;
		            }

		            // 3 arg
		            case IROpCode.Roll:
		            {
		                stack = new LinkedStack<ArgTarget>();
						if (ins.Args[2].Source == SourceType.SyntheticStack)
							stack.Push(new ArgTarget(ins, 3));
						if (ins.Args[1].Source == SourceType.SyntheticStack)
							stack.Push(new ArgTarget(ins, 2));
						if (ins.Args[0].Source == SourceType.SyntheticStack)
							stack.Push(new ArgTarget(ins, 1));
		                goto EvalLoop;
		            }
		            default: break;
		        }
		    }
		    // We'll only get here if we've gotten to 0 for i.
		    return;

		EvalLoop:
		    for (i2 = i - 1; i2 >= 0 && stack.Depth > 0; i2--)
		    {
		        ins = mBldr.Instructions[i2];
		        switch (ins.OpCode)
		        {
		            // 2 arg, 1 return
		            case IROpCode.And:
		            case IROpCode.EQ:
		            case IROpCode.GT:
		            case IROpCode.GTEQ:
		            case IROpCode.LT:
		            case IROpCode.LTEQ:
		            case IROpCode.Max:
		            case IROpCode.Min:
		            case IROpCode.NEQ:
		            case IROpCode.Or:
						stack.Pop().SetSourceType(SourceType.ILStack, 0);
						if (ins.Args[1].Source == SourceType.SyntheticStack)
							stack.Push(new ArgTarget(ins, 2));
						if (ins.Args[0].Source == SourceType.SyntheticStack)
							stack.Push(new ArgTarget(ins, 1));
						ins.Destination1IsILStack = true;
						break;

					// 2 arg, 1 return, possible F26Dot6
					case IROpCode.MD:
					case IROpCode.Add:
					case IROpCode.Div:
					case IROpCode.Sub:
						stack.Pop().SetSourceType(SourceType.ILStack, 0, true, ref ins.Destination1IsF26Dot6);
						if (ins.Args[1].Source == SourceType.SyntheticStack)
							stack.Push(new ArgTarget(ins, 2));
						if (ins.Args[0].Source == SourceType.SyntheticStack)
							stack.Push(new ArgTarget(ins, 1));
						ins.Destination1IsILStack = true;
						break;

		            // 0 args, 1 return
		            case IROpCode.MPPEM:
		            case IROpCode.MPS:
		                stack.Pop().SetSourceType(SourceType.ILStack, 0);
		                ins.Destination1IsILStack = true;
						break;

					// 1 arg, 1 return, possible F26Dot6
					case IROpCode.Abs:
					case IROpCode.Ceiling:
					case IROpCode.Floor:
					case IROpCode.GC:
					case IROpCode.Neg:
					case IROpCode.RCvt:
					case IROpCode.Round:
						stack.Pop().SetSourceType(SourceType.ILStack, 0, true, ref ins.Destination1IsF26Dot6);
						if (ins.Args[0].Source == SourceType.SyntheticStack)
							stack.Push(new ArgTarget(ins, 1));
						ins.Destination1IsILStack = true;
						break;

					// 1 arg, 1 return
					case IROpCode.SynthToIL:
					case IROpCode.Even:
		            case IROpCode.GetInfo:
		            case IROpCode.Not:
		            case IROpCode.Odd:
		            case IROpCode.RS:
		                stack.Pop().SetSourceType(SourceType.ILStack, 0);
						if (ins.Args[0].Source == SourceType.SyntheticStack)
							stack.Push(new ArgTarget(ins, 1));
		                ins.Destination1IsILStack = true;
		                break;

		            #region Special
					// Mul is really really special.
					// Not only is it possible for it
					// to return an F26Dot6, it is also
					// possible for it to be expanded to
					// a LoadInt. (Used to load large
					// numbers to the stack)
					case IROpCode.Mul:
						if (ins.Args[0].Source == SourceType.Constant && ins.Args[1].Source == SourceType.Constant)
						{
							stack.Pop().SetSourceType(SourceType.Constant, F26Dot6.AsLiteral(F26Dot6.FromLiteral(ins.Args[0].Constant) * F26Dot6.FromLiteral(ins.Args[1].Constant)));
							mBldr.Instructions.RemoveAt(i2);
							ins.Destination1IsILStack = true;
						}
						else
						{
							stack.Pop().SetSourceType(SourceType.ILStack, 0, true, ref ins.Destination1IsF26Dot6);
							if (ins.Args[1].Source == SourceType.SyntheticStack)
								stack.Push(new ArgTarget(ins, 2));
							if (ins.Args[0].Source == SourceType.SyntheticStack)
								stack.Push(new ArgTarget(ins, 1));
							ins.Destination1IsILStack = true;
						}
						break;
					
		            // LoadInt is special.
		            case IROpCode.LoadInt:
		                stack.Pop().SetSourceType(SourceType.Constant, ((LoadInt)ins).ValuesToLoad[0]);
		                mBldr.Instructions.RemoveAt(i2);
		                ins.Destination1IsILStack = true;
		                break;

		            // Dup is special.
		            case IROpCode.Dup:
		            {
		                if (stack.Depth > 1 && !inIf)
		                {
		                    // We can only do this if both
		                    // destinations are in this context.
		                    stack.Pop().SetSourceType(SourceType.ILStack, 0);
		                    stack.Pop().SetSourceType(SourceType.ILStack, 0);
							if (ins.Args[0].Source == SourceType.SyntheticStack)
								stack.Push(new ArgTarget(ins, 1));
							ins.Destination1IsILStack = true;
							ins.Destination2IsILStack = true;
		                }
		                else
		                {
		                    goto ExitForLoop;
		                }
		                break;
		            }

		            // If is special.
		            case IROpCode.If:
		            {
						goto ExitForLoop;
		            }

		            // Swap is special.
		            case IROpCode.Swap:
		            {
		                if (stack.Depth > 1 && !inIf)
						{
							if (ins.Args[0].Source == SourceType.Constant)
							{
								stack.Pop().SetSourceType(SourceType.ILStack, 0);
								stack.Pop().SetSourceType(SourceType.Constant, ins.Args[0].Constant);
								mBldr.Instructions[i2] = new SynthToIL();
								ins = mBldr.Instructions[i2];
								stack.Push(new ArgTarget(ins, 1));
							}
							else if (ins.Args[1].Source == SourceType.Constant)
							{
								stack.Pop().SetSourceType(SourceType.Constant, ins.Args[1].Constant);
								stack.Pop().SetSourceType(SourceType.ILStack, 0);
								mBldr.Instructions[i2] = new SynthToIL();
								ins = mBldr.Instructions[i2];
								stack.Push(new ArgTarget(ins, 1));
							}
							else
							{
								stack.Pop().SetSourceType(SourceType.ILStack, 0);
								stack.Pop().SetSourceType(SourceType.ILStack, 0);
								if (ins.Args[1].Source == SourceType.SyntheticStack)
									stack.Push(new ArgTarget(ins, 2));
								if (ins.Args[0].Source == SourceType.SyntheticStack)
									stack.Push(new ArgTarget(ins, 1));
								ins.Destination1IsILStack = true;
								ins.Destination2IsILStack = true;
							}
		                }
						else if (stack.Depth == 1 && ins.Args[1].Source == SourceType.Constant)
						{
							stack.Pop().SetSourceType(SourceType.Constant, ins.Args[1].Constant);
							mBldr.Instructions.RemoveAt(i2);
							goto ExitForLoop;
						}
		                else
		                {
		                    goto ExitForLoop;
		                }
		                break;
		            }

		            // Roll is special
		            case IROpCode.Roll:
		            {
		                if (stack.Depth > 2 && !inIf)
		                {
							if (ins.Args[0].Source == SourceType.Constant)
							{
								if (ins.Args[1].Source == SourceType.Constant)
								{
									stack.Pop().SetSourceType(SourceType.ILStack, 0);
									stack.Pop().SetSourceType(SourceType.Constant, ins.Args[1].Constant);
									stack.Pop().SetSourceType(SourceType.Constant, ins.Args[0].Constant);
									mBldr.Instructions[i2] = new SynthToIL();
									ins = mBldr.Instructions[i2];
									stack.Push(new ArgTarget(ins, 1));
									ins.Destination1IsILStack = true;
								}
								else
								{
									stack.Pop().SetSourceType(SourceType.ILStack, 0);
									stack.Pop().SetSourceType(SourceType.ILStack, 0);
									stack.Pop().SetSourceType(SourceType.Constant, ins.Args[0].Constant);
									mBldr.Instructions[i2] = new Swap();
									ins = mBldr.Instructions[i2];
									stack.Push(new ArgTarget(ins, 2));
									stack.Push(new ArgTarget(ins, 1));
									ins.Destination1IsILStack = true;
								}
							}
							else if (ins.Args[1].Source == SourceType.Constant)
							{
								stack.Pop().SetSourceType(SourceType.ILStack, 0);
								stack.Pop().SetSourceType(SourceType.Constant, ins.Args[1].Constant);
								stack.Pop().SetSourceType(SourceType.ILStack, 0);
								mBldr.Instructions[i2] = new Swap();
								ins = mBldr.Instructions[i2];
								stack.Push(new ArgTarget(ins, 2));
								stack.Push(new ArgTarget(ins, 1));
								ins.Destination1IsILStack = true;
							}
							else
							{
								stack.Pop().SetSourceType(SourceType.ILStack, 0);
								stack.Pop().SetSourceType(SourceType.ILStack, 0);
								stack.Pop().SetSourceType(SourceType.ILStack, 0);
								if (ins.Args[2].Source == SourceType.SyntheticStack)
									stack.Push(new ArgTarget(ins, 3));
								if (ins.Args[1].Source == SourceType.SyntheticStack)
									stack.Push(new ArgTarget(ins, 2));
								if (ins.Args[0].Source == SourceType.SyntheticStack)
									stack.Push(new ArgTarget(ins, 1));
								ins.Destination1IsILStack = true;
							}
		                }
		                else
		                {
		                    goto ExitForLoop;
		                }
		                break;
		            }

		            #endregion

		            // 1 arg, no return
					case IROpCode.MDAP:
					case IROpCode.MDRP:
		            case IROpCode.S45Round:
		            case IROpCode.ScanCtrl:
		            case IROpCode.SCVTCI:
		            case IROpCode.SDB:
					case IROpCode.SDS:
					case IROpCode.ShC:
		            case IROpCode.SLoop:
		            case IROpCode.SMD:
		            case IROpCode.SRound:
		            case IROpCode.SRP0:
		            case IROpCode.SRP1:
		            case IROpCode.SRP2:
		            case IROpCode.SZP0:
		            case IROpCode.SZP1:
		            case IROpCode.SZP2:
		            case IROpCode.SZPS:
						if (ins.Args[0].Source == SourceType.SyntheticStack)
							stack.Push(new ArgTarget(ins, 1));
		                break;

					// 2 arg, no return
					case IROpCode.InstCtrl:
					case IROpCode.MIAP:
					case IROpCode.MIRP:
					case IROpCode.MSIRP:
					case IROpCode.SDPVTL:
					case IROpCode.SFVTL:
					case IROpCode.SPVTL:
		            case IROpCode.WCvtF:
		            case IROpCode.WCvtP:
		            case IROpCode.WS:
						if (ins.Args[1].Source == SourceType.SyntheticStack)
							stack.Push(new ArgTarget(ins, 2));
						if (ins.Args[0].Source == SourceType.SyntheticStack)
							stack.Push(new ArgTarget(ins, 1));
		                break;

		            // 0 arg, 2 return
		            case IROpCode.GFV:
		            case IROpCode.GPV:
		                if (stack.Depth > 1 && !inIf)
		                {
		                    stack.Pop().SetSourceType(SourceType.ILStack, 0);
		                    stack.Pop().SetSourceType(SourceType.ILStack, 0);
							ins.Destination1IsILStack = true;
							ins.Destination2IsILStack = true;
		                }
		                else
		                {
		                    goto ExitForLoop;
		                }
		                break;

		            // 0 arg, no return (no effect on the stack)
		            case IROpCode.SFVTCA:
		            case IROpCode.SFVTPV:
		            case IROpCode.SPVTCA:
		            case IROpCode.SVTCA:
		            case IROpCode.RTDG:
		            case IROpCode.RTG:
		            case IROpCode.RTHG:
		            case IROpCode.RUTG:
		            case IROpCode.ROff:
		            case IROpCode.RDTG:
					case IROpCode.IUP:
		                break;

		            default:
		                goto ExitForLoop;
		        }
		    }
		ExitForLoop:
		    i = i2;
		    goto Resume;

		}
		#endregion

		#region Immediate Constant Folding
		private struct CF_SO
		{
			public bool IsConstant;
			public int ConstantValue;
			public int InstructionIndex;
			public CF_SO(int constantValue, int instrIdx)
			{
				this.IsConstant = true;
				this.ConstantValue = constantValue;
				this.InstructionIndex = instrIdx;
			}
		}
		/// <summary>
		/// Folds a select few immediate constants into
		/// their destinations. This is needed in order
		/// to linearize the stack properly.
		/// </summary>
		private static void ImmediateConstantFolding(IRMethodBuilder mBldr)
		{
			//LinkedStack<CF_SO> stack = new LinkedStack<CF_SO>();
			bool hasConstant = false;
			int constant = 0;
			int constantIndex = 0;
			for (int i = 0; i < mBldr.Instructions.Count; i++)
			{
				IRInstruction ins = mBldr.Instructions[i];
				switch (ins.OpCode)
				{
					case IROpCode.LoadInt:
						hasConstant = true;
						constant = ((LoadInt)ins).ValuesToLoad[0];
						constantIndex = i;
						//stack.Push(new CF_SO(((LoadInt)ins).ValuesToLoad[0], i));
						break;

					case IROpCode.Mul:
					case IROpCode.Swap:
						//if (stack.Depth > 0)
						//{
						//    if (stack.Depth > 1)
						//    {

						//    }
						//}
						if (hasConstant)
						{
							if (ins.Args[0].Source != SourceType.Constant)
							{
								ins.Args[0].Constant = constant;
								ins.Args[0].Source = SourceType.Constant;
							}
							else if (ins.Args[1].Source != SourceType.Constant)
							{
								ins.Args[1].Constant = constant;
								ins.Args[1].Source = SourceType.Constant;
							}
							else
							{
								continue;
							}
							mBldr.Instructions.RemoveAt(constantIndex);
							hasConstant = false;
							i--;
						}
						break;
					case IROpCode.Roll:
						if (hasConstant)
						{
							if (ins.Args[0].Source != SourceType.Constant)
							{
								ins.Args[0].Constant = constant;
								ins.Args[0].Source = SourceType.Constant;
							}
							else if (ins.Args[1].Source != SourceType.Constant)
							{
								ins.Args[1].Constant = constant;
								ins.Args[1].Source = SourceType.Constant;
							}
							else if (ins.Args[2].Source != SourceType.Constant)
							{
								ins.Args[2].Constant = constant;
								ins.Args[2].Source = SourceType.Constant;
							}
							else
							{
								continue;
							}
							mBldr.Instructions.RemoveAt(constantIndex);
							i--;
							hasConstant = false;
						}
						break;

					case IROpCode.CIndex:
					case IROpCode.MIndex:
						if (hasConstant)
						{
							if (ins.Args[0].Source != SourceType.Constant)
							{
								ins.Args[0].Constant = constant;
								ins.Args[0].Source = SourceType.Constant;
								mBldr.Instructions.RemoveAt(constantIndex);
								i--;
								hasConstant = false;
							}
						}
						else if (ins.Args[0].Source != SourceType.Constant)
						{
							throw new Exception("Hit an MIndex (or a CIndex) without a constant to work from!");
						}
						break;

					default:
						hasConstant = false;
						//stack.Clear();
						break;
				}
			}
		}
		#endregion

		#region DoStackAnalysis
		private struct ParamLocationArgTriplet
		{
			public ParameterType ParamType;
			public int InstructionIndex;
			public int InstructionArgNumber;
			public ParamLocationArgTriplet(ParameterType paramType, int instrIdx, int instrArgNum)
			{
				this.ParamType = paramType;
				this.InstructionIndex = instrIdx;
				this.InstructionArgNumber = instrArgNum;
			}
		}

		private static void DoStackAnalysis(IRMethodBuilder mBldr)
		{
			const int HitElseStackDepth = unchecked((int)0x80000000);
			int extraStackRequired = 0;
			LinkedStack<ParamLocationArgTriplet> ExpectedTypes = new LinkedStack<ParamLocationArgTriplet>();
			int curStackDepth = 0;
			LinkedStack<int> StackDepths = new LinkedStack<int>();
			for (int i = 0; i < mBldr.Instructions.Count; i++)
			{
				IRInstruction ins = mBldr.Instructions[i];
				switch (ins.OpCode)
				{
					// 2 arg, 1 return
					case IROpCode.And:
					case IROpCode.EQ:
					case IROpCode.GT:
					case IROpCode.GTEQ:
					case IROpCode.LT:
					case IROpCode.LTEQ:
					case IROpCode.Max:
					case IROpCode.Min:
					case IROpCode.NEQ:
					case IROpCode.Or:
					case IROpCode.Add:
					case IROpCode.Div:
					case IROpCode.Mul:
					case IROpCode.Sub:
						if (curStackDepth < 2)
						{
							if (curStackDepth < 1)
							{
								if (ins.ExpectsArg1F26Dot6)
									ExpectedTypes.Push(new ParamLocationArgTriplet(ParameterType.F26Dot6, i, 1));
								else
									ExpectedTypes.Push(new ParamLocationArgTriplet(ParameterType.Integer, i, 1));
								extraStackRequired++;
							}
							else
							{
								curStackDepth--;
							}
							if (ins.ExpectsArg2F26Dot6)
								ExpectedTypes.Push(new ParamLocationArgTriplet(ParameterType.F26Dot6, i, 2));
							else
								ExpectedTypes.Push(new ParamLocationArgTriplet(ParameterType.Integer, i, 2));
							extraStackRequired++;
						}
						else
						{
							curStackDepth -= 2;
						}
						curStackDepth++;
						break;

					// 1 arg, 2 returns
					case IROpCode.Dup:
						if (curStackDepth < 1)
						{
							if (ins.ExpectsArg1F26Dot6)
								ExpectedTypes.Push(new ParamLocationArgTriplet(ParameterType.F26Dot6, i, 1));
							else
								ExpectedTypes.Push(new ParamLocationArgTriplet(ParameterType.Integer, i, 1));
							extraStackRequired++;
						}
						else
						{
							curStackDepth--;
						}
						curStackDepth += 2;
						break;

					#region Special
					// If is special
					case IROpCode.If:
					{
						if (curStackDepth < 1)
						{
							if (ins.ExpectsArg1F26Dot6)
								ExpectedTypes.Push(new ParamLocationArgTriplet(ParameterType.F26Dot6, i, 1));
							else
								ExpectedTypes.Push(new ParamLocationArgTriplet(ParameterType.Integer, i, 1));
							extraStackRequired++;
						}
						else
						{
							curStackDepth--;
						}
						StackDepths.Push(curStackDepth);
						break;
					}

					// Else is special
					case IROpCode.Else:
					{
						int oSDepth = curStackDepth;
						curStackDepth = StackDepths.Pop();
						StackDepths.Push(HitElseStackDepth | oSDepth);
						break;
					}

					// EIf is special.
					case IROpCode.EIf:
					{
						int sDepth = StackDepths.Pop();
						if ((sDepth & HitElseStackDepth) != 0)
						{
							sDepth &= ~HitElseStackDepth;
						}
						if (curStackDepth != sDepth)
						{
							// This means that the if and
							// else (if it exists) sides of the
							// statement produce a different 
							// stack depth.
							return;
						}
						// Otherwise stack depths are the same.
						break;
					}

					//// MIndex is special
					//case IROpCode.MIndex:
					//case IROpCode.CIndex:
					//{
					//    if (ins.Args[0].Source == SourceType.Constant)
					//    {
					//        curStackDepth -= ins.Args[0].Constant;
					//        if (curStackDepth < 0)
					//        {
					//            for (int i2 = 0; i2 < -curStackDepth; i2++)
					//            {
					//                ExpectedTypes.Push(ParameterType.Integer);
					//            }
					//            extraStackRequired += -curStackDepth;
					//            curStackDepth = 0;
					//        }
					//        curStackDepth += ins.Args[0].Constant;
					//        if (ins.OpCode == IROpCode.CIndex)
					//            curStackDepth++;
					//    }
					//    else
					//    {
					//        // Otherwise we don't know how deep
					//        // of a stack it needs.
					//        throw new Exception("This should never occur!");
					//    }
					//    break;
					//}
					#endregion

					// 1 arg, 0 returns
					case IROpCode.MDAP:
					case IROpCode.MDRP:
					case IROpCode.S45Round:
					case IROpCode.ScanCtrl:
					case IROpCode.SCVTCI:
					case IROpCode.SDB:
					case IROpCode.SDS:
					case IROpCode.ShC:
					case IROpCode.SLoop:
					case IROpCode.SMD:
					case IROpCode.SRound:
					case IROpCode.SRP0:
					case IROpCode.SRP1:
					case IROpCode.SRP2:
					case IROpCode.SZP0:
					case IROpCode.SZP1:
					case IROpCode.SZP2:
					case IROpCode.SZPS:
						if (curStackDepth < 1)
						{
							if (ins.ExpectsArg1F26Dot6)
								ExpectedTypes.Push(new ParamLocationArgTriplet(ParameterType.F26Dot6, i, 1));
							else
								ExpectedTypes.Push(new ParamLocationArgTriplet(ParameterType.Integer, i, 1));
							extraStackRequired++;
						}
						else
						{
							curStackDepth--;
						}
						break;

					// 3 args, 3 returns
					case IROpCode.Roll:
						if (curStackDepth < 3)
						{
							if (curStackDepth < 2)
							{
								if (curStackDepth < 1)
								{
									if (ins.ExpectsArg1F26Dot6)
										ExpectedTypes.Push(new ParamLocationArgTriplet(ParameterType.F26Dot6, i, 1));
									else
										ExpectedTypes.Push(new ParamLocationArgTriplet(ParameterType.Integer, i, 1));
									extraStackRequired++;
								}
								else
								{
									curStackDepth--;
								}
								if (ins.ExpectsArg2F26Dot6)
									ExpectedTypes.Push(new ParamLocationArgTriplet(ParameterType.F26Dot6, i, 2));
								else
									ExpectedTypes.Push(new ParamLocationArgTriplet(ParameterType.Integer, i, 2));
								extraStackRequired++;
							}
							else
							{
								curStackDepth -= 2;
							}
							if (ins.ExpectsArg3F26Dot6)
								ExpectedTypes.Push(new ParamLocationArgTriplet(ParameterType.F26Dot6, i, 3));
							else
								ExpectedTypes.Push(new ParamLocationArgTriplet(ParameterType.Integer, i, 3));
							extraStackRequired++;
						}
						else
						{
							curStackDepth -= 3;
						}
						curStackDepth += 3;
						break;

					// 2 args, 2 returns
					case IROpCode.Swap:
						if (curStackDepth < 2)
						{
							if (curStackDepth < 1)
							{
								if (ins.ExpectsArg1F26Dot6)
									ExpectedTypes.Push(new ParamLocationArgTriplet(ParameterType.F26Dot6, i, 1));
								else
									ExpectedTypes.Push(new ParamLocationArgTriplet(ParameterType.Integer, i, 1));
								extraStackRequired++;
							}
							else
							{
								curStackDepth--;
							}
							if (ins.ExpectsArg2F26Dot6)
								ExpectedTypes.Push(new ParamLocationArgTriplet(ParameterType.F26Dot6, i, 2));
							else
								ExpectedTypes.Push(new ParamLocationArgTriplet(ParameterType.Integer, i, 2));
							extraStackRequired++;
						}
						else
						{
							curStackDepth -= 2;
						}
						curStackDepth += 2;
						break;


					// 0 args, 1 return
					case IROpCode.LoadInt:
					case IROpCode.MPPEM:
					case IROpCode.MPS:
						curStackDepth++;
						break;

					// 1 arg, 1 return
					case IROpCode.Abs:
					case IROpCode.Ceiling:
					case IROpCode.Floor:
					case IROpCode.GC:
					case IROpCode.Neg:
					case IROpCode.RCvt:
					case IROpCode.Round:
					case IROpCode.Even:
					case IROpCode.GetInfo:
					case IROpCode.Not:
					case IROpCode.Odd:
					case IROpCode.RS:
						if (curStackDepth < 1)
						{
							if (ins.ExpectsArg1F26Dot6)
								ExpectedTypes.Push(new ParamLocationArgTriplet(ParameterType.F26Dot6, i, 1));
							else
								ExpectedTypes.Push(new ParamLocationArgTriplet(ParameterType.Integer, i, 1));
							extraStackRequired++;
						}
						else
						{
							curStackDepth--;
						}
						curStackDepth++;
						break;

					// 2 arg, no return
					case IROpCode.InstCtrl:
					case IROpCode.MIAP:
					case IROpCode.MIRP:
					case IROpCode.SCFS:
					case IROpCode.SDPVTL:
					case IROpCode.SFVTL:
					case IROpCode.SPVTL:
					case IROpCode.WCvtF:
					case IROpCode.WCvtP:
					case IROpCode.WS:
						if (curStackDepth < 2)
						{
							if (curStackDepth < 1)
							{
								if (ins.ExpectsArg1F26Dot6)
									ExpectedTypes.Push(new ParamLocationArgTriplet(ParameterType.F26Dot6, i, 1));
								else
									ExpectedTypes.Push(new ParamLocationArgTriplet(ParameterType.Integer, i, 1));
								extraStackRequired++;
							}
							else
							{
								curStackDepth--;
							}
							if (ins.ExpectsArg2F26Dot6)
								ExpectedTypes.Push(new ParamLocationArgTriplet(ParameterType.F26Dot6, i, 2));
							else
								ExpectedTypes.Push(new ParamLocationArgTriplet(ParameterType.Integer, i, 2));
							extraStackRequired++;
						}
						else
						{
							curStackDepth -= 2;
						}
						break;

					// 0 arg, 2 return
					case IROpCode.GFV:
					case IROpCode.GPV:
						curStackDepth += 2;
						break;

					// 0 arg, no return (no effect on the stack)
					case IROpCode.FDef:
					case IROpCode.EndF:
					case IROpCode.SFVTCA:
					case IROpCode.SFVTPV:
					case IROpCode.SPVTCA:
					case IROpCode.SVTCA:
					case IROpCode.RTDG:
					case IROpCode.RTG:
					case IROpCode.RTHG:
					case IROpCode.RUTG:
					case IROpCode.ROff:
					case IROpCode.RDTG:
					case IROpCode.IUP:
						break;

					// These mean we can't do stack analysis on this.
					case IROpCode.JmpR:
					case IROpCode.JROT:
					case IROpCode.JROF:
						return;
						

					// Return is special
					case IROpCode.Return:
					{
						if (curStackDepth > 0)
						{
							throw new Exception("Need to handle this!");
						}
						break;
					}

					default:
						return;// throw new Exception("Unknown stack analysis!");
				}


			}
			while (ExpectedTypes.Depth > 0)
			{
				ParamLocationArgTriplet val = ExpectedTypes.Pop();
				mBldr.Parameters.Add(new IRParameter(val.ParamType));
				mBldr.Instructions[val.InstructionIndex].Args[val.InstructionArgNumber - 1].Source = SourceType.Parameter;
				mBldr.Instructions[val.InstructionIndex].Args[val.InstructionArgNumber - 1].ParameterIndex = mBldr.Parameters.Count - 1;
			}
			//throw new Exception("Well lookie there, it finished!");
		}
		#endregion

		#region SynthToILRemoval
		/// <summary>
		/// Removes a large number of SynthToIL instructions
		/// that are left over from other optimizations.
		/// </summary>
		private static void SynthToILRemoval(IRMethodBuilder mBldr)
		{
			bool prevWasSynth = false;
			for (int i = 0; i < mBldr.Instructions.Count; i++)
			{
				IRInstruction ins = mBldr.Instructions[i];
				switch (ins.OpCode)
				{
					case IROpCode.SynthToIL:
						if (ins.Args[0].Source == SourceType.SyntheticStack)
						{
							prevWasSynth = true;
						}
						else
						{
							mBldr.Instructions.RemoveAt(i);
							i--;
						}
						break;
					default:
						if (prevWasSynth)
						{
							if (ins.Args[0].Source == SourceType.Constant)
							{
								if (ins.Args[1].Source == SourceType.Constant)
								{
									if (ins.Args[2].Source == SourceType.ILStack)
									{
										ins.Args[2].Source = SourceType.SyntheticStack;
										i--;
										mBldr.Instructions.RemoveAt(i);
									}
								}
								else if (ins.Args[1].Source == SourceType.ILStack)
								{
									ins.Args[1].Source = SourceType.SyntheticStack;
									i--;
									mBldr.Instructions.RemoveAt(i);
								}
							}
							else if (ins.Args[0].Source == SourceType.ILStack)
							{
								ins.Args[0].Source = SourceType.SyntheticStack;
								i--;
								mBldr.Instructions.RemoveAt(i);
							}
							prevWasSynth = false;
						}
						break;
				}
			}
		}
		#endregion

		#region ComplexIfCreation
		/// <summary>
		/// Performs the creation of complex if statements.
		/// These statements are much more effecient than
		/// the normal if statements.
		/// </summary>
		private static void ComplexIfCreation(IRMethodBuilder mBldr)
		{
			#region Complex Condition Testing
			//if (mBldr.MethodName == "Function_65")
			//{
			//    ComplexCondition cond = new ComplexCondition();
			//    ComplexCondition andCond = new ComplexCondition();
			//    ComplexCondition andCondSideA = new ComplexCondition();
			//    ComplexCondition andCondSideB = new ComplexCondition();
			//    andCond.Condition = IfCondition.And;
			//    andCond.ParentCondition = cond;
			//    andCond.SideA = andCondSideA;
			//    andCond.SideB = andCondSideB;
			//    andCondSideA.SideA = new ComplexCondition.ComplexMeasurePointSize();
			//    andCondSideA.SideB = new ComplexCondition.ComplexLiteral(9);
			//    andCondSideA.Condition = IfCondition.GreaterOrEqual;
			//    andCondSideB.SideA = new ComplexCondition.ComplexMeasurePointSize();
			//    andCondSideB.SideB = new ComplexCondition.ComplexLiteral(11);
			//    andCondSideB.Condition = IfCondition.LessOrEqual;
			//    ComplexCondition orCond = new ComplexCondition();
			//    orCond.Condition = IfCondition.NotEqual;
			//    orCond.SideA = new ComplexCondition.ComplexMeasurePointSize();
			//    orCond.SideB = new ComplexCondition.ComplexLiteral(10);
			//    cond.SideA = andCond;
			//    cond.SideB = orCond;
			//    cond.Condition = IfCondition.And;
			//    mBldr.Instructions.Insert(1, cond);
			//    mBldr.Instructions.Insert(2, new ConsoleWriteLine("Hey! It was true!"));
			//    mBldr.Instructions.Insert(3, new Else());
			//    mBldr.Instructions.Insert(4, new ConsoleWriteLine("Woops, it was false!"));
			//    mBldr.Instructions.Insert(5, new EIf());
			//}
			#endregion

			// Start at the 2nd instruction,
			// As nothing can be done if it's
			// the first instruction.
			for (int i = 1; i < mBldr.Instructions.Count; i++)
			{
				IRInstruction ins = mBldr.Instructions[i];
				switch (ins.OpCode)
				{
					case IROpCode.If:
					{
						IRInstruction ins2 = mBldr.Instructions[i - 1];
						switch (ins2.OpCode)
						{
							case IROpCode.EQ:
							case IROpCode.NEQ:
							case IROpCode.GT:
							case IROpCode.GTEQ:
							case IROpCode.LT:
							case IROpCode.LTEQ:
							{
								mBldr.Instructions[i] = new ComplexIf();
								ins = mBldr.Instructions[i];
								ins.Args[0] = ins2.Args[0];
								ins.Args[1] = ins2.Args[1];
								ComplexIf cIf = (ComplexIf)ins;
								switch (ins2.OpCode)
								{
									case IROpCode.EQ:
										cIf.Condition = IfCondition.Equal;
										break;
									case IROpCode.NEQ:
										cIf.Condition = IfCondition.NotEqual;
										break;
									case IROpCode.GT:
										cIf.Condition = IfCondition.Greater;
										break;
									case IROpCode.GTEQ:
										cIf.Condition = IfCondition.GreaterOrEqual;
										break;
									case IROpCode.LT:
										cIf.Condition = IfCondition.Less;
										break;
									case IROpCode.LTEQ:
										cIf.Condition = IfCondition.LessOrEqual;
										break;
								}
								mBldr.Instructions.RemoveAt(i - 1);
								break;
							}
						}
						break;
					}
				}
			}
		}
		#endregion

	}
}
