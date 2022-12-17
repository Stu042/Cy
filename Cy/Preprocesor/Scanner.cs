using Cy.Constants;
using Cy.Preprocesor.Interfaces;

using System;
using System.Collections.Generic;
using System.Linq;

namespace Cy.Preprocesor;


public class Scanner {
	readonly IErrorDisplay _display;
	readonly ScannerCursor _cursor;

	string filename;
	List<Token> tokens;
	int line;


	static readonly Dictionary<string, TokenType> keywords = new() {
		{ CommandNames.This, TokenType.THIS },
		{ CommandNames.If, TokenType.IF },
		{ CommandNames.While, TokenType.WHILE },
		{ CommandNames.For, TokenType.FOR },
		{ CommandNames.Each, TokenType.EACH },
		{ CommandNames.Else, TokenType.ELSE },
		{ CommandNames.Return, TokenType.RETURN },
		{ CommandNames.False, TokenType.FALSE },
		{ CommandNames.True, TokenType.TRUE },
		{ CommandNames.Null, TokenType.NULL },
	};


	static readonly Dictionary<string, TokenType> baseTypes = new() {
		{ BasicTypeNames.Int, TokenType.INT },
		{ BasicTypeNames.Int8, TokenType.INT8 },
		{ BasicTypeNames.Int16, TokenType.INT16 },
		{ BasicTypeNames.Int32, TokenType.INT32 },
		{ BasicTypeNames.Int64, TokenType.INT64 },
		{ BasicTypeNames.Int128, TokenType.INT128 },
		{ BasicTypeNames.Float, TokenType.FLOAT },
		{ BasicTypeNames.Float16, TokenType.FLOAT16 },
		{ BasicTypeNames.Float32, TokenType.FLOAT32 },
		{ BasicTypeNames.Float64, TokenType.FLOAT64 },
		{ BasicTypeNames.Float128, TokenType.FLOAT128 },
		{ BasicTypeNames.Ascii, TokenType.ASCII },
		{ BasicTypeNames.Utf8, TokenType.UTF8 },
		{ BasicTypeNames.Bool, TokenType.BOOL },
		{ BasicTypeNames.Void, TokenType.VOID },
	};


	public Scanner(ScannerCursor cursor, IErrorDisplay display) {
		_cursor = cursor;
		_display = display;
	}


	public List<Token> ScanTokens(string filename, string alltext) {
		tokens = new();
		this.filename = filename;
		line = 1;
		_cursor.NewFile(alltext);
		while (!_cursor.IsAtEnd()) {
			_cursor.Start();
			ScanToken();
		}
		AddToken(TokenType.EOF, "\0");
		return tokens;
	}


	void ScanToken() {
		char c = _cursor.Advance();
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
				AddToken(_cursor.Match('-') ? TokenType.MINUSMINUS : TokenType.MINUS);
				break;
			case '+':
				AddToken(_cursor.Match('+') ? TokenType.PLUSPLUS : TokenType.PLUS);
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
			case '{':
				AddToken(TokenType.LEFT_BRACE);
				break;
			case '}':
				AddToken(TokenType.RIGHT_BRACE);
				break;
			case '!':
				AddToken(_cursor.Match('=') ? TokenType.BANG_EQUAL : TokenType.BANG);
				break;
			case '=':
				AddToken(_cursor.Match('=') ? TokenType.EQUAL_EQUAL : TokenType.EQUAL);
				break;
			case '<':
				AddToken(_cursor.Match('=') ? TokenType.LESS_EQUAL : TokenType.LESS);
				break;
			case '>':
				AddToken(_cursor.Match('=') ? TokenType.GREATER_EQUAL : TokenType.GREATER);
				break;
			case '/':
				if (_cursor.Match('/')) {            // if matching "//"
					while (_cursor.Peek() != '\n' && !_cursor.IsAtEnd()) {
						_cursor.Advance();
					}
					AddToken(TokenType.IGNORED);
				} else if (_cursor.Match('*')) {     // if matching "/*"
					while (_cursor.Peek() != '*' && _cursor.PeekNext() != '/' && !_cursor.IsAtEnd()) {
						if (_cursor.Peek() == '\n') {
							line++;
						}
						_cursor.Advance();
					}
					if (_cursor.IsAtEnd()) {
						Error("Multiline comment not terminated.");
						break;
					} else {
						_cursor.Advance();
						_cursor.Advance();
						AddToken(TokenType.IGNORED);
					}
				} else {
					AddToken(TokenType.SLASH);
				}
				break;
			case '\\':  // carry on new line
				_cursor.Advance();
				while (char.IsWhiteSpace(_cursor.Peek())) {
					if (_cursor.Advance() == '\n') {
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
			case '\n':
				AddToken(TokenType.NEWLINE);
				line++;
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
		while (IsAlphaNumeric(_cursor.Peek())) {
			_cursor.Advance();
		}
		var text = new string(_cursor.ToArray());
		var type = TokenType.IDENTIFIER;
		if (keywords.ContainsKey(text)) {
			type = keywords[text];
		} else if (baseTypes.ContainsKey(text)) {
			type = baseTypes[text];
		}
		AddToken(type);
	}


	void Number() { // numbers can look like, 1, 12, 123 or 1.2, 1.23, 12.345, etc...
		while (IsDigit(_cursor.Peek()))
			_cursor.Advance();
		if (_cursor.Peek() == '.' && IsDigit(_cursor.PeekNext())) {   // Look for a fractional part.
			_cursor.Advance();                                       // Consume the "."
			while (IsDigit(_cursor.Peek())) {
				_cursor.Advance();
			}
			AddToken(TokenType.FLOAT_LITERAL, double.Parse(_cursor.ToString()));
		} else {
			AddToken(TokenType.INT_LITERAL, int.Parse(_cursor.ToString()));
		}
	}


	void String() {
		while (_cursor.Peek() != '"' && !_cursor.IsAtEnd()) {
			if (_cursor.Peek() == '\n') {
				line++;
			}
			_cursor.Advance();
		}
		if (_cursor.IsAtEnd()) {         // Unterminated string.
			Error("Unterminated string.");
			return;
		}
		_cursor.Advance();               // The closing ".
		AddToken(TokenType.STR_LITERAL, new string(_cursor.ToArray(1, -1)));         // Trim the surrounding quotes.
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
		tokens.Add(new Token(type, _cursor.ToString(), null, line, _cursor.Offset(), filename));
	}

	void AddToken(TokenType type, string lexeme) {
		tokens.Add(new Token(type, lexeme, null, line, _cursor.Offset(), filename));
	}

	void AddToken(TokenType type, object literal) {
		tokens.Add(new Token(type, _cursor.ToString(), literal, line, _cursor.Offset(), filename));
	}

	void Error(string message) {
		_display.Error(filename, line, _cursor.Offset(), _cursor.GetLineStr(), "Scanner error, " + message);
	}
}
