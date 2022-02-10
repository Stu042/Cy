using Cy.Enums;

namespace Cy.Preprocesor;


/// <summary>Global SymbolDefinition must have a null parent and be named "".</summary>
public class DefinitionTable {
	public TypeDefinition Types;

	public DefinitionTable() {
		Types = new TypeDefinition("", null, null, -1, AccessModifier.Public, false, null);
	}
	public TypeDefinition LookTypeUp(string typeName, string currentTypeName) {
		var currentType = Types.LookUpTypeHere(currentTypeName);
		return currentType.LookUpType(typeName);
	}
}
