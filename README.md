# Cy

A compiler, written using C#, for an asynchronous C-like language that attempts to be simple and clean to use.

Examples of Cy source can be found in the directory, TestFiles.cy. These will be used for testing the compiler and vary from as basic as possible to - eventually - covering all common uses.

The frontend compiler creates llvm ir which can be used with llvm to run immediately or compile to native code, this allows for use with varied architectures and utilises builtin optimisation by llvm.
