using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;



namespace Cy {


	public class ExprValue {
		//public string llvmtype; // type for llvm, i.e. i32, fp64, etc
		public string llvmref;  // this will be %1 etc, or actual number if a literal
		public bool isLiteral;  // is this an actual value
		public object value;    // if literal this is the value
		public CyType info;
		/*public ExprValue(string llvmtype, string llvmref, bool isLiteral, object value = null) {
			this.llvmtype = llvmtype;
			this.llvmref = llvmref;
			this.isLiteral = isLiteral;
			this.value = value;
		}*/
		public ExprValue(Token.Kind tokenType, string llvmref, bool isLiteral, object value = null) {
			this.llvmref = llvmref;
			this.isLiteral = isLiteral;
			this.value = value;
			switch (tokenType) {
				case Token.Kind.INT_LITERAL:
					info = new CyType(CyType.Kind.INT, 0);
					break;
				case Token.Kind.FLOAT_LITERAL:
					info = new CyType(CyType.Kind.FLOAT, 0);
					break;
				case Token.Kind.INT8:
					info = new CyType(CyType.Kind.INT, 8);
					break;
				case Token.Kind.INT16:
					info = new CyType(CyType.Kind.INT, 16);
					break;
				case Token.Kind.INT:
				case Token.Kind.INT32:
					info = new CyType(CyType.Kind.INT, 32);
					break;
				case Token.Kind.INT64:
					info = new CyType(CyType.Kind.INT, 64);
					break;
				case Token.Kind.INT128:
					info = new CyType(CyType.Kind.INT, 128);
					break;
				case Token.Kind.FLOAT16:
					info = new CyType(CyType.Kind.FLOAT, 16);
					break;
				case Token.Kind.FLOAT32:
					info = new CyType(CyType.Kind.FLOAT, 32);
					break;
				case Token.Kind.FLOAT64:
					info = new CyType(CyType.Kind.FLOAT, 64);
					break;
				case Token.Kind.FLOAT128:
					info = new CyType(CyType.Kind.FLOAT, 128);
					break;
				case Token.Kind.STR:
				case Token.Kind.STR_LITERAL:
					info = new CyType(CyType.Kind.STR, 8);
					break;
			}
		}
		public ExprValue(CyType info, string llvmref, bool isLiteral, object value = null) {
			this.llvmref = llvmref;
			this.isLiteral = isLiteral;
			this.value = value;
			this.info = info;
		}
		public ExprValue(CyType.Kind cyKind, string llvmref, bool isLiteral, object value = null) {
			this.llvmref = llvmref;
			this.isLiteral = isLiteral;
			this.value = value;
			switch (cyKind) {
				case CyType.Kind.INT:
					if (isLiteral)
						info = new CyType(CyType.Kind.INT, 0);
					else
						info = new CyType(CyType.Kind.INT, 32);
					break;
				case CyType.Kind.FLOAT:
					if (isLiteral)
						info = new CyType(CyType.Kind.FLOAT, 0);
					else
						info = new CyType(CyType.Kind.FLOAT, 64);
					break;
				case CyType.Kind.STR:
					info = new CyType(CyType.Kind.STR, 8);
					break;
			}
		}
		public ExprValue(CyType.Kind cyKind, int size, string llvmref, bool isLiteral, object value = null) {
			this.llvmref = llvmref;
			this.isLiteral = isLiteral;
			this.value = value;
			switch (cyKind) {
				case CyType.Kind.INT:
					info = new CyType(CyType.Kind.INT, size);
					break;
				case CyType.Kind.FLOAT:
					info = new CyType(CyType.Kind.FLOAT, size);
					break;
				case CyType.Kind.STR:
					info = new CyType(CyType.Kind.STR, size);
					break;
			}
		}
		public override string ToString() {
			return $"{info.LLVM()} {llvmref}";
		}
	}



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







		public object VisitAssignExpr(Expr.Assign expr, object options) {
			return options;
		}


		public object VisitBinaryExpr(Expr.Binary expr, object options) {
			ExprValue leftVal = (ExprValue)expr.left.Accept(this, options);
			ExprValue rightVal = (ExprValue)expr.right.Accept(this, options);
			switch (expr.token.type) {
				case Token.Kind.PLUS:
					if (leftVal.isLiteral && rightVal.isLiteral) {
						return Type.Literal.Add(leftVal, rightVal);
					}
					break;
				case Token.Kind.STAR:
					if (leftVal.isLiteral && rightVal.isLiteral) {
						return Type.Literal.Mult(leftVal, rightVal);
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

		class FuncDef {
			public string name;
			public CyType retType;
			public FuncDef(string name, CyType retType) {
				this.name = name;
				this.retType = retType;
			}
		}
		List<FuncDef> curFunc = new List<FuncDef>();

		public object VisitFunctionStmt(Stmt.Function stmt, object options) {
			string funcName = stmt.token.lexeme;
			if (stmt.token.lexeme == "Main")
				funcName = funcName.ToLower();
			// TODO set i32 to whatever function returns (type), check return matches type here - last body.accept() could return type?
			AppendLine($"define dso_local {stmt.returnType.info.LLVM()} @{funcName}() #0 {{");
			FuncDef func = new FuncDef(funcName, stmt.returnType.info);
			curFunc.Add(func);
			foreach (Stmt body in stmt.body)
				body.Accept(this, options);
			AppendLine("}\n\n");
			curFunc.RemoveAt(curFunc.Count - 1);
			return options;
		}


		public object VisitLiteralExpr(Expr.Literal expr, object options) {
			return new ExprValue(expr.token.type, expr.value.ToString(), true, expr.value);
		}


		public object VisitReturnStmt(Stmt.Return stmt, object options) {
			if (stmt.value != null) {
				ExprValue value = (ExprValue)stmt.value.Accept(this, options);
				if (value.isLiteral)
					value = Type.Literal.Cast(value, curFunc[curFunc.Count - 1].retType);
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


		// creating a variable
		public object VisitVarStmt(Stmt.Var stmt, object options) {
			if (stmt.initialiser != null) {	// var equals something
				
			}
			return options;
		}

		public object VisitTypeStmt(Stmt.Type stmt, object options) {
			throw new NotImplementedException();
		}




	} // Compiler
}
