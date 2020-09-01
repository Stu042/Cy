using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;



namespace Cy {
	class Compiler : Expr.IVisitor, Stmt.IVisitor {
		string outputFilename;
		StringBuilder code = new StringBuilder();
		uint indent = 0;



		//public void Compile(Expr expr) {
		//	expr.Accept(this, null);
		//}

		public void Compile(Stmt stmt) {
			stmt.Accept(this, null);
		}



		public override string ToString() {
			return filePreLLVMCode.Replace(fileName, outputFilename) + code.ToString() + filePostLLVMCode;
		}


		void AppendLine(string line) {
			if (line.Contains("}"))
				--indent;
			for (int i = 0; i < indent; i++)
				code.Append("  ");
			code.AppendLine(line);
			if (line.Contains("{"))
				indent++;
		}


		/*************************************/
		/* this is all a hack, todo fix this */
		void RunCmd(string args) {
			System.Diagnostics.Process process = new System.Diagnostics.Process();
			System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
			startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
			startInfo.FileName = "cmd.exe";
			startInfo.Arguments = "/C " + args;
			process.StartInfo = startInfo;
			process.Start();
		}

		string GetStr(string needle, string[] haystack) {
			foreach (string str in haystack) {
				if (str.IndexOf(needle) != -1)
					return str;
			}
			return "";
		}

		string fileName = @"C:\tmp\IHopeNoOneUsesThisFileName.c";
		string filePreLLVMCode;
		string filePostLLVMCode;
		void GetLLVMPreCode() {
			StringBuilder pre = new StringBuilder();
			string llfileName = @"IHopeNoOneUsesThisFileName.ll";
			using (StreamWriter sw = File.CreateText(fileName))
				sw.WriteLine(" ");
			RunCmd($"clang.exe {fileName} -S -emit-llvm");
			string[] llvmPreCode = File.ReadAllLines(llfileName);
			RunCmd($"del /f {fileName}");

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



		class ExprValue {
			public string llvmtype; // type for llvm, i.e. i32, fp64, etc
			public string llvmref;  // this will be %1 etc, or actual number if a literal
			public bool isLiteral;  // is this an actual value
			public object value;    // if literal this is the value
			public Token.Kind type;	// token.kind
			public ExprValue(string llvmtype, string llvmref, Token.Kind type, bool isLiteral, object value=null) {
				this.llvmtype = llvmtype;
				this.llvmref = llvmref;
				this.type = type;
				this.isLiteral = isLiteral;
				this.value = value;
			}
			public override string ToString() {
				return $"{llvmtype} {llvmref}";
			}
		}






		public object VisitAssignExpr(Expr.Assign expr, object options) {
			return options;
		}

		public object VisitBinaryExpr(Expr.Binary expr, object options) {
			ExprValue leftVal = (ExprValue)expr.left.Accept(this, options);
			ExprValue rightVal = (ExprValue)expr.right.Accept(this, options);
			switch (expr.token.type) {
				case Token.Kind.PLUS:
					if (leftVal.isLiteral && rightVal.isLiteral) {
						if (leftVal.type == Token.Kind.INT_LITERAL) {
							if (rightVal.type == Token.Kind.INT_LITERAL) {  // int + int
								long preval = (long)leftVal.value + (long)rightVal.value;
								string val = preval.ToString();
								return new ExprValue("i32", val, Token.Kind.INT_LITERAL, true, preval);
							} else {                                                // int + double
								double preval = (long)leftVal.value + (double)rightVal.value;
								string val = BitConverter.DoubleToInt64Bits(preval).ToString("X");
								return new ExprValue("double", val, Token.Kind.FLOAT_LITERAL, true, preval);
							}
						} else {	// left is double literal
							if (rightVal.type == Token.Kind.INT_LITERAL) {  // double + int
								double preval = (double)leftVal.value + (long)rightVal.value;
								string val = BitConverter.DoubleToInt64Bits(preval).ToString("X");
								return new ExprValue("double", val, Token.Kind.FLOAT_LITERAL, true, preval);
							} else {                                                // double + double
								double preval = (double)leftVal.value + (double)rightVal.value;
								string val = BitConverter.DoubleToInt64Bits(preval).ToString("X");
								return new ExprValue("double", val, Token.Kind.FLOAT_LITERAL, true, preval);
							}
						}
					}
					break;
				case Token.Kind.STAR:
					if (leftVal.isLiteral && rightVal.isLiteral) {
						int preval = int.Parse(leftVal.llvmref) * int.Parse(rightVal.llvmref);
						string val = preval.ToString();
						return new ExprValue("i32", val, Token.Kind.INT_LITERAL, true, preval);
					}
					break;
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

		public object VisitFunctionStmt(Stmt.Function stmt, object options) {
			string funcName = stmt.token.lexeme;
			if (stmt.token.lexeme == "Main")
				funcName = funcName.ToLower();
			// TODO set i32 to whatever function returns (type), check return matches type here - last body.accept() could return type?
			AppendLine($"define dso_local {stmt.returnType.llvm} @{funcName}() #0 {{");
			foreach (Stmt body in stmt.body)
				body.Accept(this, options);
			AppendLine("}\n\n");
			return options;
		}


		public object VisitLiteralExpr(Expr.Literal expr, object options) {
			if (expr.token.type == Token.Kind.INT_LITERAL)
				return new ExprValue("i32", expr.value.ToString(), Token.Kind.INT_LITERAL, true, expr.value);
			else if (expr.token.type == Token.Kind.FLOAT_LITERAL)
				return new ExprValue("fp32", expr.value.ToString(), Token.Kind.FLOAT_LITERAL, true, expr.value);
			return new ExprValue("i32", expr.value.ToString(), expr.token.type, true, expr.value);
		}


		public object VisitReturnStmt(Stmt.Return stmt, object options) {
			if (stmt.value != null) {
				ExprValue value = (ExprValue)stmt.value.Accept(this, options);
				AppendLine($"ret {value}");
			} else {
				AppendLine("ret");
			}
			return options;
		}

		public object VisitUnaryExpr(Expr.Unary expr, object options) {
			return options;
		}

		public object VisitVariableExpr(Expr.Variable var, object options) {
			return options;
		}

		public object VisitVarStmt(Stmt.Var stmt, object options) {
			return options;
		}

		public object VisitTypeStmt(Stmt.Type stmt, object options) {
			throw new NotImplementedException();
		}




	} // Compiler
}
