using System;


namespace Cy {
	public partial class Scanner {
		class Cursor {
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
				if ((current + 1) >= source.Length) {
					return '\0';
				}
				return source[current + 1];
			}

			public char[] ToArray(int startOffset = 0, int endOffset = 0) {
				int len = (current + startOffset) - (start - endOffset);
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
				int s = start;
				int l = s;
				while (source[s] != '\n' && s > 0) {
					s--;
					l++;
				}
				s++;
				while (source[l] != '\n' && l < source.Length) {
					l++;
				}
				char[] res = new char[l];
				Array.Copy(source, s, res, 0, l - s);
				return new string(res);
			}


			public override string ToString() {
				return new string(ToArray());
			}
		}
	}
}
