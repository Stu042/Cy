using System.Diagnostics;

namespace Cy.Utils;
public static class Command {
	public static void Run(string command, string args) {
		var process = new Process {
			StartInfo = new ProcessStartInfo {
				WindowStyle = ProcessWindowStyle.Hidden,
				FileName = command,
				Arguments = args
			}
		};
		process.Start();
		process.WaitForExit();
	}
}
