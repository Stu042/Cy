using System;
using System.Collections.Generic;
using System.Linq;

using Cy.Constants;
using Cy.Preprocesor;
using Cy.Preprocesor.Interfaces;
using Cy.Setup;

using FluentAssertions;

using Xunit;



namespace CyTests
{
	public class ScannerTests
	{
		static readonly string test1_Filename = "test1.cy";
		IErrorDisplay errorDisplay;

		readonly List<Token> test1_ExpectedTokens = new() {
			new Token(TokenType.INT, "int", null, 1, 0, test1_Filename),
			new Token(TokenType.IGNORED, " ", null, 1, 3, test1_Filename),
			new Token(TokenType.IDENTIFIER, "Main", null,1, 4, test1_Filename),
			new Token(TokenType.LEFT_PAREN, "(", null, 1, 8, test1_Filename),
			new Token(TokenType.RIGHT_PAREN, ")", null,1, 9, test1_Filename),
			new Token(TokenType.COLON, ":", null, 1, 10, test1_Filename),
			new Token(TokenType.IGNORED, "\r", null, 1, 11, test1_Filename),
			new Token(TokenType.NEWLINE, "\n", null, 1, 0, test1_Filename),
			new Token(TokenType.RETURN, "return", null, 2, 2, test1_Filename),
			new Token(TokenType.IGNORED, " ", null, 2, 8, test1_Filename),
			new Token(TokenType.INT_LITERAL, "2", 2,2, 9, test1_Filename),
			new Token(TokenType.IGNORED, "\r", null, 2, 10, test1_Filename),
			new Token(TokenType.NEWLINE, "\n", null, 2, 0, test1_Filename),
			new Token(TokenType.EOF, "\0", null, 3, 0, test1_Filename)
		};
		static readonly string test1_Source =
			@"int Main():
	return 2
";

		static readonly string test2_Filename = "test2.cy";
		static readonly string test2_Source = @"Data:
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
		readonly List<Token> test2_ExpectedTokens = new() {
			new Token(TokenType.IDENTIFIER, "Data", null, 1, 0, test2_Filename),
			new Token(TokenType.COLON, ":", null, 1, 4, test2_Filename),
			new Token(TokenType.IGNORED, "\r", null,1, 5, test2_Filename),
			new Token(TokenType.NEWLINE, "\n", null, 1, 0, test2_Filename),
			new Token(TokenType.INT, "int", null, 2, 2, test2_Filename),
			new Token(TokenType.IGNORED, " ", null, 2, 5, test2_Filename),
			new Token(TokenType.IDENTIFIER, "a", null, 2, 6, test2_Filename),
			new Token(TokenType.IGNORED, "\r", null,  2, 7, test2_Filename),
			new Token(TokenType.NEWLINE, "\n", null, 2, 0, test2_Filename),
			new Token(TokenType.INT, "int", null, 3, 2, test2_Filename),
			new Token(TokenType.IGNORED, " ", null, 3, 5, test2_Filename),
			new Token(TokenType.IDENTIFIER, "b", null, 3, 6, test2_Filename),
			new Token(TokenType.IGNORED, "\r", null, 3, 7, test2_Filename),
			new Token(TokenType.NEWLINE, "\n", null, 3, 0, test2_Filename),
			new Token(TokenType.IGNORED, "\r", null, 4, 1, test2_Filename),
			new Token(TokenType.NEWLINE, "\n", null, 4, 0, test2_Filename),
			new Token(TokenType.IDENTIFIER, "Data", null, 5, 2, test2_Filename),
			new Token(TokenType.LEFT_PAREN, "(", null, 5, 6, test2_Filename),
			new Token(TokenType.INT, "int", null, 5, 7, test2_Filename),
			new Token(TokenType.IGNORED, " ", null, 5, 10, test2_Filename),
			new Token(TokenType.IDENTIFIER, "aa", null, 5, 11, test2_Filename),
			new Token(TokenType.COMMA, ",", null, 5, 13, test2_Filename),
			new Token(TokenType.IGNORED, " ", null, 5, 14, test2_Filename),
			new Token(TokenType.INT, "int", null, 5, 15, test2_Filename),
			new Token(TokenType.IGNORED, " ", null, 5, 18, test2_Filename),
			new Token(TokenType.IDENTIFIER, "bb", null,5, 19, test2_Filename),
			new Token(TokenType.RIGHT_PAREN, ")", null,5, 21, test2_Filename),
			new Token(TokenType.COLON, ":", null, 5, 22, test2_Filename),
			new Token(TokenType.IGNORED, "\r", null,  5, 23, test2_Filename),
			new Token(TokenType.NEWLINE, "\n", null,  5, 0, test2_Filename),
			new Token(TokenType.IDENTIFIER, "a", null,  6, 3, test2_Filename),
			new Token(TokenType.IGNORED, " ", null,  6, 4, test2_Filename),
			new Token(TokenType.EQUAL, "=", null,  6, 5, test2_Filename),
			new Token(TokenType.IGNORED, " ", null,  6, 6, test2_Filename),
			new Token(TokenType.IDENTIFIER, "aa", null,  6, 7, test2_Filename),
			new Token(TokenType.IGNORED, "\r", null,  6, 9, test2_Filename),
			new Token(TokenType.NEWLINE, "\n", null,  6, 0, test2_Filename),
			new Token(TokenType.IDENTIFIER, "b", null,  7, 3, test2_Filename),
			new Token(TokenType.IGNORED, " ", null,  7, 4, test2_Filename),
			new Token(TokenType.EQUAL, "=", null,  7, 5, test2_Filename),
			new Token(TokenType.IGNORED, " ", null, 7, 6, test2_Filename),
			new Token(TokenType.IDENTIFIER, "bb", null,  7, 7, test2_Filename),
			new Token(TokenType.IGNORED, "\r", null, 7, 9, test2_Filename),
			new Token(TokenType.NEWLINE, "\n", null, 7, 0, test2_Filename),
			new Token(TokenType.IGNORED, "\r", null, 8, 1, test2_Filename),
			new Token(TokenType.NEWLINE, "\n", null, 8, 0, test2_Filename),
			new Token(TokenType.INT, "int", null, 1, 2, test2_Filename),
			new Token(TokenType.IGNORED, " ", null,  9, 5, test2_Filename),
			new Token(TokenType.IDENTIFIER, "Mult", null, 9, 6, test2_Filename),
			new Token(TokenType.LEFT_PAREN, "(", null,  9, 10, test2_Filename),
			new Token(TokenType.RIGHT_PAREN, ")", null, 9, 11, test2_Filename),
			new Token(TokenType.COLON, ":", null,  9, 12, test2_Filename),
			new Token(TokenType.IGNORED, "\r", null, 9, 13, test2_Filename),
			new Token(TokenType.NEWLINE, "\n", null,  9, 0, test2_Filename),
			new Token(TokenType.RETURN, "return", null, 10, 3, test2_Filename),
			new Token(TokenType.IGNORED, " ", null, 10, 9, test2_Filename),
			new Token(TokenType.IDENTIFIER, "a", null,  10, 10, test2_Filename),
			new Token(TokenType.STAR, "*", null,  10, 11, test2_Filename),
			new Token(TokenType.IDENTIFIER, "b", null, 10, 12, test2_Filename),
			new Token(TokenType.IGNORED, "\r", null, 10, 13, test2_Filename),
			new Token(TokenType.NEWLINE, "\n", null, 10, 0, test2_Filename),
			new Token(TokenType.IGNORED, "\r", null, 11, 1, test2_Filename),
			new Token(TokenType.NEWLINE, "\n", null, 11, 0, test2_Filename),
			new Token(TokenType.INT, "int", null, 0,  1, test2_Filename),
			new Token(TokenType.IGNORED, " ", null, 12, 4, test2_Filename),
			new Token(TokenType.IDENTIFIER, "Main", null, 12, 5, test2_Filename),
			new Token(TokenType.LEFT_PAREN, "(", null, 12, 9, test2_Filename),
			new Token(TokenType.RIGHT_PAREN, ")", null,  12, 10, test2_Filename),
			new Token(TokenType.COLON, ":", null, 12, 11, test2_Filename),
			new Token(TokenType.IGNORED, "\r", null, 12, 12, test2_Filename),
			new Token(TokenType.NEWLINE, "\n", null, 12, 0, test2_Filename),
			new Token(TokenType.IDENTIFIER, "Data", null, 13, 2, test2_Filename),
			new Token(TokenType.IGNORED, " ", null, 13, 6, test2_Filename),
			new Token(TokenType.IDENTIFIER, "d", null, 13, 7, test2_Filename),
			new Token(TokenType.IGNORED, " ", null,  13, 8, test2_Filename),
			new Token(TokenType.EQUAL, "=", null,  13, 9, test2_Filename),
			new Token(TokenType.IGNORED, " ", null, 13, 10, test2_Filename),
			new Token(TokenType.IDENTIFIER, "Data", null, 13, 11, test2_Filename),
			new Token(TokenType.LEFT_PAREN, "(", null, 13, 15, test2_Filename),
			new Token(TokenType.INT_LITERAL, "10",  1, 13, 16, test2_Filename),
			new Token(TokenType.COMMA, ",", null,  13, 18, test2_Filename),
			new Token(TokenType.IGNORED, " ", null,  13, 19, test2_Filename),
			new Token(TokenType.INT_LITERAL, "2", 2, 13, 20, test2_Filename),
			new Token(TokenType.RIGHT_PAREN, ")", null, 13, 21, test2_Filename),
			new Token(TokenType.IGNORED, "\r", null,  13, 22, test2_Filename),
			new Token(TokenType.NEWLINE, "\n", null,  13, 0, test2_Filename),
			new Token(TokenType.IGNORED, "//return d.a * d.b\r", null, 14, 2, test2_Filename),
			new Token(TokenType.NEWLINE, "\n", null, 14, 0, test2_Filename),
			new Token(TokenType.RETURN, "return", null, 15, 2, test2_Filename),
			new Token(TokenType.IGNORED, " ", null, 15, 8, test2_Filename),
			new Token(TokenType.IDENTIFIER, "d", null, 15, 9, test2_Filename),
			new Token(TokenType.DOT, ".", null, 15, 10, test2_Filename),
			new Token(TokenType.IDENTIFIER, "Mult", null, 15, 11, test2_Filename),
			new Token(TokenType.LEFT_PAREN, "(", null,  15, 15, test2_Filename),
			new Token(TokenType.RIGHT_PAREN, ")", null, 15, 16, test2_Filename),
			new Token(TokenType.IGNORED, "\r", null,  15, 17, test2_Filename),
			new Token(TokenType.NEWLINE, "\n", null,  15, 0, test2_Filename),
			new Token(TokenType.EOF, "\0", null,  16, 0, test2_Filename)
		};

		readonly Scanner scanner = null;


		public ScannerTests()
		{
			var config = new Config();
			errorDisplay = new ErrorDisplay(config);
			var scannerCursor = new ScannerCursor();
			if (scanner == null)
			{
				scanner = new Scanner(scannerCursor, errorDisplay);
			}
		}


		[Fact]
		public void TestSourceToTokens1()
		{
			var expected = test1_ExpectedTokens.Where(item => item.tokenType != TokenType.IGNORED).Select(item => item.Clone()).ToList();
			var actualTokens = scanner.ScanTokens(test1_Filename, test1_Source).Where(item => item.tokenType != TokenType.IGNORED).ToList();
			expected.Should().BeEquivalentTo(actualTokens);
		}

		[Fact]
		public void TestSourceToTokens2()
		{
			var expected = test2_ExpectedTokens.Where(item => item.tokenType != TokenType.IGNORED).Select(item => item.Clone()).ToList();
			var actualTokens = scanner.ScanTokens(test2_Filename, test2_Source).Where(item => item.tokenType != TokenType.IGNORED).ToList();
			expected.Should().BeEquivalentTo(actualTokens);
		}
	}
}
