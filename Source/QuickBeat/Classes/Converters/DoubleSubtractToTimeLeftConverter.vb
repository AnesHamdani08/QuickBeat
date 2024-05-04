Imports System.Globalization

Namespace Converters
    Public Class DoubleSubtractToTimeLeftConverter
        Implements IValueConverter

        Public Function Convert(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.Convert
            Dim iValue As Double = 0
            Dim iParam As Double = 0
            Double.TryParse(value?.ToString, iValue)
            Double.TryParse(parameter?.ToString, iParam)

            Dim LengthTS = TimeSpan.FromSeconds(iValue - iParam)
            Return $"{If(LengthTS.Minutes < 10, 0, "")}{LengthTS.Minutes}:{If(LengthTS.Seconds < 10, 0, "")}{LengthTS.Seconds}"
        End Function

        Public Function ConvertBack(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.ConvertBack
            Dim iParam As Double = 0
            Double.TryParse(parameter?.ToString, iParam)

            Return CType(value, TimeSpan).TotalMinutes - iParam
        End Function
    End Class
End Namespace