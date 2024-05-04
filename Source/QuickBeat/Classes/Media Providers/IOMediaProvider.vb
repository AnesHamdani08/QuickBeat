Imports QuickBeat.Interfaces
Imports QuickBeat.Utilities

Namespace Player
    <Serializable>
    Public Class IOMediaProvider
        Implements IMediaProvider

        Public Property Name As String = "I/O" Implements IMediaProvider.Name

        Public Property Author As String = "Anes08" Implements IMediaProvider.Author

        Public Property Version As New Version(1, 0, 0, 0) Implements IMediaProvider.Version

        Public Property Token As String Implements IMediaProvider.Token

        Sub New(Token As String)
            Me.Token = Token
        End Sub
#Disable Warning
        Public Async Function Fetch() As Task(Of String) Implements IMediaProvider.Fetch
            Return Token
        End Function

        Public Async Function FetchThumbnail() As Task(Of ImageSource) Implements IMediaProvider.FetchThumbnail
            If IO.File.Exists(Token) Then
                Dim Tags As TagLib.File = Nothing
                Try
                    Tags = Utilities.SharedProperties.Instance.RequestTags(Token, TagLib.ReadStyle.Average)
                Catch ex As Exception
                    Utilities.DebugMode.Instance.Log(Of Metadata)("Error while reading tags: " & ex.ToString)
                End Try
                If Tags IsNot Nothing AndAlso Tags.Tag.Pictures.Length > 0 Then
                    For Each picture In Tags.Tag.Pictures
                        Dim BI = New IO.MemoryStream(picture.Data.Data).ToBitmapSource
                        Tags.Dispose()
                        Return BI
                    Next
                End If
            End If
            Return Nothing
        End Function
#Enable Warning
    End Class
End Namespace