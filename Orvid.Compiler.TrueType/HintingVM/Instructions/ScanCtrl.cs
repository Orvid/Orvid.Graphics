using System;
using System.IO;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;
using Orvid.Compiler.TrueType.Utils;

namespace Orvid.Compiler.TrueType.HintingVM.Instructions
{
	public class ScanCtrl : IRInstruction
	{

		public override IROpCode OpCode
		{
			get { return IROpCode.ScanCtrl; }
		}

		public ScanCtrl()
		{
		}

		public override void Emit(IRMethodBuilder IRbldr, ILGenerator gen, MethodBuilder mBldr, TypeBuilder tBldr, bool isGlyph)
		{
			LoadArgument(gen, 1, IRbldr);
			gen.Emit(OpCodes.Pop);
			EmitWarning(gen, "ScanCtrl doesn't do anything yet!");
		}

		public override void WriteText(IRMethodBuilder IRbldr, StreamWriter tOut)
		{
			IRbldr.TWriteLine(tOut, "ScanCtrl[]" + GetArgString(2, false));
			IRbldr.curInstructionLength = ("ScanCtrl[]" + GetArgString(2, false)).Length;
		}
	}
}
