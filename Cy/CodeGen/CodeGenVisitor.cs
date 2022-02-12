using Cy.Preprocesor;
using Cy.Preprocesor.Interfaces;

using System;
using System.Collections.Generic;
using System.Text;

namespace Cy.CodeGen;



public partial class CodeGenVisitor : IExprVisitor, IStmtVisitor {
	public string Run(List<List<Stmt>> toplevel, LlvmTypes llvmTypes) {
		var text = new List<string>();
		foreach (var stmt in toplevel) {
			foreach (var section in stmt) {
				text.Add((string)section.Accept(this, new Options(llvmTypes)));
			}
		}
		return String.Join('\n', text);
	}


	public object VisitAssignExpr(Expr.Assign expr, object options) {
		throw new NotImplementedException();
	}

	public object VisitBinaryExpr(Expr.Binary expr, object options) {
		var opts = Options.GetOptions(options);
		var bob = new StringBuilder();
		var left = (ExpressionValue)expr.left.Accept(this, options);
		var right = (ExpressionValue)expr.right.Accept(this, options);
		switch (expr.token.tokenType) {
			case TokenType.PLUS: {
					if (left.IsLiteral && right.IsLiteral) {
						return new ExpressionValue {
							IsLiteral = true,
							TextValue = ExpressionValue.AddLiteral(left, right),
							Value = null,
							ValueType = ExpressionValue.GetType(left, right)
						};
					}
				}
				break;
			case TokenType.MINUS: {
					if (left.IsLiteral && right.IsLiteral) {
						return new ExpressionValue {
							IsLiteral = true,
							TextValue = ExpressionValue.SubLiteral(left, right),
							Value = null,
							ValueType = ExpressionValue.GetType(left, right)
						};
					}
				}
				break;
			case TokenType.STAR: {
					if (left.IsLiteral && right.IsLiteral) {
						return new ExpressionValue {
							IsLiteral = true,
							TextValue = ExpressionValue.MultLiteral(left, right),
							Value = null,
							ValueType = ExpressionValue.GetType(left, right)
						};
					}
				}
				break;
			case TokenType.SLASH: {
					if (left.IsLiteral && right.IsLiteral) {
						return new ExpressionValue {
							IsLiteral = true,
							TextValue = ExpressionValue.DivLiteral(left, right),
							Value = null,
							ValueType = ExpressionValue.GetType(left, right)
						};
					}
				}
				break;
			default:
				// error
				break;
		}
		return bob.ToString();
	}

	public object VisitBlockStmt(Stmt.Block stmt, object options) {
		throw new NotImplementedException();
	}

	public object VisitCallExpr(Expr.Call expr, object options) {
		throw new NotImplementedException();
	}

	public object VisitClassStmt(Stmt.ClassDefinition stmt, object options) {
		throw new NotImplementedException();
	}

	public object VisitExpressionStmt(Stmt.Expression stmt, object options) {
		throw new NotImplementedException();
	}

	public object VisitForStmt(Stmt.For stmt, object options) {
		throw new NotImplementedException();
	}

	public object VisitFunctionStmt(Stmt.Function stmt, object options) {
		var opts = Options.GetOptions(options);
		var info = stmt.returnType.info;
		var returnType = opts.TypesToLlvm.GetType(stmt.returnType.info);
		opts.ReturnType.Push(returnType);
		var bob = new StringBuilder(opts.Tabs + "define dso_local " + returnType.LlvmTypeName + " @" + stmt.token.lexeme + "() #0 {\n");
		opts.IncTab();
		foreach (var body in stmt.body) {
			var line = body.Accept(this, opts);
			bob.Append(line);
		}
		var poppedType = opts.ReturnType.Pop();
		opts.DecTab();
		if (poppedType != returnType) {
			// error, wrong type
		}
		bob.AppendLine("}");
		return bob.ToString();
	}

	public object VisitGetExpr(Expr.Get obj, object options) {
		throw new NotImplementedException();
	}

	public object VisitGroupingExpr(Expr.Grouping expr, object options) {
		throw new NotImplementedException();
	}

	public object VisitIfStmt(Stmt.If stmt, object options) {
		throw new NotImplementedException();
	}

	public object VisitInputVarStmt(Stmt.InputVar invar, object options) {
		throw new NotImplementedException();
	}

	public object VisitLiteralExpr(Expr.Literal expr, object options) {
		var opts = Options.GetOptions(options);
		return new ExpressionValue {
			TextValue = expr.token.tokenType switch {
				TokenType.INT_LITERAL or TokenType.FLOAT_LITERAL => expr.value.ToString(),
				TokenType.STR_LITERAL => "\"" + expr.value.ToString() + "\"",
				_ => expr.value.ToString()
			},
			IsLiteral = true,
			Value = expr.value,
			ValueType = expr.token.tokenType switch {
				TokenType.INT_LITERAL => ExpressionValue.Type.INT,
				TokenType.FLOAT_LITERAL => ExpressionValue.Type.FLOAT,
				TokenType.STR_LITERAL => ExpressionValue.Type.STRING,
				_ => ExpressionValue.Type.UNKNOWN
			}
		};
	}

	public object VisitReturnStmt(Stmt.Return stmt, object options) {
		var opts = Options.GetOptions(options);
		var expressionValue = (ExpressionValue)stmt.value.Accept(this, opts);
		return $"{opts.Tabs}ret {opts.ReturnType.Peek().LlvmTypeName} {expressionValue.TextValue}\n";
	}

	public object VisitSetExpr(Expr.Set expr, object options) {
		throw new NotImplementedException();
	}

	public object VisitTypeStmt(Stmt.StmtType stmt, object options) {
		throw new NotImplementedException();
	}

	public object VisitUnaryExpr(Expr.Unary expr, object options) {
		throw new NotImplementedException();
	}

	public object VisitVariableExpr(Expr.Variable var, object options) {
		throw new NotImplementedException();
	}

	public object VisitVarStmt(Stmt.Var stmt, object options) {
		throw new NotImplementedException();
	}

	public object VisitWhileStmt(Stmt.While stmt, object options) {
		throw new NotImplementedException();
	}
}
