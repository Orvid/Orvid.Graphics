using System;
using System.IO;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;
using Orvid.Compiler.TrueType.Utils;

namespace Orvid.Compiler.TrueType.HintingVM.Instructions
{
	public class EIf : IRInstruction
	{

		public override IROpCode OpCode
		{
			get { return IROpCode.EIf; }
		}

		public EIf()
		{
		}

		public override void Emit(IRMethodBuilder IRbldr, ILGenerator gen, MethodBuilder mBldr, TypeBuilder tBldr, bool isGlyph)
		{
			Label l = IRbldr.IfEndConditionBranches.Pop();
			gen.MarkLabel(l);
		}

		public override void WriteText(IRMethodBuilder IRbldr, StreamWriter tOut)
		{
			IRbldr.curIdent = IRbldr.curIdent.Substring(0, IRbldr.curIdent.Length - 4);
			IRbldr.TWriteLine(tOut, "EIf");
			IRbldr.curInstructionLength = 3;
		}
	}
}
