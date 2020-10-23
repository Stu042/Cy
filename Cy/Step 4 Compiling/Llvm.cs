using System;
using System.Collections.Generic;
using System.Text;


namespace Cy {

	// utils to help working with llvm
	public static class Llvm {

		// string name needs to be fullname, unique name for this method/function/object - as unique as I want it anyway
		public static string GetUniqueName(string name, string[] inputLlvmTypes = null) {
			StringBuilder str = new StringBuilder();
			if (inputLlvmTypes != null) {
				foreach (string t in inputLlvmTypes)
					str.Append(t);
			}
			return name + str.ToString();
		}
		public static string GetUniqueName(string name, List<string> inputLlvmTypes) {
			return GetUniqueName(name, inputLlvmTypes.ToArray());
		}

		public static string[] FullNameArray(Tracking tracking, string next = "") {
			TypeHierarchy.Environ cur = tracking.GetCurrent();
			TypeHierarchy.Environ glob = tracking.GetGlobal();
			List<string> name = new List<string>();
			if (next != "")
				name.Add(next);
			if (cur != glob) {
				do {
					name.Add(cur.name);
					cur = cur.parent;
				} while (cur != glob);
			}
			name.Reverse();
			return name.ToArray();
		}

		// return the full name of a function/object that is defined in the current scope
		// Note: next doesnt need to exist
		public static string FullName(Tracking tracking, string next, string[] inArgTypes = null) {
			string[] name = FullNameArray(tracking, next);
			return Llvm.GetUniqueName(string.Join(".", name), inArgTypes);
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

