using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cy {

	// miscellaneous stuff to help all
	static class Util {

		// run a command in a shell, waits for completion of command before returning
		public static void RunCmd(string args) {
			System.Diagnostics.Process process = new System.Diagnostics.Process();
			System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo {
				WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden,
				FileName = "cmd.exe",
				Arguments = "/C " + args
			};
			process.StartInfo = startInfo;
			process.Start();
			process.WaitForExit();
		}

		// find a string in an array of strings, even if string only matches a part of a single array item
		public static string FindStrInArray(string needle, string[] haystack) {
			foreach (string str in haystack) {
				if (str.IndexOf(needle) != -1)
					return str;
			}
			return "";
		}



	}

}
