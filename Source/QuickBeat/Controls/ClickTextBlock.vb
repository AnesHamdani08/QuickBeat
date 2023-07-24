Namespace Controls
    Public Class ClickTextBlock
        Inherits TextBlock
        Shared ReadOnly Property CommandProperty As DependencyProperty = DependencyProperty.Register("Command", GetType(ICommand), GetType(ClickTextBlock), New PropertyMetadata(Nothing, New PropertyChangedCallback(Sub(d As DependencyObject, e As DependencyPropertyChangedEventArgs)
                                                                                                                                                                                                                          With CType(d, ClickTextBlock)
                                                                                                                                                                                                                              If e.OldValue IsNot Nothing Then RemoveHandler CType(e.OldValue, ICommand).CanExecuteChanged, .Command_CanExecuteChangedHandler
                                                                                                                                                                                                                              If e.NewValue IsNot Nothing Then AddHandler CType(e.NewValue, ICommand).CanExecuteChanged, .Command_CanExecuteChangedHandler
                                                                                                                                                                                                                          End With
                                                                                                                                                                                                                      End Sub)))
        Property Command As ICommand
            Get
                Return GetValue(CommandProperty)
            End Get
            Set(value As ICommand)
                SetValue(CommandProperty, value)
            End Set
        End Property
        Shared ReadOnly Property CommandParameterProperty As DependencyProperty = DependencyProperty.Register("CommandParameter", GetType(Object), GetType(ClickTextBlock))
        Property CommandParameter As Object
            Get
                Return GetValue(CommandParameterProperty)
            End Get
            Set(value As Object)
                SetValue(CommandParameterProperty, value)
            End Set
        End Property
        Private Command_CanExecuteChangedHandler As New EventHandler(Sub(sender As Object, e As EventArgs)
                                                                         IsEnabled = Command?.CanExecute(CommandParameter)
                                                                     End Sub)
        Overloads ReadOnly Property TextDecorations As TextDecorationCollection
            Get
                Return MyBase.TextDecorations
            End Get
        End Property

        Private _Opacity = Opacity
        Overloads Property Opacity As Double
            Get
                Return MyBase.Opacity
            End Get
            Set(value As Double)
                MyBase.Opacity = value
                _Opacity = value
            End Set
        End Property

        Overloads Property IsEnabled As Boolean
            Get
                Return MyBase.IsEnabled
            End Get
            Set(value As Boolean)
                MyBase.IsEnabled = value
                If value Then
                    'BeginAnimation(OpacityProperty, New Animation.DoubleAnimation(_Opacity, New Duration(TimeSpan.FromMilliseconds(200))))
                    MyBase.Opacity = _Opacity
                Else
                    'BeginAnimation(OpacityProperty, New Animation.DoubleAnimation(0.75, New Duration(TimeSpan.FromMilliseconds(200))))
                    MyBase.Opacity = 0.75
                End If
            End Set
        End Property

        Property DecorateOnMouseEnter As Boolean = True

        Property CommandTriggerMode As CommandTrigger = CommandTrigger.Click

        Private Sub ClickTextBlock_MouseEnter(sender As Object, e As MouseEventArgs) Handles Me.MouseEnter
            If DecorateOnMouseEnter Then
                MyBase.TextDecorations = System.Windows.TextDecorations.Underline
            End If
            'BeginAnimation(OpacityProperty, New Animation.DoubleAnimation(0.75, New Duration(TimeSpan.FromMilliseconds(200))))
            MyBase.Opacity = 0.75
        End Sub

        Private Sub ClickTextBlock_MouseLeave(sender As Object, e As MouseEventArgs) Handles Me.MouseLeave
            MyBase.TextDecorations = Nothing
            'If DecorateOnMouseEnter Then
            'BeginAnimation(OpacityProperty, New Animation.DoubleAnimation(_Opacity, New Duration(TimeSpan.FromMilliseconds(200))))
            MyBase.Opacity = _Opacity
            'End If
        End Sub

        Private Sub ClickTextBlock_MouseLeftButtonUp(sender As Object, e As MouseButtonEventArgs) Handles Me.MouseUp
            If e.ChangedButton = MouseButton.Left AndAlso CommandTriggerMode = CommandTrigger.Click Then
                Command?.Execute(CommandParameter)
                e.Handled = True
            End If
        End Sub

        Private Sub ClickTextBlock_MouseDown(sender As Object, e As MouseButtonEventArgs) Handles Me.MouseDown
            If CommandTriggerMode = CommandTrigger.DoubleClick Then
                If e.ClickCount >= 2 Then
                    Command?.Execute(CommandParameter)
                End If
            End If
            e.Handled = True
        End Sub

        Private Sub ClickTextBlock_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
            _Opacity = Opacity
        End Sub

        Enum CommandTrigger
            Click
            DoubleClick
        End Enum
    End Class
End Namespace