Namespace Controls
    Public Class ClickProgressBar
        Inherits ProgressBar
        Public Shared IsDraggingProperty As DependencyProperty = DependencyProperty.Register("IsDragging", GetType(Boolean), GetType(ClickProgressBar))
        ''' <summary>
        ''' Used to indicate when the user is dragging on the seekbar
        ''' </summary>
        ''' <returns></returns>
        Property IsDragging As Boolean
            Get
                Return GetValue(IsDraggingProperty)
            End Get
            Set(value As Boolean)
                SetValue(IsDraggingProperty, value)
            End Set
        End Property

        Public Shared IsDraggableProperty As DependencyProperty = DependencyProperty.Register("DragValue", GetType(Boolean), GetType(ClickProgressBar))
        ''' <summary>
        ''' Used to allow user dragging on the seekbar
        ''' </summary>
        ''' <returns></returns>
        Property IsDraggable As Boolean
            Get
                Return GetValue(IsDraggableProperty)
            End Get
            Set(value As Boolean)
                SetValue(IsDraggableProperty, value)
            End Set
        End Property

        Private _IsDown As Boolean = False
        Private Sub ClickProgressBar_MouseLeftButtonUp(sender As Object, e As MouseButtonEventArgs) Handles Me.MouseLeftButtonUp
            _IsDown = False
            IsDragging = False
            Dim pos = e.GetPosition(Me)
            Dim factor = pos.X / ActualWidth
            Value = Maximum * factor
        End Sub

        Private Sub ClickProgressBar_MouseLeftButtonDown(sender As Object, e As MouseButtonEventArgs) Handles Me.MouseLeftButtonDown
            _IsDown = True
            IsDragging = True
        End Sub

        Private Sub ClickProgressBar_MouseMove(sender As Object, e As MouseEventArgs) Handles Me.MouseMove
            If IsDraggable AndAlso _IsDown Then
                Dim pos = e.GetPosition(Me)
                Dim factor = pos.X / ActualWidth
                Value = Maximum * factor
            End If
        End Sub

        Private Sub ClickPrgressBar_MouseLeave(sender As Object, e As MouseEventArgs) Handles Me.MouseLeave
            If _IsDown Then
                _IsDown = False
                IsDragging = False
            End If
        End Sub
    End Class
End Namespace