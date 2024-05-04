﻿Imports System.Globalization
Imports MetaBrainz.MusicBrainz.Interfaces
Imports QuickBeat.Utilities

Namespace Converters
    Public Class MetadataToFanartImageConverter
        Implements IValueConverter
        Public Function Convert(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.Convert
            'release:=album, recording:=track
            'special characters:= + - && || ! ( ) { } [ ] ^ " ~ * ? : \/               
            Dim meta = TryCast(value, Player.Metadata)
            Dim SpecialCharacters As String() = New String() {"+", "-", "&&", "||", "!", "(", ")", "{", "}", "[", "]", "^", """", "~", "*", "?", ":", "\/"}
            Dim SearchQuery As String = ""
            Dim EArtist = value.Replace("+", "\+").Replace("-", "\-").Replace("&&", "\&&").Replace("||", "\||").Replace("!", "\!").Replace("(", "\(").Replace(")", "\)").Replace("{", "\{").Replace("}", "\}").Replace("[", "\[").Replace("]", "\]").Replace("^", "\^").Replace("""", "\""").Replace("~", "\~").Replace("*", "\*").Replace("?", "\?").Replace(":", "\:").Replace("\/", "\\/")
            SearchQuery = $"Artist:{EArtist}"
            Dim Query As New MetaBrainz.MusicBrainz.Query("QuickBeat", New Version(1, 0, 0, 0))
            Dim Result As Searches.ISearchResults(Of Searches.ISearchResult(Of Entities.IArtist))
            Try
                Result = Query.FindArtists(SearchQuery, 1)
            Catch ex As Exception
                Utilities.DebugMode.Instance.Log(Of MetadataToFanartImageConverter)(ex.ToString)
                Return meta?.DefaultCover
            End Try
            If Result Is Nothing Then Return meta?.DefaultCover
            Dim FResult = Result?.Results.FirstOrDefault
            If FResult Is Nothing Then
                Return meta?.DefaultCover
            End If
            Dim Artist = New FanartTv.Music.Artist(FResult.Item.Id.ToString)
            If parameter Is Nothing Then
                Dim BgURL = Artist.List.AImagesrtistbackground?.FirstOrDefault?.Url
                If Not String.IsNullOrEmpty(BgURL) Then
                    Return New Uri(BgURL).ToBitmapSource
                End If
            Else
                Select Case parameter.ToString.ToLower
                    Case "bg", "background"
                        Dim BgURL = Artist.List.AImagesrtistbackground?.FirstOrDefault?.Url
                        If Not String.IsNullOrEmpty(BgURL) Then
                            Return New Uri(BgURL).ToBitmapSource
                        End If
                    Case "pic", "thumb"
                        Dim ThumbURL = Artist.List.Artistthumb?.FirstOrDefault?.Url
                        If Not String.IsNullOrEmpty(ThumbURL) Then
                            Return New Uri(ThumbURL).ToBitmapSource
                        End If
                End Select
            End If
            Return meta?.DefaultCover
        End Function

        Public Function ConvertBack(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.ConvertBack
            Return value?.ToString
        End Function
    End Class
End Namespace