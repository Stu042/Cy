using Cy.Enums;
using Cy.Preprocesor;

using System.Collections.Generic;

namespace Cy.Types;


/// <summary> Create a collection of all type definitions. </summary>
public class TypeTableCreate {
	readonly TypeTableCreateVisitor _visitor;
	readonly TypeTable _typeTable;

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

	public TypeTableCreate(TypeTable typeTable, TypeTableCreateVisitor visitor) {
		_typeTable = typeTable;
		_visitor = visitor;
		foreach (var type in builtinTypes) {
			_typeTable.Add(type);
		};
	}

	public TypeTable Create(List<List<Stmt>> allFilesStmts) {
		foreach (var stmts in allFilesStmts) {
			foreach (var stmt in stmts) {
				stmt.Accept(_visitor, _typeTable);
			}
		}
		return _typeTable;
	}
}

