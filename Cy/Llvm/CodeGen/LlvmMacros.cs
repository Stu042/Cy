using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;


namespace Cy.Llvm.CodeGen;


[DebuggerDisplay("LlvmMacros")]
public class LlvmMacros {
	string DataLayout;
	string Triple;
	IEnumerable<string> Attributes;
	string ModuleFlags;
	string Ident;
	List<string> macros;
	readonly string filename;
	readonly string[] lines = {
		"int main() {",
		"  return 0;",
		"}"
	};


	public LlvmMacros() {
		filename = "uniqueNameMin" + Guid.NewGuid().ToString();
	}


	public async Task<bool> Setup() {
		await File.WriteAllLinesAsync(filename + ".c", lines);
		CompileMinC();
		var text = File.ReadAllLines(filename + ".ll");
		File.Delete(filename + ".c");
		File.Delete(filename + ".ll");
		DataLayout = text.FirstOrDefault(t => t.StartsWith("target datalayout ="));
		Triple = text.FirstOrDefault(t => t.StartsWith("target triple ="));
		Attributes = text.Where(t => t.StartsWith("attributes #"));
		ModuleFlags = text.FirstOrDefault(t => t.StartsWith("!llvm.module.flags ="));
		Ident = text.FirstOrDefault(t => t.StartsWith("!llvm.ident ="));
		var allMacros = text.Where(t => t.StartsWith("!"));
		var allMacrosTogether = String.Join('\n', allMacros);
		var regex = new Regex(@"\![0-9] = !.*$", RegexOptions.Multiline);
		var matches = regex.Matches(allMacrosTogether);
		macros = new List<string>();
		foreach (Match match in matches.Cast<Match>()) {
			macros.Add(match.Value);
		}
		var versionMacro = macros.FirstOrDefault(m => m.Contains("clang version"));
		if (versionMacro != null) {
			var versionMacroIdx = macros.IndexOf(versionMacro);
			string version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
			macros[versionMacroIdx] = $"{versionMacro[..versionMacro.IndexOf('=')]}= !{{!\"cy version {version}\"}}";
		}
		return true;
	}

	public string Header() {
		return $"{DataLayout}\n{Triple}\n";
	}
	public string Footer() {
		return $"{String.Join('\n', Attributes)}\n\n{ModuleFlags}\n{Ident}\n\n{String.Join('\n', macros)}\n";
	}


	void CompileMinC() {
		System.Diagnostics.Process process = new Process() {
			StartInfo = new ProcessStartInfo {
				WindowStyle = ProcessWindowStyle.Hidden,
				FileName = "cmd.exe",
				Arguments = "/C clang -S -emit-llvm ." + Path.DirectorySeparatorChar + filename + ".c"
			}
		};
		process.Start();
		process.WaitForExit();
	}
}
