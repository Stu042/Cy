using Cy.Enums;

using System.Collections.Generic;

namespace Cy.Types;

public class Type {
	public string Name;
	public AccessModifier Modifier;
	public TypeFormat Format;
	public int BitSize;
	public int ByteSize;

	public Type(string name, AccessModifier modifier, TypeFormat format, int bitSize, int byteSize) {
		Name = name;
		Modifier = modifier;
		Format = format;
		BitSize = bitSize;
		ByteSize = byteSize;
	}
}


public class MethodType : Type {
	public string MethodName;
	public MethodType(string methodName, string typeName, AccessModifier modifier, TypeFormat format, int bitSize, int byteSize)
	: base(typeName, modifier, format, bitSize, byteSize) {
		MethodName = methodName;
	}
}


public class ObjectType : Type {
	Dictionary<string, Type> Children;

	public ObjectType(string name, AccessModifier modifier, TypeFormat format, int bitSize, int byteSize, Dictionary<string, Type> children = null)
	: base(name, modifier, format, bitSize, byteSize) {
		if (children != null) {
			Children = children;
		} else {
			Children = new Dictionary<string, Type>();
		}
	}
	public void AddChild(Type child) {
		Children.Add(child.Name, child);
	}
	public Type GetChild(string name) {
		Children.TryGetValue(name, out Type result);
		return result;
	}
}
