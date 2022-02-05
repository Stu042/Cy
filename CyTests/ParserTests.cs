using System;
using System.Collections.Generic;
using System.Linq;

using Cy.Preprocesor;
using Cy.Preprocesor.Interfaces;

using FluentAssertions;

using Xunit;



namespace CyTests {
	public class ParserTests {
		static readonly string test1_Filename = "test1.cy";
		readonly ErrorDisplay errorDisplay;

		static readonly List<Token> ifTokens = new() {
			new Token(TokenType.IF, "if", null, 0, 1, 0, test1_Filename),
			new Token(TokenType.LEFT_PAREN, "(", null, 0, 1, 3, test1_Filename),
			new Token(TokenType.IDENTIFIER, "a", null, 0, 1, 8, test1_Filename),
			new Token(TokenType.RIGHT_PAREN, ")", null, 0, 1, 11, test1_Filename),
			new Token(TokenType.COLON, ":", null, 0, 1, 12, test1_Filename),
			new Token(TokenType.NEWLINE, "\n", null, 0, 1, 13, test1_Filename),
			new Token(TokenType.INT, "int", null, 1, 2, 1, test1_Filename),
			new Token(TokenType.IDENTIFIER, "b", null, 1, 2, 5, test1_Filename),
			new Token(TokenType.NEWLINE, "\n", null, 0, 1, 13, test1_Filename),
			new Token(TokenType.EOF, "\0", null, 0, 1, 14, test1_Filename),
		};
		static readonly List<Stmt> ifStatement = new() {
			new Stmt.If(
				ifTokens[0],
				new Expr.Variable(ifTokens[2]),
				new List<Stmt> {
					new Stmt.Var(ifTokens[6], ifTokens[7], null)
				},
				null
			)
		};

		//static readonly List<Cy.Token> forTokens = new() {
		//	new Cy.Token(Cy.TokenType.FOR, "for", null, 0, 1, 0, test1_Filename),
		//	new Cy.Token(Cy.TokenType.LEFT_PAREN, "(", null, 0, 1, 3, test1_Filename),
		//	new Cy.Token(Cy.TokenType.INT, "int", null, 0, 1, 4, test2_Filename),
		//	new Cy.Token(Cy.TokenType.IDENTIFIER, "a", null, 0, 1, 8, test1_Filename),
		//	new Cy.Token(Cy.TokenType.IDENTIFIER, "data", null, 0, 1, 10, test2_Filename),
		//	new Cy.Token(Cy.TokenType.RIGHT_PAREN, ")", null, 0, 1, 14, test1_Filename),
		//	new Cy.Token(Cy.TokenType.COLON, ":", null, 0, 1, 15, test2_Filename),
		//};
		//static readonly List<Cy.Ast.Stmt> forStatement = new() {
		//	new Cy.Ast.Stmt(
		//		new Cy.Ast.Stmt.For(forTokens[0],
		//			new Cy.Ast.Stmt.StmtType(new List<Cy.Token> { forTokens[2] }), forTokens[3], 
		//		)
		//	)
		//};

		static readonly List<Token> callTokens = new() {
			new Token(TokenType.INT32, "int32", null, 0, 1, 0, test1_Filename),
			new Token(TokenType.IDENTIFIER, "afunction", null, 0, 1, 0, test1_Filename),
			new Token(TokenType.LEFT_PAREN, "(", null, 0, 1, 9, test1_Filename),
			new Token(TokenType.INT32, "int32", null, 0, 1, 0, test1_Filename),
			new Token(TokenType.IDENTIFIER, "a", null, 0, 1, 10, test1_Filename),
			new Token(TokenType.RIGHT_PAREN, ")", null, 0, 1, 11, test1_Filename),
			new Token(TokenType.COLON, ":", null, 0, 1, 11, test1_Filename),
			new Token(TokenType.NEWLINE, "\n", null, 0, 1, 12, test1_Filename),
			new Token(TokenType.INT, "int", null, 1, 2, 1, test1_Filename),
			new Token(TokenType.IDENTIFIER, "b", null, 1, 2, 5, test1_Filename),
			new Token(TokenType.NEWLINE, "\n", null, 0, 1, 13, test1_Filename),
			new Token(TokenType.EOF, "\0", null, 0, 1, 13, test1_Filename),
		};
		static readonly List<Stmt> callStatement = new() {
			new Stmt.Function(
				new Stmt.StmtType(new List<Token>() { callTokens[0] }),
				callTokens[1],
				new List<Stmt.InputVar>() {
					new Stmt.InputVar(
						new Stmt.StmtType(new List<Token>() { callTokens[3] }),
						callTokens[4]
					)
				},
				new List<Stmt> {
					new Stmt.Var(callTokens[7], callTokens[8], null)
				}
			)
		};

		static readonly List<Token> callMultArgsTokens = new() {
			new Token(TokenType.INT32, "int32", null, 0, 1, 0, test1_Filename),
			new Token(TokenType.IDENTIFIER, "afunction", null, 0, 1, 0, test1_Filename),
			new Token(TokenType.LEFT_PAREN, "(", null, 0, 1, 9, test1_Filename),
			new Token(TokenType.INT32, "int32", null, 0, 1, 0, test1_Filename),
			new Token(TokenType.IDENTIFIER, "a", null, 0, 1, 10, test1_Filename),
			new Token(TokenType.COMMA, ",", null, 0, 1, 10, test1_Filename),
			new Token(TokenType.INT32, "float32", null, 0, 1, 0, test1_Filename),
			new Token(TokenType.IDENTIFIER, "b", null, 0, 1, 10, test1_Filename),
			new Token(TokenType.RIGHT_PAREN, ")", null, 0, 1, 11, test1_Filename),
			new Token(TokenType.COLON, ":", null, 0, 1, 11, test1_Filename),
			new Token(TokenType.NEWLINE, "\n", null, 0, 1, 12, test1_Filename),
			new Token(TokenType.INT, "int", null, 1, 2, 1, test1_Filename),
			new Token(TokenType.IDENTIFIER, "c", null, 1, 2, 5, test1_Filename),
			new Token(TokenType.NEWLINE, "\n", null, 0, 1, 13, test1_Filename),
			new Token(TokenType.EOF, "\0", null, 0, 1, 13, test1_Filename),
		};
		static readonly List<Stmt> callMultArgsStatement = new() {
			new Stmt.Function(
				new Stmt.StmtType(new List<Token>() { callMultArgsTokens[0] }),
				callMultArgsTokens[1],
				new List<Stmt.InputVar>() {
					new Stmt.InputVar(new Stmt.StmtType(new List<Token>() { callMultArgsTokens[3] }), callMultArgsTokens[4]),
					new Stmt.InputVar(new Stmt.StmtType(new List<Token>() { callMultArgsTokens[6] }), callMultArgsTokens[7])
				},
				new List<Stmt> {
					new Stmt.Var(callMultArgsTokens[10], callMultArgsTokens[11], null)
				}
			)
		};

		static readonly List<Token> unaryTokens = new() {
			new Token(TokenType.MINUSMINUS, "--", null, 0, 1, 0, test1_Filename),
			new Token(TokenType.IDENTIFIER, "a", null, 0, 1, 2, test1_Filename),
			new Token(TokenType.NEWLINE, "\n", null, 0, 1, 3, test1_Filename),
			new Token(TokenType.EOF, "\0", null, 0, 1, 4, test1_Filename),
		};
		static readonly List<Stmt> unaryStatement = new() {
			new Stmt.Expression(new Expr.Unary(unaryTokens[0], new Expr.Variable(unaryTokens[1])))
		};

		static readonly List<Token> whileTokens = new() {
			new Token(TokenType.WHILE, "while", null, 0, 1, 0, test1_Filename),
			new Token(TokenType.IDENTIFIER, "a", null, 0, 1, 2, test1_Filename),
			new Token(TokenType.EQUAL_EQUAL, "==", null, 0, 1, 2, test1_Filename),
			new Token(TokenType.IDENTIFIER, "a", null, 0, 1, 2, test1_Filename),
			new Token(TokenType.COLON, ":", null, 0, 0, 2, test1_Filename),
			new Token(TokenType.NEWLINE, "\n", null, 0, 0, 3, test1_Filename),
			new Token(TokenType.INT, "int", null, 1, 1, 1, test1_Filename),
			new Token(TokenType.IDENTIFIER, "c", null, 1, 1, 5, test1_Filename),
			new Token(TokenType.NEWLINE, "\n", null, 0, 1, 13, test1_Filename),
			new Token(TokenType.EOF, "\0", null, 0, 1, 13, test1_Filename),
		};
		//static readonly List<Cy.Ast.Stmt> whileStatement = new() {
		//	new Cy.Ast.Stmt.While(whileTokens[0], new Cy.Ast.Expr.Compare)
		//};


		public class ErrorDisplay : IErrorDisplay {
			public void Error(Token token, string linestr, string message) { }
			public void Error(string filename, int line, int offset, string lineText, string message) { }
			public void Error(Token tok, string message) { }
		}

		public ParserTests() {
			errorDisplay = new();
		}



		[Fact]
		public void Test_Tokens_To_Call_Ast() {
			var cursor = new ParserCursor(callTokens);
			var parser = new Parser(cursor, errorDisplay);
			var returnedAst = parser.Parse();
			Asts.Display(new List<List<Stmt>>() { returnedAst });
			callStatement.Should().BeEquivalentTo(returnedAst);
		}
		[Fact]
		public void Test_Tokens_To_Call_With_Multiple_Args_Ast() {
			var cursor = new ParserCursor(callMultArgsTokens);
			Parser parser = new(cursor, errorDisplay);
			var returnedAst = parser.Parse();
			callMultArgsStatement.Should().BeEquivalentTo(returnedAst);
		}

		//	"if (a):
		//		int b"
		[Fact]
		public void Test_Tokens_To_If_Ast() {
			var cursor = new ParserCursor(ifTokens);
			Parser parser = new(cursor, errorDisplay);
			var returnedAst = parser.Parse();
			ifStatement.Should().BeEquivalentTo(returnedAst);
		}

		[Fact]
		public void Test_Tokens_To_Unary_Ast() {
			var cursor = new ParserCursor(unaryTokens);
			var parser = new Parser(cursor, errorDisplay);
			var returnedAst = parser.Parse();
			unaryStatement.Should().BeEquivalentTo(returnedAst);
		}



		//	"for (int a each b):"
		//[Fact]
		//public void Test_TokensToForAst() {
		//	var cursor = new Cursor(forTokens);
		//	var parser = new Parser(cursor);
		//	var expected = forStatement;
		//	var returnedTokens = parser.Parse();
		//	forStatement.Should().BeEquivalentTo(returnedAst);
		//}
	}
}
