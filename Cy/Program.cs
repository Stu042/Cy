using System;
using System.IO;
using System.Collections.Generic;




namespace Cy {
    class Program {

        static int Main(string[] args) {
			List<List<Token>> allTokens = new List<List<Token>>();

			Scanner scanner = new Scanner();
			foreach (var filename in args) {
				string alltext = File.ReadAllText(filename);
				List<Token> tokens = scanner.ScanTokens(filename, alltext);
				allTokens.Add(tokens);
			}

			Console.WriteLine("\n\nTokens:");
			foreach (var tokens in allTokens)
				scanner.Show(tokens);

			List<List<Stmt>> allStmts = new List<List<Stmt>>();
			foreach (var tokens in allTokens) {
				Parser parser = new Parser(tokens);
				List<Stmt> stmts = parser.Parse();
				allStmts.Add(stmts);
			}

			Console.WriteLine("\n\nAST:");
			foreach (var stmts in allStmts) {
				foreach (var stmt in stmts) {
					Console.WriteLine(new AstPrinter().Print(stmt));
				}
			}

			Compiler compiler = new Compiler();
			int count = allStmts.Count;
			for (var i = 0; i < count; i++) {
				List<Stmt> stmts = allStmts[i];
				compiler.Prep(stmts[0].token.filename);
				foreach (var stmt in stmts)
					compiler.Compile(stmt);
				Console.WriteLine("\n\nCompiler Output:");
				Console.WriteLine(compiler.ToString());
			}

			return 0;
		}



	}
}
