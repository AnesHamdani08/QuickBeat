Imports System.Globalization
Namespace Converters
    Public Class GenericMultiValueConverter
        Implements IMultiValueConverter

        Public Function Convert(values() As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IMultiValueConverter.Convert
            If parameter Is Nothing Then
                Return values.Clone 'required because values() will be disposed after this function is called
            Else
                Return String.Join(parameter.ToString, values)
            End If
        End Function

        Public Function ConvertBack(value As Object, targetTypes() As Type, parameter As Object, culture As CultureInfo) As Object() Implements IMultiValueConverter.ConvertBack
            If parameter Is Nothing Then
                Return value
            Else
                Return value.ToString.Split(parameter.ToString)
            End If
        End Function
    End Class
End Namespace