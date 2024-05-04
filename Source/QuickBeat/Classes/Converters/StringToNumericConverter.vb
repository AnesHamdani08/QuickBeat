Imports System.Globalization
Namespace Converters
    Public Class StringToNumericConverter
        Implements IValueConverter

        Public Function Convert(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.Convert
            Dim v As String = Nothing
            If TypeOf value Is ComboBoxItem Then
                v = CType(value, ComboBoxItem).Content.ToString
            ElseIf TypeOf value Is String Then
                v = value.ToString
            End If
            If parameter Is Nothing Then
                Dim iStr As Integer = -1
                Integer.TryParse(v, iStr)
                Return iStr
            Else
                Select Case parameter.ToString.ToLower
                    Case "uinteger"
                        Dim iStr As UInteger = 0
                        UInteger.TryParse(v, iStr)
                        Return iStr
                    Case "integer"
                        Dim iStr As Integer = -1
                        Integer.TryParse(v, iStr)
                        Return iStr
                    Case "double"
                        Dim iStr As Double = -1
                        Double.TryParse(v, iStr)
                        Return iStr
                    Case "ulong"
                        Dim iStr As ULong = 0
                        ULong.TryParse(v, iStr)
                        Return iStr
                    Case "long"
                        Dim iStr As Long = -1
                        Long.TryParse(v, iStr)
                        Return iStr
                End Select
            End If
            Return 0
        End Function

        Public Function ConvertBack(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.ConvertBack
            Return value?.ToString
        End Function
    End Class
End Namespace