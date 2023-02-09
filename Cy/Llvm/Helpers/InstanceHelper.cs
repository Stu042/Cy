namespace Cy.Llvm.Helpers;


/// <summary> Create unique names for each instance in llvmir. </summary>
public class InstanceHelper {
	int instanceCount;

	public InstanceHelper() {
		instanceCount = 0;
	}

	public string NewName() {
		var newName = $"%{instanceCount++}";
		return newName;
	}
	public void InstanceNameInc() {
		instanceCount++;
	}
}
