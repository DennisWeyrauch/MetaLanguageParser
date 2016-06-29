Sub Main()
    Dim i As int
    Dim j As int
    j = 0
    If ((i < 15) AND (i > 0)) Then
        Do While (i < 15) 
            i = i + 1
            Console.Write(i)
        Loop
    Else 
        Do 
            i = i - 1
            Console.Write(i)
        Loop While (i > 0)
    End If
End Sub
