using System.Linq;
using System.Collections.Generic;
using Cy.Scanner;

namespace Cy.Parser {

	/// <summary>Control the point in the token list we are looking at.
	/// Check - looks to see what TokenType is.
	/// Match - if we get he TokenType we expect jump to its position.
	/// Peek - return token at position.</summary>
	public class Cursor {
		public int current { get; private set; }
		readonly List<Token> tokens;


		public Cursor(List<Token> tokens) {
			var toks = tokens.FindAll(token => token.tokenType != TokenType.IGNORED);
			this.tokens = new List<Token>(toks.Count);
			bool lastWasNewline = true;
			foreach (Token token in toks) {
				if (token.tokenType == TokenType.EOF && !lastWasNewline) {
					this.tokens.Add(new Token(TokenType.NEWLINE));
				}
				if (token.tokenType != TokenType.NEWLINE) {
					lastWasNewline = false;
					this.tokens.Add(token);
				} else if (!lastWasNewline) {
					lastWasNewline = true;
					this.tokens.Add(token);
				}
			}
			current = 0;
		}


		/// <summary>Is the expected token type next.</summary>
		public bool IsCheck(TokenType expected) {
			if (IsAtEnd()) {
				return false;
			}
			return tokens[current].tokenType == expected;
		}

		/// <summary>Are allExpected token types next.</summary>
		public bool IsCheckAll(params TokenType[] allExpected) {
			int offset = 0;
			foreach (TokenType expected in allExpected) {
				if (!IsCheckAt(expected, offset)) {
					return false;
				}
				offset++;
			}
			return true;
		}

		public bool IsCheckAny(params TokenType[] allExpected) {
			var tokenType = Peek().tokenType;
			if (allExpected.Any(expected => expected == tokenType)) {
				return true;
			}
			return false;
		}

		/// <summary>Is expected token type at current + 1.</summary>
		public bool IsCheckNext(TokenType expected) {
			if (IsAtEnd(1)) {
				return false;
			}
			return tokens[current + 1].tokenType == expected;
		}

		/// <summary>Is expected token type at current + offset.</summary>
		public bool IsCheckAt(TokenType expected, int offset = 1) {
			if (IsAtEnd(offset)) {
				return false;
			}
			return tokens[current + offset].tokenType == expected;
		}

		/// <summary>Matches any type, including IDENTIFIER for a user defined type.</summary>
		public bool IsCheckAnyType() {
			return tokens[current].IsAnyType();
		}


		/// <summary>If token is expected return true and advance.</summary>
		public bool IsMatch(TokenType expected) {
			if (IsCheck(expected)) {
				Advance();
				return true;
			}
			return false;
		}

		/// <summary>If any of anyExpected TokenType is next return true and advance.</summary>
		public bool IsMatchAny(params TokenType[] anyExpected) {
			var nextTokenType = Peek().tokenType;
			if (anyExpected.Any(expected => expected == nextTokenType)) {
				Advance();
				return true;
			}
			return false;
		}


		public Token Consume(TokenType expected, string message) {
			if (IsCheck(expected)) {
				return Advance();
			}
			throw new ParseException(Peek(), message);
		}
		public Token ConsumeAny(string message, params TokenType[] expecteds) {
			if (IsCheckAny(expecteds)) {
				return Advance();
			}
			throw new ParseException(Peek(), message);
		}

		public bool IsAtEnd(int offset = 0) {
			return (current + offset) >= tokens.Count || (current + offset) < 0 || tokens[current + offset].tokenType == TokenType.EOF;
		}

		public Token Advance() {
			return tokens[current++];
		}

		public Token Peek() {
			if (IsAtEnd()) {
				return Token.EOF;
			}
			return tokens[current];
		}

		public Token PeekAt(int offset = 1) {
			if ((current + offset) >= tokens.Count) {
				return Token.EOF;
			}
			return tokens[current + offset];
		}
		public Token PeekNext() {
			int offset = 1;
			if ((current + offset) >= tokens.Count) {
				return Token.EOF;
			}
			return tokens[current + offset];
		}

		public Token Previous() {
			int index = current - 1;
			if (index >= tokens.Count || index < 0) {
				return Token.EOF;
			}
			return tokens[index];
		}
	}   // Cursor
}
