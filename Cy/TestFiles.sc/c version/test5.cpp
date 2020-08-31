class Data {
	public:
		int a;
		float b;
};

int main() {
	Data *d = new Data();
	d->a = 5;
	d->b = 0.2;
	int ans = d->a * d->b;
	delete d;
	return ans;
}

