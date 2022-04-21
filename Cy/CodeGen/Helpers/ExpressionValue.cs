using Cy.CodeGen.Llvm;
using Cy.Enums;

namespace Cy.CodeGen.Helpers;

/// <summary>Perform arithmetic on an expression, used to get the value of an expresion.</summary>
public class ExpressionValue {
	/// <summary>Is this a static value?</summary>
	public bool IsLiteral;
	/// <summary>Value in text</summary>
	public string TextRepresentation;
	/// <summary>Value in whatever type it is</summary>
	public object Value;
	/// <summary>Base type, i.e. int, float, void, etc</summary>
	public BaseType BaseType;
	/// <summary>Llvm Instance info</summary>
	public LlvmInstance Instance;


	public static BaseType GetBaseType(ExpressionValue left, ExpressionValue right) {
		return left.BaseType switch {
			BaseType.INT => right.BaseType switch {
				BaseType.INT => BaseType.INT,
				BaseType.FLOAT => BaseType.FLOAT,
				BaseType.REFERENCE => BaseType.REFERENCE,
				BaseType.OBJECT => BaseType.UNKNOWN,
				BaseType.VOID => BaseType.UNKNOWN,
				BaseType.UNKNOWN => BaseType.UNKNOWN,
				_ => BaseType.UNKNOWN
			},
			BaseType.FLOAT => right.BaseType switch {
				BaseType.INT => BaseType.FLOAT,
				BaseType.FLOAT => BaseType.FLOAT,
				BaseType.REFERENCE => BaseType.REFERENCE,
				BaseType.OBJECT => BaseType.UNKNOWN,
				BaseType.VOID => BaseType.UNKNOWN,
				BaseType.UNKNOWN => BaseType.UNKNOWN,
				_ => BaseType.UNKNOWN
			},
			BaseType.REFERENCE => right.BaseType switch {
				BaseType.INT => BaseType.REFERENCE,
				BaseType.FLOAT => BaseType.UNKNOWN,
				BaseType.REFERENCE => BaseType.REFERENCE,
				BaseType.OBJECT => BaseType.UNKNOWN,
				BaseType.VOID => BaseType.UNKNOWN,
				BaseType.UNKNOWN => BaseType.UNKNOWN,
				_ => BaseType.UNKNOWN
			},
			BaseType.OBJECT => right.BaseType switch {
				BaseType.INT => BaseType.UNKNOWN,
				BaseType.FLOAT => BaseType.UNKNOWN,
				BaseType.REFERENCE => BaseType.UNKNOWN,
				BaseType.OBJECT => BaseType.UNKNOWN,
				BaseType.VOID => BaseType.UNKNOWN,
				BaseType.UNKNOWN => BaseType.UNKNOWN,
				_ => BaseType.UNKNOWN
			},
			BaseType.VOID => right.BaseType switch {
				BaseType.INT => BaseType.UNKNOWN,
				BaseType.FLOAT => BaseType.UNKNOWN,
				BaseType.REFERENCE => BaseType.UNKNOWN,
				BaseType.OBJECT => BaseType.UNKNOWN,
				BaseType.VOID => BaseType.UNKNOWN,
				BaseType.UNKNOWN => BaseType.UNKNOWN,
				_ => BaseType.UNKNOWN
			},
			BaseType.UNKNOWN or _ => BaseType.UNKNOWN
		};
	}
	public static object AddLiteral(ExpressionValue left, ExpressionValue right) {
		if (left.IsLiteral && right.IsLiteral) {
			switch (left.BaseType) {
				case BaseType.INT:
					switch (right.BaseType) {
						case BaseType.INT: {
								var total = (int)left.Value + (int)right.Value;
								return total;
							}
						case BaseType.FLOAT: {
								var total = (int)left.Value + (double)right.Value;
								return total;
							}
						case BaseType.REFERENCE: {
								var total = (int)left.Value + (string)right.Value;
								return total;
							}
						case BaseType.OBJECT:
						case BaseType.VOID:
						case BaseType.UNKNOWN:
						default:
							// error
							break;
					}
					break;
				case BaseType.FLOAT:
					switch (right.BaseType) {
						case BaseType.INT: {
								var total = (double)left.Value + (int)right.Value;
								return total;
							}
						case BaseType.FLOAT: {
								var total = (double)left.Value + (double)right.Value;
								return total;
							}
						case BaseType.REFERENCE: {
								var total = (double)left.Value + (string)right.Value;
								return total;
							}
						case BaseType.OBJECT:
						case BaseType.VOID:
						case BaseType.UNKNOWN:
						default:
							// error
							break;
					}
					break;
				case BaseType.REFERENCE:
					switch (right.BaseType) {
						case BaseType.INT: {
								var total = (string)left.Value + (int)right.Value;
								return total;
							}
						case BaseType.FLOAT: {
								var total = (string)left.Value + (double)right.Value;
								return total;
							}
						case BaseType.REFERENCE: {
								var total = (string)left.Value + (string)right.Value;
								return total;
							}
						case BaseType.OBJECT:
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
		return string.Empty;
	}
	public static object SubLiteral(ExpressionValue left, ExpressionValue right) {
		if (left.IsLiteral && right.IsLiteral) {
			switch (left.BaseType) {
				case BaseType.INT:
					switch (right.BaseType) {
						case BaseType.INT: {
								var total = (int)left.Value - (int)right.Value;
								return total;
							}
						case BaseType.FLOAT: {
								var total = (int)left.Value - (double)right.Value;
								return total;
							}
						case BaseType.REFERENCE:
							// error
							break;
						case BaseType.OBJECT:
						case BaseType.VOID:
						case BaseType.UNKNOWN:
						default:
							// error
							break;
					}
					break;
				case BaseType.FLOAT:
					switch (right.BaseType) {
						case BaseType.INT: {
								var total = (double)left.Value - (int)right.Value;
								return total;
							}
						case BaseType.FLOAT: {
								var total = (double)left.Value - (double)right.Value;
								return total;
							}
						case BaseType.REFERENCE:
							// error
							break;
						case BaseType.OBJECT:
						case BaseType.VOID:
						case BaseType.UNKNOWN:
						default:
							// error
							break;
					}
					break;
				case BaseType.OBJECT:
				case BaseType.VOID:
				case BaseType.REFERENCE:
					// error
					break;
				case BaseType.UNKNOWN:
				default:
					// error
					break;
			}
		}
		return string.Empty;
	}
	public static object MultLiteral(ExpressionValue left, ExpressionValue right) {
		if (left.IsLiteral && right.IsLiteral) {
			switch (left.BaseType) {
				case BaseType.INT:
					switch (right.BaseType) {
						case BaseType.INT:
							var intTotal = (int)left.Value * (int)right.Value;
							return intTotal;
						case BaseType.FLOAT:
							var floatTotal = (int)left.Value * (double)right.Value;
							return floatTotal;
						case BaseType.REFERENCE:
							// error
							break;
						case BaseType.OBJECT:
						case BaseType.VOID:
						case BaseType.UNKNOWN:
						default:
							// error
							break;
					}
					break;
				case BaseType.FLOAT:
					switch (right.BaseType) {
						case BaseType.INT:
							var intTotal = (double)left.Value * (int)right.Value;
							return intTotal;
						case BaseType.FLOAT:
							var floatTotal = (double)left.Value * (double)right.Value;
							return floatTotal;
						case BaseType.REFERENCE:
							// error
							break;
						case BaseType.OBJECT:
						case BaseType.VOID:
						case BaseType.UNKNOWN:
						default:
							// error
							break;
					}
					break;
				case BaseType.OBJECT:
				case BaseType.VOID:
				case BaseType.REFERENCE:
					// error
					break;
				case BaseType.UNKNOWN:
				default:
					// error
					break;
			}
		}
		return string.Empty;
	}
	public static object DivLiteral(ExpressionValue left, ExpressionValue right) {
		if (left.IsLiteral && right.IsLiteral) {
			switch (left.BaseType) {
				case BaseType.INT:
					switch (right.BaseType) {
						case BaseType.INT:
							var intTotal = (int)left.Value / (int)right.Value;
							return intTotal;
						case BaseType.FLOAT:
							var floatTotal = (int)left.Value / (double)right.Value;
							return floatTotal;
						case BaseType.REFERENCE:
							// error
							break;
						case BaseType.OBJECT:
						case BaseType.VOID:
						case BaseType.UNKNOWN:
						default:
							// error
							break;
					}
					break;
				case BaseType.FLOAT:
					switch (right.BaseType) {
						case BaseType.INT:
							var intTotal = (double)left.Value / (int)right.Value;
							return intTotal;
						case BaseType.FLOAT:
							var floatTotal = (double)left.Value / (double)right.Value;
							return floatTotal;
						case BaseType.REFERENCE:
							// error
							break;
						case BaseType.OBJECT:
						case BaseType.VOID:
						case BaseType.UNKNOWN:
						default:
							// error
							break;
					}
					break;
				case BaseType.OBJECT:
				case BaseType.VOID:
				case BaseType.REFERENCE:
					// error
					break;
				case BaseType.UNKNOWN:
				default:
					// error
					break;
			}
		}
		return string.Empty;
	}
	public static object ModLiteral(ExpressionValue left, ExpressionValue right) {
		if (left.IsLiteral && right.IsLiteral) {
			switch (left.BaseType) {
				case BaseType.INT:
					switch (right.BaseType) {
						case BaseType.INT:
							var intTotal = (int)left.Value % (int)right.Value;
							return intTotal;
						case BaseType.FLOAT:
							var floatTotal = (int)left.Value % (double)right.Value;
							return floatTotal;
						case BaseType.REFERENCE:
							// error
							break;
						case BaseType.OBJECT:
						case BaseType.VOID:
						case BaseType.UNKNOWN:
						default:
							// error
							break;
					}
					break;
				case BaseType.FLOAT:
					switch (right.BaseType) {
						case BaseType.INT:
							var intTotal = (double)left.Value % (int)right.Value;
							return intTotal;
						case BaseType.FLOAT:
							var floatTotal = (double)left.Value % (double)right.Value;
							return floatTotal;
						case BaseType.REFERENCE:
							// error
							break;
						case BaseType.OBJECT:
						case BaseType.VOID:
						case BaseType.UNKNOWN:
						default:
							// error
							break;
					}
					break;
				case BaseType.OBJECT:
				case BaseType.VOID:
				case BaseType.REFERENCE:
					// error
					break;
				case BaseType.UNKNOWN:
				default:
					// error
					break;
			}
		}
		return string.Empty;
	}

	public static ExpressionValue CastLiteral(ExpressionValue value, BaseType requestedType) {
		switch (value.BaseType) {
			case BaseType.INT:
				var intValue = (long)value.Value;
				switch (requestedType) {
					case BaseType.INT:
						return value;
					case BaseType.FLOAT:
						return new ExpressionValue {
							IsLiteral = true,
							TextRepresentation = ((double)intValue).ToString(),
							Value = (double)intValue,
							BaseType = BaseType.FLOAT
						};
					case BaseType.REFERENCE:
						return new ExpressionValue {
							IsLiteral = true,
							TextRepresentation = intValue.ToString(),
							Value = intValue.ToString(),
							BaseType = BaseType.REFERENCE
						};
					case BaseType.OBJECT:
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
							TextRepresentation = ((int)floatValue).ToString(),
							Value = (int)floatValue,
							BaseType = BaseType.INT
						};
					case BaseType.FLOAT:
						return value;
					case BaseType.REFERENCE:
						return new ExpressionValue {
							IsLiteral = true,
							TextRepresentation = floatValue.ToString(),
							Value = floatValue.ToString(),
							BaseType = BaseType.REFERENCE
						};
					case BaseType.OBJECT:
					case BaseType.VOID:
					case BaseType.UNKNOWN:
					default:
						// error
						break;
				}
				break;
			case BaseType.OBJECT:
			case BaseType.VOID:
			case BaseType.REFERENCE:
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

