using Cy.Parsing.Interfaces;
using Cy.TokenGenerator;

using System;
using System.Collections.Generic;
using System.Text;


namespace Cy.Parsing;

public static class Asts {
	public static void Display(Stmt[][] allFilesStmts) {
		new AstPrinter().DisplayAllAsts(allFilesStmts);
	}

	protected class AstPrinter : IExprVisitor, IStmtVisitor {

		public void DisplayAllAsts(Stmt[][] allFilesStmts) {
			Console.WriteLine("\nAST:");
			foreach (var stmts in allFilesStmts) {
				DisplayAsts(stmts);
			}
		}

		public void DisplayAsts(Stmt[] stmts) {
			foreach (var stmt in stmts) {
				Console.WriteLine(GetString(stmt));
			}
		}

		public string GetString(Expr expr) {
			return (string)expr.Accept(this, null);
		}

		public string GetString(Stmt stmt) {
			return (string)stmt.Accept(this, null);
		}


		public object VisitAssignExpr(Expr.Assign expr, object options) {
			return Parenthesize2("=", expr.Token.lexeme, expr.value);
		}

		public object VisitBinaryExpr(Expr.Binary expr, object options) {
			return Parenthesize(expr.Token.lexeme, expr.left, expr.right);
		}

		public object VisitBlockStmt(Stmt.Block stmt, object options) {
			StringBuilder builder = new();
			builder.Append("(block ");
			foreach (Stmt statement in stmt.statements) {
				builder.Append(statement.Accept(this, null));
			}
			builder.Append(')');
			return builder.ToString();
		}

		public object VisitExpressionStmt(Stmt.Expression stmt, object options) {
			return Parenthesize(".", stmt.expression);
		}


		public object VisitInputVarStmt(Stmt.InputVar invar, object options) {
			return invar.type.Token.lexeme + " " + invar.Token.lexeme;
		}

		public object VisitFunctionStmt(Stmt.Function stmt, object options) {
			StringBuilder builder = new();
			string typestr;
			if (stmt.returnType != null) {
				typestr = (string)stmt.returnType.Accept(this, null);
			} else {
				typestr = "void";
			}
			builder.Append($"({typestr} {stmt.Token.lexeme}(");
			foreach (var param in stmt.input) {
				if (param != stmt.input[0]) {
					builder.Append(", ");
				}
				builder.Append(param.Accept(this, null));
			}
			builder.Append(") ");
			foreach (Stmt body in stmt.body) {
				builder.Append(body.Accept(this, null));
			}
			builder.Append(')');
			return builder.ToString();
		}

		public object VisitClassStmt(Stmt.ClassDefinition obj, object options) {
			StringBuilder builder = new();
			builder.Append($"({obj.Token.lexeme}: ");
			List<string> memberStr = new();
			foreach (Stmt.Var memb in obj.members) {
				memberStr.Add((string)memb.Accept(this, null));
			}
			builder.Append(Parenthesize2("members: ", memberStr.ToArray()));
			List<string> methodStr = new();
			foreach (Stmt.Function memb in obj.methods) {
				methodStr.Add((string)memb.Accept(this, null));
			}
			builder.Append(Parenthesize2("methods:", methodStr.ToArray()));
			builder.Append(')');
			return builder.ToString();
		}


		public object VisitLiteralExpr(Expr.Literal expr, object options) {
			if (expr.value == null) {
				return "null";
			}
			return expr.value.ToString();
		}

		public object VisitSetExpr(Expr.Set expr, object options) {
			return Parenthesize2("=", expr.obj, expr.Token.lexeme, expr.value);
		}

		public object VisitReturnStmt(Stmt.Return stmt, object options) {
			if (stmt.value == null) {
				return "(return)";
			}
			return Parenthesize("return", stmt.value);
		}

		public object VisitTypeStmt(Stmt.StmtType stmt, object options) {
			return stmt.Token.lexeme;
		}

		public object VisitUnaryExpr(Expr.Unary expr, object options) {
			return Parenthesize(expr.Token.lexeme, expr.right);
		}

		public object VisitVariableExpr(Expr.Variable var, object options) {
			return var.Token.lexeme;
		}

		public object VisitVarStmt(Stmt.Var stmt, object options) {
			string typestr = (string)stmt.stmtType.Accept(this, null);
			if (stmt.initialiser == null) {
				return Parenthesize2(typestr, stmt.Token);
			}
			return Parenthesize2(typestr, stmt.Token, "=", stmt.initialiser);
		}

		public object VisitGetExpr(Expr.Get expr, object options) {
			return Parenthesize2(".", expr.obj, expr.Token.lexeme);
		}

		public object VisitCallExpr(Expr.Call expr, object options) {
			return Parenthesize2("call", expr.callee, Parenthesize("args: ", expr.arguments.ToArray()));
		}

		public object VisitIfStmt(Stmt.If stmt, object options) {
			return Parenthesize("if", stmt.value);
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


		string Parenthesize(string name, params Expr[] exprs) {
			StringBuilder builder = new();
			builder.Append('(').Append(name);
			foreach (Expr expr in exprs) {
				builder.Append(' ');
				builder.Append(expr.Accept(this, null));
			}
			builder.Append(')');
			return builder.ToString();
		}

		string Parenthesize2(string name, params object[] parts) {
			StringBuilder builder = new();
			builder.Append('(').Append(name);
			foreach (object part in parts) {
				builder.Append(' ');
				if (part is Expr expr) {
					builder.Append(expr.Accept(this, null));
				} else if (part is Stmt stmt) {
					builder.Append(stmt.Accept(this, null));
				} else if (part is Token token) {
					builder.Append(token.lexeme);
				} else if (part != null) {
					builder.Append(part);
				}
			}
			builder.Append(')');
			return builder.ToString();
		}
	}

}
