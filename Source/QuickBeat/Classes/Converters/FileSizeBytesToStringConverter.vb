Imports System.Globalization
Namespace Converters
    Public Class FileSizeBytesToStringConverter
        Implements IValueConverter

        Public Function Convert(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.Convert
            Dim TheSize As Long = 0
            If Long.TryParse(value?.ToString, TheSize) Then
                Select Case TheSize
                    Case Is >= 1099511627776
                        Dim DoubleBytes = CDbl(TheSize / 1099511627776) 'TB
                        Return FormatNumber(DoubleBytes, 2) & " TB"
                    Case 1073741824 To 1099511627775
                        Dim DoubleBytes = CDbl(TheSize / 1073741824) 'GB
                        Return FormatNumber(DoubleBytes, 2) & " GB"
                    Case 1048576 To 1073741823
                        Dim DoubleBytes = CDbl(TheSize / 1048576) 'MB
                        Return FormatNumber(DoubleBytes, 2) & " MB"
                    Case 1024 To 1048575
                        Dim DoubleBytes = CDbl(TheSize / 1024) 'KB
                        Return FormatNumber(DoubleBytes, 2) & " KB"
                    Case 0 To 1023
                        Dim DoubleBytes = TheSize ' bytes
                        Return FormatNumber(DoubleBytes, 2) & " bytes"
                    Case Else
                        Return ""
                End Select
            End If
            Return "NaN"
        End Function

        Public Function ConvertBack(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.ConvertBack
            Dim sValue = value?.ToString
            If Not String.IsNullOrEmpty(sValue) Then
                If sValue.EndsWith("TB") Then
                ElseIf sValue.EndsWith("GB") Then
                    Return CDbl(sValue.Replace(" TB", "")) * 1099511627776
                ElseIf sValue.EndsWith("MB") Then
                    Return CDbl(sValue.Replace(" MB", "")) * 1048576
                ElseIf sValue.EndsWith("KB") Then
                    Return CDbl(sValue.Replace(" KB", "")) * 1024
                ElseIf sValue.EndsWith("bytes") Then
                    Return CDbl(sValue.Replace(" bytes", ""))
                End If
            End If
            Return -1
        End Function
    End Class
End Namespace