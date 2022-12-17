using Cy.Llvm.Helpers;
using Cy.Util;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;


namespace Cy.Llvm.CodeGen;



/// <summary> Result from the CodeWriter, returned by Compiler. </summary>
public class CodeOutput {
	public FileNames FileName;
	public string Code;

}


/// <summary> Creates the LlvmIr code representing the cy code.  </summary>
[DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
public class CodeWriter {
	readonly List<CodeFile> files;
	CodeFile currentFile;
	public List<CodeOutput> Code { get => Output(); }

	public CodeWriter() {
		files = new List<CodeFile>();
		currentFile = null;
	}


	/// <summary> A new file of code. </summary>
	public void NewFile(string fullFilename) {
		currentFile = new CodeFile(fullFilename);
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
		currentFile.AddPreCode(line);
	}
	/// <summary> Add a line of code to main part of block. </summary>
	public void AddCode(string line) {
		currentFile.AddCode(line);
	}

	/// <summary> Create a unique label name to be used for this code file. </summary>
	public string Label() {
		return currentFile.LabelHelper.NewLabel();
	}

	/// <summary> Create a unique instance name to be used for this code file. </summary>
	public string Instance() {
		return currentFile.NewName();
	}

	/// <summary> Return all files of code in llvmir format. </summary>
	public List<CodeOutput> Output() {
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

	private string GetDebuggerDisplay() {
		return "LlvmIr CodeWriter";
	}


	// a file
	[DebuggerDisplay("LlvmIr CodeFile")]
	class CodeFile {
		public readonly LabelHelper LabelHelper;
		public FileNames FullFilename;
		List<CodeFunction> functions;
		CodeFunction currentFunction;


		public CodeFile(string fullFilename) {
			LabelHelper = new LabelHelper();
			currentFunction = new CodeFunction();
			functions = new List<CodeFunction>();
			functions.Add(currentFunction);
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
			return $"; ModuleID = '{FullFilename.Cy}'\nsource_filename = \"{FullFilename.Cy}\"\ntarget datalayout = \"e-m:w-p270:32:32-p271:32:32-p272:64:64-i64:64-f80:128-n8:16:32:64-S128\"\ntarget triple = \"x86_64-pc-windows-msvc19.33.31630\"\n\n";
		}
		string Footer() {    // todo get target triple using a better method
			return "\n\nattributes #0 = { noinline nounwind optnone uwtable \"correctly-rounded-divide-sqrt-fp-math\"=\"false\" \"disable-tail-calls\"=\"false\" \"frame-pointer\"=\"none\" \"less-precise-fpmad\"=\"false\" \"min-legal-vector-width\"=\"0\" \"no-infs-fp-math\"=\"false\" \"no-jump-tables\"=\"false\" \"no-nans-fp-math\"=\"false\" \"no-signed-zeros-fp-math\"=\"false\" \"no-trapping-math\"=\"true\" \"stack-protector-buffer-size\"=\"8\" \"target-cpu\"=\"x86-64\" \"target-features\"=\"+cx8,+fxsr,+mmx,+sse,+sse2,+x87\" \"unsafe-fp-math\"=\"false\" \"use-soft-float\"=\"false\" }\n\n!llvm.module.flags = !{!0, !1}\n!llvm.ident = !{!2}\n\n!0 = !{i32 1, !\"wchar_size\", i32 2}\n!1 = !{i32 7, !\"PIC Level\", i32 2}\n!2 = !{!\"cy version 0.1.0\"}\n";
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
			blocks = new List<CodeBlock>();
			blocks.Add(currentBlock);
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
