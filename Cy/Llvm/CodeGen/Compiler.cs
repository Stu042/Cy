using Cy.Llvm.Helpers;
using Cy.Preprocesor;
using Cy.Types;

using System.Collections.Generic;
using System.Diagnostics;

namespace Cy.Llvm.CodeGen;


public class CompileOptions {
	public TypeTable TypeTable;
	public CodeHelper Code;
	public BackendTypeHelper BackendTypeHelper;
}



[DebuggerDisplay("LlvmIr Compiler")]
public class Compiler {
	readonly CompileVisitor.CompileVisitor _compileVisitor;
	readonly TypeTable _typeTable;
	readonly CodeHelper _codeWriter;
	readonly BackendTypeHelper _llvmTypeHelper;


	public Compiler(CompileVisitor.CompileVisitor compileVisitor, TypeTable typeTable, CodeHelper codeWriter, BackendTypeHelper llvmTypeHelper) {
		_compileVisitor = compileVisitor;
		_typeTable = typeTable;
		_codeWriter = codeWriter;
		_llvmTypeHelper = llvmTypeHelper;
	}

	public List<CodeOutput> Compile(List<List<Stmt>> allFilesStmts) {
		var options = new CompileOptions {
			Code = _codeWriter,
			TypeTable = _typeTable,
			BackendTypeHelper = _llvmTypeHelper,
		};
		foreach (var stmts in allFilesStmts) {
			_codeWriter.NewFile(stmts[0].Token.Filename);
			foreach (var stmt in stmts) {
				stmt.Accept(_compileVisitor, options);
			}
		}
		return _codeWriter.Code;
	}
}
