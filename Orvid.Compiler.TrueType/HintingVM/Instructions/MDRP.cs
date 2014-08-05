using System;
using System.IO;
using System.Text;
using Orvid.TrueType;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;
using Orvid.Compiler.TrueType.Utils;

namespace Orvid.Compiler.TrueType.HintingVM.Instructions
{
	public class MDRP : IRInstruction
	{
		public bool SetRp0;
		public bool UseMinimumDistance;
		public bool Round;
		public DistanceType MeasureDistanceType;

		public override IROpCode OpCode
		{
			get { return IROpCode.MDRP; }
		}

		public MDRP(byte b)
		{
			MeasureDistanceType = (DistanceType)(b & 0x03);
			Round = IsBitSet(b, 2);
			UseMinimumDistance = IsBitSet(b, 3);
			SetRp0 = IsBitSet(b, 4);
		}

		public override void Emit(IRMethodBuilder IRbldr, ILGenerator gen, MethodBuilder mBldr, TypeBuilder tBldr, bool isGlyph)
		{
			LoadArgument(gen, 1, IRbldr);
			LoadBool(gen, Round);
			LoadBool(gen, UseMinimumDistance);
			LoadBool(gen, SetRp0);
			LoadInt(gen, (int)MeasureDistanceType);
			LoadGraphicsState(gen, IRbldr);
			gen.Emit(OpCodes.Call, GraphicsState_MoveDirectRelativePoint);
		}

		public override void WriteText(IRMethodBuilder IRbldr, StreamWriter tOut)
		{
			IRbldr.TWriteLine(tOut, "MDRP[" + BoolToInt(SetRp0).ToString() + BoolToInt(UseMinimumDistance).ToString() + BoolToInt(Round).ToString() + ((int)MeasureDistanceType).ToString() + "]" + GetArgString(1, false));
			IRbldr.curInstructionLength = ("MDRP[" + BoolToInt(SetRp0).ToString() + BoolToInt(UseMinimumDistance).ToString() + BoolToInt(Round).ToString() + ((int)MeasureDistanceType).ToString() + "]" + GetArgString(1, false)).Length;
		}
	}
}
