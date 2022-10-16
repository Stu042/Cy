using Cy.Enums;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Cy.Types;

/* Types have differing formats:

	Basic types:
		i8, int, float, f64
	Method Type:
		int FunctionName(i32 param1, string[] param2)
	Objects:
		ObjectName {
			i16 property1
			f32 property2
			string property3
			ObjectName2 object
		}
*/


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

	public BaseType(string name, AccessModifier modifier, TypeFormat format, int bitSize, int byteSize) {
		Name = name;
		Modifier = modifier;
		Format = format;
		BitSize = bitSize;
		ByteSize = byteSize;
	}
}



/// <summary>Base of any method type. </summary>
public class MethodType : BaseType {
	public string MethodName;

	public MethodType(string methodName, string typeName, AccessModifier modifier, TypeFormat format, int bitSize, int byteSize)
	: base(typeName, modifier, format, bitSize, byteSize) {
		MethodName = methodName;
	}
}



/// <summary>Base of all object types, mostly user defined types... </summary>
public class ObjectType : BaseType {
	/// <summary> List of child types in this object (struct) </summary>
	readonly List<BaseType> children;

	public ObjectType(string name, AccessModifier modifier, TypeFormat format, int bitSize, int byteSize, List<BaseType> children = null)
	: base(name, modifier, format, bitSize, byteSize) {
		if (children != null) {
			this.children = children;
		} else {
			this.children = new List<BaseType>();
		}
	}


	public int ChildCount() {
		return children.Count;
	}

	public BaseType GetChildAtIndex(int idx) {
		return children[idx];
	}

	public void AddChild(BaseType child) {
		children.Add(child);
		BitSize += child.BitSize;
		ByteSize += child.ByteSize;
	}

	/// <summary> Return child property or default if it doesnt exist here. </summary>
	public ObjectChildType GetChild(string name) {
		var child = children.FirstOrDefault(child => child.Name == name);
		return child as ObjectChildType;
	}

	/// <summary> Return index of property or -1 if it doesnt exist here. </summary>
	public int ChildIndex(string name) {
		int index = children.FindIndex(child => child.Name == name);
		return index;
	}
}

/// <summary> ObjectTypes children, members and functions? </summary>
public class ObjectChildType : BaseType {
	public string Identifier;


	public ObjectChildType(string identifier, string name, AccessModifier modifier, TypeFormat format, int bitSize, int byteSize)
	: base(name, modifier, format, bitSize, byteSize) {
		Identifier = identifier;
	}
	public ObjectChildType(string identifier, BaseType baseType)
	: base(baseType.Name, baseType.Modifier, baseType.Format, baseType.BitSize, baseType.ByteSize) {
		Identifier = identifier;
	}
}
