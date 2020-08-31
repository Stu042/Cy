
class Data
{
public:
	int a;
	float b;

	Data(int aa, float bb)
	{
		a = aa;
		b = bb;
	}
};

int main()
{
	Data *d = new Data(10, 2);
	int ans = d->a * d->b;
	delete d;
	return ans;
}
