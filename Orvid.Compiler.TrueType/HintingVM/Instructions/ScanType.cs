using System;
using System.IO;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;
using Orvid.Compiler.TrueType.Utils;

namespace Orvid.Compiler.TrueType.HintingVM.Instructions
{
	public class ScanType : IRInstruction
	{

		public override IROpCode OpCode
		{
			get { return IROpCode.ScanType; }
		}

		public ScanType()
		{
		}

		public override void Emit(IRMethodBuilder IRbldr, ILGenerator gen, MethodBuilder mBldr, TypeBuilder tBldr, bool isGlyph)
		{
			LoadGraphicsState(gen, IRbldr);
			LoadArgument(gen, 1, IRbldr);
			gen.Emit(OpCodes.Stfld, GraphicsState_ScanControl);
		}

		public override void WriteText(IRMethodBuilder IRbldr, StreamWriter tOut)
		{
			IRbldr.TWriteLine(tOut, "ScanType[]");
			IRbldr.curInstructionLength = 10;
		}
	}
}
