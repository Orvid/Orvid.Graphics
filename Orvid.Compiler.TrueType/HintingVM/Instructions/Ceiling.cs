using System;
using System.IO;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;
using Orvid.Compiler.TrueType.Utils;

namespace Orvid.Compiler.TrueType.HintingVM.Instructions
{
	public class Ceiling : IRInstruction
	{

		public override IROpCode OpCode
		{
			get { return IROpCode.Ceiling; }
		}

		public Ceiling()
		{
		}

		public override void Emit(IRMethodBuilder IRbldr, ILGenerator gen, MethodBuilder mBldr, TypeBuilder tBldr, bool isGlyph)
		{
			LoadArgument(gen, 1, IRbldr);
			gen.Emit(OpCodes.Call, F26Dot6_Ceiling);
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
			IRbldr.TWriteLine(tOut, "Ceiling[]" + GetArgString(1, true));
			IRbldr.curInstructionLength = ("Ceiling[]" + GetArgString(1, true)).Length;
		}

		public override bool ExpectsArg1F26Dot6 { get { return true; } }
	}
}
