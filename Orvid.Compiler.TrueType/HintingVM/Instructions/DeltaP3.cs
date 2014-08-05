using System;
using System.IO;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;
using Orvid.Compiler.TrueType.Utils;

namespace Orvid.Compiler.TrueType.HintingVM.Instructions
{
	public class DeltaP3 : IRInstruction
	{

		public override IROpCode OpCode
		{
			get { return IROpCode.DeltaP3; }
		}

		public DeltaP3()
		{
		}

		public override void Emit(IRMethodBuilder IRbldr, ILGenerator gen, MethodBuilder mBldr, TypeBuilder tBldr, bool isGlyph)
		{
#warning Need to do this with the IL Stack.
			LoadGraphicsState(gen, IRbldr);
			gen.Emit(OpCodes.Ldc_I4_2);
			gen.Emit(OpCodes.Call, GraphicsState_AddPointExceptions);
		}

		public override void WriteText(IRMethodBuilder IRbldr, StreamWriter tOut)
		{
			IRbldr.TWriteLine(tOut, "DeltaP3[]");
			IRbldr.curInstructionLength = 9;
		}
	}
}
