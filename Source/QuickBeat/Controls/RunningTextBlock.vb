Imports System.Windows.Documents.DocumentStructures
Imports System.Windows.Media.Animation

Namespace Controls
    Public Class RunningTextBlock
        Inherits TextBlock

        Shared ReadOnly Property CompareToProperty As DependencyProperty = DependencyProperty.Register("CompareTo", GetType(Double), GetType(RunningTextBlock), New UIPropertyMetadata(Double.MaxValue))
        Property CompareTo As Double
            Get
                Return GetValue(CompareToProperty)
            End Get
            Set(value As Double)
                SetValue(CompareToProperty, value)
            End Set
        End Property

        Shared ReadOnly Property AnimationDurationProperty As DependencyProperty = DependencyProperty.Register("AnimationDuration", GetType(TimeSpan), GetType(RunningTextBlock), New UIPropertyMetadata(TimeSpan.FromSeconds(5)))
        Property AnimationDuration As TimeSpan
            Get
                Return GetValue(AnimationDurationProperty)
            End Get
            Set(value As TimeSpan)
                SetValue(AnimationDurationProperty, value)
            End Set
        End Property

        Private _SB As Storyboard
        Private Sub RunningTextBlock_SizeChanged(sender As Object, e As SizeChangedEventArgs) Handles Me.SizeChanged
            CheckSize()
        End Sub

        Private Sub CheckSize()
            If Not IsEnabled Then
                Me.BeginAnimation(MarginProperty, Nothing)
                Margin = New Thickness(0, Margin.Top, Margin.Right, Margin.Bottom)
                If _SB IsNot Nothing Then
                    _SB.Stop()
                    _SB = Nothing
                End If
                Return
            End If
            If Double.IsNaN(Me.ActualWidth) Then Return
            If _SB IsNot Nothing Then
                _SB.Stop()
                _SB = Nothing
            End If
            If Me.ActualWidth > CompareTo AndAlso Not String.IsNullOrEmpty(Text) Then
#Disable Warning
                Dim TF As New FormattedText(Text, Globalization.CultureInfo.CurrentCulture, FlowDirection.LeftToRight, New Typeface(Me.FontFamily, Me.FontStyle, Me.FontWeight, Me.FontStretch), Me.FontSize, Brushes.White)
#Enable Warning
                Dim sb As New Storyboard With {.RepeatBehavior = RepeatBehavior.Forever}
                Dim sO As New ThicknessAnimation(Me.Margin, New Thickness(-TF.Width, Margin.Top, Margin.Right, Margin.Bottom), New Duration(AnimationDuration))
                Dim sI As New ThicknessAnimation(New Thickness(+TF.Width, Margin.Top, Margin.Right, Margin.Bottom), New Thickness(0, Margin.Top, Margin.Right, Margin.Bottom), New Duration(AnimationDuration))
                Storyboard.SetTarget(sO, Me) : Storyboard.SetTarget(sI, Me)
                Storyboard.SetTargetProperty(sO, New PropertyPath("Margin")) : Storyboard.SetTargetProperty(sI, New PropertyPath("Margin"))
                sb.Children.Add(sO) : sb.Children.Add(sI)
                sI.BeginTime = AnimationDuration
                sb.Begin()
                _SB = sb
            Else
                Me.BeginAnimation(MarginProperty, New ThicknessAnimation(New Thickness(0, Margin.Top, Margin.Right, Margin.Bottom), TimeSpan.FromMilliseconds(500)))
            End If
        End Sub

        Private Sub RunningTextBlock_IsEnabledChanged(sender As Object, e As DependencyPropertyChangedEventArgs) Handles Me.IsEnabledChanged
            CheckSize()
        End Sub
    End Class
End Namespace