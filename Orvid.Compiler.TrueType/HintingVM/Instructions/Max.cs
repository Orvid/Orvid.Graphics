using System;
using System.IO;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;
using Orvid.Compiler.TrueType.Utils;

namespace Orvid.Compiler.TrueType.HintingVM.Instructions
{
	public class Max : IRInstruction
	{

		public override IROpCode OpCode
		{
			get { return IROpCode.Max; }
		}

		public Max()
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

			Label vALess = gen.DefineLabel();
			Label vBLess = gen.DefineLabel();
			Label Final = gen.DefineLabel();
			gen.Emit(OpCodes.Blt_S, vALess);
			gen.Emit(OpCodes.Br_S, vBLess);

			gen.MarkLabel(vALess);
			gen.Emit(OpCodes.Ldloc_0);
			gen.Emit(OpCodes.Br_S, Final);

			gen.MarkLabel(vBLess);
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
			IRbldr.TWriteLine(tOut, "Max[]" + GetArgString(2, true));
			IRbldr.curInstructionLength = ("Max[]" + GetArgString(2, true)).Length;
		}
	}
}
