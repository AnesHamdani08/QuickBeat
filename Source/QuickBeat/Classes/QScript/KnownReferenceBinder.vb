Imports QuickBeat.Player.WPF.Commands

Namespace QScript
    Public Class KnownReferenceBinder
        Implements Aqua.IReferenceBinder

        Public Function GetReference(Type As Type) As Object Implements Aqua.IReferenceBinder.GetReference
            Select Case Type
                Case GetType(Player.Player)
                    Return Utilities.SharedProperties.Instance.Player
                Case GetType(Player.Playlist)
                    Return Utilities.SharedProperties.Instance.Player?.Playlist
                Case GetType(Hotkeys.HotkeyManager)
                    Return Utilities.SharedProperties.Instance.HotkeyManager
                Case GetType(Library.Library)
                    Return Utilities.SharedProperties.Instance.Library
                Case GetType(Utilities.SharedProperties)
                    Return Utilities.SharedProperties.Instance
                Case GetType(QScript.SettingsBrowser)
                    Return My.Windows.SettingsBrowser
                Case GetType(Classes.NamedPipeManager)
                    Return Utilities.SharedProperties.Instance.FilePipe
                Case GetType(Classes.Themes)
                    Return Utilities.SharedProperties.Instance.Themes
                Case GetType(Youtube.YoutubeDL)
                    Return Utilities.SharedProperties.Instance.YoutubeDL
                Case GetType(Speech.Synthesis.SpeechSynthesizer)
                    Return Utilities.SharedProperties.Instance.TTSE
                Case GetType(ObjectModel.ObservableCollection(Of Player.Playlist))
                    Return Utilities.SharedProperties.Instance.CustomPlaylists
                Case GetType(ObjectModel.ObservableCollection(Of Player.Playlist))
                    Return Utilities.SharedProperties.Instance.RecommendedPlaylists
                Case GetType(Classes.MemoryManager)
                    Return Utilities.SharedProperties.Instance.MemoryManager
                Case GetType(Classes.SleepTimer)
                    Return Utilities.SharedProperties.Instance.SleepTimer
                Case GetType(ObjectModel.ObservableCollection(Of Type))
                    Return Utilities.SharedProperties.Instance.Themes
                Case GetType(DiscordRPC.DiscordRpcClient)
                    Return Utilities.SharedProperties.Instance.DiscordClient
                Case GetType(UPnP.HttpFileServer)
                    Return Utilities.SharedProperties.Instance.HTTPFileServer
                Case GetType(Utilities.SharedProperties.ShowNotificationDelegate)
                    Return Utilities.SharedProperties.Instance.ShowNotification
                Case GetType(Utilities.DebugMode)
                    Return Utilities.DebugMode.Instance
            End Select
            Return Nothing
        End Function

        Public Function GetReference(Name As String) As Object Implements Aqua.IReferenceBinder.GetReference
            Select Case Name.ToLower
                Case "player"
                    Return Utilities.SharedProperties.Instance.Player
                Case "playlist"
                    Return Utilities.SharedProperties.Instance.Player?.Playlist
                Case "hotkeymanager"
                    Return Utilities.SharedProperties.Instance.HotkeyManager
                Case "library"
                    Return Utilities.SharedProperties.Instance.Library
                Case "sharedproperties"
                    Return Utilities.SharedProperties.Instance
                Case "settingsbrowser"
                    Return My.Windows.SettingsBrowser
                Case "namedpipemanager"
                    Return Utilities.SharedProperties.Instance.FilePipe
                Case "themes"
                    Return Utilities.SharedProperties.Instance.Themes
                Case "youtubedl"
                    Return Utilities.SharedProperties.Instance.YoutubeDL
                Case "tts"
                    Return Utilities.SharedProperties.Instance.TTSE
                Case "customplaylists"
                    Return Utilities.SharedProperties.Instance.CustomPlaylists
                Case "recommendedplaylists"
                    Return Utilities.SharedProperties.Instance.RecommendedPlaylists
                Case "memorymanager"
                    Return Utilities.SharedProperties.Instance.MemoryManager
                Case "sleeptimer"
                    Return Utilities.SharedProperties.Instance.SleepTimer
                Case "themes"
                    Return Utilities.SharedProperties.Instance.Themes
                Case "discordclient"
                    Return Utilities.SharedProperties.Instance.DiscordClient
                Case "httpfileserver"
                    Return Utilities.SharedProperties.Instance.HTTPFileServer
                Case "shownotification"
                    Return Utilities.SharedProperties.Instance.ShowNotification
                Case "api"
                    Return Utilities.SharedProperties.Instance.APIPipe
                Case "debugmode"
                    Return Utilities.DebugMode.Instance
            End Select
            Return Nothing
        End Function

        Public Function GetAvailableReferencesNames() As String() Implements Aqua.IReferenceBinder.GetAvailableReferencesNames
            Return New String() {"Player", "Playlist", "HotkeyManager", "Library", "SharedProperties", "SettingsBrowser", "NamedPipeManager", "Themes", "YoutubeDL", "TTS",
                "CustomPlaylists", "RecommendedPlaylists", "MemoryManager", "SleepTimer", "Themes", "DiscordClient", "HTTPFileServer", "ShowNotification", "API", "DebugMode"}
        End Function
    End Class
End Namespace