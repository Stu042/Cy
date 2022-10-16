using Cy.Constants;
using Cy.Util;

using System;
using System.Collections.Generic;
using System.Linq;

namespace Cy.Preprocesor {
	// Write tokens to console
	public class TokenDisplay {
		public void DisplayAllTokens(List<List<Token>> allFilesTokens, bool allTokens = false) {
			var tokenCount = allFilesTokens.Sum(tokens => allTokens ? tokens.Count : tokens.Where(tok => tok.tokenType != TokenType.IGNORED).Count());
			ColourConsole.WriteLine($"\n{tokenCount} Tokens:", Colour.Hue.FG_DarkGrey);
			foreach (var tokens in allFilesTokens) {
				ColourConsole.Write("File: ", Colour.Hue.FG_DarkGrey);
				ColourConsole.WriteLine(tokens[0].filename, Colour.Hue.FG_Grey);
				if (allTokens) {
					Show(tokens);
				} else {
					Show(tokens.Where(tok => tok.tokenType != TokenType.IGNORED));
				}
			}
		}

		void Show(IEnumerable<Token> tokens) {
			foreach (Token token in tokens) {
				Console.WriteLine(FormattedToken(token));
			}
		}

		string FormattedToken(Token token) {
			string lexemeStr = TidyLexemeString(token.tokenType, token.lexeme);
			string literalStr = token.literal?.ToString();
			//return Colour.Create($"{lexemeStr,-10} ", Colour.Hue.FG_DarkCyan) +
			//		Colour.Create($"{token.tokenType,-20} ", Colour.Hue.FG_Green) +
			//		Colour.Create($"{literalStr,-20} ", Colour.Hue.FG_DarkBlue) +
			//		Colour.Create("Line:", Colour.Hue.FG_DarkGrey) +
			//		Colour.Create($"{token.line,4} ", Colour.Hue.FG_Grey) +
			//		Colour.Create("Offset:", Colour.Hue.FG_DarkGrey) +
			//		Colour.Create($"{token.offset,3}", Colour.Hue.FG_Grey);
			return ColourConsole.LineProperty($"//LEXEME {lexemeStr,-10} //TOKENTYPE {token.tokenType,-20} //LITERAL {literalStr,-20} //LINE Line://LINE_NUMBER{token.line,4} //OFFSET Offset://OFFSET_NUMBER {token.offset,3}");
		}

		string TidyLexemeString(TokenType tokenType, string lexeme) {
			if (tokenType == TokenType.NEWLINE) {
				lexeme = "\\n";
			}
			if (tokenType == TokenType.EOF) {
				lexeme = "\\0";
			}
			if (lexeme == "\r") {
				lexeme = "\\r";
			}
			if (lexeme == "\t") {
				lexeme = "\\t";
			}
			return lexeme;
		}
	}
}
