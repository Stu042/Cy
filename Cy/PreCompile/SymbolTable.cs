using Cy.Common;
using Cy.Common.Interfaces;
using Cy.Scanner;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace Cy.Compiler {

	public enum AccessModifier {
		Public,
		Private,
		Protected
	}


	/// <summary>Info for a type/namespace/object/method (an object).
	/// Global type table must have a null parent and be named "".</summary>
	public class SymbolDefinition {
		/// <summary>Name given in source code, last part of fully qualified name.</summary>
		public string TypeName;
		/// <summary>Name given in source code for this instance.</summary>
		public string InstanceName;
		/// <summary>Namespace this belongs to, or null if this is the global namespace.</summary>
		public SymbolDefinition Parent;
		public List<SymbolDefinition> Children;
		/// <summary>Is symbol Public, Private or Protected.</summary>
		public AccessModifier Modifier;
		/// <summary>Is this object a functional object.</summary>
		public bool IsFunctional;
		/// <summary>Size (in bytes) of type.</summary>
		public int Size;
		/// <summary>Token information.</summary>
		public Token[] Tokens;

		public SymbolDefinition(string typeName, string instanceName, SymbolDefinition parent, int size, AccessModifier accessModifier, bool isFunctional, Token[] tokens) {
			TypeName = typeName;
			InstanceName = instanceName;
			Parent = parent;
			Children = new List<SymbolDefinition>();
			Modifier = accessModifier;
			IsFunctional = isFunctional;
			Tokens = tokens;
			Size = size;
		}

		public override string ToString() {
			var isFunction = IsFunctional ? "()" : "";
			return $"{Modifier}, {TypeName} {InstanceName}{isFunction}, {Size}";
		}

		public string FullyQualifiedName() {
			var nameParts = new List<string>();
			var cur = this;
			while (cur.Parent != null) {
				nameParts.Add(cur.TypeName);
				cur = cur.Parent;
			}
			nameParts.Reverse();
			return String.Join('.', nameParts);
		}
	}



	/// <summary>Global SymbolDefinition must have a null parent and be named "".</summary>
	public class SymbolTable {
		public SymbolDefinition Types;

		public SymbolTable() {
			Types = new SymbolDefinition("", null, null, -1, AccessModifier.Public, false, null);
		}
		public SymbolDefinition LookUp(string typeName, string currentTypeName) {
			SymbolDefinition currentType = LookUpHere(currentTypeName, Types);
			return LookUp(typeName, currentType);
		}
		public SymbolDefinition LookUp(string typeName, SymbolDefinition currentType) {
			var type = LookUpHere(typeName, currentType);
			if (type == null) {
				type = LookUpHere(typeName, Types);
			}
			return type;
		}
		public SymbolDefinition LookUpHere(string typeName, SymbolDefinition currentType) {
			var typeNameParts = typeName.Split('.');
			foreach (var typeNamePart in typeNameParts) {
				currentType = currentType.Children.Find(curr => curr.TypeName == typeNamePart);
			}
			return currentType;
		}
	}



	/// <summary>Class to create the symbol table, given an AST.</summary>
	public class CreateSymbolTable : IExprVisitor, IStmtVisitor {
		public class Options {
			public bool InClassDefinition;
		}
		static readonly string BUILTIN_FILENAME = "builtin";
		static readonly int DEFAULT_INTSIZE = 4;
		static readonly int DEFAULT_FLOATSIZE = 8;

		/// <summary>Current fully qualified name given in source code.</summary>
		public SymbolTable TypeTable;

		SymbolDefinition[] standardTypes = new SymbolDefinition[] {
			new SymbolDefinition("int", null, null, DEFAULT_INTSIZE, AccessModifier.Public, false, new Token[] { new Token(TokenType.INT, "int", null, 0,0,0, BUILTIN_FILENAME) }),
			new SymbolDefinition("int8", null, null, 1, AccessModifier.Public, false, new Token[] { new Token(TokenType.INT8, "int8", null, 0,0,0, BUILTIN_FILENAME) }),
			new SymbolDefinition("int16", null, null, 2, AccessModifier.Public, false, new Token[] { new Token(TokenType.INT16, "int16", null, 0,0,0, BUILTIN_FILENAME) }),
			new SymbolDefinition("int32", null, null, 4, AccessModifier.Public, false, new Token[] { new Token(TokenType.INT32, "int32", null, 0,0,0, BUILTIN_FILENAME) }),
			new SymbolDefinition("int64", null, null, 8, AccessModifier.Public, false, new Token[] { new Token(TokenType.INT64, "int64", null, 0,0,0, BUILTIN_FILENAME) }),
			new SymbolDefinition("int128", null, null, 16, AccessModifier.Public, false, new Token[] { new Token(TokenType.INT128, "int128", null, 0,0,0, BUILTIN_FILENAME) }),
			new SymbolDefinition("float", null, null, DEFAULT_FLOATSIZE, AccessModifier.Public, false, new Token[] { new Token(TokenType.FLOAT, "float", null, 0,0,0, BUILTIN_FILENAME) }),
			new SymbolDefinition("float16", null, null, 2, AccessModifier.Public, false, new Token[] { new Token(TokenType.FLOAT16, "float16", null, 0,0,0, BUILTIN_FILENAME) }),
			new SymbolDefinition("float32", null, null, 4, AccessModifier.Public, false, new Token[] { new Token(TokenType.FLOAT32, "float32", null, 0,0,0, BUILTIN_FILENAME) }),
			new SymbolDefinition("float64", null, null, 8, AccessModifier.Public, false, new Token[] { new Token(TokenType.FLOAT64, "float64", null, 0,0,0, BUILTIN_FILENAME) }),
			new SymbolDefinition("float128", null, null, 16, AccessModifier.Public, false, new Token[] { new Token(TokenType.FLOAT128, "float128", null, 0,0,0, BUILTIN_FILENAME) }),
			new SymbolDefinition("bool", null, null, 1, AccessModifier.Public, false, new Token[] { new Token(TokenType.BOOL, "bool", null, 0,0,0, BUILTIN_FILENAME) }),
			new SymbolDefinition("void", null, null, 0, AccessModifier.Public, false, new Token[] { new Token(TokenType.VOID, "void", null, 0,0,0, BUILTIN_FILENAME) })
		};

		public CreateSymbolTable() {
			TypeTable = new SymbolTable();
			PopulateStandardTypes();
		}
		void PopulateStandardTypes() {
			foreach (var typedef in standardTypes) {
				typedef.Parent = TypeTable.Types;
				TypeTable.Types.Children.Add(typedef);
			}
		}


		public SymbolTable Parse(List<List<Stmt>> toplevel) {
			foreach (var stmt in toplevel) {
				foreach (var section in stmt) {
					section.Accept(this, null);
				}
			}
			return TypeTable;
		}
		void CalcSizes() {
			var typedef = TypeTable.Types;
			foreach (var child in typedef.Children) {
				var names = child.Tokens.Select(tok => tok.lexeme);
				var name = String.Join('.', names);
				LookUp(name, typedef);
			}
		}
		void CalcSizes(SymbolDefinition typedef) {    // do until all set...
			foreach (var child in typedef.Children) {
				if (child.Size == -1) {
					var names = child.Tokens.Select(tok => tok.lexeme);
					var name = String.Join('.', names);
					var type = LookUp(name, typedef);
					child.Size = type.Size;
				}
				if (child.Children.Count > 0) {
					CalcSizes(child);
				}
			}
		}
		SymbolDefinition LookUp(string typeName, SymbolDefinition currentType) {
			var type = LookUpHere(typeName, TypeTable.Types);
			if (type == null || type.Size == -1) {
				type = LookUpHere(typeName, currentType);
			}
			return type;
		}
		SymbolDefinition LookUpHere(string typeName, SymbolDefinition currentType) {
			var typeNameParts = typeName.Split('.');
			foreach (var typeNamePart in typeNameParts) {
				currentType = currentType.Children.Find(curr => curr.TypeName == typeNamePart);
			}
			return currentType;
		}


		SymbolDefinition AddSymbolDefinition(string sourceName, string instanceName, AccessModifier accessModifier, bool isFunctional, Token[] tokens) {
			var typeDef = new SymbolDefinition(sourceName, instanceName, TypeTable.Types, -1, accessModifier, isFunctional, tokens);
			TypeTable.Types.Children.Add(typeDef);
			return typeDef;
		}



		public object VisitGroupingExpr(Expr.Grouping expr, object options = null) {
			return null;
		}

		public object VisitAssignExpr(Expr.Assign expr, object options = null) {
			return null;
		}

		public object VisitBinaryExpr(Expr.Binary expr, object options = null) {
			expr.left.Accept(this);
			expr.right.Accept(this);
			return null;
		}

		public object VisitBlockStmt(Stmt.Block stmt, object options = null) {
			var previousTypeTableTypes = TypeTable.Types;
			TypeTable.Types = AddSymbolDefinition("void", "", AccessModifier.Public, true, new Token[] { stmt.token });
			foreach (Stmt statement in stmt.statements) {
				statement.Accept(this);
			}
			TypeTable.Types = previousTypeTableTypes;
			return null;
		}

		public object VisitExpressionStmt(Stmt.Expression stmt, object options = null) {
			stmt.expression.Accept(this);
			return null;
		}


		public object VisitInputVarStmt(Stmt.InputVar stmt, object options = null) {
			var tokens = (Token[])stmt.type.Accept(this);
			AddSymbolDefinition(tokens[0].lexeme, stmt.token.lexeme, AccessModifier.Private, false, tokens);
			return null;
		}

		public object VisitFunctionStmt(Stmt.Function stmt, object options = null) {
			Token[] typeTokens;
			typeTokens = (Token[])stmt.returnType.Accept(this);
			var previousTypeTable = TypeTable.Types;
			var tokLexemes = typeTokens.Select(tok => tok.lexeme);
			var lexeme = String.Join(".", tokLexemes);
			TypeTable.Types = AddSymbolDefinition(lexeme, stmt.token.lexeme, AccessModifier.Public, true, typeTokens);
			foreach (var param in stmt.input) {
				param.Accept(this);
			}
			foreach (Stmt body in stmt.body) {
				body.Accept(this);
			}
			TypeTable.Types = previousTypeTable;
			return null;
		}

		public object VisitClassStmt(Stmt.ClassDefinition stmt, object options = null) {
			var previousTypeTable = TypeTable.Types;
			TypeTable.Types = AddSymbolDefinition(stmt.token.lexeme, null, AccessModifier.Public, false, new Token[] { stmt.token });
			foreach (Stmt.Var memb in stmt.members) {
				memb.Accept(this, new Options { InClassDefinition = true });
			}
			foreach (Stmt.Function method in stmt.methods) {
				method.Accept(this, new Options { InClassDefinition = true });
			}
			TypeTable.Types = previousTypeTable;
			return null;
		}


		public object VisitLiteralExpr(Expr.Literal expr, object options = null) {
			return null;
		}

		public object VisitSetExpr(Expr.Set expr, object options = null) {
			return null;
		}

		public object VisitReturnStmt(Stmt.Return stmt, object options = null) {
			return null;
		}

		public object VisitTypeStmt(Stmt.StmtType stmt, object options = null) {
			return stmt.info;
		}

		public object VisitUnaryExpr(Expr.Unary expr, object options = null) {
			return null;
		}

		public object VisitVariableExpr(Expr.Variable expr, object options = null) {
			return null;
		}

		public object VisitVarStmt(Stmt.Var stmt, object options = null) {
			var typeTokens = (Token[])stmt.stmtType.Accept(this);
			var opts = GetOptions(options);
			if (opts.InClassDefinition) {
				AddSymbolDefinition(typeTokens[0].lexeme, stmt.token.lexeme, AccessModifier.Public, false, typeTokens);
			} else {
				var tokLexemes = typeTokens.Select(tok => tok.lexeme);
				var lexeme = String.Join(".", tokLexemes);
				AddSymbolDefinition(lexeme, stmt.token.lexeme, AccessModifier.Public, false, typeTokens);
			}
			if (stmt.initialiser != null) {
				stmt.initialiser.Accept(this);
			}
			return null;
		}

		public object VisitGetExpr(Expr.Get expr, object options = null) {
			return null;
		}

		public object VisitCallExpr(Expr.Call expr, object options = null) {
			return null;
		}

		public object VisitIfStmt(Stmt.If stmt, object options = null) {
			return null;
		}

		public object VisitForStmt(Stmt.For stmt, object options = null) {
			AddSymbolDefinition(stmt.iterator.lexeme, "", AccessModifier.Private, true, new Token[] { stmt.iteratorType.token });
			return null;
		}

		public object VisitWhileStmt(Stmt.While stmt, object options = null) {
			return null;
		}



		Options GetOptions(object options) {
			if (options is Options opts) {
				return opts;
			}
			return new Options {
				InClassDefinition = false
			};
		}
	}




	/// <summary>Write SymbolTable to console.</summary>
	public class DisplaySymbolTable {
		int TAB_SIZE { get; } = 4;
		public void DisplayTable(SymbolTable typeTable) {
			foreach (var typedef in typeTable.Types.Children) {
				DisplayType(typedef);
			}
		}
		void DisplayType(SymbolDefinition typedef, int tabCount = 0) {
			Console.WriteLine(GetTabs(tabCount) + typedef);
			foreach (var child in typedef.Children) {
				DisplayType(child, tabCount + 1);
			}
		}
		string GetTabs(int tabCount) {
			var oneTab = new String(' ', TAB_SIZE);
			var tabs = new StringBuilder();
			for (var i = 0; i < tabCount; i++) {
				tabs.Append(oneTab);
			}
			return tabs.ToString();
		}
	}

}
