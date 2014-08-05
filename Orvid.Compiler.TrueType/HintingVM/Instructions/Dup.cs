using System;
using System.IO;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;
using Orvid.Compiler.TrueType.Utils;

namespace Orvid.Compiler.TrueType.HintingVM.Instructions
{
	public class Dup : IRInstruction
	{

		public override IROpCode OpCode
		{
			get { return IROpCode.Dup; }
		}

		public Dup()
		{
		}

		public override void Emit(IRMethodBuilder IRbldr, ILGenerator gen, MethodBuilder mBldr, TypeBuilder tBldr, bool isGlyph)
		{
			if (Args[0].Source == SourceType.ILStack)
			{
				if (Destination1IsILStack && Destination2IsILStack)
				{
					gen.Emit(OpCodes.Dup);
				}
				else if (!Destination1IsILStack && !Destination2IsILStack)
				{
					gen.Emit(OpCodes.Stloc_0);
					LoadGraphicsState(gen, IRbldr);
					gen.Emit(OpCodes.Ldfld, GraphicsState_Stack);
					gen.Emit(OpCodes.Dup);
					gen.Emit(OpCodes.Ldloc_0);
					gen.Emit(OpCodes.Call, LinkedStack_Push);
					gen.Emit(OpCodes.Ldloc_0);
					gen.Emit(OpCodes.Call, LinkedStack_Push);
				}
				else // Only 1 of the 2 is pushing to the IL stack
				{
					gen.Emit(OpCodes.Stloc_0);
					LoadGraphicsState(gen, IRbldr);
					gen.Emit(OpCodes.Ldfld, GraphicsState_Stack);
					gen.Emit(OpCodes.Ldloc_0);
					gen.Emit(OpCodes.Call, LinkedStack_Push);
					gen.Emit(OpCodes.Ldloc_0);
				}
			}
			else
			{
				if (!Destination1IsILStack)
				{
					LoadGraphicsState(gen, IRbldr);
					gen.Emit(OpCodes.Ldfld, GraphicsState_Stack);
				}
				LoadArgument(gen, 1, IRbldr);
				gen.Emit(OpCodes.Dup);
				gen.Emit(OpCodes.Stloc_0);
				if (!Destination1IsILStack)
				{
					gen.Emit(OpCodes.Call, LinkedStack_Push);
				}
				if (!Destination2IsILStack)
				{
					LoadGraphicsState(gen, IRbldr);
					gen.Emit(OpCodes.Ldfld, GraphicsState_Stack);
				}
				gen.Emit(OpCodes.Ldloc_0);
				if (!Destination2IsILStack)
				{
					gen.Emit(OpCodes.Call, LinkedStack_Push);
				}
			}
		}

		public override void WriteText(IRMethodBuilder IRbldr, StreamWriter tOut)
		{
			IRbldr.TWriteLine(tOut, "Dup[]" + GetArgString(1, true, 2));
			IRbldr.curInstructionLength = ("Dup[]" + GetArgString(1, true, 2)).Length;
		}
	}
}
