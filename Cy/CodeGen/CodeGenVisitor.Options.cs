using Cy.CodeGen.Llvm;

using System.Collections.Generic;

namespace Cy.CodeGen;



public partial class CodeGenVisitor {
	public class Options {
		public class Tabs {
			const int TabSize = 2;
			public int CurrentTab;
			public Tabs() {
				CurrentTab = 0;
			}
			public string Show { get => new(' ', CurrentTab * TabSize); }
			public void Inc() {
				CurrentTab++;
			}
			public void Dec() {
				--CurrentTab;
			}
		}

		public InstanceTable Variable;
		public Tabs Tab;
		public LlvmTypes TypesToLlvm;
		public Stack<LlvmType> ReturnType;
		public CodeBuilder Code;
		public Options(LlvmTypes llvmTypes) {
			Tab = new Tabs();
			TypesToLlvm = llvmTypes;
			Code = new CodeBuilder();
			ReturnType = new Stack<LlvmType>();
			Variable = new InstanceTable();
		}

		public static Options GetOptions(object obj) {
			return (Options)obj;
		}
	}
}
