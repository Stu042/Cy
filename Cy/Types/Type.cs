using Cy.Enums;
using Cy.Preprocesor;

using System.Collections.Generic;
using System.Linq;

namespace Cy.Types;


/// <summary>Base of all frontend types, basic types will usually be this. </summary>
public class BaseType {
	/// <summary> Full name of type. </summary>
	public string Name;
	/// <summary> Public, private etc. </summary>
	public AccessModifier Modifier;
	/// <summary>Void, int, bool object, etc...</summary>
	public TypeFormat Format;
	/// <summary>Size in bits of type, some maybe 1 bit but still be 1 byte in size as well (hints for storage) </summary>
	public int BitSize;
	/// <summary>Byte size of type, usually used for storage. </summary>
	public int ByteSize;
	/// <summary> Token of type definition. </summary>
	public Token Token;


	public BaseType(string name, AccessModifier modifier, TypeFormat format, int bitSize, int byteSize, Token token) {
		Name = name;
		Modifier = modifier;
		Format = format;
		BitSize = bitSize;
		ByteSize = byteSize;
		Token = token;
	}
}



/// <summary>Base of any method type. </summary>
public class MethodType : BaseType {
	public string MethodName;

	public MethodType(string methodName, string typeName, AccessModifier modifier, TypeFormat format, int bitSize, int byteSize, Token token)
	: base(typeName, modifier, format, bitSize, byteSize, token) {
		MethodName = methodName;
	}
}



/// <summary>Base of all object types, mostly user defined types... </summary>
public class ObjectType : BaseType {
	/// <summary> List of child types in this object (struct) </summary>
	public readonly List<BaseType> Children;


	public ObjectType(string name, AccessModifier modifier, TypeFormat format, int bitSize, int byteSize, Token token, List<BaseType> children = null)
	: base(name, modifier, format, bitSize, byteSize, token) {
		if (children != null) {
			this.Children = children;
		} else {
			this.Children = new List<BaseType>();
		}
	}


	public void AddChild(BaseType child) {
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
public class ObjectChildType : BaseType {
	public string Identifier;


	public ObjectChildType(string identifier, string name, AccessModifier modifier, TypeFormat format, int bitSize, int byteSize, Token token)
	: base(name, modifier, format, bitSize, byteSize, token) {
		Identifier = identifier;
	}
	public ObjectChildType(string identifier, BaseType baseType)
	: base(baseType?.Name, baseType?.Modifier ?? AccessModifier.Public, baseType?.Format ?? TypeFormat.Int, baseType?.BitSize ?? 0, baseType?.ByteSize ?? 0, baseType?.Token) {
		Identifier = identifier;
	}
}
