using System;
using System.IO;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;
using Orvid.Compiler.TrueType.Utils;

namespace Orvid.Compiler.TrueType.HintingVM.Instructions
{
	public class MDAP : IRInstruction
	{
		public bool Round;

		public override IROpCode OpCode
		{
			get { return IROpCode.MDAP; }
		}

		public MDAP(byte b)
		{
			Round = IsBitSet(b, 0);
		}

		public override void Emit(IRMethodBuilder IRbldr, ILGenerator gen, MethodBuilder mBldr, TypeBuilder tBldr, bool isGlyph)
		{
			LoadArgument(gen, 1, IRbldr);
			if (Round)
				gen.Emit(OpCodes.Ldc_I4_1);
			else
				gen.Emit(OpCodes.Ldc_I4_0);
			LoadGraphicsState(gen, IRbldr);
			gen.Emit(OpCodes.Call, GraphicsState_MoveDirectAbsolutePoint);
		}

		public override void WriteText(IRMethodBuilder IRbldr, StreamWriter tOut)
		{
			IRbldr.TWriteLine(tOut, "MDAP[" + BoolToInt(Round).ToString() + "]" + GetArgString(1, false));
			IRbldr.curInstructionLength = ("MDAP[" + BoolToInt(Round).ToString() + "]" + GetArgString(1, false)).Length;
		}
	}
}
