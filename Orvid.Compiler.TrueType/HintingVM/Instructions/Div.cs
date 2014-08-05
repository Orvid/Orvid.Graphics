using System;
using System.IO;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;
using Orvid.Compiler.TrueType.Utils;

namespace Orvid.Compiler.TrueType.HintingVM.Instructions
{
	public class Div : IRInstruction
	{

		public override IROpCode OpCode
		{
			get { return IROpCode.Div; }
		}

		public Div()
		{
		}

		public override void Emit(IRMethodBuilder IRbldr, ILGenerator gen, MethodBuilder mBldr, TypeBuilder tBldr, bool isGlyph)
		{
			LoadArgument(gen, 1, IRbldr);
			gen.Emit(OpCodes.Stloc_2);

			LoadArgument(gen, 2, IRbldr);
			gen.Emit(OpCodes.Ldloc_2);

			gen.Emit(OpCodes.Call, F26Dot6_Divide);
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

		public override void WriteText(IRMethodBuilder IRbldr, StreamWriter tOut)
		{
			IRbldr.TWriteLine(tOut, "Div[]" + GetArgString(2, true));
			IRbldr.curInstructionLength = ("Div[]" + GetArgString(2, true)).Length;
		}

		public override bool ExpectsArg1F26Dot6 { get { return true; } }
		public override bool ExpectsArg2F26Dot6 { get { return true; } }
	}
}
