using System;
using System.IO;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;
using Orvid.Compiler.TrueType.Utils;

namespace Orvid.Compiler.TrueType.HintingVM.Instructions
{
	public class LoneDeltaC : IRInstruction
	{
		public int Magnitude;

		public override IROpCode OpCode
		{
			get { return IROpCode.LoneDeltaC; }
		}

		public LoneDeltaC(int magnitude)
		{
			this.Magnitude = magnitude;
		}

		public override void Emit(IRMethodBuilder IRbldr, ILGenerator gen, MethodBuilder mBldr, TypeBuilder tBldr, bool isGlyph)
		{
			if (Args[1].Source == SourceType.ILStack && Args[0].Source != SourceType.ILStack)
			{
				gen.Emit(OpCodes.Stloc_0);
			}
			LoadArgument(gen, 1, IRbldr);
			if (Args[1].Source == SourceType.ILStack && Args[0].Source != SourceType.ILStack)
			{
				gen.Emit(OpCodes.Ldloc_0);
			}
			LoadArgument(gen, 2, IRbldr);
			LoadInt(gen, Magnitude);
			LoadGraphicsState(gen, IRbldr);
			gen.Emit(OpCodes.Call, GraphicsState_AddLoneCvtException);
		}

		public override void WriteText(IRMethodBuilder IRbldr, StreamWriter tOut)
		{
			IRbldr.TWriteLine(tOut, "Synth-LoneDeltaC" + GetArgString(2, false));
			IRbldr.curInstructionLength = ("Synth-LoneDeltaC" + GetArgString(2, false)).Length;
		}
	}
}
