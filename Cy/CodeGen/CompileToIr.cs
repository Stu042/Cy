using Cy.Parsing;
using Cy.Setup;
using Cy.Types;

using System.IO;
using System.Text;

namespace Cy.CodeGen;


public class CompileToIr {
	readonly CodeGenVisitor _codeGenVisitor;
	readonly Llvm.Foundation _foundation;
	readonly Config _config;
	
	readonly string filename;

	public CompileToIr(CodeGenVisitor codeGenVisitor, Llvm.Foundation foundation, Config config){
		_codeGenVisitor = codeGenVisitor;
		_foundation = foundation;
		_config = config;
		filename = _config.FileOut;
	}

	public string GenerateLlvmIr(Stmt[][] toplevel, TypeDefinitionTable definitionTable) {
		var preCode = _foundation.GetPreLLVMCode();
		var generatedCode = _codeGenVisitor.Run(toplevel, definitionTable, _config);
		var postCode = _foundation.GetPostLLVMCode();
		var code = new StringBuilder();
		code.Append(preCode);
		code.Append(generatedCode);
		code.Append(postCode);
		File.WriteAllText(filename, code.ToString());
		return code.ToString();
	}
}
