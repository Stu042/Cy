using System;

namespace Cy {

	public sealed class Config {
		private static readonly Lazy<Config> lazy =
			new Lazy<Config>(() => new Config());

		public string[] Includes { get; set; }
		public string[] FilesIn { get; set; }
		public string FileOut { get; set; }
		public bool DisplayTokens { get; set; }
		public bool Verbose { get; set; }
		public bool DisplayAsts { get; set; }
		public int TabSize { get; set; }

		public static Config Instance { get { return lazy.Value; } }

		private Config() { }
		public void Init(string[] includes, string[] filesIn, string output, bool tokens, bool verbose, bool ast, int tabSize) {
			Includes = includes;
			FilesIn = filesIn;
			FileOut = output;
			DisplayTokens = tokens;
			Verbose = verbose;
			DisplayAsts = ast;
			TabSize = tabSize;
		}
	}
}
