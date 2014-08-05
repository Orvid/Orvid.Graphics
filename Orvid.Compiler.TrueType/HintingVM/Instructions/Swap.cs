using System;
using System.IO;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;
using Orvid.Compiler.TrueType.Utils;

namespace Orvid.Compiler.TrueType.HintingVM.Instructions
{
	public class Swap : IRInstruction
	{

		public override IROpCode OpCode
		{
			get { return IROpCode.Swap; }
		}

		public Swap()
		{
		}

		public override void Emit(IRMethodBuilder IRbldr, ILGenerator gen, MethodBuilder mBldr, TypeBuilder tBldr, bool isGlyph)
		{
			if (Args[0].Source == SourceType.ILStack && Args[1].Source == SourceType.SyntheticStack)
			{
				LoadArgument(gen, 2, IRbldr);
				if (!Destination1IsILStack)
				{
					gen.Emit(OpCodes.Stloc_1);
					gen.Emit(OpCodes.Stloc_0);
					LoadGraphicsState(gen, IRbldr);
					gen.Emit(OpCodes.Ldfld, GraphicsState_Stack);
					gen.Emit(OpCodes.Dup);
					gen.Emit(OpCodes.Ldloc_0);
					gen.Emit(OpCodes.Call, LinkedStack_Push);
					gen.Emit(OpCodes.Ldloc_1);
					gen.Emit(OpCodes.Call, LinkedStack_Push);
				}
			}
			else if ((Args[0].Source == SourceType.Constant && Args[1].Source != SourceType.ILStack) || (Args[1].Source == SourceType.Constant && Args[0].Source != SourceType.ILStack))
			{
				LoadArgument(gen, 1, IRbldr);
				LoadArgument(gen, 2, IRbldr);
				if (!Destination1IsILStack)
				{
					LoadGraphicsState(gen, IRbldr);
					gen.Emit(OpCodes.Ldfld, GraphicsState_Stack);
					gen.Emit(OpCodes.Dup);
					gen.Emit(OpCodes.Ldloc_0);
					gen.Emit(OpCodes.Call, LinkedStack_Push);
					gen.Emit(OpCodes.Ldloc_1);
					gen.Emit(OpCodes.Call, LinkedStack_Push);
				}
			}
			else
			{
				LoadArgument(gen, 1, IRbldr);
				gen.Emit(OpCodes.Stloc_0);
				LoadArgument(gen, 2, IRbldr);
				gen.Emit(OpCodes.Stloc_1);
				if (Destination1IsILStack)
				{
					gen.Emit(OpCodes.Ldloc_0);
					gen.Emit(OpCodes.Ldloc_1);
				}
				else
				{
					LoadGraphicsState(gen, IRbldr);
					gen.Emit(OpCodes.Ldfld, GraphicsState_Stack);
					gen.Emit(OpCodes.Dup);
					gen.Emit(OpCodes.Ldloc_0);
					gen.Emit(OpCodes.Call, LinkedStack_Push);
					gen.Emit(OpCodes.Ldloc_1);
					gen.Emit(OpCodes.Call, LinkedStack_Push);
				}
			}
		}

		public override void WriteText(IRMethodBuilder IRbldr, StreamWriter tOut)
		{
			IRbldr.TWriteLine(tOut, "Swap[]" + GetArgString(2, true));
			IRbldr.curInstructionLength = ("Swap[]" + GetArgString(2, true)).Length;
		}
	}
}
