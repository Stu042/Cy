using System;
using System.Collections.Generic;
using System.Linq;

namespace Cy.Scanner {
	public partial class Scanner {
		string filename;
		List<Token> tokens;
		int line;
		int currentIndent;
		Cursor cursor;


		static readonly Dictionary<string, TokenType> keywords = new Dictionary<string, TokenType> {
			{ "this", TokenType.THIS },
			{ "if", TokenType.IF },
			{ "while", TokenType.WHILE },
			{ "for", TokenType.FOR },
			{ "each", TokenType.EACH },
			{ "else", TokenType.ELSE },
			{ "return", TokenType.RETURN },
			{ "false", TokenType.FALSE },
			{ "true", TokenType.TRUE },
			{ "null", TokenType.NULL },
			{ "print", TokenType.PRINT },
			{ "super", TokenType.SUPER },
		};


		static readonly Dictionary<string, TokenType> baseTypes = new Dictionary<string, TokenType> {
			{ "int", TokenType.INT },
			{ "int8", TokenType.INT8 },
			{ "int16", TokenType.INT16 },
			{ "int32", TokenType.INT32 },
			{ "int64", TokenType.INT64 },
			{ "int128", TokenType.INT128 },
			{ "float", TokenType.FLOAT },
			{ "float16", TokenType.FLOAT16 },
			{ "float32", TokenType.FLOAT32 },
			{ "float64", TokenType.FLOAT64 },
			{ "float128", TokenType.FLOAT128 },
			{ "ascii", TokenType.ASCII },
			{ "utf8", TokenType.UTF8 },
			{ "bool", TokenType.BOOL },
			{ "void", TokenType.VOID },
		};

		IErrorDisplay display;

		public Scanner(IErrorDisplay display) {
			this.display = display;
		}


		public List<Token> ScanTokens(string filename, string alltext) {
			tokens = new();
			cursor = new();
			this.filename = filename;
			line = 1;
			currentIndent = 0;
			cursor.NewFile(alltext);
			while (!cursor.IsAtEnd()) {
				cursor.Start();
				ScanToken();
			}
			AddToken(TokenType.EOF);
			return tokens;
		}


		void ScanToken() {
			char c = cursor.Advance();
			switch (c) {
				case '(':
					AddToken(TokenType.LEFT_PAREN);
					break;
				case ')':
					AddToken(TokenType.RIGHT_PAREN);
					break;
				case ',':
					AddToken(TokenType.COMMA);
					break;
				case '.':
					AddToken(TokenType.DOT);
					break;
				case '-':
					AddToken(cursor.Match('-') ? TokenType.MINUSMINUS : TokenType.MINUS);
					break;
				case '+':
					AddToken(cursor.Match('+') ? TokenType.PLUSPLUS : TokenType.PLUS);
					break;
				case ';':
					AddToken(TokenType.SEMICOLON);
					break;
				case ':':
					AddToken(TokenType.COLON);
					break;
				case '*':
					AddToken(TokenType.STAR);
					break;
				case '#':
					AddToken(TokenType.HASH);
					break;
				case '!':
					AddToken(cursor.Match('=') ? TokenType.BANG_EQUAL : TokenType.BANG);
					break;
				case '=':
					AddToken(cursor.Match('=') ? TokenType.EQUAL_EQUAL : TokenType.EQUAL);
					break;
				case '<':
					AddToken(cursor.Match('=') ? TokenType.LESS_EQUAL : TokenType.LESS);
					break;
				case '>':
					AddToken(cursor.Match('=') ? TokenType.GREATER_EQUAL : TokenType.GREATER);
					break;
				case '/':
					if (cursor.Match('/')) {            // if matching "//"
						while (cursor.Peek() != '\n' && !cursor.IsAtEnd()) {
							cursor.Advance();
						}
						AddToken(TokenType.IGNORED);
					} else if (cursor.Match('*')) {     // if matching "/*"
						while (cursor.Peek() != '*' && cursor.PeekNext() != '/' && !cursor.IsAtEnd()) {
							if (cursor.Peek() == '\n') {
								line++;
							}
							cursor.Advance();
						}
						if (cursor.IsAtEnd()) {
							Error("Multiline comment not terminated.");
							break;
						} else {
							cursor.Advance();
							cursor.Advance();
							AddToken(TokenType.IGNORED);
						}
					} else {
						AddToken(TokenType.SLASH);
					}
					break;
				case '\\':  // carry on new line
					cursor.Advance();
					while (char.IsWhiteSpace(cursor.Peek())) {
						if (cursor.Advance() == '\n') {
							line++;
						}
					}
					AddToken(TokenType.BACKSLASH);
					break;
				case '\t':
				case ' ':
				case '\r':
					AddToken(TokenType.IGNORED);                    // Ignore most whitespace.
					break;
				case '\n': {
						AddToken(TokenType.NEWLINE);
						line++;
						currentIndent = 0;
						while (cursor.Peek() == '\t' && !cursor.IsAtEnd()) {
							currentIndent++;
							cursor.Advance();
						}
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
			while (IsAlphaNumeric(cursor.Peek())) {
				cursor.Advance();
			}
			string text = new string(cursor.ToArray());
			TokenType type = TokenType.IDENTIFIER;
			if (keywords.ContainsKey(text)) {
				type = keywords[text];
			} else if (baseTypes.ContainsKey(text)) {
				type = baseTypes[text];
			}
			AddToken(type);
		}


		void Number() { // numbers can look like, 1, 12, 123 or 1.2, 1.23, 12.345, etc...
			while (IsDigit(cursor.Peek()))
				cursor.Advance();
			if (cursor.Peek() == '.' && IsDigit(cursor.PeekNext())) {   // Look for a fractional part.
				cursor.Advance();                                       // Consume the "."
				while (IsDigit(cursor.Peek())) {
					cursor.Advance();
				}
				AddToken(TokenType.FLOAT_LITERAL, double.Parse(cursor.ToString()));
			} else {
				AddToken(TokenType.INT_LITERAL, int.Parse(cursor.ToString()));
			}
		}


		void String() {
			while (cursor.Peek() != '"' && !cursor.IsAtEnd()) {
				if (cursor.Peek() == '\n') {
					line++;
				}
				cursor.Advance();
			}
			if (cursor.IsAtEnd()) {         // Unterminated string.
				Error("Unterminated string.");
				return;
			}
			cursor.Advance();               // The closing ".
			AddToken(TokenType.STR_LITERAL, new string(cursor.ToArray(1, -1)));         // Trim the surrounding quotes.
		}


		bool IsAlpha(char c) {
			return char.IsLetter(c) || c == '_' || c == '~';
		}

		bool IsAlphaNumeric(char c) {
			return IsAlpha(c) || IsDigit(c);
		}

		bool IsDigit(char c) {
			return char.IsDigit(c);
		}


		void AddToken(TokenType type) {
			tokens.Add(new Token(type, cursor.ToString(), null, currentIndent, line, cursor.Offset(), filename));
		}

		void AddToken(TokenType type, object literal) {
			tokens.Add(new Token(type, cursor.ToString(), literal, currentIndent, line, cursor.Offset(), filename));
		}

		void Error(string message) {
			display.Error(filename, line, cursor.Offset(), cursor.GetLineStr(), "Scanner error. " + message);
		}


		// Write tokens to console
		public void Show(List<Token> tokens) {
			foreach (Token token in tokens) {
				Console.WriteLine(token.ToFormattedString());
			}
		}

		public void DisplayAllTokens(List<List<Token>> allFilesTokens) {
			if (Config.Instance.DisplayTokens) {
				var tokenCount = allFilesTokens.Sum(tokens => tokens.Count);
				Console.WriteLine($"\n\n{tokenCount} Tokens:");
				foreach (var tokens in allFilesTokens) {
					this.Show(tokens);
				}
			}
		}
	}
}

