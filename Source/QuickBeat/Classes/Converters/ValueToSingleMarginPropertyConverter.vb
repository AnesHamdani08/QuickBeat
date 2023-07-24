Imports System.Globalization
Namespace Converters
    Public Class ValueToSingleMarginPropertyConverter
        Implements IValueConverter

        Public Function Convert(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.Convert
            If parameter Is Nothing Then Return New Thickness(value, 0, 0, 0)
            Dim params = parameter?.ToString.Split(";")
            Select Case params(0)
                Case "left"
                    Return New Thickness(If(params(1) = "reverse", -value, value), 0, 0, 0)
                Case "right"
                    Return New Thickness(0, 0, If(params(1) = "reverse", -value, value), 0)
                Case "top"
                    Return New Thickness(0, If(params(1) = "reverse", -value, value), 0, 0)
                Case "bottom"
                    Return New Thickness(0, 0, 0, If(params(1) = "reverse", -value, value))
            End Select
            Return Nothing
        End Function

        Public Function ConvertBack(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.ConvertBack
            If parameter Is Nothing Then Return CType(value, Thickness).Left
            Dim params = parameter?.ToString.Split(";")
            Select Case params(0)
                Case "left"
                    Return If(params(1) = "reverse", -CType(value, Thickness).Left, CType(value, Thickness).Left)
                Case "right"
                    Return If(params(1) = "reverse", -CType(value, Thickness).Right, CType(value, Thickness).Right)
                Case "top"
                    Return If(params(1) = "reverse", -CType(value, Thickness).Top, CType(value, Thickness).Top)
                Case "bottom"
                    Return If(params(1) = "reverse", -CType(value, Thickness).Bottom, CType(value, Thickness).Bottom)
            End Select
            Return Nothing
        End Function
    End Class
End Namespace