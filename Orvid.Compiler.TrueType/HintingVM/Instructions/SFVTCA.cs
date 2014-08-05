using System;
using System.IO;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;
using Orvid.Compiler.TrueType.Utils;

namespace Orvid.Compiler.TrueType.HintingVM.Instructions
{
	public class SFVTCA : IRInstruction
	{
		public bool IsAxisX;

		public override IROpCode OpCode
		{
			get { return IROpCode.SFVTCA; }
		}

		public SFVTCA(byte instr)
		{
			IsAxisX = IsBitSet(instr, 0);
		}

		public override void Emit(IRMethodBuilder IRbldr, ILGenerator gen, MethodBuilder mBldr, TypeBuilder tBldr, bool isGlyph)
		{
			LoadGraphicsState(gen, IRbldr);
			if (IsAxisX)
				gen.Emit(OpCodes.Ldsfld, VecF2Dot14_Axis_X);
			else
				gen.Emit(OpCodes.Ldsfld, VecF2Dot14_Axis_Y);
			gen.Emit(OpCodes.Stfld, GraphicsState_Freedom_Vector);
			LoadGraphicsState(gen, IRbldr);
			gen.Emit(OpCodes.Call, GraphicsState_RecalcProjFreedomDotProduct);
		}

		public override void WriteText(IRMethodBuilder IRbldr, StreamWriter tOut)
		{
			IRbldr.TWriteLine(tOut, "SFVTCA[" + BoolToInt(IsAxisX).ToString() + "]");
			IRbldr.curInstructionLength = 9;
		}
	}
}
