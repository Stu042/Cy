# Cy

A compiler, written using C#, for an asynchronous C-like language that attempts to be simple and clean to use.

Examples of Cy source can be found in the directory, TestFiles.cy. These will be used for testing the compiler and vary from as basic as possible to - eventually - covering all common uses.

The frontend compiler creates llvm ir which can be used with llvm to run immediately or compile to native code, this allows for use with varied architectures and able to utilise optimisation by llvm.



## Example Code

```cy
// the simplest program
int Main():
	return 42
```

```cy
// Gotta have a hello world program, also showing string arrays are possible
int Main(str[] argv):
	print("Hello world\n")
	return 0
```

```cy
// Recursion will be possible and sized types are shown here
i32 Main():
	return Factorial(42)

i32 Factorial(i32 num):
	if (num > 1)
		return num * Factorial(num - 1)
	return num
```

```cy
// A simple class example
int Main():
	User user = User("Stu")
	user.Hello()
	print("User has {user.NameLength()} characters.\n")
	return 0


User:
	str name
	
	User(str name):
		this.name = name

	Hello():
		print("{name} says hi.\n")
	
	int NameLength():
		return name.count	// str is an array of i8, arrays have the member variable, count
```
