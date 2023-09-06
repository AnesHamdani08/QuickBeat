Imports System.ComponentModel
Imports System.Threading
Imports HandyControl.Tools.Command
Imports QuickBeat.Player
Imports QuickBeat.Library.WPF.Commands
Imports QuickBeat.Utilities
Imports QuickBeat.Player.WPF.Commands
Imports QuickBeat.WPF.Commands
Imports EDeezer = E.Deezer
Imports QuickBeat.Classes
Imports QuickBeat.Interfaces

Namespace Utilities

    Public Class Commands
        Public Shared LibraryScanCommandX = AsyncCommand.Create(Of Library.Library)(New Func(Of Library.Library, CancellationToken, Task)(Function(library, token)
                                                                                                                                              Utilities.DebugMode.Instance.Log(Of Commands)("Attempting to scan...")
                                                                                                                                              If library Is Nothing Then
                                                                                                                                                  Utilities.DebugMode.Instance.Log(Of Commands)("Scan failed, Error:=null object")
                                                                                                                                                  Return Task.FromResult(Of Boolean)(False)
                                                                                                                                              End If
                                                                                                                                              If My.Settings.APP_LIBRARY_PATHS Is Nothing OrElse My.Settings.APP_LIBRARY_PATHS.Count = 0 Then Task.FromResult(Of Boolean)(False)
                                                                                                                                              Dim Paths As New List(Of String)
                                                                                                                                              For Each path In My.Settings.APP_LIBRARY_PATHS
                                                                                                                                                  Paths.Add(path)
                                                                                                                                              Next
                                                                                                                                              Dim Files = GetFiles(Paths)
                                                                                                                                              Dim TBA = Files.Except(library.Select(Of String)(Function(k) k.Path))
                                                                                                                                              Dim TBR = library.Select(Of String)(Function(k) k.Path).Except(Files)
                                                                                                                                              SharedProperties.Instance.Notification(ResourceResolver.Strings.LIBRARY, ResourceResolver.Strings.TEXT_LIBRARY_SCAN_RESULT.Replace("%c0", TBA.Count).Replace("%c1", TBR.Count), HandyControl.Data.NotifyIconInfoType.Info)
                                                                                                                                              Utilities.DebugMode.Instance.Log(Of Commands)("Found " & TBA.Count & " Files to be added. Adding...")
                                                                                                                                              For Each file In TBA
                                                                                                                                                  library.Add(Metadata.FromFile(file, True))
                                                                                                                                                  If token.IsCancellationRequested Then
                                                                                                                                                      Utilities.DebugMode.Instance.Log(Of Commands)("Scan canceled.")
                                                                                                                                                      Return Task.FromResult(Of Boolean)(False)
                                                                                                                                                  End If
                                                                                                                                              Next
                                                                                                                                              Dim i = 0
                                                                                                                                              Utilities.DebugMode.Instance.Log(Of Commands)("Found " & TBR.Count & " Files to be removed. Removing...")
                                                                                                                                              For Each file In TBR
                                                                                                                                                  Dim TBRi = library.FirstOrDefault(Function(k) k.Path = file)
                                                                                                                                                  If TBRi Is Nothing Then
                                                                                                                                                      i += 1
                                                                                                                                                      Continue For
                                                                                                                                                  End If
                                                                                                                                                  library.Remove(TBRi)
                                                                                                                                                  If token.IsCancellationRequested Then
                                                                                                                                                      Utilities.DebugMode.Instance.Log(Of Commands)("Scan canceled.")
                                                                                                                                                      Return Task.FromResult(Of Boolean)(False)
                                                                                                                                                  End If
                                                                                                                                              Next
                                                                                                                                              If i > 0 Then Utilities.DebugMode.Instance.Log(Of Commands)(i & " Files failed to be removed.")
                                                                                                                                              If i > 0 Then SharedProperties.Instance.Notification(ResourceResolver.Strings.LIBRARY, ResourceResolver.Strings.TEXT_LIBRARY_SCAN_RESULT_REMOVE_FAILURE.Replace("%c0", i), HandyControl.Data.NotifyIconInfoType.Error)
                                                                                                                                              Return Task.FromResult(Of Boolean)(True)
                                                                                                                                          End Function))

        Public Shared LibraryRebuildCommandX = AsyncCommand.Create(Of Library.Library)(New Func(Of Library.Library, CancellationToken, Task)(Function(library, token)
                                                                                                                                                 If library Is Nothing Then Return Task.FromResult(Of Boolean)(False)
                                                                                                                                                 Utilities.DebugMode.Instance.Log(Of Commands)("Rebuilding library...")
                                                                                                                                                 library.Clear()
                                                                                                                                                 For Each path In My.Settings.APP_LIBRARY_PATHS
                                                                                                                                                     If Not IO.Directory.Exists(path) Then Continue For
                                                                                                                                                     For Each file In GetFiles(path)
                                                                                                                                                         library.Add(Metadata.FromFile(file, True))
                                                                                                                                                         If token.IsCancellationRequested Then
                                                                                                                                                             Return Task.FromResult(Of Boolean)(False)
                                                                                                                                                         End If
                                                                                                                                                     Next
                                                                                                                                                 Next
                                                                                                                                                 Utilities.DebugMode.Instance.Log(Of Commands)("Library rebuilt successfuly.")
                                                                                                                                                 Return Task.FromResult(Of Boolean)(True)
                                                                                                                                             End Function))

        Public Shared LibraryScanCommand As New LibraryScanCommand

        Public Shared LibraryRebuildCommand As New LibraryRebuildCommand

        Public Shared LibraryRefreshTagsCommand As New LibraryRefreshTagsCommand

        Public Shared LibraryClearCommand As New LibraryClearCommand

        Public Shared LibraryPathAddCommand As New LibraryPathAddCommand

        Public Shared LibraryPathRemoveCommand As New LibraryPathRemoveCommand

        Public Shared LibraryPathClearCommand As New LibraryPathClearCommand

        Public Shared AddToSharedQueueCommand As New AddToSharedQueueCommand

        Public Shared AddItemsToSharedQueueCommand As New AddItemsToSharedQueueCommand

        Public Shared AddToSharedPlaylistCommand As New AddToSharedPlaylistCommand

        Public Shared AddItemsToSharedPlaylistCommand As New AddItemsToSharedPlaylistCommand

        Public Shared AddItemsToPlaylistFromPickerCommand As New AddItemsToPlaylistFromPickerCommand

        Public Shared RefreshMetadataActivePathCommand As New RefreshMetadataActivePathCommand

        Public Shared ViewImageCommand As New ViewImageCommand

        Public Shared ShowTagEditorCommand As New ShowTagEditorCommand

        Public Shared LoadResourceDictionaryCommand As New LoadResourceDictionaryCommand

        Public Shared MergeResourceDictionaryCommand As New MergeResourceDictionaryCommand

        Public Shared ClearMergedResourceDictionariesCommand As New ClearMergedResourceDictionariesCommand

        Public Shared RemoveMergedResourceDictionaryCommand As New RemoveMergedResourceDictionaryCommand

        Public Shared RestoreResourcesCommand As New RestoreResourcesCommand

        Public Shared ExportPlaylistCommand As New ExportPlaylistCommand

        Public Shared ImportPlaylistCommand As New ImportPlaylistCommand

        Public Shared RemoveCustomPlaylistCommand As New RemoveCustomPlaylistCommand

        Public Shared ViewPlaylistCommand As New ViewPlaylistCommand

        Public Shared DeezerBrowseCommand As New Deezer.WPF.BrowseCommand
    End Class

End Namespace
Namespace Hotkeys.WPF.Commands
    Public Class AddHotkeyCommand
        Implements ICommand

        Public Event CanExecuteChanged As EventHandler Implements ICommand.CanExecuteChanged

        Private _Hotkey As Hotkeys.HotkeyManager

        Sub New(HotkeyManager As Hotkeys.HotkeyManager)
            _Hotkey = HotkeyManager
            AddHandler CommandManager.RequerySuggested, New EventHandler(Sub(sender As Object, e As EventArgs)
                                                                             RaiseEvent CanExecuteChanged(Me, New EventArgs())
                                                                         End Sub)
        End Sub

        Public Sub Execute(parameter As Object) Implements ICommand.Execute
            _Hotkey.Add(TryCast(parameter, Hotkeys.HotkeyManager.Hotkey))
        End Sub

        Public Function CanExecute(parameter As Object) As Boolean Implements ICommand.CanExecute
            Return (_Hotkey IsNot Nothing AndAlso TypeOf parameter Is Hotkeys.HotkeyManager.Hotkey AndAlso parameter IsNot Nothing)
        End Function
    End Class

    Public Class RemoveHotkeyCommand
        Implements ICommand

        Public Event CanExecuteChanged As EventHandler Implements ICommand.CanExecuteChanged

        Private _Hotkey As Hotkeys.HotkeyManager

        Sub New(HotkeyManager As Hotkeys.HotkeyManager)
            _Hotkey = HotkeyManager
            AddHandler CommandManager.RequerySuggested, New EventHandler(Sub(sender As Object, e As EventArgs)
                                                                             RaiseEvent CanExecuteChanged(Me, New EventArgs())
                                                                         End Sub)
        End Sub

        Public Sub Execute(parameter As Object) Implements ICommand.Execute
            _Hotkey.Remove(TryCast(parameter, Hotkeys.HotkeyManager.Hotkey))
        End Sub

        Public Function CanExecute(parameter As Object) As Boolean Implements ICommand.CanExecute
            Return (_Hotkey IsNot Nothing AndAlso TypeOf parameter Is Hotkeys.HotkeyManager.Hotkey AndAlso parameter IsNot Nothing)
        End Function
    End Class

    Public Class ClearHotkeysCommand
        Implements ICommand

        Public Event CanExecuteChanged As EventHandler Implements ICommand.CanExecuteChanged

        Private _Hotkey As Hotkeys.HotkeyManager

        Sub New(HotkeyManager As Hotkeys.HotkeyManager)
            _Hotkey = HotkeyManager
            AddHandler CommandManager.RequerySuggested, New EventHandler(Sub(sender As Object, e As EventArgs)
                                                                             RaiseEvent CanExecuteChanged(Me, New EventArgs())
                                                                         End Sub)
        End Sub

        Public Sub Execute(parameter As Object) Implements ICommand.Execute
            _Hotkey.Clear()
        End Sub

        Public Function CanExecute(parameter As Object) As Boolean Implements ICommand.CanExecute
            Return (_Hotkey IsNot Nothing)
        End Function
    End Class

    Public Class EnableHotkeyCommand
        Implements ICommand

        Public Event CanExecuteChanged As EventHandler Implements ICommand.CanExecuteChanged

        Private _Hotkey As Hotkeys.HotkeyManager

        Sub New(HotkeyManager As Hotkeys.HotkeyManager)
            _Hotkey = HotkeyManager
            AddHandler CommandManager.RequerySuggested, New EventHandler(Sub(sender As Object, e As EventArgs)
                                                                             RaiseEvent CanExecuteChanged(Me, New EventArgs())
                                                                         End Sub)
        End Sub

        Public Sub Execute(parameter As Object) Implements ICommand.Execute
            _Hotkey.Enable(TryCast(parameter, Hotkeys.HotkeyManager.Hotkey))
        End Sub

        Public Function CanExecute(parameter As Object) As Boolean Implements ICommand.CanExecute
            Return (_Hotkey IsNot Nothing AndAlso TypeOf parameter Is Hotkeys.HotkeyManager.Hotkey AndAlso parameter IsNot Nothing)
        End Function
    End Class

    Public Class DisableHotkeyCommand
        Implements ICommand

        Public Event CanExecuteChanged As EventHandler Implements ICommand.CanExecuteChanged

        Private _Hotkey As Hotkeys.HotkeyManager

        Sub New(HotkeyManager As Hotkeys.HotkeyManager)
            _Hotkey = HotkeyManager
            AddHandler CommandManager.RequerySuggested, New EventHandler(Sub(sender As Object, e As EventArgs)
                                                                             RaiseEvent CanExecuteChanged(Me, New EventArgs())
                                                                         End Sub)
        End Sub

        Public Sub Execute(parameter As Object) Implements ICommand.Execute
            _Hotkey.Disable(TryCast(parameter, Hotkeys.HotkeyManager.Hotkey))
        End Sub

        Public Function CanExecute(parameter As Object) As Boolean Implements ICommand.CanExecute
            Return (_Hotkey IsNot Nothing AndAlso TypeOf parameter Is Hotkeys.HotkeyManager.Hotkey AndAlso parameter IsNot Nothing)
        End Function
    End Class

End Namespace
Namespace Library.WPF.Commands

    Public Class LibraryPathAddCommand
        Implements ICommand

        Public Event CanExecuteChanged As EventHandler Implements ICommand.CanExecuteChanged

        Sub New()
            AddHandler CommandManager.RequerySuggested, New EventHandler(Sub(sender As Object, e As EventArgs)
                                                                             RaiseEvent CanExecuteChanged(Me, New EventArgs())
                                                                         End Sub)
        End Sub

        Public Sub Execute(parameter As Object) Implements ICommand.Execute
            Dim path = parameter
            If Not IO.Directory.Exists(path) Then
                Dim dialog = New Microsoft.Win32.SaveFileDialog()
                dialog.Title = "Select a Directory"
                dialog.Filter = "Directory|*.this.directory"
                dialog.FileName = "select"
                If dialog.ShowDialog() Then
                    path = dialog.FileName
                    'Remove fake filename from resulting path                    
                    path = path.Replace("\select.this.directory", "")
                    path = path.Replace(".this.directory", "")
                    'If user has changed the filename, create the New directory
                    If Not System.IO.Directory.Exists(path) Then
                        System.IO.Directory.CreateDirectory(path)
                    End If
                    ' Our final value Is in path                    
                End If
            End If
            'If My.Settings.APP_LIBRARY_PATHS.Contains(path) Then Return
            'My.Settings.APP_LIBRARY_PATHS.Add(path)
            'My.Settings.Save()
            If SharedProperties.Instance.LibraryPaths.Contains(path) Then Return
            SharedProperties.Instance.LibraryPaths.Add(path)
        End Sub

        Public Function CanExecute(parameter As Object) As Boolean Implements ICommand.CanExecute
            Return True
        End Function
    End Class

    Public Class LibraryScanCommand
        Implements ICommand, ComponentModel.INotifyPropertyChanged

        Public Event CanExecuteChanged As EventHandler Implements ICommand.CanExecuteChanged
        Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged

        Private _IsScanning As Boolean
        Property IsScanning As Boolean
            Get
                Return _IsScanning
            End Get
            Set(value As Boolean)
                _IsScanning = value
                OnPropertyChanged()
            End Set
        End Property

        Private _StopScanning As Boolean
        Public Property StopScanning As Boolean
            Get
                Return _StopScanning
            End Get
            Set(value As Boolean)
                _StopScanning = value
                OnPropertyChanged()
            End Set
        End Property

        Sub New()
            AddHandler CommandManager.RequerySuggested, New EventHandler(Sub(sender As Object, e As EventArgs)
                                                                             RaiseEvent CanExecuteChanged(Me, New EventArgs())
                                                                         End Sub)
        End Sub

        Public Sub OnPropertyChanged(<Runtime.CompilerServices.CallerMemberName> Optional CallerName As String = Nothing)
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(CallerName))
        End Sub

        Public Async Sub Execute(parameter As Object) Implements ICommand.Execute
            Utilities.DebugMode.Instance.Log(Of Utilities.Commands)("Attempting to scan...")
            IsScanning = True 'Block incoming calls until we're done with this one
            If parameter Is Nothing Then
                Utilities.DebugMode.Instance.Log(Of Utilities.Commands)("Scan failed, Error:=null object")
                IsScanning = False
                StopScanning = False
                Return
            End If
            If My.Settings.APP_LIBRARY_PATHS Is Nothing OrElse My.Settings.APP_LIBRARY_PATHS.Count = 0 Then
                IsScanning = False
                StopScanning = False
                Return
            End If
            Dim Paths As New List(Of String)
            For Each path In My.Settings.APP_LIBRARY_PATHS
                Paths.Add(path)
            Next
            Await Task.Run(Sub()
                               Dim Files = GetFiles(Paths)
                               Dim TBA = Files.Except(CType(parameter, Library).Select(Of String)(Function(k) k.Path))
                               Dim TBR = CType(parameter, Library).Select(Of String)(Function(k) k.Path).Except(Files)
                               Dim TBRMeta = TBR.Select(Of Metadata)(Function(k) CType(parameter, Library).FirstOrDefault(Function(l) l.Path = k)).ToList
                               Application.Current.Dispatcher.Invoke(Sub()
                                                                         SharedProperties.Instance.Notification(ResourceResolver.Strings.LIBRARY, ResourceResolver.Strings.TEXT_LIBRARY_SCAN_RESULT.Replace("%c0", TBA.Count).Replace("%c1", TBR.Count), HandyControl.Data.NotifyIconInfoType.Info)
                                                                         Utilities.DebugMode.Instance.Log(Of Utilities.Commands)("Found " & TBA.Count & " Files to be added. Adding...")
                                                                     End Sub)
                               For Each file In TBA
                                   Dim meta = Metadata.FromFile(file, True)
                                   Application.Current.Dispatcher.Invoke(Sub()
                                                                             CType(parameter, Library).Add(meta)
                                                                         End Sub)
                                   If StopScanning Then
                                       Utilities.DebugMode.Instance.Log(Of Utilities.Commands)("Scan canceled at adding phase.")
                                       IsScanning = False
                                       StopScanning = False
                                       Return
                                   End If
                               Next
                               Dim i = 0
                               Application.Current.Dispatcher.Invoke(Sub()
                                                                         Utilities.DebugMode.Instance.Log(Of Utilities.Commands)("Found " & TBR.Count & " Files to be removed. Removing...")
                                                                     End Sub)
                               Do While i < TBRMeta.Count
                                   Dim m = TBRMeta(i)
                                   Application.Current.Dispatcher.Invoke(Sub()
                                                                             If m Is Nothing Then i += 1 : Return
                                                                             If CType(parameter, Library).Remove(m) Then
                                                                                 TBRMeta.RemoveAt(0)
                                                                             Else
                                                                                 i += 1
                                                                             End If
                                                                         End Sub)
                                   If StopScanning Then
                                       Utilities.DebugMode.Instance.Log(Of Utilities.Commands)("Scan canceled as removing phase.")
                                       IsScanning = False
                                       StopScanning = False
                                       Return
                                   End If
                               Loop
                               If i > 0 Then Utilities.DebugMode.Instance.Log(Of Utilities.Commands)(i & " Files failed to be removed.")
                               If i > 0 Then
                                   Application.Current.Dispatcher.Invoke(Sub()
                                                                             SharedProperties.Instance.Notification(ResourceResolver.Strings.LIBRARY, ResourceResolver.Strings.TEXT_LIBRARY_SCAN_RESULT_REMOVE_FAILURE.Replace("%c0", i), HandyControl.Data.NotifyIconInfoType.Error)
                                                                         End Sub)
                               End If
                           End Sub)
            StopScanning = False
            IsScanning = False
        End Sub

        Public Function CanExecute(parameter As Object) As Boolean Implements ICommand.CanExecute
            Return (Not IsScanning AndAlso parameter IsNot Nothing AndAlso TypeOf parameter Is Library)
        End Function
    End Class

    Public Class LibraryRebuildCommand
        Implements ICommand, ComponentModel.INotifyPropertyChanged

        Public Event CanExecuteChanged As EventHandler Implements ICommand.CanExecuteChanged
        Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged

        Public Sub OnPropertyChanged(<Runtime.CompilerServices.CallerMemberName> Optional CallerName As String = Nothing)
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(CallerName))
        End Sub

        Private _IsRebuilding As Boolean
        Property IsRebuilding As Boolean
            Get
                Return _IsRebuilding
            End Get
            Set(value As Boolean)
                _IsRebuilding = value
                OnPropertyChanged()
            End Set
        End Property

        Private _StopRebuilding As Boolean
        Property StopRebuilding As Boolean
            Get
                Return _StopRebuilding
            End Get
            Set(value As Boolean)
                _StopRebuilding = value
                OnPropertyChanged()
            End Set
        End Property

        Sub New()
            AddHandler CommandManager.RequerySuggested, New EventHandler(Sub(sender As Object, e As EventArgs)
                                                                             RaiseEvent CanExecuteChanged(Me, New EventArgs())
                                                                         End Sub)
        End Sub

        Public Async Sub Execute(parameter As Object) Implements ICommand.Execute
            IsRebuilding = True
            If parameter Is Nothing Then
                IsRebuilding = False
                Return
            End If
            Utilities.DebugMode.Instance.Log(Of Utilities.Commands)("Rebuilding ctype(parameter,Library)...")
            CType(parameter, Library).Clear()
            Await Task.Run(Sub()
                               Dim LibPaths(My.Settings.APP_LIBRARY_PATHS.Count - 1) As String
                               My.Settings.APP_LIBRARY_PATHS.CopyTo(LibPaths, 0)
                               Dim files = GetFiles(LibPaths).Select(Of Metadata)(Function(k) Metadata.FromFile(k, True))
                               Dim i = 0
                               For Each file In files
                                   Application.Current.Dispatcher.Invoke(Sub()
                                                                             If Not StopRebuilding Then CType(parameter, Library).Add(file)
                                                                         End Sub)
                                   If StopRebuilding Then
                                       Return
                                   End If
                               Next
                           End Sub)
            CType(parameter, Library).EnsureMostPlayed()
            Utilities.DebugMode.Instance.Log(Of Utilities.Commands)("Library rebuilt successfuly.")
            StopRebuilding = False
            IsRebuilding = False
            HandyControl.Controls.MessageBox.Ask(Utilities.ResourceResolver.Strings.QUERY_APP_RESTART)
        End Sub

        Public Function CanExecute(parameter As Object) As Boolean Implements ICommand.CanExecute
            Return (Not IsRebuilding AndAlso parameter IsNot Nothing AndAlso TypeOf parameter Is Library)
        End Function
    End Class

    Public Class LibraryRefreshTagsCommand
        Implements ICommand, ComponentModel.INotifyPropertyChanged

        Public Event CanExecuteChanged As EventHandler Implements ICommand.CanExecuteChanged
        Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged

        Private _IsScanning As Boolean
        Property IsScanning As Boolean
            Get
                Return _IsScanning
            End Get
            Set(value As Boolean)
                _IsScanning = value
                OnPropertyChanged()
            End Set
        End Property

        Private _StopScanning As Boolean
        Public Property StopScanning As Boolean
            Get
                Return _StopScanning
            End Get
            Set(value As Boolean)
                _StopScanning = value
                OnPropertyChanged()
            End Set
        End Property

        Sub New()
            AddHandler CommandManager.RequerySuggested, New EventHandler(Sub(sender As Object, e As EventArgs)
                                                                             RaiseEvent CanExecuteChanged(Me, New EventArgs())
                                                                         End Sub)
        End Sub

        Public Sub OnPropertyChanged(<Runtime.CompilerServices.CallerMemberName> Optional CallerName As String = Nothing)
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(CallerName))
        End Sub

        Public Async Sub Execute(parameter As Object) Implements ICommand.Execute
            Utilities.DebugMode.Instance.Log(Of Utilities.Commands)("Attempting to scan...")
            IsScanning = True 'Block incoming calls until we're done with this one
            If parameter Is Nothing Then
                Utilities.DebugMode.Instance.Log(Of Utilities.Commands)("Tag refresh failed, Error:=null object")
                IsScanning = False
                StopScanning = False
                Return
            End If
            Await Task.Run(Sub()
                               For Each file In CType(parameter, Library)
                                   file.RefreshTagsFromFile_ThreadSafe(True)
                                   If StopScanning Then
                                       Utilities.DebugMode.Instance.Log(Of Utilities.Commands)("Tag refresh canceled.")
                                       IsScanning = False
                                       StopScanning = False
                                       Return
                                   End If
                               Next
                           End Sub)
            StopScanning = False
            IsScanning = False
        End Sub

        Public Function CanExecute(parameter As Object) As Boolean Implements ICommand.CanExecute
            Return (Not IsScanning AndAlso parameter IsNot Nothing AndAlso TypeOf parameter Is Library)
        End Function
    End Class

    Public Class LibraryPathRemoveCommand
        Implements ICommand

        Public Event CanExecuteChanged As EventHandler Implements ICommand.CanExecuteChanged

        Sub New()
            AddHandler CommandManager.RequerySuggested, New EventHandler(Sub(sender As Object, e As EventArgs)
                                                                             RaiseEvent CanExecuteChanged(Me, New EventArgs())
                                                                         End Sub)
        End Sub

        Public Sub Execute(parameter As Object) Implements ICommand.Execute
            'My.Settings.APP_LIBRARY_PATHS.Remove(parameter)
            'My.Settings.Save()
            SharedProperties.Instance.LibraryPaths.Remove(parameter)
        End Sub

        Public Function CanExecute(parameter As Object) As Boolean Implements ICommand.CanExecute
            'Return My.Settings.APP_LIBRARY_PATHS.Contains(parameter?.ToString)
            Return SharedProperties.Instance.LibraryPaths.Contains(parameter?.ToString)
        End Function
    End Class

    Public Class LibraryPathClearCommand
        Implements ICommand

        Public Event CanExecuteChanged As EventHandler Implements ICommand.CanExecuteChanged

        Sub New()
            AddHandler CommandManager.RequerySuggested, New EventHandler(Sub(sender As Object, e As EventArgs)
                                                                             RaiseEvent CanExecuteChanged(Me, New EventArgs())
                                                                         End Sub)
        End Sub

        Public Sub Execute(parameter As Object) Implements ICommand.Execute
            'My.Settings.APP_LIBRARY_PATHS.Clear()
            'My.Settings.Save()
            SharedProperties.Instance.LibraryPaths.Clear()
        End Sub

        Public Function CanExecute(parameter As Object) As Boolean Implements ICommand.CanExecute
            Return True
        End Function
    End Class

    Public Class LibraryClearCommand
        Implements ICommand

        Public Event CanExecuteChanged As EventHandler Implements ICommand.CanExecuteChanged

        Sub New()
            AddHandler CommandManager.RequerySuggested, New EventHandler(Sub(sender As Object, e As EventArgs)
                                                                             RaiseEvent CanExecuteChanged(Me, New EventArgs())
                                                                         End Sub)
        End Sub

        Public Sub Execute(parameter As Object) Implements ICommand.Execute
            TryCast(parameter, Library)?.Clear()
        End Sub

        Public Function CanExecute(parameter As Object) As Boolean Implements ICommand.CanExecute
            Return (TypeOf parameter Is Library AndAlso parameter IsNot Nothing)
        End Function
    End Class

End Namespace
Namespace Player.WPF.Commands
    Public Class AddToSharedQueueCommand
        Implements ICommand

        Public Event CanExecuteChanged As EventHandler Implements ICommand.CanExecuteChanged
        Sub New()
            AddHandler CommandManager.RequerySuggested, New EventHandler(Sub(sender As Object, e As EventArgs)
                                                                             RaiseEvent CanExecuteChanged(Me, New EventArgs())
                                                                         End Sub)
        End Sub

        Public Sub Execute(parameter As Object) Implements ICommand.Execute
            TryCast(Utilities.SharedProperties.Instance?.Player?.Playlist, Playlist)?.Queue.Add(Metadata.FromFile(parameter.ToString))
        End Sub

        Public Function CanExecute(parameter As Object) As Boolean Implements ICommand.CanExecute
            Return IO.File.Exists(parameter?.ToString)
        End Function
    End Class
    Public Class AddItemsToSharedQueueCommand
        Implements ICommand

        Public Event CanExecuteChanged As EventHandler Implements ICommand.CanExecuteChanged
        Sub New()
            AddHandler CommandManager.RequerySuggested, New EventHandler(Sub(sender As Object, e As EventArgs)
                                                                             RaiseEvent CanExecuteChanged(Me, New EventArgs())
                                                                         End Sub)
        End Sub

        Public Sub Execute(parameter As Object) Implements ICommand.Execute
            For Each item In parameter
                TryCast(Utilities.SharedProperties.Instance?.Player?.Playlist, Playlist)?.Queue.Add(TryCast(item, Metadata))
            Next
        End Sub

        Public Function CanExecute(parameter As Object) As Boolean Implements ICommand.CanExecute
            Return parameter IsNot Nothing
        End Function
    End Class
    Public Class AddToSharedPlaylistCommand
        Implements ICommand

        Public Event CanExecuteChanged As EventHandler Implements ICommand.CanExecuteChanged
        Sub New()
            AddHandler CommandManager.RequerySuggested, New EventHandler(Sub(sender As Object, e As EventArgs)
                                                                             RaiseEvent CanExecuteChanged(Me, New EventArgs())
                                                                         End Sub)
        End Sub

        Public Sub Execute(parameter As Object) Implements ICommand.Execute
            If TypeOf parameter Is IEnumerable(Of Metadata) Then
            Else
                TryCast(Utilities.SharedProperties.Instance?.Player?.Playlist, Playlist)?.Add(Metadata.FromFile(parameter.ToString))
            End If
        End Sub

        Public Function CanExecute(parameter As Object) As Boolean Implements ICommand.CanExecute
            Return IO.File.Exists(parameter?.ToString)
        End Function
    End Class
    Public Class AddItemsToSharedPlaylistCommand
        Implements ICommand

        Public Event CanExecuteChanged As EventHandler Implements ICommand.CanExecuteChanged
        Sub New()
            AddHandler CommandManager.RequerySuggested, New EventHandler(Sub(sender As Object, e As EventArgs)
                                                                             RaiseEvent CanExecuteChanged(Me, New EventArgs())
                                                                         End Sub)
        End Sub

        Public Sub Execute(parameter As Object) Implements ICommand.Execute
            For Each item In parameter
                TryCast(Utilities.SharedProperties.Instance?.Player?.Playlist, Playlist)?.Add(TryCast(item, Metadata))
            Next
        End Sub

        Public Function CanExecute(parameter As Object) As Boolean Implements ICommand.CanExecute
            Return parameter IsNot Nothing
        End Function
    End Class
    Public Class AddItemsToPlaylistFromPickerCommand
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
                For Each item In parameter
                    DLG.DialogPlaylistResult?.Add(TryCast(item, Metadata))
                Next
            End If
        End Sub

        Public Function CanExecute(parameter As Object) As Boolean Implements ICommand.CanExecute
            Return parameter IsNot Nothing
        End Function
    End Class
    Public Class RefreshMetadataActivePathCommand
        Implements ICommand

        Public Event CanExecuteChanged As EventHandler Implements ICommand.CanExecuteChanged
        Sub New()
            AddHandler CommandManager.RequerySuggested, New EventHandler(Sub(sender As Object, e As EventArgs)
                                                                             RaiseEvent CanExecuteChanged(Me, New EventArgs())
                                                                         End Sub)
        End Sub

        Private _IsBusy As Boolean

        Public Async Sub Execute(parameter As Object) Implements ICommand.Execute
            _IsBusy = True
            With TryCast(parameter, Metadata)
                Await .GetActivePath
            End With
            _IsBusy = False
        End Sub

        Public Function CanExecute(parameter As Object) As Boolean Implements ICommand.CanExecute
            Return (Not _IsBusy) AndAlso (parameter IsNot Nothing AndAlso TypeOf parameter Is Metadata)
        End Function
    End Class
End Namespace
Namespace WPF.Commands
    Public Class ViewImageCommand
        Implements ICommand

        Public Event CanExecuteChanged As EventHandler Implements ICommand.CanExecuteChanged
        Sub New()
            AddHandler CommandManager.RequerySuggested, New EventHandler(Sub(sender As Object, e As EventArgs)
                                                                             RaiseEvent CanExecuteChanged(Me, New EventArgs())
                                                                         End Sub)
        End Sub

        Public Sub Execute(parameter As Object) Implements ICommand.Execute
            Dim IV As New HandyControl.Controls.ImageViewer()
            IV.ImageSource = BitmapFrame.Create(TryCast(parameter, BitmapSource))
            Dim Popup As New HandyControl.Controls.Window() With {.Content = IV, .Width = 500, .Height = 500, .Background = Nothing, .WindowStartupLocation = WindowStartupLocation.CenterScreen}
            AddHandler Popup.Loaded, Sub()
                                         Popup.InvalidateVisual()
                                     End Sub
            Popup.ShowDialog()
        End Sub

        Public Function CanExecute(parameter As Object) As Boolean Implements ICommand.CanExecute
            Return TypeOf parameter Is BitmapSource
        End Function
    End Class

    Public Class ShowTagEditorCommand
        Implements ICommand

        Public Event CanExecuteChanged As EventHandler Implements ICommand.CanExecuteChanged

        Sub New()
            AddHandler CommandManager.RequerySuggested, New EventHandler(Sub(sender As Object, e As EventArgs)
                                                                             RaiseEvent CanExecuteChanged(Me, New EventArgs())
                                                                         End Sub)
        End Sub

        Public Sub Execute(parameter As Object) Implements ICommand.Execute
            'parameter:=File Path
            Dim TE As New Dialogs.TagEditor(parameter) With {.Owner = Application.Current.MainWindow}
            TE.ShowDialog()
        End Sub

        Public Function CanExecute(parameter As Object) As Boolean Implements ICommand.CanExecute
            Return IO.File.Exists(parameter)
        End Function
    End Class

    Public Class LoadResourceDictionaryCommand
        Implements ICommand

        Public Event CanExecuteChanged As EventHandler Implements ICommand.CanExecuteChanged
        Sub New()
            AddHandler CommandManager.RequerySuggested, New EventHandler(Sub(sender As Object, e As EventArgs)
                                                                             RaiseEvent CanExecuteChanged(Me, New EventArgs())
                                                                         End Sub)
        End Sub

        Public Sub Execute(parameter As Object) Implements ICommand.Execute
            Dim OFD As New Microsoft.Win32.OpenFileDialog With {.Filter = "Resource Dictionary|*.xaml|All Files|*.*"}
            If OFD.ShowDialog Then
                Dim ResDict As New ResourceDictionary() With {.Source = New Uri(OFD.FileName, UriKind.Absolute)}
                Application.Current.Resources = ResDict
            End If
        End Sub

        Public Function CanExecute(parameter As Object) As Boolean Implements ICommand.CanExecute
            Return True
        End Function
    End Class

    Public Class MergeResourceDictionaryCommand
        Implements ICommand

        Public Event CanExecuteChanged As EventHandler Implements ICommand.CanExecuteChanged
        Sub New()
            AddHandler CommandManager.RequerySuggested, New EventHandler(Sub(sender As Object, e As EventArgs)
                                                                             RaiseEvent CanExecuteChanged(Me, New EventArgs())
                                                                         End Sub)
        End Sub

        Public Sub Execute(parameter As Object) Implements ICommand.Execute
            Dim ResDictSource As Uri = Nothing
            If parameter Is Nothing Then
                Dim OFD As New Microsoft.Win32.OpenFileDialog With {.Filter = "Resource Dictionary|*.xaml|All Files|*.*"}
                If OFD.ShowDialog Then
                    ResDictSource = New Uri(OFD.FileName, UriKind.Absolute)
                End If
            Else
                Dim RelURI = Dialogs.InputBox.ShowSingle("Relative Path")
                If Not String.IsNullOrEmpty(RelURI) Then
                    ResDictSource = New Uri(RelURI, UriKind.Relative)
                End If
            End If
            If ResDictSource IsNot Nothing Then
                Try
                    Dim ResDict As New ResourceDictionary() With {.Source = ResDictSource}
                    Application.Current.Resources.MergedDictionaries.Add(ResDict)
                Catch ex As Exception
                    Utilities.DebugMode.Instance.Log(Of MergeResourceDictionaryCommand)(ex.ToString)
                    HandyControl.Controls.MessageBox.Error(ex.ToString)
                End Try
            End If
        End Sub

        Public Function CanExecute(parameter As Object) As Boolean Implements ICommand.CanExecute
            Return True
        End Function
    End Class

    Public Class ClearMergedResourceDictionariesCommand
        Implements ICommand

        Public Event CanExecuteChanged As EventHandler Implements ICommand.CanExecuteChanged
        Sub New()
            AddHandler CommandManager.RequerySuggested, New EventHandler(Sub(sender As Object, e As EventArgs)
                                                                             RaiseEvent CanExecuteChanged(Me, New EventArgs())
                                                                         End Sub)
        End Sub

        Public Sub Execute(parameter As Object) Implements ICommand.Execute
            Application.Current.Resources.MergedDictionaries.Clear()
        End Sub

        Public Function CanExecute(parameter As Object) As Boolean Implements ICommand.CanExecute
            Return Application.Current.Resources.MergedDictionaries.Count > 0
        End Function
    End Class

    Public Class RemoveMergedResourceDictionaryCommand
        Implements ICommand

        Public Event CanExecuteChanged As EventHandler Implements ICommand.CanExecuteChanged
        Sub New()
            AddHandler CommandManager.RequerySuggested, New EventHandler(Sub(sender As Object, e As EventArgs)
                                                                             RaiseEvent CanExecuteChanged(Me, New EventArgs())
                                                                         End Sub)
        End Sub

        Public Sub Execute(parameter As Object) Implements ICommand.Execute
            Dim Value = Dialogs.InputBox.ShowSingle(Application.Current.MainWindow, "Index", description:=String.Join(Environment.NewLine, Application.Current.Resources.MergedDictionaries.Select(Of String)(Function(k) $"Keys[{k.Keys.Count}],Source[{k.Source?.ToString}]")))
            Dim iValue As Integer = -1
            If Integer.TryParse(Value, iValue) Then
                Application.Current.Resources.MergedDictionaries.RemoveAt(iValue)
            End If
        End Sub

        Public Function CanExecute(parameter As Object) As Boolean Implements ICommand.CanExecute
            Return True
        End Function
    End Class

    Public Class RestoreResourcesCommand
        Implements ICommand

        Public Event CanExecuteChanged As EventHandler Implements ICommand.CanExecuteChanged
        Sub New()
            AddHandler CommandManager.RequerySuggested, New EventHandler(Sub(sender As Object, e As EventArgs)
                                                                             RaiseEvent CanExecuteChanged(Me, New EventArgs())
                                                                         End Sub)
        End Sub

        Public Sub Execute(parameter As Object) Implements ICommand.Execute
            Dim ResDict As New ResourceDictionary()
            Dim IR As New HandyControl.Themes.IntellisenseResources() With {.Source = New Uri("Pack://application:,,,/HandyControl;Component/DesignTime/DesignTimeResources.xaml")}
            Dim DTRes As New ResourceDictionary With {.Source = New Uri("Pack://application:,,,/HandyControl;Component/DesignTime/DesignTimeResources.xaml")}
            Dim TR As New HandyControl.Themes.ThemeResources()
            Dim lTR As New ResourceDictionary
            HandyControl.Themes.ThemeDictionary.SetKey(lTR, "Light")
            lTR.MergedDictionaries.Add(New HandyControl.Themes.ColorPresetResources() With {.TargetTheme = HandyControl.Themes.ApplicationTheme.Light})
            TR.ThemeDictionaries.Add("Light", lTR)
            Dim dTR As New ResourceDictionary
            HandyControl.Themes.ThemeDictionary.SetKey(lTR, "Dark")
            lTR.MergedDictionaries.Add(New HandyControl.Themes.ColorPresetResources() With {.TargetTheme = HandyControl.Themes.ApplicationTheme.Dark})
            TR.ThemeDictionaries.Add("Dark", dTR)
            Dim Theme As New HandyControl.Themes.Theme
            Dim LangR As New ResourceDictionary With {.Source = New Uri("/Resources/EN_US.xaml", UriKind.Relative)}
            Dim GeoR As New ResourceDictionary With {.Source = New Uri("/Resources/Glitter.xaml", UriKind.Relative)}
            Dim DataR As New ResourceDictionary With {.Source = New Uri("/Styles/DataTemplates.xaml", UriKind.Relative)}

            ResDict.MergedDictionaries.Add(IR)
            ResDict.MergedDictionaries.Add(DTRes)
            ResDict.MergedDictionaries.Add(TR)
            ResDict.MergedDictionaries.Add(Theme)
            ResDict.MergedDictionaries.Add(LangR)
            ResDict.MergedDictionaries.Add(GeoR)
            ResDict.MergedDictionaries.Add(DataR)

            Application.Current.Resources = ResDict
        End Sub

        Public Function CanExecute(parameter As Object) As Boolean Implements ICommand.CanExecute
            Return True
        End Function
    End Class

    Public Class ExportPlaylistCommand
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
                If DLG.DialogPlaylistResult Is Nothing Then Return
                Dim SFD As New Microsoft.Win32.SaveFileDialog With {.Filter = "QuickBeat Object|*.qbo|Playlist|*.m3u;*.m3u8", .FileName = DLG.DialogPlaylistResult?.Name}
                If SFD.ShowDialog Then
                    Try
                        If SFD.FileName.EndsWith(".qbo") Then
                            If IO.File.Exists(SFD.FileName) Then
                                Using fs As New IO.FileStream(SFD.FileName, IO.FileMode.Truncate, IO.FileAccess.Write, IO.FileShare.Read)
                                    Dim BinF As New System.Runtime.Serialization.Formatters.Binary.BinaryFormatter()
                                    BinF.Serialize(fs, DLG.DialogPlaylistResult)
                                    fs.Flush()
                                End Using
                            Else
                                Using fs As New IO.FileStream(SFD.FileName, IO.FileMode.Create, IO.FileAccess.Write, IO.FileShare.Read)
                                    Dim BinF As New System.Runtime.Serialization.Formatters.Binary.BinaryFormatter()
                                    BinF.Serialize(fs, DLG.DialogPlaylistResult)
                                    fs.Flush()
                                End Using
                            End If
                        Else
                            Dim M3U As New M3UFile(Nothing) With {.Name = DLG.DialogPlaylistResult.Name}
                            For Each meta In DLG.DialogPlaylistResult
                                M3U.Add(meta)
                            Next
                            If SFD.FileName.EndsWith(".m3u8") Then
                                M3U.Save(SFD.FileName, Text.Encoding.UTF8)
                            Else
                                M3U.Save(SFD.FileName)
                            End If
                        End If
                    Catch ex As Exception
                        Utilities.DebugMode.Instance.Log(Of ExportPlaylistCommand)(ex.ToString)
                        HandyControl.Controls.MessageBox.Error(Utilities.ResourceResolver.Strings.QUERY_ERROR_UNKNOWN & Environment.NewLine & ex.Message)
                    End Try
                End If
            End If
        End Sub

        Public Function CanExecute(parameter As Object) As Boolean Implements ICommand.CanExecute
            Return True
        End Function
    End Class

    Public Class ImportPlaylistCommand
        Implements ICommand

        Public Event CanExecuteChanged As EventHandler Implements ICommand.CanExecuteChanged

        Sub New()
            AddHandler CommandManager.RequerySuggested, New EventHandler(Sub(sender As Object, e As EventArgs)
                                                                             RaiseEvent CanExecuteChanged(Me, New EventArgs())
                                                                         End Sub)
        End Sub

        Public Sub Execute(parameter As Object) Implements ICommand.Execute
            Dim OFD As New Microsoft.Win32.OpenFileDialog With {.Filter = "QuickBeat Object|*.qbo|Playlist|*.m3u;*.m3u8", .CheckFileExists = True}
            If OFD.ShowDialog Then
                Try
                    If OFD.FileName.EndsWith(".qbo") Then
                        Using fs As New IO.FileStream(OFD.FileName, IO.FileMode.Open, IO.FileAccess.Read, IO.FileShare.Read)
                            Dim BinF As New System.Runtime.Serialization.Formatters.Binary.BinaryFormatter()
                            Dim Obj = BinF.Deserialize(fs)
                            If Obj.GetType Is GetType(Player.Playlist) Then
                                Dim pObj = TryCast(Obj, Player.Playlist)
                                If HandyControl.Controls.MessageBox.Ask(Utilities.ResourceResolver.Strings.QUERY_PLAYLIST_IMPORT.Replace("%n", pObj?.Name).Replace("%c", pObj?.Count)) = MessageBoxResult.OK Then
                                    pObj.Cover = Utilities.CommonFunctions.ToCoverImage(pObj.Name)
                                    SharedProperties.Instance.CustomPlaylists.Add(pObj)
                                End If
                            End If
                        End Using
                    Else
                        Dim M3U As New M3UFile(OFD.FileName)
                        M3U.Load()
                        If HandyControl.Controls.MessageBox.Ask(Utilities.ResourceResolver.Strings.QUERY_PLAYLIST_IMPORT.Replace("%n", If(String.IsNullOrEmpty(M3U.Name), "Unknown", M3U.Name)).Replace("%c", M3U.Count)) = MessageBoxResult.OK Then
                            Dim pM3U = M3U.ToPlaylist
                            pM3U.Name = If(String.IsNullOrEmpty(M3U.Name), Guid.NewGuid.ToString, M3U.Name)
                            pM3U.Cover = Utilities.CommonFunctions.ToCoverImage(pM3U.Name)
                            SharedProperties.Instance.CustomPlaylists.Add(pM3U)
                        End If
                    End If
                Catch ex As Exception
                    Utilities.DebugMode.Instance.Log(Of ExportPlaylistCommand)(ex.ToString)
                    HandyControl.Controls.MessageBox.Error(Utilities.ResourceResolver.Strings.QUERY_ERROR_UNKNOWN & Environment.NewLine & ex.Message)
                End Try
            End If
        End Sub

        Public Function CanExecute(parameter As Object) As Boolean Implements ICommand.CanExecute
            Return True
        End Function
    End Class

    Public Class RemoveCustomPlaylistCommand
        Implements ICommand

        Public Event CanExecuteChanged As EventHandler Implements ICommand.CanExecuteChanged

        Sub New()
            AddHandler CommandManager.RequerySuggested, New EventHandler(Sub(sender As Object, e As EventArgs)
                                                                             RaiseEvent CanExecuteChanged(Me, New EventArgs())
                                                                         End Sub)
        End Sub

        Public Sub Execute(parameter As Object) Implements ICommand.Execute
            Dim pParam = TryCast(parameter, Playlist)
            If pParam IsNot Nothing Then
                If HandyControl.Controls.MessageBox.Ask(Utilities.ResourceResolver.Strings.QUERY_REMOVE.Replace("%n", pParam.Name)) = MessageBoxResult.OK Then
                    Utilities.SharedProperties.Instance.CustomPlaylists?.Remove(pParam)
                End If
            End If
        End Sub

        Public Function CanExecute(parameter As Object) As Boolean Implements ICommand.CanExecute
            Return parameter IsNot Nothing AndAlso TypeOf parameter Is Player.Playlist
        End Function
    End Class

    Public Class ViewPlaylistCommand
        Implements ICommand

        Public Event CanExecuteChanged As EventHandler Implements ICommand.CanExecuteChanged

        Sub New()
            AddHandler CommandManager.RequerySuggested, New EventHandler(Sub(sender As Object, e As EventArgs)
                                                                             RaiseEvent CanExecuteChanged(Me, New EventArgs())
                                                                         End Sub)
        End Sub

        Public Sub Execute(parameter As Object) Implements ICommand.Execute
            Dim pParam = TryCast(parameter, Playlist)
            If pParam IsNot Nothing Then
                Utilities.SharedProperties.Instance.Library?.FocusGroup(pParam)
            End If
        End Sub

        Public Function CanExecute(parameter As Object) As Boolean Implements ICommand.CanExecute
            Return parameter IsNot Nothing AndAlso TypeOf parameter Is Player.Playlist
        End Function
    End Class
End Namespace
Namespace Utilities.Deezer.WPF
    Public Class BrowseCommand
        Implements ICommand, INotifyPropertyChanged, Interfaces.IStartupItem

        Public Event CanExecuteChanged As EventHandler Implements ICommand.CanExecuteChanged
        Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged

        Public Const PLAYLIST_LIMIT As Integer = 50
        Public Const ARTIST_ALBUM_LIMIT As Integer = 5
        Public Const ARTIST_RADIO_LIMIT As Integer = 30
        Public Const ARTIST_TOP_LIMIT As Integer = 15

        Private _Preview As Boolean
        Property Preview As Boolean
            Get
                Return _Preview
            End Get
            Set(value As Boolean)
                _Preview = value
                RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(NameOf(Preview)))
            End Set
        End Property

        Private _IsBusy As Boolean
        Property IsBusy As Boolean
            Get
                Return _IsBusy
            End Get
            Set(value As Boolean)
                _IsBusy = value
                If value Then
                    Configuration.IsLoading = True
                Else
                    Configuration.IsLoading = False
                End If
                RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(NameOf(IsBusy)))
            End Set
        End Property

        Public Property Configuration As New StartupItemConfiguration("Deezer Relay") Implements IStartupItem.Configuration

        Sub New()
            AddHandler CommandManager.RequerySuggested, New EventHandler(Sub(sender As Object, e As EventArgs)
                                                                             RaiseEvent CanExecuteChanged(Me, New EventArgs())
                                                                         End Sub)
            If Not SharedProperties.Instance.ItemsConfiguration.Contains(Configuration) Then
                SharedProperties.Instance.ItemsConfiguration.Add(Configuration)
                Init()
            End If
        End Sub

        Public Async Sub Execute(parameter As Object) Implements ICommand.Execute
            If Not Utilities.SharedProperties.Instance.IsInternetConnected Then Return
            IsBusy = True
            Select Case parameter?.ToString.ToLower
                Case "search/track"
                    Dim query = Dialogs.InputBox.ShowSingle(Utilities.ResourceResolver.Strings.QUERY)
                    If Not String.IsNullOrEmpty(query) Then
                        Configuration.SetStatus("Acquiring data...", 0)
                        Dim Session = E.Deezer.DeezerSession.CreateNew
                        Dim Result = Await Session.Search.Tracks(query, aCount:=1)
                        Dim tr = Result?.FirstOrDefault
                        If tr IsNot Nothing Then
                            Configuration.SetStatus("Decoding...", 50)
                            Dim MD As New Player.Metadata() With {.Location = Player.Metadata.FileLocation.Remote, .OriginalPath = tr.Link, .Provider = New DeezerMediaProvider(tr.Id), .BlockDOWNLOADPROC = True, .Album = tr.AlbumName, .Artists = New String() {tr.ArtistName}, .PlayCount = tr.Rank, .Index = tr.Number, .Title = tr.Title, .Length = tr.Duration, .Path = tr.Preview}
                            Dim url = If(tr.HasPicture(E.Deezer.Api.PictureSize.Medium), tr.GetPicture(E.Deezer.Api.PictureSize.Medium), If(tr.HasPicture(EDeezer.Api.PictureSize.Large), tr.GetPicture(EDeezer.Api.PictureSize.Large), If(tr.HasPicture(EDeezer.Api.PictureSize.Small), tr.GetPicture(EDeezer.Api.PictureSize.Small), If(tr.HasPicture(EDeezer.Api.PictureSize.ExtraLarge), tr.GetPicture(EDeezer.Api.PictureSize.ExtraLarge), Nothing))))
                            Dim bi As New BitmapImage
                            bi.BeginInit()
                            bi.UriSource = New Uri(url)
                            bi.EndInit()
                            MD.Covers = New ImageSource() {bi}
                            Configuration.SetStatus("Routing Data...", 80)
                            If Preview Then
                                SharedProperties.Instance.Player?.Preview(MD)
                            Else
                                SharedProperties.Instance.Player?.LoadAndAddCommand.Execute(MD)
                            End If
                        End If
                        Session?.Dispose()
                    End If
                Case "search/artist"
                    Dim query = Dialogs.InputBox.ShowSingle(Utilities.ResourceResolver.Strings.QUERY)
                    If Not String.IsNullOrEmpty(query) Then
                        Configuration.SetStatus("Acquiring data...", 0)
                        Dim Session = E.Deezer.DeezerSession.CreateNew
                        Dim Result = Await Session.Search.Artists(query, 0, 1)
                        Dim Artist = Result?.FirstOrDefault
                        If Artist IsNot Nothing Then
                            Dim MDG As New Player.MetadataGroup() With {.Name = Artist.Name, .Category = "Deezer", .PlayCount = Convert.ToInt32(Artist.Fans)}
                            'Picture                    
                            Dim tUri As Uri = Nothing
                            If Artist.HasPicture(EDeezer.Api.PictureSize.Medium) Then
                                tUri = New Uri(Artist.GetPicture(EDeezer.Api.PictureSize.Medium))
                            ElseIf Artist.HasPicture(EDeezer.Api.PictureSize.Large) Then
                                tUri = New Uri(Artist.GetPicture(EDeezer.Api.PictureSize.Large))
                            ElseIf Artist.HasPicture(EDeezer.Api.PictureSize.Small) Then
                                tUri = New Uri(Artist.GetPicture(EDeezer.Api.PictureSize.Small))
                            ElseIf Artist.HasPicture(EDeezer.Api.PictureSize.ExtraLarge) Then
                                tUri = New Uri(Artist.GetPicture(EDeezer.Api.PictureSize.ExtraLarge))
                            End If
                            If tUri IsNot Nothing Then
                                Dim bi As New BitmapImage
                                bi.BeginInit()
                                bi.UriSource = tUri
                                bi.EndInit()
                                MDG.Cover = bi
                            End If
                            Dim ttl As New List(Of Tuple(Of String, String, EDeezer.Api.ITrack)) 'Title, Thumb, Track
                            Try
                                For Each album In Await Artist.GetAlbums(aCount:=ARTIST_ALBUM_LIMIT)
                                    Dim url As String = Nothing
                                    If album.HasPicture(EDeezer.Api.PictureSize.Medium) Then
                                        url = album.GetPicture(EDeezer.Api.PictureSize.Medium)
                                    ElseIf album.HasPicture(EDeezer.Api.PictureSize.Large) Then
                                        url = album.GetPicture(EDeezer.Api.PictureSize.Large)
                                    ElseIf album.HasPicture(EDeezer.Api.PictureSize.Small) Then
                                        url = album.GetPicture(EDeezer.Api.PictureSize.Small)
                                    ElseIf album.HasPicture(EDeezer.Api.PictureSize.ExtraLarge) Then
                                        url = album.GetPicture(EDeezer.Api.PictureSize.ExtraLarge)
                                    End If
                                    For Each track In Await album.GetTracks()
                                        ttl.Add(New Tuple(Of String, String, EDeezer.Api.ITrack)(album.Title, url, track))
                                    Next
                                Next
                            Catch ex As Exception
                                Utilities.DebugMode.Instance.Log(Of BrowseCommand)(ex.ToString)
                            End Try
                            Configuration.SetStatus("Decoding...", 50)
                            For Each rTr In ttl
                                If String.IsNullOrEmpty(rTr.Item3.Preview) Then Continue For
                                'Dim tr = Await Session.Browse.GetTrackById(rTr.Id)
                                Dim MD As New Player.Metadata() With {.Location = Player.Metadata.FileLocation.Remote, .OriginalPath = rTr.Item3.Link, .Provider = New DeezerMediaProvider(rTr.Item3.Id), .BlockDOWNLOADPROC = True, .Album = rTr.Item1, .Artists = New String() {rTr.Item3.ArtistName}, .PlayCount = rTr.Item3.Rank, .Index = rTr.Item3.Number, .Title = rTr.Item3.Title, .Length = rTr.Item3.Duration, .Path = rTr.Item3.Preview}
                                Dim uri As Uri = If(String.IsNullOrEmpty(rTr.Item2), Nothing, New Uri(rTr.Item2))
                                If uri IsNot Nothing Then
                                    Dim bi As New BitmapImage
                                    bi.BeginInit()
                                    bi.UriSource = uri
                                    bi.EndInit()
                                    MD.Covers = New ImageSource() {bi}
                                End If
                                MDG.Add(MD)
                            Next
                            Configuration.SetStatus("Routing Data...", 80)
                            SharedProperties.Instance.Library?.FocusGroup(MDG)
                        End If
                        Session?.Dispose()
                    End If
                Case "search/album"
                    Dim query = Dialogs.InputBox.ShowSingle(Utilities.ResourceResolver.Strings.QUERY)
                    If Not String.IsNullOrEmpty(query) Then
                        Configuration.SetStatus("Acquiring data...", 0)
                        Dim Session = E.Deezer.DeezerSession.CreateNew
                        Dim Result = Await Session.Search.Albums(query, 0, 1)
                        Dim Album = Result?.FirstOrDefault
                        If Album IsNot Nothing Then
                            Dim MDG As New Player.MetadataGroup() With {.Name = Album.Title, .Category = "Deezer", .PlayCount = Convert.ToInt32(Album.Fans)}
                            'Picture                    
                            Dim tUri As Uri = Nothing
                            If Album.HasPicture(EDeezer.Api.PictureSize.Medium) Then
                                tUri = New Uri(Album.GetPicture(EDeezer.Api.PictureSize.Medium))
                            ElseIf Album.HasPicture(EDeezer.Api.PictureSize.Large) Then
                                tUri = New Uri(Album.GetPicture(EDeezer.Api.PictureSize.Large))
                            ElseIf Album.HasPicture(EDeezer.Api.PictureSize.Small) Then
                                tUri = New Uri(Album.GetPicture(EDeezer.Api.PictureSize.Small))
                            ElseIf Album.HasPicture(EDeezer.Api.PictureSize.ExtraLarge) Then
                                tUri = New Uri(Album.GetPicture(EDeezer.Api.PictureSize.ExtraLarge))
                            End If
                            If tUri IsNot Nothing Then
                                Dim bi As New BitmapImage
                                bi.BeginInit()
                                bi.UriSource = tUri
                                bi.EndInit()
                                MDG.Cover = bi
                            End If
                            Dim ttl = Await Album.GetTracks()
                            Configuration.SetStatus("Decoding...", 50)
                            Dim url As String = Nothing
                            If Album.HasPicture(EDeezer.Api.PictureSize.Medium) Then
                                url = Album.GetPicture(EDeezer.Api.PictureSize.Medium)
                            ElseIf Album.HasPicture(EDeezer.Api.PictureSize.Large) Then
                                url = Album.GetPicture(EDeezer.Api.PictureSize.Large)
                            ElseIf Album.HasPicture(EDeezer.Api.PictureSize.Small) Then
                                url = Album.GetPicture(EDeezer.Api.PictureSize.Small)
                            ElseIf Album.HasPicture(EDeezer.Api.PictureSize.ExtraLarge) Then
                                url = Album.GetPicture(EDeezer.Api.PictureSize.ExtraLarge)
                            End If
                            Dim uri As Uri = If(String.IsNullOrEmpty(url), Nothing, New Uri(url))
                            Dim AlbumThumb As ImageSource = Nothing
                            If uri IsNot Nothing Then
                                Dim bi As New BitmapImage
                                bi.BeginInit()
                                bi.UriSource = uri
                                bi.EndInit()
                                AlbumThumb = bi
                            End If
                            For Each rTr In ttl
                                If String.IsNullOrEmpty(rTr.Preview) Then Continue For
                                Dim MD As New Player.Metadata() With {.Location = Player.Metadata.FileLocation.Remote, .OriginalPath = rTr.Link, .Provider = New DeezerMediaProvider(rTr.Id), .BlockDOWNLOADPROC = True, .Album = Album.Title, .Artists = New String() {rTr.ArtistName}, .PlayCount = rTr.Rank, .Index = rTr.Number, .Title = rTr.Title, .Length = rTr.Duration, .Path = rTr.Preview}
                                If AlbumThumb IsNot Nothing Then MD.Covers = New ImageSource() {AlbumThumb}
                                MDG.Add(MD)
                            Next
                            Configuration.SetStatus("Routing Data...", 80)
                            SharedProperties.Instance.Library?.FocusGroup(MDG)
                        End If
                        Session?.Dispose()
                    End If
                Case "search/playlist"
                    Dim query = Dialogs.InputBox.ShowSingle(Utilities.ResourceResolver.Strings.QUERY)
                    If Not String.IsNullOrEmpty(query) Then
                        Configuration.SetStatus("Acquiring data...", 0)
                        Dim Session = E.Deezer.DeezerSession.CreateNew
                        Dim Result = Await Session.Search.Playlists(query, 0, 1)
                        Dim Playlist = Result?.FirstOrDefault
                        If Playlist IsNot Nothing Then
                            Dim MDG As New Player.MetadataGroup() With {.Name = Playlist.Title, .Category = "Deezer", .PlayCount = Convert.ToInt32(Playlist.Fans)}
                            'Picture                    
                            Dim tUri As Uri = Nothing
                            If Playlist.HasPicture(EDeezer.Api.PictureSize.Medium) Then
                                tUri = New Uri(Playlist.GetPicture(EDeezer.Api.PictureSize.Medium))
                            ElseIf Playlist.HasPicture(EDeezer.Api.PictureSize.Large) Then
                                tUri = New Uri(Playlist.GetPicture(EDeezer.Api.PictureSize.Large))
                            ElseIf Playlist.HasPicture(EDeezer.Api.PictureSize.Small) Then
                                tUri = New Uri(Playlist.GetPicture(EDeezer.Api.PictureSize.Small))
                            ElseIf Playlist.HasPicture(EDeezer.Api.PictureSize.ExtraLarge) Then
                                tUri = New Uri(Playlist.GetPicture(EDeezer.Api.PictureSize.ExtraLarge))
                            End If
                            If tUri IsNot Nothing Then
                                Dim bi As New BitmapImage
                                bi.BeginInit()
                                bi.UriSource = tUri
                                bi.EndInit()
                                MDG.Cover = bi
                            End If
                            Dim ttl = Await Playlist.GetTracks(aCount:=PLAYLIST_LIMIT)
                            Configuration.SetStatus("Decoding...", 50)
                            For Each rTr In ttl
                                If String.IsNullOrEmpty(rTr.Preview) Then Continue For
                                Dim MD As New Player.Metadata() With {.Location = Player.Metadata.FileLocation.Remote, .OriginalPath = rTr.Link, .Provider = New DeezerMediaProvider(rTr.Id), .BlockDOWNLOADPROC = True, .Album = rTr.AlbumName, .Artists = New String() {rTr.ArtistName}, .PlayCount = rTr.Rank, .Index = rTr.Number, .Title = rTr.Title, .Length = rTr.Duration, .Path = rTr.Preview}
                                MD.Covers = New ImageSource() {MDG.Cover}
                                MDG.Add(MD)
                            Next
                            Configuration.SetStatus("Routing Data...", 80)
                            SharedProperties.Instance.Library?.FocusGroup(MDG)
                        End If
                        Session?.Dispose()
                    End If
                Case "search/radio"
                    Dim query = Dialogs.InputBox.ShowSingle(Utilities.ResourceResolver.Strings.QUERY)
                    If Not String.IsNullOrEmpty(query) Then
                        Configuration.SetStatus("Acquiring data...", 0)
                        Dim Session = E.Deezer.DeezerSession.CreateNew
                        Dim Result = Await Session.Search.Radio(query, 0, 1)
                        Dim Radio = Result?.FirstOrDefault
                        If Radio IsNot Nothing Then
                            Dim MDG As New Player.MetadataGroup() With {.Name = Radio.Title & " Radio", .Category = "Deezer"}
                            'Picture                    
                            Dim tUri As Uri = Nothing
                            If Radio.HasPicture(EDeezer.Api.PictureSize.Medium) Then
                                tUri = New Uri(Radio.GetPicture(EDeezer.Api.PictureSize.Medium))
                            ElseIf Radio.HasPicture(EDeezer.Api.PictureSize.Large) Then
                                tUri = New Uri(Radio.GetPicture(EDeezer.Api.PictureSize.Large))
                            ElseIf Radio.HasPicture(EDeezer.Api.PictureSize.Small) Then
                                tUri = New Uri(Radio.GetPicture(EDeezer.Api.PictureSize.Small))
                            ElseIf Radio.HasPicture(EDeezer.Api.PictureSize.ExtraLarge) Then
                                tUri = New Uri(Radio.GetPicture(EDeezer.Api.PictureSize.ExtraLarge))
                            End If
                            If tUri IsNot Nothing Then
                                Dim bi As New BitmapImage
                                bi.BeginInit()
                                bi.UriSource = tUri
                                bi.EndInit()
                                MDG.Cover = bi
                            End If
                            Dim ttl = Await Radio.GetTracks(aCount:=ARTIST_RADIO_LIMIT)
                            Configuration.SetStatus("Decoding...", 50)
                            For Each rTr In ttl
                                If String.IsNullOrEmpty(rTr.Preview) Then Continue For
                                Dim MD As New Player.Metadata() With {.Location = Player.Metadata.FileLocation.Remote, .OriginalPath = rTr.Link, .Provider = New DeezerMediaProvider(rTr.Id), .BlockDOWNLOADPROC = True, .Album = rTr.AlbumName, .Artists = New String() {"Deezer"}, .PlayCount = rTr.Rank, .Index = rTr.Number, .Title = rTr.Title, .Length = rTr.Duration, .Path = rTr.Preview}
                                MD.Covers = New ImageSource() {MDG.Cover}
                                MDG.Add(MD)
                            Next
                            Configuration.SetStatus("Routing Data...", 80)
                            SharedProperties.Instance.Library?.FocusGroup(MDG)
                        End If
                        Session?.Dispose()
                    End If
                Case "artist/top"
                    Dim query = Dialogs.InputBox.ShowSingle("ID")
                    Dim iQuery As Integer = 0
                    If Not String.IsNullOrEmpty(query) AndAlso UInteger.TryParse(query, iQuery) Then
                        Configuration.SetStatus("Acquiring data...", 0)
                        Dim Session = EDeezer.DeezerSession.CreateNew
                        Dim Artist = Await Session.Browse.GetArtistById(iQuery)
                        If Artist IsNot Nothing Then
                            Dim MDG As New Player.MetadataGroup() With {.Name = Artist.Name, .Category = "Deezer", .PlayCount = Convert.ToInt32(Artist.Fans)}
                            'Picture                    
                            Dim tUri As Uri = Nothing
                            If Artist.HasPicture(EDeezer.Api.PictureSize.Medium) Then
                                tUri = New Uri(Artist.GetPicture(EDeezer.Api.PictureSize.Medium))
                            ElseIf Artist.HasPicture(EDeezer.Api.PictureSize.Large) Then
                                tUri = New Uri(Artist.GetPicture(EDeezer.Api.PictureSize.Large))
                            ElseIf Artist.HasPicture(EDeezer.Api.PictureSize.Small) Then
                                tUri = New Uri(Artist.GetPicture(EDeezer.Api.PictureSize.Small))
                            ElseIf Artist.HasPicture(EDeezer.Api.PictureSize.ExtraLarge) Then
                                tUri = New Uri(Artist.GetPicture(EDeezer.Api.PictureSize.ExtraLarge))
                            End If
                            If tUri IsNot Nothing Then
                                Dim bi As New BitmapImage
                                bi.BeginInit()
                                bi.UriSource = tUri
                                bi.EndInit()
                                MDG.Cover = bi
                            End If
                            Dim ttl = Await Artist.GetTopTracks(aCount:=ARTIST_TOP_LIMIT)
                            Configuration.SetStatus("Decoding...", 50)
                            If ttl.Count = 0 Then
                                IsBusy = False
                                Return
                            End If
                            For Each rTr In ttl
                                If String.IsNullOrEmpty(rTr.Preview) Then Continue For
                                Dim MD As New Player.Metadata() With {.Location = Player.Metadata.FileLocation.Remote, .OriginalPath = rTr.Link, .Provider = New DeezerMediaProvider(rTr.Id), .BlockDOWNLOADPROC = True, .Album = rTr.AlbumName, .Artists = New String() {rTr.ArtistName}, .PlayCount = rTr.Rank, .Index = rTr.Number, .Title = rTr.Title, .Length = rTr.Duration, .Path = rTr.Preview}
                                MD.Covers = New ImageSource() {MDG.Cover}
                                MDG.Add(MD)
                            Next
                            Configuration.SetStatus("Routing Data...", 80)
                            SharedProperties.Instance.Library?.FocusGroup(MDG)
                        End If
                        Session?.Dispose()
                    End If
                Case "artist/radio"
                    Dim query = Dialogs.InputBox.ShowSingle("ID")
                    Dim iQuery As Integer = 0
                    If Not String.IsNullOrEmpty(query) AndAlso UInteger.TryParse(query, iQuery) Then
                        Configuration.SetStatus("Acquiring data...", 0)
                        Dim Session = EDeezer.DeezerSession.CreateNew
                        Dim Artist = Await Session.Browse.GetArtistById(iQuery)
                        If Artist IsNot Nothing Then
                            Dim MDG As New Player.MetadataGroup() With {.Name = Artist.Name & " Radio", .Category = "Deezer", .PlayCount = Convert.ToInt32(Artist.Fans)}
                            'Picture                    
                            Dim tUri As Uri = Nothing
                            If Artist.HasPicture(EDeezer.Api.PictureSize.Medium) Then
                                tUri = New Uri(Artist.GetPicture(EDeezer.Api.PictureSize.Medium))
                            ElseIf Artist.HasPicture(EDeezer.Api.PictureSize.Large) Then
                                tUri = New Uri(Artist.GetPicture(EDeezer.Api.PictureSize.Large))
                            ElseIf Artist.HasPicture(EDeezer.Api.PictureSize.Small) Then
                                tUri = New Uri(Artist.GetPicture(EDeezer.Api.PictureSize.Small))
                            ElseIf Artist.HasPicture(EDeezer.Api.PictureSize.ExtraLarge) Then
                                tUri = New Uri(Artist.GetPicture(EDeezer.Api.PictureSize.ExtraLarge))
                            End If
                            If tUri IsNot Nothing Then
                                Dim bi As New BitmapImage
                                bi.BeginInit()
                                bi.UriSource = tUri
                                bi.EndInit()
                                MDG.Cover = bi
                            End If
                            Dim ttl = Await Artist.GetSmartRadio(aCount:=ARTIST_RADIO_LIMIT)
                            Configuration.SetStatus("Decoding...", 50)
                            If ttl.Count = 0 Then
                                HandyControl.Controls.MessageBox.Error("No Radio Found!")
                                IsBusy = False
                                Return
                            End If
                            For Each rTr In ttl
                                If String.IsNullOrEmpty(rTr.Preview) Then Continue For
                                Dim MD As New Player.Metadata() With {.Location = Player.Metadata.FileLocation.Remote, .OriginalPath = If(String.IsNullOrEmpty(rTr.Link), Artist.Link, rTr.Link), .Provider = New DeezerMediaProvider(rTr.Id), .BlockDOWNLOADPROC = True, .Album = rTr.AlbumName, .Artists = New String() {rTr.ArtistName}, .PlayCount = rTr.Rank, .Index = rTr.Number, .Title = rTr.Title, .Length = rTr.Duration, .Path = rTr.Preview}
                                MD.Covers = New ImageSource() {MDG.Cover}
                                MDG.Add(MD)
                            Next
                            Configuration.SetStatus("Routing Data...", 80)
                            SharedProperties.Instance.Library?.FocusGroup(MDG)
                        End If
                        Session?.Dispose()
                    End If
                Case "track"
                    Dim query = Dialogs.InputBox.ShowSingle("ID")
                    Dim iQuery As Integer = 0
                    If Not String.IsNullOrEmpty(query) AndAlso UInteger.TryParse(query, iQuery) Then
                        Configuration.SetStatus("Acquiring data...", 0)
                        Dim Session = E.Deezer.DeezerSession.CreateNew
                        Dim tr = Await Session.Browse.GetTrackById(iQuery)
                        If tr IsNot Nothing Then
                            Configuration.SetStatus("Decoding...", 50)
                            Dim MD As New Player.Metadata() With {.Location = Player.Metadata.FileLocation.Remote, .OriginalPath = tr.Link, .Provider = New DeezerMediaProvider(tr.Id), .BlockDOWNLOADPROC = True, .Album = tr.AlbumName, .Artists = New String() {tr.ArtistName}, .PlayCount = tr.Rank, .Index = tr.Number, .Title = tr.Title, .Length = tr.Duration, .Path = tr.Preview}
                            Dim url = If(tr.HasPicture(E.Deezer.Api.PictureSize.Medium), tr.GetPicture(E.Deezer.Api.PictureSize.Medium), If(tr.HasPicture(EDeezer.Api.PictureSize.Large), tr.GetPicture(EDeezer.Api.PictureSize.Large), If(tr.HasPicture(EDeezer.Api.PictureSize.Small), tr.GetPicture(EDeezer.Api.PictureSize.Small), If(tr.HasPicture(EDeezer.Api.PictureSize.ExtraLarge), tr.GetPicture(EDeezer.Api.PictureSize.ExtraLarge), Nothing))))
                            Dim bi As New BitmapImage
                            bi.BeginInit()
                            bi.UriSource = New Uri(url)
                            bi.EndInit()
                            MD.Covers = New ImageSource() {bi}
                            Configuration.SetStatus("Routing Data...", 80)
                            If Preview Then
                                SharedProperties.Instance.Player?.Preview(MD)
                            Else
                                SharedProperties.Instance.Player?.LoadAndAddCommand.Execute(MD)
                            End If
                        End If
                        Session?.Dispose()
                    End If
                Case "album"
                    Dim query = Dialogs.InputBox.ShowSingle("ID")
                    Dim iQuery As Integer = 0
                    If Not String.IsNullOrEmpty(query) AndAlso UInteger.TryParse(query, iQuery) Then
                        Configuration.SetStatus("Acquiring data...", 0)
                        Dim Session = EDeezer.DeezerSession.CreateNew
                        Dim Album = Await Session.Browse.GetAlbumById(iQuery)
                        If Album IsNot Nothing Then
                            Dim MDG As New Player.MetadataGroup() With {.Name = Album.Title, .Category = "Deezer", .PlayCount = Convert.ToInt32(Album.Fans)}
                            'Picture                    
                            Dim tUri As Uri = Nothing
                            If Album.HasPicture(EDeezer.Api.PictureSize.Medium) Then
                                tUri = New Uri(Album.GetPicture(EDeezer.Api.PictureSize.Medium))
                            ElseIf Album.HasPicture(EDeezer.Api.PictureSize.Large) Then
                                tUri = New Uri(Album.GetPicture(EDeezer.Api.PictureSize.Large))
                            ElseIf Album.HasPicture(EDeezer.Api.PictureSize.Small) Then
                                tUri = New Uri(Album.GetPicture(EDeezer.Api.PictureSize.Small))
                            ElseIf Album.HasPicture(EDeezer.Api.PictureSize.ExtraLarge) Then
                                tUri = New Uri(Album.GetPicture(EDeezer.Api.PictureSize.ExtraLarge))
                            End If
                            If tUri IsNot Nothing Then
                                Dim bi As New BitmapImage
                                bi.BeginInit()
                                bi.UriSource = tUri
                                bi.EndInit()
                                MDG.Cover = bi
                            End If
                            Dim ttl = Await Album.GetTracks()
                            Configuration.SetStatus("Decoding...", 50)
                            Dim url As String = Nothing
                            If Album.HasPicture(EDeezer.Api.PictureSize.Medium) Then
                                url = Album.GetPicture(EDeezer.Api.PictureSize.Medium)
                            ElseIf Album.HasPicture(EDeezer.Api.PictureSize.Large) Then
                                url = Album.GetPicture(EDeezer.Api.PictureSize.Large)
                            ElseIf Album.HasPicture(EDeezer.Api.PictureSize.Small) Then
                                url = Album.GetPicture(EDeezer.Api.PictureSize.Small)
                            ElseIf Album.HasPicture(EDeezer.Api.PictureSize.ExtraLarge) Then
                                url = Album.GetPicture(EDeezer.Api.PictureSize.ExtraLarge)
                            End If
                            Dim uri As Uri = If(String.IsNullOrEmpty(url), Nothing, New Uri(url))
                            Dim AlbumThumb As ImageSource = Nothing
                            If uri IsNot Nothing Then
                                Dim bi As New BitmapImage
                                bi.BeginInit()
                                bi.UriSource = uri
                                bi.EndInit()
                                AlbumThumb = bi
                            End If
                            For Each rTr In ttl
                                If String.IsNullOrEmpty(rTr.Preview) Then Continue For
                                Dim MD As New Player.Metadata() With {.Location = Player.Metadata.FileLocation.Remote, .OriginalPath = rTr.Link, .Provider = New DeezerMediaProvider(rTr.Id), .BlockDOWNLOADPROC = True, .Album = Album.Title, .Artists = New String() {rTr.ArtistName}, .PlayCount = rTr.Rank, .Index = rTr.Number, .Title = rTr.Title, .Length = rTr.Duration, .Path = rTr.Preview}
                                If AlbumThumb IsNot Nothing Then MD.Covers = New ImageSource() {AlbumThumb}
                                MDG.Add(MD)
                            Next
                            Configuration.SetStatus("Routing Data...", 80)
                            SharedProperties.Instance.Library?.FocusGroup(MDG)
                        End If
                        Session?.Dispose()
                    End If
                Case "playlist"
                    Dim query = Dialogs.InputBox.ShowSingle("ID")
                    Dim iQuery As Integer = 0
                    If Not String.IsNullOrEmpty(query) AndAlso UInteger.TryParse(query, iQuery) Then
                        Configuration.SetStatus("Acquiring data...", 0)
                        Dim Session = EDeezer.DeezerSession.CreateNew
                        Dim Playlist = Await Session.Browse.GetPlaylistById(iQuery)
                        If Playlist IsNot Nothing Then
                            Dim MDG As New Player.MetadataGroup() With {.Name = Playlist.Title, .Category = "Deezer", .PlayCount = Convert.ToInt32(Playlist.Fans)}
                            'Picture                    
                            Dim tUri As Uri = Nothing
                            If Playlist.HasPicture(EDeezer.Api.PictureSize.Medium) Then
                                tUri = New Uri(Playlist.GetPicture(EDeezer.Api.PictureSize.Medium))
                            ElseIf Playlist.HasPicture(EDeezer.Api.PictureSize.Large) Then
                                tUri = New Uri(Playlist.GetPicture(EDeezer.Api.PictureSize.Large))
                            ElseIf Playlist.HasPicture(EDeezer.Api.PictureSize.Small) Then
                                tUri = New Uri(Playlist.GetPicture(EDeezer.Api.PictureSize.Small))
                            ElseIf Playlist.HasPicture(EDeezer.Api.PictureSize.ExtraLarge) Then
                                tUri = New Uri(Playlist.GetPicture(EDeezer.Api.PictureSize.ExtraLarge))
                            End If
                            If tUri IsNot Nothing Then
                                Dim bi As New BitmapImage
                                bi.BeginInit()
                                bi.UriSource = tUri
                                bi.EndInit()
                                MDG.Cover = bi
                            End If
                            Dim ttl = Await Playlist.GetTracks(aCount:=PLAYLIST_LIMIT)
                            Configuration.SetStatus("Decoding...", 50)
                            For Each rTr In ttl
                                If String.IsNullOrEmpty(rTr.Preview) Then Continue For
                                Dim MD As New Player.Metadata() With {.Location = Player.Metadata.FileLocation.Remote, .OriginalPath = rTr.Link, .Provider = New DeezerMediaProvider(rTr.Id), .BlockDOWNLOADPROC = True, .Album = rTr.AlbumName, .Artists = New String() {rTr.ArtistName}, .PlayCount = rTr.Rank, .Index = rTr.Number, .Title = rTr.Title, .Length = rTr.Duration, .Path = rTr.Preview}
                                MD.Covers = New ImageSource() {MDG.Cover}
                                MDG.Add(MD)
                            Next
                            Configuration.SetStatus("Routing Data...", 80)
                            SharedProperties.Instance.Library?.FocusGroup(MDG)
                        End If
                        Session?.Dispose()
                    End If
                Case "radio"
                    Dim query = Dialogs.InputBox.ShowSingle("ID")
                    Dim iQuery As Integer = 0
                    If Not String.IsNullOrEmpty(query) AndAlso UInteger.TryParse(query, iQuery) Then
                        Configuration.SetStatus("Acquiring data...", 0)
                        Dim Session = EDeezer.DeezerSession.CreateNew
                        Dim Radio = Await Session.Browse.GetRadioById(iQuery)
                        If Radio IsNot Nothing Then
                            Dim MDG As New Player.MetadataGroup() With {.Name = Radio.Title, .Category = "Deezer"}
                            'Picture                    
                            Dim tUri As Uri = Nothing
                            If Radio.HasPicture(EDeezer.Api.PictureSize.Medium) Then
                                tUri = New Uri(Radio.GetPicture(EDeezer.Api.PictureSize.Medium))
                            ElseIf Radio.HasPicture(EDeezer.Api.PictureSize.Large) Then
                                tUri = New Uri(Radio.GetPicture(EDeezer.Api.PictureSize.Large))
                            ElseIf Radio.HasPicture(EDeezer.Api.PictureSize.Small) Then
                                tUri = New Uri(Radio.GetPicture(EDeezer.Api.PictureSize.Small))
                            ElseIf Radio.HasPicture(EDeezer.Api.PictureSize.ExtraLarge) Then
                                tUri = New Uri(Radio.GetPicture(EDeezer.Api.PictureSize.ExtraLarge))
                            End If
                            If tUri IsNot Nothing Then
                                Dim bi As New BitmapImage
                                bi.BeginInit()
                                bi.UriSource = tUri
                                bi.EndInit()
                                MDG.Cover = bi
                            End If
                            Dim ttl = Await Radio.GetTracks(aCount:=ARTIST_RADIO_LIMIT)
                            Configuration.SetStatus("Decoding...", 50)
                            For Each rTr In ttl
                                If String.IsNullOrEmpty(rTr.Preview) Then Continue For
                                Dim MD As New Player.Metadata() With {.Location = Player.Metadata.FileLocation.Remote, .OriginalPath = rTr.Link, .Provider = New DeezerMediaProvider(rTr.Id), .BlockDOWNLOADPROC = True, .Album = rTr.AlbumName, .Artists = New String() {"Deezer"}, .PlayCount = rTr.Rank, .Index = rTr.Number, .Title = rTr.Title, .Length = rTr.Duration, .Path = rTr.Preview}
                                MD.Covers = New ImageSource() {MDG.Cover}
                                MDG.Add(MD)
                            Next
                            Configuration.SetStatus("Routing Data...", 80)
                            SharedProperties.Instance.Library?.FocusGroup(MDG)
                        End If
                        Session?.Dispose()
                    End If
            End Select
            Configuration.SetStatus("All Good", 100)
            IsBusy = False
        End Sub

        Public Function CanExecute(parameter As Object) As Boolean Implements ICommand.CanExecute
            Return Utilities.SharedProperties.Instance.IsInternetConnected AndAlso Not String.IsNullOrEmpty(parameter)
        End Function

        Public Sub Init() Implements IStartupItem.Init
            Configuration.SetStatus("All Good", 100)
        End Sub
    End Class


End Namespace