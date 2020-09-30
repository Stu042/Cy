using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;



namespace Cy {



	class Compiler : Expr.IVisitor, Stmt.IVisitor {
		string outputFilename;
		StringBuilder code = new StringBuilder();
		ExprList instances;



		public Compiler() {
			instances = new ExprList();
		}



		//public void Compile(Expr expr) {
		//	expr.Accept(this, null);
		//}

		public void Compile(Stmt stmt) {
			stmt.Accept(this, null);
		}



		public override string ToString() {
			return filePreLLVMCode.Replace(fileName, outputFilename) + code.ToString() + filePostLLVMCode;
		}




		/*************************************/
		/* this is all a hack, todo fix this */
		/* it is getting the base llvm ir required */
		void RunCmd(string args) {
			System.Diagnostics.Process process = new System.Diagnostics.Process();
			System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo {
				WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden,
				FileName = "cmd.exe",
				Arguments = "/C " + args
			};
			process.StartInfo = startInfo;
			process.Start();
			process.WaitForExit();
		}

		string GetStr(string needle, string[] haystack) {
			foreach (string str in haystack) {
				if (str.IndexOf(needle) != -1)
					return str;
			}
			return "";
		}

		string fileName = @"C:\tmp\IHopeNoOneUsesThisFileName.c"; // Path.GetTempFileName();
		string filePreLLVMCode;
		string filePostLLVMCode;
		void GetLLVMPreCode() {
			StringBuilder pre = new StringBuilder();
			string llfileName = @"IHopeNoOneUsesThisFileName.ll";
			string[] llvmPreCode;
			try {
				using (StreamWriter sw = File.CreateText(fileName))
					sw.WriteLine(" ");
				RunCmd($"clang.exe {fileName} -S -emit-llvm");
				llvmPreCode = File.ReadAllLines(llfileName);
			} finally {
				RunCmd($"del /f {fileName}");
				RunCmd($"del /f {llfileName}");
			}
			string modulestr = GetStr("ModuleID", llvmPreCode);
			pre.AppendLine(modulestr);
			string srcstr = GetStr("source_filename", llvmPreCode);
			pre.AppendLine(srcstr.Replace("\\\\", "\\"));
			string datalayoutstr = GetStr("target datalayout", llvmPreCode);
			pre.AppendLine(datalayoutstr);
			string triplestr = GetStr("target triple", llvmPreCode);
			pre.AppendLine(triplestr);
			pre.AppendLine();
			filePreLLVMCode = pre.ToString();
			filePostLLVMCode = "attributes #0 = { noinline nounwind optnone uwtable \"correctly-rounded-divide-sqrt-fp-math\"=\"false\" \"disable-tail-calls\"=\"false\" \"frame-pointer\"=\"none\" \"less-precise-fpmad\"=\"false\" \"min-legal-vector-width\"=\"0\" \"no-infs-fp-math\"=\"false\" \"no-jump-tables\"=\"false\" \"no-nans-fp-math\"=\"false\" \"no-signed-zeros-fp-math\"=\"false\" \"no-trapping-math\"=\"false\" \"stack-protector-buffer-size\"=\"8\" \"target-cpu\"=\"x86-64\" \"target-features\"=\"+cx8,+fxsr,+mmx,+sse,+sse2,+x87\" \"unsafe-fp-math\"=\"false\" \"use-soft-float\"=\"false\" }\n\n!llvm.module.flags = !{!0, !1}\n!llvm.ident = !{!2}\n\n!0 = !{i32 1, !\"wchar_size\", i32 2}\n!1 = !{i32 7, !\"PIC Level\", i32 2}\n!2 = !{!\"cy version " + typeof(Program).Assembly.GetName().Version + "\"}\n\n";
		}
		/* end of hack */
		/***************/


		public void Prep(string outputFilename) {
			this.outputFilename = outputFilename;
			GetLLVMPreCode();
		}







		public object VisitAssignExpr(Expr.Assign expr, object options) {
			return options;
		}


		public object VisitBinaryExpr(Expr.Binary expr, object options) {
			ExprValue leftVal = (ExprValue)expr.left.Accept(this, options);
			ExprValue rightVal = (ExprValue)expr.right.Accept(this, options);
			switch (expr.token.type) {
				case Token.Kind.PLUS: {
					ExprValue.ExprValueAndString xpandstr = leftVal.Add(rightVal);
					code.Append(xpandstr.code);
					return xpandstr.exprValue;
				}
				case Token.Kind.MINUS: {
					ExprValue.ExprValueAndString xpandstr = leftVal.Sub(rightVal);
					code.Append(xpandstr.code);
					return xpandstr.exprValue;
				}
				case Token.Kind.STAR: {
					ExprValue.ExprValueAndString xpandstr = leftVal.Mult(rightVal);
					code.Append(xpandstr.code);
					return xpandstr.exprValue;
				}
				case Token.Kind.SLASH: {
					ExprValue.ExprValueAndString xpandstr = leftVal.Div(rightVal);
					code.Append(xpandstr.code);
					return xpandstr.exprValue;
				}
			}
			return options;
		}


		public object VisitBlockStmt(Stmt.Block stmt, object options) {
			foreach (Stmt statement in stmt.statements) {
				statement.Accept(this, options);
			}
			return options;
		}


		public object VisitExpressionStmt(Stmt.Expression stmt, object options) {
			return options;
		}

		class MethodInstance {
			public string name;
			public CyType type;
			public bool isFunction;
			public MethodInstance(string name, CyType type, bool isFunction) {
				this.name = name;
				this.type = type;
				this.isFunction = isFunction;
			}
		}
		List<MethodInstance> curEnv = new List<MethodInstance>();

		public object VisitFunctionStmt(Stmt.Function stmt, object options) {
			string funcName = stmt.token.lexeme;
			if (stmt.token.lexeme == "Main")
				funcName = funcName.ToLower();
			code.Append(Llvm.Indent($"define dso_local {stmt.returnType.info.Llvm()} @{funcName}() #0 {{\n"));
			MethodInstance func = new MethodInstance(funcName, stmt.returnType.info, true);
			curEnv.Add(func);
			foreach (Stmt body in stmt.body)
				body.Accept(this, options);
			code.Append(Llvm.Indent("}\n\n"));
			if (curEnv[curEnv.Count - 1] != func)
				Display.Error(stmt.token, $"Compiler trying to return from a different object: {curEnv[curEnv.Count - 1].name}, should be {func.name}");
			curEnv.RemoveAt(curEnv.Count - 1);
			return options;
		}


		public object VisitReturnStmt(Stmt.Return stmt, object options) {
			if (stmt.value != null) {
				ExprValue value = (ExprValue)stmt.value.Accept(this, options);
				if (curEnv[curEnv.Count - 1].isFunction) {
					value.Cast(curEnv[curEnv.Count - 1].type);
					code.Append(Llvm.Indent($"ret {value}\n"));
				} else
					Display.Error(stmt.token, $"Object is non functional, unable to return from this: {curEnv[curEnv.Count - 1].name}");
			} else {
				code.Append(Llvm.Indent("ret\n"));
			}
			return options;
		}




		public object VisitLiteralExpr(Expr.Literal expr, object options) {
			return new ExprValueLiteral(expr.token.type, expr.value);
		}



		public object VisitUnaryExpr(Expr.Unary expr, object options) {
			return options;
		}



		public object VisitVariableExpr(Expr.Variable var, object options) {
			ExprValue xpvar = instances.GetVar(var.token.lexeme);
			return xpvar;
		}


		// creating a variable
		public object VisitVarStmt(Stmt.Var stmt, object options) {
			ExprValue xpllvm = new ExprValue(stmt.stmtType.info);
			instances.AddVar(stmt.token.lexeme, xpllvm);
			code.Append(xpllvm.StackAllocate());
			if (stmt.initialiser != null) { // var equals something
				ExprValue init = (ExprValue)stmt.initialiser.Accept(this, options);
				code.Append(xpllvm.Store(init.llvmref));
			} else {
				code.Append(xpllvm.Store(0));
			}
			return options;
		}

		public object VisitTypeStmt(Stmt.StmtType stmt, object options) {
			throw new NotImplementedException();
		}

		public object VisitInputVarStmt(Stmt.InputVar invar, object options) {
			throw new NotImplementedException();
		}
	} // Compiler
}
