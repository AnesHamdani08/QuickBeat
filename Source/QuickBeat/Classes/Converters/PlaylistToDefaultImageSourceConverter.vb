Imports System.Globalization
Namespace Converters
    Public Class PlaylistToDefaultImageSourceConverter
        Implements IValueConverter

        Public Function Convert(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.Convert
            If TypeOf value Is Player.Playlist Then
                Dim meta = CType(value, Player.Playlist).FirstOrDefault(Function(k) k.HasCover)
                If meta Is Nothing Then Return Nothing
                meta.EnsureCovers()
                Return meta.DefaultCover
            End If
            Return Nothing
        End Function

        Public Function ConvertBack(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.ConvertBack
            Return Nothing
        End Function
    End Class
End Namespace