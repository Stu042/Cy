using Cy.Preprocesor;
using Cy.Preprocesor.Interfaces;

using System;
using System.Collections.Generic;
using System.IO;


namespace Cy.Setup;

public class CyCompiler {
	readonly SymbolTableCreate _createSymbolTable;
	readonly Scanner _scanner;
	readonly DisplaySymbolTable _displaySymbolTable;
	readonly Config _config;
	readonly Parser _parser;

	public CyCompiler(SymbolTableCreate createSymbolTable, Scanner scanner, DisplaySymbolTable displaySymbolTable, Config config, Parser parser) {
		_createSymbolTable = createSymbolTable;
		_scanner = scanner;
		_displaySymbolTable = displaySymbolTable;
		_config = config;
		_parser = parser;
	}

	public int Compile() {
		var allFilesTokens = ScanFiles();
		var allFilesStmts = ParseTokens(allFilesTokens);
		var symbolTable = MapSymbols(allFilesStmts);
		return 0;
	}


	List<List<Token>> ScanFiles() {
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
			Tokens.DisplayAllTokens(allFilesTokens);
		}
		return allFilesTokens;
	}

	List<List<Stmt>> ParseTokens(List<List<Token>> allFilesTokens) {
		var allFilesStmts = new List<List<Stmt>>();
		foreach (var tokens in allFilesTokens) {
			var stmts = _parser.Parse(tokens);
			allFilesStmts.Add(stmts);
		}
		if (_config.DisplayAsts) {
			Asts.Display(allFilesStmts);
		}
		return allFilesStmts;
	}

	SymbolTable MapSymbols(List<List<Stmt>> allFilesStmts) {
		var typeTable = new SymbolTable();
		_createSymbolTable.Parse(allFilesStmts);
		if (_config.DisplayPreCompileSymbols) {
			_displaySymbolTable.DisplayTable(typeTable);
		}
		return typeTable;
	}
}
