using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;



namespace Cy {
	class Compiler : Expr.IVisitor, Stmt.IVisitor {
		string outputFilename;
		StringBuilder preCode = new StringBuilder();
		StringBuilder code = new StringBuilder();
		StringBuilder postCode = new StringBuilder();
		uint indent = 0;



		//public void Compile(Expr expr) {
		//	expr.Accept(this, null);
		//}

		public void Compile(Stmt stmt) {
			stmt.Accept(this, null);
		}



		public override string ToString() {
			return filePreLLVMCode + code.ToString() + filePostLLVMCode;
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
			filePostLLVMCode = "!llvm.module.flags = !{!0, !1}\n!llvm.ident = !{!2}\n\n!0 = !{i32 1, !\"wchar_size\", i32 2}\n!1 = !{i32 7, !\"PIC Level\", i32 2}\n!2 = !{!\"cy version 0.0.1\"}\n\nattributes #0 = { noinline nounwind optnone uwtable \"correctly-rounded-divide-sqrt-fp-math\"=\"false\" \"disable-tail-calls\"=\"false\" \"frame-pointer\"=\"none\" \"less-precise-fpmad\"=\"false\" \"min-legal-vector-width\"=\"0\" \"no-infs-fp-math\"=\"false\" \"no-jump-tables\"=\"false\" \"no-nans-fp-math\"=\"false\" \"no-signed-zeros-fp-math\"=\"false\" \"no-trapping-math\"=\"false\" \"stack-protector-buffer-size\"=\"8\" \"target-cpu\"=\"x86-64\" \"target-features\"=\"+cx8,+fxsr,+mmx,+sse,+sse2,+x87\" \"unsafe-fp-math\"=\"false\" \"use-soft-float\"=\"false\" }";
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
			string funcName;
			if (stmt.token.lexeme == "Main")
				funcName = stmt.token.lexeme.ToLower();
			else
				funcName = stmt.token.lexeme;
			// TODO set i32 to whatever function returns (type), check return matches type here - last body.accept() could return type?
			AppendLine($"define dso_local i32 @{funcName}() #0 {{");
			foreach (Stmt body in stmt.body)
				body.Accept(this, options);
			AppendLine("}\n\n");
			return options;
		}


		public object VisitLiteralExpr(Expr.Literal expr, object options) {
			if (expr.token.type == Token.Kind.INT_LITERAL)
				return "i32 " + expr.value.ToString();
			else if (expr.token.type == Token.Kind.FLOAT_LITERAL)
				return "fp32 " + expr.value.ToString();
			return expr.value.ToString();
		}


		public object VisitReturnStmt(Stmt.Return stmt, object options) {
			if (stmt.value != null) {
				string value = (string)stmt.value.Accept(this, options);
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
