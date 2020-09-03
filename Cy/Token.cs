
namespace Cy {
	public class Token {
		public enum Kind {
			// Single-character tokens.
			LEFT_PAREN, RIGHT_PAREN, LEFT_BRACE, RIGHT_BRACE,
			COMMA, DOT, MINUS, PLUS, SEMICOLON, SLASH, STAR,
			COLON, NEWLINE, HASH,

			// One or two character tokens.
			BANG, BANG_EQUAL, EQUAL, EQUAL_EQUAL, GREATER, GREATER_EQUAL, LESS, LESS_EQUAL,

			// types
			INT, INT8, INT16, INT32, INT64, INT128,
			FLOAT, FLOAT16, FLOAT32, FLOAT64, FLOAT128,
			STR,

			// Literals.
			IDENTIFIER, STR_LITERAL, INT_LITERAL, FLOAT_LITERAL,

			// Keywords.
			AND, CLASS, ELSE, FALSE, FUN, FOR, IF, NIL, OR, PRINT, RETURN, SUPER, THIS, TRUE, WHILE,

			EOF,
			ANY	// used for parsing, equals any type of token
		}


		public Kind type;		// type of token
		public string lexeme;	// actual text of token from source
		public object literal;	// value if a literal
		public int indent;		// indent of the line
		public int line;        // line number
		public int offset;      // index from start of line
		public string filename;      // index from start of line


		public Token(Kind type) {
			this.type = type;
			this.lexeme = "";
			this.literal = null;
			this.indent = 0;
			this.line = 0;
			this.offset = 0;
			this.filename = "";
		}

		public Token(Kind type, string lexeme, object literal, int indent, int line, int offset, string filename) {
			this.type = type;
			this.lexeme = lexeme;
			this.literal = literal;
			this.indent = indent;
			this.line = line;
			this.offset = offset;
			this.filename = filename;
		}

		public bool IsLiteral() {
			switch (type) {
				case Kind.STR_LITERAL:
				case Kind.INT_LITERAL:
				case Kind.FLOAT_LITERAL:
					return true;
				default:
					return false;
			}
		}
		public bool IsAnyType() {
			switch (type) {
				case Kind.INT:
				case Kind.INT8:
				case Kind.INT16:
				case Kind.INT32:
				case Kind.INT64:
				case Kind.INT128:
				case Kind.FLOAT:
				case Kind.FLOAT16:
				case Kind.FLOAT32:
				case Kind.FLOAT64:
				case Kind.FLOAT128:
				case Kind.IDENTIFIER:
				case Kind.STR:
					return true;
				default:
					return false;
			}
		}
		public bool IsBasicType() {
			switch (type) {
				case Kind.INT:
				case Kind.INT8:
				case Kind.INT16:
				case Kind.INT32:
				case Kind.INT64:
				case Kind.INT128:
				case Kind.FLOAT:
				case Kind.FLOAT16:
				case Kind.FLOAT32:
				case Kind.FLOAT64:
				case Kind.FLOAT128:
				case Kind.STR:
					return true;
				default:
					return false;
			}
		}
		public bool IsNumericType() {
			switch (type) {
				case Kind.INT:
				case Kind.INT8:
				case Kind.INT16:
				case Kind.INT32:
				case Kind.INT64:
				case Kind.INT128:
				case Kind.FLOAT:
				case Kind.FLOAT16:
				case Kind.FLOAT32:
				case Kind.FLOAT64:
				case Kind.FLOAT128:
					return true;
				default:
					return false;
			}
		}

		public override string ToString() {
			if (type == Kind.NEWLINE)
				return $"NewLine I{indent}-L{line}:O{offset}";
			if (type == Kind.EOF)
				return $"EOF I{indent}-L{line}:O{offset}";
			return $"{lexeme} {literal} I{indent}-L{line}:O{offset}";
		}




	}
}
