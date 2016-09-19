'Added comments, testing out multilude of DataTypes
Public Class Program
    /* Fields */
    /* Prop */
    /* Ctor */
    /* Cctor */
    /* Dtor */
    
    Sub Main()
        Dim j As Integer
        j = 0
        Dim i As Integer
        i = 7
        Dim dsas As _MISSING_Ratiopharm
        
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
    End
    

End

Class myClass
    /* Fields */
    /* Prop */
    /* Ctor */
    /* Cctor */
    /* Dtor */
    
    Sub Main()
        Dim i As Integer
        i = 0
        
        'Inner Method Comments!
        Console.Write(i)
    End
    

End

