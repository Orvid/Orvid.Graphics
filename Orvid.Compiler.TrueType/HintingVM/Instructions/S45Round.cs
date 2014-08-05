using System;
using System.IO;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;
using Orvid.Compiler.TrueType.Utils;

namespace Orvid.Compiler.TrueType.HintingVM.Instructions
{
	public class S45Round : IRInstruction
	{

		public override IROpCode OpCode
		{
			get { return IROpCode.S45Round; }
		}

		public S45Round()
		{
		}

		public override void Emit(IRMethodBuilder IRbldr, ILGenerator gen, MethodBuilder mBldr, TypeBuilder tBldr, bool isGlyph)
		{
			if (Args[0].Source == SourceType.ILStack)
			{
				gen.Emit(OpCodes.Stloc_0);
			}
			LoadGraphicsState(gen, IRbldr);
			gen.Emit(OpCodes.Ldc_I4_7);
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
			IRbldr.TWriteLine(tOut, "S45Round[]" + GetArgString(1, false));
			IRbldr.curInstructionLength = ("S45Round[]" + GetArgString(1, false)).Length;
		}
	}
}
