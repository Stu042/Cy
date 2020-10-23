using System;
using System.Collections.Generic;
using System.Text;
using System.IO;



namespace Cy {


	// track types, visibility what is avail etc
	public class Tracking {
		TypeHierarchy.Environ global;
		TypeHierarchy.Environ current;

		public Tracking(TypeHierarchy.Environ global) {
			this.global = global;
			current = global;
		}

		public TypeHierarchy.Environ GetCurrent() {
			return current;
		}
		public TypeHierarchy.Environ GetGlobal() {
			return global;
		}

		// call when entering an object
		public void InObject(string objName) {
			foreach (var c in current.children) {
				if (c.name == objName && !c.isFunc) {
					current = c;
					return;
				}
			}
		}

		// call when finished object
		public bool OutObject(string objName) {
			if (objName != current.name) {
				Console.WriteLine($"Tracking Error: Exiting wrong object, in: {current.name}, attempting to exit {objName}");
				// TODO deal with error
				return false;
			} else
				current = current.parent;
			return true;
		}

		// call when entering a function
		public void InFunction(string funcName) {
			foreach (var c in current.children) {
				if (c.name == funcName && c.isFunc) {
					current = c;
					return;
				}
			}
		}

		// call when finished function - after return
		public bool OutFunction(string funcName) {
			if (funcName != current.name) {
				Console.WriteLine($"Tracking Error: Exiting wrong function, in: {current.name}, attempting to exit {funcName}");
				// TODO deal with error
				return false;
			} else
				current = current.parent;
			return true;
		}



		TypeHierarchy.Environ FindFrom(string[] destObj, TypeHierarchy.Environ start) {
			int destIdx = 0;
			do {
				start = start.children.Find(x => x.name == destObj[destIdx++]);
			} while (destIdx < destObj.Length && start != null && start.attr == TypeHierarchy.Environ.Attribute.PUBLIC);
			if (destIdx == destObj.Length && (start.attr == TypeHierarchy.Environ.Attribute.PUBLIC || destIdx == 1))
				return start;
			return null;
		}


		// Return true, if contents of dest are visible from current.
		// Only use after a complete type scan has been completed, otherwise false negatives may appear.
		public bool IsVisible(string[] destObj) {
			TypeHierarchy.Environ cur = current;
			do {
				TypeHierarchy.Environ found = FindFrom(destObj, cur);
				if (found != null)
					return true;
				cur = cur.parent;
			} while (cur != global);
			return false;
		}

		public TypeHierarchy.Environ Find(string[] destObj) {
			TypeHierarchy.Environ cur = current;
			do {
				TypeHierarchy.Environ found = FindFrom(destObj, cur);
				if (found != null)
					return found;
				cur = cur.parent;
			} while (cur != null);
			return null;
		}

		public Method FindMethod(string name, string[] inargs) {
			TypeHierarchy.Environ p = current.parent;
			return p.methods.Fetch(name, inargs);
		}
		public Method FindMethod(string name, List<string> inargs) {
			TypeHierarchy.Environ p = current.parent;
			return p.methods.Fetch(name, inargs);
		}

		public string[] FullNameArray(string next = "") {
			TypeHierarchy.Environ cur = current;
			List<string> name = new List<string>();
			if (next != "")
				name.Add(next);
			if (cur != global) {
				do {
					name.Add(cur.name);
					cur = cur.parent;
				} while (cur != global);
			}
			name.Reverse();
			return name.ToArray();
		}

		// return the full name of a function/object that is defined in the current scope
		// Note: next doesnt need to exist
		public string FullName(string next, string[] inArgTypes = null) {
			string[] name = FullNameArray(next);
			return Llvm.GetUniqueName(string.Join(".", name), inArgTypes);
		}

		public bool HasChild(TypeHierarchy.Environ env, string childName) {
			var found = env.children.Find(x => x.name == childName);
			return (found != null);
		}


	} // Tracking




	class Compiler : Expr.IVisitor, Stmt.IVisitor {
		string outputFilename;
		StringBuilder typeCode;
		StringBuilder code;
		ExprList instances;
		Tracking tracking;

		public Compiler(TypeHierarchy.Environ typeEnvironment) {
			tracking = new Tracking(typeEnvironment);
			typeCode = new StringBuilder();
			code = new StringBuilder();
			instances = new ExprList();
		}



		// access to all declared variables and their visibilty
		// TODO preppend with full function/object name
		public class ExprList {
			Dictionary<string, ExprValue> instances;

			public ExprList() {
				instances = new Dictionary<string, ExprValue>();
			}


			public void AddVar(string varname, ExprValue xpval) {
				instances.Add(varname, xpval);
			}

			public ExprValue GetVar(string name) {
				return instances[name];
			}

			public void UpdateExprVal(string name, ExprValue newXpVal) {
				instances[name] = newXpVal;
			}

		} // ExprList





		class FunctionInstance {
			public string name;
			public CyType type;
			public bool isFunction;
			public FunctionInstance(string name, CyType type, bool isFunction) {
				this.name = name;
				this.type = type;
				this.isFunction = isFunction;
			}
		} // MethodInstance
		List<FunctionInstance> curEnv = new List<FunctionInstance>();



		//public void Compile(Expr expr) {
		//	expr.Accept(this, null);
		//}

		public void Compile(Stmt stmt) {
			stmt.Accept(this, null);
		}



		public override string ToString() {
			return filePreLLVMCode.Replace(fileName, outputFilename) + typeCode.ToString() + code.ToString() + filePostLLVMCode;
		}




		////////////////////////////////////////////
		// this is all a hack, todo fix this
		// it is getting the base llvm ir required
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
				Util.RunCmd($"clang.exe {fileName} -S -emit-llvm");
				llvmPreCode = File.ReadAllLines(llfileName);
			} finally {
				Util.RunCmd($"del /f {fileName}");
				Util.RunCmd($"del /f {llfileName}");
			}
			string modulestr = Util.GetStr("ModuleID", llvmPreCode);
			pre.AppendLine(modulestr);
			string srcstr = Util.GetStr("source_filename", llvmPreCode);
			pre.AppendLine(srcstr.Replace("\\\\", "\\"));
			string datalayoutstr = Util.GetStr("target datalayout", llvmPreCode);
			pre.AppendLine(datalayoutstr);
			string triplestr = Util.GetStr("target triple", llvmPreCode);
			pre.AppendLine(triplestr);
			pre.AppendLine();
			filePreLLVMCode = pre.ToString();
			filePostLLVMCode = "attributes #0 = { noinline norecurse nounwind optnone uwtable \"correctly-rounded-divide-sqrt-fp-math\"=\"false\" \"disable-tail-calls\"=\"false\" \"frame-pointer\"=\"none\" \"less-precise-fpmad\"=\"false\" \"min-legal-vector-width\"=\"0\" \"no-infs-fp-math\"=\"false\" \"no-jump-tables\"=\"false\" \"no-nans-fp-math\"=\"false\" \"no-signed-zeros-fp-math\"=\"false\" \"no-trapping-math\"=\"false\" \"stack-protector-buffer-size\"=\"8\" \"target-cpu\"=\"x86-64\" \"target-features\"=\"+cx8,+fxsr,+mmx,+sse,+sse2,+x87\" \"unsafe-fp-math\"=\"false\" \"use-soft-float\"=\"false\" }\nattributes #1 = { noinline nounwind optnone uwtable \"correctly-rounded-divide-sqrt-fp-math\"=\"false\" \"disable-tail-calls\"=\"false\" \"frame-pointer\"=\"none\" \"less-precise-fpmad\"=\"false\" \"min-legal-vector-width\"=\"0\" \"no-infs-fp-math\"=\"false\" \"no-jump-tables\"=\"false\" \"no-nans-fp-math\"=\"false\" \"no-signed-zeros-fp-math\"=\"false\" \"no-trapping-math\"=\"false\" \"stack-protector-buffer-size\"=\"8\" \"target-cpu\"=\"x86-64\" \"target-features\"=\"+cx8,+fxsr,+mmx,+sse,+sse2,+x87\" \"unsafe-fp-math\"=\"false\" \"use-soft-float\"=\"false\" }\n\n!llvm.module.flags = !{!0, !1}\n!llvm.ident = !{!2}\n\n!0 = !{i32 1, !\"wchar_size\", i32 2}\n!1 = !{i32 7, !\"PIC Level\", i32 2}\n!2 = !{!\"cy version " + typeof(Program).Assembly.GetName().Version + "\"}\n\n";
		}
		// end of hack */
		/////////////////


		public void Prep(string outputFilename) {
			this.outputFilename = outputFilename;
			GetLLVMPreCode();
		}



		public object VisitGetExpr(Expr.Get expr, object options) {
			ExprValue xp = new ExprValue(code);
			return xp;
		}

		public object VisitCallExpr(Expr.Call expr, object options) {
			ExprValue xpvalRet = null;
			string[] findName = { expr.callee.token.lexeme };
			TypeHierarchy.Environ env = tracking.Find(findName);
			if (env != null) {
				if (env.isFunc) {
					List<ExprValue> xpvslist = new List<ExprValue>();
					List<string> argsTypes = new List<string>();
					foreach (var arg in expr.arguments) {
						ExprValue xpvs = (ExprValue)arg.Accept(this, options);
						xpvslist.Add(xpvs);
						argsTypes.Add(xpvs.cytype.Llvm());
					}
					Method m = tracking.FindMethod(expr.callee.token.lexeme, argsTypes);
					if (m != null) {
						List<string> argarray = new List<string>();
						foreach (var xpvs in xpvslist)
							argarray.Add(xpvs.ToString());
						xpvalRet = new ExprValue(code, m.Cytype());
						code.Append(Llvm.Indent($"{xpvalRet.llvmref} = call {m.GetLlvmType()} @{m.UniqueName()}({string.Join(", ", argarray.ToArray())})\n"));
					} else {
						Display.Error(expr.token, $"Compiler Error: unable to find function {expr.callee.token.lexeme}.");
					}
				} else {
					List<ExprValue> xpvslist = new List<ExprValue>();
					List<string> argsTypes = new List<string>();
					foreach (var arg in expr.arguments) {
						ExprValue xpvs = (ExprValue)arg.Accept(this, options);
						xpvslist.Add(xpvs);
						argsTypes.Add(xpvs.cytype.Llvm());
					}
					Method m = tracking.FindMethod(expr.callee.token.lexeme, argsTypes);
					if (m != null) {
						xpvalRet = new ExprValue(code, m.Cytype());
						code.Append(Llvm.Indent($"{xpvalRet.llvmref} = call {m.GetLlvmType()} @{m.UniqueName()}(%class.Data* %2)"));
					} else {
						//if (is default constructor) then do it inline
						//else	error as trying to call nonexistant class method
					}
				}
			}
			return xpvalRet;
		}

		public object VisitAssignExpr(Expr.Assign expr, object options) {
			ExprValue xpv = (ExprValue)expr.Accept(this, options);
			return xpv;
		}


		public object VisitBinaryExpr(Expr.Binary expr, object options) {
			ExprValue leftVal = (ExprValue)expr.left.Accept(this, options);
			ExprValue rightVal = (ExprValue)expr.right.Accept(this, options);
			switch (expr.token.type) {
				case Token.Kind.PLUS: {
					ExprValue xpv = leftVal.Add(rightVal);
					return xpv;
				}
				case Token.Kind.MINUS: {
					ExprValue xpv = leftVal.Sub(rightVal);
					return xpv;
				}
				case Token.Kind.STAR: {
					ExprValue xpv = leftVal.Mult(rightVal);
					return xpv;
				}
				case Token.Kind.SLASH: {
					ExprValue xpv = leftVal.Div(rightVal);
					return xpv;
				}
			}
			return options;
		}


		public object VisitBlockStmt(Stmt.Block stmt, object options) {
			foreach (Stmt statement in stmt.statements)
				statement.Accept(this, options);
			return options;
		}


		public object VisitExpressionStmt(Stmt.Expression stmt, object options) {
			return options;
		}


		public object VisitFunctionStmt(Stmt.Function stmt, object options) {
			Llvm.RefStartFunc();
			string funcName = stmt.token.lexeme;
			if (stmt.token.lexeme == "Main")
				funcName = funcName.ToLower();              // until we add our own startup main code
			string[] inargtypes = new string[stmt.input.Count];
			int idx = 0;
			foreach (Stmt.InputVar inarg in stmt.input)
				inargtypes[idx++] = inarg.type.info.Llvm();
			string fullName = tracking.FullName(funcName, inargtypes);
			tracking.InFunction(stmt.token.lexeme);
			FunctionInstance func = new FunctionInstance(funcName, stmt.returnType.info, true);
			curEnv.Add(func);
			idx = 0;
			string[] inargs = new string[stmt.input.Count];
			ExprValue[] inargxpvs = new ExprValue[stmt.input.Count];
			foreach (Stmt.InputVar inarg in stmt.input) {
				inargxpvs[idx] = (ExprValue)inarg.Accept(this, options);
				inargs[idx++] = instances.GetVar(inarg.token.lexeme).ToString();
			}
			if (fullName == "main")
				code.Append(Llvm.Indent($"define dso_local {stmt.returnType.info.Llvm()} @{fullName}({string.Join(", ", inargs)}) #0 {{\n"));
			else
				code.Append(Llvm.Indent($"define dso_local {stmt.returnType.info.Llvm()} @{fullName}({string.Join(", ", inargs)}) #1 {{\n"));
			Llvm.RefStartBody();
			for (int i=0; i < inargxpvs.Length; i++) {
				ExprValue xp = new ExprValue(code, inargxpvs[i].cytype);
				xp.StackAllocate();
				xp.Store(inargxpvs[i]);
				instances.UpdateExprVal(stmt.input[i].token.lexeme, xp);
			}
			foreach (Stmt body in stmt.body)
				body.Accept(this, options);
			code.Append(Llvm.Indent("}\n\n"));
			if (curEnv[curEnv.Count - 1] != func)
				Display.Error(stmt.token, $"Compiler trying to return from a different object: {curEnv[curEnv.Count - 1].name}, should be {func.name}");
			curEnv.RemoveAt(curEnv.Count - 1);
			tracking.OutFunction(stmt.token.lexeme);
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



		void WriteClassDefinition(string fullName, List<Stmt.Var> members) {
			int idx = 0;
			string[] memtypes = new string[members.Count];
			foreach (Stmt.Var v in members)
				memtypes[idx++] = v.stmtType.info.Llvm();
			typeCode.Append("%class." + fullName + " = type { " + string.Join(", ", memtypes) + " }\n");
		}

		public object VisitClassStmt(Stmt.StmtClass obj, object options) {
			string fullName = tracking.FullName(obj.token.lexeme);
			tracking.InObject(obj.token.lexeme);
			WriteClassDefinition(fullName, obj.members);
			foreach (Stmt.Function f in obj.methods)
				f.Accept(this, options);
			tracking.OutObject(obj.token.lexeme);
			return options;
		}



		public object VisitLiteralExpr(Expr.Literal expr, object options) {
			return new ExprValueLiteral(code, expr.token.type, expr.value);
		}

		public object VisitSetExpr(Expr.Set expr, object options) {
			throw new NotImplementedException();
		}

		public object VisitUnaryExpr(Expr.Unary expr, object options) {
			throw new NotImplementedException();
		}



		public object VisitVariableExpr(Expr.Variable var, object options) {
			ExprValue xpvar = instances.GetVar(var.token.lexeme);
			xpvar = xpvar.Load();
			return xpvar;
		}


		// creating a variable
		public object VisitVarStmt(Stmt.Var stmt, object options) {
			ExprValue xpv = new ExprValue(code, stmt.stmtType.info);
			instances.AddVar(stmt.token.lexeme, xpv);
			xpv.StackAllocate();
			if (stmt.initialiser != null) {     // var equals something
				ExprValue init = (ExprValue)stmt.initialiser.Accept(this, options);
 				xpv.Store(init.llvmref);
			}
			return options;
		}

		public object VisitTypeStmt(Stmt.StmtType stmt, object options) {
			throw new NotImplementedException();
		}

		public object VisitInputVarStmt(Stmt.InputVar invar, object options) {
			ExprValue xpllvm = new ExprValue(code, invar.type.info);
			instances.AddVar(invar.token.lexeme, xpllvm);
			//code.Append(xpllvm.StackAllocate());
			//if (invar.initialiser != null) {     // var equals something
			//	ExprValue init = (ExprValue)stmt.initialiser.Accept(this, options);
			//	code.Append(init.code);
			//	code.Append(xpllvm.Store(init.exprValue.llvmref));
			//}
			return xpllvm;
		}
	} // Compiler
}
