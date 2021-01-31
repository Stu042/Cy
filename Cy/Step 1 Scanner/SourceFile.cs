using System;


namespace Cy {
	class SourceFile {
		char[] source;
		public int start { get; private set; }
		int current;

		// prep source file
		public SourceFile(string text) {
			source = text.ToCharArray();
			start = 0;
			current = 0;
		}

		// start scanning for a new token
		public void Start() {
			start = current;
		}

		// is this the char you are looking for
		public bool IsMatch(char expected) {
			if (IsAtEnd() || source[current] != expected)
				return false;
			current++;
			return true;
		}

		// is end of file
		public bool IsAtEnd() {
			return current >= source.Length;
		}

		// next char
		public char Advance() {
			return source[current++];
		}

		// peek this char
		public char Peek() {
			if (IsAtEnd())
				return '\0';
			return source[current];
		}

		// peek next char
		public char PeekNext() {
			if ((current + 1) >= source.Length)
				return '\0';
			return source[current + 1];
		}

		// return current match as an array of chars (from start to offset)
		public char[] ToArray(int startOffset = 0, int endOffset = 0) {
			int len = (current + startOffset) - (start - endOffset);
			char[] res = new char[len];
			Array.Copy(source, start, res, 0, len);
			return res;
		}

		// index into the line of this matches start
		public int OffsetWithinLine() {
			int s = start;
			int c = 0;
			while (source[s] != '\n' && s > 0) {
				--s;
				c++;
			}
			return c;
		}


		// Get the string for this line.
		public string GetLineStr(int line) {
			int s = start;
			int l = s;
			while (source[s] != '\n' && s > 0) {
				s--;
				l++;
			}
			s++;
			while (source[l] != '\n' && l < source.Length)
				l++;
			char[] res = new char[l];
			Array.Copy(source, s, res, 0, l - s);
			return new string(res);
		}


		public override string ToString() {
			return new string(ToArray());
		}

	}

}

