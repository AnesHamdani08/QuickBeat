Imports System.Web
Imports QuickBeat.Interfaces
Namespace Youtube
    <Serializable>
    Public Class YoutubeDLMediaProvider
        Implements IMediaProvider

        Public Property Name As String = "Youtube" Implements IMediaProvider.Name

        Public Property Author As String = "Anes08" Implements IMediaProvider.Author

        Public Property Version As New Version(1, 0, 0, 0) Implements IMediaProvider.Version

        Public Property Token As String Implements IMediaProvider.Token

        Sub New(Token As String)
            Me.Token = Token
        End Sub

        Public Async Function Fetch() As Task(Of String) Implements IMediaProvider.Fetch
            Dim yt As Youtube.YoutubeDL = If(Utilities.SharedProperties.Instance.YoutubeDL, New Youtube.YoutubeDL(My.Settings.APP_YOUTUBEDL_PATH))
            Dim vid = Await yt.GetVideo(Token)
            If vid IsNot Nothing Then
                Return vid.DirectURL
            End If
            Return ""
        End Function

        Public Async Function FetchThumbnail() As Task(Of ImageSource) Implements IMediaProvider.FetchThumbnail
            If Not Utilities.SharedProperties.Instance.IsInternetConnected Then Return Nothing
            Dim uri As New Uri(Token)
            Dim query = HttpUtility.ParseQueryString(uri.Query)
            Dim videoId = String.Empty
            If query.AllKeys.Contains("v") Then
                videoId = query("v")
            Else
                videoId = uri.Segments.Last()
            End If
            Dim URL = $"http://img.youtube.com/vi/{videoId}/mqdefault.jpg"
            Try
                Dim thumb As New BitmapImage
                thumb.BeginInit()
                thumb.UriSource = New Uri(URL, UriKind.Absolute)
                thumb.EndInit()
                Return thumb
            Catch ex As Exception
                Utilities.DebugMode.Instance.Log(Of YoutubeDLMediaProvider)(ex.ToString)
                Return Nothing
            End Try
        End Function
    End Class
End Namespace