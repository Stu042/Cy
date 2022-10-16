using Cy.Constants;
using Cy.Preprocesor.Interfaces;
using Cy.Setup;
using Cy.Util;

using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Cy.Preprocesor;
public class ErrorDisplay : IErrorDisplay {
	readonly Config _config;

	public ErrorDisplay(Config config) {
		_config = config;
	}


	public void Error(Token token, string linestr, string message) {
		Error(token.filename, token.line, token.offset, linestr, message);
	}

	public void Error(Token tok, string message) {
		Error(tok.filename, tok.line, tok.offset, GetLine(tok.filename, tok.line), message);
	}

	public void Error(string filename, int line, int offset, string lineText, string message) {
		var infoText = BuildInfoText(line, offset, _config.TabSize);
		var errorLine = BuildErrorLine(lineText, line, _config.TabSize);
		var pointerLine = BuildPointerLine(infoText, lineText, offset, _config.TabSize);
		ColourConsole.Write($"Error in {filename},", Colour.Hue.FG_Red);
		var newMessage = TokenTypeToString(message);
		ColourConsole.WriteLine($" {newMessage}", Colour.Hue.FG_DarkGrey);
		ColourConsole.Write(infoText, Colour.Hue.FG_DarkBlue);
		ColourConsole.WriteLine(errorLine, Colour.Hue.FG_White);
		ColourConsole.Write(pointerLine, Colour.Hue.FG_Blue);
		if (pointerLine != null) {
			ColourConsole.WriteLine("^", Colour.Hue.FG_Cyan);
		}
	}

	string TokenTypeToString(string message) {
		var tokenTypeValues = (TokenType[])Enum.GetValues(typeof(TokenType));
		foreach (var tokenTypeValue in tokenTypeValues) {
			var pattern = $"\\b{Regex.Escape(tokenTypeValue.ToString())}\\b";
			message = Regex.Replace(message, pattern, "'" + TokenTypeStr.TokenTypeString[tokenTypeValue] + "'");
		}
		return message;
	}

	// i.e. "    15|24 "
	string BuildInfoText(int line, int offset, int tabSize) {
		var tabstr = new string(' ', tabSize);
		return $"{tabstr}{line}|{offset} ";
	}

	// i.e. "int Main():"
	string BuildErrorLine(string lineText, int line, int tabSize) {
		var tabstr = new string(' ', tabSize);
		var fixedLineText = lineText.Replace("\t", tabstr);
		return fixedLineText;
	}

	// i.e. "--------^"
	string BuildPointerLine(string infoText, string errorText, int offset, int tabSize) {
		if (offset < 0 || offset >= errorText.Length) {
			offset = errorText.Length;
		}
		var output = new StringBuilder(offset + infoText.Length);
		var firstPart = errorText[..offset];
		var tabcount = firstPart.Count(c => c == '\t');
		output.Append(new string('-', tabcount * tabSize - tabcount + offset + infoText.Length));
		return output.ToString();
	}

	/// <summary>Return text at line from file in filename</summary>
	string GetLine(string filename, int line) {
		if (filename != "") {
			string[] alltext = File.ReadAllLines(filename);
			int idx = line - 1;
			if (idx < alltext.Length) {
				var text = alltext[idx];
				return text;
			}
		}
		return "unknown";
	}
}
