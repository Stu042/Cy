using System;
using System.Collections.Generic;
using System.Linq;

namespace Cy.Types;

/// <summary>
/// Used to track current fully qualified namespace - helpful for types and scope
/// </summary>
public class NamespaceHelper {
	const char NAMESPACE_DELIMITER = '.';
	readonly Stack<string> names = new();
	public string Current { get => String.Join(NAMESPACE_DELIMITER, names); }


	/// <summary> Add a name to the current namespace. </summary>
	public void Enter(string name) {
		names.Push(name);
	}

	/// <summary> Remove a name from the current namespace. </summary>
	public void Leave() {
		names.Pop();
	}

	/// <summary> Full name of namespace, dot delimited. </summary>
	public string FullName(string currentName) {
		var fullName = new string[] { Current, currentName };
		var fullNameSpace = String.Join(NAMESPACE_DELIMITER, fullName);
		return fullNameSpace;
	}

	/// <summary> Just the last part of the fullname. </summary>
	public string SimpleName(string fullName) {
		var fullNameSplit = fullName.Split(NAMESPACE_DELIMITER);
		return fullNameSplit[^1];
	}

	/// <summary> With string part, join them to create a name, i.e. AnObject.AnotherObject </summary>
	public string CreateName(IEnumerable<string> parts) {
		var fullName = String.Join(NAMESPACE_DELIMITER, parts);
		return fullName;
	}
}

