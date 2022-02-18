
using Cy.Enums;

using System;

namespace Cy.CodeGen;

public partial class CodeGenVisitor {
	public class ExpressionValue {
		public bool IsLiteral;
		public string TextValue;
		public object Value;
		public BaseType ValueType;


		public static BaseType GetType(ExpressionValue left, ExpressionValue right) {
			return left.ValueType switch {
				BaseType.INT => right.ValueType switch {
					BaseType.INT => BaseType.INT,
					BaseType.FLOAT => BaseType.FLOAT,
					BaseType.STRING => BaseType.STRING,
					BaseType.UNKNOWN or _ => BaseType.UNKNOWN
				},
				BaseType.FLOAT => right.ValueType switch {
					BaseType.INT => BaseType.FLOAT,
					BaseType.FLOAT => BaseType.FLOAT,
					BaseType.STRING => BaseType.STRING,
					BaseType.UNKNOWN or _ => BaseType.UNKNOWN
				},
				BaseType.STRING => right.ValueType switch {
					BaseType.INT => BaseType.STRING,
					BaseType.FLOAT => BaseType.STRING,
					BaseType.STRING => BaseType.STRING,
					BaseType.UNKNOWN or _ => BaseType.UNKNOWN
				},
				BaseType.UNKNOWN or _ => BaseType.UNKNOWN
			};
		}
		public static object AddLiteral(ExpressionValue left, ExpressionValue right) {
			if (left.IsLiteral && right.IsLiteral) {
				switch (left.ValueType) {
					case BaseType.INT:
						switch (right.ValueType) {
							case BaseType.INT: {
									var total = (int)left.Value + (int)right.Value;
									return total;
								}
							case BaseType.FLOAT: {
									var total = (int)left.Value + (double)right.Value;
									return total;
								}
							case BaseType.STRING: {
									var total = (int)left.Value + (string)right.Value;
									return total;
								}
							case BaseType.VOID:
							case BaseType.UNKNOWN:
							default:
								// error
								break;
						}
						break;
					case BaseType.FLOAT:
						switch (right.ValueType) {
							case BaseType.INT: {
									var total = (double)left.Value + (int)right.Value;
									return total;
								}
							case BaseType.FLOAT: {
									var total = (double)left.Value + (double)right.Value;
									return total;
								}
							case BaseType.STRING: {
									var total = (double)left.Value + (string)right.Value;
									return total;
								}
							case BaseType.VOID:
							case BaseType.UNKNOWN:
							default:
								// error
								break;
						}
						break;
					case BaseType.STRING:
						switch (right.ValueType) {
							case BaseType.INT: {
									var total = (string)left.Value + (int)right.Value;
									return total;
								}
							case BaseType.FLOAT: {
									var total = (string)left.Value + (double)right.Value;
									return total;
								}
							case BaseType.STRING: {
									var total = (string)left.Value + (string)right.Value;
									return total;
								}
							case BaseType.VOID:
							case BaseType.UNKNOWN:
							default:
								// error
								break;
						}
						break;
					case BaseType.UNKNOWN:
					default:
						// error
						break;
				}
			}
			return String.Empty;
		}
		public static object SubLiteral(ExpressionValue left, ExpressionValue right) {
			if (left.IsLiteral && right.IsLiteral) {
				switch (left.ValueType) {
					case BaseType.INT:
						switch (right.ValueType) {
							case BaseType.INT: {
									var total = (int)left.Value - (int)right.Value;
									return total;
								}
							case BaseType.FLOAT: {
									var total = (int)left.Value - (double)right.Value;
									return total;
								}
							case BaseType.STRING:
								// error
								break;
							case BaseType.VOID:
							case BaseType.UNKNOWN:
							default:
								// error
								break;
						}
						break;
					case BaseType.FLOAT:
						switch (right.ValueType) {
							case BaseType.INT: {
									var total = (double)left.Value - (int)right.Value;
									return total;
								}
							case BaseType.FLOAT: {
									var total = (double)left.Value - (double)right.Value;
									return total;
								}
							case BaseType.STRING:
								// error
								break;
							case BaseType.VOID:
							case BaseType.UNKNOWN:
							default:
								// error
								break;
						}
						break;
					case BaseType.VOID:
					case BaseType.STRING:
						// error
						break;
					case BaseType.UNKNOWN:
					default:
						// error
						break;
				}
			}
			return String.Empty;
		}
		public static object MultLiteral(ExpressionValue left, ExpressionValue right) {
			if (left.IsLiteral && right.IsLiteral) {
				switch (left.ValueType) {
					case BaseType.INT:
						switch (right.ValueType) {
							case BaseType.INT:
								var intTotal = (int)left.Value * (int)right.Value;
								return intTotal;
							case BaseType.FLOAT:
								var floatTotal = (int)left.Value * (double)right.Value;
								return floatTotal;
							case BaseType.STRING:
								// error
								break;
							case BaseType.VOID:
							case BaseType.UNKNOWN:
							default:
								// error
								break;
						}
						break;
					case BaseType.FLOAT:
						switch (right.ValueType) {
							case BaseType.INT:
								var intTotal = (double)left.Value * (int)right.Value;
								return intTotal;
							case BaseType.FLOAT:
								var floatTotal = (double)left.Value * (double)right.Value;
								return floatTotal;
							case BaseType.STRING:
								// error
								break;
							case BaseType.VOID:
							case BaseType.UNKNOWN:
							default:
								// error
								break;
						}
						break;
					case BaseType.VOID:
					case BaseType.STRING:
						// error
						break;
					case BaseType.UNKNOWN:
					default:
						// error
						break;
				}
			}
			return String.Empty;
		}
		public static object DivLiteral(ExpressionValue left, ExpressionValue right) {
			if (left.IsLiteral && right.IsLiteral) {
				switch (left.ValueType) {
					case BaseType.INT:
						switch (right.ValueType) {
							case BaseType.INT:
								var intTotal = (int)left.Value / (int)right.Value;
								return intTotal;
							case BaseType.FLOAT:
								var floatTotal = (int)left.Value / (double)right.Value;
								return floatTotal;
							case BaseType.STRING:
								// error
								break;
							case BaseType.VOID:
							case BaseType.UNKNOWN:
							default:
								// error
								break;
						}
						break;
					case BaseType.FLOAT:
						switch (right.ValueType) {
							case BaseType.INT:
								var intTotal = (double)left.Value / (int)right.Value;
								return intTotal;
							case BaseType.FLOAT:
								var floatTotal = (double)left.Value / (double)right.Value;
								return floatTotal;
							case BaseType.STRING:
								// error
								break;
							case BaseType.VOID:
							case BaseType.UNKNOWN:
							default:
								// error
								break;
						}
						break;
					case BaseType.VOID:
					case BaseType.STRING:
						// error
						break;
					case BaseType.UNKNOWN:
					default:
						// error
						break;
				}
			}
			return String.Empty;
		}
		public static object ModLiteral(ExpressionValue left, ExpressionValue right) {
			if (left.IsLiteral && right.IsLiteral) {
				switch (left.ValueType) {
					case BaseType.INT:
						switch (right.ValueType) {
							case BaseType.INT:
								var intTotal = (int)left.Value % (int)right.Value;
								return intTotal;
							case BaseType.FLOAT:
								var floatTotal = (int)left.Value % (double)right.Value;
								return floatTotal;
							case BaseType.STRING:
								// error
								break;
							case BaseType.VOID:
							case BaseType.UNKNOWN:
							default:
								// error
								break;
						}
						break;
					case BaseType.FLOAT:
						switch (right.ValueType) {
							case BaseType.INT:
								var intTotal = (double)left.Value % (int)right.Value;
								return intTotal;
							case BaseType.FLOAT:
								var floatTotal = (double)left.Value % (double)right.Value;
								return floatTotal;
							case BaseType.STRING:
								// error
								break;
							case BaseType.VOID:
							case BaseType.UNKNOWN:
							default:
								// error
								break;
						}
						break;
					case BaseType.VOID:
					case BaseType.STRING:
						// error
						break;
					case BaseType.UNKNOWN:
					default:
						// error
						break;
				}
			}
			return String.Empty;
		}

		public static ExpressionValue CastLiteral(ExpressionValue value, BaseType requestedType) {
			switch (value.ValueType) {
				case BaseType.INT:
					var intValue = (Int64)value.Value;
					switch (requestedType) {
						case BaseType.INT:
							return value;
						case BaseType.FLOAT:
							return new ExpressionValue {
								IsLiteral = true,
								TextValue = ((double)intValue).ToString(),
								Value = (double)intValue,
								ValueType = BaseType.FLOAT
							};
						case BaseType.STRING:
							return new ExpressionValue {
								IsLiteral = true,
								TextValue = intValue.ToString(),
								Value = intValue.ToString(),
								ValueType = BaseType.STRING
							};
						case BaseType.VOID:
						case BaseType.UNKNOWN:
						default:
							// error
							break;
					}
					break;
				case BaseType.FLOAT:
					var floatValue = (double)value.Value;
					switch (requestedType) {
						case BaseType.INT:
							return new ExpressionValue {
								IsLiteral = true,
								TextValue = ((int)floatValue).ToString(),
								Value = (int)floatValue,
								ValueType = BaseType.INT
							};
						case BaseType.FLOAT:
							return value;
						case BaseType.STRING:
							return new ExpressionValue {
								IsLiteral = true,
								TextValue = floatValue.ToString(),
								Value = floatValue.ToString(),
								ValueType = BaseType.STRING
							};
						case BaseType.VOID:
						case BaseType.UNKNOWN:
						default:
							// error
							break;
					}
					break;
				case BaseType.VOID:
				case BaseType.STRING:
					// error
					break;
				case BaseType.UNKNOWN:
				default:
					// error
					break;
			}
			return null;
		}
	}
}
