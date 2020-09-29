using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cy {

	public static class Llvm {
		public static Func<string> NextVarRef;

		static Llvm() {
			NextVarRef = GetNextLLvmVarRef();
		}

		public static Func<string> GetNextLLvmVarRef() {
			int num = 0;
			return new Func<string>(() => {
				return $"%{num++}";
			});
		}


		public static string FloatName(int size) {
			switch (size) {
				case 16:
					return "half";
				case 32:
					return "float";
				case 64:
					return "double";
				case 128:
					return "fp128";
				default:
					throw new CyType.TypeError("Unknown size of float requested.");
			}
		}


		static int indent = 0;
		// Add indentation to a line of code and a newline at end
		public static string Indent(string line) {
			StringBuilder str = new StringBuilder();
			if (line.Contains("}"))
				--indent;
			for (int i = 0; i < indent; i++)
				str.Append("  ");
			str.Append(line);
			if (line.Contains("{"))
				indent++;
			return str.ToString();
		}
	}



	public class ExprValueLiteral : ExprValue {
		public object value;	// actual value
		// llvmref will equal text version of value
		public ExprValueLiteral(Token.Kind tokenType, object value) {
			this.llvmref = value.ToString();
			this.value = value;
			this.info = new CyType(tokenType);
		}
		public ExprValueLiteral(CyType cyType, object value) {
			this.llvmref = value.ToString();
			this.value = value;
			this.info = cyType;
		}

		/// <summary>Cast a value to a diff type, throw exception if not possible. returns code string in case this is required.</summary>
		public override string Cast(CyType newType) {
			object ans = 0;
			switch (info.kind) {
				case CyType.Kind.INT:
					switch (newType.kind) {
						case CyType.Kind.INT:
							info = newType;
							return "";
						case CyType.Kind.FLOAT: {
							// llvmref = llvmref + ".0";	// TODO is this reqd?
							info = newType;
							value = (double)value;
							return "";
						}
						case CyType.Kind.STR:
							throw new CyType.TypeError("Unable to cast int as str");
						case CyType.Kind.USERDEFINED:
							throw new CyType.TypeError("Unable to cast int as object");
						default:
							throw new CyType.TypeError("Attempting to cast int as unknown type");
					}
				case CyType.Kind.FLOAT:
					switch (newType.kind) {
						case CyType.Kind.INT: {
							int i = (int)double.Parse(llvmref);
							llvmref = i.ToString();
							info = newType;
							value = (int)value;
							return "";
						}
						case CyType.Kind.FLOAT:
							info = newType;
							return "";
						case CyType.Kind.STR:
							throw new CyType.TypeError("Unable to cast float as str.");
						case CyType.Kind.USERDEFINED:
							throw new CyType.TypeError("Unable to cast float as object.");
						default:
							throw new CyType.TypeError("Attempting to cast float as unknown type.");
					}
				case CyType.Kind.STR:
					switch (newType.kind) {
						case CyType.Kind.INT:
							throw new CyType.TypeError("Unable to cast str as int.");
						case CyType.Kind.FLOAT:
							throw new CyType.TypeError("Unable to cast str as float.");
						case CyType.Kind.STR:
							return "";
						case CyType.Kind.USERDEFINED:
							throw new CyType.TypeError("Unable to cast str as object.");
						default:
							throw new CyType.TypeError("Unable to cast str as unknown.");
					}
				case CyType.Kind.USERDEFINED:
					switch (newType.kind) {
						case CyType.Kind.INT:
							throw new CyType.TypeError("Unable to cast object as int.");
						case CyType.Kind.FLOAT:
							throw new CyType.TypeError("Unable to cast object as float.");
						case CyType.Kind.STR:
							throw new CyType.TypeError("Unable to cast object as str.");
						case CyType.Kind.USERDEFINED:
							return "";  // TODO fix, wrong as likely a diff user defined type
						default:
							throw new CyType.TypeError("Unable to cast object as unknown.");
					}
				default:
					throw new CyType.TypeError("Attempting to cast from an unknown type.");
			}
		}


		// bring a variable copy back ready to use
		public override ExprValueAndString Load() {
			return new ExprValueAndString(this, "");
		}

		// allocate space for a variable on the stack
		public override string StackAllocate() {
			return "";
		}

		// store a value in this llvm variable
		public override string Store(object val) {	// TODO show an error error
			return "";
		}




		// Add this by another
		public override ExprValueAndString Add(ExprValue rhs) {
			ExprValueAndString rhsvar = rhs.Load();
			if (rhsvar.exprValue is ExprValueLiteral rhsLiteral) {
				ExprValueLiteral rhsl = (ExprValueLiteral)rhs;
				ExprValue xpval = new ExprValueLiteral(new CyType(info, rhs.info), AddLiterals(this, rhsl));
				ExprValueLiteral xpvallit = new ExprValueLiteral(new CyType(info, rhs.info), xpval);
				return new ExprValueAndString(xpvallit, "");
			} else {
				ExprValue xpval = new ExprValue(new CyType(info, rhs.info));
				return new ExprValueAndString(xpval, rhsvar.code + Llvm.Indent($"{xpval.llvmref} = add nsw {info.Llvm()} {llvmref}, {rhsvar.exprValue.llvmref}\n"));
			}
		}
		// Add this by another
		public override ExprValueAndString Sub(ExprValue rhs) {
			ExprValueAndString rhsvar = rhs.Load();
			if (rhsvar.exprValue is ExprValueLiteral rhsLiteral) {
				ExprValueLiteral rhsl = (ExprValueLiteral)rhs;
				ExprValue xpval = new ExprValueLiteral(new CyType(info, rhs.info), SubLiterals(this, rhsl));
				ExprValueLiteral xpvallit = new ExprValueLiteral(new CyType(info, rhs.info), xpval);
				return new ExprValueAndString(xpvallit, "");
			} else {
				ExprValue xpval = new ExprValue(new CyType(info, rhs.info));
				return new ExprValueAndString(xpval, rhsvar.code + Llvm.Indent($"{xpval.llvmref} = sub nsw {info.Llvm()} {llvmref}, {rhsvar.exprValue.llvmref}\n"));
			}
		}
		// Mult this by another
		public override ExprValueAndString Mult(ExprValue rhs) {
			ExprValueAndString rhsvar = rhs.Load();
			if (rhsvar.exprValue is ExprValueLiteral rhsLiteral) {
				ExprValueLiteral rhsl = (ExprValueLiteral)rhs;
				ExprValue xpval = new ExprValueLiteral(new CyType(info, rhs.info), MultLiterals(this, rhsl));
				ExprValueLiteral xpvallit = new ExprValueLiteral(new CyType(info, rhs.info), xpval);
				return new ExprValueAndString(xpvallit, "");
			} else {
				ExprValue xpval = new ExprValue(new CyType(info, rhs.info));
				return new ExprValueAndString(xpval, rhsvar.code + Llvm.Indent($"{xpval.llvmref} = mul nsw {info.Llvm()} {llvmref}, {rhsvar.exprValue.llvmref}\n"));
			}
		}
		// Divide this by another
		public override ExprValueAndString Div(ExprValue rhs) {
			ExprValueAndString rhsvar = rhs.Load();
			if (rhsvar.exprValue is ExprValueLiteral rhsLiteral) {
				ExprValueLiteral rhsl = (ExprValueLiteral)rhs;
				ExprValue xpval = new ExprValueLiteral(new CyType(info, rhs.info), DivLiterals(this, rhsl));
				ExprValueLiteral xpvallit = new ExprValueLiteral(new CyType(info, rhs.info), xpval);
				return new ExprValueAndString(xpvallit, "");
			} else {
				ExprValue xpval = new ExprValue(new CyType(info, rhs.info));
				return new ExprValueAndString(xpval, rhsvar.code + Llvm.Indent($"{xpval.llvmref} = div nsw {info.Llvm()} {llvmref}, {rhsvar.exprValue.llvmref}\n"));
			}
		}


		object AddLiterals(ExprValueLiteral lhs, ExprValueLiteral rhs) {
			switch (info.kind) {
				case CyType.Kind.INT: {
					int l = (int)lhs.value;
					switch (rhs.info.kind) {
						case CyType.Kind.INT:
							return l + (int)lhs.value;
						case CyType.Kind.FLOAT:
							return l + (double)lhs.value;
						case CyType.Kind.STR:
							return 0;
						case CyType.Kind.USERDEFINED:
							return 0;
						case CyType.Kind.UNKNOWN:
							return 0;
						default:
							return 0;
					}
				}
				case CyType.Kind.FLOAT: {
					double l = (double)lhs.value;
					switch (rhs.info.kind) {
						case CyType.Kind.INT:
							return l + (int)lhs.value;
						case CyType.Kind.FLOAT:
							return l + (double)lhs.value;
						case CyType.Kind.STR:
							return 0;
						case CyType.Kind.USERDEFINED:
							return 0;
						case CyType.Kind.UNKNOWN:
							return 0;
						default:
							return 0;
					}
				}
				case CyType.Kind.STR:
					return 0;
				case CyType.Kind.USERDEFINED:
					return 0;
				case CyType.Kind.UNKNOWN:
					return 0;
				default:
					return 0;
			}
		}

		object SubLiterals(ExprValueLiteral lhs, ExprValueLiteral rhs) {
			switch (info.kind) {
				case CyType.Kind.INT: {
					int l = (int)lhs.value;
					switch (rhs.info.kind) {
						case CyType.Kind.INT:
							return l + (int)lhs.value;
						case CyType.Kind.FLOAT:
							return l + (double)lhs.value;
						case CyType.Kind.STR:
							return 0;
						case CyType.Kind.USERDEFINED:
							return 0;
						case CyType.Kind.UNKNOWN:
							return 0;
						default:
							return 0;
					}
				}
				case CyType.Kind.FLOAT: {
					double l = (double)lhs.value;
					switch (rhs.info.kind) {
						case CyType.Kind.INT:
							return l + (int)lhs.value;
						case CyType.Kind.FLOAT:
							return l + (double)lhs.value;
						case CyType.Kind.STR:
							return 0;
						case CyType.Kind.USERDEFINED:
							return 0;
						case CyType.Kind.UNKNOWN:
							return 0;
						default:
							return 0;
					}
				}
				case CyType.Kind.STR:
					return 0;
				case CyType.Kind.USERDEFINED:
					return 0;
				case CyType.Kind.UNKNOWN:
					return 0;
				default:
					return 0;
			}
		}

		object MultLiterals(ExprValueLiteral lhs, ExprValueLiteral rhs) {
			switch (info.kind) {
				case CyType.Kind.INT: {
					int l = (int)lhs.value;
					switch (rhs.info.kind) {
						case CyType.Kind.INT:
							return l * (int)lhs.value;
						case CyType.Kind.FLOAT:
							return l * (double)lhs.value;
						case CyType.Kind.STR:
							return 0;
						case CyType.Kind.USERDEFINED:
							return 0;
						case CyType.Kind.UNKNOWN:
							return 0;
						default:
							return 0;
					}
				}
				case CyType.Kind.FLOAT: {
					double l = (double)lhs.value;
					switch (rhs.info.kind) {
						case CyType.Kind.INT:
							return l * (int)lhs.value;
						case CyType.Kind.FLOAT:
							return l * (double)lhs.value;
						case CyType.Kind.STR:
							return 0;
						case CyType.Kind.USERDEFINED:
							return 0;
						case CyType.Kind.UNKNOWN:
							return 0;
						default:
							return 0;
					}
				}
				case CyType.Kind.STR:
					return 0;
				case CyType.Kind.USERDEFINED:
					return 0;
				case CyType.Kind.UNKNOWN:
					return 0;
				default:
					return 0;
			}
		}

		object DivLiterals(ExprValueLiteral lhs, ExprValueLiteral rhs) {
			switch (info.kind) {
				case CyType.Kind.INT: {
					int l = (int)lhs.value;
					switch (rhs.info.kind) {
						case CyType.Kind.INT:
							return l / (int)lhs.value;
						case CyType.Kind.FLOAT:
							return l / (double)lhs.value;
						case CyType.Kind.STR:
							return 0;
						case CyType.Kind.USERDEFINED:
							return 0;
						case CyType.Kind.UNKNOWN:
							return 0;
						default:
							return 0;
					}
				}
				case CyType.Kind.FLOAT: {
					double l = (double)lhs.value;
					switch (rhs.info.kind) {
						case CyType.Kind.INT:
							return l / (int)lhs.value;
						case CyType.Kind.FLOAT:
							return l / (double)lhs.value;
						case CyType.Kind.STR:
							return 0;
						case CyType.Kind.USERDEFINED:
							return 0;
						case CyType.Kind.UNKNOWN:
							return 0;
						default:
							return 0;
					}
				}
				case CyType.Kind.STR:
					return 0;
				case CyType.Kind.USERDEFINED:
					return 0;
				case CyType.Kind.UNKNOWN:
					return 0;
				default:
					return 0;
			}
		}

	} // ExprValueLiteral





	// info to track a declared variable
	public class ExprValue {
		public string llvmref;  // this will be %1 etc, or actual number if a literal
		public CyType info;

		public struct ExprValueAndString {
			public ExprValue exprValue;
			public string code;
			public ExprValueAndString(ExprValue exprValue, string code) {
				this.exprValue = exprValue;
				this.code = code;
			}
		}


		public ExprValue() {
			this.llvmref = "";
		}
		public ExprValue(Token.Kind tokenType) {
			this.llvmref = Llvm.NextVarRef();
			this.info = new CyType(tokenType);
		}
		public ExprValue(CyType cyType) {
			this.llvmref = Llvm.NextVarRef();
			this.info = cyType;
		}


		/// <summary>Cast a value to a diff type, throw exception if not possible. returns code string in case this is required.</summary>
		public virtual string Cast(CyType newType) {
			switch (info.kind) {
				case CyType.Kind.INT:
					switch (newType.kind) {
						case CyType.Kind.INT:
							if (info.size == newType.size)
								return "";
							if (info.size < newType.size) {
								string loadllvmref = Llvm.NextVarRef();
								string newllvmref = Llvm.NextVarRef();
								string code = Llvm.Indent($"{loadllvmref} = load {info.Llvm()}, {info.Llvm()}* {llvmref}, align {info.AlignSize()}\n") +
											Llvm.Indent($"{newllvmref} = sext {info.Llvm()} {loadllvmref} to {newType.Llvm()}\n");
								llvmref = newllvmref;
								info = newType;
								return code;
							}
							throw new CyType.TypeError($"Unable to cast int{info.size} as smaller int{newType.size}");
						case CyType.Kind.FLOAT: {
							string loadllvmref = Llvm.NextVarRef();
							string newllvmref = Llvm.NextVarRef();
							string code = Llvm.Indent($"{loadllvmref} = load {info.Llvm()}, {info.Llvm()}* {llvmref}, align {info.AlignSize()}\n") +
											Llvm.Indent($"{newllvmref} = sitofp {info.Llvm()} {loadllvmref} to {Llvm.FloatName(newType.size)}\n");
							llvmref = newllvmref;
							info = newType;
							return code;
						}
						case CyType.Kind.STR:
							throw new CyType.TypeError("Unable to cast int as str");
						case CyType.Kind.USERDEFINED:
							throw new CyType.TypeError("Unable to cast int as object");
						default:
							throw new CyType.TypeError("Attempting to cast int as unknown type");
					}
				case CyType.Kind.FLOAT:
					switch (newType.kind) {
						case CyType.Kind.INT: {
							string loadllvmref = Llvm.NextVarRef();
							string newllvmref = Llvm.NextVarRef();
							string code;
							if (info.size < newType.size) { 
								code = Llvm.Indent($"{loadllvmref} = load {info.Llvm()}, {info.Llvm()}* {llvmref}, align {info.AlignSize()}\n") +
										Llvm.Indent($"{newllvmref} = fptosi {info.Llvm()} {loadllvmref} to i{newType.size}\n");
							} else if (info.size > newType.size) {
								code = Llvm.Indent($"{loadllvmref} = load {info.Llvm()}, {info.Llvm()}* {llvmref}, align {info.AlignSize()}\n") +
										Llvm.Indent($"{newllvmref} = fptrunc {info.Llvm()} {loadllvmref} to i{newType.size}\n");
							} else {
								return "";
							}
							llvmref = newllvmref;
							info = newType;
							return code;
						}
						case CyType.Kind.FLOAT: {
							string loadllvmref = Llvm.NextVarRef();
							string newllvmref = Llvm.NextVarRef();
							string code;
							if (info.size < newType.size) {
								code = Llvm.Indent($"{loadllvmref} = load {info.Llvm()}, {info.Llvm()}* {llvmref}, align {info.AlignSize()}\n") +
										Llvm.Indent($"{newllvmref} = fpext {info.Llvm()} {loadllvmref} to i{newType.size}\n");
							} else if (info.size > newType.size) {
								code = Llvm.Indent($"{loadllvmref} = load {info.Llvm()}, {info.Llvm()}* {llvmref}, align {info.AlignSize()}\n") +
										Llvm.Indent($"{newllvmref} = fptrunc {info.Llvm()} {loadllvmref} to i{newType.size}\n");
							} else {
								return "";
							}
							llvmref = newllvmref;
							info = newType;
							return code;
						}
						case CyType.Kind.STR:
							throw new CyType.TypeError("Unable to cast float as str.");
						case CyType.Kind.USERDEFINED:
							throw new CyType.TypeError("Unable to cast float as object.");
						default:
							throw new CyType.TypeError("Attempting to cast float as unknown type.");
					}
				case CyType.Kind.STR:
					switch (newType.kind) {
						case CyType.Kind.INT:
							throw new CyType.TypeError("Unable to cast str as int.");
						case CyType.Kind.FLOAT:
							throw new CyType.TypeError("Unable to cast str as float.");
						case CyType.Kind.STR:
							return "";
						case CyType.Kind.USERDEFINED:
							throw new CyType.TypeError("Unable to cast str as object.");
						default:
							throw new CyType.TypeError("Unable to cast str as unknown.");
					}
				case CyType.Kind.USERDEFINED:
					switch (newType.kind) {
						case CyType.Kind.INT:
							throw new CyType.TypeError("Unable to cast object as int.");
						case CyType.Kind.FLOAT:
							throw new CyType.TypeError("Unable to cast object as float.");
						case CyType.Kind.STR:
							throw new CyType.TypeError("Unable to cast object as str.");
						case CyType.Kind.USERDEFINED:
							return "";	// TODO fix, wrong as likely a diff user defined type
						default:
							throw new CyType.TypeError("Unable to cast object as unknown.");
					}
				default:
					throw new CyType.TypeError("Attempting to cast from an unknown type.");
			}
		}



		// bring a variable copy back ready to use
		public virtual ExprValueAndString Load() {
			string llvmref = Llvm.NextVarRef();
			ExprValue xpval = new ExprValue(info);
			return new ExprValueAndString(xpval, Llvm.Indent($"{xpval.llvmref} = load {info.Llvm()}, {info.Llvm()}* {llvmref}, align {info.AlignSize()}\n"));
		}

		// allocate space for a variable on the stack
		public virtual string StackAllocate() {
			return Llvm.Indent($"{llvmref} = alloca {info.Llvm()}, align {info.AlignSize()}\n");
		}

		// store a value in this llvm variable
		public virtual string Store(object val) {
			return Llvm.Indent($"store {info.Llvm()} {val.ToString()} {info.Llvm()}* {llvmref}, align {info.AlignSize()}\n");
		}


		// Add this by another
		public virtual ExprValueAndString Add(ExprValue rhs) {
			ExprValueAndString lhsvar = Load();
			ExprValueAndString rhsvar = rhs.Load();
			ExprValue xpval = new ExprValue(new CyType(info, rhs.info));
			return new ExprValueAndString(xpval, lhsvar.code + rhsvar.code + Llvm.Indent($"{xpval.llvmref} = add nsw {info.Llvm()} {lhsvar.exprValue.llvmref}, {rhsvar.exprValue.llvmref}\n"));
		}
		// Add this by another
		public virtual ExprValueAndString Sub(ExprValue rhs) {
			ExprValueAndString lhsvar = Load();
			ExprValueAndString rhsvar = rhs.Load();
			ExprValue xpval = new ExprValue(new CyType(info, rhs.info));
			return new ExprValueAndString(xpval, lhsvar.code + rhsvar.code + Llvm.Indent($"{xpval.llvmref} = sub nsw {info.Llvm()} {lhsvar.exprValue.llvmref}, {rhsvar.exprValue.llvmref}\n"));
		}
		// Mult this by another
		public virtual ExprValueAndString Mult(ExprValue rhs) {
			ExprValueAndString lhsvar = Load();
			ExprValueAndString rhsvar = rhs.Load();
			ExprValue xpval = new ExprValue(new CyType(info, rhs.info));
			return new ExprValueAndString(xpval, lhsvar.code + rhsvar.code + Llvm.Indent($"{xpval.llvmref} = mul nsw {info.Llvm()} {lhsvar.exprValue.llvmref}, {rhsvar.exprValue.llvmref}\n"));
		}
		// Divide this by another
		public virtual ExprValueAndString Div(ExprValue rhs) {
			ExprValueAndString lhsvar = Load();
			ExprValueAndString rhsvar = rhs.Load();
			ExprValue xpval = new ExprValue(new CyType(info, rhs.info));
			return new ExprValueAndString(xpval, lhsvar.code + rhsvar.code + Llvm.Indent($"{xpval.llvmref} = div nsw {info.Llvm()} {lhsvar.exprValue.llvmref}, {rhsvar.exprValue.llvmref}\n"));
		}


		// will return something like "%1 i32"
		public override string ToString() {
			return $"{info.Llvm()} {llvmref}";
		}


	} // ExprValue
}
