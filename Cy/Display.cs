using System;


namespace Cy {
	static class Display {
		public static void Error(Token token, string linestr, string message) {
			Error(token.filename, token.line, token.offset, linestr, message);
		}

		public static void Error(string filename, int line, int offset, string linestr, string message) {
			Console.WriteLine($"Error in {filename}: {message}");
			string errorstr = $"\t{line}|{offset} ";
			Console.WriteLine($"{errorstr}{linestr}");
			int c = 0;
			int count = errorstr.Length + offset;
			while (c++ < count)
				Console.Write(" ");
			Console.WriteLine("^");
		}

		public static void Error(Token tok, string message) {
			Error(tok.filename, tok.line, tok.offset, "", message);
		}

		// TODO cache source files to auto get linestr


	}
}
