Imports QuickBeat.Utilities
Imports Windows.Foundation

Class Application
    ' Application-level events, such as Startup, Exit, and DispatcherUnhandledException
    ' can be handled in this file.

    Private Sub Application_Startup(sender As Object, e As StartupEventArgs) Handles Me.Startup
        If My.Settings.APP_FIRSTRUN Then
            My.Settings.Upgrade()
            My.Settings.APP_FIRSTRUN = False
            My.Settings.Save()
        End If
        If Process.GetProcessesByName(Process.GetCurrentProcess.ProcessName).Length > 1 Then
            Dim Pipe As New Classes.NamedPipeManager("QuickBeatPump", IO.Pipes.PipeDirection.InOut)
            Pipe.Init()
            If e.Args.Length = 1 Then
                Pipe.Write($"preview {e.Args.FirstOrDefault}")
            ElseIf e.Args.Length > 0 Then
                For i As Integer = 0 To e.Args.Length - 1
                    If i = e.Args.Length - 1 Then
                        Pipe.Write($"appendload {e.Args(i)}")
                    Else
                        Pipe.Write($"append {e.Args(i)}")
                    End If
                Next
            Else
                Pipe.Write("activate")
            End If
            Process.GetCurrentProcess.Kill()
        End If

        Console.SetOut(New Utilities.MultiTextWriter.ControlWriter(My.Windows.DeveloperConsole.ConsoleOut_TB, My.Windows.DeveloperConsole.Dispatcher))

        If SharedProperties.Instance.IsLogging Then Utilities.DebugMode.Instance.Log(Of Application)("Initializing...")
        If SharedProperties.Instance.IsLogging Then Utilities.DebugMode.Instance.Log(Of Application)("Applying Theme")
        If My.Settings.APP_THEME_LIGHT Then
            HandyControl.Themes.ThemeManager.Current.ApplicationTheme = HandyControl.Themes.ApplicationTheme.Light
        Else
            HandyControl.Themes.ThemeManager.Current.ApplicationTheme = HandyControl.Themes.ApplicationTheme.Dark
        End If

        Debug.WriteLine("Initializing SharedProperties...")
        Utilities.SharedProperties.Instance.Init()
    End Sub

    Private Sub Application_Exit(sender As Object, e As ExitEventArgs) Handles Me.[Exit]
        If SharedProperties.Instance.IsLogging Then Utilities.DebugMode.Instance.Log(Of Application)("Initiating Exit Sequence...")
        Dim Lib64 = SharedProperties.Instance.Library.Save() : If SharedProperties.Instance.IsLogging Then Utilities.DebugMode.Instance.Log(Of Application)("Generator 1: 60%")
        Dim Hk64 = SharedProperties.Instance.HotkeyManager.Save : If SharedProperties.Instance.IsLogging Then Utilities.DebugMode.Instance.Log(Of Application)("Generator 1: 80%")
        Dim Pl64 = SharedProperties.Instance.Player.SavePlaylist : If SharedProperties.Instance.IsLogging Then Utilities.DebugMode.Instance.Log(Of Application)("Generator 1: 100%")
        Dim BinF As New System.Runtime.Serialization.Formatters.Binary.BinaryFormatter

        If SharedProperties.Instance.IsLogging Then Utilities.DebugMode.Instance.Log(Of Application)("Generator 1 Now Activated.")
        If SharedProperties.Instance.IsLogging Then Utilities.DebugMode.Instance.Log(Of Application)("Saving...")
        My.Settings.APP_LIBRARY.Clear()
        My.Settings.APP_PLAYER_PLAYLIST_CUSTOM.Clear()

        My.Settings.APP_PLAYER_SETTINGS = SharedProperties.Instance.Player.SaveSettings

        For Each item In Lib64
            My.Settings.APP_LIBRARY.Add(item)
        Next
        For Each item In SharedProperties.Instance.CustomPlaylists
            If item Is Nothing OrElse item.Count = 0 Then Continue For
            Dim PlMem As New IO.MemoryStream
            BinF.Serialize(PlMem, item)
            My.Settings.APP_PLAYER_PLAYLIST_CUSTOM.Add(Convert.ToBase64String(PlMem.ToArray))
        Next

        My.Settings.APP_HOTKEYS = Convert.ToBase64String(Hk64.ToArray)
        My.Settings.APP_PLAYER_PLAYLIST = Convert.ToBase64String(Pl64.ToArray)

        'Recent songs
        Dim RecentlyPlayed = SharedProperties.Instance.CustomPlaylists.FirstOrDefault(Function(k) k.Tag = "Recent")
        If RecentlyPlayed IsNot Nothing Then
            My.Settings.APP_PLAYER_RECENT.Clear()
            For i As Integer = 0 To My.Settings.APP_PLAYER_RECENT_LIMIT - 1
                Dim meta = RecentlyPlayed(i)
                If meta.Location = QuickBeat.Player.Metadata.FileLocation.Local Then
                    Dim MDMem As New IO.MemoryStream
                    BinF.Serialize(MDMem, meta)
                    My.Settings.APP_PLAYER_RECENT.Insert(0, Convert.ToBase64String(MDMem.ToArray))
                End If
            Next
        End If

        If Not String.IsNullOrEmpty(SharedProperties.Instance.YoutubeDL.YoutubeDLLocation) Then My.Settings.APP_YOUTUBEDL_PATH = SharedProperties.Instance.YoutubeDL.YoutubeDLLocation
        My.Settings.APP_THEME_INDEX = SharedProperties.Instance.ThemesSelectedIndex
        My.Settings.APP_THEME_LIGHT = If(HandyControl.Themes.ThemeManager.Current.ActualApplicationTheme = HandyControl.Themes.ApplicationTheme.Light, True, False)

        My.Settings.Save()
        If SharedProperties.Instance.IsLogging Then Utilities.DebugMode.Instance.Log(Of Application)("All Good!, See You on the Next One!")
    End Sub
End Class
