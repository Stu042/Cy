using Cocona;


namespace Cy.Startup;

public class CommandLineCommands {
	// -STAv -o temp.c -i test.cy
	public void Cy(
		[Option('i', Description = "Input files to compile.")] string[] input,
		[Option('A', Description = "Display parser generated ASTs.")] bool ast = false,
		[Option('v', Description = "Verbose output.")] bool verbose = false,
		[Option('T', Description = "Display all scanner generated tokens, including ignored tokens.")] bool allTokens = false,
		[Option('t', Description = "Display scanner generated tokens, don't display ignored tokens.")] bool tokens = false,
		[Option('I', Description = "Includes to use.")] string[] includes = null,
		[Option('o', Description = "Output file name.")] string output = "main.c",
		[Option('s', Description = "Tab size (in spaces).")] int tabSize = 4,
		[Option('S', Description = "Display Symbol Table.")] bool preCompileSymbols = false,
		[Option('c', Description = "Coloured output.")] bool colour = true) {
		CommandLineInput.Instance.Init(includes, input, output, allTokens, tokens, verbose, ast, tabSize, preCompileSymbols, colour);
	}
}
