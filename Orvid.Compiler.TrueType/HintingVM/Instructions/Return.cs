using System;
using System.IO;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;
using Orvid.Compiler.TrueType.Utils;

namespace Orvid.Compiler.TrueType.HintingVM.Instructions
{
	public class Return : IRInstruction
	{

		public override IROpCode OpCode
		{
			get { return IROpCode.Return; }
		}

		public Return()
		{
		}

		public override void Emit(IRMethodBuilder IRbldr, ILGenerator gen, MethodBuilder mBldr, TypeBuilder tBldr, bool isGlyph)
		{
			gen.Emit(OpCodes.Ret);
		}

		public override void WriteText(IRMethodBuilder IRbldr, StreamWriter tOut)
		{
			IRbldr.TWriteLine(tOut, "Synth-Return" + GetArgString(0, true));
			IRbldr.curInstructionLength = ("Synth-Return" + GetArgString(0, true)).Length;
		}
	}
}
