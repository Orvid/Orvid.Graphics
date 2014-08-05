using System;
using System.IO;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;
using Orvid.Compiler.TrueType.Utils;

namespace Orvid.Compiler.TrueType.HintingVM.Instructions
{
	public class NEQ : IRInstruction
	{

		public override IROpCode OpCode
		{
			get { return IROpCode.NEQ; }
		}

		public NEQ()
		{
		}

		public override void Emit(IRMethodBuilder IRbldr, ILGenerator gen, MethodBuilder mBldr, TypeBuilder tBldr, bool isGlyph)
		{
			LoadArgument(gen, 1, IRbldr);
			LoadArgument(gen, 2, IRbldr);
			gen.Emit(OpCodes.Ceq);
			gen.Emit(OpCodes.Not);

			if (!Destination1IsILStack)
			{
				gen.Emit(OpCodes.Stloc_0);
				LoadGraphicsState(gen, IRbldr);
				gen.Emit(OpCodes.Ldfld, GraphicsState_Stack);
				gen.Emit(OpCodes.Ldloc_0);
				gen.Emit(OpCodes.Call, LinkedStack_Push);
			}
		}

		public override void WriteText(IRMethodBuilder IRbldr, StreamWriter tOut)
		{
			IRbldr.TWriteLine(tOut, "NEQ[]" + GetArgString(2, true));
			IRbldr.curInstructionLength = ("NEQ[]" + GetArgString(2, true)).Length;
		}
	}
}
