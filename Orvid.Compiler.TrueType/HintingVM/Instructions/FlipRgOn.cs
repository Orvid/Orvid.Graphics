using System;
using System.IO;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;
using Orvid.Compiler.TrueType.Utils;

namespace Orvid.Compiler.TrueType.HintingVM.Instructions
{
	public class FlipRgOn : IRInstruction
	{

		public override IROpCode OpCode
		{
			get { return IROpCode.FlipRgOn; }
		}

		public FlipRgOn()
		{
		}

		public override void Emit(IRMethodBuilder IRbldr, ILGenerator gen, MethodBuilder mBldr, TypeBuilder tBldr, bool isGlyph)
		{
			LoadGraphicsState(gen, IRbldr);
			LoadArgument(gen, 1, IRbldr);
			LoadArgument(gen, 2, IRbldr);
			gen.Emit(OpCodes.Call, GraphicsState_SetRangeOnCurve);
		}

		public override void WriteText(IRMethodBuilder IRbldr, StreamWriter tOut)
		{
			IRbldr.TWriteLine(tOut, "FlipRgOn[]");
			IRbldr.curInstructionLength = 10;
		}
	}
}
