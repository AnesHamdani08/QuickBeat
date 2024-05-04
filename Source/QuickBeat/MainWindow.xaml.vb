Imports System.ComponentModel
Imports System.Net.Security
Imports System.Runtime.InteropServices.WindowsRuntime
Imports System.Web.UI.WebControls
Imports HandyControl.Data
Imports Microsoft.Web.WebView2.Core
Imports QuickBeat.SubtitlesParser.Classes
Imports QuickBeat.Utilities

Class MainWindow
#Region "Events"
    Public Event CurrentSubtitleChanged(oldValue As String, newValue As String, item As SubtitlesParser.Classes.SubtitleItem)
#End Region
#Region "Raisers"
    Protected Sub OnCurrentSubtitleChanged(oldValue As String, newValue As String, item As SubtitlesParser.Classes.SubtitleItem)
        SubtitlesItemsControlScrollIntoView(item)
        RaiseEvent CurrentSubtitleChanged(oldValue, newValue, item)
    End Sub
#End Region
#Region "Fields"
    Private Sub LibraryFocusHandler(g As Player.MetadataGroup)
        TryCast(Me.TryFindResource("Commands.SetTabIndexCommand"), RoutedUICommand).Execute(5, Me)
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

    Public Shared HomeSubTabSelectedIndexProperty As DependencyProperty = DependencyProperty.Register("HomeSubTabSelectedIndex", GetType(Integer), GetType(MainWindow))
    Property HomeSubTabSelectedIndex As Integer
        Get
            Return GetValue(HomeSubTabSelectedIndexProperty)
        End Get
        Set(value As Integer)
            SetValue(HomeSubTabSelectedIndexProperty, value)
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

    Public Shared IsPatternSelectionProperty As DependencyProperty = DependencyProperty.Register("IsPatternSelection", GetType(Boolean), GetType(MainWindow))
    Property IsPatternSelection As Boolean
        Get
            Return GetValue(IsPatternSelectionProperty)
        End Get
        Set(value As Boolean)
            SetValue(IsPatternSelectionProperty, value)
        End Set
    End Property

    Public Shared IsWebViewInstalledProperty As DependencyProperty = DependencyProperty.Register("IsWebViewInstalled", GetType(Boolean), GetType(MainWindow), New UIPropertyMetadata(IsWebViewRuntimeInstalled))
    Property IsWebViewInstalled As Boolean
        Get
            Return GetValue(IsWebViewInstalledProperty)
        End Get
        Set(value As Boolean)
            SetValue(IsWebViewInstalledProperty, value)
        End Set
    End Property

    Public Shared IsLiveBackgroundProperty As DependencyProperty = DependencyProperty.Register("IsLiveBackground", GetType(Boolean), GetType(MainWindow))
    Property IsLiveBackground As Boolean
        Get
            Return GetValue(IsLiveBackgroundProperty)
        End Get
        Set(value As Boolean)
            SetValue(IsLiveBackgroundProperty, value)
        End Set
    End Property

    Public Shared IsDiscordProperty As DependencyProperty = DependencyProperty.Register("IsDiscord", GetType(Boolean), GetType(MainWindow), New PropertyMetadata(My.Settings.APP_DISCORD, New PropertyChangedCallback(Sub(s, e)
                                                                                                                                                                                                                          SharedProperties.Instance.IsDiscordClientInitialized = e.NewValue
                                                                                                                                                                                                                      End Sub)))
    Property IsDiscord As Boolean
        Get
            Return GetValue(IsDiscordProperty)
        End Get
        Set(value As Boolean)
            SetValue(IsDiscordProperty, value)
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

    Public Shared CurrentSubtitleProperty As DependencyProperty = DependencyProperty.Register("CurrentSubtitle", GetType(SubtitlesParser.Classes.SubtitleItem), GetType(MainWindow))
    Property CurrentSubtitle As SubtitlesParser.Classes.SubtitleItem
        Get
            Return GetValue(CurrentSubtitleProperty)
        End Get
        Set(value As SubtitlesParser.Classes.SubtitleItem)
            SetValue(CurrentSubtitleProperty, value)
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

    Public Shared AquaVersionProperty As DependencyProperty = DependencyProperty.Register("AquaVersion", GetType(String), GetType(MainWindow))
    Property AquaVersion As String
        Get
            Return GetValue(AquaVersionProperty)
        End Get
        Set(value As String)
            SetValue(AquaVersionProperty, value)
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

    Public Shared ShowPopupNotificationProperty As DependencyProperty = DependencyProperty.Register("ShowPopupNotification", GetType(Boolean), GetType(MainWindow), New PropertyMetadata(My.Settings.APP_PLAYER_MEDIALOADED_SHOWPOPUP, New PropertyChangedCallback(Sub(s, e)
                                                                                                                                                                                                                                                                       Dim bV As Boolean
                                                                                                                                                                                                                                                                       If Boolean.TryParse(e.NewValue?.ToString, bV) Then
                                                                                                                                                                                                                                                                           If bV Then
                                                                                                                                                                                                                                                                               If SharedProperties.Instance.ActiveControlPopup Is Nothing Then SharedProperties.Instance.ActiveControlPopup = New UI.Windows.ActiveControlPopup With {.AutoClose = False, .Timeout = 4000}
                                                                                                                                                                                                                                                                           Else
                                                                                                                                                                                                                                                                               SharedProperties.Instance.ActiveControlPopup?.Close()
                                                                                                                                                                                                                                                                               SharedProperties.Instance.ActiveControlPopup = Nothing
                                                                                                                                                                                                                                                                           End If
                                                                                                                                                                                                                                                                       End If
                                                                                                                                                                                                                                                                   End Sub)))
    Property ShowPopupNotification As Boolean
        Get
            Return GetValue(ShowPopupNotificationProperty)
        End Get
        Set(value As Boolean)
            SetValue(ShowPopupNotificationProperty, value)
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

    Public Shared IsJumpListFrequentProperty As DependencyProperty = DependencyProperty.Register("IsJumpListFrequent", GetType(Boolean), GetType(MainWindow), New PropertyMetadata(SharedProperties.Instance.JumpList_ShowFrequent, New PropertyChangedCallback(Sub(s, e)
                                                                                                                                                                                                                                                                    SharedProperties.Instance.JumpList_ShowFrequent = e.NewValue
                                                                                                                                                                                                                                                                    Dim JTaskPlayAll As System.Windows.Shell.JumpTask = Nothing
                                                                                                                                                                                                                                                                    Dim JTaskShuffleAll As System.Windows.Shell.JumpTask = Nothing
                                                                                                                                                                                                                                                                    If SharedProperties.Instance.JumpList_ShowTasks Then
                                                                                                                                                                                                                                                                        JTaskPlayAll = New System.Windows.Shell.JumpTask() With {.Title = "Play All", .Description = "Play All Your Songs", .CustomCategory = "Tasks", .ApplicationPath = IO.Path.Combine(My.Application.Info.DirectoryPath, "QuickBeat.exe"), .Arguments = "PlayAll"}
                                                                                                                                                                                                                                                                        JTaskShuffleAll = New System.Windows.Shell.JumpTask() With {.Title = "Shuffle All", .Description = "Shuffle All Your Songs", .CustomCategory = "Tasks", .ApplicationPath = IO.Path.Combine(My.Application.Info.DirectoryPath, "QuickBeat.exe"), .Arguments = "ShuffleAll"}
                                                                                                                                                                                                                                                                    End If
                                                                                                                                                                                                                                                                    Dim JList As New System.Windows.Shell.JumpList(If(SharedProperties.Instance.JumpList_ShowTasks, {JTaskPlayAll, JTaskShuffleAll}, {}), SharedProperties.Instance.JumpList_ShowFrequent, True)
                                                                                                                                                                                                                                                                    System.Windows.Shell.JumpList.SetJumpList(Application.Current, JList)
                                                                                                                                                                                                                                                                End Sub)))
    Property IsJumpListFrequent As Boolean
        Get
            Return GetValue(IsJumpListFrequentProperty)
        End Get
        Set(value As Boolean)
            SetValue(IsJumpListFrequentProperty, value)
        End Set
    End Property

    Public Shared IsJumplistRecentlyPlayedProperty As DependencyProperty = DependencyProperty.Register("IsJumplistRecentlyPlayed", GetType(Boolean), GetType(MainWindow), New PropertyMetadata(SharedProperties.Instance.JumpList_RecentlyPlayed, New PropertyChangedCallback(Sub(s, e)
                                                                                                                                                                                                                                                                                  SharedProperties.Instance.JumpList_RecentlyPlayed = e.NewValue
                                                                                                                                                                                                                                                                              End Sub)))
    Property IsJumplistRecentlyPlayed As Boolean
        Get
            Return GetValue(IsJumplistRecentlyPlayedProperty)
        End Get
        Set(value As Boolean)
            SetValue(IsJumplistRecentlyPlayedProperty, value)
        End Set
    End Property

    Public Shared IsJumplistTasksProperty As DependencyProperty = DependencyProperty.Register("IsJumplistTasks", GetType(Boolean), GetType(MainWindow), New PropertyMetadata(SharedProperties.Instance.JumpList_ShowTasks, New PropertyChangedCallback(Sub(s, e)
                                                                                                                                                                                                                                                           SharedProperties.Instance.JumpList_ShowTasks = e.NewValue
                                                                                                                                                                                                                                                           Dim JTaskPlayAll As System.Windows.Shell.JumpTask = Nothing
                                                                                                                                                                                                                                                           Dim JTaskShuffleAll As System.Windows.Shell.JumpTask = Nothing
                                                                                                                                                                                                                                                           If SharedProperties.Instance.JumpList_ShowTasks Then
                                                                                                                                                                                                                                                               JTaskPlayAll = New System.Windows.Shell.JumpTask() With {.Title = "Play All", .Description = "Play All Your Songs", .CustomCategory = "Tasks", .ApplicationPath = IO.Path.Combine(My.Application.Info.DirectoryPath, "QuickBeat.exe"), .Arguments = "PlayAll"}
                                                                                                                                                                                                                                                               JTaskShuffleAll = New System.Windows.Shell.JumpTask() With {.Title = "Shuffle All", .Description = "Shuffle All Your Songs", .CustomCategory = "Tasks", .ApplicationPath = IO.Path.Combine(My.Application.Info.DirectoryPath, "QuickBeat.exe"), .Arguments = "ShuffleAll"}
                                                                                                                                                                                                                                                           End If
                                                                                                                                                                                                                                                           Dim JList As New System.Windows.Shell.JumpList(If(SharedProperties.Instance.JumpList_ShowTasks, {JTaskPlayAll, JTaskShuffleAll}, {}), SharedProperties.Instance.JumpList_ShowFrequent, True)
                                                                                                                                                                                                                                                           System.Windows.Shell.JumpList.SetJumpList(Application.Current, JList)
                                                                                                                                                                                                                                                       End Sub)))
    Property IsJumplistTasks As Boolean
        Get
            Return GetValue(IsJumplistTasksProperty)
        End Get
        Set(value As Boolean)
            SetValue(IsJumplistTasksProperty, value)
        End Set
    End Property

    Public Shared JumplistMediaLinkProperty As DependencyProperty = DependencyProperty.Register("JumplistMediaLink", GetType(Integer), GetType(MainWindow), New PropertyMetadata(SharedProperties.Instance.JumpList_MediaLink, New PropertyChangedCallback(Sub(s, e)
                                                                                                                                                                                                                                                               SharedProperties.Instance.JumpList_MediaLink = e.NewValue
                                                                                                                                                                                                                                                           End Sub)))
    Property JumplistMediaLink As Integer
        Get
            Return GetValue(JumplistMediaLinkProperty)
        End Get
        Set(value As Integer)
            SetValue(JumplistMediaLinkProperty, value)
        End Set
    End Property

    Public Shared ProxyMediaLinkProperty As DependencyProperty = DependencyProperty.Register("ProxyMediaLink", GetType(Integer), GetType(MainWindow))
    Property ProxyMediaLink As Integer
        Get
            Return GetValue(ProxyMediaLinkProperty)
        End Get
        Set(value As Integer)
            SetValue(ProxyMediaLinkProperty, value)
        End Set
    End Property

    Public Shared ScanLibraryAtStartupProperty As DependencyProperty = DependencyProperty.Register("ScanLibraryAtStartup", GetType(Boolean), GetType(MainWindow), New PropertyMetadata(SharedProperties.Instance.ScanLibraryAtStartup, New PropertyChangedCallback(Sub(s, e)
                                                                                                                                                                                                                                                                       SharedProperties.Instance.ScanLibraryAtStartup = e.NewValue
                                                                                                                                                                                                                                                                   End Sub)))
    Property ScanLibraryAtStartup As Boolean
        Get
            Return GetValue(ScanLibraryAtStartupProperty)
        End Get
        Set(value As Boolean)
            SetValue(ScanLibraryAtStartupProperty, value)
        End Set
    End Property

    Public Shared SubtitlesPathProperty As DependencyProperty = DependencyProperty.Register("SubtitlesPath", GetType(String), GetType(MainWindow), New UIPropertyMetadata(New PropertyChangedCallback(Sub(d, e)
                                                                                                                                                                                                          If d IsNot Nothing AndAlso TypeOf d Is MainWindow Then
                                                                                                                                                                                                              If TryCast(d, MainWindow).IsUsingSubtitles Then
                                                                                                                                                                                                                  With TryCast(d, MainWindow)
                                                                                                                                                                                                                      .IsUsingSubtitles = False
                                                                                                                                                                                                                      .Subtitles?.Clear()
                                                                                                                                                                                                                      .CurrentSubtitle = Nothing
                                                                                                                                                                                                                      For Each item In ._SubtilesPositionSyncProcs
                                                                                                                                                                                                                          Un4seen.Bass.Bass.BASS_ChannelRemoveSync(item.Item1, item.Item2)
                                                                                                                                                                                                                      Next
                                                                                                                                                                                                                      ._SubtilesPositionSyncProcs.Clear()
                                                                                                                                                                                                                  End With
                                                                                                                                                                                                              End If
                                                                                                                                                                                                              If IO.File.Exists(e.NewValue) Then
                                                                                                                                                                                                                  Dim subP As New SubtitlesParser.Classes.Parsers.SubParser
                                                                                                                                                                                                                  Using fs As New IO.FileStream(e.NewValue, IO.FileMode.Open, IO.FileAccess.Read, IO.FileShare.Read)
                                                                                                                                                                                                                      d?.SetValue(MainWindow.SubtitlesProperty, New ObjectModel.ObservableCollection(Of SubtitlesParser.Classes.SubtitleItem))
                                                                                                                                                                                                                      For Each item In subP.ParseStream(e.NewValue, fs)
                                                                                                                                                                                                                          TryCast(d, MainWindow).Subtitles?.Add(item)
                                                                                                                                                                                                                          Dim PosSyncProc As New Un4seen.Bass.SYNCPROC(Sub(h, c, data, u)
                                                                                                                                                                                                                                                                           Application.Current.Dispatcher.Invoke(Sub()
                                                                                                                                                                                                                                                                                                                     Dim oldValue = TryCast(d?.GetValue(CurrentSubtitleProperty), SubtitleItem)
                                                                                                                                                                                                                                                                                                                     TryCast(d, MainWindow).CurrentSubtitle = item
                                                                                                                                                                                                                                                                                                                     item.IsCurrent = True
                                                                                                                                                                                                                                                                                                                     TryCast(d, MainWindow)?.OnCurrentSubtitleChanged(oldValue?.JoinedLines, item.JoinedLines, item)
                                                                                                                                                                                                                                                                                                                 End Sub)
                                                                                                                                                                                                                                                                       End Sub)
                                                                                                                                                                                                                          Dim stream = SharedProperties.Instance.Player?.Stream
                                                                                                                                                                                                                          Dim PosSyncProcHandle = Un4seen.Bass.Bass.BASS_ChannelSetSync(stream, Un4seen.Bass.BASSSync.BASS_SYNC_POS, Un4seen.Bass.Bass.BASS_ChannelSeconds2Bytes(stream, TimeSpan.FromMilliseconds(item.StartTime).TotalSeconds), PosSyncProc, IntPtr.Zero)
                                                                                                                                                                                                                          TryCast(d, MainWindow)._SubtilesPositionSyncProcs.Add(New Tuple(Of Integer, Integer, Un4seen.Bass.SYNCPROC)(stream, PosSyncProcHandle, PosSyncProc))
                                                                                                                                                                                                                      Next
                                                                                                                                                                                                                      d?.SetValue(MainWindow.IsUsingSubtitlesProperty, True)
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
                                                                                                                                                                                                                   d?.SetValue(MainWindow.SubtitlesPathProperty, Nothing)
                                                                                                                                                                                                                   'd?.SetValue(MainWindow.SubtitlesProperty, Nothing)
                                                                                                                                                                                                                   TryCast(d?.GetValue(MainWindow.SubtitlesProperty), ObjectModel.ObservableCollection(Of SubtitlesParser.Classes.SubtitleItem))?.Clear()
                                                                                                                                                                                                                   d?.SetValue(MainWindow.CurrentSubtitleProperty, Nothing)
                                                                                                                                                                                                                   For Each item In TryCast(d, MainWindow)._SubtilesPositionSyncProcs
                                                                                                                                                                                                                       Un4seen.Bass.Bass.BASS_ChannelRemoveSync(item.Item1, item.Item2)
                                                                                                                                                                                                                   Next
                                                                                                                                                                                                                   TryCast(d, MainWindow)._SubtilesPositionSyncProcs.Clear()
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

    Public Shared SelectedUPnPItemProperty As DependencyProperty = DependencyProperty.Register("SelectedUPnPItem", GetType(UPnP.UPnPItem), GetType(MainWindow))
    Property SelectedUPnPItem As UPnP.UPnPItem
        Get
            Return GetValue(SelectedUPnPItemProperty)
        End Get
        Set(value As UPnP.UPnPItem)
            SetValue(SelectedUPnPItemProperty, value)
        End Set
    End Property

    Public Shared WebViewBlockPopupsProperty As DependencyProperty = DependencyProperty.Register("WebViewBlockPopups", GetType(Boolean), GetType(MainWindow), New PropertyMetadata(True))
    Property WebViewBlockPopups As Boolean
        Get
            Return GetValue(WebViewBlockPopupsProperty)
        End Get
        Set(value As Boolean)
            SetValue(WebViewBlockPopupsProperty, value)
        End Set
    End Property

    Public Shared WebViewBlockIFramesProperty As DependencyProperty = DependencyProperty.Register("WebViewBlockIFrames", GetType(Boolean), GetType(MainWindow))
    Property WebViewBlockIFrames As Boolean
        Get
            Return GetValue(WebViewBlockIFramesProperty)
        End Get
        Set(value As Boolean)
            SetValue(WebViewBlockIFramesProperty, value)
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
            Case 5
                RadioButton_MainTabControl_MetadataGroup.IsChecked = True
            Case 7
                RadioButton_MainTabControl_UPnP.IsChecked = True
        End Select
        If e.Parameter = TabControl_Main.Items.Count - 3 Then
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
        SharedProperties.Instance.Player.PauseVideoEffects()
    End Sub

    Private Sub HyperLink_About_Icon_RequestNavigate(sender As Object, e As RequestNavigateEventArgs)
        Process.Start(e.Uri.AbsoluteUri)
    End Sub

    Private Sub SortLibrary_CanExecute(sender As Object, e As CanExecuteRoutedEventArgs)
        e.CanExecute = e.Parameter.ToString.ToLower = "none" OrElse e.Parameter.ToString.ToLower = "title" OrElse e.Parameter.ToString.ToLower = "artist" OrElse e.Parameter.ToString.ToLower = "album" OrElse e.Parameter.ToString.ToLower = "custom"
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
            Case "custom"
                Dim Filter = Dialogs.InputBox.ShowSingle(Me, Utilities.ResourceResolver.Strings.SORT, Utilities.ResourceResolver.Strings.DESCRIPTION, footer:="All Metadata Tags are Supported, Input is Case Sensitive")
                ListBox_Library.Items.SortDescriptions.Clear()
                ListBox_Library.Items.SortDescriptions.Add(New SortDescription(Filter, ListSortDirection.Ascending))
        End Select
    End Sub

    Private Sub OpenInExplorer_CanExecute(sender As Object, e As CanExecuteRoutedEventArgs)
        e.CanExecute = True
    End Sub

    Private Sub OpenInExplorer_Executed(sender As Object, e As ExecutedRoutedEventArgs)
        If IO.File.Exists(e.Parameter.ToString) Then
            Process.Start("explorer.exe", "/select," & e.Parameter)
        Else
            Process.Start(e.Parameter.ToString)
        End If
    End Sub

    Private Sub PickPlayerVideoEffect_CanExecute(sender As Object, e As CanExecuteRoutedEventArgs)
        e.CanExecute = Not IsNothing(SharedProperties.Instance?.Player)
    End Sub

    Private Sub PickPlayerVideoEffect_Executed(sender As Object, e As ExecutedRoutedEventArgs)
        Dim vep As New Dialogs.VideoEffectPicker
        If vep.ShowDialog Then
            Dim vfx As Player.VFX.VideoEffects.VideoEffect = Activator.CreateInstance(vep.DialogVideoEffectResult)
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
            Image_NowPlaying_Cover.Visibility = Visibility.Collapsed
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
        Dim ofd As New Microsoft.Win32.OpenFileDialog With {.Title = "Subtitles", .Filter = SubtitlesParser.Classes.SubtitlesFormat.SupportedSubtitlesFormatsFileFilter}
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
            Image_NowPlaying_Cover.Visibility = Visibility.Visible
        End If
    End Sub

    Private Sub Commands_ShowControlPopup_CanExecute(sender As Object, e As CanExecuteRoutedEventArgs)
        e.CanExecute = If(SharedProperties.Instance.ActiveControlPopup Is Nothing, True, Not SharedProperties.Instance.ActiveControlPopup.IsShown)
    End Sub

    Private Sub Commands_ShowControlPopup_Executed(sender As Object, e As ExecutedRoutedEventArgs)
        If SharedProperties.Instance.ActiveControlPopup Is Nothing Then
            SharedProperties.Instance.ActiveControlPopup = New UI.Windows.ActiveControlPopup With {.AutoClose = False, .Timeout = 4000}
        End If
        SharedProperties.Instance.ActiveControlPopup?.RevealCollapse()
    End Sub

    Private Sub Commands_WriteSYLTToCurrentFile_CanExecute(sender As Object, e As CanExecuteRoutedEventArgs)
        e.CanExecute = IO.File.Exists(SharedProperties.Instance.Player.Path) AndAlso Not Commands_WriteSYLTToCurrentFile_IsBusy
    End Sub

    Private Async Sub Commands_WriteSYLTToCurrentFile_Executed(sender As Object, e As ExecutedRoutedEventArgs)
        Commands_WriteSYLTToCurrentFile_IsBusy = True
        Dim ofd As New Microsoft.Win32.OpenFileDialog With {.Title = "Subtitles", .Filter = SubtitlesParser.Classes.SubtitlesFormat.SupportedSubtitlesFormatsFileFilter}
        If ofd.ShowDialog Then
            Await WriteSYLTToCurrentFile(IO.File.ReadAllText(ofd.FileName))
        End If
        Commands_WriteSYLTToCurrentFile_IsBusy = False
    End Sub

    Private Sub Commands_SettingsScrollTo_CanExecute(sender As Object, e As CanExecuteRoutedEventArgs)
        e.CanExecute = True
    End Sub

    Private Sub Commands_SettingsScrollTo_Executed(sender As Object, e As ExecutedRoutedEventArgs)
        'fix offset
        Select Case e.Parameter?.ToString.ToLower
            Case "top", "0"
                ScrollViewer_Settings.ScrollToTop()
            Case "bottom"
                ScrollViewer_Settings.ScrollToBottom()
            Case "windows", "win"
                StackPanel_Title_Windows.BringIntoView()
            Case "player"
                StackPanel_Title_Player.BringIntoView()
            Case "network", "lan"
                StackPanel_Title_Network.BringIntoView()
            Case "library", "cache"
                StackPanel_Title_Library.BringIntoView()
            Case "keyboard", "hotkeys"
                StackPanel_Title_Keyboard.BringIntoView()
        End Select
    End Sub

    Private Sub Commands_ParseSyncedLyricsFromUnsyncedLyrics_CanExecute(sender As Object, e As CanExecuteRoutedEventArgs)
        e.CanExecute = e.Parameter IsNot Nothing
    End Sub

    Private Sub Commands_ParseSyncedLyricsFromUnsyncedLyrics_Executed(sender As Object, e As ExecutedRoutedEventArgs)
        Dim ib As New Dialogs.InputBox With {.Owner = Me, .Description = ""} 'select parsing method
        ib.AddComboBox("Method", New Object() {"[MM:SS]Text", "Internal Subtitles Parser"}, DefaultInput:=0)
        If ib.ShowDialog Then
            If IsUsingSubtitles Then
                IsUsingSubtitles = False
                Subtitles?.Clear()
                CurrentSubtitle = Nothing
                For Each item In _SubtilesPositionSyncProcs
                    Un4seen.Bass.Bass.BASS_ChannelRemoveSync(item.Item1, item.Item2)
                Next
                _SubtilesPositionSyncProcs.Clear()
            End If
            Select Case CInt(ib.Value("Method"))
                Case 0
                    SetValue(MainWindow.SubtitlesProperty, New ObjectModel.ObservableCollection(Of SubtitlesParser.Classes.SubtitleItem))
                    Dim i = 0
                    For Each line In CStr(e.Parameter).Split(New String() {vbCr, vbLf, vbCrLf}, StringSplitOptions.RemoveEmptyEntries)
                        Dim sTime = line.Substring(1, line.IndexOf("]") - 1).Split(":")
                        Dim pText = line.Substring(line.IndexOf("]") + 1)
                        Dim item As New SubtitleItem() With {.StartTime = (CDbl(sTime(0)) * 60 + CDbl(sTime(1))) * 1000, .Index = i}
                        item.EndTime = item.StartTime + 1000 'for compatibility reasons
                        item.Lines.Add(pText) : item.PlaintextLines.Add(pText)
                        Subtitles?.Add(item)
                        Dim PosSyncProc As New Un4seen.Bass.SYNCPROC(Sub(h, c, data, u)
                                                                         Application.Current.Dispatcher.Invoke(Sub()
                                                                                                                   Dim oldValue = TryCast(GetValue(CurrentSubtitleProperty), SubtitleItem)
                                                                                                                   CurrentSubtitle = item
                                                                                                                   item.IsCurrent = True
                                                                                                                   OnCurrentSubtitleChanged(oldValue?.JoinedLines, item.JoinedLines, item)
                                                                                                               End Sub)
                                                                     End Sub)
                        Dim stream = SharedProperties.Instance.Player?.Stream
                        Dim PosSyncProcHandle = Un4seen.Bass.Bass.BASS_ChannelSetSync(stream, Un4seen.Bass.BASSSync.BASS_SYNC_POS, Un4seen.Bass.Bass.BASS_ChannelSeconds2Bytes(stream, TimeSpan.FromMilliseconds(item.StartTime).TotalSeconds), PosSyncProc, IntPtr.Zero)
                        _SubtilesPositionSyncProcs.Add(New Tuple(Of Integer, Integer, Un4seen.Bass.SYNCPROC)(stream, PosSyncProcHandle, PosSyncProc))
                        i += 1
                    Next
                    SetValue(MainWindow.IsUsingSubtitlesProperty, True)
                Case 1
                    Dim subP As New SubtitlesParser.Classes.Parsers.SubParser
                    Dim subStream As New IO.MemoryStream
                    Using sw As New IO.StreamWriter(subStream)
                        sw.Write(CStr(e.Parameter))
                        SetValue(MainWindow.SubtitlesProperty, New ObjectModel.ObservableCollection(Of SubtitlesParser.Classes.SubtitleItem))
                        For Each item In subP.ParseStream(subStream)
                            Subtitles?.Add(item)
                            Dim PosSyncProc As New Un4seen.Bass.SYNCPROC(Sub(h, c, data, u)
                                                                             Application.Current.Dispatcher.Invoke(Sub()
                                                                                                                       Dim oldValue = TryCast(GetValue(CurrentSubtitleProperty), SubtitleItem)
                                                                                                                       CurrentSubtitle = item
                                                                                                                       item.IsCurrent = True
                                                                                                                       OnCurrentSubtitleChanged(oldValue?.JoinedLines, item.JoinedLines, item)
                                                                                                                   End Sub)
                                                                         End Sub)
                            Dim stream = SharedProperties.Instance.Player?.Stream
                            Dim PosSyncProcHandle = Un4seen.Bass.Bass.BASS_ChannelSetSync(stream, Un4seen.Bass.BASSSync.BASS_SYNC_POS, Un4seen.Bass.Bass.BASS_ChannelSeconds2Bytes(stream, TimeSpan.FromMilliseconds(item.StartTime).TotalSeconds), PosSyncProc, IntPtr.Zero)
                            _SubtilesPositionSyncProcs.Add(New Tuple(Of Integer, Integer, Un4seen.Bass.SYNCPROC)(stream, PosSyncProcHandle, PosSyncProc))
                        Next
                        SetValue(MainWindow.IsUsingSubtitlesProperty, True)
                        sw.Close()
                    End Using
                    subP = Nothing
            End Select
        End If
    End Sub
#End Region

    Private Sub NotifyIcon_Main_MouseDoubleClick(sender As Object, e As RoutedEventArgs)
        Show()
        SharedProperties.Instance.Player.ResumeVideoEffects()
    End Sub

    Private Sub MainWindow_Initialized(sender As Object, e As EventArgs) Handles Me.Initialized
        Utilities.SharedProperties.Instance.ShowNotification = New Utilities.SharedProperties.ShowNotificationDelegate(Sub(t, m, i)
                                                                                                                           Dispatcher.Invoke(Sub()
                                                                                                                                                 NotifyIcon_Main.ShowBalloonTip(t, m, i)
                                                                                                                                             End Sub)
                                                                                                                       End Sub)
        If SharedProperties.Instance.Player IsNot Nothing Then
            AddHandler SharedProperties.Instance.Player.MediaLoaded, Sub(o, n)
                                                                         Application.Current.Dispatcher.Invoke(Sub()
                                                                                                                   IsUsingSubtitles = True
                                                                                                                   IsUsingSubtitles = False
                                                                                                               End Sub)
                                                                         Dim tag = SharedProperties.Instance.RequestTags(SharedProperties.Instance.Player.Path, TagLib.ReadStyle.None)
                                                                         Dim tagv2 = TryCast(tag?.GetTag(TagLib.TagTypes.Id3v2), TagLib.Id3v2.Tag)
                                                                         If tagv2 IsNot Nothing Then
                                                                             Dim frames = tagv2.GetFrames(Of TagLib.Id3v2.SynchronisedLyricsFrame)
                                                                             If frames.Count > 0 Then 'found SYLT frames
                                                                                 Dim i = 0
                                                                                 Application.Current.Dispatcher.Invoke(Sub()
                                                                                                                           For Each syncText In frames.FirstOrDefault.Text
                                                                                                                               Dim item As New SubtitleItem With {.StartTime = syncText.Time, .Index = 0}
                                                                                                                               item.Lines.Add(syncText.Text) : item.PlaintextLines.Add(syncText.Text)
                                                                                                                               Subtitles?.Add(item)
                                                                                                                               Dim PosSyncProc As New Un4seen.Bass.SYNCPROC(Sub(h, c, data, u)
                                                                                                                                                                                Application.Current.Dispatcher.Invoke(Sub()
                                                                                                                                                                                                                          Dim oldValue = TryCast(GetValue(CurrentSubtitleProperty), SubtitleItem)
                                                                                                                                                                                                                          CurrentSubtitle = item
                                                                                                                                                                                                                          item.IsCurrent = True
                                                                                                                                                                                                                          OnCurrentSubtitleChanged(oldValue?.JoinedLines, item.JoinedLines, item)
                                                                                                                                                                                                                      End Sub)
                                                                                                                                                                            End Sub)
                                                                                                                               Dim stream = SharedProperties.Instance.Player?.Stream
                                                                                                                               Dim PosSyncProcHandle = Un4seen.Bass.Bass.BASS_ChannelSetSync(stream, Un4seen.Bass.BASSSync.BASS_SYNC_POS, Un4seen.Bass.Bass.BASS_ChannelSeconds2Bytes(stream, syncText.Time / 1000), PosSyncProc, IntPtr.Zero)
                                                                                                                               _SubtilesPositionSyncProcs.Add(New Tuple(Of Integer, Integer, Un4seen.Bass.SYNCPROC)(stream, PosSyncProcHandle, PosSyncProc))
                                                                                                                               i += 1
                                                                                                                           Next
                                                                                                                           IsUsingSubtitles = True
                                                                                                                       End Sub)
                                                                             End If
                                                                         End If
                                                                     End Sub
        End If
        Dim TabTimer As New Timers.Timer With {.Interval = 1000}
        AddHandler TabTimer.Elapsed, New Timers.ElapsedEventHandler(Sub(s, _e)
                                                                        Try
                                                                            Me.Dispatcher.Invoke(Sub()
                                                                                                     If TabControl_Main.SelectedIndex <> 0 OrElse Me.WindowState = WindowState.Minimized Then
                                                                                                         Return
                                                                                                     End If
                                                                                                     If HomeTabSwitchTicksLeft <= 0 Then 'Just in case ;)
                                                                                                         Select Case HomeSubTabSelectedIndex 'TODO Change those when you change home screen tabs
                                                                                                             Case 0 'Most Played Track
                                                                                                                 HomeSubTabSelectedIndex = 1
                                                                                                             Case 1 'MP Artist
                                                                                                                 If TryCast(TabControl_Home_Sub.Items.Item(2), TabItem)?.Visibility = Visibility.Visible Then
                                                                                                                     HomeSubTabSelectedIndex = 2
                                                                                                                 ElseIf TryCast(TabControl_Home_Sub.Items.Item(3), TabItem)?.Visibility = Visibility.Visible Then
                                                                                                                     HomeSubTabSelectedIndex = 3
                                                                                                                 ElseIf TryCast(TabControl_Home_Sub.Items.Item(4), TabItem)?.Visibility = Visibility.Visible Then
                                                                                                                     HomeSubTabSelectedIndex = 4
                                                                                                                 ElseIf TryCast(TabControl_Home_Sub.Items.Item(5), TabItem)?.Visibility = Visibility.Visible Then
                                                                                                                     HomeSubTabSelectedIndex = 5
                                                                                                                 End If
                                                                                                             Case 2 'Deezer MP Artist                                                                                                                 
                                                                                                                 If TryCast(TabControl_Home_Sub.Items.Item(3), TabItem)?.Visibility = Visibility.Visible Then
                                                                                                                     HomeSubTabSelectedIndex = 3
                                                                                                                 ElseIf TryCast(TabControl_Home_Sub.Items.Item(4), TabItem)?.Visibility = Visibility.Visible Then
                                                                                                                     HomeSubTabSelectedIndex = 4
                                                                                                                 Else
                                                                                                                     HomeSubTabSelectedIndex = 5
                                                                                                                 End If
                                                                                                             Case 3 'Deezer MP Related Artist                                                                                                                 
                                                                                                                 If TryCast(TabControl_Home_Sub.Items.Item(4), TabItem)?.Visibility = Visibility.Visible Then
                                                                                                                     HomeSubTabSelectedIndex = 4
                                                                                                                 Else
                                                                                                                     HomeSubTabSelectedIndex = 5
                                                                                                                 End If
                                                                                                             Case 4 'Deezer Random Track
                                                                                                                 HomeSubTabSelectedIndex = 5
                                                                                                             Case 5 'Random
                                                                                                                 HomeSubTabSelectedIndex = 0
                                                                                                         End Select
                                                                                                         'HomeSubTabSelectedIndex = If(HomeSubTabSelectedIndex = 0, 1, 0) '0, 1, 0
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
        AquaVersion = SharedProperties.Instance.Aqua.Version

        If My.Settings.APP_FIRSTRUN Then
            Dim melo As New Player.InternalMetadata("Resources/Neffex - Fight Back.mp3") With {.PlayCount = 1000000}
            melo.RefreshTagsFromFile()
#Disable Warning
            SharedProperties.Instance.Player.LoadSong(melo)
#Enable Warning
            My.Settings.APP_FIRSTRUN = False
            My.Settings.Save()
        ElseIf String.IsNullOrEmpty(SharedProperties.Instance.Player.Path) Then
            Dim melo As New Player.InternalMetadata("Resources/Neffex - Fight Back.mp3") With {.PlayCount = 2000000}
            melo.RefreshTagsFromFile()
#Disable Warning
            SharedProperties.Instance.Player.LoadSong(melo)
#Enable Warning
        End If

        If Not String.IsNullOrEmpty(My.Settings.AQUA_STARTUP_SCRIPT) Then
            Try
                SharedProperties.Instance.Aqua.RunFile(My.Settings.AQUA_STARTUP_SCRIPT)
            Catch ex As Exception
                Utilities.DebugMode.Instance.Log(Of MainWindow)(ex.ToString)
            End Try
        End If
    End Sub

    Private Sub MenuItem_KeyboardNavigation_Click(sender As Object, e As RoutedEventArgs)
        HandyControl.Controls.MessageBox.Show(Me, Utilities.ResourceResolver.Strings.HINT_KEYBOARD_NAVIGATION, ResourceResolver.Strings.APPNAME, icon:=MessageBoxImage.Information)
    End Sub

    Private Sub MainWindow_PreviewKeyDown(sender As Object, e As KeyEventArgs) Handles Me.PreviewKeyDown
        If TypeOf e.OriginalSource Is TextBox OrElse TypeOf e.OriginalSource Is HandyControl.Controls.SearchBar Then
            Return
        End If
        Dim mods = Keyboard.Modifiers
        Select Case e.Key
            'Case Key.A
            '    If mods = ModifierKeys.Control Then
            'TryCast(Me.TryFindResource("Commands.SetTabIndexCommand"), RoutedUICommand).Execute(8, Me)
            'LoadMelody()
            'Dim melo As New Player.InternalMetadata() With {.Path = "Resources/Neffex - Fight Back.mp3", .Provider = New Player.InternalMediaProvider("Resources/Neffex - Fight Back.mp3"), .PlayCount = 1000000}
            'melo.RefreshTagsFromFile()
            'SharedProperties.Instance.Player.LoadSong(melo)
            'End If
            Case Key.D1
                If mods = ModifierKeys.Control Then
                    TryCast(Me.TryFindResource("Commands.SetTabIndexCommand"), RoutedUICommand)?.Execute(0, TabControl_Main)
                End If
            Case Key.D2
                If mods = ModifierKeys.Control Then
                    TryCast(Me.TryFindResource("Commands.SetTabIndexCommand"), RoutedUICommand)?.Execute(1, TabControl_Main)
                End If
            Case Key.D3
                If mods = ModifierKeys.Control Then
                    TryCast(Me.TryFindResource("Commands.SetTabIndexCommand"), RoutedUICommand)?.Execute(2, TabControl_Main)
                End If
            Case Key.D4
                If mods = ModifierKeys.Control Then
                    TryCast(Me.TryFindResource("Commands.SetTabIndexCommand"), RoutedUICommand)?.Execute(3, TabControl_Main)
                End If
            Case Key.D5
                If mods = ModifierKeys.Control Then
                    TryCast(Me.TryFindResource("Commands.SetTabIndexCommand"), RoutedUICommand)?.Execute(4, TabControl_Main)
                End If
            Case Key.D6
                If mods = ModifierKeys.Control Then
                    TryCast(Me.TryFindResource("Commands.SetTabIndexCommand"), RoutedUICommand)?.Execute(5, TabControl_Main)
                End If
            Case Key.D7
                If mods = ModifierKeys.Control Then
                    TryCast(Me.TryFindResource("Commands.SetTabIndexCommand"), RoutedUICommand)?.Execute(6, TabControl_Main)
                End If
            Case Key.D8
                If mods = ModifierKeys.Control Then
                    TryCast(Me.TryFindResource("Commands.SetTabIndexCommand"), RoutedUICommand)?.Execute(7, TabControl_Main)
                End If
            Case Key.F11
                Me.IsFullScreen = Not Me.IsFullScreen
            Case Key.D
                If mods = ModifierKeys.Control Then
                    If DebugMode.Instance.IsEnabled Then My.Windows.DeveloperConsole.Show()
                End If
            Case Key.Space
                If mods = ModifierKeys.Control Then
                    SharedProperties.Instance.Player.PlayPauseCommand.Execute(SharedProperties.Instance.Player.Stream)
                    e.Handled = True
                End If
            Case Key.Left
                If mods = ModifierKeys.Control Then
                    SharedProperties.Instance.Player.PreviousCommand.Execute(SharedProperties.Instance.Player.Playlist)
                    e.Handled = True
                ElseIf mods = ModifierKeys.Shift Then
                    SharedProperties.Instance.Player.Position -= 10
                    e.Handled = True
                End If
            Case Key.Right
                If mods = ModifierKeys.Control Then
                    SharedProperties.Instance.Player.NextCommand.Execute(SharedProperties.Instance.Player.Playlist)
                    e.Handled = True
                ElseIf mods = ModifierKeys.Shift Then
                    SharedProperties.Instance.Player.Position += 10
                    e.Handled = True
                End If
            Case Key.Up
                If mods = ModifierKeys.Control Then
                    SharedProperties.Instance.Player.Volume += 2
                    e.Handled = True
                End If
            Case Key.Down
                If mods = ModifierKeys.Control Then
                    SharedProperties.Instance.Player.Volume -= 2
                    e.Handled = True
                End If
            Case Key.S
                If mods = ModifierKeys.Control Then
                    SharedProperties.Instance.Player.Playlist.IsShuffling = Not SharedProperties.Instance.Player.Playlist.IsShuffling
                    e.Handled = True
                End If
            Case Key.R
                If mods = ModifierKeys.Control Then
                    SharedProperties.Instance.Player.IsLooping = Not SharedProperties.Instance.Player.IsLooping
                    e.Handled = True
                ElseIf mods = ModifierKeys.Shift Then
                    SharedProperties.Instance.Player.Playlist.IsLooping = Not SharedProperties.Instance.Player.Playlist.IsLooping
                    e.Handled = True
                End If
            Case Key.N
                If mods = ModifierKeys.Control Then
                    SharedProperties.Instance.Player.NewPlaylistCommand.Execute(Nothing)
                    e.Handled = True
                End If
            Case Key.F
                If mods = ModifierKeys.Control Then
                    SharedProperties.Instance.Player.StreamMetadata.IsFavorite = Not SharedProperties.Instance.Player.StreamMetadata.IsFavorite
                    e.Handled = True
                End If
        End Select
    End Sub

    Private Sub MainWindow_Closing(sender As Object, e As CancelEventArgs) Handles Me.Closing
        Select Case CType(My.Settings.APP_CLOSINGBEHAVIOUR, ClosingBehaviour)
            Case ClosingBehaviour.Minimize
                Me.WindowState = WindowState.Minimized
                e.Cancel = True
            Case ClosingBehaviour.SystemTray
                Hide()
                e.Cancel = True
        End Select

        If Not e.Cancel Then
            'SharedProperties.Instance.Player?.Pause() 'messes with Player.SaveState

            'Saving settings
            My.Settings.APP_VIEW_SIDEBAR_RIGHT_VISIBILITY = Grid_SideBar_Right.Visibility
            My.Settings.APP_VIEW_BIGCOVER = IsBigCover
            My.Settings.APP_VIEW_BOTTOMCONTROLS = IsBottomControls
            My.Settings.APP_VIEW_CONTROLS_COVERBACKGROUND = IsLiveBackground
            My.Settings.APP_PLAYER_MEDIALOADED_SHOWPOPUP = ShowPopupNotification
            My.Settings.APP_DISCORD = IsDiscord
            My.Settings.APP_NEXTINSTANCE_ARGTYPE = ProxyMediaLink
            Dim RWVFX As Player.VFX.ReactiveWaveVideoEffect = TryCast(Me.TryFindResource("RWVFX"), Player.VFX.ReactiveWaveVideoEffect)
            If RWVFX IsNot Nothing Then
                My.Settings.APP_VIEW_SEEKBAR = RWVFX.WaveType
                My.Settings.APP_VIEW_SEEKBAR_FPS = RWVFX.FramerateLimit
            End If

            My.Settings.Save()

            If Not String.IsNullOrEmpty(My.Settings.AQUA_CLOSING_SCRIPT) Then
                Try
                    SharedProperties.Instance.Aqua.RunFile(My.Settings.AQUA_CLOSING_SCRIPT)
                Catch ex As Exception
                    Utilities.DebugMode.Instance.Log(Of MainWindow)(ex.ToString)
                End Try
            End If
        End If
    End Sub

    Private Sub ComboBox_Settings_StartupBehaviour_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
        My.Settings.APP_STARTUPBEHAVIOUR = ComboBox_Settings_StartupBehaviour.SelectedIndex
    End Sub

    Private Sub ComboBox_Settings_ClosingBehaviour_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
        My.Settings.APP_CLOSINGBEHAVIOUR = ComboBox_Settings_ClosingBehaviour.SelectedIndex
    End Sub

    Private Sub MainWindow_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        AddHandler SharedProperties.Instance.Library.AlbumFocused, AddressOf LibraryFocusHandler
        AddHandler SharedProperties.Instance.Library.ArtistFocused, AddressOf LibraryFocusHandler
        AddHandler SharedProperties.Instance.Library.GroupFocused, AddressOf LibraryFocusHandler

        Grid_SideBar_Right.Visibility = My.Settings.APP_VIEW_SIDEBAR_RIGHT_VISIBILITY

        ComboBox_Settings_StartupBehaviour.SelectedIndex = My.Settings.APP_STARTUPBEHAVIOUR
        ComboBox_Settings_ClosingBehaviour.SelectedIndex = My.Settings.APP_CLOSINGBEHAVIOUR

        Dim RWVFX As Player.VFX.ReactiveWaveVideoEffect = TryCast(Me.TryFindResource("RWVFX"), Player.VFX.ReactiveWaveVideoEffect)
        If RWVFX IsNot Nothing Then
            RWVFX.WaveType = My.Settings.APP_VIEW_SEEKBAR
            RWVFX.FramerateLimit = My.Settings.APP_VIEW_SEEKBAR_FPS
        End If
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
            SharedProperties.Instance.Player.PreviewCommand.Execute(Player.Metadata.FromFile(data))
        End If
    End Sub

    Private Sub SearchBar_SearchStarted(sender As Object, e As HandyControl.Data.FunctionEventArgs(Of String))
        'TODO Not Bound atm
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
            Dim pos = e.GetPosition(Image_VisualizerOutput)
            Dim scaleX = Image_VisualizerOutput.ActualWidth / If(Image_VisualizerOutput.Source?.Width, 1)
            Dim scaleY = Image_VisualizerOutput.ActualHeight / If(Image_VisualizerOutput.Source?.Height, 1)
            SharedProperties.Instance.Player.VideoEffect.OnMouseClick(New Point(pos.X / scaleX, pos.Y / scaleY))
        End If
    End Sub

    Private Sub Thumb_Sidebar_Right_MouseDoubleClick(sender As Object, e As MouseButtonEventArgs)
        Grid_SideBar_Right.Visibility = If(Grid_SideBar_Right.Visibility = Visibility.Visible, Visibility.Collapsed, Visibility.Visible)
    End Sub

    Private Sub Button_Settings_Reset_Click(sender As Object, e As RoutedEventArgs)
        If HandyControl.Controls.MessageBox.Ask(Utilities.ResourceResolver.Strings.QUERY_SETTINGSRESET) = MessageBoxResult.OK Then
            My.Settings.Reset()
            My.Settings.APP_FIRSTRUN = False 'To prevent the app from auto upgrading at startup
            My.Settings.Save()
        End If
    End Sub

    Private Sub Button_Settings_ForceStop_Click(sender As Object, e As RoutedEventArgs)
        If HandyControl.Controls.MessageBox.Ask(Utilities.ResourceResolver.Strings.QUERY_FORCESTOP) = MessageBoxResult.OK Then
            Process.GetCurrentProcess.Kill()
        End If
    End Sub

    Private Async Sub MenuItem_CheckForUpdates_Click(sender As Object, e As RoutedEventArgs)
        'TODO make it better
        Dim helper As New Utilities.GithubUpdatesHelper
        Dim Vinfo = Await helper.GetLatestRelease("AnesHamdani08", "QuickBeat")
        If Vinfo Is Nothing Then
            HandyControl.Controls.MessageBox.Show(Me, Utilities.ResourceResolver.Strings.QUERY_UPDATES_ERROR, icon:=MessageBoxImage.Error)
        Else
            Dim v As New Version(Vinfo.TagName)
            If v.Equals(My.Application.Info.Version) Then
                HandyControl.Controls.MessageBox.Show(Me, Utilities.ResourceResolver.Strings.QUERY_UPDATES_NOTFOUND, caption:=Utilities.ResourceResolver.Strings.APPNAME, icon:=MessageBoxImage.Information)
            ElseIf v < My.Application.Info.Version Then
                HandyControl.Controls.MessageBox.Show(Me, "You Guys Are Getting Future Updates ?" & Environment.NewLine & "The Current Version is Greater than the Latest Version (○□○)", caption:=Utilities.ResourceResolver.Strings.APPNAME, icon:=MessageBoxImage.Question)
            Else
                Dim SB As New Text.StringBuilder()
                SB.AppendLine(Utilities.ResourceResolver.Strings.QUERY_UPDATES_FOUND)
                SB.AppendLine(Vinfo.TagName & If(Vinfo.Prerelease, "(Pre-Release)", ""))
                SB.AppendLine(Vinfo.Body)
                HandyControl.Controls.MessageBox.Show(Me, SB.ToString, caption:=Utilities.ResourceResolver.Strings.APPNAME, icon:=MessageBoxImage.Warning)
            End If
        End If
    End Sub

    Private Sub TreeView_UPnP_Expanded(sender As Object, e As RoutedEventArgs)
        Dim tvi = TryCast(e.OriginalSource, TreeViewItem)
        If tvi IsNot Nothing Then
            If tvi.DataContext IsNot Nothing AndAlso TypeOf tvi.DataContext Is UPnP.UPnPItem Then
                Dim DT = TryCast(tvi.DataContext, UPnP.UPnPItem)
                SelectedUPnPItem = DT
                If TypeOf DT.Tag Is UPnP.ContentDirectory.CD_Item Then
                    DT.Items.Clear()
                    Dim cdObj = TryCast(DT.Parent.GetCDObjectInformation(DT.Tag.ID), UPnP.ContentDirectory.CD_Item)
                    If cdObj IsNot Nothing Then
                        If DT.Parent IsNot Nothing AndAlso DT.Parent.SelectedRendererIndex = -1 Then
                            Dim cdObjRes = cdObj.Resource.FirstOrDefault(Function(k) UPnP.UPnPProvider.TryGetMimeTypeFromDLNAProtocole(k.protocolInfo) = "audio/mpeg")
                            If cdObjRes IsNot Nothing Then
                                Dim Meta As New Player.Metadata() With {.Location = Player.Metadata.FileLocation.Remote, .Path = cdObjRes.URI.ToString, .OriginalPath = cdObjRes.URI.ToString, .Title = cdObj.Title, .Album = cdObj.Album, .Artists = New String() {cdObj.Artist}, .Genres = New String() {cdObj.Genre}, .Length = TimeSpan.Parse(cdObjRes.duration).TotalSeconds}
#Disable Warning
                                Utilities.SharedProperties.Instance.Player.LoadSong(Meta)
#Enable Warning
                            End If
                        Else
                            If DT.Parent Is Nothing Then Return
                            DT.Parent.SelectedRenderer.SetAVUriCommand?.Execute(cdObj)
                            DT.Parent.SelectedRenderer.PlaybackControlCommand?.Execute("play")
                        End If
                    End If
                Else
                    If DT.Parent IsNot Nothing Then
                        DT.Items.Clear()
                        DT.Parent.LoadChildren(DT)
                    End If
                End If
            End If
        End If
    End Sub

    Private Sub TreeView_UPnP_Collapsed(sender As Object, e As RoutedEventArgs)
        Dim tvi = TryCast(e.OriginalSource, TreeViewItem)
        If tvi IsNot Nothing Then
            If tvi.DataContext IsNot Nothing AndAlso TypeOf tvi.DataContext Is UPnP.UPnPItem Then
                Dim DT = TryCast(tvi.DataContext, UPnP.UPnPItem)
                If DT.Parent IsNot Nothing Then
                    DT.Items.Clear()
                    DT.Items.Add(New UPnP.UPnPItem("Loading...", Utilities.CommonFunctions.GenerateCoverImage(Utilities.ResourceResolver.Images.CLOCK)))
                End If
            End If
        End If
    End Sub

    Private Sub Button_Settings_Save_Click(sender As Object, e As RoutedEventArgs)
        ''TODO Experiment with config file import export
        'Dim config = System.Configuration.ConfigurationManager.OpenExeConfiguration(Configuration.ConfigurationUserLevel.PerUserRoamingAndLocal)
        'MsgBox(config.FilePath)
        'Return
        Button_Settings_Save.IsEnabled = False
        'TODO to be changed when adding/removing settings
        'Saving settings
        My.Settings.APP_VIEW_SIDEBAR_RIGHT_VISIBILITY = Grid_SideBar_Right.Visibility
        My.Settings.APP_VIEW_BIGCOVER = IsBigCover
        My.Settings.APP_VIEW_BOTTOMCONTROLS = IsBottomControls
        My.Settings.APP_VIEW_CONTROLS_COVERBACKGROUND = IsLiveBackground
        My.Settings.APP_DISCORD = IsDiscord
        My.Settings.APP_NEXTINSTANCE_ARGTYPE = ProxyMediaLink
        My.Settings.APP_LIBRARY_STARTUPSCAN = ScanLibraryAtStartup
        My.Settings.APP_PLAYER_MEDIALOADED_SHOWPOPUP = ShowPopupNotification
        Dim RWVFX As Player.VFX.ReactiveWaveVideoEffect = TryCast(Me.TryFindResource("RWVFX"), Player.VFX.ReactiveWaveVideoEffect)
        If RWVFX IsNot Nothing Then
            My.Settings.APP_VIEW_SEEKBAR = RWVFX.WaveType
            My.Settings.APP_VIEW_SEEKBAR_FPS = RWVFX.FramerateLimit
        End If

        My.Settings.Save()

        SharedProperties.Instance.Save()

        Button_Settings_Save.IsEnabled = True
    End Sub

    Private Sub MainWindow_StateChanged(sender As Object, e As EventArgs) Handles Me.StateChanged
        If Me.WindowState = WindowState.Minimized Then
            SharedProperties.Instance.Player.PauseVideoEffects()
        Else
            SharedProperties.Instance.Player.ResumeVideoEffects()
        End If
    End Sub

    Private _LastPlaylistSearchQuery As String
    Private _LastPlaylistSearchIndex As Integer = -1
    Private Sub SearchBar_Playlist_SearchStarted(sender As Object, e As HandyControl.Data.FunctionEventArgs(Of String))
        Dim fItems = SharedProperties.Instance.Player?.Playlist?.Where(Function(k) k.TitleArtistAlbum.ToLower.Contains(e.Info.ToLower))
        If e.Info = _LastPlaylistSearchQuery Then
            If (_LastPlaylistSearchIndex + 1) < fItems.Count Then
                ListBox_Playlist.ScrollIntoView(fItems(_LastPlaylistSearchIndex + 1))
                _LastPlaylistSearchIndex += 1
                TryCast(ListBox_Playlist.ItemContainerGenerator.ContainerFromItem(fItems(_LastPlaylistSearchIndex - 1)), ListBoxItem)?.Focus()
            Else
                ListBox_Playlist.ScrollIntoView(fItems.FirstOrDefault)
                _LastPlaylistSearchIndex = 0
                TryCast(ListBox_Playlist.ItemContainerGenerator.ContainerFromItem(fItems.FirstOrDefault), ListBoxItem)?.Focus()
            End If
        Else
            _LastPlaylistSearchQuery = e.Info
            _LastPlaylistSearchIndex = 0
            ListBox_Playlist.ScrollIntoView(fItems.FirstOrDefault)
            TryCast(ListBox_Playlist.ItemContainerGenerator.ContainerFromItem(fItems.FirstOrDefault), ListBoxItem)?.Focus()
        End If
    End Sub

    Private Sub MenuItem_Firewall_Regarding_Click(sender As Object, e As RoutedEventArgs)
        HandyControl.Controls.MessageBox.Show(Me, Utilities.ResourceResolver.Strings.FIREWALL_HINT, Utilities.ResourceResolver.Strings.APPNAME, icon:=MessageBoxImage.Information)
    End Sub

#Region "WebView2"
    Private _WebViewScriptBusy As Boolean = False
    Private _EngimaCaptchaFound As Boolean = False
    Private _EnigmaPreviousTab As Integer = -1
    Private _MRE As New Threading.ManualResetEventSlim(False)
    Private _WebViewSTC As Classes.StartupItemConfiguration
    Private WithEvents _CoreWebView As CoreWebView2 = Nothing 'WebView2_WPF_Control?.CoreWebView2 'Perforamnce Reasons

    Private Sub _CoreWebView_DownloadStarting(sender As Object, e As CoreWebView2DownloadStartingEventArgs) Handles _CoreWebView.DownloadStarting
        e.Cancel = True
        DebugMode.Instance.Log(Of CoreWebView2)($"Recieved Download Request With: {e.DownloadOperation.MimeType}, URI: {e.DownloadOperation.Uri}")
        'result is in e.DownloadOperationUri
#Disable Warning
        If SubtitlesParser.Classes.SubtitlesFormat.SupportedSubtitlesFormatsSCSV.Split(";").Contains(IO.Path.GetExtension(e.ResultFilePath)) Then 'subtitle file
            WebView2_HandleRemoteLyricsFile(e.DownloadOperation.Uri) 'runs on its pace due to wv2 STA
            Return
        End If
        Dim meta = New Player.Metadata With {.Location = Player.Metadata.FileLocation.Remote, .Path = System.Web.HttpUtility.UrlDecode(e.DownloadOperation.Uri)}
        SharedProperties.Instance.Player.LoadSong(meta)
        'SharedProperties.Instance.Player.LoadAndAddCommand.Execute(meta)
#Enable Warning
        If _EnigmaPreviousTab <> -1 Then TryCast(Me.TryFindResource("Commands.SetTabIndexCommand"), RoutedUICommand).Execute(_EnigmaPreviousTab, Me)
        _CoreWebView.Navigate("about:blank")
        _WebViewScriptBusy = False
        _WebViewSTC.SetStatus("Enjoy The Song!", 100)
        _EngimaCaptchaFound = False
        _EnigmaPreviousTab = -1
    End Sub
    Private Async Sub _CoreWebView_FrameCreated(sender As Object, e As CoreWebView2FrameCreatedEventArgs) Handles _CoreWebView.FrameCreated
        WebViewRemoveAds()
        If WebViewBlockIFrames Then
            'Full Ads Removing-ish, Disabled for Now
            DebugMode.Instance.Log(Of CoreWebView2)($"Frame Created: Name:={If(e.Frame?.Name, "null")},ID:={If(e.Frame?.FrameId, "null")}, Removing frames...")
            Await _CoreWebView.ExecuteScriptAsync("var iframes = document.getElementsByTagName('iframe');")
            Await _CoreWebView.ExecuteScriptAsync("for (var i = 0; i < iframes.length; i++) {if(iframes[i].src.startsWith('https://googleads')) iframes[i].parentNode.removeChild(iframes[i]);}")
        End If
    End Sub
    Private Sub WebView2_WPF_Control_NavigationCompleted(sender As Object, e As CoreWebView2NavigationCompletedEventArgs) Handles WebView2_WPF_Control.NavigationCompleted
        DebugMode.Instance.Log(Of CoreWebView2)($"Navigation Completed: ID:={e.NavigationId}, HTTPCode:={e.HttpStatusCode}, WebCode:={e.WebErrorStatus}")
        DebugMode.Instance.Log(Of CoreWebView2)($"Navigation Completed, Attempting to Remove Ads")
        WebViewRemoveAds()
        _MRE.Set()
    End Sub
    Private Sub _CoreWebView_NewWindowRequested(sender As Object, e As CoreWebView2NewWindowRequestedEventArgs) Handles _CoreWebView.NewWindowRequested
        If WebViewBlockPopups Then
            e.Handled = True
        End If
    End Sub
    Private Sub _CoreWebView_ContextMenuRequested(sender As Object, e As CoreWebView2ContextMenuRequestedEventArgs) Handles _CoreWebView.ContextMenuRequested
        Dim mItem = _CoreWebView.Environment.CreateContextMenuItem(ResourceResolver.Strings.PARSESYNCHRONISEDLYRICS, Nothing, CoreWebView2ContextMenuItemKind.Command)
        AddHandler mItem.CustomItemSelected, New EventHandler(Of Object)(Sub(s, _e)
                                                                             Dispatcher.Invoke(Async Function()
                                                                                                   Await WebView2_HandleLyricsParsing()
                                                                                               End Function)
                                                                         End Sub)
        Dim mItem0 = _CoreWebView.Environment.CreateContextMenuItem(ResourceResolver.Strings.PARSETEXTLYRICS, Nothing, CoreWebView2ContextMenuItemKind.Command)
        AddHandler mItem0.CustomItemSelected, New EventHandler(Of Object)(Sub(s, _e)
                                                                              Dispatcher.Invoke(Async Function()
                                                                                                    Await WebView2_HandleTextLyricsParsing()
                                                                                                End Function)
                                                                          End Sub)
        e.MenuItems.Add(mItem)
        e.MenuItems.Add(mItem0)
    End Sub
    Private Async Function WebView2_HandleRemoteLyricsFile(uri As String) As Task
        If HandyControl.Controls.MessageBox.Show(Me, Utilities.ResourceResolver.Strings.QUERY_SYNCHRONISEDLYRICSDETECTED & Environment.NewLine & Utilities.ResourceResolver.Strings.QUERY_REPLACEDATA, Utilities.ResourceResolver.Strings.APPNAME, MessageBoxButton.YesNo, MessageBoxImage.Question) <> MessageBoxResult.Yes Then
            Return
        End If
        Using httpc As New Net.Http.HttpClient()
            Dim data = Await httpc.GetStringAsync(uri)
            Await WriteSYLTToCurrentFile(data)
        End Using
    End Function
    Private _WebView2_TextLyricsParsing_IsBusy As Boolean = False
    Private Async Function WebView2_HandleTextLyricsParsing() As Task
        If _WebView2_TextLyricsParsing_IsBusy Then Return
        Dim result = ExtractString(Await _CoreWebView.ExecuteScriptAsync("document.getSelection().toString()"))
        If result <> "" Then 'nothing was selected
            Dim tag = SharedProperties.Instance.RequestTags(SharedProperties.Instance.Player.Path, TagLib.ReadStyle.None)
            If Not String.IsNullOrEmpty(tag.Tag.Lyrics) Then
                If HandyControl.Controls.MessageBox.Show(Me, Utilities.ResourceResolver.Strings.QUERY_REPLACEDATA, Utilities.ResourceResolver.Strings.APPNAME, MessageBoxButton.YesNo, MessageBoxImage.Question) <> MessageBoxResult.Yes Then
                    _WebView2_TextLyricsParsing_IsBusy = False
                    Return
                End If
            End If
            Dim sResult = result.Split(New String() {"\n", "\r", "\r\n"}, StringSplitOptions.None) 'js things
            tag.Tag.Lyrics = String.Join(Environment.NewLine, sResult)
            Dim _Path = SharedProperties.Instance.Player.Path
            Dim info As New IO.FileInfo(_Path)
            Dim _WasReadOnly As Boolean
            If info.IsReadOnly Then
                If HandyControl.Controls.MessageBox.Ask(Utilities.ResourceResolver.Strings.QUERY_FILEUNLOCK) = MessageBoxResult.OK Then
                    info.IsReadOnly = False
                    _WasReadOnly = True
                Else
                    _WebView2_TextLyricsParsing_IsBusy = False
                    Return
                End If
            End If
            Dim _WasPlaying As Boolean
            Dim _Position As Double
            Dim _WasActivelyPlaying As Boolean
            If Utilities.SharedProperties.Instance.Player.Path = _Path Then
                _WasPlaying = True
                _Position = Utilities.SharedProperties.Instance.Player.Position
                _WasActivelyPlaying = Utilities.SharedProperties.Instance.Player.IsPlaying
                Utilities.SharedProperties.Instance.Player.Stop()
            End If
            Try
                tag.Save()
            Catch ex As Exception
                Utilities.DebugMode.Instance.Log(Of MainWindow)(ex.ToString)
                If _WasReadOnly Then info.IsReadOnly = True
                Dim errorCode As Integer = Runtime.InteropServices.Marshal.GetHRForException(ex) And ((1 << 16) - 1)
                If (errorCode = 32 OrElse errorCode = 33) Then
                    HandyControl.Controls.MessageBox.Error(Utilities.ResourceResolver.Strings.QUERY_FILEACCESSDENIED & Environment.NewLine & ex.Message)
                    _WebView2_TextLyricsParsing_IsBusy = False
                    Return
                End If
            End Try
            If _WasPlaying Then
                Await Utilities.SharedProperties.Instance.Player.LoadSong(Utilities.SharedProperties.Instance.Player.StreamMetadata)
                Utilities.SharedProperties.Instance.Player.Position = _Position
                If _WasActivelyPlaying Then Utilities.SharedProperties.Instance.Player.PlayCommand.Execute(Utilities.SharedProperties.Instance.Player.Stream)
            End If
            If _WasReadOnly Then
                info.IsReadOnly = True
            End If
            'Update Library
            Dim Meta = Player.Metadata.FromFile(_Path, True, True)
            If Meta Is Nothing Then
                'Check Player
                If Utilities.SharedProperties.Instance.Player.StreamMetadata?.Path = _Path Then
                    Utilities.SharedProperties.Instance.Player.StreamMetadata.Covers = Nothing
                    Utilities.SharedProperties.Instance.Player.StreamMetadata.RefreshTagsFromFile_ThreadSafe()
                    Utilities.SharedProperties.Instance.Player.StreamMetadata.EnsureCovers()
                End If
            Else
                Meta.RefreshTagsFromFile_ThreadSafe(True)
            End If
        End If
        _WebView2_TextLyricsParsing_IsBusy = False
    End Function

    Private _WebView2_LyricsParsing_IsBusy As Boolean = False
    Private Async Function WebView2_HandleLyricsParsing() As Task
        If _WebView2_LyricsParsing_IsBusy Then Return
        Dim result = ExtractString(Await _CoreWebView.ExecuteScriptAsync("document.getSelection().toString()"))
        If result <> "" Then 'nothing was selected                                                                                 
            Await WriteSYLTToCurrentFile(String.Join(Environment.NewLine, result.Split(New String() {"\n", "\r", "\r\n"}, StringSplitOptions.None))) 'js things
        End If
        _WebView2_LyricsParsing_IsBusy = False
    End Function

    Private Async Function InitializeWebView() As Task
        If _CoreWebView IsNot Nothing Then
            Return
        Else 'Is nothing
            If WebView2_WPF_Control.CoreWebView2 IsNot Nothing Then 'assign to local variable
                _CoreWebView = WebView2_WPF_Control.CoreWebView2 'this was added because of a perforamnce loss at MainWindow.ctor()
                Return
            End If
        End If
        DebugMode.Instance.Log(Of CoreWebView2)($"Initializing WebView2...")
        _WebViewSTC = New Classes.StartupItemConfiguration("Enigma")
        SharedProperties.Instance.ItemsConfiguration.Add(_WebViewSTC)
        _WebViewSTC.SetStatus("Initializing...", 0)
        Dim DataPath As String = IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "QuickBeat", GetEncodedSpecialAppName, "WebView2")
        If Not IO.Directory.Exists(DataPath) Then IO.Directory.CreateDirectory(DataPath)
        Dim core = Await CoreWebView2Environment.CreateAsync(Nothing, DataPath)
        Dim _ix = TabControl_Main.SelectedIndex
        TryCast(Me.TryFindResource("Commands.SetTabIndexCommand"), RoutedUICommand).Execute(8, Me)
        Await WebView2_WPF_Control.EnsureCoreWebView2Async(core)
        _CoreWebView = WebView2_WPF_Control.CoreWebView2
        TryCast(Me.TryFindResource("Commands.SetTabIndexCommand"), RoutedUICommand).Execute(_ix, Me)
        _WebViewSTC.SetStatus("All Good!", 100)
        DebugMode.Instance.Log(Of CoreWebView2)($"Initialized WebView2.")
    End Function
    Private Async Sub WebViewRemoveAds()
        'Minor ads removing
        DebugMode.Instance.Log(Of CoreWebView2)($"Attempting to Remove Ads...")
        Dim length = Await _CoreWebView.ExecuteScriptAsync("document.getElementsByTagName(""ins"").length")
        If length > 0 Then
            DebugMode.Instance.Log(Of CoreWebView2)($"Ads Found. Removing...")
            'remove ads            
            Await _CoreWebView.ExecuteScriptAsync("var element = document.getElementsByTagName(""ins""), index;")
            Await _CoreWebView.ExecuteScriptAsync("for (index = element.length - 1; index >= 0; index--) {    element[index].parentNode.removeChild(element[index]);}")
        Else
            DebugMode.Instance.Log(Of CoreWebView2)($"No Ads Found.")
        End If
    End Sub
    Private Async Sub SearchFreeMP3DownloadNet(query As String, Optional index As Integer = -1)
        If _WebViewScriptBusy Then Return
        DebugMode.Instance.Log(Of CoreWebView2)($"Starting Enigma...")
        _WebViewScriptBusy = True
        _WebViewSTC.SetStatus("Navigating...", 10)
        _MRE.Reset()
        DebugMode.Instance.Log(Of CoreWebView2)($"Enigma: Navigating...")
        WebViewNavigate("https://free-mp3-download.net/")
        Await Task.Run(Sub() _MRE.Wait(10000))
        DebugMode.Instance.Log(Of CoreWebView2)($"Enigma: Navigation Completed, Executing Scripts....")
        _WebViewSTC.SetStatus("Doing Magic...", 25)
        Await _CoreWebView.ExecuteScriptAsync($"document.getElementById(""q"").value = ""{query}""")
        Await _CoreWebView.ExecuteScriptAsync("document.getElementById(""snd"").click()")
        DebugMode.Instance.Log(Of CoreWebView2)($"Enigma: Searching...")
        Await Task.Delay(250) 'Some good delay
        _WebViewSTC.SetStatus("Searching...", 35)
        Dim i = 0 'Timeout
        Dim Success As Boolean = False
        Do
            Dim sLen = Await _CoreWebView.ExecuteScriptAsync("document.getElementById(""results"").getElementsByTagName(""span"").length")
            Dim tLen = Await _CoreWebView.ExecuteScriptAsync("document.getElementById(""results"").getElementsByTagName(""table"").length")
            If sLen > 0 Then
                Success = False
                Exit Do
            ElseIf tLen > 0 Then
                Success = True
                Exit Do
            End If
            Await Task.Delay(50)
            i += 1
            If i = 40 Then '2 sec passed
                Exit Do
            End If
        Loop
        DebugMode.Instance.Log(Of CoreWebView2)($"Enigma: Done Searching: Success:={Success}, TimedOut:={i = 40}")
        If i = 40 Then 'hit timeout
            _WebViewSTC.SetError(True, New TimeoutException("Timed Out"))
            _WebViewScriptBusy = False
            Return
        ElseIf Not Success Then
            _WebViewSTC.SetError(True, New ArgumentOutOfRangeException("No Results Found"))
            _WebViewScriptBusy = False
            Return
        End If
        Dim TotalResult = Await _CoreWebView.ExecuteScriptAsync("document.getElementById(""results_t"").getElementsByTagName(""tr"").length")
        DebugMode.Instance.Log(Of CoreWebView2)($"Enigma: Total Results:={TotalResult}")
        Dim Results As New List(Of String)
        For j As Integer = 0 To TotalResult - 1
            Dim result = Await _CoreWebView.ExecuteScriptAsync($"document.getElementById(""results_t"").getElementsByTagName(""tr"")[{j}].getElementsByTagName(""td"")[0].innerText")
            DebugMode.Instance.Log(Of CoreWebView2)($"Enigma: Result#{j}:={result}")
            Results.Add(ExtractString(result))
        Next
        Dim sIndex = index
        Dim ib As New Dialogs.InputBox() With {.Owner = Me, .WindowStartupLocation = WindowStartupLocation.CenterOwner}
        ib.AddComboBox("Song", Results.Select(Of Object)(Function(k) CObj(k)))
        If Not ib.ShowDialog Then
            _WebViewScriptBusy = False
            _WebViewSTC.SetStatus("All Good!", 100)
            Return
        End If
        sIndex = CInt(ib.Value("Song"))
        _MRE.Reset()
        DebugMode.Instance.Log(Of CoreWebView2)($"Enigma: Selecting Item with Index:={sIndex},Item:={Results(sIndex)}")
        DebugMode.Instance.Log(Of CoreWebView2)($"Enigma: Navigating...")
        Await _CoreWebView.ExecuteScriptAsync($"document.getElementById(""results_t"").getElementsByTagName(""tr"")[{sIndex}].getElementsByTagName(""td"")[2].getElementsByTagName(""a"")[0].getElementsByTagName(""button"")[0].click()")
        Await Task.Run(Sub() _MRE.Wait(10000))
        DebugMode.Instance.Log(Of CoreWebView2)($"Enigma: Extracting Metadata...")
        Dim CoverLink = Await _CoreWebView.ExecuteScriptAsync("document.getElementById(""cov"").src")
        If CoverLink = "null" Then
            WebViewRemoveAds()
            CoverLink = ExtractString(Await _CoreWebView.ExecuteScriptAsync("document.getElementById(""cov"").src"))
        End If
        Dim SongTitle = Await _CoreWebView.ExecuteScriptAsync("document.getElementById(""song-title"").innerText")
        If SongTitle.StartsWith("""Name: ") Then SongTitle = ExtractString(SongTitle).Remove(0, "Name: ".Length)
        Dim SongDuration = Await _CoreWebView.ExecuteScriptAsync("document.getElementById(""song-duration"").innerText")
        If SongDuration.StartsWith("""Duration: ") Then SongDuration = ExtractString(SongDuration).Remove(0, "Duration: ".Length)
        Dim SongDate = Await _CoreWebView.ExecuteScriptAsync("document.getElementById(""song-release"").innerText")
        If SongDate.StartsWith("""Release date: ") Then SongDate = ExtractString(SongDate).Remove(0, "Release date: ".Length)
        Dim PreviewLink = ExtractString(Await _CoreWebView.ExecuteScriptAsync("document.getElementsByTagName(""audio"")[0].src"))
        DebugMode.Instance.Log(Of CoreWebView2)($"Enigma: Found Metadata With: Title:={SongTitle}, Duration:={SongDuration}, Date:={SongDate}, Cover:={If(CoverLink = "ul", "N/A", "Found")}, PreviewLink:={If(PreviewLink = "ul", "N/A", "Found")}")
        Dim r = Await _CoreWebView.ExecuteScriptAsync("document.getElementById(""captcha"")") 'null if none, {} if any
        If r = "null" Then
            DebugMode.Instance.Log(Of CoreWebView2)($"Enigma: Downloading...")
            Await _CoreWebView.ExecuteScriptAsync("document.getElementsByClassName(""dl btn waves-effect waves-light blue darken-4"")[0].click()")
        Else
            DebugMode.Instance.Log(Of CoreWebView2)($"Enigma: Captcha Found, Notifying User...")
            _EngimaCaptchaFound = True
            _EnigmaPreviousTab = TabControl_Main.SelectedIndex
            TryCast(Me.TryFindResource("Commands.SetTabIndexCommand"), RoutedUICommand).Execute(8, Me)
            HandyControl.Controls.MessageBox.Show(Me, Utilities.ResourceResolver.Strings.CAPTCHA_VALIDATE, "Enigma", icon:=MessageBoxImage.Information)
        End If
        DebugMode.Instance.Log(Of CoreWebView2)($"Enigma: Done.")
        _WebViewScriptBusy = False
        _WebViewSTC.SetStatus("Enjoy The Song!", 100)
    End Sub
    Private Sub WebViewNavigate(url As String)
        DebugMode.Instance.Log(Of CoreWebView2)($"Navigating...")
        _CoreWebView?.Navigate(url)
    End Sub
    Private Function ExtractString(str As String, Optional LastOnly As Boolean = False) As String
        Return If(LastOnly, str.TrimEnd(New Char() {""""}), str.TrimStart(New Char() {""""}).TrimEnd(New Char() {""""}))
        'If str.Length < 2 OrElse (LastOnly AndAlso str.Length < 1) Then
        '    Return str
        'Else
        '    Return If(LastOnly, str.Remove(str.Length - 1, 1), str.Remove(0, 1).Remove(str.Length - 2, 1))
        'End If
    End Function
    Private Shared Function IsWebViewRuntimeInstalled() As Boolean
        Dim readValue = My.Computer.Registry.GetValue("HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Microsoft\EdgeUpdate\Clients\{F3017226-FE2A-4295-8BDF-00C3A9A7E4C5}", "pv", Nothing)
        If readValue Is Nothing Then
            'Key doesn't exist
            Return False
        Else
            'Key existed, check value
            Return True
        End If
    End Function
    Private Sub Commands_FreeMP3Download_CanExecute(sender As Object, e As CanExecuteRoutedEventArgs)
        e.CanExecute = IsWebViewInstalled AndAlso SharedProperties.Instance.IsInternetConnected AndAlso Not _WebViewScriptBusy
    End Sub
    Private Async Sub Commands_FreeMP3Download_Executed(sender As Object, e As ExecutedRoutedEventArgs)
        'TODO add artist and album meta group
        'TODO add current playing song in the player and listen to the current song via browser
        'TODO add control player from browser
        Dim Query = Dialogs.InputBox.ShowSingle(Me, Utilities.ResourceResolver.Strings.QUERY)
        If Not String.IsNullOrEmpty(Query) Then
            Await InitializeWebView()
            SearchFreeMP3DownloadNet(Query)
        End If
    End Sub
    Private Sub MenuItem_WebView2_Info_Click(sender As Object, e As RoutedEventArgs)
        HandyControl.Controls.MessageBox.Show(Me, Utilities.ResourceResolver.Strings.WEBVIEW2_HINT, Utilities.ResourceResolver.Strings.APPNAME, icon:=MessageBoxImage.Information)
    End Sub
    Private Sub Commands_WebView2Controls_CanExecute(sender As Object, e As CanExecuteRoutedEventArgs)
        If IsWebViewInstalled Then
            Select Case e.Parameter?.ToString
                Case "navigate"
                    e.CanExecute = True
                Case "reload"
                    e.CanExecute = _CoreWebView IsNot Nothing
                Case "goback"
                    e.CanExecute = WebView2_WPF_Control.CanGoBack
                Case "goforward"
                    e.CanExecute = WebView2_WPF_Control.CanGoForward
            End Select
        Else
            e.CanExecute = False
        End If
    End Sub
    Private Async Sub Commands_WebView2Controls_Executed(sender As Object, e As ExecutedRoutedEventArgs)
        Select Case e.Parameter?.ToString
            Case "navigate"
                Dim url = Dialogs.InputBox.ShowSingle(Me, "URL", "https://")
                If Not String.IsNullOrEmpty(url) Then
                    If _CoreWebView Is Nothing Then
                        Await InitializeWebView()
                    End If
                    _CoreWebView?.Navigate(url)
                    TryCast(Me.TryFindResource("Commands.SetTabIndexCommand"), RoutedUICommand).Execute(8, Me)
                End If
            Case "reload"
                WebView2_WPF_Control.Reload()
            Case "goback"
                WebView2_WPF_Control.GoBack()
            Case "goforward"
                WebView2_WPF_Control.GoForward()
        End Select
    End Sub
    Private Sub MenuItem_DumpStream_Disclaimer_Click(sender As Object, e As RoutedEventArgs)
        HandyControl.Controls.MessageBox.Show(Me, Utilities.ResourceResolver.Strings.DUMPSTREAM_DISCLAIMER, Utilities.ResourceResolver.Strings.DISCLAIMER, icon:=MessageBoxImage.Warning)
    End Sub

#End Region
    Private Async Function WriteSYLTToCurrentFile(data As String) As Task
        Dim sp As New SubtitlesParser.Classes.Parsers.SubParser()
        Try
            Dim subs As List(Of SubtitleItem)
            Using mems As New IO.MemoryStream
                Using sw As New IO.StreamWriter(mems)
                    sw.Write(data)
                    sw.Flush()
                    subs = sp.ParseStream(mems)
                End Using
            End Using
            If subs Is Nothing OrElse subs.Count = 0 Then Return 'fix this
            'request perm
            Dim tag = SharedProperties.Instance.RequestTags(SharedProperties.Instance.Player.Path, TagLib.ReadStyle.None)
            Dim tagv2 = TryCast(tag.GetTag(TagLib.TagTypes.Id3v2), TagLib.Id3v2.Tag)
            If tagv2 Is Nothing Then
                HandyControl.Controls.MessageBox.Show(Me, Utilities.ResourceResolver.Strings.QUERY_ERROR_TAGVERSION, Utilities.ResourceResolver.Strings.APPNAME, icon:=MessageBoxImage.Error)
            Else
                Dim frames = tagv2.GetFrames(Of TagLib.Id3v2.SynchronisedLyricsFrame)
                Dim cFrames = frames.Where(Function(k) k.Type = TagLib.Id3v2.SynchedTextType.Lyrics)?.Count
                Dim ib As New Dialogs.InputBox() With {.Owner = Me, .WindowStartupLocation = WindowStartupLocation.CenterOwner}
                ib.AddTextBox(Utilities.ResourceResolver.Strings.OFFSET, PlaceHolder:="in milliseconds, 1s = 1000ms")
                If cFrames.GetValueOrDefault > 0 Then
                    ib.AddComboBox(Utilities.ResourceResolver.Strings.EXISTING, New Object() {Utilities.ResourceResolver.Strings.APPEND, Utilities.ResourceResolver.Strings.CLEAR})
                End If
                ib.AddComboBox(Utilities.ResourceResolver.Strings.LANGUAGE, New Object() {"Arabic", "Chinese",
                               "English", "French", "German", "Indonesian", "Italian", "Japanese", "Korean", "Portuguese",
                               "Russian", "Spanish", "Turkish", "Vietnamese"}, DefaultInput:=2)
                If ib.ShowDialog() Then
                    If cFrames.GetValueOrDefault > 0 AndAlso CInt(ib.Value(Utilities.ResourceResolver.Strings.EXISTING)) = 1 Then
                        Dim i = 0
                        Do While i < cFrames
                            tagv2.RemoveFrame(frames(i))
                            i += 1
                        Loop
                        frames = tagv2.GetFrames(Of TagLib.Id3v2.SynchronisedLyricsFrame)
                    End If
                    Dim langcode As String
                    Select Case CInt(ib.Value(Utilities.ResourceResolver.Strings.LANGUAGE)) 'to lang code
                        Case 0
                            langcode = "ara"
                        Case 1
                            langcode = "zho"
                        Case 2
                            langcode = "eng"
                        Case 3
                            langcode = "fra"
                        Case 4
                            langcode = "deu"
                        Case 5
                            langcode = "ind"
                        Case 6
                            langcode = "ita"
                        Case 7
                            langcode = "jpn"
                        Case 8
                            langcode = "kor"
                        Case 9
                            langcode = "por"
                        Case 10
                            langcode = "rus"
                        Case 11
                            langcode = "spa"
                        Case 12
                            langcode = "tur"
                        Case 13
                            langcode = "vie"
                        Case Else
                            'hmmmmmmmm
                            langcode = "eng"
                    End Select
                    Dim aFrame = frames.FirstOrDefault(Function(k) k.Language = langcode)
                    Dim doOverwrite As Boolean = False
                    If aFrame IsNot Nothing Then
                        If HandyControl.Controls.MessageBox.Show(Me, Utilities.ResourceResolver.Strings.QUERY_ERROR_EXISTING_LANGUAGE & Environment.NewLine & Utilities.ResourceResolver.Strings.REPLACE & "?", Utilities.ResourceResolver.Strings.APPNAME, MessageBoxButton.YesNo, MessageBoxImage.Question) = MessageBoxResult.Yes Then
                            doOverwrite = True
                        Else
                            Return
                        End If
                    End If
                    Dim offset = CInt(ib.Value(Utilities.ResourceResolver.Strings.OFFSET))
                    If offset <> 0 Then
                        For Each subI In subs
                            subI.StartTime += offset
                        Next
                    End If
                    Dim sFrame As New TagLib.Id3v2.SynchronisedLyricsFrame("QuickBeat Baby! Enjoy the lyrics :)", langcode, TagLib.Id3v2.SynchedTextType.Lyrics, TagLib.StringType.UTF8) With {
                        .Format = TagLib.Id3v2.TimestampFormat.AbsoluteMilliseconds,
                        .Text = subs.Select(Of TagLib.Id3v2.SynchedText)(Function(k) New TagLib.Id3v2.SynchedText(k.StartTime, k.JoinedLines)).ToArray
                    }
                    If doOverwrite Then
                        tagv2.ReplaceFrame(aFrame, sFrame)
                    Else
                        tagv2.AddFrame(sFrame)
                    End If
                    Dim _Path = SharedProperties.Instance.Player.Path
                    Dim info As New IO.FileInfo(_Path)
                    Dim _WasReadOnly As Boolean
                    If info.IsReadOnly Then
                        If HandyControl.Controls.MessageBox.Ask(Utilities.ResourceResolver.Strings.QUERY_FILEUNLOCK) = MessageBoxResult.OK Then
                            info.IsReadOnly = False
                            _WasReadOnly = True
                        Else
                            Return
                        End If
                    End If
                    Dim _WasPlaying As Boolean
                    Dim _Position As Double
                    Dim _WasActivelyPlaying As Boolean
                    If Utilities.SharedProperties.Instance.Player.Path = _Path Then
                        _WasPlaying = True
                        _Position = Utilities.SharedProperties.Instance.Player.Position
                        _WasActivelyPlaying = Utilities.SharedProperties.Instance.Player.IsPlaying
                        Utilities.SharedProperties.Instance.Player.Stop()
                    End If
                    Try
                        tag.Save()
                    Catch ex As Exception
                        Utilities.DebugMode.Instance.Log(Of MainWindow)(ex.ToString)
                        If _WasReadOnly Then info.IsReadOnly = True
                        Dim errorCode As Integer = Runtime.InteropServices.Marshal.GetHRForException(ex) And ((1 << 16) - 1)
                        If (errorCode = 32 OrElse errorCode = 33) Then
                            HandyControl.Controls.MessageBox.Error(Utilities.ResourceResolver.Strings.QUERY_FILEACCESSDENIED & Environment.NewLine & ex.Message)
                            Return
                        End If
                    End Try
                    If _WasPlaying Then
                        Await Utilities.SharedProperties.Instance.Player.LoadSong(Utilities.SharedProperties.Instance.Player.StreamMetadata)
                        Utilities.SharedProperties.Instance.Player.Position = _Position
                        If _WasActivelyPlaying Then Utilities.SharedProperties.Instance.Player.PlayCommand.Execute(Utilities.SharedProperties.Instance.Player.Stream)
                    End If
                    If _WasReadOnly Then
                        info.IsReadOnly = True
                    End If
                    'Update Library
                    Dim Meta = Player.Metadata.FromFile(_Path, True, True)
                    If Meta Is Nothing Then
                        'Check Player
                        If Utilities.SharedProperties.Instance.Player.StreamMetadata?.Path = _Path Then
                            Utilities.SharedProperties.Instance.Player.StreamMetadata.Covers = Nothing
                            Utilities.SharedProperties.Instance.Player.StreamMetadata.RefreshTagsFromFile_ThreadSafe()
                            Utilities.SharedProperties.Instance.Player.StreamMetadata.EnsureCovers()
                        End If
                    Else
                        Meta.RefreshTagsFromFile_ThreadSafe(True)
                    End If
                End If
            End If
        Catch ex As Exception
            Utilities.DebugMode.Instance.Log(Of MainWindow)(ex.ToString)
            HandyControl.Controls.MessageBox.Show(Me, Utilities.ResourceResolver.Strings.QUERY_ERROR_PARSE & Environment.NewLine & ex.Message, Utilities.ResourceResolver.Strings.APPNAME, icon:=MessageBoxImage.Error)
        End Try
    End Function
    Private Sub SubtitlesItemsControlScrollIntoView(Item As SubtitleItem)
        If Not IsUsingSubtitles OrElse Not Grid_SideBar_Right_ToggleButton_Subtitles.IsChecked Then Return
        Dim i = Subtitles.IndexOf(Item)
        If i = -1 Then Return
        Dim sv As ScrollViewer = VisualTreeHelper.GetChild(Grid_SideBar_Right_ItemsControl_Subtitles, 0)
        Dim gr As Grid = VisualTreeHelper.GetChild(sv, 0)
        Dim sip = VisualTreeHelper.GetChild(gr, 0)
        Dim ip As ItemsPresenter = VisualTreeHelper.GetChild(sip, 0)
        Dim vsp As VirtualizingStackPanel = VisualTreeHelper.GetChild(ip, 0)
        vsp.BringIndexIntoViewPublic(i)
    End Sub

    Private Commands_WriteSYLTToCurrentFile_IsBusy As Boolean = False

    Private Sub MenuItem_NotifyIcon_Exit_Click(sender As Object, e As RoutedEventArgs)
        Application.Current.Shutdown()
    End Sub

    Private Sub TextBox_Chat_Message_KeyUp(sender As Object, e As KeyEventArgs) Handles TextBox_Chat_Message.KeyUp
        If e.Key = Key.Enter Then
            If SharedProperties.Instance.HTTPFileServer.QuickLink?.SendMessageCommand.CanExecute(TextBox_Chat_Message.Text) Then SharedProperties.Instance.HTTPFileServer.QuickLink?.SendMessageCommand.Execute(TextBox_Chat_Message.Text)
            TextBox_Chat_Message.Clear()
        End If
    End Sub

    Private Sub ChatBubble_Selected(sender As Object, e As RoutedEventArgs)
        If TypeOf e.OriginalSource Is FrameworkElement AndAlso TypeOf CType(e.OriginalSource, FrameworkElement).Tag Is UPnP.QuickLink.ChatInfo Then
            Dim info As UPnP.QuickLink.ChatInfo = CType(e.OriginalSource, FrameworkElement).Tag
            If info.Type = ChatMessageType.Audio Then
                If info.Role = ChatRoleType.Sender Then
                    SharedProperties.Instance.Player.Preview(Player.Metadata.FromFile(info.Data))
                Else
                    Dim dMeta = CType(info.Data, Player.Metadata)
                    Dim meta As New Player.Metadata With {.Location = Player.Metadata.FileLocation.Remote}
                    dMeta.CopyTo(meta)
                    meta.Path = $"{SharedProperties.Instance.HTTPFileServer.QuickLink.LinkedToAddress}/{System.Net.WebUtility.UrlEncode(dMeta.Path)}"
                    SharedProperties.Instance.Player.Preview(meta, New String() {"Authorization: Bearer " & SharedProperties.Instance.HTTPFileServer.QuickLink.GetLinkToken.ToString})
                End If
            ElseIf info.Type = ChatMessageType.Image Then
                Dim IV As New HandyControl.Controls.ImageViewer With {
.ImageSource = BitmapFrame.Create(TryCast(info.Message, BitmapSource))
}
                Dim Popup As New HandyControl.Controls.Window() With {.Content = IV, .Width = 500, .Height = 500, .Background = Nothing, .WindowStartupLocation = WindowStartupLocation.CenterScreen}
                AddHandler Popup.Loaded, Sub()
                                             Popup.InvalidateVisual()
                                         End Sub
                Popup.ShowDialog()
            End If
        End If
    End Sub

    Private Sub Grid_Chat_GotFocus(sender As Object, e As RoutedEventArgs)
        SharedProperties.Instance.HTTPFileServer.QuickLink.LockChatRead = True
    End Sub
End Class
