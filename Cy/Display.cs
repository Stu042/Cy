using System;
using System.IO;


namespace Cy {
	static class Display {
		public static void Error(Token token, string linestr, string message) {
			Error(token.filename, token.line, token.offset, linestr, message);
		}

		public static void Error(Token tok, string message) {
			Error(tok.filename, tok.line, tok.offset, GetLine(tok.filename, tok.line), message);
		}

		public static void Error(string filename, int line, int offset, string linestr, string message) {
			Console.WriteLine($"Error in {filename}: {message}");
			string errorstr = $"\t{line}|{offset} ";
			Console.WriteLine($"{errorstr}{linestr}");
			int c = 0;
			int count = errorstr.Length + Math.Min(offset, linestr.Length);
			while (c < errorstr.Length) {
				if (errorstr[c] == '\t')
					Console.Write("\t");
				else
					Console.Write(" ");
				c++;
			}
			while (c < count) {
				if (linestr[c++ - errorstr.Length] == '\t')
					Console.Write("\t");
				else
					Console.Write(" ");
			}
			Console.WriteLine("^");
		}

		static string GetLine(string filename, int line) {
			string[] alltext = File.ReadAllLines(filename);
			int idx = Math.Min(line, alltext.Length - 1);
			return alltext[idx];
		}

	}
}
