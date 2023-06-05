using Cy.Enums;
using Cy.Llvm.Helpers;
using Cy.Preprocesor;
using Cy.Preprocesor.Interfaces;
using Cy.Types;

using System;
using System.Collections.Generic;
using System.Linq;


namespace Cy.Llvm.CodeGen.CompileVisitor;


public partial class CompileVisitor : IExprVisitor, IStmtVisitor {
	public object VisitFunctionStmt(Stmt.Function stmt, object options) {
		var helper = options as CompileOptions;
		FrontendType returnType;
		if (stmt.returnType != null) {
			returnType = stmt.returnType.Accept(this, options) as FrontendType;
		} else {
			returnType = new FrontendType("void", AccessModifier.Public, FrontendTypeFormat.Void, 0, 0, stmt.Token);
		}
		var funcName = FunctionName(stmt.Token, stmt.input, helper);
		helper.Code.EnterScope(funcName);
		var inputs = InputParams(stmt.input, helper);
		var inputStr = BuildInputParamString(inputs, helper);
		try {
			helper.Code.AddPreCode($"define dso_local {BackendTypeHelper.FrontendTypeToBackend(returnType)} @{funcName}({inputStr}) #0 {{");
		} catch (Exception ex) {
			Error(returnType.Token, ex.Message);
		}
		helper.Code.InstanceNameInc();
		helper.Code.Indent();
		foreach (var body in stmt.body) {
			body.Accept(this, options);
		}
		helper.Code.Dedent();
		helper.Code.AddCode("}");
		helper.Code.ExitScope();
		return null;
	}


	string BuildInputParamString(List<ExpressionInstance> inputs, CompileOptions helper) {
		var backendInputStrs = new List<string>();
		foreach (var instance in inputs) {
			var text = $"{BackendTypeHelper.FrontendTypeToBackend(instance)} {instance.BackendName}";
			backendInputStrs.Add(text);
		}
		var inputStr = String.Join(", ", backendInputStrs);
		return inputStr;
	}
	List<ExpressionInstance> InputParams(Stmt.InputVar[] stmtInput, CompileOptions helper) {
		var inputs = new List<ExpressionInstance>();
		foreach (var param in stmtInput) {
			var input = param.Accept(this, helper) as ExpressionInstance;
			inputs.Add(input);
		}
		return inputs;
	}
	string FunctionName(Token stmtToken, IEnumerable<Stmt.InputVar> inputVars, CompileOptions helper) {
		var nameSpace = String.IsNullOrEmpty(helper.TypeTable.NamespaceHelper.Current) ? String.Empty : helper.TypeTable.NamespaceHelper.Current + "_";
		if (String.IsNullOrEmpty(nameSpace) && stmtToken.Lexeme == "Main") {
			return "Main";
		}
		var inputTypes = inputVars.Select(si => si.type.Token.Lexeme);
		var inputStr = String.Join('_', inputTypes);
		var funcNameNoInput = nameSpace + stmtToken.Lexeme;
		if (funcNameNoInput == "main") {
			Error(stmtToken, "Unable to use 'main' as a function name, did you mean 'Main'?");
		}
		var funcName = funcNameNoInput + (String.IsNullOrEmpty(inputStr) ? String.Empty : "_" + inputStr);
		return funcName;
	}
}
