﻿using Cy.Preprocesor.Interfaces;


namespace Cy.Preprocesor;

public abstract class Expr {
	public Token Token;

	public abstract object Accept(IExprVisitor visitor, object options = null);

	public class Grouping : Expr {
		public Expr expression;
		public Grouping(Token token, Expr expr) {
			this.Token = token;
			expression = expr;
		}
		public override object Accept(IExprVisitor visitor, object options = null) {
			return visitor.VisitGroupingExpr(this, options);
		}
	}

	/// <summary>A hardcoded value, i.e. a number.</summary>
	public class Literal : Expr {
		public object value;
		public Literal(Token token, object value) {
			this.Token = token;
			this.value = value;
		}
		public override object Accept(IExprVisitor visitor, object options = null) {
			return visitor.VisitLiteralExpr(this, options);
		}
	}


	public class Set : Expr {
		public Expr obj;
		public Expr value;
		public Set(Expr obj, Token token, Expr value) {
			this.obj = obj;
			this.Token = token;
			this.value = value;
		}
		public override object Accept(IExprVisitor visitor, object options = null) {
			return visitor.VisitSetExpr(this, options);
		}

	}


	/// <summary>.</summary>
	public class Get : Expr {
		public Expr obj;
		public Get(Expr obj, Token token) {
			this.obj = obj;
			this.Token = token;
		}
		public override object Accept(IExprVisitor visitor, object options = null) {
			return visitor.VisitGetExpr(this, options);
		}
	}


	/// <summary>Multiply, add, subtract, etc...</summary>
	public class Binary : Expr {
		public Expr left;
		public Expr right;
		public Binary(Expr left, Token token, Expr right) {
			this.left = left;
			this.Token = token;
			this.right = right;
		}
		public override object Accept(IExprVisitor visitor, object options = null) {
			return visitor.VisitBinaryExpr(this, options);
		}
	}

	/// <summary>Call a method/function.</summary>
	public class Call : Expr {          // token is end of function call - rparen
		public Expr callee;             // function we are calling (might be a constructor with no function body as yet)
		public Expr[] arguments;		// input args
		public Call(Expr callee, Token token, Expr[] arguments) {
			this.callee = callee;
			this.Token = token;
			this.arguments = arguments;
		}
		public override object Accept(IExprVisitor visitor, object options = null) {
			return visitor.VisitCallExpr(this, options);
		}
	}


	/// <summary>Get a value from Expr to assign to a variable.</summary>
	public class Assign : Expr {
		public Expr value;
		public Assign(Token token, Expr value) {
			this.Token = token;
			this.value = value;
		}
		public override object Accept(IExprVisitor visitor, object options = null) {
			return visitor.VisitAssignExpr(this, options);
		}
	}


	/// <summary>Simply a variable.</summary>
	public class Variable : Expr {
		public Variable(Token token) {
			this.Token = token;
		}
		public override object Accept(IExprVisitor visitor, object options = null) {
			return visitor.VisitVariableExpr(this, options);
		}
	}


	// todo add pre -- and ++
	/// <summary>Minus and not (!)</summary>
	public class Unary : Expr {
		public Expr right;
		public Unary(Token token, Expr right) {
			this.Token = token;
			this.right = right;
		}
		public override object Accept(IExprVisitor visitor, object options = null) {
			return visitor.VisitUnaryExpr(this, options);
		}
	}
} // Expr
