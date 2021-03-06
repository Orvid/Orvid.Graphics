using System;
using System.IO;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;
using Orvid.Compiler.TrueType.Utils;

namespace Orvid.Compiler.TrueType.HintingVM.Instructions
{
	public class SZPS : IRInstruction
	{

		public override IROpCode OpCode
		{
			get { return IROpCode.SZPS; }
		}

		public SZPS()
		{
		}

		public override void Emit(IRMethodBuilder IRbldr, ILGenerator gen, MethodBuilder mBldr, TypeBuilder tBldr, bool isGlyph)
		{
			LoadArgument(gen, 1, IRbldr);
			gen.Emit(OpCodes.Dup);
			gen.Emit(OpCodes.Dup);

			gen.Emit(OpCodes.Ldc_I4_0);
			LoadGraphicsState(gen, IRbldr);
			gen.Emit(OpCodes.Call, GraphicsState_SetZonePointer);

			gen.Emit(OpCodes.Ldc_I4_1);
			LoadGraphicsState(gen, IRbldr);
			gen.Emit(OpCodes.Call, GraphicsState_SetZonePointer);

			gen.Emit(OpCodes.Ldc_I4_2);
			LoadGraphicsState(gen, IRbldr);
			gen.Emit(OpCodes.Call, GraphicsState_SetZonePointer);
		}

		public override void WriteText(IRMethodBuilder IRbldr, StreamWriter tOut)
		{
			IRbldr.TWriteLine(tOut, "SZPS[]" + GetArgString(1, false));
			IRbldr.curInstructionLength = ("SZPS[]" + GetArgString(1, false)).Length;
		}
	}
}
