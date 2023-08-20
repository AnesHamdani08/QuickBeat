Imports System.Collections.Specialized
Imports System.Globalization

Namespace Player
    <Serializable>
    Public Class Playlist
        Inherits ObjectModel.ObservableCollection(Of Metadata)

        Public Enum AddBehaviour
            First
            Last
        End Enum

        Private _Index As Integer = -1
        Property Index As Integer
            Get
                Return _Index
            End Get
            Set(value As Integer)
                If value = _Index Then Return
                If value < 0 OrElse value >= Count Then Return
                _Index = value
                If Parent?.Path = Item(value)?.Path Then Return
                If IsShuffling Then 'Update shuffle index to the new selected value
                    _ShuffleIndexListIndex = _ShuffledIndexList.IndexOf(value)
                End If
#Disable Warning
                If AutoPlay Then Parent.LoadSong(Item(value))
#Enable Warning
                OnPropertyChanged(New ComponentModel.PropertyChangedEventArgs(NameOf(Index)))
                OnPropertyChanged(New ComponentModel.PropertyChangedEventArgs(NameOf(ActualIndex)))
                OnPropertyChanged(New ComponentModel.PropertyChangedEventArgs(NameOf(CurrentItem)))
                OnPropertyChanged(New ComponentModel.PropertyChangedEventArgs(NameOf(NextItem)))
                OnPropertyChanged(New ComponentModel.PropertyChangedEventArgs(NameOf(PreviousItem)))
            End Set
        End Property

        ''' <summary>
        ''' Return a one-based index of the current playlist (i.e <see cref="Index"/> + 1)
        ''' </summary>
        ''' <returns></returns>
        ReadOnly Property ActualIndex As Integer
            Get
                Return Index + 1
            End Get
        End Property

        <NonSerialized> Private _Parent As Player
        Property Parent As Player
            Get
                Return _Parent
            End Get
            Set(value As Player)
                _Parent = value
                OnPropertyChanged(New ComponentModel.PropertyChangedEventArgs(NameOf(Parent)))
            End Set
        End Property

        Private _Name As String = "Generic Playlist"
        Property Name As String
            Get
                Return _Name
            End Get
            Set(value As String)
                _Name = value
                OnPropertyChanged(New ComponentModel.PropertyChangedEventArgs(NameOf(Name)))
            End Set
        End Property

        Private _Description As String = "Generic Empty Playlist"
        Property Description As String
            Get
                Return _Description
            End Get
            Set(value As String)
                _Description = value
                OnPropertyChanged(New ComponentModel.PropertyChangedEventArgs(NameOf(Description)))
            End Set
        End Property

        Private _Category As String = "Generic Playlist"
        Property Category As String
            Get
                Return _Category
            End Get
            Set(value As String)
                _Category = value
                OnPropertyChanged(New ComponentModel.PropertyChangedEventArgs(NameOf(Category)))
            End Set
        End Property

        Property Tag As Object

        Private _TotalDuration As TimeSpan
        Property TotalDuration As TimeSpan
            Get
                Return _TotalDuration
            End Get
            Set(value As TimeSpan)
                _TotalDuration = value
                OnPropertyChanged(New ComponentModel.PropertyChangedEventArgs(NameOf(TotalDuration)))
                OnPropertyChanged(New ComponentModel.PropertyChangedEventArgs(NameOf(TotalDurationString)))
            End Set
        End Property

        ReadOnly Property TotalDurationString As String
            Get
                Return If(TotalDuration.Hours > 0, TotalDuration.ToString("hh\:mm\:ss"), TotalDuration.ToString("mm\:ss"))
            End Get
        End Property

        Private _AutoPlay As Boolean = True
        ''' <summary>
        ''' Wheter or not to invoke <see cref="Player.LoadSong(Metadata)"/> when <see cref="Index"/> or <see cref="QueueIndex"/> are changed.
        ''' </summary>
        ''' <returns></returns>
        Property AutoPlay As Boolean
            Get
                Return _AutoPlay
            End Get
            Set(value As Boolean)
                _AutoPlay = value
                OnPropertyChanged(New ComponentModel.PropertyChangedEventArgs(NameOf(AutoPlay)))
            End Set
        End Property

        Private _IsLooping As Boolean = False
        Property IsLooping As Boolean
            Get
                Return _IsLooping
            End Get
            Set(value As Boolean)
                _IsLooping = value
                OnPropertyChanged(New ComponentModel.PropertyChangedEventArgs(NameOf(IsLooping)))
            End Set
        End Property

        Private _ShuffledIndexList As New List(Of Integer)
        Private _ShuffleIndexListIndex As Integer = 0

        Private _IsShuffling As Boolean
        Property IsShuffling As Boolean
            Get
                Return _IsShuffling
            End Get
            Set(value As Boolean)
                _IsShuffling = value
                If value Then
                    Shuffle()
                Else
                    _ShuffledIndexList.Clear()
                End If
                OnPropertyChanged(New ComponentModel.PropertyChangedEventArgs(NameOf(IsShuffling)))
            End Set
        End Property

        Private _SongAddBehaviour As AddBehaviour
        Property SongAddBehaviour As AddBehaviour
            Get
                Return _SongAddBehaviour
            End Get
            Set(value As AddBehaviour)
                _SongAddBehaviour = value
                OnPropertyChanged(New ComponentModel.PropertyChangedEventArgs(NameOf(SongAddBehaviour)))
            End Set
        End Property

        ReadOnly Property CurrentItem As Metadata
            Get
                Return If(QueueIndex = -1, If(Index > -1 AndAlso Index < Count, Item(Index), Nothing), Queue(QueueIndex))
            End Get
        End Property

        ReadOnly Property NextItem() As Metadata
            Get
                Return [Next](False, False)
            End Get
        End Property

        ReadOnly Property PreviousItem() As Metadata
            Get
                Return Previous(False, False)
            End Get
        End Property

        Private _Queue As New ObjectModel.ObservableCollection(Of Metadata)
        ReadOnly Property Queue As ObjectModel.ObservableCollection(Of Metadata)
            Get
                Return _Queue
            End Get
        End Property

        Private _QueueIndexSkipNext As Boolean = False
        Private _QueueIndex As Integer = -1
        Property QueueIndex As Integer
            Get
                Return _QueueIndex
            End Get
            Set(value As Integer)
                If _QueueIndexSkipNext Then _QueueIndexSkipNext = False : Return
                If value < 0 OrElse value >= Queue.Count Then Return
                _QueueIndex = value
                If value > 0 Then
                    Do While _QueueIndex > 0
                        If Queue.Count = 0 Then
                            _QueueIndex = -1
                            OnPropertyChanged(New ComponentModel.PropertyChangedEventArgs(NameOf(QueueIndex)))
                            [Next]()
                            Return
                        End If
                        _QueueIndexSkipNext = True
                        Queue.RemoveAt(0)
                        _QueueIndexSkipNext = False
                        _QueueIndex -= 1
                    Loop
                    _QueueIndex = 0
                End If
                If AutoPlay Then
#Disable Warning
                    Parent.LoadSong(Queue.Item(_QueueIndex))
#Enable Warning
                End If
                OnPropertyChanged(New ComponentModel.PropertyChangedEventArgs(NameOf(QueueIndex)))
            End Set
        End Property

        <NonSerialized> Private _Cover As ImageSource
        Property Cover As ImageSource
            Get
                Return _Cover
            End Get
            Set(value As ImageSource)
                _Cover = value
                OnPropertyChanged(New ComponentModel.PropertyChangedEventArgs(NameOf(Cover)))
            End Set
        End Property

        ReadOnly Property HasQueueItems As Boolean
            Get
                Return (Queue.Count > 0)
            End Get
        End Property

        ReadOnly Property HasItems As Boolean
            Get
                Return (Count > 0)
            End Get
        End Property

        ReadOnly Property HasCover As Boolean
            Get
                Return Not IsNothing(Cover)
            End Get
        End Property

        <NonSerialized> Private _IsQueueChangedNotifierAttached As Boolean = False
        <NonSerialized> Private QueueChangedNotifier As Specialized.NotifyCollectionChangedEventHandler

        Public Sub New()
            QueueChangedNotifier = New Specialized.NotifyCollectionChangedEventHandler(Sub(sender, e)
                                                                                           OnPropertyChanged(New ComponentModel.PropertyChangedEventArgs(NameOf(HasQueueItems)))
                                                                                       End Sub)
            AddHandler Queue.CollectionChanged, QueueChangedNotifier
            _IsQueueChangedNotifierAttached = True
        End Sub

        Sub EnsureQueueNotifier()
            If QueueChangedNotifier Is Nothing Then
                QueueChangedNotifier = New Specialized.NotifyCollectionChangedEventHandler(Sub(sender, e)
                                                                                               OnPropertyChanged(New ComponentModel.PropertyChangedEventArgs(NameOf(HasQueueItems)))
                                                                                           End Sub)
            End If
            If Not _IsQueueChangedNotifierAttached Then
                AddHandler Queue.CollectionChanged, QueueChangedNotifier
                _IsQueueChangedNotifierAttached = True
                OnPropertyChanged(New ComponentModel.PropertyChangedEventArgs(NameOf(HasQueueItems)))
            End If
        End Sub

        ''' <summary>
        ''' Adds a new item to the playlist
        ''' </summary>
        ''' <param name="item"></param>
        ''' <returns>
        ''' If successful returns the new item index, else returns -1
        ''' </returns>
        Overloads Function Add(item As Metadata) As Integer
            If item Is Nothing Then Return -1
            item.Index = Count 'Doesn't matter if AddBehaviour is set to first or last, insert will refresh the indexes            
            If SongAddBehaviour = AddBehaviour.Last Then MyBase.Add(item) Else Insert(0, item)
            If IsShuffling Then
                AddToShuffle(item.Index) 'Item.Index to avoid AddBehaviour effect
            End If
            If TotalDuration = TimeSpan.Zero Then
                TotalDuration = TimeSpan.FromSeconds(Me.Sum(Function(k) k.Length))
            Else
                TotalDuration += item.LengthTS
            End If
            OnPropertyChanged(New ComponentModel.PropertyChangedEventArgs(NameOf(HasItems)))
            Return item.Index
        End Function

        Overloads Sub Insert(index As Integer, item As Metadata)
            If item Is Nothing Then Return
            MyBase.Insert(index, item)
            RefreshItemsIndex()
        End Sub

        Overloads Function Remove(item As Metadata) As Boolean
            If item Is CurrentItem Then
                If MyBase.Remove(item) Then
                    '[Next]()
                    Dim _ix = Index
                    _Index = -1
                    Index = _ix
                    RefreshItemsIndex()
                    Return True
                End If
            Else
                RefreshItemsIndex()
                Return MyBase.Remove(item)
            End If
            Return False
        End Function

        Overloads Sub Move(oldIndex As Integer, newIndex As Integer)
            If oldIndex >= Count OrElse oldIndex < 0 Then Return
            Dim _newIndex, _oldIndex As Integer
            _oldIndex = oldIndex
            If newIndex >= Count Then
                _newIndex = 0
            ElseIf newIndex < 0 Then
                _newIndex = Count - 1
            ElseIf newIndex > 0 AndAlso newIndex < Count Then
                _newIndex = newIndex
            Else
                Return
            End If
            If Item(_oldIndex) Is CurrentItem Then
                MyBase.Move(_oldIndex, _newIndex)
                _Index = _newIndex
                OnPropertyChanged(New ComponentModel.PropertyChangedEventArgs(NameOf(Index)))
                OnPropertyChanged(New ComponentModel.PropertyChangedEventArgs(NameOf(ActualIndex)))
                OnPropertyChanged(New ComponentModel.PropertyChangedEventArgs(NameOf(NextItem)))
                OnPropertyChanged(New ComponentModel.PropertyChangedEventArgs(NameOf(PreviousItem)))
            Else
                If _newIndex > Index Then
                    _Index -= 1
                End If
                MyBase.Move(_oldIndex, _newIndex)
            End If
            RefreshItemsIndex()
        End Sub

        Function [Next](Optional Update As Boolean = True, Optional UpdateParent As Boolean = True) As Metadata
            If Count = 0 AndAlso Queue.Count = 0 Then Return Nothing
            If Queue.Count > 0 Then
                If UpdateParent Then
                    If QueueIndex = -1 Then
                        QueueIndex = 0
                    Else
                        If Queue.Count = 1 Then Queue.Clear() : _QueueIndex = -1 : [Next](Update, UpdateParent) Else QueueIndex = 1
                    End If
                Else
                    If Update Then
                        If QueueIndex = -1 Then
                            _QueueIndex = 0
                        Else
                            If Queue.Count = 1 Then Queue.Clear() : [Next](Update, UpdateParent) Else _QueueIndex = 1
                        End If
                        QueueIndex += 1
                    Else
                        Return Queue.Item(If(QueueIndex = -1, 0, 1))
                    End If
                End If
                Return If(_QueueIndex >= Queue.Count Or _QueueIndex < 0, Nothing, Queue.Item(QueueIndex))
            Else
                _QueueIndex = -1
            End If
            If UpdateParent Then
                Index = If(IsShuffling, NextShuffle(), If(Index + 1 = Count, If(IsLooping, 0, -1), Index + 1))
            Else
                If Update Then
                    _Index = If(IsShuffling, NextShuffle(), If(Index + 1 = Count, If(IsLooping, 0, -1), Index + 1))
                    OnPropertyChanged(New ComponentModel.PropertyChangedEventArgs(NameOf(Index)))
                    OnPropertyChanged(New ComponentModel.PropertyChangedEventArgs(NameOf(ActualIndex)))
                Else
                    Dim i = If(IsShuffling, NextShuffle(False), If(Index + 1 = Count, If(IsLooping, 0, -1), Index + 1))
                    If i = -1 Then Return Nothing
                    Return Item(i)
                End If
            End If
            Return If(Index = -1, Nothing, Item(Index))
        End Function

        Function Previous(Optional Update As Boolean = True, Optional UpdateParent As Boolean = True) As Metadata
            If Queue.Count > 0 Then _QueueIndex = -1
            If Count = 0 Then Return Nothing
            If UpdateParent Then
                Dim DeterminedValue = If(IsShuffling, PreviousShuffle(), If(Index = 0, If(IsLooping, Count - 1, -1), Index - 1))
                Index = DeterminedValue
            Else
                If Update Then
                    Dim DeterminedValue = If(IsShuffling, PreviousShuffle(), If(Index = 0, If(IsLooping, Count - 1, -1), Index - 1))
                    _Index = DeterminedValue
                Else
                    Dim DeterminedValue = If(IsShuffling, PreviousShuffle(False), If(Index = 0, If(IsLooping, Count - 1, -1), If(Index < 0, -1, Index - 1)))
                    Return If(DeterminedValue = -1, Nothing, Item(DeterminedValue))
                End If
            End If
            Return If(Index = -1, Nothing, Item(Index))
        End Function

#Region "Shuffle"
        Private Sub Shuffle()
            _ShuffledIndexList.Clear()
            _ShuffledIndexList = Enumerable.Range(0, Count).ToList
            Dim Rnd As New Random()
            Dim j As Int32
            Dim temp As String
            For n As Int32 = _ShuffledIndexList.Count - 1 To 0 Step -1
                j = Rnd.Next(0, n + 1)
                ' Swap them.
                temp = _ShuffledIndexList(n)
                _ShuffledIndexList(n) = _ShuffledIndexList(j)
                _ShuffledIndexList(j) = temp
            Next n
        End Sub
        Private Function NextShuffle(Optional UpdateShuffleIndex As Boolean = True) As Integer
            If UpdateShuffleIndex Then
                _ShuffleIndexListIndex = If(_ShuffleIndexListIndex + 1 = Count, 0, _ShuffleIndexListIndex + 1)
                Return _ShuffledIndexList(_ShuffleIndexListIndex)
            Else
                Return _ShuffledIndexList(If(_ShuffleIndexListIndex + 1 = Count, 0, _ShuffleIndexListIndex + 1))
            End If
        End Function
        Private Function PreviousShuffle(Optional UpdateShuffleIndex As Boolean = True) As Integer
            If UpdateShuffleIndex Then
                _ShuffleIndexListIndex = If(_ShuffleIndexListIndex - 1 = -1, Count - 1, _ShuffleIndexListIndex - 1)
                Return _ShuffledIndexList(_ShuffleIndexListIndex)
            Else
                Return _ShuffledIndexList(If(_ShuffleIndexListIndex - 1 = -1, Count - 1, _ShuffleIndexListIndex - 1))
            End If
        End Function
        Private Sub AddToShuffle(index As Integer)
            Dim rnd As New Random
            _ShuffledIndexList.Insert(rnd.Next(0, _ShuffledIndexList.Count), index)
        End Sub
#End Region
        Public Sub RefreshItemsIndex()
            For i As Integer = 0 To Count - 1
                Item(i).Index = i
            Next
        End Sub

        Private Sub Playlist_CollectionChanged(sender As Object, e As NotifyCollectionChangedEventArgs) Handles Me.CollectionChanged
            OnPropertyChanged(New ComponentModel.PropertyChangedEventArgs(NameOf(HasItems)))
        End Sub

        ''' <summary>
        ''' Copies this instance data to a new one and return in.
        ''' </summary>
        ''' <remarks>
        ''' All configuration properties are ignored, only <see cref="Name"/> and items are copied using <see cref="Metadata.Clone()"/>.
        ''' </remarks>
        ''' <returns></returns>
        Public Function Clone() As Playlist
            Dim cpl As New Playlist() With {.Name = Name}
            For Each meta In Me
                cpl.Add(meta.Clone)
            Next
            Return cpl
        End Function
    End Class
End Namespace
Namespace Converters
    Public Class PlaylistAddBehaviourToIntegerConverter
        Implements IValueConverter

        Public Function Convert(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.Convert
            Dim _v As Integer
            If Integer.TryParse(value, _v) Then
                Return _v
            Else
                Return value
            End If
        End Function

        Public Function ConvertBack(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.ConvertBack
            Return CType(value, Player.Playlist.AddBehaviour)
        End Function
    End Class
End Namespace