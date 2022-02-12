using System.Collections.Generic;

namespace Cy.CodeGen;



public partial class CodeGenVisitor {
	public class Options {
		public LlvmTypes TypesToLlvm;
		public int CurrentTab;
		public Stack<LlvmType> ReturnType;
		public Options(LlvmTypes llvmTypes) {
			TypesToLlvm = llvmTypes;
			ReturnType = new Stack<LlvmType>();
			CurrentTab = 0;
		}

		public string Tabs { get => new(' ', CurrentTab * 2); }
		public void IncTab() {
			CurrentTab++;
		}
		public void DecTab() {
			--CurrentTab;
		}
		public static Options GetOptions(object obj) {
			return (Options)obj;
		}
	}
}
