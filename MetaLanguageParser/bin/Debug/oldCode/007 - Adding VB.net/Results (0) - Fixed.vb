Module Module1
	Sub Main()
		If ((i < 15) & (i > 0)) Then
        	do while ((++i) < 15) 
            	Console.Write(i);
            loop
        Else 
        	do 
            	Console.Write(i);
            loop while (i > 0);
        End If
	End Sub
End Module