Imports System.ComponentModel
Imports QuickBeat.Classes
Imports QuickBeat.Interfaces
Imports QuickBeat.Utilities
Imports Un4seen.Bass
Imports Windows.Media

Namespace Player
    <Serializable>
    Public Class Player
        Implements Interfaces.IStartupItem, INotifyPropertyChanged

#Region "Classes"
        Public Class StreamControlHandle
            Implements INotifyPropertyChanged

            Public Event OnPlay(sender As StreamControlHandle)
            Public Event OnPause(sender As StreamControlHandle)
            Public Event OnStop(sender As StreamControlHandle)
            Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged

            Private _Handle As Integer
            ReadOnly Property Handle As Integer
                Get
                    Return _Handle
                End Get
            End Property

            Property Tag As Object

            Property IsPlaying As Boolean
                Get
                    Return (Bass.BASS_ChannelIsActive(Handle) = BASSActive.BASS_ACTIVE_PLAYING)
                End Get
                Set(value As Boolean)
                    If value Then
                        Play()
                    Else
                        Pause()
                    End If
                    Notify()
                End Set
            End Property

            Property IsPaused As Boolean
                Get
                    Return (Bass.BASS_ChannelIsActive(Handle) = BASSActive.BASS_ACTIVE_PAUSED)
                End Get
                Set(value As Boolean)
                    If value Then
                        Pause()
                    Else
                        Play()
                    End If
                    Notify()
                End Set
            End Property

            ReadOnly Property IsStalled As Boolean
                Get
                    Return (Bass.BASS_ChannelIsActive(Handle) = BASSActive.BASS_ACTIVE_STALLED)
                End Get
            End Property

            Property IsStopped As Boolean
                Get
                    Return (Bass.BASS_ChannelIsActive(Handle) = BASSActive.BASS_ACTIVE_STOPPED)
                End Get
                Set(value As Boolean)
                    If value Then
                        [Stop]()
                    Else
                        Play()
                    End If
                    Notify()
                End Set
            End Property

            Property Position As Double
                Get
                    Return Bass.BASS_ChannelBytes2Seconds(Handle, Bass.BASS_ChannelGetPosition(Handle, BASSMode.BASS_POS_BYTE))
                End Get
                Set(value As Double)
                    If Handle = 0 Then Return
                    Bass.BASS_ChannelSetPosition(Handle, value)
                End Set
            End Property

            ReadOnly Property PositionString As String
                Get
                    Dim ts = TimeSpan.FromSeconds(Position)
                    Return $"{If(ts.Minutes < 10, 0, "")}{ts.Minutes}:{If(ts.Seconds < 10, 0, "")}{ts.Seconds}"
                End Get
            End Property

            ReadOnly Property Length As Double
                Get
                    Return Bass.BASS_ChannelBytes2Seconds(Handle, Bass.BASS_ChannelGetLength(Handle, BASSMode.BASS_POS_BYTE))
                End Get
            End Property

            Sub New(Handle As Integer)
                _Handle = Handle
            End Sub

            Public Sub OnPropertyChanged(<Runtime.CompilerServices.CallerMemberName> Optional CallerName As String = Nothing)
                RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(CallerName))
            End Sub

            Function Play() As Boolean
                Dim result = Bass.BASS_ChannelPlay(Handle, False)
                If result Then
                    Notify()
                    RaiseEvent OnPlay(Me)
                End If
                Return result
            End Function

            Function Pause() As Boolean
                Dim result = Bass.BASS_ChannelPause(Handle)
                If result Then
                    Notify()
                    RaiseEvent OnPause(Me)
                End If
                Return result
            End Function

            Function [Stop]() As Boolean
                Dim result = Bass.BASS_ChannelStop(Handle)
                If result Then
                    Notify()
                    RaiseEvent OnStop(Me)
                Else
                    If Bass.BASS_ChannelIsActive(Handle) = BASSActive.BASS_ACTIVE_STOPPED Then
                        RaiseEvent OnStop(Me)
                    End If
                End If
                Return result
            End Function

            Private Sub Notify()
                OnPropertyChanged(NameOf(IsPlaying))
                OnPropertyChanged(NameOf(IsPaused))
                OnPropertyChanged(NameOf(IsStalled))
                OnPropertyChanged(NameOf(IsStopped))
                OnPropertyChanged(NameOf(Position))
                OnPropertyChanged(NameOf(PositionString))
                OnPropertyChanged(NameOf(Length))
            End Sub
        End Class

        Public Class ABLoopHandle
            Implements INotifyPropertyChanged

            Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged

            Public Sub OnPropertyChanged(<Runtime.CompilerServices.CallerMemberName> Optional CallerName As String = Nothing)
                RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(CallerName))
            End Sub

            Private _BSyncHandle As Integer

            Private _BSyncProc As New SYNCPROC(Sub(h, c, d, u)
                                                   Bass.BASS_ChannelSetPosition(Stream, A)
                                               End Sub)

            Private _Stream As Integer
            Property Stream As Integer
                Get
                    Return _Stream
                End Get
                Set(value As Integer)
                    RevokeHandle()
                    EnsureHandle()
                    _Stream = value
                    OnPropertyChanged()
                End Set
            End Property

            Private _A As Double
            Property A As Double
                Get
                    Return _A
                End Get
                Set(value As Double)
                    _A = value
                    OnPropertyChanged()
                    OnPropertyChanged(NameOf(AString))
                End Set
            End Property

            ReadOnly Property AString As String
                Get
                    Dim ts = TimeSpan.FromSeconds(A)
                    Return $"{If(ts.Minutes < 10, 0, "")}{ts.Minutes}:{If(ts.Seconds < 10, 0, "")}{ts.Seconds}"
                End Get
            End Property

            Private _B As Double
            Property B As Double
                Get
                    Return _B
                End Get
                Set(value As Double)
                    _B = value
                    If _BSyncHandle <> 0 Then
                        Bass.BASS_ChannelRemoveSync(Stream, _BSyncHandle)
                    End If
                    If Stream <> 0 Then _BSyncHandle = Bass.BASS_ChannelSetSync(Stream, BASSSync.BASS_SYNC_POS, Bass.BASS_ChannelSeconds2Bytes(Stream, value), _BSyncProc, IntPtr.Zero)

                    OnPropertyChanged()
                    OnPropertyChanged(NameOf(BString))
                End Set
            End Property

            ReadOnly Property BString As String
                Get
                    Dim ts = TimeSpan.FromSeconds(B)
                    Return $"{If(ts.Minutes < 10, 0, "")}{ts.Minutes}:{If(ts.Seconds < 10, 0, "")}{ts.Seconds}"
                End Get
            End Property
            Public Sub EnsureHandle()
                If _BSyncHandle = 0 Then
                    _BSyncHandle = Bass.BASS_ChannelSetSync(Stream, BASSSync.BASS_SYNC_POS, Bass.BASS_ChannelSeconds2Bytes(Stream, If(B <> 0, B, Bass.BASS_ChannelGetLength(Stream))), _BSyncProc, IntPtr.Zero)
                End If
            End Sub

            Public Sub RevokeHandle()
                If _BSyncHandle <> 0 Then
                    If Bass.BASS_ChannelRemoveSync(Stream, _BSyncHandle) Then
                        _BSyncHandle = 0
                    End If
                End If
            End Sub

        End Class

        Public Class PeakLevels
            Implements INotifyPropertyChanged

            Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged

            Public Sub OnPropertyChanged(<Runtime.CompilerServices.CallerMemberName> Optional CallerName As String = Nothing)
                RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(CallerName))
                Select Case CallerName
                    Case NameOf(IsActive)
                        If IsActive Then
                            Task.Run(Async Function()
                                         Do While IsActive
                                             Dim _Level = Bass.BASS_ChannelGetLevel(Stream)
                                             _Left = Utils.LowWord32(_Level) / 1000
                                             _Right = Utils.HighWord32(_Level) / 1000
                                             OnPropertyChanged(NameOf(Left))
                                             OnPropertyChanged(NameOf(Right))
                                             Await Task.Delay(22)
                                         Loop
                                     End Function)
                        End If
                End Select
            End Sub

            Private _Stream As Integer
            Property Stream As Integer
                Get
                    Return _Stream
                End Get
                Set(value As Integer)
                    _Stream = value
                    OnPropertyChanged()
                End Set
            End Property

            Private _Left As Double
            ''' <summary>
            ''' The left channel's peak level, 0->32
            ''' </summary>
            ''' <returns></returns>
            Property Left As Double
                Get
                    Return _Left
                End Get
                Set(value As Double)
                    _Left = value
                    OnPropertyChanged()
                End Set
            End Property

            Property _Right As Double
            ''' <summary>
            ''' The right channel's peak level, 0->32
            ''' </summary>
            ''' <returns></returns>
            Property Right As Double
                Get
                    Return _Right
                End Get
                Set(value As Double)
                    _Right = value
                    OnPropertyChanged()
                End Set
            End Property

            Private _IsActive As Boolean
            Property IsActive As Boolean
                Get
                    Return _IsActive
                End Get
                Set(value As Boolean)
                    _IsActive = value
                    OnPropertyChanged()
                End Set
            End Property
        End Class
        Public Class BassStream
            Implements INotifyPropertyChanged

            Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged

            Public Sub OnPropertyChanged(<Runtime.CompilerServices.CallerMemberName> Optional CallerName As String = Nothing)
                RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(CallerName))
            End Sub

            Private _IsFreed
            Property IsFreed As Boolean
                Get
                    Return _IsFreed
                End Get
                Set(value As Boolean)
                    _IsFreed = value
                    OnPropertyChanged()
                    OnPropertyChanged(NameOf(ControlHandle))
                End Set
            End Property

            Property AutoFree As Boolean
                Get
                    If (Bass.BASS_ChannelFlags(Handle, BASSFlag.BASS_DEFAULT, BASSFlag.BASS_DEFAULT) And BASSFlag.BASS_STREAM_AUTOFREE) = BASSFlag.BASS_STREAM_AUTOFREE Then
                        Return True
                    Else
                        Return False
                    End If
                End Get
                Set(value As Boolean)
                    If value Then
                        ' loop flag was not set, so set it
                        Bass.BASS_ChannelFlags(Handle, BASSFlag.BASS_STREAM_AUTOFREE, BASSFlag.BASS_STREAM_AUTOFREE)
                        OnPropertyChanged()
                    Else
                        ' loop flag was set, so remove it
                        Bass.BASS_ChannelFlags(Handle, BASSFlag.BASS_DEFAULT, BASSFlag.BASS_STREAM_AUTOFREE)
                        OnPropertyChanged()
                    End If
                End Set
            End Property

            Private _Handle As Integer
            Public ReadOnly Property Handle As Integer
                Get
                    Return _Handle
                End Get
            End Property

            Private _ControlHandle As StreamControlHandle
            ReadOnly Property ControlHandle As StreamControlHandle
                Get
                    Return _ControlHandle
                End Get
            End Property

            Private _Metadata As Metadata
            Property Metadata As Metadata
                Get
                    Return _Metadata
                End Get
                Set(value As Metadata)
                    _Metadata = value
                    OnPropertyChanged()
                End Set
            End Property

            Private _StreamInfo As String
            ReadOnly Property StreamInfo As String
                Get
                    Return _StreamInfo
                End Get
            End Property

            Private _proc As New SYNCPROC(Sub(h, channel, data, user)
                                              IsFreed = True
                                          End Sub)

            Sub New(handle As Integer)
                _Handle = handle
                _ControlHandle = New StreamControlHandle(handle)
                If handle <> 0 Then
                    _StreamInfo = Bass.BASS_ChannelGetInfo(handle)?.ToString
                End If
                Bass.BASS_ChannelSetSync(handle, BASSSync.BASS_SYNC_FREE, 0, _proc, IntPtr.Zero)
                OnPropertyChanged(NameOf(Me.Handle))
                OnPropertyChanged(NameOf(ControlHandle))
                OnPropertyChanged(NameOf(StreamInfo))
            End Sub
        End Class

        Public Enum AudioQuality
            LQ '<=128kbps
            SQ '192/256kbps
            HQ '320kbps
            UHQ '>320kbps
        End Enum
#End Region
#Region "IStartupItem Impl."
        Public Sub OnPropertyChanged(<Runtime.CompilerServices.CallerMemberName> Optional CallerName As String = Nothing)
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(CallerName))
            Select Case CallerName
                Case NameOf(IsPlaying)
                    If IsPlaying Then
                        If Level IsNot Nothing Then Level.IsActive = True
                        If IsPreviewing Then
                            PreviewControlHandle?.Stop()
                        End If
                        Task.Run(Async Function()
                                     Do While IsPlaying
                                         OnPropertyChanged(NameOf(Position))
                                         OnPropertyChanged(NameOf(PositionString))
                                         Await Task.Delay(1000)
                                     Loop
                                 End Function)
                        OnPropertyChanged(NameOf(TaskbarState))
                        OnPropertyChanged(NameOf(TaskbarOverlay))
                        If IsUsingSMTC Then GetSMTC().PlaybackStatus = MediaPlaybackStatus.Playing : GetSMTC().DisplayUpdater.Update()
                    End If
                Case NameOf(IsPaused)
                    If IsPaused Then
                        If Level IsNot Nothing Then Level.IsActive = False
                        OnPropertyChanged(NameOf(TaskbarState))
                        OnPropertyChanged(NameOf(TaskbarOverlay))
                        If IsUsingSMTC Then GetSMTC().PlaybackStatus = MediaPlaybackStatus.Paused : GetSMTC().DisplayUpdater.Update()
                    End If
                Case NameOf(IsStopped)
                    If IsStopped Then
                        If Level IsNot Nothing Then Level.IsActive = False
                        OnPropertyChanged(NameOf(TaskbarState))
                        OnPropertyChanged(NameOf(TaskbarOverlay))
                        If IsUsingSMTC Then GetSMTC().PlaybackStatus = MediaPlaybackStatus.Stopped : GetSMTC().DisplayUpdater.Update()
                    End If
                Case NameOf(IsStalled)
                    If IsStalled Then
                        If Level IsNot Nothing Then Level.IsActive = False
                        OnPropertyChanged(NameOf(TaskbarState))
                        OnPropertyChanged(NameOf(TaskbarOverlay))
                        If IsUsingSMTC Then GetSMTC().PlaybackStatus = MediaPlaybackStatus.Closed : GetSMTC().DisplayUpdater.Update()
                    End If
                Case NameOf(Position)
                    OnPropertyChanged(NameOf(TaskbarProgress))
                    If IsUsingSMTC Then GetSMTC()?.UpdateTimelineProperties(New SystemMediaTransportControlsTimelineProperties() With {
                                                                            .StartTime = TimeSpan.Zero,
                                                                            .EndTime = TimeSpan.FromSeconds(Length),
                                                                            .Position = TimeSpan.FromSeconds(Position),
                                                                            .MinSeekTime = TimeSpan.FromMilliseconds(100),
                                                                            .MaxSeekTime = TimeSpan.FromSeconds(Length - Position)}) : GetSMTC().DisplayUpdater.Update()
            End Select
        End Sub

        Sub Init() Implements IStartupItem.Init
            If SharedProperties.Instance.IsLogging Then Utilities.DebugMode.Instance.Log(Of Player)("Initializing BASS...")
            Configuration.SetStatus("Initializing...", 0)
            Bass.BASS_SetConfig(BASSConfig.BASS_CONFIG_DEV_DEFAULT, 1) 'So BASS output will follow active device.
            Dim outdevices = Bass.BASS_GetDeviceInfos
            Me.Devices.Clear()
            For Each device In outdevices
                Me.Devices.Add(device)
            Next
            Dim IsInit = Bass.BASS_Init(-1, SampleRate, BASSInit.BASS_DEVICE_DEFAULT, IntPtr.Zero)
            _Output = Bass.BASS_GetDevice
            OnPropertyChanged(NameOf(Output))
            OnIsInitializedChanged(IsInit)
            If SharedProperties.Instance.IsLogging Then
                If IsInit Then Utilities.DebugMode.Instance.Log(Of Player)("Done initializing player.") Else Utilities.DebugMode.Instance.Log(Of Player)("An error occured at player initialization, " & Bass.BASS_ErrorGetCode.ToString)
            End If
            If IsInit Then Configuration.SetStatus("All Good", 100) Else Configuration.SetError(True, New Exception("An error occured at initialization level. " & Bass.BASS_ErrorGetCode.ToString))
        End Sub

#End Region
#Region "Events"
        Event IsInitializedChanged()
        Event MediaEnded(ByRef IsHandled As Boolean)
        ''' <summary>
        ''' Occures when <see cref="Stream"/> is changed
        ''' </summary>
        Event MediaLoaded(OldValue As Integer, NewValue As Integer)
        Event PositionChanged(newPosition As Integer)
        Event MetadataChanged()
        <NonSerialized> Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged
#End Region
#Region "Properties"

        Private _Configuration As New StartupItemConfiguration("Player")
        Public Property Configuration As StartupItemConfiguration Implements IStartupItem.Configuration
            Get
                Return _Configuration
            End Get
            Set(value As StartupItemConfiguration)
                _Configuration = value
                OnPropertyChanged()
            End Set
        End Property

        <NonSerialized> Private _IsInitialized As Boolean
        ReadOnly Property IsInitialized As Boolean
            Get
                Return _IsInitialized
            End Get
        End Property

        Property Devices As New ObjectModel.ObservableCollection(Of BASS_DEVICEINFO)

        Private _Output As Integer = -1
        Property Output As Integer
            Get
                Return _Output
            End Get
            Set(value As Integer)
                If value < 0 OrElse value = _Output OrElse value >= Devices.Count Then
                    OnPropertyChanged()
                    Return
                End If
                Dim IsInit = Bass.BASS_Init(value, SampleRate, BASSInit.BASS_DEVICE_DEFAULT, IntPtr.Zero)
                Dim errcode = Bass.BASS_ErrorGetCode
                If errcode = BASSError.BASS_OK OrElse errcode = BASSError.BASS_ERROR_ALREADY Then _Output = value 'Bass.BASS_GetDevice
                OnPropertyChanged()
                OnIsInitializedChanged(IsInit)
                If (IsInit OrElse errcode = BASSError.BASS_ERROR_ALREADY) AndAlso Stream <> 0 Then
                    If Not Bass.BASS_ChannelSetDevice(Stream, value) Then
                        Configuration.SetError(True, New InvalidOperationException(Bass.BASS_ErrorGetCode.ToString))
                    Else
                        Configuration.SetError(False, Nothing)
                    End If
                End If
            End Set
        End Property

        Property SampleRate = 44100

        Private _Stream As BassStream
        ReadOnly Property ChannelStream As BassStream
            Get
                Return _Stream
            End Get
        End Property
        ReadOnly Property Stream As Integer
            Get
                Return If(_Stream Is Nothing, 0, _Stream.Handle)
            End Get
        End Property

        Private _Metadata As Metadata
        ReadOnly Property StreamMetadata As Metadata
            Get
                Return _Metadata
            End Get
        End Property

        Private _StreamQuality As AudioQuality
        Property StreamQuality As AudioQuality
            Get
                Return _StreamQuality
            End Get
            Set(value As AudioQuality)
                _StreamQuality = value
                OnPropertyChanged()
            End Set
        End Property

        Private _Bitrate As Integer
        Property Bitrate As Integer
            Get
                Return _Bitrate
            End Get
            Set(value As Integer)
                _Bitrate = value
                OnPropertyChanged()
            End Set
        End Property

        Private _Playlist As New Playlist With {.Parent = Me, .Category = "Queue", .Description = "Current playing queue", .Cover = New BitmapImage(New Uri("\Resources\MusicRecord.png", UriKind.Relative))}
        ReadOnly Property Playlist As Playlist
            Get
                Return _Playlist
            End Get
        End Property

        Property AutoPlay As Boolean = True

        ReadOnly Property IsPlaying As Boolean
            Get
                Return (Bass.BASS_ChannelIsActive(Stream) = BASSActive.BASS_ACTIVE_PLAYING)
            End Get
        End Property

        ReadOnly Property IsPaused As Boolean
            Get
                Return (Bass.BASS_ChannelIsActive(Stream) = BASSActive.BASS_ACTIVE_PAUSED)
            End Get
        End Property

        ReadOnly Property IsStalled As Boolean
            Get
                Return (Bass.BASS_ChannelIsActive(Stream) = BASSActive.BASS_ACTIVE_STALLED)
            End Get
        End Property

        ReadOnly Property IsStopped As Boolean
            Get
                Return (Bass.BASS_ChannelIsActive(Stream) = BASSActive.BASS_ACTIVE_STOPPED)
            End Get
        End Property

        ''' <summary>
        ''' Stream's position in seconds
        ''' </summary>
        ''' <returns></returns>
        Property Position As Double
            Get
                Return Bass.BASS_ChannelBytes2Seconds(Stream, Bass.BASS_ChannelGetPosition(Stream, BASSMode.BASS_POS_BYTE))
            End Get
            Set(value As Double)
                If Stream = 0 Then Return
                If Bass.BASS_ChannelSetPosition(Stream, value) Then
                    RaiseEvent PositionChanged(value)
                End If
            End Set
        End Property

        ReadOnly Property PositionString As String
            Get
                Dim ts = TimeSpan.FromSeconds(Position)
                Return $"{If(ts.Minutes < 10, 0, "")}{ts.Minutes}:{If(ts.Seconds < 10, 0, "")}{ts.Seconds}"
            End Get
        End Property

        Private _IsLooping As Boolean
        Property IsLooping As Boolean
            Get
                If (Bass.BASS_ChannelFlags(Stream, BASSFlag.BASS_DEFAULT, BASSFlag.BASS_DEFAULT) And BASSFlag.BASS_SAMPLE_LOOP) = BASSFlag.BASS_SAMPLE_LOOP Then
                    Return True
                Else
                    Return False
                End If
            End Get
            Set(value As Boolean)
                If value Then
                    ' loop flag was not set, so set it
                    Bass.BASS_ChannelFlags(Stream, BASSFlag.BASS_SAMPLE_LOOP, BASSFlag.BASS_SAMPLE_LOOP)
                    _IsLooping = True
                    OnPropertyChanged()
                Else
                    ' loop flag was set, so remove it
                    Bass.BASS_ChannelFlags(Stream, BASSFlag.BASS_DEFAULT, BASSFlag.BASS_SAMPLE_LOOP)
                    _IsLooping = False
                    OnPropertyChanged()
                End If
            End Set
        End Property

        Private _IsMuted As Boolean
        Property IsMuted As Boolean
            Get
                Return _IsMuted
            End Get
            Set(value As Boolean)
                If value Then
                    Volume = 0
                Else
                    Volume = _Volume
                End If
                _IsMuted = value
                OnPropertyChanged()
            End Set
        End Property

        ''' <summary>
        ''' Raw value, not affected by <see cref="TrueVolume"/>
        ''' </summary>
        Private _RawVolume As Double = 100
        Private _Volume As Double = 100
        ''' <summary>
        ''' 0 to 100, when setting takes into account <see cref="TrueVolume"/>
        ''' </summary>
        ''' <returns></returns>
        Property Volume As Double
            Get
                Return If(TrueVolume, Math.Sqrt(_Volume) * 10, _Volume)
            End Get
            Set(value As Double)
                Dim truevalue = If(TrueVolume, (value / 10) ^ 2, value)
                If truevalue > 100 Then Return
                If Bass.BASS_ChannelSetAttribute(Stream, BASSAttribute.BASS_ATTRIB_VOL, truevalue / 100) Then
                    If truevalue > 0 Then
                        _Volume = truevalue
                        _RawVolume = value
                        _IsMuted = False
                    Else
                        _IsMuted = True
                    End If
                    OnPropertyChanged(NameOf(IsMuted))
                    OnPropertyChanged(NameOf(IsHighVolume))
                    OnPropertyChanged()
                End If
            End Set
        End Property

        Private _TrueVolume As Boolean
        ''' <summary>
        ''' Eliminates the great change in volume when below 50, but slight change when above 50.
        ''' </summary>
        ''' <returns></returns>
        Property TrueVolume As Boolean
            Get
                Return _TrueVolume
            End Get
            Set(value As Boolean)
                _TrueVolume = value
                OnPropertyChanged()
                OnPropertyChanged(NameOf(Volume))
            End Set
        End Property

        ReadOnly Property IsHighVolume As Boolean
            Get
                Return Volume > 60
            End Get
        End Property

        Private _Level As New PeakLevels()
        Property Level As PeakLevels
            Get
                Return _Level
            End Get
            Set(value As PeakLevels)
                _Level = value
                OnPropertyChanged()
            End Set
        End Property

        Private _IsFadingAudio As Boolean
        Property IsFadingAudio As Boolean
            Get
                Return _IsFadingAudio
            End Get
            Set(value As Boolean)
                _IsFadingAudio = value
                If Not value Then
                    If IsPaused AndAlso Not IsMuted Then
                        Bass.BASS_ChannelSetAttribute(Stream, BASSAttribute.BASS_ATTRIB_VOL, Volume / 100)
                    End If
                End If
                OnPropertyChanged()
            End Set
        End Property

        Private _IsCrossfading As Boolean
        Property IsCrossfading As Boolean
            Get
                Return _IsCrossfading
            End Get
            Set(value As Boolean)
                _IsCrossfading = value
                If Not value Then
                    If _CrossfadeProcHandle <> 0 Then
                        If Bass.BASS_ChannelRemoveSync(Stream, _CrossfadeProcHandle) Then _CrossfadeProcHandle = 0
                    End If
                End If
                OnPropertyChanged()
            End Set
        End Property

        Private _CrossfadeDuration As Integer = 5000
        Property CrossfadeDuration As Integer
            Get
                Return _CrossfadeDuration
            End Get
            Set(value As Integer)
                _CrossfadeDuration = value
                OnPropertyChanged()
            End Set
        End Property

        Private _IsSkippingSilence As Boolean
        Property IsSkippingSilence As Boolean
            Get
                Return _IsSkippingSilence
            End Get
            Set(value As Boolean)
                _IsSkippingSilence = value
                OnPropertyChanged()
            End Set
        End Property

        Private _AutoStop As Boolean = True
        Property AutoStop As Boolean
            Get
                Return _AutoStop
            End Get
            Set(value As Boolean)
                _AutoStop = value
                OnPropertyChanged()
            End Set
        End Property

        Private _Path As String
        ''' <summary>
        ''' Leave for player use,Use <see cref="LoadSong(Metadata)"/> or <see cref="LoadSong(String, Boolean)"/> instead of setting path in here.
        ''' </summary>
        ''' <returns></returns>
        Property Path As String
            Get
                Return _Path
            End Get
            Set(value As String)
                _Path = value
                OnPropertyChanged()
            End Set
        End Property

        ReadOnly Property Length As Double
            Get
                Return Bass.BASS_ChannelBytes2Seconds(Stream, Bass.BASS_ChannelGetLength(Stream, BASSMode.BASS_POS_BYTE))
            End Get
        End Property

        Private _Profile As New Profile(Me)
        Property EffectsProfile As Profile
            Get
                Return _Profile
            End Get
            Set(value As Profile)
                If value Is _Profile Then Return
                If _Profile IsNot Nothing Then _Profile.Dispose()
                value.Parent = Me
                _Profile = value
                For Each fx In value
                    fx.HStream = Stream
                    fx.Apply()
                Next
                OnPropertyChanged()
            End Set
        End Property

        Private _VideoEffect As VideoEffects.VideoEffect
        Property VideoEffect As VideoEffects.VideoEffect
            Get
                Return _VideoEffect
            End Get
            Set(value As VideoEffects.VideoEffect)
                If _VideoEffect IsNot Nothing Then
                    _VideoEffect.Stop()
                End If
                _VideoEffect = value
                If _VideoEffect IsNot Nothing Then _VideoEffect.Stream = Stream
                OnPropertyChanged()
            End Set
        End Property

        Private _IsUsingSMTC As Boolean = True
        Property IsUsingSMTC As Boolean
            Get
                Return _IsUsingSMTC
            End Get
            Set(value As Boolean)
                _IsUsingSMTC = value
                OnPropertyChanged()
            End Set
        End Property

        Private _IsUsingSMTCCopyFromFile As Boolean
        Property IsUsingSMTCCopyFromFile As Boolean
            Get
                Return _IsUsingSMTCCopyFromFile
            End Get
            Set(value As Boolean)
                _IsUsingSMTCCopyFromFile = value
                OnPropertyChanged()
            End Set
        End Property

        Private _RemoteTagsReading As Boolean
        Property RemoteTagsReading As Boolean
            Get
                Return _RemoteTagsReading
            End Get
            Set(value As Boolean)
                _RemoteTagsReading = value
                OnPropertyChanged()
            End Set
        End Property

        Private _IsPreviewing As Boolean
        Property IsPreviewing As Boolean
            Get
                Return _IsPreviewing
            End Get
            Set(value As Boolean)
                _IsPreviewing = value
                OnPropertyChanged()
            End Set
        End Property

        Private _PreviewControlHandle As StreamControlHandle
        Property PreviewControlHandle As StreamControlHandle
            Get
                Return _PreviewControlHandle
            End Get
            Set(value As StreamControlHandle)
                _PreviewControlHandle = value
                OnPropertyChanged()
            End Set
        End Property

        Private _ABLoopHandle As ABLoopHandle
        Property ABLoop As ABLoopHandle
            Get
                Return _ABLoopHandle
            End Get
            Set(value As ABLoopHandle)
                _ABLoopHandle?.RevokeHandle()
                If value IsNot Nothing Then
                    value.Stream = Stream
                    value.EnsureHandle()
                End If
                _ABLoopHandle = value
                OnPropertyChanged()
                OnPropertyChanged(NameOf(IsABLoop))
            End Set
        End Property
        Property IsABLoop As Boolean
            Get
                Return ABLoop IsNot Nothing
            End Get
            Set(value As Boolean)
                If Not value Then
                    ABLoop = Nothing
                End If
                OnPropertyChanged()
            End Set
        End Property

        <NonSerialized> Private _Modules As New ObjectModel.ObservableCollection(Of EngineModule)
        Property Modules As ObjectModel.ObservableCollection(Of EngineModule)
            Get
                Return _Modules
            End Get
            Set(value As ObjectModel.ObservableCollection(Of EngineModule))
                _Modules = value
                OnPropertyChanged()
            End Set
        End Property

        <NonSerialized> Private _Streams As New ObjectModel.ObservableCollection(Of BassStream)
        Property Streams As ObjectModel.ObservableCollection(Of BassStream)
            Get
                Return _Streams
            End Get
            Set(value As ObjectModel.ObservableCollection(Of BassStream))
                _Streams = value
                OnPropertyChanged()
            End Set
        End Property

        <NonSerialized> Private _IsLoadingStream As Boolean = False
        Property IsLoadingStream As Boolean
            Get
                Return _IsLoadingStream
            End Get
            Set(value As Boolean)
                _IsLoadingStream = value
                OnPropertyChanged()
            End Set
        End Property


        ReadOnly Property ModuleStatus(name As String) As Boolean
            Get
                Dim _module = Modules.FirstOrDefault(Function(k) k.Name = name)
                Return If(_module Is Nothing, False, _module.IsEnabled)
            End Get
        End Property

        ReadOnly Property IsPlayingFromPlaylist As Boolean
            Get
                Return Path = Playlist?.CurrentItem?.Path
            End Get
        End Property

        Private _DownloadFinished As Boolean
        Property DownloadFinished As Boolean
            Get
                Return _DownloadFinished
            End Get
            Set(value As Boolean)
                _DownloadFinished = value
                OnPropertyChanged()
            End Set
        End Property

        <NonSerialized> Private _EndSyncProcHandle As Integer = 0
        <NonSerialized> Private _EndSyncProc As New SYNCPROC(Sub(h, c, d, u)
                                                                 If IsLooping Then Return
                                                                 Application.Current.Dispatcher.Invoke(Sub()
                                                                                                           If OnMediaEnded() Then Return
                                                                                                           Playlist?.Next()
                                                                                                       End Sub)
                                                             End Sub)

        <NonSerialized> Private _CrossfadeProcHandle As Integer = 0
        <NonSerialized> Private _CrossfadeSyncProc As New SYNCPROC(Sub(h, c, d, u)
                                                                       'c = stream
                                                                       Bass.BASS_ChannelSlideAttribute(c, BASSAttribute.BASS_ATTRIB_VOL, 0, CrossfadeDuration)
                                                                       Application.Current.Dispatcher.Invoke(Sub()
                                                                                                                 Dim NextSong = Playlist?.Next(True, False)
                                                                                                                 IsLoadingStream = True
                                                                                                                 Dim nStream = Bass.BASS_StreamCreateFile(NextSong.Path, 0, 0, BASSFlag.BASS_SAMPLE_FLOAT Or BASSFlag.BASS_STREAM_AUTOFREE)
                                                                                                                 IsLoadingStream = False
                                                                                                                 If nStream <> 0 Then
                                                                                                                     Me.Path = NextSong.Path
                                                                                                                     StreamMetadata?.FreeCovers()
                                                                                                                     OnStreamChanged(nStream)
                                                                                                                     OnMetadataChanged(NextSong)
                                                                                                                     If AutoPlay Then Play()
                                                                                                                     'Utilities.SharedProperties.Instance.Library?.IncreasePlaycount(NextSong.Path, 1)
                                                                                                                 End If
                                                                                                             End Sub)
                                                                   End Sub)

        <NonSerialized> Private _SilencSkipProcHandle As Integer = 0

        <NonSerialized> Private _UWPPlayer As New Playback.MediaPlayer
        <NonSerialized> Private WithEvents _SMTC As SystemMediaTransportControls = _UWPPlayer.SystemMediaTransportControls
        Private _SMTC_Thumb_Path As String
#End Region
#Region "Fields"
        Private _data() As Byte ' local data buffer        
        Private _datastream As IO.MemoryStream 'total local data buffer ,tag-related
        Private _filename As String
        Private _stopdownproc As Boolean = False
        Private _alreadyfoundtags As Boolean = False

        Private _HTTPDOWNLOADPROC As New DOWNLOADPROC(Sub(buffer As IntPtr, length As Integer, user As IntPtr)
                                                          If _stopdownproc Then Return
                                                          'If RemoteTagsReading AndAlso StreamMetadata.Location = Metadata.FileLocation.Remote Then
                                                          If buffer = IntPtr.Zero Then
                                                              ' finished downloading                                                             
                                                              DownloadFinished = True
                                                          Else
                                                              ' increase the data buffer as needed
                                                              If _data Is Nothing OrElse _data.Length < length Then
                                                                  _data = New Byte(length) {}
                                                              End If
                                                              ' copy from managed to unmanaged memory
                                                              Runtime.InteropServices.Marshal.Copy(buffer, _data, 0, length)
                                                              ' write to file
                                                              'TODO write to file
                                                              If _datastream Is Nothing Then
                                                                  _datastream = New IO.MemoryStream()
                                                              End If
                                                              Dim XData = New Byte(length) {}
                                                              ' copy from unmanaged to managed memory
                                                              Runtime.InteropServices.Marshal.Copy(buffer, XData, 0, length)
                                                              _datastream.Write(XData, 0, XData.Length)
                                                              If _alreadyfoundtags Then Return
                                                              Try
                                                                  Dim Tag = TagLib.File.Create(New FileBytesAbstraction("output" & IO.Path.GetExtension(StreamMetadata.Path), _datastream?.ToArray))
                                                                  If Tag.Tag.Title <> "" Then
                                                                      'TODO remove to implement cache dump or prompt to keep cache since it may cause memory problems                                                                      
                                                                      If Not _alreadyfoundtags Then StreamMetadata.LoadFromTFile(Tag, True)
                                                                  End If
                                                              Catch
                                                              End Try
                                                          End If
                                                          'End If
                                                      End Sub)
#End Region
#Region "Raisers"
        Private Sub OnIsInitializedChanged(value As Boolean)
            If IsInitialized = value Then Return
            _IsInitialized = value
            OnPropertyChanged(NameOf(IsInitialized))
            RaiseEvent IsInitializedChanged()
        End Sub

        Private Function OnMediaEnded() As Boolean
            'DisposeSMTC()
            If IsUsingSMTC Then GetSMTC.IsEnabled = False
            Dim IsHandled As Boolean
            RaiseEvent MediaEnded(IsHandled)
            Return IsHandled
        End Function

        Protected Overridable Sub OnMediaLoaded(OldValue As Integer, NewValue As Integer)
            RaiseEvent MediaLoaded(OldValue, NewValue)
        End Sub

        Public Sub OnStreamChanged(value As Integer)
            If value = Stream OrElse value = 0 Then
                _SMTC.IsEnabled = False
                Return
            End If
            'Cleanup
            'HTTP stuff cleaning
            _data = Nothing
            _datastream = Nothing
            DownloadFinished = False
            'General cleaning
            If _EndSyncProcHandle <> 0 Then
                If Bass.BASS_ChannelRemoveSync(Stream, _EndSyncProcHandle) Then _EndSyncProcHandle = 0
            End If
            If _CrossfadeProcHandle <> 0 Then
                If Bass.BASS_ChannelRemoveSync(Stream, _CrossfadeProcHandle) Then _CrossfadeProcHandle = 0
            End If
            If _SilencSkipProcHandle <> 0 Then
                If Bass.BASS_ChannelRemoveSync(Stream, _SilencSkipProcHandle) Then _SilencSkipProcHandle = 0
            End If
            ABLoop?.RevokeHandle()
            ABLoop = Nothing

            Dim OldStream = Stream
            _Stream = New BassStream(value)
            Dim Bitrate As Single
            If Bass.BASS_ChannelGetAttribute(Stream, BASSAttribute.BASS_ATTRIB_BITRATE, Bitrate) Then
                Bitrate = Math.Floor(Bitrate)
                If Bitrate <= 128 Then
                    StreamQuality = AudioQuality.LQ
                ElseIf Bitrate > 128 AndAlso Bitrate < 256 Then
                    StreamQuality = AudioQuality.SQ
                ElseIf Bitrate > 256 AndAlso Bitrate <= 320 Then
                    StreamQuality = AudioQuality.HQ
                ElseIf Bitrate > 320 Then
                    StreamQuality = AudioQuality.UHQ
                End If
                Me.Bitrate = Bitrate
            End If
            OnPropertyChanged(NameOf(ChannelStream))
            OnPropertyChanged(NameOf(Stream))
            OnMediaLoaded(OldStream, Stream)

            If Streams?.FirstOrDefault(Function(k) k.Handle = Stream) Is Nothing Then
                Streams.Insert(0, ChannelStream)
            End If

            'Applying settings
            If _Stream.Handle <> 0 Then
                _EndSyncProcHandle = Bass.BASS_ChannelSetSync(_Stream.Handle, BASSSync.BASS_SYNC_END, 0, _EndSyncProc, IntPtr.Zero)
                Bass.BASS_ChannelSetDevice(_Stream.Handle, Output)
            End If
            'Silence Skip
            Dim cueInPos As Double
            Dim cueOutPos As Double
            If IsSkippingSilence Then
                If Un4seen.Bass.Utils.DetectCuePoints(Path, 10, cueInPos, cueOutPos, -25, -42, 0) Then
                    If Not IsCrossfading Then _SilencSkipProcHandle = Bass.BASS_ChannelSetSync(Stream, BASSSync.BASS_SYNC_POS, Bass.BASS_ChannelSeconds2Bytes(Stream, cueOutPos), _EndSyncProc, IntPtr.Zero)
                    Position = cueInPos
                Else
                    If SharedProperties.Instance.IsLogging Then Utilities.DebugMode.Instance.Log(Of Player)("Unable to detect cue points. " & Bass.BASS_ErrorGetCode.ToString)
                End If
            End If
            If IsCrossfading Then
                If Not IsMuted AndAlso (Playlist?.CurrentItem?.Path = Path) Then
                    If Bass.BASS_ChannelSetAttribute(Stream, BASSAttribute.BASS_ATTRIB_VOL, 0) Then Bass.BASS_ChannelSlideAttribute(Stream, BASSAttribute.BASS_ATTRIB_VOL, Volume / 100, CrossfadeDuration)
                End If
                _CrossfadeProcHandle = Bass.BASS_ChannelSetSync(Stream, BASSSync.BASS_SYNC_POS, Bass.BASS_ChannelSeconds2Bytes(Stream, If(IsSkippingSilence AndAlso cueOutPos <> 0, cueOutPos, Length) - (CrossfadeDuration / 1000)), _CrossfadeSyncProc, IntPtr.Zero)
            End If
            If _IsLooping Then
                IsLooping = True
            Else
                OnPropertyChanged(NameOf(IsLooping))
            End If
            If IsMuted Then Volume = 0 Else If Not IsCrossfading Then Volume = _RawVolume

            If Level Is Nothing Then Level = New PeakLevels
            Level.Stream = Stream

            If VideoEffect IsNot Nothing Then
                VideoEffect.Stream = Stream
            End If

            'Notify UI
            OnPropertyChanged(NameOf(Stream))
            OnPropertyChanged(NameOf(IsPlaying))
            OnPropertyChanged(NameOf(IsPaused))
            OnPropertyChanged(NameOf(IsStalled))
            OnPropertyChanged(NameOf(IsStopped))
            'OnPropertyChanged(NameOf(IsLooping))
            'OnPropertyChanged(NameOf(Volume))            
            OnPropertyChanged(NameOf(IsMuted))
            OnPropertyChanged(NameOf(Length))
            OnPropertyChanged(NameOf(IsPlayingFromPlaylist))
        End Sub

        Public Async Sub OnMetadataChanged(value As Metadata, Optional Force As Boolean = False, Optional SkipMemoryFreeing As Boolean = False)
            If Not Force AndAlso value Is StreamMetadata Then Return
            If PreviewControlHandle IsNot Nothing AndAlso PreviewControlHandle.Tag IsNot Nothing AndAlso TypeOf PreviewControlHandle.Tag Is Metadata Then
                TryCast(PreviewControlHandle.Tag, Metadata).FreeCovers()
                PreviewControlHandle.Tag = Nothing
            End If
            If Not SkipMemoryFreeing Then StreamMetadata?.FreeCovers() : If StreamMetadata IsNot Nothing Then StreamMetadata.IsInUse = False
            _Metadata = value : _Metadata.IsInUse = True
            If value.Length = 0 Then value.Length = Bass.BASS_ChannelBytes2Seconds(Stream, Bass.BASS_ChannelGetLength(Stream, BASSMode.BASS_POS_BYTE))
            If value Is Nothing Then
                If _SMTC IsNot Nothing Then _SMTC.IsEnabled = False
                Return
            Else
                If _SMTC IsNot Nothing Then _SMTC.IsEnabled = True
            End If
            _Metadata.EnsureCovers()
            ''If _Metadata.HasCover AndAlso _Metadata.Covers Is Nothing Then
            ''    Dim Covers As New List(Of ImageSource)
            ''    Dim Tags = TagLib.File.Create(_Metadata.Path)
            ''    For Each picture In Tags.Tag.Pictures
            ''        Dim BI As New BitmapImage
            ''        BI.BeginInit()
            ''        BI.CacheOption = BitmapCacheOption.OnDemand
            ''        'BI.DecodePixelHeight = 150
            ''        'BI.DecodePixelWidth = 150
            ''        BI.StreamSource = New IO.MemoryStream(picture.Data.Data)
            ''        BI.EndInit()
            ''        Covers.Add(BI)
            ''    Next
            ''    Tags.Dispose()
            ''    _Metadata.Covers = Covers
            ''End If
            'SMTC  
            Dim corrstream = Streams.FirstOrDefault(Function(k) k.Handle = Stream)
            If corrstream IsNot Nothing Then
                corrstream.Metadata = value
            End If
            If IsUsingSMTC Then
                Dim DpAdp = GetSMTCDisplayAdapter()
                DpAdp.ClearAll()
                If IsUsingSMTCCopyFromFile Then
                    Await DpAdp.CopyFromFileAsync(MediaPlaybackType.Music, Await Windows.Storage.StorageFile.GetFileFromPathAsync(value.Path))
                Else
                    DpAdp.Type = MediaPlaybackType.Music
                    With DpAdp.MusicProperties
                        .Title = If(String.IsNullOrEmpty(_Metadata.Title), "", _Metadata.Title)
                        .Artist = If(String.IsNullOrEmpty(_Metadata.Artists?.FirstOrDefault), "", _Metadata.Artists.FirstOrDefault)
                        .AlbumTitle = If(String.IsNullOrEmpty(_Metadata.Album), "", _Metadata.Album)
                        .AlbumTitle = If(String.IsNullOrEmpty(.Artist), "", .Artist)
                        .Genres.Clear()
                        If _Metadata.Genres IsNot Nothing Then
                            For Each genre In _Metadata.Genres
                                .Genres.Add(genre)
                            Next
                        End If
                        .TrackNumber = _Metadata.Index
                    End With
                    If IO.File.Exists(_SMTC_Thumb_Path) Then
                        Try
                            IO.File.Delete(_SMTC_Thumb_Path)
                        Catch ex As Exception
                            Dim log As New HandyControl.Tools.LogMessage(HandyControl.Tools.Logger.Level.Error, "Couldn't delete the SMTC thumbnail file. " & Configuration.Exception?.ToString, Now, "QuickBeat.Player", "OnMetadataChanged", 478)
                            Console.WriteLine(log.ToString)
                        End Try
                    End If
                    If TryCast(value.DefaultCover, BitmapImage)?.StreamSource IsNot Nothing Then
                        _SMTC_Thumb_Path = IO.Path.GetTempFileName()
                        Await Utilities.SaveCoverToFile(_SMTC_Thumb_Path, TryCast(TryCast(value.DefaultCover, BitmapImage)?.StreamSource, IO.MemoryStream).ToArray)
                        DpAdp.Thumbnail = Windows.Storage.Streams.RandomAccessStreamReference.CreateFromFile(Await Windows.Storage.StorageFile.GetFileFromPathAsync(_SMTC_Thumb_Path))
                    End If
                End If
                DpAdp.Update()
            End If
#Disable Warning
            SharedProperties.Instance.Library?.IncreasePlaycountAsync(value.Path, 1)
#Enable Warning
            OnPropertyChanged(NameOf(StreamMetadata))
            RaiseEvent MetadataChanged()
        End Sub

#End Region
#Region "Handlers"

        Private Sub _SMTC_AutoRepeatModeChangeRequested(sender As SystemMediaTransportControls, args As AutoRepeatModeChangeRequestedEventArgs) Handles _SMTC.AutoRepeatModeChangeRequested
            Application.Current.Dispatcher.Invoke(Sub()
                                                      Select Case args.RequestedAutoRepeatMode
                                                          Case MediaPlaybackAutoRepeatMode.None
                                                              IsLooping = False
                                                              Playlist.IsLooping = False
                                                          Case MediaPlaybackAutoRepeatMode.List
                                                              IsLooping = False
                                                              Playlist.IsLooping = True
                                                          Case MediaPlaybackAutoRepeatMode.Track
                                                              IsLooping = True
                                                              Playlist.IsLooping = False
                                                      End Select
                                                  End Sub)
        End Sub

        Private Sub _SMTC_ButtonPressed(sender As SystemMediaTransportControls, args As SystemMediaTransportControlsButtonPressedEventArgs) Handles _SMTC.ButtonPressed
            Application.Current.Dispatcher.Invoke(Sub()
                                                      Select Case args.Button
                                                          Case SystemMediaTransportControlsButton.FastForward
                                                              Position += 10
                                                          Case SystemMediaTransportControlsButton.Next
                                                              [Next]()
                                                          Case SystemMediaTransportControlsButton.Pause
                                                              Pause()
                                                          Case SystemMediaTransportControlsButton.Play
                                                              Play()
                                                          Case SystemMediaTransportControlsButton.Previous
                                                              Previous()
                                                          Case SystemMediaTransportControlsButton.Rewind
                                                              Position -= 10
                                                          Case SystemMediaTransportControlsButton.Stop
                                                              [Stop]()
                                                      End Select
                                                  End Sub)
        End Sub

        Private Sub _SMTC_PlaybackPositionChangeRequested(sender As SystemMediaTransportControls, args As PlaybackPositionChangeRequestedEventArgs) Handles _SMTC.PlaybackPositionChangeRequested
            Application.Current.Dispatcher.Invoke(Sub() Position = args.RequestedPlaybackPosition.TotalSeconds)
        End Sub

        Private Sub _SMTC_ShuffleEnabledChangeRequested(sender As SystemMediaTransportControls, args As ShuffleEnabledChangeRequestedEventArgs) Handles _SMTC.ShuffleEnabledChangeRequested
            Application.Current.Dispatcher.Invoke(Sub() Playlist.IsShuffling = args.RequestedShuffleEnabled)
        End Sub

#End Region
#Region "Navigation"
        Sub LoadSong(Path As String, Optional CreateOnly As Boolean = False)
            If SharedProperties.Instance.IsLogging Then Utilities.DebugMode.Instance.Log(Of Player)("Attempting to load song, Path:=" & Path & "...")
            IsLoadingStream = True
            Dim nStream = Bass.BASS_StreamCreateFile(Path, 0, 0, BASSFlag.BASS_SAMPLE_FLOAT Or BASSFlag.BASS_STREAM_AUTOFREE)
            IsLoadingStream = False
            If CreateOnly Then
                OnStreamChanged(nStream)
                Return
            End If
            If nStream <> 0 Then
                If IsPreviewing Then
                    PreviewControlHandle?.Stop()
                End If
                Me.Path = Path
                If AutoStop Then Me.Stop()
                OnStreamChanged(nStream)
                OnMetadataChanged(Metadata.FromFile(Path))
                If AutoPlay Then Play()
                'Utilities.SharedProperties.Instance.Library?.IncreasePlaycount(Path, 1)
            Else
                Console.WriteLine($"Error while creating stream: {Path},{Bass.BASS_ErrorGetCode.ToString}")
                [Next]()
            End If
        End Sub

        Async Function LoadSong(Metadata As Metadata) As Task
            If SharedProperties.Instance.IsLogging Then Utilities.DebugMode.Instance.Log(Of Player)("Attempting to load song, Metadata:=" & Metadata.ToString)
            If Metadata.Location = Metadata.FileLocation.Remote Then
                _stopdownproc = False
                If RemoteTagsReading Then
                    StreamMetadata.IsInUse = False
                    Metadata.IsInUse = True
                    _Metadata = Metadata
                    _alreadyfoundtags = False
                End If
            End If
            IsLoadingStream = True
            Dim nStream As Integer = 0
            Await Task.Run(Sub()
                               nStream = If(Metadata.Location = Metadata.FileLocation.Local,
                                Bass.BASS_StreamCreateFile(Metadata.Path, 0, 0, BASSFlag.BASS_SAMPLE_FLOAT Or BASSFlag.BASS_STREAM_AUTOFREE),
                                Bass.BASS_StreamCreateURL(Metadata.Path, 0, BASSFlag.BASS_SAMPLE_FLOAT Or BASSFlag.BASS_STREAM_AUTOFREE, _HTTPDOWNLOADPROC, IntPtr.Zero))
                           End Sub)
            IsLoadingStream = False
            If nStream <> 0 Then
                If IsPreviewing Then
                    PreviewControlHandle?.Stop()
                End If
                Path = Metadata.Path
                If AutoStop Then Me.Stop()
                OnStreamChanged(nStream)
                If Metadata.Location = Metadata.FileLocation.Remote AndAlso RemoteTagsReading Then
                    OnMetadataChanged(Metadata, True, True)
                    Metadata.Length = Length
                Else
                    OnMetadataChanged(Metadata)
                End If
                If AutoPlay Then Play()
                'Utilities.SharedProperties.Instance.Library?.IncreasePlaycount(Metadata.Path, 1)
            Else
                Debug.WriteLine($"Error while creating stream: {Metadata.Location.ToString}/{Metadata.Path},{Bass.BASS_ErrorGetCode.ToString}")
                [Next]()
            End If
        End Function

        Async Sub Preview(Metadata As Metadata)
            If Metadata Is Nothing Then Return
            If SharedProperties.Instance.IsLogging Then Utilities.DebugMode.Instance.Log(Of Player)("Attempting to preview song, Metadata:=" & Metadata.ToString)
            IsLoadingStream = True
            Dim nStream As Integer = 0
            Await Task.Run(Sub()
                               nStream = If(Metadata.Location = Metadata.FileLocation.Local,
                                Bass.BASS_StreamCreateFile(Metadata.Path, 0, 0, BASSFlag.BASS_SAMPLE_FLOAT Or BASSFlag.BASS_STREAM_AUTOFREE),
                                Bass.BASS_StreamCreateURL(Metadata.Path, 0, BASSFlag.BASS_SAMPLE_FLOAT Or BASSFlag.BASS_STREAM_AUTOFREE, _HTTPDOWNLOADPROC, IntPtr.Zero))
                           End Sub)
            IsLoadingStream = False
            If nStream <> 0 Then
                Bass.BASS_ChannelSetAttribute(nStream, BASSAttribute.BASS_ATTRIB_VOL, 0)
                Bass.BASS_ChannelSlideAttribute(nStream, BASSAttribute.BASS_ATTRIB_VOL, _Volume / 100, 1000)
                If IsPreviewing Then
                    PreviewControlHandle?.Stop()
                End If
                Dim SCH As New StreamControlHandle(nStream)
                SCH.Tag = StreamMetadata
                SCH.Position = SCH.Length / 4
                AddHandler SCH.OnStop, Sub(sender)
                                           _Metadata.IsInUse = False
                                           If TryCast(sender.Tag, Metadata) IsNot Nothing Then
                                               _Metadata = TryCast(sender.Tag, Metadata)
                                               _Metadata.IsInUse = True
                                               _Metadata.EnsureCovers()
                                           End If
                                           OnPropertyChanged(NameOf(StreamMetadata))
                                           IsPreviewing = False
                                           PreviewControlHandle = Nothing
                                           Bass.BASS_StreamFree(SCH.Handle)
                                       End Sub
                IsPreviewing = True
                If IsPlaying Then Pause()
                PreviewControlHandle = SCH
                SCH.Play()
                Metadata.IsInUse = True
                _Metadata = Metadata
                Metadata.EnsureCovers()
                OnPropertyChanged(NameOf(StreamMetadata))
            Else
                IsPreviewing = False
            End If
        End Sub

        Sub Play()
            PlayCommand.Execute(Stream)
            OnPropertyChanged(NameOf(IsPlaying))
            OnPropertyChanged(NameOf(IsPaused))
            OnPropertyChanged(NameOf(IsStalled))
            OnPropertyChanged(NameOf(IsStopped))
        End Sub

        Sub Pause()
            PauseCommand.Execute(Stream)
            OnPropertyChanged(NameOf(IsPlaying))
            OnPropertyChanged(NameOf(IsPaused))
            OnPropertyChanged(NameOf(IsStalled))
            OnPropertyChanged(NameOf(IsStopped))
        End Sub

        Sub [Stop]()
            Bass.BASS_ChannelStop(Stream)
            OnPropertyChanged(NameOf(IsPlaying))
            OnPropertyChanged(NameOf(IsPaused))
            OnPropertyChanged(NameOf(IsStalled))
            OnPropertyChanged(NameOf(IsStopped))
        End Sub

        Sub [Next]()
            NextCommand.Execute(Playlist)
        End Sub

        Sub Previous()
            PreviousCommand.Execute(Playlist)
        End Sub

#End Region
#Region "Methods"
        Sub DisposeSMTC()
            _UWPPlayer.Dispose()
            _UWPPlayer = Nothing
        End Sub

        Function GetSMTCDisplayAdapter() As SystemMediaTransportControlsDisplayUpdater
            Dim smtc = GetSMTC()
            smtc.AutoRepeatMode = If(IsLooping, MediaPlaybackAutoRepeatMode.Track, If(Playlist.IsLooping, MediaPlaybackAutoRepeatMode.List, MediaPlaybackAutoRepeatMode.None))
            smtc.IsFastForwardEnabled = True
            smtc.IsNextEnabled = True
            smtc.IsPauseEnabled = True
            smtc.IsPlayEnabled = True
            smtc.IsPreviousEnabled = True
            smtc.IsRewindEnabled = True
            smtc.IsStopEnabled = True
            smtc.PlaybackRate = 1
            smtc.PlaybackStatus = If(IsPlaying, MediaPlaybackStatus.Playing, If(IsPaused, MediaPlaybackStatus.Paused, If(IsStopped, MediaPlaybackStatus.Stopped, MediaPlaybackStatus.Closed)))
            smtc.ShuffleEnabled = True
            smtc.DisplayUpdater.Update()
            Return smtc.DisplayUpdater
        End Function

        Function GetSMTC() As SystemMediaTransportControls
            If _UWPPlayer Is Nothing Then
                _UWPPlayer = New Playback.MediaPlayer
            End If
            Return _UWPPlayer.SystemMediaTransportControls
        End Function

        Sub RefreshDevices()
            Dim outdevices = Bass.BASS_GetDeviceInfos
            Me.Devices.Clear()
            For Each device In outdevices
                Me.Devices.Add(device)
            Next
        End Sub

        'Sub Load(data As Specialized.StringCollection)
        '    If SharedProperties.Instance.IsLogging Then Utilities.DebugMode.Instance.Log(Of Player)("Attempting to load data, Count:=" & data?.Count)
        '    If data IsNot Nothing AndAlso data.Count > 0 Then
        '        Configuration.SetStatus("Loading Data...", 0)
        '        Dim BinF As New System.Runtime.Serialization.Formatters.Binary.BinaryFormatter()
        '        Dim ProgStep = If(data.Count = 0, 100, 100 / data.Count)
        '        Dim i = 1
        '        For Each bin In data
        '            Dim Metadata = BinF.Deserialize(New IO.MemoryStream(Convert.FromBase64String(bin)))
        '            Playlist.Add(Metadata)
        '            Configuration.SetStatus($"{i}/{data.Count}", Configuration.Progress + ProgStep)
        '            i += 1
        '        Next
        '    Else
        '        Configuration.SetStatus("No data to be loaded", 100)
        '    End If
        '    If SharedProperties.Instance.IsLogging Then Utilities.DebugMode.Instance.Log(Of Player)("Done loading data.")
        '    Configuration.SetStatus(Configuration.Status, 100)
        'End Sub

        'Iterator Function Save() As IEnumerable(Of String)
        '    If SharedProperties.Instance.IsLogging Then Utilities.DebugMode.Instance.Log(Of Player)("Attempting to save data")
        '    Dim BinF As New System.Runtime.Serialization.Formatters.Binary.BinaryFormatter
        '    For Each metadata In Playlist
        '        Dim mem As New IO.MemoryStream
        '        BinF.Serialize(mem, metadata)
        '        Yield Convert.ToBase64String(mem.ToArray)
        '    Next
        '    If SharedProperties.Instance.IsLogging Then Utilities.DebugMode.Instance.Log(Of Player)("Done saving data.")
        'End Function

        Sub LoadPlaylist(data As IO.MemoryStream)
            If data Is Nothing Then Return
            Dim BinF As New System.Runtime.Serialization.Formatters.Binary.BinaryFormatter
            Dim Pl As Playlist = BinF.Deserialize(data)
            Pl.Cover = New BitmapImage(New Uri("\Resources\MusicRecord.png", UriKind.Relative))
            If String.IsNullOrEmpty(Pl.Description) Then Pl.Description = "Current playing queue"
            If String.IsNullOrEmpty(Pl.Category) Then Pl.Category = "Queue"
            _Playlist.Clear()
            Pl.Parent = Me
            Pl.EnsureQueueNotifier()
            Pl.RefreshItemsIndex()
            _Playlist = Pl
            OnPropertyChanged(NameOf(Playlist))
        End Sub

        Sub LoadPlaylist(playlist As Playlist)
            If playlist Is Nothing Then Return
            '_Playlist.Clear()
            If String.IsNullOrEmpty(playlist.Description) Then playlist.Description = "Current playing queue"
            If String.IsNullOrEmpty(playlist.Category) Then playlist.Category = "Queue"
            playlist.Parent = Me
            playlist.EnsureQueueNotifier()
            playlist.RefreshItemsIndex()
            _Playlist = playlist
            OnPropertyChanged(NameOf(Me.Playlist))
            Me.Playlist.Index = 0
        End Sub

        Sub LoadGroup(group As MetadataGroup)
            Dim Playlist = group.ToPlaylist()
            If String.IsNullOrEmpty(Playlist.Description) Then Playlist.Description = "Current playing queue"
            If String.IsNullOrEmpty(Playlist.Category) Then Playlist.Category = "Queue"
            Playlist.Parent = Me
            Playlist.EnsureQueueNotifier()
            Playlist.RefreshItemsIndex()
            _Playlist = Playlist
            OnPropertyChanged(NameOf(Me.Playlist))
            Me.Playlist.Index = 0
        End Sub

        Function SavePlaylist() As IO.MemoryStream
            Dim BinF As New System.Runtime.Serialization.Formatters.Binary.BinaryFormatter
            Dim PlMem As New IO.MemoryStream
            BinF.Serialize(PlMem, Playlist)
            Return PlMem
        End Function

        Sub LoadSettings(data As String)
            If String.IsNullOrEmpty(data) Then Return
            Dim SH As New SettingsHelper()
            SH.Load(data)
            If SH.ContainsKey("AutoStop") Then AutoStop = SH.GetItem("AutoStop")
            If SH.ContainsKey("CrossfadeDuration") Then CrossfadeDuration = SH.GetItem("CrossfadeDuration")
            If SH.ContainsKey("IsCrossfading") Then IsCrossfading = SH.GetItem("IsCrossfading")
            If SH.ContainsKey("IsFadingAudio") Then IsFadingAudio = SH.GetItem("IsFadingAudio")
            If SH.ContainsKey("IsLooping") Then IsLooping = SH.GetItem("IsLooping")
            If SH.ContainsKey("IsMuted") Then IsMuted = SH.GetItem("IsMuted")
            If SH.ContainsKey("IsSkippingSilence") Then IsSkippingSilence = SH.GetItem("IsSkippingSilence")
            If SH.ContainsKey("IsUsingSMTCCopyFromFile") Then IsUsingSMTCCopyFromFile = SH.GetItem("IsUsingSMTCCopyFromFile")
            If SH.ContainsKey("IsUsingSMTC") Then IsUsingSMTC = SH.GetItem("IsUsingSMTC")
            If SH.ContainsKey("RemoteTagsReading") Then RemoteTagsReading = SH.GetItem("RemoteTagsReading")
            Dim MD64 = SH.GetItem("Metadata")
            If Not String.IsNullOrEmpty(MD64) Then
                Dim BinF As New System.Runtime.Serialization.Formatters.Binary.BinaryFormatter
                Try
                    Dim Meta = BinF.Deserialize(New IO.MemoryStream(Convert.FromBase64String(MD64)))
                    '_Metadata = Meta
                    'Meta.EnsureCovers()                    
                    'OnPropertyChanged(NameOf(StreamMetadata))                                        
                    LoadSong(Meta.Path, True)
                    OnMetadataChanged(Meta)
                Catch ex As Exception
                    If SharedProperties.Instance.IsLogging Then Utilities.DebugMode.Instance.Log(Of Player)("An error occured while loading Metadata.Exception:=" & ex.Message)
                End Try
            End If
            Dim PF64 = SH.GetItem("EffectsProfile")
            If Not String.IsNullOrEmpty(PF64) Then
                Dim BinF As New System.Runtime.Serialization.Formatters.Binary.BinaryFormatter
                Try
                    Dim Profile = BinF.Deserialize(New IO.MemoryStream(Convert.FromBase64String(PF64)))
                    EffectsProfile = Profile
                Catch ex As Exception
                    If SharedProperties.Instance.IsLogging Then Utilities.DebugMode.Instance.Log(Of Player)("An error occured while loading Profile.Exception:=" & ex.Message)
                End Try
            End If
            If SH.ContainsKey("Path") Then Path = SH.GetItem("Path")
            If SH.ContainsKey("Volume") Then Volume = SH.GetItem("Volume")
            If SH.ContainsKey("TrueVolume") Then TrueVolume = SH.GetItem("TrueVolume")
            If SH.ContainsSection("Modules") Then
                For Each item In SH.GetSection("Modules")
                    Try
                        If Modules.FirstOrDefault(Function(k) k.GetType.Equals(Type.GetType(item.Value))) IsNot Nothing Then Continue For
                        Dim emodule = TryCast(Activator.CreateInstance(Type.GetType(item.Value)), EngineModule)
                        If emodule IsNot Nothing Then
                            Modules.Add(emodule)
                            emodule.Init()
                        End If
                    Catch ex As Exception
                        If SharedProperties.Instance.IsLogging Then Utilities.DebugMode.Instance.Log(Of Player)("An error occured while loading modules.Exception:=" & ex.Message)
                    End Try
                Next
            End If
        End Sub

        Function SaveSettings() As String
            Dim SH As New SettingsHelper
            SH.AddItem("AutoStop", AutoStop)
            SH.AddItem("CrossfadeDuration", CrossfadeDuration)
            SH.AddItem("IsCrossfading", IsCrossfading)
            SH.AddItem("IsFadingAudio", IsFadingAudio)
            SH.AddItem("IsLooping", IsLooping)
            SH.AddItem("IsMuted", IsMuted)
            SH.AddItem("IsSkippingSilence", IsSkippingSilence)
            SH.AddItem("IsUsingSMTC", IsUsingSMTC)
            SH.AddItem("IsUsingSMTCCopyFromFile", IsUsingSMTCCopyFromFile)
            SH.AddItem("RemoteTagsReading", RemoteTagsReading)
            Dim BinF As New System.Runtime.Serialization.Formatters.Binary.BinaryFormatter
            Dim MD64Mem As New IO.MemoryStream()
            Try
                BinF.Serialize(MD64Mem, StreamMetadata)
            Catch ex As Exception
                If SharedProperties.Instance.IsLogging Then Utilities.DebugMode.Instance.Log(Of Player)("An error occured while saving Metadata.Exception:=" & ex.Message)
            End Try
            Dim MD64 = If(MD64Mem.Length > 0, Convert.ToBase64String(MD64Mem.ToArray), Nothing)
            If Not String.IsNullOrEmpty(MD64) Then SH.AddItem("Metadata", MD64)
            Dim PF64Mem As New IO.MemoryStream()
            Try
                For Each profile In EffectsProfile
                    profile.Clean()
                Next
                BinF.Serialize(PF64Mem, EffectsProfile)
            Catch ex As Exception
                If SharedProperties.Instance.IsLogging Then Utilities.DebugMode.Instance.Log(Of Player)("An error occured while saving Profile.Exception:=" & ex.Message)
            End Try
            Dim PF64 = If(PF64Mem.Length > 0, Convert.ToBase64String(PF64Mem.ToArray), Nothing)
            If Not String.IsNullOrEmpty(PF64) Then SH.AddItem("EffectsProfile", PF64)
            SH.AddItem("Path", Path)
            SH.AddItem("Volume", Volume)
            SH.AddItem("TrueVolume", TrueVolume)
            If Modules.Count > 0 Then
                SH.StartSection("Modules")
                For i As Integer = 0 To Modules.Count - 1
                    SH.AddItem($"Module{i}", Modules(i).GetType.AssemblyQualifiedName)
                Next
                SH.EndSection()
            End If
            Return SH.Dump
        End Function


        ''' <summary>
        ''' Gets <paramref name="Bands"/> data (0.0 -> 1.0) from the current stream.
        ''' </summary>
        ''' <param name="Bands">The requested bands frequencies in Hertz</param>
        ''' <param name="FFTlength">The length of the FFT, Must be 1/2 the Data</param>
        ''' <param name="FFTdata">The FFT Data</param>
        ''' <returns>Returns A <see cref="Single()"/> containing the peak data (0.0 -> 1.0)</returns>
        Public Function GetChannelPeakData(Bands As Single(), FFTlength As Integer, FFTdata As BASSData) As Single()
            'Dim bands = 3

            'Dim bandfreq As Single() = {100, 5000, 16000}

            Dim bandlevel(Bands.Length - 1) As Single
            'Dim fft(511), freq As Single
            Dim fft(FFTlength - 1), freq As Single

            Bass.BASS_ChannelGetAttribute(Stream, BASSAttribute.BASS_ATTRIB_FREQ, freq) ' get the sample rate
            'Bass.BASS_ChannelGetData(Stream, fft, BASSData.BASS_DATA_FFT1024) ' get the FFT data (1024 sample)
            Bass.BASS_ChannelGetData(Stream, fft, FFTdata) ' get the FFT data (1024 sample)
            Dim b0 As Integer = 0
            'For x As Integer = 0 To bands - 1
            For x As Integer = 0 To Bands.Length - 1
                bandlevel(x) = 0
                Try
                    'Dim b1 As Integer = Math.Ceiling(bandfreq(x) / freq * 1024)
                    Dim b1 As Integer = Math.Ceiling(Bands(x) / freq * (FFTlength + 1))
                    If b1 > (FFTlength - 1) Then
                        b1 = (FFTlength - 1)
                    End If
                    Do While b0 < b1
                        If bandlevel(x) < fft(1 + b0) Then
                            bandlevel(x) = fft(1 + b0)
                        End If
                        b0 += 1
                    Loop
                Catch
                End Try
            Next x
            ' the bandlevel array now contains the level of each band
            Return bandlevel
        End Function

        ''' <summary>
        ''' Gets <paramref name="Bands"/> data (0.0 -> 1.0) from the current stream.
        ''' </summary>
        ''' <param name="Bands">The requested bands frequencies in Hertz</param>
        ''' <param name="FFTlength">The length of the FFT, Must be 1/2 the Data</param>
        ''' <param name="FFTdata">The FFT Data</param>
        ''' <param name="Maximum">The maximum peak a band can hold</param>
        ''' <returns>Returns A <see cref="Single()"/> containing the peak data (0.0 -> 1.0)</returns>
        Public Function GetChannelPeakData2(Bands As Single(), FFTlength As Integer, FFTdata As BASSData, Maximum As Integer) As Single()
            Dim bandlevel(Bands.Length - 1) As Single
            Dim fft(FFTlength - 1) As Single
            Dim freq As Single
            Bass.BASS_ChannelGetAttribute(Stream, BASSAttribute.BASS_ATTRIB_FREQ, freq) ' get the sample rate
            Bass.BASS_ChannelGetData(Stream, fft, FFTdata) ' get the FFT data (1024 sample)
            Dim b0 As Integer = 0
            For x As Integer = 0 To Bands.Length - 1
                bandlevel(x) = 0
                Dim b1 As Integer = CInt(Math.Truncate(Math.Ceiling(Bands(x) / freq * (FFTlength * 2))))
                If b1 > (FFTlength - 1) Then
                    b1 = (FFTlength - 1)
                End If
                Do While b0 < b1
                    If bandlevel(x) < fft(1 + b0) Then
                        bandlevel(x) = fft(1 + b0)
                    End If
                    b0 += 1
                Loop
                Dim y As Integer
                y = CInt(Math.Truncate(Math.Sqrt(bandlevel(x)) * 3 * Maximum - 4))
                If y > Maximum Then
                    y = Maximum
                End If
                If y < 0 Then
                    y = 0
                End If
                bandlevel(x) = (CByte(Math.Truncate(y)))
            Next x

            Return bandlevel
        End Function
#End Region
#Region "WPF Support"
        <NonSerialized> Private _PlayCommand As New WPF.Commands.PlayCommand
        ReadOnly Property PlayCommand As WPF.Commands.PlayCommand
            Get
                Return _PlayCommand
            End Get
        End Property
        <NonSerialized> Private _PauseCommand As New WPF.Commands.PauseCommand
        ReadOnly Property PauseCommand As WPF.Commands.PauseCommand
            Get
                Return _PauseCommand
            End Get
        End Property
        <NonSerialized> Private _PlayPauseCommand As New WPF.Commands.PlayPauseCommand
        ReadOnly Property PlayPauseCommand As WPF.Commands.PlayPauseCommand
            Get
                Return _PlayPauseCommand
            End Get
        End Property
        <NonSerialized> Private _LoadCommand As New WPF.Commands.LoadCommand(Me)
        ReadOnly Property LoadCommand As WPF.Commands.LoadCommand
            Get
                Return _LoadCommand
            End Get
        End Property
        <NonSerialized> Private _LoadURLCommand As New WPF.Commands.LoadURLCommand(Me)
        ReadOnly Property LoadURLCommand As WPF.Commands.LoadURLCommand
            Get
                Return _LoadURLCommand
            End Get
        End Property
        <NonSerialized> Private _LoadPlaylistCommand As New WPF.Commands.LoadPlaylistCommand(Me)
        ReadOnly Property LoadPlaylistCommand As WPF.Commands.LoadPlaylistCommand
            Get
                Return _LoadPlaylistCommand
            End Get
        End Property
        <NonSerialized> Private _LoadGroupCommand As New WPF.Commands.LoadGroupCommand(Me)
        ReadOnly Property LoadGroupCommand As WPF.Commands.LoadGroupCommand
            Get
                Return _LoadGroupCommand
            End Get
        End Property
        <NonSerialized> Private _LoadAndAddCommand As New WPF.Commands.LoadAndAddCommand(Me)
        ReadOnly Property LoadAndAddCommand As WPF.Commands.LoadAndAddCommand
            Get
                Return _LoadAndAddCommand
            End Get
        End Property
        <NonSerialized> Private _SetPositionCommand As New WPF.Commands.SetPositionCommand(Me)
        ReadOnly Property SetPositionCommand As WPF.Commands.SetPositionCommand
            Get
                Return _SetPositionCommand
            End Get
        End Property
        <NonSerialized> Private _MoveUpCommand As New WPF.Commands.MoveUpCommand(Me)
        ReadOnly Property MoveUpCommand As WPF.Commands.MoveUpCommand
            Get
                Return _MoveUpCommand
            End Get
        End Property
        <NonSerialized> Private _MoveDownCommand As New WPF.Commands.MoveDownCommand(Me)
        ReadOnly Property MoveDownCommand As WPF.Commands.MoveDownCommand
            Get
                Return _MoveDownCommand
            End Get
        End Property
        <NonSerialized> Private _SetACommand As New WPF.Commands.SetACommand(Me)
        ReadOnly Property SetACommand As WPF.Commands.SetACommand
            Get
                Return _SetACommand
            End Get
        End Property
        <NonSerialized> Private _SetBCommand As New WPF.Commands.SetBCommand(Me)
        ReadOnly Property SetBCommand As WPF.Commands.SetBCommand
            Get
                Return _SetBCommand
            End Get
        End Property
        <NonSerialized> Private _FFCommand As New WPF.Commands.FastForwardCommand
        ReadOnly Property FFCommand As WPF.Commands.FastForwardCommand
            Get
                Return _FFCommand
            End Get
        End Property
        <NonSerialized> Private _RWCommand As New WPF.Commands.RewindCommand
        ReadOnly Property RWCommand As WPF.Commands.RewindCommand
            Get
                Return _RWCommand
            End Get
        End Property
        <NonSerialized> Private _NextCommand As New WPF.Commands.NextCommand
        ReadOnly Property NextCommand As WPF.Commands.NextCommand
            Get
                Return _NextCommand
            End Get
        End Property
        <NonSerialized> Private _PreviousCommand As New WPF.Commands.PreviousCommand
        ReadOnly Property PreviousCommand As WPF.Commands.PreviousCommand
            Get
                Return _PreviousCommand
            End Get
        End Property
        <NonSerialized> Private _AddToPlaylistCommand As New WPF.Commands.AddToPlaylistCommand
        ReadOnly Property AddToPlaylistCommand As WPF.Commands.AddToPlaylistCommand
            Get
                Return _AddToPlaylistCommand
            End Get
        End Property
        <NonSerialized> Private _AddToQueueCommand As New WPF.Commands.AddToQueueCommand
        ReadOnly Property AddToQueueCommand As WPF.Commands.AddToQueueCommand
            Get
                Return _AddToQueueCommand
            End Get
        End Property
        <NonSerialized> Private _RemoveFromPlaylistCommand As New WPF.Commands.RemoveFromPlaylistCommand(Me)
        ReadOnly Property RemoveFromPlaylistCommand As WPF.Commands.RemoveFromPlaylistCommand
            Get
                Return _RemoveFromPlaylistCommand
            End Get
        End Property
        <NonSerialized> Private _AddAudioEffectCommand As New WPF.Commands.AddAudioEffectCommand
        ReadOnly Property AddAudioEffectCommand As WPF.Commands.AddAudioEffectCommand
            Get
                Return _AddAudioEffectCommand
            End Get
        End Property
        <NonSerialized> Private _RemoveAudioEffectCommand As New WPF.Commands.RemoveAudioEffectCommand
        ReadOnly Property RemoveAudioEffectCommand As WPF.Commands.RemoveAudioEffectCommand
            Get
                Return _RemoveAudioEffectCommand
            End Get
        End Property
        <NonSerialized> Private _ClearAudioEffectsCommand As New WPF.Commands.ClearAudioEffectsCommand
        ReadOnly Property ClearAudioEffectsCommand As WPF.Commands.ClearAudioEffectsCommand
            Get
                Return _ClearAudioEffectsCommand
            End Get
        End Property
        <NonSerialized> Private _ConfigAudioEffectCommand As New WPF.Commands.ConfigAudioEffectCommand
        ReadOnly Property ConfigAudioEffectCommand As WPF.Commands.ConfigAudioEffectCommand
            Get
                Return _ConfigAudioEffectCommand
            End Get
        End Property
        <NonSerialized> Private _AddEngineModuleCommand As New WPF.Commands.AddEngineModuleCommand
        ReadOnly Property AddEngineModuleCommand As WPF.Commands.AddEngineModuleCommand
            Get
                Return _AddEngineModuleCommand
            End Get
        End Property
        <NonSerialized> Private _RemoveEngineModuleCommand As New WPF.Commands.RemoveEngineModuleCommand
        ReadOnly Property RemoveEngineModuleCommand As WPF.Commands.RemoveEngineModuleCommand
            Get
                Return _RemoveEngineModuleCommand
            End Get
        End Property
        <NonSerialized> Private _ClearEngineModulesCommand As New WPF.Commands.ClearEngineModulesCommand
        ReadOnly Property ClearEngineModulesCommand As WPF.Commands.ClearEngineModulesCommand
            Get
                Return _ClearEngineModulesCommand
            End Get
        End Property
        <NonSerialized> Private _ConfigEngineModuleCommand As New WPF.Commands.ConfigEngineModuleCommand
        ReadOnly Property ConfigEngineModuleCommand As WPF.Commands.ConfigEngineModuleCommand
            Get
                Return _ConfigEngineModuleCommand
            End Get
        End Property
        <NonSerialized> Private _ShowMetadataInfoCommand As New WPF.Commands.ShowMetadataInfoCommand
        ReadOnly Property ShowMetadataInfoCommand As WPF.Commands.ShowMetadataInfoCommand
            Get
                Return _ShowMetadataInfoCommand
            End Get
        End Property
        <NonSerialized> Private _SavePlaylistCommand As New WPF.Commands.SavePlaylistCommand
        ReadOnly Property SavePlaylistCommand As WPF.Commands.SavePlaylistCommand
            Get
                Return _SavePlaylistCommand
            End Get
        End Property
        <NonSerialized> Private _ChangePlaylistNameCommand As New WPF.Commands.ChangePlaylistNameCommand
        ReadOnly Property ChangePlaylistNameCommand As WPF.Commands.ChangePlaylistNameCommand
            Get
                Return _ChangePlaylistNameCommand
            End Get
        End Property
        <NonSerialized> Private _RefreshMetadataCoverCommand As New WPF.Commands.RefreshMetadataCoverCommand
        ReadOnly Property RefreshMetadataCoverCommand As WPF.Commands.RefreshMetadataCoverCommand
            Get
                Return _RefreshMetadataCoverCommand
            End Get
        End Property
        <NonSerialized> Private _AddToPlaylistFromPickerCommand As New WPF.Commands.AddToPlaylistFromPickerCommand()
        ReadOnly Property AddToPlaylistFromPickerCommand As WPF.Commands.AddToPlaylistFromPickerCommand
            Get
                Return _AddToPlaylistFromPickerCommand
            End Get
        End Property
        <NonSerialized> Private _ClearPlaylistCommand As New WPF.Commands.ClearPlaylistCommand
        ReadOnly Property ClearPlaylistCommand As WPF.Commands.ClearPlaylistCommand
            Get
                Return _ClearPlaylistCommand
            End Get
        End Property
        <NonSerialized> Private _NewPlaylistCommand As New WPF.Commands.NewPlaylistCommand(Me)
        ReadOnly Property NewPlaylistCommand As WPF.Commands.NewPlaylistCommand
            Get
                Return _NewPlaylistCommand
            End Get
        End Property
        <NonSerialized> Private _StopControlHandleCommand As New WPF.Commands.StopControlHandleCommand()
        ReadOnly Property StopControlHandleCommand As WPF.Commands.StopControlHandleCommand
            Get
                Return _StopControlHandleCommand
            End Get
        End Property
        <NonSerialized> Private _PreviewCommand As New WPF.Commands.PreviewCommand(Me)
        ReadOnly Property PreviewCommand As WPF.Commands.PreviewCommand
            Get
                Return _PreviewCommand
            End Get
        End Property
        <NonSerialized> Private _SwitchStreamCommand As New WPF.Commands.SwitchStreamCommand(Me)
        ReadOnly Property SwitchStreamCommand As WPF.Commands.SwitchStreamCommand
            Get
                Return _SwitchStreamCommand
            End Get
        End Property
        <NonSerialized> Private _SyncWithPlaylistCommand As New WPF.Commands.SyncWithPlaylistCommand(Me)
        ReadOnly Property SyncWithPlaylistCommand As WPF.Commands.SyncWithPlaylistCommand
            Get
                Return _SyncWithPlaylistCommand
            End Get
        End Property
        <NonSerialized> Private _RefreshDevicesCommand As New WPF.Commands.RefreshDevicesCommand()
        ReadOnly Property RefreshDevicesCommand As WPF.Commands.RefreshDevicesCommand
            Get
                Return _RefreshDevicesCommand
            End Get
        End Property
        <NonSerialized> Private _EnqueueGroupCommand As New WPF.Commands.EnqueueGroupCommand(Me)
        ReadOnly Property EnqueueGroupCommand As WPF.Commands.EnqueueGroupCommand
            Get
                Return _EnqueueGroupCommand
            End Get
        End Property
        <NonSerialized> Private _AddGroupToPlaylistCommand As New WPF.Commands.AddGroupToPlaylistCommand(Me)
        ReadOnly Property AddGroupToPlaylistCommand As WPF.Commands.AddGroupToPlaylistCommand
            Get
                Return _AddGroupToPlaylistCommand
            End Get
        End Property

        ReadOnly Property TaskbarState As Shell.TaskbarItemProgressState
            Get
                Return If(IsPlaying, Shell.TaskbarItemProgressState.Normal, If(IsPaused, Shell.TaskbarItemProgressState.Paused, If(IsStalled, Shell.TaskbarItemProgressState.Error, Shell.TaskbarItemProgressState.None)))
            End Get
        End Property
        ReadOnly Property TaskbarProgress As Double
            Get
                Return Position / Length
            End Get
        End Property
        <NonSerialized> Private _TaskbarOverlayPlaying As ImageSource
        Property TaskbarOverlayPlaying As ImageSource
            Get
                Return _TaskbarOverlayPlaying
            End Get
            Set(value As ImageSource)
                _TaskbarOverlayPlaying = value
                OnPropertyChanged()
            End Set
        End Property
        <NonSerialized> Private _TaskbarOverlayPaused As ImageSource
        Property TaskbarOverlayPaused As ImageSource
            Get
                Return _TaskbarOverlayPaused
            End Get
            Set(value As ImageSource)
                _TaskbarOverlayPaused = value
                OnPropertyChanged()
            End Set
        End Property
        <NonSerialized> Private _TaskbarOverlayStopped As ImageSource
        Property TaskbarOverlayStopped As ImageSource
            Get
                Return _TaskbarOverlayStopped
            End Get
            Set(value As ImageSource)
                _TaskbarOverlayStopped = value
                OnPropertyChanged()
            End Set
        End Property
        ReadOnly Property TaskbarOverlay As ImageSource
            Get
                Select Case TaskbarState
                    Case Shell.TaskbarItemProgressState.Normal
                        Return TaskbarOverlayPlaying
                    Case Shell.TaskbarItemProgressState.Paused
                        Return TaskbarOverlayPaused
                    Case Shell.TaskbarItemProgressState.Error
                        Return TaskbarOverlayStopped
                End Select
                Return Nothing
            End Get
        End Property

#End Region
    End Class
End Namespace
