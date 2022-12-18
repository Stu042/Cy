using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Cy.Llvm.CodeGen;

public class LlvmMacros {
	public string DataLayout;
	public string Triple;
	public string[] Attributes;
	public string ModuleFlags;
	public string Ident;
	List<string> macros;
	const string filename = "uniqueNameMin";

	public async Task<bool> Init() {
		string[] lines = {
			"int main() {", "        return 0;", "}"
		};
		await File.WriteAllLinesAsync(filename + ".c", lines);
		CompileMinC();
		var text = File.ReadAllLines(filename + ".ll");
		File.Delete(filename + ".c");
		File.Delete(filename + ".ll");
		DataLayout = text.FirstOrDefault(t => t.StartsWith("target datalayout = "));
		Triple = text.FirstOrDefault(t => t.StartsWith("target triple = "));
		Attributes = text.Where(t => t.StartsWith("attributes #")).ToArray();
		ModuleFlags = text.FirstOrDefault(t => t.StartsWith("!llvm.module.flags = !{!0"));
		Ident = text.FirstOrDefault(t => t.StartsWith("!llvm.ident = !{"));
		var allMacros = text.Where(t => t.StartsWith("!")).ToArray();
		var allMacrosTogether = String.Join('\n', allMacros);
		Regex regex = new Regex(@"\![0-9] = !.*$", RegexOptions.Multiline);
		var matches = regex.Matches(allMacrosTogether);
		macros = new List<string>();
		foreach (Match match in matches.Cast<Match>()) {
			macros.Add(match.Value);
		}
		//macros.Last().Replace();	// todo swap !3 = !{!"clang version 15.0.1"} or !5 = !{!"Ubuntu clang version 14.0.0-1ubuntu1"} to similar to !3 = !{!"cy version 0.1"}
		return true;
	}

	public string Header() {
		return $"{DataLayout}\n{Triple}\n";
	}
	public string Footer() {
		return $"{string.Join('\n', Attributes)}\n\n{ModuleFlags}\n{Ident}\n\n{string.Join('\n', macros)}\n";
	}


	void CompileMinC() {
		System.Diagnostics.Process process = new System.Diagnostics.Process();
		System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
		startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
		startInfo.FileName = "cmd.exe";
		startInfo.Arguments = "/C clang -S -emit-llvm ." + Path.DirectorySeparatorChar + filename + ".c";
		process.StartInfo = startInfo;
		process.Start();
		process.WaitForExit();
	}

}

