using Cy.Preprocesor.Interfaces;

using System.Collections.Generic;


namespace Cy.Preprocesor {
	public class Parser {
		readonly ParserCursor cursor;
		readonly IErrorDisplay display;

		public Parser(ParserCursor cursor, IErrorDisplay display) {
			this.cursor = cursor;
			this.display = display;
		}

		public List<Stmt> Parse() {
			var statements = new List<Stmt>();
			while (!cursor.IsAtEnd()) {
				try {
					var s = Declaration();
					if (s != null) {
						statements.Add(s);
					}
				} catch (ParserException e) {
					display.Error(e.token, "Parser error:" + e.Message);
					Synchronise();
				}
			}
			return statements;
		}

		// <summary>Check for class definitions, function definition - including constructors and destructors and variable declaration.</summary>
		Stmt Declaration() {
			if (cursor.IsCheckAll(TokenType.IDENTIFIER, TokenType.COLON)) {
				return DefineClass();
			}
			if (IsContructorDeclaration()) {
				return ConstructorDeclaration();
			}
			if (cursor.IsCheckAnyType()) {
				if (IsFuncDeclaration()) {
					return FunDeclaration();
				}
				if (cursor.IsCheckNext(TokenType.IDENTIFIER)) {
					return VarDeclaration();
				}
			}
			return Statement();
		}

		bool IsFuncDeclaration() {
			if (cursor.IsCheckAnyType() && cursor.IsCheckNext(TokenType.IDENTIFIER)) {
				return CheckIsFuncArgs(2);
			}
			return false;
		}

		bool IsContructorDeclaration() {
			if (cursor.IsCheckAll(TokenType.IDENTIFIER, TokenType.LEFT_PAREN)) {
				return CheckIsFuncArgs();
			}
			return false;
		}

		/// <summary>matches: IDENTIFIER(<anything in here>):\n</summary>
		bool CheckIsFuncArgs(int idxtostart = 1) {
			int lparenCount = 0;
			do {
				TokenType toktype = cursor.PeekAt(idxtostart).tokenType;
				if (cursor.IsCheckAt(TokenType.LEFT_PAREN, idxtostart)) {
					lparenCount++;
				} else if (toktype == TokenType.RIGHT_PAREN) {
					--lparenCount;
				} else if (toktype == TokenType.BACKSLASH) {
					do {
						idxtostart++;
					} while (!cursor.IsCheckAt(TokenType.NEWLINE, idxtostart) && !cursor.IsCheckAt(TokenType.EOF, idxtostart));
				} else if (toktype == TokenType.NEWLINE || toktype == TokenType.EOF) {
					return false;
				}
				idxtostart++;
			} while (lparenCount > 0);
			if (!cursor.IsCheckAt(TokenType.COLON, idxtostart++)) {
				return false;
			}
			if (cursor.IsCheckAt(TokenType.NEWLINE, idxtostart)) {
				return true;
			}
			return false;
		}

		Stmt.ClassDefinition DefineClass() {
			Token name = cursor.Advance();
			int startIndent = name.indent;
			cursor.Consume(TokenType.COLON, "Expect a : after '" + name.lexeme + "' for an object definition.");
			cursor.Consume(TokenType.NEWLINE, "Expect a newline after '" + name.lexeme + "' for an object definition.");
			var members = new List<Stmt.Var>();
			var methods = new List<Stmt.Function>();
			var classes = new List<Stmt.ClassDefinition>();
			while (!cursor.IsAtEnd() && (cursor.Peek().indent > startIndent || cursor.Peek().tokenType == TokenType.NEWLINE)) {
				var astmt = Declaration();
				if (astmt is Stmt.Var v) {
					members.Add(v);
				} else if (astmt is Stmt.Function f) {
					methods.Add(f);
				} else if (astmt is Stmt.ClassDefinition c) {
					classes.Add(c);
				} else {
					display.Error(astmt.token, "Object definitions should contain only methods, properties or class definitions.");
				}
			}
			return new Stmt.ClassDefinition(name, members, methods, classes);
		}


		/// <summary>Declare a function/method.</summary>
		Stmt.Function FunDeclaration() {
			var returnType = new Stmt.StmtType(new List<Token> { cursor.Advance() });
			Token name = cursor.Consume(TokenType.IDENTIFIER, "Expected function name.");
			cursor.Consume(TokenType.LEFT_PAREN, "Expect open bracket after function name.");
			var parameters = new List<Stmt.InputVar>();
			if (!cursor.IsCheck(TokenType.RIGHT_PAREN)) {
				do {
					Token typeTok;
					if (cursor.IsCheckAnyType()) {
						typeTok = cursor.Advance();
					} else {
						throw new ParserException(cursor.Peek(), "Expect parameter type.");
					}
					Token id = cursor.Consume(TokenType.IDENTIFIER, "Expect parameter name.");
					parameters.Add(new Stmt.InputVar(new Stmt.StmtType(new List<Token> { typeTok }), id));
				} while (cursor.IsMatch(TokenType.COMMA));
			}
			cursor.Consume(TokenType.RIGHT_PAREN, "Expect closing bracket after parameters.");
			cursor.Consume(TokenType.COLON, "Expect colon before function body.");
			cursor.Consume(TokenType.NEWLINE, "Expect newline before function body.");
			List<Stmt> body = Block();
			return new Stmt.Function(returnType, name, parameters, body);
		}

		Stmt.Function ConstructorDeclaration() {
			Token name = cursor.Advance();
			string structorType;
			Stmt.StmtType returnType;
			if (name.lexeme[0] == '~') {
				structorType = "Destructor";
				returnType = new Stmt.StmtType(new List<Token> { new Token("void", TokenType.VOID) });
			} else {
				structorType = "Constructor";
				returnType = new Stmt.StmtType(new List<Token> { new Token("void", TokenType.VOID)/*new Token(name.lexeme, name.tokenType)*/ });
			}
			cursor.Consume(TokenType.LEFT_PAREN, $"Expect opening bracket after {structorType} name.");
			List<Stmt.InputVar> parameters = new();
			if (!cursor.IsCheck(TokenType.RIGHT_PAREN)) {
				do {
					Token typeTok;
					if (cursor.IsCheckAnyType()) {
						typeTok = cursor.Advance();
					} else {
						throw new ParserException(cursor.Peek(), "Expect parameter type.");
					}
					Token id = cursor.Consume(TokenType.IDENTIFIER, "Expect parameter name.");
					parameters.Add(new Stmt.InputVar(new Stmt.StmtType(new List<Token> { typeTok }), id));
				} while (cursor.IsMatch(TokenType.COMMA));
			}
			cursor.Consume(TokenType.RIGHT_PAREN, "Expect closing bracket after parameters.");
			cursor.Consume(TokenType.COLON, $"Expect colon before {structorType} body.");
			cursor.Consume(TokenType.NEWLINE, $"Expect 'newline' before {structorType} body.");
			var body = Block();
			return new Stmt.Function(returnType, name, parameters, body);
		}

		Stmt Statement() {
			var tokenType = cursor.Peek().tokenType;
			return tokenType switch {
				TokenType.FOR => ForStatement(),
				TokenType.IF => IfStatement(),
				TokenType.RETURN => ReturnStatement(),
				TokenType.WHILE => WhileStatement(),
				_ => ExpressionStatement(),
			};
		}

		/// <summary>Statement is a while statement, i.e. "while(condition):" or "while condition:"</summary>
		Stmt WhileStatement() {
			Token whileKeyword = cursor.Advance();
			Expr condition = Expression();
			cursor.Consume(TokenType.COLON, "Expect colon after for statement.");
			cursor.Consume(TokenType.NEWLINE, "Expect newline at end of for statement.");
			var body = Block();
			return new Stmt.While(whileKeyword, condition, body);
		}

		/// <summary>Statement is a for statement, i.e. "for(type iterator collection):", maybe do "int a = for b:"</summary>
		Stmt ForStatement() {
			Token forKeyword = cursor.Advance();
			cursor.Consume(TokenType.LEFT_PAREN, "Expect opening bracket after for statement.");
			Token iteratorTypeToken = cursor.Advance();
			var iteratorType = new Stmt.StmtType(new List<Token> { iteratorTypeToken });
			Token iteratorName = cursor.Consume(TokenType.IDENTIFIER, "Expect variable name for iterator in for statement.");
			Expr collection = Expression();
			cursor.Consume(TokenType.RIGHT_PAREN, "Expect closing bracket after for statement.");
			cursor.Consume(TokenType.COLON, "Expect colon after for statement.");
			cursor.Consume(TokenType.NEWLINE, "Expect newline at end of for statement.");
			var body = Block();
			return new Stmt.For(forKeyword, iteratorType, iteratorName, collection, body);
		}

		/// <summary>Statement is an if statement, i.e. "if condition:" or "if (condition):"</summary>
		Stmt IfStatement() {
			Token ifKeyword = cursor.Advance();
			Expr condition = Expression();
			cursor.Consume(TokenType.COLON, "Expect colon after if statement.");
			cursor.Consume(TokenType.NEWLINE, "Expect newline after colon.");
			var body = Block();
			List<Stmt> elseBody = null;
			if (cursor.IsMatch(TokenType.ELSE)) {
				elseBody = Block();
			}
			return new Stmt.If(ifKeyword, condition, body, elseBody);
		}

		Stmt ReturnStatement() {
			Token keyword = cursor.Advance();
			Expr value = null;
			if (!cursor.IsCheckAny(TokenType.NEWLINE, TokenType.EOF)) {
				value = Expression();
			}
			cursor.ConsumeAny("Expect newline after return value.", TokenType.NEWLINE, TokenType.EOF);  // new line is not reqd (could be eof)
			return new Stmt.Return(keyword, value);
		}


		// Cy will not require this...
		Stmt ExpressionStatement() {
			Expr expr = Expression();
			cursor.ConsumeAny("Expect newline after expression.", TokenType.NEWLINE, TokenType.EOF);
			return new Stmt.Expression(expr);
		}

		/// <summary>Get the current Block of statements.</summary>
		List<Stmt> Block() {
			List<Stmt> statements = new();
			int startIndent = cursor.Peek().indent;
			while (!cursor.IsAtEnd() && cursor.Peek().indent >= startIndent) {
				Stmt stmt = Declaration();
				if (stmt != null) {
					statements.Add(stmt);
				}
			}
			return statements;
		}

		/// <summary>Create a variable which optionally has an assigned expression.
		/// Some syntatic sugar for a for statement, i.e. int a = each b:</summary>
		Stmt VarDeclaration() {
			Token typeToken = cursor.Advance();
			var type = new Stmt.StmtType(new List<Token> { typeToken });
			Token name = cursor.Consume(TokenType.IDENTIFIER, "Expect variable name.");
			Expr initializer = null;
			if (cursor.IsMatch(TokenType.EQUAL)) {
				if (cursor.IsMatch(TokenType.EACH)) {
					Token forKeyword = cursor.Previous();
					Expr collection = Expression();
					cursor.Consume(TokenType.COLON, "Expect colon after each statement.");
					cursor.Consume(TokenType.NEWLINE, "Expect newline at end of each statement.");
					var body = Block();
					return new Stmt.For(forKeyword, type, name, collection, body);
				} else {
					initializer = Expression();
				}
			}
			cursor.Consume(TokenType.NEWLINE, "Expect newline after variable declaration.");
			return new Stmt.Var(typeToken, name, initializer);
		}

		/// <summary>Create an expression. could be a+b or 2+3 or could be rhs of an assign, a=2, etc...</summary>
		Expr Expression() {
			Expr expr = Assignment();
			return expr;
		}

		Expr Assignment() {
			Expr expr = Or();
			if (cursor.IsMatch(TokenType.EQUAL)) {
				Token equals = cursor.Previous();
				Expr value = Assignment();
				if (expr is Expr.Variable vexpr) {
					Token name = vexpr.token;
					return new Expr.Assign(name, value);
				} else if (expr is Expr.Get gexpr) {
					return new Expr.Set(gexpr.obj, gexpr.token, value);
				}
				display.Error(equals, "Invalid assignment target."); // [no-throw]
			}
			return expr;
		}
		/*
		Ast.Stmt ForStatement() {
			Token forKeyword = cursor.Advance();
			cursor.Consume(TokenType.LEFT_PAREN, "Expect opening bracket after for statement.");
			Token iteratorType = cursor.Advance();
			Token iteratorName = cursor.Consume(TokenType.IDENTIFIER, "Expect variable name for iterator in for statement.");
			Ast.Expr collection = Expression();
			cursor.Consume(TokenType.RIGHT_PAREN, "Expect closing bracket after for statement.");
			cursor.Consume(TokenType.COLON, "Expect colon after for statement.");
			cursor.Consume(TokenType.NEWLINE, "Expect newline at end of for statement.");
			var body = Block();
			return new Ast.Stmt.For(forKeyword, iteratorType, iteratorName, collection, body);
		}
		*/

		Expr Or() {
			Expr expr = And();
			while (cursor.IsMatch(TokenType.OR)) {
				Token op = cursor.Previous();
				Expr right = And();
				//				expr = new Expr.Logical(expr, op, right);
			}
			return expr;
		}

		Expr And() {
			Expr expr = Equality();
			while (cursor.IsMatch(TokenType.AND)) {
				Token op = cursor.Previous();
				Expr right = Equality();
				//expr = new Ast.Expr.Logical(expr, op, right);
			}
			return expr;
		}

		/// <summary>Expression tests for not equal and equal.</summary>
		Expr Equality() {
			Expr expr = Compare();
			while (cursor.IsMatchAny(TokenType.BANG_EQUAL, TokenType.EQUAL_EQUAL)) {
				Token op = cursor.Previous();
				Expr right = Compare();
				expr = new Expr.Binary(expr, op, right);
			}
			return expr;
		}

		Expr Compare() {
			Expr expr = Addition();
			while (cursor.IsMatchAny(TokenType.GREATER, TokenType.GREATER_EQUAL, TokenType.LESS, TokenType.LESS_EQUAL)) {
				Token op = cursor.Previous();
				Expr right = Addition();
				expr = new Expr.Binary(expr, op, right);
			}
			return expr;
		}

		/// <summary>Expression is an addition or subtraction.</summary>
		Expr Addition() {
			Expr expr = Multiplication();
			while (cursor.IsMatchAny(TokenType.MINUS, TokenType.PLUS)) {
				Token op = cursor.Previous();
				Expr right = Multiplication();
				expr = new Expr.Binary(expr, op, right);
			}
			return expr;
		}

		/// <summary>Expression is a multiplication or division.</summary>
		Expr Multiplication() {
			Expr expr = Unary();
			while (cursor.IsMatchAny(TokenType.SLASH, TokenType.STAR)) {
				Token op = cursor.Previous();
				Expr right = Unary();
				expr = new Expr.Binary(expr, op, right);
			}
			return expr;
		}

		/// <summary>Expression is a number.</summary>
		Expr Unary() {
			if (cursor.IsMatchAny(TokenType.BANG, TokenType.MINUS, TokenType.MINUSMINUS, TokenType.PLUSPLUS)) {
				Token op = cursor.Previous();
				Expr right = Unary();
				return new Expr.Unary(op, right);
			}
			return Call();
		}

		/// <summary>Call a method/function.</summary>
		Expr Call() {
			Expr expr = Primary();
			while (true) {
				if (cursor.IsMatch(TokenType.LEFT_PAREN)) {
					expr = FinishCall(expr);
				} else if (cursor.IsMatch(TokenType.DOT)) {
					Token name = cursor.Consume(TokenType.IDENTIFIER, "Expect property name after dot.");
					expr = new Expr.Get(expr, name);
				} else {
					break;
				}
			}
			return expr;
		}

		/// <summary>Finish calling a method/function.</summary>
		Expr FinishCall(Expr callee) {
			var arguments = new List<Expr>();
			if (!cursor.IsCheck(TokenType.RIGHT_PAREN)) {
				do {
					arguments.Add(Expression());
				} while (cursor.IsMatch(TokenType.COMMA));
			}
			Token paren = cursor.Consume(TokenType.RIGHT_PAREN, "Expect closing bracket after arguments.");
			return new Expr.Call(callee, paren, arguments);
		}

		/// <summary>Parse an expression.</summary>
		Expr Primary() {
			var startToken = cursor.Peek();
			if (cursor.IsMatch(TokenType.FALSE)) {
				return new Expr.Literal(cursor.Previous(), false);
			}
			if (cursor.IsMatch(TokenType.TRUE)) {
				return new Expr.Literal(cursor.Previous(), true);
			}
			if (cursor.IsMatch(TokenType.NULL)) {
				return new Expr.Literal(cursor.Previous(), null);
			}
			if (cursor.IsMatch(TokenType.IDENTIFIER)) {
				Expr expr = new Expr.Variable(cursor.Previous());
				while (cursor.IsMatch(TokenType.DOT)) {
					Token name = cursor.Consume(TokenType.IDENTIFIER, "Expect property name after dot.");
					expr = new Expr.Get(expr, name);
				}
				return expr;
			}
			if (cursor.Peek().IsLiteral()) {
				Token tok = cursor.Advance();
				return new Expr.Literal(tok, tok.literal);
			}

			// for grouping expressions, i.e. 2 * (5+2)
			if (cursor.IsMatch(TokenType.LEFT_PAREN)) {
				Expr expr = Expression();
				cursor.Consume(TokenType.RIGHT_PAREN, "Expect matching ')' after expression.");
				return new Expr.Grouping(startToken, expr);
			}
			throw new ParserException(cursor.Peek(), "Expect expression.");
		}

		/// <summary>After encountering an error try find next sane position to continue parsing</summary>
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
	}
}
