Imports QuickBeat.Interfaces
Imports EDeezer = E.Deezer
Namespace Utilities.Deezer
    <Serializable>
    Public Class DeezerMediaProvider
        Implements IMediaProvider

        Public Property Name As String = "Deezer" Implements IMediaProvider.Name

        Public Property Author As String = "Anes08" Implements IMediaProvider.Author

        Public Property Version As New Version(1, 0, 0, 0) Implements IMediaProvider.Version

        Public Property Token As String Implements IMediaProvider.Token

        Sub New(Token As String)
            Token = Token
        End Sub
#Disable Warning
        Public Async Function Fetch() As Task(Of String) Implements IMediaProvider.Fetch
            If Not Utilities.SharedProperties.Instance.IsInternetConnected Then Return ""
            Dim Session = EDeezer.DeezerSession.CreateNew()
            Dim iToken As UInteger
            If UInteger.TryParse(Token, iToken) Then
                Try
                    Dim Track = Await Session.Browse.GetTrackById(iToken)
                    Session.Dispose()
                    Return Track?.Preview
                Catch ex As Exception
                    Utilities.DebugMode.Instance.Log(Of DeezerProvider)(ex.ToString)
                End Try
            End If
            Session?.Dispose()
            Return ""
        End Function
#Enable Warning
        Public Async Function FetchThumbnail() As Task(Of ImageSource) Implements IMediaProvider.FetchThumbnail
            If Not Utilities.SharedProperties.Instance.IsInternetConnected Then Return Nothing
            Dim Session = EDeezer.DeezerSession.CreateNew()
            Dim iToken As UInteger
            If UInteger.TryParse(Token, iToken) Then
                Try
                    Dim Track = Await Session.Browse.GetTrackById(iToken)
                    Session.Dispose()
                    Dim url = If(Track.HasPicture(EDeezer.Api.PictureSize.Medium), Track.GetPicture(EDeezer.Api.PictureSize.Medium), If(Track.HasPicture(EDeezer.Api.PictureSize.Large), Track.GetPicture(EDeezer.Api.PictureSize.Large), If(Track.HasPicture(EDeezer.Api.PictureSize.Small), Track.GetPicture(EDeezer.Api.PictureSize.Small), If(Track.HasPicture(EDeezer.Api.PictureSize.ExtraLarge), Track.GetPicture(EDeezer.Api.PictureSize.ExtraLarge), Nothing))))
                    If String.IsNullOrEmpty(url) Then Return Nothing
                    Return New Uri(url, UriKind.Absolute).ToBitmapSource
                Catch ex As Exception
                    Utilities.DebugMode.Instance.Log(Of DeezerProvider)(ex.ToString)
                End Try
            End If
            Session?.Dispose()
            Return Nothing
        End Function
    End Class
End Namespace