using System;
using System.IO;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;
using Orvid.Compiler.TrueType.Utils;

namespace Orvid.Compiler.TrueType.HintingVM.Instructions
{
	public class MPS : IRInstruction
	{

		public override IROpCode OpCode
		{
			get { return IROpCode.MPS; }
		}

		public MPS()
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
			gen.Emit(OpCodes.Ldfld, GraphicsState_PointSize);
			// It comes out of the field as a double.
			// So we truncate it to a U2, then pad
			// that to an I4, ready to be pushed on the
			// stack.
			gen.Emit(OpCodes.Conv_U2);
			gen.Emit(OpCodes.Conv_I4);
			if (!Destination1IsILStack)
			{
				gen.Emit(OpCodes.Call, LinkedStack_Push);
			}
		}

		public override void WriteText(IRMethodBuilder IRbldr, StreamWriter tOut)
		{
			IRbldr.TWriteLine(tOut, "MPS[]" + GetArgString(0, true));
			IRbldr.curInstructionLength = ("MPS[]" + GetArgString(0, true)).Length;
		}
	}
}
