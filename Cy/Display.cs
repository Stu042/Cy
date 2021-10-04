using System;
using System.IO;
using System.Linq;

namespace Cy {
	static class Display {
		public static void Error(Token token, string linestr, string message) {
			Error(token.filename, token.line, token.offset, linestr, message);
		}

		public static void Error(string filename, int line, int offset, string lineText, string message) {
			var tabstr = new string('T', Config.Instance.TabSize);
			Console.WriteLine($"Error in {filename}, {message}");
			string infoText = $"{tabstr}{line}|{offset} ";
			Console.WriteLine($"{infoText}{lineText.Replace("\t", tabstr)}");
			var lineTextToErrorTabCount = lineText.Substring(0, offset).Count(c => c == '\t');
			var fixedLineText = lineText.Replace("\t", tabstr);
			//Console.Write(new string(' ', infoText.Length + lineTextToErrorTabCount * Config.Instance.TabSize + offset));
			Console.Write(new string('I', infoText.Length));
			Console.Write(new string('T', lineTextToErrorTabCount * Config.Instance.TabSize));
			Console.Write(new string('O', offset));
			Console.WriteLine("^");
		}

		public static void Error(Token tok, string message) {
			Error(tok.filename, tok.line, tok.offset, GetLine(tok.filename, tok.line), message);
		}

		static string GetLine(string filename, int line) {
			if (filename != "") {
				string[] alltext = File.ReadAllLines(filename);
				int idx = Math.Clamp(line - 1, 0, alltext.Length - 1);
				var text = alltext[idx];
				return text;
			}
			return "unknown";
		}
	}
}
