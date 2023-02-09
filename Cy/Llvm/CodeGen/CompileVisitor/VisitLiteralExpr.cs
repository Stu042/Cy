using Cy.Preprocesor;
using Cy.Preprocesor.Interfaces;

using System;

namespace Cy.Llvm.CodeGen.CompileVisitor;


public partial class CompileVisitor : IExprVisitor, IStmtVisitor {
	public object VisitLiteralExpr(Expr.Literal expr, object options) {
		var helper = options as CompileOptions;
		if (expr.value == null) {
			return "null";
		}
		var instance = helper.Code.LastInstance();
		switch (expr.Token.TokenType) {
			case Constants.TokenType.FLOAT_LITERAL:
				switch (instance.BackendType) {
					case "half":
						helper.Code.AddCode($"store {instance.BackendType} {ToHexString((Half)expr.value)}, ptr {instance.BackendName}, align 4");
						break;
					case "float":
						helper.Code.AddCode($"store {instance.BackendType} {ToHexString((float)expr.value)}, ptr {instance.BackendName}, align 4");
						break;
					case "double":
						helper.Code.AddCode($"store {instance.BackendType} {ToHexString((double)expr.value)}, ptr {instance.BackendName}, align 4");
						break;

				}
				break;
			case Constants.TokenType.INT_LITERAL:
				helper.Code.AddCode($"store {instance.BackendType} {expr.value}, int* {instance.BackendName}, align 4");
				break;
			case Constants.TokenType.STR_LITERAL:
				helper.Code.AddCode($"store {instance.BackendType} {expr.value}, int* {instance.BackendName}, align 4");	// todo
				break;
		}
		return null;
	}

	string ToHexString(Half h) {
		var bytes = BitConverter.GetBytes(h);
		var i = BitConverter.ToInt32(bytes, 0);
		return "0x" + i.ToString("X4");
	}
	string ToHexString(float f) {
		var bytes = BitConverter.GetBytes(f);
		var i = BitConverter.ToInt32(bytes, 0);
		return "0x" + i.ToString("X8");
	}
	string ToHexString(double d) {
		var bytes = BitConverter.GetBytes(d);
		var i = BitConverter.ToInt64(bytes, 0);
		return "0x" + i.ToString("X16");
	}
}
