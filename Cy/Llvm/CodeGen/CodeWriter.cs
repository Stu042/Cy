
using System.Text;

namespace Cy.Llvm.CodeGen;

public class CodeWriter {
	readonly StringBuilder allocator;
	readonly StringBuilder code;

	public CodeWriter() {
		allocator = new StringBuilder();
		code = new StringBuilder();
	}

	public void Allocate(string str) {
		allocator.AppendLine(str);
	}
	public void Code(string str) {
		code.AppendLine(str);
	}

	public override string ToString() {
		return allocator.ToString() + code.ToString();
	}
}
