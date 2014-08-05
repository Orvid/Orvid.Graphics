using System;
using System.IO;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;
using Orvid.Compiler.TrueType.Utils;

namespace Orvid.Compiler.TrueType.HintingVM.Instructions
{
	public class ConsoleWriteLine : IRInstruction
	{
		public string Message;

		public override IROpCode OpCode
		{
			get { return IROpCode.ConsoleWriteLine; }
		}

		public ConsoleWriteLine(string msg)
		{
			this.Message = msg;
		}

		public override void Emit(IRMethodBuilder IRbldr, ILGenerator gen, MethodBuilder mBldr, TypeBuilder tBldr, bool isGlyph)
		{
			gen.Emit(OpCodes.Ldstr, Message);
			gen.Emit(OpCodes.Call, typeof(Console).GetMethod("WriteLine", new Type[] { typeof(string) }));
		}

		public override void WriteText(IRMethodBuilder IRbldr, StreamWriter tOut)
		{
			IRbldr.TWriteLine(tOut, this.ToString());
			IRbldr.curInstructionLength = (this.ToString()).Length;
		}

		public override string ToString()
		{
			return "ConsoleWriteLine(\"" + Message + "\")";
		}
	}
}
