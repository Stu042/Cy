using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cy.CodeGen;

/// <summary>Contains the generated LLVMIR for the source code.</summary>
/// <remarks>Allows for:
/// <list type="bullet">
///    <item><description>Three main code segments per block of code, allocate, assign and build.</description></item>
///    <item><description>Blocks within functions and other blocks.</description></item>
///    <item><description>Code within each block cannot affect or change code in another block.</description></item>
/// </list>
/// </remarks>
public class CodeBuilder {
	readonly Stack<CodeWriter> writer;
	readonly List<Stack<CodeWriter>> allCode;
	Stack<CodeWriter> savedWriters;
	public CodeBuilder() {
		writer = new Stack<CodeWriter>();
		writer.Push(new CodeWriter());
		allCode = new List<Stack<CodeWriter>>();
		savedWriters = new Stack<CodeWriter>();
	}

	public void Allocate(string code) {
		writer.Peek().Allocate(code);
	}
	public void Assign(string code) {
		writer.Peek().Assign(code);
	}
	public void Build(string code) {
		writer.Peek().Build(code);
	}

	public void NewFunction() {
		writer.Push(new CodeWriter());
	}
	public void EndFunction() {
		savedWriters.Push(writer.Pop());
		if (writer.Count == 1) {
			allCode.Add(savedWriters);
			savedWriters = new Stack<CodeWriter>();
		}
	}
	public void EnterCodeBlock() {
		writer.Push(new CodeWriter());
	}
	public void LeaveCodeBlock() {
		savedWriters.Push(writer.Pop());
	}
	
	public string Code() {
		var code = new StringBuilder();
		code.Append(writer.Peek().Code());
		var innerCode = allCode.Select(ac => ac.Peek().Code());
		foreach (var ic in innerCode) {
			code.Append(ic);
		}
		return code.ToString();
	}
}


public class CodeWriter {
	readonly StringBuilder allocator;
	readonly StringBuilder assigner;
	readonly StringBuilder builder;

	public CodeWriter() {
		allocator = new StringBuilder();
		assigner = new StringBuilder();
		builder = new StringBuilder();
	}

	public void Allocate(string code) {
		allocator.AppendLine(code);
	}
	public void Assign(string code) {
		assigner.AppendLine(code);
	}
	public void Build(string code) {
		builder.AppendLine(code);
	}

	public string Code() {
		return allocator.ToString() + assigner.ToString() + builder.ToString();
	}
}
