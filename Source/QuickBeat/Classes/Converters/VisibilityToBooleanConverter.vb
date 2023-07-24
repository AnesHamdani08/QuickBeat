Imports System.Globalization
Namespace Converters
    Public Class VisibilityToBooleanConverter
        Implements IValueConverter

        Public Function Convert(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.Convert
            If TypeOf value Is Visibility Then
                If CType(value, Visibility) = Visibility.Visible Then
                    Return True
                Else
                    Return False
                End If
            End If
            Return True
        End Function

        Public Function ConvertBack(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.ConvertBack
            Try
                If value = True Then
                    Return Visibility.Visible
                Else
                    Return Visibility.Collapsed
                End If
            Catch ex As Exception
                Return True
            End Try
        End Function
    End Class
End Namespace