namespace Cy.Preprocesor.Interfaces;

public interface IExprVisitor {
	/// <summary>Deal with brackets, i.e. (x+y).</summary>
	object VisitGroupingExpr(Expr.Grouping expr, object options);

	/// <summary>Get a value from Expr to assign to a variable. i.e. a=b</summary>
	object VisitAssignExpr(Expr.Assign expr, object options);

	/// <summary>Multiply, add, subtract, etc...</summary>
	object VisitBinaryExpr(Expr.Binary expr, object options);
	
	/// <summary>Call a method/function.</summary>
	object VisitCallExpr(Expr.Call expr, object options);
	
	/// <summary>a.b</summary>
	object VisitGetExpr(Expr.Get obj, object options);

	/// <summary>"A hardcoded value, i.e. hello world" or 42</summary>
	object VisitLiteralExpr(Expr.Literal expr, object options);
	
	object VisitSetExpr(Expr.Set expr, object options);

	/// <summary>Minus and not (!)</summary>
	object VisitUnaryExpr(Expr.Unary expr, object options);
	
	/// <summary>Simply a variable.</summary>
	object VisitVariableExpr(Expr.Variable var, object options);
}
