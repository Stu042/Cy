using Cy.Enums;

namespace Cy.Preprocesor;


/// <summary>Global SymbolDefinition must have a null parent and be named "".</summary>
public class TypeTable {
	public SymbolDefinition Types;

	public TypeTable() {
		Types = new SymbolDefinition("", null, null, -1, AccessModifier.Public, false, null);
	}
	public SymbolDefinition LookTypeUp(string typeName, string currentTypeName) {
		var currentType = Types.LookUpTypeHere(currentTypeName);
		return currentType.LookUpType(typeName);
	}
}
