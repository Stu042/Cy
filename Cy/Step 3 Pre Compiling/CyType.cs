using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cy {

	// defines all types, gives type info for llvm
	public class CyType {
		/// <summary>Basic types.</summary>
		public enum Kind { UNKNOWN, INT, FLOAT, STR, USERDEFINED }; // USERDEFINED will typically be classes but who knows what the future will bring
		/// <summary>INT, FLOAT, STR.</summary>
		public Kind kind;
		/// <summary>Bit size of type.</summary>
		public int size;
		/// <summary>Is this a pointer.</summary>
		public bool isPtr;


		/// <summary>Type error exception for compilation.</summary>
		public class TypeError : Exception {
			public TypeError() { }
			public TypeError(string message) : base(string.Format("Type error: {0}", message)) { }
		}



		public CyType(Kind kind, int size, bool isPtr=false) {
			this.kind = kind;
			this.size = size;
			this.isPtr = isPtr;
		}

		// when doing arithmetic (add, sub, mult, etc) with two types this will be the resultant type
		public CyType(CyType lhs, CyType rhs) {
			switch (lhs.kind) {
				case Kind.INT:
					switch (rhs.kind) {
						case Kind.INT:
							kind = Kind.INT;
							size = Math.Min(Math.Max(lhs.size, rhs.size), 128);
							isPtr = false;
							break;
						case Kind.FLOAT:
							kind = Kind.FLOAT;
							size = Math.Min(Math.Max(lhs.size, rhs.size), 128);
							isPtr = false;
							break;
						case Kind.STR:
						case Kind.USERDEFINED:
						case Kind.UNKNOWN:
						default:
							kind = Kind.UNKNOWN;
							size = Math.Min(Math.Max(lhs.size, rhs.size), 128);
							isPtr = false;
							break;
					}
					break;
				case Kind.FLOAT:
					switch (rhs.kind) {
						case Kind.INT:
						case Kind.FLOAT:
							kind = Kind.FLOAT;
							size = Math.Min(Math.Max(lhs.size, rhs.size), 128);
							isPtr = false;
							break;
						case Kind.STR:
						case Kind.USERDEFINED:
						case Kind.UNKNOWN:
						default:
							kind = Kind.UNKNOWN;
							size = Math.Min(Math.Max(lhs.size, rhs.size), 128);
							isPtr = false;
							break;
					}
					break;
				case Kind.STR:
				case Kind.USERDEFINED:
				case Kind.UNKNOWN:
				default:
					kind = Kind.UNKNOWN;
					size = Math.Min(Math.Max(lhs.size, rhs.size), 128);
					isPtr = false;
					break;
			}
		}


		public CyType(Token.Kind tokKind) {
			switch (tokKind) {
				case Token.Kind.INT8:
					this.kind = Kind.INT;
					this.size = 8;
					this.isPtr = false;
					break;
				case Token.Kind.INT16:
					this.kind = Kind.INT;
					this.size = 16;
					this.isPtr = false;
					break;
				case Token.Kind.INT:
				case Token.Kind.INT32:
				case Token.Kind.INT_LITERAL:
					this.kind = Kind.INT;
					this.size = 32;
					this.isPtr = false;
					break;
				case Token.Kind.INT64:
					this.kind = Kind.INT;
					this.size = 64;
					this.isPtr = false;
					break;
				case Token.Kind.INT128:
					this.kind = Kind.INT;
					this.size = 128;
					this.isPtr = false;
					break;
				case Token.Kind.FLOAT16:
					this.kind = Kind.FLOAT;
					this.size = 16;
					this.isPtr = false;
					break;
				case Token.Kind.FLOAT32:
					this.kind = Kind.FLOAT;
					this.size = 32;
					this.isPtr = false;
					break;
				case Token.Kind.FLOAT64:
				case Token.Kind.FLOAT:
				case Token.Kind.FLOAT_LITERAL:
					this.kind = Kind.FLOAT;
					this.size = 64;
					this.isPtr = false;
					break;
				case Token.Kind.FLOAT128:
					this.kind = Kind.FLOAT;
					this.size = 128;
					this.isPtr = false;
					break;
				case Token.Kind.STR:
				case Token.Kind.STR_LITERAL:
					this.kind = Kind.STR;
					this.size = 8;
					this.isPtr = true;
					break;
				default:
					this.kind = Kind.USERDEFINED;
					this.size = 8;  // should check for Token.Kind.IDENTIFIER...
					this.isPtr = true;
					break;
			}
		}

		public void Set(Kind type, int size) {
			this.kind = type;
			this.size = size;
		}

		/// <summary>Return the align size for llvm.</summary>
		public int AlignSize() {
			int s = size;
			if (isPtr)
				s = 32;
			if (s > 64)
				return 16;
			else if (s > 32)
				return 8;
			else if (s > 16)
				return 4;
			return 1;
		}


		/// <summary>Return the type string for llvm.</summary>
		public string Llvm() {
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
					if (isPtr)
						return $"*i{s}";
					return $"i{s}";
				case Kind.FLOAT:
					switch (s) {
						case 16:
							if (isPtr)
								return $"*half";
							return $"half";
						case 32:
							if (isPtr)
								return $"*float";
							return $"float";
						case 64:
							if (isPtr)
								return $"*double";
							return $"double";
						case 128:
							if (isPtr)
								return $"*fp128";
							return $"fp128";
						default:
							throw new TypeError("Unknown size of type float requested.");
					}
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
	public static class CyTypeUtil {

		public static CyType CombineTypes(CyType lhs, CyType rhs) {
			switch (lhs.kind) {
				case CyType.Kind.INT:
					switch (rhs.kind) {
						case CyType.Kind.INT:
							return new CyType(CyType.Kind.INT, Math.Min(Math.Max(lhs.size, rhs.size), 128));
						case CyType.Kind.FLOAT:
							return new CyType(CyType.Kind.FLOAT, Math.Min(Math.Max(lhs.size, rhs.size), 128));
						case CyType.Kind.STR:
						case CyType.Kind.USERDEFINED:
						case CyType.Kind.UNKNOWN:
						default:
							return new CyType(CyType.Kind.UNKNOWN, Math.Min(Math.Max(lhs.size, rhs.size), 128));
					}
				case CyType.Kind.FLOAT:
					switch (rhs.kind) {
						case CyType.Kind.INT:
						case CyType.Kind.FLOAT:
							return new CyType(CyType.Kind.FLOAT, Math.Min(Math.Max(lhs.size, rhs.size), 128));
						case CyType.Kind.STR:
						case CyType.Kind.USERDEFINED:
						case CyType.Kind.UNKNOWN:
						default:
							return new CyType(CyType.Kind.UNKNOWN, Math.Min(Math.Max(lhs.size, rhs.size), 128));
					}
				case CyType.Kind.STR:
				case CyType.Kind.USERDEFINED:
				case CyType.Kind.UNKNOWN:
				default:
					return new CyType(CyType.Kind.UNKNOWN, Math.Min(Math.Max(lhs.size, rhs.size), 128));
			}
		}



		public static class Literal {

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
						throw new CyType.TypeError($"Unknown type for object of val: {val}");
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
								return true;		// probably not true... need id as well as kind
							default:
								return false;
						}
					default:
						return false;
				}
			}


			/// <summary>Get Stmt.Type.Kind of this object</summary>
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



		} // Literal


	} // Types
}
