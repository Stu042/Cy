using Cy.Enums;
using Cy.Preprocesor;
using Cy.Preprocesor.Interfaces;

using System.Collections.Generic;
using System.Linq;

namespace Cy.Types;


/// <summary> Create a collection of all type definitions. </summary>
public class TypeTableCreate {
	readonly TypeTableCreateVisitor _visitor;
	readonly TypeTableCreateVisitorOptions _typeTableCreateVisitorOptions;
	readonly IErrorDisplay _errorDisplay;

	readonly FrontendType[] builtinTypes = new FrontendType[] {
		new FrontendType(Constants.BasicTypeNames.Int, AccessModifier.Public, FrontendTypeFormat.Int, 32, 4, null),
		new FrontendType(Constants.BasicTypeNames.Int8, AccessModifier.Public, FrontendTypeFormat.Int, 8, 1, null),
		new FrontendType(Constants.BasicTypeNames.Int16, AccessModifier.Public, FrontendTypeFormat.Int, 16, 2, null),
		new FrontendType(Constants.BasicTypeNames.Int32, AccessModifier.Public, FrontendTypeFormat.Int, 32, 4, null),
		new FrontendType(Constants.BasicTypeNames.Int64, AccessModifier.Public, FrontendTypeFormat.Int, 64, 8, null),
		new FrontendType(Constants.BasicTypeNames.Int128, AccessModifier.Public, FrontendTypeFormat.Int, 128, 16, null),
		new FrontendType(Constants.BasicTypeNames.Float, AccessModifier.Public, FrontendTypeFormat.Float, 64, 8, null),
		new FrontendType(Constants.BasicTypeNames.Float16, AccessModifier.Public, FrontendTypeFormat.Float, 16, 2, null),
		new FrontendType(Constants.BasicTypeNames.Float32, AccessModifier.Public, FrontendTypeFormat.Float, 32, 4, null),
		new FrontendType(Constants.BasicTypeNames.Float64, AccessModifier.Public, FrontendTypeFormat.Float, 64, 8, null),
		new FrontendType(Constants.BasicTypeNames.Float128, AccessModifier.Public, FrontendTypeFormat.Float, 128, 16, null),
		new FrontendType(Constants.BasicTypeNames.Bool, AccessModifier.Public, FrontendTypeFormat.Bool, 1, 1, null),
		new FrontendType(Constants.BasicTypeNames.Void, AccessModifier.Public, FrontendTypeFormat.Void, 0, 0, null)
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
			if (child.Format == FrontendTypeFormat.Int && child.ByteSize == 0) {                    // this requires updating as will its parents
				UpdateDefinitions(objectType, child as ObjectChildType, typeTable);
			}
		}
		typeTable.NamespaceHelper.Leave();
	}

	void UpdateDefinitions(FrontendType parent, ObjectChildType child, TypeTable typeTable) {   // we got a hot one, update this objects parent with the size and its parents as well
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
