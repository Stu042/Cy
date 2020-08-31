using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cy {
	class AstPrinter : Expr.IVisitor, Stmt.IVisitor {
		public string Print(Expr expr) {
			return (string)expr.Accept(this, null);
		}

		public string Print(Stmt stmt) {
			return (string)stmt.Accept(this, null);
		}


		public object VisitAssignExpr(Expr.Assign expr, object options) {
			return Parenthesize2("=", expr.token.lexeme, expr.value);
		}

		public object VisitBinaryExpr(Expr.Binary expr, object options) {
			return Parenthesize(expr.token.lexeme, expr.left, expr.right);
		}

		public object VisitBlockStmt(Stmt.Block stmt, object options) {
			StringBuilder builder = new StringBuilder();
			builder.Append("(block ");
			foreach (Stmt statement in stmt.statements) {
				builder.Append(statement.Accept(this, null));
			}
			builder.Append(")");
			return builder.ToString();
		}

		public object VisitExpressionStmt(Stmt.Expression stmt, object options) {
			return Parenthesize(";", stmt.expression);
		}

		public object VisitFunctionStmt(Stmt.Function stmt, object options) {
			StringBuilder builder = new StringBuilder();
			builder.Append("(fun " + stmt.token.lexeme + "(");
			foreach (Token param in stmt.input) {
				if (param != stmt.input[0])
					builder.Append(" ");
				builder.Append(param.lexeme);
			}
			builder.Append(") ");
			foreach (Stmt body in stmt.body)
				builder.Append(body.Accept(this, null));
			builder.Append(")");
			return builder.ToString();
		}


		public object VisitLiteralExpr(Expr.Literal expr, object options) {
			if (expr.value == null)
				return "nil";
			return expr.value.ToString();
		}

		public object VisitReturnStmt(Stmt.Return stmt, object options) {
			if (stmt.value == null)
				return "(return)";
			return Parenthesize("return", stmt.value);
		}

		public object VisitTypeStmt(Stmt.Type stmt, object options) {
			throw new NotImplementedException();
		}

		public object VisitUnaryExpr(Expr.Unary expr, object options) {
			return Parenthesize(expr.token.lexeme, expr.right);
		}

		public object VisitVariableExpr(Expr.Variable var, object options) {
			return var.token.lexeme;
		}

		public object VisitVarStmt(Stmt.Var stmt, object options) {
			if (stmt.initializer == null)
				return Parenthesize2("var", stmt.token);
			return Parenthesize2("var", stmt.token, "=", stmt.initializer);
		}



		string Parenthesize(string name, params Expr[] exprs) {
			StringBuilder builder = new StringBuilder();
			builder.Append("(").Append(name);
			foreach (Expr expr in exprs) {
				builder.Append(" ");
				builder.Append(expr.Accept(this, null));
			}
			builder.Append(")");
			return builder.ToString();
		}


		string Parenthesize2(string name, params object[] parts) {
			StringBuilder builder = new StringBuilder();
			builder.Append("(").Append(name);
			foreach (object part in parts) {
				builder.Append(" ");
				if (part is Expr expr) {
					builder.Append(expr.Accept(this, null));
				} else if (part is Stmt stmt) {
					builder.Append(stmt.Accept(this, null));
				} else if (part is Token token) {
					builder.Append(token.lexeme);
				} else {
					builder.Append(part);
				}
			}
			builder.Append(")");
			return builder.ToString();
		}

	} // AstPrinter
}
