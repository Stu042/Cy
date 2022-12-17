using Cy.Preprocesor;
using Cy.Preprocesor.Interfaces;
using Cy.Types;
using Cy.Util;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace Cy.Llvm.CodeGen;


public class ExpressionInstance {
	/// <summary> Frontend BaseType </summary>
	public FrontendType Type;
	/// <summary> LlvmIr name of instance, ie %0, %1 </summary>
	public string Name;
	public int IndirectionLevels;	// effectively how many * we need to refer to this
}



public class BackendTypeHelper {
	public string BaseTypeToBackend(FrontendType type) {
		switch(type.Format) {
			case Enums.FrontendTypeFormat.Void:
				return "void";
			case Enums.FrontendTypeFormat.Bool:
				return "i8";
			case Enums.FrontendTypeFormat.Int:
				return $"i{type.BitSize}";
			case Enums.FrontendTypeFormat.Float:
				return $"f{type.BitSize}";
			case Enums.FrontendTypeFormat.String:
				return $"i8*";
			case Enums.FrontendTypeFormat.Method:
			case Enums.FrontendTypeFormat.Object:
			default:
				throw new Exception("Unable to convert BaseType to Llvm type.");
		}
	}
	public string BaseTypeToBackend(ExpressionInstance instance) {
		switch (instance.Type.Format) {
			case Enums.FrontendTypeFormat.Void:
				return "void";
			case Enums.FrontendTypeFormat.Bool:
				return "i8" + new string('*', instance.IndirectionLevels);
			case Enums.FrontendTypeFormat.Int:
				return $"i{instance.Type.BitSize}" + new string('*', instance.IndirectionLevels);
			case Enums.FrontendTypeFormat.Float:
				return $"f{instance.Type.BitSize}" + new string('*', instance.IndirectionLevels);
			case Enums.FrontendTypeFormat.String:
				return $"i8*" + new string('*', instance.IndirectionLevels);
			case Enums.FrontendTypeFormat.Method:
			case Enums.FrontendTypeFormat.Object:
			default:
				throw new Exception("Unable to convert BaseType to Llvm type.");
		}
	}

	//public string Allocate(ExpressionInstance instance) {
		
	//}
}



public class CompileOptions {
	public TypeTable TypeTable;
	public CodeWriter CodeWriter;
	public BackendTypeHelper BackendTypeHelper;
}



public class CompileVisitor : IExprVisitor, IStmtVisitor {
	readonly IErrorDisplay _errorDisplay;
	public bool FoundError;


	public CompileVisitor(IErrorDisplay errorDisplay) {
		_errorDisplay = errorDisplay;
		FoundError = false;
	}


	public object VisitAssignExpr(Expr.Assign expr, object options) {
		var helper = options as CompileOptions;
		expr.value.Accept(this, options);
		return null;
	}

	public object VisitBinaryExpr(Expr.Binary expr, object options) {
		var helper = options as CompileOptions;
		expr.left.Accept(this, options);
		expr.right.Accept(this, options);
		return null;
	}

	public object VisitBlockStmt(Stmt.Block stmt, object options) {
		var helper = options as CompileOptions;
		foreach (Stmt statement in stmt.statements) {
			statement.Accept(this, options);
		}
		return null;
	}

	public object VisitCallExpr(Expr.Call expr, object options) {
		var helper = options as CompileOptions;
		expr.callee.Accept(this, options);
		foreach (var args in expr.arguments) {
			args.Accept(this, options);
		}
		return null;
	}

	public object VisitClassStmt(Stmt.ClassDefinition stmt, object options) {
		var helper = options as CompileOptions;
		helper.TypeTable.NamespaceHelper.Enter(stmt.Token.Lexeme);
		foreach (var memb in stmt.members) {
			memb.Accept(this, options);
		}
		foreach (var memb in stmt.methods) {
			memb.Accept(this, options);
		}
		helper.TypeTable.NamespaceHelper.Leave();
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
		if (condition.Type.Format != Enums.FrontendTypeFormat.Bool) {
			_errorDisplay.Error(stmt.value.Token, "Expected a bool value.");
			FoundError = true;
		}
		// write if condition
		foreach (var bodyStmt in stmt.body) {
			bodyStmt.Accept(this, options);
		}
		foreach (var elseStmt in stmt.elseBody) {	// label first one for condition, else label next statement
			elseStmt.Accept(this, options);
		}
		return null;
	}

	public object VisitWhileStmt(Stmt.While stmt, object options) {
		var helper = options as CompileOptions;
		var condition = stmt.condition.Accept(this, options) as ExpressionInstance;
		if (condition.Type.Format != Enums.FrontendTypeFormat.Bool) {
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
		var type = invar.type.Accept(this, options) as FrontendType;
		var text = $"{helper.BackendTypeHelper.BaseTypeToBackend(type)} {helper.CodeWriter.Instance()}";
		return text;
	}

	public object VisitFunctionStmt(Stmt.Function stmt, object options) {
		var helper = options as CompileOptions;
		FrontendType returnType;
		if (stmt.returnType != null) {
			returnType = stmt.returnType.Accept(this, options) as FrontendType;
		} else {
			returnType = FrontendType.Void();
		}
		var inputs = new List<string>();
		foreach (var param in stmt.input) {
			var text = param.Accept(this, options) as string;
			inputs.Add(text);
		}
		var inputStr = string.Join(", ", inputs);
		var funcName = stmt.Token.Lexeme;
		if (funcName == "main") {
			Error(stmt.Token, "Unable to use 'main' as a function name, did you mean 'Main'?");
		}
		helper.CodeWriter.AddPreCode($"define dso_local {helper.BackendTypeHelper.BaseTypeToBackend(returnType)} @{funcName}({inputStr}) #0 {{");
		foreach (Stmt body in stmt.body) {
			body.Accept(this, options);
		}
		helper.CodeWriter.AddCode("}");
		return null;
	}

	public object VisitLiteralExpr(Expr.Literal expr, object options) {
		var helper = options as CompileOptions;
		if (expr.value == null) {
			return "null";
		}
		return null;
	}

	public object VisitSetExpr(Expr.Set expr, object options) {
		var helper = options as CompileOptions;
		return null;
	}

	public object VisitReturnStmt(Stmt.Return stmt, object options) {
		var helper = options as CompileOptions;
		if (stmt.value == null) {
		}
		return null;
	}

	public object VisitTypeStmt(Stmt.StmtType stmt, object options) {
		var helper = options as CompileOptions;
		var typeName = String.Join('.', stmt.info.Select(i => i.Lexeme));
		var baseType = helper.TypeTable.LookUp(typeName);
		return baseType;
	}

	public object VisitUnaryExpr(Expr.Unary expr, object options) {
		var helper = options as CompileOptions;
		return null;
	}

	public object VisitVarStmt(Stmt.VarDefinition stmt, object options) {
		var helper = options as CompileOptions;
		var baseType = stmt.stmtType.Accept(this, options) as FrontendType;
		var expr = new ExpressionInstance {
			IndirectionLevels = 0,
			Name = stmt.Token.Lexeme,
			Type = baseType,
		};
		var instanceName = helper.CodeWriter.Instance();
		helper.CodeWriter.AddPreCode($"  {instanceName} = alloca {helper.BackendTypeHelper.BaseTypeToBackend(baseType)}, align 4");
		return null;
	}

	public object VisitVariableExpr(Expr.Variable expr, object options) {
		var helper = options as CompileOptions;
		// create new ExpressionInstance
		return null;    // return ExpressionInstance
	}

	public object VisitGetExpr(Expr.Get expr, object options) {
		var helper = options as CompileOptions;
		var exprGet = expr.obj.Accept(this, options);
		return exprGet;
	}

	public object VisitGroupingExpr(Expr.Grouping expr, object options) {
		var helper = options as CompileOptions;
		var exprGroup = expr.expression.Accept(this, options);
		return exprGroup;
	}


	void Error(Token token, string message) {
		_errorDisplay.Error(token, message);
		FoundError = true;
	}
}
