using System.Collections.Generic;

namespace Cy.Types;



/// <summary> All types used, with full name. </summary>
public class TypeTable {
	readonly NamespaceHelper _namespaceHelper;
	public Dictionary<string, BaseType> types = new();


	public TypeTable(NamespaceHelper namespaceHelper) {
		_namespaceHelper = namespaceHelper;
	}

	public void Add(BaseType type) {
		types.Add(type.Name, type);
	}

	/// <summary> Lookup type, return null if not found else type definition. </summary>
	public BaseType LookUp(string name) {
		if (types.ContainsKey(name)) {
			return types[name];
		}
		var simpleName = _namespaceHelper.SimpleName(name);
		if (types.ContainsKey(simpleName)) {
			return types[simpleName];
		}
		return null;
	}
}
