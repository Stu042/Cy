using Cy.TokenGenerator;

using System;

namespace Cy.Parsing {
	public class Ast {
		public Token Token;
		public Guid Id;
		public Ast(Token token) {
			Token = token;
			Id = new Guid();	
		}
	}
}
