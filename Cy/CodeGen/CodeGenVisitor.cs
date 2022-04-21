using Cy.CodeGen.Helpers;
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
		var opts = Options.Get(options);
		var left = (ExpressionValue)expr.left.Accept(this, options);
		var right = (ExpressionValue)expr.right.Accept(this, options);
		object value;
		switch (expr.Token.tokenType) {
			case TokenType.PLUS:
				value = (left.IsLiteral && right.IsLiteral) ? ExpressionValue.AddLiteral(left, right) : "";
				break;
			case TokenType.MINUS:
				value = (left.IsLiteral && right.IsLiteral) ? ExpressionValue.SubLiteral(left, right) : "";
				break;
			case TokenType.STAR:
				value = (left.IsLiteral && right.IsLiteral) ? ExpressionValue.MultLiteral(left, right) : "";
				break;
			case TokenType.SLASH:
				value = (left.IsLiteral && right.IsLiteral) ? ExpressionValue.DivLiteral(left, right) : "";
				break;
			case TokenType.PERCENT:
				value = (left.IsLiteral && right.IsLiteral) ? ExpressionValue.ModLiteral(left, right) : "";
				break;
			default:
				value = "";
				break;
		};
		var instance = new LlvmInstance {
			FullyQualifiedTypeName = left.Instance.FullyQualifiedTypeName + expr.Token.lexeme + right.Instance.FullyQualifiedTypeName,
			LlvmName = value.ToString(),
			LlvmType = null,
			TypeDef = left.Instance.TypeDef
		};
		return new ExpressionValue {
			IsLiteral = left.IsLiteral && right.IsLiteral,
			TextRepresentation = value.ToString(),
			Value = value,
			BaseType = ExpressionValue.GetBaseType(left, right),
			Instance = instance
		};
	}

	public object VisitBlockStmt(Stmt.Block stmt, object options) {
		var opts = Options.Get(options);
		opts.LlvmInstance.NewBlock();

		opts.LlvmInstance.EndBlock();
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
		var opts = Options.Get(options);
		opts.Code.NewFunction();
		opts.LlvmInstance.NewBlock();

		var returnType = opts.LlvmInstance.GetInstance(stmt.returnType.info);
		opts.ReturnType.Push(returnType);
		opts.Code.Allocate($"{opts.Tab.Show}define dso_local {returnType.LlvmType} @{stmt.Token.lexeme} () #0 {{");
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
		opts.LlvmInstance.EndBlock();
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
			TextRepresentation = expr.Token.tokenType switch {
				TokenType.INT_LITERAL or TokenType.FLOAT_LITERAL => expr.value.ToString(),
				TokenType.STR_LITERAL => "\"" + expr.value.ToString() + "\"",
				_ => expr.value.ToString()
			},
			IsLiteral = true,
			Value = expr.value,
			BaseType = expr.Token.tokenType switch {
				TokenType.INT_LITERAL => BaseType.INT,
				TokenType.FLOAT_LITERAL => BaseType.FLOAT,
				TokenType.STR_LITERAL => BaseType.REFERENCE,
				_ => BaseType.UNKNOWN
			},
			Instance = new LlvmInstance {
				FullyQualifiedTypeName = expr.Token.lexeme,
				LlvmName = expr.value.ToString(),
				LlvmType = null
			}
		};
	}

	public object VisitReturnStmt(Stmt.Return stmt, object options) {
		var opts = Options.Get(options);
		var expressionValue = (ExpressionValue)stmt.value.Accept(this, opts);
		expressionValue = ExpressionValue.CastLiteral(expressionValue, opts.ReturnType.Peek().TypeDef.BaseType);
		opts.Code.Build($"{opts.Tab.Show}ret {opts.ReturnType.Peek().LlvmType} {expressionValue.TextRepresentation}");
		return opts;
	}

	public object VisitSetExpr(Expr.Set expr, object options) {
		throw new NotImplementedException();
	}

	public object VisitTypeStmt(Stmt.StmtType stmt, object options) {
		var opts = Options.Get(options);
		var instance = opts.LlvmInstance.GetInstance(stmt.info);
		return instance;
	}

	public object VisitUnaryExpr(Expr.Unary expr, object options) {
		throw new NotImplementedException();
	}

	public object VisitVariableExpr(Expr.Variable var, object options) {

		throw new NotImplementedException();
	}

	public object VisitVarStmt(Stmt.Var stmt, object options) {
		var opts = Options.Get(options);
		var instance = (LlvmInstance)stmt.stmtType.Accept(this, options);
		opts.Code.Allocate($"{opts.Tab.Show}{instance.LlvmName} = alloca {instance.LlvmType}, align {opts.Conf.DefaultAlignment}");
		var exprValue = (ExpressionValue)stmt.initialiser.Accept(this, options);
		opts.Code.Assign($"{opts.Tab.Show}store {instance.LlvmType} {exprValue.TextRepresentation}, {instance.LlvmType}* {instance.LlvmName}, align {opts.Conf.DefaultAlignment}");
		return opts;
	}

	public object VisitWhileStmt(Stmt.While stmt, object options) {
		throw new NotImplementedException();
	}
}
