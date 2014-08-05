using System;
using System.IO;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;
using Orvid.Compiler.TrueType.Utils;

namespace Orvid.Compiler.TrueType.HintingVM.Instructions
{
	public class SPVTCA : IRInstruction
	{
		public bool IsAxisX;

		public override IROpCode OpCode
		{
			get { return IROpCode.SPVTCA; }
		}

		public SPVTCA(byte instr)
		{
			IsAxisX = IsBitSet(instr, 0);
		}

		public override void Emit(IRMethodBuilder IRbldr, ILGenerator gen, MethodBuilder mBldr, TypeBuilder tBldr, bool isGlyph)
		{
			LoadGraphicsState(gen, IRbldr);
			gen.Emit(OpCodes.Dup);
			gen.Emit(OpCodes.Dup);
			if (IsAxisX)
				gen.Emit(OpCodes.Ldsfld, VecF2Dot14_Axis_X);
			else
				gen.Emit(OpCodes.Ldsfld, VecF2Dot14_Axis_Y);
			gen.Emit(OpCodes.Stfld, GraphicsState_Projection_Vector);

			if (IsAxisX)
				gen.Emit(OpCodes.Ldsfld, VecF2Dot14_Axis_X);
			else
				gen.Emit(OpCodes.Ldsfld, VecF2Dot14_Axis_Y);
			gen.Emit(OpCodes.Stfld, GraphicsState_Dual_Projection_Vector);
			
			gen.Emit(OpCodes.Call, GraphicsState_RecalcProjFreedomDotProduct);
		}

		public override void WriteText(IRMethodBuilder IRbldr, StreamWriter tOut)
		{
			IRbldr.TWriteLine(tOut, "SPVTCA[" + BoolToInt(IsAxisX).ToString() + "]");
			IRbldr.curInstructionLength = 9;
		}
	}
}
