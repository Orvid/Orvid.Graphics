using System;
using System.IO;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;
using Orvid.Compiler.TrueType.Utils;

namespace Orvid.Compiler.TrueType.HintingVM.Instructions
{
	public class WCvtF : IRInstruction
	{

		public override IROpCode OpCode
		{
			get { return IROpCode.WCvtF; }
		}

		public WCvtF()
		{
		}

		public override void Emit(IRMethodBuilder IRbldr, ILGenerator gen, MethodBuilder mBldr, TypeBuilder tBldr, bool isGlyph)
		{
			LoadArgument(gen, 1, IRbldr);
			LoadArgument(gen, 2, IRbldr);
			LoadGraphicsState(gen, IRbldr);
			gen.Emit(OpCodes.Call, GraphicsState_WriteCvtEntryF);
		}

		public override void WriteText(IRMethodBuilder IRbldr, StreamWriter tOut)
		{
			IRbldr.TWriteLine(tOut, "WCvtF[]" + GetArgString(2, false));
			IRbldr.curInstructionLength = ("WCvtF[]" + GetArgString(2, false)).Length;
		}
	}
}
