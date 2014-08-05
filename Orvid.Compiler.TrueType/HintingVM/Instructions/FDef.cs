using System;
using System.IO;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;
using Orvid.Compiler.TrueType.Utils;

namespace Orvid.Compiler.TrueType.HintingVM.Instructions
{
	public class FDef : IRInstruction
	{

		public override IROpCode OpCode
		{
			get { return IROpCode.FDef; }
		}

		public FDef()
		{
		}

		public override void Emit(IRMethodBuilder IRbldr, ILGenerator gen, MethodBuilder mBldr, TypeBuilder tBldr, bool isGlyph)
		{
		}

		public override void WriteText(IRMethodBuilder IRbldr, StreamWriter tOut)
		{
			IRbldr.TWriteLine(tOut, "FDef");
			IRbldr.curInstructionLength = 4;
			IRbldr.curIdent += "".PadLeft(4, ' ');
		}
	}
}
