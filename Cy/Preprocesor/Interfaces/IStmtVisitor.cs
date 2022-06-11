namespace Cy.Preprocesor.Interfaces;

public interface IStmtVisitor {
	/// <summary>A group of statements, i.e. multiple lines of code.</summary>
	object VisitBlockStmt(Stmt.Block stmt, object options);
	/// <summary>A class (object), group of properties and methods.</summary>
	object VisitClassStmt(Stmt.ClassDefinition stmt, object options);
	object VisitExpressionStmt(Stmt.Expression stmt, object options);
	/// <summary>A function.</summary>
	object VisitFunctionStmt(Stmt.Function stmt, object options);
	/// <summary>Input to a function.</summary>
	object VisitInputVarStmt(Stmt.InputVar invar, object options);
	/// <summary>An if statement.</summary>
	object VisitIfStmt(Stmt.If stmt, object options);
	/// <summary>A return statement.</summary>
	object VisitReturnStmt(Stmt.Return stmt, object options);
	/// <summary>A type for a variable or function statement.</summary>
	object VisitTypeStmt(Stmt.StmtType stmt, object options);
	/// <summary>A variable statement.</summary>
	object VisitVarStmt(Stmt.VarDefinition stmt, object options);
	/// <summary>A for loop statement.</summary>
	object VisitForStmt(Stmt.For stmt, object options);
	/// <summary>A while loop statement.</summary>
	object VisitWhileStmt(Stmt.While stmt, object options);
}
