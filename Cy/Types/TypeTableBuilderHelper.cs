namespace Cy.Types;



/// <summary> All types used, with full name. </summary>
public class TypeTableBuilderHelper {
	public TypeTable TypeTable;


	public TypeTableBuilderHelper(TypeTable typeTable) {
		TypeTable = typeTable;
	}


	/// <summary> Add a new type to the table. </summary>
	public void Add(FrontendType type) {
		TypeTable.Types.Add(type.Name, type);
	}

	/// <summary> Lookup type, return null if not found else type definition. </summary>
	public FrontendType LookUp(string name) {
		var result = TypeTable.LookUp(name);
		return result;
	}
}
