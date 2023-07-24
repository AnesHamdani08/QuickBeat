Imports System.ComponentModel
Imports System.Security.Policy
Imports System.Threading
Imports HandyControl.Controls
Imports HandyControl.Tools.Command
Imports QuickBeat.Player
Imports QuickBeat.Hotkeys.WPF.Commands
Imports QuickBeat.Library.WPF.Commands
Imports QuickBeat.Library
Imports QuickBeat.Utilities
Imports QuickBeat.Player.WPF.Commands
Imports QuickBeat.WPF.Commands

Namespace Utilities

    Public Class Commands
        Public Shared LibraryScanCommandX = AsyncCommand.Create(Of Library.Library)(New Func(Of Library.Library, CancellationToken, Task)(Function(library, token)
                                                                                                                                              If SharedProperties.Instance.IsLogging Then Utilities.DebugMode.Instance.Log(Of Commands)("Attempting to scan...")
                                                                                                                                              If library Is Nothing Then
                                                                                                                                                  If SharedProperties.Instance.IsLogging Then Utilities.DebugMode.Instance.Log(Of Commands)("Scan failed, Error:=null object")
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
                                                                                                                                              If SharedProperties.Instance.IsLogging Then Utilities.DebugMode.Instance.Log(Of Commands)("Found " & TBA.Count & " Files to be added. Adding...")
                                                                                                                                              For Each file In TBA
                                                                                                                                                  library.Add(Metadata.FromFile(file, True))
                                                                                                                                                  If token.IsCancellationRequested Then
                                                                                                                                                      If SharedProperties.Instance.IsLogging Then Utilities.DebugMode.Instance.Log(Of Commands)("Scan canceled.")
                                                                                                                                                      Return Task.FromResult(Of Boolean)(False)
                                                                                                                                                  End If
                                                                                                                                              Next
                                                                                                                                              Dim i = 0
                                                                                                                                              If SharedProperties.Instance.IsLogging Then Utilities.DebugMode.Instance.Log(Of Commands)("Found " & TBR.Count & " Files to be removed. Removing...")
                                                                                                                                              For Each file In TBR
                                                                                                                                                  Dim TBRi = library.FirstOrDefault(Function(k) k.Path = file)
                                                                                                                                                  If TBRi Is Nothing Then
                                                                                                                                                      i += 1
                                                                                                                                                      Continue For
                                                                                                                                                  End If
                                                                                                                                                  library.Remove(TBRi)
                                                                                                                                                  If token.IsCancellationRequested Then
                                                                                                                                                      If SharedProperties.Instance.IsLogging Then Utilities.DebugMode.Instance.Log(Of Commands)("Scan canceled.")
                                                                                                                                                      Return Task.FromResult(Of Boolean)(False)
                                                                                                                                                  End If
                                                                                                                                              Next
                                                                                                                                              If SharedProperties.Instance.IsLogging AndAlso i > 0 Then Utilities.DebugMode.Instance.Log(Of Commands)(i & " Files failed to be removed.")
                                                                                                                                              If i > 0 Then SharedProperties.Instance.Notification(ResourceResolver.Strings.LIBRARY, ResourceResolver.Strings.TEXT_LIBRARY_SCAN_RESULT_REMOVE_FAILURE.Replace("%c0", i), HandyControl.Data.NotifyIconInfoType.Error)
                                                                                                                                              Return Task.FromResult(Of Boolean)(True)
                                                                                                                                          End Function))

        Public Shared LibraryRebuildCommandX = AsyncCommand.Create(Of Library.Library)(New Func(Of Library.Library, CancellationToken, Task)(Function(library, token)
                                                                                                                                                 If library Is Nothing Then Return Task.FromResult(Of Boolean)(False)
                                                                                                                                                 If SharedProperties.Instance.IsLogging Then Utilities.DebugMode.Instance.Log(Of Commands)("Rebuilding library...")
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
                                                                                                                                                 If SharedProperties.Instance.IsLogging Then Utilities.DebugMode.Instance.Log(Of Commands)("Library rebuilt successfuly.")
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

        Public Shared ViewImageCommand As New ViewImageCommand

        Public Shared ShowTagEditorCommand As New ShowTagEditorCommand

        Public Shared LoadResourceDictionaryCommand As New LoadResourceDictionaryCommand

        Public Shared MergeResourceDictionaryCommand As New MergeResourceDictionaryCommand

        Public Shared ClearMergedResourceDictionariesCommand As New ClearMergedResourceDictionariesCommand

        Public Shared RemoveMergedResourceDictionaryCommand As New RemoveMergedResourceDictionaryCommand

        Public Shared RestoreResourcesCommand As New RestoreResourcesCommand
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
            If SharedProperties.Instance.IsLogging Then Utilities.DebugMode.Instance.Log(Of Utilities.Commands)("Attempting to scan...")
            IsScanning = True 'Block incoming calls until we're done with this one
            If parameter Is Nothing Then
                If SharedProperties.Instance.IsLogging Then Utilities.DebugMode.Instance.Log(Of Utilities.Commands)("Scan failed, Error:=null object")
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
                                                                         If SharedProperties.Instance.IsLogging Then Utilities.DebugMode.Instance.Log(Of Utilities.Commands)("Found " & TBA.Count & " Files to be added. Adding...")
                                                                     End Sub)
                               For Each file In TBA
                                   Dim meta = Metadata.FromFile(file, True)
                                   Application.Current.Dispatcher.Invoke(Sub()
                                                                             CType(parameter, Library).Add(meta)
                                                                         End Sub)
                                   If StopScanning Then
                                       If SharedProperties.Instance.IsLogging Then Utilities.DebugMode.Instance.Log(Of Utilities.Commands)("Scan canceled at adding phase.")
                                       IsScanning = False
                                       StopScanning = False
                                       Return
                                   End If
                               Next
                               Dim i = 0
                               Application.Current.Dispatcher.Invoke(Sub()
                                                                         If SharedProperties.Instance.IsLogging Then Utilities.DebugMode.Instance.Log(Of Utilities.Commands)("Found " & TBR.Count & " Files to be removed. Removing...")
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
                                       If SharedProperties.Instance.IsLogging Then Utilities.DebugMode.Instance.Log(Of Utilities.Commands)("Scan canceled as removing phase.")
                                       IsScanning = False
                                       StopScanning = False
                                       Return
                                   End If
                               Loop
                               If SharedProperties.Instance.IsLogging AndAlso i > 0 Then Utilities.DebugMode.Instance.Log(Of Utilities.Commands)(i & " Files failed to be removed.")
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
            If SharedProperties.Instance.IsLogging Then Utilities.DebugMode.Instance.Log(Of Utilities.Commands)("Rebuilding ctype(parameter,Library)...")
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
            If SharedProperties.Instance.IsLogging Then Utilities.DebugMode.Instance.Log(Of Utilities.Commands)("Library rebuilt successfuly.")
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
            If SharedProperties.Instance.IsLogging Then Utilities.DebugMode.Instance.Log(Of Utilities.Commands)("Attempting to scan...")
            IsScanning = True 'Block incoming calls until we're done with this one
            If parameter Is Nothing Then
                If SharedProperties.Instance.IsLogging Then Utilities.DebugMode.Instance.Log(Of Utilities.Commands)("Tag refresh failed, Error:=null object")
                IsScanning = False
                StopScanning = False
                Return
            End If
            Await Task.Run(Sub()
                               For Each file In CType(parameter, Library)
                                   file.RefreshTagsFromFile_ThreadSafe(True)
                                   If StopScanning Then
                                       If SharedProperties.Instance.IsLogging Then Utilities.DebugMode.Instance.Log(Of Utilities.Commands)("Tag refresh canceled.")
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
            Dim Popup As New HandyControl.Controls.BlurWindow() With {.Content = IV, .Width = 500, .Height = 500, .Background = Nothing, .WindowStartupLocation = WindowStartupLocation.CenterScreen}
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
End Namespace