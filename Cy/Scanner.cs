using System;
using System.Collections.Generic;
using System.IO;


namespace Cy {
	class Scanner {


		class Cursor {
			char[] source;
			public int start { get; private set; }
			int current;



			public void NewFile(string text) {
				source = text.ToCharArray();
				start = 0;
				current = 0;
			}


			public void Start() {
				start = current;
			}


			public bool Match(char expected) {
				if (IsAtEnd() || source[current] != expected)
					return false;
				current++;
				return true;
			}


			public bool IsAtEnd() {
				return current >= source.Length;
			}


			public char Advance() {
				return source[current++];
			}


			public char Peek() {
				if (IsAtEnd())
					return '\0';
				return source[current];
			}


			public char PeekNext() {
				if ((current + 1) >= source.Length)
					return '\0';
				return source[current + 1];
			}


			public char[] ToArray(int startOffset = 0, int endOffset = 0) {
				int len = (current + startOffset) - (start - endOffset);
				char[] res = new char[len];
				Array.Copy(source, start, res, 0, len);
				return res;
			}


			public int Offset() {
				int s = start;
				int c = 0;
				while (source[s] != '\n' && s > 0) {
					--s;
					c++;
				}
				c--;
				return c;
			}


			/// <summary>
			/// Get the string for this line.
			/// </summary>
			public string GetLineStr(int line) {
				int s = start;
				int l = s;
				while (source[s] != '\n' && s > 0) {
					s--;
					l++;
				}
				s++;
				while (source[l] != '\n' && l < source.Length)
					l++;
				char[] res = new char[l];
				Array.Copy(source, s, res, 0, l - s);
				return new string(res);
			}


			public override string ToString() {
				return new string(ToArray());

			}

		}


		string filename;
		List<Token> tokens;
		int line;
		int currentIndent;
		private Cursor cursor;
		bool inLineWrap;

		static readonly Dictionary<string, Token.Kind> keywords = new Dictionary<string, Token.Kind> {
			{ "else", Token.Kind.ELSE },
			{ "false", Token.Kind.FALSE },
			{ "for", Token.Kind.FOR },
			{ "if", Token.Kind.IF },
			{ "null", Token.Kind.NIL },
			{ "print", Token.Kind.PRINT },
			{ "return", Token.Kind.RETURN },
			{ "super", Token.Kind.SUPER },
			{ "this", Token.Kind.THIS },
			{ "true", Token.Kind.TRUE },
			{ "while", Token.Kind.WHILE },
		};


		static readonly Dictionary<string, Token.Kind> baseTypes = new Dictionary<string, Token.Kind> {
			{ "int", Token.Kind.INT },
			{ "i8", Token.Kind.INT8 },
			{ "i16", Token.Kind.INT16 },
			{ "i32", Token.Kind.INT32 },
			{ "i64", Token.Kind.INT64 },
			{ "i128", Token.Kind.INT128 },
			{ "float", Token.Kind.FLOAT },
			{ "f16", Token.Kind.FLOAT16 },
			{ "f32", Token.Kind.FLOAT32 },
			{ "f64", Token.Kind.FLOAT64 },
			{ "f128", Token.Kind.FLOAT128 },
			{ "str", Token.Kind.STR }
		};




		public List<Token> ScanTokens(string filename, string alltext) {
			this.filename = filename;
			tokens = new List<Token>();
			line = 1;
			currentIndent = 0;
			cursor = new Cursor();
			cursor.NewFile(alltext);
			inLineWrap = false;
			while (!cursor.IsAtEnd()) {
				cursor.Start();
				ScanToken();
			}
			return tokens;
		}


		/// <summary>
		/// Removes extra new lines
		/// </summary>
		void Tidy() {
			List<Token> tidyTokens = new List<Token>(tokens.Count);
			Token prevtok = tokens[0];
			foreach (Token tok in tokens) {
				if (tok.type == Token.Kind.NEWLINE && prevtok.type == Token.Kind.NEWLINE) {
					// skip the duplicate new lines
				} else {
					tidyTokens.Add(tok);
				}
				prevtok = tok;
			}
			tokens = tidyTokens;
		}



		
		void ScanToken() {
			char c = cursor.Advance();
			switch (c) {
				case '(':
					AddToken(Token.Kind.LEFT_PAREN);
					break;
				case ')':
					AddToken(Token.Kind.RIGHT_PAREN);
					break;
				case ',':
					AddToken(Token.Kind.COMMA);
					break;
				case '.':
					AddToken(Token.Kind.DOT);
					break;
				case '-':
					AddToken(Token.Kind.MINUS);
					break;
				case '+':
					AddToken(Token.Kind.PLUS);
					break;
				case ';':
					AddToken(Token.Kind.SEMICOLON);
					break;
				case ':':
					AddToken(Token.Kind.COLON);
					break;
				case '*':
					AddToken(Token.Kind.STAR);
					break;
				case '#':
					AddToken(Token.Kind.HASH);
					break;
				case '!':
					AddToken(cursor.Match('=') ? Token.Kind.BANG_EQUAL : Token.Kind.BANG);
					break;
				case '=':
					AddToken(cursor.Match('=') ? Token.Kind.EQUAL_EQUAL : Token.Kind.EQUAL);
					break;
				case '<':
					AddToken(cursor.Match('=') ? Token.Kind.LESS_EQUAL : Token.Kind.LESS);
					break;
				case '>':
					AddToken(cursor.Match('=') ? Token.Kind.GREATER_EQUAL : Token.Kind.GREATER);
					break;
				case '/':
					if (cursor.Match('/')) {
						while (cursor.Peek() != '\n' && !cursor.IsAtEnd())
							cursor.Advance();
					} else if (cursor.Match('*')) {
						while (cursor.Peek() != '*' && cursor.PeekNext() != '/' && !cursor.IsAtEnd()) {
							if (cursor.Peek() == '\n')
								line++;
							cursor.Advance();
						}
						if (cursor.Peek() == '*' && cursor.PeekNext() == '/') {
							cursor.Advance();
							cursor.Advance();
							break;
						} else if (cursor.IsAtEnd()) {
							Error("Multiline comment not terminated.");
							break;
						}
						cursor.Advance();
					} else {
						AddToken(Token.Kind.SLASH);
					}
					break;
				case '\\':  // carry on new line
					inLineWrap = true;
					cursor.Advance();
					while (char.IsWhiteSpace(cursor.Peek()))
						if (cursor.Advance() == '\n')
							line++;
					break;
				case '\t':
				case ' ':
				case '\r':
					// Ignore most whitespace.
					break;
				case '\n': {
					line++;
					if (inLineWrap) {
						inLineWrap = false;
						break;
					}
					AddToken(Token.Kind.NEWLINE, "", null);
					int indent = 0;
					while (cursor.Peek() == '\t' && !cursor.IsAtEnd()) {
						indent++;
						cursor.Advance();
					}
					currentIndent = indent;
				}
				break;
				case '"':
					String();
					break;
				default:
					if (IsDigit(c)) {
						Number();
					} else if (IsAlpha(c)) {
						Identifier();
					} else {
						Error("Unexpected character.");
					}
					break;
			}
		}



		void Identifier() {
			while (IsAlphaNumeric(cursor.Peek()))
				cursor.Advance();
			// See if the identifier is a reserved word.
			string text = new string(cursor.ToArray());
			Token.Kind type = Token.Kind.IDENTIFIER;
			if (keywords.ContainsKey(text))
				type = keywords[text];
			else if (baseTypes.ContainsKey(text))
				type = baseTypes[text];
			AddToken(type);
		}


		void Number() {
			while (IsDigit(cursor.Peek()))
				cursor.Advance();
			if (cursor.Peek() == '.' && IsDigit(cursor.PeekNext())) {   // Look for a fractional part.
				cursor.Advance();                                       // Consume the "."
				while (IsDigit(cursor.Peek()))
					cursor.Advance();
				AddToken(Token.Kind.FLOAT_LITERAL, double.Parse(cursor.ToString()));
			} else {
				AddToken(Token.Kind.INT_LITERAL, long.Parse(cursor.ToString()));
			}
		}


		void String() {
			while (cursor.Peek() != '"' && !cursor.IsAtEnd()) {
				if (cursor.Peek() == '\n')
					line++;
				cursor.Advance();
			}
			if (cursor.IsAtEnd()) {         // Unterminated string.
				Error("Unterminated string.");
				return;
			}
			cursor.Advance();               // The closing ".
			AddToken(Token.Kind.STR_LITERAL, new string(cursor.ToArray(1, -1)));         // Trim the surrounding quotes.
		}




		bool IsAlpha(char c) {
			return char.IsLetter(c) || c == '_';
		}


		bool IsAlphaNumeric(char c) {
			return IsAlpha(c) || IsDigit(c);
		}


		bool IsDigit(char c) {
			return char.IsDigit(c);
		}




		void AddToken(Token.Kind type) {
			tokens.Add(new Token(type, cursor.ToString(), null, currentIndent, line, cursor.Offset(), filename));
		}

		void AddToken(Token.Kind type, object literal) {
			tokens.Add(new Token(type, cursor.ToString(), literal, currentIndent, line, cursor.Offset(), filename));
		}

		void AddToken(Token.Kind type, string text, object literal) {
			tokens.Add(new Token(type, text, literal, currentIndent, line, cursor.Offset(), filename));
		}




		void Error(string message) {
			Display.Error(filename, line, cursor.Offset(), cursor.GetLineStr(line), "Scanner error: " + message);
		}


		// Write tokens to console
		public void Show(List<Token> tokens) {
			foreach (Token token in tokens)
				Console.WriteLine(token);
		}

	}
}

