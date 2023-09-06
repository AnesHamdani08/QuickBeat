Imports System.Reflection
Imports QuickBeat.Utilities
Imports Un4seen.Bass

Namespace Player.WPF.Commands
    Module Constants
        Public Const AudioSlideDuration As Integer = 500
    End Module
    Public Class LoadCommand
        Implements ICommand

        Public Event CanExecuteChanged As EventHandler Implements ICommand.CanExecuteChanged
        Private _Player As Player

        Sub New(player As Player)
            _Player = player
            AddHandler CommandManager.RequerySuggested, New EventHandler(Sub(sender As Object, e As EventArgs)
                                                                             RaiseEvent CanExecuteChanged(Me, New EventArgs())
                                                                         End Sub)
        End Sub

        Public Sub Execute(parameter As Object) Implements ICommand.Execute
            Dim path As String = ""
            If IO.File.Exists(parameter?.ToString) Then
                path = parameter?.ToString
            Else
                Dim Ofd As New Microsoft.Win32.OpenFileDialog() With {.CheckFileExists = True, .Multiselect = False, .Filter = "Supported Files|*.mp3;*.m4a;*.mp4;*.wav;*.aiff;*.mp2;*.mp1;*.ogg;*.wma;*.flac;*.alac;*.webm;*.midi;*.mid|Playlist|*.qbo;*.m3u;*.m3u8|All files|*.*"}
                If Ofd.ShowDialog Then
                    path = Ofd.FileName
                End If
            End If
            If Not String.IsNullOrEmpty(path) Then
                If path.EndsWith(".qbo") Then
                    Using fs As New IO.FileStream(path, IO.FileMode.Open, IO.FileAccess.Read, IO.FileShare.Read)
                        Dim BinF As New System.Runtime.Serialization.Formatters.Binary.BinaryFormatter()
                        Dim Obj = BinF.Deserialize(fs)
                        If Obj.GetType Is GetType(Playlist) Then
                            Dim pObj = TryCast(Obj, Playlist)
                            If HandyControl.Controls.MessageBox.Ask(Utilities.ResourceResolver.Strings.QUERY_PLAYLIST_LOAD.Replace("%n", pObj?.Name).Replace("%c", pObj?.Count)) = MessageBoxResult.OK Then
                                pObj.Cover = Utilities.CommonFunctions.ToCoverImage(pObj.Name)
                                _Player.LoadPlaylist(pObj)
                            End If
                        End If
                    End Using
                ElseIf path.EndsWith(".m3u") OrElse path.EndsWith(".m3u") Then
                    Dim M3U As New M3UFile(path)
                    M3U.Load()
                    If HandyControl.Controls.MessageBox.Ask(Utilities.ResourceResolver.Strings.QUERY_PLAYLIST_LOAD.Replace("%n", If(String.IsNullOrEmpty(M3U.Name), "Unknown", M3U.Name)).Replace("%c", M3U.Count)) = MessageBoxResult.OK Then
                        Dim pM3U = M3U.ToPlaylist
                        pM3U.Name = If(String.IsNullOrEmpty(M3U.Name), Guid.NewGuid.ToString, M3U.Name)
                        pM3U.Cover = Utilities.CommonFunctions.ToCoverImage(pM3U.Name)
                        _Player.LoadPlaylist(pM3U)
                    End If
                Else
                    _Player.LoadSong(path)
                End If
            End If
        End Sub

        Public Function CanExecute(parameter As Object) As Boolean Implements ICommand.CanExecute
            Return _Player.IsInitialized
        End Function
    End Class
    Public Class LoadURLCommand
        Implements ICommand

        Public Event CanExecuteChanged As EventHandler Implements ICommand.CanExecuteChanged
        Private _Player As Player

        Sub New(player As Player)
            _Player = player
            AddHandler CommandManager.RequerySuggested, New EventHandler(Sub(sender As Object, e As EventArgs)
                                                                             RaiseEvent CanExecuteChanged(Me, New EventArgs())
                                                                         End Sub)
        End Sub

        Public Async Sub Execute(parameter As Object) Implements ICommand.Execute
            If IO.File.Exists(parameter?.ToString) Then
                _Player.LoadSong(parameter?.ToString)
                Return
            End If
            Dim url = Dialogs.InputBox.ShowSingle("URL")
            If Not String.IsNullOrEmpty(url) Then
                Await _Player.LoadSong(New Metadata() With {.Location = Metadata.FileLocation.Remote, .Path = url, .OriginalPath = url})
            End If
        End Sub

        Public Function CanExecute(parameter As Object) As Boolean Implements ICommand.CanExecute
            Return _Player.IsInitialized
        End Function
    End Class
    Public Class LoadPlaylistCommand
        Implements ICommand

        Public Event CanExecuteChanged As EventHandler Implements ICommand.CanExecuteChanged
        Private _Player As Player

        Sub New(player As Player)
            _Player = player
            AddHandler CommandManager.RequerySuggested, New EventHandler(Sub(sender As Object, e As EventArgs)
                                                                             RaiseEvent CanExecuteChanged(Me, New EventArgs())
                                                                         End Sub)
        End Sub

        Public Sub Execute(parameter As Object) Implements ICommand.Execute
            _Player?.LoadPlaylist(TryCast(parameter, Playlist))
        End Sub

        Public Function CanExecute(parameter As Object) As Boolean Implements ICommand.CanExecute
            Return (parameter IsNot Nothing) AndAlso (TypeOf parameter Is Playlist)
        End Function
    End Class
    Public Class LoadGroupCommand
        Implements ICommand

        Public Event CanExecuteChanged As EventHandler Implements ICommand.CanExecuteChanged
        Private _Player As Player

        Sub New(player As Player)
            _Player = player
            AddHandler CommandManager.RequerySuggested, New EventHandler(Sub(sender As Object, e As EventArgs)
                                                                             RaiseEvent CanExecuteChanged(Me, New EventArgs())
                                                                         End Sub)
        End Sub

        Public Sub Execute(parameter As Object) Implements ICommand.Execute
            _Player?.LoadGroup(TryCast(parameter, MetadataGroup))
        End Sub

        Public Function CanExecute(parameter As Object) As Boolean Implements ICommand.CanExecute
            Return (parameter IsNot Nothing) AndAlso (TypeOf parameter Is MetadataGroup)
        End Function
    End Class
    Public Class LoadAndAddCommand
        Implements ICommand

        Public Event CanExecuteChanged As EventHandler Implements ICommand.CanExecuteChanged
        Private _Player As Player

        Sub New(player As Player)
            _Player = player
            AddHandler CommandManager.RequerySuggested, New EventHandler(Sub(sender As Object, e As EventArgs)
                                                                             RaiseEvent CanExecuteChanged(Me, New EventArgs())
                                                                         End Sub)
        End Sub

        Public Sub Execute(parameter As Object) Implements ICommand.Execute
            If IO.File.Exists(parameter?.ToString) Then
                _Player.Playlist.Index = _Player.Playlist.Add(Metadata.FromFile(parameter?.ToString, True))
                '_Player.Playlist.Index = _Player.Playlist.Count - 1
                Return
            End If
            If TypeOf parameter Is Metadata Then
                _Player.Playlist.Index = _Player.Playlist.Add(TryCast(parameter, Metadata))
            Else
                Dim Ofd As New Microsoft.Win32.OpenFileDialog() With {.CheckFileExists = True, .Multiselect = False, .Filter = "Supported Files|*.mp3;*.m4a;*.mp4;*.wav;*.aiff;*.mp2;*.mp1;*.ogg;*.wma;*.flac;*.alac;*.webm;*.midi;*.mid|All files|*.*"}
                If Ofd.ShowDialog() Then
                    _Player.Playlist.Add(Metadata.FromFile(Ofd.FileName, True))
                    _Player.Playlist.Index = _Player.Playlist.Count - 1
                End If
            End If
        End Sub

        Public Function CanExecute(parameter As Object) As Boolean Implements ICommand.CanExecute
            Return _Player.IsInitialized
        End Function
    End Class
    Public Class PlayCommand
        Implements ICommand

        Public Event CanExecuteChanged As EventHandler Implements ICommand.CanExecuteChanged

        Sub New()
            AddHandler CommandManager.RequerySuggested, New EventHandler(Sub(sender As Object, e As EventArgs)
                                                                             RaiseEvent CanExecuteChanged(Me, New EventArgs())
                                                                         End Sub)
        End Sub

        Public Sub Execute(parameter As Object) Implements ICommand.Execute
            Bass.BASS_ChannelPlay(CInt(parameter), False)
            If CInt(parameter) = Utilities.SharedProperties.Instance.Player?.Stream Then
                Utilities.SharedProperties.Instance.Player?.OnPropertyChanged(NameOf(Player.IsPlaying))
                Utilities.SharedProperties.Instance.Player?.OnPropertyChanged(NameOf(Player.IsPaused))
                Utilities.SharedProperties.Instance.Player?.OnPropertyChanged(NameOf(Player.IsStalled))
                Utilities.SharedProperties.Instance.Player?.OnPropertyChanged(NameOf(Player.IsStopped))
                If Utilities.SharedProperties.Instance.Player?.IsFadingAudio AndAlso Not Utilities.SharedProperties.Instance.Player?.IsMuted Then
                    Bass.BASS_ChannelSlideAttribute(CInt(parameter), BASSAttribute.BASS_ATTRIB_VOL, Utilities.SharedProperties.Instance.Player?.Volume / 100, Constants.AudioSlideDuration)
                End If
            End If
        End Sub

        Public Function CanExecute(parameter As Object) As Boolean Implements ICommand.CanExecute
            Return (CInt(parameter) <> 0)
        End Function
    End Class
    Public Class PauseCommand
        Implements ICommand

        Public Event CanExecuteChanged As EventHandler Implements ICommand.CanExecuteChanged

        Sub New()
            AddHandler CommandManager.RequerySuggested, New EventHandler(Sub(sender As Object, e As EventArgs)
                                                                             RaiseEvent CanExecuteChanged(Me, New EventArgs())
                                                                         End Sub)
        End Sub

        Public Async Sub Execute(parameter As Object) Implements ICommand.Execute
            If CInt(parameter) = Utilities.SharedProperties.Instance.Player?.Stream AndAlso Utilities.SharedProperties.Instance.Player?.IsFadingAudio AndAlso Not Utilities.SharedProperties.Instance.Player?.IsMuted Then
                Bass.BASS_ChannelSlideAttribute(CInt(parameter), BASSAttribute.BASS_ATTRIB_VOL, 0, Constants.AudioSlideDuration)
                Await Task.Delay(Constants.AudioSlideDuration)
            End If
            Bass.BASS_ChannelPause(CInt(parameter))
            If CInt(parameter) = Utilities.SharedProperties.Instance.Player?.Stream Then
                Utilities.SharedProperties.Instance.Player?.OnPropertyChanged(NameOf(Player.IsPlaying))
                Utilities.SharedProperties.Instance.Player?.OnPropertyChanged(NameOf(Player.IsPaused))
                Utilities.SharedProperties.Instance.Player?.OnPropertyChanged(NameOf(Player.IsStalled))
                Utilities.SharedProperties.Instance.Player?.OnPropertyChanged(NameOf(Player.IsStopped))
            End If
        End Sub

        Public Function CanExecute(parameter As Object) As Boolean Implements ICommand.CanExecute
            Return (CInt(parameter) <> 0)
        End Function
    End Class
    Public Class PlayPauseCommand
        Implements ICommand

        Public Event CanExecuteChanged As EventHandler Implements ICommand.CanExecuteChanged

        Sub New()
            AddHandler CommandManager.RequerySuggested, New EventHandler(Sub(sender As Object, e As EventArgs)
                                                                             RaiseEvent CanExecuteChanged(Me, New EventArgs())
                                                                         End Sub)
        End Sub

        Public Async Sub Execute(parameter As Object) Implements ICommand.Execute
            Select Case Bass.BASS_ChannelIsActive(CInt(parameter))
                Case BASSActive.BASS_ACTIVE_PLAYING
                    If CInt(parameter) = Utilities.SharedProperties.Instance.Player?.Stream AndAlso Utilities.SharedProperties.Instance.Player?.IsFadingAudio AndAlso Not Utilities.SharedProperties.Instance.Player?.IsMuted Then
                        Bass.BASS_ChannelSlideAttribute(CInt(parameter), BASSAttribute.BASS_ATTRIB_VOL, 0, Constants.AudioSlideDuration)
                        Await Task.Delay(Constants.AudioSlideDuration)
                    End If
                    Bass.BASS_ChannelPause(CInt(parameter))
                Case Else
                    Bass.BASS_ChannelPlay(CInt(parameter), False)
                    If Utilities.SharedProperties.Instance.Player?.IsFadingAudio AndAlso Not Utilities.SharedProperties.Instance.Player?.IsMuted Then
                        Bass.BASS_ChannelSlideAttribute(CInt(parameter), BASSAttribute.BASS_ATTRIB_VOL, Utilities.SharedProperties.Instance.Player?.Volume / 100, Constants.AudioSlideDuration)
                    End If
            End Select
            If CInt(parameter) = Utilities.SharedProperties.Instance.Player?.Stream Then
                Utilities.SharedProperties.Instance.Player?.OnPropertyChanged(NameOf(Player.IsPlaying))
                Utilities.SharedProperties.Instance.Player?.OnPropertyChanged(NameOf(Player.IsPaused))
                Utilities.SharedProperties.Instance.Player?.OnPropertyChanged(NameOf(Player.IsStalled))
                Utilities.SharedProperties.Instance.Player?.OnPropertyChanged(NameOf(Player.IsStopped))
            End If
        End Sub

        Public Function CanExecute(parameter As Object) As Boolean Implements ICommand.CanExecute
            Return (CInt(parameter) <> 0)
        End Function
    End Class
    Public Class FastForwardCommand
        Implements ICommand

        Public Event CanExecuteChanged As EventHandler Implements ICommand.CanExecuteChanged

        Sub New()
            AddHandler CommandManager.RequerySuggested, New EventHandler(Sub(sender As Object, e As EventArgs)
                                                                             RaiseEvent CanExecuteChanged(Me, New EventArgs())
                                                                         End Sub)
        End Sub

        Public Sub Execute(parameter As Object) Implements ICommand.Execute
            Bass.BASS_ChannelSetPosition(CInt(parameter), seconds:=Bass.BASS_ChannelBytes2Seconds(CInt(parameter), Bass.BASS_ChannelGetPosition(CInt(parameter), BASSMode.BASS_POS_BYTE)) + 10)
        End Sub

        Public Function CanExecute(parameter As Object) As Boolean Implements ICommand.CanExecute
            Return (CInt(parameter) <> 0)
        End Function
    End Class
    Public Class RewindCommand
        Implements ICommand

        Public Event CanExecuteChanged As EventHandler Implements ICommand.CanExecuteChanged

        Sub New()
            AddHandler CommandManager.RequerySuggested, New EventHandler(Sub(sender As Object, e As EventArgs)
                                                                             RaiseEvent CanExecuteChanged(Me, New EventArgs())
                                                                         End Sub)
        End Sub

        Public Sub Execute(parameter As Object) Implements ICommand.Execute
            Bass.BASS_ChannelSetPosition(CInt(parameter), seconds:=Bass.BASS_ChannelBytes2Seconds(CInt(parameter), Bass.BASS_ChannelGetPosition(CInt(parameter), BASSMode.BASS_POS_BYTE)) - 10)
        End Sub

        Public Function CanExecute(parameter As Object) As Boolean Implements ICommand.CanExecute
            Return (CInt(parameter) <> 0)
        End Function
    End Class
    Public Class NextCommand
        Implements ICommand

        Public Event CanExecuteChanged As EventHandler Implements ICommand.CanExecuteChanged
        Sub New()
            AddHandler CommandManager.RequerySuggested, New EventHandler(Sub(sender As Object, e As EventArgs)
                                                                             RaiseEvent CanExecuteChanged(Me, New EventArgs())
                                                                         End Sub)
        End Sub

        Public Sub Execute(parameter As Object) Implements ICommand.Execute
            CType(parameter, Playlist).Next()
        End Sub

        Public Function CanExecute(parameter As Object) As Boolean Implements ICommand.CanExecute
            Return (parameter IsNot Nothing) AndAlso (TypeOf parameter Is Playlist) AndAlso (CType(parameter, Playlist).Count > 0)
        End Function
    End Class
    Public Class PreviousCommand
        Implements ICommand

        Public Event CanExecuteChanged As EventHandler Implements ICommand.CanExecuteChanged
        Sub New()
            AddHandler CommandManager.RequerySuggested, New EventHandler(Sub(sender As Object, e As EventArgs)
                                                                             RaiseEvent CanExecuteChanged(Me, New EventArgs())
                                                                         End Sub)
        End Sub

        Public Sub Execute(parameter As Object) Implements ICommand.Execute
            With CType(parameter, Playlist)
                If .Parent Is Nothing OrElse .Parent.Position < 5 Then
                    CType(parameter, Playlist).Previous()
                Else
                    .Parent.Position = 0
                End If
            End With
        End Sub

        Public Function CanExecute(parameter As Object) As Boolean Implements ICommand.CanExecute
            Return (parameter IsNot Nothing) AndAlso (TypeOf parameter Is Playlist) AndAlso (CType(parameter, Playlist).Count > 0)
        End Function
    End Class
    Public Class StopControlHandleCommand
        Implements ICommand

        Public Event CanExecuteChanged As EventHandler Implements ICommand.CanExecuteChanged

        Sub New()
            AddHandler CommandManager.RequerySuggested, New EventHandler(Sub(sender As Object, e As EventArgs)
                                                                             RaiseEvent CanExecuteChanged(Me, New EventArgs())
                                                                         End Sub)
        End Sub

        Public Sub Execute(parameter As Object) Implements ICommand.Execute
            TryCast(parameter, Player.StreamControlHandle)?.Stop()
        End Sub

        Public Function CanExecute(parameter As Object) As Boolean Implements ICommand.CanExecute
            Return TypeOf parameter Is Player.StreamControlHandle
        End Function
    End Class

    Public Class StopControlHandleAndResumeCommand
        Implements ICommand

        Public Event CanExecuteChanged As EventHandler Implements ICommand.CanExecuteChanged
        Private _Parent As Player
        Sub New(parent As Player)
            _Parent = parent
            AddHandler CommandManager.RequerySuggested, New EventHandler(Sub(sender As Object, e As EventArgs)
                                                                             RaiseEvent CanExecuteChanged(Me, New EventArgs())
                                                                         End Sub)
        End Sub

        Public Sub Execute(parameter As Object) Implements ICommand.Execute
            TryCast(parameter, Player.StreamControlHandle)?.Stop()
            _Parent.Play()
        End Sub

        Public Function CanExecute(parameter As Object) As Boolean Implements ICommand.CanExecute
            Return TypeOf parameter Is Player.StreamControlHandle
        End Function
    End Class
    Public Class PreviewCommand
        Implements ICommand

        Public Event CanExecuteChanged As EventHandler Implements ICommand.CanExecuteChanged
        Private _Player As Player

        Sub New(player As Player)
            _Player = player
            AddHandler CommandManager.RequerySuggested, New EventHandler(Sub(sender As Object, e As EventArgs)
                                                                             RaiseEvent CanExecuteChanged(Me, New EventArgs())
                                                                         End Sub)
        End Sub

        Public Sub Execute(parameter As Object) Implements ICommand.Execute
            If TypeOf parameter Is Metadata Then
                _Player?.Preview(TryCast(parameter, Metadata))
            ElseIf IO.File.Exists(parameter?.ToString) Then
                _Player?.Preview(Metadata.FromFile(parameter))
            End If
        End Sub

        Public Function CanExecute(parameter As Object) As Boolean Implements ICommand.CanExecute
            Return _Player.IsInitialized AndAlso parameter IsNot Nothing AndAlso (TypeOf parameter Is Metadata Or TypeOf parameter Is String)
        End Function
    End Class
    Public Class SwitchStreamCommand
        Implements ICommand

        Public Event CanExecuteChanged As EventHandler Implements ICommand.CanExecuteChanged
        Private _Player As Player

        Sub New(player As Player)
            _Player = player
            AddHandler CommandManager.RequerySuggested, New EventHandler(Sub(sender As Object, e As EventArgs)
                                                                             RaiseEvent CanExecuteChanged(Me, New EventArgs())
                                                                         End Sub)
        End Sub

        Public Sub Execute(parameter As Object) Implements ICommand.Execute
            If _Player Is Nothing Then Return
            _Player.OnStreamChanged(TryCast(parameter, Player.BassStream)?.Handle)
            Dim meta = TryCast(parameter, Player.BassStream)?.Metadata
            If meta IsNot Nothing Then _Player.OnMetadataChanged(meta)
            _Player.OnPropertyChanged(NameOf(Player.IsPlaying))
            _Player.OnPropertyChanged(NameOf(Player.Length))
        End Sub

        Public Function CanExecute(parameter As Object) As Boolean Implements ICommand.CanExecute
            Return _Player.IsInitialized AndAlso TypeOf parameter Is Player.BassStream
        End Function
    End Class
    Public Class SetPositionCommand
        Implements ICommand

        Public Event CanExecuteChanged As EventHandler Implements ICommand.CanExecuteChanged
        Private _Player As Player

        Sub New(player As Player)
            _Player = player
            AddHandler CommandManager.RequerySuggested, New EventHandler(Sub(sender As Object, e As EventArgs)
                                                                             RaiseEvent CanExecuteChanged(Me, New EventArgs())
                                                                         End Sub)
        End Sub

        Public Sub Execute(parameter As Object) Implements ICommand.Execute
            If _Player Is Nothing Then Return
            _Player.Position = TimeSpan.FromMilliseconds(parameter).TotalSeconds
        End Sub

        Public Function CanExecute(parameter As Object) As Boolean Implements ICommand.CanExecute
            Return _Player.IsInitialized
        End Function
    End Class
    Public Class AddToPlaylistCommand
        Implements ICommand

        Public Event CanExecuteChanged As EventHandler Implements ICommand.CanExecuteChanged
        Sub New()
            AddHandler CommandManager.RequerySuggested, New EventHandler(Sub(sender As Object, e As EventArgs)
                                                                             RaiseEvent CanExecuteChanged(Me, New EventArgs())
                                                                         End Sub)
        End Sub

        Public Sub Execute(parameter As Object) Implements ICommand.Execute
            Dim Ofd As New Microsoft.Win32.OpenFileDialog() With {.CheckFileExists = True, .Multiselect = True, .Filter = "Supported Files|*.mp3;*.m4a;*.mp4;*.wav;*.aiff;*.mp2;*.mp1;*.ogg;*.wma;*.flac;*.alac;*.webm;*.midi;*.mid|All files|*.*"}
            If Ofd.ShowDialog() Then
                For Each file In Ofd.FileNames
                    TryCast(parameter, Playlist)?.Add(Metadata.FromFile(file))
                Next
            End If
        End Sub

        Public Function CanExecute(parameter As Object) As Boolean Implements ICommand.CanExecute
            Return (TypeOf parameter Is Playlist) AndAlso (parameter IsNot Nothing)
        End Function
    End Class
    Public Class AddToQueueCommand
        Implements ICommand

        Public Event CanExecuteChanged As EventHandler Implements ICommand.CanExecuteChanged
        Sub New()
            AddHandler CommandManager.RequerySuggested, New EventHandler(Sub(sender As Object, e As EventArgs)
                                                                             RaiseEvent CanExecuteChanged(Me, New EventArgs())
                                                                         End Sub)
        End Sub

        Public Sub Execute(parameter As Object) Implements ICommand.Execute
            If TypeOf parameter Is Playlist Then
                Dim Ofd As New Microsoft.Win32.OpenFileDialog() With {.CheckFileExists = True, .Multiselect = True, .Filter = "Supported Files|*.mp3;*.m4a;*.mp4;*.wav;*.aiff;*.mp2;*.mp1;*.ogg;*.wma;*.flac;*.alac;*.webm;*.midi;*.mid|All files|*.*"}
                If Ofd.ShowDialog() Then
                    For Each file In Ofd.FileNames
                        TryCast(parameter, Playlist)?.Queue.Add(Metadata.FromFile(file))
                    Next
                End If
            ElseIf TypeOf TryCast(parameter, Object())?.FirstOrDefault Is Playlist AndAlso TypeOf TryCast(parameter, Object())?.LastOrDefault Is Metadata Then
                TryCast(parameter(0), Playlist)?.Queue.Add(TryCast(parameter(1), Metadata))
            End If
        End Sub

        Public Function CanExecute(parameter As Object) As Boolean Implements ICommand.CanExecute
            Return (parameter IsNot Nothing) AndAlso ((TypeOf parameter Is Playlist) OrElse (TypeOf TryCast(parameter, Object())?.FirstOrDefault Is Playlist))
        End Function
    End Class
    Public Class RemoveFromPlaylistCommand
        Implements ICommand

        Public Event CanExecuteChanged As EventHandler Implements ICommand.CanExecuteChanged
        Private _Player As Player

        Sub New(player As Player)
            _Player = player
            AddHandler CommandManager.RequerySuggested, New EventHandler(Sub(sender As Object, e As EventArgs)
                                                                             RaiseEvent CanExecuteChanged(Me, New EventArgs())
                                                                         End Sub)
        End Sub

        Public Sub Execute(parameter As Object) Implements ICommand.Execute
            _Player?.Playlist.Remove(parameter) '_Player?.Playlist.FirstOrDefault(Function(k) k.Path = parameter))
        End Sub

        Public Function CanExecute(parameter As Object) As Boolean Implements ICommand.CanExecute
            Return _Player.IsInitialized
        End Function
    End Class
    Public Class AddGroupToPlaylistCommand
        Implements ICommand

        Public Event CanExecuteChanged As EventHandler Implements ICommand.CanExecuteChanged
        Private _Player As Player

        Sub New(player As Player)
            _Player = player
            AddHandler CommandManager.RequerySuggested, New EventHandler(Sub(sender As Object, e As EventArgs)
                                                                             RaiseEvent CanExecuteChanged(Me, New EventArgs())
                                                                         End Sub)
        End Sub

        Public Sub Execute(parameter As Object) Implements ICommand.Execute
            For Each item In TryCast(parameter, MetadataGroup)
                _Player?.Playlist.Add(item) '_Player?.Playlist.FirstOrDefault(Function(k) k.Path = parameter))
            Next
        End Sub

        Public Function CanExecute(parameter As Object) As Boolean Implements ICommand.CanExecute
            Return _Player.IsInitialized AndAlso TypeOf parameter Is MetadataGroup
        End Function
    End Class
    Public Class EnqueueGroupCommand
        Implements ICommand

        Public Event CanExecuteChanged As EventHandler Implements ICommand.CanExecuteChanged
        Private _Player As Player

        Sub New(player As Player)
            _Player = player
            AddHandler CommandManager.RequerySuggested, New EventHandler(Sub(sender As Object, e As EventArgs)
                                                                             RaiseEvent CanExecuteChanged(Me, New EventArgs())
                                                                         End Sub)
        End Sub

        Public Sub Execute(parameter As Object) Implements ICommand.Execute
            For Each item In TryCast(parameter, MetadataGroup)
                _Player?.Playlist.Queue.Add(item) '_Player?.Playlist.FirstOrDefault(Function(k) k.Path = parameter))
            Next
        End Sub

        Public Function CanExecute(parameter As Object) As Boolean Implements ICommand.CanExecute
            Return _Player.IsInitialized AndAlso TypeOf parameter Is MetadataGroup
        End Function
    End Class
    Public Class SearchCommand
        Implements ICommand

        Public Event CanExecuteChanged As EventHandler Implements ICommand.CanExecuteChanged
        Private _Library As Library.Library
        Sub New(Library As Library.Library)
            _Library = Library
            AddHandler CommandManager.RequerySuggested, New EventHandler(Sub(sender As Object, e As EventArgs)
                                                                             RaiseEvent CanExecuteChanged(Me, New EventArgs())
                                                                         End Sub)
        End Sub

        Public Sub Execute(parameter As Object) Implements ICommand.Execute
            If _Library IsNot Nothing Then _Library.Search()
        End Sub

        Public Function CanExecute(parameter As Object) As Boolean Implements ICommand.CanExecute
            Return (_Library IsNot Nothing)
        End Function
    End Class
    Public Class FocusArtistGroupCommand
        Implements ICommand

        Public Event CanExecuteChanged As EventHandler Implements ICommand.CanExecuteChanged
        Private _Library As Library.Library
        Sub New(Library As Library.Library)
            _Library = Library
            AddHandler CommandManager.RequerySuggested, New EventHandler(Sub(sender As Object, e As EventArgs)
                                                                             RaiseEvent CanExecuteChanged(Me, New EventArgs())
                                                                         End Sub)
        End Sub

        Public Sub Execute(parameter As Object) Implements ICommand.Execute
            If _Library IsNot Nothing Then _Library.FocusArtistGroup(parameter)
        End Sub

        Public Function CanExecute(parameter As Object) As Boolean Implements ICommand.CanExecute
            Return (_Library IsNot Nothing)
        End Function
    End Class
    Public Class FocusAlbumGroupCommand
        Implements ICommand

        Public Event CanExecuteChanged As EventHandler Implements ICommand.CanExecuteChanged
        Private _Library As Library.Library
        Sub New(Library As Library.Library)
            _Library = Library
            AddHandler CommandManager.RequerySuggested, New EventHandler(Sub(sender As Object, e As EventArgs)
                                                                             RaiseEvent CanExecuteChanged(Me, New EventArgs())
                                                                         End Sub)
        End Sub

        Public Sub Execute(parameter As Object) Implements ICommand.Execute
            If _Library IsNot Nothing Then _Library.FocusAlbumGroup(parameter)
        End Sub

        Public Function CanExecute(parameter As Object) As Boolean Implements ICommand.CanExecute
            Return (_Library IsNot Nothing)
        End Function
    End Class
    Public Class FocusGroupCommand
        Implements ICommand

        Public Event CanExecuteChanged As EventHandler Implements ICommand.CanExecuteChanged
        Private _Library As Library.Library
        Sub New(Library As Library.Library)
            _Library = Library
            AddHandler CommandManager.RequerySuggested, New EventHandler(Sub(sender As Object, e As EventArgs)
                                                                             RaiseEvent CanExecuteChanged(Me, New EventArgs())
                                                                         End Sub)
        End Sub

        Public Sub Execute(parameter As Object) Implements ICommand.Execute
            If _Library IsNot Nothing Then
                If TypeOf parameter Is Playlist Then
                    _Library.FocusGroup(TryCast(parameter, Playlist))
                ElseIf TypeOf parameter Is MetadataGroup Then
                    _Library.FocusGroup(TryCast(parameter, MetadataGroup))
                End If
            End If
        End Sub

        Public Function CanExecute(parameter As Object) As Boolean Implements ICommand.CanExecute
            Return (_Library IsNot Nothing) AndAlso ((TypeOf parameter Is Playlist) OrElse (TypeOf parameter Is MetadataGroup))
        End Function
    End Class
    Public Class AddAudioEffectCommand
        Implements ICommand

        Public Event CanExecuteChanged As EventHandler Implements ICommand.CanExecuteChanged

        Sub New()
            AddHandler CommandManager.RequerySuggested, New EventHandler(Sub(sender As Object, e As EventArgs)
                                                                             RaiseEvent CanExecuteChanged(Me, New EventArgs())
                                                                         End Sub)
        End Sub

        Public Sub Execute(parameter As Object) Implements ICommand.Execute
            If parameter Is Nothing Then
                'Dim AudioEffectType = GetType(QuickBeat.Player.Profile.AudioEffect)
                'Dim AudioEffectsList = GetType(QuickBeat.Player.AudioEffects).GetNestedTypes().Where(Function(k) AudioEffectType.IsAssignableFrom(k))
                'Dim i = InputBox("Found " & AudioEffectsList.Count & " Audio Effect: " & Environment.NewLine & String.Join(Environment.NewLine, AudioEffectsList.Select(Of String)(Function(k) k.Name)))
                Dim dlg As New Dialogs.AudioEffectPicker
                If dlg.ShowDialog Then
                    SharedProperties.Instance?.Player?.EffectsProfile?.Add(Activator.CreateInstance(dlg.DialogAudioEffectResult))
                End If
            Else
                If TypeOf parameter Is Type AndAlso TryCast(parameter, Type).IsAssignableFrom(GetType(QuickBeat.Player.Profile.AudioEffect)) Then
                    SharedProperties.Instance?.Player?.EffectsProfile?.Add(Activator.CreateInstance(TryCast(parameter, Type)))
                ElseIf TypeOf parameter Is Profile.AudioEffect Then
                    SharedProperties.Instance?.Player?.EffectsProfile?.Add(parameter)
                End If
            End If
        End Sub

        Public Function CanExecute(parameter As Object) As Boolean Implements ICommand.CanExecute
            Return True
        End Function
    End Class
    Public Class ClearAudioEffectsCommand
        Implements ICommand

        Public Event CanExecuteChanged As EventHandler Implements ICommand.CanExecuteChanged

        Sub New()
            AddHandler CommandManager.RequerySuggested, New EventHandler(Sub(sender As Object, e As EventArgs)
                                                                             RaiseEvent CanExecuteChanged(Me, New EventArgs())
                                                                         End Sub)
        End Sub

        Public Sub Execute(parameter As Object) Implements ICommand.Execute
            SharedProperties.Instance?.Player?.EffectsProfile?.Clear()
        End Sub

        Public Function CanExecute(parameter As Object) As Boolean Implements ICommand.CanExecute
            Return True
        End Function
    End Class
    Public Class RemoveAudioEffectCommand
        Implements ICommand

        Public Event CanExecuteChanged As EventHandler Implements ICommand.CanExecuteChanged

        Sub New()
            AddHandler CommandManager.RequerySuggested, New EventHandler(Sub(sender As Object, e As EventArgs)
                                                                             RaiseEvent CanExecuteChanged(Me, New EventArgs())
                                                                         End Sub)
        End Sub

        Public Sub Execute(parameter As Object) Implements ICommand.Execute
            SharedProperties.Instance?.Player.EffectsProfile.Remove(TryCast(parameter, QuickBeat.Player.Profile.AudioEffect))
        End Sub

        Public Function CanExecute(parameter As Object) As Boolean Implements ICommand.CanExecute
            Return (Not IsNothing(parameter) AndAlso TypeOf parameter Is QuickBeat.Player.Profile.AudioEffect)
        End Function
    End Class
    Public Class ConfigAudioEffectCommand
        Implements ICommand

        Public Event CanExecuteChanged As EventHandler Implements ICommand.CanExecuteChanged

        Sub New()
            AddHandler CommandManager.RequerySuggested, New EventHandler(Sub(sender As Object, e As EventArgs)
                                                                             RaiseEvent CanExecuteChanged(Me, New EventArgs())
                                                                         End Sub)
        End Sub

        Public Sub Execute(parameter As Object) Implements ICommand.Execute
            Dim ExposedProperties As New Dictionary(Of Reflection.PropertyInfo, IEnumerable(Of Attribute))
            Dim ExposedMethods As New Dictionary(Of Reflection.MethodInfo, IEnumerable(Of Attribute))

            For Each prop In parameter.GetType.GetProperties
                Dim AttList = prop.GetCustomAttributes.Where(Function(k) (TypeOf k Is Profile.AudioEffect.FieldAttribute) OrElse (TypeOf k Is Profile.AudioEffect.MethodAttribute) OrElse (TypeOf k Is Profile.AudioEffect.MethodGroupAttribute) OrElse (TypeOf k Is Profile.AudioEffect.MarginAttribute))
                If AttList.Count > 0 Then ExposedProperties.Add(prop, AttList)
            Next
            For Each met In parameter.GetType.GetMethods
                Dim AttList = met.GetCustomAttributes.Where(Function(k) (TypeOf k Is Profile.AudioEffect.FieldAttribute) OrElse (TypeOf k Is Profile.AudioEffect.MethodAttribute) OrElse (TypeOf k Is Profile.AudioEffect.MethodGroupAttribute) OrElse (TypeOf k Is Profile.AudioEffect.MarginAttribute))
                If AttList.Count > 0 Then ExposedMethods.Add(met, AttList)
            Next
            Dim ExposedGroups = ExposedProperties.Where(Function(k) k.Value.FirstOrDefault(Function(l) TypeOf l Is Profile.AudioEffect.MethodGroupAttribute) IsNot Nothing).Select(Function(m) m.Value.FirstOrDefault)

            'Creating effect configuration window
            Dim ConfigMethod = parameter.GetType.GetMethods.FirstOrDefault(Function(k) k.GetCustomAttributes.Any(Function(l) TypeOf l Is Profile.AudioEffect.ConfigAttribute)) 'ExposedMethods.FirstOrDefault(Function(k) k.Value.FirstOrDefault(Function(l) TypeOf l Is Profile.AudioEffect.ConfigAttribute) IsNot Nothing)
            If ConfigMethod IsNot Nothing Then
                CallByName(parameter, ConfigMethod.Name, CallType.Method, Nothing)
                Return
            End If
            Dim ConfigWindow As New HandyControl.Controls.Window With {.Width = 300, .WindowStartupLocation = WindowStartupLocation.CenterOwner, .Owner = Application.Current.MainWindow}
            Dim WindowContent As New StackPanel With {.Margin = New Thickness(10)}
            If ExposedGroups?.Count > 0 Then 'Starting with grouped controls first
                For Each group As Profile.AudioEffect.MethodGroupAttribute In ExposedGroups
                    Dim GroupBox As New GroupBox() With {.Header = group.DisplayName, .Margin = New Thickness(0, 0, 0, 10)}
                    Select Case group.GridStackWrap
                        Case TriState.UseDefault 'Grid                            
                            Dim GroupBoxContent As New Grid With {.Width = group.Width, .Height = group.Height, .HorizontalAlignment = HorizontalAlignment.Stretch, .VerticalAlignment = VerticalAlignment.Stretch}
                            For Each prop In ExposedProperties.Where(Function(k) TryCast(k.Value.FirstOrDefault(Function(l) TypeOf l Is Profile.AudioEffect.FieldAttribute), Profile.AudioEffect.FieldAttribute)?.Group = group.DisplayName)
                                Dim MarginAttr = TryCast(prop.Value.FirstOrDefault(Function(k) TypeOf k Is Profile.AudioEffect.MarginAttribute), Profile.AudioEffect.MarginAttribute)
                                Dim PropAttr = TryCast(prop.Value.FirstOrDefault(Function(k) TypeOf k Is Profile.AudioEffect.FieldAttribute), Profile.AudioEffect.FieldAttribute)
                                If PropAttr.ValueType.IsNumericType AndAlso Not PropAttr.ValueType.IsEnum Then
                                    Dim PropSlider As New Slider With {.ToolTip = PropAttr.DisplayName & If(String.IsNullOrEmpty(PropAttr?.Unit), "", "(" & PropAttr.Unit & ")") & Environment.NewLine & PropAttr.Description, .IsMoveToPointEnabled = True, .TickPlacement = Primitives.TickPlacement.BottomRight, .TickFrequency = PropAttr.Maximum / 100}
                                    If MarginAttr IsNot Nothing Then
                                        PropSlider.HorizontalAlignment = MarginAttr.HorizontalAlignment
                                        PropSlider.VerticalAlignment = MarginAttr.VerticalAlignment
                                        PropSlider.Margin = MarginAttr.Margin
                                    End If
                                    PropSlider.Minimum = PropAttr.Minimum : PropSlider.Maximum = PropAttr.Maximum
                                    AddHandler PropSlider.ValueChanged, New RoutedPropertyChangedEventHandler(Of Double)(Sub(s, e)
                                                                                                                             CallByName(parameter, prop.Key.Name, CallType.Set, {e.NewValue})
                                                                                                                         End Sub)
                                    AddHandler TryCast(parameter, Profile.AudioEffect).PropertyChanged, Sub(s, e)
                                                                                                            If e.PropertyName = prop.Key.Name Then
                                                                                                                PropSlider.Value = prop.Key.GetValue(parameter)
                                                                                                            End If
                                                                                                        End Sub
                                    'GroupBoxContent.Children.Add(PropSlider)
                                    PropSlider.Value = prop.Key.GetValue(parameter)
                                    GroupBoxContent.Children.Add(New Controls.ValueSlider(Nothing, PropSlider, PropAttr?.DisplayName, PropAttr?.UnitShortForm))
                                ElseIf PropAttr.ValueType.IsEnum Then
                                    Dim PropCBox As New ComboBox With {.ToolTip = PropAttr.DisplayName & Environment.NewLine & PropAttr.Description}
                                    Dim Res = Application.Current.TryFindResource("ComboBoxExtend")
                                    If Res IsNot Nothing Then
                                        PropCBox.Style = Res
                                        HandyControl.Controls.TitleElement.SetTitle(PropCBox, PropAttr?.DisplayName)
                                    End If
                                    If MarginAttr IsNot Nothing Then
                                        PropCBox.HorizontalAlignment = MarginAttr.HorizontalAlignment
                                        PropCBox.VerticalAlignment = MarginAttr.VerticalAlignment
                                        PropCBox.Margin = MarginAttr.Margin
                                    End If
                                    For Each item In System.Enum.GetNames(PropAttr.ValueType)
                                        PropCBox.Items.Add(New ComboBoxItem With {.Content = item})
                                    Next
                                    AddHandler PropCBox.SelectionChanged, New SelectionChangedEventHandler(Sub(s, e)
                                                                                                               CallByName(parameter, prop.Key.Name, CallType.Set, {PropCBox.SelectedIndex})
                                                                                                           End Sub)
                                    AddHandler TryCast(parameter, Profile.AudioEffect).PropertyChanged, Sub(s, e)
                                                                                                            If e.PropertyName = prop.Key.Name Then
                                                                                                                PropCBox.SelectedIndex = prop.Key.GetValue(parameter)
                                                                                                            End If
                                                                                                        End Sub
                                    'GroupBoxContent.Children.Add(PropSlider)
                                    PropCBox.SelectedIndex = prop.Key.GetValue(parameter)
                                    GroupBoxContent.Children.Add(PropCBox)
                                ElseIf Type.GetTypeCode(PropAttr.ValueType) = TypeCode.Boolean Then
                                    Dim PropToggleButton As New Primitives.ToggleButton With {.Content = PropAttr.DisplayName, .ToolTip = PropAttr.Description}
                                    If MarginAttr IsNot Nothing Then
                                        PropToggleButton.HorizontalAlignment = MarginAttr.HorizontalAlignment
                                        PropToggleButton.VerticalAlignment = MarginAttr.VerticalAlignment
                                        PropToggleButton.Margin = MarginAttr.Margin
                                    End If
                                    AddHandler PropToggleButton.Checked, Sub()
                                                                             CallByName(parameter, prop.Key.Name, CallType.Set, {True})
                                                                         End Sub
                                    AddHandler PropToggleButton.Unchecked, Sub()
                                                                               CallByName(parameter, prop.Key.Name, CallType.Set, {False})
                                                                           End Sub
                                    GroupBoxContent.Children.Add(PropToggleButton)
                                ElseIf Type.GetTypeCode(PropAttr.ValueType) = TypeCode.String Then
                                    Dim PropTextBox As New TextBox With {.ToolTip = PropAttr.DisplayName & Environment.NewLine & PropAttr.Description}
                                    If MarginAttr IsNot Nothing Then
                                        PropTextBox.HorizontalAlignment = MarginAttr.HorizontalAlignment
                                        PropTextBox.VerticalAlignment = MarginAttr.VerticalAlignment
                                        PropTextBox.Margin = MarginAttr.Margin
                                    End If
                                    AddHandler PropTextBox.TextChanged, New TextChangedEventHandler(Sub(e, d)
                                                                                                        CallByName(parameter, prop.Key.Name, CallType.Set, {PropTextBox.Text})
                                                                                                    End Sub)
                                    GroupBoxContent.Children.Add(PropTextBox)
                                Else
                                    MsgBox("wtf is this ?")
                                End If
                            Next
                            For Each met In ExposedMethods.Where(Function(k) TryCast(k.Value.FirstOrDefault(Function(l) TypeOf l Is Profile.AudioEffect.MethodAttribute), Profile.AudioEffect.MethodAttribute)?.Group = group.DisplayName)
                                Dim MarginAttr = TryCast(met.Value.FirstOrDefault(Function(k) TypeOf k Is Profile.AudioEffect.MarginAttribute), Profile.AudioEffect.MarginAttribute)
                                Dim MetAttr = TryCast(met.Value.FirstOrDefault(Function(k) TypeOf k Is Profile.AudioEffect.MethodAttribute), Profile.AudioEffect.MethodAttribute)
                                Dim MetButton As New Button With {.Content = MetAttr.DisplayName, .ToolTip = MetAttr.Description}
                                If MarginAttr IsNot Nothing Then
                                    MetButton.HorizontalAlignment = MarginAttr.HorizontalAlignment
                                    MetButton.VerticalAlignment = MarginAttr.VerticalAlignment
                                    MetButton.Margin = MarginAttr.Margin
                                End If
                                AddHandler MetButton.Click, Sub()
                                                                CallByName(parameter, met.Key.Name, CallType.Method, Nothing)
                                                            End Sub
                                GroupBoxContent.Children.Add(MetButton)
                            Next
                            If group.Scroll Then
                                Dim SV As New ScrollViewer With {.VerticalScrollBarVisibility = group.VerticalScrollBarVisibility, .HorizontalScrollBarVisibility = group.HorizontalScrollBarVisibility}
                                SV.Content = GroupBoxContent
                                GroupBox.Content = SV
                            Else
                                GroupBox.Content = GroupBoxContent
                            End If
                        Case TriState.True 'Stack
                            Dim GroupBoxContent As New VirtualizingStackPanel With {.Width = group.Width, .Height = group.Height, .HorizontalAlignment = HorizontalAlignment.Stretch, .VerticalAlignment = VerticalAlignment.Stretch, .Orientation = group.Orientation}
                            For Each prop In ExposedProperties.Where(Function(k) TryCast(k.Value.FirstOrDefault(Function(l) TypeOf l Is Profile.AudioEffect.FieldAttribute), Profile.AudioEffect.FieldAttribute)?.Group = group.DisplayName)
                                Dim MarginAttr = TryCast(prop.Value.FirstOrDefault(Function(k) TypeOf k Is Profile.AudioEffect.MarginAttribute), Profile.AudioEffect.MarginAttribute)
                                Dim PropAttr = TryCast(prop.Value.FirstOrDefault(Function(k) TypeOf k Is Profile.AudioEffect.FieldAttribute), Profile.AudioEffect.FieldAttribute)
                                If PropAttr.ValueType.IsNumericType AndAlso Not PropAttr.ValueType.IsEnum Then
                                    Dim PropSlider As New Slider With {.ToolTip = PropAttr.DisplayName & If(String.IsNullOrEmpty(PropAttr?.Unit), "", "(" & PropAttr.Unit & ")") & Environment.NewLine & PropAttr.Description, .IsMoveToPointEnabled = True, .TickPlacement = Primitives.TickPlacement.BottomRight, .TickFrequency = PropAttr.Maximum / 100}
                                    If MarginAttr IsNot Nothing Then
                                        PropSlider.HorizontalAlignment = MarginAttr.HorizontalAlignment
                                        PropSlider.VerticalAlignment = MarginAttr.VerticalAlignment
                                        PropSlider.Margin = MarginAttr.Margin
                                    Else
                                        PropSlider.Margin = New Thickness(0, 0, 10, 10)
                                    End If
                                    PropSlider.Minimum = PropAttr.Minimum : PropSlider.Maximum = PropAttr.Maximum
                                    AddHandler PropSlider.ValueChanged, New RoutedPropertyChangedEventHandler(Of Double)(Sub(s, e)
                                                                                                                             CallByName(parameter, prop.Key.Name, CallType.Set, {e.NewValue})
                                                                                                                         End Sub)
                                    AddHandler TryCast(parameter, Profile.AudioEffect).PropertyChanged, Sub(s, e)
                                                                                                            If e.PropertyName = prop.Key.Name Then
                                                                                                                PropSlider.Value = prop.Key.GetValue(parameter)
                                                                                                            End If
                                                                                                        End Sub
                                    'GroupBoxContent.Children.Add(PropSlider)
                                    PropSlider.Value = prop.Key.GetValue(parameter)
                                    GroupBoxContent.Children.Add(New Controls.ValueSlider(Nothing, PropSlider, PropAttr?.DisplayName, PropAttr?.UnitShortForm))
                                ElseIf PropAttr.ValueType.IsEnum Then
                                    Dim PropCBox As New ComboBox With {.ToolTip = PropAttr.DisplayName & Environment.NewLine & PropAttr.Description}
                                    Dim Res = Application.Current.TryFindResource("ComboBoxExtend")
                                    If Res IsNot Nothing Then
                                        PropCBox.Style = Res
                                        HandyControl.Controls.TitleElement.SetTitle(PropCBox, PropAttr?.DisplayName)
                                    End If
                                    If MarginAttr IsNot Nothing Then
                                        PropCBox.HorizontalAlignment = MarginAttr.HorizontalAlignment
                                        PropCBox.VerticalAlignment = MarginAttr.VerticalAlignment
                                        PropCBox.Margin = MarginAttr.Margin
                                    End If
                                    For Each item In System.Enum.GetNames(PropAttr.ValueType)
                                        PropCBox.Items.Add(New ComboBoxItem With {.Content = item})
                                    Next
                                    AddHandler PropCBox.SelectionChanged, New SelectionChangedEventHandler(Sub(s, e)
                                                                                                               CallByName(parameter, prop.Key.Name, CallType.Set, {PropCBox.SelectedIndex})
                                                                                                           End Sub)
                                    AddHandler TryCast(parameter, Profile.AudioEffect).PropertyChanged, Sub(s, e)
                                                                                                            If e.PropertyName = prop.Key.Name Then
                                                                                                                PropCBox.SelectedIndex = prop.Key.GetValue(parameter)
                                                                                                            End If
                                                                                                        End Sub
                                    'GroupBoxContent.Children.Add(PropSlider)
                                    PropCBox.SelectedIndex = prop.Key.GetValue(parameter)
                                    GroupBoxContent.Children.Add(PropCBox)
                                ElseIf Type.GetTypeCode(PropAttr.ValueType) = TypeCode.Boolean Then
                                    Dim PropToggleButton As New Primitives.ToggleButton With {.Content = PropAttr.DisplayName, .ToolTip = PropAttr.Description}
                                    If MarginAttr IsNot Nothing Then
                                        PropToggleButton.HorizontalAlignment = MarginAttr.HorizontalAlignment
                                        PropToggleButton.VerticalAlignment = MarginAttr.VerticalAlignment
                                        PropToggleButton.Margin = MarginAttr.Margin
                                    Else
                                        PropToggleButton.Margin = New Thickness(0, 0, 10, 10)
                                    End If
                                    AddHandler PropToggleButton.Checked, Sub()
                                                                             CallByName(parameter, prop.Key.Name, CallType.Set, {True})
                                                                         End Sub
                                    AddHandler PropToggleButton.Unchecked, Sub()
                                                                               CallByName(parameter, prop.Key.Name, CallType.Set, {False})
                                                                           End Sub
                                    GroupBoxContent.Children.Add(PropToggleButton)
                                ElseIf Type.GetTypeCode(PropAttr.ValueType) = TypeCode.String Then
                                    Dim PropTextBox As New TextBox With {.ToolTip = PropAttr.DisplayName & Environment.NewLine & PropAttr.Description}
                                    If MarginAttr IsNot Nothing Then
                                        PropTextBox.HorizontalAlignment = MarginAttr.HorizontalAlignment
                                        PropTextBox.VerticalAlignment = MarginAttr.VerticalAlignment
                                        PropTextBox.Margin = MarginAttr.Margin
                                    Else
                                        PropTextBox.Margin = New Thickness(0, 0, 10, 10)
                                    End If
                                    AddHandler PropTextBox.TextChanged, New TextChangedEventHandler(Sub(e, d)
                                                                                                        CallByName(parameter, prop.Key.Name, CallType.Set, {PropTextBox.Text})
                                                                                                    End Sub)
                                    GroupBoxContent.Children.Add(PropTextBox)
                                Else
                                    MsgBox("wtf is this ?")
                                End If
                            Next
                            For Each met In ExposedMethods.Where(Function(k) TryCast(k.Value.FirstOrDefault(Function(l) TypeOf l Is Profile.AudioEffect.MethodAttribute), Profile.AudioEffect.MethodAttribute)?.Group = group.DisplayName)
                                Dim MarginAttr = TryCast(met.Value.FirstOrDefault(Function(k) TypeOf k Is Profile.AudioEffect.MarginAttribute), Profile.AudioEffect.MarginAttribute)
                                Dim MetAttr = TryCast(met.Value.FirstOrDefault(Function(k) TypeOf k Is Profile.AudioEffect.MethodAttribute), Profile.AudioEffect.MethodAttribute)
                                Dim MetButton As New Button With {.Content = MetAttr.DisplayName, .ToolTip = MetAttr.Description}
                                If MarginAttr IsNot Nothing Then
                                    MetButton.HorizontalAlignment = MarginAttr.HorizontalAlignment
                                    MetButton.VerticalAlignment = MarginAttr.VerticalAlignment
                                    MetButton.Margin = MarginAttr.Margin
                                Else
                                    MetButton.Margin = New Thickness(0, 0, 10, 10)
                                End If
                                AddHandler MetButton.Click, Sub()
                                                                CallByName(parameter, met.Key.Name, CallType.Method, Nothing)
                                                            End Sub
                                GroupBoxContent.Children.Add(MetButton)
                            Next
                            If group.Scroll Then
                                Dim SV As New ScrollViewer With {.VerticalScrollBarVisibility = group.VerticalScrollBarVisibility, .HorizontalScrollBarVisibility = group.HorizontalScrollBarVisibility}
                                SV.Content = GroupBoxContent
                                GroupBox.Content = SV
                            Else
                                GroupBox.Content = GroupBoxContent
                            End If
                        Case TriState.False 'Wrap
                            Dim GroupBoxContent As New WrapPanel With {.Width = group.Width, .Height = group.Height, .HorizontalAlignment = HorizontalAlignment.Stretch, .VerticalAlignment = VerticalAlignment.Stretch, .Orientation = group.Orientation}
                            For Each prop In ExposedProperties.Where(Function(k) TryCast(k.Value.FirstOrDefault(Function(l) TypeOf l Is Profile.AudioEffect.FieldAttribute), Profile.AudioEffect.FieldAttribute)?.Group = group.DisplayName)
                                Dim MarginAttr = TryCast(prop.Value.FirstOrDefault(Function(k) TypeOf k Is Profile.AudioEffect.MarginAttribute), Profile.AudioEffect.MarginAttribute)
                                Dim PropAttr = TryCast(prop.Value.FirstOrDefault(Function(k) TypeOf k Is Profile.AudioEffect.FieldAttribute), Profile.AudioEffect.FieldAttribute)
                                If PropAttr.ValueType.IsNumericType AndAlso Not PropAttr.ValueType.IsEnum Then
                                    Dim PropSlider As New Slider With {.ToolTip = PropAttr.DisplayName & If(String.IsNullOrEmpty(PropAttr?.Unit), "", "(" & PropAttr.Unit & ")") & Environment.NewLine & PropAttr.Description, .IsMoveToPointEnabled = True, .TickPlacement = Primitives.TickPlacement.BottomRight, .TickFrequency = PropAttr.Maximum / 100}
                                    If MarginAttr IsNot Nothing Then
                                        PropSlider.HorizontalAlignment = MarginAttr.HorizontalAlignment
                                        PropSlider.VerticalAlignment = MarginAttr.VerticalAlignment
                                        PropSlider.Margin = MarginAttr.Margin
                                    Else
                                        PropSlider.Margin = New Thickness(0, 0, 10, 10)
                                    End If
                                    PropSlider.Minimum = PropAttr.Minimum : PropSlider.Maximum = PropAttr.Maximum
                                    AddHandler PropSlider.ValueChanged, New RoutedPropertyChangedEventHandler(Of Double)(Sub(s, e)
                                                                                                                             CallByName(parameter, prop.Key.Name, CallType.Set, {e.NewValue})
                                                                                                                         End Sub)
                                    AddHandler TryCast(parameter, Profile.AudioEffect).PropertyChanged, Sub(s, e)
                                                                                                            If e.PropertyName = prop.Key.Name Then
                                                                                                                PropSlider.Value = prop.Key.GetValue(parameter)
                                                                                                            End If
                                                                                                        End Sub
                                    'GroupBoxContent.Children.Add(PropSlider)
                                    PropSlider.Value = prop.Key.GetValue(parameter)
                                    GroupBoxContent.Children.Add(New Controls.ValueSlider(Nothing, PropSlider, PropAttr?.DisplayName, PropAttr?.UnitShortForm))
                                ElseIf PropAttr.ValueType.IsEnum Then
                                    Dim PropCBox As New ComboBox With {.ToolTip = PropAttr.DisplayName & Environment.NewLine & PropAttr.Description}
                                    Dim Res = Application.Current.TryFindResource("ComboBoxExtend")
                                    If Res IsNot Nothing Then
                                        PropCBox.Style = Res
                                        HandyControl.Controls.TitleElement.SetTitle(PropCBox, PropAttr?.DisplayName)
                                    End If
                                    If MarginAttr IsNot Nothing Then
                                        PropCBox.HorizontalAlignment = MarginAttr.HorizontalAlignment
                                        PropCBox.VerticalAlignment = MarginAttr.VerticalAlignment
                                        PropCBox.Margin = MarginAttr.Margin
                                    End If
                                    For Each item In System.Enum.GetNames(PropAttr.ValueType)
                                        PropCBox.Items.Add(New ComboBoxItem With {.Content = item})
                                    Next
                                    AddHandler PropCBox.SelectionChanged, New SelectionChangedEventHandler(Sub(s, e)
                                                                                                               CallByName(parameter, prop.Key.Name, CallType.Set, {PropCBox.SelectedIndex})
                                                                                                           End Sub)
                                    AddHandler TryCast(parameter, Profile.AudioEffect).PropertyChanged, Sub(s, e)
                                                                                                            If e.PropertyName = prop.Key.Name Then
                                                                                                                PropCBox.SelectedIndex = prop.Key.GetValue(parameter)
                                                                                                            End If
                                                                                                        End Sub
                                    'GroupBoxContent.Children.Add(PropSlider)
                                    PropCBox.SelectedIndex = prop.Key.GetValue(parameter)
                                    GroupBoxContent.Children.Add(PropCBox)
                                ElseIf Type.GetTypeCode(PropAttr.ValueType) = TypeCode.Boolean Then
                                    Dim PropToggleButton As New Primitives.ToggleButton With {.Content = PropAttr.DisplayName, .ToolTip = PropAttr.Description}
                                    If MarginAttr IsNot Nothing Then
                                        PropToggleButton.HorizontalAlignment = MarginAttr.HorizontalAlignment
                                        PropToggleButton.VerticalAlignment = MarginAttr.VerticalAlignment
                                        PropToggleButton.Margin = MarginAttr.Margin
                                    Else
                                        PropToggleButton.Margin = New Thickness(0, 0, 10, 10)
                                    End If
                                    AddHandler PropToggleButton.Checked, Sub()
                                                                             CallByName(parameter, prop.Key.Name, CallType.Set, {True})
                                                                         End Sub
                                    AddHandler PropToggleButton.Unchecked, Sub()
                                                                               CallByName(parameter, prop.Key.Name, CallType.Set, {False})
                                                                           End Sub
                                    GroupBoxContent.Children.Add(PropToggleButton)
                                ElseIf Type.GetTypeCode(PropAttr.ValueType) = TypeCode.String Then
                                    Dim PropTextBox As New TextBox With {.ToolTip = PropAttr.DisplayName & Environment.NewLine & PropAttr.Description}
                                    If MarginAttr IsNot Nothing Then
                                        PropTextBox.HorizontalAlignment = MarginAttr.HorizontalAlignment
                                        PropTextBox.VerticalAlignment = MarginAttr.VerticalAlignment
                                        PropTextBox.Margin = MarginAttr.Margin
                                    Else
                                        PropTextBox.Margin = New Thickness(0, 0, 10, 10)
                                    End If
                                    AddHandler PropTextBox.TextChanged, New TextChangedEventHandler(Sub(e, d)
                                                                                                        CallByName(parameter, prop.Key.Name, CallType.Set, {PropTextBox.Text})
                                                                                                    End Sub)
                                    GroupBoxContent.Children.Add(PropTextBox)
                                Else
                                    MsgBox("wtf is this ?")
                                End If
                            Next
                            For Each met In ExposedMethods.Where(Function(k) TryCast(k.Value.FirstOrDefault(Function(l) TypeOf l Is Profile.AudioEffect.MethodAttribute), Profile.AudioEffect.MethodAttribute)?.Group = group.DisplayName)
                                Dim MarginAttr = TryCast(met.Value.FirstOrDefault(Function(k) TypeOf k Is Profile.AudioEffect.MarginAttribute), Profile.AudioEffect.MarginAttribute)
                                Dim MetAttr = TryCast(met.Value.FirstOrDefault(Function(k) TypeOf k Is Profile.AudioEffect.MethodAttribute), Profile.AudioEffect.MethodAttribute)
                                Dim MetButton As New Button With {.Content = MetAttr.DisplayName, .ToolTip = MetAttr.Description}
                                If MarginAttr IsNot Nothing Then
                                    MetButton.HorizontalAlignment = MarginAttr.HorizontalAlignment
                                    MetButton.VerticalAlignment = MarginAttr.VerticalAlignment
                                    MetButton.Margin = MarginAttr.Margin
                                Else
                                    MetButton.Margin = New Thickness(0, 0, 10, 10)
                                End If
                                AddHandler MetButton.Click, Sub()
                                                                CallByName(parameter, met.Key.Name, CallType.Method, Nothing)
                                                            End Sub
                                GroupBoxContent.Children.Add(MetButton)
                            Next
                            If group.Scroll Then
                                Dim SV As New ScrollViewer With {.VerticalScrollBarVisibility = group.VerticalScrollBarVisibility, .HorizontalScrollBarVisibility = group.HorizontalScrollBarVisibility}
                                SV.Content = GroupBoxContent
                                GroupBox.Content = SV
                            Else
                                GroupBox.Content = GroupBoxContent
                            End If
                    End Select
                    WindowContent.Children.Add(GroupBox)
                Next
            End If
            'Ungrouped methods
            For Each prop In ExposedProperties.Where(Function(k) k.Value.FirstOrDefault(Function(l) TypeOf l Is Profile.AudioEffect.FieldAttribute) IsNot Nothing)
                Dim PropAttr = TryCast(prop.Value.FirstOrDefault(Function(k) TypeOf k Is Profile.AudioEffect.FieldAttribute), Profile.AudioEffect.FieldAttribute)
                If PropAttr Is Nothing Then Continue For
                If PropAttr.Group <> "" Then Continue For
                If PropAttr.ValueType.IsNumericType AndAlso Not PropAttr.ValueType.IsEnum Then
                    Dim PropSlider As New Slider With {.ToolTip = PropAttr.DisplayName & If(String.IsNullOrEmpty(PropAttr?.Unit), "", "(" & PropAttr.Unit & ")") & Environment.NewLine & PropAttr.Description, .IsMoveToPointEnabled = True}
                    PropSlider.Margin = New Thickness(0, 0, 10, 10)
                    PropSlider.Minimum = PropAttr.Minimum : PropSlider.Maximum = PropAttr.Maximum
                    AddHandler PropSlider.ValueChanged, New RoutedPropertyChangedEventHandler(Of Double)(Sub(s, e)
                                                                                                             CallByName(parameter, prop.Key.Name, CallType.Set, {e.NewValue})
                                                                                                         End Sub)
                    AddHandler TryCast(parameter, Profile.AudioEffect).PropertyChanged, Sub(s, e)
                                                                                            If e.PropertyName = prop.Key.Name Then
                                                                                                PropSlider.Value = prop.Key.GetValue(parameter)
                                                                                            End If
                                                                                        End Sub
                    'WindowContent.Children.Add(PropSlider)                    
                    PropSlider.Value = prop.Key.GetValue(parameter)
                    WindowContent.Children.Add(New Controls.ValueSlider(Nothing, PropSlider, PropAttr?.DisplayName, PropAttr?.UnitShortForm))
                ElseIf PropAttr.ValueType.IsEnum Then
                    Dim PropCBox As New ComboBox With {.ToolTip = PropAttr.DisplayName & Environment.NewLine & PropAttr.Description}
                    PropCBox.Margin = New Thickness(0, 0, 10, 10)
                    Dim Res = Application.Current.TryFindResource("ComboBoxExtend")
                    If Res IsNot Nothing Then
                        PropCBox.Style = Res
                        HandyControl.Controls.TitleElement.SetTitle(PropCBox, PropAttr?.DisplayName)
                    End If
                    For Each item In System.Enum.GetNames(PropAttr.ValueType)
                        PropCBox.Items.Add(New ComboBoxItem With {.Content = item})
                    Next
                    AddHandler PropCBox.SelectionChanged, New SelectionChangedEventHandler(Sub(s, e)
                                                                                               CallByName(parameter, prop.Key.Name, CallType.Set, {PropCBox.SelectedIndex})
                                                                                           End Sub)
                    AddHandler TryCast(parameter, Profile.AudioEffect).PropertyChanged, Sub(s, e)
                                                                                            If e.PropertyName = prop.Key.Name Then
                                                                                                PropCBox.SelectedIndex = prop.Key.GetValue(parameter)
                                                                                            End If
                                                                                        End Sub
                    'GroupBoxContent.Children.Add(PropSlider)
                    PropCBox.SelectedIndex = prop.Key.GetValue(parameter)
                    WindowContent.Children.Add(PropCBox)
                ElseIf Type.GetTypeCode(PropAttr.ValueType) = TypeCode.Boolean Then
                    Dim PropToggleButton As New Primitives.ToggleButton With {.Content = PropAttr.DisplayName, .ToolTip = PropAttr.Description}
                    PropToggleButton.Margin = New Thickness(0, 0, 10, 10)
                    AddHandler PropToggleButton.Checked, Sub()
                                                             CallByName(parameter, prop.Key.Name, CallType.Set, {True})
                                                         End Sub
                    AddHandler PropToggleButton.Unchecked, Sub()
                                                               CallByName(parameter, prop.Key.Name, CallType.Set, {False})
                                                           End Sub
                    WindowContent.Children.Add(PropToggleButton)
                ElseIf Type.GetTypeCode(PropAttr.ValueType) = TypeCode.String Then
                    Dim PropTextBox As New TextBox With {.ToolTip = PropAttr.DisplayName & Environment.NewLine & PropAttr.Description}
                    PropTextBox.Margin = New Thickness(0, 0, 10, 10)
                    AddHandler PropTextBox.TextChanged, New TextChangedEventHandler(Sub(e, d)
                                                                                        CallByName(parameter, prop.Key.Name, CallType.Set, {PropTextBox.Text})
                                                                                    End Sub)
                    WindowContent.Children.Add(PropTextBox)
                Else
                    MsgBox("wtf is this ?")
                End If
            Next
            For Each met In ExposedMethods.Where(Function(k) k.Value.FirstOrDefault(Function(l) TypeOf l Is Profile.AudioEffect.MethodAttribute) IsNot Nothing)
                Dim MarginAttr = TryCast(met.Value.FirstOrDefault(Function(k) TypeOf k Is Profile.AudioEffect.MarginAttribute), Profile.AudioEffect.MarginAttribute)
                Dim MetAttr = TryCast(met.Value.FirstOrDefault(Function(k) TypeOf k Is Profile.AudioEffect.MethodAttribute), Profile.AudioEffect.MethodAttribute)
                If MetAttr IsNot Nothing AndAlso MetAttr.Group <> "" Then Continue For
                Dim MetButton As New Button With {.Content = MetAttr.DisplayName, .ToolTip = MetAttr.Description}
                MetButton.Margin = New Thickness(0, 0, 10, 10)
                AddHandler MetButton.Click, Sub()
                                                CallByName(parameter, met.Key.Name, CallType.Method, Nothing)
                                            End Sub
                WindowContent.Children.Add(MetButton)
            Next
            ConfigWindow.Content = WindowContent
            ConfigWindow.Show()
        End Sub

        Public Function CanExecute(parameter As Object) As Boolean Implements ICommand.CanExecute
            Return (Not IsNothing(parameter) AndAlso TypeOf parameter Is QuickBeat.Player.Profile.AudioEffect)
        End Function
    End Class
    Public Class AddEngineModuleCommand
        Implements ICommand

        Public Event CanExecuteChanged As EventHandler Implements ICommand.CanExecuteChanged

        Sub New()
            AddHandler CommandManager.RequerySuggested, New EventHandler(Sub(sender As Object, e As EventArgs)
                                                                             RaiseEvent CanExecuteChanged(Me, New EventArgs())
                                                                         End Sub)
        End Sub

        Public Sub Execute(parameter As Object) Implements ICommand.Execute
            If parameter Is Nothing Then
                Dim dlg As New Dialogs.EngineModulePicker
                If dlg.ShowDialog Then
                    If SharedProperties.Instance.Player?.Modules.FirstOrDefault(Function(k) k.GetType.Equals(dlg.DialogEngineModuleResult)) IsNot Nothing Then Return
                    Dim enginemodule = Activator.CreateInstance(dlg.DialogEngineModuleResult)
                    SharedProperties.Instance?.Player?.Modules?.Add(enginemodule)
                    TryCast(enginemodule, QuickBeat.Player.EngineModule)?.Init()
                End If
            Else
                If TypeOf parameter Is Type AndAlso TryCast(parameter, Type).IsAssignableFrom(GetType(QuickBeat.Player.EngineModule)) Then
                    SharedProperties.Instance?.Player?.EffectsProfile?.Add(Activator.CreateInstance(TryCast(parameter, Type)))
                ElseIf TypeOf parameter Is EngineModule Then
                    If SharedProperties.Instance.Player?.Modules.FirstOrDefault(Function(k) k.GetType.Equals(parameter)) IsNot Nothing Then Return
                    SharedProperties.Instance?.Player?.Modules?.Add(parameter)
                End If
            End If
        End Sub

        Public Function CanExecute(parameter As Object) As Boolean Implements ICommand.CanExecute
            Return True
        End Function
    End Class
    Public Class ClearEngineModulesCommand
        Implements ICommand

        Public Event CanExecuteChanged As EventHandler Implements ICommand.CanExecuteChanged

        Sub New()
            AddHandler CommandManager.RequerySuggested, New EventHandler(Sub(sender As Object, e As EventArgs)
                                                                             RaiseEvent CanExecuteChanged(Me, New EventArgs())
                                                                         End Sub)
        End Sub

        Public Sub Execute(parameter As Object) Implements ICommand.Execute
            For Each enginemodule In SharedProperties.Instance.Player?.Modules
                enginemodule.IsEnabled = False
            Next
            SharedProperties.Instance?.Player?.Modules?.Clear()
        End Sub

        Public Function CanExecute(parameter As Object) As Boolean Implements ICommand.CanExecute
            Return True
        End Function
    End Class
    Public Class RemoveEngineModuleCommand
        Implements ICommand

        Public Event CanExecuteChanged As EventHandler Implements ICommand.CanExecuteChanged

        Sub New()
            AddHandler CommandManager.RequerySuggested, New EventHandler(Sub(sender As Object, e As EventArgs)
                                                                             RaiseEvent CanExecuteChanged(Me, New EventArgs())
                                                                         End Sub)
        End Sub

        Public Sub Execute(parameter As Object) Implements ICommand.Execute
            SharedProperties.Instance?.Player?.Modules.Remove(TryCast(parameter, QuickBeat.Player.EngineModule))
        End Sub

        Public Function CanExecute(parameter As Object) As Boolean Implements ICommand.CanExecute
            Return (Not IsNothing(parameter) AndAlso TypeOf parameter Is QuickBeat.Player.EngineModule)
        End Function
    End Class
    Public Class ConfigEngineModuleCommand
        Implements ICommand

        Public Event CanExecuteChanged As EventHandler Implements ICommand.CanExecuteChanged

        Sub New()
            AddHandler CommandManager.RequerySuggested, New EventHandler(Sub(sender As Object, e As EventArgs)
                                                                             RaiseEvent CanExecuteChanged(Me, New EventArgs())
                                                                         End Sub)
        End Sub

        Public Sub Execute(parameter As Object) Implements ICommand.Execute
            TryCast(parameter, QuickBeat.Player.EngineModule)?.Config()
        End Sub

        Public Function CanExecute(parameter As Object) As Boolean Implements ICommand.CanExecute
            Return (Not IsNothing(parameter) AndAlso TypeOf parameter Is QuickBeat.Player.EngineModule)
        End Function
    End Class
    Public Class ShowMetadataInfoCommand
        Implements ICommand

        Public Event CanExecuteChanged As EventHandler Implements ICommand.CanExecuteChanged
        Sub New()
            AddHandler CommandManager.RequerySuggested, New EventHandler(Sub(sender As Object, e As EventArgs)
                                                                             RaiseEvent CanExecuteChanged(Me, New EventArgs())
                                                                         End Sub)
        End Sub

        Public Sub Execute(parameter As Object) Implements ICommand.Execute
            With CType(parameter, Metadata)
                HandyControl.Controls.MessageBox.Info($"Title: { .Title}{Environment.NewLine}Artists: {String.Join(";", .Artists)}{Environment.NewLine}Album: { .Album}{Environment.NewLine}Genres: {String.Join(";", If(.Genres, New String() {}))}{Environment.NewLine}Length: { .LengthString}{Environment.NewLine}Path: { .Path}{Environment.NewLine}Playcount: { .PlayCount}", ResourceResolver.Strings.APPNAME)
            End With
        End Sub

        Public Function CanExecute(parameter As Object) As Boolean Implements ICommand.CanExecute
            Return (TypeOf parameter Is Metadata) AndAlso (parameter IsNot Nothing)
        End Function
    End Class
    Public Class SavePlaylistCommand
        Implements ICommand

        Public Event CanExecuteChanged As EventHandler Implements ICommand.CanExecuteChanged
        Sub New()
            AddHandler CommandManager.RequerySuggested, New EventHandler(Sub(sender As Object, e As EventArgs)
                                                                             RaiseEvent CanExecuteChanged(Me, New EventArgs())
                                                                         End Sub)
        End Sub

        Public Sub Execute(parameter As Object) Implements ICommand.Execute
            Dim cPlaylist = TryCast(parameter, Playlist)?.Clone
            cPlaylist.Cover = Utilities.CommonFunctions.ToCoverImage(cPlaylist.Name)
            SharedProperties.Instance?.CustomPlaylists.Add(cPlaylist)
        End Sub

        Public Function CanExecute(parameter As Object) As Boolean Implements ICommand.CanExecute
            Return (parameter IsNot Nothing) AndAlso (TypeOf parameter Is Playlist)
        End Function
    End Class
    Public Class ChangePlaylistNameCommand
        Implements ICommand

        Public Event CanExecuteChanged As EventHandler Implements ICommand.CanExecuteChanged
        Sub New()
            AddHandler CommandManager.RequerySuggested, New EventHandler(Sub(sender As Object, e As EventArgs)
                                                                             RaiseEvent CanExecuteChanged(Me, New EventArgs())
                                                                         End Sub)
        End Sub

        Public Sub Execute(parameter As Object) Implements ICommand.Execute
            Dim psname = Dialogs.InputBox.ShowSingle(Utilities.ResourceResolver.Strings.NAME)
            If Not String.IsNullOrEmpty(psname) Then
                TryCast(parameter, Playlist).Name = psname
                TryCast(parameter, Playlist).Cover = psname.ToCoverImage
            End If
        End Sub

        Public Function CanExecute(parameter As Object) As Boolean Implements ICommand.CanExecute
            Return (parameter IsNot Nothing) AndAlso (TryCast(parameter, Playlist) IsNot Nothing)
        End Function
    End Class
    Public Class RefreshMetadataCoverCommand
        Implements ICommand

        Public Event CanExecuteChanged As EventHandler Implements ICommand.CanExecuteChanged
        Sub New()
            AddHandler CommandManager.RequerySuggested, New EventHandler(Sub(sender As Object, e As EventArgs)
                                                                             RaiseEvent CanExecuteChanged(Me, New EventArgs())
                                                                         End Sub)
        End Sub

        Public Sub Execute(parameter As Object) Implements ICommand.Execute
            TryCast(parameter, Metadata)?.EnsureCovers()
        End Sub

        Public Function CanExecute(parameter As Object) As Boolean Implements ICommand.CanExecute
            If (parameter IsNot Nothing) AndAlso (TypeOf parameter Is Metadata) Then
                Return IsNothing(CType(parameter, Metadata).Covers)
            End If
            Return False
        End Function
    End Class
    Public Class AddToPlaylistFromPickerCommand
        Implements ICommand

        Public Event CanExecuteChanged As EventHandler Implements ICommand.CanExecuteChanged

        Sub New()
            AddHandler CommandManager.RequerySuggested, New EventHandler(Sub(sender As Object, e As EventArgs)
                                                                             RaiseEvent CanExecuteChanged(Me, New EventArgs())
                                                                         End Sub)
        End Sub

        Public Sub Execute(parameter As Object) Implements ICommand.Execute
            Dim DLG As New Dialogs.PlaylistPicker() With {.Owner = Application.Current.MainWindow}
            If DLG.ShowDialog Then
                DLG.DialogPlaylistResult?.Add(TryCast(parameter, Metadata))
            End If
        End Sub

        Public Function CanExecute(parameter As Object) As Boolean Implements ICommand.CanExecute
            Return (parameter IsNot Nothing) AndAlso (TypeOf parameter Is Metadata)
        End Function
    End Class
    Public Class ClearPlaylistCommand
        Implements ICommand

        Public Event CanExecuteChanged As EventHandler Implements ICommand.CanExecuteChanged

        Sub New()
            AddHandler CommandManager.RequerySuggested, New EventHandler(Sub(sender As Object, e As EventArgs)
                                                                             RaiseEvent CanExecuteChanged(Me, New EventArgs())
                                                                         End Sub)
        End Sub

        Public Sub Execute(parameter As Object) Implements ICommand.Execute
            TryCast(parameter, Playlist)?.Clear()
        End Sub

        Public Function CanExecute(parameter As Object) As Boolean Implements ICommand.CanExecute
            Return (parameter IsNot Nothing) AndAlso (TypeOf parameter Is Playlist)
        End Function
    End Class
    Public Class NewPlaylistCommand
        Implements ICommand

        Public Event CanExecuteChanged As EventHandler Implements ICommand.CanExecuteChanged
        Private _Player As Player

        Sub New(player As Player)
            _Player = player
            AddHandler CommandManager.RequerySuggested, New EventHandler(Sub(sender As Object, e As EventArgs)
                                                                             RaiseEvent CanExecuteChanged(Me, New EventArgs())
                                                                         End Sub)
        End Sub

        Public Sub Execute(parameter As Object) Implements ICommand.Execute
            _Player?.LoadPlaylist(New Playlist With {.Cover = New BitmapImage(New Uri("\Resources\MusicRecord.png", UriKind.Relative))})
        End Sub

        Public Function CanExecute(parameter As Object) As Boolean Implements ICommand.CanExecute
            Return True
        End Function
    End Class
    Public Class SetOutputCommand
        Implements ICommand

        Public Event CanExecuteChanged As EventHandler Implements ICommand.CanExecuteChanged
        Private _vfx As VideoEffects.VideoEffect

        Sub New(vfx As VideoEffects.VideoEffect)
            _vfx = vfx
            AddHandler CommandManager.RequerySuggested, New EventHandler(Sub(sender As Object, e As EventArgs)
                                                                             RaiseEvent CanExecuteChanged(Me, New EventArgs())
                                                                         End Sub)
        End Sub

        Public Sub Execute(parameter As Object) Implements ICommand.Execute
            _vfx.Output = TryCast(parameter, Image)
        End Sub

        Public Function CanExecute(parameter As Object) As Boolean Implements ICommand.CanExecute
            Return (TypeOf parameter Is Image)
        End Function

    End Class
    Public Class SetWidthCommand
        Implements ICommand

        Public Event CanExecuteChanged As EventHandler Implements ICommand.CanExecuteChanged
        Private _vfx As VideoEffects.VideoEffect

        Sub New(vfx As VideoEffects.VideoEffect)
            _vfx = vfx
            AddHandler CommandManager.RequerySuggested, New EventHandler(Sub(sender As Object, e As EventArgs)
                                                                             RaiseEvent CanExecuteChanged(Me, New EventArgs())
                                                                         End Sub)
        End Sub

        Public Sub Execute(parameter As Object) Implements ICommand.Execute
            _vfx.Width = CDbl(parameter)
        End Sub

        Public Function CanExecute(parameter As Object) As Boolean Implements ICommand.CanExecute
            Dim ResV As Double
            Return Double.TryParse(parameter.ToString, ResV)
        End Function
    End Class
    Public Class SetHeightCommand
        Implements ICommand

        Public Event CanExecuteChanged As EventHandler Implements ICommand.CanExecuteChanged
        Private _vfx As VideoEffects.VideoEffect

        Sub New(vfx As VideoEffects.VideoEffect)
            _vfx = vfx
            AddHandler CommandManager.RequerySuggested, New EventHandler(Sub(sender As Object, e As EventArgs)
                                                                             RaiseEvent CanExecuteChanged(Me, New EventArgs())
                                                                         End Sub)
        End Sub

        Public Sub Execute(parameter As Object) Implements ICommand.Execute
            _vfx.Height = CDbl(parameter)
        End Sub

        Public Function CanExecute(parameter As Object) As Boolean Implements ICommand.CanExecute
            Dim ResH As Double
            Return Double.TryParse(parameter.ToString, ResH)
        End Function
    End Class
    Public Class SetResolutionCommand
        Implements ICommand

        Public Event CanExecuteChanged As EventHandler Implements ICommand.CanExecuteChanged
        Private _vfx As VideoEffects.VideoEffect

        Sub New(vfx As VideoEffects.VideoEffect)
            _vfx = vfx
            AddHandler CommandManager.RequerySuggested, New EventHandler(Sub(sender As Object, e As EventArgs)
                                                                             RaiseEvent CanExecuteChanged(Me, New EventArgs())
                                                                         End Sub)
        End Sub

        Public Sub Execute(parameter As Object) Implements ICommand.Execute
            Dim res = parameter.ToString.ToLower.Split("x")
            _vfx.Width = res.FirstOrDefault
            _vfx.Height = res.LastOrDefault
        End Sub

        Public Function CanExecute(parameter As Object) As Boolean Implements ICommand.CanExecute
            Return parameter.ToString.ToLower.Split("x").Length = 2
        End Function
    End Class
    Public Class MoveUpCommand
        Implements ICommand

        Public Event CanExecuteChanged As EventHandler Implements ICommand.CanExecuteChanged
        Private _Player As Player

        Sub New(player As Player)
            _Player = player
            AddHandler CommandManager.RequerySuggested, New EventHandler(Sub(sender As Object, e As EventArgs)
                                                                             RaiseEvent CanExecuteChanged(Me, New EventArgs())
                                                                         End Sub)
        End Sub

        Public Sub Execute(parameter As Object) Implements ICommand.Execute
            If TypeOf parameter Is Metadata Then
                Dim i = TryCast(parameter, Metadata)?.Index
                _Player.Playlist.Move(i, i - 1)
            ElseIf TypeOf parameter Is Integer Then
                _Player.Playlist.Move(parameter, parameter - 1)
            End If
        End Sub

        Public Function CanExecute(parameter As Object) As Boolean Implements ICommand.CanExecute
            Return Not IsNothing(_Player?.Playlist) AndAlso (TypeOf parameter Is Integer OrElse TypeOf parameter Is Metadata)
        End Function
    End Class
    Public Class MoveDownCommand
        Implements ICommand

        Public Event CanExecuteChanged As EventHandler Implements ICommand.CanExecuteChanged
        Private _Player As Player

        Sub New(player As Player)
            _Player = player
            AddHandler CommandManager.RequerySuggested, New EventHandler(Sub(sender As Object, e As EventArgs)
                                                                             RaiseEvent CanExecuteChanged(Me, New EventArgs())
                                                                         End Sub)
        End Sub

        Public Sub Execute(parameter As Object) Implements ICommand.Execute
            If TypeOf parameter Is Metadata Then
                Dim i = TryCast(parameter, Metadata)?.Index
                _Player.Playlist.Move(i, i + 1)
            ElseIf TypeOf parameter Is Integer Then
                _Player.Playlist.Move(parameter, parameter + 1)
            End If
        End Sub

        Public Function CanExecute(parameter As Object) As Boolean Implements ICommand.CanExecute
            Return Not IsNothing(_Player?.Playlist) AndAlso (TypeOf parameter Is Integer OrElse TypeOf parameter Is Metadata)
        End Function
    End Class
    Public Class SetACommand
        Implements ICommand

        Public Event CanExecuteChanged As EventHandler Implements ICommand.CanExecuteChanged
        Private _Player As Player

        Sub New(player As Player)
            _Player = player
            AddHandler CommandManager.RequerySuggested, New EventHandler(Sub(sender As Object, e As EventArgs)
                                                                             RaiseEvent CanExecuteChanged(Me, New EventArgs())
                                                                         End Sub)
        End Sub

        Public Sub Execute(parameter As Object) Implements ICommand.Execute
            If _Player.IsABLoop Then
                _Player.ABLoop.A = parameter
            Else
                _Player.ABLoop = New Player.ABLoopHandle() With {.A = parameter, .B = _Player.Length}
            End If
        End Sub

        Public Function CanExecute(parameter As Object) As Boolean Implements ICommand.CanExecute
            Return Not IsNothing(_Player) AndAlso TypeOf parameter Is Double
        End Function
    End Class
    Public Class SetBCommand
        Implements ICommand

        Public Event CanExecuteChanged As EventHandler Implements ICommand.CanExecuteChanged
        Private _Player As Player

        Sub New(player As Player)
            _Player = player
            AddHandler CommandManager.RequerySuggested, New EventHandler(Sub(sender As Object, e As EventArgs)
                                                                             RaiseEvent CanExecuteChanged(Me, New EventArgs())
                                                                         End Sub)
        End Sub

        Public Sub Execute(parameter As Object) Implements ICommand.Execute
            If _Player.IsABLoop Then
                _Player.ABLoop.B = parameter
            End If
        End Sub

        Public Function CanExecute(parameter As Object) As Boolean Implements ICommand.CanExecute
            Return Not IsNothing(_Player?.ABLoop) AndAlso _Player.ABLoop.B <> 0 AndAlso TypeOf parameter Is Double
        End Function
    End Class
    Public Class SyncWithPlaylistCommand
        Implements ICommand

        Public Event CanExecuteChanged As EventHandler Implements ICommand.CanExecuteChanged
        Private _Player As Player

        Sub New(player As Player)
            _Player = player
            AddHandler CommandManager.RequerySuggested, New EventHandler(Sub(sender As Object, e As EventArgs)
                                                                             RaiseEvent CanExecuteChanged(Me, New EventArgs())
                                                                         End Sub)
        End Sub

        Public Async Sub Execute(parameter As Object) Implements ICommand.Execute
            Await _Player.LoadSong(_Player.Playlist?.CurrentItem)
        End Sub

        Public Function CanExecute(parameter As Object) As Boolean Implements ICommand.CanExecute
            Return Not IsNothing(_Player) AndAlso _Player.Path <> _Player.Playlist?.CurrentItem?.Path
        End Function
    End Class
    Public Class RefreshDevicesCommand
        Implements ICommand

        Public Event CanExecuteChanged As EventHandler Implements ICommand.CanExecuteChanged

        Sub New()
            AddHandler CommandManager.RequerySuggested, New EventHandler(Sub(sender As Object, e As EventArgs)
                                                                             RaiseEvent CanExecuteChanged(Me, New EventArgs())
                                                                         End Sub)
        End Sub

        Public Sub Execute(parameter As Object) Implements ICommand.Execute
            TryCast(parameter, Player)?.RefreshDevices()
        End Sub

        Public Function CanExecute(parameter As Object) As Boolean Implements ICommand.CanExecute
            Return parameter IsNot Nothing AndAlso TypeOf parameter Is Player
        End Function
    End Class
    Public Class FileTagAddCoverCommand
        Implements ICommand

        Public Event CanExecuteChanged As EventHandler Implements ICommand.CanExecuteChanged

        Sub New()
            AddHandler CommandManager.RequerySuggested, New EventHandler(Sub(sender As Object, e As EventArgs)
                                                                             RaiseEvent CanExecuteChanged(Me, New EventArgs())
                                                                         End Sub)
        End Sub

        Public Sub Execute(parameter As Object) Implements ICommand.Execute
            Dim DLG As New Dialogs.TagPictureDialog(Application.Current.MainWindow)
            If DLG.ShowDialog Then
                CType(parameter, FileTag).Covers.Add(DLG.Result)
            End If
        End Sub

        Public Function CanExecute(parameter As Object) As Boolean Implements ICommand.CanExecute
            Return parameter IsNot Nothing AndAlso TypeOf parameter Is FileTag
        End Function
    End Class

    Public Class FileTagClearCoversCommand
        Implements ICommand

        Public Event CanExecuteChanged As EventHandler Implements ICommand.CanExecuteChanged

        Sub New()
            AddHandler CommandManager.RequerySuggested, New EventHandler(Sub(sender As Object, e As EventArgs)
                                                                             RaiseEvent CanExecuteChanged(Me, New EventArgs())
                                                                         End Sub)
        End Sub

        Public Sub Execute(parameter As Object) Implements ICommand.Execute
            TryCast(parameter, FileTag).Covers?.Clear()
        End Sub

        Public Function CanExecute(parameter As Object) As Boolean Implements ICommand.CanExecute
            Return parameter IsNot Nothing AndAlso TypeOf parameter Is FileTag
        End Function
    End Class

    Public Class FileTagRemoveCoverCommand
        Implements ICommand

        Public Event CanExecuteChanged As EventHandler Implements ICommand.CanExecuteChanged

        Sub New()
            AddHandler CommandManager.RequerySuggested, New EventHandler(Sub(sender As Object, e As EventArgs)
                                                                             RaiseEvent CanExecuteChanged(Me, New EventArgs())
                                                                         End Sub)
        End Sub

        Public Sub Execute(parameter As Object) Implements ICommand.Execute
            Dim _i = Dialogs.InputBox.ShowSingle(Application.Current.MainWindow, Utilities.ResourceResolver.Strings.INDEX)
            Dim i = 0
            If Integer.TryParse(_i, i) Then
                CType(parameter, FileTag).Covers.RemoveAt(i)
            End If
        End Sub

        Public Function CanExecute(parameter As Object) As Boolean Implements ICommand.CanExecute
            Return parameter IsNot Nothing AndAlso TypeOf parameter Is FileTag
        End Function
    End Class

    Public Class FileTagSaveCommand
        Implements ICommand

        Public Event CanExecuteChanged As EventHandler Implements ICommand.CanExecuteChanged

        Sub New()
            AddHandler CommandManager.RequerySuggested, New EventHandler(Sub(sender As Object, e As EventArgs)
                                                                             RaiseEvent CanExecuteChanged(Me, New EventArgs())
                                                                         End Sub)
        End Sub

        Public Sub Execute(parameter As Object) Implements ICommand.Execute
            TryCast(parameter, FileTag)?.Save()
        End Sub

        Public Function CanExecute(parameter As Object) As Boolean Implements ICommand.CanExecute
            Return parameter IsNot Nothing AndAlso TypeOf parameter Is FileTag
        End Function
    End Class

    Public Class FileTagMoveCoverCommand
        Implements ICommand

        Public Event CanExecuteChanged As EventHandler Implements ICommand.CanExecuteChanged

        Sub New()
            AddHandler CommandManager.RequerySuggested, New EventHandler(Sub(sender As Object, e As EventArgs)
                                                                             RaiseEvent CanExecuteChanged(Me, New EventArgs())
                                                                         End Sub)
        End Sub

        Public Sub Execute(parameter As Object) Implements ICommand.Execute
            Dim IB As New Dialogs.InputBox With {.Owner = Application.Current.MainWindow, .WindowStartupLocation = WindowStartupLocation.CenterOwner}
            IB.AddTextBox(Utilities.ResourceResolver.Strings.FROM)
            IB.AddTextBox(Utilities.ResourceResolver.Strings.TO)
            If Not IB.ShowDialog Then Return
            Dim _i = IB.Value(Utilities.ResourceResolver.Strings.FROM)
            Dim _j = IB.Value(Utilities.ResourceResolver.Strings.TO)
            Dim i = 0
            Dim j = 0
            If Integer.TryParse(_i, i) Then
                If Integer.TryParse(_j, j) Then
                    If i = j OrElse i < 0 OrElse i >= TryCast(parameter, FileTag)?.Covers?.Count OrElse j < 0 OrElse j >= TryCast(parameter, FileTag)?.Covers?.Count Then Return
                    CType(parameter, FileTag).Covers.Move(i, j)
                End If
            End If
        End Sub

        Public Function CanExecute(parameter As Object) As Boolean Implements ICommand.CanExecute
            Return parameter IsNot Nothing AndAlso TypeOf parameter Is FileTag
        End Function
    End Class
    Public Class FileTagCopyCoverCommand
        Implements ICommand

        Public Event CanExecuteChanged As EventHandler Implements ICommand.CanExecuteChanged

        Sub New()
            AddHandler CommandManager.RequerySuggested, New EventHandler(Sub(sender As Object, e As EventArgs)
                                                                             RaiseEvent CanExecuteChanged(Me, New EventArgs())
                                                                         End Sub)
        End Sub

        Public Sub Execute(parameter As Object) Implements ICommand.Execute
            Dim _i = Dialogs.InputBox.ShowSingle(Application.Current.MainWindow, Utilities.ResourceResolver.Strings.INDEX)
            Dim i = 0
            If Integer.TryParse(_i, i) Then
                If i < 0 OrElse i >= TryCast(parameter, FileTag)?.Covers?.Count Then Return
                Dim Cover = CType(parameter, FileTag).Covers.Item(i)
                Dim BI As New BitmapImage
                BI.BeginInit()
                BI.StreamSource = New IO.MemoryStream(Cover.Data.Data)
                BI.EndInit()
                Clipboard.SetImage(BI)
            End If
        End Sub

        Public Function CanExecute(parameter As Object) As Boolean Implements ICommand.CanExecute
            Return parameter IsNot Nothing AndAlso TypeOf parameter Is FileTag
        End Function
    End Class

    ''' <summary>
    ''' Uses <see cref="Utilities.Commands.ViewImageCommand"/>
    ''' </summary>
    Public Class FileTagViewCoverCommand
        Implements ICommand

        Public Event CanExecuteChanged As EventHandler Implements ICommand.CanExecuteChanged

        Sub New()
            AddHandler CommandManager.RequerySuggested, New EventHandler(Sub(sender As Object, e As EventArgs)
                                                                             RaiseEvent CanExecuteChanged(Me, New EventArgs())
                                                                         End Sub)
        End Sub

        Public Sub Execute(parameter As Object) Implements ICommand.Execute
            Dim _i = Dialogs.InputBox.ShowSingle(Application.Current.MainWindow, Utilities.ResourceResolver.Strings.INDEX)
            Dim i = 0
            If Integer.TryParse(_i, i) Then
                If i < 0 OrElse i >= TryCast(parameter, FileTag)?.Covers?.Count Then Return
                Dim Cover = CType(parameter, FileTag).Covers.Item(i)
                Dim BI As New BitmapImage
                BI.BeginInit()
                BI.StreamSource = New IO.MemoryStream(Cover.Data.Data)
                BI.EndInit()
                Utilities.Commands.ViewImageCommand.Execute(BI)
            End If
        End Sub

        Public Function CanExecute(parameter As Object) As Boolean Implements ICommand.CanExecute
            Return parameter IsNot Nothing AndAlso TypeOf parameter Is FileTag
        End Function
    End Class
End Namespace
