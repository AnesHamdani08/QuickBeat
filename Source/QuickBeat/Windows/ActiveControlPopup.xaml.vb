Imports QuickBeat.Utilities
Namespace UI.Windows
    Public Class ActiveControlPopup
        Private _IsShown As Boolean = False
        Private _IsAnimating As Boolean = False
        Private _ActiveStoryBoard As Animation.Storyboard

        ReadOnly Property IsShown As Boolean
            Get
                Return _IsShown
            End Get
        End Property

        ReadOnly Property IsOpen As Boolean
            Get
                Return MaxWidth > 40
            End Get
        End Property

        Public Shared TimeoutProperty As DependencyProperty = DependencyProperty.Register("Timeout", GetType(Integer), GetType(ActiveControlPopup), New PropertyMetadata(2000))
        Property Timeout As Integer
            Get
                Return GetValue(TimeoutProperty)
            End Get
            Set(value As Integer)
                SetValue(TimeoutProperty, value)
            End Set
        End Property

        Public Shared DynamicBackgroundProperty As DependencyProperty = DependencyProperty.Register("DynamicBackground", GetType(Boolean), GetType(ActiveControlPopup), New PropertyMetadata(True))
        Property DynamicBackground As Boolean
            Get
                Return GetValue(DynamicBackgroundProperty)
            End Get
            Set(value As Boolean)
                SetValue(DynamicBackgroundProperty, value)
            End Set
        End Property

        Public Shared FillBackgroundProperty As DependencyProperty = DependencyProperty.Register("FillBackground", GetType(Brush), GetType(ActiveControlPopup), New UIPropertyMetadata(New SolidColorBrush(Colors.Transparent)))
        Property FillBackground As Brush
            Get
                Return GetValue(FillBackgroundProperty)
            End Get
            Set(value As Brush)
                SetValue(FillBackgroundProperty, value)
            End Set
        End Property

        Public Shared AutoCloseProperty As DependencyProperty = DependencyProperty.Register("AutoClose", GetType(Boolean), GetType(ActiveControlPopup), New PropertyMetadata(True))
        Property AutoClose As Boolean
            Get
                Return GetValue(AutoCloseProperty)
            End Get
            Set(value As Boolean)
                SetValue(AutoCloseProperty, value)
            End Set
        End Property

        Public Shared LocationProperty As DependencyProperty = DependencyProperty.Register("Location", GetType(StartupLocation), GetType(ActiveControlPopup), New UIPropertyMetadata(StartupLocation.Top))
        Property Location As StartupLocation
            Get
                Return GetValue(LocationProperty)
            End Get
            Set(value As StartupLocation)
                SetValue(LocationProperty, value)
            End Set
        End Property

        Private Sub NotificationPopUp_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
            If DynamicBackground AndAlso SharedProperties.Instance.Player.StreamMetadata?.DefaultCover IsNot Nothing Then
                Dim Avg = CommonFunctions.GetAverageColor(SharedProperties.Instance.Player.StreamMetadata.DefaultCover)
                Dim Luma = Avg.Luminance
                If HandyControl.Themes.ThemeManager.Current.ApplicationTheme = HandyControl.Themes.ApplicationTheme.Dark Then
                    If Math.Floor(Luma) >= 128 Then
                        Dim xLuma = Luma - 200
                        If xLuma < 0 Then xLuma = 0
                        Avg.R -= xLuma
                        Avg.G -= xLuma
                        Avg.B -= xLuma
                    End If
                Else
                    If Math.Floor(Luma) <= 128 Then
                        Dim xLuma = 200 - Avg.Luminance
                        If xLuma > 255 Then xLuma = 255
                        Avg.R += (xLuma)
                        Avg.G += (xLuma)
                        Avg.B += (xLuma)
                    End If
                End If
                Me.FillBackground = New SolidColorBrush(Avg)
            Else
                If HandyControl.Themes.ThemeManager.Current.ApplicationTheme = HandyControl.Themes.ApplicationTheme.Dark Then
                    Me.FillBackground = Brushes.Black
                Else
                    Me.FillBackground = Brushes.White
                End If
            End If
            AddHandler SharedProperties.Instance.Player.MetadataChanged, New Player.Player.MetadataChangedEventHandler(Sub()
                                                                                                                           If Me.MaxWidth > 40 Then
                                                                                                                               Dim ft As New FormattedText(If(SharedProperties.Instance.Player.StreamMetadata?.TitleArtist, "N/A"), Globalization.CultureInfo.CurrentCulture, FlowDirection.LeftToRight, New Typeface(Me.FontFamily, FontStyles.Normal, FontWeights.Regular, FontStretches.Normal), 12, New SolidColorBrush(Color.FromRgb(255, 193, 7)), VisualTreeHelper.GetDpi(Me).PixelsPerDip)
                                                                                                                               Me.BeginAnimation(MaxWidthProperty, New Animation.DoubleAnimation(ft.Width + 60, New Duration(TimeSpan.FromMilliseconds(500))) With {.BeginTime = TimeSpan.FromMilliseconds(500), .AccelerationRatio = 0.95, .EasingFunction = New Animation.PowerEase() With {.Power = 2, .EasingMode = Animation.EasingMode.EaseInOut}})
                                                                                                                               Me.BeginAnimation(LeftProperty, New Animation.DoubleAnimation(My.Computer.Screen.WorkingArea.Width / 2 - ft.Width / 2, New Duration(TimeSpan.FromMilliseconds(1500))) With {.BeginTime = TimeSpan.FromMilliseconds(500), .DecelerationRatio = 0.9, .EasingFunction = New Animation.PowerEase() With {.Power = 2, .EasingMode = Animation.EasingMode.EaseInOut}})
                                                                                                                           End If
                                                                                                                           If DynamicBackground AndAlso SharedProperties.Instance.Player.StreamMetadata?.DefaultCover IsNot Nothing Then
                                                                                                                               Dim Avg = CommonFunctions.GetAverageColor(SharedProperties.Instance.Player.StreamMetadata.DefaultCover)
                                                                                                                               Dim Luma = Avg.Luminance
                                                                                                                               If HandyControl.Themes.ThemeManager.Current.ApplicationTheme = HandyControl.Themes.ApplicationTheme.Dark Then
                                                                                                                                   If Math.Floor(Luma) >= 128 Then
                                                                                                                                       Dim xLuma = Luma - 200
                                                                                                                                       If xLuma < 0 Then xLuma = 0
                                                                                                                                       Avg.R -= xLuma
                                                                                                                                       Avg.G -= xLuma
                                                                                                                                       Avg.B -= xLuma
                                                                                                                                   End If
                                                                                                                               Else
                                                                                                                                   If Math.Floor(Luma) <= 128 Then
                                                                                                                                       Dim xLuma = 200 - Avg.Luminance
                                                                                                                                       If xLuma > 255 Then xLuma = 255
                                                                                                                                       Avg.R += (xLuma)
                                                                                                                                       Avg.G += (xLuma)
                                                                                                                                       Avg.B += (xLuma)
                                                                                                                                   End If
                                                                                                                               End If
                                                                                                                               Me.FillBackground = New SolidColorBrush(Avg)
                                                                                                                           End If
                                                                                                                       End Sub)
        End Sub

        Public Shadows Sub Show()
            _IsShown = True
            MyBase.Show()
        End Sub

        ''' <summary>
        ''' Shows the popup in a beatiful animation UwU
        ''' </summary>
        Public Sub Reveal()
            _ActiveStoryBoard?.Stop()
            _IsAnimating = True

            Me.MaxWidth = 40
            Dim ft As New FormattedText(If(SharedProperties.Instance.Player.StreamMetadata?.TitleArtist, "N/A"), Globalization.CultureInfo.CurrentCulture, FlowDirection.LeftToRight, New Typeface(Me.FontFamily, FontStyles.Normal, FontWeights.Regular, FontStretches.Normal), 12, New SolidColorBrush(Color.FromRgb(255, 193, 7)), VisualTreeHelper.GetDpi(Me).PixelsPerDip)
            Me.Left = My.Computer.Screen.WorkingArea.Width / 2
            Me.Top = If(Location = StartupLocation.Top, -10, My.Computer.Screen.WorkingArea.Height + 10)

            If Not _IsShown Then Show()

            Dim sb As New Animation.Storyboard With {.BeginTime = TimeSpan.FromMilliseconds(100)}
            _ActiveStoryBoard = sb

            Dim tAnim As New Animation.DoubleAnimation(If(Location = StartupLocation.Top, 10, My.Computer.Screen.WorkingArea.Height - 60), New Duration(TimeSpan.FromMilliseconds(500))) With {.DecelerationRatio = 0.9, .EasingFunction = New Animation.PowerEase() With {.Power = 2, .EasingMode = Animation.EasingMode.EaseInOut}}
            Dim sAnim As New Animation.DoubleAnimation(40, ft.Width + 60, New Duration(TimeSpan.FromMilliseconds(500))) With {.BeginTime = TimeSpan.FromMilliseconds(500), .AccelerationRatio = 0.95, .EasingFunction = New Animation.PowerEase() With {.Power = 2, .EasingMode = Animation.EasingMode.EaseInOut}} '30 + 5 +5 +10
            Dim OsAnim As New Animation.DoubleAnimation(0, 1, New Duration(TimeSpan.FromMilliseconds(250))) With {.DecelerationRatio = 0.9, .EasingFunction = New Animation.PowerEase() With {.Power = 2, .EasingMode = Animation.EasingMode.EaseInOut}} '30 + 5 +5
            Dim lAnim As New Animation.DoubleAnimation((My.Computer.Screen.WorkingArea.Width / 2) - ft.Width / 2, New Duration(TimeSpan.FromMilliseconds(1500))) With {.BeginTime = TimeSpan.FromMilliseconds(500), .DecelerationRatio = 0.9, .EasingFunction = New Animation.PowerEase() With {.Power = 2, .EasingMode = Animation.EasingMode.EaseInOut}}

            Animation.Storyboard.SetTarget(tAnim, Me)
            Animation.Storyboard.SetTarget(sAnim, Me)
            Animation.Storyboard.SetTarget(OsAnim, Me)
            Animation.Storyboard.SetTarget(lAnim, Me)
            Animation.Storyboard.SetTargetProperty(tAnim, New PropertyPath("(Window.Top)"))
            Animation.Storyboard.SetTargetProperty(sAnim, New PropertyPath("(Window.MaxWidth)"))
            Animation.Storyboard.SetTargetProperty(OsAnim, New PropertyPath("Opacity"))
            Animation.Storyboard.SetTargetProperty(lAnim, New PropertyPath("(Window.Left)"))
            sb.Children.Add(tAnim)
            sb.Children.Add(sAnim)
            sb.Children.Add(OsAnim)
            sb.Children.Add(lAnim)

            sb.Begin(Me, isControllable:=True)

            AddHandler sb.Completed, Sub()
                                         _IsAnimating = False
                                     End Sub


        End Sub

        ''' <summary>
        ''' Hides the popup in a beatiful animation UwU
        ''' </summary>
        Public Sub Conceal()
            _ActiveStoryBoard?.Stop()
            _IsAnimating = True
            Dim sb As New Animation.Storyboard
            _ActiveStoryBoard = sb
            Dim hAnim As New Animation.DoubleAnimation(40, New Duration(TimeSpan.FromMilliseconds(500))) With {.EasingFunction = New Animation.PowerEase() With {.Power = 2, .EasingMode = Animation.EasingMode.EaseInOut}}
            Dim OhAnim As New Animation.DoubleAnimation(1, 0, New Duration(TimeSpan.FromMilliseconds(350))) With {.BeginTime = TimeSpan.FromMilliseconds(1000), .AccelerationRatio = 0.95, .EasingFunction = New Animation.PowerEase() With {.Power = 2, .EasingMode = Animation.EasingMode.EaseInOut}}
            Dim nlAnim As New Animation.DoubleAnimation(My.Computer.Screen.WorkingArea.Width / 2, New Duration(TimeSpan.FromMilliseconds(500))) With {.DecelerationRatio = 0.9, .EasingFunction = New Animation.PowerEase() With {.Power = 2, .EasingMode = Animation.EasingMode.EaseInOut}}
            Dim ntAnim As New Animation.DoubleAnimation(If(Location = StartupLocation.Top, -10, My.Computer.Screen.WorkingArea.Height + 10), New Duration(TimeSpan.FromMilliseconds(350))) With {.BeginTime = TimeSpan.FromMilliseconds(1000), .AccelerationRatio = 0.95, .EasingFunction = New Animation.PowerEase() With {.Power = 2, .EasingMode = Animation.EasingMode.EaseInOut}}

            Animation.Storyboard.SetTarget(hAnim, Me)
            Animation.Storyboard.SetTarget(OhAnim, Me)
            Animation.Storyboard.SetTarget(nlAnim, Me)
            Animation.Storyboard.SetTarget(ntAnim, Me)
            Animation.Storyboard.SetTargetProperty(hAnim, New PropertyPath("(Window.MaxWidth)"))
            Animation.Storyboard.SetTargetProperty(OhAnim, New PropertyPath("Opacity"))
            Animation.Storyboard.SetTargetProperty(nlAnim, New PropertyPath("(Window.Left)"))
            Animation.Storyboard.SetTargetProperty(ntAnim, New PropertyPath("(Window.Top)"))
            sb.Children.Add(hAnim)
            sb.Children.Add(OhAnim)
            sb.Children.Add(nlAnim)
            sb.Children.Add(ntAnim)

            AddHandler sb.Completed, Sub()
                                         _IsShown = False
                                         _IsAnimating = False
                                         If AutoClose Then Me.Close()
                                     End Sub

            sb.Begin(Me, isControllable:=True)

            If Border_Main.MaxHeight <> 54 Then
                Border_Main.BeginAnimation(MaxHeightProperty, New Animation.DoubleAnimation(54, New Duration(TimeSpan.FromMilliseconds(250))))
            End If
        End Sub

        ''' <summary>
        ''' Collapse (Doesn't close) the popup in a beatiful animation UwU
        ''' </summary>
        ''' <remarks>
        ''' Ignores <see cref="AutoClose"/>
        ''' </remarks>
        Public Sub Collapse()
            _ActiveStoryBoard?.Stop()
            _IsAnimating = True
            Dim sb As New Animation.Storyboard()
            _ActiveStoryBoard = sb

            Dim hAnim As New Animation.DoubleAnimation(40, New Duration(TimeSpan.FromMilliseconds(500))) With {.EasingFunction = New Animation.PowerEase() With {.Power = 2, .EasingMode = Animation.EasingMode.EaseInOut}}
            Dim nlAnim As New Animation.DoubleAnimation(My.Computer.Screen.WorkingArea.Width / 2, New Duration(TimeSpan.FromMilliseconds(500))) With {.DecelerationRatio = 0.9, .EasingFunction = New Animation.PowerEase() With {.Power = 2, .EasingMode = Animation.EasingMode.EaseInOut}}

            Animation.Storyboard.SetTarget(hAnim, Me)
            Animation.Storyboard.SetTarget(nlAnim, Me)
            Animation.Storyboard.SetTargetProperty(hAnim, New PropertyPath("(Window.MaxWidth)"))
            Animation.Storyboard.SetTargetProperty(nlAnim, New PropertyPath("(Window.Left)"))
            sb.Children.Add(hAnim)
            sb.Children.Add(nlAnim)

            sb.Begin(Me, isControllable:=True)

            AddHandler sb.Completed, Sub()
                                         _IsAnimating = False
                                     End Sub

            If Border_Main.MaxHeight <> 54 Then
                Border_Main.BeginAnimation(MaxHeightProperty, New Animation.DoubleAnimation(54, New Duration(TimeSpan.FromMilliseconds(250))))
            End If
        End Sub

        ''' <summary>
        ''' <see cref="Reveal()"/> + <see cref="Conceal()"/> while keeping <see cref="Timeout"/> between the two calls   
        ''' </summary>
        ''' <remarks>
        ''' Doesn't internally call <see cref="Reveal()"/> and <see cref="Conceal()"/>, all code is re-written inside.    
        ''' <see cref="AutoClose"/> is taken into account
        ''' </remarks>
        Public Sub RevealConceal()
            _ActiveStoryBoard?.Stop()
            _IsAnimating = True
            Me.MaxWidth = 40
            Dim ft As New FormattedText(If(SharedProperties.Instance.Player.StreamMetadata?.TitleArtist, "N/A"), Globalization.CultureInfo.CurrentCulture, FlowDirection.LeftToRight, New Typeface(Me.FontFamily, FontStyles.Normal, FontWeights.Regular, FontStretches.Normal), 12, New SolidColorBrush(Color.FromRgb(255, 193, 7)), VisualTreeHelper.GetDpi(Me).PixelsPerDip)
            Me.Left = My.Computer.Screen.WorkingArea.Width / 2
            Me.Top = If(Location = StartupLocation.Top, -10, My.Computer.Screen.WorkingArea.Height + 10)

            If Not _IsShown Then Show()

            Dim sb As New Animation.Storyboard With {.BeginTime = TimeSpan.FromMilliseconds(100)}
            _ActiveStoryBoard = sb

            Dim tAnim As New Animation.DoubleAnimation(If(Location = StartupLocation.Top, 10, My.Computer.Screen.WorkingArea.Height - 60), New Duration(TimeSpan.FromMilliseconds(500))) With {.DecelerationRatio = 0.9, .EasingFunction = New Animation.PowerEase() With {.Power = 2, .EasingMode = Animation.EasingMode.EaseInOut}}
            Dim sAnim As New Animation.DoubleAnimation(40, ft.Width + 60, New Duration(TimeSpan.FromMilliseconds(500))) With {.BeginTime = TimeSpan.FromMilliseconds(500), .AccelerationRatio = 0.95, .EasingFunction = New Animation.PowerEase() With {.Power = 2, .EasingMode = Animation.EasingMode.EaseInOut}} '30 + 5 +5 +10
            Dim OsAnim As New Animation.DoubleAnimation(0, 1, New Duration(TimeSpan.FromMilliseconds(250))) With {.DecelerationRatio = 0.9, .EasingFunction = New Animation.PowerEase() With {.Power = 2, .EasingMode = Animation.EasingMode.EaseInOut}} '30 + 5 +5
            Dim lAnim As New Animation.DoubleAnimation(My.Computer.Screen.WorkingArea.Width / 2 - ft.Width / 2, New Duration(TimeSpan.FromMilliseconds(1500))) With {.BeginTime = TimeSpan.FromMilliseconds(500), .DecelerationRatio = 0.9, .EasingFunction = New Animation.PowerEase() With {.Power = 2, .EasingMode = Animation.EasingMode.EaseInOut}}

            Dim hAnim As New Animation.DoubleAnimation(40, New Duration(TimeSpan.FromMilliseconds(500))) With {.BeginTime = TimeSpan.FromMilliseconds(500 + Timeout), .EasingFunction = New Animation.PowerEase() With {.Power = 2, .EasingMode = Animation.EasingMode.EaseInOut}}
            Dim OhAnim As New Animation.DoubleAnimation(1, 0, New Duration(TimeSpan.FromMilliseconds(350))) With {.BeginTime = TimeSpan.FromMilliseconds(1500 + Timeout), .AccelerationRatio = 0.95, .EasingFunction = New Animation.PowerEase() With {.Power = 2, .EasingMode = Animation.EasingMode.EaseInOut}}
            Dim nlAnim As New Animation.DoubleAnimation(My.Computer.Screen.WorkingArea.Width / 2, New Duration(TimeSpan.FromMilliseconds(500))) With {.BeginTime = TimeSpan.FromMilliseconds(500 + Timeout), .DecelerationRatio = 0.9, .EasingFunction = New Animation.PowerEase() With {.Power = 2, .EasingMode = Animation.EasingMode.EaseInOut}}
            Dim ntAnim As New Animation.DoubleAnimation(If(Location = StartupLocation.Top, -10, My.Computer.Screen.WorkingArea.Height + 10), New Duration(TimeSpan.FromMilliseconds(350))) With {.BeginTime = TimeSpan.FromMilliseconds(1500 + Timeout), .AccelerationRatio = 0.95, .EasingFunction = New Animation.PowerEase() With {.Power = 2, .EasingMode = Animation.EasingMode.EaseInOut}}

            Animation.Storyboard.SetTarget(tAnim, Me)
            Animation.Storyboard.SetTarget(sAnim, Me)
            Animation.Storyboard.SetTarget(hAnim, Me)
            Animation.Storyboard.SetTarget(OsAnim, Me)
            Animation.Storyboard.SetTarget(OhAnim, Me)
            Animation.Storyboard.SetTarget(lAnim, Me)
            Animation.Storyboard.SetTarget(nlAnim, Me)
            Animation.Storyboard.SetTarget(ntAnim, Me)

            Animation.Storyboard.SetTargetProperty(tAnim, New PropertyPath("(Window.Top)"))
            Animation.Storyboard.SetTargetProperty(sAnim, New PropertyPath("(Window.MaxWidth)"))
            Animation.Storyboard.SetTargetProperty(hAnim, New PropertyPath("(Window.MaxWidth)"))
            Animation.Storyboard.SetTargetProperty(OsAnim, New PropertyPath("Opacity"))
            Animation.Storyboard.SetTargetProperty(OhAnim, New PropertyPath("Opacity"))
            Animation.Storyboard.SetTargetProperty(lAnim, New PropertyPath("(Window.Left)"))
            Animation.Storyboard.SetTargetProperty(nlAnim, New PropertyPath("(Window.Left)"))
            Animation.Storyboard.SetTargetProperty(ntAnim, New PropertyPath("(Window.Top)"))

            sb.Children.Add(tAnim)
            sb.Children.Add(sAnim)
            sb.Children.Add(hAnim)
            sb.Children.Add(OsAnim)
            sb.Children.Add(OhAnim)
            sb.Children.Add(lAnim)
            sb.Children.Add(nlAnim)
            sb.Children.Add(ntAnim)

            AddHandler sb.Completed, Sub()
                                         _IsShown = False
                                         _IsAnimating = False
                                         If AutoClose Then Me.Close()
                                     End Sub

            sb.Begin(Me, isControllable:=True)



        End Sub


        ''' <summary>
        ''' <see cref="Reveal()"/> + <see cref="Collapse()"/> while keeping <see cref="Timeout"/> between the two calls   
        ''' </summary>
        ''' <remarks>
        ''' Doesn't internally call <see cref="Reveal"/> and <see cref="Collapse()"/>, all code is re-written inside.    
        ''' Ignores <see cref="AutoClose"/>
        ''' </remarks>
        Public Sub RevealCollapse()
            _ActiveStoryBoard?.Stop()
            _IsAnimating = True
            Me.MaxWidth = 40
            Dim ft As New FormattedText(If(SharedProperties.Instance.Player.StreamMetadata?.TitleArtist, "N/A"), Globalization.CultureInfo.CurrentCulture, FlowDirection.LeftToRight, New Typeface(Me.FontFamily, FontStyles.Normal, FontWeights.Regular, FontStretches.Normal), 12, New SolidColorBrush(Color.FromRgb(255, 193, 7)), VisualTreeHelper.GetDpi(Me).PixelsPerDip)
            Me.Left = My.Computer.Screen.WorkingArea.Width / 2
            Me.Top = If(Location = StartupLocation.Top, -10, My.Computer.Screen.WorkingArea.Height + 10)

            If Not _IsShown Then Show()

            Dim sb As New Animation.Storyboard With {.BeginTime = TimeSpan.FromMilliseconds(100)}
            _ActiveStoryBoard = sb

            Dim tAnim As New Animation.DoubleAnimation(If(Location = StartupLocation.Top, 10, My.Computer.Screen.WorkingArea.Height - 60), New Duration(TimeSpan.FromMilliseconds(500))) With {.DecelerationRatio = 0.9, .EasingFunction = New Animation.PowerEase() With {.Power = 2, .EasingMode = Animation.EasingMode.EaseInOut}}
            Dim sAnim As New Animation.DoubleAnimation(40, ft.Width + 60, New Duration(TimeSpan.FromMilliseconds(500))) With {.BeginTime = TimeSpan.FromMilliseconds(500), .AccelerationRatio = 0.95, .EasingFunction = New Animation.PowerEase() With {.Power = 2, .EasingMode = Animation.EasingMode.EaseInOut}} '30 + 5 +5 +10
            Dim OsAnim As New Animation.DoubleAnimation(0, 1, New Duration(TimeSpan.FromMilliseconds(250))) With {.DecelerationRatio = 0.9, .EasingFunction = New Animation.PowerEase() With {.Power = 2, .EasingMode = Animation.EasingMode.EaseInOut}} '30 + 5 +5
            Dim lAnim As New Animation.DoubleAnimation(My.Computer.Screen.WorkingArea.Width / 2 - ft.Width / 2, New Duration(TimeSpan.FromMilliseconds(1500))) With {.BeginTime = TimeSpan.FromMilliseconds(500), .DecelerationRatio = 0.9, .EasingFunction = New Animation.PowerEase() With {.Power = 2, .EasingMode = Animation.EasingMode.EaseInOut}}

            Dim hAnim As New Animation.DoubleAnimation(40, New Duration(TimeSpan.FromMilliseconds(500))) With {.BeginTime = TimeSpan.FromMilliseconds(500 + Timeout), .EasingFunction = New Animation.PowerEase() With {.Power = 2, .EasingMode = Animation.EasingMode.EaseInOut}}
            Dim nlAnim As New Animation.DoubleAnimation(My.Computer.Screen.WorkingArea.Width / 2, New Duration(TimeSpan.FromMilliseconds(500))) With {.BeginTime = TimeSpan.FromMilliseconds(500 + Timeout), .DecelerationRatio = 0.9, .EasingFunction = New Animation.PowerEase() With {.Power = 2, .EasingMode = Animation.EasingMode.EaseInOut}}

            Animation.Storyboard.SetTarget(tAnim, Me)
            Animation.Storyboard.SetTarget(sAnim, Me)
            Animation.Storyboard.SetTarget(hAnim, Me)
            Animation.Storyboard.SetTarget(OsAnim, Me)
            Animation.Storyboard.SetTarget(lAnim, Me)
            Animation.Storyboard.SetTarget(nlAnim, Me)

            Animation.Storyboard.SetTargetProperty(tAnim, New PropertyPath("(Window.Top)"))
            Animation.Storyboard.SetTargetProperty(sAnim, New PropertyPath("(Window.MaxWidth)"))
            Animation.Storyboard.SetTargetProperty(hAnim, New PropertyPath("(Window.MaxWidth)"))
            Animation.Storyboard.SetTargetProperty(OsAnim, New PropertyPath("Opacity"))
            Animation.Storyboard.SetTargetProperty(lAnim, New PropertyPath("(Window.Left)"))
            Animation.Storyboard.SetTargetProperty(nlAnim, New PropertyPath("(Window.Left)"))

            sb.Children.Add(tAnim)
            sb.Children.Add(sAnim)
            sb.Children.Add(hAnim)
            sb.Children.Add(OsAnim)
            sb.Children.Add(lAnim)
            sb.Children.Add(nlAnim)

            AddHandler sb.Completed, Sub()
                                         _IsAnimating = False
                                     End Sub

            sb.Begin(Me, isControllable:=True)



        End Sub

        Public Enum StartupLocation
            Top
            Bottom
        End Enum

        Private Sub Commands_Close_CanExecute(sender As Object, e As CanExecuteRoutedEventArgs)
            e.CanExecute = True
        End Sub

        Private Sub Commands_Close_Executed(sender As Object, e As ExecutedRoutedEventArgs)
            Conceal()
        End Sub

        Private Sub Commands_Collapse_CanExecute(sender As Object, e As CanExecuteRoutedEventArgs)
            e.CanExecute = True
        End Sub

        Private Sub Commands_Collapse_Executed(sender As Object, e As ExecutedRoutedEventArgs)
            Collapse()
        End Sub

        Public Shared Sub RevealConcealQuick(Text As String, Timeout As Integer, Optional Image As ImageSource = Nothing, Optional DynamicBG As Boolean = True, Optional Location As StartupLocation = StartupLocation.Top)
            Dim npu As New NotificationPopUp() With {.Text = Text, .Timeout = Timeout, .Image = Image, .AutoClose = True, .DynamicBackground = DynamicBG, .Location = Location}
            npu.RevealConceal()
        End Sub

        Private Sub Commands_Reveal_CanExecute(sender As Object, e As CanExecuteRoutedEventArgs)
            e.CanExecute = Me.MaxWidth <= 40
        End Sub

        Private Sub Commands_Reveal_Executed(sender As Object, e As ExecutedRoutedEventArgs)
            Reveal()
        End Sub

        Private Sub ActiveControlPopup_MouseEnter(sender As Object, e As MouseEventArgs) Handles Me.MouseEnter
            If Me.MaxWidth <= 40 Then Return
            Border_Main.BeginAnimation(MaxHeightProperty, New Animation.DoubleAnimation(90, New Duration(TimeSpan.FromMilliseconds(250))))
        End Sub

        Private Sub ActiveControlPopup_MouseLeave(sender As Object, e As MouseEventArgs) Handles Me.MouseLeave
            If Me.MaxWidth <= 40 AndAlso Border_Main.MaxHeight = 54 Then Return
            Border_Main.BeginAnimation(MaxHeightProperty, New Animation.DoubleAnimation(54, New Duration(TimeSpan.FromMilliseconds(250))))
        End Sub

        Private Sub ActiveControlPopup_Closed(sender As Object, e As EventArgs) Handles Me.Closed
            _IsShown = False
        End Sub

        Private Sub GeoImage_MouseRightButtonUp(sender As Object, e As MouseButtonEventArgs)
            Dim Mods = Keyboard.Modifiers
            If Mods.HasFlag(ModifierKeys.Control) Then
                If SharedProperties.Instance.Player IsNot Nothing Then SharedProperties.Instance.Player.Position += 10
            ElseIf Mods.HasFlag(ModifierKeys.Shift) Then
                SharedProperties.Instance.Player?.Next()
            Else
                SharedProperties.Instance.Player?.PlayPause()
            End If
        End Sub

        Private Sub GeoImage_PreviewMouseLeftButtonUp(sender As Object, e As MouseButtonEventArgs)
            Dim Mods = Keyboard.Modifiers
            If Mods.HasFlag(ModifierKeys.Control) Then
                If SharedProperties.Instance.Player IsNot Nothing Then SharedProperties.Instance.Player.Position -= 10
                e.Handled = True
            ElseIf Mods.HasFlag(ModifierKeys.Shift) Then
                SharedProperties.Instance.Player?.Previous()
                e.Handled = True
            End If
        End Sub

        Private Sub GeoImage_PreviewMouseDown(sender As Object, e As MouseButtonEventArgs)
            If e.ClickCount >= 2 AndAlso e.ChangedButton = MouseButton.Left Then
                SharedProperties.Instance.ActivateApp()
                Collapse()
                e.Handled = True
            End If
        End Sub

        Private Sub Button_ActivateApp_Click(sender As Object, e As RoutedEventArgs)
            SharedProperties.Instance.ActivateApp()
        End Sub

        Private Sub Button_Help_Click(sender As Object, e As RoutedEventArgs)
            HandyControl.Controls.MessageBox.Show(Me, Utilities.ResourceResolver.Strings.HINT_INFOTIP_KEYBOARD_NAVIGATION, ResourceResolver.Strings.APPNAME, icon:=MessageBoxImage.Information)
        End Sub
    End Class
End Namespace