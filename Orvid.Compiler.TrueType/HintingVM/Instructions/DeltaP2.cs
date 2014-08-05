using System;
using System.IO;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;
using Orvid.Compiler.TrueType.Utils;

namespace Orvid.Compiler.TrueType.HintingVM.Instructions
{
	public class DeltaP2 : IRInstruction
	{

		public override IROpCode OpCode
		{
			get { return IROpCode.DeltaP2; }
		}

		public DeltaP2()
		{
		}

		public override void Emit(IRMethodBuilder IRbldr, ILGenerator gen, MethodBuilder mBldr, TypeBuilder tBldr, bool isGlyph)
		{
#warning Need to do this with the IL Stack.
			LoadGraphicsState(gen, IRbldr);
			gen.Emit(OpCodes.Ldc_I4_1);
			gen.Emit(OpCodes.Call, GraphicsState_AddPointExceptions);
		}

		public override void WriteText(IRMethodBuilder IRbldr, StreamWriter tOut)
		{
			IRbldr.TWriteLine(tOut, "DeltaP2[]");
			IRbldr.curInstructionLength = 9;
		}
	}
}
