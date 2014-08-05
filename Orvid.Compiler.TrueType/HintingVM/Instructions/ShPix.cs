using System;
using System.IO;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;
using Orvid.Compiler.TrueType.Utils;

namespace Orvid.Compiler.TrueType.HintingVM.Instructions
{
	public class ShPix : IRInstruction
	{

		public override IROpCode OpCode
		{
			get { return IROpCode.ShPix; }
		}

		public ShPix()
		{
		}

		public override void Emit(IRMethodBuilder IRbldr, ILGenerator gen, MethodBuilder mBldr, TypeBuilder tBldr, bool isGlyph)
		{
			LoadArgument(gen, 1, IRbldr);
			LoadGraphicsState(gen, IRbldr);
			gen.Emit(OpCodes.Call, GraphicsState_LoopedShiftPixel);
		}

		public override void WriteText(IRMethodBuilder IRbldr, StreamWriter tOut)
		{
			IRbldr.TWriteLine(tOut, "ShPix[](" + (Args[0].IsF26Dot6 ? "" : "(F26Dot6)") + ((Args[0].Source == SourceType.Constant) ? Args[1].Constant.ToString() : Args[0].Source.ToString()) + ", ...)");
			IRbldr.curInstructionLength = ("ShPix[](" + (Args[0].IsF26Dot6 ? "" : "(F26Dot6)") + ((Args[0].Source == SourceType.Constant) ? Args[1].Constant.ToString() : Args[0].Source.ToString()) + ", ...)").Length;
		}

		public override bool ExpectsArg1F26Dot6 { get { return true; } }
	}
}
