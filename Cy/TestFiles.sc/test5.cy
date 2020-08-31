Data:
	int a
	int b

	Data(int aa, int bb):
		a = aa
		b = bb


int Main():
	Data d = Data(10, 2)
	return d.a * d.b

