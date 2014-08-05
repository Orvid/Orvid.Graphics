using System;
using System.IO;
using System.Text;
using Orvid.TrueType;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;
using Orvid.Compiler.TrueType.Utils;

namespace Orvid.Compiler.TrueType.HintingVM.Instructions
{
	public class Round : IRInstruction
	{
		public DistanceType MeasuredDistanceType;

		public override IROpCode OpCode
		{
			get { return IROpCode.Round; }
		}

		public Round(byte b)
		{
			MeasuredDistanceType = (DistanceType)(b & 0x03);
		}

		public override void Emit(IRMethodBuilder IRbldr, ILGenerator gen, MethodBuilder mBldr, TypeBuilder tBldr, bool isGlyph)
		{
			if (Args[0].Source == SourceType.ILStack)
			{
				if (Args[0].IsF26Dot6)
				{
					gen.Emit(OpCodes.Stloc_2);
				}
				else
				{
					gen.Emit(OpCodes.Stloc_0);
				}
			}
			LoadGraphicsState(gen, IRbldr);
			if (Args[0].Source == SourceType.ILStack)
			{
				if (Args[0].IsF26Dot6)
				{
					gen.Emit(OpCodes.Ldloc_2);
				}
				else
				{
					gen.Emit(OpCodes.Ldloc_0);
				}
			}
			LoadArgument(gen, 1, IRbldr);
			LoadInt(gen, (int)(MeasuredDistanceType));
			gen.Emit(OpCodes.Call, GraphicsState_Round);
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
			IRbldr.TWriteLine(tOut, "Round[" + ((int)MeasuredDistanceType).ToString() + "]" + GetArgString(1, true));
			IRbldr.curInstructionLength = ("Round[" + ((int)MeasuredDistanceType).ToString() + "]" + GetArgString(1, true)).Length;
		}

		public override bool ExpectsArg1F26Dot6 { get { return true; } }
	}
}
