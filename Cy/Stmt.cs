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
			object VisitReturnStmt(Return stmt, object options);
			object VisitVarStmt(Var stmt, object options);
			object VisitTypeStmt(Type stmt, object options);
			
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


		/// <summary>
		/// For those times we need an expression at the start of a line.
		/// </summary>
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


		/// <summary>
		/// Function definition.
		/// </summary>
		public class Function : Stmt {
			public Type returnType;
			public List<Token> input;
			public List<Stmt> body;
			public Function(Type returnType, Token token, List<Token> input, List<Stmt> body) {
				this.returnType = returnType;
				this.token = token;
				this.input = input;
				this.body = body;
		    }
			public override object Accept(IVisitor visitor, object options) {
				return visitor.VisitFunctionStmt(this, options);
			}
		}


		/// <summary>
		/// The return statement.
		/// </summary>
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



		public class Var : Stmt {
			public Expr initialiser;
			public Var(Token token, Expr initialiser) {
				this.token = token;
				this.initialiser = initialiser;
			}
			public override object Accept(IVisitor visitor, object options) {
				return visitor.VisitVarStmt(this, options);
			}
		}



		public class Type : Stmt {
			public CyType info;
			public Type(Token token) {
				this.token = token;
				switch(token.type) {
					case Token.Kind.INT:
					case Token.Kind.INT32:
						info = new CyType(CyType.Kind.INT, 32);
						break;
					case Token.Kind.INT8:
						info = new CyType(CyType.Kind.INT, 8);
						break;
					case Token.Kind.INT16:
						info = new CyType(CyType.Kind.INT, 16);
						break;
					case Token.Kind.INT64:
						info = new CyType(CyType.Kind.INT, 64);
						break;
					case Token.Kind.INT128:
						info = new CyType(CyType.Kind.INT, 128);
						break;
					case Token.Kind.FLOAT64:
					case Token.Kind.FLOAT:
						info = new CyType(CyType.Kind.FLOAT, 64);
						break;
					case Token.Kind.FLOAT16:
						info = new CyType(CyType.Kind.FLOAT, 16);
						break;
					case Token.Kind.FLOAT32:
						info = new CyType(CyType.Kind.FLOAT, 32);
						break;
					case Token.Kind.FLOAT128:
						info = new CyType(CyType.Kind.FLOAT, 128);
						break;
					default:
						info = new CyType(CyType.Kind.USERDEFINED, 32);
						break;
				}
			}
			public override object Accept(IVisitor visitor, object options) {
				return visitor.VisitTypeStmt(this, options);
			}
		}


	} // Stmt


}
