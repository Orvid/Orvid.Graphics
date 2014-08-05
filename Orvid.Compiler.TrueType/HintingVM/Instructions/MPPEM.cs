using System;
using System.IO;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;
using Orvid.Compiler.TrueType.Utils;

namespace Orvid.Compiler.TrueType.HintingVM.Instructions
{
	public class MPPEM : IRInstruction
	{

		public override IROpCode OpCode
		{
			get { return IROpCode.MPPEM; }
		}

		public MPPEM()
		{
		}

		public override void Emit(IRMethodBuilder IRbldr, ILGenerator gen, MethodBuilder mBldr, TypeBuilder tBldr, bool isGlyph)
		{
			if (!Destination1IsILStack)
			{
				LoadGraphicsState(gen, IRbldr);
				gen.Emit(OpCodes.Ldfld, GraphicsState_Stack);
			}
			LoadGraphicsState(gen, IRbldr);
			gen.Emit(OpCodes.Call, GraphicsState_GetPixelsPerEM);
			gen.Emit(OpCodes.Conv_U2);
			gen.Emit(OpCodes.Conv_I4);
			if (!Destination1IsILStack)
			{
				gen.Emit(OpCodes.Call, LinkedStack_Push);
			}
		}

		public override void WriteText(IRMethodBuilder IRbldr, StreamWriter tOut)
		{
			IRbldr.TWriteLine(tOut, "MPPEM[]" + GetArgString(0, true));
			IRbldr.curInstructionLength = ("MPPEM[]" + GetArgString(0, true)).Length;
		}
	}
}
