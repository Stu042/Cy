using Cy.Enums;
using Cy.Preprocesor;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace Cy.Types;


/// <summary>Base of all frontend types, basic types will usually be this. </summary>
public class FrontendType {
	/// <summary> Full name of type. </summary>
	public string Name;
	/// <summary> Public, private etc. </summary>
	public AccessModifier Modifier;
	/// <summary>Void, int, bool object, etc...</summary>
	public FrontendTypeFormat Format;
	/// <summary>Size in bits of type, some maybe 1 bit but still be 1 byte in size as well (hints for storage) </summary>
	public int BitSize;
	/// <summary>Byte size of type, usually used for storage. </summary>
	public int ByteSize;
	/// <summary> Token of type definition. </summary>
	public Token Token;


	public FrontendType(string name, AccessModifier modifier, FrontendTypeFormat format, int bitSize, int byteSize, Token token) {
		Name = name;
		Modifier = modifier;
		Format = format;
		BitSize = bitSize;
		ByteSize = byteSize;
		Token = token;
	}

	public bool CanCast(FrontendType other) {
		if (Format == FrontendTypeFormat.Method) {
			return false;
		}
		if (Format == FrontendTypeFormat.Object) {
			return false;	// todo object will have some kind of casting permitted
		}
		if (Format == FrontendTypeFormat.Void) {
			return false;
		}
		if (Format == FrontendTypeFormat.String) {
			return false;	// todo will be castable to i8[]
		}
		if (Format == FrontendTypeFormat.Bool) {
			return false;
		}
		// int and float left
		return true;
	}

	public static FrontendType Void() {
		return new FrontendType("void", AccessModifier.Public, FrontendTypeFormat.Void, 0, 0, null);
	}
}



/// <summary>Base of any method type. </summary>
public class MethodType : FrontendType {
	public string MethodName;

	public MethodType(string methodName, string typeName, AccessModifier modifier, FrontendTypeFormat format, int bitSize, int byteSize, Token token)
	: base(typeName, modifier, format, bitSize, byteSize, token) {
		MethodName = methodName;
	}
}



/// <summary>Base of all object types, mostly user defined types... </summary>
public class ObjectType : FrontendType {
	/// <summary> List of child types in this object (struct) </summary>
	public readonly List<FrontendType> Children;


	public ObjectType(string name, AccessModifier modifier, FrontendTypeFormat format, int bitSize, int byteSize, Token token, List<FrontendType> children = null)
	: base(name, modifier, format, bitSize, byteSize, token) {
		if (children != null) {
			this.Children = children;
		} else {
			this.Children = new List<FrontendType>();
		}
	}


	public void AddChild(FrontendType child) {
		Children.Add(child);
		if (child is ObjectType) {
			return;
		}
		BitSize += child.BitSize;
		ByteSize += child.ByteSize;
	}

	/// <summary> Return child property or default if it doesnt exist here. </summary>
	public ObjectChildType GetChild(string name) {
		var child = Children.FirstOrDefault(child => child.Name == name);
		return child as ObjectChildType;
	}
}



/// <summary> ObjectTypes children, members and functions? </summary>
public class ObjectChildType : FrontendType {
	public string Identifier;


	public ObjectChildType(string identifier, string name, AccessModifier modifier, FrontendTypeFormat format, int bitSize, int byteSize, Token token)
	: base(name, modifier, format, bitSize, byteSize, token) {
		Identifier = identifier;
	}
	public ObjectChildType(string identifier, FrontendType baseType)
	: base(baseType?.Name, baseType?.Modifier ?? AccessModifier.Public, baseType?.Format ?? FrontendTypeFormat.Int, baseType?.BitSize ?? 0, baseType?.ByteSize ?? 0, baseType?.Token) {
		Identifier = identifier;
	}
}
