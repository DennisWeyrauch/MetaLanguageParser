Module Module1
    Sub Main()
        Dim enemy1 As String
        Dim enemy2 As String
        Dim dist1 As int
        Dim dist2 As int
        Do While (true) 
            enemy1 = Console.ReadLine() dist1 = Console.ReadLine() enemy2 = Console.ReadLine() dist2 = Console.ReadLine() If (dist1 < dist2) Then
                Console.WriteLine(enemy1)
            Else 
                Console.WriteLine(enemy2)
            End If
        Loop
    End Sub
    

End Module
