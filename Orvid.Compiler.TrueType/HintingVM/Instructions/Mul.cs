using System;
using System.IO;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;
using Orvid.Compiler.TrueType.Utils;
using Orvid.TrueType;

namespace Orvid.Compiler.TrueType.HintingVM.Instructions
{
	public class Mul : IRInstruction
	{

		public override IROpCode OpCode
		{
			get { return IROpCode.Mul; }
		}

		public Mul()
		{
		}

		public override void Emit(IRMethodBuilder IRbldr, ILGenerator gen, MethodBuilder mBldr, TypeBuilder tBldr, bool isGlyph)
		{
			if (Args[0].Source == SourceType.Constant && Args[1].Source == SourceType.Constant)
			{
				// Mul is commonly used for loading
				// values larger than a short to the
				// stack, say for example, selectors
				// for GetInfo instructions.
				LoadInt(gen, F26Dot6.AsLiteral(F26Dot6.FromLiteral(Args[0].Constant) * F26Dot6.FromLiteral(Args[1].Constant)));
				if (Destination1IsF26Dot6)
				{
					gen.Emit(OpCodes.Call, F26Dot6_FromLiteral);
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

				gen.Emit(OpCodes.Call, F26Dot6_Multiply);
				if (!Destination1IsF26Dot6)
				{
					gen.Emit(OpCodes.Call, F26Dot6_AsLiteral);
				}
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

		public override void WriteText(IRMethodBuilder IRbldr, StreamWriter tOut)
		{
			IRbldr.TWriteLine(tOut, "Mul[]" + GetArgString(2, true));
			IRbldr.curInstructionLength = ("Mul[]" + GetArgString(2, true)).Length;
		}

		public override bool ExpectsArg1F26Dot6 { get { return true; } }
		public override bool ExpectsArg2F26Dot6 { get { return true; } }
	}
}
