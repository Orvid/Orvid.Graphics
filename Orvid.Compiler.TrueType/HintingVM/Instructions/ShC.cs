using System;
using System.IO;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;
using Orvid.Compiler.TrueType.Utils;

namespace Orvid.Compiler.TrueType.HintingVM.Instructions
{
	public class ShC : IRInstruction
	{
		public bool UseZp0;

		public override IROpCode OpCode
		{
			get { return IROpCode.ShC; }
		}

		public ShC(byte b)
		{
			UseZp0 = IsBitSet(b, 0);
		}

		public override void Emit(IRMethodBuilder IRbldr, ILGenerator gen, MethodBuilder mBldr, TypeBuilder tBldr, bool isGlyph)
		{
			LoadArgument(gen, 1, IRbldr);
			LoadBool(gen, UseZp0);
			LoadGraphicsState(gen, IRbldr);
			gen.Emit(OpCodes.Call, GraphicsState_ShiftContour);
		}

		public override void WriteText(IRMethodBuilder IRbldr, StreamWriter tOut)
		{
			IRbldr.TWriteLine(tOut, "ShC[" + BoolToInt(UseZp0).ToString() + "]" + GetArgString(1, false));
			IRbldr.curInstructionLength = ("ShC[" + BoolToInt(UseZp0).ToString() + "]" + GetArgString(1, false)).Length;
		}
	}
}
