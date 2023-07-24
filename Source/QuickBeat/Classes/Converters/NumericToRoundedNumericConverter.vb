Imports System.Globalization
Namespace Converters
    Public Class NumericToRoundedNumericConverter
        Implements IValueConverter

        Public Function Convert(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.Convert
            Return If(parameter?.ToString.Contains("+"), Math.Round(CDbl(value), CInt(parameter.ToString.Split("+").FirstOrDefault)) & parameter.ToString.Split("+").LastOrDefault, Math.Round(CDbl(value), CInt(parameter)))
        End Function

        Public Function ConvertBack(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.ConvertBack
            Return value
        End Function
    End Class
End Namespace