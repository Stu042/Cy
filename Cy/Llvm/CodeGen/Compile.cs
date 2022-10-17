using Cy.Preprocesor;
using Cy.Preprocesor.Interfaces;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace Cy.Llvm.CodeGen;
public class CompileVisitor : IExprVisitor, IStmtVisitor {

	public void Compile(List<List<Stmt>> allFilesStmts) {
		foreach (var stmts in allFilesStmts) {
			foreach (var stmt in stmts) {
				stmt.Accept(this, null);
			}
		}
	}



	public object VisitAssignExpr(Expr.Assign expr, object options) {
		expr.value.Accept(this, options);
		return null;
	}

	public object VisitBinaryExpr(Expr.Binary expr, object options) {
		return null;
	}

	public object VisitBlockStmt(Stmt.Block stmt, object options) {
		foreach (Stmt statement in stmt.statements) {
			statement.Accept(this, options);
		}
		return null;
	}

	public object VisitExpressionStmt(Stmt.Expression stmt, object options) {
		return null;
	}


	public object VisitInputVarStmt(Stmt.InputVar invar, object options) {
		return null;
	}

	public object VisitFunctionStmt(Stmt.Function stmt, object options) {
		if (stmt.returnType != null) {
		} else {
		}
		foreach (var param in stmt.input) {
			param.Accept(this, options);
		}
		foreach (Stmt body in stmt.body) {
			body.Accept(this, options);
		}
		return null;
	}

	public object VisitClassStmt(Stmt.ClassDefinition obj, object options) {
		foreach (var memb in obj.members) {
			memb.Accept(this, options);
		}
		foreach (var memb in obj.methods) {
			memb.Accept(this, options);
		}
		return null;
	}


	public object VisitLiteralExpr(Expr.Literal expr, object options) {
		if (expr.value == null) {
			return "null";
		}
		return null;
	}

	public object VisitSetExpr(Expr.Set expr, object options) {
		return null;
	}

	public object VisitReturnStmt(Stmt.Return stmt, object options) {
		if (stmt.value == null) {
		}
		return null;
	}

	public object VisitTypeStmt(Stmt.StmtType stmt, object options) {
		return null;
	}

	public object VisitUnaryExpr(Expr.Unary expr, object options) {
		return null;
	}

	public object VisitVariableExpr(Expr.Variable var, object options) {
		return null;
	}

	public object VisitVarStmt(Stmt.VarDefinition stmt, object options) {
		return null;
	}

	public object VisitGetExpr(Expr.Get expr, object options) {
		return null;
	}

	public object VisitCallExpr(Expr.Call expr, object options) {
		return null;
	}

	public object VisitIfStmt(Stmt.If stmt, object options) {
		return null;
	}

	public object VisitForStmt(Stmt.For stmt, object options) {
		throw new NotImplementedException();
	}

	public object VisitWhileStmt(Stmt.While stmt, object options) {
		throw new NotImplementedException();
	}

	public object VisitGroupingExpr(Expr.Grouping expr, object options) {
		throw new NotImplementedException();
	}

}
