Block_End	End
Block_Start	???
Statement_Close	§EOF
Indent	    
### Conditional
##Unary
Negate	NOT {0}
##Binary
And	{0} And {1}
Or	{0} Or {1}
Xor	{0} Xor {1}
Equal	{0} = {1}
NotEqual	NOT {0} = {1}
GreaterThan	{0} > {1}
LessThan	{0} < {1}
GreaterThanOrEqual	{0} > {1} And {0} = {1}
LesserThanOrEqual	{0} < {1} And {0} = {1}
##ShortCircuit
AndAlso	{0} AndAlso {1}
OrElse	{0} OrElse {1}
### Arithmetic
##Unary
#PreInc	++{0}
#PostInc	{0}++
#PreDec	--{0}
#PostDec	{0}--
##Binary
#Add	{0} + {1}
#Sub	{0} - {1}
#Multi	{0} * {1}
#Div	{0} / {1}
#Modulo	{0} % {1}
Assign	{0} = {1}
#########
Dim $$name$$ as $$type$$ 'Line Comment
'Comment
Keywords are case-insensitive

Module Program
    Sub Main()
        Console.WriteLine("Hello, world!")
    End Sub
End Module


' Wrong
Sub MyFunc
$$code$$
End Sub

' Correct
Sub MyFunc
	$$code$$
End Sub


$$cond$$ := W/o () // Both are allowed
Sub Branches
	If $$cond$$ Then $$code$$
	If ($$cond$$) Then $$code$$
	If NOT ($$cond$$) Then $$code$$

	If ($$cond$$) Then
		$$code$$
	End if 'endif is invalid

	'If $$cond$$ Then $$code$$
	'Else ....
	' --> Invalid
	If ($$cond$$) Then
		$$code$$
	ElseIf ($$cond$$) Then
		$$code$$
	Else If ($$cond$$) Then
		$$code$$
	Else
		$$code$$
	End if
End Sub
Sub SwitchCase
	'' Filter := Integer / String
	Select Case $$filter$$§inc
		$$cases$$
	End Select
	'' Single Case
	Case $$value$$§inc
		$$code$$
	'' Default case
	Case Else
		$$code$$
	''
	value :=
		val
		val, value
	val :=
		AtomVal
		AtomVal To AtomVal
	$$value$$ := /(\d+( To \d+)?)(, \d+( To \d+)?)*/
	2
	3,
	4, 5
	5 To
	6, To
	7, To 8
	8, 9 To 10
	9 To 10
	
	
End Sub

Module DevComments

##Define SwitchCase
switch ($$filter$$) {§inc
	$$cases$$§dec
}
##Define Case
case $$value$$§inc
	$$code$$
	break;
##Define MultiCase (Fall through)
case $$value,value,value,...$$
	$$code$$
	break;
##Define Default Case
default:
	$$code$$
	break;
#### Implement that
## MultiCase (C#)
$$§forEach($$values$$)::
case $$item$$:$$§inc
	$$code$$
	break;§dec
###Tokenize
Function
	§forEach(	values	)
	Value
		§n	"case "	item	":"
End Function
§inc	§n	code	§n	"break;"	§dec
## MultiCase (VB.NET)
Case $$§forEach($$values$$)::$$item$$::, $$§inc
	$$code$$
	break;§dec
###Tokenize
"Case "
Function
	§forEach(	values	)
	Value
		item
	ListSeperator (Like, add between every element == not after last)
		", "
End Function
§inc	§n	code	§n	"break;"	§dec
####### Loops
Do §inc
	$$code$$ §dec
Loop Until $$cond$$	'Do until True --> while false
Loop while $$cond$$	'Do until false --> while true

Do Until $$cond$$ §inc	'Do until True --> while false
Do while $$cond$$ §inc	'Do until false --> while true
	$$code$$ §dec
Loop
$$$$ OPTION-->OnlyLiteralsAsForCondition
Dim a as Integer
For a = 0 To 10
	$$code$$
Next
For a = 10 To 0 Step -1
	$$code$$
Next

####
Dim arr As Integer() = { 1, 2, 3 }
Dim i As Integer
For Each i In arr
    ' During each iteration of the For Each loop, i will assume one of three values:
    ' 1, 2, or 3 as integers.
Next i