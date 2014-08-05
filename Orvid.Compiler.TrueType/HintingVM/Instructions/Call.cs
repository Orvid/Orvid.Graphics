using System;
using System.IO;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;
using Orvid.Compiler.TrueType.Utils;
using Orvid.TrueType;

namespace Orvid.Compiler.TrueType.HintingVM.Instructions
{
	public class Call : IRInstruction
	{

		public override IROpCode OpCode
		{
			get { return IROpCode.Call; }
		}

		public Call()
		{
		}

		public override void Emit(IRMethodBuilder IRbldr, ILGenerator gen, MethodBuilder mBldr, TypeBuilder tBldr, bool isGlyph)
		{
			if (Args[0].Source == SourceType.Constant)
			{
				if (IRbldr.ParentAssembly.FpgmFunctions[Args[0].Constant].mBldr == null)
				{
					IRbldr.ParentAssembly.DelayedFpgmFunctions.Enqueue(IRbldr);
					IRbldr.Delayed = true;
					return;
				}
				else
				{
					LoadGraphicsState(gen, IRbldr);
					gen.Emit(OpCodes.Call, IRbldr.ParentAssembly.FpgmFunctions[Args[0].Constant].mBldr);
				}
			}
			else
			{
				if (Args[0].Source == SourceType.ILStack)
				{
					gen.Emit(OpCodes.Stloc_0);
				}
				LoadGraphicsState(gen, IRbldr);
				//gen.Emit(OpCodes.Ldfld, GraphicsState_Functions);
				if (Args[0].Source == SourceType.ILStack)
				{
					gen.Emit(OpCodes.Ldloc_0);
				}
				LoadArgument(gen, 1, IRbldr);
				//gen.Emit(OpCodes.Callvirt, typeof(Dictionary<int, HintingMethod>).GetMethod("get_Item"));
				//LoadGraphicsState(gen, IRbldr);
				//gen.Emit(OpCodes.Callvirt, typeof(HintingMethod).GetMethod("Invoke"));
				gen.Emit(OpCodes.Call, typeof(GraphicsState).GetMethod("CallFunction"));
			}
		}

		public override void WriteText(IRMethodBuilder IRbldr, StreamWriter tOut)
		{
			IRbldr.TWriteLine(tOut, "Call[]" + GetArgString(1, false));
			IRbldr.curInstructionLength = ("Call[]" + GetArgString(1, false)).Length;
		}
	}
}
