using Cocona;


namespace Cy.Setup;

public class CommandLineCommands {
	// -STAv -o temp.c -i test.cy
	public void Cy(
		[Option('i', Description = "Input files to compile.")] string[] input,
		[Option('A', Description = "Display parser generated ASTs.")] bool ast = false,
		[Option('v', Description = "Verbose output.")] bool verbose = false,
		[Option('T', Description = "Display scanner generated tokens.")] bool tokens = false,
		[Option('I', Description = "Includes to use.")] string[] includes = null,
		[Option('o', Description = "Output file name.")] string output = "main.c",
		[Option('s', Description = "Tab size (in spaces).")] int tabSize = 4,
		[Option('S', Description = "Display Symbol Table.")] bool preCompileSymbols = false) {
		CommandLineInput.Instance.Init(includes, input, output, tokens, verbose, ast, tabSize, preCompileSymbols);
	}
}


/*
class Program
{
    public Program(ILogger<Program> logger)
    {
        logger.LogInformation("Create Instance");
    }

    static void Main(string[] args)
    {
        CoconaApp.Create()
            .ConfigureServices(services =>
            {
                services.AddTransient<MyService>();
            })
            .Run<Program>(args);
    }

    public void Hello([FromService]MyService myService)
    {
        myService.Hello("Hello Konnichiwa!");
    }
}

class MyService
{
    private readonly ILogger _logger;

    public MyService(ILogger<MyService> logger)
    {
        _logger = logger;
    }

    public void Hello(string message)
    {
        _logger.LogInformation(message);
    }
}
*/