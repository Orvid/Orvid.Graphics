using System;
using System.IO;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;
using Orvid.Compiler.TrueType.Utils;

namespace Orvid.Compiler.TrueType.HintingVM.Instructions
{
	/// <summary>
	/// This is the single most complex op-code.
	/// </summary>
	public class ComplexCondition : IRInstruction
	{
		#region Complex Statements
		public class ComplexLiteral : ComplexCondition
		{
			public int LiteralValue;

			public ComplexLiteral(int val)
			{
				this.LiteralValue = val;
			}

			public override IROpCode OpCode
			{
				get { return IROpCode.ComplexCondition_ComplexLiteral; }
			}

			public override void Emit(IRMethodBuilder IRbldr, ILGenerator gen, MethodBuilder mBldr, TypeBuilder tBldr, bool isGlyph)
			{
				LoadInt(gen, LiteralValue);
			}

			public override string ToString()
			{
				return LiteralValue.ToString();
			}
		}

		public class ComplexMeasurePointSize : ComplexCondition
		{
			public override IROpCode OpCode
			{
				get { return IROpCode.ComplexCondition_ComplexMeasurePointSize; }
			}
			public override void Emit(IRMethodBuilder IRbldr, ILGenerator gen, MethodBuilder mBldr, TypeBuilder tBldr, bool isGlyph)
			{
				LoadGraphicsState(gen, IRbldr);
				gen.Emit(OpCodes.Ldfld, GraphicsState_PointSize);
			}

			public override string ToString()
			{
				return "MeasurePointSize()";
			}
		}

		#endregion

		private ComplexCondition local_SideA;
		public ComplexCondition SideA
		{
			get { return local_SideA; }
			set
			{
				if (local_SideA != null)
					local_SideA.ParentCondition = null;
				value.ParentCondition = this;
				local_SideA = value;
			}
		}
		private ComplexCondition local_SideB;
		public ComplexCondition SideB
		{
			get { return local_SideB; }
			set
			{
				if (local_SideB != null)
					local_SideB.ParentCondition = null;
				value.ParentCondition = this;
				local_SideB = value;
			}
		}
		private Label local_LabelFalse;
		public Label LabelFalse
		{
			get { return local_LabelFalse; }
			set
			{
				if (SideA != null)
					SideA.LabelFalse = value;
				if (SideB != null)
					SideB.LabelFalse = value;
				local_LabelFalse = value;
			}
		}

		public bool NeedsInverse;
		public Label LabelTrue;

		public IfCondition Condition;
		public ComplexCondition ParentCondition;


		public override IROpCode OpCode
		{
			get { return IROpCode.ComplexCondition; }
		}

		public ComplexCondition()
		{
		}

		public override void Emit(IRMethodBuilder IRbldr, ILGenerator gen, MethodBuilder mBldr, TypeBuilder tBldr, bool isGlyph)
		{
			switch (Condition)
			{
				// Simple comparison conditions.
				case IfCondition.Equal:
				case IfCondition.NotEqual:
				case IfCondition.Greater:
				case IfCondition.GreaterOrEqual:
				case IfCondition.Less:
				case IfCondition.LessOrEqual:
				{
					SideA.Emit(IRbldr, gen, mBldr, tBldr, isGlyph);
					SideB.Emit(IRbldr, gen, mBldr, tBldr, isGlyph);
					//if (this.ParentCondition != null && this.ParentCondition.Condition == IfCondition.Or && (this.ParentCondition.SideA == this || (this.ParentCondition.ParentCondition != null && this.ParentCondition.ParentCondition.Condition == IfCondition.Or)))
					if (NeedsInverse)
					{
					    Label ltrue = LabelTrue;
					    switch (Condition)
					    {
					        case IfCondition.Equal:
					            gen.Emit(OpCodes.Beq, ltrue);
					            break;
					        case IfCondition.NotEqual:
					            gen.Emit(OpCodes.Bne_Un, ltrue);
					            break;
					        case IfCondition.Greater:
					            gen.Emit(OpCodes.Bgt, ltrue);
					            break;
					        case IfCondition.GreaterOrEqual:
					            gen.Emit(OpCodes.Bge, ltrue);
					            break;
					        case IfCondition.Less:
					            gen.Emit(OpCodes.Blt, ltrue);
					            break;
					        case IfCondition.LessOrEqual:
					            gen.Emit(OpCodes.Ble, ltrue);
					            break;
					        default:
					            throw new Exception("Unknown condition for a ComplexIf!");
					    }
					}
					else
					{
						Label lfalse = LabelFalse;
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
					}
					break;
				}
				case IfCondition.And:
				{
					if (this.ParentCondition == null)
					{
						Label lFalse = gen.DefineLabel();
						this.LabelFalse = lFalse;
					}
					else if (ParentCondition.Condition != IfCondition.And)
					{
						Label lFalse = gen.DefineLabel();
						this.LabelFalse = lFalse;
					}
					SideA.Emit(IRbldr, gen, mBldr, tBldr, isGlyph);
					SideB.Emit(IRbldr, gen, mBldr, tBldr, isGlyph);
					if (this.ParentCondition != null)
					{
						if (ParentCondition.Condition != IfCondition.And)
						{
							gen.MarkLabel(LabelFalse);
						}
					}
					else
					{
						IRbldr.IfEndConditionBranches.Push(LabelFalse);
					}
					break;
				}
				case IfCondition.Or:
				{
					if (this.ParentCondition == null)
					{
						Label lFalse = gen.DefineLabel();
						this.LabelFalse = lFalse;
					}
					else
					{
						Label lFalse = gen.DefineLabel();
						SideA.LabelFalse = lFalse;
					}
					SideA.Emit(IRbldr, gen, mBldr, tBldr, isGlyph);
					SideB.Emit(IRbldr, gen, mBldr, tBldr, isGlyph);
					if (this.ParentCondition != null)
					{
						if (SideA.LabelFalse != LabelFalse)
							gen.MarkLabel(SideA.LabelFalse);
					}
					else
					{
						Label l = gen.DefineLabel();
						gen.Emit(OpCodes.Br, l);
						gen.MarkLabel(LabelFalse);
						IRbldr.IfEndConditionBranches.Push(l);
					}
					break;
				}
				default:
					throw new Exception("Unknown condition for complex if!");
			}
		}

		public override void WriteText(IRMethodBuilder IRbldr, StreamWriter tOut)
		{
			// Only the top-level instruction will ever have this called.
			IRbldr.TWriteLine(tOut, "ComplexCondition(" + this.ToString() + ")");
			IRbldr.curInstructionLength = ("ComplexCondition(" + this.ToString() + ")").Length;
			IRbldr.curIdent += "".PadLeft(4, ' ');
		}

		public override string ToString()
		{
			if ((this.Condition == IfCondition.And || this.Condition == IfCondition.Or) && ParentCondition != null)
			{
				return "(" + SideA.ToString() + " " + GetConditionSymbol(Condition) + " " + SideB.ToString() + ")";
			}
			else
			{
				return SideA.ToString() + " " + GetConditionSymbol(Condition) + " " + SideB.ToString();
			}
		}

		public static string GetConditionSymbol(IfCondition cond)
		{
			switch (cond)
			{
				case IfCondition.Equal:
					return "==";
				case IfCondition.NotEqual:
					return "!=";
				case IfCondition.Greater:
					return ">";
				case IfCondition.GreaterOrEqual:
					return ">=";
				case IfCondition.Less:
					return "<";
				case IfCondition.LessOrEqual:
					return "<=";
				case IfCondition.Or:
					return "||";
				case IfCondition.And:
					return "&&";
				default:
					return "!!!!! UNKNOWN !!!!!";
			}
		}
	}
}
