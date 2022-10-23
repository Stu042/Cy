using Cy.Enums;
using Cy.Preprocesor;

using System.Collections.Generic;
using System.Linq;

using static Cy.Preprocesor.Stmt;

namespace Cy.Types;


/// <summary> Create a collection of all type definitions. </summary>
public class TypeTableCreate {
	readonly TypeTableCreateVisitor _visitor;
	readonly TypeTableCreateVisitorOptions _typeTableCreateVisitorOptions;

	readonly BaseType[] builtinTypes = new BaseType[] {
		new BaseType(Constants.BasicTypeNames.Int, AccessModifier.Public, TypeFormat.Int, 32, 4),
		new BaseType(Constants.BasicTypeNames.Int8, AccessModifier.Public, TypeFormat.Int, 8, 1),
		new BaseType(Constants.BasicTypeNames.Int16, AccessModifier.Public, TypeFormat.Int, 16, 2),
		new BaseType(Constants.BasicTypeNames.Int32, AccessModifier.Public, TypeFormat.Int, 32, 4),
		new BaseType(Constants.BasicTypeNames.Int64, AccessModifier.Public, TypeFormat.Int, 64, 8),
		new BaseType(Constants.BasicTypeNames.Int128, AccessModifier.Public, TypeFormat.Int, 128, 16),
		new BaseType(Constants.BasicTypeNames.Float, AccessModifier.Public, TypeFormat.Float, 64, 8),
		new BaseType(Constants.BasicTypeNames.Float16, AccessModifier.Public, TypeFormat.Float, 16, 2),
		new BaseType(Constants.BasicTypeNames.Float32, AccessModifier.Public, TypeFormat.Float, 32, 4),
		new BaseType(Constants.BasicTypeNames.Float64, AccessModifier.Public, TypeFormat.Float, 64, 8),
		new BaseType(Constants.BasicTypeNames.Float128, AccessModifier.Public, TypeFormat.Float, 128, 16),
		new BaseType(Constants.BasicTypeNames.Bool, AccessModifier.Public, TypeFormat.Bool, 1, 1),
		new BaseType(Constants.BasicTypeNames.Void, AccessModifier.Public, TypeFormat.Void, 0, 0)
	};

	public TypeTableCreate(TypeTableCreateVisitor visitor, TypeTableCreateVisitorOptions typeTableCreateVisitorOptions) {
		_visitor = visitor;
		_typeTableCreateVisitorOptions = typeTableCreateVisitorOptions;
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


	void UpdateForwardDefinitions(TypeTable typeTable){
		foreach (var (name,type) in  typeTable.Types) {
			if (type is ObjectType objectType) {
				UpdateForwardDefinitionsRecursive(objectType, typeTable);
			}
		}
	}

	void UpdateForwardDefinitionsRecursive(ObjectType objectType, TypeTable typeTable) {	// calls itself when we have a new object to check
		typeTable.NamespaceHelper.Enter(objectType.Name);
		foreach (var child in objectType.Children) {
			if (child is ObjectType childObjectType) {
				UpdateForwardDefinitionsRecursive(childObjectType, typeTable);		// we need to enter another namespace...
			}
			if (child.Format == TypeFormat.Int && child.ByteSize == 0) {					// this requires updating as will its parents
				UpdateDefinitions(objectType, child, typeTable);
			}
		}
		typeTable.NamespaceHelper.Leave();
	}

	void UpdateDefinitions(BaseType parent, BaseType child, TypeTable typeTable) {	// we got a hot one, update this objects parent with the size and its parents as well
		var existingType = typeTable.LookUp(child.Name);
		child.BitSize += existingType.BitSize;
		child.ByteSize += existingType.ByteSize;
		if (existingType == null) {
			// error this type still doesnt exist even after scanning all files
		} else {
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

