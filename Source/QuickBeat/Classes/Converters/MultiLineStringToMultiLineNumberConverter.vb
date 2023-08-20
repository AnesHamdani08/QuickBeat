Imports System.Globalization

Namespace Converters
    Public Class MultiLineStringToMultiLineNumberConverter
        Implements IValueConverter

        Public Function Convert(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.Convert
            Dim S As String = ""
            For i As Integer = 0 To value?.ToString.Split(Environment.NewLine).Length - 1
                S &= i + 1 & Environment.NewLine
            Next
            Return S
        End Function

        Public Function ConvertBack(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.ConvertBack
            Return value
        End Function
    End Class
End Namespace