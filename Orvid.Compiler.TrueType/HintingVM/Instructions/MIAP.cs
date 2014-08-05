using System;
using System.IO;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;
using Orvid.Compiler.TrueType.Utils;

namespace Orvid.Compiler.TrueType.HintingVM.Instructions
{
	public class MIAP : IRInstruction
	{
		public bool Round;

		public override IROpCode OpCode
		{
			get { return IROpCode.MIAP; }
		}

		public MIAP(byte b)
		{
			Round = IsBitSet(b, 0);
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
			if (Round)
				gen.Emit(OpCodes.Ldc_I4_1);
			else
				gen.Emit(OpCodes.Ldc_I4_0);
			LoadGraphicsState(gen, IRbldr);
			gen.Emit(OpCodes.Call, GraphicsState_MoveIndirectAbsolutePoint);
		}

		public override void WriteText(IRMethodBuilder IRbldr, StreamWriter tOut)
		{
			IRbldr.TWriteLine(tOut, "MIAP[" + BoolToInt(Round).ToString() + "]" + GetArgString(2, false));
			IRbldr.curInstructionLength = ("MIAP[" + BoolToInt(Round).ToString() + "]" + GetArgString(2, false)).Length;
		}
	}
}
