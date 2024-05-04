Namespace Controls
    ''' <summary>
    ''' Simple Image Control with Double-Click to View in Image Viewer Behaviour
    ''' </summary>
    Public Class ExpandableImage
        Inherits Image

        Private Sub ExpandableImage_MouseDown(sender As Object, e As MouseButtonEventArgs) Handles Me.MouseDown
            If Source IsNot Nothing AndAlso e.ClickCount >= 2 Then 'Trigger Image Viewer
                Dim BS = TryCast(Me.Source, BitmapSource)
                If BS Is Nothing Then Return
                Dim IV As New HandyControl.Controls.ImageViewer With {
            .ImageSource = BitmapFrame.Create(BS)
        }
                Dim Popup As New HandyControl.Controls.Window() With {.Content = IV, .Width = 500, .Height = 500, .Background = Nothing, .WindowStartupLocation = WindowStartupLocation.CenterScreen, .Owner = Application.Current.MainWindow}
                e.Handled = True
                Popup.ShowDialog()
            End If
        End Sub
    End Class
End Namespace