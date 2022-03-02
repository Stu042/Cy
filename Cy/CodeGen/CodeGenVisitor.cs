using Cy.CodeGen.Llvm;
using Cy.Enums;
using Cy.Parsing;
using Cy.Parsing.Interfaces;
using Cy.Setup;
using Cy.TokenGenerator;
using Cy.Types;

using System;
using System.Collections.Generic;
using System.Text;

namespace Cy.CodeGen;



public partial class CodeGenVisitor : IExprVisitor, IStmtVisitor {
	public string Run(Stmt[][] toplevel, TypeDefinitionTable typedefTable, Config conf) {
		var opts = new Options(typedefTable, conf);
		foreach (var stmt in toplevel) {
			foreach (var section in stmt) {
				section.Accept(this, opts);
			}
		}
		return opts.Code.Code();
	}


	public object VisitAssignExpr(Expr.Assign expr, object options) {
		throw new NotImplementedException();
	}

	public object VisitBinaryExpr(Expr.Binary expr, object options) {
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
			ValueType = ExpressionValue.GetBaseType(left, right)
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
		opts.Code.NewFunction();

		var returnType = opts.TypesToLlvm.GetInstance(stmt.returnType.info);
		opts.ReturnType.Push(returnType);
		opts.Code.Allocate(opts.Tab.Show + "define dso_local " + returnType.LlvmType + " @" + stmt.token.lexeme + "() #0 {");
		opts.Tab.Inc();
		foreach (var body in stmt.body) {
			body.Accept(this, opts);
		}
		var poppedType = opts.ReturnType.Pop();
		opts.Tab.Dec();
		if (poppedType != returnType) {
			// error, wrong type
		}
		opts.Code.Build("}");
		opts.Code.EndFunction();
		return opts;
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
				TokenType.STR_LITERAL => BaseType.REFERENCE,
				_ => BaseType.UNKNOWN
			}
		};
	}

	public object VisitReturnStmt(Stmt.Return stmt, object options) {
		var opts = Options.GetOptions(options);
		var expressionValue = (ExpressionValue)stmt.value.Accept(this, opts);
		expressionValue = ExpressionValue.CastLiteral(expressionValue, opts.ReturnType.Peek().TypeDef.BaseType);
		opts.Code.Build($"{opts.Tab.Show}ret {opts.ReturnType.Peek().LlvmType} {expressionValue.TextValue}");
		return opts;
	}

	public object VisitSetExpr(Expr.Set expr, object options) {
		throw new NotImplementedException();
	}

	public object VisitTypeStmt(Stmt.StmtType stmt, object options) {
		var opts = Options.GetOptions(options);
		var llvmType = opts.TypesToLlvm.GetInstance(stmt.info);
		return llvmType;
	}

	public object VisitUnaryExpr(Expr.Unary expr, object options) {
		throw new NotImplementedException();
	}

	public object VisitVariableExpr(Expr.Variable var, object options) {

		throw new NotImplementedException();
	}

	public object VisitVarStmt(Stmt.Var stmt, object options) {
		var opts = Options.GetOptions(options);
		var stmtType = (LlvmInstance)stmt.stmtType.Accept(this, options);
		var exprValue = (ExpressionValue)stmt.initialiser.Accept(this, options);
		// create new instance of type stmtType equal to exprValue
		throw new NotImplementedException();
	}

	public object VisitWhileStmt(Stmt.While stmt, object options) {
		throw new NotImplementedException();
	}
}
