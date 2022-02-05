using Cy.Setup;

using System;
using System.Text;

namespace Cy.Preprocesor;

public enum AccessModifier {
	Public,
	Private,
	Protected
}


/// <summary>Global SymbolDefinition must have a null parent and be named "".</summary>
public class SymbolTable {
	public SymbolDefinition Types;

	public SymbolTable() {
		Types = new SymbolDefinition("", null, null, -1, AccessModifier.Public, false, null);
	}
	public SymbolDefinition LookUp(string typeName, string currentTypeName) {
		SymbolDefinition currentType = LookUpHere(currentTypeName, Types);
		return LookUp(typeName, currentType);
	}
	public SymbolDefinition LookUp(string typeName, SymbolDefinition currentType) {
		var type = LookUpHere(typeName, currentType);
		if (type == null) {
			type = LookUpHere(typeName, Types);
		}
		return type;
	}
	public SymbolDefinition LookUpHere(string typeName, SymbolDefinition currentType) {
		var typeNameParts = typeName.Split('.');
		foreach (var typeNamePart in typeNameParts) {
			currentType = currentType.Children.Find(curr => curr.TypeName == typeNamePart);
		}
		return currentType;
	}
}




/// <summary>Write SymbolTable to console.</summary>
public class DisplaySymbolTable {
	readonly Config _config;

	public DisplaySymbolTable(Config config) {
		_config = config;
	}
	public void DisplayTable(SymbolTable typeTable) {
		Console.WriteLine("\nSymbol Table:");
		foreach (var typedef in typeTable.Types.Children) {
			DisplayType(typedef);
		}
	}
	void DisplayType(SymbolDefinition typedef, int tabCount = 0) {
		Console.WriteLine(GetTabs(tabCount) + typedef);
		foreach (var child in typedef.Children) {
			DisplayType(child, tabCount + 1);
		}
	}
	string GetTabs(int tabCount) {
		var oneTab = new string(' ', _config.TabSize);
		var tabs = new StringBuilder();
		for (var i = 0; i < tabCount; i++) {
			tabs.Append(oneTab);
		}
		return tabs.ToString();
	}
}
