//#define DebuggingWS
using System;
using System.IO;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;
using Orvid.Compiler.TrueType.Utils;

namespace Orvid.Compiler.TrueType.HintingVM.Instructions
{
	public class WS : IRInstruction
	{

		public override IROpCode OpCode
		{
			get { return IROpCode.WS; }
		}

		public WS()
		{
		}

#if DebuggingWS
		private const int EntryToBreakOn = 9;
#endif

		public override void Emit(IRMethodBuilder IRbldr, ILGenerator gen, MethodBuilder mBldr, TypeBuilder tBldr, bool isGlyph)
		{
			if (Args[0].Source != SourceType.ILStack)
			{
				LoadArgument(gen, 1, IRbldr);
				gen.Emit(OpCodes.Stloc_0);
			}
			else
			{
				gen.Emit(OpCodes.Stloc_0);
			}

			if (Args[1].Source == SourceType.ILStack)
			{
				gen.Emit(OpCodes.Stloc_1);
			}
			LoadGraphicsState(gen, IRbldr);
			gen.Emit(OpCodes.Ldfld, GraphicsState_Storage);
			if (Args[1].Source == SourceType.ILStack)
			{
				gen.Emit(OpCodes.Ldloc_1);
			}
			LoadArgument(gen, 2, IRbldr);
#if DebuggingWS
			gen.Emit(OpCodes.Dup);
			LoadInt(gen, (uint)EntryToBreakOn, false, 4);
			Label fls = gen.DefineLabel();
			gen.Emit(OpCodes.Bne_Un_S, fls);
			EmitException(gen, "Trying to write to storage " + EntryToBreakOn.ToString() + "!");
			gen.MarkLabel(fls);
#endif
			gen.Emit(OpCodes.Ldloc_0);
			gen.Emit(OpCodes.Stelem_I4);
		}

		public override void WriteText(IRMethodBuilder IRbldr, StreamWriter tOut)
		{
			IRbldr.TWriteLine(tOut, "WS[]" + GetArgString(2, false));
			IRbldr.curInstructionLength = ("WS[]" + GetArgString(2, false)).Length;
		}
	}
}
