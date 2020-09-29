using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;



namespace Cy {

	/*
	// info to track a declared variable
	public class ExprValue {
		public string llvmref;  // this will be %1 etc, or actual number if a literal
		public bool isLiteral;  // is this an actual value
		public object value;    // if literal this is the value
		public CyType info;

		public ExprValue(Token.Kind tokenType, string llvmref, bool isLiteral, object value = null) {
			this.llvmref = llvmref;
			this.isLiteral = isLiteral;
			this.value = value;
			this.info = new CyType(tokenType);
		}
		public ExprValue(CyType info, string llvmref, bool isLiteral, object value = null) {
			this.llvmref = llvmref;
			this.isLiteral = isLiteral;
			this.value = value;
			this.info = info;
		}
		
		//public ExprValue(CyType.Kind cyKind, int size, string llvmref, bool isLiteral, object value = null) {
		//	this.llvmref = llvmref;
		//	this.isLiteral = isLiteral;
		//	this.value = value;
		//	info = new CyType(cyKind, size);
		//}
		

		// allocate space for a variable on the stack
		public string StackAllocate() {
			return $"{llvmref} = alloca {info.Llvm()}, align {info.AlignSize()}";
		}

		// store a value in this llvm variable
		public string Store(object val) {
			return $"store {info.Llvm()} {val.ToString()} {info.Llvm()}* {llvmref}, align {info.AlignSize()}";
		}

		// bring a variable copy back ready to use
		public string Load(Func<string> NextLLvmVarRef) {	// need to return at least the llvm ref here as well...
			return $"{NextLLvmVarRef()} = load {info.Llvm()}, {info.Llvm()}* {llvmref}, align {info.AlignSize()}";
		}

		// Mult this by another
		public string Mult(Func<string> NextLLvmVarRef, ExprValue rhs) {
			if (!isLiteral) {
				Load(NextLLvmVarRef)
			}
			return $"{NextLLvmVarRef()} = load {info.Llvm()}, {info.Llvm()}* {llvmref}, align {info.AlignSize()}";
		}


		// will return something like "%1 i32"
		public override string ToString() {
			return $"{info.Llvm()} {llvmref}";
		}


	} // ExprValue
	*/

	// access to all declared variables and their visibilty
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


	} // ExprList







	// tracks which object and function we are in/compiling (hierarchy)
	public class CurEnv {

		// an environment, object or function, that may contain variables
		class EnvItem {
			public enum Attribute { PRIVATE, PUBLIC };
			public string name;             // object or functions name
			public Attribute attr;          // are the contents of this item visible to outside?
			public EnvItem parent;          // parent EnvItem
			public List<EnvItem> children;  // list of children
			public EnvItem(string name, Attribute attr, EnvItem parent) {
				this.name = name;
				this.attr = attr;
				this.parent = parent;
				this.children = new List<EnvItem>();
			}
		}


		public const string globalName = "*!Global!*"; // safe as its not possible for an object or function to start with *!

		List<EnvItem> env;  // full environment
		EnvItem cur;        // current part of env we are processing/in


		public CurEnv() {
			env = new List<EnvItem>();
			EnvItem item = new EnvItem(globalName, EnvItem.Attribute.PUBLIC, null);
			cur = item;
			env.Add(item);
		}

		// call when entering an object
		public void InObject(string objName) {
			EnvItem item = new EnvItem(objName, EnvItem.Attribute.PUBLIC, cur);  // contents of objects are all public (currently)
			cur.children.Add(item);
			env.Add(item);
		}
		// call when finished object
		public bool OutObject(string objName) {
			if (objName != cur.name) {
				Console.WriteLine($"EnvError: Exiting wrong object, in: {cur.name}, attempting to exit {objName}");
				// TODO deal with error
				return false;
			} else
				cur = cur.parent;
			return true;
		}
		// call when entering a function
		public void InFunction(string funcName) {
			EnvItem item = new EnvItem(funcName, EnvItem.Attribute.PRIVATE, cur);    // contents of functions are all private
			cur.children.Add(item);
			env.Add(item);
		}
		// call when finished function - after return
		public bool OutFunction(string funcName) {
			if (funcName != cur.name) {
				Console.WriteLine($"EnvError: Exiting wrong function, in: {cur.name}, attempting to exit {funcName}");
				// TODO deal with error
				return false;
			} else
				cur = cur.parent;
			return true;
		}

		// returns a int list describing the current hierarchy, starts at globalName then goes to current
		public List<int> Get() {
			return Get(env[env.Count - 1]);
		}

		// returns a int list describing the current hierarchy, starts at globalName then goes to item
		List<int> Get(EnvItem item) {
			List<int> hierarchy = new List<int>();
			int idx = env.IndexOf(item);
			do {
				hierarchy.Add(idx);
				idx = env.IndexOf(env[idx].parent);
			} while (env[idx].parent != null);
			hierarchy.Reverse();
			return hierarchy;
		}


		// just to make IsVisible easier and clearer
		class EnvListWalk {
			List<EnvItem> env;
			List<int> list;
			int idxIntoList;
			public EnvListWalk(List<EnvItem> env, List<int> list) {
				this.env = env;
				this.list = list;
				this.idxIntoList = 0;
			}
			public int EnvIdx() {
				return list[idxIntoList];
			}
			public int EnvPrev() {
				return list[idxIntoList - 1];
			}
			public void Next() {
				idxIntoList++;
			}
			public bool SomeLeft() {
				return idxIntoList < list.Count();
			}
			public bool PrevContainsCur() {
				return env[EnvPrev()].children.Contains(env[EnvIdx()]);
			}
			public bool PrevContainsCurAndVisible() {
				return env[EnvPrev()].children.Contains(env[EnvIdx()]) && env[EnvIdx()].attr == EnvItem.Attribute.PUBLIC;
			}
		}

		// Return true, if contents of dest are visible from current.
		// Only use after a complete type scan has been completed, otherwise false negatives may appear.
		public bool IsVisible(List<int> dest) {
			EnvListWalk destList = new EnvListWalk(env, dest);
			List<int> curItems = Get(cur);     // env 0 is always global
			EnvListWalk curList = new EnvListWalk(env, curItems);
			while (curList.EnvIdx() == destList.EnvIdx() && destList.SomeLeft() && curList.SomeLeft()) {
				curList.Next();
				destList.Next();
			}
			if (destList.SomeLeft() && destList.PrevContainsCur()) {
				destList.Next();
				while (destList.SomeLeft() && destList.PrevContainsCurAndVisible())
					destList.Next();
			}
			return !destList.SomeLeft();
		}

	} // CurEnv









	public class Symbols : Expr.IVisitor, Stmt.IVisitor {

		public object VisitAssignExpr(Expr.Assign expr, object options) {
			return null;
		}

		public object VisitBinaryExpr(Expr.Binary expr, object options) {
			return null;
		}

		public object VisitBlockStmt(Stmt.Block stmt, object options) {
			foreach (Stmt statement in stmt.statements)
				statement.Accept(this, null);
			return null;
		}

		public object VisitExpressionStmt(Stmt.Expression stmt, object options) {
			return null;
		}

		public object VisitInputVarStmt(Stmt.InputVar invar, object options) {
			return invar.type.token.lexeme + " " + invar.token.lexeme;
		}

		public object VisitFunctionStmt(Stmt.Function stmt, object options) {
			// create a symbol table to start this functional object
			foreach (var param in stmt.input)
				param.Accept(this, options);
			foreach (Stmt body in stmt.body)
				body.Accept(this, null);
			return null;
		}


		public object VisitLiteralExpr(Expr.Literal expr, object options) {
			if (expr.value == null)
				return "nil";
			return expr.value.ToString();
		}

		public object VisitReturnStmt(Stmt.Return stmt, object options) {
			if (stmt.value == null)
				return "(return)";
			return null;
		}

		public object VisitTypeStmt(Stmt.StmtType stmt, object options) {
			return stmt.token.lexeme;
		}

		public object VisitUnaryExpr(Expr.Unary expr, object options) {
			return null;
		}

		public object VisitVariableExpr(Expr.Variable var, object options) {
			return var.token.lexeme;
		}

		public object VisitVarStmt(Stmt.Var stmt, object options) {
			if (stmt.initialiser == null)
				return null;
			return null;
		}


	} // Symbols
}
