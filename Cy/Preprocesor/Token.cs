using Cy.Constants;

namespace Cy.Preprocesor;

public class Token {
	public TokenType TokenType; // type of token
	public string Lexeme;       // actual text of token from source
	public object Literal;      // value if a literal
	public int Line;            // line number
	public int Offset;          // index from start of line
	public string Filename;     // source filename

	public static readonly Token EOF = new Token(TokenType.EOF);

	public Token() {
		TokenType = TokenType.EOF;
		Lexeme = "";
		Literal = null;
		Line = 0;
		Offset = 0;
		Filename = "";
	}
	public Token(TokenType type) {
		TokenType = type;
		Lexeme = "";
		Literal = null;
		Line = 0;
		Offset = 0;
		Filename = "";
	}
	public Token(string lexeme, TokenType type) {
		TokenType = type;
		Lexeme = lexeme;
		Literal = null;
		Line = 0;
		Offset = 0;
		Filename = "";
	}

	public Token(TokenType type, string lexeme, object literal, int line, int offset, string filename) {
		TokenType = type;
		Lexeme = lexeme;
		Literal = literal;
		Line = line;
		Offset = offset;
		Filename = filename;
	}

	public bool IsLiteral() {
		return TokenType switch {
			TokenType.STR_LITERAL or TokenType.INT_LITERAL or TokenType.FLOAT_LITERAL => true,
			_ => false,
		};
	}
	public bool IsAnyType() {
		return TokenType switch {
			TokenType.STR_LITERAL or TokenType.INT_LITERAL or TokenType.FLOAT_LITERAL or
			TokenType.INT or TokenType.INT8 or TokenType.INT16 or TokenType.INT32 or
			TokenType.INT64 or TokenType.INT128 or TokenType.FLOAT or TokenType.FLOAT16
			or TokenType.FLOAT32 or TokenType.FLOAT64 or TokenType.FLOAT128 or TokenType.IDENTIFIER
			or TokenType.ASCII or TokenType.UTF8 or TokenType.VOID or TokenType.BOOL => true,
			_ => false,
		};
	}
	public bool IsBasicType() {
		return TokenType switch {
			TokenType.INT or TokenType.INT8 or TokenType.INT16 or TokenType.INT32 or TokenType.INT64
			or TokenType.INT128 or TokenType.FLOAT or TokenType.FLOAT16 or TokenType.FLOAT32 or
			TokenType.FLOAT64 or TokenType.FLOAT128 or TokenType.ASCII or TokenType.VOID or TokenType.UTF8 or TokenType.BOOL => true,
			_ => false,
		};
	}
	public bool IsNumericType() {
		return TokenType switch {
			TokenType.INT or TokenType.INT8 or TokenType.INT16 or TokenType.INT32 or TokenType.INT64 or
			TokenType.INT128 or TokenType.FLOAT or TokenType.FLOAT16 or TokenType.FLOAT32 or TokenType.FLOAT64
			or TokenType.FLOAT128 or TokenType.BOOL => true,
			_ => false,
		};
	}

	public override string ToString() {
		string lexemeStr = Lexeme;
		string literalStr = Literal?.ToString();
		if (TokenType == TokenType.NEWLINE) {
			lexemeStr = "\\n";
		}
		if (TokenType == TokenType.EOF) {
			lexemeStr = "\\0";
		}
		if (Lexeme == "\r") {
			lexemeStr = "\\r";
		}
		if (Lexeme == "\t") {
			lexemeStr = "\\t";
		}
		if (string.IsNullOrEmpty(literalStr)) {
			return $"Lexeme: {lexemeStr}, Type: {TokenType}, Line:{Line}, Offset:{Offset}";
		}
		return $"Lexeme: {lexemeStr}, Type: {TokenType}, Literal: {literalStr}, Line:{Line}, Offset:{Offset}";
	}

	public Token Clone() {
		var tok = new Token {
			TokenType = TokenType,
			Lexeme = Lexeme,
			Literal = Literal,
			Line = Line,
			Offset = Offset,
			Filename = Filename
		};
		return tok;
	}
}
