using Cy.Preprocesor;
using Cy.Setup;

using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Cy.CodeGen;


public class GenIr {
	readonly CodeGenVisitor _codeGenVisitor;
	readonly Llvm.Foundation _foundation;
	readonly Config _config;
	
	readonly string filename;
	readonly StringBuilder preCode;
	readonly StringBuilder postCode;
	readonly StringBuilder code;

	public GenIr(CodeGenVisitor codeGenVisitor, Llvm.Foundation foundation, Config config){
		_codeGenVisitor = codeGenVisitor;
		_foundation = foundation;
		_config = config;
		filename = _config.FileOut;
		preCode = new StringBuilder();
		postCode = new StringBuilder();
		code = new StringBuilder();
	}

	public string GenerateLlvmIr(List<List<Stmt>> toplevel, TypeDefinitionTable definitionTable) {
		var llvmTypes = new LlvmTypes(definitionTable);
		preCode.Append(_foundation.GetPreLLVMCode());
		code.Append(_codeGenVisitor.Run(toplevel, llvmTypes));
		postCode.Append(_foundation.GetPostLLVMCode());
		preCode.Append(code);
		preCode.Append(postCode);
		File.WriteAllText(filename, preCode.ToString());
		return preCode.ToString();
	}
}
