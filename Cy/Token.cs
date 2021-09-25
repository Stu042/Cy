using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace Cy {
	public class Token : ICloneable {
		public TokenType tokenType; // type of token
		public string lexeme;       // actual text of token from source
		public object literal;      // value if a literal
		public int indent;          // indent of the line
		public int line;            // line number
		public int offset;          // index from start of line
		public string filename;     // source filename

		public Token() {
			tokenType = TokenType.EOF;
			lexeme = "";
			literal = null;
			indent = 0;
			line = 0;
			offset = 0;
			filename = "";
		}
		public Token(TokenType type) {
			tokenType = type;
			lexeme = "";
			literal = null;
			indent = 0;
			line = 0;
			offset = 0;
			filename = "";
		}

		public Token(TokenType type, string lexeme, object literal, int indent, int line, int offset, string filename) {
			this.tokenType = type;
			this.lexeme = lexeme;
			this.literal = literal;
			this.indent = indent;
			this.line = line;
			this.offset = offset;
			this.filename = filename;
		}

		public bool IsLiteral() {
			switch (tokenType) {
				case TokenType.STR_LITERAL:
				case TokenType.INT_LITERAL:
				case TokenType.FLOAT_LITERAL:
					return true;
				default:
					return false;
			}
		}
		public bool IsAnyType() {
			switch (tokenType) {
				case TokenType.STR_LITERAL:
				case TokenType.INT_LITERAL:
				case TokenType.FLOAT_LITERAL:
				case TokenType.INT:
				case TokenType.INT8:
				case TokenType.INT16:
				case TokenType.INT32:
				case TokenType.INT64:
				case TokenType.INT128:
				case TokenType.FLOAT:
				case TokenType.FLOAT16:
				case TokenType.FLOAT32:
				case TokenType.FLOAT64:
				case TokenType.FLOAT128:
				case TokenType.IDENTIFIER:
				case TokenType.STR:
					return true;
				default:
					return false;
			}
		}
		public bool IsBasicType() {
			switch (tokenType) {
				case TokenType.INT:
				case TokenType.INT8:
				case TokenType.INT16:
				case TokenType.INT32:
				case TokenType.INT64:
				case TokenType.INT128:
				case TokenType.FLOAT:
				case TokenType.FLOAT16:
				case TokenType.FLOAT32:
				case TokenType.FLOAT64:
				case TokenType.FLOAT128:
				case TokenType.STR:
					return true;
				default:
					return false;
			}
		}
		public bool IsNumericType() {
			switch (tokenType) {
				case TokenType.INT:
				case TokenType.INT8:
				case TokenType.INT16:
				case TokenType.INT32:
				case TokenType.INT64:
				case TokenType.INT128:
				case TokenType.FLOAT:
				case TokenType.FLOAT16:
				case TokenType.FLOAT32:
				case TokenType.FLOAT64:
				case TokenType.FLOAT128:
					return true;
				default:
					return false;
			}
		}

		//public override string ToString() {
		//	string lexemeStr = lexeme;
		//	string literalStr = (literal == null) ? "null" : literal.ToString();
		//	if (tokenType == TokenType.NEWLINE) {
		//		lexemeStr = "\\n";
		//	}
		//	if (tokenType == TokenType.EOF) {
		//		lexemeStr = "\\0";
		//	}
		//	lexemeStr = lexemeStr.Replace("\r", "\\r");
		//	lexemeStr = lexemeStr.Replace("\n", "\\n");
		//	return $"new Cy.Token(Cy.TokenType.{tokenType}, \"{lexemeStr}\",  {literalStr}, {indent}, {line}, {offset}, test2_Filename),";
		//}
		public override string ToString() {
			string lexemeStr = lexeme;
			string literalStr = literal?.ToString();
			if (tokenType == TokenType.NEWLINE) {
				lexemeStr = "\\n";
			}
			if (tokenType == TokenType.EOF) {
				lexemeStr = "\\0";
			}
			if (lexeme == "\r") {
				lexemeStr = "\\r";
			}
			return $"{lexemeStr,-10} {tokenType,-20} {literalStr,-20} Indent:{indent,2} Line:{line,4} Offset:{offset,3}";
		}

		public Token Clone() {
			var tok = new Token();
			tok.tokenType = tokenType;
			tok.lexeme = lexeme;
			tok.literal = literal;
			tok.indent = indent;
			tok.line = line;
			tok.offset = offset;
			tok.filename = filename;
			return tok;
		}

		object ICloneable.Clone() {
			var tok = new Token();
			tok.tokenType = tokenType;
			tok.lexeme = lexeme;
			tok.literal = literal;
			tok.indent = indent;
			tok.line = line;
			tok.offset = offset;
			tok.filename = filename;
			return tok;
		}
	}
}
