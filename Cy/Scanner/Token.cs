﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace Cy {
	public class Token {
		public TokenType tokenType; // type of token
		public string lexeme;       // actual text of token from source
		public object literal;      // value if a literal
		public int indent;          // indent of the line
		public int line;            // line number
		public int offset;          // index from start of line
		public string filename;     // source filename

		public static readonly Token EOF = new Token(TokenType.EOF);

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
			return tokenType switch {
				TokenType.STR_LITERAL or TokenType.INT_LITERAL or TokenType.FLOAT_LITERAL => true,
				_ => false,
			};
		}
		public bool IsAnyType() {
			return tokenType switch {
				TokenType.STR_LITERAL or TokenType.INT_LITERAL or TokenType.FLOAT_LITERAL or TokenType.INT or TokenType.INT8 or TokenType.INT16 or TokenType.INT32 or TokenType.INT64 or TokenType.INT128 or TokenType.FLOAT or TokenType.FLOAT16 or TokenType.FLOAT32 or TokenType.FLOAT64 or TokenType.FLOAT128 or TokenType.IDENTIFIER or TokenType.STR => true,
				_ => false,
			};
		}
		public bool IsBasicType() {
			return tokenType switch {
				TokenType.INT or TokenType.INT8 or TokenType.INT16 or TokenType.INT32 or TokenType.INT64 or TokenType.INT128 or TokenType.FLOAT or TokenType.FLOAT16 or TokenType.FLOAT32 or TokenType.FLOAT64 or TokenType.FLOAT128 or TokenType.STR => true,
				_ => false,
			};
		}
		public bool IsNumericType() {
			return tokenType switch {
				TokenType.INT or TokenType.INT8 or TokenType.INT16 or TokenType.INT32 or TokenType.INT64 or TokenType.INT128 or TokenType.FLOAT or TokenType.FLOAT16 or TokenType.FLOAT32 or TokenType.FLOAT64 or TokenType.FLOAT128 => true,
				_ => false,
			};
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
			var tok = new Token {
				tokenType = tokenType,
				lexeme = lexeme,
				literal = literal,
				indent = indent,
				line = line,
				offset = offset,
				filename = filename
			};
			return tok;
		}
	}
}