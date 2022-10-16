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

	public CyCompiler(Scanner scanner, Config config, Parser parser, TokenDisplay tokenDisplay, TypeTableCreate typeTableCreate) {
		_scanner = scanner;
		_config = config;
		_parser = parser;
		_tokenDisplay = tokenDisplay;
		_typeTableCreate = typeTableCreate;
	}

	public int Compile() {
		var allFilesTokens = ScanFiles();
		var allFilesStmts = ParseTokens(allFilesTokens);
		var typeTable = CreateTypeTable(allFilesStmts);
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

	Types.TypeTable CreateTypeTable(List<List<Stmt>> allFilesStmts) {
		var typeTable = _typeTableCreate.Create(allFilesStmts);
		if (_config.DisplayTokens) {
			ColourConsole.WriteLine("\n//FG_Grey Types:");
			foreach (var type in typeTable.types) {
				var typeDef = type.Value;
				ColourConsole.WriteLine($"//FG_Blue {typeDef.Format, -20} //FG_Green {typeDef.Name, -40} //FG_Grey {typeDef.BitSize, 5} {typeDef.ByteSize, -4}");
				var objectDef = typeDef as ObjectType;
				if (objectDef != null) {
					for (var index = 0; index < objectDef.ChildCount(); index++) {
						var child = objectDef.GetChildAtIndex(index);
						ColourConsole.WriteLine($"\t//FG_Blue {child.Format,-20} //FG_Green {child.Name,-40} //FG_Grey {child.BitSize,5} {child.ByteSize,-4}");
					}
				}
			}
		}
		return typeTable;
	}
}
