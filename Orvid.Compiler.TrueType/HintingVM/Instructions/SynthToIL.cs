using System;
using System.IO;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;
using Orvid.Compiler.TrueType.Utils;

namespace Orvid.Compiler.TrueType.HintingVM.Instructions
{
	public class SynthToIL : IRInstruction
	{

		public override IROpCode OpCode
		{
			get { return IROpCode.SynthToIL; }
		}

		public SynthToIL()
		{
		}

		public override void Emit(IRMethodBuilder IRbldr, ILGenerator gen, MethodBuilder mBldr, TypeBuilder tBldr, bool isGlyph)
		{
			if (Args[0].Source == SourceType.SyntheticStack)
			{
				LoadGraphicsState(gen, IRbldr);
				gen.Emit(OpCodes.Ldfld, GraphicsState_Stack);
				gen.Emit(OpCodes.Call, LinkedStack_Pop);
			}
			// Otherwise it's not actually needed, and
			// was likely inserted by an instruction like
			// swap, because one of it's args was a constant.
		}

		public override void WriteText(IRMethodBuilder IRbldr, StreamWriter tOut)
		{
			if (Args[0].Source == SourceType.SyntheticStack)
			{
				IRbldr.TWriteLine(tOut, "SynthToIL");
				IRbldr.curInstructionLength = ("SynthToIL").Length;
			}
			else
			{
				IRbldr.curInstructionLength = 0;
			}
		}
	}
}
