Imports System.Globalization
Namespace Converters
    Public Class MetadataToLyricsConverter
        Implements IValueConverter

        Public Function Convert(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.Convert
            If TypeOf value Is Player.Metadata Then
                With CType(value, Player.Metadata)
                    If Not IO.File.Exists(.Path) Then Return Nothing
                    Try
                        Dim tag = Utilities.SharedProperties.Instance.RequestTags(.Path, TagLib.ReadStyle.None Or TagLib.ReadStyle.PictureLazy)
                        Return tag.Tag.Lyrics
                    Catch
                        'TODO just don't forget about this
                    End Try
                End With
            End If
            Return Nothing
        End Function

        Public Function ConvertBack(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.ConvertBack
            Return Nothing
        End Function
    End Class
End Namespace