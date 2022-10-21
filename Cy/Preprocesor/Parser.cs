using Cy.Constants;
using Cy.Preprocesor.Interfaces;

using System.Collections.Generic;


namespace Cy.Preprocesor;

public class Parser {
	readonly IErrorDisplay _display;
	readonly ParserCursor _parserCursor;

	public Parser(ParserCursor parserCursor, IErrorDisplay display) {
		_parserCursor = parserCursor;
		_display = display;
	}

	public List<Stmt> Parse(List<Token> tokens) {
		_parserCursor.Init(tokens);
		var statements = new List<Stmt>();
		while (!_parserCursor.IsAtEnd()) {
			try {
				var s = Declaration();
				if (s != null) {
					statements.Add(s);
				}
			} catch (ParserException e) {
				_display.Error(e.token, e.Message);
				Synchronise();
			}
			while (_parserCursor.IsMatch(TokenType.NEWLINE)) { }
		}
		return statements;
	}

	// <summary>Check for class definitions, function definition - including constructors and destructors and variable declaration.</summary>
	Stmt Declaration() {
		while (_parserCursor.Consume(TokenType.NEWLINE) != null) { }
		if (_parserCursor.IsCheckAll(TokenType.IDENTIFIER, TokenType.LEFT_BRACE)) {
			return DefineClass();
		}
		if (IsContructorDeclaration()) {
			return ConstructorDeclaration();
		}
		if (_parserCursor.IsCheckAnyType()) {
			if (IsFuncDeclaration()) {
				return FunDeclaration();
			}
			if (_parserCursor.IsCheckNext(TokenType.IDENTIFIER)) {
				return VarDeclaration();
			}
		}
		return Statement();
	}

	bool IsFuncDeclaration() {
		if (_parserCursor.IsCheckAnyType() && _parserCursor.IsCheckNext(TokenType.IDENTIFIER)) {
			return CheckIsFuncArgs(2);
		}
		return false;
	}

	bool IsContructorDeclaration() {
		if (_parserCursor.IsCheckAll(TokenType.IDENTIFIER, TokenType.LEFT_PAREN)) {
			return CheckIsFuncArgs();
		}
		return false;
	}

	/// <summary>matches: IDENTIFIER(<anything in here>):\n</summary>
	bool CheckIsFuncArgs(int idxtostart = 1) {
		int lparenCount = 0;
		do {
			TokenType toktype = _parserCursor.PeekAt(idxtostart).tokenType;
			if (_parserCursor.IsCheckAt(TokenType.LEFT_PAREN, idxtostart)) {
				lparenCount++;
			} else if (toktype == TokenType.RIGHT_PAREN) {
				--lparenCount;
			} else if (toktype == TokenType.BACKSLASH) {
				do {
					idxtostart++;
				} while (!_parserCursor.IsCheckAt(TokenType.NEWLINE, idxtostart) && !_parserCursor.IsCheckAt(TokenType.EOF, idxtostart));
			} else if (toktype == TokenType.NEWLINE || toktype == TokenType.EOF) {
				return false;
			}
			idxtostart++;
		} while (lparenCount > 0);
		return true;
	}

	Stmt.ClassDefinition DefineClass() {
		Token name = _parserCursor.Advance();
		_parserCursor.Consume(TokenType.LEFT_BRACE, $"Expect a {TokenType.LEFT_BRACE} after '" + name.lexeme + "' for an object definition.");
		_parserCursor.Consume(TokenType.NEWLINE, "Expect a newline after '" + name.lexeme + "' for an object definition.");
		var members = new List<Stmt.VarDefinition>();
		var methods = new List<Stmt.Function>();
		var classes = new List<Stmt.ClassDefinition>();
		while (!_parserCursor.IsAtEnd() && (_parserCursor.Peek().tokenType != TokenType.RIGHT_BRACE || _parserCursor.Peek().tokenType == TokenType.NEWLINE)) {
			var astmt = Declaration();                      // todo problem here, just before classes.Add(c); is called...
			if (astmt is Stmt.VarDefinition v) {
				members.Add(v);
			} else if (astmt is Stmt.Function f) {
				methods.Add(f);
			} else if (astmt is Stmt.ClassDefinition c) {
				classes.Add(c);
			} else {
				_display.Error(astmt.token, "Object definitions should contain only methods, properties or class definitions.");
			}
		}
		_parserCursor.Consume(TokenType.RIGHT_BRACE, $"Expect a {TokenType.RIGHT_BRACE} at end of object definition.");
		_parserCursor.Consume(TokenType.NEWLINE, $"Expect a {TokenType.NEWLINE} after object definition.");
		return new Stmt.ClassDefinition(name, members.ToArray(), methods.ToArray(), classes.ToArray());
	}


	/// <summary>Declare a function/method.</summary>
	Stmt.Function FunDeclaration() {
		var returnType = new Stmt.StmtType(new Token[] { _parserCursor.Advance() });
		Token name = _parserCursor.Consume(TokenType.IDENTIFIER, $"Expected {TokenType.IDENTIFIER}.");
		_parserCursor.Consume(TokenType.LEFT_PAREN, $"Expect {TokenType.LEFT_PAREN} after function name.");
		var parameters = new List<Stmt.InputVar>();
		if (!_parserCursor.IsCheck(TokenType.RIGHT_PAREN)) {
			do {
				Token typeTok;
				if (_parserCursor.IsCheckAnyType()) {
					typeTok = _parserCursor.Advance();
				} else {
					throw new ParserException(_parserCursor.Peek(), "Expect parameter type.");
				}
				Token id = _parserCursor.Consume(TokenType.IDENTIFIER, "Expect parameter name.");
				parameters.Add(new Stmt.InputVar(new Stmt.StmtType(new Token[] { typeTok }), id));
			} while (_parserCursor.IsMatch(TokenType.COMMA));
		}
		_parserCursor.Consume(TokenType.RIGHT_PAREN, $"Expect {TokenType.RIGHT_PAREN} after parameters.");
		List<Stmt> body = Block();
		return new Stmt.Function(returnType, name, parameters.ToArray(), body.ToArray());
	}

	Stmt.Function ConstructorDeclaration() {
		Token name = _parserCursor.Advance();
		string structorType;
		Stmt.StmtType returnType;
		if (name.lexeme[0] == '~') {
			structorType = "Destructor";
			returnType = new Stmt.StmtType(new Token[] { new Token("void", TokenType.VOID) });
		} else {
			structorType = "Constructor";
			returnType = new Stmt.StmtType(new Token[] { new Token("void", TokenType.VOID)/*new Token(name.lexeme, name.tokenType)*/ });
		}
		_parserCursor.Consume(TokenType.LEFT_PAREN, $"Expect opening bracket after {structorType} name.");
		List<Stmt.InputVar> parameters = new();
		if (!_parserCursor.IsCheck(TokenType.RIGHT_PAREN)) {
			do {
				Token typeTok;
				if (_parserCursor.IsCheckAnyType()) {
					typeTok = _parserCursor.Advance();
				} else {
					throw new ParserException(_parserCursor.Peek(), "Expect parameter type.");
				}
				Token id = _parserCursor.Consume(TokenType.IDENTIFIER, "Expect parameter name.");
				parameters.Add(new Stmt.InputVar(new Stmt.StmtType(new Token[] { typeTok }), id));
			} while (_parserCursor.IsMatch(TokenType.COMMA));
		}
		_parserCursor.Consume(TokenType.RIGHT_PAREN, "Expect closing bracket after parameters.");
		_parserCursor.Consume(TokenType.COLON, $"Expect colon before {structorType} body.");
		_parserCursor.Consume(TokenType.NEWLINE, $"Expect 'newline' before {structorType} body.");
		var body = Block();
		return new Stmt.Function(returnType, name, parameters.ToArray(), body.ToArray());
	}

	Stmt Statement() {
		var tokenType = _parserCursor.Peek().tokenType;
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
		Token whileKeyword = _parserCursor.Advance();
		Expr condition = Expression();
		_parserCursor.Consume(TokenType.COLON, "Expect colon after for statement.");
		_parserCursor.Consume(TokenType.NEWLINE, "Expect newline at end of for statement.");
		var body = Block();
		return new Stmt.While(whileKeyword, condition, body.ToArray());
	}

	/// <summary>Statement is a for statement, i.e. "for(type iterator collection):", maybe do "int a = for b:"</summary>
	Stmt ForStatement() {
		Token forKeyword = _parserCursor.Advance();
		_parserCursor.Consume(TokenType.LEFT_PAREN, "Expect opening bracket after for statement.");
		Token iteratorTypeToken = _parserCursor.Advance();
		var iteratorType = new Stmt.StmtType(new Token[] { iteratorTypeToken });
		Token iteratorName = _parserCursor.Consume(TokenType.IDENTIFIER, "Expect variable name for iterator in for statement.");
		Expr collection = Expression();
		_parserCursor.Consume(TokenType.RIGHT_PAREN, "Expect closing bracket after for statement.");
		_parserCursor.Consume(TokenType.COLON, "Expect colon after for statement.");
		_parserCursor.Consume(TokenType.NEWLINE, "Expect newline at end of for statement.");
		var body = Block();
		return new Stmt.For(forKeyword, iteratorType, iteratorName, collection, body.ToArray());
	}

	/// <summary>Statement is an if statement, i.e. "if condition:" or "if (condition):"</summary>
	Stmt IfStatement() {
		Token ifKeyword = _parserCursor.Advance();
		Expr condition = Expression();
		_parserCursor.Consume(TokenType.LEFT_BRACE, "Expect colon after if statement.");
		_parserCursor.Consume(TokenType.NEWLINE, $"Expect newline after {TokenType.LEFT_BRACE}.");
		var body = Block();
		List<Stmt> elseBody = null;
		if (_parserCursor.IsMatch(TokenType.ELSE)) {
			elseBody = Block();
		}
		return new Stmt.If(ifKeyword, condition, body.ToArray(), elseBody.ToArray());
	}

	Stmt ReturnStatement() {
		Token keyword = _parserCursor.Advance();
		Expr value = null;
		if (!_parserCursor.IsCheckAny(TokenType.NEWLINE, TokenType.EOF)) {
			value = Expression();
		}
		_parserCursor.ConsumeAny("Expect newline after return value.", TokenType.NEWLINE, TokenType.EOF);  // new line is not reqd (could be eof)
		return new Stmt.Return(keyword, value);
	}


	// Cy will not require this...
	Stmt ExpressionStatement() {
		Expr expr = Expression();
		_parserCursor.ConsumeAny("Expect newline after expression.", TokenType.NEWLINE, TokenType.EOF);
		return new Stmt.Expression(expr);
	}

	/// <summary>Get the current Block of statements.</summary>
	List<Stmt> Block() {
		_parserCursor.ConsumeSkipNewline(TokenType.LEFT_BRACE, $"Expect {TokenType.LEFT_BRACE} before code block.");
		_parserCursor.Consume(TokenType.NEWLINE, $"Expect newline after {TokenType.LEFT_BRACE}.");
		List<Stmt> statements = new();
		while (!_parserCursor.IsAtEnd() && _parserCursor.Peek().tokenType != TokenType.RIGHT_BRACE) {
			Stmt stmt = Declaration();
			if (stmt != null) {
				statements.Add(stmt);
			}
		}
		_parserCursor.Consume(TokenType.RIGHT_BRACE, $"Missing {TokenType.RIGHT_BRACE}");
		return statements;
	}

	/// <summary>Create a variable which optionally has an assigned expression.</summary>
	Stmt VarDeclaration() {
		Token typeToken = _parserCursor.Advance();
		Token name = _parserCursor.Consume(TokenType.IDENTIFIER, "Expect variable name.");
		Expr initializer = null;
		if (_parserCursor.IsMatch(TokenType.EQUAL)) {
			initializer = Expression();
		}
		_parserCursor.Consume(TokenType.NEWLINE, "Expect newline after variable declaration.");
		return new Stmt.VarDefinition(typeToken, name, initializer);
	}

	/// <summary>Create an expression. could be a+b or 2+3 or could be rhs of an assign, a=2, etc...</summary>
	Expr Expression() {
		Expr expr = Assignment();
		return expr;
	}

	Expr Assignment() {
		Expr expr = Or();
		if (_parserCursor.IsMatch(TokenType.EQUAL)) {
			Token equals = _parserCursor.Previous();
			Expr value = Assignment();
			if (expr is Expr.Variable vexpr) {
				Token name = vexpr.token;
				return new Expr.Assign(name, value);
			} else if (expr is Expr.Get gexpr) {
				return new Expr.Set(gexpr.obj, gexpr.token, value);
			}
			_display.Error(equals, "Invalid assignment target."); // [no-throw]
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
		while (_parserCursor.IsMatch(TokenType.LOGICAL_OR)) {
			Token op = _parserCursor.Previous();
			Expr right = And();
			//				expr = new Expr.Logical(expr, op, right);
		}
		return expr;
	}

	Expr And() {
		Expr expr = Equality();
		while (_parserCursor.IsMatch(TokenType.LOGICAL_AND)) {
			Token op = _parserCursor.Previous();
			Expr right = Equality();
			//expr = new Ast.Expr.Logical(expr, op, right);
		}
		return expr;
	}

	/// <summary>Expression tests for not equal and equal.</summary>
	Expr Equality() {
		Expr expr = Compare();
		while (_parserCursor.IsMatchAny(TokenType.BANG_EQUAL, TokenType.EQUAL_EQUAL)) {
			Token op = _parserCursor.Previous();
			Expr right = Compare();
			expr = new Expr.Binary(expr, op, right);
		}
		return expr;
	}

	Expr Compare() {
		Expr expr = Addition();
		while (_parserCursor.IsMatchAny(TokenType.GREATER, TokenType.GREATER_EQUAL, TokenType.LESS, TokenType.LESS_EQUAL)) {
			Token op = _parserCursor.Previous();
			Expr right = Addition();
			expr = new Expr.Binary(expr, op, right);
		}
		return expr;
	}

	/// <summary>Expression is an addition or subtraction.</summary>
	Expr Addition() {
		Expr expr = Multiplication();
		while (_parserCursor.IsMatchAny(TokenType.MINUS, TokenType.PLUS)) {
			Token op = _parserCursor.Previous();
			Expr right = Multiplication();
			expr = new Expr.Binary(expr, op, right);
		}
		return expr;
	}

	/// <summary>Expression is a multiplication or division.</summary>
	Expr Multiplication() {
		Expr expr = Unary();
		while (_parserCursor.IsMatchAny(TokenType.SLASH, TokenType.STAR)) {
			Token op = _parserCursor.Previous();
			Expr right = Unary();
			expr = new Expr.Binary(expr, op, right);
		}
		return expr;
	}

	/// <summary>Expression is a number.</summary>
	Expr Unary() {
		if (_parserCursor.IsMatchAny(TokenType.BANG, TokenType.MINUS, TokenType.MINUSMINUS, TokenType.PLUSPLUS)) {
			Token op = _parserCursor.Previous();
			Expr right = Unary();
			return new Expr.Unary(op, right);
		}
		return Call();
	}

	/// <summary>Call a method/function.</summary>
	Expr Call() {
		Expr expr = Primary();
		while (true) {
			if (_parserCursor.IsMatch(TokenType.LEFT_PAREN)) {
				expr = FinishCall(expr);
			} else if (_parserCursor.IsMatch(TokenType.DOT)) {
				Token name = _parserCursor.Consume(TokenType.IDENTIFIER, "Expect property name after dot.");
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
		if (!_parserCursor.IsCheck(TokenType.RIGHT_PAREN)) {
			do {
				arguments.Add(Expression());
			} while (_parserCursor.IsMatch(TokenType.COMMA));
		}
		Token paren = _parserCursor.Consume(TokenType.RIGHT_PAREN, "Expect closing bracket after arguments.");
		return new Expr.Call(callee, paren, arguments.ToArray());
	}

	/// <summary>Parse an expression.</summary>
	Expr Primary() {
		var startToken = _parserCursor.Peek();
		if (_parserCursor.IsMatch(TokenType.FALSE)) {
			return new Expr.Literal(_parserCursor.Previous(), false);
		}
		if (_parserCursor.IsMatch(TokenType.TRUE)) {
			return new Expr.Literal(_parserCursor.Previous(), true);
		}
		if (_parserCursor.IsMatch(TokenType.NULL)) {
			return new Expr.Literal(_parserCursor.Previous(), null);
		}
		if (_parserCursor.IsMatch(TokenType.IDENTIFIER)) {
			Expr expr = new Expr.Variable(_parserCursor.Previous());
			while (_parserCursor.IsMatch(TokenType.DOT)) {
				Token name = _parserCursor.Consume(TokenType.IDENTIFIER, "Expect property name after dot.");
				expr = new Expr.Get(expr, name);
			}
			return expr;
		}
		if (_parserCursor.Peek().IsLiteral()) {
			Token tok = _parserCursor.Advance();
			return new Expr.Literal(tok, tok.literal);
		}

		// for grouping expressions, i.e. 2 * (5+2)
		if (_parserCursor.IsMatch(TokenType.LEFT_PAREN)) {
			Expr expr = Expression();
			_parserCursor.Consume(TokenType.RIGHT_PAREN, "Expect matching ')' after expression.");
			return new Expr.Grouping(startToken, expr);
		}
		throw new ParserException(_parserCursor.Peek(), "Expect expression.");
	}

	/// <summary>After encountering an error try find next sane position to continue parsing</summary>
	void Synchronise() {
		_parserCursor.Advance();
		while (!_parserCursor.IsAtEnd()) {
			if (_parserCursor.Previous().tokenType == TokenType.NEWLINE) {
				return;
			}
			switch (_parserCursor.Peek().tokenType) {
				case TokenType.NEWLINE:
					return;
				default:
					break;
			}
			_parserCursor.Advance();
		}
	}
}
