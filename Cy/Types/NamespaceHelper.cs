using System;
using System.Collections.Generic;
using System.Linq;

namespace Cy.Types;

/// <summary>
/// Used to track current fully qualified namespace - helpful for types and scope
/// </summary>
public class NamespaceHelper {
	public const char NAMESPACE_DELIMITER = '.';
	readonly Stack<string> names = new();
	public string Current { get => String.Join(NAMESPACE_DELIMITER, names.Reverse()); }


	/// <summary> Add a name to the current namespace. </summary>
	public void Enter(string name) {
		names.Push(name);
	}

	/// <summary> Remove a name from the current namespace. </summary>
	public void Leave() {
		names.Pop();
	}

	/// <summary> Just the last part of the fullname. </summary>
	public string SimpleName(string fullName) {
		var fullNameSplit = fullName.Split(NAMESPACE_DELIMITER);
		return fullNameSplit[^1];
	}

	/// <summary> With string part, join them to create a name, i.e. AnObject.AnotherObject </summary>
	public string BuildName(IEnumerable<string> parts) {
		var name = String.Join(NAMESPACE_DELIMITER, parts);
		return name;
	}

	public string FullNamePlus(string current) {
		if (string.IsNullOrEmpty(current)) {
			return Current;
		}
		var currentNames = new Stack<string>(names);
		currentNames.Push(current);
		var fullNameSpace = String.Join(NAMESPACE_DELIMITER, currentNames.Reverse());
		return fullNameSpace;
	}
}

