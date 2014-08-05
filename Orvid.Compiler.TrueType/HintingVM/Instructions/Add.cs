using System;
using System.IO;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;
using Orvid.Compiler.TrueType.Utils;

namespace Orvid.Compiler.TrueType.HintingVM.Instructions
{
	public class Add : IRInstruction
	{
		public override IROpCode OpCode
		{
			get { return IROpCode.Add; }
		}

		public Add()
		{
		}

		public override void Emit(IRMethodBuilder IRbldr, ILGenerator gen, MethodBuilder mBldr, TypeBuilder tBldr, bool isGlyph)
		{
			if ((Args[0].Source == SourceType.Constant && !Args[1].IsF26Dot6)
			 || (Args[1].Source == SourceType.Constant && !Args[0].IsF26Dot6)
				)
			{
				LoadArgument(gen, 1, IRbldr, false);
				LoadArgument(gen, 2, IRbldr, false);
				gen.Emit(OpCodes.Add);
				if (Destination1IsF26Dot6)
				{
					gen.Emit(OpCodes.Call, F26Dot6_FromLiteral);
				}
				else if (!Destination1IsILStack)
				{
					gen.Emit(OpCodes.Stloc_0);
					LoadGraphicsState(gen, IRbldr);
					gen.Emit(OpCodes.Ldfld, GraphicsState_Stack);
					gen.Emit(OpCodes.Ldloc_0);
					gen.Emit(OpCodes.Call, LinkedStack_Push);
				}
			}
			else
			{
				LoadArgument(gen, 1, IRbldr);
				if (Args[1].Source == SourceType.ILStack)
				{
					gen.Emit(OpCodes.Stloc_2);
				}

				LoadArgument(gen, 2, IRbldr);
				if (Args[1].Source == SourceType.ILStack)
				{
					gen.Emit(OpCodes.Ldloc_2);
				}

				gen.Emit(OpCodes.Call, F26Dot6_Add);
				if (!Destination1IsF26Dot6)
				{
					gen.Emit(OpCodes.Call, F26Dot6_AsLiteral);
				}

				if (!Destination1IsILStack)
				{
					gen.Emit(OpCodes.Stloc_0);
					LoadGraphicsState(gen, IRbldr);
					gen.Emit(OpCodes.Ldfld, GraphicsState_Stack);
					gen.Emit(OpCodes.Ldloc_0);
					gen.Emit(OpCodes.Call, LinkedStack_Push);
				}
			}
		}

		public override void WriteText(IRMethodBuilder IRbldr, StreamWriter tOut)
		{
			IRbldr.TWriteLine(tOut, "Add[]" + GetArgString(2, true));
			IRbldr.curInstructionLength = ("Add[]" + GetArgString(2, true)).Length;
		}

		public override bool ExpectsArg1F26Dot6 { get { return true; } }
		public override bool ExpectsArg2F26Dot6 { get { return true; } }
	}
}
