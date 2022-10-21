using System.Collections.Generic;

namespace Cy.Types;



/// <summary> All types used, with full name. </summary>
public class TypeTable {
	public NamespaceHelper NamespaceHelper;
	public Dictionary<string, BaseType> types = new();


	public TypeTable(NamespaceHelper namespaceHelper) {
		NamespaceHelper = namespaceHelper;
	}

	public void Add(BaseType type) {
		types.Add(type.Name, type);
	}

	/// <summary> Lookup type, return null if not found else type definition. </summary>
	public BaseType LookUp(string name) {
		if (types.ContainsKey(name)) {
			return types[name];
		}
		var simpleName = NamespaceHelper.FullNamePlus(name);
		if (types.ContainsKey(simpleName)) {
			return types[simpleName];
		}
		return null;
	}
}
