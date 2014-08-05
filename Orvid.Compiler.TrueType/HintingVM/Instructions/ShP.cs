using System;
using System.IO;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;
using Orvid.Compiler.TrueType.Utils;

namespace Orvid.Compiler.TrueType.HintingVM.Instructions
{
	public class ShP : IRInstruction
	{
		public bool UseZp0;

		public override IROpCode OpCode
		{
			get { return IROpCode.ShP; }
		}

		public ShP(byte b)
		{
			UseZp0 = IsBitSet(b, 0);
		}

		public override void Emit(IRMethodBuilder IRbldr, ILGenerator gen, MethodBuilder mBldr, TypeBuilder tBldr, bool isGlyph)
		{
			LoadBool(gen, UseZp0);
			LoadGraphicsState(gen, IRbldr);
			gen.Emit(OpCodes.Call, GraphicsState_LoopedShiftPoint);
		}

		public override void WriteText(IRMethodBuilder IRbldr, StreamWriter tOut)
		{
			IRbldr.TWriteLine(tOut, "ShP[" + BoolToInt(UseZp0).ToString() + "]");
			IRbldr.curInstructionLength = 6;
		}
	}
}
