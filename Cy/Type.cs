using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cy {

	/// <summary>
	/// Deals with casting, literal arithmetic
	/// </summary>
	public static class Type {

		public static class Literal {


			/// <summary>
			///  Type error exception for compilation.
			/// </summary>
			public class TypeError : Exception {
				public TypeError() { }
				public TypeError(string message) : base(string.Format("Type error: {0}", message)) { }
			}

			public static string LLVMType(object val) {
				switch (ObjectType(val)) {
					case Stmt.Type.Kind.INT:
						return "i32";
					case Stmt.Type.Kind.FLOAT:
						return "fp64";
					case Stmt.Type.Kind.STR:
						return "*i8";
					case Stmt.Type.Kind.USERDEFINED:
						return "*i8";
					default:
						throw new TypeError("Unknown type");
				}
			}


			/// <summary>
			/// Can a value be cast to a diff type
			/// </summary>
			public static bool CanCast(Stmt.Type.Kind fromTypeKind, Stmt.Type.Kind toTypeKind) {
				switch (fromTypeKind) {
					case Stmt.Type.Kind.INT:
					case Stmt.Type.Kind.FLOAT:
						switch (toTypeKind) {
							case Stmt.Type.Kind.INT:
							case Stmt.Type.Kind.FLOAT:
								return true;
							case Stmt.Type.Kind.STR:
							case Stmt.Type.Kind.USERDEFINED:
								return false;
							default:
								return false;
						}
					case Stmt.Type.Kind.STR:
						switch (toTypeKind) {
							case Stmt.Type.Kind.INT:
							case Stmt.Type.Kind.FLOAT:
								return false;
							case Stmt.Type.Kind.STR:
								return true;
							case Stmt.Type.Kind.USERDEFINED:
								return false;
							default:
								return false;
						}
					case Stmt.Type.Kind.USERDEFINED:
						switch (toTypeKind) {
							case Stmt.Type.Kind.INT:
							case Stmt.Type.Kind.FLOAT:
							case Stmt.Type.Kind.STR:
								return false;
							case Stmt.Type.Kind.USERDEFINED:
								return true;
							default:
								return false;
						}
					default:
						return false;
				}
			}


			/// <summary>
			/// Cast a value to a diff type, throw exception if not possible
			/// </summary>
			public static object Cast(object val, Stmt.Type.Kind toTypeKind) {
				object ans = 0;
				switch (ObjectType(val)) {
					case Stmt.Type.Kind.INT:
						switch (toTypeKind) {
							case Stmt.Type.Kind.INT:
								return val;
							case Stmt.Type.Kind.FLOAT:
								return (double)val;
							case Stmt.Type.Kind.STR:
								throw new TypeError("Unable to cast int as str");
							case Stmt.Type.Kind.USERDEFINED:
								throw new TypeError("Unable to cast int object as object");
							default:
								throw new TypeError("Attempting to cast int as unknown type");
						}

					case Stmt.Type.Kind.FLOAT:
						switch (toTypeKind) {
							case Stmt.Type.Kind.INT:
								return (int)val;
							case Stmt.Type.Kind.FLOAT:
								return val;
							case Stmt.Type.Kind.STR:
								throw new TypeError("Unable to cast float as str");
							case Stmt.Type.Kind.USERDEFINED:
								throw new TypeError("Unable to cast float as str");
							default:
								throw new TypeError("Attempting to cast float as unknown type");
						}
					case Stmt.Type.Kind.STR:
						switch (toTypeKind) {
							case Stmt.Type.Kind.INT:
								throw new TypeError("Unable to cast str as int");
							case Stmt.Type.Kind.FLOAT:
								throw new TypeError("Unable to cast str as float");
							case Stmt.Type.Kind.STR:
								return val;
							case Stmt.Type.Kind.USERDEFINED:
								throw new TypeError("Unable to cast str as object");
							default:
								throw new TypeError("Unable to cast str as unknown");
						}
					case Stmt.Type.Kind.USERDEFINED:
						switch (toTypeKind) {
							case Stmt.Type.Kind.INT:
								throw new TypeError("Unable to cast object as int");
							case Stmt.Type.Kind.FLOAT:
								throw new TypeError("Unable to cast object as float");
							case Stmt.Type.Kind.STR:
								throw new TypeError("Unable to cast object as str");
							case Stmt.Type.Kind.USERDEFINED:
								return val;
							default:
								throw new TypeError("Unable to cast object as unknown");
						}
					default:
						return false;
				}
			}


			/// <summary>
			/// Get Stmt.Type.Kind of this object
			/// </summary>
			static Stmt.Type.Kind ObjectType(object val) {
				Stmt.Type.Kind kind;
				if (val is int i) {
					kind = Stmt.Type.Kind.INT;
				} else if (val is float f) {
					kind = Stmt.Type.Kind.FLOAT;
				} else if (val is string s) {
					kind = Stmt.Type.Kind.STR;
				} else {
					kind = Stmt.Type.Kind.USERDEFINED;
				}
				return kind;
			}


			/// <summary>
			/// Convert a float/f64 to the hexdecimal representation required for llvm
			/// </summary>
			public static string Float64Str(double val) {
				return BitConverter.DoubleToInt64Bits(val).ToString("X");
			}


			/// <summary>
			/// Add two literal values
			/// </summary>
			public static ExprValue Add(ExprValue lhs, ExprValue rhs) {
				if (lhs.value is int il) {
					if (rhs.value is int ir) {
						int val = il + ir;
						return new ExprValue("i32", val.ToString(), true, val);
					} else if (rhs.value is double dr) {
						double val = il + dr;
						return new ExprValue("fp64", val.ToString(), true, val);
					}
				} else if (lhs.value is double dl) {
					if (rhs.value is int ir) {
						double val = dl + ir;
						return new ExprValue("fp64", val.ToString(), true, val);
					} else if (rhs.value is double dr) {
						double val = dl + dr;
						return new ExprValue("fp64", val.ToString(), true, val);
					}
				}
				throw new TypeError("Expression value not float or integer");
			}


			/// <summary>
			/// Multiply two literal values
			/// </summary>
			public static ExprValue Mult(ExprValue lhs, ExprValue rhs) {
				if (lhs.value is int il) {
					if (rhs.value is int ir) {
						int val = il * ir;
						return new ExprValue("i32", val.ToString(), true, val);
					} else if (rhs.value is double dr) {
						double val = il * dr;
						return new ExprValue("fp64", val.ToString(), true, val);
					}
				} else if (lhs.value is double dl) {
					if (rhs.value is int ir) {
						double val = dl * ir;
						return new ExprValue("fp64", val.ToString(), true, val);
					} else if (rhs.value is double dr) {
						double val = dl * dr;
						return new ExprValue("fp64", val.ToString(), true, val);
					}
				}
				throw new TypeError("Expression value not float or integer");
			}


		} // Literal

	} // Types
}
