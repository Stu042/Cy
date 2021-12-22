using Cy.Common;

namespace Cy.Common.Interfaces {
	public interface IExprVisitor {
		object VisitGroupingExpr(Expr.Grouping expr, object options);
		object VisitAssignExpr(Expr.Assign expr, object options);
		object VisitBinaryExpr(Expr.Binary expr, object options);
		/// <summary>Call a method/function.</summary>
		object VisitCallExpr(Expr.Call expr, object options);
		object VisitGetExpr(Expr.Get obj, object options);
		object VisitLiteralExpr(Expr.Literal expr, object options);
		object VisitSetExpr(Expr.Set expr, object options);
		object VisitUnaryExpr(Expr.Unary expr, object options);
		object VisitVariableExpr(Expr.Variable var, object options);
	}
}