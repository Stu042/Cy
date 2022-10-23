using Cy.Enums;
using Cy.Preprocesor;
using Cy.Preprocesor.Interfaces;
using Cy.Util;

using System.Collections.Generic;
using System.Linq;

namespace Cy.Types;


/// <summary> Create a collection of all type definitions. </summary>
public class TypeTableCreate {
	readonly TypeTableCreateVisitor _visitor;
	readonly TypeTableCreateVisitorOptions _typeTableCreateVisitorOptions;
	readonly IErrorDisplay _errorDisplay;

	readonly BaseType[] builtinTypes = new BaseType[] {
		new BaseType(Constants.BasicTypeNames.Int, AccessModifier.Public, TypeFormat.Int, 32, 4, null),
		new BaseType(Constants.BasicTypeNames.Int8, AccessModifier.Public, TypeFormat.Int, 8, 1, null),
		new BaseType(Constants.BasicTypeNames.Int16, AccessModifier.Public, TypeFormat.Int, 16, 2, null),
		new BaseType(Constants.BasicTypeNames.Int32, AccessModifier.Public, TypeFormat.Int, 32, 4, null),
		new BaseType(Constants.BasicTypeNames.Int64, AccessModifier.Public, TypeFormat.Int, 64, 8, null),
		new BaseType(Constants.BasicTypeNames.Int128, AccessModifier.Public, TypeFormat.Int, 128, 16, null),
		new BaseType(Constants.BasicTypeNames.Float, AccessModifier.Public, TypeFormat.Float, 64, 8, null),
		new BaseType(Constants.BasicTypeNames.Float16, AccessModifier.Public, TypeFormat.Float, 16, 2, null),
		new BaseType(Constants.BasicTypeNames.Float32, AccessModifier.Public, TypeFormat.Float, 32, 4, null),
		new BaseType(Constants.BasicTypeNames.Float64, AccessModifier.Public, TypeFormat.Float, 64, 8, null),
		new BaseType(Constants.BasicTypeNames.Float128, AccessModifier.Public, TypeFormat.Float, 128, 16, null),
		new BaseType(Constants.BasicTypeNames.Bool, AccessModifier.Public, TypeFormat.Bool, 1, 1, null),
		new BaseType(Constants.BasicTypeNames.Void, AccessModifier.Public, TypeFormat.Void, 0, 0, null)
	};

	public TypeTableCreate(TypeTableCreateVisitor visitor, TypeTableCreateVisitorOptions typeTableCreateVisitorOptions, IErrorDisplay errorDisplay) {
		_visitor = visitor;
		_typeTableCreateVisitorOptions = typeTableCreateVisitorOptions;
		_errorDisplay = errorDisplay;
		foreach (var type in builtinTypes) {
			_typeTableCreateVisitorOptions.TypeTableBuilderHelper.Add(type);
		};
	}

	public TypeTable Create(List<List<Stmt>> allFilesStmts) {
		foreach (var stmts in allFilesStmts) {
			foreach (var stmt in stmts) {
				stmt.Accept(_visitor, _typeTableCreateVisitorOptions);
			}
		}
		UpdateForwardDefinitions(_typeTableCreateVisitorOptions.TypeTableBuilderHelper.TypeTable);
		return _typeTableCreateVisitorOptions.TypeTableBuilderHelper.TypeTable;
	}

	public void Display(TypeTable typeTable) {
		ColourConsole.WriteLine("\n//FG_Grey Types:");
		foreach (var type in typeTable.Types) {
			var typeDef = type.Value;
			ColourConsole.WriteLine($"//FG_Cyan {typeDef.Format,-20} //FG_Green {typeDef.Name,-40} //FG_Grey {typeDef.BitSize,5} {typeDef.ByteSize,-4}");
			if (typeDef is ObjectType objectDef) {
				foreach (var child in objectDef.Children) {
					if (child is ObjectChildType objectChildType) {
						if (objectChildType.Format == Enums.TypeFormat.Int && objectChildType.ByteSize == 0) {
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

	void UpdateForwardDefinitions(TypeTable typeTable) {
		foreach (var (name, type) in typeTable.Types) {
			if (type is ObjectType objectType) {
				UpdateForwardDefinitionsRecursive(objectType, typeTable);
			}
		}
	}

	void UpdateForwardDefinitionsRecursive(ObjectType objectType, TypeTable typeTable) {    // calls itself when we have a new object to check
		typeTable.NamespaceHelper.Enter(objectType.Name);
		foreach (var child in objectType.Children) {
			if (child is ObjectType childObjectType) {
				UpdateForwardDefinitionsRecursive(childObjectType, typeTable);      // we need to enter another namespace...
			}
			if (child.Format == TypeFormat.Int && child.ByteSize == 0) {                    // this requires updating as will its parents
				UpdateDefinitions(objectType, child as ObjectChildType, typeTable);
			}
		}
		typeTable.NamespaceHelper.Leave();
	}

	void UpdateDefinitions(BaseType parent, ObjectChildType child, TypeTable typeTable) {   // we got a hot one, update this objects parent with the size and its parents as well
		var existingType = typeTable.LookUp(child.Name);
		if (existingType == null) {
			_errorDisplay.Error(child.Token, $"Unable to find definition for {child.Name} in {parent.Name}.");
		} else {
			child.Format = existingType.Format;
			child.BitSize += existingType.BitSize;
			child.ByteSize += existingType.ByteSize;
			var parentName = typeTable.NamespaceHelper.Current.Split(NamespaceHelper.NAMESPACE_DELIMITER).ToList();
			while (parent != null) {
				parent.BitSize += existingType.BitSize;
				parent.ByteSize += existingType.ByteSize;
				parentName.RemoveAt(parentName.Count - 1);
				var parentKey = string.Join(NamespaceHelper.NAMESPACE_DELIMITER, parentName);
				typeTable.Types.TryGetValue(parentKey, out parent);
			}
		}
	}
}
