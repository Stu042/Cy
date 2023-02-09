using Cy.Types;

using System.Collections.Generic;


namespace Cy.Llvm.CodeGen;


/// <summary> Keep track of instances. </summary>
public class SymbolTable {
	public List<Instance> Instances;

	
	public SymbolTable() {
		Instances = new List<Instance>();
	}


	public void ExitScope(string currentBackendScope) {
		for (var idx = 0; idx < Instances.Count; idx++) {
			if (Instances[idx].BackendScope == currentBackendScope) {
				Instances.RemoveAt(idx);
				--idx;
			}
		}
	}

	public void AddInstance(Instance instance) {
		Instances.Add(instance);
	}
}



public class Instance {
	/// <summary> Cy name of instance </summary>
	public string FrontendName;
	/// <summary> Frontend Type </summary>
	public FrontendType FrontendType;
	/// <summary> namespace.object.function name, etc </summary>
	public string FrontendScope;
	/// <summary> LlvmIr name of instance, ie %0, %1 </summary>
	public string BackendName;
	/// <summary> LlvmIr type </summary>
	public string BackendType;
	/// <summary> blank or the name of the function we are in (maybe structs as well) </summary>
	public string BackendScope;
	/// <summary> how many * we need in the backend to refer to this </summary>
	public int IndirectionLevels;
}

public class ExpressionInstance : Instance {
}
public class FunctionInstance : Instance {
}

