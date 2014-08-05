using System;
using System.IO;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;
using Orvid.Compiler.TrueType.Utils;

namespace Orvid.Compiler.TrueType.HintingVM.Instructions
{
	public class CIndex : IRInstruction
	{

		public override IROpCode OpCode
		{
			get { return IROpCode.CIndex; }
		}

		public CIndex()
		{
		}

		public override void Emit(IRMethodBuilder IRbldr, ILGenerator gen, MethodBuilder mBldr, TypeBuilder tBldr, bool isGlyph)
		{
#warning Need to figure out how to do this with the IL Stack.
			if (Args[0].Source == SourceType.ILStack)
			{
				gen.Emit(OpCodes.Stloc_0);
			}
			LoadGraphicsState(gen, IRbldr);
			gen.Emit(OpCodes.Ldfld, GraphicsState_Stack);
			if (Args[0].Source == SourceType.ILStack)
			{
				gen.Emit(OpCodes.Ldloc_0);
			}
			LoadArgument(gen, 1, IRbldr);
			gen.Emit(OpCodes.Call, LinkedStack_CopyToTop);
		}

		public override void WriteText(IRMethodBuilder IRbldr, StreamWriter tOut)
		{
			IRbldr.TWriteLine(tOut, "CIndex[]" + GetArgString(1, true));
			IRbldr.curInstructionLength = ("CIndex[]" + GetArgString(1, true)).Length;
		}
	}
}
