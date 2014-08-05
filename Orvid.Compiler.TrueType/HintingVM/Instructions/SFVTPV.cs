using System;
using System.IO;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;
using Orvid.Compiler.TrueType.Utils;

namespace Orvid.Compiler.TrueType.HintingVM.Instructions
{
	public class SFVTPV : IRInstruction
	{

		public override IROpCode OpCode
		{
			get { return IROpCode.SFVTPV; }
		}

		public SFVTPV()
		{
		}

		public override void Emit(IRMethodBuilder IRbldr, ILGenerator gen, MethodBuilder mBldr, TypeBuilder tBldr, bool isGlyph)
		{
			LoadGraphicsState(gen, IRbldr);
			gen.Emit(OpCodes.Dup);
			gen.Emit(OpCodes.Ldfld, GraphicsState_Projection_Vector);
			gen.Emit(OpCodes.Stfld, GraphicsState_Freedom_Vector);
			LoadGraphicsState(gen, IRbldr);
			gen.Emit(OpCodes.Call, GraphicsState_RecalcProjFreedomDotProduct);
		}

		public override void WriteText(IRMethodBuilder IRbldr, StreamWriter tOut)
		{
			IRbldr.TWriteLine(tOut, "SFVTPV[]");
			IRbldr.curInstructionLength = 8;
		}
	}
}
