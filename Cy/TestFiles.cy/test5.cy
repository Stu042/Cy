Data:
	int a
	int b

	Data(int aa, int bb):
		a = aa
		b = bb

	int Mult():
		return a*b

int Main():
	Data d = Data(10, 2)
	//return d.a * d.b
	return d.Mult()


