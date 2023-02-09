using Cy.Startup;

using System;
using System.Text;

namespace Cy.Util;


/// <summary> Used to output to the console with colours. </summary>
public static class ColourConsole {
	const string START_OF_MARKER = "//";

	public static void WriteLine(string line, Colour.Hue fgColour = Colour.Hue.Reset, Colour.Hue bgColour = Colour.Hue.Reset) {
		if (CommandLineInput.Instance.Colour) {
			line = Colour.Create(line, fgColour, bgColour);
		}
		Console.WriteLine(line);
	}
	public static void Write(string line, Colour.Hue fgColour = Colour.Hue.Reset, Colour.Hue bgColour = Colour.Hue.Reset) {
		if (CommandLineInput.Instance.Colour) {
			line = Colour.Create(line, fgColour, bgColour);
		}
		Console.Write(line);
	}

	public static void WriteLine(string line, Colour.Property fgColour = Colour.Property.Current, Colour.Property bgColour = Colour.Property.Current) {
		if (CommandLineInput.Instance.Colour) {
			line = Colour.Create(line, fgColour, bgColour);
		}
		Console.WriteLine(line);
	}
	public static void Write(string line, Colour.Property fgColour = Colour.Property.Current, Colour.Property bgColour = Colour.Property.Current) {
		if (CommandLineInput.Instance.Colour) {
			line = Colour.Create(line, fgColour, bgColour);
		}
		Console.Write(line);
	}

	/// <summary> Add colour to text by prepending Colour.Hue values with //, i.e. "//FG_Red Some red text" </summary>
	public static void WriteColour(string line) {
		var str = LineColour(line);
		Console.Write(str);
	}
	/// <summary> Add colour to text by prepending Colour.Hue values with //, i.e. "//FG_Red Some red text" </summary>
	public static void WriteLineColour(string line) {
		var str = LineColour(line);
		Console.WriteLine(str);
	}
	static string LineColour(string line) {
		var start = line.IndexOf(START_OF_MARKER);
		if (start == -1 || !CommandLineInput.Instance.Colour) {
			return line;
		}
		var bob = new StringBuilder();
		var oldStart = 0;
		while (start > -1) {
			bob.Append(line.AsSpan(oldStart, start - oldStart));
			start += START_OF_MARKER.Length;
			var end = line.IndexOf(' ', start);
			var colourName = line[start..end];
			if (CommandLineInput.Instance.Colour) {
				try {
					var colour = (Colour.Hue)Enum.Parse(typeof(Colour.Hue), colourName);
					var colourText = Colour.Get(colour);
					bob.Append(colourText);
				} catch (Exception ex) {
					Console.WriteLine($"Unable to parse colour, named: {colourName}, message: {ex.Message}");
				}
			}
			oldStart = end + 1;
			start = line.IndexOf(START_OF_MARKER, end);
		}
		if (CommandLineInput.Instance.Colour) {
			bob.Append(line.AsSpan(oldStart, line.Length - oldStart));
			bob.Append(Colour.Get(Colour.Hue.Reset));
		}
		return bob.ToString();
	}

	/// <summary> Add colour to text by prepending Colour.Property values with //, i.e. "//LEXEME A lexeme" </summary>
	public static void WriteProperty(string line) {
		var str = LineProperty(line);
		Console.Write(str);
	}
	/// <summary> Add colour to text by prepending Colour.Property values with //, i.e. "//LEXEME A lexeme" </summary>
	public static void WriteLineProperty(string line) {
		var str = LineProperty(line);
		Console.WriteLine(str);
	}
	public static string LineProperty(string line) {
		var start = line.IndexOf(START_OF_MARKER);
		if (start == -1 || !CommandLineInput.Instance.Colour) {
			return line;
		}
		var bob = new StringBuilder();
		var oldStart = 0;
		while (start > -1) {
			bob.Append(line.AsSpan(oldStart, start - oldStart));
			start += START_OF_MARKER.Length;
			var end = line.IndexOf(' ', start);
			var propertyName = line[start..end];
			if (CommandLineInput.Instance.Colour) {
				try {
					var property = (Colour.Property)Enum.Parse(typeof(Colour.Property), propertyName);
					var colourText = Colour.Get(property);
					bob.Append(colourText);
				} catch (Exception ex) {
					Console.WriteLine($"Unable to parse properties colour named: {propertyName}, message: {ex.Message}");
				}
			}
			oldStart = end + 1;
			start = line.IndexOf(START_OF_MARKER, end);
		}
		if (CommandLineInput.Instance.Colour) {
			bob.Append(line.AsSpan(oldStart, line.Length - oldStart));
			bob.Append(Colour.Get(Colour.Hue.Reset));
		}
		return bob.ToString();
	}


	/// <summary> Add colour to text by prepending Colour.Hue or Colour.Property values with //, i.e. "//FG_Red Some red text, //LEXEME a lexeme" </summary>
	public static void Write(string line) {
		var str = Line(line);
		Console.Write(str);
	}
	/// <summary> Add colour to text by prepending Colour.Hue or Colour.Property values with //, i.e. "//FG_Red Some red text, //LEXEME a lexeme" </summary>
	public static void WriteLine(string line) {
		var str = Line(line);
		Console.WriteLine(str);
	}
	public static string Line(string line) {
		var start = line.IndexOf(START_OF_MARKER);
		if (start == -1 || !CommandLineInput.Instance.Colour) {
			return line;
		}
		var bob = new StringBuilder();
		var oldStart = 0;
		while (start > -1) {
			bob.Append(line.AsSpan(oldStart, start - oldStart));
			start += START_OF_MARKER.Length;
			var end = line.IndexOf(' ', start);
			var name = line[start..end];
			if (CommandLineInput.Instance.Colour) {
				try {
					var property = (Colour.Property)Enum.Parse(typeof(Colour.Property), name);
					var colourText = Colour.Get(property);
					bob.Append(colourText);
				} catch {
					try {
						var colour = (Colour.Hue)Enum.Parse(typeof(Colour.Hue), name);
						var colourText = Colour.Get(colour);
						bob.Append(colourText);
					} catch (Exception ex) {
						Console.WriteLine($"Unable to parse property or colour, named: {name}, message: {ex.Message}");
					}
				}
			}
			oldStart = end + 1;
			start = line.IndexOf(START_OF_MARKER, end);
		}
		if (CommandLineInput.Instance.Colour) {
			bob.Append(line.AsSpan(oldStart, line.Length - oldStart));
			bob.Append(Colour.Get(Colour.Hue.Reset));
		}
		return bob.ToString();
	}
}