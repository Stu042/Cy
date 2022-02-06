using Cy.Setup;

using System;
using System.Text;

namespace Cy.Preprocesor;

/// <summary>Write SymbolTable to console.</summary>
public class SymbolTableDisplay {
	readonly Config _config;

	public SymbolTableDisplay(Config config) {
		_config = config;
	}
	public void DisplayTable(SymbolTable typeTable) {
		Console.WriteLine("\nSymbol Table:");
		foreach (var typedef in typeTable.Types.Children) {
			DisplayType(typedef);
		}
	}
	void DisplayType(SymbolDefinition typedef, int tabCount = 0) {
		Console.WriteLine(GetTabs(tabCount) + typedef.UiString());
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
