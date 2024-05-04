Imports QuickBeat.Interfaces
Imports QuickBeat.Utilities

Namespace Classes
    Public Class API
        Implements Interfaces.IStartupItem

        Private WithEvents _NPM_PSA As New NamedPipeManager("QuickBeatPSA", IO.Pipes.PipeDirection.InOut)
        Private WithEvents _NPM_CTL As New NamedPipeManager("QuickBeatCTL", IO.Pipes.PipeDirection.InOut)

        Private WithEvents _Player As Player.Player

        Public Property Configuration As New StartupItemConfiguration("Quickbeat API") Implements IStartupItem.Configuration

        Private _LockCount As Integer = 0
        Private _IsLocked As Boolean
        ''' <summary>
        ''' Returns wheter or not the API Dispatch is temporarily blocked
        ''' </summary>
        ''' <returns></returns>
        ReadOnly Property IsLocked As Boolean
            Get
                Return _IsLocked
            End Get
        End Property

        Private _IsRunning As Boolean
        ReadOnly Property IsRunning As Boolean
            Get
                Return _IsRunning
            End Get
        End Property

        Public Sub Init() Implements IStartupItem.Init
            '_NPM_PSA.StartServer()
            _NPM_CTL.StartServer()
            '_Timer.Start()
            _Player = SharedProperties.Instance.Player
            Configuration.SetStatus("Running...", 100)
            Utilities.SharedProperties.Instance?.ItemsConfiguration.Add(Configuration)
            _IsRunning = True
        End Sub

        Public Sub [Stop]()
            _NPM_CTL.StopServer()
            _Player = Nothing
            ClearLock()
            Configuration.SetStatus("Stopped", 100)
            _IsRunning = False
        End Sub

        Private Sub _NPM_ReceiveString(sender As NamedPipeManager, msg As String) Handles _NPM_CTL.ReceiveString
            Dim sMsg = msg.Split("|")
            Select Case sMsg.FirstOrDefault
                Case "play"
                    SharedProperties.Instance.Player.Play()
                Case "pause"
                    SharedProperties.Instance.Player.Pause()
                Case "playpause"
                    SharedProperties.Instance.Player.PlayPause()
                Case "next"
                    SharedProperties.Instance.Player.Next()
                Case "previous"
                    SharedProperties.Instance.Player.Playlist?.Previous()
                Case "currentitem"
                    Dim cItem = SharedProperties.Instance.Player.StreamMetadata
                    If cItem Is Nothing Then
                        If Not _NPM_PSA.Write("currentitem|none", 50) Then IncreaseLockCount()
                    Else
                        With cItem
                            Dim sItem = $"currentitem|{ .Title}|{ .JoinedArtists}|{ .Album}|{ .LengthString}"
                            If Not _NPM_PSA.Write(sItem, 50) Then IncreaseLockCount()
                        End With
                    End If
                Case "currentitemcover"
                    SharedProperties.Instance.Player.StreamMetadata?.EnsureCovers()
                    Dim cover = If(SharedProperties.Instance.Player.StreamMetadata?.Covers IsNot Nothing, SharedProperties.Instance.Player.StreamMetadata?.Covers(0), Nothing)
                    If cover Is Nothing Then
                        If Not _NPM_PSA.Write("currentitemcover|null", 50) Then IncreaseLockCount()
                        Return
                    End If
                    'Cache Thumb
                    Dim cmem As New IO.MemoryStream
                    Dim pngEnco As New PngBitmapEncoder
                    pngEnco.Frames.Add(BitmapFrame.Create(cover))
                    pngEnco.Save(cmem)
                    If Not _NPM_PSA.Write("currentitemcover|" & Convert.ToBase64String(cmem.ToArray), 50) Then IncreaseLockCount()
                    cmem.Dispose()
                Case "nextitem"
                    Dim nItem = SharedProperties.Instance.Player.Playlist.NextItem
                    If nItem Is Nothing Then
                        If Not _NPM_PSA.Write("nextitem|none", 50) Then IncreaseLockCount()
                    Else
                        With nItem
                            Dim sItem = $"nextitem|{ .Title}|{ .JoinedArtists}|{ .Album}|{ .LengthString}"
                            If Not _NPM_PSA.Write(sItem, 50) Then IncreaseLockCount()
                        End With
                    End If
                Case "previousitem"
                    Dim pItem = SharedProperties.Instance.Player.Playlist.PreviousItem
                    If pItem Is Nothing Then
                        If Not _NPM_PSA.Write("previousitem|none", 50) Then IncreaseLockCount()
                    Else
                        With pItem
                            Dim sItem = $"previousitem|{ .Title}|{ .JoinedArtists}|{ .Album}|{ .LengthString}"
                            If Not _NPM_PSA.Write(sItem, 50) Then IncreaseLockCount()
                        End With
                    End If
                Case "status"
                    Dim state = If(SharedProperties.Instance.Player.IsPlaying, "playing",
                                                            If(SharedProperties.Instance.Player.IsPaused, "paused",
                                                            If(SharedProperties.Instance.Player.IsStopped, "stopped",
                                                            If(SharedProperties.Instance.Player.IsStalled, "stalled",
                                                            If(SharedProperties.Instance.Player.IsTransitioning, "transistion", "unknown")))))
                    If Not _NPM_PSA.Write($"status|{state}", 50) Then IncreaseLockCount()
                Case "seek"
                    Dim dPos As Double = 0
                    If Double.TryParse(sMsg.LastOrDefault, dPos) Then
                        SharedProperties.Instance.Player.Position = dPos
                    End If
                Case "position"
                    If Not _NPM_PSA.Write($"position|{SharedProperties.Instance.Player.PositionString}", 50) Then IncreaseLockCount()
                Case Else
                    Return 'skip ClearLock incase of wrong API call
            End Select
            ClearLock()
        End Sub

        Private Sub _Player_PlaystateChanged(sender As Player.Player) Handles _Player.PlaystateChanged
            If _IsLocked Then Return
            Dim state = If(_Player.IsPlaying, "playing",
                                                           If(_Player.IsPaused, "paused",
                                                           If(_Player.IsStopped, "stopped",
                                                           If(_Player.IsStalled, "stalled",
                                                           If(_Player.IsTransitioning, "transistion", "unknown")))))
            If Not _NPM_PSA.Write($"status|{state}", 50) Then IncreaseLockCount()
        End Sub

        Private Sub _Player_MetadataChanged() Handles _Player.MetadataChanged
            If _IsLocked Then Return
            Dim cItem = _Player.StreamMetadata
            If cItem IsNot Nothing Then
                With cItem
                    Dim sItem = $"currentitem|{ .Title}|{ .JoinedArtists}|{ .Album}|{ .LengthString}"
                    If Not _NPM_PSA.Write(sItem, 50) Then IncreaseLockCount()
                End With
            End If
        End Sub

        Private Sub _Player_PositionChanged(newPosition As Double) Handles _Player.PositionChanged
            If _IsLocked Then Return
            Dim TS = TimeSpan.FromSeconds(newPosition)
            If Not _NPM_PSA.Write($"position|{ $"{If(TS.Minutes < 10, 0, "")}{TS.Minutes}:{If(TS.Seconds < 10, 0, "")}{TS.Seconds}"}", 50) Then IncreaseLockCount()
        End Sub

        Private Sub IncreaseLockCount()
            _LockCount += 1
            If _LockCount = 5 Then
                _IsLocked = True
                Configuration.SetStatus("Locked", 100)
            End If
        End Sub

        Private Sub ClearLock()
            _LockCount = 0
            _IsLocked = False
            Configuration.SetStatus(If(IsRunning, "Running...", "Stopped"), 100)
        End Sub
    End Class
End Namespace