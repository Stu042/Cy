using Cy.Llvm.Helpers;
using Cy.Preprocesor;
using Cy.Preprocesor.Interfaces;
using Cy.Types;

using System;
using System.Linq;


namespace Cy.Llvm.CodeGen.CompileVisitor;


public partial class CompileVisitor : IExprVisitor, IStmtVisitor {
	readonly IErrorDisplay _errorDisplay;
	public bool FoundError;


	public CompileVisitor(IErrorDisplay errorDisplay) {
		_errorDisplay = errorDisplay;
		FoundError = false;
	}


	public object VisitBlockStmt(Stmt.Block stmt, object options) {
		var helper = options as CompileOptions;
		foreach (Stmt statement in stmt.Statements) {
			statement.Accept(this, options);
		}
		return null;
	}

	public object VisitClassStmt(Stmt.ClassDefinition stmt, object options) {
		var helper = options as CompileOptions;
		helper.Code.EnterScope(stmt.Token.Lexeme);
		helper.Code.Indent();
		foreach (var memb in stmt.Members) {
			memb.Accept(this, options);
		}
		foreach (var memb in stmt.Methods) {
			memb.Accept(this, options);
		}
		helper.Code.Dedent();
		helper.Code.ExitScope();
		return null;
	}

	public object VisitExpressionStmt(Stmt.Expression stmt, object options) {
		var helper = options as CompileOptions;
		stmt.expression.Accept(this, options);
		return null;
	}

	public object VisitForStmt(Stmt.For stmt, object options) {
		var helper = options as CompileOptions;
		stmt.iteratorType.Accept(this, options);
		stmt.condition.Accept(this, options);
		foreach (var body in stmt.body) {
			body.Accept(this, options);
		}
		return null;
	}

	public object VisitIfStmt(Stmt.If stmt, object options) {
		var helper = options as CompileOptions;
		var condition = stmt.value.Accept(this, options) as ExpressionInstance;
		if (condition.FrontendType.Format != Enums.FrontendTypeFormat.Bool) {
			Error(stmt.value.Token, "Expected a bool value from expression.");
		}
		// write if condition
		foreach (var bodyStmt in stmt.body) {
			bodyStmt.Accept(this, options);
		}
		foreach (var elseStmt in stmt.elseBody) {   // label first one for condition, else label next statement
			elseStmt.Accept(this, options);
		}
		return null;
	}

	public object VisitWhileStmt(Stmt.While stmt, object options) {
		var helper = options as CompileOptions;
		var condition = stmt.condition.Accept(this, options) as ExpressionInstance;
		if (condition.FrontendType.Format != Enums.FrontendTypeFormat.Bool) {
			Error(stmt.condition.Token, "Expected a bool value.");
		}
		// write while condition with a label
		foreach (var bodyStmt in stmt.body) {
			bodyStmt.Accept(this, options);
		}
		// write "branch back to while condition"
		// label next statement
		return null;
	}

	public object VisitInputVarStmt(Stmt.InputVar invar, object options) {
		var helper = options as CompileOptions;
		var frontendType = invar.type.Accept(this, options) as FrontendType;
		var exprInstance = new ExpressionInstance {
			IndirectionLevels = 0,
			FrontendName = invar.Token.Lexeme,
			FrontendType = frontendType,
			BackendName = helper.Code.InstanceName(),
			BackendScope = helper.Code.BackendScope,
			FrontendScope = helper.Code.FrontendScope,
		};
		helper.Code.AddInstance(exprInstance);
		return exprInstance;
	}

	public object VisitReturnStmt(Stmt.Return stmt, object options) {
		var helper = options as CompileOptions;
		if (stmt.value == null) {
		}
		return null;
	}

	public object VisitTypeStmt(Stmt.StmtType stmt, object options) {
		var helper = options as CompileOptions;
		var typeName = string.Join('.', stmt.info.Select(i => i.Lexeme));
		var frontendType = helper.TypeTable.LookUp(typeName);
		frontendType.Token = stmt.Token;
		return frontendType;
	}

	public object VisitVarStmt(Stmt.VarDefinition stmt, object options) {
		var helper = options as CompileOptions;
		var frontendType = stmt.stmtType.Accept(this, options) as FrontendType;
		var instanceName = helper.Code.InstanceName();
		string backendType;
		try {
			backendType = BackendTypeHelper.FrontendTypeToBackend(frontendType);
		} catch (Exception e) {
			backendType = "ERROR";
			Error(frontendType.Token, e.Message);
		}
		var exprInstance = new ExpressionInstance {
			IndirectionLevels = 0,
			BackendName = instanceName,
			FrontendName = stmt.Token.Lexeme,
			FrontendType = frontendType,
			BackendType = backendType,
			FrontendScope = helper.Code.FrontendScope,
			BackendScope = helper.Code.BackendScope,
		};
		helper.Code.AddInstance(exprInstance);
		helper.Code.AddPreCode($"{instanceName} = alloca {backendType}, align 4");
		stmt.Initialiser.Accept(this, options);
		return null;
	}


	void Error(Token token, string message) {
		_errorDisplay.Error(token, message);
		FoundError = true;
	}
}
