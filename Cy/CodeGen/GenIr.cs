using Cy.Preprocesor;
using Cy.Setup;

using System.Collections.Generic;
using System.Text;

namespace Cy.CodeGen;


public class GenIr {
	readonly CodeGenVisitor _codeGenVisitor;
	readonly Llvm.Foundation _foundation;
	readonly Config _config;
	
	readonly string filename;
	StringBuilder preCode;
	StringBuilder postCode;
	StringBuilder code;

	public GenIr(CodeGenVisitor codeGenVisitor, Llvm.Foundation foundation, Config config){
		_codeGenVisitor = codeGenVisitor;
		_foundation = foundation;
		_config = config;
		filename = _config.FileOut;
		preCode = new StringBuilder();
		postCode = new StringBuilder();
		code = new StringBuilder();
	}

	public string GenerateLlvmIr(List<List<Stmt>> toplevel, DefinitionTable definitionTable) {
		preCode.Append(_foundation.GetPreLLVMCode());
		code.Append(_codeGenVisitor.Run(toplevel));
		postCode.Append(_foundation.GetPostLLVMCode());
		preCode.Append(code);
		preCode.Append(postCode);
		return preCode.ToString();
	}
}
