using System;
using System.IO;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;
using Orvid.Compiler.TrueType.Utils;

namespace Orvid.Compiler.TrueType.HintingVM.Instructions
{
	public class MSIRP : IRInstruction
	{
		public bool SetRp0;

		public override IROpCode OpCode
		{
			get { return IROpCode.MSIRP; }
		}

		public MSIRP(byte b)
		{
			SetRp0 = IsBitSet(b, 0);
		}

		public override void Emit(IRMethodBuilder IRbldr, ILGenerator gen, MethodBuilder mBldr, TypeBuilder tBldr, bool isGlyph)
		{
			if (Args[1].Source == SourceType.ILStack)
			{
				gen.Emit(OpCodes.Stloc_0);
			}
			LoadArgument(gen, 1, IRbldr);
			if (Args[1].Source == SourceType.ILStack)
			{
				gen.Emit(OpCodes.Ldloc_0);
			}
			LoadArgument(gen, 2, IRbldr);
			if (SetRp0)
				gen.Emit(OpCodes.Ldc_I4_1);
			else
				gen.Emit(OpCodes.Ldc_I4_0);
			LoadGraphicsState(gen, IRbldr);
			gen.Emit(OpCodes.Call, GraphicsState_MoveStackIndirectRelativePoint);
		}

		public override void WriteText(IRMethodBuilder IRbldr, StreamWriter tOut)
		{
			IRbldr.TWriteLine(tOut, "MSIRP[" + BoolToInt(SetRp0).ToString() + "]" + GetArgString(2, false));
			IRbldr.curInstructionLength = ("MSIRP[" + BoolToInt(SetRp0).ToString() + "]" + GetArgString(2, false)).Length;
		}

		public override bool ExpectsArg1F26Dot6 { get { return true; } }
	}
}
