using System;
using System.IO;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;
using Orvid.Compiler.TrueType.Utils;
using Orvid.TrueType;

namespace Orvid.Compiler.TrueType.HintingVM.Instructions
{
	public class LoopCall : IRInstruction
	{

		public override IROpCode OpCode
		{
			get { return IROpCode.LoopCall; }
		}

		public LoopCall()
		{
		}

		public override void Emit(IRMethodBuilder IRbldr, ILGenerator gen, MethodBuilder mBldr, TypeBuilder tBldr, bool isGlyph)
		{
			// Loop Init
			LoadArgument(gen, 1, IRbldr);
			gen.Emit(OpCodes.Stloc_S, (byte)5);

			LoadGraphicsState(gen, IRbldr);
			gen.Emit(OpCodes.Ldfld, GraphicsState_Functions);
			LoadGraphicsState(gen, IRbldr);
			gen.Emit(OpCodes.Ldfld, GraphicsState_Stack);
			gen.Emit(OpCodes.Call, LinkedStack_Pop);
			gen.Emit(OpCodes.Callvirt, typeof(Dictionary<int, HintingMethod>).GetMethod("get_Item"));
			gen.Emit(OpCodes.Stloc_S, (byte)6);

			gen.Emit(OpCodes.Ldc_I4_0);
			gen.Emit(OpCodes.Stloc_S, (byte)4);
			Label loopCondition = gen.DefineLabel();
			gen.Emit(OpCodes.Br_S, loopCondition);
			Label loopBody = gen.DefineLabel();

			// The loop body.
			gen.MarkLabel(loopBody);
			gen.Emit(OpCodes.Ldloc_S, (byte)6);
			LoadGraphicsState(gen, IRbldr);
			gen.Emit(OpCodes.Callvirt, typeof(HintingMethod).GetMethod("Invoke"));

			// Increment the var.
			gen.Emit(OpCodes.Ldc_I4_1);
			gen.Emit(OpCodes.Ldloc_S, (byte)4);
			gen.Emit(OpCodes.Add);
			gen.Emit(OpCodes.Stloc_S, (byte)4);

			// Loop Condition
			gen.MarkLabel(loopCondition);
			gen.Emit(OpCodes.Ldloc_S, (byte)4);
			gen.Emit(OpCodes.Ldloc_S, (byte)5);
			gen.Emit(OpCodes.Blt_Un_S, loopBody);
			LoadGraphicsState(gen, IRbldr);
			gen.Emit(OpCodes.Ldc_I4_1);
			gen.Emit(OpCodes.Stfld, GraphicsState_Loop);
		}

		public override void WriteText(IRMethodBuilder IRbldr, StreamWriter tOut)
		{
			IRbldr.TWriteLine(tOut, "LoopCall[]");
			IRbldr.curInstructionLength = 10;
		}
	}
}
