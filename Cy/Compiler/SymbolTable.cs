using System;
using System.Collections.Generic;
using System.Linq;

using Cy.Common;
using Cy.Common.Interfaces;
using Cy.Scanner;

namespace Cy.Compiler {

	/// <summary>Symbols for this block, starting with the global block.</summary>
	public class SymbolTable {
		public string context;                              // name of this context, i.e. "" global and "Main" for Main function
		public SymbolTable parent;
		public List<SymbolTable> scopes;
		public readonly Dictionary<string, Symbol> symbols;

		public SymbolTable(SymbolTable parent, string context) {
			this.parent = parent;
			this.context = context;
			scopes = new List<SymbolTable>();
			symbols = new Dictionary<string, Symbol>();
		}
		public void Insert(Symbol symbol) {
			symbols.Add(symbol.sourceName, symbol);
		}
		public Symbol LookUp(string symbolName) {
			var symbolTable = this;
			while (symbolTable != null) {
				if (symbolTable.symbols.ContainsKey(symbolName)) {
					return symbols[symbolName];
				}
				symbolTable = symbolTable.parent;
			}
			return null;
		}
	}


	/// <summary>Info for an instance from source file.</summary>
	public class Symbol {
		public enum Attribute {
			Public,
			Private,
			Protected
		}

		/// <summary>Name given in source code, last part of fully qualified name.</summary>
		public string sourceName;
		/// <summary>Is symbol Public, Private or Protected.</summary>
		public Attribute attribute;
		/// <summary>Type information.</summary>
		public Token type;

		public Symbol(string sourceName, Attribute attribute = Attribute.Public, Token type = null) {
			this.sourceName = sourceName;
			this.attribute = attribute;
			this.type = type;
		}

		public override string ToString() {
			return $"{sourceName}, {attribute}, {string.Join('.', type.lexeme)}";
		}
	}


	/// <summary>Write SymbolTable to console.</summary>
	public class DisplaySymbolTable {
		public void DisplayTable(SymbolTable symbolTable) {
			foreach (var symbol in symbolTable.symbols) {
				Console.WriteLine(symbol.Value);
			}
			foreach (var scope in symbolTable.scopes) {
				DisplayTable(scope);
			}
		}
	}




	/// <summary>Class to create the symbol table, given an AST.</summary>
	public class CreateSymbolTable : IExprVisitor, IStmtVisitor {
		enum State {
			InFunction,
			InClass,
			InGlobal
		}

		/// <summary>Current fully qualified name given in source code.</summary>
		public SymbolTable SymbolTable { get; private set; }
		SymbolTable currentSymbolTable;

		public CreateSymbolTable() {
			SymbolTable = new SymbolTable(null, "");
			currentSymbolTable = SymbolTable;
		}

		public void Parse(List<List<Stmt>> toplevel) {
			foreach (var stmt in toplevel) {
				foreach (var s in stmt) {
					s.Accept(this, null);
				}
			}
		}


		public object VisitAssignExpr(Expr.Assign expr, object options) {
			return null;
		}

		public object VisitBinaryExpr(Expr.Binary expr, object options) {
			expr.left.Accept(this, null);
			expr.right.Accept(this, null);
			return null;
		}

		public object VisitBlockStmt(Stmt.Block stmt, object options) {
			var previousSymbolTable = currentSymbolTable;
			currentSymbolTable = new SymbolTable(currentSymbolTable, "Block");
			foreach (Stmt statement in stmt.statements) {
				statement.Accept(this, null);
			}
			currentSymbolTable = previousSymbolTable;
			return null;
		}

		public object VisitExpressionStmt(Stmt.Expression stmt, object options) {
			stmt.expression.Accept(this, null);
			return null;
		}


		public object VisitInputVarStmt(Stmt.InputVar stmt, object options) {
			var type = (Token)stmt.type.Accept(this, null);
			AddSymbol(stmt.token.lexeme, Symbol.Attribute.Private, type);
			return null;
		}

		public object VisitFunctionStmt(Stmt.Function stmt, object options) {
			Token type = null;
			if (stmt.returnType != null) {
				type = (Token)stmt.returnType.Accept(this, null);
			} else {
				type = new Token(TokenType.VOID);
			}
			AddSymbol(stmt.token.lexeme, Symbol.Attribute.Public, type);
			var previousSymbolTable = currentSymbolTable;
			currentSymbolTable = new SymbolTable(currentSymbolTable, stmt.token.lexeme);
			foreach (var param in stmt.input) {
				param.Accept(this, null);
			}
			foreach (Stmt body in stmt.body) {
				body.Accept(this, null);
			}
			currentSymbolTable = previousSymbolTable;
			return null;
		}

		public object VisitClassStmt(Stmt.ClassDefinition stmt, object options) {
			var type = stmt.token;
			AddSymbol(stmt.token.lexeme, Symbol.Attribute.Public, type);
			var previousSymbolTable = currentSymbolTable;
			currentSymbolTable = new SymbolTable(currentSymbolTable, stmt.token.lexeme);
			foreach (Stmt.Var memb in stmt.members) {
				memb.Accept(this, null);
			}
			foreach (Stmt.Function memb in stmt.methods) {
				memb.Accept(this, null);
			}
			currentSymbolTable = previousSymbolTable;
			return null;
		}


		public object VisitLiteralExpr(Expr.Literal expr, object options) {
			return null;
		}

		public object VisitSetExpr(Expr.Set expr, object options) {
			return null;
		}

		public object VisitReturnStmt(Stmt.Return stmt, object options) {
			return null;
		}

		public object VisitTypeStmt(Stmt.StmtType stmt, object options) {
			return stmt.token;
		}

		public object VisitUnaryExpr(Expr.Unary expr, object options) {
			return null;
		}

		public object VisitVariableExpr(Expr.Variable expr, object options) {
			return null;
		}

		public object VisitVarStmt(Stmt.Var stmt, object options) {
			var type = (Token)stmt.stmtType.Accept(this, null);
			AddSymbol(stmt.token.lexeme, Symbol.Attribute.Public, type);
			if (stmt.initialiser != null) {
				stmt.initialiser.Accept(this, null);
			}
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
			AddSymbol(stmt.iterator.lexeme, Symbol.Attribute.Private, stmt.iteratorType.token);
			return null;
		}

		public object VisitWhileStmt(Stmt.While stmt, object options) {
			return null;
		}

		public object VisitGroupingExpr(Expr.Grouping expr, object options) {
			throw new NotImplementedException();
		}


		Symbol AddSymbol(string sourceName, Symbol.Attribute attribute = Symbol.Attribute.Public, Token type = null) {
			var sym = new Symbol(sourceName, attribute, type);
			currentSymbolTable.Insert(sym);
			return sym;
		}

		string UniqueString(string postfix) {
			var g = Guid.NewGuid();
			var guidString = Convert.ToBase64String(g.ToByteArray());
			guidString = guidString.Replace('=', '_').Replace('+', '_').Replace('/', '_') + '_' + postfix;
			return guidString;
		}
	}
}
