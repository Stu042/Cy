using System;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using Xunit;



namespace CyTests {
	public class ErrorDisplayTests {
		readonly Type type;
		readonly object instance;
		readonly MethodInfo buildErrorLine;
		readonly MethodInfo buildPointerLine;
		readonly MethodInfo buildInfoText;		// BuildInfoText(int line, int offset, int tabSize)

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

		//"    blah"
		//"    1|1 "
		//"    1|1     blah"
		//"------------^"
		[Fact]
		public void Test_BuildErrorLine_offset1() {
			string lineText = "\tblah";
			int line = 1;
			int offset = 1;
			int tabSize = 4;
			var errorLine = (string)buildErrorLine.Invoke(instance, new object[] { lineText, line, tabSize });
			errorLine.Should().Be("    blah");
			var infoText = (string)buildInfoText.Invoke(instance, new object[] { line, offset, tabSize });
			infoText.Should().Be("    1|1 ");
			var pointerLine = (string)buildPointerLine.Invoke(instance, new object[] { infoText, lineText, offset, tabSize });
			pointerLine.Should().Be("------------^");
		}

		//"    blah"
		//"    1|4 "
		//"    1|4     blah"
		//"---------------^"
		[Fact]
		public void Test_BuildErrorLine_offset4() {
			string lineText = "\tblah";
			int line = 1;
			int offset = 4;
			int tabSize = 4;
			var errorLine = (string)buildErrorLine.Invoke(instance, new object[] { lineText, line, tabSize });
			errorLine.Should().Be("    blah");
			var infoText = (string)buildInfoText.Invoke(instance, new object[] { line, offset, tabSize });
			infoText.Should().Be("    1|4 ");
			var pointerLine = (string)buildPointerLine.Invoke(instance, new object[] { infoText, lineText, offset, tabSize });
			pointerLine.Should().Be("---------------^");
		}
		//"    blah"
		//"    1|0 "
		//"    1|0     blah"
		//"--------^"
		[Fact]
		public void Test_BuildErrorLine_offset0() {
			string lineText = "\tblah";
			int line = 1;
			int offset = 0;
			int tabSize = 4;
			var errorLine = (string)buildErrorLine.Invoke(instance, new object[] { lineText, line, tabSize });
			errorLine.Should().Be("    blah");
			var infoText = (string)buildInfoText.Invoke(instance, new object[] { line, offset, tabSize });
			infoText.Should().Be("    1|0 ");
			var pointerLine = (string)buildPointerLine.Invoke(instance, new object[] { infoText, lineText, offset, tabSize });
			pointerLine.Should().Be("--------^");
		}

		// should error
		[Fact]
		public void Test_BuildErrorLine_offsetMinus1() {
			string lineText = "\tblah";
			int line = 1;
			int offset = -1;
			int tabSize = 4;
			var infoText = (string)buildInfoText.Invoke(instance, new object[] { line, offset, tabSize });
			var pointerLine = (string)buildPointerLine.Invoke(instance, new object[] { infoText, lineText, offset, tabSize });
			pointerLine.Should().Be("Error offset is out of range, offset -1, line:     1|-1 \tblah");
		}

		// should error
		[Fact]
		public void Test_BuildErrorLine_offset5() {
			string lineText = "\tblah";
			int line = 1;
			int offset = 5;
			int tabSize = 4;
			var infoText = (string)buildInfoText.Invoke(instance, new object[] { line, offset, tabSize });
			var pointerLine = (string)buildPointerLine.Invoke(instance, new object[] { infoText, lineText, offset, tabSize });
			pointerLine.Should().Be("Error offset is out of range, offset 5, line:     1|5 \tblah");
		}
	}
}
