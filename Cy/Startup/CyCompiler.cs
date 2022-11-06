using Cy.Llvm.CodeGen;
using Cy.Preprocesor;
using Cy.Types;
using Cy.Util;

using System;
using System.Collections.Generic;
using System.IO;


namespace Cy.Setup;

public class CyCompiler {
	readonly Config _config;
	readonly Scanner _scanner;
	readonly Parser _parser;
	readonly TokenDisplay _tokenDisplay;
	readonly TypeTableCreate _typeTableCreate;
	readonly Compiler _compiler;

	public CyCompiler(Scanner scanner, Config config, Parser parser, TokenDisplay tokenDisplay, TypeTableCreate typeTableCreate, Compiler compiler) {
		_scanner = scanner;
		_config = config;
		_parser = parser;
		_tokenDisplay = tokenDisplay;
		_typeTableCreate = typeTableCreate;
		_compiler = compiler;
	}

	public int Compile() {
		var allFilesTokens = ScanFiles();
		var allFilesStmts = ParseTokens(allFilesTokens);
		var typeTable = CreateTypeTable(allFilesStmts);
		Compile(allFilesStmts);
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
		if (_config.DisplayTokens || _config.DisplayAllTokens) {
			_tokenDisplay.DisplayAllTokens(allFilesTokens, _config.DisplayAllTokens);
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

	TypeTable CreateTypeTable(List<List<Stmt>> allFilesStmts) {
		var typeTable = _typeTableCreate.Create(allFilesStmts);
		if (_config.DisplayAsts) {
			TypeTable.Display(typeTable);
		}
		return typeTable;
	}

	void Compile(List<List<Stmt>> allFilesStmts) {
		var allCode = _compiler.Compile(allFilesStmts);
		ColourConsole.WriteLine("\n\n//FG_Blue Source Code\n");
		foreach (var code in allCode) {
			ColourConsole.WriteLine($"//FG_DarkBlue File Name: {code.FileName.Llvm}");
			ColourConsole.WriteLine(code.Code);
		}
	}
}
