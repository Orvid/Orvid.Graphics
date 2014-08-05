using System;
using System.IO;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;
using Orvid.Compiler.TrueType.Utils;

namespace Orvid.Compiler.TrueType.HintingVM.Instructions
{
	public class Depth : IRInstruction
	{

		public override IROpCode OpCode
		{
			get { return IROpCode.Depth; }
		}

		public Depth()
		{
		}

		public override void Emit(IRMethodBuilder IRbldr, ILGenerator gen, MethodBuilder mBldr, TypeBuilder tBldr, bool isGlyph)
		{
			LoadGraphicsState(gen, IRbldr);
			gen.Emit(OpCodes.Ldfld, GraphicsState_Stack);
			if (!Destination1IsILStack)
			{
				gen.Emit(OpCodes.Dup);
			}
			gen.Emit(OpCodes.Ldfld, LinkedStack_Depth);
			if (!Destination1IsILStack)
			{
				gen.Emit(OpCodes.Call, LinkedStack_Push);
			}
		}

		public override void WriteText(IRMethodBuilder IRbldr, StreamWriter tOut)
		{
			IRbldr.TWriteLine(tOut, "Depth[]");
			IRbldr.curInstructionLength = 7;
		}
	}
}
