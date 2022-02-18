using Cy.Enums;

namespace Cy.Types;


/// <summary>Global SymbolDefinition must have a null parent and be named "".</summary>
public class TypeDefinitionTable {
	public TypeDefinition Global;

	public TypeDefinitionTable() {
		Global = new TypeDefinition("", null, null, -1, AccessModifier.Public, BaseType.VOID, null);
	}
	public TypeDefinition LookTypeUp(string typeName, string currentTypeName) {
		var currentType = Global.LookUpTypeHere(currentTypeName);
		return currentType.LookUpType(typeName);
	}
}
