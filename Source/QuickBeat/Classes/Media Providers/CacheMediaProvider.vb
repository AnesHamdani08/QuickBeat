Imports QuickBeat.Interfaces
Imports QuickBeat.Utilities

Namespace Player
    <Serializable>
    Public Class CacheMediaProvider
        Implements IMediaProvider

        Public Property Name As String = "Local Cache" Implements IMediaProvider.Name

        Public Property Author As String = "Anes08" Implements IMediaProvider.Author

        Public Property Version As New Version(1, 0, 0, 0) Implements IMediaProvider.Version

        Public Property Token As String Implements IMediaProvider.Token

        Sub New(ID As String)
            Me.Token = ID
        End Sub
#Disable Warning
        Public Async Function Fetch() As Task(Of String) Implements IMediaProvider.Fetch
            Return Token
        End Function

        Public Async Function FetchThumbnail() As Task(Of ImageSource) Implements IMediaProvider.FetchThumbnail
            If Utilities.SharedProperties.Instance.RemoteLibrary.ContainsID(Token) Then
                Dim MData = Utilities.SharedProperties.Instance.RemoteLibrary.GetItem(Token)
                If MData IsNot Nothing Then
                    Try
                        Dim ext = IO.Path.GetExtension(MData.Path)
                        Dim Tag = TagLib.File.Create(New Classes.FileBytesAbstraction("output" & If(String.IsNullOrEmpty(ext), ".mp3", ext), MData.Data))
                        If Tag.Tag.Pictures?.Length > 0 Then
                            For Each picture In Tag.Tag.Pictures
                                Dim BI = New IO.MemoryStream(picture.Data.Data).ToBitmapSource
                                Tag.Dispose()
                                Return BI
                            Next
                        End If
                    Catch
                    End Try
                End If
            End If
                Return Nothing
        End Function
#Enable Warning
    End Class
End Namespace