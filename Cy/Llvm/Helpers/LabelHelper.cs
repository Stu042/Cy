namespace Cy.Llvm.Helpers;


/// <summary> Create unique labels for llvmir. </summary>
public class LabelHelper {
	int labelCount;

	public LabelHelper() {
		labelCount = 0;
	}

	public string NewLabel() {
		var newName = $"{labelCount++}:";
		return newName;
	}
}
