Namespace Utilities
    Public Class GenericDebugListener
        Inherits TraceListener

        Public Overrides Sub Write(message As String)
            Console.Write(message)
        End Sub

        Public Overrides Sub WriteLine(message As String)
            Console.WriteLine(message)
        End Sub
    End Class
End Namespace