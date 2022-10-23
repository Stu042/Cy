using Cy.Preprocesor;
using Cy.Preprocesor.Interfaces;

using System.Linq;


namespace Cy.snippet;
public class AstVisitor : IAstVisitor {
	public object VisitAssignExpr(Expr.Assign expr, object options) {
		expr.value.Accept(this, options);
		return null;
	}

	public object VisitBinaryExpr(Expr.Binary expr, object options) {
		expr.left.Accept(this, options);
		expr.right.Accept(this, options);
		return null;
	}

	public object VisitBlockStmt(Stmt.Block stmt, object options) {
		foreach (var statement in stmt.statements) {
			statement.Accept(this, options);
		}
		return null;
	}

	public object VisitCallExpr(Expr.Call expr, object options) {
		expr.callee.Accept(this, options);
		foreach (var args in expr.arguments) {
			args.Accept(this, options);
		}
		return null;
	}

	public object VisitClassStmt(Stmt.ClassDefinition stmt, object options) {
		foreach (var memb in stmt.members) {
			memb.Accept(this, options);
		}
		foreach (var memb in stmt.methods) {
			memb.Accept(this, options);
		}
		return null;
	}

	public object VisitExpressionStmt(Stmt.Expression stmt, object options) {
		stmt.expression.Accept(this, options);
		return null;
	}

	public object VisitForStmt(Stmt.For stmt, object options) {
		stmt.iteratorType.Accept(this, options);
		stmt.condition.Accept(this, options);
		foreach (var body in stmt.body) {
			body.Accept(this, options);
		}
		return null;
	}

	public object VisitFunctionStmt(Stmt.Function stmt, object options) {
		if (stmt.returnType != null) {
			stmt.returnType.Accept(this, options);
		} else {
		}
		foreach (var param in stmt.input) {
			param.Accept(this, options);
		}
		foreach (var body in stmt.body) {
			body.Accept(this, options);
		}
		return null;
	}

	public object VisitGetExpr(Expr.Get obj, object options) {
		return null;
	}

	public object VisitGroupingExpr(Expr.Grouping expr, object options) {
		expr.expression.Accept(this, options);
		return null;
	}

	public object VisitIfStmt(Stmt.If stmt, object options) {
		stmt.value.Accept(this, options);
		foreach (var body in stmt.body) {
			body.Accept(this, options);
		}
		foreach (var elseBody in stmt.elseBody) {
			elseBody.Accept(this, options);
		}
		return null;
	}

	public object VisitInputVarStmt(Stmt.InputVar invar, object options) {
		invar.type.Accept(this, options);
		return null;
	}

	public object VisitLiteralExpr(Expr.Literal expr, object options) {
		return null;
	}

	public object VisitReturnStmt(Stmt.Return stmt, object options) {
		if (stmt.value == null) {
		} else {
			stmt.value.Accept(this, options);
		}
		return null;
	}

	public object VisitSetExpr(Expr.Set expr, object options) {
		expr.obj.Accept(this, options);
		expr.value.Accept(this, options);
		return null;
	}

	public object VisitTypeStmt(Stmt.StmtType stmt, object options) {
		var names = stmt.info.Select(info => info.Lexeme);
		return string.Join('.', names);
	}

	public object VisitUnaryExpr(Expr.Unary expr, object options) {
		return null;
	}

	public object VisitVariableExpr(Expr.Variable var, object options) {
		return null;
	}

	// returns method definitions
	public object VisitVarStmt(Stmt.VarDefinition stmt, object options) {
		return null;
	}

	public object VisitWhileStmt(Stmt.While stmt, object options) {
		stmt.condition.Accept(this, options);
		foreach (var body in stmt.body) {
			body.Accept(this, options);
		}
		return null;
	}
}

