using System;
using System.IO;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;
using Orvid.Compiler.TrueType.Utils;

namespace Orvid.Compiler.TrueType.HintingVM.Instructions
{
	public class ISect : IRInstruction
	{

		public override IROpCode OpCode
		{
			get { return IROpCode.ISect; }
		}

		public ISect()
		{
		}

		public override void Emit(IRMethodBuilder IRbldr, ILGenerator gen, MethodBuilder mBldr, TypeBuilder tBldr, bool isGlyph)
		{
			LoadGraphicsState(gen, IRbldr);
			gen.Emit(OpCodes.Call, GraphicsState_ISect);
		}

		public override void WriteText(IRMethodBuilder IRbldr, StreamWriter tOut)
		{
			IRbldr.TWriteLine(tOut, "ISect[]");
			IRbldr.curInstructionLength = 7;
		}
	}
}
