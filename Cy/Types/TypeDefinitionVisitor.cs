using Cy.Enums;
using Cy.Parsing;
using Cy.Parsing.Interfaces;
using Cy.Setup;
using Cy.TokenGenerator;

using System;
using System.Linq;

namespace Cy.Types;

/// <summary>Create the symbol table, given an AST.</summary>
public class TypeDefinitionVisitor : IExprVisitor, IStmtVisitor {
	readonly Config _config;
	public class Options {
		public bool InClassDefinition;
	}
	public TypeDefinitionTable TypeTable;

	static readonly string BUILTIN_FILENAME = "builtin";

	public TypeDefinitionVisitor(Config config) {
		_config = config;
		TypeTable = new TypeDefinitionTable();
		PopulateStandardTypes();
	}
	void PopulateStandardTypes() {
		var standardTypes = new TypeDefinition[] {
			new TypeDefinition("int", null, null, _config.DefaultIntSize * 8, _config.DefaultAlignment, AccessModifier.Public, BaseType.INT, new Token[] { new Token(TokenType.INT, "int", null, 0,0,0, BUILTIN_FILENAME) }),
			new TypeDefinition("int8", null, null, 8, _config.DefaultAlignment, AccessModifier.Public, BaseType.INT, new Token[] { new Token(TokenType.INT8, "int8", null, 0,0,0, BUILTIN_FILENAME) }),
			new TypeDefinition("int16", null, null, 16, _config.DefaultAlignment, AccessModifier.Public, BaseType.INT, new Token[] { new Token(TokenType.INT16, "int16", null, 0,0,0, BUILTIN_FILENAME) }),
			new TypeDefinition("int32", null, null, 32, _config.DefaultAlignment, AccessModifier.Public, BaseType.INT, new Token[] { new Token(TokenType.INT32, "int32", null, 0,0,0, BUILTIN_FILENAME) }),
			new TypeDefinition("int64", null, null, 64, _config.DefaultAlignment, AccessModifier.Public, BaseType.INT, new Token[] { new Token(TokenType.INT64, "int64", null, 0,0,0, BUILTIN_FILENAME) }),
			new TypeDefinition("int128", null, null, 128, _config.DefaultAlignment, AccessModifier.Public, BaseType.INT, new Token[] { new Token(TokenType.INT128, "int128", null, 0,0,0, BUILTIN_FILENAME) }),
			new TypeDefinition("float", null, null, _config.DefaultFloatSize * 8, _config.DefaultAlignment, AccessModifier.Public, BaseType.FLOAT, new Token[] { new Token(TokenType.FLOAT, "float", null, 0,0,0, BUILTIN_FILENAME) }),
			new TypeDefinition("float16", null, null, 16, _config.DefaultAlignment, AccessModifier.Public, BaseType.FLOAT, new Token[] { new Token(TokenType.FLOAT16, "float16", null, 0,0,0, BUILTIN_FILENAME) }),
			new TypeDefinition("float32", null, null, 32, _config.DefaultAlignment, AccessModifier.Public, BaseType.FLOAT, new Token[] { new Token(TokenType.FLOAT32, "float32", null, 0,0,0, BUILTIN_FILENAME) }),
			new TypeDefinition("float64", null, null, 64, _config.DefaultAlignment, AccessModifier.Public, BaseType.FLOAT, new Token[] { new Token(TokenType.FLOAT64, "float64", null, 0,0,0, BUILTIN_FILENAME) }),
			new TypeDefinition("float128", null, null, 128, _config.DefaultAlignment, AccessModifier.Public, BaseType.FLOAT, new Token[] { new Token(TokenType.FLOAT128, "float128", null, 0,0,0, BUILTIN_FILENAME) }),
			new TypeDefinition("bool", null, null, 1, _config.DefaultAlignment, AccessModifier.Public, BaseType.INT, new Token[] { new Token(TokenType.BOOL, "bool", null, 0,0,0, BUILTIN_FILENAME) }),
			new TypeDefinition("void", null, null, 0, _config.DefaultAlignment, AccessModifier.Public, BaseType.VOID, new Token[] { new Token(TokenType.VOID, "void", null, 0,0,0, BUILTIN_FILENAME) })
		};
		foreach (var typedef in standardTypes) {
			typedef.Parent = TypeTable.Global;
			TypeTable.Global.Children.Add(typedef);
		}
	}


	public TypeDefinitionTable Parse(Stmt[][] toplevel) {
		foreach (var stmt in toplevel) {
			foreach (var section in stmt) {
				section.Accept(this, null);
			}
		}
		CalculateTypeDefinitionSizes.SetSizes(TypeTable);
		CalculateTypeDefinitionOffsets.CalculateOffsets(TypeTable, _config.DefaultAlignment);
		return TypeTable;
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
		var previousTypeTableTypes = TypeTable.Global;
		var typeDef = new TypeDefinition("void", stmt.Token.lexeme, TypeTable.Global, -1, _config.DefaultAlignment, AccessModifier.Private, BaseType.VOID, new Token[] { stmt.Token });
		TypeTable.Global.Children.Add(typeDef);
		foreach (Stmt statement in stmt.statements) {
			statement.Accept(this);
		}
		TypeTable.Global = previousTypeTableTypes;
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
		var previousTypeTable = TypeTable.Global;
		var tokLexemes = typeTokens.Select(tok => tok.lexeme);
		var lexeme = string.Join(".", tokLexemes);
		var typeDef = new TypeDefinition(lexeme, stmt.Token.lexeme, TypeTable.Global, -1, _config.DefaultAlignment, AccessModifier.Public, BaseType.VOID, typeTokens);   // change BaseType.VOID
		TypeTable.Global.Children.Add(typeDef);
		TypeTable.Global = typeDef;
		foreach (var param in stmt.input) {
			param.Accept(this);
		}
		foreach (Stmt body in stmt.body) {
			body.Accept(this);
		}
		TypeTable.Global = previousTypeTable;
		return null;
	}

	public object VisitClassStmt(Stmt.ClassDefinition stmt, object options = null) {
		var previousTypeTable = TypeTable.Global;
		var typeDef = new TypeDefinition(stmt.Token.lexeme, null, TypeTable.Global, -1, _config.DefaultAlignment, AccessModifier.Public, BaseType.VOID, new Token[] { stmt.Token });   // change BaseType.VOID
		TypeTable.Global.Children.Add(typeDef);
		TypeTable.Global = typeDef;
		foreach (Stmt.Var memb in stmt.members) {
			memb.Accept(this, new Options { InClassDefinition = true });
		}
		foreach (Stmt.Function method in stmt.methods) {
			method.Accept(this, new Options { InClassDefinition = true });
		}
		TypeTable.Global = previousTypeTable;
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
			var typeDef = new TypeDefinition(typeTokens.Last().lexeme, stmt.Token.lexeme, TypeTable.Global, -1, _config.DefaultAlignment, AccessModifier.Public, BaseType.VOID, typeTokens);   // change BaseType.VOID
			TypeTable.Global.Children.Add(typeDef);
		}//else {
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
		var typeDef = new TypeDefinition(stmt.iterator.lexeme, null, TypeTable.Global, -1, _config.DefaultAlignment, AccessModifier.Private, BaseType.VOID, new Token[] { stmt.iteratorType.Token });   // change BaseType.VOID
		TypeTable.Global.Children.Add(typeDef);
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


public static class CalculateTypeDefinitionSizes {
	public static void SetSizes(TypeDefinitionTable symbolTable) {
		while (!CheckAllSizesSet(symbolTable.Global)) {
			CalcSizes(symbolTable.Global);
		}
	}

	static void CalcSizes(TypeDefinition symbol) {    // repeat until all set...
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
	static int CalcSize(TypeDefinition symbol) {    // repeat until all set...
		foreach (var child in symbol.Children) {
			child.ByteSize = SetSize(child);
		}
		var size = TotalOfChildSizes(symbol);
		return size;
	}
	static int SetSize(TypeDefinition symbol) {
		if (symbol.ByteSize == -1) {
			var names = symbol.Tokens.Select(tok => tok.lexeme);
			var name = string.Join('.', names);
			var type = symbol.LookUpType(name);
			symbol.ByteSize = type.ByteSize;
			return type.ByteSize;
		}
		return -1;
	}

	static bool CheckAllSizesSet(TypeDefinition symbol) {
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

	static bool ChildSizesSet(TypeDefinition symbol, bool onlyMembers = false) {
		return symbol.Children.All(child => child.ByteSize >= 0 || !onlyMembers || onlyMembers && child.IsMember);
	}
	static int TotalOfChildSizes(TypeDefinition symbol, bool onlyMembers = true) {
		if (ChildSizesSet(symbol, onlyMembers)) {
			return symbol.Children.Sum(child => child.ByteSize >= 0 && (onlyMembers && child.IsMember || !onlyMembers) ? child.ByteSize : 0);
		}
		return -1;
	}
}

public static class CalculateTypeDefinitionOffsets {
	public static void CalculateOffsets(TypeDefinitionTable symbolTable, int alignment) {
		foreach (var child in symbolTable.Global.Children) {
			CalcChildOffsets(child, alignment);
		}
	}
	static void CalcChildOffsets(TypeDefinition symbol, int alignment) {
		int offset = 0;
		foreach (var child in symbol.Children) {
			if (child.IsMember) {
				child.Offset = offset;
				offset += Offset(child.ByteSize, alignment);
				CalcChildOffsets(child, alignment);
			}
		}
	}
	static int Offset(int byteSize, int alignment) {
		var offset = Math.Max(byteSize, alignment);
		(int remainder, int quotient) = Math.DivRem(offset, alignment);
		if (remainder != 0) {
			offset = (quotient + 1) * 4;
		}
		return offset;
	}
}
