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


		bool IsFuncArgs(int idxtostart) {
			if (cursor.PeekNext(idxtostart++).type == Token.Kind.LEFT_PAREN) {
				Token.Kind toktype = cursor.PeekNext(idxtostart).type;
				while (toktype != Token.Kind.RIGHT_PAREN) {
					if (toktype == Token.Kind.NEWLINE || toktype == Token.Kind.EOF)
						return false;
					idxtostart++;
					toktype = cursor.PeekNext(idxtostart).type;
				}
				idxtostart++;
				if (cursor.PeekNext(idxtostart).type == Token.Kind.COLON)
					return true;
			}
			return false;
		}
		bool IsFuncDeclaration() {
			if (cursor.PeekAnyType() && cursor.PeekNext().type == Token.Kind.IDENTIFIER)	// func or method
				return IsFuncArgs(2);
			return false;
		}
		bool IsConOrDestructorDeclaration() {
			if (cursor.Peek().type == Token.Kind.IDENTIFIER && cursor.PeekNext().type == Token.Kind.LEFT_PAREN)		// con/destructor
				return IsFuncArgs(1);
			return false;
		}


		Stmt Declaration() {
			try {
				if (cursor.Peek().type == Token.Kind.IDENTIFIER && cursor.PeekNext().type == Token.Kind.COLON)
					return ClassDeclaration();
				if (cursor.PeekAnyType()) {
					if (IsFuncDeclaration())
						return FunDeclaration("function");
					else if (IsConOrDestructorDeclaration())
						return FunDeclaration("structor");
					else if (cursor.PeekNext().type == Token.Kind.IDENTIFIER)
						return VarDeclaration();
				}
				return Statement();
			} catch (ParseError e) {
				Display.Error(e.token, e.Message);
				Synchronize();
			}
			return null;
		}


		Stmt.StmtClass ClassDeclaration() {
			Token name = cursor.Advance();
			int startindent = name.indent;
			cursor.Consume(Token.Kind.COLON, "Expect ':' after " + name.lexeme + " for an object definition.");
			cursor.Consume(Token.Kind.NEWLINE, "Expect 'newline' after " + name.lexeme + ": for an object definition.");
			List<Stmt.Var> members = new List<Stmt.Var>();
			List<Stmt.Function> methods = new List<Stmt.Function>();
			List<Stmt.StmtClass> classes = new List<Stmt.StmtClass>();
			Stmt astmt;
			while (cursor.Peek().indent > startindent) {
				astmt = Declaration();
				if (astmt is Stmt.Var v) {
					members.Add(v);
				} else if (astmt is Stmt.Function f) {
					methods.Add(f);
				} else if (astmt is Stmt.StmtClass c) {
					classes.Add(c);
				} else {
					Error(astmt.token, "Object definitions should only contain methods, members or objects.");
				}
			}
			return new Stmt.StmtClass(name, members, methods, classes);
		}


		/// <summary>
		/// Declare a function/method.
		/// </summary>
		Stmt.Function FunDeclaration(string kind) {
			Stmt.StmtType type;
			if (kind == "function") {
				type = new Stmt.StmtType(cursor.Advance());
			} else {
				Token t = cursor.Peek();
				if (kind == "structor")     // destructor
					t = new Token(Token.Kind.VOID, t.lexeme, t.literal, t.indent, t.line, t.offset, t.filename);
				type = new Stmt.StmtType(t);
			}
			Token name = cursor.Consume(Token.Kind.IDENTIFIER, "Expect " + kind + " name.");
			cursor.Consume(Token.Kind.LEFT_PAREN, "Expect '(' after " + kind + " name.");
			List<Stmt.InputVar> parameters = new List<Stmt.InputVar>();
			if (!cursor.Check(Token.Kind.RIGHT_PAREN)) {
				do {
					if (parameters.Count >= 255)
						Error(cursor.Peek(), "Cannot have more than 255 parameters.");
					Token typeTok;
					if (cursor.PeekAnyType())
						typeTok = cursor.Advance();
					else
						throw new ParseError(cursor.Peek(), "Expect parameter type.");
					Token id = cursor.Consume(Token.Kind.IDENTIFIER, "Expect parameter name.");
					parameters.Add(new Stmt.InputVar(new Stmt.StmtType(typeTok), id));
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
			cursor.Consume(Token.Kind.NEWLINE, "Expect 'newline' after return value.");	// new line is not reqd (could be eof)
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
			while (startIndent <= cursor.Peek().indent && !cursor.IsAtEnd()) {
				Stmt stmt = Declaration();
				if (stmt != null)
					statements.Add(stmt);
			}
			return statements;
		}


		Expr Assignment() {
			Expr expr = Or();
			if (cursor.Match(Token.Kind.EQUAL)) {
				Token equals = cursor.Previous();
				Expr value = Assignment();
				if (expr is Expr.Variable vexpr) {
					Token name = vexpr.token;
					return new Expr.Assign(name, value);
				} else if (expr is Expr.Get gexpr) {
					return new Expr.Set(gexpr.obj, gexpr.token, value);
				}
				Error(equals, "Invalid assignment target."); // [no-throw]
			}
			return expr;
		}

		/// <summary>
		/// Create a variable which optionally has an assigned expression.
		/// </summary>
		Stmt VarDeclaration() {
			Token type = cursor.Advance();
			Token name = cursor.Consume(Token.Kind.IDENTIFIER, "Expect variable name.");
			Expr initializer = null;
			if (cursor.Match(Token.Kind.EQUAL))
				initializer = Expression();
			cursor.Consume(Token.Kind.NEWLINE, "Expect 'newline' after variable declaration.");
			return new Stmt.Var(type, name, initializer);
		}


		/// <summary>
		/// Create an expression. could be a+b or 2+3 or could be rhs of an assign, a=2, etc...
		/// </summary>
		Expr Expression() {
			Expr expr = Assignment();
			//cursor.Consume(Token.Kind.NEWLINE, "Expect 'newline' after expression.");
			return expr;
		}

		Expr Or() {
			Expr expr = And();
			while (cursor.Match(Token.Kind.OR)) {
				Token op = cursor.Previous();
				Expr right = And();
//				expr = new Expr.Logical(expr, op, right);
			}
			return expr;
		}

		Expr And() {
			Expr expr = Equality();
			while (cursor.Match(Token.Kind.AND)) {
				Token op = cursor.Previous();
				Expr right = Equality();
//				expr = new Expr.Logical(expr, op, right);
			}
			return expr;
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
			return Call();
		}

		Expr FinishCall(Expr callee) {
			List<Expr> arguments = new List<Expr>();
			if (!cursor.Check(Token.Kind.RIGHT_PAREN)) {
				do {
					if (arguments.Count >= 255)
						Error(cursor.Peek(), "Cannot have more than 255 arguments.");
					arguments.Add(Expression());
				} while (cursor.Match(Token.Kind.COMMA));
			}
			Token paren = cursor.Consume(Token.Kind.RIGHT_PAREN, "Expect ')' after arguments.");
			return new Expr.Call(callee, paren, arguments);
		}

		Expr Call() {
			Expr expr = Primary();
			while (true) {
				if (cursor.Match(Token.Kind.LEFT_PAREN)) {
					expr = FinishCall(expr);
				} else if (cursor.Match(Token.Kind.DOT)) {
					Token name = cursor.Consume(Token.Kind.IDENTIFIER, "Expect property name after '.'.");
					expr = new Expr.Get(expr, name);
				} else {
					break;
				}
			}
			return expr;
		}
		
		Expr Primary() {
			if (cursor.Match(Token.Kind.FALSE))
				return new Expr.Literal(cursor.Previous(), false);
			if (cursor.Match(Token.Kind.TRUE))
				return new Expr.Literal(cursor.Previous(), true);
			if (cursor.Match(Token.Kind.NIL))
				return new Expr.Literal(cursor.Previous(), null);
			if (cursor.Match(Token.Kind.IDENTIFIER)) {
				Expr expr = new Expr.Variable(cursor.Previous());
				while (cursor.Match(Token.Kind.DOT)) {
					Token name = cursor.Consume(Token.Kind.IDENTIFIER, "Expect property name after '.'.");
					expr = new Expr.Get(expr, name);
				}
				return expr;
			}
			if (cursor.Peek().IsLiteral()) {
				Token tok = cursor.Advance();
				return new Expr.Literal(tok, tok.literal);
			}
			
			/* for grouping expressions, i.e. 2 * (5+2)
			if (cursor.Match(Token.Kind.LEFT_PAREN)) {
				Expr expr = Expression();
				cursor.Consume(Token.Kind.RIGHT_PAREN, "Expect matching ')' after expression.");
				return new Expr.Grouping(expr);
			}
			*/
			throw new ParseError(cursor.Peek(), "Expect expression.");
		}










		void Synchronize() {
			cursor.Advance();
			while (!cursor.IsAtEnd()) {
				if (cursor.Previous().type == Token.Kind.NEWLINE)
					return;
				switch (cursor.Peek().type) {
					case Token.Kind.NEWLINE:
						return;
					default:
						break;
				}
				cursor.Advance();
			}
		}


		void Error(Token token, string message) {
			Display.Error(token, message);
		}





		/// <summary>
		/// Control the point in the token list we are looking at.
		/// </summary>
		class Cursor {
			int current;
			List<Token> tokens;


			public Cursor(List<Token> tokens) {
				this.tokens = tokens;
				current = 0;
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
			public bool PeekAnyType() {
				if (tokens[current].IsAnyType()) {
					return true;
				}
				return false;
			}

			/// <summary>
			/// Is the expected token type next.
			/// </summary>
			public bool Check(Token.Kind expected) {
				if (IsAtEnd())
					return false;
				return tokens[current].type == expected || expected == Token.Kind.ANY;
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
				return current >= tokens.Count || tokens[current].type == Token.Kind.EOF;
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


		}   // Cursor




		/// <summary>
		///  Error exception for parser.
		/// </summary>
		class ParseError : Exception {
			public Token token;
			public ParseError() { }
			public ParseError(Token token, string message) : base(string.Format("Parse error: {0} : {1}", message, token)) {
				this.token = token;
			}
		}



	} // Parser
}
