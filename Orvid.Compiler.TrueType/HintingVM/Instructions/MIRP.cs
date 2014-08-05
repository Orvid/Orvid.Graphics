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
	public class MIRP : IRInstruction
	{
		public bool SetRp0;
		public bool UseMinimumDistance;
		public bool Round;
		public DistanceType MeasureDistanceType;

		public override IROpCode OpCode
		{
			get { return IROpCode.MIRP; }
		}

		public MIRP(byte b)
		{
			MeasureDistanceType = (DistanceType)(b & 0x03);
			Round = IsBitSet(b, 2);
			UseMinimumDistance = IsBitSet(b, 3);
			SetRp0 = IsBitSet(b, 4);
		}

		public override void Emit(IRMethodBuilder IRbldr, ILGenerator gen, MethodBuilder mBldr, TypeBuilder tBldr, bool isGlyph)
		{
			if (Args[1].Source == SourceType.ILStack && Args[0].Source != SourceType.ILStack)
			{
				gen.Emit(OpCodes.Stloc_0);
			}
			LoadArgument(gen, 1, IRbldr);
			if (Args[1].Source == SourceType.ILStack && Args[0].Source != SourceType.ILStack)
			{
				gen.Emit(OpCodes.Ldloc_0);
			}
			LoadArgument(gen, 2, IRbldr);
			LoadBool(gen, Round);
			LoadBool(gen, UseMinimumDistance);
			LoadBool(gen, SetRp0);
			LoadInt(gen, (int)MeasureDistanceType);
			LoadGraphicsState(gen, IRbldr);
			gen.Emit(OpCodes.Call, GraphicsState_MoveIndirectRelativePoint);
		}

		public override void WriteText(IRMethodBuilder IRbldr, StreamWriter tOut)
		{
			IRbldr.TWriteLine(tOut, "MIRP[" + BoolToInt(SetRp0).ToString() + BoolToInt(UseMinimumDistance).ToString() + BoolToInt(Round).ToString() + ((int)MeasureDistanceType).ToString() + "]" + GetArgString(2, false));
			IRbldr.curInstructionLength = ("MIRP[" + BoolToInt(SetRp0).ToString() + BoolToInt(UseMinimumDistance).ToString() + BoolToInt(Round).ToString() + ((int)MeasureDistanceType).ToString() + "]" + GetArgString(2, false)).Length;
		}
	}
}
