using System;
using System.IO;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;
using Orvid.Compiler.TrueType.Utils;

namespace Orvid.Compiler.TrueType.HintingVM.Instructions
{
	public class SRound : IRInstruction
	{

		public override IROpCode OpCode
		{
			get { return IROpCode.SRound; }
		}

		public SRound()
		{
		}

		public override void Emit(IRMethodBuilder IRbldr, ILGenerator gen, MethodBuilder mBldr, TypeBuilder tBldr, bool isGlyph)
		{
			if (Args[0].Source == SourceType.ILStack)
			{
				gen.Emit(OpCodes.Stloc_0);
			}
			LoadGraphicsState(gen, IRbldr);
			gen.Emit(OpCodes.Ldc_I4_6);
			gen.Emit(OpCodes.Stfld, GraphicsState_RoundMode);
			LoadGraphicsState(gen, IRbldr);
			if (Args[0].Source == SourceType.ILStack)
			{
				gen.Emit(OpCodes.Ldloc_0);
			}
			LoadArgument(gen, 1, IRbldr);
			gen.Emit(OpCodes.Call, GraphicsState_SetSuperRoundMode);
		}

		public override void WriteText(IRMethodBuilder IRbldr, StreamWriter tOut)
		{
			IRbldr.TWriteLine(tOut, "SRound[]" + GetArgString(1, false));
			IRbldr.curInstructionLength = ("SRound[]" + GetArgString(1, false)).Length;
		}
	}
}
