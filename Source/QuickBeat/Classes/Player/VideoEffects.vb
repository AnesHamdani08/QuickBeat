Imports System.ComponentModel
Imports System.Timers
Imports QuickBeat.Classes
Imports QuickBeat.Interfaces
Imports Un4seen.Bass.AddOn.Sfx
Imports Un4seen.Bass

Namespace Player
    Public Class VideoEffects
        Public MustInherit Class VideoEffect
            Implements ComponentModel.INotifyPropertyChanged, Interfaces.IStartupItem

            Public Event OnUpdateRequested(frame As ImageSource)
            Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged
            Protected Overridable Sub OnPropertyChanged(<Runtime.CompilerServices.CallerMemberName> Optional CallerName As String = Nothing)
                RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(CallerName))
            End Sub

            Private _Name As String
            Property Name As String
                Get
                    Return _Name
                End Get
                Set(value As String)
                    _Name = value
                    If Configuration IsNot Nothing Then Configuration.Name = value
                    OnPropertyChanged()
                End Set
            End Property

            Private _UsesCustomControl As Boolean
            Property UsesCustomControl As Boolean
                Get
                    Return _UsesCustomControl
                End Get
                Set(value As Boolean)
                    _UsesCustomControl = value
                    OnPropertyChanged()
                End Set
            End Property

            Private _ActualFramerate As Double = 0
            Property ActualFramerate As Double
                Get
                    Return _ActualFramerate
                End Get
                Set(value As Double)
                    _ActualFramerate = value
                    OnPropertyChanged()
                End Set
            End Property

            Private _Height As Double
            Property Height As Double
                Get
                    Return _Height
                End Get
                Set(value As Double)
                    _Height = value
                    OnPropertyChanged()
                    OnSizeChanged()
                End Set
            End Property

            Private _Width As Double
            Property Width As Double
                Get
                    Return _Width
                End Get
                Set(value As Double)
                    _Width = value
                    OnPropertyChanged()
                    OnSizeChanged()
                End Set
            End Property

            Private _StreamOldValue As Integer = -1
            Private _Stream As Integer = -1
            Property Stream As Integer
                Get
                    Return _Stream
                End Get
                Set(value As Integer)
                    _StreamOldValue = _Stream
                    _Stream = value
                    OnPropertyChanged()
                    OnStreamChanged(_StreamOldValue)
                End Set
            End Property

            Private _Metadata As Metadata
            ''' <summary>
            ''' The same reference to <see cref="Player.StreamMetadata"/>, modifying this will change it across the app
            ''' </summary>
            ''' <returns></returns>
            Property Metadata As Metadata
                Get
                    Return _Metadata
                End Get
                Set(value As Metadata)
                    _Metadata = value
                    OnPropertyChanged()
                    OnMetadataChanged()
                End Set
            End Property

            Private _Output As Image
            Property Output As Image
                Get
                    Return _Output
                End Get
                Set(value As Image)
                    _Output = value
                    OnPropertyChanged()
                End Set
            End Property

            Private _CurrentFrame As ImageSource
            Property CurrentFrame As ImageSource
                Get
                    Return _CurrentFrame
                End Get
                Set(value As ImageSource)
                    _CurrentFrame = value
                    OnPropertyChanged()
                End Set
            End Property

            Private _Effect As Effects.Effect
            Property Effect As Effects.Effect
                Get
                    Return _Effect
                End Get
                Set(value As Effects.Effect)
                    _Effect = value
                    OnPropertyChanged()
                End Set
            End Property

            Private _IsEnabled As Boolean
            Property IsEnabled As Boolean
                Get
                    Return _IsEnabled
                End Get
                Set(value As Boolean)
                    _IsEnabled = value
                    _InternalFPSClock.Enabled = value
                    If value Then
                        RemoveHandler System.Windows.Media.CompositionTarget.Rendering, _CompositionTarget_Rendering_Handler
                        AddHandler System.Windows.Media.CompositionTarget.Rendering, _CompositionTarget_Rendering_Handler
                    Else
                        RemoveHandler System.Windows.Media.CompositionTarget.Rendering, _CompositionTarget_Rendering_Handler
                    End If
                    OnPropertyChanged()
                End Set
            End Property

            Public Property Configuration As New StartupItemConfiguration("Generic Video Effect") Implements IStartupItem.Configuration

            Public Property SetOutputCommand As New WPF.Commands.SetOutputCommand(Me)
            Public Property SetWidthCommand As New WPF.Commands.SetWidthCommand(Me)
            Public Property SetHeightCommand As New WPF.Commands.SetHeightCommand(Me)
            Public Property SetResolutionCommand As New WPF.Commands.SetResolutionCommand(Me)

            Private WithEvents _InternalFPSClock As New Forms.Timer With {.Interval = 1000}
            Private _CompositionTarget_Rendering_Handler As New EventHandler(Sub(sender, e)
                                                                                 Dim ig = Render()
                                                                                 If Not UsesCustomControl Then Update(ig)
                                                                                 _ActualFramerate += 1
                                                                             End Sub)

            Public Sub Start()
                OnStartRequested()
                IsEnabled = True
                _InternalFPSClock.Start()
            End Sub
            Public Sub [Stop]()
                _InternalFPSClock.Stop()
                IsEnabled = False
                OnStopRequested()
            End Sub
            Public Sub Update(frame As ImageSource, Optional ActualFramerateHandled As Boolean = False)
                If Not ActualFramerateHandled Then _ActualFramerate += 1
                frame.Freeze()
                CurrentFrame = frame
                If Output IsNot Nothing Then Output.Source = frame
                RaiseEvent OnUpdateRequested(frame)
            End Sub

            MustOverride Sub OnStartRequested()
            MustOverride Sub OnStopRequested()
            MustOverride Sub OnStreamChanged(oldValue As Integer)
            MustOverride Sub OnMetadataChanged()
            MustOverride Sub OnSizeChanged()
            MustOverride Sub OnPlayerPaused()
            MustOverride Sub OnPlayerResumed()
            ''' <summary>
            ''' Occures when the video effect is clicked on.
            ''' </summary>
            ''' <param name="position">Mouse position relative to the control</param>
            MustOverride Sub OnMouseClick(position As Point)
            MustOverride Function Render() As ImageSource
            Public MustOverride Sub Init() Implements IStartupItem.Init

            Public Overridable Function GetCustomControl() As UIElement
                Return Nothing
            End Function

            Private Sub _InternalFPSClock_Tick(sender As Object, e As EventArgs) Handles _InternalFPSClock.Tick
                OnPropertyChanged(NameOf(ActualFramerate))
                _ActualFramerate = 0
            End Sub
        End Class

        Public Class WMPVideoEffect
            Inherits VideoEffect

            Private _Hwnd As IntPtr
            ReadOnly Property Hwnd As IntPtr
                Get
                    Return _Hwnd
                End Get
            End Property

            Private _IsSFXLoaded As Boolean = False
            Private _hSFX3 As Integer

            Private _PicHost As New Forms.Integration.WindowsFormsHost With {.Child = New Forms.PictureBox}
            Private _PicBOX As New Forms.PictureBox

            Sub New()
                Me.Name = "Windows Media Player"
                UsesCustomControl = True
            End Sub

            Public Overrides Function GetCustomControl() As UIElement
                Return _PicHost
            End Function

            Public Overrides Sub OnStreamChanged(oldValue As Integer)
                BassSfx.BASS_SFX_PluginSetStream(_hSFX3, Stream)
            End Sub

            Public Overrides Sub OnSizeChanged()
                BassSfx.BASS_SFX_PluginResize(_hSFX3, Width, Height)
            End Sub

            Public Overrides Sub OnMouseClick(position As Point)
                BassSfx.BASS_SFX_PluginClicked(_hSFX3, position.X, position.Y)
            End Sub

            Public Overrides Sub OnMetadataChanged()
            End Sub

            Public Overrides Sub OnStartRequested()
                If Utilities.SharedProperties.Instance?.Player?.ModuleStatus("BASS SFX") Then
                    Configuration.SetStatus("All Good", 100)
                    _hSFX3 = BassSfx.BASS_SFX_PluginCreate("0AA02E8D-F851-4CB0-9F64-BBA9BE7A983D", _PicHost.Child.Handle, Width, Height, BASSSFXFlag.BASS_SFX_DEFAULT)
                    If _hSFX3 = -1 Then
                        Configuration.SetError(True, New InvalidOperationException("Couldn't load the visualizer"))
                    Else
                        MyBase.Name = BassSfx.BASS_SFX_PluginGetType(_hSFX3).ToString.Replace("BASS_SFX_", "") & "/" & BassSfx.BASS_SFX_PluginGetName(_hSFX3)
                        BassSfx.BASS_SFX_PluginSetStream(_hSFX3, If(Stream = -1, Utilities.SharedProperties.Instance.Player?.Stream, Stream))
                        BassSfx.BASS_SFX_PluginStart(_hSFX3)
                        Configuration.SetError(False, Nothing)
                        Configuration.SetStatus("Loaded visualizer " & BassSfx.BASS_SFX_PluginGetName(_hSFX3), 100)
                    End If
                Else
                    HandyControl.Controls.MessageBox.Error("Please enable BASS SFX module before starting the plugin.")
                End If
            End Sub

            Public Overrides Sub OnStopRequested()
                BassSfx.BASS_SFX_PluginStop(_hSFX3)
                BassSfx.BASS_SFX_PluginFree(_hSFX3)
                _ExCount = 0
            End Sub

            Public Overrides Sub OnPlayerPaused()
            End Sub

            Public Overrides Sub OnPlayerResumed()
            End Sub

            Private _ExCount As Integer = 0

            Public Overrides Function Render() As ImageSource
                If (_hSFX3 <> -1) Then
                    Try
                        BassSfx.BASS_SFX_PluginRender(_hSFX3, Stream, _PicHost.Child.CreateGraphics.GetHdc)
                    Catch ex As Exception
                        _ExCount += 1
                        If _ExCount > 10 Then
                            Configuration.SetError(True, New Exception("Reached exception limit", ex))
                            Exit Try
                        End If
                    End Try
                    If _ExCount > 10 Then
                        [Stop]()
                    End If
                End If
                Return Nothing
            End Function

            Public Overrides Sub Init()
                'Dim helper As New Interop.WindowInteropHelper(Application.Current.MainWindow)
                '_Hwnd = helper.EnsureHandle
                'Configuration.IsErrored = False
                'Configuration.SetStatus("Hooked to " & Application.Current.MainWindow.Title, 100)
                'If BassSfx.BASS_SFX_Init(System.Diagnostics.Process.GetCurrentProcess().Handle, _Hwnd) Then
                '    _IsSFXLoaded = True
                '    Configuration.SetStatus("All Good, " & Configuration.Status, 100)
                'Else
                '    Dim err = BassSfx.BASS_SFX_ErrorGetCode
                '    If err = BASSSFXError.BASS_SFX_ERROR_ALREADY Then _IsSFXLoaded = True : Return
                '    _IsSFXLoaded = False
                '    Configuration.SetError(True, New InvalidOperationException("Couldn't find a valid window"))
                'End If
                If Utilities.SharedProperties.Instance.Player?.ModuleStatus("BASS SFX") Then
                    Configuration.SetStatus("All Good, " & Configuration.Status, 100)
                Else
                    Configuration.SetStatus("BASS SFX module is not enabled.", 0)
                    Configuration.SetError(True, New InvalidOperationException("Couldn't find a valid engine"))
                End If
            End Sub
        End Class

        Public Class PluginVideoEffect
            Inherits VideoEffect

            Private _Hwnd As IntPtr
            ReadOnly Property Hwnd As IntPtr
                Get
                    Return _Hwnd
                End Get
            End Property

            Private _IsSFXLoaded As Boolean = False
            Private _hSFX3 As Integer

            Private _PicHost As New Forms.Integration.WindowsFormsHost With {.Child = New Forms.PictureBox}
            Private WithEvents _PicBOX As New Forms.PictureBox
            Private _HoldRender As Boolean
            Private _file As String

            Sub New()
                Me.Name = "External Plugin"
                UsesCustomControl = True
            End Sub

            Public Overrides Function GetCustomControl() As UIElement
                Return _PicHost
            End Function

            Public Overrides Sub OnStreamChanged(oldValue As Integer)
                BassSfx.BASS_SFX_PluginSetStream(_hSFX3, Stream)
            End Sub

            Public Overrides Sub OnSizeChanged()
                _HoldRender = True
                If BassSfx.BASS_SFX_PluginGetType(_hSFX3) = BASSSFXPlugin.BASS_SFX_SONIQUE Then
                    [Stop]()
                    Start()
                Else
                    BassSfx.BASS_SFX_PluginResize(_hSFX3, Width, Height)
                End If
                _HoldRender = False
            End Sub

            Public Overrides Sub OnMouseClick(position As Point)
                Dim v = BassSfx.BASS_SFX_PluginClicked(_hSFX3, position.X, position.Y)
            End Sub
            Public Overrides Sub OnMetadataChanged()
            End Sub

            Public Overrides Sub OnStartRequested()
                If Utilities.SharedProperties.Instance?.Player?.ModuleStatus("BASS SFX") Then
                    Configuration.SetStatus("All Good", 100)
                    If _file = "" Then
                        Dim ofd As New Microsoft.Win32.OpenFileDialog
                        If Not ofd.ShowDialog Then
                            Configuration.SetStatus("No file provided.", 0)
                            Configuration.SetError(True, New ArgumentNullException("file", "Please provie a file path."))
                            Return
                        End If
                        _file = ofd.FileName
                    End If
                    _hSFX3 = BassSfx.BASS_SFX_PluginCreate(_file, _PicHost.Child.Handle, Width, Height, BASSSFXFlag.BASS_SFX_DEFAULT)
                    If _hSFX3 = -1 Then
                        Configuration.SetError(True, New InvalidOperationException("Couldn't load the visualizer"))
                    Else
                        MyBase.Name = BassSfx.BASS_SFX_PluginGetType(_hSFX3).ToString.Replace("BASS_SFX_", "") & "/" & BassSfx.BASS_SFX_PluginGetName(_hSFX3)
                        BassSfx.BASS_SFX_PluginSetStream(_hSFX3, If(Stream = -1, Utilities.SharedProperties.Instance.Player?.Stream, Stream))
                        BassSfx.BASS_SFX_PluginStart(_hSFX3)
                        Configuration.SetError(False, Nothing)
                        Configuration.SetStatus("Loaded visualizer " & BassSfx.BASS_SFX_PluginGetName(_hSFX3), 100)
                    End If
                Else
                    HandyControl.Controls.MessageBox.Error("Please enable BASS SFX module before starting the plugin.")
                End If
            End Sub

            Public Overrides Sub OnStopRequested()
                BassSfx.BASS_SFX_PluginStop(_hSFX3)
                BassSfx.BASS_SFX_PluginFree(_hSFX3)
                _ExCount = 0
            End Sub
            Public Overrides Sub OnPlayerPaused()
            End Sub

            Public Overrides Sub OnPlayerResumed()
            End Sub

            Private _ExCount As Integer = 0

            Public Overrides Function Render() As ImageSource
                If _HoldRender Then Return Nothing
                If (_hSFX3 <> -1) Then
                    Try
                        BassSfx.BASS_SFX_PluginRender(_hSFX3, Stream, _PicHost.Child.CreateGraphics.GetHdc)
                    Catch ex As Exception
                        _ExCount += 1
                        If _ExCount > 10 Then
                            Configuration.SetError(True, New Exception("Reached exception limit", ex))
                            Exit Try
                        End If
                    End Try
                    If _ExCount > 10 Then
                        [Stop]()
                    End If
                End If
                Return Nothing
            End Function

            Public Overrides Sub Init()
                'Dim helper As New Interop.WindowInteropHelper(Application.Current.MainWindow)
                '_Hwnd = helper.EnsureHandle
                'Configuration.IsErrored = False
                'Configuration.SetStatus("Hooked to " & Application.Current.MainWindow.Title, 100)
                'If BassSfx.BASS_SFX_Init(System.Diagnostics.Process.GetCurrentProcess().Handle, _Hwnd) Then
                '    _IsSFXLoaded = True
                '    Configuration.SetStatus("All Good, " & Configuration.Status, 100)
                'Else
                '    Dim err = BassSfx.BASS_SFX_ErrorGetCode
                '    If err = BASSSFXError.BASS_SFX_ERROR_ALREADY Then _IsSFXLoaded = True : Return
                '    _IsSFXLoaded = False
                '    Configuration.SetError(True, New InvalidOperationException("Couldn't find a valid window"))
                'End If
                If Utilities.SharedProperties.Instance.Player?.ModuleStatus("BASS SFX") Then
                    Configuration.SetStatus("All Good, " & Configuration.Status, 100)
                Else
                    Configuration.SetStatus("BASS SFX module is not enabled.", 0)
                    Configuration.SetError(True, New InvalidOperationException("Couldn't find a valid engine"))
                End If
            End Sub

            Private Sub _PicBOX_MouseUp(sender As Object, e As Forms.MouseEventArgs) Handles _PicBOX.MouseUp
                BassSfx.BASS_SFX_PluginClicked(_hSFX3, e.Location.X, e.Location.Y)
            End Sub
        End Class

        Public Class BarsVideoEffect
            Inherits VideoEffect

            Private wBG As Brush = Nothing
            Private wFG As Brush = Nothing

            Sub New()
                MyBase.Name = "Bars"
            End Sub

            Public Overrides Sub OnStartRequested()
                If Metadata Is Nothing Then
                    wBG = Brushes.Transparent
                    wFG = New SolidColorBrush(Color.FromRgb(255, 193, 7))
                Else
                    OnMetadataChanged()
                End If
            End Sub

            Public Overrides Sub OnStopRequested()
                'Throw New NotImplementedException()
            End Sub
            Public Overrides Sub OnMouseClick(position As Point)

            End Sub
            Public Overrides Sub OnPlayerPaused()
            End Sub

            Public Overrides Sub OnPlayerResumed()
            End Sub
            Public Overrides Sub OnMetadataChanged()
                If Metadata.DefaultCover Is Nothing Then
                    wBG = Brushes.Transparent
                    wFG = New SolidColorBrush(Color.FromRgb(255, 193, 7))
                End If
                Dim Clr = Utilities.CommonFunctions.GetAverageColor(Metadata.DefaultCover)
                Dim Luma = Utilities.CommonFunctions.Luminance(Clr)
                If Luma < 40 Then
                    wBG = Brushes.White
                Else
                    wBG = Brushes.Black
                End If
                wFG = New SolidColorBrush(Clr)
            End Sub

            Public Overrides Function Render() As ImageSource
                Dim data = Utilities.SharedProperties.Instance.Player.GetChannelPeakData2(New Single() {10, 206, 413, 620, 827, 1034, 1241, 1448, 1655, 1862, 2068, 2275, 2482, 2689, 2896, 3103, 3310, 3517, 3724, 3931, 4137, 4344, 4551, 4758, 4965, 5172, 5379, 5586, 5793, 6000, 6206, 6413, 6620, 6827, 7034, 7241, 7448, 7655, 7862, 8068, 8275, 8482, 8689, 8896, 9103, 9310, 9517, 9724, 9931, 10137, 10344, 10551, 10758, 10965, 11172, 11379, 11586, 11793, 12000}, 2048, Un4seen.Bass.BASSData.BASS_DATA_FFT4096, 100)
                Dim x As New DrawingVisual
                Dim dc = x.RenderOpen
                Dim i = data.Length - 1
                dc.PushTransform(New RotateTransform With {.Angle = 180})
                data = Utilities.CommonFunctions.CalcMovAvg(data, 1)
                'dc.DrawRectangle(Brushes.Transparent, New Pen(Brushes.Green, 10), New Rect(0, 0, 1, 100))                
                dc.DrawRectangle(wBG, Nothing, New Rect(0, 0, (i + 1) * 10 + 5 * (i), 160))
                For Each peak In data
                    'Dim x_db = If(peak * 100 < 1, 0, 10 * Math.Log10(Math.Abs(peak * 100) ^ 2))
                    'dc.DrawRectangle(Brushes.Red, Nothing, New Rect(10 * i + (5 * i), 0, 10, x_db)) 'peak * 1280))                    
                    dc.DrawRectangle(Brushes.Transparent, Nothing, New Rect(10 * i + (5 * i), 0, 10, 100))
                    Dim x_db = If(peak < 1, 0, 10 * Math.Log10(Math.Abs(peak) ^ 2))
                    dc.DrawRectangle(wFG, Nothing, New Rect(10 * i + (5 * i), 0, 10, peak))
                    i -= 1
                Next
                dc.Close()
                Dim v As New DrawingImage(x.Drawing)
                Return v
                'Update(Utilities.CommonFunctions.ToImageSource(_Visuals.CreateSpectrumLinePeak(Utilities.SharedProperties.Instance.Player.Stream, Width, Height, System.Drawing.Color.Red, System.Drawing.Color.Red, System.Drawing.Color.Black, System.Drawing.Color.Empty, Width / 160, 3, 2, 250, False, False, False)))
            End Function

            Public Overrides Sub Init()
                'Throw New NotImplementedException()
            End Sub

            Public Overrides Sub OnStreamChanged(oldValue As Integer)
                'Throw New NotImplementedException()
            End Sub

            Public Overrides Sub OnSizeChanged()
                'Throw New NotImplementedException()
            End Sub
        End Class

        Public Class SpectrumVideoEffect
            Inherits VideoEffect

            Private wPen As Pen = Nothing
            Private wBrush As Brush = Nothing

            Sub New()
                MyBase.Name = "Spectrum"
            End Sub

            Public Overrides Sub OnStartRequested()
                If Metadata Is Nothing Then
                    wPen = New Pen(Brushes.Green, 1) With {.DashCap = PenLineCap.Round, .EndLineCap = PenLineCap.Round, .LineJoin = PenLineJoin.Miter, .StartLineCap = PenLineCap.Round}
                    wPen.Freeze()
                    wBrush = Brushes.Transparent
                Else
                    OnMetadataChanged()
                End If
            End Sub

            Public Overrides Sub OnStopRequested()
                'Throw New NotImplementedException()                
            End Sub

            Public Overrides Sub OnMouseClick(position As Point)
            End Sub
            Public Overrides Sub OnPlayerPaused()
            End Sub

            Public Overrides Sub OnPlayerResumed()
            End Sub

            Public Overrides Sub OnMetadataChanged()
                If Metadata.DefaultCover Is Nothing Then
                    wPen = New Pen(Brushes.Green, 1) With {.DashCap = PenLineCap.Round, .EndLineCap = PenLineCap.Round, .LineJoin = PenLineJoin.Miter, .StartLineCap = PenLineCap.Round}
                    wPen.Freeze()
                    wBrush = Brushes.Transparent
                End If
                Dim Clr = Utilities.CommonFunctions.GetAverageColor(Metadata.DefaultCover)
                Dim Luma = Utilities.CommonFunctions.Luminance(Clr)
                If Luma < 40 Then
                    wBrush = Brushes.White
                Else
                    wBrush = Brushes.Black
                End If
                wPen = New Pen(New SolidColorBrush(Clr), 1) With {.DashCap = PenLineCap.Round, .EndLineCap = PenLineCap.Round, .LineJoin = PenLineJoin.Miter, .StartLineCap = PenLineCap.Round}
            End Sub


            Public Overrides Function Render() As ImageSource
                Dim data = Utilities.SharedProperties.Instance.Player.GetChannelPeakData2(New Single() {10, 206, 413, 620, 827, 1034, 1241, 1448, 1655, 1862, 2068, 2275, 2482, 2689, 2896, 3103, 3310, 3517, 3724, 3931, 4137, 4344, 4551, 4758, 4965, 5172, 5379, 5586, 5793, 6000, 6206, 6413, 6620, 6827, 7034, 7241, 7448, 7655, 7862, 8068, 8275, 8482, 8689, 8896, 9103, 9310, 9517, 9724, 9931, 10137, 10344, 10551, 10758, 10965, 11172, 11379, 11586, 11793, 12000}, 4096, Un4seen.Bass.BASSData.BASS_DATA_FFT8192, 100)
                Dim x As New DrawingVisual
                Dim dc = x.RenderOpen
                Dim i = data.Length - 1
                dc.PushTransform(New RotateTransform With {.Angle = 180})
                Dim _Peak As Single = 0
                data = Utilities.CommonFunctions.CalcMovAvg(data, 1)
                'dc.DrawRectangle(Brushes.Transparent, New Pen(Brushes.Green, 10), New Rect(0, 0, 1, 100))                
                dc.DrawRectangle(wBrush, Nothing, New Rect(0, 0, (i + 2) * 10 + 5 * (i + 2), 110))
                For Each peak In data
                    'Dim x_db = If(peak * 100 < 1, 0, 10 * Math.Log10(Math.Abs(peak * 100) ^ 2))
                    'dc.DrawRectangle(Brushes.Red, Nothing, New Rect(10 * i + (5 * i), 0, 10, x_db)) 'peak * 1280))                    
                    dc.DrawRectangle(Brushes.Transparent, Nothing, New Rect(10 * i + (5 * i), 0, 10, 100))
                    'Dim x_db = If(peak < 1, 0, 10 * Math.Log10(Math.Abs(peak) ^ 2))
                    dc.DrawLine(wPen, New Point(10 * (i + 1) + (5 * (i + 1) + 5), _Peak), New Point(10 * i + (5 * i) + 5, peak))
                    _Peak = peak
                    i -= 1
                Next

                dc.Close()
                Dim v As New DrawingImage(x.Drawing)
                Return v
                'Update(Utilities.CommonFunctions.ToImageSource(_Visuals.CreateSpectrumLinePeak(Utilities.SharedProperties.Instance.Player.Stream, Width, Height, System.Drawing.Color.Red, System.Drawing.Color.Red, System.Drawing.Color.Black, System.Drawing.Color.Empty, Width / 160, 3, 2, 250, False, False, False)))
            End Function

            Public Overrides Sub Init()
                'Throw New NotImplementedException()
            End Sub

            Public Overrides Sub OnStreamChanged(oldValue As Integer)
                'Throw New NotImplementedException()
            End Sub

            Public Overrides Sub OnSizeChanged()
                'Throw New NotImplementedException()
            End Sub
        End Class

        Public Class BarsPeakVideoEffect
            Inherits VideoEffect

            Private Bg As Color = Nothing
            Private Fg As Color = Nothing


            Public Overrides Sub OnStartRequested()
                _Visuals.ScaleFactorSqr = 2
                _Visuals.ScaleFactorSqrBoost = 0.1
                If Metadata Is Nothing Then
                    Bg = Colors.Transparent
                    Fg = Color.FromRgb(255, 193, 7)
                Else
                    OnMetadataChanged()
                End If
            End Sub

            Public Overrides Sub OnStopRequested()

            End Sub

            Public Overrides Sub OnStreamChanged(oldValue As Integer)

            End Sub

            Public Overrides Sub OnSizeChanged()

            End Sub

            Public Overrides Sub OnMouseClick(position As Point)

            End Sub
            Public Overrides Sub OnPlayerPaused()
            End Sub

            Public Overrides Sub OnPlayerResumed()
            End Sub

            Public Overrides Sub OnMetadataChanged()
                If Metadata.DefaultCover Is Nothing Then
                    Bg = Colors.Transparent
                    Fg = Color.FromRgb(255, 193, 7)
                End If
                Dim Clr = Utilities.CommonFunctions.GetAverageColor(Metadata.DefaultCover)
                Dim Luma = Utilities.CommonFunctions.Luminance(Clr)
                If Luma < 40 Then
                    Bg = Colors.White
                Else
                    Bg = Colors.Black
                End If
                Fg = Clr
            End Sub

            Sub New()
                MyBase.New
                Name = "Bars With Peak"
            End Sub

            Public Overrides Sub Init()
                Me.Configuration.IsLoaded = True
            End Sub

            Private _Visuals As New Un4seen.Bass.Misc.Visuals With {.ScaleFactorSqr = 1, .ScaleFactorSqrBoost = 0.005} 'Low,High
            Public Overrides Function Render() As ImageSource
                Return Utilities.ToImageSource(_Visuals.CreateSpectrumLinePeak(Stream, Width, Height, Utilities.CommonFunctions.ToGDIColor(Fg), Utilities.CommonFunctions.ToGDIColor(Fg), Utilities.CommonFunctions.ToGDIColor(Fg), Utilities.CommonFunctions.ToGDIColor(Bg), Width / 128, 5, 5, 250, False, False, False))
            End Function
        End Class

        Public Class VideoPlayerVideoEffect
            Inherits VideoEffect

            Public Overrides Sub OnStartRequested()
                If Stream <> 0 Then
                    Dim info = Bass.BASS_ChannelGetInfo(Stream)
                    If info.ctype = BASSChannelType.BASS_CTYPE_STREAM_VIDEO OrElse info.ctype = BASSChannelType.BASS_CTYPE_STREAM_MP4 Then
                        If Not IO.File.Exists(Utilities.SharedProperties.Instance.Player.Path) Then
                            Application.Current.Dispatcher.Invoke(Sub()
                                                                      _MediaElement.Visibility = Visibility.Collapsed
                                                                      _TextBlock_Error.Visibility = Visibility.Visible
                                                                      _TextBlock_Error.Text = "Remote files are not supported."
                                                                  End Sub)
                            Return
                        End If
                        _MediaElement.Source = New Uri(Utilities.SharedProperties.Instance.Player.Path)
                        _MediaElement.Position = TimeSpan.FromSeconds(Utilities.SharedProperties.Instance.Player.Position)
                        _MediaElement.Volume = Utilities.SharedProperties.Instance.Player.Volume
                        _MediaElement.Play()
                        Application.Current.Dispatcher.Invoke(Sub()
                                                                  _MediaElement.Visibility = Visibility.Visible
                                                                  _TextBlock_Error.Visibility = Visibility.Collapsed
                                                              End Sub)
                    Else
                        Application.Current.Dispatcher.Invoke(Sub()
                                                                  _MediaElement.Visibility = Visibility.Collapsed
                                                                  _TextBlock_Error.Visibility = Visibility.Visible
                                                                  _TextBlock_Error.Text = "Not a video file."
                                                              End Sub)
                    End If
                End If
            End Sub

            Public Overrides Sub OnStopRequested()
                _MediaElement.Stop()
                _MediaElement.Close()
            End Sub

            Public Overrides Sub OnStreamChanged(oldValue As Integer)
                If oldValue <> -1 AndAlso _SetPosSYNCPROCHandle <> 0 Then
                    Bass.BASS_ChannelRemoveSync(oldValue, _SetPosSYNCPROCHandle)
                End If
                If Not IO.File.Exists(Utilities.SharedProperties.Instance.Player.Path) Then
                    Application.Current.Dispatcher.Invoke(Sub()
                                                              _MediaElement.Visibility = Visibility.Collapsed
                                                              _TextBlock_Error.Visibility = Visibility.Visible
                                                              _TextBlock_Error.Text = "Remote files are not supported."
                                                          End Sub)
                    Return
                End If
                _SetPosSYNCPROCHandle = Bass.BASS_ChannelSetSync(Stream, BASSSync.BASS_SYNC_SETPOS, 0, _SetPosSYNCPROC, IntPtr.Zero)
                If Stream <> 0 Then
                    _MediaElement.Close()
                    '_MediaElement.Source = New Uri("\Resources\MusicRecord.png", UriKind.Relative)
                    Dim info = Bass.BASS_ChannelGetInfo(Stream)
                    If info.ctype = BASSChannelType.BASS_CTYPE_STREAM_VIDEO OrElse info.ctype = BASSChannelType.BASS_CTYPE_STREAM_MP4 OrElse info.ctype = BASSChannelType.BASS_CTYPE_STREAM_MF Then
                        _MediaElement.Source = New Uri(Utilities.SharedProperties.Instance.Player.Path)
                        _MediaElement.Position = TimeSpan.FromSeconds(Utilities.SharedProperties.Instance.Player.Position)
                        _MediaElement.Volume = 0
                        _MediaElement.Play()
                        Application.Current.Dispatcher.Invoke(Sub()
                                                                  _MediaElement.Visibility = Visibility.Visible
                                                                  _TextBlock_Error.Visibility = Visibility.Collapsed
                                                              End Sub)
                    Else
                        Application.Current.Dispatcher.Invoke(Sub()
                                                                  _MediaElement.Visibility = Visibility.Collapsed
                                                                  _TextBlock_Error.Visibility = Visibility.Visible
                                                                  _TextBlock_Error.Text = "Not a video file."
                                                              End Sub)
                    End If
                End If
            End Sub

            Public Overrides Sub OnSizeChanged()

            End Sub

            Public Overrides Sub OnMouseClick(position As Point)

            End Sub
            Public Overrides Sub OnPlayerPaused()
                _MediaElement.Pause()
            End Sub

            Public Overrides Sub OnPlayerResumed()
                _MediaElement.Play()
            End Sub

            Public Overrides Sub OnMetadataChanged()
            End Sub

            Sub New()
                MyBase.New
                Name = "Video Player"
                UsesCustomControl = True
                _Grid_Host.Children.Add(_MediaElement)
                _Grid_Host.Children.Add(_TextBlock_Error)
            End Sub

            Private _Grid_Host As New Grid
            Private _TextBlock_Error As New TextBlock With {.HorizontalAlignment = HorizontalAlignment.Center, .VerticalAlignment = VerticalAlignment.Center, .FontSize = 16, .Visibility = Visibility.Collapsed}
            Private _MediaElement As New MediaElement With {.LoadedBehavior = MediaState.Manual, .UnloadedBehavior = MediaState.Close, .Stretch = Stretch.Fill}
            Private _SetPosSYNCPROCHandle As Integer = 0
            Private _SetPosSYNCPROC As New SYNCPROC(Sub(h, channel, data, user)
                                                        Application.Current.Dispatcher.Invoke(Sub()
                                                                                                  _MediaElement.Position = TimeSpan.FromSeconds(Utilities.SharedProperties.Instance.Player.Position)
                                                                                              End Sub)
                                                    End Sub)

            Public Overrides Function GetCustomControl() As UIElement
                Return _Grid_Host
            End Function

            Public Overrides Sub Init()

            End Sub

            Public Overrides Function Render() As ImageSource
                Return Nothing
            End Function
        End Class

    End Class
End Namespace