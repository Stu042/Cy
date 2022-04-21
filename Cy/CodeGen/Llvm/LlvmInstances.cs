﻿using Cy.Enums;
using Cy.Setup;
using Cy.TokenGenerator;
using Cy.Types;

using System.Collections.Generic;
using System.Linq;

namespace Cy.CodeGen.Llvm;


public class LlvmInstance {
	public string LlvmName;
	public string LlvmType;
	public string FullyQualifiedTypeName;
	public TypeDefinition TypeDef;
}


/// <summary>Provides instance tracking and creation for LlvmIr code generation.</summary>
public class LlvmInstances {
	/// <summary>FQTypeName points to LlvmInstance.</summary>
	readonly Stack<Dictionary<string, LlvmInstance>> instances;
	/// <summary>Index for current instance name, i.e. %1, %2, etc...</summary>
	readonly Stack<int> instanceCount;
	/// <summary>Minimum size of alignment used.</summary>
	readonly int defaultAlignment;

	public LlvmInstances(Config conf) {
		defaultAlignment = conf.DefaultAlignment;
		instances = new Stack<Dictionary<string, LlvmInstance>>();
		instances.Push(new Dictionary<string, LlvmInstance>());
		instanceCount = new Stack<int>();
		instanceCount.Push(0);
	}

	public void NewBlock() {
		instances.Push(new Dictionary<string, LlvmInstance>());
		instanceCount.Push(0);
	}
	public void EndBlock() {
		instances.Pop();
		instanceCount.Pop();
	}

	public string NewTempInstance() {
		var llvmNameIdx = instanceCount.Pop() + 1;
		instanceCount.Push(llvmNameIdx);
		return $"%{llvmNameIdx}";
	}


	public LlvmInstance NewInstance(bool isLiteral, string textRepresentation, object value, BaseType baseType) {
		string fullName = null;
		if (isLiteral) {
			var inst = new LlvmInstance {
				LlvmName = textRepresentation,
				LlvmType = null,
				FullyQualifiedTypeName = fullName
			};
			instances.Peek().Add(fullName, inst);
			return inst;
		}
		return null;
	}

	public LlvmInstance NewInstance(TypeDefinition typdef) {
		var fullName = typdef.FullyQualifiedName();
		if (GetInstance(fullName) == null) {
			var llvmNameIdx = instanceCount.Pop() + 1;
			instanceCount.Push(llvmNameIdx);
			var inst = new LlvmInstance {
				LlvmName = $"%{llvmNameIdx}",
				LlvmType = GetLlvmType(typdef.Tokens[0].tokenType),
				FullyQualifiedTypeName = fullName,
				TypeDef = typdef
			};
			instances.Peek().Add(fullName, inst);
			return inst;
		}
		return null;
	}

	public LlvmInstance GetInstance(Token[] tokens) {
		var fullName = GetFQInstanceName(tokens);
		return GetInstance(fullName);
	}
	public LlvmInstance GetInstance(string fullName) {
		foreach (var inst in instances) {
			if (inst.TryGetValue(fullName, out LlvmInstance llvmType)) {
				return llvmType;
			}
		}
		return null;
	}

	public static string GetLlvmType(TokenType tokenType) {
		return tokenType switch {
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
			TokenType.VOID => string.Empty,
			TokenType.ASCII => "i8*",
			_ => "i8*"
		};
	}

	int GetAlign(TypeDefinition typdef) {
		var alignment = typdef.ByteSize switch {
			0 => 0,
			1 => 1,
			2 => 2,
			4 => 4,
			8 => 4,
			_ => 4
		};
		if (alignment < defaultAlignment) {
			alignment = defaultAlignment;
		}
		return alignment;
	}

	string GetFQInstanceName(Token[] tokens) {
		string[] parts = tokens.Select(t => t.lexeme).ToArray();
		return string.Join('.', parts);
	}
}
