using Cy.Preprocesor.Interfaces;


namespace Cy.Preprocesor;

public abstract class Stmt {
	public Token token;
	public abstract object Accept(IStmtVisitor visitor, object options = null);

	/// <summary>A group of statements.</summary>
	public class Block : Stmt {
		public Stmt[] statements;
		public Block(Stmt[] statements) {
			token = statements[0].token;
			this.statements = statements;
		}
		public override object Accept(IStmtVisitor visitor, object options = null) {
			return visitor.VisitBlockStmt(this, options);
		}
	}


	/// <summary>For those times we need an expression at the start of a line.</summary>
	public class Expression : Stmt {
		public Expr expression;
		public Expression(Expr expression) {
			token = expression.token;
			this.expression = expression;
		}
		public override object Accept(IStmtVisitor visitor, object options = null) {
			return visitor.VisitExpressionStmt(this, options);
		}
	}


	/// <summary>variables defined as part of a function (input)<summary>
	public class InputVar : Stmt {
		public StmtType type;
		public InputVar(StmtType type, Token token) {
			this.type = type;
			this.token = token;
		}
		public override object Accept(IStmtVisitor visitor, object options = null) {
			return visitor.VisitInputVarStmt(this, options);
		}
	}

	/// <summary>Function definition.</summary>
	public class Function : Stmt {
		public StmtType returnType;
		public InputVar[] input;
		public Stmt[] body;
		public Function(StmtType returnType, Token token, InputVar[] input, Stmt[] body) {
			this.returnType = returnType;
			this.token = token;
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
		public Stmt[] body;

		public For(Token forKeyword, StmtType iteratorType, Token iterator, Expr condition, Stmt[] body) {
			token = forKeyword;
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
		public Stmt[] body;
		public Stmt[] elseBody;
		public If(Token ifKeyword, Expr value, Stmt[] body, Stmt[] elseBody) {
			token = ifKeyword;
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
		public Return(Token token, Expr value) {
			this.token = token;
			this.value = value;
		}
		public override object Accept(IStmtVisitor visitor, object options = null) {
			return visitor.VisitReturnStmt(this, options);
		}
	}


	/// <summary>Variable declaration, with possible assignment.</summary>
	public class VarDefinition : Stmt {
		public StmtType stmtType;
		public Expr initialiser;
		public VarDefinition(Token typeToken, Token token, Expr initialiser) {
			stmtType = new StmtType(new Token[] { typeToken });
			this.token = token;
			this.initialiser = initialiser;
		}
		public override object Accept(IStmtVisitor visitor, object options = null) {
			return visitor.VisitVarStmt(this, options);
		}
	}


	public class ClassDefinition : Stmt {
		public VarDefinition[] members;
		public Function[] methods;
		public ClassDefinition[] classes;
		public ClassDefinition(Token token, VarDefinition[] members, Function[] methods, ClassDefinition[] classes) {
			this.token = token;
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
		public StmtType(Token[] tokens) {
			token = tokens[0];
			info = tokens;
		}
		public StmtType() {
			token = new Token(TokenType.VOID);
			info = new Token[] { token };
		}
		public override object Accept(IStmtVisitor visitor, object options = null) {
			return visitor.VisitTypeStmt(this, options);
		}
	}

	public class While : Stmt {
		public Expr condition;
		public Stmt[] body;

		public While(Token whileKeyword, Expr condition, Stmt[] body) {
			token = whileKeyword;
			this.condition = condition;
			this.body = body;
		}
		public override object Accept(IStmtVisitor visitor, object options = null) {
			return visitor.VisitWhileStmt(this, options);
		}
	}
}
