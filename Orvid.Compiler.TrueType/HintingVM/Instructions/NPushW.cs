using System;
using System.IO;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;
using Orvid.Compiler.TrueType.Utils;

namespace Orvid.Compiler.TrueType.HintingVM.Instructions
{
	public class NPushW : IRInstruction
	{
		public uint[] ValuesToLoad;

		public override IROpCode OpCode
		{
			get { return IROpCode.NPushW; }
		}

		public NPushW(uint[] values)
		{
			this.ValuesToLoad = values;
		}

		public override void Emit(IRMethodBuilder IRbldr, ILGenerator gen, MethodBuilder mBldr, TypeBuilder tBldr, bool isGlyph)
		{
			throw new Exception("This shouldn't be getting called! This instruction should have been replaced by a Synth-LoadInt!"); 
			//LoadGraphicsState(gen, IRbldr);
			//gen.Emit(OpCodes.Ldfld, GraphicsState_Stack);
			//for (uint i = 0; i < ValuesToLoad.Length; i++)
			//{
			//    gen.Emit(OpCodes.Dup);
			//    LoadInt(gen, ValuesToLoad[i], true, 2);
			//    gen.Emit(OpCodes.Call, LinkedStack_Push);
			//}
			//gen.Emit(OpCodes.Pop);
		}

		public override void WriteText(IRMethodBuilder IRbldr, StreamWriter tOut)
		{
			IRbldr.TWriteLine(tOut, "NPushW[] " + ValuesToLoad.Length.ToString());
			IRbldr.curLineNumber++;
			IRbldr.TWriteLine(tOut, "{");
			IRbldr.curLineNumber++;
			IRbldr.curIdent += "".PadLeft(4, ' ');

			for (uint i = 0; i < ValuesToLoad.Length; i++)
			{
				IRbldr.TWriteLine(tOut, ((short)(ushort)ValuesToLoad[i]).ToString() + ",");
				IRbldr.curLineNumber++;
			}

			IRbldr.curIdent = IRbldr.curIdent.Substring(0, IRbldr.curIdent.Length - 4);
			IRbldr.TWriteLine(tOut, "}");
			IRbldr.curInstructionLength = 1;
		}
	}
}
