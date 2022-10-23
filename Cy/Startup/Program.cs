using Cocona;

using Cy.Preprocesor;
using Cy.Preprocesor.Interfaces;
using Cy.Types;

using Microsoft.Extensions.DependencyInjection;


namespace Cy.Setup;

static class Program {
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
		services.AddScoped<TokenDisplay, TokenDisplay>();
		services.AddScoped<Scanner, Scanner>();
		services.AddScoped<ScannerCursor, ScannerCursor>();
		services.AddScoped<Parser, Parser>();
		services.AddScoped<ParserCursor, ParserCursor>();
		services.AddScoped<TypeTableCreate, TypeTableCreate>();
		services.AddScoped<TypeTableCreateVisitor, TypeTableCreateVisitor>();
		services.AddScoped<Types.TypeTableBuilderHelper, Types.TypeTableBuilderHelper>();
		services.AddScoped<NamespaceHelper, NamespaceHelper>();
		services.AddSingleton<Types.TypeTable, Types.TypeTable>();
		services.AddScoped<TypeTableCreateVisitorOptions, TypeTableCreateVisitorOptions>(); 
	}
}
