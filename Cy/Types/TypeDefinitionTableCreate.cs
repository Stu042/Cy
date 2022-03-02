﻿using Cy.Enums;
using Cy.Parsing;
using Cy.Parsing.Interfaces;
using Cy.TokenGenerator;

using System.Collections.Generic;
using System.Linq;

namespace Cy.Types;

/// <summary>Create the symbol table, given an AST.</summary>
public class TypeDefinitionTableCreate : IExprVisitor, IStmtVisitor {

	readonly CalculateTypeDefinitionSizes _calculateSymbolSizes;
	readonly CalculateTypeDefinitionOffsets _calculateSymbolOffsets;
	public class Options {
		public bool InClassDefinition;
	}
	public TypeDefinitionTable SymbolTable;

	static readonly string BUILTIN_FILENAME = "builtin";
	static readonly int DEFAULT_INTSIZE = 4;
	static readonly int DEFAULT_FLOATSIZE = 8;

	readonly TypeDefinition[] standardTypes = new TypeDefinition[] {
		new TypeDefinition("int", null, null, DEFAULT_INTSIZE * 8, DEFAULT_INTSIZE, AccessModifier.Public, BaseType.INT, new Token[] { new Token(TokenType.INT, "int", null, 0,0,0, BUILTIN_FILENAME) }),
		new TypeDefinition("int8", null, null, 8, 1, AccessModifier.Public, BaseType.INT, new Token[] { new Token(TokenType.INT8, "int8", null, 0,0,0, BUILTIN_FILENAME) }),
		new TypeDefinition("int16", null, null, 16, 2, AccessModifier.Public, BaseType.INT, new Token[] { new Token(TokenType.INT16, "int16", null, 0,0,0, BUILTIN_FILENAME) }),
		new TypeDefinition("int32", null, null, 32, 4, AccessModifier.Public, BaseType.INT, new Token[] { new Token(TokenType.INT32, "int32", null, 0,0,0, BUILTIN_FILENAME) }),
		new TypeDefinition("int64", null, null, 64, 8, AccessModifier.Public, BaseType.INT, new Token[] { new Token(TokenType.INT64, "int64", null, 0,0,0, BUILTIN_FILENAME) }),
		new TypeDefinition("int128", null, null, 128, 16, AccessModifier.Public, BaseType.INT, new Token[] { new Token(TokenType.INT128, "int128", null, 0,0,0, BUILTIN_FILENAME) }),
		new TypeDefinition("float", null, null, DEFAULT_FLOATSIZE * 8, DEFAULT_FLOATSIZE, AccessModifier.Public, BaseType.FLOAT, new Token[] { new Token(TokenType.FLOAT, "float", null, 0,0,0, BUILTIN_FILENAME) }),
		new TypeDefinition("float16", null, null, 16, 2, AccessModifier.Public, BaseType.FLOAT, new Token[] { new Token(TokenType.FLOAT16, "float16", null, 0,0,0, BUILTIN_FILENAME) }),
		new TypeDefinition("float32", null, null, 32, 4, AccessModifier.Public, BaseType.FLOAT, new Token[] { new Token(TokenType.FLOAT32, "float32", null, 0,0,0, BUILTIN_FILENAME) }),
		new TypeDefinition("float64", null, null, 64, 8, AccessModifier.Public, BaseType.FLOAT, new Token[] { new Token(TokenType.FLOAT64, "float64", null, 0,0,0, BUILTIN_FILENAME) }),
		new TypeDefinition("float128", null, null, 128, 16, AccessModifier.Public, BaseType.FLOAT, new Token[] { new Token(TokenType.FLOAT128, "float128", null, 0,0,0, BUILTIN_FILENAME) }),
		new TypeDefinition("bool", null, null, 1, 1, AccessModifier.Public, BaseType.INT, new Token[] { new Token(TokenType.BOOL, "bool", null, 0,0,0, BUILTIN_FILENAME) }),
		new TypeDefinition("void", null, null, 0, 0, AccessModifier.Public, BaseType.VOID, new Token[] { new Token(TokenType.VOID, "void", null, 0,0,0, BUILTIN_FILENAME) })
	};

	public TypeDefinitionTableCreate(CalculateTypeDefinitionSizes calculateSymbolSizes, CalculateTypeDefinitionOffsets calculateSymbolOffsets) {
		_calculateSymbolSizes = calculateSymbolSizes;
		_calculateSymbolOffsets = calculateSymbolOffsets;
		SymbolTable = new TypeDefinitionTable();
		PopulateStandardTypes();
	}
	void PopulateStandardTypes() {
		foreach (var typedef in standardTypes) {
			typedef.Parent = SymbolTable.Global;
			SymbolTable.Global.Children.Add(typedef);
		}
	}

	public TypeDefinitionTable Parse(Stmt[][] toplevel) {
		foreach (var stmt in toplevel) {
			foreach (var section in stmt) {
				section.Accept(this, null);
			}
		}
		_calculateSymbolSizes.SetSizes(SymbolTable);
		_calculateSymbolOffsets.CalculateOffsets(SymbolTable);
		return SymbolTable;
	}

	TypeDefinition AddTypeDefinition(string sourceName, string functionName, AccessModifier accessModifier, Token[] tokens) {
		var typeDef = new TypeDefinition(sourceName, functionName, SymbolTable.Global, -1, -1, accessModifier, BaseType.VOID, tokens);
		SymbolTable.Global.Children.Add(typeDef);
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
		var previousTypeTableTypes = SymbolTable.Global;
		SymbolTable.Global = AddTypeDefinition("void", stmt.token.lexeme, AccessModifier.Private, new Token[] { stmt.token });
		foreach (Stmt statement in stmt.statements) {
			statement.Accept(this);
		}
		SymbolTable.Global = previousTypeTableTypes;
		return null;
	}

	public object VisitExpressionStmt(Stmt.Expression stmt, object options = null) {
		stmt.expression.Accept(this);
		return null;
	}


	public object VisitInputVarStmt(Stmt.InputVar stmt, object options = null) {
		//var tokens = (Token[])stmt.type.Accept(this);
		//AddSymbolDefinition(tokens[0].lexeme, stmt.token.lexeme, AccessModifier.Private, false, tokens);
		return null;
	}

	public object VisitFunctionStmt(Stmt.Function stmt, object options = null) {
		Token[] typeTokens;
		typeTokens = (Token[])stmt.returnType.Accept(this);
		var previousTypeTable = SymbolTable.Global;
		var tokLexemes = typeTokens.Select(tok => tok.lexeme);
		var lexeme = string.Join(".", tokLexemes);
		SymbolTable.Global = AddTypeDefinition(lexeme, stmt.token.lexeme, AccessModifier.Public, typeTokens);
		foreach (var param in stmt.input) {
			param.Accept(this);
		}
		foreach (Stmt body in stmt.body) {
			body.Accept(this);
		}
		SymbolTable.Global = previousTypeTable;
		return null;
	}

	public object VisitClassStmt(Stmt.ClassDefinition stmt, object options = null) {
		var previousTypeTable = SymbolTable.Global;
		SymbolTable.Global = AddTypeDefinition(stmt.token.lexeme, null, AccessModifier.Public, new Token[] { stmt.token });
		foreach (Stmt.Var memb in stmt.members) {
			memb.Accept(this, new Options { InClassDefinition = true });
		}
		foreach (Stmt.Function method in stmt.methods) {
			method.Accept(this, new Options { InClassDefinition = true });
		}
		SymbolTable.Global = previousTypeTable;
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
			AddTypeDefinition(typeTokens[0].lexeme, stmt.token.lexeme, AccessModifier.Public, typeTokens);
		}
		//else {
		//var tokLexemes = typeTokens.Select(tok => tok.lexeme);
		//var lexeme = string.Join(".", tokLexemes);
		//AddSymbolDefinition(lexeme, stmt.token.lexeme, AccessModifier.Public, false, typeTokens);
		//}
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
		AddTypeDefinition(stmt.iterator.lexeme, "", AccessModifier.Private, new Token[] { stmt.iteratorType.token });
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


public class CalculateTypeDefinitionSizes {
	public void SetSizes(TypeDefinitionTable symbolTable) {
		while (!CheckAllSizesSet(symbolTable.Global)) {
			CalcSizes(symbolTable.Global);
		}
	}

	void CalcSizes(TypeDefinition symbol) {    // repeat until all set...
		foreach (var child in symbol.Children) {
			if (child.ByteSize == -1) {
				if (child.Children.Count > 0) {
					var total = CalcSize(child);
					child.ByteSize = total;
				} else {
					SetSize(child);
				}
			}
		}
	}
	int CalcSize(TypeDefinition symbol) {    // repeat until all set...
		foreach (var child in symbol.Children) {
			child.ByteSize = SetSize(child);
		}
		var size = TotalOfChildSizes(symbol);
		return size;
	}
	int SetSize(TypeDefinition symbol) {
		if (symbol.ByteSize == -1) {
			var names = symbol.Tokens.Select(tok => tok.lexeme);
			var name = string.Join('.', names);
			var type = symbol.LookUpType(name);
			symbol.ByteSize = type.ByteSize;
			return type.ByteSize;
		}
		return -1;
	}

	bool CheckAllSizesSet(TypeDefinition symbol) {
		if (!ChildSizesSet(symbol) || symbol.TypeName != "" && symbol.ByteSize == -1) {
			return false;
		}
		foreach (var child in symbol.Children) {
			if (child.Children.Count > 0) {
				return CheckAllSizesSet(child);
			}
		}
		return true;
	}

	bool ChildSizesSet(TypeDefinition symbol, bool onlyMembers = false) {
		return symbol.Children.All(child => child.ByteSize >= 0 || !onlyMembers || onlyMembers && child.IsMember);
	}
	int TotalOfChildSizes(TypeDefinition symbol, bool onlyMembers = true) {
		if (ChildSizesSet(symbol, onlyMembers)) {
			return symbol.Children.Sum(child => child.ByteSize >= 0 && (onlyMembers && child.IsMember || !onlyMembers) ? child.ByteSize : 0);
		}
		return -1;
	}
}

public class CalculateTypeDefinitionOffsets {
	public void CalculateOffsets(TypeDefinitionTable symbolTable) {
		foreach (var child in symbolTable.Global.Children) {
			CalcChildOffsets(child);
		}
	}

	void CalcChildOffsets(TypeDefinition symbol) {
		int offset = 0;
		foreach (var child in symbol.Children) {
			if (child.IsMember) {
				child.Offset = offset;
				offset += child.ByteSize;
				CalcChildOffsets(child);
			}
		}
	}
}
