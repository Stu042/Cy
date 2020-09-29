Data:
	int a
	int b

	Data(int a, int b):
		this.a = a
		this.b = b


int Main():
	Data d = Data(10, 2)
	return d.a * d.b


