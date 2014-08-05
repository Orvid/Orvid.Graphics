using System;
using System.IO;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;
using Orvid.Compiler.TrueType.Utils;

namespace Orvid.Compiler.TrueType.HintingVM.Instructions
{
	public class Min : IRInstruction
	{

		public override IROpCode OpCode
		{
			get { return IROpCode.Min; }
		}

		public Min()
		{
		}

		public override void Emit(IRMethodBuilder IRbldr, ILGenerator gen, MethodBuilder mBldr, TypeBuilder tBldr, bool isGlyph)
		{
			LoadArgument(gen, 1, IRbldr);
			gen.Emit(OpCodes.Dup);
			gen.Emit(OpCodes.Stloc_0);

			LoadArgument(gen, 2, IRbldr);
			gen.Emit(OpCodes.Dup);
			gen.Emit(OpCodes.Stloc_1);

			Label vAGreater = gen.DefineLabel();
			Label vBGreater = gen.DefineLabel();
			Label Final = gen.DefineLabel();
			gen.Emit(OpCodes.Bgt_S, vAGreater);
			gen.Emit(OpCodes.Br_S, vBGreater);

			gen.MarkLabel(vAGreater);
			gen.Emit(OpCodes.Ldloc_0);
			gen.Emit(OpCodes.Br_S, Final);

			gen.MarkLabel(vBGreater);
			gen.Emit(OpCodes.Ldloc_1);
			gen.Emit(OpCodes.Br_S, Final);

			gen.MarkLabel(Final);

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
			IRbldr.TWriteLine(tOut, "Min[]" + GetArgString(2, true));
			IRbldr.curInstructionLength = ("Min[]" + GetArgString(2, true)).Length;
		}
	}
}
