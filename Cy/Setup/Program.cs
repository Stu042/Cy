using Cocona;

using Cy.Preprocesor;
using Cy.Preprocesor.Interfaces;

using Microsoft.Extensions.DependencyInjection;


namespace Cy.Setup;
class Program {
	public static int Main(string[] args) {
		var app = CoconaApp.Create(args, options => {
			options.TreatPublicMethodsAsCommands = true;
			options.EnableShellCompletionSupport = true;
		});
		app.AddCommands<CommandLineCommands>();
		app.Run();
		var services = new ServiceCollection();
		ConfigureServices(services);
		var serviceProvider = services
			.AddSingleton<CyCompiler, CyCompiler>()
			.BuildServiceProvider();
		var result = serviceProvider.GetService<CyCompiler>().Compile();
		return result;
	}

	static void ConfigureServices(IServiceCollection services) {
		services.AddSingleton<Config, Config>();
		services.AddScoped<IErrorDisplay, ErrorDisplay>();
		services.AddScoped<Scanner, Scanner>();
		services.AddScoped<ScannerCursor, ScannerCursor>();
		services.AddScoped<Parser, Parser>();
		services.AddScoped<ParserCursor, ParserCursor>();
		services.AddScoped<SymbolTableCreate, SymbolTableCreate>();
		services.AddScoped<CalculateSymbolSizes, CalculateSymbolSizes>();
		services.AddScoped<CalculateSymbolOffsets, CalculateSymbolOffsets>();
		services.AddScoped<SymbolTableDisplay, SymbolTableDisplay>();
    }
}
