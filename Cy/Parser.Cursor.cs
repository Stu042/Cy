using System.Collections.Generic;

namespace Cy {
	partial class Parser {
		/// <summary>Control the point in the token list we are looking at.</summary>
		class Cursor {
			int current;
			readonly List<Token> tokens;


			public Cursor(List<Token> tokens) {
				this.tokens = tokens;
				current = 0;
			}

			/// <summary>Is the expected token type next.</summary>
			public bool Check(TokenType expected) {
				while (tokens[current].tokenType == TokenType.IGNORED) {
					current++;
					if (IsAtEnd()) {
						return false;
					}
				}
				return tokens[current].tokenType == expected;
			}
			public bool CheckNext(TokenType expected, int offset = 1) {
				if (IsAtEnd(offset)) {
					return false;
				}
				return tokens[current + offset].tokenType == expected;
			}
			/// <summary>Are allExpected token types next.</summary>
			public bool Check(params TokenType[] allExpected) {
				int offset = 0;
				foreach (TokenType expected in allExpected) {
					if (!CheckNext(expected, offset)) {
						return false;
					}
					offset++;
				}
				return true;
			}

			public bool Match(TokenType expected) {
				if (Check(expected)) {
					Advance();
					return true;
				}
				return false;
			}

			public bool Match(params TokenType[] expected) {
				foreach (var exp in expected) {
					if (Check(exp)) {
						Advance();
						return true;
					}
				}
				return false;
			}
			public bool MatchNext(TokenType expected, int offset = 1) {
				if (Check(expected)) {
					Advance();
					return true;
				}
				return false;
			}

			/// <summary>Matches any type, including IDENTIFIER for a user defined type...</summary>
			public bool PeekAnyType() {
				if (tokens[current].IsAnyType()) {
					return true;
				}
				return false;
			}

			/// <summary>Does the line contain (in order, starting at current position) the specified token kinds - dont start with ANY</summary>
			public bool LineCheck(params TokenType[] allExpected) {
				int lineOffset = 0;
				int paramOffset = 0;
				while (tokens[current + lineOffset].tokenType != TokenType.NEWLINE && (current + lineOffset) < tokens.Count) {
					if (allExpected[paramOffset] == tokens[current + lineOffset].tokenType) {
						while (tokens[current + lineOffset].tokenType != TokenType.NEWLINE && (current + lineOffset) < tokens.Count) {
							if (allExpected[paramOffset] == tokens[current + lineOffset].tokenType) {
								paramOffset++;
								if (paramOffset >= allExpected.Length)
									return true;
							}
							lineOffset++;
						}
					}
					lineOffset++;
				}
				return false;
			}

			public Token Consume(TokenType expected, string message) {
				if (Check(expected)) {
					return Advance();
				}
				throw new ParseError(Peek(), message);
			}

			public bool IsAtEnd(int offset = 0) {
				return (current + offset) >= tokens.Count || tokens[current + offset].tokenType == TokenType.EOF || (current + offset) < 0;
			}

			public Token Advance() {
				return tokens[current++];
			}

			public Token Peek() {
				if (IsAtEnd()) {
					return new Token(TokenType.EOF);
				}
				return tokens[current];
			}
			public bool Peek(TokenType type) {
				if (IsAtEnd()) {
					return false;
				}
				return Match(type);
			}

			public Token PeekNext(int offset = 1) {
				if ((current + offset) >= tokens.Count)
					return new Token(TokenType.EOF);
				return tokens[current + offset];
			}
			public bool PeekNext(TokenType type, int offset = 1) {
				if ((current + offset) >= tokens.Count) {
					return false;
				}
				return MatchNext(type, offset);
			}

			public Token Previous() {
				if ((current - 1) >= tokens.Count)
					return new Token(TokenType.EOF);
				return tokens[current - 1];
			}


		}   // Cursor



	}
}
