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
			public Expr initializer;
			public Var(Token token, Expr initializer) {
				this.token = token;
				this.initializer = initializer;
			}
			public override object Accept(IVisitor visitor, object options) {
				return visitor.VisitVarStmt(this, options);
			}
		}



		public class Type : Stmt {
			public enum Kind { UNKNOWN, INT, FLOAT, STR, USERDEFINED }; // USERDEFINED will typically be classes but who knows what the future will bring
			public Kind kind;
			public int size;
			public string llvm;
			public Type(Token token) {
				this.token = token;
				switch(token.type) {
					case Token.Kind.INT:
					case Token.Kind.INT32:
						kind = Kind.INT;
						size = 32;
						llvm = "i32";
						break;
					case Token.Kind.INT8:
						kind = Kind.INT;
						size = 8;
						llvm = "i8";
						break;
					case Token.Kind.INT16:
						kind = Kind.INT;
						size = 16;
						llvm = "i16";
						break;
					case Token.Kind.INT64:
						kind = Kind.INT;
						size = 64;
						llvm = "i64";
						break;
					case Token.Kind.INT128:
						kind = Kind.INT;
						size = 128;
						llvm = "i128";
						break;
					case Token.Kind.FLOAT64:
					case Token.Kind.FLOAT:
						kind = Kind.FLOAT;
						size = 64;
						llvm = "fp64";
						break;
					case Token.Kind.FLOAT16:
						kind = Kind.FLOAT;
						size = 16;
						llvm = "fp16";
						break;
					case Token.Kind.FLOAT32:
						kind = Kind.FLOAT;
						size = 32;
						llvm = "fp32";
						break;
					case Token.Kind.FLOAT128:
						kind = Kind.FLOAT;
						size = 128;
						llvm = "fp128";
						break;
					default:
						kind = Kind.USERDEFINED;
						size = 32;
						llvm = "*i8";
						break;
				}
			}
			public override object Accept(IVisitor visitor, object options) {
				return visitor.VisitTypeStmt(this, options);
			}
		}


	} // Stmt


}
