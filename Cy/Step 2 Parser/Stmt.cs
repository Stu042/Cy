using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;



namespace Cy {
	public abstract class Stmt {
		public Token token;
		public abstract object Accept(IVisitor visitor, object options);

		public interface IVisitor {
			object VisitBlockStmt(Block stmt, object options);
			object VisitExpressionStmt(Expression stmt, object options);
			object VisitFunctionStmt(Function stmt, object options);
			object VisitInputVarStmt(InputVar invar, object options);
			object VisitReturnStmt(Return stmt, object options);
			object VisitVarStmt(Var stmt, object options);
			object VisitTypeStmt(StmtType stmt, object options);
			object VisitClassStmt(StmtClass stmt, object options);
		}



		/// <summary>
		/// A group of statements.
		/// </summary>
		public class Block : Stmt {
			public List<Stmt> statements;
			public Block(List<Stmt> statements) {
				this.token = statements[0].token;
				this.statements = statements;
			}
			public override object Accept(IVisitor visitor, object options) {
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
			public override object Accept(IVisitor visitor, object options) {
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
			public override object Accept(IVisitor visitor, object options) {
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
			public override object Accept(IVisitor visitor, object options) {
				return visitor.VisitFunctionStmt(this, options);
			}
		}


		/// <summary>The return statement.</summary>
		public class Return : Stmt {
			public Expr value;
			public Return(Token token, Expr value) {
				this.token = token;
				this.value = value;
			}
			public override object Accept(IVisitor visitor, object options) {
				return visitor.VisitReturnStmt(this, options);
			}
		}


		/// <summary>Variable declaration, with possible assignment.</summary>
		public class Var : Stmt {
			public StmtType stmtType;
			public Expr initialiser;
			public Var(Token typeToken, Token token, Expr initialiser) {
				this.stmtType = new StmtType(typeToken);
				this.token = token;
				this.initialiser = initialiser;
			}
			public override object Accept(IVisitor visitor, object options) {
				return visitor.VisitVarStmt(this, options);
			}
		}

		public class StmtClass : Stmt {
			public List<Var> members;
			public List<Function> methods;
			public List<StmtClass> classes;
			public StmtClass(Token token, List<Var> members, List<Function> methods, List<StmtClass> classes) {
				this.token = token;
				this.members = members;
				this.methods = methods;
				this.classes = classes;
			}
			public override object Accept(IVisitor visitor, object options) {
				return visitor.VisitClassStmt(this, options);
			}
		}

		/// <summary>A basic or user defined type.</summary>
		public class StmtType : Stmt {
			public CyType info;
			public StmtType(Token token) {
				this.token = token;
				info = new CyType(token.type);
			}
			public StmtType() {
				this.token = new Token(Token.Kind.VOID);
				info = new CyType(token.type);
			}
			public override object Accept(IVisitor visitor, object options) {
				return visitor.VisitTypeStmt(this, options);
			}
		}


	} // Stmt


}
