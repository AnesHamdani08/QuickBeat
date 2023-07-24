Namespace Controls
    Public Class ValueSlider
        Inherits Grid

        Property _Slider As Slider
        ReadOnly Property Slider As Slider
            Get
                Return _Slider
            End Get
        End Property

        Property _TextBlock As TextBlock
        ReadOnly Property TextBlock As TextBlock
            Get
                Return _TextBlock
            End Get
        End Property

        Private Sub ValueSlider_Initialized(sender As Object, e As EventArgs) Handles Me.Initialized
            Me.ColumnDefinitions.Add(New ColumnDefinition)
            Me.ColumnDefinitions.Add(New ColumnDefinition With {.Width = GridLength.Auto})
            Dim TB As TextBlock = If(_TextBlock, New TextBlock)
            Dim Slider As Slider = If(_Slider, New Slider With {.Margin = New Thickness(0, 0, 10, 0)})
            AddHandler Slider.ValueChanged, New RoutedPropertyChangedEventHandler(Of Double)(Sub(d, _e)
                                                                                                 TB.Text = Math.Round(_e.NewValue, 2)
                                                                                             End Sub)
            Grid.SetColumn(Slider, 0)
            Grid.SetColumn(TB, 1)
            Me.Children.Add(Slider)
            Me.Children.Add(TB)
            _Slider = Slider
            _TextBlock = TB
            TB.Text = Math.Round(Slider.Value, 2)
        End Sub

        Sub New(TextBlock As TextBlock, Slider As Slider)
            _TextBlock = TextBlock
            _Slider = Slider
        End Sub
    End Class
End Namespace