using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cy {

	// info to track a variable declared in llvm
	// includes methods to perform standard function on a value
	public class ExprValue {
		public string llvmref;  // this will be %1 etc, or actual number if a literal
		public CyType cytype;
		protected StringBuilder code;


		public ExprValue(StringBuilder code) {
			this.llvmref = "";
			this.code = code;
		}
		public ExprValue(StringBuilder code, Token.Kind tokenType) {
			this.llvmref = Llvm.RefNew();
			this.cytype = new CyType(tokenType);
			this.code = code;
		}
		public ExprValue(StringBuilder code, CyType cyType) {
			this.llvmref = Llvm.RefNew();
			this.cytype = cyType;
			this.code = code;
		}


		/// <summary>Cast a value to a diff type, throw exception if not possible. returns code string in case this is required.</summary>
		public virtual void Cast(CyType newType) {
			switch (cytype.kind) {
				case CyType.Kind.INT:
					switch (newType.kind) {
						case CyType.Kind.INT:
							if (cytype.size == newType.size)
								return;
							if (cytype.size < newType.size) {
								string loadllvmref = Llvm.RefNew();
								string newllvmref = Llvm.RefNew();
								code.Append(Llvm.Indent($"{loadllvmref} = load {cytype.Llvm()}, {cytype.Llvm()}* {llvmref}, align {cytype.AlignSize()}\n") +
											Llvm.Indent($"{newllvmref} = sext {cytype.Llvm()} {loadllvmref} to {newType.Llvm()}\n"));
								llvmref = newllvmref;
								cytype = newType;
								return;
							}
							throw new CyType.TypeError($"Unable to cast int{cytype.size} as smaller int{newType.size}");
						case CyType.Kind.FLOAT: {
							string loadllvmref = Llvm.RefNew();
							string newllvmref = Llvm.RefNew();
							code.Append(Llvm.Indent($"{loadllvmref} = load {cytype.Llvm()}, {cytype.Llvm()}* {llvmref}, align {cytype.AlignSize()}\n") +
											Llvm.Indent($"{newllvmref} = sitofp {cytype.Llvm()} {loadllvmref} to {Llvm.FloatName(newType.size)}\n"));
							llvmref = newllvmref;
							cytype = newType;
							return;
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
							string loadllvmref = Llvm.RefNew();
							string newllvmref = Llvm.RefNew();
							if (cytype.size < newType.size) {
								code.Append(Llvm.Indent($"{loadllvmref} = load {cytype.Llvm()}, {cytype.Llvm()}* {llvmref}, align {cytype.AlignSize()}\n") +
										Llvm.Indent($"{newllvmref} = fptosi {cytype.Llvm()} {loadllvmref} to i{newType.size}\n"));
							} else if (cytype.size > newType.size) {
								code.Append(Llvm.Indent($"{loadllvmref} = load {cytype.Llvm()}, {cytype.Llvm()}* {llvmref}, align {cytype.AlignSize()}\n") +
										Llvm.Indent($"{newllvmref} = fptrunc {cytype.Llvm()} {loadllvmref} to i{newType.size}\n"));
							} else
								return;
							llvmref = newllvmref;
							cytype = newType;
							return;
						}
						case CyType.Kind.FLOAT: {
							string loadllvmref = Llvm.RefNew();
							string newllvmref = Llvm.RefNew();
							if (cytype.size < newType.size) {
								code.Append(Llvm.Indent($"{loadllvmref} = load {cytype.Llvm()}, {cytype.Llvm()}* {llvmref}, align {cytype.AlignSize()}\n") +
										Llvm.Indent($"{newllvmref} = fpext {cytype.Llvm()} {loadllvmref} to i{newType.size}\n"));
							} else if (cytype.size > newType.size) {
								code.Append(Llvm.Indent($"{loadllvmref} = load {cytype.Llvm()}, {cytype.Llvm()}* {llvmref}, align {cytype.AlignSize()}\n") +
										Llvm.Indent($"{newllvmref} = fptrunc {cytype.Llvm()} {loadllvmref} to i{newType.size}\n"));
							} else {
								return;
							}
							llvmref = newllvmref;
							cytype = newType;
							return;
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
							return;
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
							return;  // TODO fix, wrong as likely a diff user defined type
						default:
							throw new CyType.TypeError("Unable to cast object as unknown.");
					}
				default:
					throw new CyType.TypeError("Attempting to cast from an unknown type.");
			}
		}



		// bring a variable copy back ready to use
		public virtual ExprValue Load() {
			//string llvmref = Llvm.NextVarRef();
			ExprValue xpval = new ExprValue(code, cytype);
			code.Append(Llvm.Indent($"{xpval.llvmref} = load {cytype.Llvm()}, {cytype.Llvm()}* {llvmref}, align {cytype.AlignSize()}\n"));
			return xpval;
		}

		// allocate space for a variable on the stack
		public virtual void StackAllocate() {
			code.Append(Llvm.Indent($"{llvmref} = alloca {cytype.Llvm()}, align {cytype.AlignSize()}\n"));
		}

		// store a value in this llvm variable
		public virtual void Store(object val) {
			if (val is ExprValue xp)
				code.Append(Llvm.Indent($"store {cytype.Llvm()} {xp.llvmref}, {cytype.Llvm()}* {llvmref}, align {cytype.AlignSize()}\n"));
			else
				code.Append(Llvm.Indent($"store {cytype.Llvm()} {val}, {cytype.Llvm()}* {llvmref}, align {cytype.AlignSize()}\n"));
		}


		// perform an arith operation on this, operation = "mul nsw" or similar
		ExprValue Arith(string operation, ExprValue rhs) {
			//ExprValue lhsvar = Load();
			//ExprValue rhsvar = rhs.Load();
			ExprValue xpv = new ExprValue(code, new CyType(cytype, rhs.cytype));
			//code.Append(Llvm.Indent($"{xpv.llvmref} = {operation} {cytype.Llvm()} {lhsvar.llvmref}, {rhsvar.llvmref}\n"));
			code.Append(Llvm.Indent($"{xpv.llvmref} = {operation} {cytype.Llvm()} {llvmref}, {rhs.llvmref}\n"));
			return xpv;
		}

		// Add this by another
		public virtual ExprValue Add(ExprValue rhs) {
			return Arith("add nsw", rhs);
		}
		// Add this by another
		public virtual ExprValue Sub(ExprValue rhs) {
			return Arith("sub nsw", rhs);
		}
		// Mult this by another
		public virtual ExprValue Mult(ExprValue rhs) {
			return Arith("mul nsw", rhs);
		}
		// Divide this by another
		public virtual ExprValue Div(ExprValue rhs) {
			return Arith("div nsw", rhs);
		}


		// will return something like "i32 %1"
		public override string ToString() {
			return $"{cytype.Llvm()} {llvmref}";
		}


	} // ExprValue




	// same but for a literal value
	public class ExprValueLiteral : ExprValue {
		public object value;	// actual value
		// llvmref will equal text version of value
		public ExprValueLiteral(StringBuilder code, Token.Kind tokenType, object value) : base(code) {
			this.llvmref = value.ToString();
			this.value = value;
			this.cytype = new CyType(tokenType);
		}
		public ExprValueLiteral(StringBuilder code,  CyType cyType, object value) : base(code) {
			this.llvmref = value.ToString();
			this.value = value;
			this.cytype = cyType;
		}

		/// <summary>Cast a value to a diff type, throw exception if not possible. returns code string in case this is required.</summary>
		public override void Cast(CyType newType) {
			switch (cytype.kind) {
				case CyType.Kind.INT:
					switch (newType.kind) {
						case CyType.Kind.INT:
							cytype = newType;
							return;
						case CyType.Kind.FLOAT: {
							// llvmref = llvmref + ".0";	// TODO is this reqd?
							cytype = newType;
							value = (double)value;
							return;
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
							cytype = newType;
							value = (int)value;
							return;
						}
						case CyType.Kind.FLOAT:
							cytype = newType;
							return;
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
							return;
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
							return;  // TODO fix, wrong as likely a diff user defined type
						default:
							throw new CyType.TypeError("Unable to cast object as unknown.");
					}
				default:
					throw new CyType.TypeError("Attempting to cast from an unknown type.");
			}
		}


		// bring a variable copy back ready to use
		public override ExprValue Load() {
			return this;
		}

		// allocate space for a variable on the stack
		public override void StackAllocate() {
		}

		// store a value in this llvm variable
		public override void Store(object val) {
			throw new CyType.TypeError("Attempting to change the value of a literal is not possible.");
		}


		// perform an arithmetice operation, operation = "mul nsw" or similar
		ExprValue Arith(string operation, ExprValue rhs) {
			ExprValue rhsvar = rhs.Load();
			if (rhsvar is ExprValueLiteral rhsLiteral) {
				ExprValueLiteral rhsl = (ExprValueLiteral)rhs;
				return new ExprValueLiteral(code, new CyType(cytype, rhs.cytype), AddLiterals(this, rhsl));
			} else {
				ExprValue xpval = new ExprValue(code, new CyType(cytype, rhs.cytype));
				code.Append(Llvm.Indent($"{xpval.llvmref} = {operation} {cytype.Llvm()} {llvmref}, {rhsvar.llvmref}\n"));
				return xpval;
			}
		}

		// Add this by another
		public override ExprValue Add(ExprValue rhs) {
			return Arith("add nsw", rhs);
		}
		// Add this by another
		public override ExprValue Sub(ExprValue rhs) {
			return Arith("sub nsw", rhs);
		}
		// Mult this by another
		public override ExprValue Mult(ExprValue rhs) {
			return Arith("mul nsw", rhs);
		}
		// Divide this by another
		public override ExprValue Div(ExprValue rhs) {
			return Arith("div nsw", rhs);
		}


		object AddLiterals(ExprValueLiteral lhs, ExprValueLiteral rhs) {
			switch (cytype.kind) {
				case CyType.Kind.INT: {
					Type tp = lhs.value.GetType();
					int l = (int)lhs.value;
					switch (rhs.cytype.kind) {
						case CyType.Kind.INT:
							return l + (int)rhs.value;
						case CyType.Kind.FLOAT:
							return l + (double)rhs.value;
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
					switch (rhs.cytype.kind) {
						case CyType.Kind.INT:
							return l + (int)rhs.value;
						case CyType.Kind.FLOAT:
							return l + (double)rhs.value;
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
			switch (cytype.kind) {
				case CyType.Kind.INT: {
					int l = (int)lhs.value;
					switch (rhs.cytype.kind) {
						case CyType.Kind.INT:
							return l + (int)rhs.value;
						case CyType.Kind.FLOAT:
							return l + (double)rhs.value;
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
					switch (rhs.cytype.kind) {
						case CyType.Kind.INT:
							return l + (int)rhs.value;
						case CyType.Kind.FLOAT:
							return l + (double)rhs.value;
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
			switch (cytype.kind) {
				case CyType.Kind.INT: {
					int l = (int)lhs.value;
					switch (rhs.cytype.kind) {
						case CyType.Kind.INT:
							return l * (int)rhs.value;
						case CyType.Kind.FLOAT:
							return l * (double)rhs.value;
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
					switch (rhs.cytype.kind) {
						case CyType.Kind.INT:
							return l * (int)rhs.value;
						case CyType.Kind.FLOAT:
							return l * (double)rhs.value;
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
			switch (cytype.kind) {
				case CyType.Kind.INT: {
					int l = (int)lhs.value;
					switch (rhs.cytype.kind) {
						case CyType.Kind.INT:
							return l / (int)rhs.value;
						case CyType.Kind.FLOAT:
							return l / (double)rhs.value;
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
					switch (rhs.cytype.kind) {
						case CyType.Kind.INT:
							return l / (int)rhs.value;
						case CyType.Kind.FLOAT:
							return l / (double)rhs.value;
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



}
