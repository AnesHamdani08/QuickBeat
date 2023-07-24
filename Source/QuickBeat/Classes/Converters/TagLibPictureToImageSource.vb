Imports System.Globalization

Namespace Converters
    Public Class TagLibPictureToImageSource
        Implements IValueConverter

        Public Function Convert(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.Convert
            Dim BI As New BitmapImage
            BI.BeginInit()
            BI.CacheOption = BitmapCacheOption.OnDemand
            'BI.DecodePixelHeight = 150
            'BI.DecodePixelWidth = 150
            BI.StreamSource = New IO.MemoryStream(TryCast(value, TagLib.IPicture)?.Data.Data)
            BI.EndInit()
            Return BI
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