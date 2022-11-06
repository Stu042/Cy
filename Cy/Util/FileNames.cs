using System.IO;

namespace Cy.Util;


/// <summary> Set a file name and return name with any file extension we are likely to use. </summary>
public class FileNames {
	public string Name;
	public string Llvm { get => Name + ".ll"; }
	public string Cy { get => Name + ".cy"; }
	public FileNames(string name) {
		Name = Path.GetFileNameWithoutExtension(name);
	}
}

