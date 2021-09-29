namespace Cy.Ast {
	public partial class Stmt {
		public interface IVisitor {
			object VisitBlockStmt(Stmt.Block stmt, object options);
			object VisitClassStmt(Stmt.ClassDefinition stmt, object options);
			object VisitExpressionStmt(Stmt.Expression stmt, object options);
			object VisitFunctionStmt(Stmt.Function stmt, object options);
			object VisitInputVarStmt(Stmt.InputVar invar, object options);
			object VisitReturnStmt(Stmt.Return stmt, object options);
			object VisitTypeStmt(Stmt.StmtType stmt, object options);
			object VisitVarStmt(Stmt.Var stmt, object options);
		}
	}
}