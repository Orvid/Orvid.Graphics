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
	/// <summary>
	/// Assigns a value in the function table
	/// to the index on the top of the stack.
	/// </summary>
	public class FSet : IRInstruction
	{
		public IRMethodBuilder MethodToSet;
		public bool HasConstantIndex;
		public int ConstantIndex;

		public override IROpCode OpCode
		{
			get { return IROpCode.FSet; }
		}

		public FSet(IRMethodBuilder mthd)
		{
			this.MethodToSet = mthd;
		}

		public override void Emit(IRMethodBuilder IRbldr, ILGenerator gen, MethodBuilder mBldr, TypeBuilder tBldr, bool isGlyph)
		{
			LoadGraphicsState(gen, IRbldr);
			gen.Emit(OpCodes.Ldfld, GraphicsState_Functions);
			if (HasConstantIndex)
			{
				LoadInt(gen, (int)ConstantIndex);
			}
			else
			{
				LoadGraphicsState(gen, IRbldr);
				gen.Emit(OpCodes.Ldfld, GraphicsState_Stack);
				gen.Emit(OpCodes.Call, LinkedStack_Pop);
			}
			gen.Emit(OpCodes.Ldnull);
			gen.Emit(OpCodes.Ldftn, MethodToSet.mBldr);
			gen.Emit(OpCodes.Newobj, typeof(HintingMethod).GetConstructor(new Type[] { typeof(object), typeof(IntPtr) }));
			gen.Emit(OpCodes.Callvirt, typeof(Dictionary<int, HintingMethod>).GetMethod("Add"));
		}

		public override void WriteText(IRMethodBuilder IRbldr, StreamWriter tOut)
		{
			if (HasConstantIndex)
			{
				IRbldr.TWriteLine(tOut, "Synth-FSet(" + ConstantIndex.ToString() + ", " + MethodToSet.mBldr.Name + ")");
				IRbldr.curInstructionLength = 14 + ConstantIndex.ToString().Length + MethodToSet.mBldr.Name.Length;
			}
			else
			{
				IRbldr.TWriteLine(tOut, "Synth-FSet[]");
				IRbldr.curInstructionLength = 12;
			}
		}
	}
}
