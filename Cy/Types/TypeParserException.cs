using Cy.Preprocesor;

using System;

namespace Cy.Types;

public class TypeParserException : Exception {
	public Token Token;

	public TypeParserException() { }
	public TypeParserException(Stmt stmt, string message) : base($"type parse error. {message} statement token: {stmt.token}") {
		Token = stmt.token;
	}
	public TypeParserException(Expr expr, string message) : base($"type parse error. {message} statement token: {expr.token}") {
		Token = expr.token;
	}
}
