Imports System.Globalization

Namespace Converters
    Public Class ThemeTypeToColorPalleteConverter
        Implements IValueConverter

        Public Function Convert(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.Convert
            If value Is Nothing Then Return Nothing
            Dim Theme = TryCast(Activator.CreateInstance(TryCast(value, Type)), Classes.Themes.Theme)
            Dim CP = Theme.ColorPallete
            Theme = Nothing
            Return CP
        End Function

        Public Function ConvertBack(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.ConvertBack
            Return GetType(Classes.Themes.Theme)
        End Function
    End Class
End Namespace