﻿using Cy.Preprocesor.Interfaces;

using System;
using System.Collections.Generic;
using System.Text;


namespace Cy.Preprocesor;

public static class Asts {
	public static void Display(List<List<Stmt>> allFilesStmts) {
		new AstPrinter().DisplayAllAsts(allFilesStmts);
	}

	protected class AstPrinter : IAstVisitor {

		public void DisplayAllAsts(List<List<Stmt>> allFilesStmts) {
			Console.WriteLine("\nAST:");
			foreach (var stmts in allFilesStmts) {
				DisplayAsts(stmts);
			}
		}

		public void DisplayAsts(List<Stmt> stmts) {
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
			return Parenthesize2("=", expr.Token.Lexeme, expr.value);
		}

		public object VisitBinaryExpr(Expr.Binary expr, object options) {
			return Parenthesize(expr.Token.Lexeme, expr.left, expr.right);
		}

		public object VisitBlockStmt(Stmt.Block stmt, object options) {
			StringBuilder builder = new();
			builder.Append("(block ");
			foreach (Stmt statement in stmt.Statements) {
				builder.Append(statement.Accept(this, null));
			}
			builder.Append(')');
			return builder.ToString();
		}

		public object VisitExpressionStmt(Stmt.Expression stmt, object options) {
			return Parenthesize(".", stmt.expression);
		}


		public object VisitInputVarStmt(Stmt.InputVar invar, object options) {
			return invar.type.Token.Lexeme + " " + invar.Token.Lexeme;
		}

		public object VisitFunctionStmt(Stmt.Function stmt, object options) {
			StringBuilder builder = new();
			string typestr;
			if (stmt.returnType != null) {
				typestr = (string)stmt.returnType.Accept(this, null);
			} else {
				typestr = "void";
			}
			builder.Append($"({typestr} {stmt.Token.Lexeme}(");
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
			builder.Append($"({obj.Token.Lexeme}: ");
			List<string> definitionStr = new();
			foreach (Stmt.ClassDefinition clss in obj.Classes) {
				definitionStr.Add((string)clss.Accept(this, null));
			}
			if (definitionStr.Count > 0) {
				builder.Append(Parenthesize2("definitions:", definitionStr.ToArray()));
			}
			List<string> memberStr = new();
			foreach (Stmt.VarDefinition memb in obj.Members) {
				memberStr.Add((string)memb.Accept(this, null));
			}
			if (memberStr.Count > 0) {
				builder.Append(Parenthesize2("members:", memberStr.ToArray()));
			}
			List<string> methodStr = new();
			foreach (Stmt.Function memb in obj.Methods) {
				methodStr.Add((string)memb.Accept(this, null));
			}
			if (methodStr.Count > 0) {
				builder.Append(Parenthesize2("methods:", methodStr.ToArray()));
			}
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
			return Parenthesize2("=", expr.obj, expr.Token.Lexeme, expr.value);
		}

		public object VisitReturnStmt(Stmt.Return stmt, object options) {
			if (stmt.value == null) {
				return "(return)";
			}
			return Parenthesize("return", stmt.value);
		}

		public object VisitTypeStmt(Stmt.StmtType stmt, object options) {
			return stmt.Token.Lexeme;
		}

		public object VisitUnaryExpr(Expr.Unary expr, object options) {
			return Parenthesize(expr.Token.Lexeme, expr.right);
		}

		public object VisitVariableExpr(Expr.Variable var, object options) {
			return var.Token.Lexeme;
		}

		public object VisitVarStmt(Stmt.VarDefinition stmt, object options) {
			string typestr = (string)stmt.stmtType.Accept(this, null);
			if (stmt.Initialiser == null) {
				return Parenthesize2(typestr, stmt.Token);
			}
			return Parenthesize2(typestr, stmt.Token, "=", stmt.Initialiser);
		}

		public object VisitGetExpr(Expr.Get expr, object options) {
			return Parenthesize2(".", expr.obj, expr.Token.Lexeme);
		}

		public object VisitCallExpr(Expr.Call expr, object options) {
			return Parenthesize2("call", expr.callee, Parenthesize("args: ", expr.arguments));
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
					builder.Append(token.Lexeme);
				} else if (part != null) {
					builder.Append(part);
				}
			}
			builder.Append(')');
			return builder.ToString();
		}
	}

}
