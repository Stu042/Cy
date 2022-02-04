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

	public Config() {
		Includes = CommandLineInput.Instance.Includes;
		Input = CommandLineInput.Instance.Input;
		FileOut = CommandLineInput.Instance.FileOut;
		DisplayTokens = CommandLineInput.Instance.DisplayTokens;
		Verbose = CommandLineInput.Instance.Verbose;
		DisplayAsts = CommandLineInput.Instance.DisplayAsts;
		TabSize = CommandLineInput.Instance.TabSize;
		DisplayPreCompileSymbols = CommandLineInput.Instance.DisplayPreCompileSymbols;
	}
}
