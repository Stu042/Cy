using Cy.Setup;

using System;
using System.Text;

namespace Cy.Preprocesor;

/// <summary>Write TypeDefinitionTable to console.</summary>
public class TypeDefinitionDisplay {
	readonly Config _config;

	public TypeDefinitionDisplay(Config config) {
		_config = config;
	}
	public void DisplayTable(DefinitionTable typeTable) {
		Console.WriteLine("\nSymbol Table:");
		foreach (var typedef in typeTable.Global.Children) {
			DisplayType(typedef);
		}
	}
	void DisplayType(TypeDefinition typedef, int tabCount = 0) {
		Console.WriteLine(new string(' ', _config.TabSize * tabCount) + typedef.UiString());
		foreach (var child in typedef.Children) {
			DisplayType(child, tabCount + 1);
		}
	}
}
