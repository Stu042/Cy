using System;
using System.IO;
using System.Collections.Generic;
using Cocona;
using Cy.Common;
using Cy.Scanner;
using Cy.Compiler;

namespace Cy {
	class Program {
		private static Config config;
		static int Main(string[] args) {
			config = Config.Instance;
			CoconaLiteApp.Run<Program>(args);
			ErrorDisplay display = new();
			var allFilesTokens = new List<List<Token>>();
			var scanner = new Scanner.Scanner(display);
			foreach (var filename in config.FilesIn) {
				if (Config.Instance.Verbose) {
					Console.WriteLine($"Reading file: {filename}");
				}
				var alltext = File.ReadAllText(filename);
				var tokens = scanner.ScanTokens(filename, alltext);
				allFilesTokens.Add(tokens);
			}
			if (Config.Instance.DisplayTokens) {
				scanner.DisplayAllTokens(allFilesTokens);
			}

			var allFilesStmts = new List<List<Stmt>>();
			foreach (var tokens in allFilesTokens) {
				Parser.Cursor cursor = new(tokens);
				Parser.Parser parser = new(cursor, display);
				var stmts = parser.Parse();
				allFilesStmts.Add(stmts);
			}
			if (Config.Instance.DisplayAsts) {
				new Ast.Printer().DisplayAllAsts(allFilesStmts);
			}

			var createTypeTable = new CreateTypeTable();
			createTypeTable.Parse(allFilesStmts);
			var typeTable = createTypeTable.TypeTable;
			if (Config.Instance.DisplayPreCompileSymbols) {
				Console.WriteLine("\nType Table:");
				var displayTypeTable = new DisplayTypeTable();
				displayTypeTable.DisplayTable(typeTable);
			}

			var createSymbolTable = new CreateSymbolTable();
			createSymbolTable.Parse(allFilesStmts);
			var symbolTable = createSymbolTable.SymbolTable;
			if (Config.Instance.DisplayPreCompileSymbols) {
				Console.WriteLine("\nPre-Compilation Symbols:");
				var displaySymbolTable = new DisplaySymbolTable();
				displaySymbolTable.DisplayTable(symbolTable);
			}
			return 0;
		}


		public void Cy(
			[Option('i', Description = "Input files to compile.")] string[] filesIn,
			[Option('A', Description = "Display parser generated ASTs.")] bool ast = false,
			[Option('v', Description = "Verbose output.")] bool verbose = false,
			[Option('T', Description = "Display scanner generated tokens.")] bool tokens = false,
			[Option('I', Description = "Includes to use.")] string[] includes = null,
			[Option('o', Description = "Output file name.")] string output = "main.c",
			[Option('s', Description = "Tab size (in spaces).")] int tabSize = 4,
			[Option('S', Description = "Display pre-compilation Symbol Table.")] bool preCompileSymbols = false
		) {
			Config.Instance.Init(includes, filesIn, output, tokens, verbose, ast, tabSize, preCompileSymbols);
		}
	}
}
