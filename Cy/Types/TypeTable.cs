using Cy.Util;

using System.Collections.Generic;

namespace Cy.Types;


/// <summary> All types used, with full name. </summary>
public class TypeTable {
	public NamespaceHelper NamespaceHelper;
	public Dictionary<string, FrontendType> Types = new();


	public TypeTable(NamespaceHelper namespaceHelper) {
		NamespaceHelper = namespaceHelper;
	}


	/// <summary> Lookup type, return null if not found else type definition. </summary>
	public FrontendType LookUp(string name) {
		if (Types.ContainsKey(name)) {
			return Types[name];
		}
		var fullName = NamespaceHelper.FullNamePlus(name);
		if (Types.ContainsKey(fullName)) {
			return Types[fullName];
		}
		return null;
	}

	public static void Display(TypeTable typeTable) {
		ColourConsole.WriteLine("\n//FG_Grey Types:");
		foreach (var type in typeTable.Types) {
			var typeDef = type.Value;
			ColourConsole.WriteLine($"//FG_Cyan {typeDef.Format,-20} //FG_Green {typeDef.Name,-40} //FG_Grey {typeDef.BitSize,5} {typeDef.ByteSize,-4}");
			if (typeDef is ObjectType objectDef) {
				foreach (var child in objectDef.Children) {
					if (child is ObjectChildType objectChildType) {
						if (objectChildType.Format == Enums.FrontendTypeFormat.Int && objectChildType.ByteSize == 0) {
							ColourConsole.WriteLine($"\t//FG_Red {objectChildType.Format,-20} {objectChildType.Name,-20} {objectChildType.Identifier,-11} {objectChildType.BitSize,5} {objectChildType.ByteSize,-4}");
						} else {
							ColourConsole.WriteLine($"\t//FG_Cyan {objectChildType.Format,-20} //FG_Blue {objectChildType.Name,-20} //FG_Green {objectChildType.Identifier,-11} //FG_Grey {objectChildType.BitSize,5} {objectChildType.ByteSize,-4}");
						}
					} else if (child is ObjectType) {
					} else {
						ColourConsole.WriteLine($"\t//FG_Cyan {child.Format,-20} //FG_Green {child.Name,-32} //FG_Grey {child.BitSize,5} {child.ByteSize,-4}");
					}
				}
			}
		}
	}
}
