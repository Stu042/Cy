using Cy.Llvm.CodeGen;
using Cy.Types;

using System;


namespace Cy.Llvm.Helpers;


public class BackendTypeHelper {
	public static string FrontendTypeToBackend(FrontendType type) {
		string typeStr;
		switch (type.Format) {
			case Enums.FrontendTypeFormat.Void:
				typeStr = "void";
				break;
			case Enums.FrontendTypeFormat.Bool:
				typeStr = "i8";
				break;
			case Enums.FrontendTypeFormat.Int:
				typeStr = $"i{type.BitSize}";
				break;
			case Enums.FrontendTypeFormat.Float:
				switch (type.BitSize) {
					case 16:
						typeStr = "half";
						break;
					case 32:
						typeStr = "float";
						break;
					case 64:
						typeStr = "double";
						break;
					default:
						throw new Exception("Size mismatch when converting FrontendType to floating point Llvm type.");
				}
				break;
			case Enums.FrontendTypeFormat.String:
				typeStr = "i8*";
				break;
			default:
				throw new Exception("Unable to convert FrontendType to Llvm type.");
		}
		return typeStr;
	}
	public static string FrontendTypeToBackend(Instance instance) {
		string typeStr;
		switch (instance.FrontendType.Format) {
			case Enums.FrontendTypeFormat.Void:
				typeStr = "void";
				break;
			case Enums.FrontendTypeFormat.Bool:
				typeStr = "i8" + new string('*', instance.IndirectionLevels);
				break;
			case Enums.FrontendTypeFormat.Int:
				typeStr = $"i{instance.FrontendType.BitSize}" + new string('*', instance.IndirectionLevels);
				break;
			case Enums.FrontendTypeFormat.Float:
				switch (instance.FrontendType.BitSize) {
					case 16:
						typeStr = "half" + new string('*', instance.IndirectionLevels);
						break;
					case 32:
						typeStr = "float" + new string('*', instance.IndirectionLevels);
						break;
					case 64:
						typeStr = "double" + new string('*', instance.IndirectionLevels);
						break;
					default:
						throw new Exception("Size mismatch when converting FrontendType to floating point Llvm type.");
				}
				break;
			case Enums.FrontendTypeFormat.String:
				typeStr = $"i8*" + new string('*', instance.IndirectionLevels);
				break;
			default:
				throw new Exception("Unable to convert FrontendType to Llvm type.");
		}
		return typeStr;
	}

	//public string Allocate(Instance instance) {
	//}
}
