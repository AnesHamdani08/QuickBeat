Imports System.Collections.ObjectModel
Imports QuickBeat.Hotkeys
Namespace Utilities.Collections
    Public Class MessagesReadOnlyObservableCollection
        Inherits ReadOnlyObservableCollection(Of HotkeyManager.Messages)

        Public Sub New()
            MyBase.New(New ObservableCollection(Of HotkeyManager.Messages)([Enum].GetValues(GetType(HotkeyManager.Messages))))
        End Sub
    End Class
End Namespace