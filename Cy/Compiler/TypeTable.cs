using System;
using System.Collections.Generic;
using System.Linq;

using Cy.Common;
using Cy.Common.Interfaces;
using Cy.Scanner;


namespace Cy.Compiler {

	public enum AccessModifier {
		Public,
		Private,
		Protected
	}


	/// <summary>Info for a type (an object).</summary>
	public class TypeDefinition {
		/// <summary>Name given in source code, last part of fully qualified name.</summary>
		public string NameInSource;
		/// <summary>Is symbol Public, Private or Protected.</summary>
		public AccessModifier Modifier;
		/// <summary>Token information.</summary>
		public Token Token;
		/// <summary>Size (in bytes) of type.</summary>
		public int Size;

		public TypeDefinition(string nameInSource, int size, AccessModifier accessModifier = AccessModifier.Public, Token token = null) {
			NameInSource = nameInSource;
			Modifier = accessModifier;
			Token = token;
			Size = size;
		}

		public override string ToString() {
			//var typeStrs = Token.Select(t => t.lexeme).ToArray();
			var typeStrs = Token.lexeme;
			return $"{NameInSource}, {Modifier}, {string.Join('.', typeStrs)}";
		}
	}



	/// <summary>Global type table must have a null parent and be named "".</summary>
	public class TypeTable {
		public string Name;                              // name of this context, i.e. "" global and "Main" for Main function
		public TypeTable Parent;
		public List<TypeTable> Children;
		public readonly Dictionary<string, TypeDefinition> Types;

		public TypeTable(TypeTable parent, string context) {
			Parent = parent;
			Name = context;
			Children = new List<TypeTable>();
			Types = new Dictionary<string, TypeDefinition>();
		}
		public void Insert(TypeDefinition typeDef) {
			Types.Add(typeDef.NameInSource, typeDef);
		}
		public TypeDefinition LookUp(string typeName) {
			var typeTable = this;
			while (typeTable != null) {
				if (typeTable.Types.ContainsKey(typeName)) {
					return Types[typeName];
				}
				typeTable = typeTable.Parent;
			}
			return null;
		}
	}




	/// <summary>Write SymbolTable to console.</summary>
	public class DisplayTypeTable {
		public void DisplayTable(TypeTable typeTable) {
			foreach (var typedef in typeTable.Types) {
				Console.WriteLine(typedef.Value);
			}
			foreach (var child in typeTable.Children) {
				DisplayTable(child);
			}
		}
	}




	/// <summary>Class to create the symbol table, given an AST.</summary>
	public class CreateTypeTable : IExprVisitor, IStmtVisitor {
		enum State {
			InFunction,
			InClass,
			InGlobal
		}

		static readonly string STDLIB_FILENAME = "stdlib";
		static readonly int DEFAULT_INTSIZE = 4;
		static readonly int DEFAULT_FLOATSIZE = 8;

		/// <summary>Current fully qualified name given in source code.</summary>
		public TypeTable TypeTable { get; private set; }
		TypeTable currentTypeTable;


		TypeDefinition[] standardTypes = new TypeDefinition[] {
			new TypeDefinition("int", DEFAULT_INTSIZE, AccessModifier.Public, new Token(TokenType.INT, "int", null, 0,0,0, STDLIB_FILENAME)),
			new TypeDefinition("int8", 1, AccessModifier.Public, new Token(TokenType.INT8, "int8", null, 0,0,0, STDLIB_FILENAME)),
			new TypeDefinition("int16", 2, AccessModifier.Public, new Token(TokenType.INT16, "int16", null, 0,0,0, STDLIB_FILENAME)),
			new TypeDefinition("int32", 4, AccessModifier.Public, new Token(TokenType.INT32, "int32", null, 0,0,0, STDLIB_FILENAME)),
			new TypeDefinition("int64", 8, AccessModifier.Public, new Token(TokenType.INT64, "int64", null, 0,0,0, STDLIB_FILENAME)),
			new TypeDefinition("int128", 16, AccessModifier.Public, new Token(TokenType.INT128, "int128", null, 0,0,0, STDLIB_FILENAME)),
			new TypeDefinition("float", DEFAULT_FLOATSIZE, AccessModifier.Public, new Token(TokenType.FLOAT, "float", null, 0,0,0, STDLIB_FILENAME)),
			new TypeDefinition("float16", 2, AccessModifier.Public, new Token(TokenType.FLOAT16, "float16", null, 0,0,0, STDLIB_FILENAME)),
			new TypeDefinition("float32", 4, AccessModifier.Public, new Token(TokenType.FLOAT32, "float32", null, 0,0,0, STDLIB_FILENAME)),
			new TypeDefinition("float64", 8, AccessModifier.Public, new Token(TokenType.FLOAT64, "float64", null, 0,0,0, STDLIB_FILENAME)),
			new TypeDefinition("float128", 16, AccessModifier.Public, new Token(TokenType.FLOAT128, "float128", null, 0,0,0, STDLIB_FILENAME)),
			new TypeDefinition("bool", 1, AccessModifier.Public, new Token(TokenType.BOOL, "bool", null, 0,0,0, STDLIB_FILENAME)),
			new TypeDefinition("void", 0, AccessModifier.Public, new Token(TokenType.VOID, "void", null, 0,0,0, STDLIB_FILENAME))
		};

		public CreateTypeTable() {
			TypeTable = new TypeTable(null, "");
			currentTypeTable = TypeTable;
			PopulateStandardTypes();
		}
		void PopulateStandardTypes() {
			foreach (var typedef in standardTypes) {
				currentTypeTable.Insert(typedef);
			}
		}


		public void Parse(List<List<Stmt>> toplevel) {
			foreach (var stmt in toplevel) {
				foreach (var section in stmt) {
					section.Accept(this, null);
				}
			}
		}

		public object VisitGroupingExpr(Expr.Grouping expr, object options) {
			return null;
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
			var previousTypeTable = currentTypeTable;
			currentTypeTable = new TypeTable(currentTypeTable, "Block");
			foreach (Stmt statement in stmt.statements) {
				statement.Accept(this, null);
			}
			currentTypeTable = previousTypeTable;
			return null;
		}

		public object VisitExpressionStmt(Stmt.Expression stmt, object options) {
			stmt.expression.Accept(this, null);
			return null;
		}


		public object VisitInputVarStmt(Stmt.InputVar stmt, object options) {
			var type = (Token)stmt.type.Accept(this, null);
			AddTypeDefinition(stmt.token.lexeme, AccessModifier.Private, type);
			return null;
		}

		public object VisitFunctionStmt(Stmt.Function stmt, object options) {
			Token token;
			if (stmt.returnType != null) {
				token = (Token)stmt.returnType.Accept(this, null);
			} else {
				token = new Token(TokenType.VOID);
			}
			AddTypeDefinition(stmt.token.lexeme, AccessModifier.Public, token);
			var previousTypeTable = currentTypeTable;
			currentTypeTable = new TypeTable(currentTypeTable, stmt.token.lexeme);
			foreach (var param in stmt.input) {
				param.Accept(this, null);
			}
			foreach (Stmt body in stmt.body) {
				body.Accept(this, null);
			}
			currentTypeTable = previousTypeTable;
			return null;
		}

		public object VisitClassStmt(Stmt.ClassDefinition stmt, object options) {
			AddTypeDefinition(stmt.token.lexeme, AccessModifier.Public, stmt.token);
			var previousSymbolTable = currentTypeTable;
			currentTypeTable = new TypeTable(currentTypeTable, stmt.token.lexeme);
			foreach (Stmt.Var memb in stmt.members) {
				memb.Accept(this, null);
			}
			foreach (Stmt.Function memb in stmt.methods) {
				memb.Accept(this, null);
			}
			currentTypeTable = previousSymbolTable;
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
			return stmt.info;
		}

		public object VisitUnaryExpr(Expr.Unary expr, object options) {
			return null;
		}

		public object VisitVariableExpr(Expr.Variable expr, object options) {
			return null;
		}

		public object VisitVarStmt(Stmt.Var stmt, object options) {
			var token = (Token)stmt.stmtType.Accept(this, null);
			AddTypeDefinition(stmt.token.lexeme, AccessModifier.Public, token);
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
			AddTypeDefinition(stmt.iterator.lexeme, AccessModifier.Private, stmt.iteratorType.token);
			return null;
		}

		public object VisitWhileStmt(Stmt.While stmt, object options) {
			return null;
		}


		void AddTypeDefinition(string sourceName, AccessModifier accessModifier = AccessModifier.Public, Token token = null) {
			var typedef = new TypeDefinition(sourceName, -1, accessModifier, token);
			currentTypeTable.Insert(typedef);
		}
	}
}
