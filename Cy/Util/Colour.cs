using Cy.Setup;

using System;
using System.Collections.Generic;
using System.Text;



namespace Cy.Util;


/// <summary> Deal with colour for console output. </summary>
public static class Colour {
	public static readonly string ResetColour = "\u001b[0m";

	/// <summary>Colours used for console output</summary>
	public enum Hue {
		Reset = -1,
		FG_Black,
		FG_DarkBlue,
		FG_DarkGreen,
		FG_DarkCyan,
		FG_DarkRed,
		FG_DarkMagenta,
		FG_DarkYellow,
		FG_Grey,
		FG_DarkGrey,
		FG_Blue,
		FG_Green,
		FG_Cyan,
		FG_Red,
		FG_Magenta,
		FG_Yellow,
		FG_White,
		BG_Black,
		BG_DarkBlue,
		BG_DarkGreen,
		BG_DarkCyan,
		BG_DarkRed,
		BG_DarkMagenta,
		BG_DarkYellow,
		BG_Grey,
		BG_DarkGrey,
		BG_Blue,
		BG_Green,
		BG_Cyan,
		BG_Red,
		BG_Magenta,
		BG_Yellow,
		BG_White,
	}

	public enum Property {
		Current = -1,
		LEXEME,
		TOKENTYPE,
		LITERAL,
		LINE,
		LINE_NUMBER,
		OFFSET,
		OFFSET_NUMBER,
	};

	public static readonly Dictionary<Property, Hue> PropertyColour = new() {
		{ Property.LEXEME, Hue.FG_DarkCyan },
		{ Property.TOKENTYPE, Hue.FG_Green },
		{ Property.LITERAL, Hue.FG_DarkBlue },
		{ Property.LINE, Hue.FG_DarkGrey },
		{ Property.LINE_NUMBER, Hue.FG_Grey },
		{ Property.OFFSET, Hue.FG_DarkGrey },
		{ Property.OFFSET_NUMBER, Hue.FG_Grey },
	};


	readonly static Dictionary<Hue, string> colourText = new() {
		{ Colour.Hue.Reset, ResetColour },
		{ Colour.Hue.FG_Black, "\u001b[30m" },
		{ Colour.Hue.FG_DarkBlue, "\u001b[34m" },
		{ Colour.Hue.FG_DarkGreen, "\u001b[32m" },
		{ Colour.Hue.FG_DarkCyan, "\u001b[36m" },
		{ Colour.Hue.FG_DarkRed, "\u001b[31m" },
		{ Colour.Hue.FG_DarkMagenta, "\u001b[35m" },
		{ Colour.Hue.FG_DarkYellow, "\u001b[33m" },
		{ Colour.Hue.FG_Grey, "\u001b[37m" },
		{ Colour.Hue.FG_DarkGrey, "\u001b[30;1m" },
		{ Colour.Hue.FG_Blue, "\u001b[34;1m" },
		{ Colour.Hue.FG_Green, "\u001b[32;1m" },
		{ Colour.Hue.FG_Cyan, "\u001b[36;1m" },
		{ Colour.Hue.FG_Red, "\u001b[31;1m" },
		{ Colour.Hue.FG_Magenta, "\u001b[35;1m" },
		{ Colour.Hue.FG_Yellow, "\u001b[33;1m" },
		{ Colour.Hue.FG_White, "\u001b[37;1m" },
		{ Colour.Hue.BG_Black, "\u001b[40m" },
		{ Colour.Hue.BG_DarkBlue, "\u001b[44m" },
		{ Colour.Hue.BG_DarkGreen, "\u001b[42m" },
		{ Colour.Hue.BG_DarkCyan, "\u001b[46m" },
		{ Colour.Hue.BG_DarkRed, "\u001b[41m" },
		{ Colour.Hue.BG_DarkMagenta, "\u001b[45m" },
		{ Colour.Hue.BG_DarkYellow, "\u001b[43m" },
		{ Colour.Hue.BG_Grey, "\u001b[47m" },
		{ Colour.Hue.BG_DarkGrey, "\u001b[40;1m" },
		{ Colour.Hue.BG_Blue, "\u001b[44;1m" },
		{ Colour.Hue.BG_Green, "\u001b[42;1m" },
		{ Colour.Hue.BG_Cyan, "\u001b[46;1m" },
		{ Colour.Hue.BG_Red, "\u001b[41;1m" },
		{ Colour.Hue.BG_Magenta, "\u001b[45;1m" },
		{ Colour.Hue.BG_Yellow, "\u001b[43;1m" },
		{ Colour.Hue.BG_White, "\u001b[47;1m" },
	};


	public static string Create(string line, Hue fgColour = Hue.Reset, Hue bgColour = Hue.Reset) {
		if (CommandLineInput.Instance.Colour) {
			if (fgColour != Hue.Reset) {
				line = colourText[fgColour] + line;
			}
			if (bgColour != Hue.Reset) {
				line = colourText[bgColour] + line;
			}
			if (fgColour != Hue.Reset || bgColour != Hue.Reset) {
				line += ResetColour;
			}
		}
		return line;
	}

	public static string Create(string line, Property fgColour = Property.Current, Property bgColour = Property.Current) {
		return Create(line, PropertyColour[fgColour], PropertyColour[bgColour]);
	}

	public static string Get(Property prop) {
		if (CommandLineInput.Instance.Colour) {
			return colourText[PropertyColour[prop]];
		}
		return string.Empty;
	}
	public static string Get(Hue hue) {
		if (CommandLineInput.Instance.Colour) {
			return colourText[hue];
		}
		return string.Empty;
	}
}


