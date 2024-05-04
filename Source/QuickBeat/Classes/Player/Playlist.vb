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
                _QueueIndex = -1 'Because QueueIndex setter has a range protection (must be > 0 and < Queue Count)
                OnPropertyChanged(New ComponentModel.PropertyChangedEventArgs(NameOf(QueueIndex)))
                OnPropertyChanged(New ComponentModel.PropertyChangedEventArgs(NameOf(Index)))
                OnPropertyChanged(New ComponentModel.PropertyChangedEventArgs(NameOf(ActualIndex)))
                OnPropertyChanged(New ComponentModel.PropertyChangedEventArgs(NameOf(CurrentItem)))
                OnPropertyChanged(New ComponentModel.PropertyChangedEventArgs(NameOf(NextItem)))
                OnPropertyChanged(New ComponentModel.PropertyChangedEventArgs(NameOf(PreviousItem)))
                'Parent?.OnIsPlayingFromPlaylistChanged()
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
        ''' Wheter or not to invoke <see cref="Player.LoadSong(Metadata, String(), Boolean)"/> when <see cref="Index"/> or <see cref="QueueIndex"/> are changed.
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

        Private _SongAddBehaviour As AddBehaviour = AddBehaviour.Last
        Property SongAddBehaviour As AddBehaviour
            Get
                Return _SongAddBehaviour
            End Get
            Set(value As AddBehaviour)
                _SongAddBehaviour = value
                OnPropertyChanged(New ComponentModel.PropertyChangedEventArgs(NameOf(SongAddBehaviour)))
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

        Private _AllowDuplicates As Boolean = True
        Property AllowDuplicates As Boolean
            Get
                Return _AllowDuplicates
            End Get
            Set(value As Boolean)
                _AllowDuplicates = value
                If Not value Then
                    Dim dList As New List(Of Metadata)
                    Dim cItem = CurrentItem
                    Dim i = 0
                    Dim l = Me.Distinct
                    Do
                        Dim meta = Me(i)
                        If dList.Contains(meta) Then
                            Me.Remove(meta)
                        Else
                            dList.Add(meta)
                            i += 1
                        End If
                        If i >= Count Then Exit Do
                    Loop
                    If IsShuffling Then _ShuffledIndexList.Clear() : Shuffle()
                    Index = Me.IndexOf(cItem)
                End If
                OnPropertyChanged(New ComponentModel.PropertyChangedEventArgs(NameOf(AllowDuplicates)))
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

        <NonSerialized> Private _Queue As New ObjectModel.ObservableCollection(Of Metadata)
        ReadOnly Property Queue As ObjectModel.ObservableCollection(Of Metadata)
            Get
                Return _Queue
            End Get
        End Property

        <NonSerialized> Private _QueueIndexSkipNext As Boolean = False
        <NonSerialized> Private _QueueIndex As Integer = -1
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
        ''' If successful returns the new item index
        ''' If fails returns -1
        ''' If duplicate found returns the duplicated item index        
        ''' </returns>
        ''' <remarks>
        ''' To get if this functions found a duplicate check the returned index vs. <see cref="Count"/> - 1 if <see cref="SongAddBehaviour"/> = Last, Otherwise check vs. 0
        ''' </remarks>
        Overloads Function Add(item As Metadata) As Integer
            If item Is Nothing Then Return -1
            If Not AllowDuplicates AndAlso Me.Contains(item) Then Return Me.IndexOf(item)
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
            If Parent IsNot Nothing AndAlso Parent.Path = item.Path Then 'sync with parent
                _Index = item.Index
                OnPropertyChanged(New ComponentModel.PropertyChangedEventArgs(NameOf(Index)))
                Parent.OnIsPlayingFromPlaylistChanged()
            End If
            Return item.Index
        End Function

        Overloads Sub Insert(index As Integer, item As Metadata)
            If item Is Nothing Then Return
            MyBase.Insert(index, item)
            RefreshItemsIndex()
        End Sub

        Sub Swap(index As Integer, item As Metadata)
            If item Is Nothing OrElse index < 0 OrElse index > Count Then Return
            item.Index = index
            MyBase.RemoveAt(index)
            MyBase.Insert(index, item)
            OnPropertyChanged(New ComponentModel.PropertyChangedEventArgs(NameOf(CurrentItem)))
            OnPropertyChanged(New ComponentModel.PropertyChangedEventArgs(NameOf(NextItem)))
            OnPropertyChanged(New ComponentModel.PropertyChangedEventArgs(NameOf(PreviousItem)))
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
                    If Parent IsNot Nothing Then Parent.IsTransitioning = True
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
                If Parent IsNot Nothing Then Parent.IsTransitioning = True
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
                If Parent IsNot Nothing Then Parent.IsTransitioning = True
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
            'Parent?.OnIsPlayingFromPlaylistChanged()
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

#Region "Serialization"
        Public Function Save() As String
            Dim ini As New Utilities.SettingsHelper
            ini.AddItem("Index", Index)
            ini.AddItem("Name", Name)
            ini.AddItem("Description", Description)
            ini.AddItem("AllowDuplicates", AllowDuplicates)
            ini.AddItem("AutoPlay", AutoPlay)
            ini.AddItem("Category", Category)
            ini.AddItem("IsLooping", IsLooping)
            ini.AddItem("IsShuffling", IsShuffling)
            ini.AddItem("SongAddBehaviour", SongAddBehaviour)
            ini.AddItem("TotalDuration", TotalDuration.TotalMilliseconds)
            ini.AddItem("ShuffledIndexList", String.Join(";", _ShuffledIndexList))
            ini.AddItem("ShuffleIndexListIndex", _ShuffleIndexListIndex)
            ini.StartSection("Data")
            For Each meta In Me
                ini.AddItem("UID", meta.Location & ";" & meta.UID)
            Next
            ini.EndSection()
            Return ini.Dump
        End Function

        Public Sub Load(dump As String)
            Dim ini As New Utilities.SettingsHelper
            ini.Load(dump)
            If ini.ContainsKey("Index") Then _Index = ini.GetItem("Index")
            If ini.ContainsKey("Name") Then Name = ini.GetItem("Name")
            If ini.ContainsKey("Description") Then Description = ini.GetItem("Description")
            If ini.ContainsKey("AllowDuplicates") Then AllowDuplicates = CBool(ini.GetItem("AllowDuplicates"))
            If ini.ContainsKey("AutoPlay") Then AutoPlay = CBool(ini.GetItem("AutoPlay"))
            If ini.ContainsKey("Category") Then Category = ini.GetItem("Category")
            If ini.ContainsKey("IsLooping") Then IsLooping = CBool(ini.GetItem("IsLooping"))
            If ini.ContainsKey("IsShuffling") Then IsShuffling = CBool(ini.GetItem("IsShuffling"))
            If ini.ContainsKey("SongAddBehaviour") Then SongAddBehaviour = CInt(ini.GetItem("SongAddBehaviour"))
            If ini.ContainsKey("TotalDuration") Then TotalDuration = TimeSpan.FromMilliseconds(ini.GetItem("TotalDuration"))
            If IsShuffling AndAlso ini.ContainsKey("ShuffledIndexList") AndAlso Not String.IsNullOrEmpty(ini.GetItem("ShuffledIndexList")) Then _ShuffledIndexList = ini.GetItem("ShuffledIndexList").Split(";").Select(Of Integer)(Function(k) CInt(k)).ToList
            If IsShuffling AndAlso ini.ContainsKey("ShuffleIndexListIndex") Then _ShuffleIndexListIndex = CInt(ini.GetItem("ShuffleIndexListIndex"))
            If ini.ContainsSection("Data") Then
                For Each uid In ini.GetSection("Data")
                    Dim sUID = uid.Value.Split(";")
                    Dim meta As Metadata = Nothing
                    Select Case sUID.FirstOrDefault
                        Case 0 '"Local"
                            meta = Metadata.FromUID(sUID.LastOrDefault)
                        Case 1 '"Remote"
                            If Utilities.SharedProperties.Instance.RemoteLibrary.ContainsID(sUID.LastOrDefault) Then
                                meta = CachedMetadata.FromCache(sUID.LastOrDefault, True)
                            Else
                                meta = New Metadata With {.Location = Metadata.FileLocation.Remote, .Path = sUID.LastOrDefault, .OriginalPath = sUID.LastOrDefault}
                            End If
                        Case 4 '"Cached"
                            meta = CachedMetadata.FromCache(sUID.LastOrDefault, True)
                        Case 3 '"Internal"
                            meta = New InternalMetadata(sUID.LastOrDefault)
                    End Select
                    If meta IsNot Nothing Then MyBase.Add(meta)
                Next
            End If
        End Sub
#End Region
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