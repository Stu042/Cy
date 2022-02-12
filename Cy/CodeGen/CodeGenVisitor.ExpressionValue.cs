
using System;

namespace Cy.CodeGen;



public partial class CodeGenVisitor {
	public class ExpressionValue {
		public enum Type { UNKNOWN, INT, FLOAT, STRING }
		public bool IsLiteral;
		public string TextValue;
		public object Value;
		public Type ValueType;


		public static Type GetType(ExpressionValue left, ExpressionValue right) {
			return left.ValueType switch {
				Type.INT => right.ValueType switch {
					Type.INT => Type.INT,
					Type.FLOAT => Type.FLOAT,
					Type.STRING => Type.STRING,
					Type.UNKNOWN => Type.UNKNOWN,
					_ => Type.UNKNOWN
				},
				Type.FLOAT => right.ValueType switch {
					Type.INT => Type.FLOAT,
					Type.FLOAT => Type.FLOAT,
					Type.STRING => Type.STRING,
					Type.UNKNOWN => Type.UNKNOWN,
					_ => Type.UNKNOWN
				},
				Type.STRING => right.ValueType switch {
					Type.INT => Type.STRING,
					Type.FLOAT => Type.STRING,
					Type.STRING => Type.STRING,
					Type.UNKNOWN => Type.UNKNOWN,
					_ => Type.UNKNOWN
				},
				Type.UNKNOWN => Type.UNKNOWN,
				_ => Type.UNKNOWN
			};
		}
		public static string AddLiteral(ExpressionValue left, ExpressionValue right) {
			if (left.IsLiteral && right.IsLiteral) {
				switch (left.ValueType) {
					case Type.INT:
						switch (right.ValueType) {
							case Type.INT: {
									var total = (int)left.Value + (int)right.Value;
									return total.ToString();
								}
							case Type.FLOAT: {
									var total = (int)left.Value + (float)right.Value;
									return total.ToString();
								}
							case Type.STRING: {
									var total = (int)left.Value + (string)right.Value;
									return total.ToString();
								}
							case Type.UNKNOWN:
							default:
								// error
								break;
						}
						break;
					case Type.FLOAT:
						switch (right.ValueType) {
							case Type.INT: {
									var total = (float)left.Value + (int)right.Value;
									return total.ToString();
								}
							case Type.FLOAT: {
									var total = (float)left.Value + (float)right.Value;
									return total.ToString();
								}
							case Type.STRING: {
									var total = (float)left.Value + (string)right.Value;
									return total.ToString();
								}
							case Type.UNKNOWN:
							default:
								// error
								break;
						}
						break;
					case Type.STRING:
						switch (right.ValueType) {
							case Type.INT: {
									var total = (string)left.Value + (int)right.Value;
									return total.ToString();
								}
							case Type.FLOAT: {
									var total = (string)left.Value + (float)right.Value;
									return total.ToString();
								}
							case Type.STRING: {
									var total = (string)left.Value + (string)right.Value;
									return total.ToString();
								}
							case Type.UNKNOWN:
							default:
								// error
								break;
						}
						break;
					case Type.UNKNOWN:
					default:
						// error
						break;
				}
			}
			return String.Empty;
		}
		public static string SubLiteral(ExpressionValue left, ExpressionValue right) {
			if (left.IsLiteral && right.IsLiteral) {
				switch (left.ValueType) {
					case Type.INT:
						switch (right.ValueType) {
							case Type.INT: {
									var total = (int)left.Value - (int)right.Value;
									return total.ToString();
								}
							case Type.FLOAT: {
									var total = (int)left.Value - (float)right.Value;
									return total.ToString();
								}
							case Type.STRING:
								// error
								break;
							case Type.UNKNOWN:
							default:
								// error
								break;
						}
						break;
					case Type.FLOAT:
						switch (right.ValueType) {
							case Type.INT: {
									var total = (float)left.Value - (int)right.Value;
									return total.ToString();
								}
							case Type.FLOAT: {
									var total = (float)left.Value - (float)right.Value;
									return total.ToString();
								}
							case Type.STRING:
								// error
								break;
							case Type.UNKNOWN:
							default:
								// error
								break;
						}
						break;
					case Type.STRING:
						// error
						break;
					case Type.UNKNOWN:
					default:
						// error
						break;
				}
			}
			return String.Empty;
		}
		public static string MultLiteral(ExpressionValue left, ExpressionValue right) {
			if (left.IsLiteral && right.IsLiteral) {
				switch (left.ValueType) {
					case Type.INT:
						switch (right.ValueType) {
							case Type.INT:
								var intTotal = (int)left.Value * (int)right.Value;
								return intTotal.ToString();
							case Type.FLOAT:
								var floatTotal = (int)left.Value * (float)right.Value;
								return floatTotal.ToString();
							case Type.STRING:
								// error
								break;
							case Type.UNKNOWN:
							default:
								// error
								break;
						}
						break;
					case Type.FLOAT:
						switch (right.ValueType) {
							case Type.INT:
								var intTotal = (float)left.Value * (int)right.Value;
								return intTotal.ToString();
							case Type.FLOAT:
								var floatTotal = (float)left.Value * (float)right.Value;
								return floatTotal.ToString();
							case Type.STRING:
								// error
								break;
							case Type.UNKNOWN:
							default:
								// error
								break;
						}
						break;
					case Type.STRING:
						// error
						break;
					case Type.UNKNOWN:
					default:
						// error
						break;
				}
			}
			return String.Empty;
		}
		public static string DivLiteral(ExpressionValue left, ExpressionValue right) {
			if (left.IsLiteral && right.IsLiteral) {
				switch (left.ValueType) {
					case Type.INT:
						switch (right.ValueType) {
							case Type.INT:
								var intTotal = (int)left.Value / (int)right.Value;
								return intTotal.ToString();
							case Type.FLOAT:
								var floatTotal = (int)left.Value / (float)right.Value;
								return floatTotal.ToString();
							case Type.STRING:
								// error
								break;
							case Type.UNKNOWN:
							default:
								// error
								break;
						}
						break;
					case Type.FLOAT:
						switch (right.ValueType) {
							case Type.INT:
								var intTotal = (float)left.Value / (int)right.Value;
								return intTotal.ToString();
							case Type.FLOAT:
								var floatTotal = (float)left.Value / (float)right.Value;
								return floatTotal.ToString();
							case Type.STRING:
								// error
								break;
							case Type.UNKNOWN:
							default:
								// error
								break;
						}
						break;
					case Type.STRING:
						// error
						break;
					case Type.UNKNOWN:
					default:
						// error
						break;
				}
			}
			return String.Empty;
		}
	}
}
