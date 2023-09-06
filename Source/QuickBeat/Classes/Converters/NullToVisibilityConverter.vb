Imports System.Globalization

Namespace Converters
    Public Class NullToVisibilityConverter
        Implements IValueConverter

        Public Function Convert(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.Convert
            Return If(parameter Is Nothing, If(value Is Nothing, Visibility.Collapsed, Visibility.Visible), If(value Is Nothing, Visibility.Visible, Visibility.Collapsed))
        End Function

        Public Function ConvertBack(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.ConvertBack
            Return value
        End Function
    End Class
End Namespace