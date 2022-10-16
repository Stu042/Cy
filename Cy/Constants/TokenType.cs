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
	LOGICAL_AND, CLASS, ELSE, FALSE, FOR, EACH, IF, NULL, LOGICAL_OR, RETURN, SUPER, THIS, TRUE, WHILE,

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
		{ TokenType.INT, "int" },
		{ TokenType.INT8, "i8" },
		{ TokenType.INT16, "i16" },
		{ TokenType.INT32, "i32" },
		{ TokenType.INT64, "i64" },
		{ TokenType.INT128, "i128" },
		{ TokenType.FLOAT, "float" },
		{ TokenType.FLOAT16, "f16" },
		{ TokenType.FLOAT32, "f32" },
		{ TokenType.FLOAT64, "f64" },
		{ TokenType.FLOAT128, "f128" },
		{ TokenType.ASCII, "str" },
		{ TokenType.UTF8, "utf8" },
		{ TokenType.BOOL, "bool" },
		{ TokenType.VOID, "void" },
		{ TokenType.IDENTIFIER, "IDENTIFIER" },
		{ TokenType.STR_LITERAL, "STR_LITERAL" },
		{ TokenType.INT_LITERAL, "INT_LITERAL" },
		{ TokenType.FLOAT_LITERAL, "FLOAT_LITERAL" },
		{ TokenType.LOGICAL_AND, "&&" },
		{ TokenType.CLASS, "class" },
		{ TokenType.ELSE, "else" },
		{ TokenType.FALSE, "false" },
		{ TokenType.FOR, "for" },
		{ TokenType.EACH, "each" },
		{ TokenType.IF, "if" },
		{ TokenType.NULL, "null" },
		{ TokenType.LOGICAL_OR, "||" },
		{ TokenType.RETURN, "return" },
		{ TokenType.SUPER, "super" },
		{ TokenType.THIS, "this" },
		{ TokenType.TRUE, "true" },
		{ TokenType.WHILE, "while" },
		{ TokenType.EOF, "EOF" },
		{ TokenType.IGNORED, "IGNORED" }
	};
}
