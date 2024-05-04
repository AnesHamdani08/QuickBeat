Namespace Controls
    Public Class ValueSlider
        Inherits Grid

        Private _Title As String
        ReadOnly Property Title As String
            Get
                Return _Title
            End Get
        End Property

        Private _Unit As String
        ReadOnly Property Unit As String
            Get
                Return _Unit
            End Get
        End Property

        Private _Slider As Slider
        ReadOnly Property Slider As Slider
            Get
                Return _Slider
            End Get
        End Property

        Private WithEvents _TextBlock As TextBlock
        ReadOnly Property TextBlock As TextBlock
            Get
                Return _TextBlock
            End Get
        End Property

        Private Sub ValueSlider_Initialized(sender As Object, e As EventArgs) Handles Me.Initialized
            Me.RowDefinitions.Add(New RowDefinition With {.Height = GridLength.Auto})
            Me.RowDefinitions.Add(New RowDefinition)
            Me.ColumnDefinitions.Add(New ColumnDefinition)
            Me.ColumnDefinitions.Add(New ColumnDefinition With {.Width = GridLength.Auto})
            Dim T_TB As New TextBlock With {.Text = Title, .Margin = New Thickness(10, 0, 0, 5)}
            Dim TB As TextBlock = If(_TextBlock, New TextBlock)
            Dim Slider As Slider = If(_Slider, New Slider With {.Margin = New Thickness(0, 0, 10, 0)})
            AddHandler Slider.ValueChanged, New RoutedPropertyChangedEventHandler(Of Double)(Sub(d, _e)
                                                                                                 If _e.NewValue > 1000000000 Then
                                                                                                     TB.Text = Math.Round(_e.NewValue / 1000000000, 2) & " G" & Unit
                                                                                                 ElseIf _e.NewValue > 1000000 Then
                                                                                                     TB.Text = Math.Round(_e.NewValue / 1000000, 2) & " M" & Unit
                                                                                                 ElseIf _e.NewValue > 1000 Then
                                                                                                     TB.Text = Math.Round(_e.NewValue / 1000, 2) & " K" & Unit
                                                                                                 ElseIf _e.NewValue > 0.01 Then 'i just realized i could just 10^-9, but it's too late
                                                                                                     TB.Text = Math.Round(_e.NewValue, 2) & " " & Unit
                                                                                                 ElseIf _e.NewValue > 0.001 Then
                                                                                                     TB.Text = Math.Round(_e.NewValue * 1000, 2) & " m" & Unit
                                                                                                 ElseIf _e.NewValue > 0.000001 Then
                                                                                                     TB.Text = Math.Round(_e.NewValue * 1000000, 2) & " µ" & Unit
                                                                                                 ElseIf _e.NewValue > 0.000000001 Then
                                                                                                     TB.Text = Math.Round(_e.NewValue * 1000000000, 2) & " n" & Unit
                                                                                                 ElseIf _e.NewValue > 0.000000000001 Then
                                                                                                     TB.Text = Math.Round(_e.NewValue * 1000000000000, 2) & " p" & Unit
                                                                                                 ElseIf _e.NewValue > 0.000000000000001 Then
                                                                                                     TB.Text = Math.Round(_e.NewValue * 1000000000000000, 2) & " f" & Unit
                                                                                                 End If
                                                                                             End Sub)
            Grid.SetColumnSpan(T_TB, 2)
            Grid.SetColumn(Slider, 0)
            Grid.SetColumn(TB, 1)
            Grid.SetRow(T_TB, 0)
            Grid.SetRow(Slider, 1)
            Grid.SetRow(TB, 1)
            Me.Children.Add(T_TB)
            Me.Children.Add(Slider)
            Me.Children.Add(TB)
            _Slider = Slider
            _TextBlock = TB
            TB.Text = Math.Round(Slider.Value, 2) & " " & Unit
        End Sub

        Sub New(TextBlock As TextBlock, Slider As Slider, Title As String, Unit As String)
            _TextBlock = TextBlock
            _Slider = Slider
            _Title = Title
            _Unit = Unit
        End Sub

        Private Sub _TextBlock_MouseLeftButtonUp(sender As Object, e As MouseButtonEventArgs) Handles _TextBlock.MouseLeftButtonUp
            Dim v = Dialogs.InputBox.ShowSingle(Application.Current.MainWindow, $"{Title}({Unit})", "", "Prefixes: G,M,K,m,µ(or u),n,p,f are supported", Slider.Value)
            If v.EndsWith("G") OrElse v.EndsWith("M") OrElse v.EndsWith("K") OrElse v.EndsWith("m") OrElse v.EndsWith("n") OrElse v.EndsWith("p") OrElse v.EndsWith("f") Then 'prefixed
                Dim dv As Double = 0
                If Double.TryParse(v.Replace("G", "").Replace("M", "").Replace("K", "").Replace("m", "").Replace("µ", "").Replace("u", "").Replace("n", "").Replace("p", "").Replace("f", ""), dv) Then
                    Select Case v.Last
                        Case "G"
                            Slider.Value = dv * 1000000000
                        Case "M"
                            Slider.Value = dv * 1000000
                        Case "K"
                            Slider.Value = dv * 1000
                        Case "m"
                            Slider.Value = dv / 1000
                        Case "µ", "u"
                            Slider.Value = dv / 1000000
                        Case "n"
                            Slider.Value = dv / 1000000000
                        Case "p"
                            Slider.Value = dv / 1000000000000
                        Case "f"
                            Slider.Value = dv / 1000000000000000
                    End Select
                End If
            Else
                Dim dv As Double = 0
                If Double.TryParse(v, dv) Then
                    Slider.Value = dv
                End If
            End If
        End Sub
    End Class
End Namespace