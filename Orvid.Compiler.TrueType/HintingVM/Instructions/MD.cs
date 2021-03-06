using System;
using System.IO;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;
using Orvid.Compiler.TrueType.Utils;

namespace Orvid.Compiler.TrueType.HintingVM.Instructions
{
	public class MD : IRInstruction
	{
		public bool UseOriginal;

		public override IROpCode OpCode
		{
			get { return IROpCode.MD; }
		}

		public MD(byte b)
		{
			UseOriginal = !IsBitSet(b, 0);
		}

		public override void Emit(IRMethodBuilder IRbldr, ILGenerator gen, MethodBuilder mBldr, TypeBuilder tBldr, bool isGlyph)
		{
			if (Args[1].Source == SourceType.ILStack)
			{
				gen.Emit(OpCodes.Stloc_0);
			}
			LoadArgument(gen, 1, IRbldr);
			if (Args[1].Source == SourceType.ILStack)
			{
				gen.Emit(OpCodes.Ldloc_0);
			}
			LoadArgument(gen, 2, IRbldr);
			LoadBool(gen, UseOriginal);
			LoadGraphicsState(gen, IRbldr);
			gen.Emit(OpCodes.Call, GraphicsState_MeasureDistance);
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
			IRbldr.TWriteLine(tOut, "MD[" + BoolToInt(UseOriginal).ToString() + "]" + GetArgString(2, true));
			IRbldr.curInstructionLength = ("MD[" + BoolToInt(UseOriginal).ToString() + "]" + GetArgString(2, true)).Length;
		}
	}
}
