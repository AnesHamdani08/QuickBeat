Imports QuickBeat.Interfaces
Imports QuickBeat.Utilities

Namespace Player
    <Serializable>
    Public Class InternalMediaProvider
        Implements IMediaProvider

        Public Property Name As String = "Internal" Implements IMediaProvider.Name

        Public Property Author As String = "Anes08" Implements IMediaProvider.Author

        Public Property Version As New Version(1, 0, 0, 0) Implements IMediaProvider.Version

        Public Property Token As String Implements IMediaProvider.Token

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="Token">The relative path to the resource</param>
        Sub New(Token As String)
            Me.Token = Token
        End Sub
#Disable Warning
        Public Async Function Fetch() As Task(Of String) Implements IMediaProvider.Fetch
            Return Token
        End Function

        Public Async Function FetchThumbnail() As Task(Of ImageSource) Implements IMediaProvider.FetchThumbnail
            Dim ResStream = Application.GetResourceStream(New Uri(Token, UriKind.Relative))
            If ResStream IsNot Nothing Then
                Dim Tags As TagLib.File = Nothing
                Try
                    Tags = TagLib.File.Create(New Classes.FileBytesAbstraction(IO.Path.GetFileName(Token), ResStream.Stream), TagLib.ReadStyle.Average)
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