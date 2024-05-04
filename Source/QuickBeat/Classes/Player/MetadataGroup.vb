Imports QuickBeat.Utilities

Namespace Player
    <Serializable>
    Public Class MetadataGroup
        Inherits ObjectModel.ObservableCollection(Of Metadata)

        Private _Name As String
        Property Name As String
            Get
                Return _Name
            End Get
            Set(value As String)
                _Name = value
                OnPropertyChanged(New ComponentModel.PropertyChangedEventArgs(NameOf(Name)))
            End Set
        End Property

        Private _Category As String
        Property Category As String
            Get
                Return _Category
            End Get
            Set(value As String)
                _Category = value
                OnPropertyChanged(New ComponentModel.PropertyChangedEventArgs(NameOf(Category)))
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

        Private _Type As GroupType
        Property Type As GroupType
            Get
                Return _Type
            End Get
            Set(value As GroupType)
                _Type = value
                OnPropertyChanged(New ComponentModel.PropertyChangedEventArgs(NameOf(Type)))
            End Set
        End Property

        ReadOnly Property TotalDurationString As String
            Get
                Return If(TotalDuration.Hours > 0, TotalDuration.ToString("hh\:mm\:ss"), TotalDuration.ToString("mm\:ss"))
            End Get
        End Property

        Private _TotalFavorite As Integer
        Property TotalFavorite As Integer
            Get
                Return _TotalFavorite
            End Get
            Set(value As Integer)
                _TotalFavorite = value
                OnPropertyChanged(New ComponentModel.PropertyChangedEventArgs(NameOf(TotalFavorite)))
            End Set
        End Property

        Private _UID As String
        Property UID As String
            Get
                Return _UID
            End Get
            Set(value As String)
                _UID = value
                OnPropertyChanged(New ComponentModel.PropertyChangedEventArgs(NameOf(UID)))
            End Set
        End Property

        Public Property Tag As Object

        Private _PlayCount As Integer
        Property PlayCount As Integer
            Get
                Return _PlayCount
            End Get
            Set(value As Integer)
                _PlayCount = value
                OnPropertyChanged(New ComponentModel.PropertyChangedEventArgs(NameOf(PlayCount)))
            End Set
        End Property

        Private _IsFavorite As Boolean
        'TODO Not Implemented
        Property IsFavorite As Boolean
            Get
                Return _IsFavorite
            End Get
            Set(value As Boolean)
                _IsFavorite = value
                OnPropertyChanged(New ComponentModel.PropertyChangedEventArgs(NameOf(IsFavorite)))
                SharedProperties.Instance.Library?.OnGroupFavoriteChanged(Me)
            End Set
        End Property

        Private _IsInUse As Boolean
        Property IsInUse As Boolean
            Get
                Return _IsInUse
            End Get
            Set(value As Boolean)
                _IsInUse = value
                OnPropertyChanged(New ComponentModel.PropertyChangedEventArgs(NameOf(IsInUse)))
            End Set
        End Property

        Private _IsCoverLocked
        Property IsCoverLocked As Boolean
            Get
                Return _IsCoverLocked
            End Get
            Set(value As Boolean)
                _IsCoverLocked = value
                OnPropertyChanged(New ComponentModel.PropertyChangedEventArgs(NameOf(IsCoverLocked)))
            End Set
        End Property

        ''' <summary>
        ''' Updates <see cref="TotalDuration"/>, <see cref="TotalFavorite"/>, <see cref="PlayCount"/> and <see cref="Cover"/>.
        ''' </summary>
        Sub UpdateValues()
            TotalDuration = TimeSpan.FromSeconds(Me.Sum(Function(k) k.Length))
            TotalFavorite = Me.Where(Function(k) k.IsFavorite).Count
            PlayCount = Me.Sum(Function(k) k.PlayCount)
            Dim _Metadata = Me.FirstOrDefault(Function(k) k.HasCover)
            If Cover Is Nothing Then
                If Not IO.File.Exists(_Metadata?.Path) Then Return
                'Fetch covers            
                Dim Covers As New List(Of ImageSource)
                Dim Tags = Utilities.SharedProperties.Instance.RequestTags(_Metadata.Path)
                For Each picture In Tags.Tag.Pictures
                    Dim BI As BitmapImage = New IO.MemoryStream(picture.Data.Data).ToBitmapSource
                    If BI IsNot Nothing Then Covers.Add(BI)
                Next
                Tags.Dispose()
                Cover = Covers.First
            End If
        End Sub

        Public Sub FreeCovers()
            If Not IsCoverLocked Then
                Cover = Nothing
            End If
        End Sub

        ''' <summary>
        ''' Copies this instance data to a new one and return in.
        ''' </summary>
        ''' <remarks>
        ''' Only properties are cloned, items references are copied.
        ''' <see cref="IsCoverLocked"/>, <see cref="IsInUse"/> and <see cref="Tag"/> are ignored.
        ''' </remarks>
        ''' <returns></returns>
        Public Function Clone() As MetadataGroup
            Dim MDG As New MetadataGroup With {.Category = Category, .Cover = Cover, .Name = Name, .PlayCount = PlayCount, .TotalDuration = TotalDuration, .TotalFavorite = TotalFavorite, .Type = Type}
            For Each _Item In Me
                MDG.Add(_Item)
            Next
            Return MDG
        End Function

        Shared Narrowing Operator CType(group As MetadataGroup) As Playlist
            Dim x As New Playlist() With {.Category = group.Category, .Name = group.Name, .TotalDuration = group.TotalDuration}
            For Each meta In group
                x.Add(meta)
            Next
            Return x
        End Operator


        Public Enum GroupType
            Undefined
            Custom
            Playlist
            Artist
            Album
            Genre
            Year
        End Enum
    End Class
End Namespace