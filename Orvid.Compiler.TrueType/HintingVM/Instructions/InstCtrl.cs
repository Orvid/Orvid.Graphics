using System;
using System.IO;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;
using Orvid.Compiler.TrueType.Utils;

namespace Orvid.Compiler.TrueType.HintingVM.Instructions
{
	public class InstCtrl : IRInstruction
	{

		public override IROpCode OpCode
		{
			get { return IROpCode.InstCtrl; }
		}

		public InstCtrl()
		{
		}

		public override void Emit(IRMethodBuilder IRbldr, ILGenerator gen, MethodBuilder mBldr, TypeBuilder tBldr, bool isGlyph)
		{
			LoadArgument(gen, 1, IRbldr);
			gen.Emit(OpCodes.Pop);

			LoadArgument(gen, 2, IRbldr);
			gen.Emit(OpCodes.Pop);
			EmitWarning(gen, "InstCtrl doesn't do anything yet!");
		}

		public override void WriteText(IRMethodBuilder IRbldr, StreamWriter tOut)
		{
			IRbldr.TWriteLine(tOut, "InstCtrl[]" + GetArgString(2, false));
			IRbldr.curInstructionLength = ("InstCtrl[]" + GetArgString(2, false)).Length;
		}
	}
}
