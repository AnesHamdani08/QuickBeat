Imports System.Globalization

Namespace Converters
    Public Class BooleanToApplicationThemeConverter
        Implements IValueConverter

        Public Function Convert(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.Convert
            Return If(parameter Is Nothing, If(CType(value, HandyControl.Themes.ApplicationTheme) = HandyControl.Themes.ApplicationTheme.Dark, True, False), If(CType(value, HandyControl.Themes.ApplicationTheme) = HandyControl.Themes.ApplicationTheme.Dark, False, True))
        End Function

        Public Function ConvertBack(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.ConvertBack
            Return If(parameter Is Nothing, If(CBool(value), HandyControl.Themes.ApplicationTheme.Dark, HandyControl.Themes.ApplicationTheme.Light), If(CBool(value), HandyControl.Themes.ApplicationTheme.Light, HandyControl.Themes.ApplicationTheme.Dark))
        End Function
    End Class
End Namespace
