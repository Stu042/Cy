using System;

namespace Cy {
	partial class Parser {
		/// <summary>
		///  Error exception for parser.
		/// </summary>
		class ParseError : Exception {
			public Token token;
			public ParseError() { }
			public ParseError(Token token, string message) : base($"Parse error: {message} :: {token}") {
				this.token = token;
			}
		}
	}
}
