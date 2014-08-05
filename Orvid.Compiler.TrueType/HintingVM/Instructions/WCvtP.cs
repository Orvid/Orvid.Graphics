using System;
using System.IO;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;
using Orvid.Compiler.TrueType.Utils;

namespace Orvid.Compiler.TrueType.HintingVM.Instructions
{
	public class WCvtP : IRInstruction
	{

		public override IROpCode OpCode
		{
			get { return IROpCode.WCvtP; }
		}

		public WCvtP()
		{
		}

		public override void Emit(IRMethodBuilder IRbldr, ILGenerator gen, MethodBuilder mBldr, TypeBuilder tBldr, bool isGlyph)
		{
			LoadArgument(gen, 1, IRbldr);
			gen.Emit(OpCodes.Stloc_2);
			LoadArgument(gen, 2, IRbldr);
			gen.Emit(OpCodes.Ldloc_2);
			LoadGraphicsState(gen, IRbldr);
			gen.Emit(OpCodes.Call, GraphicsState_WriteCvtEntry);
		}

		public override void WriteText(IRMethodBuilder IRbldr, StreamWriter tOut)
		{
			IRbldr.TWriteLine(tOut, "WCvtP[]" + GetArgString(2, false));
			IRbldr.curInstructionLength = ("WCvtP[]" + GetArgString(2, false)).Length;
		}

		public override bool ExpectsArg1F26Dot6 { get { return true; } }
	}
}
