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

        Private _TextBlock As TextBlock
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
                                                                                                 TB.Text = Math.Round(_e.NewValue, 2) & " " & Unit
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
    End Class
End Namespace