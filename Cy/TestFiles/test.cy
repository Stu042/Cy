
Token {
	Position {				// so Position from global is Token.Position but within Token is just called Position
		int line
		int offset
	}
	int tokenType			// type of token
	int literal				// value if a literal
	Position position		// line number and index from start of line
}


MyObject {
	int a
	f64 b
}

int Main() {
	return 2
}
