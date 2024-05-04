Imports System.ComponentModel
Imports System.Timers
Imports QuickBeat.Classes
Imports QuickBeat.Interfaces
Imports Un4seen.Bass.AddOn.Sfx
Imports Un4seen.Bass
Imports QuickBeat.Utilities
Imports Un4seen.Bass.Misc

Namespace Player.VFX
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

            Private _FramerateLimit As Integer = 0
            Overridable Property FramerateLimit As Integer
                Get
                    Return _FramerateLimit
                End Get
                Set(value As Integer)
                    _FramerateLimit = value
                    If value > 0 Then
                        _InternalFPSLimitDistributor.Interval = 1000 / value
                        _InternalFPSLimitDistributor.Start()
                    Else
                        _InternalFPSLimitDistributor.Stop()
                    End If
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

            Private _IsStoppedByPowerSaving As Boolean = False
            Protected ReadOnly Property IsStoppedByPowerSaving As Boolean
                Get
                    Return _IsStoppedByPowerSaving
                End Get
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
                    _IsStoppedByPowerSaving = False
                    OnPropertyChanged()
                End Set
            End Property

            Private _IsProxied As Boolean
            Property IsProxied As Boolean
                Get
                    Return _IsProxied
                End Get
                Set(value As Boolean)
                    _IsProxied = value
                    If value Then
                        SharedProperties.Instance.Player?.SetProxyVideoEffect(Me)
                    Else
                        SharedProperties.Instance.Player?.DisableProxyVideoEffect(Me)
                    End If
                    OnPropertyChanged()
                End Set
            End Property

            Public Overridable Property Configuration As New StartupItemConfiguration("Generic Video Effect") Implements IStartupItem.Configuration

            Public Property SetOutputCommand As New WPF.Commands.SetOutputCommand(Me)
            Public Property SetWidthCommand As New WPF.Commands.SetWidthCommand(Me)
            Public Property SetHeightCommand As New WPF.Commands.SetHeightCommand(Me)
            Public Property SetResolutionCommand As New WPF.Commands.SetResolutionCommand(Me)
            Public Property SetFramerateLimitCommand As New WPF.Commands.SetFramerateLimitCommand(Me)

            WithEvents _InternalFPSClock As New Forms.Timer With {.Interval = 1000}
            WithEvents _InternalFPSLimitDistributor As New Forms.Timer
            Private _InternalFPSLimit_DoRender As Boolean = False
            Private _CompositionTarget_Rendering_Handler As New EventHandler(Sub(sender, e)
                                                                                 If FramerateLimit > 0 Then 'We have a framelimit
                                                                                     If Not _InternalFPSLimit_DoRender Then Return
                                                                                     _InternalFPSLimit_DoRender = False
                                                                                     PreUpdate()
                                                                                 Else
                                                                                     PreUpdate()
                                                                                 End If
                                                                             End Sub)

            Public Sub Start()
                If IsEnabled Then Return
                OnStartRequested()
                IsEnabled = True
                _InternalFPSClock.Start()
            End Sub
            Public Sub [Stop]()
                _InternalFPSClock.Stop()
                IsEnabled = False
                OnStopRequested()
            End Sub
            Private Sub PreUpdate()
                Dim ig = Render()
                If UsesCustomControl Then
                    _ActualFramerate += 1
                Else
                    Update(ig)
                End If
            End Sub
            Public Sub Update(frame As ImageSource, Optional ActualFramerateHandled As Boolean = False)
                If Not ActualFramerateHandled Then _ActualFramerate += 1
                frame?.Freeze()
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

            Public Overridable Sub OnThemeChanged()

            End Sub

            Public Sub OnPowerSavingResume()
                If IsEnabled = False AndAlso _IsStoppedByPowerSaving Then
                    IsEnabled = True
                End If
            End Sub
            Public Sub OnPowerSavingPause()
                If IsEnabled Then
                    IsEnabled = False
                    _IsStoppedByPowerSaving = True
                End If
            End Sub

            Private Sub _InternalFPSClock_Tick(sender As Object, e As EventArgs) Handles _InternalFPSClock.Tick
                OnPropertyChanged(NameOf(ActualFramerate))
                _ActualFramerate = 0
            End Sub

            Private Sub _InternalFPSLimitDistributor_Tick(sender As Object, e As EventArgs) Handles _InternalFPSLimitDistributor.Tick
                If FramerateLimit = 0 Then _InternalFPSLimitDistributor.Stop() : Return
                _InternalFPSLimit_DoRender = True
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

            Private _hSFX3 As Integer

            Private _PicHost As New Forms.Integration.WindowsFormsHost With {.Child = New Forms.PictureBox}

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

        Public Class MCIVideoPlayerVideoEffect
            Inherits VideoEffect

            Private _Hwnd As IntPtr
            ReadOnly Property Hwnd As IntPtr
                Get
                    Return _Hwnd
                End Get
            End Property

            Private _PicHost As New Forms.Integration.WindowsFormsHost With {.Child = New Forms.Panel}
            Private _MCIDevice As New MCI.MP3Player()
            Private _SetPosSYNCPROCHandle As Integer = 0
            Private _SetPosSYNCPROC As New SYNCPROC(Sub(h, channel, data, user)
                                                        _MCIDevice.Position = Utilities.SharedProperties.Instance.Player.Position * 1000
                                                    End Sub)

            Sub New()
                Me.Name = "Media Control Interface"
                UsesCustomControl = True
            End Sub

            Public Overrides Function GetCustomControl() As UIElement
                Return _PicHost
            End Function

            Public Overrides Sub OnStreamChanged(oldValue As Integer)
                _MCIDevice.Close()
                If oldValue <> -1 AndAlso _SetPosSYNCPROCHandle <> 0 Then
                    Bass.BASS_ChannelRemoveSync(oldValue, _SetPosSYNCPROCHandle)
                End If
                If IO.File.Exists(Utilities.SharedProperties.Instance.Player.Path) Then
                    If Stream <> 0 Then
                        Dim info = Bass.BASS_ChannelGetInfo(Stream)
                        If info.ctype = BASSChannelType.BASS_CTYPE_STREAM_VIDEO OrElse info.ctype = BASSChannelType.BASS_CTYPE_STREAM_MP4 OrElse info.ctype = BASSChannelType.BASS_CTYPE_STREAM_MF Then
                            _MCIDevice.Open(Utilities.SharedProperties.Instance.Player.Path, _PicHost.Child.Handle)
                            If _MCIDevice.IsOpened Then _MCIDevice.VolumeAll = 0
                            If _MCIDevice.IsOpened Then _MCIDevice.Position = Utilities.SharedProperties.Instance.Player.Position * 1000
                            If _MCIDevice.IsOpened Then _MCIDevice.Play()
                        End If
                    End If
                End If
                _SetPosSYNCPROCHandle = Bass.BASS_ChannelSetSync(Stream, BASSSync.BASS_SYNC_SETPOS, 0, _SetPosSYNCPROC, IntPtr.Zero)
            End Sub

            Public Overrides Sub OnSizeChanged()
            End Sub

            Public Overrides Sub OnMouseClick(position As Point)
            End Sub

            Public Overrides Sub OnMetadataChanged()
            End Sub

            Public Overrides Sub OnStartRequested()
                If Not IO.File.Exists(Utilities.SharedProperties.Instance.Player.Path) Then
                    If Stream <> 0 Then
                        Dim info = Bass.BASS_ChannelGetInfo(Stream)
                        If info.ctype = BASSChannelType.BASS_CTYPE_STREAM_VIDEO OrElse info.ctype = BASSChannelType.BASS_CTYPE_STREAM_MP4 Then
                            _MCIDevice.Open(Utilities.SharedProperties.Instance.Player.Path, _PicHost.Child.Handle)
                            If _MCIDevice.IsOpened Then _MCIDevice.VolumeAll = 0
                        End If
                    End If
                End If
                _SetPosSYNCPROCHandle = Bass.BASS_ChannelSetSync(Stream, BASSSync.BASS_SYNC_SETPOS, 0, _SetPosSYNCPROC, IntPtr.Zero)
            End Sub

            Public Overrides Sub OnStopRequested()
                If _MCIDevice.IsOpened Then _MCIDevice.Stop()
            End Sub

            Public Overrides Sub OnPlayerPaused()
                If _MCIDevice.IsOpened Then _MCIDevice.Pause()
            End Sub

            Public Overrides Sub OnPlayerResumed()
                If _MCIDevice.IsOpened AndAlso Not _MCIDevice.IsPlaying Then _MCIDevice.Play()
            End Sub


            Public Overrides Function Render() As ImageSource
                Return Nothing
            End Function

            Public Overrides Sub Init()

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

            Private _hSFX3 As Integer
            Private _hImage As IntPtr

            Private _Image As New Image
            Private _PicHost As New Forms.Integration.WindowsFormsHost With {.Child = New Forms.PictureBox}
            Private _HoldRender As Boolean
            Private _file As String

            Sub New()
                Me.Name = "External Plugin"
                UsesCustomControl = True
            End Sub

            Public Overrides Function GetCustomControl() As UIElement
                Return _PicHost
                'Return _Image
            End Function

            Public Overrides Sub OnStreamChanged(oldValue As Integer)
                BassSfx.BASS_SFX_PluginSetStream(_hSFX3, Stream)
            End Sub

            Public Overrides Sub OnSizeChanged()
                If IsEnabled = False Then Return
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
                    'Dim h = New Interop.WindowInteropHelper(Application.Current.MainWindow)
                    'Dim hCtl = TryCast(PresentationSource.FromVisual(_Image), Interop.HwndSource)?.Handle
                    'If hCtl = IntPtr.Zero Then Return
                    '_hImage = hCtl
                    _hSFX3 = BassSfx.BASS_SFX_PluginCreate(_file, _PicHost.Child.Handle, Width, Height, BASSSFXFlag.BASS_SFX_DEFAULT)
                    '_hSFX3 = BassSfx.BASS_SFX_PluginCreate(_file, hCtl, Width, Height, BASSSFXFlag.BASS_SFX_DEFAULT)
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
                        BassSfx.BASS_SFX_PluginRender(_hSFX3, Stream, _PicHost.Child.CreateGraphics.GetHdc) 'System.Drawing.Graphics.FromHwnd(_hImage).GetHdc) '
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
                If Metadata?.HasCover Then
                    Dim Clr = Utilities.CommonFunctions.GetAverageColor(Metadata.DefaultCover)
                    Dim Luma = Utilities.CommonFunctions.Luminance(Clr)
                    If Luma < 40 Then
                        wBG = Brushes.White
                    Else
                        wBG = Brushes.Black
                    End If
                    wFG = New SolidColorBrush(Clr)
                Else
                    wFG = Brushes.Green
                    wBG = Brushes.Transparent
                End If
            End Sub

            Public Overrides Function Render() As ImageSource
                'Dim data = Utilities.SharedProperties.Instance.Player.GetChannelPeakData2(New Single() {10, 206, 413, 620, 827, 1034, 1241, 1448, 1655, 1862, 2068, 2275, 2482, 2689, 2896, 3103, 3310, 3517, 3724, 3931, 4137, 4344, 4551, 4758, 4965, 5172, 5379, 5586, 5793, 6000, 6206, 6413, 6620, 6827, 7034, 7241, 7448, 7655, 7862, 8068, 8275, 8482, 8689, 8896, 9103, 9310, 9517, 9724, 9931, 10137, 10344, 10551, 10758, 10965, 11172, 11379, 11586, 11793, 12000}, 2048, Un4seen.Bass.BASSData.BASS_DATA_FFT4096, 100)
                Dim data = Utilities.SharedProperties.Instance.Player.GetChannelPeakData2(New Single() {10, 206, 413, 620, 827, 1034, 1241, 1448, 1655, 1862, 2068, 2275, 2482, 2689, 2896, 3103, 3310, 3517, 3724, 3931, 4137, 4344, 4551, 4758, 4965, 5172, 5379, 5586, 5793, 6000, 6206, 6413, 6620, 6827, 7034, 7241, 7448, 7655, 7862, 8068, 8275, 8482, 8689, 8896, 9103, 9310, 9517, 9724, 9931, 10137, 10344, 10551, 10758, 10965, 11172, 11379, 11586, 11793, 12000}, 256, Un4seen.Bass.BASSData.BASS_DATA_FFT512, 100)
                Dim x As New DrawingVisual
                Dim dc = x.RenderOpen
                Dim i = data.Count - 1
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
                If Metadata?.HasCover Then
                    Dim Clr = Utilities.CommonFunctions.GetAverageColor(Metadata.DefaultCover)
                    Dim Luma = Utilities.CommonFunctions.Luminance(Clr)
                    If Luma < 40 Then
                        wBrush = Brushes.White
                    Else
                        wBrush = Brushes.Black
                    End If
                    wPen = New Pen(New SolidColorBrush(Clr), 1) With {.DashCap = PenLineCap.Round, .EndLineCap = PenLineCap.Round, .LineJoin = PenLineJoin.Miter, .StartLineCap = PenLineCap.Round}
                Else
                    wPen = New Pen(Brushes.Green, 1) With {.DashCap = PenLineCap.Round, .EndLineCap = PenLineCap.Round, .LineJoin = PenLineJoin.Miter, .StartLineCap = PenLineCap.Round}
                    wPen.Freeze()
                    wBrush = Brushes.Transparent
                End If
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
                If Metadata?.HasCover Then
                    Dim Clr = Utilities.CommonFunctions.GetAverageColor(Metadata.DefaultCover)
                    Dim Luma = Utilities.CommonFunctions.Luminance(Clr)
                    If Luma < 40 Then
                        Bg = Colors.White
                    Else
                        Bg = Colors.Black
                    End If
                    Fg = Clr
                Else
                    Fg = Colors.Green
                    Bg = Colors.Transparent
                End If
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

        Public Class FramerateCalibrationVideoEffect
            Inherits VideoEffect

            Private wPen As Pen = Nothing

            Sub New()
                MyBase.Name = "Framerate Calibration"
            End Sub

            Public Overrides Sub OnStartRequested()
                If Metadata Is Nothing Then
                    wPen = New Pen(Brushes.Green, 1) With {.DashCap = PenLineCap.Round, .EndLineCap = PenLineCap.Round, .LineJoin = PenLineJoin.Miter, .StartLineCap = PenLineCap.Round}
                    wPen.Freeze()
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
                Else
                    Dim Clr = Utilities.CommonFunctions.GetAverageColor(Metadata.DefaultCover)
                    wPen = New Pen(New SolidColorBrush(Clr), 1) With {.DashCap = PenLineCap.Round, .EndLineCap = PenLineCap.Round, .LineJoin = PenLineJoin.Miter, .StartLineCap = PenLineCap.Round}
                End If
            End Sub

            Dim phase = 0
            Public Overrides Function Render() As ImageSource
                Dim x As New DrawingVisual
                Dim dc = x.RenderOpen

                'ref
                'Dim path As New PathFigure()
                'path.StartPoint = New Point(10, 100)
                'path.Segments.Add(New BezierSegment(New Point(200, -200), New Point(200, 400), New Point(400, 100), True))                                
                dc.PushClip(Geometry.Parse("M 0,0 H 400 V 400 H 0 Z"))
                dc.DrawRectangle(Nothing, Nothing, New Rect(0, 0, 400, 400))
                phase += 1
                If phase >= 0 Then phase = -395
#Disable Warning
                dc.DrawText(New FormattedText(phase, Globalization.CultureInfo.CurrentCulture, FlowDirection.LeftToRight, New Typeface(New FontFamily("Arial"), FontStyles.Normal, FontWeights.DemiBold, FontStretches.Normal), 132 / 2, New SolidColorBrush(Color.FromRgb(255, 193, 7))), New Point(0, 0))
#Enable Warning
                Dim path As New PathFigure With {
                    .StartPoint = New Point(10 + phase, 100)
                }
                path.Segments.Add(New BezierSegment(New Point(200 + phase, -200), New Point(200 + phase, 400), New Point(400 + phase, 100), True))
                Dim cpath As New PathFigure With {
                    .StartPoint = New Point(400 + phase, 100)
                }
                cpath.Segments.Add(New BezierSegment(New Point(600 + phase, -200), New Point(600 + phase, 400), New Point(800 + phase, 100), True))
                Dim gpath As New PathGeometry({path, cpath})
                dc.DrawGeometry(Nothing, wPen, gpath)
                dc.Close()
                Dim v As New DrawingImage(x.Drawing)
                Return v
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
    End Class
    Public Class ReactiveWaveVideoEffect
        Inherits VFX.VideoEffects.VideoEffect

        Private wPen As Pen = Nothing
        Private wtPen As Pen = Nothing
        Private wStBrush As Brush = Nothing
        Private Bg As Color = Nothing
        Private Fg As Color = Nothing

        Sub New()
            MyBase.Name = "Reactive Wave"
        End Sub

        Private _FramerateLimit As Integer = MyBase.FramerateLimit
        Public Overrides Property FramerateLimit As Integer
            Get
                Return MyBase.FramerateLimit
            End Get
            Set(value As Integer)
                If WaveType <> 7 Then
                    MyBase.FramerateLimit = value
                    _FramerateLimit = value
                End If
            End Set
        End Property

        Private _WaveType As Integer = 0
        ''' <summary>
        ''' 0: Wave, 1: Bars, 2: Spectrum, 3: Lines, 4:Waveform, 5:Spectrum, 6:Dot, 7:None
        ''' </summary>
        ''' <returns></returns>
        Property WaveType As Integer
            Get
                Return _WaveType
            End Get
            Set(value As Integer)
                If _WaveType = 7 AndAlso value <> 7 Then
                    MyBase.FramerateLimit = _FramerateLimit
                    IsEnabled = True
                End If
                _WaveType = value
                OnPropertyChanged()
                If value = 7 Then
                    MyBase.FramerateLimit = 1
                End If
            End Set
        End Property

        Private _OverrideForegroundBrush As Brush
        Property OverrideForegroundBrush As Brush
            Get
                Return _OverrideForegroundBrush
            End Get
            Set(value As Brush)
                _OverrideForegroundBrush = value
                OnPropertyChanged()
            End Set
        End Property

        Public Overrides Sub OnStartRequested()
            _Visuals.ScaleFactorSqr = 2
            _Visuals.ScaleFactorSqrBoost = 0.1
            If Metadata Is Nothing Then
                Bg = Colors.Transparent
                Fg = Color.FromRgb(255, 193, 7)
                wPen = New Pen(Brushes.Green, 1) With {.DashCap = PenLineCap.Round, .EndLineCap = PenLineCap.Round, .LineJoin = PenLineJoin.Miter, .StartLineCap = PenLineCap.Round}
                wtPen = New Pen(Brushes.Green, 3) With {.DashCap = PenLineCap.Round, .EndLineCap = PenLineCap.Round, .LineJoin = PenLineJoin.Miter, .StartLineCap = PenLineCap.Round}
                wStBrush = New SolidColorBrush(Color.FromArgb(150, 0, 255, 0))
                wPen.Freeze()
                wtPen.Freeze()
                wStBrush.Freeze()
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
            Dim x As New DrawingVisual
            Dim v As New DrawingImage(x.Drawing)
            Update(v)
            If Not IsStoppedByPowerSaving Then IsEnabled = False
        End Sub

        Public Overrides Sub OnPlayerResumed()
            If Not IsStoppedByPowerSaving Then IsEnabled = True
        End Sub

        Public Overrides Sub OnThemeChanged()
            OnMetadataChanged()
        End Sub

        Public Overrides Sub OnMetadataChanged()
            If HandyControl.Themes.ThemeManager.Current.ApplicationTheme = HandyControl.Themes.ApplicationTheme.Dark Then
                wPen = New Pen(New SolidColorBrush(Color.FromRgb(255, 255, 255)), 1) With {.DashCap = PenLineCap.Round, .EndLineCap = PenLineCap.Round, .LineJoin = PenLineJoin.Miter, .StartLineCap = PenLineCap.Round}
                wtPen = New Pen(New SolidColorBrush(Color.FromRgb(255, 255, 255)), 3) With {.DashCap = PenLineCap.Round, .EndLineCap = PenLineCap.Round, .LineJoin = PenLineJoin.Miter, .StartLineCap = PenLineCap.Round}
                wStBrush = New SolidColorBrush(Color.FromArgb(150, 230, 230, 230))
                wPen.Freeze()
                wtPen.Freeze()
                wStBrush.Freeze()
                'If Not Metadata.HasCover Then
                Bg = Colors.Transparent
                Fg = Colors.White
                'End If
            Else
                wPen = New Pen(New SolidColorBrush(Color.FromRgb(0, 0, 0)), 1) With {.DashCap = PenLineCap.Round, .EndLineCap = PenLineCap.Round, .LineJoin = PenLineJoin.Miter, .StartLineCap = PenLineCap.Round}
                wtPen = New Pen(New SolidColorBrush(Color.FromRgb(0, 0, 0)), 3) With {.DashCap = PenLineCap.Round, .EndLineCap = PenLineCap.Round, .LineJoin = PenLineJoin.Miter, .StartLineCap = PenLineCap.Round}
                wStBrush = New SolidColorBrush(Color.FromArgb(150, 25, 25, 25))
                wPen.Freeze()
                wtPen.Freeze()
                wStBrush.Freeze()
                'If Not Metadata?.HasCover Then
                Bg = Colors.Transparent
                Fg = Colors.Black
                'End If
            End If
            lastvalues = {-100, -100, -100}
            'If Metadata?.HasCover Then
            '    Dim Clr = Utilities.CommonFunctions.GetAverageColor(Metadata.DefaultCover)
            '    Dim Luma = Utilities.CommonFunctions.Luminance(Clr)
            '    If Luma < 20 Then
            '        Fg = Colors.White
            '    End If
            '    If Luma < 40 Then
            '        Fg = Clr
            '    Else
            '        Fg = Colors.Black
            '    End If
            '    Bg = Clr
            'End If
        End Sub

        Private _Visuals As New Un4seen.Bass.Misc.Visuals With {.ScaleFactorSqr = 1, .ScaleFactorSqrBoost = 0.005} 'Low,High
        Dim phase = 0
        Dim lastvalues As Single() = {-100, -100, -100}
        Dim targetvalues As Single() = {0, 0, 0}
        Public Overrides Function Render() As ImageSource
            Select Case WaveType
                Case 0
                    Dim data = Utilities.SharedProperties.Instance.Player.GetChannelPeakData2(New Single() {150, 5000, 10000}, 128, Un4seen.Bass.BASSData.BASS_DATA_FFT256, 10)
                    Dim x As New DrawingVisual
                    Dim dc = x.RenderOpen
                    'Updating values
                    For _i As Integer = 0 To 2
                        If lastvalues(_i) = -100 Then
                            lastvalues(_i) = 0
                            targetvalues(_i) = data(_i)
                        End If
                    Next
                    For _i As Integer = 0 To 2
                        If Math.Round(lastvalues(_i), 1) = Math.Round(targetvalues(_i), 1) Then
                            targetvalues(_i) = Math.Round(If(data(_i) < 5, data(_i), data(_i) * 2), 1)
                        Else
                            If lastvalues(_i) < targetvalues(_i) Then lastvalues(_i) += 0.1 Else lastvalues(_i) -= 0.1
                        End If
                    Next
                    phase += 1
                    If phase >= 0 Then
                        lastvalues(2) = lastvalues(1)
                        lastvalues(1) = lastvalues(0) 'Symetric
                        phase = -395
                    End If
                    Dim _Level = Bass.BASS_ChannelGetLevel(Stream)
                    Dim _Left = Utils.LowWord32(_Level) / 1000 '0-32
                    Dim _Right = Utils.HighWord32(_Level) / 1000 '0-32
                    'ref
                    'Dim path As New PathFigure()
                    'path.StartPoint = New Point(10, 100)
                    'path.Segments.Add(New BezierSegment(New Point(200, -200), New Point(200, 400), New Point(400, 100), True))                                
                    dc.PushTransform(New ScaleTransform(Width / 400, Height / 100))
                    dc.PushClip(Geometry.Parse("M 0,0 H 400 V 100 H 0 Z"))
                    dc.DrawRectangle(Brushes.Transparent, Nothing, New Rect(0, 0, 400, 100))

                    Dim path As New PathFigure With {
                        .StartPoint = New Point(10 + phase, 100)
                    }
                    path.Segments.Add(New BezierSegment(New Point(200 + phase, lastvalues(0) * -10),
                                                    New Point(200 + phase, lastvalues(1) * 20),
                                                    New Point(400 + phase, 100), True))
                    Dim cpath As New PathFigure With {
                        .StartPoint = New Point(400 + phase, 100)
                    }
                    cpath.Segments.Add(New BezierSegment(New Point(600 + phase, lastvalues(1) * -10),
                                                     New Point(600 + phase, lastvalues(2) * 20),
                                                     New Point(800 + phase, 100), True))
                    Dim gpath As New PathGeometry({path, cpath})
                    dc.DrawGeometry(wStBrush, wPen, gpath)
                    dc.PushTransform(New TranslateTransform(150, 25))
                    dc.DrawGeometry(wStBrush, wPen, gpath)
                    dc.Close()
                    Dim v As New DrawingImage(x.Drawing)
                    Return v
                Case 1, 2, 3
                    Dim data = Utilities.SharedProperties.Instance.Player.GetChannelPeakData2(New Single() {10, 206, 413, 620, 827, 1034, 1241, 1448, 2896, 3103, 3310, 3517, 4137, 4758, 4965, 5172, 5379, 5586, 5793, 6000, 6206, 6413, 6620, 6827, 7034, 7241, 7448, 7655, 7862, 8068, 8275, 8482, 8689, 8896, 9103, 9310, 9517, 9724, 9931, 10137, 10344, 10551, 10758, 10965, 11172, 11379, 11586, 11793, 12000}, 256, Un4seen.Bass.BASSData.BASS_DATA_FFT512, 100)
                    For _i As Integer = 0 To data.Length - 1
                        data(_i) = Math.Sqrt(data(_i)) * 10
                    Next
                    Dim x As New DrawingVisual
                    Dim dc = x.RenderOpen
                    Dim i = data.Length - 1
                    dc.PushTransform(New ScaleTransform(Width / 730, Height / 100))
                    dc.PushTransform(New RotateTransform(180, 350, 50))
                    Dim _Peak As Single = 0
                    data = Utilities.CommonFunctions.CalcMovAvg(data, 1)
                    dc.DrawRectangle(Brushes.Transparent, Nothing, New Rect(0, 0, (i + 1) * 10 + 5 * (i), 100))
                    For Each peak In data
                        If WaveType = 1 Then
                            dc.DrawRectangle(Brushes.Transparent, Nothing, New Rect(10 * i + (5 * i), 0, 10, 100))
                            dc.DrawRectangle(wStBrush, Nothing, New Rect(10 * i + (5 * i), 0, 10, peak))
                        ElseIf WaveType = 2 Then
                            dc.DrawRectangle(Brushes.Transparent, Nothing, New Rect(10 * i + (5 * i), 0, 10, 100))
                            dc.DrawLine(wtPen, New Point(10 * (i + 1) + (5 * (i + 1) + 5), _Peak), New Point(10 * i + (5 * i) + 5, peak))
                            _Peak = peak
                        ElseIf WaveType = 3 Then
                            dc.DrawRectangle(Brushes.Transparent, Nothing, New Rect(10 * i + (5 * i), 0, 10, 100))
                            dc.DrawLine(wtPen, New Point(10 * i + (5 * i), peak), New Point((10 * i + (5 * i)) + 10, peak))
                            _Peak = peak
                        End If
                        i -= 1
                    Next
                    dc.Close()
                    Dim v As New DrawingImage(x.Drawing)
                    Return v
                Case 4
                    Return Utilities.ToImageSource(_Visuals.CreateWaveForm(Stream, Width, Height, Utilities.CommonFunctions.ToGDIColor(Fg), Utilities.CommonFunctions.ToGDIColor(Fg), System.Drawing.Color.Empty, System.Drawing.Color.Empty, 2, False, True, False))
                Case 5
                    Return Utilities.ToImageSource(_Visuals.CreateSpectrumWave(Stream, Width, Height, Utilities.CommonFunctions.ToGDIColor(Fg), Utilities.CommonFunctions.ToGDIColor(Fg), System.Drawing.Color.Empty, 1, False, False, False))
                Case 6
                    Return Utilities.ToImageSource(_Visuals.CreateSpectrumDot(Stream, Width, Height, Utilities.CommonFunctions.ToGDIColor(Fg), Utilities.CommonFunctions.ToGDIColor(Fg), System.Drawing.Color.Empty, 2, 2, False, True, False))
                Case 7
                    Dim x As New DrawingVisual
                    Dim dc = x.RenderOpen
                    dc.DrawRectangle(wStBrush, Nothing, New Rect(0, 0, Width, Height))
                    dc.Close()
                    Dim v As New DrawingImage(x.Drawing)
                    IsEnabled = False
                    Return v
            End Select
            Return Nothing
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
End Namespace