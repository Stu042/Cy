using System.Collections.Generic;


namespace Cy {
	partial class Parser {
		readonly Cursor cursor;

		public Parser(List<Token> tokens) {
			cursor = new Cursor(tokens);
		}

		public List<Ast.Stmt> Parse() {
			List<Ast.Stmt> statements = new List<Ast.Stmt>();
			while (!cursor.IsAtEnd()) {
				Ast.Stmt s = Declaration();
				if (s != null) {
					statements.Add(s);
				}
			}
			return statements;
		}


		bool IsFuncArgs(int idxtostart) {
			if (cursor.PeekNext(idxtostart++).tokenType == TokenType.LEFT_PAREN) {
				TokenType toktype = cursor.PeekNext(idxtostart).tokenType;
				while (toktype != TokenType.RIGHT_PAREN) {
					if (toktype == TokenType.NEWLINE || toktype == TokenType.EOF) {
						return false;
					}
					idxtostart++;
					toktype = cursor.PeekNext(idxtostart).tokenType;
				}
				idxtostart++;
				if (cursor.PeekNext(idxtostart).tokenType == TokenType.COLON) {
					return true;
				}
			}
			return false;
		}
		bool IsFuncDeclaration() {
			if (cursor.PeekAnyType() && cursor.PeekNext().tokenType == TokenType.IDENTIFIER) {   // func or method
				return IsFuncArgs(2);
			}
			return false;
		}
		bool IsConOrDestructorDeclaration() {
			if (cursor.Peek().tokenType == TokenType.IDENTIFIER && cursor.PeekNext().tokenType == TokenType.LEFT_PAREN) {     // con/destructor
				return IsFuncArgs(1);
			}
			return false;
		}

		Ast.Stmt Declaration() {
			try {
				if (cursor.Peek().tokenType == TokenType.IDENTIFIER && cursor.PeekNext().tokenType == TokenType.COLON) {
					return DefineClass();
				}
				if (cursor.PeekAnyType()) {
					if (IsFuncDeclaration()) {
						return FunDeclaration("function");
					} else if (IsConOrDestructorDeclaration()) {
						return FunDeclaration("structor");
					} else if (cursor.PeekNext().tokenType == TokenType.IDENTIFIER) {
						return VarDeclaration();
					}
				}
				return Statement();
			} catch (ParseError e) {
				Display.Error(e.token, e.Message);
				Synchronize();
			}
			return null;
		}


		Ast.Stmt.ClassDefinition DefineClass() {
			Token name = cursor.Advance();
			int startindent = name.indent;
			cursor.Consume(TokenType.COLON, "Expect a : after '" + name.lexeme + "' for an object definition.");
			cursor.Consume(TokenType.NEWLINE, "Expect a newline after '" + name.lexeme + "' for an object definition.");
			var members = new List<Ast.Stmt.Var>();
			var methods = new List<Ast.Stmt.Function>();
			var classes = new List<Ast.Stmt.ClassDefinition>();
			Ast.Stmt astmt;
			while (cursor.Peek().indent > startindent) {
				astmt = Declaration();
				if (astmt is Ast.Stmt.Var v) {
					members.Add(v);
				} else if (astmt is Ast.Stmt.Function f) {
					methods.Add(f);
				} else if (astmt is Ast.Stmt.ClassDefinition c) {
					classes.Add(c);
				} else {
					Error(astmt.token, "Object definitions should only contain methods, members or objects.");
				}
			}
			return new Ast.Stmt.ClassDefinition(name, members, methods, classes);
		}


		/// <summary>
		/// Declare a function/method.
		/// </summary>
		Ast.Stmt.Function FunDeclaration(string kind) {
			Ast.Stmt.StmtType type;
			if (kind == "function") {
				type = new Ast.Stmt.StmtType(cursor.Advance());
			} else {
				Token t = cursor.Peek();
				if (kind == "structor") {     // destructor
					t = new Token(TokenType.VOID, t.lexeme, t.literal, t.indent, t.line, t.offset, t.filename);
				}
				type = new Ast.Stmt.StmtType(t);
			}
			Token name = cursor.Consume(TokenType.IDENTIFIER, "Expect " + kind + " name.");
			cursor.Consume(TokenType.LEFT_PAREN, "Expect '(' after " + kind + " name.");
			List<Ast.Stmt.InputVar> parameters = new List<Ast.Stmt.InputVar>();
			if (!cursor.Check(TokenType.RIGHT_PAREN)) {
				do {
					Token typeTok;
					if (cursor.PeekAnyType()) {
						typeTok = cursor.Advance();
					} else {
						throw new ParseError(cursor.Peek(), "Expect parameter type.");
					}
					Token id = cursor.Consume(TokenType.IDENTIFIER, "Expect parameter name.");
					parameters.Add(new Ast.Stmt.InputVar(new Ast.Stmt.StmtType(typeTok), id));
				} while (cursor.Match(TokenType.COMMA));
			}
			cursor.Consume(TokenType.RIGHT_PAREN, "Expect ')' after parameters.");
			cursor.Consume(TokenType.COLON, "Expect ':' before " + kind + " body.");
			cursor.Consume(TokenType.NEWLINE, "Expect 'newline' before " + kind + " body.");
			List<Ast.Stmt> body = Block();
			return new Ast.Stmt.Function(type, name, parameters, body);
		}


		Ast.Stmt Statement() {
			/*
			if (cursor.Match(TokenType.FOR)) {
				return ForStatement();
			}
			if (cursor.Match(TokenType.IF)) {
				return IfStatement();
			}
			if (cursor.Match(TokenType.PRINT)) {
				return PrintStatement();
			}
			*/
			if (cursor.Match(TokenType.RETURN)) {
				return ReturnStatement();
			}
			/*
			if (cursor.Match(TokenType.WHILE)) {
				return WhileStatement();
			}
			*/
			return ExpressionStatement();
		}



		Ast.Stmt ReturnStatement() {
			Token keyword = cursor.Previous();
			Expr value = null;
			if (!cursor.Check(TokenType.NEWLINE)) {
				value = Expression();
			}
			cursor.Consume(TokenType.NEWLINE, "Expect 'newline' after return value.");  // new line is not reqd (could be eof)
			return new Ast.Stmt.Return(keyword, value);
		}


		Ast.Stmt ExpressionStatement() {
			Expr expr = Expression();
			cursor.Consume(TokenType.NEWLINE, "Expect 'newline' after expression.");
			return new Ast.Stmt.Expression(expr);
		}


		/// <summary>
		/// Get the current Block of statements.
		/// </summary>
		List<Ast.Stmt> Block() {
			List<Ast.Stmt> statements = new List<Ast.Stmt>();
			int startIndent = cursor.Peek().indent;
			while (startIndent <= cursor.Peek().indent && !cursor.IsAtEnd()) {
				Ast.Stmt stmt = Declaration();
				if (stmt != null) {
					statements.Add(stmt);
				}
			}
			return statements;
		}


		Expr Assignment() {
			Expr expr = Or();
			if (cursor.Match(TokenType.EQUAL)) {
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
		Ast.Stmt VarDeclaration() {
			Token type = cursor.Advance();
			Token name = cursor.Consume(TokenType.IDENTIFIER, "Expect variable name.");
			Expr initializer = null;
			if (cursor.Match(TokenType.EQUAL)) {
				initializer = Expression();
			}
			cursor.Consume(TokenType.NEWLINE, "Expect 'newline' after variable declaration.");
			return new Ast.Stmt.Var(type, name, initializer);
		}


		/// <summary>
		/// Create an expression. could be a+b or 2+3 or could be rhs of an assign, a=2, etc...
		/// </summary>
		Expr Expression() {
			Expr expr = Assignment();
			//cursor.Consume(TokenType.NEWLINE, "Expect 'newline' after expression.");
			return expr;
		}

		Expr Or() {
			Expr expr = And();
			while (cursor.Match(TokenType.OR)) {
				Token op = cursor.Previous();
				Expr right = And();
				//				expr = new Expr.Logical(expr, op, right);
			}
			return expr;
		}

		Expr And() {
			Expr expr = Equality();
			while (cursor.Match(TokenType.AND)) {
				Token op = cursor.Previous();
				Expr right = Equality();
				//				expr = new Expr.Logical(expr, op, right);
			}
			return expr;
		}

		Expr Equality() {
			Expr expr = Compare();
			while (cursor.Match(TokenType.BANG_EQUAL, TokenType.EQUAL_EQUAL)) {
				Token op = cursor.Previous();
				Expr right = Compare();
				expr = new Expr.Binary(expr, op, right);
			}
			return expr;
		}

		Expr Compare() {
			Expr expr = Addition();
			while (cursor.Match(TokenType.GREATER, TokenType.GREATER_EQUAL, TokenType.LESS, TokenType.LESS_EQUAL)) {
				Token op = cursor.Previous();
				Expr right = Addition();
				expr = new Expr.Binary(expr, op, right);
			}
			return expr;
		}

		Expr Addition() {
			Expr expr = Multiplication();
			while (cursor.Match(TokenType.MINUS, TokenType.PLUS)) {
				Token op = cursor.Previous();
				Expr right = Multiplication();
				expr = new Expr.Binary(expr, op, right);
			}
			return expr;
		}

		Expr Multiplication() {
			Expr expr = Unary();
			while (cursor.Match(TokenType.SLASH, TokenType.STAR)) {
				Token op = cursor.Previous();
				Expr right = Unary();
				expr = new Expr.Binary(expr, op, right);
			}
			return expr;
		}

		Expr Unary() {
			if (cursor.Match(TokenType.BANG, TokenType.MINUS)) {
				Token op = cursor.Previous();
				Expr right = Unary();
				return new Expr.Unary(op, right);
			}
			return Call();
		}

		Expr FinishCall(Expr callee) {
			List<Expr> arguments = new List<Expr>();
			if (!cursor.Check(TokenType.RIGHT_PAREN)) {
				do {
					if (arguments.Count >= 255)
						Error(cursor.Peek(), "Cannot have more than 255 arguments.");
					arguments.Add(Expression());
				} while (cursor.Match(TokenType.COMMA));
			}
			Token paren = cursor.Consume(TokenType.RIGHT_PAREN, "Expect ')' after arguments.");
			return new Expr.Call(callee, paren, arguments);
		}

		Expr Call() {
			Expr expr = Primary();
			while (true) {
				if (cursor.Match(TokenType.LEFT_PAREN)) {
					expr = FinishCall(expr);
				} else if (cursor.Match(TokenType.DOT)) {
					Token name = cursor.Consume(TokenType.IDENTIFIER, "Expect property name after '.'.");
					expr = new Expr.Get(expr, name);
				} else {
					break;
				}
			}
			return expr;
		}

		Expr Primary() {
			if (cursor.Match(TokenType.FALSE)) {
				return new Expr.Literal(cursor.Previous(), false);
			}
			if (cursor.Match(TokenType.TRUE)) {
				return new Expr.Literal(cursor.Previous(), true);
			}
			if (cursor.Match(TokenType.NIL)) {
				return new Expr.Literal(cursor.Previous(), null);
			}
			if (cursor.Match(TokenType.IDENTIFIER)) {
				Expr expr = new Expr.Variable(cursor.Previous());
				while (cursor.Match(TokenType.DOT)) {
					Token name = cursor.Consume(TokenType.IDENTIFIER, "Expect property name after '.'.");
					expr = new Expr.Get(expr, name);
				}
				return expr;
			}
			if (cursor.Peek().IsLiteral()) {
				Token tok = cursor.Advance();
				return new Expr.Literal(tok, tok.literal);
			}

			/* for grouping expressions, i.e. 2 * (5+2)
			if (cursor.Match(TokenType.LEFT_PAREN)) {
				Expr expr = Expression();
				cursor.Consume(TokenType.RIGHT_PAREN, "Expect matching ')' after expression.");
				return new Expr.Grouping(expr);
			}
			*/
			throw new ParseError(cursor.Peek(), "Expect expression.");
		}

		void Synchronize() {
			cursor.Advance();
			while (!cursor.IsAtEnd()) {
				if (cursor.Previous().tokenType == TokenType.NEWLINE) {
					return;
				}
				switch (cursor.Peek().tokenType) {
					case TokenType.NEWLINE:
						return;
					default:
						break;
				}
				cursor.Advance();
			}
		}


		void Error(Token token, string message) {
			Display.Error(token, "Parser error:" + message);
		}
	}
}
