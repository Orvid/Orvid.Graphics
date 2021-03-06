using System;
using System.IO;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;
using Orvid.Compiler.TrueType.Utils;

namespace Orvid.Compiler.TrueType.HintingVM.Instructions
{
	public class DeltaC2 : IRInstruction
	{

		public override IROpCode OpCode
		{
			get { return IROpCode.DeltaC2; }
		}

		public DeltaC2()
		{
		}

		public override void Emit(IRMethodBuilder IRbldr, ILGenerator gen, MethodBuilder mBldr, TypeBuilder tBldr, bool isGlyph)
		{
#warning Need to do this with the IL Stack.
			LoadGraphicsState(gen, IRbldr);
			gen.Emit(OpCodes.Ldc_I4_1);
			gen.Emit(OpCodes.Call, GraphicsState_AddCvtExceptions);
		}

		public override void WriteText(IRMethodBuilder IRbldr, StreamWriter tOut)
		{
			IRbldr.TWriteLine(tOut, "DeltaC2[]");
			IRbldr.curInstructionLength = 9;
		}
	}
}
