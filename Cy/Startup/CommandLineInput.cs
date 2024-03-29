﻿using System;


namespace Cy.Startup;


// only used as input to Config
public sealed class CommandLineInput {
	private static readonly Lazy<CommandLineInput> lazy = new(() => new CommandLineInput());

	public string[] Includes { get; set; }
	public string[] Input { get; set; }
	public string FileOut { get; set; }
	public bool DisplayAllTokens { get; set; }
	public bool DisplayTokens { get; set; }
	public bool Verbose { get; set; }
	public bool DisplayAsts { get; set; }
	public int TabSize { get; set; }
	public bool DisplayPreCompileSymbols { get; set; }
	public bool Colour { get; set; }

	public static CommandLineInput Instance { get { return lazy.Value; } }

	private CommandLineInput() { }
	public void Init(string[] includes, string[] filesIn, string output, bool allTokens, bool tokens, bool verbose, bool ast, int tabSize, bool displayPreCompileSymbols, bool colour) {
		Includes = includes;
		Input = filesIn;
		FileOut = output;
		DisplayAllTokens = allTokens;
		DisplayTokens = tokens;
		Verbose = verbose;
		DisplayAsts = ast;
		TabSize = tabSize;
		DisplayPreCompileSymbols = displayPreCompileSymbols;
		Colour = colour;
	}
}
