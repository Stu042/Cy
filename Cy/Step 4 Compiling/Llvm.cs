using System;
using System.Collections.Generic;
using System.Text;


namespace Cy {

	// utils to help working with llvm
	public static class Llvm {

		/// <summary>returns the unique name for this function - name plus types</summary>
		public static string GetFuncName(string tokName, List<Stmt.InputVar> inargs) {
			if (tokName == "Main")
				tokName = tokName.ToLower();              // until we add our own startup main code
			return Llvm.GetUniqueName(tokName, ArgsToStr(inargs));

		}

		public static string[] ArgsToStr(List<Stmt.InputVar> inargs = null) {
			if (inargs == null)
				return null;
			string[] inargtypes = new string[inargs.Count];
			int idx = 0;
			foreach (Stmt.InputVar inarg in inargs)
				inargtypes[idx++] = inarg.type.info.Llvm();
			return inargtypes;
		}
		public static List<string> ArgsToStrList(List<Stmt.InputVar> inargs = null) {
			if (inargs == null)
				return null;
			List<string> inargtypes = new List<string>();
			foreach (Stmt.InputVar inarg in inargs)
				inargtypes.Add(inarg.type.info.Llvm());
			return inargtypes;
		}

		/// <summary>string name needs to be fullname, unique name for this method/function/object - as unique as I want it anyway</summary>
		public static string GetUniqueName(string name, string[] inputLlvmTypes = null) {
			string args;
			if (inputLlvmTypes == null)
				args = "";
			else
				args = string.Join("", inputLlvmTypes);
			return name + args;
		}
		public static string GetUniqueName(string name, List<string> inputLlvmTypes) {
			return GetUniqueName(name, inputLlvmTypes.ToArray());
		}

		static string[] FullNameArray(TypeHierarchy.Environ cur, TypeHierarchy.Environ global, string next = "") {
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

		/// <summary>return the full name of a function/object that is defined in the current scope. Note: next doesnt need to exist</summary>
		public static string FullName(Tracking tracking, string next, List<Stmt.InputVar> inargs = null) {
			TypeHierarchy.Environ cur = tracking.GetCurrent();
			TypeHierarchy.Environ glob = tracking.GetGlobal();
			string[] name = FullNameArray(cur, glob, next);
			return Llvm.GetUniqueName(string.Join(".", name), ArgsToStr(inargs));
		}
		public static string FullName(TypeHierarchy.Environ cur, TypeHierarchy.Environ global, string next, List<Stmt.InputVar> inargs = null) {
			string[] name = FullNameArray(cur, global, next);
			return Llvm.GetUniqueName(string.Join(".", name), ArgsToStr(inargs));
		}



		static int num = 0;
		static int offset = 0;
		public static string RefNew() {
			return $"%{offset + num++}";
		}
		public static void RefStartFunc() {
			offset = 0;
			num = 0;
		}
		public static void RefStartBody() {
			offset = 1;
		}



		public static string FloatName(int size) {
			switch (size) {
				case 16:
					return "half";
				case 32:
					return "float";
				case 64:
					return "double";
				case 128:
					return "fp128";
				default:
					throw new CyType.TypeError("Unknown size of float requested.");
			}
		}


		static int indent = 0;
		// Add indentation to a line of code and a newline at end
		public static string Indent(string line) {
			StringBuilder str = new StringBuilder();
			if (line.Contains("}"))
				--indent;
			for (int i = 0; i < indent; i++)
				str.Append("  ");
			str.Append(line);
			if (line.Contains("{"))
				indent++;
			return str.ToString();
		}
	
	} // Llvm


}

