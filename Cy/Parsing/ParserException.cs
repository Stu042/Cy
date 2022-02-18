using Cy.TokenGenerator;

using System;

namespace Cy.Parsing;
class ParserException : Exception {
	public Token token;

	public ParserException() { }
	public ParserException(Token token, string message) : base($"Parse error: {message} :: {token.lexeme}") {
		this.token = token;
	}
}
