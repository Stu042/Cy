namespace Cy.Setup;


public class Config {
	public string[] Includes { get; }
	public string[] Input { get; }
	public string FileOut { get; }
	public bool DisplayTokens { get; }
	public bool Verbose { get; }
	public bool DisplayAsts { get; }
	public int TabSize { get; }
	public bool DisplayPreCompileSymbols { get; }
	public bool DisplayIr { get; }

	public string Version { get; }
	public string ClangExe { get; }
	public int DefaultAlignment { get; }

	public Config() {
		Includes = CommandLineInput.Instance.Includes;
		Input = CommandLineInput.Instance.Input;
		FileOut = CommandLineInput.Instance.FileOut ?? "cy.ll";
		DisplayTokens = CommandLineInput.Instance.DisplayTokens;
		Verbose = CommandLineInput.Instance.Verbose;
		DisplayAsts = CommandLineInput.Instance.DisplayAsts;
		TabSize = CommandLineInput.Instance.TabSize;
		DisplayPreCompileSymbols = CommandLineInput.Instance.DisplayPreCompileSymbols;
		DisplayIr = CommandLineInput.Instance.DisplayIr;
		Version = typeof(Program).Assembly.GetName().Version.ToString();
		ClangExe = @"C:\Program Files\LLVM\bin\clang.exe";
		DefaultAlignment = 4;
	}
}
