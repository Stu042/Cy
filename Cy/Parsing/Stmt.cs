using Cy.Parsing.Interfaces;
using Cy.TokenGenerator;

using System.Collections.Generic;



namespace Cy.Parsing;
public abstract class Stmt : Ast {
	
	public Stmt(Token token) : base(token) {}

	public abstract object Accept(IStmtVisitor visitor, object options = null);

	/// <summary>A group of statements.</summary>
	public class Block : Stmt {
		public List<Stmt> statements;
		public Block(List<Stmt> statements) : base(statements[0].Token) {
			Token = statements[0].Token;
			this.statements = statements;
		}
		public override object Accept(IStmtVisitor visitor, object options = null) {
			return visitor.VisitBlockStmt(this, options);
		}
	}


	/// <summary>For those times we need an expression at the start of a line - note, never.</summary>
	public class Expression : Stmt {
		public Expr expression;
		public Expression(Expr expression) : base(expression.Token) {
			this.expression = expression;
		}
		public override object Accept(IStmtVisitor visitor, object options = null) {
			return visitor.VisitExpressionStmt(this, options);
		}
	}


	/// <summary>variables defined as part of a function (input)<summary>
	public class InputVar : Stmt {
		public StmtType type;
		public InputVar(StmtType type, Token token) : base(token) {
			this.type = type;
		}
		public override object Accept(IStmtVisitor visitor, object options = null) {
			return visitor.VisitInputVarStmt(this, options);
		}
	}

	/// <summary>Function definition.</summary>
	public class Function : Stmt {
		public StmtType returnType;
		public List<InputVar> input;
		public List<Stmt> body;
		public Function(StmtType returnType, Token token, List<InputVar> input, List<Stmt> body) : base(token) {
			this.returnType = returnType;
			this.input = input;
			this.body = body;
		}
		public override object Accept(IStmtVisitor visitor, object options = null) {
			return visitor.VisitFunctionStmt(this, options);
		}
	}


	public class For : Stmt {
		public Token forKeyword;
		public StmtType iteratorType;
		public Token iterator;
		public Expr condition;
		public List<Stmt> body;

		public For(Token forKeyword, StmtType iteratorType, Token iterator, Expr condition, List<Stmt> body) : base(forKeyword) {
			this.iteratorType = iteratorType;
			this.iterator = iterator;
			this.condition = condition;
			this.body = body;
		}
		public override object Accept(IStmtVisitor visitor, object options = null) {
			return visitor.VisitForStmt(this, options);
		}
	}

	public class If : Stmt {
		public Expr value;
		public List<Stmt> body;
		public List<Stmt> elseBody;
		public If(Token ifKeyword, Expr value, List<Stmt> body, List<Stmt> elseBody) : base(ifKeyword) {
			this.value = value;
			this.body = body;
			this.elseBody = elseBody;
		}
		public override object Accept(IStmtVisitor visitor, object options = null) {
			return visitor.VisitIfStmt(this, options);
		}
	}

	/// <summary>The return statement.</summary>
	public class Return : Stmt {
		public Expr value;
		public Return(Token token, Expr value) : base(token) {
			this.value = value;
		}
		public override object Accept(IStmtVisitor visitor, object options = null) {
			return visitor.VisitReturnStmt(this, options);
		}
	}


	/// <summary>Variable declaration, with possible assignment.</summary>
	public class Var : Stmt {
		public StmtType stmtType;
		public Expr initialiser;
		public Var(Token typeToken, Token token, Expr initialiser) : base(token) {
			stmtType = new StmtType(new List<Token>() { typeToken });
			this.initialiser = initialiser;
		}
		public override object Accept(IStmtVisitor visitor, object options = null) {
			return visitor.VisitVarStmt(this, options);
		}
	}


	public class ClassDefinition : Stmt {
		public List<Var> members;
		public List<Function> methods;
		public List<ClassDefinition> classes;
		public ClassDefinition(Token token, List<Var> members, List<Function> methods, List<ClassDefinition> classes) : base(token) {
			this.members = members;
			this.methods = methods;
			this.classes = classes;
		}
		public override object Accept(IStmtVisitor visitor, object options = null) {
			return visitor.VisitClassStmt(this, options);
		}
	}

	/// <summary>A basic or user defined type.</summary>
	public class StmtType : Stmt {
		public Token[] info;
		public StmtType(List<Token> tokens) : base(tokens[0]) {
			info = tokens.ToArray();
		}
		public StmtType() : base(new Token(TokenType.VOID)) {
			info = new Token[] { Token };
		}
		public override object Accept(IStmtVisitor visitor, object options = null) {
			return visitor.VisitTypeStmt(this, options);
		}
	}

	public class While : Stmt {
		public Expr condition;
		public List<Stmt> body;

		public While(Token whileKeyword, Expr condition, List<Stmt> body) : base(whileKeyword) {
			this.condition = condition;
			this.body = body;
		}
		public override object Accept(IStmtVisitor visitor, object options = null) {
			return visitor.VisitWhileStmt(this, options);
		}
	}
}
