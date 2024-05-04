Imports System.ComponentModel
Imports HandyControl.Tools.Interop.InteropValues
Imports UPNPLib

Namespace UPnP
    Public Class MediaRenderer
        Implements ComponentModel.INotifyPropertyChanged, IDisposable

        Public MediaRendererDevice As UPnPDevice = Nothing

        Private WithEvents AVTransport As AVTransportService = Nothing
        Public RendererControl As RendererControlService = Nothing

        Private WithEvents _Timer_Progress As New Forms.Timer With {.Interval = 1000}
        Private WithEvents _Timer_State As New Forms.Timer With {.Interval = 5000} 'Low freq. timer, so we don't exhaust the upnp lib                

        Private _Info As UPnPInfo
        Public Overridable Property Info As UPnPInfo
            Get
                Return _Info
            End Get
            Set(value As UPnPInfo)
                _Info = value
                OnPropertyChanged()
            End Set
        End Property

        Private _IsBusy As Boolean
        Property IsBusy As Boolean
            Get
                Return _IsBusy
            End Get
            Set(value As Boolean)
                _IsBusy = value
                OnPropertyChanged()
            End Set
        End Property

        Sub New()

        End Sub
        Public Sub New(ByVal MediaRendererDevice As UPnPDevice)
            Info = New UPnPInfo(MediaRendererDevice)
            Me.MediaRendererDevice = MediaRendererDevice
            For Each myService As UPnPService In MediaRendererDevice.Services
                If myService.ServiceTypeIdentifier = "urn:schemas-upnp-org:service:AVTransport:1" Then
                    AVTransport = New AVTransportService(myService)
                    PlaybackControlCommand = New DelegatePlaybackControlCommand(Me)
                    SetAVUriCommand = New DelegateSetAVUriCommand(Me)
                    Exit For 'end the for-loop
                End If
            Next
            If AVTransport Is Nothing Then
                Throw New InvalidOperationException("Couldn't find a valid renderer.")
            End If
        End Sub

        Public Sub ForceInfoPass()
            OnStateRequested()
        End Sub

        Public Event CombinedErrorCountReachedThreshold()
        Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged
        Public Sub OnPropertyChanged(<Runtime.CompilerServices.CallerMemberName> Optional CallerName As String = Nothing)
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(CallerName))
        End Sub

        Private Sub AVTransport_CombinedErrorCountReachedThreshold() Handles AVTransport.CombinedErrorCountReachedThreshold
            RaiseEvent CombinedErrorCountReachedThreshold()
        End Sub

        'ServiceTypeIdentifier = urn:schemas-upnp-org:service:AVTransport:1
        Public Class AVTransportService
            Public Structure PositionInfo
                Public Track As UInt32
                Public TrackDuration As String
                Public TrackMetaData As String
                Public TrackURI As String
                Public RelativeTime As String
                Public AbsoluteTime As String
                Public RelativeCount As Integer
                Public AbsoluteCount As Integer
            End Structure

            Public Enum TransportStatusEnum
                ERROR_OCCURRED
                STOPPED
                OK
            End Enum

            Public Enum TransportStateEnum
                STOPPED 'always supported
                PLAYING 'always supported
                TRANSITIONING
                PAUSED_PLAYBACK
                PAUSED_RECORDING
                RECORDING
                NO_MEDIA_PRESENT
            End Enum

            Public Structure TransportInfo
                Public CurrentTransportState As TransportStateEnum
                Public CurrentTransportStatus As TransportStatusEnum
                Public CurrentSpeed As String
            End Structure

            Public Enum SeekMode
                TRACK_NR
                ABS_TIME
                REL_TIME
                ABS_COUNT
                REL_COUNT
                CHANNEL_FREQ
                TAPE_INDEX
                REL_TAPE_INDEX
                FRAME
                REL_FRAME
            End Enum

            Public Event CombinedErrorCountReachedThreshold()

            Dim AVTransportService As UPnPService = Nothing
            Public Property CombinedErrorThreshold As Integer = 20
            Private _CombinedErrorCount As Integer = 0
            Public Property CombinedErrorCount As Integer
                Get
                    Return _CombinedErrorCount
                End Get
                Set(value As Integer)
                    _CombinedErrorCount = value
                    If value >= CombinedErrorThreshold Then
                        RaiseEvent CombinedErrorCountReachedThreshold()
                    End If
                End Set
            End Property

            Public Sub New(ByRef AVTransportService As UPnPService)
                Me.AVTransportService = AVTransportService
            End Sub

            Public Sub SetAVTransportURI(ByVal InstanceID As UInt32, ByVal CurrentURI As String, ByVal CurrentURIMetaData As String)
                If AVTransportService Is Nothing Then Exit Sub

                Dim inObject(2) As Object
                Dim outObject As Object = Nothing

                inObject.SetValue(0, 0) 'Instance ID
                inObject.SetValue(CurrentURI, 1) 'CurrentURI
                inObject.SetValue(CurrentURIMetaData, 2) 'CurrentURIMetadata

                Try
                    Me.AVTransportService.InvokeAction("SetAVTransportURI", inObject, outObject)
                Catch ex As Exception
                    CombinedErrorCount += 1
                    Debug.WriteLine("Error in AVTransportService:SetAVTransportURI :" & ex.ToString)
                End Try
            End Sub

            Public Function GetPositionInfo(ByVal InstanceID As UInt32) As PositionInfo
                If AVTransportService Is Nothing Then Return Nothing

                Dim tmpPosInfo As New PositionInfo
                Dim inObject(0) As Object
                Dim outObject(7) As Object

                inObject.SetValue(InstanceID, 0) 'Instance ID

                Try
                    Me.AVTransportService.InvokeAction("GetPositionInfo", inObject, outObject)
                    tmpPosInfo.Track = CUInt(outObject(0))
                    tmpPosInfo.TrackDuration = CStr(outObject(1))
                    tmpPosInfo.TrackMetaData = CStr(outObject(2))
                    tmpPosInfo.TrackURI = CStr(outObject(3))
                    tmpPosInfo.RelativeTime = CStr(outObject(4))
                    tmpPosInfo.AbsoluteTime = CStr(outObject(5))
                    tmpPosInfo.RelativeCount = CInt(outObject(6))
                    tmpPosInfo.AbsoluteCount = CInt(outObject(7))
                Catch ex As Exception
                    CombinedErrorCount += 1
                    Debug.WriteLine("Error in AVTransportService:GetPositionInfo :" & ex.ToString)
                End Try

                Return tmpPosInfo
            End Function

            Public Function GetTransportInfo(ByVal InstanceID As UInt32) As TransportInfo
                If AVTransportService Is Nothing Then Return Nothing

                Dim tmpTransInfo As New TransportInfo
                Dim inObject(0) As Object
                Dim outObject(2) As Object

                inObject.SetValue(InstanceID, 0) 'Instance ID

                Try
                    Me.AVTransportService.InvokeAction("GetTransportInfo", inObject, outObject)

                    tmpTransInfo.CurrentTransportState = [Enum].Parse(GetType(TransportStateEnum), CStr(outObject(0)).ToUpper)
                    tmpTransInfo.CurrentTransportStatus = [Enum].Parse(GetType(TransportStatusEnum), CStr(outObject(1)).ToUpper)
                    tmpTransInfo.CurrentSpeed = CStr(outObject(2))
                Catch ex As Exception
                    CombinedErrorCount += 1
                    Debug.WriteLine("Error in AVTransportService:GetTransportInfo :" & ex.ToString)
                End Try

                Return tmpTransInfo
            End Function

            Public Sub Seek(ByVal InstanceID As UInt32, ByVal Unit As String, ByVal Target As String)
                If AVTransportService Is Nothing Then Exit Sub

                Dim inObject(2) As Object
                Dim outObject As Object = Nothing

                inObject.SetValue(0, 0) 'Instance ID
                inObject.SetValue(Unit, 1) 'Unit
                inObject.SetValue(Target, 2) 'Target

                Try
                    Me.AVTransportService.InvokeAction("Seek", inObject, outObject)
                Catch ex As Exception
                    CombinedErrorCount += 1
                    Debug.WriteLine("Error in AVTransportService:Seek :" & ex.ToString)
                End Try
            End Sub

            Public Sub [Stop](ByVal InstanceID As UInt32)
                If AVTransportService Is Nothing Then Exit Sub

                Dim inObject(0) As Object
                Dim outObject As Object = Nothing

                inObject.SetValue(InstanceID, 0) 'Instance ID

                Try
                    Me.AVTransportService.InvokeAction("Stop", inObject, outObject)
                Catch ex As Exception
                    CombinedErrorCount += 1
                    Debug.WriteLine("Error in AVTransportService:Stop :" & ex.ToString)
                End Try
            End Sub

            Public Sub [Next](ByVal InstanceID As UInt32)
                If AVTransportService Is Nothing Then Exit Sub

                Dim inObject(0) As Object
                Dim outObject As Object = Nothing

                inObject.SetValue(InstanceID, 0) 'Instance ID

                Try
                    Me.AVTransportService.InvokeAction("Next", inObject, outObject)
                Catch ex As Exception
                    CombinedErrorCount += 1
                    Debug.WriteLine("Error in AVTransportService:Next :" & ex.ToString)
                End Try
            End Sub

            Public Sub [Previous](ByVal InstanceID As UInt32)
                If AVTransportService Is Nothing Then Exit Sub

                Dim inObject(0) As Object
                Dim outObject As Object = Nothing

                inObject.SetValue(InstanceID, 0) 'Instance ID

                Try
                    Me.AVTransportService.InvokeAction("Previous", inObject, outObject)
                Catch ex As Exception
                    CombinedErrorCount += 1
                    Debug.WriteLine("Error in AVTransportService:Previous :" & ex.ToString)
                End Try
            End Sub

            ' ''<summary>
            ' ''Starts playing the CurrentURI
            ' ''</summary>
            ' ''<value>Age of the claimant.</value>
            ' ''<param name="InstanceID">InstanceID, 0 by default</param>
            ' ''<param name="TransportPlaySpeed">String representing the playbackspeed. "1" is normal playback (always supported), "1/10" is slow playback (not always supported, check allowed values), "-1" is reverse playback (not always supported, check allowed values),...</param>
            Public Sub Play(ByVal InstanceID As UInt32, ByVal TransportPlaySpeed As String)
                If AVTransportService Is Nothing Then Exit Sub

                Dim inObject(1) As Object
                Dim outObject As Object = Nothing

                inObject.SetValue(InstanceID, 0) 'Instance ID
                inObject.SetValue(TransportPlaySpeed, 1) 'TransportPlaySpeed: "1", "1/10", "-1"

                Try
                    Me.AVTransportService.InvokeAction("Play", inObject, outObject)
                Catch ex As Exception
                    CombinedErrorCount += 1
                    Debug.WriteLine("Error in AVTransportService:Play :" & ex.ToString)
                End Try
            End Sub

            Public Sub Pause(ByVal InstanceID As UInt32)
                If AVTransportService Is Nothing Then Exit Sub

                Dim inObject(0) As Object
                Dim outObject As Object = Nothing

                inObject.SetValue(InstanceID, 0) 'Instance ID

                Try
                    Me.AVTransportService.InvokeAction("Pause", inObject, outObject)
                Catch ex As Exception
                    CombinedErrorCount += 1
                    Debug.WriteLine("Error in AVTransportService:Pause :" & ex.ToString)
                End Try
            End Sub

            'sub for convenience
            Public Sub Play()
                Me.Play(CUInt(0), "1")
            End Sub

            'sub for convenience
            Public Sub Pause()
                Me.Pause(CUInt(0))
            End Sub

            'sub for convenience
            Public Sub Play(ByVal urlToPlay As String)
                Me.SetAVTransportURI(urlToPlay)
                Me.Play(CUInt(0), "1")
            End Sub

            'sub for convenience
            Public Sub SetAVTransportURI(ByVal CurrentURI As String)
                Me.SetAVTransportURI(CUInt(0), CurrentURI, "")
            End Sub

            'sub for convenience
            Public Sub [Stop]()
                Me.Stop(CUInt(0))
            End Sub

            'sub for convenience
            Public Sub [Next]()
                Me.Next(CUInt(0))
            End Sub

            'sub for convenience
            Public Sub [Previous]()
                Me.Previous(CUInt(0))
            End Sub

            'sub for convenience
            Public Function GetTransportInfo() As TransportInfo
                Return Me.GetTransportInfo(CUInt(0))
            End Function

            'sub for convenience
            Public Function GetPositionInfo() As PositionInfo
                Return GetPositionInfo(CUInt(0))
            End Function

            'sub for convenience
            ''' <summary>
            ''' Seeks to current position from beginning
            ''' </summary>
            ''' <param name="Position">Position in seconds</param>
            Public Sub Seek(Position As Integer)
                Dim ts = TimeSpan.FromSeconds(Position)
                Seek(CUInt(0), "REL_TIME", ts.ToString("hh':'mm':'ss"))
            End Sub

            'TransportPlaySpeed Info
            '-----------------------
            'String representation of a rational fraction, indicates the speed relative to normal speed. 
            'Example values are ‘1’, ‘1/2’, ‘2’, ‘-1’, ‘1/10’, etc. 
            'Actually supported speeds can be retrieved from the AllowedValueList of this state variable in the AVTransport service description. 
            'Value ‘1’ is required, value ‘0’ is not allowed.
            Public ReadOnly Property TransportPlaySpeed() As String
                Get
                    Try
                        Return Me.AVTransportService.QueryStateVariable("TransportPlaySpeed")
                    Catch ex As Exception
                        CombinedErrorCount += 1
                        Debug.WriteLine("Error in QueryStateVariable:TransportPlaySpeed :" & ex.ToString)
                    End Try
                    Return "1"
                End Get
            End Property

            Public ReadOnly Property TransportState() As TransportStateEnum
                Get
                    Try
                        Return [Enum].Parse(GetType(TransportStateEnum), CStr(Me.AVTransportService.QueryStateVariable("TransportState")))
                    Catch ex As Exception
                        CombinedErrorCount += 1
                        Return TransportStateEnum.PLAYING
                    End Try
                End Get
            End Property

            Public ReadOnly Property TransportStatus() As TransportStatusEnum
                Get
                    Try
                        Return [Enum].Parse(GetType(TransportStatusEnum), CStr(Me.AVTransportService.QueryStateVariable("TransportStatus")))
                    Catch ex As Exception
                        CombinedErrorCount += 1
                        Debug.WriteLine("Error in QueryStateVariable:TransportStatus :" & ex.ToString)
                    End Try
                    Return TransportStatusEnum.ERROR_OCCURRED
                End Get
            End Property

            Public ReadOnly Property CurrentPlayMode() As String
                Get
                    Try
                        Return Me.AVTransportService.QueryStateVariable("CurrentPlayMode")
                    Catch ex As Exception
                        CombinedErrorCount += 1
                        Debug.WriteLine("Error in QueryStateVariable:CurrentPlayMode :" & ex.ToString)
                    End Try
                    Return "ERROR"
                End Get
            End Property

            Public ReadOnly Property RelativeTimePosition() As String
                Get
                    Try
                        Return CStr(Me.AVTransportService.QueryStateVariable("RelativeTimePosition"))
                    Catch ex As Exception
                        CombinedErrorCount += 1
                        Debug.WriteLine("Error in QueryStateVariable:RelativeTimePosition :" & ex.ToString)
                    End Try
                    Return "00:00:00"
                End Get
            End Property
        End Class

        'ServiceID = urn:upnp-org:serviceId:RenderingControlServiceID
        Public Class RendererControlService
            Dim RendererControlService As UPnPService = Nothing

            Public Sub New(ByRef RendererControlService As UPnPService)
                Me.RendererControlService = RendererControlService
            End Sub
        End Class

#Region "Proxy Methods"
        Public Overridable Sub Play()
            AVTransport?.Play()
        End Sub
        Public Overridable Sub Pause()
            AVTransport?.Pause()
        End Sub
        Public Overridable Sub [Stop]()
            AVTransport?.Stop()
        End Sub
        Public Overridable Sub Seek(Position As Integer)
            AVTransport?.Seek(Position)
        End Sub
        Public Overridable Sub [Next]()
            AVTransport?.Next()
        End Sub
        Public Overridable Sub Previous()
            AVTransport?.Previous()
        End Sub
        Public Overridable Sub SetAVTransportURI(InstanceID As UInt32, CurrentURI As String, CurrentURIMetaData As String)
            AVTransport?.SetAVTransportURI(InstanceID, CurrentURI, CurrentURIMetaData)
        End Sub
        Protected Sub SetDuration(Duration As Double)
            _Duration = Duration
            OnPropertyChanged(NameOf(Duration))
            OnPropertyChanged(NameOf(DurationString))
        End Sub
        Protected Sub SetPosition(Position As Integer)
            _Position = Position
            OnPropertyChanged(NameOf(Position))
            OnPropertyChanged(NameOf(PositionString))
        End Sub
        Protected Sub SetIsPlaying(Value As Boolean)
            _IsPlaying = Value
            OnPropertyChanged(NameOf(IsPlaying))
        End Sub
        Protected Sub SetIsPaused(Value As Boolean)
            _IsPaused = Value
            OnPropertyChanged(NameOf(IsPaused))
        End Sub
        Protected Sub SetIsStopped(Value As Boolean)
            _IsStopped = Value
            OnPropertyChanged(NameOf(IsStopped))
        End Sub
#End Region
#Region "Info Properties"
        Private _IsPlaying As Boolean
        Property IsPlaying As Boolean
            Get
                Return _IsPlaying
            End Get
            Set(value As Boolean)
                _IsPlaying = value
                If value Then
                    Play()
                Else
                    Pause()
                End If
                OnPropertyChanged()
            End Set
        End Property
        Private _IsPaused As Boolean
        Property IsPaused As Boolean
            Get
                Return _IsPaused
            End Get
            Set(value As Boolean)
                _IsPaused = value
                If value Then
                    Pause()
                Else
                    Play()
                End If
                OnPropertyChanged()
            End Set
        End Property
        Private _IsStopped As Boolean
        Property IsStopped As Boolean
            Get
                Return _IsStopped
            End Get
            Set(value As Boolean)
                _IsStopped = value
                If value Then
                    [Stop]()
                Else
                    Play()
                End If
                OnPropertyChanged()
            End Set
        End Property
        Private _Position As Double
        Property Position As Double
            Get
                Return _Position
            End Get
            Set(value As Double)
                _Position = value
                Seek(CUInt(value))
                OnPropertyChanged()
                OnPropertyChanged(NameOf(PositionString))
            End Set
        End Property
        ReadOnly Property PositionString As String
            Get
                Dim ts = TimeSpan.FromSeconds(Position)
                Return $"{If(ts.Minutes < 10, 0, "")}{ts.Minutes}:{If(ts.Seconds < 10, 0, "")}{ts.Seconds}"
            End Get
        End Property
        Private _Duration As Double
        ReadOnly Property Duration As Double
            Get
                Return _Duration
            End Get
        End Property
        ReadOnly Property DurationString As String
            Get
                Dim ts = TimeSpan.FromSeconds(Duration)
                Return $"{If(ts.Minutes < 10, 0, "")}{ts.Minutes}:{If(ts.Seconds < 10, 0, "")}{ts.Seconds}"
            End Get
        End Property

        Private _CurrentTrackTitle As String
        Property CurrentTrackTitle As String
            Get
                Return _CurrentTrackTitle
            End Get
            Set(value As String)
                _CurrentTrackTitle = value
                OnPropertyChanged()
            End Set
        End Property

        Private _CurrentTrackArtist As String
        Property CurrentTrackArtist As String
            Get
                Return _CurrentTrackArtist
            End Get
            Set(value As String)
                _CurrentTrackArtist = value
                OnPropertyChanged()
            End Set
        End Property

        Private _CurrentTrackAlbum As String
        Property CurrentTrackAlbum As String
            Get
                Return _CurrentTrackAlbum
            End Get
            Set(value As String)
                _CurrentTrackAlbum = value
                OnPropertyChanged()
            End Set
        End Property

        Private _CurrentTrackGenre As String
        Property CurrentTrackGenre As String
            Get
                Return _CurrentTrackGenre
            End Get
            Set(value As String)
                _CurrentTrackGenre = value
                OnPropertyChanged()
            End Set
        End Property

        Private _PlaybackControlCommand As DelegatePlaybackControlCommand
        Public Property PlaybackControlCommand As DelegatePlaybackControlCommand
            Get
                Return _PlaybackControlCommand
            End Get
            Set(value As DelegatePlaybackControlCommand)
                _PlaybackControlCommand = value
                OnPropertyChanged()
            End Set
        End Property
        Private _SetAVUriCommand As DelegateSetAVUriCommand
        Public Property SetAVUriCommand As DelegateSetAVUriCommand
            Get
                Return _SetAVUriCommand
            End Get
            Set(value As DelegateSetAVUriCommand)
                _SetAVUriCommand = value
                OnPropertyChanged()
            End Set
        End Property
#End Region
#Region "State Monitor"
        Public Overridable Sub StartMonitoring()
            _Timer_State.Start()
        End Sub
        Protected Sub StartProgressMonitoring()
            _Timer_Progress.Start()
        End Sub
        Protected Sub StopProgressMonitoring()
            _Timer_Progress.Stop()
        End Sub
        Public Overridable Sub StopMonitoring()
            _Timer_State.Stop()
            _Timer_Progress.Stop()
        End Sub
        Private Sub _Timer_Progress_Tick(sender As Object, e As EventArgs) Handles _Timer_Progress.Tick
            Dim sw As New Stopwatch : sw.Start()
            OnProgressRequested()
            sw.Stop()
            If sw.ElapsedMilliseconds > 10000 Then
                StopProgressMonitoring()
                Debug.WriteLine("Extreme Progress Latency on " & Info.ToString & ", Progress Monitoring Stopped.")
            ElseIf sw.ElapsedMilliseconds > 5000 Then
                _Timer_Progress.Interval = 5000
                Debug.WriteLine("High Progress Latency on " & Info.ToString & ", Interval Increased to 5000.")
            Else
                If _Timer_Progress.Interval <> 1000 Then
                    _Timer_Progress.Interval = 1000
                    Debug.WriteLine("Normal Progress Latency on " & Info.ToString & ", Interval Decreased to 1000.")
                End If
            End If
        End Sub
        Private Sub _Timer_State_Tick(sender As Object, e As EventArgs) Handles _Timer_State.Tick
            Dim sw As New Stopwatch : sw.Start()
            OnStateRequested()
            SW.Stop()
            If sw.ElapsedMilliseconds > 10000 Then
                StopProgressMonitoring()
                Debug.WriteLine("Extreme State Latency on " & Info.ToString & ", State Monitoring Stopped.")
            ElseIf sw.ElapsedMilliseconds > 5000 Then
                _Timer_State.Interval = 10000
                Debug.WriteLine("High State Latency on " & Info.ToString & ", Interval Increased to 10000.")
            Else
                If _Timer_State.Interval <> 5000 Then
                    _Timer_State.Interval = 5000
                    Debug.WriteLine("Normal State Latency on " & Info.ToString & ", Interval Decreased to 5000.")
                End If
            End If
        End Sub
        Protected Overridable Sub OnProgressRequested()
            Dim pos As Date = Date.MinValue
            Try
                If Date.TryParse(AVTransport.RelativeTimePosition, pos) Then
                    _Position = pos.Hour * 3600 + pos.Minute * 60 + pos.Second
                    OnPropertyChanged(NameOf(Position))
                    OnPropertyChanged(NameOf(PositionString))
                End If
            Catch ex As Exception
                Utilities.DebugMode.Instance.Log(Of MediaRenderer)(ex.ToString)
            End Try
        End Sub
        Protected Overridable Sub OnStateRequested()
            Select Case AVTransport.TransportState
                Case AVTransportService.TransportStateEnum.PLAYING
                    _IsPlaying = True
                    _IsPaused = False
                    _IsStopped = False
                    OnPropertyChanged(NameOf(IsPlaying))
                    OnPropertyChanged(NameOf(IsPaused))
                    OnPropertyChanged(NameOf(IsStopped))
                    _Timer_Progress.Start()
                    Dim dur As Date = Date.MinValue
                    If Date.TryParse(AVTransport.GetPositionInfo.TrackDuration, dur) Then
                        _Duration = dur.Hour * 3600 + dur.Minute * 60 + dur.Second
                        OnPropertyChanged(NameOf(Duration))
                        OnPropertyChanged(NameOf(DurationString))
                    End If
                    Dim title = AVTransport.GetPositionInfo
                    If Not String.IsNullOrEmpty(title.TrackMetaData) AndAlso title.TrackMetaData <> "NOT_IMPLEMENTED" Then
                        Try
                            Dim xdoc As XDocument = XDocument.Parse(title.TrackMetaData)
                            Dim obj = TryCast(ContentDirectory.CD_Object.Create_CD_Object(If(xdoc.Root.Name.LocalName = "DIDL-Lite", xdoc.Root.FirstNode, xdoc.Root)), ContentDirectory.CD_Item)
                            If obj IsNot Nothing Then
                                CurrentTrackTitle = obj.Title
                                CurrentTrackArtist = obj.Artist
                                CurrentTrackAlbum = obj.Album
                                CurrentTrackGenre = obj.Genre
                            End If
                        Catch ex As Exception
                            Utilities.DebugMode.Instance.Log(Of MediaRenderer)(ex.ToString)
                        End Try
                    End If
                Case AVTransportService.TransportStateEnum.PAUSED_PLAYBACK
                    _IsPlaying = False
                    _IsPaused = True
                    _IsStopped = False
                    OnPropertyChanged(NameOf(IsPlaying))
                    OnPropertyChanged(NameOf(IsPaused))
                    OnPropertyChanged(NameOf(IsStopped))
                    _Timer_Progress.Stop()
                    If Duration = 0 Then
                        Dim dur As Date = Date.MinValue
                        If Date.TryParse(AVTransport.GetPositionInfo.TrackDuration, dur) Then
                            _Duration = dur.Hour * 3600 + dur.Minute * 60 + dur.Second
                            OnPropertyChanged(NameOf(Duration))
                            OnPropertyChanged(NameOf(DurationString))
                        End If
                    End If
                Case AVTransportService.TransportStateEnum.STOPPED
                    _IsPlaying = False
                    _IsPaused = False
                    _IsStopped = True
                    OnPropertyChanged(NameOf(IsPlaying))
                    OnPropertyChanged(NameOf(IsPaused))
                    OnPropertyChanged(NameOf(IsStopped))
                    _Timer_Progress.Stop()
            End Select
        End Sub
#End Region
#Region "WPF Commands Support"
        Public Class DelegatePlaybackControlCommand
            Implements ICommand

            Public Event CanExecuteChanged As EventHandler Implements ICommand.CanExecuteChanged
            Private _Renderer As MediaRenderer

            Sub New(Renderer As MediaRenderer)
                _Renderer = Renderer
                AddHandler CommandManager.RequerySuggested, New EventHandler(Sub(sender As Object, e As EventArgs)
                                                                                 RaiseEvent CanExecuteChanged(Me, New EventArgs())
                                                                             End Sub)
            End Sub

            Public Async Sub Execute(parameter As Object) Implements ICommand.Execute
                Select Case parameter?.ToString.ToLower
                    Case "play"
                        _Renderer.Play()
                    Case "pause"
                        _Renderer.Pause()
                    Case "playpause"
                        If _Renderer.IsPlaying Then
                            _Renderer.Pause()
                        Else
                            _Renderer.Play()
                        End If
                    Case "stop"
                        _Renderer.Stop()
                    Case "next"
                        _Renderer.Next()
                    Case "previous"
                        _Renderer.Previous()
                    Case "ff"
                        _Renderer.Seek(_Renderer.Position + 10)
                    Case "rw"
                        Dim rwPos = _Renderer.Position - 10
                        _Renderer.Seek(If(rwPos < 0, 0, rwPos))
                End Select
                Await Task.Delay(500)
                _Renderer.ForceInfoPass()
            End Sub

            Public Function CanExecute(parameter As Object) As Boolean Implements ICommand.CanExecute
                Return _Renderer IsNot Nothing AndAlso parameter IsNot Nothing
            End Function
        End Class
        Public Class DelegateSetAVUriCommand
            Implements ICommand

            Public Event CanExecuteChanged As EventHandler Implements ICommand.CanExecuteChanged
            Private _Renderer As MediaRenderer

            Sub New(Renderer As MediaRenderer)
                _Renderer = Renderer
                AddHandler CommandManager.RequerySuggested, New EventHandler(Sub(sender As Object, e As EventArgs)
                                                                                 RaiseEvent CanExecuteChanged(Me, New EventArgs())
                                                                             End Sub)
            End Sub

            Public Async Sub Execute(parameter As Object) Implements ICommand.Execute
                Dim obj = TryCast(parameter, ContentDirectory.CD_Object)
                If obj.Resource Is Nothing Then Return
                _Renderer.SetAVTransportURI(CUInt(0), obj.Resource.FirstOrDefault?.URI.ToString, obj.XMLDump)
                Await Task.Delay(500)
                _Renderer.ForceInfoPass()
            End Sub

            Public Function CanExecute(parameter As Object) As Boolean Implements ICommand.CanExecute
                Return _Renderer IsNot Nothing AndAlso TypeOf parameter Is ContentDirectory.CD_Object AndAlso parameter IsNot Nothing
            End Function
        End Class
#End Region
#Region "IDisposable"
        Private disposedValue As Boolean
        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not disposedValue Then
                If disposing Then
                    ' TODO: dispose managed state (managed objects)
                    StopMonitoring()
                End If

                ' TODO: free unmanaged resources (unmanaged objects) and override finalizer
                ' TODO: set large fields to null
                disposedValue = True
            End If
        End Sub

        ' ' TODO: override finalizer only if 'Dispose(disposing As Boolean)' has code to free unmanaged resources
        ' Protected Overrides Sub Finalize()
        '     ' Do not change this code. Put cleanup code in 'Dispose(disposing As Boolean)' method
        '     Dispose(disposing:=False)
        '     MyBase.Finalize()
        ' End Sub

        Public Sub Dispose() Implements IDisposable.Dispose
            ' Do not change this code. Put cleanup code in 'Dispose(disposing As Boolean)' method
            Dispose(disposing:=True)
            GC.SuppressFinalize(Me)
        End Sub
#End Region
    End Class
End Namespace
