namespace Cy.TokenGenerator {
	public enum TokenType {
		// Single-character tokens.
		LEFT_PAREN, RIGHT_PAREN, LEFT_BRACE, RIGHT_BRACE,
		COMMA, DOT, MINUS, PLUS, SEMICOLON, SLASH, BACKSLASH,
		STAR, COLON, NEWLINE, HASH, PERCENT,

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
		AND, CLASS, ELSE, FALSE, FOR, EACH, IF, NULL, OR, RETURN, SUPER, THIS, TRUE, WHILE,

		EOF,
		IGNORED     // token to represent unimportant text for compilation, i.e. remarks etc
	}
}
