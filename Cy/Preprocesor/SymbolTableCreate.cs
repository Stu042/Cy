using Cy.Enums;
using Cy.Preprocesor.Interfaces;

using System.Collections.Generic;
using System.Linq;

namespace Cy.Preprocesor;

/// <summary>Create the symbol table, given an AST.</summary>
public class SymbolTableCreate : IExprVisitor, IStmtVisitor {

	readonly CalculateSymbolSizes _calculateSymbolSizes;
	readonly CalculateSymbolOffsets _calculateSymbolOffsets;
	public class Options {
		public bool InClassDefinition;
	}
	public TypeTable SymbolTable;

	static readonly string BUILTIN_FILENAME = "builtin";
	static readonly int DEFAULT_INTSIZE = 4;
	static readonly int DEFAULT_FLOATSIZE = 8;

	readonly SymbolDefinition[] standardTypes = new SymbolDefinition[] {
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

	public SymbolTableCreate(CalculateSymbolSizes calculateSymbolSizes, CalculateSymbolOffsets calculateSymbolOffsets) {
		_calculateSymbolSizes = calculateSymbolSizes;
		_calculateSymbolOffsets = calculateSymbolOffsets;
		SymbolTable = new TypeTable();
		PopulateStandardTypes();
	}
	void PopulateStandardTypes() {
		foreach (var typedef in standardTypes) {
			typedef.Parent = SymbolTable.Types;
			SymbolTable.Types.Children.Add(typedef);
		}
	}

	public TypeTable Parse(List<List<Stmt>> toplevel) {
		foreach (var stmt in toplevel) {
			foreach (var section in stmt) {
				section.Accept(this, null);
			}
		}
		_calculateSymbolSizes.SetSizes(SymbolTable);
		_calculateSymbolOffsets.CalculateOffsets(SymbolTable);
		return SymbolTable;
	}

	SymbolDefinition AddSymbolDefinition(string sourceName, string instanceName, AccessModifier accessModifier, bool isFunctional, Token[] tokens) {
		var typeDef = new SymbolDefinition(sourceName, instanceName, SymbolTable.Types, -1, accessModifier, isFunctional, tokens);
		SymbolTable.Types.Children.Add(typeDef);
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
		var previousTypeTableTypes = SymbolTable.Types;
		SymbolTable.Types = AddSymbolDefinition("void", stmt.token.lexeme, AccessModifier.Private, true, new Token[] { stmt.token });
		foreach (Stmt statement in stmt.statements) {
			statement.Accept(this);
		}
		SymbolTable.Types = previousTypeTableTypes;
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
		var previousTypeTable = SymbolTable.Types;
		var tokLexemes = typeTokens.Select(tok => tok.lexeme);
		var lexeme = string.Join(".", tokLexemes);
		SymbolTable.Types = AddSymbolDefinition(lexeme, stmt.token.lexeme, AccessModifier.Public, true, typeTokens);
		foreach (var param in stmt.input) {
			param.Accept(this);
		}
		foreach (Stmt body in stmt.body) {
			body.Accept(this);
		}
		SymbolTable.Types = previousTypeTable;
		return null;
	}

	public object VisitClassStmt(Stmt.ClassDefinition stmt, object options = null) {
		var previousTypeTable = SymbolTable.Types;
		SymbolTable.Types = AddSymbolDefinition(stmt.token.lexeme, null, AccessModifier.Public, false, new Token[] { stmt.token });
		foreach (Stmt.VarDefinition memb in stmt.members) {
			memb.Accept(this, new Options { InClassDefinition = true });	
		}
		foreach (Stmt.Function method in stmt.methods) {
			method.Accept(this, new Options { InClassDefinition = true });
		}
		SymbolTable.Types = previousTypeTable;
		return null;
	}


	public object VisitLiteralExpr(Expr.Literal expr, object options = null) {
		return null;
	}

	public object VisitSetExpr(Expr.Set expr, object options = null) {
		expr.value.Accept(this, options);
		return null;
	}

	public object VisitReturnStmt(Stmt.Return stmt, object options = null) {
		stmt.value.Accept(this, options);
		return null;
	}

	public object VisitTypeStmt(Stmt.StmtType stmt, object options = null) {
		var names = stmt.info.Select(info => info.lexeme);
		return string.Join('.', names);
	}

	public object VisitUnaryExpr(Expr.Unary expr, object options = null) {
		return null;
	}

	public object VisitVariableExpr(Expr.Variable expr, object options = null) {
		return null;
	}

	public object VisitVarStmt(Stmt.VarDefinition stmt, object options = null) {
		var typeTokens = (Token[])stmt.stmtType.Accept(this);
		var opts = GetOptions(options);
		if (opts.InClassDefinition) {
			AddSymbolDefinition(typeTokens[0].lexeme, stmt.token.lexeme, AccessModifier.Public, false, typeTokens);
		} else {
			var tokLexemes = typeTokens.Select(tok => tok.lexeme);
			var lexeme = string.Join(".", tokLexemes);
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


public class CalculateSymbolSizes {
	public void SetSizes(TypeTable symbolTable) {
		while (!CheckAllSizesSet(symbolTable.Types)) {
			CalcSizes(symbolTable.Types);
		}
	}

	void CalcSizes(SymbolDefinition symbol) {    // repeat until all set...
		foreach (var child in symbol.Children) {
			if (child.Size == -1) {
				if (child.Children.Count > 0) {
					var total = CalcSize(child);
					child.Size = total;
				} else {
					SetSize(child);
				}
			}
		}
	}
	int CalcSize(SymbolDefinition symbol) {    // repeat until all set...
		foreach (var child in symbol.Children) {
			child.Size = SetSize(child);
		}
		var size = TotalOfChildSizes(symbol);
		return size;
	}
	int SetSize(SymbolDefinition symbol) {
		if (symbol.Size == -1) {
			var names = symbol.Tokens.Select(tok => tok.lexeme);
			var name = string.Join('.', names);
			var type = symbol.LookUpType(name);
			return type.Size;
		}
		return -1;
	}

	bool CheckAllSizesSet(SymbolDefinition symbol) {
		if (!ChildSizesSet(symbol) || symbol.TypeName != "" && symbol.Size == -1) {
			return false;
		}
		foreach (var child in symbol.Children) {
			if (child.Children.Count > 0) {
				return CheckAllSizesSet(child);
			}
		}
		return true;
	}

	bool ChildSizesSet(SymbolDefinition symbol, bool onlyMembers = false) {
		return symbol.Children.All(child => (child.Size >= 0) || !onlyMembers || (onlyMembers && child.IsMember));
	}
	int TotalOfChildSizes(SymbolDefinition symbol, bool onlyMembers = true) {
		if (ChildSizesSet(symbol, onlyMembers)) {
			return symbol.Children.Sum(child => child.Size >= 0 && (onlyMembers && child.IsMember || !onlyMembers) ? child.Size : 0);
		}
		return -1;
	}
}

public class CalculateSymbolOffsets {
	public void CalculateOffsets(TypeTable symbolTable) {
		foreach (var child in symbolTable.Types.Children) {
			CalcChildOffsets(child);
		}
	}

	void CalcChildOffsets(SymbolDefinition symbol) {
		int offset = 0;
		foreach(var child in symbol.Children) {
			if (child.IsMember) {
				child.Offset = offset;
				offset += child.Size;
				CalcChildOffsets(child);
			}
		}
	}
}
