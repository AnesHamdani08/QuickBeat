Imports System.Globalization
Imports QuickBeat.Utilities

Namespace Converters
    Public Class TagLibPictureToImageSource
        Implements IValueConverter

        Public Function Convert(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.Convert
            Return New IO.MemoryStream(TryCast(value, TagLib.IPicture)?.Data.Data).ToBitmapSource
        End Function

        Public Function ConvertBack(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.ConvertBack
            Dim Enco As New PngBitmapEncoder
            Dim OutStream As New IO.MemoryStream
            Enco.Frames.Add(BitmapFrame.Create(TryCast(value, BitmapSource)))
            Enco.Save(OutStream)
            Return New TagLib.Picture(New TagLib.ByteVector(OutStream.ToArray))
        End Function
    End Class
End Namespace