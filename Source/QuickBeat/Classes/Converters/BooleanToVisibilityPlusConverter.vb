Imports System.Globalization

Namespace Converters
    Public Class BooleanToVisibilityPlusConverter
        Implements IValueConverter

        Public Function Convert(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.Convert
            Return If(parameter Is Nothing, If(CBool(value), Visibility.Visible, Visibility.Collapsed), If(CBool(value), Visibility.Collapsed, Visibility.Visible))
        End Function

        Public Function ConvertBack(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.ConvertBack
            Return If(value = Visibility.Visible, If(parameter Is Nothing, True, False), If(parameter Is Nothing, False, True))
        End Function
    End Class
End Namespace
