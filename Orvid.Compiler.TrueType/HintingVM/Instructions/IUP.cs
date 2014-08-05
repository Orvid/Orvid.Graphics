using System;
using System.IO;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;
using Orvid.Compiler.TrueType.Utils;

namespace Orvid.Compiler.TrueType.HintingVM.Instructions
{
	public class IUP : IRInstruction
	{
		public bool InterpolateX;

		public override IROpCode OpCode
		{
			get { return IROpCode.IUP; }
		}

		public IUP(byte b)
		{
			InterpolateX = IsBitSet(b, 0);
		}

		public override void Emit(IRMethodBuilder IRbldr, ILGenerator gen, MethodBuilder mBldr, TypeBuilder tBldr, bool isGlyph)
		{
			LoadBool(gen, InterpolateX);
			LoadGraphicsState(gen, IRbldr);
			gen.Emit(OpCodes.Call, GraphicsState_InterpolateUntouchedPoints);
		}

		public override void WriteText(IRMethodBuilder IRbldr, StreamWriter tOut)
		{
			IRbldr.TWriteLine(tOut, "IUP[" + BoolToInt(InterpolateX).ToString() + "]");
			IRbldr.curInstructionLength = 6;
		}
	}
}
