Imports System.Globalization
Namespace Converters
    Public Class StringToFanartImageConverter
        Implements IValueConverter
        Public Function Convert(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.Convert
            'release:=album, recording:=track
            'special characters:= + - && || ! ( ) { } [ ] ^ " ~ * ? : \/               
            Dim SpecialCharacters As String() = New String() {"+", "-", "&&", "||", "!", "(", ")", "{", "}", "[", "]", "^", """", "~", "*", "?", ":", "\/"}
            Dim SearchQuery As String = ""
            Dim EArtist = value.Replace("+", "\+").Replace("-", "\-").Replace("&&", "\&&").Replace("||", "\||").Replace("!", "\!").Replace("(", "\(").Replace(")", "\)").Replace("{", "\{").Replace("}", "\}").Replace("[", "\[").Replace("]", "\]").Replace("^", "\^").Replace("""", "\""").Replace("~", "\~").Replace("*", "\*").Replace("?", "\?").Replace(":", "\:").Replace("\/", "\\/")
            SearchQuery = $"Artist:{EArtist}"
            Dim Query As New MetaBrainz.MusicBrainz.Query("QuickBeat", New Version(1, 0, 0, 0))
            Dim Result = Query.FindArtists(SearchQuery, 1)
            Dim FResult = Result.Results.FirstOrDefault
            If FResult Is Nothing Then
                Return Nothing
            End If
            Dim Artist = New FanartTv.Music.Artist(FResult.Item.Id.ToString)
            If parameter Is Nothing Then
                Dim BgURL = Artist.List.AImagesrtistbackground?.FirstOrDefault?.Url
                If Not String.IsNullOrEmpty(BgURL) Then
                    Using WC As New Net.WebClient()
                        Dim Data = WC.DownloadData(BgURL)
                        Dim BI As New BitmapImage
                        BI.BeginInit()
                        BI.StreamSource = New IO.MemoryStream(Data)
                        BI.EndInit()
                        Return BI
                    End Using
                End If
            Else
                Select Case parameter.ToString.ToLower
                    Case "bg", "background"
                        Dim BgURL = Artist.List.AImagesrtistbackground?.FirstOrDefault?.Url
                        If Not String.IsNullOrEmpty(BgURL) Then
                            Using WC As New Net.WebClient()
                                Dim Data = WC.DownloadData(BgURL)
                                Dim BI As New BitmapImage
                                BI.BeginInit()
                                BI.StreamSource = New IO.MemoryStream(Data)
                                BI.EndInit()
                                Return BI
                            End Using
                        End If
                    Case "pic", "thumb"
                        Dim ThumbURL = Artist.List.Artistthumb?.FirstOrDefault?.Url
                        If Not String.IsNullOrEmpty(ThumbURL) Then
                            Using WC As New Net.WebClient()
                                Dim Data = WC.DownloadData(ThumbURL)
                                Dim BI As New BitmapImage
                                BI.BeginInit()
                                BI.StreamSource = New IO.MemoryStream(Data)
                                BI.EndInit()
                                Return BI
                            End Using
                        End If
                End Select
            End If
            Return Nothing
        End Function

        Public Function ConvertBack(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.ConvertBack
            Return value?.ToString
        End Function
    End Class
End Namespace