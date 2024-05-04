Imports System.ComponentModel

Namespace Player
    <Serializable>
    Public Class FileTag
        Implements ComponentModel.INotifyPropertyChanged

        <NonSerialized> Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged

        Public Sub OnPropertyChanged(<Runtime.CompilerServices.CallerMemberName> Optional CallerName As String = Nothing)
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(CallerName))
        End Sub

        <NonSerialized> Private _File As TagLib.File

        Private _Path As String
        Property Path As String
            Get
                Return _Path
            End Get
            Set(value As String)
                _Path = value
                _File?.Dispose()
                _File = Utilities.SharedProperties.Instance.RequestTags(value)
                EnsureCovers(_File)
                OnPropertyChanged()
            End Set
        End Property

        Property Title As String
            Get
                Return _File.Tag.Title
            End Get
            Set(value As String)
                _File.Tag.Title = value
                OnPropertyChanged()
            End Set
        End Property

        Property Artists As String
            Get
                Return String.Join(";", _File.Tag.Performers)
            End Get
            Set(value As String)
                _File.Tag.Performers = value.Split(";")
                OnPropertyChanged()
            End Set
        End Property

        Property Album As String
            Get
                Return _File.Tag.Album
            End Get
            Set(value As String)
                _File.Tag.Album = value
                OnPropertyChanged()
            End Set
        End Property

        Property Year As UInteger
            Get
                Return _File.Tag.Year
            End Get
            Set(value As UInteger)
                _File.Tag.Year = value
                OnPropertyChanged()
            End Set
        End Property

        Property Track As UInteger
            Get
                Return _File.Tag.Track
            End Get
            Set(value As UInteger)
                _File.Tag.Track = value
                OnPropertyChanged()
            End Set
        End Property

        Property Genres As String
            Get
                Return String.Join(";", _File.Tag.Genres)
            End Get
            Set(value As String)
                _File.Tag.Genres = value.Split(";")
                OnPropertyChanged()
            End Set
        End Property

        Property Comment As String
            Get
                Return _File.Tag.Comment
            End Get
            Set(value As String)
                _File.Tag.Comment = value
                OnPropertyChanged()
            End Set
        End Property

        Property AlbumArtists As String
            Get
                Return String.Join(";", _File.Tag.AlbumArtists)
            End Get
            Set(value As String)
                _File.Tag.AlbumArtists = value.Split(";")
                OnPropertyChanged()
            End Set
        End Property

        Property Lyrics As String
            Get
                Return _File.Tag.Lyrics
            End Get
            Set(value As String)
                _File.Tag.Lyrics = value
                OnPropertyChanged()
            End Set
        End Property

        Property Composer As String
            Get
                Return String.Join(";", _File.Tag.Composers)
            End Get
            Set(value As String)
                _File.Tag.Composers = value.Split(";")
                OnPropertyChanged()
            End Set
        End Property

        Property DiscNumber As UInteger
            Get
                Return _File.Tag.Disc
            End Get
            Set(value As UInteger)
                _File.Tag.Disc = value
                OnPropertyChanged()
            End Set
        End Property

        <NonSerialized> Private _Covers As ObjectModel.ObservableCollection(Of TagLib.IPicture)
        Property Covers As ObjectModel.ObservableCollection(Of TagLib.IPicture)
            Get
                Return _Covers
            End Get
            Set(value As ObjectModel.ObservableCollection(Of TagLib.IPicture))
                _Covers = If(value, New ObjectModel.ObservableCollection(Of TagLib.IPicture))
                OnPropertyChanged()
            End Set
        End Property

        ReadOnly Property AddCoverCommand As WPF.Commands.FileTagAddCoverCommand
        ReadOnly Property RemoveCoverCommand As WPF.Commands.FileTagRemoveCoverCommand
        ReadOnly Property ClearCoverCommand As WPF.Commands.FileTagClearCoversCommand
        ReadOnly Property MoveCoverCommand As WPF.Commands.FileTagMoveCoverCommand
        ReadOnly Property CopyCoverCommand As WPF.Commands.FileTagCopyCoverCommand
        ReadOnly Property ViewCoverCommand As WPF.Commands.FileTagViewCoverCommand
        ReadOnly Property SaveCommand As WPF.Commands.FileTagSaveCommand

        Sub New()
            _Covers = New ObjectModel.ObservableCollection(Of TagLib.IPicture)
            AddCoverCommand = New WPF.Commands.FileTagAddCoverCommand
            RemoveCoverCommand = New WPF.Commands.FileTagRemoveCoverCommand
            ClearCoverCommand = New WPF.Commands.FileTagClearCoversCommand
            MoveCoverCommand = New WPF.Commands.FileTagMoveCoverCommand
            CopyCoverCommand = New WPF.Commands.FileTagCopyCoverCommand
            ViewCoverCommand = New WPF.Commands.FileTagViewCoverCommand
            SaveCommand = New WPF.Commands.FileTagSaveCommand
        End Sub

        Sub Save()
            _File.Tag.Pictures = Covers.ToArray
            _File.Save()
        End Sub

        Public Sub EnsureCovers(Optional _Tag As TagLib.File = Nothing)
            If Covers.Count = 0 AndAlso IO.File.Exists(Path) Then
                Dim Tags As TagLib.File = _Tag
                If Tags Is Nothing Then
                    Try
                        Tags = Utilities.SharedProperties.Instance.RequestTags(Path, TagLib.ReadStyle.Average)
                    Catch ex As Exception
                        Utilities.DebugMode.Instance.Log(Of Metadata)("Error while reading tags: " & ex.ToString)
                    End Try
                End If
                If Tags IsNot Nothing AndAlso Tags.Tag.Pictures.Length > 0 Then
                    Covers.Clear()
                    For Each picture In Tags.Tag.Pictures
                        Covers.Add(picture)
                    Next
                    If _Tag Is Nothing Then
                        Tags.Dispose()
                    End If
                End If
            End If
        End Sub
    End Class
End Namespace