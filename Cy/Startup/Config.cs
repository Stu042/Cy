namespace Cy.Startup;


public class Config {
	public string[] Includes { get; }
	public string[] Input { get; }
	public string FileOut { get; }
	public bool DisplayAllTokens { get; }
	public bool DisplayTokens { get; }
	public bool Verbose { get; }
	public bool DisplayAsts { get; }
	public int TabSize { get; }
	public bool DisplayPreCompileSymbols { get; }
	public bool Colour { get; }

	public Config() {
		Includes = CommandLineInput.Instance.Includes;
		Input = CommandLineInput.Instance.Input;
		FileOut = CommandLineInput.Instance.FileOut;
		DisplayAllTokens = CommandLineInput.Instance.DisplayAllTokens;
		DisplayTokens = CommandLineInput.Instance.DisplayTokens;
		Verbose = CommandLineInput.Instance.Verbose;
		DisplayAsts = CommandLineInput.Instance.DisplayAsts;
		TabSize = CommandLineInput.Instance.TabSize;
		DisplayPreCompileSymbols = CommandLineInput.Instance.DisplayPreCompileSymbols;
		Colour = CommandLineInput.Instance.Colour;
	}
}
