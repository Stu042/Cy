using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Cocona;


namespace Cy {
	class Program {
		static int Main(string[] args) {
			CoconaApp.Run<Program>(args);
			var allFilesTokens = new List<List<Token>>();
			var scanner = new Scanner();
			foreach (var filename in args) {
				if (Config.Instance.Verbose) {
					Console.WriteLine($"Reading file: {filename}");
				}
				var alltext = File.ReadAllText(filename);
				var tokens = scanner.ScanTokens(filename, alltext);
				allFilesTokens.Add(tokens);
			}
			DisplayTokens(scanner, allFilesTokens);

			var allFilesStmts = new List<List<Ast.Stmt>>();
			foreach (var tokens in allFilesTokens) {
				var parser = new Parser(tokens);
				var stmts = parser.Parse();
				allFilesStmts.Add(stmts);
			}
			DisplayAsts(allFilesStmts);

			return 0;
		}


		public static void DisplayTokens(Scanner scanner, List<List<Token>> allFilesTokens) {
			if (Config.Instance.DisplayTokens) {
				var tokenCount = allFilesTokens.Sum(tokens => tokens.Count);
				Console.WriteLine($"\n\n{tokenCount} Tokens:");
				foreach (var tokens in allFilesTokens) {
					scanner.Show(tokens);
				}
			}
		}


		public static void DisplayAsts(List<List<Ast.Stmt>> allFilesStmts) {
			if (Config.Instance.DisplayAsts) {
				Console.WriteLine("\n\nAST:");
				foreach (var stmts in allFilesStmts) {
					foreach (var stmt in stmts) {
						Console.WriteLine(new Ast.Printer().Print(stmt));
					}
				}
			}
		}


		public void Cy(
		[Option('A', Description = "Display parser generated ASTs.")] bool ast,
		[Option('v', Description = "Verbose output.")] bool verbose,
		[Option('I', Description = "Includes to use.")] string[] includes,
		[Option('i', Description = "Input files to compile.")] string[] filesIn,
		[Option('T', Description = "Display scanner generated tokens.")] bool tokens,
		[Option('o', Description = "Output file name.")] string output = "main.c"
		) {
			Config.Instance.Init(includes, filesIn, output, tokens, verbose, ast);
		}
	}
}
