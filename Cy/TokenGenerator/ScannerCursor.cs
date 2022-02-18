
using System;


namespace Cy.TokenGenerator;

public class ScannerCursor {
	char[] source;
	int start;
	int current;

	public void NewFile(string text) {
		source = text.ToCharArray();
		start = 0;
		current = 0;
	}

	public void Start() {
		start = current;
	}

	public bool Match(char expected) {
		if (IsAtEnd() || source[current] != expected) {
			return false;
		}
		current++;
		return true;
	}

	public bool IsAtEnd() {
		return current >= source.Length;
	}

	public char Advance() {
		return source[current++];
	}

	public char Peek() {
		if (IsAtEnd()) {
			return '\0';
		}
		return source[current];
	}

	public char PeekNext() {
		if (current + 1 >= source.Length) {
			return '\0';
		}
		return source[current + 1];
	}

	public char[] ToArray(int startOffset = 0, int endOffset = 0) {
		int len = current + startOffset - (start - endOffset);
		char[] res = new char[len];
		Array.Copy(source, start, res, 0, len);
		return res;
	}

	public int Offset() {
		int s = start;
		int c = 0;
		while (source[s] != '\n' && s > 0) {
			--s;
			c++;
		}
		return c;
	}


	/// <summary>
	/// Get the string for this line.
	/// </summary>
	public string GetLineStr() {
		int beginning = start;
		int end = start;
		while (source[beginning] != '\n' && beginning > 0) {
			beginning--;
		}
		if (source[beginning] == '\n') {
			beginning++;
		}
		while (source[end] != '\n' && source[end] != '\r' && end < source.Length) {
			end++;
		}
		return new string(source.AsSpan(beginning, end - beginning));
	}


	public string LexemeString() {
		return new string(ToArray());
	}
}
