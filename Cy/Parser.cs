﻿using System.Collections.Generic;


namespace Cy.Parser {
	partial class Parser {
		readonly Cursor cursor;

		public Parser(Cursor cursor) {
			this.cursor = cursor;
		}

		public List<Ast.Stmt> Parse() {
			var statements = new List<Ast.Stmt>();
			while (!cursor.IsAtEnd()) {
				var s = Declaration();
				if (s != null) {
					statements.Add(s);
				}
			}
			return statements;
		}

		// <summary>Check for class definitions, function definition - including constructors and destructors and variable declaration.</summary>
		Ast.Stmt Declaration() {
			try {
				if (cursor.IsMatch(TokenType.IDENTIFIER) && cursor.IsMatch(TokenType.COLON)) {
					return DefineClass();
				}
				if (cursor.IsCheckAnyType()) {
					if (IsFuncDeclaration()) {
						return FunDeclaration();
					} else if (IsContructorDeclaration()) {
						return ConstructorDeclaration("structor");
					} else if (cursor.IsCheckNext(TokenType.IDENTIFIER)) {
						return VarDeclaration();
					}
				}
				return Statement();
			} catch (ParseException e) {
				Display.Error(e.token, e.Message);
				Synchronise();
			}
			return null;
		}

		bool IsFuncDeclaration() {
			if (cursor.IsCheckAnyType() && cursor.IsCheckNext(TokenType.IDENTIFIER)) {   // func, method or property
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
		bool IsContructorDeclaration() {
			if (cursor.PeekNext().tokenType == TokenType.IDENTIFIER &&
				cursor.PeekAt(2).tokenType == TokenType.LEFT_PAREN) {     // destructor
				return IsFuncArgs(2);
			}
			return false;
		}
		bool IsDestructorDeclaration() {
			if (cursor.Peek().tokenType == TokenType.TILDE &&
				cursor.PeekNext().tokenType == TokenType.IDENTIFIER &&
				cursor.PeekAt(2).tokenType == TokenType.LEFT_PAREN) {     // destructor
				return IsFuncArgs(2);
			}
			return false;
		}
		/// <summary>matches: (<anything in here>):</summary>
		bool IsFuncArgs(int idxtostart) {
			int lparenCount = 0;
			if (cursor.IsCheckAt(TokenType.LEFT_PAREN, idxtostart)) {
				lparenCount++;
			}
			while (lparenCount > 0) { 
				idxtostart++;
				TokenType toktype = cursor.PeekAt(idxtostart).tokenType;
				while (toktype != TokenType.RIGHT_PAREN) {
					if (toktype == TokenType.BACKSLASH) {
						while (!cursor.IsCheckAt(TokenType.NEWLINE, idxtostart)) {
							idxtostart++;
						}
					}
					if (toktype == TokenType.NEWLINE || toktype == TokenType.EOF) {
						return false;
					}
					idxtostart++;
					toktype = cursor.PeekAt(idxtostart).tokenType;
				}
				idxtostart++;
				if (cursor.IsCheckAt(TokenType.COLON, idxtostart)) {
					return true;
				}
			}
			return false;
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
					Error(astmt.token, "Object definitions should only contain methods, properties or class definitions.");
				}
			}
			return new Ast.Stmt.ClassDefinition(name, members, methods, classes);
		}


		/// <summary>
		/// Declare a function/method.
		/// </summary>
		Ast.Stmt.Function FunDeclaration() {
			var returnType = new Ast.Stmt.StmtType(cursor.Advance());
			Token name = cursor.Consume(TokenType.IDENTIFIER, "Expected function name.");
			cursor.Consume(TokenType.LEFT_PAREN, "Expect open bracket after function name.");
			var parameters = new List<Ast.Stmt.InputVar>();
			if (!cursor.IsCheck(TokenType.RIGHT_PAREN)) {
				do {
					Token typeTok;
					if (cursor.IsCheckAnyType()) {
						typeTok = cursor.Advance();
					} else {
						throw new ParseException(cursor.Peek(), "Expect parameter type.");
					}
					Token id = cursor.Consume(TokenType.IDENTIFIER, "Expect parameter name.");
					parameters.Add(new Ast.Stmt.InputVar(new Ast.Stmt.StmtType(typeTok), id));
				} while (cursor.IsMatch(TokenType.COMMA));
			}
			cursor.Consume(TokenType.RIGHT_PAREN, "Expect closing bracket after parameters.");
			cursor.Consume(TokenType.COLON, "Expect colon before function body.");
			cursor.Consume(TokenType.NEWLINE, "Expect newline before function body.");
			List<Ast.Stmt> body = Block();
			return new Ast.Stmt.Function(returnType, name, parameters, body);
		}

		Ast.Stmt.Function ConstructorDeclaration(string kind) {
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
			if (!cursor.IsCheck(TokenType.RIGHT_PAREN)) {
				do {
					Token typeTok;
					if (cursor.IsCheckAnyType()) {
						typeTok = cursor.Advance();
					} else {
						throw new ParseException(cursor.Peek(), "Expect parameter type.");
					}
					Token id = cursor.Consume(TokenType.IDENTIFIER, "Expect parameter name.");
					parameters.Add(new Ast.Stmt.InputVar(new Ast.Stmt.StmtType(typeTok), id));
				} while (cursor.IsMatch(TokenType.COMMA));
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
			if (cursor.IsMatch(TokenType.RETURN)) {
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
			Ast.Expr value = null;
			if (!cursor.IsCheck(TokenType.NEWLINE)) {
				value = Expression();
			}
			cursor.Consume(TokenType.NEWLINE, "Expect 'newline' after return value.");  // new line is not reqd (could be eof)
			return new Ast.Stmt.Return(keyword, value);
		}


		Ast.Stmt ExpressionStatement() {
			Ast.Expr expr = Expression();
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


		Ast.Expr Assignment() {
			Ast.Expr expr = Or();
			if (cursor.IsMatch(TokenType.EQUAL)) {
				Token equals = cursor.Previous();
				Ast.Expr value = Assignment();
				if (expr is Ast.Expr.Variable vexpr) {
					Token name = vexpr.token;
					return new Ast.Expr.Assign(name, value);
				} else if (expr is Ast.Expr.Get gexpr) {
					return new Ast.Expr.Set(gexpr.obj, gexpr.token, value);
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
			Ast.Expr initializer = null;
			if (cursor.IsMatch(TokenType.EQUAL)) {
				initializer = Expression();
			}
			cursor.Consume(TokenType.NEWLINE, "Expect 'newline' after variable declaration.");
			return new Ast.Stmt.Var(type, name, initializer);
		}


		/// <summary>
		/// Create an expression. could be a+b or 2+3 or could be rhs of an assign, a=2, etc...
		/// </summary>
		Ast.Expr Expression() {
			Ast.Expr expr = Assignment();
			//cursor.Consume(TokenType.NEWLINE, "Expect 'newline' after expression.");
			return expr;
		}

		Ast.Expr Or() {
			Ast.Expr expr = And();
			while (cursor.IsMatch(TokenType.OR)) {
				Token op = cursor.Previous();
				Ast.Expr right = And();
				//				expr = new Expr.Logical(expr, op, right);
			}
			return expr;
		}

		Ast.Expr And() {
			Ast.Expr expr = Equality();
			while (cursor.IsMatch(TokenType.AND)) {
				Token op = cursor.Previous();
				Ast.Expr right = Equality();
				//				expr = new Expr.Logical(expr, op, right);
			}
			return expr;
		}

		Ast.Expr Equality() {
			Ast.Expr expr = Compare();
			while (cursor.IsMatch(TokenType.BANG_EQUAL, TokenType.EQUAL_EQUAL)) {
				Token op = cursor.Previous();
				Ast.Expr right = Compare();
				expr = new Ast.Expr.Binary(expr, op, right);
			}
			return expr;
		}

		Ast.Expr Compare() {
			Ast.Expr expr = Addition();
			while (cursor.IsMatch(TokenType.GREATER, TokenType.GREATER_EQUAL, TokenType.LESS, TokenType.LESS_EQUAL)) {
				Token op = cursor.Previous();
				Ast.Expr right = Addition();
				expr = new Ast.Expr.Binary(expr, op, right);
			}
			return expr;
		}

		Ast.Expr Addition() {
			Ast.Expr expr = Multiplication();
			while (cursor.IsMatch(TokenType.MINUS, TokenType.PLUS)) {
				Token op = cursor.Previous();
				Ast.Expr right = Multiplication();
				expr = new Ast.Expr.Binary(expr, op, right);
			}
			return expr;
		}

		Ast.Expr Multiplication() {
			Ast.Expr expr = Unary();
			while (cursor.IsMatch(TokenType.SLASH, TokenType.STAR)) {
				Token op = cursor.Previous();
				Ast.Expr right = Unary();
				expr = new Ast.Expr.Binary(expr, op, right);
			}
			return expr;
		}

		Ast.Expr Unary() {
			if (cursor.IsMatch(TokenType.BANG, TokenType.MINUS)) {
				Token op = cursor.Previous();
				Ast.Expr right = Unary();
				return new Ast.Expr.Unary(op, right);
			}
			return Call();
		}

		Ast.Expr FinishCall(Ast.Expr callee) {
			List<Ast.Expr> arguments = new List<Ast.Expr>();
			if (!cursor.IsCheck(TokenType.RIGHT_PAREN)) {
				do {
					if (arguments.Count >= 255)
						Error(cursor.Peek(), "Cannot have more than 255 arguments.");
					arguments.Add(Expression());
				} while (cursor.IsMatch(TokenType.COMMA));
			}
			Token paren = cursor.Consume(TokenType.RIGHT_PAREN, "Expect ')' after arguments.");
			return new Ast.Expr.Call(callee, paren, arguments);
		}

		Ast.Expr Call() {
			Ast.Expr expr = Primary();
			while (true) {
				if (cursor.IsMatch(TokenType.LEFT_PAREN)) {
					expr = FinishCall(expr);
				} else if (cursor.IsMatch(TokenType.DOT)) {
					Token name = cursor.Consume(TokenType.IDENTIFIER, "Expect property name after '.'.");
					expr = new Ast.Expr.Get(expr, name);
				} else {
					break;
				}
			}
			return expr;
		}

		Ast.Expr Primary() {
			if (cursor.IsMatch(TokenType.FALSE)) {
				return new Ast.Expr.Literal(cursor.Previous(), false);
			}
			if (cursor.IsMatch(TokenType.TRUE)) {
				return new Ast.Expr.Literal(cursor.Previous(), true);
			}
			if (cursor.IsMatch(TokenType.NULL)) {
				return new Ast.Expr.Literal(cursor.Previous(), null);
			}
			if (cursor.IsMatch(TokenType.IDENTIFIER)) {
				Ast.Expr expr = new Ast.Expr.Variable(cursor.Previous());
				while (cursor.IsMatch(TokenType.DOT)) {
					Token name = cursor.Consume(TokenType.IDENTIFIER, "Expect property name after '.'.");
					expr = new Ast.Expr.Get(expr, name);
				}
				return expr;
			}
			if (cursor.Peek().IsLiteral()) {
				Token tok = cursor.Advance();
				return new Ast.Expr.Literal(tok, tok.literal);
			}

			/* for grouping expressions, i.e. 2 * (5+2)
			if (cursor.Match(TokenType.LEFT_PAREN)) {
				Expr expr = Expression();
				cursor.Consume(TokenType.RIGHT_PAREN, "Expect matching ')' after expression.");
				return new Expr.Grouping(expr);
			}
			*/
			throw new ParseException(cursor.Peek(), "Expect expression.");
		}

		void Synchronise() {
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