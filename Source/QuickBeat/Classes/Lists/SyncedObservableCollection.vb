Imports System.Collections.ObjectModel
Imports System.ComponentModel

Namespace Utilities.Collections
    Public Class SyncedObservableCollection(Of T)
        Inherits ObservableCollection(Of T)

        Delegate Sub SyncStart(sender As Object, ModifiedItem As T, ChangeType As CollectionChangeAction)

        Private _Sync As SyncStart

        Sub New(Sync As SyncStart)
            _Sync = Sync
        End Sub

        Sub New(Sync As SyncStart, collection As IEnumerable(Of T))
            MyBase.New(collection)
            _Sync = Sync
        End Sub

        Sub New(Sync As SyncStart, list As IList(Of T))
            MyBase.New(list)
            _Sync = Sync
        End Sub

        Sub New(Sync As SyncStart, StringCollection As Specialized.StringCollection)
            _Sync = Sync
            If GetType(T) Is GetType(String) Then
                For Each child In StringCollection
                    Add((CTypeDynamic(Of T)(child)))
                Next
            End If
        End Sub

        Shadows Sub Add(item As T)
            MyBase.Add(item)
            _Sync?.Invoke(Me, item, CollectionChangeAction.Add)
        End Sub

        Shadows Function Remove(item As T) As Boolean
            If MyBase.Remove(item) Then
                _Sync?.Invoke(item, item, CollectionChangeAction.Remove)
                Return True
            End If
            Return False
        End Function

        Shadows Sub RemoveAt(index As Integer)
            _Sync?.Invoke(Me, Item(index), CollectionChangeAction.Remove)
            MyBase.RemoveAt(index)
        End Sub

        Shadows Sub Clear()
            MyBase.Clear()
            _Sync?.Invoke(Me, Nothing, CollectionChangeAction.Refresh)
        End Sub
    End Class
End Namespace