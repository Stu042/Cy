using Cy.Preprocesor;
using Cy.Preprocesor.Interfaces;

using System;
using System.Collections.Generic;
using System.IO;


namespace Cy.Setup;

public class CyCompiler {
	readonly Scanner _scanner;
	readonly Config _config;
	readonly IErrorDisplay _errorDisplay;

	public CyCompiler(Scanner scanner, Config config, IErrorDisplay errorDisplay) {
		_scanner = scanner;
		_config = config;
		_errorDisplay = errorDisplay;
	}

	public int Compile() {
		var allFilesTokens = new List<List<Token>>();
		foreach (var filename in _config.Input) {
			if (_config.Verbose) {
				Console.WriteLine($"Reading file: {filename}");
			}
			var alltext = File.ReadAllText(filename);
			var tokens = _scanner.ScanTokens(filename, alltext);
			allFilesTokens.Add(tokens);
		}
		if (_config.DisplayTokens) {
			_scanner.DisplayAllTokens(allFilesTokens);
		}

		var allFilesStmts = new List<List<Stmt>>();
		foreach (var tokens in allFilesTokens) {
			var cursor = new ParserCursor(tokens);
			var parser = new Parser(cursor, _errorDisplay);
			var stmts = parser.Parse();
			allFilesStmts.Add(stmts);
		}
		if (_config.DisplayAsts) {
			new AstPrinter().DisplayAllAsts(allFilesStmts);
		}

		var createSymbolTable = new CreateSymbolTable();
		var typeTable = createSymbolTable.Parse(allFilesStmts);
		if (_config.DisplayPreCompileSymbols) {
			Console.WriteLine("\nSymbol Table:");
			var displayTypeTable = new DisplaySymbolTable();
			displayTypeTable.DisplayTable(typeTable);
		}
		return 0;
	}
}


/*
class Program
{
    public Program(ILogger<Program> logger)
    {
        logger.LogInformation("Create Instance");
    }

    static void Main(string[] args)
    {
        CoconaApp.Create()
            .ConfigureServices(services =>
            {
                services.AddTransient<MyService>();
            })
            .Run<Program>(args);
    }

    public void Hello([FromService]MyService myService)
    {
        myService.Hello("Hello Konnichiwa!");
    }
}

class MyService
{
    private readonly ILogger _logger;

    public MyService(ILogger<MyService> logger)
    {
        _logger = logger;
    }

    public void Hello(string message)
    {
        _logger.LogInformation(message);
    }
}
*/