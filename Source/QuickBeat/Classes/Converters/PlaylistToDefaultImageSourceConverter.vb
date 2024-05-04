Imports System.Globalization
Namespace Converters
    ''' <summary>
    ''' Supports both <see cref="Player.Playlist"/> and <see cref="Player.MetadataGroup"/>
    ''' Add a parameter to decode to thumbnails
    ''' </summary>
    Public Class PlaylistToDefaultImageSourceConverter
        Implements IValueConverter

        Public Function Convert(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.Convert
            If TypeOf value Is Player.Playlist Then
                If CType(value, Player.Playlist).Cover Is Nothing Then
                    Dim meta = CType(value, Player.Playlist).FirstOrDefault(Function(k) k.HasCover)
                    If meta Is Nothing Then Return Nothing
                    meta.EnsureCovers(DecodeToThumbnail:=If(parameter Is Nothing, False, True))
                    Return meta.DefaultCover
                Else
                    Return CType(value, Player.Playlist).Cover
                End If
            ElseIf TypeOf value Is Player.MetadataGroup Then
                If CType(value, Player.MetadataGroup).Cover Is Nothing Then
                    Dim meta = CType(value, Player.MetadataGroup).FirstOrDefault(Function(k) k.HasCover)
                    If meta Is Nothing Then Return Nothing
                    meta.EnsureCovers(DecodeToThumbnail:=If(parameter Is Nothing, False, True))
                    Return meta.DefaultCover
                Else
                    Return CType(value, Player.MetadataGroup).Cover
                End If
            End If
                Return Nothing
        End Function

        Public Function ConvertBack(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.ConvertBack
            Return Nothing
        End Function
    End Class
End Namespace