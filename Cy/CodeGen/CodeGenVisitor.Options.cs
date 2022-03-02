using Cy.CodeGen.Llvm;
using Cy.Setup;
using Cy.Types;

using System.Collections.Generic;

namespace Cy.CodeGen;

public partial class CodeGenVisitor {

	/// <summary>
	/// Helper class to provide multiple utilities that require state within CodeGenVisitor.
	/// </summary>
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

		public Tabs Tab;
		public LlvmInstances TypesToLlvm;
		public Stack<LlvmInstance> ReturnType;
		public CodeBuilder Code;
		public TypeDefinitionTable TypeDefTable;
		public Options(TypeDefinitionTable typeDefTable, Config conf) {
			Tab = new Tabs();
			TypesToLlvm = new LlvmInstances(conf);
			Code = new CodeBuilder();
			ReturnType = new Stack<LlvmInstance>();
			TypeDefTable = typeDefTable;
		}

		public static Options GetOptions(object obj) {
			return (Options)obj;
		}
	}
}
