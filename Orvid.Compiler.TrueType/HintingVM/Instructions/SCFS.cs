using System;
using System.IO;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;
using Orvid.Compiler.TrueType.Utils;

namespace Orvid.Compiler.TrueType.HintingVM.Instructions
{
	public class SCFS : IRInstruction
	{

		public override IROpCode OpCode
		{
			get { return IROpCode.SCFS; }
		}

		public SCFS()
		{
		}

		public override void Emit(IRMethodBuilder IRbldr, ILGenerator gen, MethodBuilder mBldr, TypeBuilder tBldr, bool isGlyph)
		{
			LoadGraphicsState(gen, IRbldr);
			LoadArgument(gen, 1, IRbldr);
			gen.Emit(OpCodes.Call, F26Dot6_FromLiteral);

			LoadArgument(gen, 2, IRbldr);
			gen.Emit(OpCodes.Call, GraphicsState_SetCoords);
		}

		public override void WriteText(IRMethodBuilder IRbldr, StreamWriter tOut)
		{
			IRbldr.TWriteLine(tOut, "SCFS[]");
			IRbldr.curInstructionLength = 6;
		}
	}
}
