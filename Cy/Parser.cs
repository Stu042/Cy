using System;
using System.Collections.Generic;

namespace Cy {
	class Parser {
		Cursor cursor;


		public Parser(List<Token> tokens) {
			cursor = new Cursor(tokens);
		}


		public List<Stmt> Parse() {
			List<Stmt> statements = new List<Stmt>();
			while (!cursor.IsAtEnd()) {
				Stmt s = Declaration();
				if (s != null)
					statements.Add(s);
			}
			return statements;
		}



		Stmt Declaration() {
			try {
				if (cursor.MatchAnyType()) {
					if (cursor.CheckAll(Token.Kind.IDENTIFIER, Token.Kind.LEFT_PAREN) && cursor.LineCheck(Token.Kind.RIGHT_PAREN, Token.Kind.COLON))
						return FunDeclaration("function");
					else if (cursor.Check(Token.Kind.IDENTIFIER))
						return VarDeclaration();
				}
				return Statement();
			} catch (ParseError) {
				Synchronize();
				return null;
			}
		}


		/// <summary>
		/// Declare a function/method.
		/// </summary>
		Stmt.Function FunDeclaration(string kind) {
			// TODO Add type here (expression or statement... Stmt me thinks).
			Stmt.Type type = new Stmt.Type(cursor.Previous());
			Token name = cursor.Consume(Token.Kind.IDENTIFIER, "Expect " + kind + " name.");
			cursor.Consume(Token.Kind.LEFT_PAREN, "Expect '(' after " + kind + " name.");
			List<Token> parameters = new List<Token>();
			if (!cursor.Check(Token.Kind.RIGHT_PAREN)) {
				do {
					if (parameters.Count >= 255)
						Error(cursor.Peek(), "Cannot have more than 255 parameters.");
					parameters.Add(cursor.Consume(Token.Kind.IDENTIFIER, "Expect parameter name."));
				} while (cursor.Match(Token.Kind.COMMA));
			}
			cursor.Consume(Token.Kind.RIGHT_PAREN, "Expect ')' after parameters.");
			cursor.Consume(Token.Kind.COLON, "Expect ':' before " + kind + " body.");
			cursor.Consume(Token.Kind.NEWLINE, "Expect 'newline' before " + kind + " body.");
			List<Stmt> body = Block();
			return new Stmt.Function(type, name, parameters, body);
		}


		Stmt Statement() {
			/*
			if (cursor.Match(Token.Kind.FOR))
				return ForStatement();
			if (cursor.Match(Token.Kind.IF))
				return IfStatement();
			if (cursor.Match(Token.Kind.PRINT))
				return PrintStatement();
			*/
			if (cursor.Match(Token.Kind.RETURN))
				return ReturnStatement();
			/*
			if (cursor.Match(Token.Kind.WHILE))
				return WhileStatement();
			if (cursor.Match(Token.Kind.LEFT_BRACE))
				return new Stmt.Block(Block());
			*/
			return ExpressionStatement();
		}



		Stmt ReturnStatement() {
			Token keyword = cursor.Previous();
			Expr value = null;
			if (!cursor.Check(Token.Kind.NEWLINE))
				value = Expression();
			cursor.Consume(Token.Kind.NEWLINE, "Expect 'newline' after return value.");
			return new Stmt.Return(keyword, value);
		}


		Stmt ExpressionStatement() {
			Expr expr = Expression();
			cursor.Consume(Token.Kind.NEWLINE, "Expect 'newline' after expression.");
			return new Stmt.Expression(expr);
		}


		/// <summary>
		/// Get the current Block of statements.
		/// </summary>
		List<Stmt> Block() {
			List<Stmt> statements = new List<Stmt>();
			int startIndent = cursor.Peek().indent;
			while (startIndent <= cursor.Peek().indent && !cursor.IsAtEnd())
				statements.Add(Declaration());
			return statements;
		}


		/// <summary>
		/// Create a variable which optionally has an assigned expression.
		/// </summary>
		Stmt VarDeclaration() {
			Token name = cursor.Consume(Token.Kind.IDENTIFIER, "Expect variable name.");
			Expr initializer = null;
			if (cursor.Match(Token.Kind.EQUAL))
				initializer = Expression();
			cursor.Consume(Token.Kind.NEWLINE, "Expect 'newline' after variable declaration.");
			return new Stmt.Var(name, initializer);
		}


		/// <summary>
		/// Create an expression. could be a+b or 2+3 or could be rhs of an assign, a=2, etc...
		/// </summary>
		Expr Expression() {
			return Equality();
		}


		Expr Equality() {
			Expr expr = Compare();
			while (cursor.Match(Token.Kind.BANG_EQUAL, Token.Kind.EQUAL_EQUAL)) {
				Token op = cursor.Previous();
				Expr right = Compare();
				expr = new Expr.Binary(expr, op, right);
			}
			return expr;
		}

		Expr Compare() {
			Expr expr = Addition();
			while (cursor.Match(Token.Kind.GREATER, Token.Kind.GREATER_EQUAL, Token.Kind.LESS, Token.Kind.LESS_EQUAL)) {
				Token op = cursor.Previous();
				Expr right = Addition();
				expr = new Expr.Binary(expr, op, right);
			}
			return expr;
		}

		Expr Addition() {
			Expr expr = Multiplication();
			while (cursor.Match(Token.Kind.MINUS, Token.Kind.PLUS)) {
				Token op = cursor.Previous();
				Expr right = Multiplication();
				expr = new Expr.Binary(expr, op, right);
			}
			return expr;
		}

		Expr Multiplication() {
			Expr expr = Unary();
			while (cursor.Match(Token.Kind.SLASH, Token.Kind.STAR)) {
				Token op = cursor.Previous();
				Expr right = Unary();
				expr = new Expr.Binary(expr, op, right);
			}
			return expr;
		}

		Expr Unary() {
			if (cursor.Match(Token.Kind.BANG, Token.Kind.MINUS)) {
				Token op = cursor.Previous();
				Expr right = Unary();
				return new Expr.Unary(op, right);
			}
			return Primary();
		}

		Expr Primary() {
			if (cursor.Match(Token.Kind.FALSE))
				return new Expr.Literal(cursor.Previous(), false);
			if (cursor.Match(Token.Kind.TRUE))
				return new Expr.Literal(cursor.Previous(), true);
			if (cursor.Match(Token.Kind.NIL))
				return new Expr.Literal(cursor.Previous(), null);
			if (cursor.Match(Token.Kind.INT_LITERAL, Token.Kind.FLOAT_LITERAL, Token.Kind.STR_LITERAL)) {
				return new Expr.Literal(cursor.Previous(), cursor.Previous().literal);
			}
			/*
			if (cursor.Match(Token.Kind.LEFT_PAREN)) {
				Expr expr = Expression();
				cursor.Consume(Token.Kind.RIGHT_PAREN, "Expect ')' after expression.");
				return new Expr.Grouping(expr);
			}
			*/
			throw new ParseError(cursor.Peek(), "Expect expression.");
			return null;
		}










		void Synchronize() {
			cursor.Advance();
			while (!cursor.IsAtEnd()) {
				if (cursor.Previous().type == Token.Kind.NEWLINE)
					return;
				switch (cursor.Peek().type) {
					case Token.Kind.CLASS:
					//case Token.Kind.FUN:
					case Token.Kind.FOR:
					case Token.Kind.IF:
					case Token.Kind.WHILE:
					case Token.Kind.PRINT:
					case Token.Kind.RETURN:
					case Token.Kind.NEWLINE:
						return;
					default:
						break;
				}
				cursor.Advance();
			}
		}


		void Error(Token token, string message) {
			Display.Error(token, "", message);
			//Console.WriteLine("Parse error: {0} in {1} at {2}, {3}:{4}", message, token.filename, token.lexeme, token.line, token.offset);
		}





		/// <summary>
		/// Control the point in the token list we are looking at.
		/// </summary>
		class Cursor {
			int start;
			int current;
			List<Token> tokens;


			public Cursor(List<Token> tokens) {
				this.tokens = tokens;
				start = 0;
				current = 0;
			}

			public void Start() {
				start = current;
			}

			public bool Match(Token.Kind expected) {
				if (Check(expected)) {
					Advance();
					return true;
				}
				return false;
			}

			public bool Match(params Token.Kind[] expected) {
				foreach (var exp in expected) {
					if (Check(exp)) {
						Advance();
						return true;
					}
				}
				return false;
			}

			/// <summary>
			/// Matches any type, including IDENTIFIER for a user defined type...
			/// </summary>
			public bool MatchAnyType() {
				return Match(Token.Kind.INT, Token.Kind.INT8, Token.Kind.INT16, Token.Kind.INT32, Token.Kind.INT64, Token.Kind.INT128, Token.Kind.FLOAT, Token.Kind.FLOAT16, Token.Kind.FLOAT32, Token.Kind.FLOAT64, Token.Kind.FLOAT128, Token.Kind.IDENTIFIER);
			}

			/// <summary>
			/// Is the expected token type next.
			/// </summary>
			public bool Check(Token.Kind expected) {
				if (IsAtEnd())
					return false;
				return expected == Token.Kind.ANY || tokens[current].type == expected;
			}


			/// <summary>
			/// Are allExpected token types next.
			/// </summary>
			public bool CheckAll(params Token.Kind[] allExpected) {
				int offset = 0;
				foreach (Token.Kind expected in allExpected) {
					if ((current + offset) >= tokens.Count || expected != Token.Kind.ANY && tokens[current + offset].type != expected)
						return false;
					offset++;
				}
				return true;
			}

			/// <summary>
			/// Does the line contain (in order, starting at current position) the specified token kinds - dont start with ANY
			/// </summary>
			public bool LineCheck(params Token.Kind[] allExpected) {
				int lineOffset = 0;
				int paramOffset = 0;
				while (tokens[current + lineOffset].type != Token.Kind.NEWLINE && (current + lineOffset) < tokens.Count) {
					if (allExpected[paramOffset] == tokens[current + lineOffset].type) {
						while (tokens[current + lineOffset].type != Token.Kind.NEWLINE && (current + lineOffset) < tokens.Count) {
							if (allExpected[paramOffset] == Token.Kind.ANY || allExpected[paramOffset] == tokens[current + lineOffset].type) {
								paramOffset++;
								if (paramOffset >= allExpected.Length)
									return true;
							}
							lineOffset++;
						}
					}
					lineOffset++;
				}
				return false;
			}

			public Token Consume(Token.Kind expected, string message) {
				if (Check(expected))
					return Advance();
				throw new ParseError(Peek(), message);
			}

			public bool IsAtEnd() {
				return current >= tokens.Count;
			}

			public Token Advance() {
				return tokens[current++];
			}

			public Token Peek() {
				if (IsAtEnd())
					return new Token(Token.Kind.EOF);
				return tokens[current];
			}

			public Token PeekNext(int offset = 1) {
				if ((current + offset) >= tokens.Count)
					return new Token(Token.Kind.EOF);
				return tokens[current + offset];
			}

			public Token Previous() {
				if ((current - 1) >= tokens.Count)
					return new Token(Token.Kind.EOF);
				return tokens[current - 1];
			}

			/*public Token[] ToArray(int startOffset = 0, int endOffset = 0) {
				int len = (current + startOffset) - (start - endOffset);
				Token[] res = new Token[len];
				Array.Copy(tokens.ToArray(), start, res, 0, len);
				return res;
			}*/
		}   // Cursor




		/// <summary>
		///  Error exception for parser.
		/// </summary>
		class ParseError : Exception {
			public ParseError() { }
			public ParseError(Token token, string message) : base(string.Format("Parse error: {0} : {1}", message, token)) { }
		}



	} // Parser
}
