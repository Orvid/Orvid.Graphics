using System;
using System.IO;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;
using Orvid.Compiler.TrueType.Utils;

namespace Orvid.Compiler.TrueType.HintingVM.Instructions
{
	public class JROT : IRInstruction
	{

		public override IROpCode OpCode
		{
			get { return IROpCode.JROT; }
		}

		public JROT()
		{
		}

		public override void Emit(IRMethodBuilder IRbldr, ILGenerator gen, MethodBuilder mBldr, TypeBuilder tBldr, bool isGlyph)
		{
			throw new Exception("This should never be getting called! Any method with a JROT should be getting interpreted!");
		}

		public override void WriteText(IRMethodBuilder IRbldr, StreamWriter tOut)
		{
			IRbldr.TWriteLine(tOut, "JROT[]" + GetArgString(2, false));
			IRbldr.curInstructionLength = ("JROT[]" + GetArgString(2, false)).Length;
		}
	}
}
