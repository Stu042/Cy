using Cy.Enums;
using Cy.Parsing;
using Cy.Parsing.Interfaces;
using Cy.Setup;
using Cy.TokenGenerator;

using System;
using System.Collections.Generic;
using System.Linq;

namespace Cy.Types;

public class LlvmType {
	public string Type;

	public static string GetLlvmType(TokenType tokenType) {
		return tokenType switch {
			TokenType.BOOL => "i1",
			TokenType.INT or TokenType.INT32 => "i32",
			TokenType.INT8 => "i8",
			TokenType.INT16 => "i16",
			TokenType.INT64 => "i64",
			TokenType.INT128 => "i128",
			TokenType.FLOAT or TokenType.FLOAT64 => "double",
			TokenType.FLOAT16 => "half",
			TokenType.FLOAT32 => "float",
			// x86_fp80
			TokenType.FLOAT128 => "fp128",
			TokenType.VOID => string.Empty,
			TokenType.ASCII => "i8*",
			_ => "i8*"
		};
	}
}


/// <summary>Add types to all expressions.</summary>
public class ExpressionTypeVisitor : IExprVisitor, IStmtVisitor {
	readonly Config _config;
	public class Options {
		public bool InClassDefinition;
	}
	TypeDefinitionTable _typeDefinitionTable;
	public Dictionary<Guid, TypeDefinition> ExpressionTypeTable;

	public ExpressionTypeVisitor(Config config, TypeDefinitionTable typeDefinitionTable) {
		_config = config;
		_typeDefinitionTable = typeDefinitionTable;
		ExpressionTypeTable = new Dictionary<Guid, TypeDefinition>();
	}

	public Dictionary<Guid, TypeDefinition> Parse(Stmt[][] toplevel) {
		foreach (var stmt in toplevel) {
			foreach (var section in stmt) {
				section.Accept(this, null);
			}
		}
		return ExpressionTypeTable;
	}

	TypeDefinition AddTypeDefinition(Guid expressionGuid, string functionName, AccessModifier accessModifier, Token[] tokens) {
		var typeDef = new TypeDefinition(null, functionName, null, -1, _config.DefaultAlignment, accessModifier, BaseType.VOID, tokens);
		ExpressionTypeTable.Add(expressionGuid, typeDef);
		return typeDef;
	}


	public object VisitGroupingExpr(Expr.Grouping expr, object options = null) {
		return null;
	}

	public object VisitAssignExpr(Expr.Assign expr, object options = null) {
		expr.Accept(this);
		return null;
	}

	public object VisitBinaryExpr(Expr.Binary expr, object options = null) {
		expr.left.Accept(this);
		expr.right.Accept(this);
		return null;
	}

	public object VisitBlockStmt(Stmt.Block stmt, object options = null) {
		var previousTypeTableTypes = TypeTable.Global;
		TypeTable.Global = AddTypeDefinition("void", stmt.Token.lexeme, AccessModifier.Private, new Token[] { stmt.Token });
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
		TypeTable.Global = AddTypeDefinition(lexeme, stmt.Token.lexeme, AccessModifier.Public, typeTokens);
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
		TypeTable.Global = AddTypeDefinition(stmt.Token.lexeme, null, AccessModifier.Public, new Token[] { stmt.Token });
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
			AddTypeDefinition(typeTokens.Last().lexeme, stmt.Token.lexeme, AccessModifier.Public, typeTokens);
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
		AddTypeDefinition(stmt.iterator.lexeme, "", AccessModifier.Private, new Token[] { stmt.iteratorType.Token });
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
