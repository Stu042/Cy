using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cy {

	public class CyType {
		public enum Kind { UNKNOWN, INT, FLOAT, STR, USERDEFINED }; // USERDEFINED will typically be classes but who knows what the future will bring
		public Kind kind;
		public int size;
		public CyType(Kind kind, int size) {
			this.kind = kind;
			this.size = size;
		}

		/// <summary>
		/// Return the type string for llvm
		/// </summary>
		public string LLVM() {
			int s = size;
			if (size == 0) {
				if (kind == Kind.INT)
					s = 32;
				else if (kind == Kind.FLOAT)
					s = 64;
				else
					s = 8;
			}
			switch (kind) {
				case Kind.INT:
					return $"i{s}";
				case Kind.FLOAT:
					return $"fp{s}";
				case Kind.STR:
					return $"*i8";
				default:
					return $"*i8";
			}
		}
	} // CyType




	/// <summary>
	/// Deals with casting, literal arithmetic
	/// </summary>
	public static class Type {

		/// <summary>
		/// Cast a value to a diff type, throw exception if not possible
		/// </summary>
		//public static object Cast(object val, Stmt.Type.Kind toTypeKind) {

		//}


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
					case CyType.Kind.INT:
						return "i32";
					case CyType.Kind.FLOAT:
						return "fp64";
					case CyType.Kind.STR:
						return "*i8";
					case CyType.Kind.USERDEFINED:
						return "*i8";
					default:
						throw new TypeError("Unknown type");
				}
			}


			/// <summary>
			/// Can a value be cast to a diff type
			/// </summary>
			public static bool CanCast(CyType.Kind fromTypeKind, CyType.Kind toTypeKind) {
				switch (fromTypeKind) {
					case CyType.Kind.INT:
					case CyType.Kind.FLOAT:
						switch (toTypeKind) {
							case CyType.Kind.INT:
							case CyType.Kind.FLOAT:
								return true;
							case CyType.Kind.STR:
							case CyType.Kind.USERDEFINED:
								return false;
							default:
								return false;
						}
					case CyType.Kind.STR:
						switch (toTypeKind) {
							case CyType.Kind.INT:
							case CyType.Kind.FLOAT:
								return false;
							case CyType.Kind.STR:
								return true;
							case CyType.Kind.USERDEFINED:
								return false;
							default:
								return false;
						}
					case CyType.Kind.USERDEFINED:
						switch (toTypeKind) {
							case CyType.Kind.INT:
							case CyType.Kind.FLOAT:
							case CyType.Kind.STR:
								return false;
							case CyType.Kind.USERDEFINED:
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
			public static ExprValue Cast(ExprValue val, CyType toType) {
				object ans = 0;
				switch (val.info.kind) {
					case CyType.Kind.INT:
						switch (toType.kind) {
							case CyType.Kind.INT:
								return val;
							case CyType.Kind.FLOAT:
								double d = (double)val.value;
								return new ExprValue(toType, d.ToString(), val.isLiteral, d);
							case CyType.Kind.STR:
								throw new TypeError("Unable to cast int as str");
							case CyType.Kind.USERDEFINED:
								throw new TypeError("Unable to cast int object as object");
							default:
								throw new TypeError("Attempting to cast int as unknown type");
						}

					case CyType.Kind.FLOAT:
						switch (toType.kind) {
							case CyType.Kind.INT:
								int i = (int)Math.Floor((double)val.value);
								return new ExprValue(toType, i.ToString(), val.isLiteral, i);
							case CyType.Kind.FLOAT:
								return val;
							case CyType.Kind.STR:
								throw new TypeError("Unable to cast float as str");
							case CyType.Kind.USERDEFINED:
								throw new TypeError("Unable to cast float as str");
							default:
								throw new TypeError("Attempting to cast float as unknown type");
						}
					case CyType.Kind.STR:
						switch (toType.kind) {
							case CyType.Kind.INT:
								throw new TypeError("Unable to cast str as int");
							case CyType.Kind.FLOAT:
								throw new TypeError("Unable to cast str as float");
							case CyType.Kind.STR:
								return val;
							case CyType.Kind.USERDEFINED:
								throw new TypeError("Unable to cast str as object");
							default:
								throw new TypeError("Unable to cast str as unknown");
						}
					case CyType.Kind.USERDEFINED:
						switch (toType.kind) {
							case CyType.Kind.INT:
								throw new TypeError("Unable to cast object as int");
							case CyType.Kind.FLOAT:
								throw new TypeError("Unable to cast object as float");
							case CyType.Kind.STR:
								throw new TypeError("Unable to cast object as str");
							case CyType.Kind.USERDEFINED:
								return val;
							default:
								throw new TypeError("Unable to cast object as unknown");
						}
					default:
						return val;
				}
			}


			/// <summary>
			/// Get Stmt.Type.Kind of this object
			/// </summary>
			static CyType.Kind ObjectType(object val) {
				CyType.Kind kind;
				if (val is int i) {
					kind = CyType.Kind.INT;
				} else if(val is long l) {
					kind = CyType.Kind.INT;
				} else if (val is float f) {
					kind = CyType.Kind.FLOAT;
				} else if (val is double d) {
					kind = CyType.Kind.FLOAT;
				} else if (val is string s) {
					kind = CyType.Kind.STR;
				} else {
					kind = CyType.Kind.USERDEFINED;
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
						return new ExprValue(lhs.info, val.ToString(), true, val);
					} else if (rhs.value is double dr) {
						double val = il + dr;
						return new ExprValue(rhs.info, val.ToString(), true, val);
					}
				} else if (lhs.value is double dl) {
					if (rhs.value is int ir) {
						double val = dl + ir;
						return new ExprValue(lhs.info, val.ToString(), true, val);
					} else if (rhs.value is double dr) {
						double val = dl + dr;
						return new ExprValue(lhs.info, val.ToString(), true, val);
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
						return new ExprValue(lhs.info, val.ToString(), true, val);
					} else if (rhs.value is double dr) {
						double val = il * dr;
						return new ExprValue(rhs.info, val.ToString(), true, val);
					}
				} else if (lhs.value is double dl) {
					if (rhs.value is int ir) {
						double val = dl * ir;
						return new ExprValue(lhs.info, val.ToString(), true, val);
					} else if (rhs.value is double dr) {
						double val = dl * dr;
						return new ExprValue(lhs.info, val.ToString(), true, val);
					}
				}
				throw new TypeError("Expression value not float or integer");
			}


		} // Literal

	} // Types
}
