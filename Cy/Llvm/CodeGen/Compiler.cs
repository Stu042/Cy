using Cy.Preprocesor;
using Cy.Types;

using System.Collections.Generic;

namespace Cy.Llvm.CodeGen;


public class Compiler {
	readonly CompileVisitor _compileVisitor;
	readonly TypeTable _typeTable;
	readonly CodeWriter _codeWriter;
	readonly LlvmTypeHelper _llvmTypeHelper;


	public Compiler(CompileVisitor compileVisitor, TypeTable typeTable, CodeWriter codeWriter, LlvmTypeHelper llvmTypeHelper) {
		_compileVisitor = compileVisitor;
		_typeTable = typeTable;
		_codeWriter = codeWriter;
		_llvmTypeHelper = llvmTypeHelper;
	}


	public List<CodeOutput> Compile(List<List<Stmt>> allFilesStmts) {
		var options = new CompileOptions {
			CodeWriter = _codeWriter,
			TypeTable = _typeTable,
			LlvmTypeHelper = _llvmTypeHelper,
		};
		foreach (var stmts in allFilesStmts) {
			_codeWriter.NewFile(stmts[0].Token.Filename);
			foreach (var stmt in stmts) {
				stmt.Accept(_compileVisitor, options);
			}
		}
		var code = _codeWriter.Output();
		return code;
	}
}
