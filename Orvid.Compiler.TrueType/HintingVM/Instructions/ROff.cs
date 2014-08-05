using System;
using System.IO;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;
using Orvid.Compiler.TrueType.Utils;

namespace Orvid.Compiler.TrueType.HintingVM.Instructions
{
	public class ROff : IRInstruction
	{

		public override IROpCode OpCode
		{
			get { return IROpCode.ROff; }
		}

		public ROff()
		{
		}

		public override void Emit(IRMethodBuilder IRbldr, ILGenerator gen, MethodBuilder mBldr, TypeBuilder tBldr, bool isGlyph)
		{
			LoadGraphicsState(gen, IRbldr);
			gen.Emit(OpCodes.Ldc_I4_5);
			gen.Emit(OpCodes.Stfld, GraphicsState_RoundMode);
		}

		public override void WriteText(IRMethodBuilder IRbldr, StreamWriter tOut)
		{
			IRbldr.TWriteLine(tOut, "ROff[]");
			IRbldr.curInstructionLength = 6;
		}
	}
}
