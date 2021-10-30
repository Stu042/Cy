using System;
using System.IO;
using System.Linq;
using System.Text;

namespace Cy {
	public class ErrorDisplay : IErrorDisplay {
		public void Error(Token token, string linestr, string message) {
			Error(token.filename, token.line, token.offset, linestr, message);
		}

		public void Error(string filename, int line, int offset, string lineText, string message) {
			var infoText = BuildInfoText(line, offset, Config.Instance.TabSize);
			var errorLine = BuildErrorLine(lineText, line, offset, Config.Instance.TabSize);
			var pointerLine = BuildPointerLine(infoText + lineText, offset + infoText.Length, Config.Instance.TabSize);
			Console.WriteLine($"Error in {filename}. {message}");
			Console.WriteLine(infoText + errorLine);
			Console.WriteLine(pointerLine);
		}

		public void Error(Token tok, string message) {
			Error(tok.filename, tok.line, tok.offset, GetLine(tok.filename, tok.line), message);
		}

		// i.e. "    15|24 "
		string BuildInfoText(int line, int offset, int tabSize) {
			var tabstr = new string(' ', tabSize);
			return $"{tabstr}{line}|{offset} ";
		}

		// i.e. "int Main():"
		string BuildErrorLine(string lineText, int line, int offset, int tabSize) {
			var tabstr = new string(' ', tabSize);
			var fixedLineText = lineText.Replace("\t", tabstr);
			return fixedLineText;
		}
		// i.e. "--------^"
		string BuildPointerLine(string ErrorLine, int offset, int tabSize) {
			if (offset > ErrorLine.Length) {
				return "Error offset is greater than line length";
			}
			var output = new StringBuilder(ErrorLine.Length);
			var firstPart = ErrorLine.Substring(0, offset);
			var tabcount = firstPart.Count(c => c == '\t');
			output.Append(new string('-', tabcount * tabSize + offset - 1));
			output.Append('^');
			return output.ToString();
		}

		// return text at line from file in filename
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
}
