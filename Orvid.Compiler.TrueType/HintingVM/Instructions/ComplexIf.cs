using System;
using System.IO;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;
using Orvid.Compiler.TrueType.Utils;

namespace Orvid.Compiler.TrueType.HintingVM.Instructions
{
	public enum IfCondition
	{
		Equal,
		NotEqual,
		Greater,
		GreaterOrEqual,
		Less,
		LessOrEqual,
		And,
		Or,
	}

	/// <summary>
	/// This is the second most complex op-code.
	/// </summary>
	public class ComplexIf : IRInstruction
	{
		public IfCondition Condition;

		public override IROpCode OpCode
		{
			get { return IROpCode.ComplexIf; }
		}

		public ComplexIf()
		{
		}

		public override void Emit(IRMethodBuilder IRbldr, ILGenerator gen, MethodBuilder mBldr, TypeBuilder tBldr, bool isGlyph)
		{
			LoadArgument(gen, 1, IRbldr);
			gen.Emit(OpCodes.Stloc_0);
			LoadArgument(gen, 2, IRbldr);
			gen.Emit(OpCodes.Ldloc_0);
			Label lfalse = gen.DefineLabel();
			switch (Condition)
			{
				case IfCondition.Equal:
					gen.Emit(OpCodes.Bne_Un, lfalse);
					break;
				case IfCondition.NotEqual:
					gen.Emit(OpCodes.Beq, lfalse);
					break;
				case IfCondition.Greater:
					gen.Emit(OpCodes.Ble, lfalse);
					break;
				case IfCondition.GreaterOrEqual:
					gen.Emit(OpCodes.Blt, lfalse);
					break;
				case IfCondition.Less:
					gen.Emit(OpCodes.Bge, lfalse);
					break;
				case IfCondition.LessOrEqual:
					gen.Emit(OpCodes.Bgt, lfalse);
					break;
				default:
					throw new Exception("Unknown condition for a ComplexIf!");
			}
			IRbldr.IfEndConditionBranches.Push(lfalse);
		}

		public override void WriteText(IRMethodBuilder IRbldr, StreamWriter tOut)
		{
			IRbldr.TWriteLine(tOut, "ComplexIf(" + this.ToString() + ")");
			IRbldr.curInstructionLength = ("ComplexIf(" + this.ToString() + ")").Length;
			IRbldr.curIdent += "".PadLeft(4, ' ');
		}

		public override string ToString()
		{
			return Args[1].ToString() + " " + ComplexCondition.GetConditionSymbol(Condition) + " " + Args[0].ToString();
		}
	}
}
