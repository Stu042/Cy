
using System;

namespace Cy.Preprocesor;
class ParserException : Exception {
	public Token Token;

	public ParserException() { }
	public ParserException(Token token, string message) : base($"parse error. {message} Token: {token}") {
		this.Token = token;
	}
}
