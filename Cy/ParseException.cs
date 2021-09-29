using System;

namespace Cy.Parser {
	class ParseException : Exception {
		public Token token;

		public ParseException() { }
		public ParseException(Token token, string message) : base($"Parse error: {message} :: {token}") {
			this.token = token;
		}
	}
}
