﻿using Cy.Preprocesor;
using Cy.Preprocesor.Interfaces;

using System.Linq;

namespace Cy.Types;


public class TypeTableCreateVisitorOptions {
	public NamespaceHelper NamespaceHelper;
	public TypeTableBuilderHelper TypeTableBuilderHelper;
	public TypeTableCreateVisitorOptions(NamespaceHelper namespaceHelper, TypeTableBuilderHelper typeTableBuilderHelper) {
		NamespaceHelper = namespaceHelper;
		TypeTableBuilderHelper = typeTableBuilderHelper;
	}
}



public class TypeTableCreateVisitor : IAstVisitor {
	public object VisitAssignExpr(Expr.Assign expr, object options) {
		expr.value.Accept(this, options);
		return null;
	}

	public object VisitBinaryExpr(Expr.Binary expr, object options) {
		expr.left.Accept(this, options);
		expr.right.Accept(this, options);
		return null;
	}

	public object VisitBlockStmt(Stmt.Block stmt, object options) {
		foreach (var statement in stmt.Statements) {
			statement.Accept(this, options);
		}
		return null;
	}

	public object VisitCallExpr(Expr.Call expr, object options) {
		expr.callee.Accept(this, options);
		foreach (var args in expr.arguments) {
			args.Accept(this, options);
		}
		return null;
	}

	public object VisitClassStmt(Stmt.ClassDefinition stmt, object options) {
		var data = options as TypeTableCreateVisitorOptions;
		data.NamespaceHelper.Enter(stmt.Token.Lexeme);
		var obj = new ObjectType(data.NamespaceHelper.Current, Enums.AccessModifier.Public, Enums.FrontendTypeFormat.Object, 0, 0, stmt.Token);
		data.TypeTableBuilderHelper.Add(obj);
		foreach (var memb in stmt.Classes) {
			memb.Accept(this, options);
		}
		foreach (var memb in stmt.Members) {
			var member = memb.Accept(this, options) as ObjectChildType;		// VisitVarStmt returns member (Type or ObjectType) definitions
			obj.AddChild(member);
		}
		foreach (var memb in stmt.Methods) {										// VisitFunctionStmt returns method definitions
			var method = memb.Accept(this, options) as MethodType;
			obj.AddChild(method);
		}
		data.NamespaceHelper.Leave();
		return obj;
	}

	public object VisitExpressionStmt(Stmt.Expression stmt, object options) {
		stmt.expression.Accept(this, options);
		return null;
	}

	public object VisitForStmt(Stmt.For stmt, object options) {
		stmt.iteratorType.Accept(this, options);
		stmt.condition.Accept(this, options);
		foreach (var body in stmt.body) {
			body.Accept(this, options);
		}
		return null;
	}

	// returns method definitions
	public object VisitFunctionStmt(Stmt.Function stmt, object options) {
		string returnTypeName;
		Enums.FrontendTypeFormat returnTypeFormat;
		if (stmt.returnType != null) {
			var returnStmt = (FrontendType)stmt.returnType.Accept(this, options);
			returnTypeName = returnStmt.Name;
			returnTypeFormat = returnStmt.Format;
		} else {
			returnTypeName = Constants.BasicTypeNames.Void;
			returnTypeFormat = Enums.FrontendTypeFormat.Void;
		}
		var functionType = new MethodType(stmt.Token.Lexeme, returnTypeName, Enums.AccessModifier.Public, returnTypeFormat, 0, 0, stmt.Token);
		foreach (var param in stmt.input) {
			param.Accept(this, options);							// todo, add params to MethodType?
		}
		foreach (var body in stmt.body) {
			body.Accept(this, options);
		}
		return functionType;
	}

	public object VisitGetExpr(Expr.Get obj, object options) {
		return null;
	}

	public object VisitGroupingExpr(Expr.Grouping expr, object options) {
		expr.expression.Accept(this, options);
		return null;
	}

	public object VisitIfStmt(Stmt.If stmt, object options) {
		stmt.value.Accept(this, options);
		foreach (var body in stmt.body) {
			body.Accept(this, options);
		}
		foreach (var elseBody in stmt.elseBody) {
			elseBody.Accept(this, options);
		}
		return null;
	}

	public object VisitInputVarStmt(Stmt.InputVar invar, object options) {
		invar.type.Accept(this, options);
		return null;
	}

	public object VisitLiteralExpr(Expr.Literal expr, object options) {
		return null;
	}

	public object VisitReturnStmt(Stmt.Return stmt, object options) {
		if (stmt.value != null) {
			stmt.value.Accept(this, options);
		}
		return null;
	}

	public object VisitSetExpr(Expr.Set expr, object options) {
		expr.obj.Accept(this, options);
		expr.value.Accept(this, options);
		return null;
	}

	public object VisitTypeStmt(Stmt.StmtType stmt, object options) {
		var data = options as TypeTableCreateVisitorOptions;
		var typeName = data.NamespaceHelper.BuildName(stmt.info.Select(info => info.Lexeme));
		var existingType = data.TypeTableBuilderHelper.LookUp(typeName);
		if (existingType == null) {
			existingType = new FrontendType(typeName, Enums.AccessModifier.Public, Enums.FrontendTypeFormat.Int, 0,0, stmt.Token);
		}
		return existingType;
	}

	public object VisitUnaryExpr(Expr.Unary expr, object options) {
		return null;
	}

	public object VisitVariableExpr(Expr.Variable var, object options) {
		return null;
	}

	public object VisitVarStmt(Stmt.VarDefinition stmt, object options) {
		var type = stmt.stmtType.Accept(this, options) as FrontendType;
		var member = new ObjectChildType(stmt.Token.Lexeme, type);
		return member;
	}

	public object VisitWhileStmt(Stmt.While stmt, object options) {
		stmt.condition.Accept(this, options);
		foreach (var body in stmt.body) {
			body.Accept(this, options);
		}
		return null;
	}
}
