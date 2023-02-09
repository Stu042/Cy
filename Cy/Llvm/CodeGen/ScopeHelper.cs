using Cy.Types;


namespace Cy.Llvm.CodeGen;


public class ScopeHelper {
	readonly NamespaceHelper _namespaceHelper;
	public string BackendScope;
	public string FrontendScope { get => _namespaceHelper.Current; }


	public ScopeHelper(NamespaceHelper namespaceHelper) {
		_namespaceHelper = namespaceHelper;
		BackendScope = _namespaceHelper.Current;
	}


	public void Enter(string frontendScopeName) {
		_namespaceHelper.Enter(frontendScopeName);
		BackendScope = _namespaceHelper.Current;
	}

	public void Exit() {
		_namespaceHelper.Leave();
		BackendScope = _namespaceHelper.Current;
	}

	/// <summary> Return Current (dot delimited) namespace with inputed current added. </summary>
	public string FullName(string current = null) {
		return _namespaceHelper.FullNamePlus(current);
	}

	/// <summary> Set namespace helper and BackendScope back to start. </summary>
	public void Reset() {
		_namespaceHelper.Reset();
		BackendScope = _namespaceHelper.Current;
	}
}
