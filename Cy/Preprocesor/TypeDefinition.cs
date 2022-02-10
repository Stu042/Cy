using Cy.Enums;

using System.Collections.Generic;

namespace Cy.Preprocesor;

/// <summary>Info for a type/namespace/object/method (an object).
/// Global type table must have a null parent and be named "".</summary>
public class TypeDefinition {
	/// <summary>Name given in source code, last part of fully qualified name.</summary>
	public string TypeName;
	/// <summary>Name given in source code for this instance.</summary>
	public string InstanceName;
	/// <summary>Namespace this belongs to, or null if this is the global namespace.</summary>
	public TypeDefinition Parent;
	public List<TypeDefinition> Children;
	/// <summary>Is symbol Public, Private or Protected.</summary>
	public AccessModifier Modifier;
	/// <summary>Is this object a functional object.</summary>
	public bool IsFunctional;
	/// <summary>Size (in bytes) of type.</summary>
	public int Size;
	/// <summary>Offset (in bytes) of type.</summary>
	public int Offset;
	/// <summary>Token information.</summary>
	public Token[] Tokens;
	public bool IsMember { get { return !IsFunctional; } }
	public bool IsInstance { get { return InstanceName != null; } }


	public TypeDefinition(string typeName, string instanceName, TypeDefinition parent, int size, AccessModifier accessModifier, bool isFunctional, Token[] tokens) {
		TypeName = typeName;
		InstanceName = instanceName;
		Parent = parent;
		Children = new List<TypeDefinition>();
		Modifier = accessModifier;
		IsFunctional = isFunctional;
		Tokens = tokens;
		Size = size;
	}

	public string FullyQualifiedName() {
		var nameParts = new List<string>();
		var cur = this;
		while (cur.Parent != null) {
			nameParts.Add(cur.TypeName);
			cur = cur.Parent;
		}
		nameParts.Reverse();
		return string.Join('.', nameParts);
	}

	public TypeDefinition GetGlobalNamespace() {
		var currentType = this;
		while (currentType.Parent != null) {
			currentType = currentType.Parent;
		}
		return currentType;
	}

	public TypeDefinition LookUpType(string typeName) {
		var type = LookUpTypeHere(typeName);
		if (type == null) {
			var globalType = GetGlobalNamespace();
			type = globalType.LookUpTypeHere(typeName);
		}
		return type;
	}
	public TypeDefinition LookUpTypeHere(string typeName) {
		TypeDefinition currentType = this;
		var typeNameParts = typeName.Split('.');
		foreach (var typeNamePart in typeNameParts) {
			currentType = currentType.Children.Find(curr => curr.TypeName == typeNamePart);
		}
		return currentType;
	}

	public string UiString() {
		var isFunction = IsFunctional ? "()" : "";
		if (IsInstance) {
			return $"{Modifier}, {TypeName} {InstanceName}{isFunction}, {Offset} {Size}";
		}
		return $"{Modifier}, {TypeName}, {Offset} {Size}";
	}
}
