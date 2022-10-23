using System.Collections.Generic;

namespace Cy.Types;


/// <summary> All types used, with full name. </summary>
public class TypeTable {
	public NamespaceHelper NamespaceHelper;
	public Dictionary<string, BaseType> Types = new();


	public TypeTable(NamespaceHelper namespaceHelper) {
		NamespaceHelper = namespaceHelper;
	}


	/// <summary> Lookup type, return null if not found else type definition. </summary>
	public BaseType LookUp(string name) {
		if (Types.ContainsKey(name)) {
			return Types[name];
		}
		var fullName = NamespaceHelper.FullNamePlus(name);
		if (Types.ContainsKey(fullName)) {
			return Types[fullName];
		}
		return null;
	}
}
