
## Array {3} = UpperBound
ArrayDecl	Dim {1}({3}) As {0}
ArrayStart	0
## {4} = CSV of contents
ArrayDef	Dim {1}() As {0} = New {0}({3}) \{{4}\}
#ArrayNew
#	Dim MyArray As System.Array = System.Array.CreateInstance(GetType(String), 4)
ArrayNew	Dim {1} As System.Array = System.Array.CreateInstance(GetType({0}), {3})
ArraySet	{Name}({Index}) = {Value}
ArrayGet	{Name}.GetValue({Index})
ArrayLen	{Name}.Length
<%##Iterator
	Dim En As System.Collections.IEnumerator
	En = {Array}.GetEnumerator
	Do While En.MoveNext
		Console.WriteLine(En.Current())
	Loop
##%>
