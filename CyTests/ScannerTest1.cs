using System;
using System.Collections.Generic;
using System.Linq;

using Xunit;



namespace CyTests {
	public class ScannerTest1 {
		private static readonly string test1_Filename = "test1.cy";
		private readonly List<Cy.Token> test1_ExpectedTokens = new() {
			new Cy.Token(Cy.TokenType.INT, "int", null, 0, 1, 0, test1_Filename),
			new Cy.Token(Cy.TokenType.IGNORED, " ", null, 0, 1, 3, test1_Filename),
			new Cy.Token(Cy.TokenType.IDENTIFIER, "Main", null, 0, 1, 4, test1_Filename),
			new Cy.Token(Cy.TokenType.LEFT_PAREN, "(", null, 0, 1, 8, test1_Filename),
			new Cy.Token(Cy.TokenType.RIGHT_PAREN, ")", null, 0, 1, 9, test1_Filename),
			new Cy.Token(Cy.TokenType.COLON, ":", null, 0, 1, 10, test1_Filename),
			new Cy.Token(Cy.TokenType.IGNORED, "\r", null, 0, 1, 11, test1_Filename),
			new Cy.Token(Cy.TokenType.NEWLINE, "\n", null, 0, 2, 0, test1_Filename),
			new Cy.Token(Cy.TokenType.RETURN, "return", null, 1, 2, 2, test1_Filename),
			new Cy.Token(Cy.TokenType.IGNORED, " ", null, 1, 2, 8, test1_Filename),
			new Cy.Token(Cy.TokenType.INT_LITERAL, "2", 2, 1, 2, 9, test1_Filename),
			new Cy.Token(Cy.TokenType.IGNORED, "\r", null, 1, 2, 10, test1_Filename),
			new Cy.Token(Cy.TokenType.NEWLINE, "\n", null, 1, 3, 0, test1_Filename),
			new Cy.Token(Cy.TokenType.EOF, "\0", null, 0, 3, 0, test1_Filename)
		};
		private static readonly string test1_Source =
			@"int Main():
	return 2
";

		private static readonly string test2_Filename = "test2.cy";
		private static readonly string test2_Source = @"Data:
	int a
	int b

	Data(int aa, int bb):
		a = aa
		b = bb

	int Mult():
		return a*b

int Main():
	Data d = Data(10, 2)
	//return d.a * d.b
	return d.Mult()
";
		private readonly List<Cy.Token> test2_ExpectedTokens = new() {
			new Cy.Token(Cy.TokenType.IDENTIFIER, "Data", null, 0, 1, 0, test2_Filename),
			new Cy.Token(Cy.TokenType.COLON, ":", null, 0, 1, 4, test2_Filename),
			new Cy.Token(Cy.TokenType.IGNORED, "\r", null, 0, 1, 5, test2_Filename),
			new Cy.Token(Cy.TokenType.NEWLINE, "\n", null, 0, 1, 0, test2_Filename),
			new Cy.Token(Cy.TokenType.INT, "int", null, 1, 2, 2, test2_Filename),
			new Cy.Token(Cy.TokenType.IGNORED, " ", null, 1, 2, 5, test2_Filename),
			new Cy.Token(Cy.TokenType.IDENTIFIER, "a", null, 1, 2, 6, test2_Filename),
			new Cy.Token(Cy.TokenType.IGNORED, "\r", null, 1, 2, 7, test2_Filename),
			new Cy.Token(Cy.TokenType.NEWLINE, "\n", null, 1, 2, 0, test2_Filename),
			new Cy.Token(Cy.TokenType.INT, "int", null, 1, 3, 2, test2_Filename),
			new Cy.Token(Cy.TokenType.IGNORED, " ", null, 1, 3, 5, test2_Filename),
			new Cy.Token(Cy.TokenType.IDENTIFIER, "b", null, 1, 3, 6, test2_Filename),
			new Cy.Token(Cy.TokenType.IGNORED, "\r", null, 1, 3, 7, test2_Filename),
			new Cy.Token(Cy.TokenType.NEWLINE, "\n", null, 1, 3, 0, test2_Filename),
			new Cy.Token(Cy.TokenType.IGNORED, "\r", null, 0, 4, 1, test2_Filename),
			new Cy.Token(Cy.TokenType.NEWLINE, "\n", null, 0, 4, 0, test2_Filename),
			new Cy.Token(Cy.TokenType.IDENTIFIER, "Data", null, 1, 5, 2, test2_Filename),
			new Cy.Token(Cy.TokenType.LEFT_PAREN, "(", null, 1, 5, 6, test2_Filename),
			new Cy.Token(Cy.TokenType.INT, "int", null, 1, 5, 7, test2_Filename),
			new Cy.Token(Cy.TokenType.IGNORED, " ", null, 1, 5, 10, test2_Filename),
			new Cy.Token(Cy.TokenType.IDENTIFIER, "aa", null, 1, 5, 11, test2_Filename),
			new Cy.Token(Cy.TokenType.COMMA, ",", null, 1, 5, 13, test2_Filename),
			new Cy.Token(Cy.TokenType.IGNORED, " ", null, 1, 5, 14, test2_Filename),
			new Cy.Token(Cy.TokenType.INT, "int", null, 1, 5, 15, test2_Filename),
			new Cy.Token(Cy.TokenType.IGNORED, " ", null, 1, 5, 18, test2_Filename),
			new Cy.Token(Cy.TokenType.IDENTIFIER, "bb", null, 1, 5, 19, test2_Filename),
			new Cy.Token(Cy.TokenType.RIGHT_PAREN, ")", null, 1, 5, 21, test2_Filename),
			new Cy.Token(Cy.TokenType.COLON, ":", null, 1, 5, 22, test2_Filename),
			new Cy.Token(Cy.TokenType.IGNORED, "\r", null, 1, 5, 23, test2_Filename),
			new Cy.Token(Cy.TokenType.NEWLINE, "\n", null, 1, 5, 0, test2_Filename),
			new Cy.Token(Cy.TokenType.IDENTIFIER, "a", null, 2, 6, 3, test2_Filename),
			new Cy.Token(Cy.TokenType.IGNORED, " ", null, 2, 6, 4, test2_Filename),
			new Cy.Token(Cy.TokenType.EQUAL, "=", null, 2, 6, 5, test2_Filename),
			new Cy.Token(Cy.TokenType.IGNORED, " ", null, 2, 6, 6, test2_Filename),
			new Cy.Token(Cy.TokenType.IDENTIFIER, "aa", null, 2, 6, 7, test2_Filename),
			new Cy.Token(Cy.TokenType.IGNORED, "\r", null, 2, 6, 9, test2_Filename),
			new Cy.Token(Cy.TokenType.NEWLINE, "\n", null, 2, 6, 0, test2_Filename),
			new Cy.Token(Cy.TokenType.IDENTIFIER, "b", null, 2, 7, 3, test2_Filename),
			new Cy.Token(Cy.TokenType.IGNORED, " ", null, 2, 7, 4, test2_Filename),
			new Cy.Token(Cy.TokenType.EQUAL, "=", null, 2, 7, 5, test2_Filename),
			new Cy.Token(Cy.TokenType.IGNORED, " ", null, 2, 7, 6, test2_Filename),
			new Cy.Token(Cy.TokenType.IDENTIFIER, "bb", null, 2, 7, 7, test2_Filename),
			new Cy.Token(Cy.TokenType.IGNORED, "\r", null, 2, 7, 9, test2_Filename),
			new Cy.Token(Cy.TokenType.NEWLINE, "\n", null, 2, 7, 0, test2_Filename),
			new Cy.Token(Cy.TokenType.IGNORED, "\r", null, 0, 8, 1, test2_Filename),
			new Cy.Token(Cy.TokenType.NEWLINE, "\n", null, 0, 8, 0, test2_Filename),
			new Cy.Token(Cy.TokenType.INT, "int", null, 1, 9, 2, test2_Filename),
			new Cy.Token(Cy.TokenType.IGNORED, " ", null, 1, 9, 5, test2_Filename),
			new Cy.Token(Cy.TokenType.IDENTIFIER, "Mult", null, 1, 9, 6, test2_Filename),
			new Cy.Token(Cy.TokenType.LEFT_PAREN, "(", null, 1, 9, 10, test2_Filename),
			new Cy.Token(Cy.TokenType.RIGHT_PAREN, ")", null, 1, 9, 11, test2_Filename),
			new Cy.Token(Cy.TokenType.COLON, ":", null, 1, 9, 12, test2_Filename),
			new Cy.Token(Cy.TokenType.IGNORED, "\r", null, 1, 9, 13, test2_Filename),
			new Cy.Token(Cy.TokenType.NEWLINE, "\n", null, 1, 9, 0, test2_Filename),
			new Cy.Token(Cy.TokenType.RETURN, "return", null, 2, 10, 3, test2_Filename),
			new Cy.Token(Cy.TokenType.IGNORED, " ", null, 2, 10, 9, test2_Filename),
			new Cy.Token(Cy.TokenType.IDENTIFIER, "a", null, 2, 10, 10, test2_Filename),
			new Cy.Token(Cy.TokenType.STAR, "*", null, 2, 10, 11, test2_Filename),
			new Cy.Token(Cy.TokenType.IDENTIFIER, "b", null, 2, 10, 12, test2_Filename),
			new Cy.Token(Cy.TokenType.IGNORED, "\r", null, 2, 10, 13, test2_Filename),
			new Cy.Token(Cy.TokenType.NEWLINE, "\n", null, 2, 10, 0, test2_Filename),
			new Cy.Token(Cy.TokenType.IGNORED, "\r", null, 0, 11, 1, test2_Filename),
			new Cy.Token(Cy.TokenType.NEWLINE, "\n", null, 0, 11, 0, test2_Filename),
			new Cy.Token(Cy.TokenType.INT, "int", null, 0, 12, 1, test2_Filename),
			new Cy.Token(Cy.TokenType.IGNORED, " ", null, 0, 12, 4, test2_Filename),
			new Cy.Token(Cy.TokenType.IDENTIFIER, "Main", null, 0, 12, 5, test2_Filename),
			new Cy.Token(Cy.TokenType.LEFT_PAREN, "(", null, 0, 12, 9, test2_Filename),
			new Cy.Token(Cy.TokenType.RIGHT_PAREN, ")", null, 0, 12, 10, test2_Filename),
			new Cy.Token(Cy.TokenType.COLON, ":", null, 0, 12, 11, test2_Filename),
			new Cy.Token(Cy.TokenType.IGNORED, "\r", null, 0, 12, 12, test2_Filename),
			new Cy.Token(Cy.TokenType.NEWLINE, "\n", null, 0, 12, 0, test2_Filename),
			new Cy.Token(Cy.TokenType.IDENTIFIER, "Data", null, 1, 13, 2, test2_Filename),
			new Cy.Token(Cy.TokenType.IGNORED, " ", null, 1, 13, 6, test2_Filename),
			new Cy.Token(Cy.TokenType.IDENTIFIER, "d", null, 1, 13, 7, test2_Filename),
			new Cy.Token(Cy.TokenType.IGNORED, " ", null, 1, 13, 8, test2_Filename),
			new Cy.Token(Cy.TokenType.EQUAL, "=", null, 1, 13, 9, test2_Filename),
			new Cy.Token(Cy.TokenType.IGNORED, " ", null, 1, 13, 10, test2_Filename),
			new Cy.Token(Cy.TokenType.IDENTIFIER, "Data", null, 1, 13, 11, test2_Filename),
			new Cy.Token(Cy.TokenType.LEFT_PAREN, "(", null, 1, 13, 15, test2_Filename),
			new Cy.Token(Cy.TokenType.INT_LITERAL, "10", 10, 1, 13, 16, test2_Filename),
			new Cy.Token(Cy.TokenType.COMMA, ",", null, 1, 13, 18, test2_Filename),
			new Cy.Token(Cy.TokenType.IGNORED, " ", null, 1, 13, 19, test2_Filename),
			new Cy.Token(Cy.TokenType.INT_LITERAL, "2", 2, 1, 13, 20, test2_Filename),
			new Cy.Token(Cy.TokenType.RIGHT_PAREN, ")", null, 1, 13, 21, test2_Filename),
			new Cy.Token(Cy.TokenType.IGNORED, "\r", null, 1, 13, 22, test2_Filename),
			new Cy.Token(Cy.TokenType.NEWLINE, "\n", null, 1, 13, 0, test2_Filename),
			new Cy.Token(Cy.TokenType.IGNORED, "//return d.a * d.b\r", null, 1, 14, 2, test2_Filename),
			new Cy.Token(Cy.TokenType.NEWLINE, "\n", null, 1, 14, 0, test2_Filename),
			new Cy.Token(Cy.TokenType.RETURN, "return", null, 1, 15, 2, test2_Filename),
			new Cy.Token(Cy.TokenType.IGNORED, " ", null, 1, 15, 8, test2_Filename),
			new Cy.Token(Cy.TokenType.IDENTIFIER, "d", null, 1, 15, 9, test2_Filename),
			new Cy.Token(Cy.TokenType.DOT, ".", null, 1, 15, 10, test2_Filename),
			new Cy.Token(Cy.TokenType.IDENTIFIER, "Mult", null, 1, 15, 11, test2_Filename),
			new Cy.Token(Cy.TokenType.LEFT_PAREN, "(", null, 1, 15, 15, test2_Filename),
			new Cy.Token(Cy.TokenType.RIGHT_PAREN, ")", null, 1, 15, 16, test2_Filename),
			new Cy.Token(Cy.TokenType.IGNORED, "\r", null, 1, 15, 17, test2_Filename),
			new Cy.Token(Cy.TokenType.NEWLINE, "\n", null, 1, 15, 0, test2_Filename),
			new Cy.Token(Cy.TokenType.EOF, "\0", null, 0, 16, 0, test2_Filename)
		};

		private readonly Cy.Scanner scanner = null;


		public ScannerTest1() {
			if (scanner == null) {
				scanner = new Cy.Scanner();
			}
		}


		[Fact]
		public void TestSourceToTokens1() {
			var expected = test1_ExpectedTokens.Where(item => item.tokenType != Cy.TokenType.IGNORED).Select(item => item.Clone()).ToList();
			var actualTokens = scanner.ScanTokens(test1_Filename, test1_Source).Where(item => item.tokenType != Cy.TokenType.IGNORED).ToList();
			Assert.NotStrictEqual(expected, actualTokens);
		}

		[Fact]
		public void TestSourceToTokens2() {
			var expected = test2_ExpectedTokens.Where(item => item.tokenType != Cy.TokenType.IGNORED).Select(item => item.Clone()).ToList();
			var actualTokens = scanner.ScanTokens(test2_Filename, test2_Source).Where(item => item.tokenType != Cy.TokenType.IGNORED).ToList();
			Assert.NotStrictEqual(expected, actualTokens);
		}
	}
}
