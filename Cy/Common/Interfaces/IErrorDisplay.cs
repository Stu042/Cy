using Cy.Scanner;

namespace Cy.Common.Interfaces {
	public interface IErrorDisplay {
		public void Error(string filename, int line, int offset, string lineText, string message);
		public void Error(Token tok, string message);
		public void Error(Token token, string linestr, string message);
	}
}