using Cy.Enums;
using Cy.Preprocesor;

using System.Collections.Generic;

namespace Cy.Types;


public class TypeTableCreate {
	readonly TypeTableCreateVisitor _visitor;
	public Dictionary<string, Type> Types;

	readonly Type[] builtinTypes = new Type[] {
		new Type("int", AccessModifier.Public, TypeFormat.Int, 32, 4),
		new Type("int8", AccessModifier.Public, TypeFormat.Int, 8, 1),
		new Type("int16", AccessModifier.Public, TypeFormat.Int, 16, 2),
		new Type("int32", AccessModifier.Public, TypeFormat.Int, 32, 4),
		new Type("int64", AccessModifier.Public, TypeFormat.Int, 64, 8),
		new Type("int128", AccessModifier.Public, TypeFormat.Int, 128, 16),
		new Type("float", AccessModifier.Public, TypeFormat.Float, 64, 8),
		new Type("float16", AccessModifier.Public, TypeFormat.Float, 16, 2),
		new Type("float32", AccessModifier.Public, TypeFormat.Float, 32, 4),
		new Type("float64", AccessModifier.Public, TypeFormat.Float, 64, 8),
		new Type("float128", AccessModifier.Public, TypeFormat.Float, 128, 16),
		new Type("bool", AccessModifier.Public, TypeFormat.Bool, 1, 1),
		new Type("void", AccessModifier.Public, TypeFormat.Void, 0, 0)
	};

	public TypeTableCreate(TypeTableCreateVisitor visitor) {
		_visitor = visitor;
		Types = new Dictionary<string, Type>();
		foreach (var type in builtinTypes) {
			Types.Add(type.Name, type);
		};
	}

	public void Create(List<List<Stmt>> allFilesStmts) {
		foreach (var stmts in allFilesStmts) {
			foreach (var stmt in stmts) {
				stmt.Accept(_visitor, Types);
			}
		}
	}
}
