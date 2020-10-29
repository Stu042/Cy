using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Specialized;


namespace Cy {

	///////////////////////////////////////////
	// info for a class memer (variable)
	public class Member {
		public string name;
		public string llvmtype;
		public Member(string name, string llvmtype) {
			this.name = name;
			this.llvmtype = llvmtype;
		}

	} // Member


	///////////////////////////////////////////
	// Defined objects and their members
	public class MemberList {
		List<Member> members; // holds source name and llvm type of member/variable/method
		int alignSize;

		public MemberList() {
			members = new List<Member>();
		}

		public void AddVar(string varname, string llvmtype, int alignSize) {
			members.Add(new Member(varname, llvmtype));
			if (alignSize > this.alignSize)
				this.alignSize = alignSize;
		}

		public string GetLlvmType(string name) {
			foreach (Member m in members)
				if (m.name == name)
					return m.llvmtype;
			return "";
		}

		public int GetIndex(string name) {
			for (int i = 0; i < members.Count; i++)
				if (members[i].name == name)
					return i;
			return -1;
		}

		public int AlignSize() {
			return alignSize;
		}

	} // MemberList





	///////////////////////////////////////////
	// info for a class Method (function)
	public class Method {
		string uniqueStr;               // unique name
		string name;                    // method name
		List<string> argLlvmTypes;      // input types
		CyType cytype;

		public Method(string uniqueStr, string name, CyType cytype, List<string> inputLlvmTypes) {
			this.uniqueStr = uniqueStr;
			this.name = name;
			this.argLlvmTypes = inputLlvmTypes;
			this.cytype = cytype;
		}

		public string GetLlvmType() {
			return cytype.Llvm();
		}

		public bool IsSame(string uniqueName) {
			return uniqueStr == uniqueName;
		}

		public string UniqueName() {
			return uniqueStr;
		}
		public CyType Cytype() {
			return cytype;
		}


	} // Method



	///////////////////////////////////////////
	public class MethodList {

		List<Method> methods;

		public MethodList() {
			methods = new List<Method>();
		}

		public void AddMethod(string name, CyType cytype, List<string> inputLlvmTypes) {
			methods.Add(new Method(Llvm.GetUniqueName(name, inputLlvmTypes), name, cytype, inputLlvmTypes));
		}

		public string GetLlvmType(string name, string llvmreturntype, List<string> inputLlvmTypes) {
			string uname = Llvm.GetUniqueName(name, inputLlvmTypes);
			foreach (Method m in methods)
				if (m.IsSame(uname))
					return m.GetLlvmType();
			return "";
		}

		public Method Fetch(string methodsName, string[] inputLlvmTypes) {
			string uniquename = Llvm.GetUniqueName(methodsName, inputLlvmTypes);
			return methods.Find(x => x.UniqueName() == uniquename);
		}
		public Method Fetch(string methodsName, List<string> inputLlvmTypes) {
			return Fetch(methodsName, inputLlvmTypes.ToArray());
		}

	} // MethodList





	// tracks which object and function we are in/[pre]compiling (hierarchy)
	// Functional objects must use unique name, but not the full name
	public class TypeHierarchy {

		// an environment, object or function, that may contain variables
		public class Environ {
			public enum Attribute { PRIVATE, PUBLIC };
			public string name;             // object or functions name
			public Attribute attr;          // are the contents of this item visible to outside?
			public Environ parent;          // parent EnvItem
			public List<Environ> children;  // list of child environments
			public MemberList members;		// vars in this object
			public MethodList methods;      // functions in this object
			public int alignSize;           // align size of this type
			public bool isFunc;				// is this a function, if not it is a class

			public Environ(string name, Attribute attr, Environ parent, bool isFunc) {
				this.name = name;
				this.attr = attr;
				this.parent = parent;
				this.isFunc = isFunc;
				this.children = new List<Environ>();
				members = new MemberList();
				methods = new MethodList();
			}

			public int AlignSize() {
				return members.AlignSize();
			}

		} // Environ


		public const string globalName = "*!Global!*"; // safe as its not possible for an object or function to start with *!

		public List<Environ> env;   // full environment
		public Environ cur;                // current part of env we are processing/in
		Stack<bool> inObj;

		public TypeHierarchy() {
			env = new List<Environ>();
			inObj = new Stack<bool>();
			inObj.Push(true);
			Environ item = new Environ(globalName, Environ.Attribute.PUBLIC, null, false);
			cur = item;
			env.Add(item);
		}


		void InItem(Environ item, bool isobj) {
			cur.children.Add(item);
			env.Add(item);
			inObj.Push(isobj);
			cur = item;
		}


		// call when entering an object
		public void InObject(string objName) {
			Environ item = new Environ(objName, Environ.Attribute.PUBLIC, cur, false);  // contents of objects are all public (currently)
			InItem(item, true);
		}

		public void SetAlignSize(int alignSize) {
			cur.alignSize = alignSize;
		}

		// call when finished object
		public bool OutObject(string objName) {
			if (objName != cur.name) {
				Console.WriteLine($"TypeHierarchy: Exiting wrong object, in: {cur.name}, attempting to exit {objName}");
				// TODO deal with error
				return false;
			} else {
				cur = cur.parent;
				inObj.Pop();
			}
			return true;
		}

		// call when entering a function
		public void InFunction(string funcName, CyType cytype, List<Stmt.InputVar> args) {
			string uniqueName = Llvm.GetFuncName(funcName, args);
			Environ item = new Environ(uniqueName, Environ.Attribute.PRIVATE, cur, true);    // contents of functions are all private
			cur.methods.AddMethod(funcName, cytype, Llvm.ArgsToStrList(args));
			InItem(item, false);
		}


		// call when finished function - after return
		public bool OutFunction(string funcName, List<Stmt.InputVar> args) {
			string uniqueName = Llvm.GetFuncName(funcName, args);
			if (uniqueName != cur.name) {
				Console.WriteLine($"EnvError: Exiting wrong function, in: {cur.name}, attempting to exit {funcName}");
				// TODO deal with error
				return false;
			} else {
				cur = cur.parent;
				inObj.Pop();
			}
			return true;
		}

		public void AddMember(string varname, string llvmtype, int alignSize) {
			if (inObj.Peek())
				cur.members.AddVar(varname, llvmtype, alignSize);
		}

		public void AddMethod(string name, CyType cytype, List<string> argllvmTypes) {
			if (inObj.Peek())
				cur.methods.AddMethod(name, cytype, argllvmTypes);
		}


	} // CurEnv









	public class MapUserTypes : Expr.IVisitor, Stmt.IVisitor {
		TypeHierarchy env;

		public MapUserTypes() {
			env = new TypeHierarchy();
		}

		// run this and return the map
		public void Run(Stmt stmt) {
			stmt.Accept(this, null);
		}

		public TypeHierarchy.Environ GetEnv() {
			return env.env[0];
		}


		public object VisitGetExpr(Expr.Get expr, object options) {
			return null;
		}

		public object VisitCallExpr(Expr.Call expr, object options) {
			return null;
		}

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
			return null;
		}

		public object VisitFunctionStmt(Stmt.Function stmt, object options) {
			env.InFunction(stmt.token.lexeme, stmt.returnType.info, stmt.input);
			foreach (Stmt statement in stmt.body)
				statement.Accept(this, null);
			env.OutFunction(stmt.token.lexeme, stmt.input);
			return null;
		}


		public object VisitLiteralExpr(Expr.Literal expr, object options) {
			return null;
		}

		public object VisitReturnStmt(Stmt.Return stmt, object options) {
			return null;
		}

		public object VisitTypeStmt(Stmt.StmtType stmt, object options) {
			return null;
		}

		public object VisitClassStmt(Stmt.StmtClass obj, object options) {
			env.InObject(obj.token.lexeme);
			foreach (Stmt.Var memb in obj.members)
				env.AddMember(memb.token.lexeme, memb.stmtType.info.Llvm(), memb.stmtType.info.AlignSize());
			foreach (Stmt.Function membfun in obj.methods)
				membfun.Accept(this, null);
			env.OutObject(obj.token.lexeme);
			return options;
		}



		public object VisitUnaryExpr(Expr.Unary expr, object options) {
			return null;
		}

		public object VisitVariableExpr(Expr.Variable var, object options) {
			return null;
		}

		// only called for functions
		public object VisitVarStmt(Stmt.Var stmt, object options) {
			env.AddMember(stmt.token.lexeme, stmt.stmtType.info.Llvm(), stmt.stmtType.info.AlignSize());
			return null;
		}

		public object VisitSetExpr(Expr.Set expr, object options) {
			return null;
		}


	} // MapUserTypes


}
