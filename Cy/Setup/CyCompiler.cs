using Cy.CodeGen;
using Cy.Parsing;
using Cy.TokenGenerator;
using Cy.Types;

using System;
using System.Collections.Generic;
using System.IO;


namespace Cy.Setup;

public class CyCompiler {
	readonly Config _config;
	readonly Scanner _scanner;
	readonly Parser _parser;
	readonly TypeDefinitionTableCreate _createSymbolTable;
	readonly TypeDefinitionDisplay _displaySymbolTable;
	readonly GenIr _compiler;

	public CyCompiler(GenIr compiler, TypeDefinitionTableCreate createSymbolTable, Scanner scanner, TypeDefinitionDisplay displaySymbolTable, Config config, Parser parser) {
		_compiler = compiler;
		_createSymbolTable = createSymbolTable;
		_scanner = scanner;
		_displaySymbolTable = displaySymbolTable;
		_config = config;
		_parser = parser;
	}

	public int Compile() {
		var allFilesTokens = ScanFiles();
		var allFilesStmts = ParseTokens(allFilesTokens);
		var typeTable = MapTypes(allFilesStmts);
		var code = _compiler.GenerateLlvmIr(allFilesStmts, typeTable);
		if (_config.DisplayIr) {
			Console.WriteLine("\nIR:\n" + code);
		}
		return 0;
	}


	Token[][] ScanFiles() {
		var listFilesTokens = new List<Token[]>();
		foreach (var filename in _config.Input) {
			if (_config.Verbose) {
				Console.WriteLine($"Reading file: {filename}");
			}
			var alltext = File.ReadAllText(filename);
			var tokens = _scanner.ScanTokens(filename, alltext);
			listFilesTokens.Add(tokens);
		}
		var allFilesTokens = listFilesTokens.ToArray();
		if (_config.DisplayTokens) {
			Tokens.DisplayAllTokens(allFilesTokens);
		}
		return allFilesTokens;
	}

	Stmt[][] ParseTokens(Token[][] allFilesTokens) {
		var a = new Stmt[allFilesTokens.Length][];
		var listFilesStmts = new List<Stmt[]>();
		foreach (var tokens in allFilesTokens) {
			var stmts = _parser.Parse(tokens);
			listFilesStmts.Add(stmts);
		}
		var allFileStmts = listFilesStmts.ToArray();
		if (_config.DisplayAsts) {
			Asts.Display(allFileStmts);
		}
		return allFileStmts;
	}

	TypeDefinitionTable MapTypes(Stmt[][] allFilesStmts) {
		var typeTable = _createSymbolTable.Parse(allFilesStmts);
		if (_config.DisplayPreCompileSymbols) {
			_displaySymbolTable.DisplayTable(typeTable);
		}
		return typeTable;
	}
}
