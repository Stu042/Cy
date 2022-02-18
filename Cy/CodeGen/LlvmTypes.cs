using Cy.TokenGenerator;
using Cy.Types;

using System;
using System.Collections.Generic;
using System.Linq;

namespace Cy.CodeGen;

public class LlvmTypes {

	/// <summary>FQTypeName points to LlvmType.</summary>
	Dictionary<string, LlvmType> TypeLookup;
	public LlvmTypes(TypeDefinitionTable definitionTable) {
		TypeLookup = new Dictionary<string, LlvmType>();
		AddChildrenToTable(definitionTable.Global);
	}

	void AddChildrenToTable(TypeDefinition parent) {
		foreach (var typdef in parent.Children) {
			string llvmName = GetLlvmType(typdef);
			string fullName = typdef.FullyQualifiedName();
			if (!TypeLookup.ContainsKey(fullName)) {
				TypeLookup.Add(fullName, new LlvmType {
					LlvmTypeName = llvmName,
					FullyQualifiedTypeName = fullName,
					TypeDef = typdef
				});
			}
			if (typdef.Children.Count > 0) {
				AddChildrenToTable(typdef);
			}
		}
	}

	public string GetLlvmType(TypeDefinition typedef) {
		return typedef.Tokens[0].tokenType switch {
			TokenType.BOOL => "i1",
			TokenType.INT or TokenType.INT32 => "i32",
			TokenType.INT8 => "i8",
			TokenType.INT16 => "i16",
			TokenType.INT64 => "i64",
			TokenType.INT128 => "i128",
			TokenType.FLOAT or TokenType.FLOAT64 => "double",
			TokenType.FLOAT16 => "half",
			TokenType.FLOAT32 => "float",
			// x86_fp80
			TokenType.FLOAT128 => "fp128",
			TokenType.VOID => String.Empty,
			TokenType.ASCII => "i8*",
			_ => "i8*"
		};
	}

	public LlvmType GetType(Token[] tokens) {
		if (TypeLookup.TryGetValue(GetFQTypeName(tokens), out LlvmType llvmType)) {
			return llvmType;
		}
		return null;
	}

	string GetFQTypeName(Token[] tokens) {
		string[] parts = tokens.Select(t => t.lexeme).ToArray();
		return String.Join('.', parts);
	}
}
