using System.Collections.Generic;



namespace Cy.Ast {
	public abstract class Stmt {
		public Token token;
		public abstract object Accept(IStmtVisitor visitor, object options);

		/// <summary>A group of statements.</summary>
		public class Block : Stmt {
			public List<Stmt> statements;
			public Block(List<Stmt> statements) {
				this.token = statements[0].token;
				this.statements = statements;
			}
			public override object Accept(IStmtVisitor visitor, object options) {
				return visitor.VisitBlockStmt(this, options);
			}
		}


		/// <summary>For those times we need an expression at the start of a line.</summary>
		public class Expression : Stmt {
			public Expr expression;
			public Expression(Expr expression) {
				this.token = expression.token;
				this.expression = expression;
			}
			public override object Accept(IStmtVisitor visitor, object options) {
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
			public override object Accept(IStmtVisitor visitor, object options) {
				return visitor.VisitInputVarStmt(this, options);
			}
		}

		/// <summary>Function definition.</summary>
		public class Function : Stmt {
			public StmtType returnType;
			public List<InputVar> input;
			public List<Stmt> body;
			public Function(StmtType returnType, Token token, List<InputVar> input, List<Stmt> body) {
				this.returnType = returnType;
				this.token = token;
				this.input = input;
				this.body = body;
			}
			public override object Accept(IStmtVisitor visitor, object options) {
				return visitor.VisitFunctionStmt(this, options);
			}
		}


		public class For : Stmt {
			public Token forKeyword;
			public Stmt.StmtType iteratorType;
			public Token iterator;
			public Expr condition;
			public List<Stmt> body;

			public For(Token forKeyword, Stmt.StmtType iteratorType, Token iterator, Expr condition, List<Stmt> body) {
				this.token = forKeyword;
				this.iteratorType = iteratorType;
				this.iterator = iterator;
				this.condition = condition;
				this.body = body;
			}
			public override object Accept(IStmtVisitor visitor, object options) {
				return visitor.VisitForStmt(this, options);
			}
		}

		public class If : Stmt {
			public Expr value;
			public List<Ast.Stmt> body;
			public List<Ast.Stmt> elseBody;
			public If(Token ifKeyword, Expr value, List<Ast.Stmt> body, List<Ast.Stmt> elseBody) {
				this.token = ifKeyword;
				this.value = value;
				this.body = body;
				this.elseBody = elseBody;
			}
			public override object Accept(IStmtVisitor visitor, object options) {
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
			public override object Accept(IStmtVisitor visitor, object options) {
				return visitor.VisitReturnStmt(this, options);
			}
		}


		/// <summary>Variable declaration, with possible assignment.</summary>
		public class Var : Stmt {
			public StmtType stmtType;
			public Expr initialiser;
			public Var(Token typeToken, Token token, Expr initialiser) {
				this.stmtType = new StmtType(new List<Token>() { typeToken });
				this.token = token;
				this.initialiser = initialiser;
			}
			public override object Accept(IStmtVisitor visitor, object options) {
				return visitor.VisitVarStmt(this, options);
			}
		}


		public class ClassDefinition : Stmt {
			public List<Var> members;
			public List<Function> methods;
			public List<ClassDefinition> classes;
			public ClassDefinition(Token token, List<Var> members, List<Function> methods, List<ClassDefinition> classes) {
				this.token = token;
				this.members = members;
				this.methods = methods;
				this.classes = classes;
			}
			public override object Accept(IStmtVisitor visitor, object options) {
				return visitor.VisitClassStmt(this, options);
			}
		}

		/// <summary>A basic or user defined type.</summary>
		public class StmtType : Stmt {
			public Token[] info;
			public StmtType(List<Token> tokens) {
				this.token = tokens[0];
				info = tokens.ToArray();
			}
			public StmtType() {
				this.token = new Token(TokenType.VOID);
				info = new Token[] { token };
			}
			public override object Accept(IStmtVisitor visitor, object options) {
				return visitor.VisitTypeStmt(this, options);
			}
		}

		public class While : Stmt {
			public Expr condition;
			public List<Stmt> body;

			public While(Token whileKeyword, Expr condition, List<Stmt> body) {
				this.token = whileKeyword;
				this.condition = condition;
				this.body = body;
			}
			public override object Accept(IStmtVisitor visitor, object options) {
				return visitor.VisitWhileStmt(this, options);
			}
		}
	}
}
