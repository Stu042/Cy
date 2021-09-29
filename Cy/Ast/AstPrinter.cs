using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cy.Ast {
	class Printer : Expr.IVisitor, Ast.Stmt.IVisitor {

		public void DisplayAllAsts(List<List<Ast.Stmt>> allFilesStmts) {
			Console.WriteLine("\n\nAST:");
			foreach (var stmts in allFilesStmts) {
				foreach (var stmt in stmts) {
					Console.WriteLine(new Ast.Printer().Print(stmt));
				}
			}
		}

		public string Print(Expr expr) {
			return (string)expr.Accept(this, null);
		}

		public string Print(Ast.Stmt stmt) {
			return (string)stmt.Accept(this, null);
		}


		public object VisitAssignExpr(Expr.Assign expr, object options) {
			return Parenthesize2("=", expr.token.lexeme, expr.value);
		}

		public object VisitBinaryExpr(Expr.Binary expr, object options) {
			return Parenthesize(expr.token.lexeme, expr.left, expr.right);
		}

		public object VisitBlockStmt(Ast.Stmt.Block stmt, object options) {
			StringBuilder builder = new StringBuilder();
			builder.Append("(block ");
			foreach (Ast.Stmt statement in stmt.statements)
				builder.Append(statement.Accept(this, null));
			builder.Append(")");
			return builder.ToString();
		}

		public object VisitExpressionStmt(Ast.Stmt.Expression stmt, object options) {
			return Parenthesize(".", stmt.expression);
		}


		public object VisitInputVarStmt(Ast.Stmt.InputVar invar, object options) {
			return invar.type.token.lexeme + " " + invar.token.lexeme;
		}

		public object VisitFunctionStmt(Ast.Stmt.Function stmt, object options) {
			StringBuilder builder = new StringBuilder();
			string typestr;
			if (stmt.returnType != null)
				typestr = (string)stmt.returnType.Accept(this, null);
			else
				typestr = "void";
			builder.Append("(" + typestr + " " + stmt.token.lexeme + "(");
			foreach (var param in stmt.input) {
				if (param != stmt.input[0])
					builder.Append(", ");
				builder.Append(param.Accept(this, null));
			}
			builder.Append(") ");
			foreach (Ast.Stmt body in stmt.body)
				builder.Append(body.Accept(this, null));
			builder.Append(")");
			return builder.ToString();
		}

		public object VisitClassStmt(Ast.Stmt.ClassDefinition obj, object options) {
			StringBuilder builder = new StringBuilder();
			builder.Append("(" + obj.token.lexeme + ": ");
			List<string> memberStr = new List<string>();
			foreach (Ast.Stmt.Var memb in obj.members)
				memberStr.Add((string)memb.Accept(this, null));
			builder.Append(Parenthesize2("members: ", memberStr.ToArray()));
			List<string> methodStr = new List<string>();
			foreach (Ast.Stmt.Function memb in obj.methods)
				methodStr.Add((string)memb.Accept(this, null));
			builder.Append(Parenthesize2("methods: ", methodStr.ToArray()));
			builder.Append(")");
			return builder.ToString();
		}


		public object VisitLiteralExpr(Expr.Literal expr, object options) {
			if (expr.value == null)
				return "nil";
			return expr.value.ToString();
		}

		public object VisitSetExpr(Expr.Set expr, object options) {
			return Parenthesize2("=", expr.obj, expr.token.lexeme, expr.value);
		}

		public object VisitReturnStmt(Ast.Stmt.Return stmt, object options) {
			if (stmt.value == null)
				return "(return)";
			return Parenthesize("return", stmt.value);
		}

		public object VisitTypeStmt(Ast.Stmt.StmtType stmt, object options) {
			return stmt.token.lexeme;
		}

		public object VisitUnaryExpr(Expr.Unary expr, object options) {
			return Parenthesize(expr.token.lexeme, expr.right);
		}

		public object VisitVariableExpr(Expr.Variable var, object options) {
			return var.token.lexeme;
		}

		public object VisitVarStmt(Ast.Stmt.Var stmt, object options) {
			string typestr = (string)stmt.stmtType.Accept(this, null);
			if (stmt.initialiser == null)
				return Parenthesize2(typestr, stmt.token);
			return Parenthesize2(typestr, stmt.token, "=", stmt.initialiser);
		}

		public object VisitGetExpr(Expr.Get expr, object options) {
			return Parenthesize2(".", expr.obj, expr.token.lexeme);
		}

		public object VisitCallExpr(Expr.Call expr, object options) {
			return Parenthesize2("call", expr.callee, Parenthesize("args: ", expr.arguments.ToArray()));
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
				} else if (part is Ast.Stmt stmt) {
					builder.Append(stmt.Accept(this, null));
				} else if (part is Token token) {
					builder.Append(token.lexeme);
				} else if (part != null) {
					builder.Append(part);
				}
			}
			builder.Append(")");
			return builder.ToString();
		}

	} // AstPrinter
}
