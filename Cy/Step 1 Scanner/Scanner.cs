using System;
using System.Collections.Generic;
using System.IO;


namespace Cy {
	class Scanner {
		int line = 1;
		int currentIndent = 0;
		bool inLineWrap = false;
		string filename;
		List<Token> tokens;
		SourceFile sourceFile;

		// keywords
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

		// primitive types
		static readonly Dictionary<string, Token.Kind> baseTypes = new Dictionary<string, Token.Kind> {
			{ "int", Token.Kind.INT },
			{ "int8", Token.Kind.INT8 },
			{ "int16", Token.Kind.INT16 },
			{ "int32", Token.Kind.INT32 },
			{ "int64", Token.Kind.INT64 },
			{ "int128", Token.Kind.INT128 },
			{ "float", Token.Kind.FLOAT },
			{ "float16", Token.Kind.FLOAT16 },
			{ "float32", Token.Kind.FLOAT32 },
			{ "float64", Token.Kind.FLOAT64 },
			{ "float128", Token.Kind.FLOAT128 },
			{ "str", Token.Kind.STR },
			{ "void", Token.Kind.VOID },
		};

		// run the scanner and return an array of tokens
		public Token[] ScanTokens(string filename, string alltext) {
			this.filename = filename;
			tokens = new List<Token>();
			sourceFile = new SourceFile(alltext);
			while (!sourceFile.IsAtEnd()) {
				sourceFile.Start();
				ScanToken();
			}
			AddToken(Token.Kind.EOF);
			Tidy();
			return tokens.ToArray();
		}

		// Removes extra new lines
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

		// scan next part of sourcefile for token
		void ScanToken() {
			char c = sourceFile.Advance();
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
					AddToken(sourceFile.IsMatch('=') ? Token.Kind.BANG_EQUAL : Token.Kind.BANG);
					break;
				case '=':
					AddToken(sourceFile.IsMatch('=') ? Token.Kind.EQUAL_EQUAL : Token.Kind.EQUAL);
					break;
				case '<':
					AddToken(sourceFile.IsMatch('=') ? Token.Kind.LESS_EQUAL : Token.Kind.LESS);
					break;
				case '>':
					AddToken(sourceFile.IsMatch('=') ? Token.Kind.GREATER_EQUAL : Token.Kind.GREATER);
					break;
				case '/':
					if (sourceFile.IsMatch('/')) {
						while (sourceFile.Peek() != '\n' && !sourceFile.IsAtEnd())
							sourceFile.Advance();
					} else if (sourceFile.IsMatch('*')) {
						while (sourceFile.Peek() != '*' && sourceFile.PeekNext() != '/' && !sourceFile.IsAtEnd()) {
							if (sourceFile.Peek() == '\n')
								line++;
							sourceFile.Advance();
						}
						if (sourceFile.Peek() == '*' && sourceFile.PeekNext() == '/') {
							sourceFile.Advance();
							sourceFile.Advance();
							break;
						} else if (sourceFile.IsAtEnd()) {
							Error("Multiline comment not terminated.");
							break;
						}
						sourceFile.Advance();
					} else {
						AddToken(Token.Kind.SLASH);
					}
					break;
				case '\\':		// carry on new line
					inLineWrap = true;
					sourceFile.Advance();
					while (char.IsWhiteSpace(sourceFile.Peek()))
						if (sourceFile.Advance() == '\n')
							line++;
					break;
				case '\t':
				case ' ':
				case '\r':
					break;		// Ignore most whitespace.
				case '\n': {
					line++;
					if (inLineWrap) {
						inLineWrap = false;
						break;
					}
					AddToken(Token.Kind.NEWLINE);
					int indent = 0;
					while (sourceFile.Peek() == '\t' && !sourceFile.IsAtEnd()) {
						indent++;
						sourceFile.Advance();
					}
					currentIndent = indent;
				}
				break;
				case '"':
					String();
					break;
				default:
					if (IsDigit(c))
						Number();
					else if (IsAlpha(c))
						Identifier();
					else
						Error("Unexpected character.");
					break;
			}
		}

		// make a token from an identifier
		void Identifier() {
			while (IsAlphaNumeric(sourceFile.Peek()))
				sourceFile.Advance();
			string text = new string(sourceFile.ToArray());
			Token.Kind type = Token.Kind.IDENTIFIER;
			if (keywords.ContainsKey(text))
				type = keywords[text];
			else if (baseTypes.ContainsKey(text))
				type = baseTypes[text];
			AddToken(type);
		}

		// make a token from a literal number
		void Number() {
			while (IsDigit(sourceFile.Peek()))
				sourceFile.Advance();
			if (sourceFile.Peek() == '.' && IsDigit(sourceFile.PeekNext())) {   // Look for a fractional part.
				sourceFile.Advance();                                       // Consume the "."
				while (IsDigit(sourceFile.Peek()))
					sourceFile.Advance();
				AddToken(Token.Kind.FLOAT_LITERAL, double.Parse(sourceFile.ToString()));
			} else {
				AddToken(Token.Kind.INT_LITERAL, int.Parse(sourceFile.ToString()));
			}
		}


		void String() {
			while (sourceFile.Peek() != '"' && !sourceFile.IsAtEnd()) {
				if (sourceFile.Peek() == '\n')
					line++;
				sourceFile.Advance();
			}
			if (sourceFile.IsAtEnd()) {         // Unterminated string.
				Error("Unterminated string.");
				return;
			}
			sourceFile.Advance();               // The closing ".
			AddToken(Token.Kind.STR_LITERAL, new string(sourceFile.ToArray(1, -1)));         // Trim the surrounding quotes.
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
			tokens.Add(new Token(type, sourceFile.ToString(), null, currentIndent, line, sourceFile.OffsetWithinLine(), filename));
		}

		void AddToken(Token.Kind type, object literal) {
			tokens.Add(new Token(type, sourceFile.ToString(), literal, currentIndent, line, sourceFile.OffsetWithinLine(), filename));
		}


		// display a scanner error
		void Error(string message) {
			Display.Error(filename, line, sourceFile.OffsetWithinLine(), sourceFile.GetLineStr(line), "Scanner error: " + message);
		}


		// Write tokens to console
		public void Show(List<Token> tokens) {
			foreach (Token token in tokens)
				Console.WriteLine(token);
		}

	}
}

