using System;
using System.IO;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;
using Orvid.Compiler.TrueType.Utils;

namespace Orvid.Compiler.TrueType.HintingVM.Instructions
{
	public class SRP0 : IRInstruction
	{

		public override IROpCode OpCode
		{
			get { return IROpCode.SRP0; }
		}

		public SRP0()
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
			gen.Emit(OpCodes.Stfld, GraphicsState_rp0);
		}

		public override void WriteText(IRMethodBuilder IRbldr, StreamWriter tOut)
		{
			IRbldr.TWriteLine(tOut, "SRP0[]" + GetArgString(1, false));
			IRbldr.curInstructionLength = ("SRP0[]" + GetArgString(1, false)).Length;
		}
	}
}
