Imports System.Globalization

Namespace Converters
    Public Class ColorToBrushConverter
        Implements IValueConverter

        Public Function Convert(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.Convert
            Return New SolidColorBrush(CType(value, Color))
        End Function

        Public Function ConvertBack(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.ConvertBack
            Return TryCast(value, SolidColorBrush)?.Color
        End Function
    End Class
End Namespace