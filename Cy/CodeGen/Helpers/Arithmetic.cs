using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Cy.CodeGen.Helpers {
	public class Arithmetic {

		ExpressionValue Add(ExpressionValue left, ExpressionValue right, TokenGenerator.TokenType tokenType, Options opts) {
			if (left.IsLiteral && right.IsLiteral) {
				var value = ExpressionValue.AddLiteral(left, right);
				return new ExpressionValue {
					IsLiteral = true,
					TextRepresentation = value.ToString(),
					Value = value,
					BaseType = ExpressionValue.GetBaseType(left, right)
				};
			}
//			if (!left.IsLiteral) {
				var lhsInstance = opts.LlvmInstance.NewTempInstance();
				var val = ExpressionValue.AddLiteral(left, right);	// change to add blah
				//opts.Code.Code($"{opts.Tab.Show}{lhsInstance} = load {left.LlvmType}, {left.LlvmType}* {left.Instance.}, align {left.Instance.Align}");
				return new ExpressionValue {
					IsLiteral = true,
					TextRepresentation = val.ToString(),
					Value = val,
					BaseType = ExpressionValue.GetBaseType(left, right)
				};
			//}
		}
	}
}
