Imports Lucene.Net.Search.Payloads

Namespace Controls
    Public Class RandomizeGrid
        Public Shared AutoClearProperty As DependencyProperty = DependencyProperty.Register("AutoClear", GetType(Boolean), GetType(RandomizeGrid), New PropertyMetadata(True))
        Property AutoClear As Boolean
            Get
                Return GetValue(AutoClearProperty)
            End Get
            Set(value As Boolean)
                SetValue(AutoClearProperty, value)
            End Set
        End Property

        Public Shared AutoExecuteProperty As DependencyProperty = DependencyProperty.Register("AutoExecute", GetType(Boolean), GetType(RandomizeGrid), New PropertyMetadata(True))
        Property AutoExecute As Boolean
            Get
                Return GetValue(AutoExecuteProperty)
            End Get
            Set(value As Boolean)
                SetValue(AutoExecuteProperty, value)
            End Set
        End Property

        Public Shared MinimumProperty As DependencyProperty = DependencyProperty.Register("Minimum", GetType(Integer), GetType(RandomizeGrid))
        Property Minimum As Integer
            Get
                Return GetValue(MinimumProperty)
            End Get
            Set(value As Integer)
                SetValue(MinimumProperty, value)
            End Set
        End Property

        Public Shared MaximumProperty As DependencyProperty = DependencyProperty.Register("Maximum", GetType(Integer), GetType(RandomizeGrid))
        Property Maximum As Integer
            Get
                Return GetValue(MaximumProperty)
            End Get
            Set(value As Integer)
                SetValue(MaximumProperty, value)
            End Set
        End Property

        Public Shared ValueProperty As DependencyProperty = DependencyProperty.Register("Value", GetType(Integer), GetType(RandomizeGrid))
        Property Value As Integer
            Get
                Return GetValue(ValueProperty)
            End Get
            Set(value As Integer)
                SetValue(ValueProperty, value)
            End Set
        End Property

        Public Shared CommandProperty As DependencyProperty = DependencyProperty.Register("Command", GetType(ICommand), GetType(RandomizeGrid))
        Property Command As ICommand
            Get
                Return GetValue(CommandProperty)
            End Get
            Set(value As ICommand)
                SetValue(CommandProperty, value)
            End Set
        End Property
        Public Shared IsHoldingProperty As DependencyProperty = DependencyProperty.Register("IsHolding", GetType(Boolean), GetType(RandomizeGrid), New PropertyMetadata(True))
        Property IsHolding As Boolean
            Get
                Return GetValue(IsHoldingProperty)
            End Get
            Set(value As Boolean)
                SetValue(IsHoldingProperty, value)
            End Set
        End Property
        Public Shared AutoHideProperty As DependencyProperty = DependencyProperty.Register("AutoHide", GetType(Boolean), GetType(RandomizeGrid), New PropertyMetadata(True))
        Property AutoHide As Boolean
            Get
                Return GetValue(AutoHideProperty)
            End Get
            Set(value As Boolean)
                SetValue(AutoHideProperty, value)
            End Set
        End Property
        Private _Pattern As New List(Of String)
        Private Sub RandomizeGrid_PreviewMouseLeftButtonDown(sender As Object, e As MouseButtonEventArgs) Handles Me.PreviewMouseLeftButtonDown
            _Pattern.Clear()
            IsHolding = True
            Grid_Main.IsEnabled = True
        End Sub

        Private Sub RandomizeGrid_PreviewMouseLeftButtonUp(sender As Object, e As MouseButtonEventArgs) Handles Me.PreviewMouseLeftButtonUp
            Dim rContent = Grid_Main.Children.OfType(Of Rectangle)
            Dim Pattern As String = String.Join("", _Pattern)
            'For Each rec In rContent
            '    If rec.Opacity > 0.4 Then 'Selected , > 0.4 because of conflict with animation duration
            '        Pattern &= rec.Tag?.ToString
            '    End If
            'Next
            Dim iPattern As Integer = 0
            If Integer.TryParse(Pattern.GetHashCode, iPattern) Then
                Dim rnd As New Random(iPattern)
                Value = rnd.Next(Minimum, Maximum + 1)
                If AutoExecute Then
                    If Command?.CanExecute(Value) Then
                        Command.Execute(Value)
                    End If
                End If
            End If
            Grid_Main.IsEnabled = False
            IsHolding = False
            If AutoClear Then
                For Each rec In rContent
                    rec.BeginAnimation(OpacityProperty, New Animation.DoubleAnimation(0.3, New Duration(TimeSpan.FromMilliseconds(250))))
                Next
            End If
            If AutoHide Then Visibility = Visibility.Collapsed
        End Sub

        Private Sub RandomizeGrid_MouseLeave(sender As Object, e As MouseEventArgs) Handles Me.MouseLeave
            If IsHolding Then 'Cancel
                Grid_Main.IsEnabled = False
                IsHolding = False
                For Each rec In Grid_Main.Children.OfType(Of Rectangle)
                    rec.BeginAnimation(OpacityProperty, New Animation.DoubleAnimation(0.3, New Duration(TimeSpan.FromMilliseconds(250))))
                Next
                If AutoHide Then Visibility = Visibility.Collapsed
            End If 'RandomizeGrid_PreviewMouseLeftButtonUp(Me, New MouseButtonEventArgs(e.Device, 0, MouseButton.Left))
        End Sub

        Private Sub Rect_XX_MouseEnter(sender As Object, e As MouseEventArgs) Handles Rect_00.MouseEnter, Rect_01.MouseEnter, Rect_02.MouseEnter, Rect_03.MouseEnter,
                                                                                      Rect_10.MouseEnter, Rect_11.MouseEnter, Rect_12.MouseEnter, Rect_13.MouseEnter,
                                                                                      Rect_20.MouseEnter, Rect_21.MouseEnter, Rect_22.MouseEnter, Rect_23.MouseEnter,
                                                                                      Rect_30.MouseEnter, Rect_31.MouseEnter, Rect_32.MouseEnter, Rect_33.MouseEnter
            If IsHolding Then
                If Not _Pattern.Contains(CType(sender, Rectangle).Tag.ToString) Then _Pattern.Add(CType(sender, Rectangle).Tag.ToString)
            End If
        End Sub
    End Class
End Namespace