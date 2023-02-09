using Cy.Preprocesor;
using Cy.Preprocesor.Interfaces;
using Cy.Types;

using System;
using System.Linq;


namespace Cy.Llvm.CodeGen.CompileVisitor;


public partial class CompileVisitor : IExprVisitor, IStmtVisitor {

	public object VisitAssignExpr(Expr.Assign expr, object options) {
		var helper = options as CompileOptions;
		expr.value.Accept(this, options);
		return null;
	}

	public object VisitBinaryExpr(Expr.Binary expr, object options) {
		var helper = options as CompileOptions;
		expr.left.Accept(this, options);
		expr.right.Accept(this, options);
		return null;
	}

	public object VisitCallExpr(Expr.Call expr, object options) {
		var helper = options as CompileOptions;
		expr.callee.Accept(this, options);
		foreach (var args in expr.arguments) {
			args.Accept(this, options);
		}
		return null;
	}

	public object VisitSetExpr(Expr.Set expr, object options) {
		var helper = options as CompileOptions;
		return null;
	}

	public object VisitUnaryExpr(Expr.Unary expr, object options) {
		var helper = options as CompileOptions;
		return null;
	}

	public object VisitVariableExpr(Expr.Variable expr, object options) {
		var helper = options as CompileOptions;
		// create new ExpressionInstance
		return null;    // return ExpressionInstance
	}

	public object VisitGetExpr(Expr.Get expr, object options) {
		var helper = options as CompileOptions;
		var exprGet = expr.obj.Accept(this, options);
		return exprGet;
	}

	public object VisitGroupingExpr(Expr.Grouping expr, object options) {
		var helper = options as CompileOptions;
		var exprGroup = expr.expression.Accept(this, options);
		return exprGroup;
	}
}
