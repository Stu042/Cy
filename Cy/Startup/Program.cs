using Cocona;

using Cy.Llvm.CodeGen;
using Cy.Llvm.CodeGen.CompileVisitor;
using Cy.Llvm.Helpers;
using Cy.Preprocesor;
using Cy.Preprocesor.Interfaces;
using Cy.Types;

using Microsoft.Extensions.DependencyInjection;

namespace Cy.Startup;


static class Program {
	public static int Main(string[] args) {
		var app = CoconaApp.Create(args, options => {
			options.TreatPublicMethodsAsCommands = true;
			options.EnableShellCompletionSupport = true;
		});
		app.AddCommands<CommandLineCommands>();
		app.Run();
		var services = new ServiceCollection();
		if (services == null) {
			return -1;
		}
		ConfigureServices(services);
		var serviceProvider = services
			.AddSingleton<CyCompiler, CyCompiler>()
			.BuildServiceProvider();
		if (serviceProvider == null) {
			return -2;
		}
		var cyCompilerService = serviceProvider.GetService<CyCompiler>();
		if (cyCompilerService == null) {
			return -3;
		}
		var result = cyCompilerService.Compile();
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
		services.AddScoped<TypeTableBuilderHelper, TypeTableBuilderHelper>();
		services.AddScoped<TypeTableCreateVisitorOptions, TypeTableCreateVisitorOptions>();
		services.AddScoped<CodeHelper, CodeHelper>();
		services.AddScoped<BackendTypeHelper, BackendTypeHelper>();
		services.AddScoped<Compiler, Compiler>();
		services.AddScoped<CompileVisitor, CompileVisitor>();
		services.AddScoped<SymbolTable, SymbolTable>();
		services.AddScoped<ScopeHelper, ScopeHelper>();
		services.AddSingleton<NamespaceHelper, NamespaceHelper>();
		services.AddSingleton<TypeTable, TypeTable>();
		services.AddSingleton<LlvmMacros, LlvmMacros>();
	}
}
