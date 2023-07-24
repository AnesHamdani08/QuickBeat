Namespace Controls
    Public Class GeoImage
        Inherits Image
        Overloads Property Source As ImageSource
            Get
                Return MyBase.Source
            End Get
            Set(value As ImageSource)
                If value Is Nothing AndAlso GeoDefault IsNot Nothing Then
                    MyBase.Source = New DrawingImage(New GeometryDrawing(GeoBrush, GeoPen, GeoDefault))
                Else
                    MyBase.Source = value
                End If
            End Set
        End Property
        Shared ReadOnly Property GeoDefaultProperty As DependencyProperty = DependencyProperty.Register("GeoDefault", GetType(Geometry), GetType(GeoImage), New UIPropertyMetadata(Nothing, New PropertyChangedCallback(Sub(d As DependencyObject, e As DependencyPropertyChangedEventArgs)
                                                                                                                                                                                                                            If d Is Nothing OrElse Not TypeOf d Is GeoImage Then Return
                                                                                                                                                                                                                            If e.NewValue Is Nothing Then CType(d, GeoImage).Source = Nothing : Return
                                                                                                                                                                                                                            With CType(d, GeoImage)
                                                                                                                                                                                                                                If .Source Is Nothing Then .SetSource(New DrawingImage(New GeometryDrawing(.GeoBrush, .GeoPen, e.NewValue)))
                                                                                                                                                                                                                            End With
                                                                                                                                                                                                                        End Sub)))
        ''' <summary>
        ''' Used in case <see cref="Source"/> is nothing
        ''' </summary>
        ''' <returns></returns>
        Property GeoDefault As Geometry
            Get
                Return GetValue(GeoDefaultProperty)
            End Get
            Set(value As Geometry)
                SetValue(GeoDefaultProperty, value)
            End Set
        End Property
        Shared ReadOnly Property GeoSourceProperty As DependencyProperty = DependencyProperty.Register("GeoSource", GetType(Geometry), GetType(GeoImage), New UIPropertyMetadata(Nothing, New PropertyChangedCallback(Sub(d As DependencyObject, e As DependencyPropertyChangedEventArgs)
                                                                                                                                                                                                                          If d Is Nothing OrElse Not TypeOf d Is GeoImage Then Return
                                                                                                                                                                                                                          If e.NewValue Is Nothing Then CType(d, GeoImage).Source = Nothing : Return
                                                                                                                                                                                                                          With CType(d, GeoImage)
                                                                                                                                                                                                                              .Source = New DrawingImage(New GeometryDrawing(.GeoBrush, .GeoPen, e.NewValue))
                                                                                                                                                                                                                          End With
                                                                                                                                                                                                                      End Sub)))
        Property GeoSource As Geometry
            Get
                Return GetValue(GeoSourceProperty)
            End Get
            Set(value As Geometry)
                SetValue(GeoSourceProperty, value)
            End Set
        End Property
        Private _GeoPen As Pen = Nothing
        Property GeoPen As Pen
            Get
                Return _GeoPen
            End Get
            Set(value As Pen)
                _GeoPen = value
                If Source Is Nothing Then Return
                If TypeOf Source IsNot DrawingImage Then Return
                If TryCast(TryCast(Source, DrawingImage)?.Drawing, GeometryDrawing)?.IsFrozen Then
                    Dim cGeo = TryCast(TryCast(Source, DrawingImage)?.Drawing, GeometryDrawing)?.Clone
                    If cGeo IsNot Nothing Then
                        cGeo.Brush = GeoBrush
                        cGeo.Pen = value
                        CType(Source, DrawingImage).Drawing = cGeo
                    End If
                Else
                    CType(CType(Source, DrawingImage).Drawing, GeometryDrawing).Pen = value
                    CType(CType(Source, DrawingImage).Drawing, GeometryDrawing).Brush = GeoBrush
                End If
            End Set
        End Property

        Shared ReadOnly Property GeoBrushProperty As DependencyProperty = DependencyProperty.Register("GeoBrush", GetType(Brush), GetType(GeoImage), New UIPropertyMetadata(Brushes.Black, New PropertyChangedCallback(Sub(d As DependencyObject, e As DependencyPropertyChangedEventArgs)
                                                                                                                                                                                                                           If e.NewValue Is Nothing OrElse CType(d, GeoImage).Source Is Nothing Then Return
                                                                                                                                                                                                                           With CType(d, GeoImage)
                                                                                                                                                                                                                               If TypeOf CType(d, GeoImage).Source IsNot DrawingImage Then Return
                                                                                                                                                                                                                               If TryCast(TryCast(.Source, DrawingImage)?.Drawing, GeometryDrawing)?.IsFrozen Then
                                                                                                                                                                                                                                   Dim cGeo = TryCast(TryCast(.Source, DrawingImage)?.Drawing, GeometryDrawing)?.Clone
                                                                                                                                                                                                                                   If cGeo IsNot Nothing Then
                                                                                                                                                                                                                                       cGeo.Brush = e.NewValue
                                                                                                                                                                                                                                       cGeo.Pen = .GeoPen
                                                                                                                                                                                                                                       CType(.Source, DrawingImage).Drawing = cGeo
                                                                                                                                                                                                                                   End If
                                                                                                                                                                                                                               Else
                                                                                                                                                                                                                                   CType(CType(.Source, DrawingImage).Drawing, GeometryDrawing).Pen = .GeoPen
                                                                                                                                                                                                                                   CType(CType(.Source, DrawingImage).Drawing, GeometryDrawing).Brush = e.NewValue
                                                                                                                                                                                                                               End If
                                                                                                                                                                                                                           End With
                                                                                                                                                                                                                       End Sub)))
        Property GeoBrush As Brush
            Get
                Return GetValue(GeoBrushProperty)
            End Get
            Set(value As Brush)
                SetValue(GeoBrushProperty, value)
            End Set
        End Property
        Shared ReadOnly Property CommandProperty As DependencyProperty = DependencyProperty.Register("Command", GetType(ICommand), GetType(GeoImage), New PropertyMetadata(Nothing, New PropertyChangedCallback(Sub(d As DependencyObject, e As DependencyPropertyChangedEventArgs)
                                                                                                                                                                                                                    With CType(d, GeoImage)
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
        Shared ReadOnly Property CommandParameterProperty As DependencyProperty = DependencyProperty.Register("CommandParameter", GetType(Object), GetType(GeoImage))
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
        Shared ReadOnly Property IsCheckedProperty As DependencyProperty = DependencyProperty.Register("IsChecked", GetType(Boolean), GetType(GeoImage), New UIPropertyMetadata(False, New PropertyChangedCallback(Sub(d As DependencyObject, e As DependencyPropertyChangedEventArgs)
                                                                                                                                                                                                                       If e.NewValue Then
                                                                                                                                                                                                                           If CType(d, GeoImage).CheckedBrush IsNot Nothing Then
                                                                                                                                                                                                                               With CType(d, GeoImage)
                                                                                                                                                                                                                                   If TypeOf CType(d, GeoImage).Source IsNot DrawingImage Then Return
                                                                                                                                                                                                                                   If TryCast(TryCast(.Source, DrawingImage)?.Drawing, GeometryDrawing)?.IsFrozen Then
                                                                                                                                                                                                                                       Dim cGeo = TryCast(TryCast(.Source, DrawingImage)?.Drawing, GeometryDrawing)?.Clone
                                                                                                                                                                                                                                       If cGeo IsNot Nothing Then
                                                                                                                                                                                                                                           cGeo.Brush = .CheckedBrush
                                                                                                                                                                                                                                           cGeo.Pen = .GeoPen
                                                                                                                                                                                                                                           CType(.Source, DrawingImage).Drawing = cGeo
                                                                                                                                                                                                                                       End If
                                                                                                                                                                                                                                   Else
                                                                                                                                                                                                                                       CType(CType(.Source, DrawingImage).Drawing, GeometryDrawing).Pen = .GeoPen
                                                                                                                                                                                                                                       CType(CType(.Source, DrawingImage).Drawing, GeometryDrawing).Brush = .CheckedBrush
                                                                                                                                                                                                                                   End If
                                                                                                                                                                                                                               End With
                                                                                                                                                                                                                           End If
                                                                                                                                                                                                                       Else
                                                                                                                                                                                                                           CType(d, GeoImage).InvalidateGeoBrush()
                                                                                                                                                                                                                       End If
                                                                                                                                                                                                                   End Sub)))
        Property IsChecked As Boolean
            Get
                Return GetValue(IsCheckedProperty)
            End Get
            Set(value As Boolean)
                SetValue(IsCheckedProperty, value)
            End Set
        End Property
        Shared ReadOnly Property CheckedBrushProperty As DependencyProperty = DependencyProperty.Register("CheckedBrush", GetType(Brush), GetType(GeoImage), New UIPropertyMetadata(Nothing, New PropertyChangedCallback(Sub(d As DependencyObject, e As DependencyPropertyChangedEventArgs)
                                                                                                                                                                                                                             If e.NewValue IsNot Nothing AndAlso CType(d, GeoImage).IsChecked Then
                                                                                                                                                                                                                                 CType(d, GeoImage).GeoBrush = e.NewValue
                                                                                                                                                                                                                             End If
                                                                                                                                                                                                                         End Sub)))
        Property CheckedBrush As Brush
            Get
                Return GetValue(CheckedBrushProperty)
            End Get
            Set(value As Brush)
                SetValue(CheckedBrushProperty, value)
            End Set
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
        Private Sub GeoImage_MouseEnter(sender As Object, e As MouseEventArgs) Handles Me.MouseEnter
            If IsEnabled OrElse Opacity - 0.25 < 0 Then
                'BeginAnimation(OpacityProperty, New Animation.DoubleAnimation(_Opacity - 0.25, New Duration(TimeSpan.FromMilliseconds(200))))
                MyBase.Opacity = _Opacity - 0.25
            End If
        End Sub
        Private Sub GeoImage_MouseLeave(sender As Object, e As MouseEventArgs) Handles Me.MouseLeave
            If _Opacity > 0 Then
                'BeginAnimation(OpacityProperty, New Animation.DoubleAnimation(_Opacity, New Duration(TimeSpan.FromMilliseconds(200))))
                MyBase.Opacity = _Opacity
            End If
        End Sub

        Private Sub GeoImage_MouseLeftButtonUp(sender As Object, e As MouseButtonEventArgs) Handles Me.MouseLeftButtonUp
            If IsEnabled Then Command?.Execute(CommandParameter)
        End Sub
        ''' <summary>
        ''' bypasses <see cref="Source"/> and directly sets the image.
        ''' </summary>
        ''' <param name="Image"></param>
        Private Sub SetSource(Image As ImageSource)
            MyBase.Source = Image
        End Sub
        ''' <summary>
        ''' Invalidates the <see cref="GeoBrush"/> and re-paint the image
        ''' </summary>
        Public Sub InvalidateGeoBrush()
            If GeoBrush Is Nothing OrElse Source Is Nothing Then Return
            If TypeOf Source IsNot DrawingImage Then Return
            If TryCast(TryCast(Source, DrawingImage)?.Drawing, GeometryDrawing)?.IsFrozen Then
                Dim cGeo = TryCast(TryCast(Source, DrawingImage)?.Drawing, GeometryDrawing)?.Clone
                If cGeo IsNot Nothing Then
                    cGeo.Brush = GeoBrush
                    cGeo.Pen = GeoPen
                    CType(Source, DrawingImage).Drawing = cGeo
                End If
            Else
                CType(CType(Source, DrawingImage).Drawing, GeometryDrawing).Pen = GeoPen
                CType(CType(Source, DrawingImage).Drawing, GeometryDrawing).Brush = GeoBrush
            End If
        End Sub
    End Class
End Namespace