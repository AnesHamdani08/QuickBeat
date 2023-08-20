Imports System.ComponentModel
Imports System.Runtime.InteropServices.WindowsRuntime
Imports QuickBeat.Utilities

Class MainWindow
#Region "Fields"
    Private Sub LibraryFocusHandler(g As Player.MetadataGroup)
        TabControl_Main.SelectedIndex = 5
    End Sub

    Public _SubtilesPositionSyncProcs As New List(Of Tuple(Of Integer, Integer, Un4seen.Bass.SYNCPROC))

#End Region
#Region "Properties"
    Public Shared HomeTabSwitchTicksLeftProperty As DependencyProperty = DependencyProperty.Register("HomeTabSwitchTicksLeft", GetType(Integer), GetType(MainWindow))
    Property HomeTabSwitchTicksLeft As Integer
        Get
            Return GetValue(HomeTabSwitchTicksLeftProperty)
        End Get
        Set(value As Integer)
            SetValue(HomeTabSwitchTicksLeftProperty, value)
        End Set
    End Property

    Public Shared IsNowPlayingProperty As DependencyProperty = DependencyProperty.Register("IsNowPlaying", GetType(Boolean), GetType(MainWindow))
    Property IsNowPlaying As Boolean
        Get
            Return GetValue(IsNowPlayingProperty)
        End Get
        Set(value As Boolean)
            SetValue(IsNowPlayingProperty, value)
        End Set
    End Property

    Public Shared InfoTextProperty As DependencyProperty = DependencyProperty.Register("InfoText", GetType(String), GetType(MainWindow))
    Property InfoText As String
        Get
            Return GetValue(InfoTextProperty)
        End Get
        Set(value As String)
            SetValue(InfoTextProperty, value)
        End Set
    End Property

    Public Shared VersionProperty As DependencyProperty = DependencyProperty.Register("Version", GetType(String), GetType(MainWindow))
    Property Version As String
        Get
            Return GetValue(VersionProperty)
        End Get
        Set(value As String)
            SetValue(VersionProperty, value)
        End Set
    End Property

    Public Shared IsBigCoverProperty As DependencyProperty = DependencyProperty.Register("IsBigCover", GetType(Boolean), GetType(MainWindow), New PropertyMetadata(True))
    Property IsBigCover As Boolean
        Get
            Return GetValue(IsBigCoverProperty)
        End Get
        Set(value As Boolean)
            SetValue(IsBigCoverProperty, value)
        End Set
    End Property

    Public Shared ShowVisualizerControlsProperty As DependencyProperty = DependencyProperty.Register("ShowVisualizerControls", GetType(Boolean), GetType(MainWindow), New PropertyMetadata(True))
    Property ShowVisualizerControls As Boolean
        Get
            Return GetValue(ShowVisualizerControlsProperty)
        End Get
        Set(value As Boolean)
            SetValue(ShowVisualizerControlsProperty, value)
        End Set
    End Property

    Public Shared IsBottomControlsProperty As DependencyProperty = DependencyProperty.Register("IsBottomControls", GetType(Boolean), GetType(MainWindow), New PropertyMetadata(True))
    Property IsBottomControls As Boolean
        Get
            Return GetValue(IsBottomControlsProperty)
        End Get
        Set(value As Boolean)
            SetValue(IsBottomControlsProperty, value)
        End Set
    End Property

    Public Shared SubtitlesPathProperty As DependencyProperty = DependencyProperty.Register("SubtitlesPath", GetType(String), GetType(MainWindow), New UIPropertyMetadata(New PropertyChangedCallback(Sub(d, e)
                                                                                                                                                                                                          If d IsNot Nothing AndAlso TypeOf d Is MainWindow Then
                                                                                                                                                                                                              With TryCast(d, MainWindow)
                                                                                                                                                                                                                  .IsUsingSubtitles = False
                                                                                                                                                                                                                  .Subtitles?.Clear()
                                                                                                                                                                                                                  .InfoText = ""
                                                                                                                                                                                                                  For Each item In ._SubtilesPositionSyncProcs
                                                                                                                                                                                                                      Un4seen.Bass.Bass.BASS_ChannelRemoveSync(item.Item1, item.Item2)
                                                                                                                                                                                                                  Next
                                                                                                                                                                                                                  ._SubtilesPositionSyncProcs.Clear()
                                                                                                                                                                                                              End With
                                                                                                                                                                                                              If IO.File.Exists(e.NewValue) Then
                                                                                                                                                                                                                  Dim subP As New SubtitlesParser.Classes.Parsers.SubParser
                                                                                                                                                                                                                  Using fs As New IO.FileStream(e.NewValue, IO.FileMode.Open, IO.FileAccess.Read, IO.FileShare.Read)
                                                                                                                                                                                                                      d?.SetValue(MainWindow.SubtitlesProperty, New ObjectModel.ObservableCollection(Of SubtitlesParser.Classes.SubtitleItem))
                                                                                                                                                                                                                      For Each item In subP.ParseStream(fs)
                                                                                                                                                                                                                          TryCast(d, MainWindow).Subtitles?.Add(item)
                                                                                                                                                                                                                          Dim PosSyncProc As New Un4seen.Bass.SYNCPROC(Sub(h, c, data, u)
                                                                                                                                                                                                                                                                           Application.Current.Dispatcher.Invoke(Sub()
                                                                                                                                                                                                                                                                                                                     TryCast(d, MainWindow).InfoText = String.Join(Environment.NewLine, item.PlaintextLines)
                                                                                                                                                                                                                                                                                                                 End Sub)
                                                                                                                                                                                                                                                                       End Sub)
                                                                                                                                                                                                                          Dim stream = SharedProperties.Instance.Player?.Stream
                                                                                                                                                                                                                          Dim PosSyncProcHandle = Un4seen.Bass.Bass.BASS_ChannelSetSync(stream, Un4seen.Bass.BASSSync.BASS_SYNC_POS, Un4seen.Bass.Bass.BASS_ChannelSeconds2Bytes(stream, TimeSpan.FromMilliseconds(item.StartTime).TotalSeconds), PosSyncProc, IntPtr.Zero)
                                                                                                                                                                                                                          TryCast(d, MainWindow)._SubtilesPositionSyncProcs.Add(New Tuple(Of Integer, Integer, Un4seen.Bass.SYNCPROC)(stream, PosSyncProcHandle, PosSyncProc))
                                                                                                                                                                                                                      Next
                                                                                                                                                                                                                      fs.Close()
                                                                                                                                                                                                                  End Using
                                                                                                                                                                                                                  subP = Nothing
                                                                                                                                                                                                              End If
                                                                                                                                                                                                          End If
                                                                                                                                                                                                      End Sub)))
    ''' <summary>
    ''' Set to the desired path to load the subtitles if supported.
    ''' </summary>
    ''' <returns></returns>
    Property SubtitlesPath As String
        Get
            Return GetValue(SubtitlesPathProperty)
        End Get
        Set(value As String)
            SetValue(SubtitlesPathProperty, value)
        End Set
    End Property

    Public Shared SubtitlesProperty As DependencyProperty = DependencyProperty.Register("Subtitles", GetType(ObjectModel.ObservableCollection(Of SubtitlesParser.Classes.SubtitleItem)), GetType(MainWindow), New PropertyMetadata(New ObjectModel.ObservableCollection(Of SubtitlesParser.Classes.SubtitleItem)))
    ''' <summary>
    ''' Current loaded subtitles list.
    ''' </summary>
    ''' <returns></returns>
    Property Subtitles As ObjectModel.ObservableCollection(Of SubtitlesParser.Classes.SubtitleItem)
        Get
            Return GetValue(SubtitlesProperty)
        End Get
        Set(value As ObjectModel.ObservableCollection(Of SubtitlesParser.Classes.SubtitleItem))
            SetValue(SubtitlesProperty, value)
        End Set
    End Property

    Public Shared IsUsingSubtitlesProperty As DependencyProperty = DependencyProperty.Register("IsUsingSubtitles", GetType(Boolean), GetType(MainWindow), New PropertyMetadata(New PropertyChangedCallback(Sub(d, e)
                                                                                                                                                                                                               If e.NewValue = False Then
                                                                                                                                                                                                                   d?.SetValue(MainWindow.SubtitlesProperty, Nothing)
                                                                                                                                                                                                                   d?.SetValue(MainWindow.InfoTextProperty, Nothing)
                                                                                                                                                                                                               End If
                                                                                                                                                                                                           End Sub)))
    ''' <summary>
    ''' Only set to false to release current subtitles and handles.
    ''' </summary>
    ''' <returns></returns>
    Property IsUsingSubtitles As Boolean
        Get
            Return GetValue(IsUsingSubtitlesProperty)
        End Get
        Set(value As Boolean)
            SetValue(IsUsingSubtitlesProperty, value)
        End Set
    End Property
#End Region
#Region "Commands"
    Private Sub SetTabIndex_CanExecute(sender As Object, e As CanExecuteRoutedEventArgs)
        e.CanExecute = True
    End Sub

    Private Sub SetTabIndex_Executed(sender As Object, e As ExecutedRoutedEventArgs)
        TabControl_Main.SelectedIndex = e.Parameter
        Select Case TabControl_Main.SelectedIndex
            Case 0
                RadioButton_MainTabControl_Home.IsChecked = True
            Case 1
                RadioButton_MainTabControl_PlaylistQueue.IsChecked = True
            Case 2
                RadioButton_MainTabControl_Library.IsChecked = True
            Case 3
                RadioButton_MainTabControl_Search.IsChecked = True
            Case 4
                RadioButton_MainTabControl_Settings.IsChecked = True
        End Select
        If e.Parameter = TabControl_Main.Items.Count - 1 Then
            IsNowPlaying = True
        Else
            IsNowPlaying = False
        End If
    End Sub

    Private Sub Close_CanExecute(sender As Object, e As CanExecuteRoutedEventArgs)
        e.CanExecute = True
    End Sub

    Private Sub Close_Executed(sender As Object, e As ExecutedRoutedEventArgs)
        Close()
    End Sub

    Private Sub Hide_CanExecute(sender As Object, e As CanExecuteRoutedEventArgs)
        e.CanExecute = True
    End Sub

    Private Sub Hide_Executed(sender As Object, e As ExecutedRoutedEventArgs)
        Hide()
    End Sub

    Private Sub HyperLink_About_Icon_RequestNavigate(sender As Object, e As RequestNavigateEventArgs)
        Process.Start(e.Uri.AbsoluteUri)
    End Sub

    Private Sub SortLibrary_CanExecute(sender As Object, e As CanExecuteRoutedEventArgs)
        e.CanExecute = e.Parameter.ToString.ToLower = "none" OrElse e.Parameter.ToString.ToLower = "title" OrElse e.Parameter.ToString.ToLower = "artist" OrElse e.Parameter.ToString.ToLower = "album"
    End Sub

    Private Sub SortLibrary_Executed(sender As Object, e As ExecutedRoutedEventArgs)
        Select Case e.Parameter?.ToString.ToLower
            Case "none"
                ListBox_Library.Items.SortDescriptions.Clear()
            Case "title"
                ListBox_Library.Items.SortDescriptions.Clear()
                ListBox_Library.Items.SortDescriptions.Add(New SortDescription("Title", ListSortDirection.Ascending))
            Case "artist"
                ListBox_Library.Items.SortDescriptions.Clear()
                ListBox_Library.Items.SortDescriptions.Add(New SortDescription("DefaultArtist", ListSortDirection.Ascending))
            Case "album"
                ListBox_Library.Items.SortDescriptions.Clear()
                ListBox_Library.Items.SortDescriptions.Add(New SortDescription("Album", ListSortDirection.Ascending))
        End Select
    End Sub

    Private Sub OpenInExplorer_CanExecute(sender As Object, e As CanExecuteRoutedEventArgs)
        e.CanExecute = IO.File.Exists(e.Parameter)
    End Sub

    Private Sub OpenInExplorer_Executed(sender As Object, e As ExecutedRoutedEventArgs)
        Process.Start("explorer.exe", "/select," & e.Parameter)
    End Sub

    Private Sub PickPlayerVideoEffect_CanExecute(sender As Object, e As CanExecuteRoutedEventArgs)
        e.CanExecute = Not IsNothing(SharedProperties.Instance?.Player)
    End Sub

    Private Sub PickPlayerVideoEffect_Executed(sender As Object, e As ExecutedRoutedEventArgs)
        Dim vep As New Dialogs.VideoEffectPicker
        If vep.ShowDialog Then
            Dim vfx As Player.VideoEffects.VideoEffect = Activator.CreateInstance(vep.DialogVideoEffectResult)
            vfx.Width = 1280
            vfx.Height = 720
            vfx.Init()
            Grid_NowPlaying_VisualizerHost.Children.Clear()
            If vfx.UsesCustomControl Then
                Grid_NowPlaying_VisualizerHost.Children.Add(vfx.GetCustomControl)
                Grid_NowPlaying_VisualizerHost.Visibility = Visibility.Visible
            Else
                Grid_NowPlaying_VisualizerHost.Visibility = Visibility.Collapsed
            End If
            SharedProperties.Instance.Player.VideoEffect = vfx
            vfx.Start()
            IsNowPlaying = True
            TryCast(Me.TryFindResource("Commands.SetTabIndexCommand"), RoutedUICommand).Execute(6, Me)
        End If
    End Sub

    Private Sub Commands_LoadSubtitles_CanExecute(sender As Object, e As CanExecuteRoutedEventArgs)
        e.CanExecute = True
    End Sub

    Private Sub Commands_LoadSubtitles_Executed(sender As Object, e As ExecutedRoutedEventArgs)
        Dim ofd As New Microsoft.Win32.OpenFileDialog With {.Title = "Subtitles", .Filter = SubtitlesParser.Classes.SubtitlesFormat.SupportedSubtitlesFormatesFileFilter}
        If ofd.ShowDialog Then
            SubtitlesPath = ofd.FileName
        End If
    End Sub

    Private Sub Commands_PlayLibrary_CanExecute(sender As Object, e As CanExecuteRoutedEventArgs)
        e.CanExecute = (Not IsNothing(SharedProperties.Instance.Player) AndAlso Not IsNothing(SharedProperties.Instance.Library) AndAlso SharedProperties.Instance.Library.Count > 0)
    End Sub

    Private Sub Commands_PlayLibrary_Executed(sender As Object, e As ExecutedRoutedEventArgs)
        Dim PL = SharedProperties.Instance.Library.ToPlaylist()
        PL.Name = "Library"
        PL.Category = "Playlist"
        PL.Description = "All Your Music"
        SharedProperties.Instance.Player.LoadPlaylist(PL)
    End Sub

    Private Sub StopPlayerVideoEffect_CanExecute(sender As Object, e As CanExecuteRoutedEventArgs)
        e.CanExecute = Not IsNothing(SharedProperties.Instance?.Player?.VideoEffect)
    End Sub

    Private Sub StopPlayerVideoEffect_Executed(sender As Object, e As ExecutedRoutedEventArgs)
        If SharedProperties.Instance?.Player?.VideoEffect IsNot Nothing Then
            SharedProperties.Instance.Player.VideoEffect = Nothing
            Grid_NowPlaying_VisualizerHost.Children.Clear()
            Grid_NowPlaying_VisualizerHost.Visibility = Visibility.Collapsed
        End If
    End Sub

#End Region

    Private Sub NotifyIcon_Main_MouseDoubleClick(sender As Object, e As RoutedEventArgs)
        Show()
    End Sub

    Private Sub MainWindow_Initialized(sender As Object, e As EventArgs) Handles Me.Initialized
        Utilities.SharedProperties.Instance.ShowNotification = New Utilities.SharedProperties.ShowNotificationDelegate(Sub(t, m, i)
                                                                                                                           Dispatcher.Invoke(Sub()
                                                                                                                                                 NotifyIcon_Main.ShowBalloonTip(t, m, i)
                                                                                                                                             End Sub)
                                                                                                                       End Sub)
        AddHandler SharedProperties.Instance.Player.MediaLoaded, Sub(o, n)
                                                                     Application.Current.Dispatcher.Invoke(Sub()
                                                                                                               SubtitlesPath = Nothing
                                                                                                           End Sub)
                                                                 End Sub
        Dim TabTimer As New Timers.Timer With {.Interval = 1000}
        AddHandler TabTimer.Elapsed, New Timers.ElapsedEventHandler(Sub(s, _e)
                                                                        Try
                                                                            Me.Dispatcher.Invoke(Sub()
                                                                                                     If TabControl_Main.SelectedIndex <> 0 OrElse Me.WindowState = WindowState.Minimized Then
                                                                                                         Return
                                                                                                     End If
                                                                                                     If HomeTabSwitchTicksLeft <= 0 Then 'Just in case ;)
                                                                                                         Select Case TabControl_Home_Sub.SelectedIndex
                                                                                                             Case 0
                                                                                                                 TabControl_Home_Sub.SelectedIndex = 1
                                                                                                             Case 1
                                                                                                                 TabControl_Home_Sub.SelectedIndex = 2
                                                                                                             Case 2
                                                                                                                 TabControl_Home_Sub.SelectedIndex = 0
                                                                                                         End Select
                                                                                                         'TabControl_Home_Sub.SelectedIndex = If(TabControl_Home_Sub.SelectedIndex = 0, 1, 0) '0, 1, 0
                                                                                                         HomeTabSwitchTicksLeft = 5
                                                                                                     Else
                                                                                                         HomeTabSwitchTicksLeft -= 1
                                                                                                     End If
                                                                                                 End Sub)
                                                                        Catch ex As Exception
                                                                        End Try
                                                                    End Sub)
        TabTimer.Start()
        If CType(My.Settings.APP_STARTUPBEHAVIOUR, StartupBehaviour) = StartupBehaviour.Minimize Then
            Me.WindowState = WindowState.Minimized
        End If
        IsBigCover = My.Settings.APP_VIEW_BIGCOVER
        IsBottomControls = My.Settings.APP_VIEW_BOTTOMCONTROLS
        Application.Current.MainWindow = Me
        Version = My.Application.Info.Version.ToString

        If Not String.IsNullOrEmpty(My.Settings.AQUA_STARTUP_SCRIPT) Then
            Try
                SharedProperties.Instance.Aqua.RunFile(My.Settings.AQUA_STARTUP_SCRIPT)
            Catch ex As Exception
                Utilities.DebugMode.Instance.Log(Of MainWindow)(ex.ToString)
            End Try
        End If
    End Sub

    Private Sub MenuItem_KeyboardNavigation_Click(sender As Object, e As RoutedEventArgs)
        HandyControl.Controls.MessageBox.Info(Utilities.ResourceResolver.Strings.HINT_KEYBOARD_NAVIGATION, ResourceResolver.Strings.APPNAME)
    End Sub

    Private Sub MainWindow_PreviewKeyDown(sender As Object, e As KeyEventArgs) Handles Me.PreviewKeyDown
        Dim mods = Keyboard.Modifiers
        Select Case e.Key
            Case Key.F11
                Me.IsFullScreen = Not Me.IsFullScreen
            Case Key.D
                If mods.HasFlag(ModifierKeys.Control) Then
                    If DebugMode.Instance.IsEnabled Then My.Windows.DeveloperConsole.Show()
                End If
            Case Key.Space
                If mods.HasFlag(ModifierKeys.Control) Then
                    SharedProperties.Instance.Player.PlayPauseCommand.Execute(SharedProperties.Instance.Player.Stream)
                    e.Handled = True
                End If
            Case Key.Left
                If mods.HasFlag(ModifierKeys.Control) Then
                    SharedProperties.Instance.Player.PreviousCommand.Execute(SharedProperties.Instance.Player.Playlist)
                    e.Handled = True
                ElseIf mods.HasFlag(ModifierKeys.Shift) Then
                    SharedProperties.Instance.Player.Position -= 10
                    e.Handled = True
                End If
            Case Key.Right
                If mods.HasFlag(ModifierKeys.Control) Then
                    SharedProperties.Instance.Player.NextCommand.Execute(SharedProperties.Instance.Player.Playlist)
                    e.Handled = True
                ElseIf mods.HasFlag(ModifierKeys.Shift) Then
                    SharedProperties.Instance.Player.Position += 10
                    e.Handled = True
                End If
            Case Key.Up
                If mods.HasFlag(ModifierKeys.Control) Then
                    SharedProperties.Instance.Player.Volume += 2
                    e.Handled = True
                End If
            Case Key.Down
                If mods.HasFlag(ModifierKeys.Control) Then
                    SharedProperties.Instance.Player.Volume -= 2
                    e.Handled = True
                End If
            Case Key.S
                If mods.HasFlag(ModifierKeys.Control) Then
                    SharedProperties.Instance.Player.Playlist.IsShuffling = Not SharedProperties.Instance.Player.Playlist.IsShuffling
                    e.Handled = True
                End If
            Case Key.R
                If mods.HasFlag(ModifierKeys.Control) Then
                    SharedProperties.Instance.Player.IsLooping = Not SharedProperties.Instance.Player.IsLooping
                    e.Handled = True
                ElseIf mods.HasFlag(ModifierKeys.Shift) Then
                    SharedProperties.Instance.Player.Playlist.IsLooping = Not SharedProperties.Instance.Player.Playlist.IsLooping
                    e.Handled = True
                End If
            Case Key.N
                If mods.HasFlag(ModifierKeys.Control) Then
                    SharedProperties.Instance.Player.NewPlaylistCommand.Execute(Nothing)
                    e.Handled = True
                End If
            Case Key.F
                If mods.HasFlag(ModifierKeys.Control) Then
                    SharedProperties.Instance.Player.StreamMetadata.IsFavorite = Not SharedProperties.Instance.Player.StreamMetadata.IsFavorite
                    e.Handled = True
                End If
        End Select
    End Sub

    Private Sub MainWindow_Closing(sender As Object, e As CancelEventArgs) Handles Me.Closing
        'Saving UI settings
        My.Settings.APP_VIEW_SIDEBAR_RIGHT_VISIBILITY = Grid_SideBar_Right.Visibility
        My.Settings.APP_VIEW_BIGCOVER = IsBigCover
        My.Settings.APP_VIEW_BOTTOMCONTROLS = IsBottomControls

        My.Settings.Save()

        Select Case CType(My.Settings.APP_CLOSINGBEHAVIOUR, ClosingBehaviour)
            Case ClosingBehaviour.Minimize
                Me.WindowState = WindowState.Minimized
                e.Cancel = True
            Case ClosingBehaviour.SystemTray
                Hide()
                e.Cancel = True
        End Select
    End Sub

    Private Sub ComboBox_Settings_StartupBehaviour_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
        My.Settings.APP_STARTUPBEHAVIOUR = ComboBox_Settings_StartupBehaviour.SelectedIndex
        My.Settings.Save()
    End Sub

    Private Sub ComboBox_Settings_ClosingBehaviour_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
        My.Settings.APP_CLOSINGBEHAVIOUR = ComboBox_Settings_ClosingBehaviour.SelectedIndex
        My.Settings.Save()
    End Sub

    Private Sub MainWindow_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        AddHandler SharedProperties.Instance.Library.AlbumFocused, AddressOf LibraryFocusHandler
        AddHandler SharedProperties.Instance.Library.ArtistFocused, AddressOf LibraryFocusHandler
        AddHandler SharedProperties.Instance.Library.GroupFocused, AddressOf LibraryFocusHandler

        Grid_SideBar_Right.Visibility = My.Settings.APP_VIEW_SIDEBAR_RIGHT_VISIBILITY

        ComboBox_Settings_StartupBehaviour.SelectedIndex = My.Settings.APP_STARTUPBEHAVIOUR
        ComboBox_Settings_ClosingBehaviour.SelectedIndex = My.Settings.APP_CLOSINGBEHAVIOUR
    End Sub

    Private Sub ListBox_Playlist_Loaded(sender As Object, e As RoutedEventArgs) Handles ListBox_Playlist.Loaded
        ListBox_Playlist.ScrollIntoView(ListBox_Playlist.SelectedItem)
    End Sub

    Private Sub MainWindow_Drop(sender As Object, e As DragEventArgs) Handles Me.Drop
        Dim data = e.Data.GetData("FileDrop")
        If TypeOf data Is String() Then
            Dim datalength = CType(data, String()).Length
            For i As Integer = 0 To datalength - 1
                If i = datalength - 1 Then
                    SharedProperties.Instance.Player.LoadAndAddCommand.Execute(data(i))
                Else
                    SharedProperties.Instance.Player.Playlist.Add(Player.Metadata.FromFile(data(i)))
                End If
            Next
        ElseIf TypeOf data Is String Then
            SharedProperties.Instance.Player.LoadAndAddCommand.Execute(data)
        End If
    End Sub

    Private Sub SearchBar_SearchStarted(sender As Object, e As HandyControl.Data.FunctionEventArgs(Of String))
        If e.Source IsNot Nothing AndAlso TypeOf e.Source Is HandyControl.Controls.SearchBar Then
            'SharedProperties.Instance.Library.SearchQuery = TryCast(e.Source, HandyControl.Controls.SearchBar)?.Text
            TryCast(e.Source, HandyControl.Controls.SearchBar)?.Command?.Execute(e.Info)
        End If
    End Sub

    Private Sub Thumb_SideBar_Right_DragDelta(sender As Object, e As Primitives.DragDeltaEventArgs)
        Dim Change = (0 - e.HorizontalChange)
        Dim AddChange = Grid_SideBar_Right.Width + Change
        Grid_SideBar_Right.Width = If(AddChange > Me.Width / 4, Me.Width / 4, If(AddChange < 150, 150, AddChange))
    End Sub

    Private Sub Thumb_Sidebar_Left_DragDelta(sender As Object, e As Primitives.DragDeltaEventArgs)
        Dim Change = e.HorizontalChange
        Dim AddChange = Grid_SideBar_Left.Width + Change
        Grid_SideBar_Left.Width = If(AddChange > Me.Width / 4, Me.Width / 4, If(AddChange < 150, 150, AddChange))
    End Sub

    Private Sub MainWindow_SizeChanged(sender As Object, e As SizeChangedEventArgs) Handles Me.SizeChanged
        Dim ContentWidth = Me.Width / 4
        If Grid_SideBar_Left.Width > ContentWidth Then Grid_SideBar_Left.Width = ContentWidth
        If Grid_SideBar_Right.Width > ContentWidth Then Grid_SideBar_Right.Width = ContentWidth
    End Sub

    Private Sub Grid_NowPlaying_VisualizerHost_SizeChanged(sender As Object, e As SizeChangedEventArgs)
        If SharedProperties.Instance?.Player?.VideoEffect IsNot Nothing Then
            With SharedProperties.Instance.Player.VideoEffect
                .Width = e.NewSize.Width
                .Height = e.NewSize.Height
                .OnSizeChanged()
            End With
        End If
    End Sub

    Private Sub Grid_NowPlaying_VisualizerHost_PreviewMouseLeftButtonUp(sender As Object, e As MouseButtonEventArgs)
        If SharedProperties.Instance?.Player?.VideoEffect IsNot Nothing Then
            SharedProperties.Instance.Player.VideoEffect.OnMouseClick(e.GetPosition(Grid_NowPlaying_VisualizerHost))
        End If
    End Sub

    Private Sub Image_VisualizerOutput_PreviewMouseLeftButtonUp(sender As Object, e As MouseButtonEventArgs)
        If SharedProperties.Instance?.Player?.VideoEffect IsNot Nothing Then
            SharedProperties.Instance.Player.VideoEffect.OnMouseClick(e.GetPosition(Image_VisualizerOutput))
        End If
    End Sub

    Private Sub Thumb_Sidebar_Right_MouseDoubleClick(sender As Object, e As MouseButtonEventArgs)
        Grid_SideBar_Right.Visibility = If(Grid_SideBar_Right.Visibility = Visibility.Visible, Visibility.Collapsed, Visibility.Visible)
    End Sub

    Private Sub Button_Settings_Reset_Click(sender As Object, e As RoutedEventArgs)
        If HandyControl.Controls.MessageBox.Ask(Utilities.ResourceResolver.Strings.QUERY_SETTINGSRESET) = MessageBoxResult.OK Then
            My.Settings.Reset()
            My.Settings.Save()
        End If
    End Sub

    Private Sub Button_Settings_ForceStop_Click(sender As Object, e As RoutedEventArgs)
        If HandyControl.Controls.MessageBox.Ask(Utilities.ResourceResolver.Strings.QUERY_FORCESTOP) = MessageBoxResult.OK Then
            Process.GetCurrentProcess.Kill()
        End If
    End Sub
    Private Async Sub MenuItem_CheckForUpdates_Click(sender As Object, e As RoutedEventArgs)
        Try
            Dim Vinfo = Await HandyControl.Tools.UpdateHelper.CheckUpdateAsync("AnesHamdani08", "QuickBeat")
            If Vinfo.IsExistNewVersion Then
                Dim SB As New Text.StringBuilder()
                SB.AppendLine(Utilities.ResourceResolver.Strings.QUERY_UPDATES_FOUND)
                SB.AppendLine(Vinfo.TagName & If(Vinfo.IsPreRelease, "(Pre-Release)", ""))
                SB.AppendLine(Vinfo.Changelog)
                HandyControl.Controls.MessageBox.Warning(SB.ToString)
            Else
                HandyControl.Controls.MessageBox.Success(Utilities.ResourceResolver.Strings.LOC_QUERY_UPDATES_NOTFOUND)
            End If
        Catch ex As Exception
            Utilities.DebugMode.Instance.Log(Of MainWindow)(ex.ToString)
            HandyControl.Controls.MessageBox.Error(Utilities.ResourceResolver.Strings.QUERY_UPDATES_ERROR)
        End Try
    End Sub

End Class
