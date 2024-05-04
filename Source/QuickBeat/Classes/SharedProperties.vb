Imports System.Collections.Specialized
Imports System.ComponentModel
Imports System.Reflection
Imports System.Threading
Imports System.Web.UI.WebControls
Imports DiscordRPC.Message
Imports QuickBeat.Hotkeys
Imports QuickBeat.Library.WPF.Commands
Imports QuickBeat.Player

Namespace Utilities
    Public Class SharedProperties
        Implements INotifyPropertyChanged

        Delegate Sub ShowNotificationDelegate(Title As String, Message As String, Icon As HandyControl.Data.NotifyIconInfoType)
#Region "Events"
        Public Event App_Activated()
        Public Event App_Deactivated()
        Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged
#End Region
#Region "Constants"
        Public Const AppName As String = "QuickBeat"
        Public Const AppCodeName As String = "Ventus 2.1"
        Public Const AppCopyright As String = "Copyright 2022-2024 Anes08"
        Public Const AppAuthor As String = "Anes08"
        Public Const AppAuthorGithub As String = "AnesHamdani08"
        Public Const AppAuthorDiscord As String = "Anes08"
        Public Const AppAuthorMessage As String = "Never Backdown !"
#End Region
#Region "Properties"
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

        Private _YoutubeDL As Youtube.YoutubeDL
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

        Private WithEvents _Library As Library.Library
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

        Private WithEvents _RemoteLibrary As Classes.QBDatabase(Of CachedMetadata)
        Public Property RemoteLibrary As Classes.QBDatabase(Of CachedMetadata)
            Get
                Return _RemoteLibrary
            End Get
            Set(value As Classes.QBDatabase(Of CachedMetadata))
                _RemoteLibrary = value
                OnPropertyChanged()
            End Set
        End Property

        Private _RemoteLibraryProperties As New List(Of Tuple(Of String, Integer, Boolean))
        ReadOnly Property RemoteLibraryProperties As List(Of Tuple(Of String, Integer, Boolean))
            Get
                Return _RemoteLibraryProperties
            End Get
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

        Private _MemoryManager As Classes.MemoryManager
        Property MemoryManager As Classes.MemoryManager
            Get
                Return _MemoryManager
            End Get
            Set(value As Classes.MemoryManager)
                _MemoryManager = value
                OnPropertyChanged()
            End Set
        End Property

        Private _SleepTimer As Classes.SleepTimer
        Property SleepTimer As Classes.SleepTimer
            Get
                Return _SleepTimer
            End Get
            Set(value As Classes.SleepTimer)
                _SleepTimer = value
                OnPropertyChanged()
            End Set
        End Property

        Private _Themes As New ObjectModel.ObservableCollection(Of Type) From {GetType(Classes.Themes.Default), GetType(Classes.Themes.Elegance), GetType(Classes.Themes.Neon), GetType(Classes.Themes.Aqua), GetType(Classes.Themes.Marine)}
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
                    Try
                        Theme = TryCast(Activator.CreateInstance(Themes(value)), Classes.Themes.Theme)
                    Catch ex As Exception
                        Utilities.DebugMode.Instance.Log(Of SharedProperties)(ex.ToString)
                        Return
                    End Try
                    _ThemesSelectedIndex = value
                    OnPropertyChanged()
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
                Player?.OnThemeChanged()
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

        Private _IsInternetConnected As Boolean
        Property IsInternetConnected As Boolean
            Get
                Return If(OverrideProperties.Instance.Locked(OverrideProperties.LockType.SharedProperties_IsInternetConnected), OverrideProperties.Instance.Value(OverrideProperties.LockType.SharedProperties_IsInternetConnected), _IsInternetConnected)
            End Get
            Set(value As Boolean)
                _IsInternetConnected = value
                OnPropertyChanged()
            End Set
        End Property

        Private _IsRanAsAdmin As Boolean = False
        ReadOnly Property IsRanAsAdmin As Boolean
            Get
                Return _IsRanAsAdmin
            End Get
        End Property

        Private _ExecutableDirectoryRequiresElevation As Boolean
        ReadOnly Property ExecutableDirectoryRequiresElevation As Boolean
            Get
                Return _ExecutableDirectoryRequiresElevation
            End Get
        End Property

        Private _TTSE As Speech.Synthesis.SpeechSynthesizer
        Property TTSE As Speech.Synthesis.SpeechSynthesizer
            Get
                Return _TTSE
            End Get
            Set(value As Speech.Synthesis.SpeechSynthesizer)
                _TTSE = value
                OnPropertyChanged()
            End Set
        End Property

        Private WithEvents _DiscordClient As DiscordRPC.DiscordRpcClient
        Property DiscordClient As DiscordRPC.DiscordRpcClient
            Get
                Return _DiscordClient
            End Get
            Set(value As DiscordRPC.DiscordRpcClient)
                _DiscordClient = value
                OnPropertyChanged()
            End Set
        End Property

        Private _IsDiscordClientInitializedTimeout As Integer = 0
        Public Property IsDiscordClientInitialized As Boolean
            Get
                Return DiscordClient?.IsInitialized
            End Get
            Set(value As Boolean)
                If _IsDiscordClientInitializedTimeout > 0 Then Return
                If value Then
                    If DiscordClient Is Nothing Then DiscordClient = New DiscordRPC.DiscordRpcClient("1157625429029552128")
                    If Not DiscordClient?.IsInitialized Then
                        _IsDiscordClientInitializedTimeout = 5
                        Task.Run(Async Function()
                                     Do While _IsDiscordClientInitializedTimeout > 0
                                         _IsDiscordClientInitializedTimeout -= 1
                                         Await Task.Delay(1000)
                                     Loop
                                     _IsDiscordClientInitializedTimeout = 0
                                 End Function)
                        DiscordClient.Dispose()
                        DiscordClient = New DiscordRPC.DiscordRpcClient("1157625429029552128")
                        DiscordClient.Initialize()
                        _Player_PlaystateChanged(Player)
                    End If
                Else
                    _IsDiscordClientInitializedTimeout = 5
                    Task.Run(Async Function()
                                 Do While _IsDiscordClientInitializedTimeout > 0
                                     _IsDiscordClientInitializedTimeout -= 1
                                     Await Task.Delay(1000)
                                 Loop
                                 _IsDiscordClientInitializedTimeout = 0
                             End Function)
                    If DiscordClient?.IsInitialized Then DiscordClient.Deinitialize()
                    DiscordClient?.Dispose()
                End If
                OnPropertyChanged()
            End Set
        End Property

        Private _HTTPFileServer As UPnP.HttpFileServer
        ReadOnly Property HTTPFileServer As UPnP.HttpFileServer
            Get
                Return _HTTPFileServer
            End Get
        End Property

        Private _JumpList_RecentlyPlayed As Boolean
        Property JumpList_RecentlyPlayed As Boolean
            Get
                Return _JumpList_RecentlyPlayed
            End Get
            Set(value As Boolean)
                _JumpList_RecentlyPlayed = value
                OnPropertyChanged()
            End Set
        End Property
        Private _JumpList_ShowFrequent As Boolean
        Property JumpList_ShowFrequent As Boolean
            Get
                Return _JumpList_ShowFrequent
            End Get
            Set(value As Boolean)
                _JumpList_ShowFrequent = value
                OnPropertyChanged()
            End Set
        End Property
        Private _JumpList_ShowTasks As Boolean
        Property JumpList_ShowTasks As Boolean
            Get
                Return _JumpList_ShowTasks
            End Get
            Set(value As Boolean)
                _JumpList_ShowTasks = value
                OnPropertyChanged()
            End Set
        End Property
        Private _JumpList_MediaLink As Integer
        Property JumpList_MediaLink As Integer
            Get
                Return _JumpList_MediaLink
            End Get
            Set(value As Integer)
                _JumpList_MediaLink = value
                OnPropertyChanged()
            End Set
        End Property

        Private _ActiveControlPopup As UI.Windows.ActiveControlPopup
        Public Property ActiveControlPopup As UI.Windows.ActiveControlPopup
            Get
                Return _ActiveControlPopup
            End Get
            Set(value As UI.Windows.ActiveControlPopup)
                _ActiveControlPopup = value
                OnPropertyChanged()
            End Set
        End Property

        Private _ScanLibraryAtStartup As Boolean = My.Settings?.APP_LIBRARY_STARTUPSCAN
        Property ScanLibraryAtStartup As Boolean
            Get
                Return _ScanLibraryAtStartup
            End Get
            Set(value As Boolean)
                _ScanLibraryAtStartup = value
                OnPropertyChanged()
            End Set
        End Property

        Private _CurrentGreeting As String
        Public Property CurrentGreeting As String
            Get
                Return _CurrentGreeting
            End Get
            Set(value As String)
                _CurrentGreeting = value
                OnPropertyChanged()
            End Set
        End Property

        Private WithEvents _FilePipe As New Classes.NamedPipeManager("QuickBeatPump", IO.Pipes.PipeDirection.InOut)
        Public Property FilePipe As Classes.NamedPipeManager
            Get
                Return _FilePipe
            End Get
            Set(value As Classes.NamedPipeManager)
                _FilePipe = value
                OnPropertyChanged()
            End Set
        End Property

        Private WithEvents _APIPipe As Classes.API
        Public Property APIPipe As Classes.API
            Get
                Return _APIPipe
            End Get
            Set(value As Classes.API)
                _APIPipe = value
                OnPropertyChanged()
            End Set
        End Property

        Private _DiscordCache As Classes.DiscordCache
        Public Property DiscordCache As Classes.DiscordCache
            Get
                Return _DiscordCache
            End Get
            Set(value As Classes.DiscordCache)
                _DiscordCache = value
                OnPropertyChanged()
            End Set
        End Property

        Public Property ShowNotification As ShowNotificationDelegate

        Public Property LibraryPaths As Collections.SyncedObservableCollection(Of String)

        Private WithEvents _Timer As New Forms.Timer With {.Interval = 5000} 'Low freq. timer, used for lazy notifications purposes e.g internet state        

        ReadOnly Property Aqua As New Aqua.Aqua With {.ReferenceBinder = New QScript.KnownReferenceBinder}
        Public ReadOnly Property AllInitialized As Boolean
            Get
                Return Not ItemsConfiguration.Any(Function(k) k.IsLoaded = False)
            End Get
        End Property

        Private _ConfigHasFile As Boolean
        ReadOnly Property ConfigHasFile As Boolean
            Get
                Return _ConfigHasFile
            End Get
        End Property

        Private _ConfigFilePath As String
        ReadOnly Property ConfigFilePath As String
            Get
                Return _ConfigFilePath
            End Get
        End Property

        Private _GlobalTagCache As New Dictionary(Of Tuple(Of TimeSpan, String, TagLib.ReadStyle), TagLib.File)
        ''' <summary>
        ''' (Path,Read,Style), Tag
        ''' </summary>
        ''' <remarks>
        ''' Do not manipulate, use the supplied methods
        ''' </remarks>
        ''' <returns></returns>
        ReadOnly Property GlobalTagCache As Dictionary(Of Tuple(Of TimeSpan, String, TagLib.ReadStyle), TagLib.File)
            Get
                Return _GlobalTagCache
            End Get
        End Property

        Private _DefaultCover As BitmapImage
        ReadOnly Property DefaultCover As BitmapImage
            Get
                If _DefaultCover Is Nothing Then
                    _DefaultCover = New BitmapImage
                    _DefaultCover.BeginInit()
                    _DefaultCover.UriSource = New Uri("Resources/MusicRecord.png", UriKind.Relative)
                    _DefaultCover.CacheOption = BitmapCacheOption.OnLoad
                    _DefaultCover.EndInit()
                    If _DefaultCover.CanFreeze Then _DefaultCover.Freeze()
                End If
                Return _DefaultCover
            End Get
        End Property
#End Region
#Region "Methods"
        Sub Init()
            Utilities.DebugMode.Instance.Log(Of SharedProperties)("Attempting to Initialize...")
            If IsInitialized Then
                Utilities.DebugMode.Instance.Log(Of SharedProperties)("Already Initialized.")
                Return
            End If
            'Init
            'Initializing Fields
            _IsInternetConnected = Utilities.CheckInternetConnection
            LibraryPaths = New Collections.SyncedObservableCollection(Of String)(New Collections.SyncedObservableCollection(Of String).SyncStart(Sub(sender, item, type)
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
            MemoryManager = New Classes.MemoryManager
            YoutubeDL = New Youtube.YoutubeDL
            Library = New Library.Library
            Player = New Player.Player With {.TaskbarOverlayPlaying = New BitmapImage(New Uri("pack://application:,,,/QuickBeat;component/Resources/CircledPlay.png")), .TaskbarOverlayPaused = New BitmapImage(New Uri("pack://application:,,,/QuickBeat;component/Resources/PauseButton.png")), .TaskbarOverlayStopped = New BitmapImage(New Uri("pack://application:,,,/QuickBeat;component/Resources/StopCircled.png"))}
            HotkeyManager = New HotkeyManager(IntPtr.Zero)
            APIPipe = New Classes.API
            SleepTimer = New Classes.SleepTimer
            TTSE = New Speech.Synthesis.SpeechSynthesizer()
            _HTTPFileServer = New UPnP.HttpFileServer(My.Settings?.APP_LIBRARY_PATHS)
            DiscordCache = New Classes.DiscordCache
            JumpList_RecentlyPlayed = My.Settings.APP_JUMPLIST_CUSTOMRECENT
            JumpList_ShowFrequent = My.Settings.APP_JUMPLIST_SHOWFREQUENT
            JumpList_ShowTasks = My.Settings.APP_JUMPLIST_SHOWTASKS
            JumpList_MediaLink = My.Settings.APP_JUMPLIST_ARGTYPE

            Utilities.DebugMode.Instance.Log(Of SharedProperties)("Locating Config File...")
            Dim cfg = System.Configuration.ConfigurationManager.OpenExeConfiguration(Configuration.ConfigurationUserLevel.PerUserRoamingAndLocal)
            _ConfigHasFile = cfg.HasFile
            _ConfigFilePath = cfg.FilePath
            OnPropertyChanged(NameOf(ConfigHasFile))
            OnPropertyChanged(NameOf(ConfigFilePath))
            Utilities.DebugMode.Instance.Log(Of SharedProperties)("Hooking Theme Changed Handler...")
            HandyControl.Themes.ThemeManager.Current.ActualApplicationThemeChanged = New HandyControl.Themes.TypedEventHandler(Of HandyControl.Themes.ThemeManager, Object)(Sub(sender, e)
                                                                                                                                                                                Player?.OnThemeChanged()
                                                                                                                                                                            End Sub)
            Utilities.DebugMode.Instance.Log(Of SharedProperties)("Checking App Privileges...")
            _IsRanAsAdmin = Utilities.CommonFunctions.IsUserAdministrator
            OnPropertyChanged(NameOf(IsRanAsAdmin))
            _ExecutableDirectoryRequiresElevation = My.Application.Info.DirectoryPath.StartsWith("C:\Program Files") OrElse My.Application.Info.DirectoryPath.StartsWith("C:\Program Files (x86)")
            OnPropertyChanged(NameOf(ExecutableDirectoryRequiresElevation))
            Utilities.DebugMode.Instance.Log(Of SharedProperties)("Starting Timer...")
            _Timer.Start()
            Utilities.DebugMode.Instance.Log(Of SharedProperties)("Setting Theme...")
            If Theme Is Nothing Then
                'Theme = New Classes.Themes.Default
                ThemesSelectedIndex = My.Settings.APP_THEME_INDEX
            End If
            Utilities.DebugMode.Instance.Log(Of SharedProperties)("Loading Remote Library Properties...")
            For Each entry In My.Settings.APP_LIBRARY_REMOTE
                Dim sEntry = entry.Split(";")
                If sEntry.Length < 3 Then Continue For 'bad entry
                If String.IsNullOrEmpty(sEntry(0)) Then Continue For 'another bad entry
                Dim pCount As Integer = 0
                Dim isFav As Boolean = False
                Integer.TryParse(sEntry(1), pCount)
                Boolean.TryParse(sEntry(2), isFav)
                RemoteLibraryProperties.Add(New Tuple(Of String, Integer, Boolean)(sEntry(0), pCount, isFav))
            Next
            Utilities.DebugMode.Instance.Log(Of SharedProperties)("Loading Local Cache...")
            RemoteLibrary = Classes.QBDatabase(Of CachedMetadata).LoadOrCreate("LocalCache")
            Utilities.DebugMode.Instance.Log(Of SharedProperties)("Loading Library...")
            Library.Init()
            Library.Load(My.Settings.APP_LIBRARY)
            If Library.Count > 0 Then
                If Library.Item(0).Path <> "Resources/Neffex - Fight Back.mp3" Then 'expected to be and stays at one
                    Dim meta As New Player.InternalMetadata("Resources/Neffex - Fight Back.mp3") : meta.RefreshTagsFromFile(True)
                    Library.Insert(0, meta)
                End If
            Else
                Dim meta As New Player.InternalMetadata("Resources/Neffex - Fight Back.mp3") : meta.RefreshTagsFromFile(True)
                Library.Add(meta, NotifySearchCache:=False)
            End If
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
                If ScanLibraryAtStartup Then
                    Utilities.DebugMode.Instance.Log(Of SharedProperties)("Scanning Library...,Reason:=Startup Scan")
                    Commands.LibraryScanCommand?.Execute(Library)
                End If
            End If
            Utilities.DebugMode.Instance.Log(Of SharedProperties)("Loading Custom Playlists...")
            Dim BinF As New System.Runtime.Serialization.Formatters.Binary.BinaryFormatter
            For Each playlist In My.Settings.APP_PLAYER_PLAYLIST_CUSTOM
                If Not String.IsNullOrEmpty(playlist) Then
                    Try
                        'Dim Pl64 = Convert.FromBase64String(playlist)
                        'Dim Pl As Player.Playlist = BinF.Deserialize(New IO.MemoryStream(Pl64))
                        Dim Pl As New Player.Playlist()
                        Pl.Load(playlist)
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
            Dim RecentSongs As New Player.Playlist With {
                .Name = "Recently Played", .Category = "Playlist", .Description = "Your last played songs", .Tag = "Recent",
                .Cover = CommonFunctions.GenerateCoverImage(ResourceResolver.Images.CLOCK)
            }
            For Each song In My.Settings.APP_PLAYER_RECENT
                If String.IsNullOrEmpty(song) Then Continue For
                'Try
                '    Dim MD64 = Convert.FromBase64String(song)
                '    Dim MD As Player.Metadata = BinF.Deserialize(New IO.MemoryStream(MD64))
                '    RecentSongs.Add(If(QuickBeat.Player.Metadata.FromFile(MD.Path, False, True), MD))
                'Catch ex As Exception
                '    Utilities.DebugMode.Instance.Log(Of SharedProperties)("Failed to load recent song from settings. Error:=" & ex.ToString)
                'End Try
                Dim MD As Player.Metadata = Metadata.FromUID(song)
                If MD IsNot Nothing Then RecentSongs.Add(MD)
            Next
            RecommendedPlaylists.Add(RecentSongs)
            Utilities.DebugMode.Instance.Log(Of SharedProperties)("Initializing Discord Cache...")
            DiscordCache.Init()
            Utilities.DebugMode.Instance.Log(Of SharedProperties)("Initializing Player...")
            Player.Init()
            Player.LoadConfigSettings(My.Settings.APP_PLAYER_CONFIG)
            Player.LoadSettings(My.Settings.APP_PLAYER_SETTINGS)
            If Not String.IsNullOrEmpty(My.Settings.APP_PLAYER_PLAYLIST) Then
                Try
                    'Dim Pl64 = Convert.FromBase64String(My.Settings.APP_PLAYER_PLAYLIST)
                    'Player.LoadPlaylist(New IO.MemoryStream(Pl64))
                    ''Replace instance reference to the current one, if left alone metadata info changing wont reflect in the library
                    'For i As Integer = 0 To Player.Playlist.Count - 1
                    '    If Player.Playlist(i).Location = QuickBeat.Player.Metadata.FileLocation.Remote Then Continue For
                    '    Dim _i = i
                    '    Dim metaref = Library.FirstOrDefault(Function(k) k.Path = Player.Playlist(_i).Path)
                    '    If metaref IsNot Nothing Then
                    '        Player.Playlist(i) = metaref
                    '    End If
                    'Next
                    Player.LoadPlaylist(My.Settings.APP_PLAYER_PLAYLIST)
                    Player.Playlist.RefreshItemsIndex()
                    Dim LSP As New Player.Playlist() With {.Name = "Last Session", .Category = "QuickBeat", .Description = "Your Last Session", .Cover = New BitmapImage(New Uri("\Resources\MusicRecord.png", UriKind.Relative))}
                    For Each song In Player.Playlist
                        LSP.Add(song)
                    Next
                    RecommendedPlaylists.Add(LSP)
                Catch ex As Exception
                    Utilities.DebugMode.Instance.Log(Of SharedProperties)("Failed to load playlist from settings. Error:=" & ex.ToString)
                    Player.Configuration.SetError(True, New ArgumentException("Failed to load playlist."))
                End Try
            End If
            Utilities.DebugMode.Instance.Log(Of SharedProperties)("Initializing Hotkey Manager...")
            If String.IsNullOrEmpty(My.Settings.APP_HOTKEYS) Then
                'HotkeyManager = New HotkeyManager(IntPtr.Zero)
                HotkeyManager.Configuration.SetStatus("All Good", 100)
            Else
                Try
                    Dim Hk64 = Convert.FromBase64String(My.Settings.APP_HOTKEYS)
                    'HotkeyManager = New HotkeyManager(IntPtr.Zero)
                    HotkeyManager.LoadFrom(New IO.MemoryStream(Hk64))
                Catch ex As Exception
                    Utilities.DebugMode.Instance.Log(Of SharedProperties)("Failed to load custom playlist from settings. Error:=" & ex.ToString)
                    'HotkeyManager = New HotkeyManager(IntPtr.Zero)
                    HotkeyManager.Configuration.SetError(True, New ArgumentException("Failed to load hotkeys."))
                End Try
            End If
            Utilities.DebugMode.Instance.Log(Of SharedProperties)("Initializing YouTube-DL...")
            If Not String.IsNullOrEmpty(My.Settings.APP_YOUTUBEDL_PATH) Then
                YoutubeDL.YoutubeDLLocation = My.Settings.APP_YOUTUBEDL_PATH
            End If
            If My.Settings.APP_DISCORD Then
                Utilities.DebugMode.Instance.Log(Of SharedProperties)("Initializing Discord RPC...")
                DiscordClient = New DiscordRPC.DiscordRpcClient("1157625429029552128")
                DiscordClient.Initialize()
            End If
            APIPipe.Init()
            'Adding config. items
            'With ItemsConfiguration
            '    .Add(Library.Configuration)
            '    .Add(Player.Configuration)
            '    .Add(HotkeyManager.Configuration)
            '    .Add(FilePipe.Configuration)
            '    .Add(APIPipe.Configuration)
            '    .Add(YoutubeDL.Configuration)
            'End With
            MemoryManager.IsPeriodicCleaning = True
            YoutubeDL.Init()
            Utilities.DebugMode.Instance.Log(Of SharedProperties)("Initializing File Pipe...")
            HTTPFileServer.Init()
            FilePipe.Init()
            FilePipe.StartServer()
            Utilities.DebugMode.Instance.Log(Of SharedProperties)("Initializing TTS Engine...")
            TTSE.SelectVoiceByHints(Speech.Synthesis.VoiceGender.Female)
            Utilities.DebugMode.Instance.Log(Of SharedProperties)("Initializing Jump List...")
            Dim JTaskPlayAll As System.Windows.Shell.JumpTask = Nothing
            Dim JTaskShuffleAll As System.Windows.Shell.JumpTask = Nothing
            If JumpList_ShowTasks Then
                JTaskPlayAll = New System.Windows.Shell.JumpTask() With {.Title = "Play All", .Description = "Play All Your Songs", .CustomCategory = "Tasks", .ApplicationPath = IO.Path.Combine(My.Application.Info.DirectoryPath, "QuickBeat.exe"), .Arguments = "PlayAll"}
                JTaskShuffleAll = New System.Windows.Shell.JumpTask() With {.Title = "Shuffle All", .Description = "Shuffle All Your Songs", .CustomCategory = "Tasks", .ApplicationPath = IO.Path.Combine(My.Application.Info.DirectoryPath, "QuickBeat.exe"), .Arguments = "ShuffleAll"}
            End If
            Dim JList As New System.Windows.Shell.JumpList(If(JumpList_ShowTasks, {JTaskPlayAll, JTaskShuffleAll}, {}), JumpList_ShowFrequent, True)
            System.Windows.Shell.JumpList.SetJumpList(Application.Current, JList)
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
            'Required because apparently calling it from main window ctor caused 2ms impact on performance :/
            Select Case Now.Hour
                Case > 22
                    CurrentGreeting = ResourceResolver.Strings.GREETING_NIGHT
                Case > 18
                    CurrentGreeting = ResourceResolver.Strings.GREETING_EVENING
                Case > 12
                    CurrentGreeting = ResourceResolver.Strings.GREETING_AFTERNOON
                Case > 6
                    CurrentGreeting = ResourceResolver.Strings.GREETING_MORNING
                Case Else
                    CurrentGreeting = "Welcome Back"
            End Select
            _ActiveControlPopup = New UI.Windows.ActiveControlPopup With {.AutoClose = False, .Timeout = 4000}
        End Sub
        Public Sub Save()
            Utilities.DebugMode.Instance.Log(Of SharedProperties)("Attempting to Save...")
            If Library.Item(0).Path = "Resources/Neffex - Fight Back.mp3" Then 'expected to be and stays at one
                Library.RemoveAt(0, False)
            End If
            Dim Lib64 = Library.Save() : Utilities.DebugMode.Instance.Log(Of Application)("Library: ✓")
            Dim Hk64 = HotkeyManager.Save : Utilities.DebugMode.Instance.Log(Of Application)("Hotkey Manager: ✓")
            Dim Pl64 = Player.SavePlaylist : Utilities.DebugMode.Instance.Log(Of Application)("Playlist: ✓")
            Dim BinF As New System.Runtime.Serialization.Formatters.Binary.BinaryFormatter

            My.Settings.APP_LIBRARY_REMOTE.Clear()
            My.Settings.APP_LIBRARY.Clear()
            My.Settings.APP_PLAYER_PLAYLIST_CUSTOM.Clear()

            My.Settings.APP_PLAYER_CONFIG = Player.SaveConfigSettings : Utilities.DebugMode.Instance.Log(Of Application)("Bass Config: ✓")
            My.Settings.APP_PLAYER_SETTINGS = Player.SaveSettings : Utilities.DebugMode.Instance.Log(Of Application)("Player: ✓")

            For Each item In RemoteLibraryProperties
                My.Settings.APP_LIBRARY_REMOTE.Add(item.Item1 & ";" & item.Item2 & ";" & item.Item3)
            Next
            Utilities.DebugMode.Instance.Log(Of Application)("Remote Library: ✓")

            For Each item In Lib64
                My.Settings.APP_LIBRARY.Add(item)
            Next
            For Each item In CustomPlaylists
                If item Is Nothing OrElse item.Count = 0 Then Continue For
                'Dim PlMem As New IO.MemoryStream
                'BinF.Serialize(PlMem, item)
                'My.Settings.APP_PLAYER_PLAYLIST_CUSTOM.Add(Convert.ToBase64String(PlMem.ToArray)) : Utilities.DebugMode.Instance.Log(Of Application)("Custom Playlist[" & item.Name & "]: ✓")
                My.Settings.APP_PLAYER_PLAYLIST_CUSTOM.Add(item.Save) : Utilities.DebugMode.Instance.Log(Of Application)("Custom Playlist[" & item.Name & "]: ✓")
            Next
            My.Settings.APP_HOTKEYS = Convert.ToBase64String(Hk64.ToArray)
            My.Settings.APP_PLAYER_PLAYLIST = Pl64 'Convert.ToBase64String(Pl64.ToArray)
            DiscordCache?.Save() : Utilities.DebugMode.Instance.Log(Of Application)("Discord Cache: ✓")

            'Recent songs
            Dim RecentlyPlayed = RecommendedPlaylists.FirstOrDefault(Function(k) k.Tag = "Recent")
            If RecentlyPlayed IsNot Nothing Then
                My.Settings.APP_PLAYER_RECENT.Clear()
                For i As Integer = 0 To If(RecentlyPlayed.Count >= My.Settings.APP_PLAYER_RECENT_LIMIT, My.Settings.APP_PLAYER_RECENT_LIMIT - 1, RecentlyPlayed.Count - 1)
                    Dim meta = RecentlyPlayed(i)
                    If meta.Location = QuickBeat.Player.Metadata.FileLocation.Local Then
                        'Dim MDMem As New IO.MemoryStream
                        'BinF.Serialize(MDMem, meta)
                        'My.Settings.APP_PLAYER_RECENT.Insert(0, Convert.ToBase64String(MDMem.ToArray))
                        My.Settings.APP_PLAYER_RECENT.Insert(0, meta.UID)
                    End If
                Next
            End If
            'TODO FIX THIS FUCKING SHIT, IT'S CRASHING AT HERE
            Utilities.DebugMode.Instance.Log(Of Application)("Recent Songs: ✓")
            If Not String.IsNullOrEmpty(YoutubeDL.YoutubeDLLocation) Then My.Settings.APP_YOUTUBEDL_PATH = YoutubeDL.YoutubeDLLocation
            My.Settings.APP_THEME_INDEX = ThemesSelectedIndex
            My.Settings.APP_THEME_LIGHT = (HandyControl.Themes.ThemeManager.Current.ActualApplicationTheme = HandyControl.Themes.ApplicationTheme.Light)
            My.Settings.APP_JUMPLIST_ARGTYPE = JumpList_MediaLink
            My.Settings.APP_JUMPLIST_CUSTOMRECENT = JumpList_RecentlyPlayed
            My.Settings.APP_JUMPLIST_SHOWFREQUENT = JumpList_ShowFrequent
            My.Settings.APP_JUMPLIST_SHOWTASKS = JumpList_ShowTasks

            My.Settings.Save()
        End Sub
        Public Sub DeInit()
            _Timer.Stop()
            Player.Free()
            APIPipe.Stop()
            HTTPFileServer.Dispose()
            FilePipe.StopServer()
        End Sub
        Public Sub App_Restart()
            'Thank you user8066390
            Dim Info As New ProcessStartInfo With {
                .Arguments = "/C choice /C Y /N /D Y /T 1 & START """" """ & System.Reflection.Assembly.GetEntryAssembly().Location & """",
                .WindowStyle = ProcessWindowStyle.Hidden,
                .CreateNoWindow = True,
                .FileName = "cmd.exe"
            }
            Process.Start(Info)
            Process.GetCurrentProcess().Kill()
        End Sub
        Private Sub OnPropertyChanged(<Runtime.CompilerServices.CallerMemberName> Optional CallerName As String = Nothing)
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(CallerName))
        End Sub
        Public Sub OnAppActivated()
            RaiseEvent App_Activated()
        End Sub
        Public Sub OnAppDeactivated()
            RaiseEvent App_Deactivated()
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
        ''' <summary>
        ''' Saves some disk usage by caching tags for a specific time, see APP_TAG_HOLDTIME setting
        ''' If <paramref name="readstyle"/> = <see cref="TagLib.ReadStyle.Average"/> then tag is created if there isn't a matching one in the cache.
        ''' However, if <see cref="TagLib.ReadStyle.PictureLazy"/> is used then it will return a match with both <see cref="TagLib.ReadStyle.Average"/> and <see cref="TagLib.ReadStyle.PictureLazy"/>.
        ''' If <see cref="TagLib.ReadStyle.None"/> is used then a match will be return based on only the file path.
        ''' </summary>
        ''' <param name="path">File path</param>
        ''' <param name="readstyle">Tag read style</param>
        ''' <returns></returns>
        Public Function RequestTags(path As String, Optional readstyle As TagLib.ReadStyle = TagLib.ReadStyle.Average) As TagLib.File
            Dim tg As KeyValuePair(Of Tuple(Of TimeSpan, String, TagLib.ReadStyle), TagLib.File)
            If readstyle = TagLib.ReadStyle.Average Then
                tg = GlobalTagCache.FirstOrDefault(Function(k) k.Key.Item2 = path AndAlso k.Key.Item3.HasFlag(TagLib.ReadStyle.Average))
            ElseIf readstyle.HasFlag(TagLib.ReadStyle.PictureLazy) Then
                tg = GlobalTagCache.FirstOrDefault(Function(k) k.Key.Item2 = path AndAlso (k.Key.Item3.HasFlag(TagLib.ReadStyle.PictureLazy) OrElse k.Key.Item3.HasFlag(TagLib.ReadStyle.Average)))
            Else
                tg = GlobalTagCache.FirstOrDefault(Function(k) k.Key.Item2 = path)
            End If
            If tg.Value Is Nothing Then
                If Not IO.File.Exists(path) Then Return Nothing
                Dim tag = TagLib.File.Create(path, readstyle)
                MemoryManager.Cancel() 'Avoid collection modified exception
                GlobalTagCache.Add(New Tuple(Of TimeSpan, String, TagLib.ReadStyle)(Now.TimeOfDay, path, readstyle), tag)
                Return tag
            Else
                Return tg.Value
            End If
        End Function

        Public Function GetRemoteFileProperties(UID As String, Optional isURL As Boolean = False) As Tuple(Of String, Integer, Boolean)
            If String.IsNullOrWhiteSpace(UID) Then Return Nothing
            If isURL Then
                Dim cUID = URLtoUID(UID)
                Return RemoteLibraryProperties.FirstOrDefault(Function(k) k.Item1 = cUID)
            Else
                Return RemoteLibraryProperties.FirstOrDefault(Function(k) k.Item1 = UID)
            End If
        End Function

        Public Function UpdateRemoteFileProperty(NewProperties As Tuple(Of Integer, Boolean), UID As String, Optional isURL As Boolean = False) As Boolean
            If String.IsNullOrWhiteSpace(UID) Then Return False
            Dim item As Tuple(Of String, Integer, Boolean) = Nothing
            If isURL Then
                Dim cUID = URLtoUID(UID)
                item = RemoteLibraryProperties.FirstOrDefault(Function(k) k.Item1 = cUID)
            Else
                item = RemoteLibraryProperties.FirstOrDefault(Function(k) k.Item1 = UID)
            End If
            If item IsNot Nothing Then
                Dim i = RemoteLibraryProperties.IndexOf(item) : If i = -1 Then Return False 'most likely not
                Dim cItem As New Tuple(Of String, Integer, Boolean)(item.Item1, NewProperties.Item1, NewProperties.Item2)
                RemoteLibraryProperties.RemoveAt(i) : RemoteLibraryProperties.Insert(i, cItem)
                Return True 'succesfuly updated
            End If
            Return False
        End Function

        Public Function UpdateOrCreateRemoteFileProperty(NewProperties As Tuple(Of Integer, Boolean), UID As String, Optional isURL As Boolean = False) As Boolean
            If String.IsNullOrWhiteSpace(UID) Then Return False
            Dim item As Tuple(Of String, Integer, Boolean) = Nothing
            If isURL Then
                Dim cUID = URLtoUID(UID)
                item = RemoteLibraryProperties.FirstOrDefault(Function(k) k.Item1 = cUID)
            Else
                item = RemoteLibraryProperties.FirstOrDefault(Function(k) k.Item1 = UID)
            End If
            If item IsNot Nothing Then
                Dim i = RemoteLibraryProperties.IndexOf(item) : If i = -1 Then Return False 'most likely not
                Dim cItem As New Tuple(Of String, Integer, Boolean)(item.Item1, NewProperties.Item1, NewProperties.Item2)
                RemoteLibraryProperties.RemoveAt(i) : RemoteLibraryProperties.Insert(i, cItem)
                Return True 'succesfuly updated
            Else
                Dim cItem As New Tuple(Of String, Integer, Boolean)(If(isURL, URLtoUID(UID), UID), NewProperties.Item1, NewProperties.Item2)
                RemoteLibraryProperties.Add(cItem)
                Return True 'succesfuly updated
            End If
        End Function

#Region "Handlers"
        Private Sub _FilePipe_ReceiveString(sender As Classes.NamedPipeManager, msg As String) Handles _FilePipe.ReceiveString
            Utilities.DebugMode.Instance.Log(Of SharedProperties)($"Recieved string from {sender.NamedPipeName}: {msg}")
            If msg.ToLower = "ping" Then
                sender.Write("pingback")
            ElseIf msg.ToLower = "playall" Then
                Dim PL = SharedProperties.Instance.Library.ToPlaylist()
                PL.Name = "Library"
                PL.Category = "Playlist"
                PL.Description = "All Your Music"
                SharedProperties.Instance.Player.LoadPlaylist(PL)
            ElseIf msg.ToLower = "shuffleall" Then
                Dim PL = SharedProperties.Instance.Library.ToPlaylist()
                PL.Name = "Library"
                PL.Category = "Playlist"
                PL.Description = "All Your Music"
                PL.IsShuffling = True
                SharedProperties.Instance.Player.LoadPlaylist(PL)
            ElseIf msg.ToLower.StartsWith("jlist") Then
                Dim path = msg.Substring(msg.IndexOf("/") + 1)
                Select Case JumpList_MediaLink
                    Case 0 'Preview
                        SharedProperties.Instance.Player.PreviewCommand.Execute(path)
                    Case 1 'Append
                        SharedProperties.Instance.Player.LoadAndAddCommand.Execute(path)
                    Case 2 'Direct Play
                        Dim iMeta = SharedProperties.Instance.Player.Playlist.FirstOrDefault(Function(k) k.Path = path)
                        If iMeta Is Nothing Then
                            Dim Meta As New Player.Metadata() With {.Path = path, .Location = If(path.IsURL, QuickBeat.Player.Metadata.FileLocation.Remote, QuickBeat.Player.Metadata.FileLocation.Local)}
#Disable Warning
                            SharedProperties.Instance.Player.LoadSong(Meta)
#Enable Warning
                        Else
                            SharedProperties.Instance.Player.Playlist.Index = iMeta.Index
                        End If
                End Select
            ElseIf msg.ToLower.StartsWith("activate") Then
                ActivateApp()
            ElseIf msg.ToLower.StartsWith("preview ") Then
                'Player.PreviewCommand.Execute(QuickBeat.Player.Metadata.FromFile(msg.Remove(0, "preview ".Length)))
                Player.PreviewCommand.Execute(msg.Remove(0, "preview ".Length))
                ActivateApp()
            ElseIf msg.ToLower.StartsWith("load ") Then
                'have built in url and playlist check
                Player.LoadCommand.Execute(msg.Remove(0, "load ".Length))
            ElseIf msg.ToLower.StartsWith("appendload ") Then
                Player.LoadAndAddCommand.Execute(msg.Remove(0, "appendload ".Length))
            ElseIf msg.ToLower.StartsWith("append ") Then
                Player.Playlist.Add(QuickBeat.Player.Metadata.FromFile(msg.Remove(0, "append ".Length))) 'lazy :)            
            End If
        End Sub

        Private Sub _Player_MetadataChanged() Handles _Player.MetadataChanged
            If My.Settings.APP_PLAYER_MEDIALOADED_SHOWPOPUP Then
                If Not Application.Current.MainWindow?.IsVisible OrElse Application.Current.MainWindow?.WindowState = WindowState.Minimized Then
                    If ActiveControlPopup IsNot Nothing AndAlso ActiveControlPopup.IsShown Then
                        If Not ActiveControlPopup.IsOpen Then
                            ActiveControlPopup.RevealCollapse()
                        End If
                    Else
                        UI.Windows.NotificationPopUp.RevealConcealQuick(Player.StreamMetadata.TitleArtist, 4000, Player.StreamMetadata.DefaultCover)
                    End If
                End If
            Else
                If Not Application.Current.MainWindow?.IsVisible OrElse Application.Current.MainWindow?.WindowState = WindowState.Minimized Then
                    If ActiveControlPopup IsNot Nothing AndAlso ActiveControlPopup.IsShown Then
                        If Not ActiveControlPopup.IsOpen Then
                            ActiveControlPopup.RevealCollapse()
                        End If
                    End If
                End If
            End If
            If _Player.StreamMetadata.Location = QuickBeat.Player.Metadata.FileLocation.Local Then
                Dim RecentlyPlayed = RecommendedPlaylists.FirstOrDefault(Function(k) k.Tag = "Recent")
                If RecentlyPlayed Is Nothing Then 'Create if not created
                    'Dim BinF As New Runtime.Serialization.Formatters.Binary.BinaryFormatter
                    Dim RecentSongs As New Player.Playlist With {
                        .Name = "Recently Played", .Category = "Playlist", .Description = "Your last played songs", .Tag = "Recent",
                        .Cover = CommonFunctions.GenerateCoverImage(ResourceResolver.Images.CLOCK)
                    }
                    For Each song In My.Settings.APP_PLAYER_RECENT
                        If String.IsNullOrEmpty(song) Then Continue For
                        Dim MD As Player.Metadata = Metadata.FromUID(song)
                        If MD IsNot Nothing Then RecentSongs.Add(MD)
                    Next
                    'For Each song In My.Settings.APP_PLAYER_RECENT
                    '    If String.IsNullOrEmpty(song) Then Continue For
                    '    Try
                    '        Dim MD64 = Convert.FromBase64String(song)
                    '        Dim MD As Player.Metadata = BinF.Deserialize(New IO.MemoryStream(MD64))
                    '        RecentSongs.Add(If(QuickBeat.Player.Metadata.FromFile(MD.Path, False, True), MD))
                    '    Catch ex As Exception
                    '        Utilities.DebugMode.Instance.Log(Of SharedProperties)("Failed to load recent song from settings. Error:=" & ex.ToString)
                    '    End Try
                    'Next
                    RecommendedPlaylists.Add(RecentSongs)
                    RecentlyPlayed = RecentSongs
                End If
                If RecentlyPlayed IsNot Nothing Then 'Add to it
                    Dim Occurence = RecentlyPlayed.FirstOrDefault(Function(k) k.Path = _Player.StreamMetadata.Path)
                    If Occurence IsNot Nothing Then
                        RecentlyPlayed.RemoveAt(RecentlyPlayed.IndexOf(Occurence))
                    End If
                    RecentlyPlayed.Insert(0, _Player.StreamMetadata)
                    Do While RecentlyPlayed.Count > My.Settings.APP_PLAYER_RECENT_LIMIT
                        RecentlyPlayed.RemoveAt(RecentlyPlayed.Count - 1)
                    Loop
                    Dim meta = RecentlyPlayed.FirstOrDefault
                    If meta IsNot Nothing Then
                        meta.EnsureCovers(True)
                        If meta.DefaultCover IsNot Nothing Then RecentlyPlayed.Cover = meta.DefaultCover
                    End If
                End If
                If JumpList_RecentlyPlayed Then
                    Dim JList = System.Windows.Shell.JumpList.GetJumpList(Application.Current)
                    If JList IsNot Nothing Then
                        Do While JList.JumpItems.LongCount(Function(k) k.CustomCategory = "Recently Played") > 0
                            JList.JumpItems.Remove(JList.JumpItems.FirstOrDefault(Function(k) k.CustomCategory = "Recently Played"))
                        Loop
                        For Each item In RecentlyPlayed.Reverse
                            If item.IsRemote Then Continue For
                            JList.JumpItems.Insert(0, New System.Windows.Shell.JumpTask() With {.Title = item.TitleArtist, .Description = item.Path, .CustomCategory = "Recently Played", .ApplicationPath = IO.Path.Combine(My.Application.Info.DirectoryPath, "QuickBeat.exe"), .Arguments = """JList"" """ & item.Path & """"})
                        Next
                        JList?.Apply()
                    End If
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
            If FavoriteSongs.Cover Is Nothing Then FavoriteSongs.Cover = CommonFunctions.GenerateCoverImage(ResourceResolver.Images.HEARTS)
        End Sub

        Private Sub _Library_GroupFavoriteChanged(sender As Player.MetadataGroup) Handles _Library.GroupFavoriteChanged
            Dim RecMDG = RecommendedPlaylists.FirstOrDefault(Function(k) k Is sender)
            If sender.IsFavorite AndAlso RecMDG Is Nothing Then
                Dim pList = sender.ToPlaylist
                If pList.Cover Is Nothing Then
                    Dim meta = pList.FirstOrDefault(Function(k) k.HasCover)
                    If meta IsNot Nothing Then
                        meta.EnsureCovers(True)
                        pList.Cover = meta.DefaultCover
                    End If
                End If
                RecommendedPlaylists.Add(pList)
            ElseIf Not sender.IsFavorite AndAlso RecMDG IsNot Nothing Then
                RecommendedPlaylists.Remove(RecMDG)
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

        Private _DiscordPresenceCache As Tuple(Of Integer, DiscordRPC.RichPresence) = Nothing
        Private _DiscordIsFetching As Boolean = False
        Private _DiscordRedundancyCounter As Integer = 0
        Private _DiscordRedundancyIsCounting As Boolean = False

        Private Sub _DiscordClient_OnReady(sender As Object, args As ReadyMessage) Handles _DiscordClient.OnReady
            Utilities.DebugMode.Instance.Log(Of SharedProperties)("Connected to " & args.User.DisplayName & "[" & args.User.Username & "]")
            OnPropertyChanged(NameOf(IsDiscordClientInitialized))
        End Sub

        Private Sub _DiscordClient_OnConnectionFailed(sender As Object, args As ConnectionFailedMessage) Handles _DiscordClient.OnConnectionFailed
            Utilities.DebugMode.Instance.Log(Of SharedProperties)("Failed to Connect to Discord: " & args.Type.ToString)
            OnPropertyChanged(NameOf(IsDiscordClientInitialized))
        End Sub

        Private Sub _DiscordClient_OnConnectionEstablished(sender As Object, args As ConnectionEstablishedMessage) Handles _DiscordClient.OnConnectionEstablished
            OnPropertyChanged(NameOf(IsDiscordClientInitialized))
        End Sub

        Private Async Sub _Player_PlaystateChanged(sender As Player.Player) Handles _Player.PlaystateChanged
            If DiscordClient Is Nothing OrElse Not DiscordClient.IsInitialized OrElse _DiscordIsFetching Then Return
            If _DiscordRedundancyIsCounting Then
                _DiscordRedundancyCounter = 0
                Return
            Else
                _DiscordRedundancyIsCounting = True
            End If
            Do While _DiscordRedundancyIsCounting
                Await Task.Delay(10)
                _DiscordRedundancyCounter += 1
                If _DiscordRedundancyCounter = 100 Then '1 sec has passed
                    _DiscordRedundancyCounter = 0
                    _DiscordRedundancyIsCounting = False
                    Exit Do
                End If
            Loop
            If sender.IsPlaying Then
                If _DiscordPresenceCache?.Item1 = sender.Stream Then
                    _DiscordPresenceCache.Item2.Timestamps = DiscordRPC.Timestamps.FromTimeSpan(TimeSpan.FromSeconds(Player.Length - Player.Position))
                    If Not DiscordClient.IsDisposed Then DiscordClient.SetPresence(_DiscordPresenceCache.Item2)
                Else
                    If sender.StreamMetadata Is Nothing Then
                        Return
                    ElseIf String.IsNullOrEmpty(sender.StreamMetadata.Title) AndAlso String.IsNullOrEmpty(sender.StreamMetadata.DefaultArtist) Then
                        Return
                    End If
                    _DiscordIsFetching = True
                    'Text.Encoding.UTF8.GetByteCount(sender.StreamMetadata.Title)
                    'TODO make byte length checks : discord allows 512 for url , idk about state and details                 
                    Dim RPC As New DiscordRPC.RichPresence With {
                        .State = sender.StreamMetadata.Title.LimitBytes(128, True),
                        .Details = sender.StreamMetadata.DefaultArtist.LimitBytes(128, True)
                    }
                    Dim cItem = DiscordCache.GetValue(sender.StreamMetadata.TitleArtist)
                    Dim linkAvail As Boolean = False
                    If cItem IsNot Nothing Then
                        If String.IsNullOrWhiteSpace(cItem.CoverLink) OrElse cItem.CoverLink = "musicrecord" Then 'not available from the start
                            linkAvail = True
                        Else
                            Try
                                linkAvail = CommonFunctions.RemoteFileExists(cItem.CoverLink)
                            Catch ex As Exception
                            End Try
                        End If
                    End If
                    If cItem IsNot Nothing AndAlso linkAvail Then
                        RPC.Assets = New DiscordRPC.Assets() With {.LargeImageKey = cItem.CoverLink.LimitBytes(256), .LargeImageText = sender.StreamMetadata.Album.LimitBytes(128)}
                        RPC.Timestamps = DiscordRPC.Timestamps.FromTimeSpan(TimeSpan.FromSeconds(sender.Length))
                        If Not String.IsNullOrWhiteSpace(cItem.Link) AndAlso Text.Encoding.UTF8.GetByteCount(cItem.Link) <= 512 Then
                            If cItem.Provider = "Deezer" Then
                                RPC.Buttons = New DiscordRPC.Button() {New DiscordRPC.Button() With {.Label = "Listen in Deezer", .Url = cItem.Link}}
                            ElseIf cItem.Provider = "MusicBrainz" Then
                                RPC.Buttons = New DiscordRPC.Button() {New DiscordRPC.Button() With {.Label = "See in MusicBrainz", .Url = cItem.Link}}
                            ElseIf sender.StreamMetadata.Provider IsNot Nothing Then
                                If TypeOf sender.StreamMetadata.Provider Is CacheMediaProvider OrElse TypeOf sender.StreamMetadata.Provider Is IOMediaProvider OrElse TypeOf sender.StreamMetadata.Provider Is InternalMediaProvider Then
                                    Try
                                        Dim hUri As New Uri(cItem.Link)
                                        RPC.Buttons = New DiscordRPC.Button() {New DiscordRPC.Button() With {.Label = ("Listen in " & hUri.Host).LimitBytes(32), .Url = cItem.Link}}
                                    Catch 'mostly a bad URL
                                    End Try
                                Else
                                    RPC.Buttons = New DiscordRPC.Button() {New DiscordRPC.Button() With {.Label = ("Listen in " & sender.StreamMetadata.Provider?.Name).LimitBytes(32), .Url = cItem.Link}}
                                End If
                            End If
                        End If
                    Else
                        'Getting Thumbnail URL
                        Dim SongLinkProvider As String = ""
                        Dim SongLink As String = If(sender.StreamMetadata.Location = QuickBeat.Player.Metadata.FileLocation.Remote, sender.StreamMetadata.OriginalPath, Nothing)
                        Dim CoverLink = If(String.IsNullOrEmpty(sender.StreamMetadata.ThumbnailCoverLink), sender.StreamMetadata.CoverLink, sender.StreamMetadata.ThumbnailCoverLink)
                        If SharedProperties.Instance.IsInternetConnected Then
                            If String.IsNullOrEmpty(CoverLink) Then
                                'Threadview issues
                                Await Application.Current.Dispatcher.Invoke(Async Function()
                                                                                Dim dProvider As New DeezerProvider() With {.BlockAutoFetch = True, .CombineQueries = False, .FallBackToDefaultQuery = False, .DecodeToThumbnails = True, .UseDefaultQuery = False, .TrackQuery = sender.StreamMetadata.TitleArtist, .ArtistQuery = sender.StreamMetadata.DefaultArtist}
                                                                                Await dProvider.FetchAsync
                                                                                If dProvider.Track IsNot Nothing Then
                                                                                    SongLinkProvider = "Deezer"
                                                                                    SongLink = dProvider.Track.OriginalPath
                                                                                    CoverLink = If(String.IsNullOrEmpty(dProvider.Track.ThumbnailCoverLink), dProvider.Track.CoverLink, dProvider.Track.ThumbnailCoverLink)
                                                                                End If
                                                                            End Function)
                            End If
                            If String.IsNullOrEmpty(CoverLink) Then
                                Dim SpecialCharacters As String() = New String() {"+", "-", "&&", "||", "!", "(", ")", "{", "}", "[", "]", "^", """", "~", "*", "?", ":", "\/"}
                                Dim SearchQuery As String = ""
                                Dim ETitle = sender.StreamMetadata.Title?.Replace("+", "\+").Replace("-", "\-").Replace("&&", "\&&").Replace("||", "\||").Replace("!", "\!").Replace("(", "\(").Replace(")", "\)").Replace("{", "\{").Replace("}", "\}").Replace("[", "\[").Replace("]", "\]").Replace("^", "\^").Replace("""", "\""").Replace("~", "\~").Replace("*", "\*").Replace("?", "\?").Replace(":", "\:").Replace("\/", "\\/")
                                Dim EArtist = sender.StreamMetadata.DefaultArtist?.Replace("+", "\+").Replace("-", "\-").Replace("&&", "\&&").Replace("||", "\||").Replace("!", "\!").Replace("(", "\(").Replace(")", "\)").Replace("{", "\{").Replace("}", "\}").Replace("[", "\[").Replace("]", "\]").Replace("^", "\^").Replace("""", "\""").Replace("~", "\~").Replace("*", "\*").Replace("?", "\?").Replace(":", "\:").Replace("\/", "\\/")
                                SearchQuery = $"Title:{ETitle} AND Artist:{EArtist}"
                                Dim Query As New MetaBrainz.MusicBrainz.Query("QuickBeat", New Version(1, 0, 0, 0))
                                Dim Result As MetaBrainz.MusicBrainz.Interfaces.Searches.ISearchResults(Of MetaBrainz.MusicBrainz.Interfaces.Searches.ISearchResult(Of MetaBrainz.MusicBrainz.Interfaces.Entities.IRecording)) = Nothing
                                Try
                                    Result = Await Query.FindRecordingsAsync(SearchQuery, 1)
                                Catch ex As Exception
                                    Utilities.DebugMode.Instance.Log(Of SharedProperties)(ex.ToString)
                                End Try
                                If Result IsNot Nothing Then
                                    If Result.Results.Count > 0 Then
                                        Dim GUID = Result.Results.FirstOrDefault?.Item.Releases?.FirstOrDefault?.Id
                                        If GUID IsNot Nothing Then
                                            SongLinkProvider = "MusicBrainz"
                                            SongLink = "https://musicbrainz.org/recording/" & Result.Results.FirstOrDefault.Item.Id.ToString
                                            Dim CA As New MetaBrainz.MusicBrainz.CoverArt.CoverArt("QuickBeat", New Version(1, 0, 0, 0), MetaBrainz.MusicBrainz.CoverArt.CoverArt.DefaultContactInfo)
                                            Try
                                                Dim rCA = Await CA.FetchReleaseAsync(GUID)
                                                If rCA.Images.Count = 1 Then
                                                    CoverLink = rCA.Images.FirstOrDefault?.Location?.ToString
                                                ElseIf rCA.Images.Count > 1 Then
                                                    CoverLink = rCA.Images.FirstOrDefault(Function(k) k.Front)?.Location?.ToString
                                                    If String.IsNullOrEmpty(CoverLink) Then
                                                        CoverLink = rCA.Images.FirstOrDefault(Function(k) k.Location IsNot Nothing)?.ToString
                                                    End If
                                                End If
                                            Catch ex As Exception
                                                Utilities.DebugMode.Instance.Log(Of SharedProperties)(ex.ToString)
                                            End Try
                                        End If
                                    End If
                                End If
                            End If
                            End If
                        If String.IsNullOrEmpty(CoverLink) OrElse Text.Encoding.UTF8.GetByteCount(CoverLink) > 256 Then
                            CoverLink = "musicrecord"
                        End If
                        RPC.Assets = New DiscordRPC.Assets() With {.LargeImageKey = CoverLink, .LargeImageText = sender.StreamMetadata.Album.LimitBytes(128, True)}
                        RPC.Timestamps = DiscordRPC.Timestamps.FromTimeSpan(TimeSpan.FromSeconds(sender.Length))
                        If Not String.IsNullOrWhiteSpace(SongLink) Then
                            If SongLinkProvider = "Deezer" Then
                                RPC.Buttons = New DiscordRPC.Button() {New DiscordRPC.Button() With {.Label = "Listen in Deezer", .Url = SongLink}}
                            ElseIf SongLinkProvider = "MusicBrainz" Then
                                RPC.Buttons = New DiscordRPC.Button() {New DiscordRPC.Button() With {.Label = "See in MusicBrainz", .Url = SongLink}}
                            ElseIf sender.StreamMetadata.Provider IsNot Nothing Then
                                If TypeOf sender.StreamMetadata.Provider Is CacheMediaProvider OrElse TypeOf sender.StreamMetadata.Provider Is IOMediaProvider OrElse TypeOf sender.StreamMetadata.Provider Is InternalMediaProvider Then
                                    Try
                                        Dim hUri As New Uri(SongLink)
                                        RPC.Buttons = New DiscordRPC.Button() {New DiscordRPC.Button() With {.Label = ("Listen in " & hUri.Host).LimitBytes(32), .Url = SongLink}}
                                    Catch 'mostly a bad URL
                                    End Try
                                Else
                                    RPC.Buttons = New DiscordRPC.Button() {New DiscordRPC.Button() With {.Label = ("Listen in " & sender.StreamMetadata.Provider?.Name).LimitBytes(32), .Url = SongLink}}
                                End If
                            End If
                        End If
                        DiscordCache.Add(sender.StreamMetadata.TitleArtist, SongLink, SongLinkProvider, CoverLink, sender.StreamMetadata.Album)
                    End If
                    _DiscordPresenceCache = New Tuple(Of Integer, DiscordRPC.RichPresence)(sender.Stream, RPC)
                    If Not DiscordClient.IsDisposed Then DiscordClient.SetPresence(RPC)
                    _DiscordIsFetching = False
                    _Player_PlaystateChanged(sender)
                End If
            Else
                DiscordClient.ClearPresence()
            End If
        End Sub

        Private Sub _Player_PositionChanged(newPosition As Double) Handles _Player.PositionChanged
            If Not DiscordClient?.IsInitialized OrElse DiscordClient?.CurrentPresence Is Nothing OrElse Not Player.IsPlaying OrElse _DiscordPresenceCache Is Nothing Then Return
            If Player.Stream = _DiscordPresenceCache.Item1 Then
                _DiscordPresenceCache.Item2.Timestamps = DiscordRPC.Timestamps.FromTimeSpan(TimeSpan.FromSeconds(Player.Length - newPosition))
                If Not DiscordClient.IsDisposed Then DiscordClient?.SetPresence(_DiscordPresenceCache.Item2)
            End If
        End Sub
#End Region
#End Region
    End Class
End Namespace

'TODO maybe make github docs
'TODO test deezer integration more, bind queries to library or something
'TODO add audio effect attirbute : control

'TODO fix search cache issue in relaese mode!!!!!!!!
'TODO test release version
'TODO Deploy


'TODO block multi instances
'TODO test quicklink chat ; fix UI design 'LOW PRIORITY
'TODO add artist info view (click on right sidebar) like spotify 'LOW PRIORITY
'TODO test GDI rendering to controls handles rather than winform element host , FIx size overflowing , Maybe use that only when in fullscreen
'TODO add Featured
'TODO add Deezer Settings: Select Image Quality
'TODO fix ytdl playlistsc
'TODO add dev (like settings browser) cache browser
'TODO update all en_us to title case
'TODO add Open graph support (low-priority, since it's running on LAN)
'TODO Redisgn audio effect selection in settings
'TODO check audio profile sometimes not working when clear all (test it first)

'TODO add assembly cache for type generation
'Add Import KeyWord
'TODO fix aqua sub prop prediction when typeing nested proerties
'TODO investigate index selection in collection printig
'TODO fix sub proerties viewer
'TODO add object browser
'TODO add commong helpers for aqua (string.join,split...)
'TODO add Delete Song and Send To Trash
'TODO add pause other players when this is playing and vice versa 'DROP for now , buggy
'TODO *ADD PING METHOD TO PIPE , AND IF NOT PINGED BACK KILL PROCESS WITH A DIALOG
'TODO maybe add metadata dumping/import/export in tag editor or sum like that (?)
'TODO Test new url detection in next instance (named pipe manager)
'TODO add next instance noactivate arg
'TODO add simple mode (simple ui)
'TODO fix double preivew cleans the syncproc Player@ln.1330, poss. solution: stores end procs by HStream and clean at StreamStop

'TODO add jumplist / discord display text (album,artist,title...)
'TODO add more animations

'TODO complete Aqua new method system
'TODO fix dev console formatting adding space and things
'TODO add logic comparison
'TODO add object browser and type locator
'TODO add custom themes
'TODO complete aqua
'TODO try and fix airspace problem
'TODO fix metadata changing on previous stream metadata , remote tags reading kicking in before the streammetadata is changed // shouldn't be a problem since load url is not adding to playlist
'TODO plugins*
'Changelog:
'Fixed Crash When Playing a New Song Using QuickLink
'Fixed Some videos not playing with youtube-dl
'Fixed Yotuube-Dl/"Find and Open" ignores search provider
'Aqua:

