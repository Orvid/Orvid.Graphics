using System;
using System.IO;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;
using Orvid.Compiler.TrueType.Utils;

namespace Orvid.Compiler.TrueType.HintingVM.Instructions
{
	public class LTEQ : IRInstruction
	{

		public override IROpCode OpCode
		{
			get { return IROpCode.LTEQ; }
		}

		public LTEQ()
		{
		}

		public override void Emit(IRMethodBuilder IRbldr, ILGenerator gen, MethodBuilder mBldr, TypeBuilder tBldr, bool isGlyph)
		{
			LoadArgument(gen, 1, IRbldr);
			gen.Emit(OpCodes.Stloc_0);
			LoadArgument(gen, 2, IRbldr);
			gen.Emit(OpCodes.Dup);
			gen.Emit(OpCodes.Stloc_1);
			gen.Emit(OpCodes.Ldloc_0);
			gen.Emit(OpCodes.Clt);
			gen.Emit(OpCodes.Ldloc_1);
			gen.Emit(OpCodes.Ldloc_0);
			gen.Emit(OpCodes.Ceq);
			gen.Emit(OpCodes.Or);

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
			IRbldr.TWriteLine(tOut, "LTEQ[]" + GetArgString(2, true));
			IRbldr.curInstructionLength = ("LTEQ[]" + GetArgString(2, true)).Length;
		}
	}
}
