Imports System.Globalization

Namespace Converters
    Public Class TextToImageSourceConverter
        Implements IValueConverter

        Public Function Convert(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.Convert
            Dim dv As New DrawingVisual()
            Dim dc = dv.RenderOpen
            Dim recBrush As New LinearGradientBrush(New GradientStopCollection From {New GradientStop(Color.FromRgb(38, 50, 56), 0), New GradientStop(Color.FromRgb(69, 90, 100), 1)}, 45)
            dc.DrawRoundedRectangle(recBrush, Nothing, New Rect(0, 0, 100, 100), 15, 15)
#Disable Warning
            Dim txt = New FormattedText(If(value?.ToString.FirstOrDefault, "-") & If(value?.ToString.LastOrDefault, "-"), Globalization.CultureInfo.CurrentCulture, FlowDirection.LeftToRight, New Typeface(New FontFamily("Arial"), FontStyles.Normal, FontWeights.DemiBold, FontStretches.Normal), 132 / 2, New SolidColorBrush(Color.FromRgb(255, 193, 7))) '2 for text length                                               
#Enable Warning
            dc.DrawText(txt, New Point((100 - txt.Width) / 2, (100 - txt.Height) / 2))
            dc.Close()
            Return New DrawingImage(dv.Drawing)
        End Function

        Public Function ConvertBack(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.ConvertBack
            Return Nothing
        End Function
    End Class
End Namespace