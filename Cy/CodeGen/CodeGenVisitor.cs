using Cy.Preprocesor;
using Cy.Preprocesor.Interfaces;

using System;
using System.Collections.Generic;
using System.Text;

namespace Cy.CodeGen;


public class CodeGenVisitor : IExprVisitor, IStmtVisitor {
	public string Run(List<List<Stmt>> toplevel) {
		var bob = new StringBuilder();
		foreach (var stmt in toplevel) {
			foreach (var section in stmt) {
				bob.Append(section.Accept(this, new Options()));
			}
		}
		return bob.ToString();
	}

	public class Options {
		public int CurrentTab;
		public Options() {
			CurrentTab = 0;
		}

		public string Tabs { get => new(' ', CurrentTab * 2); }
		public void IncTab() {
			CurrentTab++;
		}
		public void DecTab() {
			--CurrentTab;
		}
		public static Options GetOptions(object obj) {
			return (Options)obj;
		}
	}


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
		var opts = Options.GetOptions(options);
		var info = stmt.returnType.info;
		var bob = new StringBuilder(opts.Tabs + "define dso_local i32 @" + stmt.token.lexeme + "() #0 {\n");
		opts.IncTab();
		foreach (var body in stmt.body) {
			var line = body.Accept(this, opts);
			bob.Append(line);
		}
		opts.DecTab();
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
		string value = expr.token.tokenType switch {
			TokenType.INT_LITERAL or TokenType.FLOAT_LITERAL => expr.value.ToString(),
			TokenType.STR_LITERAL => "\"" + expr.value.ToString() + "\"",
			_ => expr.value.ToString(),
		};
		return value;
	}

	public object VisitReturnStmt(Stmt.Return stmt, object options) {
		var opts = Options.GetOptions(options);
		var bob = new StringBuilder(opts.Tabs + "ret ");
		bob.Append(stmt.value.Accept(this, opts) + "\n");
		return bob.ToString();
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
