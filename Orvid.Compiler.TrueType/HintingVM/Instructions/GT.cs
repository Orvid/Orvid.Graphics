using System;
using System.IO;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;
using Orvid.Compiler.TrueType.Utils;

namespace Orvid.Compiler.TrueType.HintingVM.Instructions
{
	public class GT : IRInstruction
	{

		public override IROpCode OpCode
		{
			get { return IROpCode.GT; }
		}

		public GT()
		{
		}

		public override void Emit(IRMethodBuilder IRbldr, ILGenerator gen, MethodBuilder mBldr, TypeBuilder tBldr, bool isGlyph)
		{
			LoadArgument(gen, 1, IRbldr);
			gen.Emit(OpCodes.Stloc_0);
			LoadArgument(gen, 2, IRbldr);
			gen.Emit(OpCodes.Ldloc_0);
			gen.Emit(OpCodes.Cgt);

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
			IRbldr.TWriteLine(tOut, "GT[]" + GetArgString(2, true));
			IRbldr.curInstructionLength = ("GT[]" + GetArgString(2, true)).Length;
		}
	}
}
