using System;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using Xunit;



namespace CyTests {
	public class ErrorDisplayTests {
		Type type;
		object instance;
		MethodInfo buildErrorLine;
		MethodInfo buildPointerLine;
		MethodInfo buildInfoText;		// BuildInfoText(int line, int offset, int tabSize)

		public ErrorDisplayTests() {
			type = typeof(Cy.ErrorDisplay);
			instance = Activator.CreateInstance(type);
			buildErrorLine = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
				.Where(x => x.Name == "BuildErrorLine" && x.IsPrivate)
				.First();
			buildPointerLine = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
				.Where(x => x.Name == "BuildPointerLine" && x.IsPrivate)
				.First();
			buildInfoText = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
				.Where(x => x.Name == "BuildInfoText" && x.IsPrivate)
				.First();
		}


		[Fact]
		public void Test_BuildErrorLine() {
			string lineText = "\tblah";
			int line = 1;
			int offset = 1;
			int tabSize = 4;
			var errorLine = (string)buildErrorLine.Invoke(instance, new object[] { lineText, line, offset, tabSize });
			errorLine.Should().Be("    blah");
		}

		[Fact]
		public void Test_BuildInfoText() {
			int line = 1;
			int offset = 1;
			int tabSize = 4;
			var infoText = (string)buildInfoText.Invoke(instance, new object[] { line, offset, tabSize });
			infoText.Should().Be("    1|1 ");
		}

		// "    1|1     blah"
		// "------------^"
		[Fact]
		public void Test_BuildPointerLine() {
			string lineText = "\tblah";
			int line = 1;
			int offset = 1;
			int tabSize = 4;
			var infoText = (string)buildInfoText.Invoke(instance, new object[] { line, offset, tabSize }); 
			var pointerLine = (string)buildPointerLine.Invoke(instance, new object[] { infoText + lineText, offset + infoText.Length, tabSize });
			pointerLine.Should().Be("------------^");
		}
	}
}

/*
      var firstName = "John";
      var lastName = "Doe";

      Type type = typeof(Hello);
      var hello = Activator.CreateInstance(type, firstName, lastName);
      MethodInfo method = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
      .Where(x => x.Name == "HelloMan" && x.IsPrivate)
      .First();
*/
