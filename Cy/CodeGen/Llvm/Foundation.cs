using Cy.Setup;

using System.Diagnostics;
using System.IO;
using System.Text;

namespace Cy.CodeGen.Llvm;

public class Foundation {
	readonly Config _config;
	readonly string[] fullPreCodeSections;

	public Foundation(Config config) {
		_config = config;
		var fileName = Path.GetRandomFileName();
		string cfilename = fileName + ".c";
		string llfileName = fileName + ".ll";
		using (StreamWriter sw = File.CreateText(cfilename)) {
			sw.WriteLine(string.Empty);
		}
		RunCmd(_config.ClangExe, $"{cfilename} -S -emit-llvm");
		fullPreCodeSections = File.ReadAllLines(llfileName);
		for (var idx = 0; idx < fullPreCodeSections.Length; idx++) {
			fullPreCodeSections[idx] = fullPreCodeSections[idx].Replace(cfilename, _config.Input[0]);
		}
		RunCmd("cmd.exe", $" /C del /f {cfilename}");
		RunCmd("cmd.exe", $" /C del /f {llfileName}");
	}

	public string GetPreLLVMCode() {
		var bob = new StringBuilder();
		string modulestr = FindSection("ModuleID");
		bob.AppendLine(modulestr);
		string srcstr = FindSection("source_filename");
		bob.AppendLine(srcstr.Replace("\\\\", "\\"));
		string datalayoutstr = FindSection("target datalayout");
		bob.AppendLine(datalayoutstr);
		string triplestr = FindSection("target triple");
		bob.AppendLine(triplestr);
		bob.AppendLine();
		return bob.ToString();
	}

	public string GetPostLLVMCode() {
		return "\n" +
			"!llvm.module.flags = !{!0, !1}\n" +
			"!llvm.ident = !{!2}\n\n" +
			"!0 = !{i32 1, !\"wchar_size\", i32 2}\n" +
			"!1 = !{i32 7, !\"PIC Level\", i32 2}\n" +
			"!2 = !{!\"cy version " + _config.Version + "\"}\n\n" +
			"attributes #0 = { noinline nounwind optnone uwtable \"correctly-rounded-divide-sqrt-fp-math\"=\"false\" \"disable-tail-calls\"=\"false\" \"frame-pointer\"=\"none\" \"less-precise-fpmad\"=\"false\" \"min-legal-vector-width\"=\"0\" \"no-infs-fp-math\"=\"false\" \"no-jump-tables\"=\"false\" \"no-nans-fp-math\"=\"false\" \"no-signed-zeros-fp-math\"=\"false\" \"no-trapping-math\"=\"false\" \"stack-protector-buffer-size\"=\"8\" \"target-cpu\"=\"x86-64\" \"target-features\"=\"+cx8,+fxsr,+mmx,+sse,+sse2,+x87\" \"unsafe-fp-math\"=\"false\" \"use-soft-float\"=\"false\" }\n";
	}


	void RunCmd(string command, string args) {
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

	string FindSection(string needle) {
		foreach (string str in fullPreCodeSections) {
			if (str.IndexOf(needle) != -1) {
				return str;
			}
		}
		return string.Empty;
	}
}


/* https://stackoverflow.com/questions/7272988/convert-hex-to-double/30580263
public static double DoubleFromHex(string hex)
{
    int exponent;
    double result;
    string doubleexponenthex = hex.Substring(0, 3);
    string doublemantissahex = hex.Substring(3);
    double mantissavalue = 1; //yes this is how it works

    for (int i = 0; i < doublemantissahex.Length; i++)
    {
        int hexsignvalue = Convert.ToInt32(doublemantissahex.Substring(i, 1),16); //Convert ,16 Converts from Hex
        mantissavalue += hexsignvalue * (1 / Math.Pow(16, i+1));
    }

    exponent = Convert.ToInt32(doubleexponenthex, 16);
    exponent = exponent - 1023;  //just how it works

    result = Math.Pow(2, exponent) * mantissavalue;
    return result;
}
*/