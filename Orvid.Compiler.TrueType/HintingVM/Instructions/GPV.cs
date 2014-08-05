using System;
using System.IO;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;
using Orvid.Compiler.TrueType.Utils;

namespace Orvid.Compiler.TrueType.HintingVM.Instructions
{
	public class GPV : IRInstruction
	{

		public override IROpCode OpCode
		{
			get { return IROpCode.GPV; }
		}

		public GPV()
		{
		}

		public override void Emit(IRMethodBuilder IRbldr, ILGenerator gen, MethodBuilder mBldr, TypeBuilder tBldr, bool isGlyph)
		{
			if (!Destination1IsILStack)
			{
				LoadGraphicsState(gen, IRbldr);
				gen.Emit(OpCodes.Ldfld, GraphicsState_Stack);
				gen.Emit(OpCodes.Dup);
			}

			// Push the X component
			LoadGraphicsState(gen, IRbldr);
			gen.Emit(OpCodes.Ldfld, GraphicsState_Projection_Vector);
			gen.Emit(OpCodes.Ldfld, VecF2Dot14_X);
			gen.Emit(OpCodes.Call, F2Dot14_AsLiteral);
			if (!Destination1IsILStack)
			{
				gen.Emit(OpCodes.Call, LinkedStack_Push);
			}

			// Push the Y component
			LoadGraphicsState(gen, IRbldr);
			gen.Emit(OpCodes.Ldfld, GraphicsState_Projection_Vector);
			gen.Emit(OpCodes.Ldfld, VecF2Dot14_Y);
			gen.Emit(OpCodes.Call, F2Dot14_AsLiteral);
			if (!Destination2IsILStack)
			{
				gen.Emit(OpCodes.Call, LinkedStack_Push);
			}
		}

		public override void WriteText(IRMethodBuilder IRbldr, StreamWriter tOut)
		{
			IRbldr.TWriteLine(tOut, "GPV[]" + GetArgString(0, true));
			IRbldr.curInstructionLength = ("GPV[]" + GetArgString(0, true)).Length;
		}
	}
}
