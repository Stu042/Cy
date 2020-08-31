using System;
using System.Collections.Generic;




namespace Cy {
    class Program {
        static int Main(string[] args) {
			Scanner scanner = new Scanner();
			List<Token> tokens = scanner.ScanAllFiles(args);
			Console.WriteLine("\n\nTokens:");
			scanner.Show(tokens);

			Parser parser = new Parser(tokens);
			List<Stmt> stmts = parser.Parse();
			Console.WriteLine("\n\nAST:");
			foreach (var stmt in stmts)
				Console.WriteLine(new AstPrinter().Print(stmt));

			Compiler compiler = new Compiler();
			compiler.Prep("TestOut1.ll");
			foreach (var stmt in stmts)
				compiler.Compile(stmt);
			Console.WriteLine("\n\nCompiler Output:");
			Console.WriteLine(compiler.ToString());

			return 0;
		}



	}
}
