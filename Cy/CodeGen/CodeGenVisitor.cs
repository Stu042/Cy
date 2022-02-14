using Cy.Enums;
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
		var left = (ExpressionValue)expr.left.Accept(this, options);
		var right = (ExpressionValue)expr.right.Accept(this, options);
		var value = expr.token.tokenType switch {
			TokenType.PLUS => (left.IsLiteral && right.IsLiteral) ? ExpressionValue.AddLiteral(left, right) : "",
			TokenType.MINUS => (left.IsLiteral && right.IsLiteral) ? ExpressionValue.SubLiteral(left, right) : "",
			TokenType.STAR => (left.IsLiteral && right.IsLiteral) ? ExpressionValue.MultLiteral(left, right) : "",
			TokenType.SLASH => (left.IsLiteral && right.IsLiteral) ? ExpressionValue.DivLiteral(left, right) : "",
			TokenType.PERCENT => (left.IsLiteral && right.IsLiteral) ? ExpressionValue.ModLiteral(left, right) : "",
			_ => ""
		};
		return new ExpressionValue {
			IsLiteral = left.IsLiteral && right.IsLiteral,
			TextValue = value.ToString(),
			Value = value,
			ValueType = ExpressionValue.GetType(left, right)
		};
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
				TokenType.INT_LITERAL => BaseType.INT,
				TokenType.FLOAT_LITERAL => BaseType.FLOAT,
				TokenType.STR_LITERAL => BaseType.STRING,
				_ => BaseType.UNKNOWN
			}
		};
	}

	public object VisitReturnStmt(Stmt.Return stmt, object options) {
		var opts = Options.GetOptions(options);
		var expressionValue = (ExpressionValue)stmt.value.Accept(this, opts);
		expressionValue = ExpressionValue.CastLiteral(expressionValue, opts.ReturnType.Peek().TypeDef.BaseType);
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
