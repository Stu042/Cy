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
// Gotta have a hello world program, also shows the format of Mains input arguments
int Main(str[] args):
    print("Hello world\n")
    return 0
```

```cy
// Recursion will be possible and sized types are shown here
int32 Main():
    return Factorial(42)

int32 Factorial(int32 num):
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

    void Hello():
        print("{name} says hi.\n")
	
    int NameLength():
        return name.length	// str is an array of int8, arrays have a member named, length
```

### Resources

The code in this project has been heavily influenced by https://github.com/munificent/craftinginterpreters

## Specification

### Entry point

Program entry is at the function Main() in the global namespace.

Main() will always run on thread #0

Main() can be defined in several ways:

- `void Main():`
- `int Main():`
- `int32 Main():`
- `int Main(str[] args):`

### Types

#### Basic Types

Most basic types have the type (starting with a lowercase letter) followed by a number to indicate bit size. Bit size must be a power of 2:

- `int8`
- `int16`
- `int32`
- `int64`
- `int128`
- `float16`
- `float32`
- `float64`
- `float128`
- `str`
- `void`

`int` and `float` are also basic types and will be sized to suit ideal performance of the targets architecture. For a 64 bit Windows or Linux system these would be 32 bits and 64 bits respectively. 32 bits for the default int is not the obvious size (as its a 64 bit machine and OS) but this is likely to be large enough for most uses and should improve CPU cache performance. Strings - initially - are an array of 8 bit ASCII characters, UTF8 and UTF16 will be added at a later stage.

#### Arrays

Arrays can be defined as:

`int[] a`

`int8[] someChars`

Note int8[] and str are synonymous.

##### Built-in Members for Arrays

`a.length` - will give the length of the array held in variable a

### Functions

The main function is defined as above and other functions can be defined in a similar manner.

Example function definition:

`int Factorial(int n): #1`

The factorial function will (by default) run on thread #1, will return an integer of default size and takes an integer of default size as input.  Main always runs on thread #0.

Functions should start with a capital alphabetic character, but can then contain numbers and underscore.

The colon after the brackets can be thought of as other important information will be coming next, in the case of a function this is the functional component.

The thread number, indicated by #1 above, it is not required and will be overridden if the function is asked to run on a different thread.

`int result = Factorial(10) #3`

The above code will call the Factorial function with input of 10, will ask it to run on thread #3. result will not equal a number until Factorial has completed executing. Called like this result will be given a Mutex to control this. If result is used immediately the calling thread will pause until result has a value.

`Factorial(10) #3 FactorialCallback(int result)`

Called like this FactorialCallback() will be called, on the default thread (#0), with the result. Callbacks must always have only the output result as an input argument. but can be asked to run on a different thread by appending `#<thread number>`

### Example Function

```cy
int Factorial(int n):
    if (n <= 1)
        return 1
    return n * Factorial(n-1)
```

### Working With Threads

To help working with threads the below commands are available to help:

- `#this` - this thread

- `#count` - count of available threads

- `#set 5` - must be only one instance in the program. Will ask the start-up code to create this amount of threads to be available for use.

- `#set default` - This will create the recommended amount of threads for the running architecture. Again must be only one instance of this in the program.

The #set command must be placed in the global namespace and recommended to be first line of the file containing Main().

### Using Cy to add a - large - array of integers using multiple threads.

```cy
#set default                                                                // allow runtime to calculate the best amount of threads to use

int64 Main(str[] args):
    int[] values = int[args.length - 1]                                     // create an uninitiliased array of ints
    for (int idx = 1; idx < args.length; idx++)                             // loop over all input - except app name
        values[idx - 1] = arg.ToInt()                                       // convert arg to an integer and add to array
    int64 total = CalculateTotal(int[] values)                              // total an int array
    return total                                                            // and the final result


int64 CalculateTotal(int[] values):
    int blockSize = values.length / #count                                  // calculate the amount of values each thread will use
    int blockStart = 0
    int64[] totals = int[#count]                                            // a place to store intermediate results
    for (int thread = 1; thread < #count; thread++):                        // for each thread
        int blockEnd = blockStart + blockSize                               // calculate end block index
        totals[thread] = TotalValues(values[blockStart..blockEnd]) #thread  // total these values by calling TotalValues() on thread numbered thread
        blockStart = blockEnd                                               // setup ready for next block
    totals[0] = TotalValues(values[blockStart..])                           // run final block on this thread - we would be waiting for results anyway
    int64 total = 0                                                         // declare final total
    int64 aTotal = each totals:                                             // let aTotal equal each intermediate total value
        total += aTotal                                                     // add the intermediate totals
    return total		


int64 TotalValues(int[] values):
    int64 total = 0
    int value = each values:
        total += value
    return value
```

The line `totals[thread] = TotalValues(values[blockStart..blockEnd]) #thread` will create a job for the thread pool to call `TotalValues` with a copy of the range of values.
`totals[thread]` will have a mutex automatically added to prevent reading the value before it is ready.
`totals[0]` in `totals[0] = TotalValues(values[blockStart..])` will not require a mutex as this is called on the same thread, although as it is part of an array that requires mutexes it can be created and ignored by the compiler.

Statements end with a new line and not a semicolon. A function or object ends when the indentation is same or less than the definition or when the file ends.

Indentation must be by tab, spaces are not counted.

### Objects

```cy
AnObject:
    int a
    float32 b

    AnObject(int a, float32 bb):
        this.a = a
        b = bb

    int Add():
        return a + b

```

Objects are defined by a name followed by a colon, then the contents. Objects can contain members and functions. A function by the same name as the Object will be used as a constructor. A destructor definition will look the same but will start with a tilde '~'.

Within an objects methods `this` is available to refer to this object and references are always denoted by a dot followed by the member or method name as in `this.a` in the example above. Or in a function utilising the above object:

```cy
AnObject obj
obj.a = 5
```

All members of objects are public, all members of functions are private. Attributes will be added at a later stage to allow object members to be private.

### Internals

How does Cy handle calling functions on different threads? Well each function that could be run on a thread - other than thread #0 - has two entry points.

First entry point is similar to standard C (cdecl).

The second entry point is designed to unwrap the input arguments from a void* to the expected C (cdecl) format.

If a function calls another on its own thread then the first entry point is used, otherwise a Job is created and sent to the JobQueue. At Job creation a shallow copy of the input arguments is taken and this is packed up in aside a void* which will be passed to the second entry point of the called function. Then the JobQueue will dispatch the Job to the requested - or first available - thread as soon as possible.

If the result of this function is to be assigned to a variable then a mutex will be automatically added to that variable and the calling thread will lock when that variable is attempted to be read or used.

If the return value it to be sent to a callback function and that function is on the same thread then it is called directly, otherwise if the callback function is to be run on a separate thread a new Job will be created and dispatched as soon as possible after the initial function is finished.

If no return value is to be assigned and no callback to be called with the result, the function will run and end with no notification given to the calling thread.

