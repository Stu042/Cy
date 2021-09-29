﻿using System.Collections.Generic;



namespace Cy.Parser {

	/// <summary>Control the point in the token list we are looking at.
	/// Check - looks to see what TokenType is.
	/// Match - if we get he TokenType we expect jump to its position.
	/// Peek - return token at position.</summary>
	public class Cursor {
		public int current { get; private set; }
		readonly List<Token> tokens;
		readonly List<Token> fullTokens;


		public Cursor(List<Token> tokens) {
			this.fullTokens = tokens;
			this.tokens = tokens.FindAll(token => token.tokenType != TokenType.IGNORED);
			current = 0;
		}


		/// <summary>Is the expected token type next.</summary>
		public bool IsCheck(TokenType expected) {
			if (IsAtEnd()) {
				return false;
			}
			return tokens[current].tokenType == expected;
		}

		/// <summary>Are allExpected token types next, jumps past IGNORED.</summary>
		public bool IsCheck(params TokenType[] allExpected) {
			int offset = 0;
			foreach (TokenType expected in allExpected) {
				if (!IsCheckAt(expected, offset)) {
					return false;
				}
				offset++;
			}
			return true;
		}

		/// <summary>Is expected token type at current + 1, jumps past IGNORED tokens.</summary>
		public bool IsCheckNext(TokenType expected) {
			if (IsAtEnd(1)) {
				return false;
			}
			return tokens[current + 1].tokenType == expected;
		}

		/// <summary>Is expected token type at current + offset, jumps past IGNORED tokens.</summary>
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

		/// <summary>If tokens is expected return true and advance.</summary>
		public bool IsMatch(params TokenType[] allExpected) {
			var firstIdx = current;
			foreach (var expected in allExpected) {
				if (!IsCheck(expected)) {
					current = firstIdx;
					return false;
				}
				current++;
			}
			return false;
		}


		public Token Consume(TokenType expected, string message) {
			if (IsCheck(expected)) {
				return Advance();
			}
			throw new ParseException(Peek(), message);
		}

		public bool IsAtEnd(int offset = 0) {
			return (current + offset) >= tokens.Count || tokens[current + offset].tokenType == TokenType.EOF || (current + offset) < 0;
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


		private int SkipIgnored(int offset = 0) {
			while (tokens[current + offset].tokenType == TokenType.IGNORED) {
				if (IsAtEnd()) {
					return offset;
				}
				offset++;
			}
			return offset;
		}
	}   // Cursor
}