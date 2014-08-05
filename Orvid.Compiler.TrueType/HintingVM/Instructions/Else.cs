using System;
using System.IO;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;
using Orvid.Compiler.TrueType.Utils;

namespace Orvid.Compiler.TrueType.HintingVM.Instructions
{
	public class Else : IRInstruction
	{

		public override IROpCode OpCode
		{
			get { return IROpCode.Else; }
		}

		public Else()
		{
		}

		public override void Emit(IRMethodBuilder IRbldr, ILGenerator gen, MethodBuilder mBldr, TypeBuilder tBldr, bool isGlyph)
		{
			Label lbl = gen.DefineLabel();
			// Branch to the end of the if statement.
			gen.Emit(OpCodes.Br, lbl);

			Label l = IRbldr.IfEndConditionBranches.Pop();
			gen.MarkLabel(l);
			IRbldr.IfEndConditionBranches.Push(lbl);
		}

		public override void WriteText(IRMethodBuilder IRbldr, StreamWriter tOut)
		{
			IRbldr.curIdent = IRbldr.curIdent.Substring(0, IRbldr.curIdent.Length - 4);
			IRbldr.TWriteLine(tOut, "Else");
			IRbldr.curInstructionLength = 4;
			IRbldr.curIdent += "".PadLeft(4, ' ');
		}
	}
}
