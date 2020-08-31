class Data {
public:
	int a;
	float b;

	Data(int aa, float bb) {
		a = aa;
		b = bb;
	}
};

int main() {
	Data d = Data(10, 2);
	return d.a * d.b;
}

