using System;
using System.IO;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;
using Orvid.Compiler.TrueType.Utils;

namespace Orvid.Compiler.TrueType.HintingVM.Instructions
{
	public class SCVTCI : IRInstruction
	{

		public override IROpCode OpCode
		{
			get { return IROpCode.SCVTCI; }
		}

		public SCVTCI()
		{
		}

		public override void Emit(IRMethodBuilder IRbldr, ILGenerator gen, MethodBuilder mBldr, TypeBuilder tBldr, bool isGlyph)
		{
			if (Args[0].Source == SourceType.ILStack)
			{
				gen.Emit(OpCodes.Stloc_0);
			}
			LoadGraphicsState(gen, IRbldr);
			if (Args[0].Source == SourceType.ILStack)
			{
				gen.Emit(OpCodes.Ldloc_0);
			}
			LoadArgument(gen, 1, IRbldr);
			gen.Emit(OpCodes.Stfld, GraphicsState_Cvt_CutIn);
		}

		public override void WriteText(IRMethodBuilder IRbldr, StreamWriter tOut)
		{
			IRbldr.TWriteLine(tOut, "SCVTCI[]" + GetArgString(1, false));
			IRbldr.curInstructionLength = ("SCVTCI[]" + GetArgString(1, false)).Length;
		}

		public override bool ExpectsArg1F26Dot6 { get { return true; } }
	}
}
