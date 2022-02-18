using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Cy.Types;

namespace Cy.CodeGen.Llvm;

public class Instance {
	public string Name;
	public string SourceName;
	public int Align;

}

public class InstanceTable {
	public class Scope {
		public int variableCount;
		public Scope() {
			variableCount = 1;
		}
	}

	readonly Stack<Scope> scope;
	public InstanceTable() {
		scope = new Stack<Scope>();
	}

	public string NewVariable(string sourceName, TypeDefinition typeDef) {
		var name = '%' + scope.Peek().variableCount.ToString();
		scope.Peek().variableCount++;
		return name;
	}

	public void NewScope() {
		scope.Push(new Scope {
			variableCount = 1
		});
	}
}
