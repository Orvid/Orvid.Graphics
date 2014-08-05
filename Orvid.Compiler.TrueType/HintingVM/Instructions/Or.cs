using System;
using System.IO;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;
using Orvid.Compiler.TrueType.Utils;

namespace Orvid.Compiler.TrueType.HintingVM.Instructions
{
	public class Or : IRInstruction
	{

		public override IROpCode OpCode
		{
			get { return IROpCode.Or; }
		}

		public Or()
		{
		}

		public override void Emit(IRMethodBuilder IRbldr, ILGenerator gen, MethodBuilder mBldr, TypeBuilder tBldr, bool isGlyph)
		{
			LoadArgument(gen, 1, IRbldr);
			LoadArgument(gen, 2, IRbldr);
			gen.Emit(OpCodes.Or);

			Label bEnd = gen.DefineLabel();
			Label bFalse = gen.DefineLabel();
			gen.Emit(OpCodes.Brfalse_S, bFalse);
			gen.Emit(OpCodes.Ldc_I4_1);
			gen.Emit(OpCodes.Br_S, bEnd);
			gen.MarkLabel(bFalse);
			gen.Emit(OpCodes.Ldc_I4_0);
			gen.MarkLabel(bEnd);

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
			IRbldr.TWriteLine(tOut, "Or[]" + GetArgString(2, true));
			IRbldr.curInstructionLength = ("Or[]" + GetArgString(2, true)).Length;
		}
	}
}
