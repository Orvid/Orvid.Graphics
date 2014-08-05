using System;
using System.IO;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;
using Orvid.Compiler.TrueType.Utils;

namespace Orvid.Compiler.TrueType.HintingVM.Instructions
{
	public class If : IRInstruction
	{

		public override IROpCode OpCode
		{
			get { return IROpCode.If; }
		}

		public If()
		{
		}

		public override void Emit(IRMethodBuilder IRbldr, ILGenerator gen, MethodBuilder mBldr, TypeBuilder tBldr, bool isGlyph)
		{
			LoadArgument(gen, 1, IRbldr);
			Label lfalse = gen.DefineLabel();
			gen.Emit(OpCodes.Brfalse, lfalse);
			IRbldr.IfEndConditionBranches.Push(lfalse);
		}

		public override void WriteText(IRMethodBuilder IRbldr, StreamWriter tOut)
		{
			IRbldr.TWriteLine(tOut, "If" + GetArgString(1, false));
			IRbldr.curInstructionLength = ("If" + GetArgString(1, false)).Length;
			IRbldr.curIdent += "".PadLeft(4, ' ');
		}
	}
}
