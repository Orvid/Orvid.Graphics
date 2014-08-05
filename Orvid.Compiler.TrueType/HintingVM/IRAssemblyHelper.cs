using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection.Emit;
using Orvid.Compiler.TrueType.HintingVM.Instructions;
using Orvid.TrueType;

namespace Orvid.Compiler.TrueType.HintingVM
{
	public static class IRAssemblyHelper
	{
		public static void FoldFPGMConstants(IRMethodBuilder mBldr)
		{
			bool hitFSet = false;
			// Make sure we can actually fold the constants.
			foreach (IRInstruction i in mBldr.Instructions)
			{
				switch (i.OpCode)
				{
					case IROpCode.NPushB:
					case IROpCode.NPushW:
					case IROpCode.PushB:
					case IROpCode.PushW:
						if (hitFSet)
							throw new Exception("The method isn't possible to optimize in this manner!");
						break;
					case IROpCode.FSet:
						hitFSet = true;
						break;
					default:
						throw new Exception("Invalid Op-Code in the FPGM Program!");
				}
			}
			LinkedStack<int> stack = new LinkedStack<int>();
			int fSetStartIndex = 0;
			for (int i = 0; i < mBldr.Instructions.Count; i++)
			{
				IRInstruction ins = mBldr.Instructions[i];
				switch (ins.OpCode)
				{
					case IROpCode.PushB:
					{
						PushB b = (PushB)ins;
						for (int i2 = 0; i2 < b.ValuesToLoad.Length; i2++)
						{
							stack.Push((int)(byte)b.ValuesToLoad[i2]);
						}
						break;
					}
					case IROpCode.NPushB:
					{
						NPushB b = (NPushB)ins;
						for (int i2 = 0; i2 < b.ValuesToLoad.Length; i2++)
						{
							stack.Push((int)(byte)b.ValuesToLoad[i2]);
						}
						break;
					}
					case IROpCode.PushW:
					{
						PushW b = (PushW)ins;
						for (int i2 = 0; i2 < b.ValuesToLoad.Length; i2++)
						{
							stack.Push((int)(short)(ushort)b.ValuesToLoad[i2]);
						}
						break;
					}
					case IROpCode.NPushW:
					{
						NPushW b = (NPushW)ins;
						for (int i2 = 0; i2 < b.ValuesToLoad.Length; i2++)
						{
							stack.Push((int)(short)(ushort)b.ValuesToLoad[i2]);
						}
						break;
					}
					case IROpCode.FSet:
					{
						if (fSetStartIndex == 0)
							fSetStartIndex = i;
						FSet b = (FSet)ins;
						b.HasConstantIndex = true;
						b.ConstantIndex = stack.Pop();
						if (!mBldr.ParentAssembly.FpgmFunctions.ContainsKey(b.ConstantIndex))
						{
							mBldr.ParentAssembly.FpgmFunctions.Add(b.ConstantIndex, b.MethodToSet);
						}
						else
						{
							throw new Exception("Tried to re-declare a FPGM function!");
						}
						break;
					}
				}
			}
			mBldr.Instructions.RemoveRange(0, fSetStartIndex);
			if (stack.Depth != 0)
				throw new Exception("The stack wasn't empty when we finished!");
		}

		public static int CurFuncNumber = 1;
		public static List<IRMethodBuilder> SeperateFunctions(IRMethodBuilder mBldr)
		{
			List<IRMethodBuilder> funcs = new List<IRMethodBuilder>();
			int fStartIdx;
			int fEndIdx;
			IRMethodBuilder newMth;
		Restart:
			fStartIdx = 0;
			fEndIdx = 0;
			newMth = null;
			for (int i = 0; i < mBldr.Instructions.Count; i++)
			{
				IRInstruction ins = mBldr.Instructions[i];
				if (ins.OpCode == IROpCode.FDef)
				{
					fStartIdx = i;
				}
				else if (ins.OpCode == IROpCode.EndF)
				{
					fEndIdx = i;
					newMth = new IRMethodBuilder("Function_" + CurFuncNumber.ToString(), false, mBldr.ParentAssembly);
					newMth.Instructions.AddRange(mBldr.Instructions.GetRange(fStartIdx, fEndIdx - fStartIdx + 1));
					funcs.Add(newMth);
					mBldr.Instructions[fStartIdx] = new FSet(newMth);
					// Remove the FDef...EndF (FDef through EndF)
					mBldr.Instructions.RemoveRange(fStartIdx + 1, fEndIdx - fStartIdx);
					CurFuncNumber++;
					goto Restart;
				}
			}
			return funcs;
		}
	}
}
