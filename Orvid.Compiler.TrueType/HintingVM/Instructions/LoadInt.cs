using System;
using System.IO;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;
using Orvid.Compiler.TrueType.Utils;

namespace Orvid.Compiler.TrueType.HintingVM.Instructions
{
	public class LoadInt : IRInstruction
	{
		public int[] ValuesToLoad;

		public override IROpCode OpCode
		{
			get { return IROpCode.LoadInt; }
		}

		public LoadInt(int[] values)
		{
			this.ValuesToLoad = values;
		}

		public override void Emit(IRMethodBuilder IRbldr, ILGenerator gen, MethodBuilder mBldr, TypeBuilder tBldr, bool isGlyph)
		{
			if (!Destination1IsILStack)
			{
				LoadGraphicsState(gen, IRbldr);
				gen.Emit(OpCodes.Ldfld, GraphicsState_Stack);
				for (uint i = 0; i < ValuesToLoad.Length; i++)
				{
					gen.Emit(OpCodes.Dup);
					LoadInt(gen, ValuesToLoad[i]);
					gen.Emit(OpCodes.Call, LinkedStack_Push);
				}
				gen.Emit(OpCodes.Pop);
			}
			else
			{
				for (uint i = 0; i < ValuesToLoad.Length; i++)
				{
					LoadInt(gen, ValuesToLoad[i]);
				}
			}
		}

		public override void WriteText(IRMethodBuilder IRbldr, StreamWriter tOut)
		{
			IRbldr.TWriteLine(tOut, "Synth-LoadInt(" + ValuesToLoad.Length.ToString() + ")->" + ((Destination1IsILStack) ? "IL" : "Synth"));
			IRbldr.curLineNumber++;
			IRbldr.TWriteLine(tOut, "{");
			IRbldr.curLineNumber++;
			IRbldr.curIdent += "".PadLeft(4, ' ');

			for (uint i = 0; i < ValuesToLoad.Length; i++)
			{
				IRbldr.TWriteLine(tOut, ValuesToLoad[i].ToString() + ",");
				IRbldr.curLineNumber++;
			}

			IRbldr.curIdent = IRbldr.curIdent.Substring(0, IRbldr.curIdent.Length - 4);
			IRbldr.TWriteLine(tOut, "}");
			IRbldr.curInstructionLength = 1;
		}
	}
}
