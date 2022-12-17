using System.Collections.Generic;

namespace Cy.Constants;


public enum TokenType {
	// Single-character tokens.
	LEFT_PAREN, RIGHT_PAREN, LEFT_BRACE, RIGHT_BRACE,
	COMMA, DOT, MINUS, PLUS, SEMICOLON, SLASH, BACKSLASH,
	STAR, COLON, NEWLINE, HASH,

	// One or two character tokens.
	BANG, BANG_EQUAL, EQUAL, EQUAL_EQUAL, GREATER, GREATER_EQUAL, LESS, LESS_EQUAL,
	MINUSMINUS, PLUSPLUS,

	// types
	INT, INT8, INT16, INT32, INT64, INT128,
	FLOAT, FLOAT16, FLOAT32, FLOAT64, FLOAT128,
	ASCII, UTF8, BOOL, VOID,

	IDENTIFIER,
	// Literals.
	STR_LITERAL, INT_LITERAL, FLOAT_LITERAL,

	// Keywords.
	LOGICAL_AND, ELSE, FALSE, FOR, EACH, IF, NULL, LOGICAL_OR, RETURN, THIS, TRUE, WHILE,

	EOF,
	IGNORED     // token to represent unimportant text for compilation, i.e. remarks etc
}


public static class TokenTypeStr {
	public static readonly Dictionary<TokenType, string> TokenTypeString = new() {	// todo move to enum?
		{ TokenType.LEFT_PAREN, "(" },
		{ TokenType.RIGHT_PAREN, ")" },
		{ TokenType.LEFT_BRACE, "{" },
		{ TokenType.RIGHT_BRACE, "}" },
		{ TokenType.COMMA, "," },
		{ TokenType.DOT, "." },
		{ TokenType.MINUS, "-" },
		{ TokenType.PLUS , "+" },
		{ TokenType.SEMICOLON, ";" },
		{ TokenType.SLASH , "/" },
		{ TokenType.BACKSLASH, "\\" },
		{ TokenType.STAR, "*" },
		{ TokenType.COLON, ":" },
		{ TokenType.NEWLINE, "\\n" },
		{ TokenType.HASH, "#" },
		{ TokenType.BANG, "!" },
		{ TokenType.BANG_EQUAL, "!=" },
		{ TokenType.EQUAL, "=" },
		{ TokenType.EQUAL_EQUAL, "==" },
		{ TokenType.GREATER, ">" },
		{ TokenType.GREATER_EQUAL, ">=" },
		{ TokenType.LESS, "<" },
		{ TokenType.LESS_EQUAL, "<=" },
		{ TokenType.MINUSMINUS, "--" },
		{ TokenType.PLUSPLUS, "++" },
		{ TokenType.INT, BasicTypeNames.Int },
		{ TokenType.INT8, BasicTypeNames.Int8 },
		{ TokenType.INT16, BasicTypeNames.Int16 },
		{ TokenType.INT32, BasicTypeNames.Int32 },
		{ TokenType.INT64, BasicTypeNames.Int64 },
		{ TokenType.INT128, BasicTypeNames.Int128 },
		{ TokenType.FLOAT, BasicTypeNames.Float },
		{ TokenType.FLOAT16, BasicTypeNames.Float16 },
		{ TokenType.FLOAT32, BasicTypeNames.Float32 },
		{ TokenType.FLOAT64, BasicTypeNames.Float64 },
		{ TokenType.FLOAT128, BasicTypeNames.Float128 },
		{ TokenType.ASCII, BasicTypeNames.Ascii },
		{ TokenType.UTF8, BasicTypeNames.Utf8 },
		{ TokenType.BOOL, BasicTypeNames.Bool },
		{ TokenType.VOID, BasicTypeNames.Void },
		{ TokenType.IDENTIFIER, "IDENTIFIER" },
		{ TokenType.STR_LITERAL, "STR_LITERAL" },
		{ TokenType.INT_LITERAL, "INT_LITERAL" },
		{ TokenType.FLOAT_LITERAL, "FLOAT_LITERAL" },
		{ TokenType.LOGICAL_AND, "&&" },
		{ TokenType.LOGICAL_OR, "||" },
		{ TokenType.ELSE, CommandNames.Else },
		{ TokenType.FALSE, CommandNames.False },
		{ TokenType.FOR, CommandNames.For },
		{ TokenType.EACH, CommandNames.Each },
		{ TokenType.IF, CommandNames.If },
		{ TokenType.NULL, CommandNames.Null },
		{ TokenType.RETURN, CommandNames.Return },
		{ TokenType.THIS, CommandNames.This },
		{ TokenType.TRUE, CommandNames.True },
		{ TokenType.WHILE, CommandNames.While },
		{ TokenType.EOF, "EOF" },
		{ TokenType.IGNORED, "IGNORED" }
	};
}
