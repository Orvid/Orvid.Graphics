using System;
using System.IO;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;
using Orvid.Compiler.TrueType.Utils;

namespace Orvid.Compiler.TrueType.HintingVM.Instructions
{
	public class Roll : IRInstruction
	{

		public override IROpCode OpCode
		{
			get { return IROpCode.Roll; }
		}

		public Roll()
		{
		}

		public override void Emit(IRMethodBuilder IRbldr, ILGenerator gen, MethodBuilder mBldr, TypeBuilder tBldr, bool isGlyph)
		{
			LoadArgument(gen, 1, IRbldr);
			gen.Emit(OpCodes.Stloc_0);
			LoadArgument(gen, 2, IRbldr);
			gen.Emit(OpCodes.Stloc_1);
			LoadArgument(gen, 3, IRbldr);
			// this is normally the 'i' variable,
			// but we're using it here so we don't
			// have to create another variable.
			gen.Emit(OpCodes.Stloc_S, (byte)4);

			if (!Destination1IsILStack)
			{
				LoadGraphicsState(gen, IRbldr);
				gen.Emit(OpCodes.Ldfld, GraphicsState_Stack);
				gen.Emit(OpCodes.Dup);
				gen.Emit(OpCodes.Dup);
			}

			gen.Emit(OpCodes.Ldloc_1);
			if (!Destination1IsILStack)
			{
				gen.Emit(OpCodes.Call, LinkedStack_Push);
			}
			gen.Emit(OpCodes.Ldloc_0);
			if (!Destination1IsILStack)
			{
				gen.Emit(OpCodes.Call, LinkedStack_Push);
			}
			gen.Emit(OpCodes.Ldloc_S, (byte)4);
			if (!Destination1IsILStack)
			{
				gen.Emit(OpCodes.Call, LinkedStack_Push);
			}
		}

		public override void WriteText(IRMethodBuilder IRbldr, StreamWriter tOut)
		{
			IRbldr.TWriteLine(tOut, "Roll[]" + GetArgString(3, true));
			IRbldr.curInstructionLength = ("Roll[]" + GetArgString(3, true)).Length;
		}
	}
}
