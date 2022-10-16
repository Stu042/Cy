using System.Collections.Generic;

namespace Cy.Types;



/// <summary> All types used, with full name. </summary>
public class TypeTable {
	public Dictionary<string, BaseType> types = new();


	public void Add(BaseType type) {
		types.Add(type.Name, type);
	}

	/// <summary> Lookup type, return null if not found else type definition. </summary>
	public BaseType LookUp(string name, string scope) {
		return null;
	}
}
