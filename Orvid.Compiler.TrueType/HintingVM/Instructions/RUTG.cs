using System;
using System.IO;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;
using Orvid.Compiler.TrueType.Utils;

namespace Orvid.Compiler.TrueType.HintingVM.Instructions
{
	public class RUTG : IRInstruction
	{

		public override IROpCode OpCode
		{
			get { return IROpCode.RUTG; }
		}

		public RUTG()
		{
		}

		public override void Emit(IRMethodBuilder IRbldr, ILGenerator gen, MethodBuilder mBldr, TypeBuilder tBldr, bool isGlyph)
		{
			LoadGraphicsState(gen, IRbldr);
			gen.Emit(OpCodes.Ldc_I4_4);
			gen.Emit(OpCodes.Stfld, GraphicsState_RoundMode);
		}

		public override void WriteText(IRMethodBuilder IRbldr, StreamWriter tOut)
		{
			IRbldr.TWriteLine(tOut, "RUTG[]");
			IRbldr.curInstructionLength = 6;
		}
	}
}
