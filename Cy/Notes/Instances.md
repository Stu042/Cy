# Instances

## Summary

Used by Llvm and code generator to track active instances of variables/members/properties.

Can refer to values that dont belong to a variable in the source code, ie. intermediate value of an arithmetic operation.

## Instances require:

Scopped, when we leave a scope all instances belonging to that scope are removed. When searching, current scope then previous scopes are searched.

	* Llvm instance name, %1, %2 or @Main @FunctionName
	* Full name in source code - only used to find instances
	* Is this a literal value
	* Type information
		* Type name in source code
		* int8, int32, float64, object, etc
		* Size in bytes
		* Size in bits
		* Base of this type, no sizing information
		* Tokens defining this type, if any
		* Offset in this scope, used for members of objects
		* Access modifier, public, private, etc
		* Parent type (all types are linked)
		* Child types

