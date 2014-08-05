using System;
using System.IO;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;
using Orvid.Compiler.TrueType.Utils;

namespace Orvid.Compiler.TrueType.HintingVM.Instructions
{
	public class FlipPt : IRInstruction
	{

		public override IROpCode OpCode
		{
			get { return IROpCode.FlipPt; }
		}

		public FlipPt()
		{
		}

		public override void Emit(IRMethodBuilder IRbldr, ILGenerator gen, MethodBuilder mBldr, TypeBuilder tBldr, bool isGlyph)
		{
#warning Need to use the IL stack here!
			LoadGraphicsState(gen, IRbldr);
			gen.Emit(OpCodes.Call, GraphicsState_LoopedFlipOnCurve);
		}

		public override void WriteText(IRMethodBuilder IRbldr, StreamWriter tOut)
		{
			IRbldr.TWriteLine(tOut, "FlipPt[]");
			IRbldr.curInstructionLength = 8;
		}
	}
}
