using Cy.Llvm.Helpers;
using Cy.Util;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;


namespace Cy.Llvm.CodeGen;



/// <summary> Result from the CodeWriter, returned by Compiler. </summary>
public class CodeOutput {
	public FileNames FileName;
	public string Code;

}




/// <summary> Creates the LlvmIr code representing the cy code.  </summary>
[DebuggerDisplay("LlvmIr CodeWriter")]
public class CodeHelper {
	readonly LlvmMacros _llvmMacros;
	readonly SymbolTable _symbolTable;
	readonly ScopeHelper _scopeHelper;
	readonly List<CodeFile> files;
	CodeFile currentFile;
	int indentation;

	public string BackendScope { get => _scopeHelper.BackendScope; }
	public string FrontendScope { get => _scopeHelper.FrontendScope; }
	public List<CodeOutput> Code { get => Output(); }   // mostly for easier debug - shows code so far


	public CodeHelper(LlvmMacros llvmMacros, SymbolTable symbolTable, ScopeHelper scopeHelper) {
		_llvmMacros = llvmMacros;
		_ = _llvmMacros.Setup().Result;
		files = new List<CodeFile>();
		currentFile = null;
		indentation = 0;
		_symbolTable = symbolTable;
		_scopeHelper = scopeHelper;
		_scopeHelper.Reset();
	}

	public void EnterScope(string frontendScopeName) {
		_scopeHelper.Enter(frontendScopeName);
	}
	public void ExitScope() {
		_symbolTable.ExitScope(_scopeHelper.FrontendScope);
		_scopeHelper.Exit();
	}

	/// <summary> Add a new instance to the symbol table </summary>
	public void AddInstance(Instance instance) {
		instance.BackendScope = _scopeHelper.BackendScope;
		instance.FrontendScope = _scopeHelper.FrontendScope;
		_symbolTable.AddInstance(instance);
	}
	/// <summary> Return last instance </summary>
	public Instance LastInstance() {
		Instance instance = _symbolTable.Instances.Last();
		return instance;
	}


	public void Indent() {
		indentation++;
	}
	public void Dedent() {
		--indentation;
	}

	/// <summary> A new file of code. </summary>
	public void NewFile(string fullFilename) {
		currentFile = new CodeFile(fullFilename, _llvmMacros);
		files.Add(currentFile);
	}
	/// <summary> A new function of code for this file. </summary>
	public void NewFunction() {
		currentFile.NewFunction();
	}
	/// <summary> A new block of code for this file. </summary>
	public void NewBlock() {
		currentFile.NewBlock();
	}

	/// <summary> Add line of code to start of a function, i.e. before AddCode(). </summary>
	public void AddPreCode(string line) {
		currentFile.AddPreCode(Indentation() + line);
	}
	/// <summary> Add a line of code to main part of block. </summary>
	public void AddCode(string line) {
		currentFile.AddCode(Indentation() + line);
	}

	/// <summary> Create a unique label name to be used for this code file. </summary>
	public string Label() {
		return currentFile.LabelHelper.NewLabel();
	}

	/// <summary> Create a unique instance name to be used for this code file. </summary>
	public string InstanceName() {
		return currentFile.NewName();
	}
	public void InstanceNameInc() {
		currentFile.InstanceNameInc();
	}



	string Indentation() {
		return new string(' ', indentation * 2);
	}

	/// <summary> Return all files of code in llvmir format. </summary>
	List<CodeOutput> Output() {
		var allCodeOutput = new List<CodeOutput>();
		foreach (var file in files) {
			var codeOutput = new CodeOutput {
				FileName = file.FullFilename,
				Code = file.Output(),
			};
			allCodeOutput.Add(codeOutput);
		}
		return allCodeOutput;
	}



	// a file
	[DebuggerDisplay("LlvmIr CodeFile")]
	class CodeFile {
		public readonly LabelHelper LabelHelper;
		public FileNames FullFilename;
		List<CodeFunction> functions;
		CodeFunction currentFunction;
		LlvmMacros _llvmMacros;

		public CodeFile(string fullFilename, LlvmMacros llvmMacros) {
			LabelHelper = new LabelHelper();
			currentFunction = new CodeFunction();
			_llvmMacros = llvmMacros;
			functions = new List<CodeFunction> {
				currentFunction
			};
			FullFilename = new FileNames(fullFilename);
		}


		public void NewFunction() {
			currentFunction = new CodeFunction();
			functions.Add(currentFunction);
		}
		public void NewBlock() {
			currentFunction.NewBlock();
		}
		public void AddPreCode(string code) {
			currentFunction.AddPreCode(code);
		}
		public void AddCode(string line) {
			currentFunction.AddCode(line);
		}

		public string NewName() {
			return currentFunction.InstanceHelper.NewName();
		}
		public void InstanceNameInc() {
			currentFunction.InstanceHelper.InstanceNameInc();
		}

		public string Output() {
			var bob = new StringBuilder();
			bob.AppendLine(Header());
			foreach (var function in functions) {
				bob.AppendLine(function.Output());
			}
			bob.AppendLine(Footer());
			return bob.ToString();
		}

		string Header() {    // todo get target triple using a better method
			return $"; ModuleID = '{FullFilename.Cy}'\r\nsource_filename = \"{FullFilename.Cy}\"\n\n{_llvmMacros.Header()}";
		}
		string Footer() {    // todo get target triple using a better method
			return _llvmMacros.Footer();
		}
	}



	// a function
	[DebuggerDisplay("LlvmIr CodeFunction")]
	class CodeFunction {
		public readonly InstanceHelper InstanceHelper;
		readonly List<string> preCode;
		readonly List<CodeBlock> blocks;
		CodeBlock currentBlock;

		public CodeFunction() {
			InstanceHelper = new InstanceHelper();
			preCode = new List<string>();
			currentBlock = new CodeBlock();
			blocks = new List<CodeBlock> {
				currentBlock
			};
		}


		public void NewBlock() {
			currentBlock = new CodeBlock();
			blocks.Add(currentBlock);
		}
		public void AddPreCode(string code) {
			preCode.Add(code);
		}
		public void AddCode(string line) {
			currentBlock.AddCode(line);
		}

		public string Output() {
			var bob = new StringBuilder();
			var pre = String.Join('\n', preCode);
			bob.AppendLine(pre);
			foreach (var block in blocks) {
				var blockText = block.Output();
				bob.AppendLine(blockText);
			}
			return bob.ToString();
		}
	}



	// a block of block
	[DebuggerDisplay("LlvmIr CodeBlock")]
	class CodeBlock {
		readonly List<CodeLine> code;


		public CodeBlock() {
			code = new List<CodeLine>();
		}


		public void AddCode(string line) {
			code.Add(new CodeLine { Line = line });
		}

		public string Output() {
			var str = BuildOutput(this);
			return str;
		}

		string BuildOutput(CodeBlock block) {
			var bob = new StringBuilder();
			foreach (var c in block.code) {
				if (c.Block != null) {
					var blockStr = BuildOutput(c.Block);
					bob.AppendLine(blockStr);
				} else {
					bob.AppendLine(c.Line);
				}
			}
			return bob.ToString();
		}
	}



	// either a line or a block of code
	[DebuggerDisplay("LlvmIr CodeLine")]
	class CodeLine {
		public CodeBlock Block;
		public string Line;

		public CodeLine(string code = null, CodeBlock block = null) {
			Block = block;
			Line = code;
		}
	}
}
