using System;
using System.IO;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;
using Orvid.Compiler.TrueType.Utils;

namespace Orvid.Compiler.TrueType.HintingVM.Instructions
{
	public class SPVTL : IRInstruction
	{
		public bool IsPerp;

		public override IROpCode OpCode
		{
			get { return IROpCode.SPVTL; }
		}

		public SPVTL(byte b)
		{
			IsPerp = IsBitSet(b, 0);
		}

		public override void Emit(IRMethodBuilder IRbldr, ILGenerator gen, MethodBuilder mBldr, TypeBuilder tBldr, bool isGlyph)
		{
			if (Args[1].Source == SourceType.ILStack && Args[0].Source != SourceType.ILStack)
			{
				gen.Emit(OpCodes.Stloc_2);
			}
			LoadArgument(gen, 1, IRbldr);
			if (Args[1].Source == SourceType.ILStack && Args[0].Source != SourceType.ILStack)
			{
				gen.Emit(OpCodes.Ldloc_2);
			}
			LoadArgument(gen, 2, IRbldr);
			LoadBool(gen, IsPerp);
			LoadGraphicsState(gen, IRbldr);
			gen.Emit(OpCodes.Call, GraphicsState_SetProjectionVectorToLine);
		}

		public override void WriteText(IRMethodBuilder IRbldr, StreamWriter tOut)
		{
			IRbldr.TWriteLine(tOut, "SPVTL[" + BoolToInt(IsPerp).ToString() + "]" + GetArgString(2, false));
			IRbldr.curInstructionLength = ("SPVTL[" + BoolToInt(IsPerp).ToString() + "]" + GetArgString(2, false)).Length;
		}
	}
}
