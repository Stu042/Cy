
using System;

namespace Cy.Preprocesor;
class ParserException : Exception {
	public Token token;

	public ParserException() { }
	public ParserException(Token token, string message) : base($"Parse error: {message} :: {token.lexeme}") {
		this.token = token;
	}
}
