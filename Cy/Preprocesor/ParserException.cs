
using System;

namespace Cy.Preprocesor;
class ParserException : Exception {
	public Token token;

	public ParserException() { }
	public ParserException(Token token, string message) : base($"parse error. {message} Token: {token}") {
		this.token = token;
	}
}
