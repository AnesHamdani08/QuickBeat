Imports System.Collections.Specialized
Imports System.ComponentModel
Imports QuickBeat.Hotkeys

Namespace Utilities
    Public Class SharedProperties
        Implements INotifyPropertyChanged

        Delegate Sub ShowNotificationDelegate(Title As String, Message As String, Icon As HandyControl.Data.NotifyIconInfoType)

        Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged

        Public Shared Property Instance As New SharedProperties

        Public Property IsLogging As Boolean = False

        Private WithEvents _Player As Player.Player
        Public Property Player As Player.Player
            Get
                Return _Player
            End Get
            Set(value As Player.Player)
                _Player = value
                OnPropertyChanged()
                RecheckAllInitialized()
            End Set
        End Property

        Private _YoutubeDL As New Youtube.YoutubeDL()
        Public Property YoutubeDL As Youtube.YoutubeDL
            Get
                Return _YoutubeDL
            End Get
            Set(value As Youtube.YoutubeDL)
                _YoutubeDL = value
                OnPropertyChanged()
                RecheckAllInitialized()
            End Set
        End Property


        Private WithEvents _Library As New Library.Library
        Public Property Library As Library.Library
            Get
                Return _Library
            End Get
            Set(value As Library.Library)
                _Library = value
                OnPropertyChanged()
                RecheckAllInitialized()
            End Set
        End Property

        Private _HotkeyManager As Hotkeys.HotkeyManager
        Public Property HotkeyManager As Hotkeys.HotkeyManager
            Get
                Return _HotkeyManager
            End Get
            Set(value As Hotkeys.HotkeyManager)
                _HotkeyManager = value
                OnPropertyChanged()
                RecheckAllInitialized()
            End Set
        End Property

        Private _CustomPlaylists As New ObjectModel.ObservableCollection(Of Player.Playlist)
        Public ReadOnly Property CustomPlaylists As ObjectModel.ObservableCollection(Of Player.Playlist)
            Get
                Return _CustomPlaylists
            End Get
        End Property

        Private _RecommendedPlaylists As New ObjectModel.ObservableCollection(Of Player.Playlist)
        Public ReadOnly Property RecommendedPlaylists As ObjectModel.ObservableCollection(Of Player.Playlist)
            Get
                Return _RecommendedPlaylists
            End Get
        End Property

        Private _MemoryManager As New Classes.MemoryManager
        Property MemoryManager As Classes.MemoryManager
            Get
                Return _MemoryManager
            End Get
            Set(value As Classes.MemoryManager)
                _MemoryManager = value
                OnPropertyChanged()
            End Set
        End Property

        Private _SleepTimer As New Classes.SleepTimer
        Property SleepTimer As Classes.SleepTimer
            Get
                Return _SleepTimer
            End Get
            Set(value As Classes.SleepTimer)
                _SleepTimer = value
                OnPropertyChanged()
            End Set
        End Property

        Private _Themes As New ObjectModel.ObservableCollection(Of Type) From {GetType(Classes.Themes.Default), GetType(Classes.Themes.Neon), GetType(Classes.Themes.Aqua), GetType(Classes.Themes.Marine)}
        Property Themes As ObjectModel.ObservableCollection(Of Type)
            Get
                Return _Themes
            End Get
            Set(value As ObjectModel.ObservableCollection(Of Type))
                _Themes = value
                OnPropertyChanged()
            End Set
        End Property

        Private _ThemesSelectedIndex As Integer = 0
        Property ThemesSelectedIndex As Integer
            Get
                Return _ThemesSelectedIndex
            End Get
            Set(value As Integer)
                If value >= 0 AndAlso value < Themes.Count Then
                    _ThemesSelectedIndex = value
                    OnPropertyChanged()
                    Theme = TryCast(Activator.CreateInstance(Themes(value)), Classes.Themes.Theme)
                End If
            End Set
        End Property

        Private _Theme As Classes.Themes.Theme
        Property Theme As Classes.Themes.Theme
            Get
                Return _Theme
            End Get
            Set(value As Classes.Themes.Theme)
                _Theme?.Dispose()
                _Theme = value
                HandyControl.Themes.PresetManager.Current.ColorPreset = If(value IsNot Nothing, value.GetPreset, Nothing)
                OnPropertyChanged()
            End Set
        End Property

        Private _IsInitialized As Boolean
        Public Property IsInitialized As Boolean
            Get
                Return _IsInitialized
            End Get
            Set(value As Boolean)
                _IsInitialized = value
                OnPropertyChanged()
            End Set
        End Property

        Private _ItemsConfiguration As New ObjectModel.ObservableCollection(Of Classes.StartupItemConfiguration)
        Property ItemsConfiguration As ObjectModel.ObservableCollection(Of Classes.StartupItemConfiguration)
            Get
                Return _ItemsConfiguration
            End Get
            Set(value As ObjectModel.ObservableCollection(Of Classes.StartupItemConfiguration))
                _ItemsConfiguration = value
                OnPropertyChanged()
            End Set
        End Property

        Private _IsInternetConnected As Boolean = Utilities.CheckInternetConnection
        Property IsInternetConnected As Boolean
            Get
                Return _IsInternetConnected
            End Get
            Set(value As Boolean)
                _IsInternetConnected = value
                OnPropertyChanged()
            End Set
        End Property

        ReadOnly Property Aqua As New Aqua.Aqua With {.ReferenceBinder = New QScript.KnownReferenceBinder}

        ReadOnly Property CurrentGreeting As String
            Get
                Select Case Now.Hour
                    Case > 22
                        Return ResourceResolver.Strings.GREETING_NIGHT
                    Case > 18
                        Return ResourceResolver.Strings.GREETING_EVENING
                    Case > 12
                        Return ResourceResolver.Strings.GREETING_AFTERNOON
                    Case > 6
                        Return ResourceResolver.Strings.GREETING_MORNING
                End Select
                Return "Welcome Back"
            End Get
        End Property

        Public Property ShowNotification As ShowNotificationDelegate
        Public Property LibraryPaths As New Collections.SyncedObservableCollection(Of String)(New Collections.SyncedObservableCollection(Of String).SyncStart(Sub(sender, item, type)
                                                                                                                                                                  Select Case type
                                                                                                                                                                      Case CollectionChangeAction.Add
                                                                                                                                                                          If My.Settings.APP_LIBRARY_PATHS.Contains(item) Then Return
                                                                                                                                                                          My.Settings.APP_LIBRARY_PATHS.Add(item)
                                                                                                                                                                          My.Settings.Save()
                                                                                                                                                                      Case CollectionChangeAction.Remove
                                                                                                                                                                          My.Settings.APP_LIBRARY_PATHS.Remove(item)
                                                                                                                                                                          My.Settings.Save()
                                                                                                                                                                      Case CollectionChangeAction.Refresh
                                                                                                                                                                          My.Settings.APP_LIBRARY_PATHS.Clear()
                                                                                                                                                                          For Each path In LibraryPaths
                                                                                                                                                                              My.Settings.APP_LIBRARY_PATHS.Add(path)
                                                                                                                                                                          Next
                                                                                                                                                                          My.Settings.Save()
                                                                                                                                                                  End Select
                                                                                                                                                              End Sub), My.Settings.APP_LIBRARY_PATHS)

        Private WithEvents _FilePipe As New Classes.NamedPipeManager("QuickBeatPump", IO.Pipes.PipeDirection.InOut)
        Private WithEvents _Timer As New Forms.Timer With {.Interval = 5000} 'Low freq. timer, used for lazy notifications purposes e.g internet state
        Public Property FilePipe As Classes.NamedPipeManager
            Get
                Return _FilePipe
            End Get
            Set(value As Classes.NamedPipeManager)
                _FilePipe = value
                OnPropertyChanged()
            End Set
        End Property

        Public ReadOnly Property AllInitialized As Boolean
            Get
                Return Not ItemsConfiguration.Any(Function(k) k.IsLoaded = False)
            End Get
        End Property

        Async Sub Init()
            Utilities.DebugMode.Instance.Log(Of SharedProperties)("Attempting to initialize...")
            If IsInitialized Then
                Utilities.DebugMode.Instance.Log(Of SharedProperties)("Already initialized.")
                Return
            End If
            'Init
            Utilities.DebugMode.Instance.Log(Of SharedProperties)("Starting Timer")
            _Timer.Start()
            Utilities.DebugMode.Instance.Log(Of SharedProperties)("Setting theme...")
            If Theme Is Nothing Then
                'Theme = New Classes.Themes.Default
                ThemesSelectedIndex = My.Settings.APP_THEME_INDEX
            End If
            Utilities.DebugMode.Instance.Log(Of SharedProperties)("Loading library...")
            Library.Load(My.Settings.APP_LIBRARY)
            If Library.Configuration.IsLoaded Then
                If Library.MostPlayedArtist IsNot Nothing Then
                    Dim playlist = Library.MostPlayedArtist.ToPlaylist(Library.MostPlayedArtist.Cover)
                    playlist.Description = "Recommendation"
                    RecommendedPlaylists.Add(playlist)
                End If
                If Library.MostPlayedArtist2 IsNot Nothing Then
                    Dim playlist = Library.MostPlayedArtist2.ToPlaylist(Library.MostPlayedArtist2.Cover)
                    playlist.Description = "Recommendation"
                    RecommendedPlaylists.Add(playlist)
                End If
                If Library.MostPlayedArtist3 IsNot Nothing Then
                    Dim playlist = Library.MostPlayedArtist3.ToPlaylist(Library.MostPlayedArtist3.Cover)
                    playlist.Description = "Recommendation"
                    RecommendedPlaylists.Add(playlist)
                End If
                Dim fav = Library.Where(Function(k) k.IsFavorite).ToPlaylist
                fav.Tag = "Favorite"
                Try
                    fav.Name = Utilities.ResourceResolver.Strings.FAVORITES
                    fav.Description = "Your favourite songs."
                    'fav.Cover = ResourceResolver.Images.HEARTS.ToImageSource(New SolidColorBrush(Color.FromRgb(255, 193, 7)), New Pen(New SolidColorBrush(Color.FromRgb(255, 193, 7)), 2))
                    'Dim dv As New DrawingVisual()
                    'Dim dc = dv.RenderOpen
                    'Dim recBrush As New LinearGradientBrush(New GradientStopCollection From {New GradientStop(Color.FromRgb(38, 50, 56), 0), New GradientStop(Color.FromRgb(69, 90, 100), 1)}, 45)
                    'dc.DrawRoundedRectangle(recBrush, Nothing, New Rect(0, 0, 100, 100), 15, 15)
                    'dc.DrawImage(ResourceResolver.Images.HEARTS.ToImageSource(New SolidColorBrush(Color.FromRgb(255, 193, 7)), Nothing), New Rect(10, 10, 80, 80))
                    'dc.Close()
                    fav.Cover = CommonFunctions.GenerateCoverImage(ResourceResolver.Images.HEARTS)
                    RecommendedPlaylists.Add(fav)
                Catch ex As Exception
                    Utilities.DebugMode.Instance.Log(Of SharedProperties)("Failed to load custom playlist from settings. Error:=" & ex.ToString)
                End Try
            End If
            Utilities.DebugMode.Instance.Log(Of SharedProperties)("Loading custom playlists...")
            Dim BinF As New System.Runtime.Serialization.Formatters.Binary.BinaryFormatter
            For Each playlist In My.Settings.APP_PLAYER_PLAYLIST_CUSTOM
                If Not String.IsNullOrEmpty(playlist) Then
                    Try
                        Dim Pl64 = Convert.FromBase64String(playlist)
                        Dim Pl As Player.Playlist = BinF.Deserialize(New IO.MemoryStream(Pl64))
                        Pl.Category = ResourceResolver.Strings.CUSTOM
                        'Dim dv As New DrawingVisual()
                        'Dim dc = dv.RenderOpen
                        'Dim recBrush As New LinearGradientBrush(New GradientStopCollection From {New GradientStop(Color.FromRgb(38, 50, 56), 0), New GradientStop(Color.FromRgb(69, 90, 100), 1)}, 45)
                        'dc.DrawRoundedRectangle(recBrush, Nothing, New Rect(0, 0, 100, 100), 15, 15)
                        'Dim txt = New FormattedText(Pl.Name.FirstOrDefault & Pl.Name.LastOrDefault, Globalization.CultureInfo.CurrentCulture, FlowDirection.LeftToRight, New Typeface(New FontFamily("Arial"), FontStyles.Normal, FontWeights.DemiBold, FontStretches.Normal), 132 / 2, New SolidColorBrush(Color.FromRgb(255, 193, 7))) '2 for text length                        
                        'dc.DrawText(txt, New Point((100 - txt.Width) / 2, (100 - txt.Height) / 2))
                        'dc.Close()
                        Pl.Cover = Pl.Name.ToCoverImage
                        CustomPlaylists.Add(Pl)
                    Catch ex As Exception
                        Utilities.DebugMode.Instance.Log(Of SharedProperties)("Failed to load custom playlist from settings. Error:=" & ex.ToString)
                    End Try
                End If
            Next
            Dim RecentSongs As New Player.Playlist() With {.Name = "Recently Played", .Category = "Playlist", .Description = "Your last played songs", .Tag = "Recent"}
            RecentSongs.Cover = CommonFunctions.GenerateCoverImage(ResourceResolver.Images.CLOCK)
            For Each song In My.Settings.APP_PLAYER_RECENT
                If String.IsNullOrEmpty(song) Then Continue For
                Try
                    Dim MD64 = Convert.FromBase64String(song)
                    Dim MD As Player.Metadata = BinF.Deserialize(New IO.MemoryStream(MD64))
                    RecentSongs.Add(If(QuickBeat.Player.Metadata.FromFile(MD.Path, False, True), MD))
                Catch ex As Exception
                    Utilities.DebugMode.Instance.Log(Of SharedProperties)("Failed to load recent song from settings. Error:=" & ex.ToString)
                End Try
            Next
            RecommendedPlaylists.Add(RecentSongs)
            Utilities.DebugMode.Instance.Log(Of SharedProperties)("Initializing player...")
            Player = New Player.Player With {.TaskbarOverlayPlaying = New BitmapImage(New Uri("pack://application:,,,/QuickBeat;component/Resources/CircledPlay.png")), .TaskbarOverlayPaused = New BitmapImage(New Uri("pack://application:,,,/QuickBeat;component/Resources/PauseButton.png")), .TaskbarOverlayStopped = New BitmapImage(New Uri("pack://application:,,,/QuickBeat;component/Resources/StopCircled.png"))}
            Player.Init()
            Player.LoadSettings(My.Settings.APP_PLAYER_SETTINGS)
            If Not String.IsNullOrEmpty(My.Settings.APP_PLAYER_PLAYLIST) Then
                Try
                    Dim Pl64 = Convert.FromBase64String(My.Settings.APP_PLAYER_PLAYLIST)
                    Player.LoadPlaylist(New IO.MemoryStream(Pl64))
                    'Replace instance reference to the current one, if left alone metadata info changing wont reflect in the library
                    For i As Integer = 0 To Player.Playlist.Count - 1
                        If Player.Playlist(i).Location = QuickBeat.Player.Metadata.FileLocation.Remote Then Continue For
                        Dim _i = i
                        Dim metaref = Library.FirstOrDefault(Function(k) k.Path = Player.Playlist(_i).Path)
                        If metaref IsNot Nothing Then
                            Player.Playlist(i) = metaref
                        End If
                    Next
                    Player.Playlist.RefreshItemsIndex()
                Catch ex As Exception
                    Utilities.DebugMode.Instance.Log(Of SharedProperties)("Failed to load playlist from settings. Error:=" & ex.ToString)
                    Player.Configuration.SetError(True, New ArgumentException("Failed to load playlist."))
                End Try
            End If
            Utilities.DebugMode.Instance.Log(Of SharedProperties)("Initializing hotkey manager...")
            If String.IsNullOrEmpty(My.Settings.APP_HOTKEYS) Then
                HotkeyManager = New HotkeyManager(IntPtr.Zero)
                HotkeyManager.Configuration.SetStatus("All Good", 100)
            Else
                Try
                    Dim Hk64 = Convert.FromBase64String(My.Settings.APP_HOTKEYS)
                    HotkeyManager = Await Hotkeys.HotkeyManager.Load(New IO.MemoryStream(Hk64))
                Catch ex As Exception
                    Utilities.DebugMode.Instance.Log(Of SharedProperties)("Failed to load custom playlist from settings. Error:=" & ex.ToString)
                    HotkeyManager = New HotkeyManager(IntPtr.Zero)
                    HotkeyManager.Configuration.SetError(True, New ArgumentException("Failed to load hotkeys."))
                End Try
            End If
            Utilities.DebugMode.Instance.Log(Of SharedProperties)("Initializing youtube-dl...")
            If Not String.IsNullOrEmpty(My.Settings.APP_YOUTUBEDL_PATH) Then
                YoutubeDL.YoutubeDLLocation = My.Settings.APP_YOUTUBEDL_PATH
            End If
            'Adding config. items
            With ItemsConfiguration
                .Add(Library.Configuration)
                .Add(Player.Configuration)
                .Add(HotkeyManager.Configuration)
                .Add(FilePipe.Configuration)
                .Add(YoutubeDL.Configuration)
            End With
            MemoryManager.IsPeriodicCleaning = True
            Utilities.DebugMode.Instance.Log(Of SharedProperties)("Initializing file pipe...")
            FilePipe.Init()
            FilePipe.StartServer()
            Utilities.DebugMode.Instance.Log(Of SharedProperties)("SharedProperties.Instance, Initialized.")
            IsInitialized = True
            Dim Args = Environment.GetCommandLineArgs.Skip(1)
            Dim ArgsCount = Args.Count
            If ArgsCount = 1 Then
                Player.ClearPlaylistCommand.Execute(Player.Playlist)
                Player.Playlist.Name = "Explorer"
                Player.LoadAndAddCommand.Execute(Args.FirstOrDefault)
            ElseIf ArgsCount > 1 Then
                Player.ClearPlaylistCommand.Execute(Player.Playlist)
                Player.Playlist.Name = "Explorer"
                For i As Integer = 0 To ArgsCount - 1
                    If i = ArgsCount - 1 Then
                        Player.LoadAndAddCommand.Execute(Args(i))
                    Else
                        Player.Playlist.Add(QuickBeat.Player.Metadata.FromFile(Args(i)))
                    End If
                Next
            End If
        End Sub

        Private Sub OnPropertyChanged(<Runtime.CompilerServices.CallerMemberName> Optional CallerName As String = Nothing)
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(CallerName))
        End Sub

        Sub Notification(Title As String, Message As String, Icon As HandyControl.Data.NotifyIconInfoType)
            ShowNotification?.Invoke(Title, Message, Icon)
        End Sub

        ''' <summary>
        ''' Used to notify the UI. Raises <see cref="PropertyChanged"/> for <see cref="AllInitialized"/>
        ''' </summary>
        Public Sub RecheckAllInitialized()
            OnPropertyChanged(NameOf(AllInitialized))
        End Sub

        Public Sub ActivateApp()
            If Application.Current.MainWindow Is Nothing Then Return
            With Application.Current.MainWindow
                If .WindowState = WindowState.Minimized Then
                    .WindowState = WindowState.Normal
                End If
                .Activate()
                .Topmost = True
                .Topmost = False
                .Focus()
            End With
        End Sub

        Private Sub _FilePipe_ReceiveString(sender As Classes.NamedPipeManager, msg As String) Handles _FilePipe.ReceiveString
            Utilities.DebugMode.Instance.Log(Of SharedProperties)($"Recieved string from {sender.NamedPipeName}: {msg}")
            If msg.ToLower.StartsWith("activate") Then
                ActivateApp()
            ElseIf msg.ToLower.StartsWith("preview ") Then
                Player.PreviewCommand.Execute(QuickBeat.Player.Metadata.FromFile(msg.Remove(0, "preview ".Length)))
                ActivateApp()
            ElseIf msg.ToLower.StartsWith("load ") Then
                Player.LoadCommand.Execute(msg.Remove(0, "load ".Length))
            ElseIf msg.ToLower.StartsWith("appendload ") Then
                Player.LoadAndAddCommand.Execute(msg.Remove(0, "appendload ".Length))
            ElseIf msg.ToLower.StartsWith("append ") Then
                Player.Playlist.Add(QuickBeat.Player.Metadata.FromFile(msg.Remove(0, "append ".Length))) 'lazy :)            
            End If
        End Sub

        Private Sub _Player_MetadataChanged() Handles _Player.MetadataChanged
            If _Player.StreamMetadata.Location = QuickBeat.Player.Metadata.FileLocation.Local Then
                Dim RecentlyPlayed = RecommendedPlaylists.FirstOrDefault(Function(k) k.Tag = "Recent")
                If RecentlyPlayed Is Nothing Then
                    Dim BinF As New Runtime.Serialization.Formatters.Binary.BinaryFormatter
                    Dim RecentSongs As New Player.Playlist() With {.Name = "Recently Played", .Category = "Playlist", .Description = "Your last played songs", .Tag = "Recent"}
                    RecentSongs.Cover = CommonFunctions.GenerateCoverImage(ResourceResolver.Images.CLOCK)
                    For Each song In My.Settings.APP_PLAYER_RECENT
                        If String.IsNullOrEmpty(song) Then Continue For
                        Try
                            Dim MD64 = Convert.FromBase64String(song)
                            Dim MD As Player.Metadata = BinF.Deserialize(New IO.MemoryStream(MD64))
                            RecentSongs.Add(If(QuickBeat.Player.Metadata.FromFile(MD.Path, False, True), MD))
                        Catch ex As Exception
                            Utilities.DebugMode.Instance.Log(Of SharedProperties)("Failed to load recent song from settings. Error:=" & ex.ToString)
                        End Try
                    Next
                    RecommendedPlaylists.Add(RecentSongs)
                    RecentlyPlayed = RecentSongs
                End If
                If RecentlyPlayed IsNot Nothing Then
                    Dim Occurence = RecentlyPlayed.FirstOrDefault(Function(k) k.Path = _Player.StreamMetadata.Path)
                    If Occurence IsNot Nothing Then
                        RecentlyPlayed.RemoveAt(RecentlyPlayed.IndexOf(Occurence))
                    End If
                    RecentlyPlayed.Insert(0, _Player.StreamMetadata)
                    Do While RecentlyPlayed.Count > My.Settings.APP_PLAYER_RECENT_LIMIT
                        RecentlyPlayed.RemoveAt(RecentlyPlayed.Count - 1)
                    Loop
                End If
            End If
            Theme?.OnMediaChanged(_Player.StreamMetadata)
            'CommonFunctions.GetImageStats(_Player.StreamMetadata.DefaultCover)
        End Sub

        Private Sub _Library_FavoriteChanged(sender As Player.Metadata) Handles _Library.FavoriteChanged
            Dim FavoriteSongs = RecommendedPlaylists.FirstOrDefault(Function(k) k.Tag = "Favorite")
            If FavoriteSongs Is Nothing Then
                Dim fav = Library.Where(Function(k) k.IsFavorite).ToPlaylist
                fav.Tag = "Favorite"
                Try
                    fav.Name = Utilities.ResourceResolver.Strings.FAVORITES
                    fav.Description = "Your favourite songs."
                    fav.Cover = CommonFunctions.GenerateCoverImage(ResourceResolver.Images.HEARTS)
                    RecommendedPlaylists.Add(fav)
                    FavoriteSongs = fav
                Catch ex As Exception
                    Utilities.DebugMode.Instance.Log(Of SharedProperties)("Failed to load custom playlist from settings. Error:=" & ex.ToString)
                End Try
            End If
            If sender.IsFavorite Then
                If FavoriteSongs.FirstOrDefault(Function(k) k.Path = sender.Path) IsNot Nothing Then Return
                FavoriteSongs.Add(sender)
            Else
                FavoriteSongs.Remove(FavoriteSongs.FirstOrDefault(Function(k) k.Path = sender.Path))
            End If
        End Sub

        Private Sub _Timer_Tick(sender As Object, e As EventArgs) Handles _Timer.Tick
            IsInternetConnected = Utilities.CheckInternetConnection
            OnPropertyChanged(NameOf(IsInternetConnected))
            If Not Player?.IsPreviewing Then
                Player?.OnPropertyChanged(NameOf(Player.IsPlaying))
                Player?.OnPropertyChanged(NameOf(Player.IsPaused))
                Player?.OnPropertyChanged(NameOf(Player.IsStalled))
                Player?.OnPropertyChanged(NameOf(Player.IsStopped))
            End If
        End Sub
    End Class
End Namespace

'TODO Check CPU usage... No can dosville baby doll it's animation related
'TODO maybe make github docs
'TODO add a video effect like mpc hc cover playlist mode
'TODO add clock while in fullscreen ?
'TODO test deezer integration more, bind queries to library or something
'TODO add audio effect attirbute : control
'TODO add more fx from bass fx
'TODO fix dev console formatting adding space and things
'TODO Add loops , add dirty file check on close
'TODO add logic comparison
'TODO add object browser and type locator
'TODO add custom themes
'TODO maybe add fanart search
'TODO add cache dump 'skip for now , data stream is corrupt , when writing to file it doesnt play
'TODO complete aqua
'TODO try and fix airspace problem
'TODO fix metadata changing on previous stream metadata , remote tags reading kicking in before the streammetadata is changed // shouldn't be a problem since load url is not adding to playlist
'TODO plugins*
'Changelog:
'Improved General App Performance
'Added UPnP/DLNA Support
'Enahced Remote Tag Reading
'Added Playlist Support to "Open With" Operations
'Added Playlist to File/Open
'Added M3U/8 Support for Import and Export Operations
'Input Dialog Can Now Be Closed or Confirmed Using Escape/Enter Keys
'Fixed App Startup with a Remote File
'Removed Controls Lock while Previewing
'Added Merged URL, YouTube-DL, Deezer Under One Menu: Remote
'Added Search Provider for Youtube-DL
'Added Automatic Link Refresh
'Added YoutubeDL Playlist Support (Tested with Youtube and SoundCloud)
'Added Deezer Integration
'Added Tab Selector to Home
'Added MultiBandEqualizer Audio Effect