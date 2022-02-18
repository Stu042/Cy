using Cy.Parsing.Interfaces;

using System;
using System.Collections.Generic;
using System.Linq;

namespace Cy.TokenGenerator;


// Write tokens to console
public static class Tokens {
	public static void DisplayAllTokens(List<List<Token>> allFilesTokens) {
		var tokenCount = allFilesTokens.Sum(tokens => tokens.Count);
		Console.WriteLine($"\n{tokenCount} Tokens:");
		foreach (var tokens in allFilesTokens) {
			Show(tokens);
		}
	}

	static void Show(List<Token> tokens) {
		foreach (Token token in tokens) {
			Console.WriteLine(token.ToFormattedString());
		}
	}
}


public class Scanner {
	readonly IErrorDisplay _display;
	readonly ScannerCursor _cursor;

	string filename;
	List<Token> tokens;
	int line;
	int currentIndent;


	static readonly Dictionary<string, TokenType> keywords = new() {
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
		{ "super", TokenType.SUPER },
	};


	static readonly Dictionary<string, TokenType> baseTypes = new() {
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


	public Scanner(ScannerCursor cursor, IErrorDisplay display) {
		_cursor = cursor;
		_display = display;
	}


	public List<Token> ScanTokens(string filename, string alltext) {
		tokens = new();
		this.filename = filename;
		line = 1;
		currentIndent = 0;
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
			case '%':
				AddToken(TokenType.PERCENT);
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
			case '\n': {
					AddToken(TokenType.NEWLINE);
					line++;
					currentIndent = 0;
					while (_cursor.Peek() == '\t' && !_cursor.IsAtEnd()) {
						currentIndent++;
						_cursor.Advance();
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
			AddToken(TokenType.FLOAT_LITERAL, double.Parse(_cursor.LexemeString()));
		} else {
			AddToken(TokenType.INT_LITERAL, int.Parse(_cursor.LexemeString()));
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
		tokens.Add(new Token(type, _cursor.LexemeString(), null, currentIndent, line, _cursor.Offset(), filename));
	}

	void AddToken(TokenType type, string lexeme) {
		tokens.Add(new Token(type, lexeme, null, currentIndent, line, _cursor.Offset(), filename));
	}

	void AddToken(TokenType type, object literal) {
		tokens.Add(new Token(type, _cursor.LexemeString(), literal, currentIndent, line, _cursor.Offset(), filename));
	}

	void Error(string message) {
		_display.Error(filename, line, _cursor.Offset(), _cursor.GetLineStr(), "Scanner error. " + message);
	}
}
