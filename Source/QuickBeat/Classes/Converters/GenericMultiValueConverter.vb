Imports System.Globalization
Namespace Converters
    Public Class GenericMultiValueConverter
        Implements IMultiValueConverter

        Public Function Convert(values() As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IMultiValueConverter.Convert
            Return values.Clone 'required because values() will be disposed after this function is called
        End Function

        Public Function ConvertBack(value As Object, targetTypes() As Type, parameter As Object, culture As CultureInfo) As Object() Implements IMultiValueConverter.ConvertBack
            Return value
        End Function
    End Class
End Namespace