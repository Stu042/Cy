using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Security.Policy;
using System.ComponentModel.Design;
using System.Collections;
using System.ComponentModel;

/*
 * Instead of using foreach...
 *	AnObj:
 *		int a
 *		str b
 *		
 *	List<AnObj> listData;
 *	for (b = all listData)
 *		b.a = 5
 *		b.b = "hello"
 * 
 * so "all" creates an iterator and allows the next item to be assigned.
 */

namespace Cy {


	// track types, visibility what is avail etc
	// Functional objects must use unique name, but not the full name
	public class TypeTracking {
		TypeHierarchy.Environ global;
		TypeHierarchy.Environ current;

		public TypeTracking(TypeHierarchy.Environ global) {
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
		public void InFunction(string funcName, List<Stmt.InputVar> inargs) {
			funcName = Llvm.GetFuncName(funcName, inargs);
			foreach (var c in current.children) {
				if (c.name == funcName && c.isFunc) {
					current = c;
					return;
				}
			}
		}

		// call when finished function - after return
		public bool OutFunction(string funcName, List<Stmt.InputVar> inargs) {
			funcName = Llvm.GetFuncName(funcName, inargs);
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
			} while (destIdx < destObj.Length && start != null && start.attr == Attribute.PUBLIC);
			if (destIdx == destObj.Length && (start.attr == Attribute.PUBLIC || destIdx == 1))
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

		public Method FindMethodIn(TypeHierarchy.Environ startEnv, string name, List<string> inargs) {
			return startEnv.methods.Fetch(name, inargs);
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
		TypeTracking typeTracking;

		public Compiler(TypeHierarchy.Environ typeEnvironment) {
			typeTracking = new TypeTracking(typeEnvironment);
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
			public void RemVar(string varname, ExprValue xpval) {
				instances.Remove(varname);
			}

			public ExprValue GetVar(string name) {
				return instances[name];
			}

			public void UpdateExprVal(string name, ExprValue newXpVal) {
				instances[name] = newXpVal;
			}

		} // ExprList


		// track currently declared variables
		public class InstanceTracking {
			class ObjEnv {
				Attribute visibility;						// private or public
				List<Dictionary<string, ExprValue>> block;	// current vars in this object

				public ObjEnv(Attribute visibility = Attribute.PRIVATE) {
					this.visibility = visibility;
					block = new List<Dictionary<string, ExprValue>> {
						new Dictionary<string, ExprValue>()
					};
				}
				public void EnterBlock() {
					block.Add(new Dictionary<string, ExprValue>());
				}
				public void LeaveBlock() {
					block.RemoveAt(block.Count - 1);
				}
				// add a var to the last block
				public void AddVar(string name, ExprValue var) {
					block[block.Count - 1].Add(name, var);
				}
				// return last occurance of a var called name, else return null from within this object
				public ExprValue GetVar(string name) {
					for (int i = block.Count; i >= 0; --i) {
						if (block[i].ContainsKey(name))
							return block[i][name];
					}
					return null;
				}
				// remove last occurance of var from this object, return true if successful
				public bool RemoveVar(string name) {
					for (int i = block.Count; i >= 0; --i) {
						if (block[i].ContainsKey(name)) {
							block[i].Remove(name);
							return true;
						}
					}
					return false;
				}
			} // ObjEnv

			Stack<ObjEnv> environs;
			ObjEnv global;

			public InstanceTracking() {
				environs = new Stack<ObjEnv>();
				global = new ObjEnv(Attribute.PUBLIC);
				environs.Push(global);
			}

			public void EnterObj(Attribute visibility = Attribute.PRIVATE) {
				environs.Push(new ObjEnv(visibility));
			}
			public void LeaveObj() {
				environs.Pop();
			}
			public void EnterFunc() {
				environs.Push(new ObjEnv(Attribute.PRIVATE));
			}
			public void LeaveFunc() {
				environs.Pop();
			}
			public void EnterBlock() {
				environs.Peek().EnterBlock();
			}
			public void LeaveBlock() {
				environs.Peek().LeaveBlock();
			}

			public void AddVar(string varName, ExprValue xpval) {
				environs.Peek().AddVar(varName, xpval);
			}
			public ExprValue GetVar(string varName) {
				ExprValue val =  environs.Peek().GetVar(varName);
				if (val == null)
					val = global.GetVar(varName);
				return val;
			}
			/*
			 * Example of how InstanceTracking works
			 * 
			 *	AnObject:			< call EnterObj with visibilty set to public
			 *		int a			< call AddVar
			 *		float b			< call AddVar
			 *		float Mult():	< call AddVar then EnterObj with visibilty set to private 
			 *			return b * a	< has access to int a and float b
			 *						< call LeaveObj
			 *		float MultAnCheck():	< call AddVar then EnterObj with visibilty set to private 
			 *			float ans = Mult()	< call AddVar
			 *			if (ans == 0)	< GetVar
			 *				return 1
			 *			return ans	< GetVar, then LeaveFunc
			 *						< LeaveObj is called
			 *		
			 *	int32 a = 0			< call AddVar, return this when a is asked for
			 *	int64 c = -1		< call AddVar, return this when c is asked for
			 *	
			 *	void AFunction():	< EnterObj is called with visibilty set to private
			 *		str a = "hello"	< call AddVar, return this when a is asked for
			 *		float a = 5.0	< call AddVar, return this when a is asked for, str a is deleted as if never existed
			 *		for(...)		< EnterBlock() is called
			 *			int a = 2	< call AddVar, return this when a is asked for, float a still exists but in a previous block
			 *		next line		< LeaveBlock is called - block is deleted along with int a
			 *		CallAFunction() < all previous is kept but not accessable from within this function. EnterFunc is called when compiling this function and when finished LeaveFunc is called
			 *		AnObject b		< call AddVar, b along with its public members are added, named b b.a and b.b float a is still accessable
			 *		b.Mult()		< In Mult int a and float b are accessable (members of AnObject) , nothing from AFunction is visible
			 *		b.MultAnCheck()	< in MultAnCheck int a and float b but if Mult declared vars they would not be visible
			 *		next line		< a, b, b.a, b.b and c are still accessable
			 *		
			 *	void CallAFunction():	< EnterFunc is called
			 *		int b = 0		< call AddVar, return this when b is asked for, int32 a and int64 c are still accessable
			 *		int c = 0		< call AddVar, return this when c is asked for, int32 a is still accessable
			 *		str h = "Stu"	< call AddVar, return this when h is asked for, LeaveFunc is called
			 */
		} // InstanceTracking



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
			string modulestr = Util.FindStrInArray("ModuleID", llvmPreCode);
			pre.AppendLine(modulestr);
			string srcstr = Util.FindStrInArray("source_filename", llvmPreCode);
			pre.AppendLine(srcstr.Replace("\\\\", "\\"));
			string datalayoutstr = Util.FindStrInArray("target datalayout", llvmPreCode);
			pre.AppendLine(datalayoutstr);
			string triplestr = Util.FindStrInArray("target triple", llvmPreCode);
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
			TypeHierarchy.Environ env = typeTracking.Find(findName);
			if (env != null) {
				if (env.isFunc) {
					List<ExprValue> xpvslist = new List<ExprValue>();
					List<string> argsTypes = new List<string>();
					foreach (var arg in expr.arguments) {
						ExprValue xpvs = (ExprValue)arg.Accept(this, options);
						xpvslist.Add(xpvs);
						argsTypes.Add(xpvs.cytype.Llvm());
					}
					Method m = typeTracking.FindMethod(expr.callee.token.lexeme, argsTypes);
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
					Method m = typeTracking.FindMethodIn(env, expr.callee.token.lexeme, argsTypes);
					if (m != null) {
						xpvalRet = new ExprValue(code, m.Cytype());
						code.Append(Llvm.Indent($"{xpvalRet.llvmref} = call {m.GetLlvmType()} @{env.name}.{m.UniqueName()}(%class.Data* %2)"));
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


		// deals with functions, such as int Main(str argv):\n body... , as well as member functions - including constructors and destructors
		public object VisitFunctionStmt(Stmt.Function stmt, object options) {
			Llvm.RefStartFunc();
			string fullName = Llvm.FullName(typeTracking, stmt.token.lexeme, stmt.input);
			typeTracking.InFunction(stmt.token.lexeme, stmt.input);
			FunctionInstance func = new FunctionInstance(stmt.token.lexeme, stmt.returnType.info, true);
			curEnv.Add(func);
			int idx = 0;
			string[] inargtypes = new string[stmt.input.Count];
			ExprValue[] inargxpvs = new ExprValue[stmt.input.Count];
			foreach (Stmt.InputVar inarg in stmt.input) {
				inargxpvs[idx] = (ExprValue)inarg.Accept(this, options);
				inargtypes[idx++] = instances.GetVar(inarg.token.lexeme).ToString();
			}
			if (fullName == "main")
				code.Append(Llvm.Indent($"define dso_local {stmt.returnType.info.Llvm()} @{fullName}({string.Join(", ", inargtypes)}) #0 {{\n"));
			else
				code.Append(Llvm.Indent($"define dso_local {stmt.returnType.info.Llvm()} @{fullName}({string.Join(", ", inargtypes)}) #1 {{\n"));
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
			typeTracking.OutFunction(stmt.token.lexeme, stmt.input);
			return options;
		}


		public object VisitReturnStmt(Stmt.Return stmt, object options) {
			if (stmt.value != null) {
				ExprValue value = (ExprValue)stmt.value.Accept(this, options);
				if (curEnv[curEnv.Count - 1].isFunction) {
					value.Cast(curEnv[curEnv.Count - 1].type);
					code.Append(Llvm.Indent($"ret {value}\n"));
				} else {
					Display.Error(stmt.token, $"Object is non functional, unable to return from this: {curEnv[curEnv.Count - 1].name}");
				}
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
			string fullName = Llvm.FullName(typeTracking, obj.token.lexeme);
			typeTracking.InObject(obj.token.lexeme);
			WriteClassDefinition(fullName, obj.members);
			// create a `this` and set options to it to pass to all child statements
			foreach (Stmt.Function f in obj.methods)
				f.Accept(this, options);
			typeTracking.OutObject(obj.token.lexeme);
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
