using System;
using System.IO;
using System.Collections.Generic;
using Cocona;


namespace Cy {
	class Program {
		private static Config config;
		static int Main(string[] args) {
			config = Config.Instance;
			CoconaLiteApp.Run<Program>(args);
			var allFilesTokens = new List<List<Token>>();
			var scanner = new Scanner.Scanner();
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

			var allFilesStmts = new List<List<Ast.Stmt>>();
			foreach (var tokens in allFilesTokens) {
				var cursor = new Parser.Cursor(tokens);
				var parser = new Parser.Parser(cursor);
				var stmts = parser.Parse();
				allFilesStmts.Add(stmts);
			}
			if (Config.Instance.DisplayAsts) {
				new Ast.Printer().DisplayAllAsts(allFilesStmts);
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
			[Option('s', Description = "Tab size (in spaces).")] int tabSize = 4
		) {
			Config.Instance.Init(includes, filesIn, output, tokens, verbose, ast, tabSize);
		}
	}
}
