using Cy.Preprocesor;
using Cy.Preprocesor.Interfaces;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LLVMSharp;

namespace Cy.CodeGen;

public class CodeGenVisitor : IExprVisitor, IStmtVisitor {
	public object VisitAssignExpr(Expr.Assign expr, object options) {
		throw new NotImplementedException();
	}

	public object VisitBinaryExpr(Expr.Binary expr, object options) {
		throw new NotImplementedException();
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
		throw new NotImplementedException();
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
		throw new NotImplementedException();
	}

	public object VisitReturnStmt(Stmt.Return stmt, object options) {
		throw new NotImplementedException();
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
