using Cy.Util;

using System;
using System.Collections.Generic;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Text;


namespace Cy.Llvm.CodeGen;


public class InstanceHelper {
	int instanceCount;

	public InstanceHelper() {
		instanceCount = 0;
	}

	public string NewName() {
		var newName = $"%{instanceCount++}";
		return newName;
	}
}


public class LabelHelper {
	int labelCount;

	public LabelHelper() {
		labelCount = 0;
	}

	public string NewLabel() {
		var newName = $"{labelCount++}:";
		return newName;
	}
}



public class CodeOutput {
	public string FileName;
	public string Code;
}


public class CodeWriter {
	readonly List<CodeFile> files;
	CodeFile currentFile;


	public CodeWriter() {
		files = new List<CodeFile>();
		currentFile = null;
	}


	/// <summary> A new file of code. </summary>
	public void NewFile(string fullFilename) {
		currentFile = new CodeFile(fullFilename);
		files.Add(currentFile);
	}
	/// <summary> A new block of code for this file. </summary>
	public void NewBlock() {
		currentFile.NewBlock();
	}
	/// <summary> Add line of code to block init. </summary>
	public void AddPreCode(string line) {
		currentFile.AddPreCode(line);
	}
	/// <summary> Add a line of code to main part of block. </summary>

	public void AddCode(string line) {
		currentFile.AddCode(line);
	}
	/// <summary> Add a line of code to block tidy up. </summary>
	public void AddPostCode(string line) {
		currentFile.AddPostCode(line);
	}

	public string Label() {
		return currentFile.LabelHelper.NewLabel();
	}

	public string Instance() {
		return currentFile.InstanceHelper.NewName();
	}

	/// <summary> Return all files of code in llvmir format. </summary>
	public List<CodeOutput> Output() {
		var allCodeOutput = new List<CodeOutput>();
		foreach (var file in files) {
			var codeOutput = new CodeOutput {
				FileName = file.FullFilename,
				Code = file.Output()
			};
			allCodeOutput.Add(codeOutput);
		}
		return allCodeOutput;
	}



	public class CodeFile {
		public readonly InstanceHelper InstanceHelper;
		public readonly LabelHelper LabelHelper;
		public string FullFilename;
		List<CodeBlock> blocks;
		CodeBlock currentBlock;


		public CodeFile(string fullFilename) {
			InstanceHelper = new InstanceHelper();
			LabelHelper = new LabelHelper();
			blocks = new List<CodeBlock>();
			currentBlock = new CodeBlock();
			blocks.Add(currentBlock);
			FullFilename = fullFilename;
		}


		public void NewBlock() {
			currentBlock = new CodeBlock();
			blocks.Add(currentBlock);
		}
		public void AddPreCode(string code) {
			currentBlock.AddPreCode(code);
		}
		public void AddCode(string line) {
			currentBlock.AddCode(line);
		}
		public void AddPostCode(string code) {
			currentBlock.AddPostCode(code);
		}

		public string Output() {
			var bob = new StringBuilder();
			bob.AppendLine(Header());
			foreach (var block in blocks) {
				bob.AppendLine(block.Output());
			}
			bob.AppendLine(Footer());
			return bob.ToString();
		}

		string Header() {    // todo get target triple using a better method
			return $"; ModuleID = '{FullFilename}'\nsource_filename = \"{FullFilename}\"\ntarget datalayout = \"e-m:w-p270:32:32-p271:32:32-p272:64:64-i64:64-f80:128-n8:16:32:64-S128\"\ntarget triple = \"x86_64-pc-windows-msvc19.33.31630\"\n\n";
		}
		string Footer() {    // todo get target triple using a better method
			return "\n\nattributes #0 = { noinline nounwind optnone uwtable \"correctly-rounded-divide-sqrt-fp-math\"=\"false\" \"disable-tail-calls\"=\"false\" \"frame-pointer\"=\"none\" \"less-precise-fpmad\"=\"false\" \"min-legal-vector-width\"=\"0\" \"no-infs-fp-math\"=\"false\" \"no-jump-tables\"=\"false\" \"no-nans-fp-math\"=\"false\" \"no-signed-zeros-fp-math\"=\"false\" \"no-trapping-math\"=\"true\" \"stack-protector-buffer-size\"=\"8\" \"target-cpu\"=\"x86-64\" \"target-features\"=\"+cx8,+fxsr,+mmx,+sse,+sse2,+x87\" \"unsafe-fp-math\"=\"false\" \"use-soft-float\"=\"false\" }\n\n!llvm.module.flags = !{!0, !1}\n!llvm.ident = !{!2}\n\n!0 = !{i32 1, !\"wchar_size\", i32 2}\n!1 = !{i32 7, !\"PIC Level\", i32 2}\n!2 = !{!\"cy version 0.1.0\"}\n";
		}
	}



	public class CodeBlock {
		readonly List<string> preCode;
		readonly List<CodeLine> code;
		readonly List<string> postCode;


		public CodeBlock() {
			preCode = new List<string>();
			code = new List<CodeLine>();
			postCode = new List<string>();
		}


		public void AddPreCode(string code) {
			preCode.Add(code);
		}
		public void AddCode(string line) {
			code.Add(new CodeLine { Line = line });
		}
		public void AddPostCode(string code) {
			postCode.Add(code);
		}

		public string Output() {
			var str = BuildOutput(this);
			return str;
		}

		string BuildOutput(CodeBlock block) {
			var bob = new StringBuilder();
			var pre = String.Join('\n', preCode);
			bob.AppendLine(pre);
			foreach (var c in block.code) {
				if (c.Block != null) {
					var blockStr = BuildOutput(c.Block);
					bob.AppendLine(blockStr);
				} else {
					bob.AppendLine(c.Line);
				}
			}
			var post = String.Join('\n', postCode);
			bob.AppendLine(post);
			return bob.ToString();
		}
	}



	public class CodeLine {
		public CodeBlock Block;
		public string Line;

		public CodeLine(string code = null, CodeBlock block = null) {
			Block = block;
			Line = code;
		}
	}
}
