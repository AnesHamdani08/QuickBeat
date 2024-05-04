Imports QuickBeat.Utilities

Class Application
    ' Application-level events, such as Startup, Exit, and DispatcherUnhandledException
    ' can be handled in this file.

    Private Sub Application_Startup(sender As Object, e As StartupEventArgs) Handles Me.Startup
        Dim sp As New SplashScreen("Resources/MusicRecord.png") : sp.Show(True)
        AddHandler System.AppDomain.CurrentDomain.UnhandledException, AddressOf Domain_UnhandledException
        Try
            If My.Settings.APP_FIRSTRUN Then
                My.Settings.Upgrade()
                My.Settings.Save()
            End If
        Catch ex As System.Configuration.ConfigurationException
            System.Windows.MessageBox.Show("Critical Error at the User Settings Engine." & Environment.NewLine & "This is Mostly an Indication that the Configuration File is Corrupted, Please Delete it and We'll Restore it for You in the Next App Startup." & Environment.NewLine & "Affected file: " & If(TryCast(ex.InnerException, System.Configuration.ConfigurationException)?.Filename, "Unknown"), "QuickBeat", MessageBoxButton.OK, MessageBoxImage.Error)
            If Not String.IsNullOrEmpty(TryCast(ex.InnerException, System.Configuration.ConfigurationException)?.Filename) Then Process.Start("explorer.exe", "/select," & TryCast(ex.InnerException, System.Configuration.ConfigurationException)?.Filename)
            Process.GetCurrentProcess.Kill()
        End Try
        'Rendering tier 0 - No graphics hardware acceleration. The DirectX version level Is less than version 7.0.
        'Rendering tier 1 - Partial graphics hardware acceleration. The DirectX version level Is greater than Or equal To version 7.0, And lesser than version 9.0.
        'Rendering tier 2 - Most graphics features use graphics hardware acceleration. The DirectX version level Is greater than Or equal to version 9.0.
        'Dim tier = RenderCapability.Tier >> 16
        Dim _BypassSingleInstance As Boolean = False
        If e.Args.FirstOrDefault = "FileAss" Then
            Select Case e.Args(1)
                Case "0"
                    Dim r = Utilities.CommonFunctions.ManageAssociations(0)
                    Dim i = 0
                    For Each res In r
                        If Not res Then i += 1
                    Next
                    If i > 0 Then
                        HandyControl.Controls.MessageBox.Show(Utilities.ResourceResolver.Strings.QUERY_ERROR_UNKOWN_MULTIPLE & "[" & i & "]")
                    End If
                Case "1"
                    Dim r = Utilities.CommonFunctions.ManageAssociations(1)
                    Dim i = 0
                    For Each res In r
                        If Not res Then i += 1
                    Next
                    If i > 0 Then
                        HandyControl.Controls.MessageBox.Show(Utilities.ResourceResolver.Strings.QUERY_ERROR_UNKOWN_MULTIPLE & "[" & i & "]")
                    End If
                Case "2"
                    Dim r = Utilities.CommonFunctions.ManageAssociations(2)
                    Dim i = 0
                    For Each res In r
                        If res Then i += 1
                    Next
                    HandyControl.Controls.MessageBox.Show(Utilities.ResourceResolver.Strings.QUERY_FOUND_MUTIPLE & "[" & i & "]")
            End Select
            Process.GetCurrentProcess.Kill()
        End If
        If Not _BypassSingleInstance Then
            Dim procS = Process.GetProcessesByName(Process.GetCurrentProcess.ProcessName) '.Length > 1
            If procS.Where(Function(k) k.MainModule.FileName.ToLower = IO.Path.Combine(My.Application.Info.DirectoryPath, "QuickBeat.exe").ToLower).Count > 1 Then
                Dim Pipe As New Classes.NamedPipeManager("QuickBeatPump", IO.Pipes.PipeDirection.InOut)
                Pipe.Init()
                Select Case e.Args.FirstOrDefault
                    Case "PlayAll"
                        Pipe.Write("PlayAll")
                    Case "ShuffleAll"
                        Pipe.Write("ShufleAll")
                    Case "JList"
                        If e.Args.Length >= 2 Then Pipe.Write("JList/" & e.Args(1))
                    Case "Bypass"
                        If e.Args.Length >= 2 Then Pipe.Write(e.Args(1))
                    Case Else
                        If e.Args.Length = 1 Then
                            If e.Args(0).EndsWith(".m3u") OrElse e.Args(0).EndsWith(".m3u8") OrElse e.Args(0).EndsWith(".qbo") Then
                                Pipe.Write($"load {e.Args.FirstOrDefault}")
                            Else
                                Select Case My.Settings.APP_NEXTINSTANCE_ARGTYPE
                                    Case 0 'Preview
                                        Pipe.Write($"preview {e.Args.FirstOrDefault}")
                                    Case 1 'Append
                                        Pipe.Write($"appendload {e.Args.FirstOrDefault}")
                                    Case 2 'Direct Play
                                        Pipe.Write($"load {e.Args.FirstOrDefault}")
                                End Select
                            End If
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
                End Select
                Process.GetCurrentProcess.Kill()
            End If
        End If

            Debug.Listeners.Add(New Utilities.GenericDebugListener)

        Utilities.DebugMode.Instance.Log(Of Application)("Initializing...")
        Utilities.DebugMode.Instance.Log(Of Application)("Applying Theme")
        If My.Settings.APP_THEME_LIGHT Then
            HandyControl.Themes.ThemeManager.Current.ApplicationTheme = HandyControl.Themes.ApplicationTheme.Light
        Else
            HandyControl.Themes.ThemeManager.Current.ApplicationTheme = HandyControl.Themes.ApplicationTheme.Dark
        End If

        FanartTv.API.Key = "22e4c00b998da3517da3ee9eea9d83eb"

        Utilities.DebugMode.Instance.Log(Of Application)("Initializing SharedProperties...")
        Utilities.SharedProperties.Instance?.Init()
    End Sub

    Private Sub Application_Exit(sender As Object, e As ExitEventArgs) Handles Me.[Exit]
        Utilities.DebugMode.Instance.Log(Of Application)("Initiating Exit Sequence...")
        If SharedProperties.Instance.DiscordClient?.IsInitialized Then
            SharedProperties.Instance.DiscordClient.ClearPresence()
            SharedProperties.Instance.DiscordClient.Dispose()
        End If
        SharedProperties.Instance.Save()
        SharedProperties.Instance.DeInit()
        Utilities.DebugMode.Instance.Log(Of Application)("All Good!, See You on the Next One!")
    End Sub

    Private Sub Application_DispatcherUnhandledException(sender As Object, e As Threading.DispatcherUnhandledExceptionEventArgs) Handles Me.DispatcherUnhandledException
        'Utilities.DebugMode.Instance.Start() 'TODO Enable while debugging only
        If e.Exception Is Nothing Then Return
        DebugMode.Instance.Log(Of Application)(e.Exception.ToString)
        'Disabled because Dispatcher exception generally doesn't terminate the app
        'Using fs As New IO.FileStream(IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "QuickBeat", "Logs", "log " & Now.ToString("MM dd yyyy HH mm ss ffff") & ".log"), IO.FileMode.Create, IO.FileAccess.Write) 'IO.Path.Combine(My.Application.Info.DirectoryPath, "log " & Now.ToString("MM dd yyyy HH mm ss ffff") & ".log"), IO.FileMode.Create, IO.FileAccess.Write)
        '    Using sw As New IO.StreamWriter(fs)
        '        'sw.WriteLine(e.Exception?.ToString)
        '        sw.WriteLine(DebugMode.Instance.Cache)
        '    End Using
        'End Using
        e.Handled = True
    End Sub

    Private Sub Domain_UnhandledException(sender As Object, e As UnhandledExceptionEventArgs)
        'Utilities.DebugMode.Instance.Start()        
        If e.ExceptionObject Is Nothing Then Return
        DebugMode.Instance.Log(Of Application)(e.ExceptionObject.ToString)
        If e.IsTerminating Then
            'Using fs As New IO.FileStream(IO.Path.Combine(My.Application.Info.DirectoryPath, "log " & Now.ToString("MM dd yyyy HH mm ss ffff") & ".log"), IO.FileMode.Create, IO.FileAccess.Write)
            Using fs As New IO.FileStream(IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "QuickBeat", GetEncodedSpecialAppName, "Logs", "log " & Now.ToString("MM dd yyyy HH mm ss ffff") & ".log"), IO.FileMode.Create, IO.FileAccess.Write)
                Using sw As New IO.StreamWriter(fs)
                    'sw.WriteLine(ex?.ToString)
                    sw.WriteLine(DebugMode.Instance.Cache)
                End Using
            End Using
        End If
    End Sub

    Private Sub Application_Activated(sender As Object, e As EventArgs) Handles Me.Activated
        SharedProperties.Instance.OnAppActivated()
    End Sub

    Private Sub Application_Deactivated(sender As Object, e As EventArgs) Handles Me.Deactivated
        SharedProperties.Instance.OnAppDeactivated()
    End Sub
End Class
