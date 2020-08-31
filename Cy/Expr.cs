

namespace Cy {
	public abstract class Expr {
		public Token token;
		public abstract object Accept(Expr.IVisitor visitor, object options);

		public interface IVisitor {
			object VisitBinaryExpr(Binary expr, object options);
			object VisitAssignExpr(Assign expr, object options);
			object VisitVariableExpr(Variable var, object options);
			object VisitLiteralExpr(Literal expr, object options);
			object VisitUnaryExpr(Unary expr, object options);
		}


		/// <summary>
		/// A hardcoded value, i.e. a number.
		/// </summary>
		public class Literal : Expr {
			public object value;
			public Literal(Token token, object value) {
				this.token = token;
				this.value = value;
			}
			public override object Accept(IVisitor visitor, object options) {
				return visitor.VisitLiteralExpr(this, options);
			}
		}


		/// <summary>
		/// Multiply, add, subtract, etc...
		/// </summary>
		public class Binary : Expr {
			public Expr left;
			public Expr right;
			public Binary(Expr left, Token token, Expr right) {
				this.left = left;
				this.token = token;
				this.right = right;
			}
			public override object Accept(IVisitor visitor, object options) {
				return visitor.VisitBinaryExpr(this, options);
			}
		}


		/// <summary>
		/// Get a value from Expr to assign to a variable.
		/// </summary>
		public class Assign : Expr {
			public Expr value;
			public Assign(Token token, Expr value) {
				this.token = token;
				this.value = value;
			}
			public override object Accept(IVisitor visitor, object options) {
				return visitor.VisitAssignExpr(this, options);
			}
		}


		/// <summary>
		/// Simply a variable.
		/// </summary>
		public class Variable : Expr {
			public Variable(Token token) {
				this.token = token;
			}
			public override object Accept(IVisitor visitor, object options) {
				return visitor.VisitVariableExpr(this, options);
			}
		}



		/// <summary>
		/// Minus and not (!)
		/// </summary>
		public class Unary : Expr {
			public Expr right;
			public Unary(Token token, Expr right) {
				this.token = token;
				this.right = right;
			}
			public override object Accept(IVisitor visitor, object options) {
				return visitor.VisitUnaryExpr(this, options);
			}
		}



	} // Expr



}

