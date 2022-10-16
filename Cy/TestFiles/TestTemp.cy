
KindaString {
	i8 a
	i8 b
	i8 c
	i8 d
}

Token {
	Position {				// so Position from global is Token.Position but within Token is just called Position
		int line
		int offset
	}
	int tokenType			// type of token
	KindaString lexeme		// actual text of token from source
	int literal				// value if a literal
	Position position		// line number and index from start of line
	KindaString filename	// source filename
}

