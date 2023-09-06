Imports System.ComponentModel
Imports QuickBeat.Utilities

Namespace Player
    <Serializable>
    Public Class Metadata
        Implements ComponentModel.INotifyPropertyChanged

        Public BlockDOWNLOADPROC As Boolean = False

        Private _Title As String
        Property Title As String
            Get
                Return _Title
            End Get
            Set(value As String)
                _Title = value
                OnPropertyChanged()
            End Set
        End Property

        Private _Artists As String()
        Property Artists As String()
            Get
                Return _Artists
            End Get
            Set(value As String())
                _Artists = value
                OnPropertyChanged()
                OnPropertyChanged(NameOf(DefaultArtist))
                OnPropertyChanged(NameOf(JoinedArtists))
            End Set
        End Property

        Private _Album As String
        Property Album As String
            Get
                Return _Album
            End Get
            Set(value As String)
                _Album = value
                OnPropertyChanged()
            End Set
        End Property

        Private _Genres As IList(Of String)
        Property Genres As IList(Of String)
            Get
                Return _Genres
            End Get
            Set(value As IList(Of String))
                _Genres = value
                OnPropertyChanged()
                OnPropertyChanged(NameOf(JoinedGenres))
            End Set
        End Property

        <NonSerialized> Private _Covers As IEnumerable(Of ImageSource)
        Property Covers As IEnumerable(Of ImageSource)
            Get
                Return _Covers
            End Get
            Set(value As IEnumerable(Of ImageSource))
                _Covers = value
                OnPropertyChanged()
                OnPropertyChanged(NameOf(DefaultCover))
            End Set
        End Property

        Private _Path As String
        ''' <summary>
        ''' Automatically copies to <see cref="OriginalPath"/> if <see cref="Location"/> is <see cref="FileLocation.Remote"/>
        ''' </summary>
        ''' <returns></returns>
        Property Path As String
            Get
                Return _Path
            End Get
            Set(value As String)
                _Path = value
                If Location = FileLocation.Local Then
                    OriginalPath = value
                End If
                OnPropertyChanged()
            End Set
        End Property

        Private _PlayCount As Integer
        Property PlayCount As Integer
            Get
                Return _PlayCount
            End Get
            Set(value As Integer)
                _PlayCount = value
                OnPropertyChanged()
            End Set
        End Property

        Private _IsFavorite As Boolean
        Property IsFavorite As Boolean
            Get
                Return _IsFavorite
            End Get
            Set(value As Boolean)
                _IsFavorite = value
                OnPropertyChanged()
                SharedProperties.Instance.Library?.OnFavoriteChanged(Me)
            End Set
        End Property

        Private _Length As Double
        ''' <summary>
        ''' Duration in seconds.
        ''' </summary>
        ''' <returns></returns>
        Property Length As Double
            Get
                Return _Length
            End Get
            Set(value As Double)
                _Length = value
                _LengthString = $"{If(LengthTS.Minutes < 10, 0, "")}{LengthTS.Minutes}:{If(LengthTS.Seconds < 10, 0, "")}{LengthTS.Seconds}"
                OnPropertyChanged()
                OnPropertyChanged(NameOf(LengthString))
                OnPropertyChanged(NameOf(LengthTS))
            End Set
        End Property

        Private _LengthTS As TimeSpan = TimeSpan.Zero
        ReadOnly Property LengthTS As TimeSpan
            Get
                If _LengthTS = TimeSpan.Zero Then _LengthTS = TimeSpan.FromSeconds(Length)
                Return _LengthTS
            End Get
        End Property
        Private _LengthString As String
        ReadOnly Property LengthString As String
            Get
                If String.IsNullOrEmpty(_LengthString) Then _LengthString = $"{If(LengthTS.Minutes < 10, 0, "")}{LengthTS.Minutes}:{If(LengthTS.Seconds < 10, 0, "")}{LengthTS.Seconds}"
                Return _LengthString
            End Get
        End Property

        Private _HasCover As Boolean
        Property HasCover As Boolean
            Get
                Return _HasCover
            End Get
            Set(value As Boolean)
                _HasCover = value
                OnPropertyChanged()
            End Set
        End Property

        Private _Provider As Interfaces.IMediaProvider
        Property Provider As Interfaces.IMediaProvider
            Get
                Return _Provider
            End Get
            Set(value As Interfaces.IMediaProvider)
                _Provider = value
                OnPropertyChanged()
            End Set
        End Property

        Private _OriginalPath As String
        Property OriginalPath As String
            Get
                Return If(String.IsNullOrEmpty(_OriginalPath), Path, _OriginalPath)
            End Get
            Set(value As String)
                _OriginalPath = value
                OnPropertyChanged()
            End Set
        End Property

        ReadOnly Property DefaultCover As ImageSource
            Get
                Return If(Covers?.FirstOrDefault, New BitmapImage(New Uri("Resources/MusicRecord.png", UriKind.Relative)))
            End Get
        End Property

        ReadOnly Property DefaultArtist As String
            Get
                Return Artists?.FirstOrDefault
            End Get
        End Property

        ReadOnly Property JoinedArtists As String
            Get
                Return If(Artists Is Nothing, "", String.Join("; ", Artists))
            End Get
        End Property

        ReadOnly Property JoinedGenres As String
            Get
                Return If(Genres Is Nothing, "", String.Join("; ", Genres))
            End Get
        End Property

        ReadOnly Property TitleArtist As String
            Get
                Return $"{Title} - {DefaultArtist}"
            End Get
        End Property

        ReadOnly Property TitleAlbum As String
            Get
                Return $"{Title} - {Album}"
            End Get
        End Property

        ReadOnly Property AlbumArtist As String
            Get
                Return $"{Album} - {DefaultArtist}"
            End Get
        End Property

        ReadOnly Property TitleArtistAlbum As String
            Get
                Return $"{Title} - {DefaultArtist} - {Album}"
            End Get
        End Property

        ReadOnly Property IndexTitle As String
            Get
                Return $"{Index}. {Title}"
            End Get
        End Property

        ReadOnly Property IndexTitleArtist As String
            Get
                Return $"{Index}. {Title} - {DefaultArtist}"
            End Get
        End Property

        Private _Location As FileLocation = FileLocation.Undefined
        Property Location As FileLocation
            Get
                Return _Location
            End Get
            Set(value As FileLocation)
                _Location = value
                OnPropertyChanged()
                OnPropertyChanged(NameOf(IsRemote))
            End Set
        End Property

        ReadOnly Property IsRemote As Boolean
            Get
                Return Location = FileLocation.Remote
            End Get
        End Property

        Property Tag As String

        Private _Index As Integer
        Property Index As Integer
            Get
                Return _Index
            End Get
            Set(value As Integer)
                _Index = value
                OnPropertyChanged()
            End Set
        End Property

        Private _IsCoverLocked As Boolean = False
        Property IsCoverLocked As Boolean
            Get
                Return _IsCoverLocked
            End Get
            Set(value As Boolean)
                _IsCoverLocked = value
                OnPropertyChanged()
            End Set
        End Property

        Private _IsInUse As Boolean
        ''' <summary>
        ''' Determines whether this instance is being used by a player
        ''' </summary>
        ''' <returns></returns>
        Property IsInUse As Boolean
            Get
                Return _IsInUse
            End Get
            Set(value As Boolean)
                _IsInUse = value
            End Set
        End Property

        <NonSerialized> Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged
        Sub OnPropertyChanged(<Runtime.CompilerServices.CallerMemberName> Optional CallerName As String = Nothing)
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(CallerName))
        End Sub

        Sub RefreshTagsFromFile(Optional SkipCover As Boolean = False)
            If Not IO.File.Exists(Path) Then Return
            Utilities.DebugMode.Instance.Log(Of Metadata)("Attempting to refresh a metadata from file, Path:=" & Path)
            Utilities.DebugMode.Instance.Log(Of Metadata)("Refreshing metadata tags...")
            Dim Tags As TagLib.File = Nothing
            Try
                Tags = TagLib.File.Create(Path, If(SkipCover, TagLib.ReadStyle.Average Or TagLib.ReadStyle.PictureLazy, TagLib.ReadStyle.Average))
            Catch ex As Exception
                Utilities.DebugMode.Instance.Log(Of Metadata)("Error while reading tags: " & ex.ToString)
            End Try
            Title = Tags.Tag.Title
            Artists = Tags.Tag.Performers
            Album = Tags.Tag.Album
            Path = Path
            Length = Tags.Properties.Duration.TotalSeconds
            HasCover = (Tags.Tag.Pictures.Length > 0)
            If Not SkipCover Then
                Dim Covers As New List(Of ImageSource)
                If Tags IsNot Nothing Then
                    For Each picture In Tags?.Tag.Pictures
                        Dim BI As New BitmapImage
                        BI.BeginInit()
                        BI.CacheOption = BitmapCacheOption.OnDemand
                        'BI.DecodePixelHeight = 150
                        'BI.DecodePixelWidth = 150
                        BI.StreamSource = New IO.MemoryStream(picture.Data.Data)
                        BI.EndInit()
                        Covers.Add(BI)
                    Next
                End If
                Covers = Covers
            End If
            If String.IsNullOrEmpty(Title) Then
                Title = IO.Path.GetFileNameWithoutExtension(Path)
            End If
            Utilities.DebugMode.Instance.Log(Of Metadata)("Done refreshing metadata.")
        End Sub
        Sub RefreshTagsFromFile_ThreadSafe(Optional SkipCover As Boolean = False)
            If Not IO.File.Exists(Path) Then Return
            Utilities.DebugMode.Instance.Log(Of Metadata)("Attempting to refresh a metadata from file, Path:=" & Path)
            Utilities.DebugMode.Instance.Log(Of Metadata)("Refreshing metadata tags...")
            Dim Tags As TagLib.File = Nothing
            Try
                Tags = TagLib.File.Create(Path, If(SkipCover, TagLib.ReadStyle.Average Or TagLib.ReadStyle.PictureLazy, TagLib.ReadStyle.Average))
            Catch ex As Exception
                Utilities.DebugMode.Instance.Log(Of Metadata)("Error while reading tags: " & ex.ToString)
            End Try
            Application.Current.Dispatcher.Invoke(Sub()
                                                      Title = Tags.Tag.Title
                                                      Artists = Tags.Tag.Performers
                                                      Album = Tags.Tag.Album
                                                      Path = Path
                                                      Length = Tags.Properties.Duration.TotalSeconds
                                                      HasCover = (Tags.Tag.Pictures.Length > 0)
                                                  End Sub)
            If Not SkipCover Then
                Dim Covers As New List(Of ImageSource)
                If Tags IsNot Nothing Then
                    For Each picture In Tags?.Tag.Pictures
                        Dim BI As New BitmapImage
                        BI.BeginInit()
                        BI.CacheOption = BitmapCacheOption.OnDemand
                        'BI.DecodePixelHeight = 150
                        'BI.DecodePixelWidth = 150
                        BI.StreamSource = New IO.MemoryStream(picture.Data.Data)
                        BI.EndInit()
                        Covers.Add(BI)
                    Next
                End If
                Application.Current.Dispatcher.Invoke(Sub()
                                                          Covers = Covers
                                                      End Sub)
            End If
            Application.Current.Dispatcher.Invoke(Sub()
                                                      If String.IsNullOrEmpty(Title) Then
                                                          Title = IO.Path.GetFileNameWithoutExtension(Path)
                                                      End If
                                                  End Sub)
            Utilities.DebugMode.Instance.Log(Of Metadata)("Done refreshing metadata.")
        End Sub
        Async Sub EnsureCovers()
            If (Covers Is Nothing OrElse Covers.Count = 0) AndAlso IO.File.Exists(Path) Then
                Dim Tags As TagLib.File = Nothing
                Try
                    Tags = TagLib.File.Create(Path, TagLib.ReadStyle.Average)
                Catch ex As Exception
                    Utilities.DebugMode.Instance.Log(Of Metadata)("Error while reading tags: " & ex.ToString)
                End Try
                If Tags IsNot Nothing AndAlso Tags.Tag.Pictures.Length > 0 Then
                    HasCover = True
                    Dim Covers As New List(Of ImageSource)
                    For Each picture In Tags.Tag.Pictures
                        Dim BI As New BitmapImage
                        BI.BeginInit()
                        BI.CacheOption = BitmapCacheOption.OnDemand
                        'BI.DecodePixelHeight = 150
                        'BI.DecodePixelWidth = 150
                        BI.StreamSource = New IO.MemoryStream(picture.Data.Data)
                        BI.EndInit()
                        Covers.Add(BI)
                    Next
                    Me.Covers = Covers
                    Tags.Dispose()
                End If
            ElseIf (Covers Is Nothing OrElse Covers.Count = 0) AndAlso Location = FileLocation.Remote AndAlso Provider IsNot Nothing Then
                Dim Thumb = Await Provider.FetchThumbnail
                If Thumb IsNot Nothing Then
                    Me.Covers = New ImageSource() {Thumb}
                End If
            End If
        End Sub

        Public Sub LoadFromTFile(file As TagLib.File, Optional SkipLength As Boolean = False)
            Title = file.Tag.Title
            Artists = file.Tag.Performers
            Album = file.Tag.Album
            If Not SkipLength Then Length = file.Properties.Duration.TotalSeconds
            HasCover = (file.Tag.Pictures.Length > 0)
            If HasCover Then
                Application.Current.Dispatcher.Invoke(Sub()
                                                          Dim Covers As New List(Of ImageSource)
                                                          For Each picture In file.Tag.Pictures
                                                              Dim BI As New BitmapImage
                                                              BI.BeginInit()
                                                              BI.CacheOption = BitmapCacheOption.OnDemand
                                                              'BI.DecodePixelHeight = 150
                                                              'BI.DecodePixelWidth = 150
                                                              BI.StreamSource = New IO.MemoryStream(picture.Data.Data)
                                                              BI.EndInit()
                                                              Covers.Add(BI)
                                                          Next
                                                          Me.Covers = Covers
                                                      End Sub)
            End If
        End Sub

        Shared Function FromFile(path As String, Optional SkipCover As Boolean = False, Optional SearchLibraryOnly As Boolean = False) As Metadata
            Utilities.DebugMode.Instance.Log(Of Metadata)("Attempting to create a metadata from file, Path:=" & path & ", SkipCover:=" & SkipCover & ", SearchLibraryOnly:=" & SearchLibraryOnly)
            If IO.File.Exists(path) Then
                'Check if available in library                
                Dim MData = Utilities.SharedProperties.Instance.Library.FirstOrDefault(Function(k) k.Path = path)
                If MData IsNot Nothing Then
                    Utilities.DebugMode.Instance.Log(Of Metadata)("Data found, Returning...")
                    Return MData
                End If
                If SearchLibraryOnly Then Return Nothing
                'Create one
                Utilities.DebugMode.Instance.Log(Of Metadata)("Couldn't find metadata, Creating one...")
                Dim Tags As TagLib.File = Nothing
                Try
                    Tags = TagLib.File.Create(path, If(SkipCover, TagLib.ReadStyle.Average Or TagLib.ReadStyle.PictureLazy, TagLib.ReadStyle.Average))
                Catch ex As Exception
                    Utilities.DebugMode.Instance.Log(Of Metadata)("Error while reading tags: " & ex.ToString)
                End Try
                Dim MD As Metadata = If(Tags Is Nothing, New Metadata With {.Location = FileLocation.Local, .Path = path, .OriginalPath = path, .Provider = New IOMediaProvider(path)}, New Metadata With {.Location = FileLocation.Local, .OriginalPath = path, .Provider = New IOMediaProvider(path), .Title = Tags.Tag.Title, .Artists = Tags.Tag.Performers, .Album = Tags.Tag.Album, .Path = path, .Length = Tags.Properties.Duration.TotalSeconds, .HasCover = (Tags.Tag.Pictures.Length > 0)})
                If Not SkipCover Then
                    Dim Covers As New List(Of ImageSource)
                    If Tags IsNot Nothing Then
                        For Each picture In Tags?.Tag.Pictures
                            Dim BI As New BitmapImage
                            BI.BeginInit()
                            BI.CacheOption = BitmapCacheOption.OnDemand
                            'BI.DecodePixelHeight = 150
                            'BI.DecodePixelWidth = 150
                            BI.StreamSource = New IO.MemoryStream(picture.Data.Data)
                            BI.EndInit()
                            Covers.Add(BI)
                        Next
                    End If
                    MD.Covers = Covers
                End If
                If String.IsNullOrEmpty(MD.Title) Then
                    MD.Title = IO.Path.GetFileNameWithoutExtension(path)
                End If
                Utilities.DebugMode.Instance.Log(Of Metadata)("Done creating metadata, Returning...")
                Return MD
            End If
            Utilities.DebugMode.Instance.Log(Of Metadata)("Couldn't create a metadata.")
            Return Nothing
        End Function

        ''' <summary>
        ''' Determines if one of the properies equals the value provided
        ''' </summary>        
        ''' <returns></returns>        
        Public Function HasValue(value As String) As Boolean
            Utilities.DebugMode.Instance.Log(Of Metadata)("Checking values, value:=" & value & "...")
            Dim Result = False
            Dim LValue = value.ToLower
            If Not String.IsNullOrEmpty(Title) Then Result = Result Or Title.ToLower.Contains(LValue)
            If Artists IsNot Nothing AndAlso Artists.Length > 0 Then Result = Result Or String.Join(",", Artists).ToLower.Contains(LValue)
            If Not String.IsNullOrEmpty(Album) Then Result = Result Or Album.ToLower.Contains(LValue)
            If Not String.IsNullOrEmpty(Path) Then Result = Result Or Path.ToLower.Contains(LValue)
            Return Result
        End Function
        ''' <summary>
        ''' Gets a valid path for current file
        ''' </summary>
        ''' <returns>
        ''' <see cref="Path"/> if <see cref="Location"/> is <see cref="FileLocation.Local"/>.
        ''' <see cref="Path"/> if <see cref="Provider"/> is nothing.
        ''' <see cref="Interfaces.IMediaProvider.Fetch()"/> if <see cref="Location"/> is <see cref="FileLocation.Remote"/> and <see cref="Provider"/> isn't nothing.
        ''' </returns>
        Public Async Function GetActivePath() As Task(Of String)
            If Location = FileLocation.Local Then
                Return Path
            ElseIf Location = FileLocation.Remote Then
                If Provider Is Nothing Then
                    Return Path
                Else
                    Try
                        Dim _IsActive As Boolean
                        Try
                            _IsActive = My.Computer.Network.Ping(Path)
                        Catch ex As Exception
                            Utilities.DebugMode.Instance.Log(Of Metadata)(ex.ToString)
                        End Try
                        If _IsActive Then
                            Return Path
                        Else
                            Dim nPath = Await Provider.Fetch
                            If Not String.IsNullOrEmpty(nPath) Then
                                _Path = nPath
                            End If
                            Return Path
                        End If
                    Catch ex As Exception
                        Utilities.DebugMode.Instance.Log(Of Metadata)(ex.ToString)
                    End Try
                End If
            End If
            Return Path
        End Function
        Public Overrides Function ToString() As String
            Return $"{Title} By {If(Artists IsNot Nothing, String.Join(";", Artists), "N/A")} From {Album} With Duration {LengthString} At {Location.ToString}:{Path}"
        End Function

        ''' <summary>
        ''' Frees <see cref="Covers"/> without notifying the UI
        ''' </summary>
        Public Sub FreeCovers()
            If Not IsCoverLocked Then
                _Covers = Nothing
                Application.Current.Dispatcher.Invoke(Sub()
                                                          SharedProperties.Instance.MemoryManager.TotalCleaned += 1
                                                      End Sub)
            End If
        End Sub


        ''' <summary>
        ''' Copies this instance data to a new one and return in.
        ''' </summary>
        ''' <remarks>
        ''' <see cref="IsCoverLocked"/>, <see cref="IsInUse"/> and <see cref="Tag"/> are ignored.
        ''' </remarks>
        ''' <returns></returns>
        Public Function Clone() As Metadata
            Return New Metadata() With {.Location = Location, .Album = Album, .Artists = Artists, .Covers = Covers, .Genres = Genres, .Index = Index, .IsFavorite = IsFavorite, .Length = Length, .Path = Path, .PlayCount = PlayCount, .Title = Title, .BlockDOWNLOADPROC = BlockDOWNLOADPROC, .OriginalPath = OriginalPath, .Provider = Provider, .HasCover = HasCover}
        End Function

#Region "Serialization"
        <Runtime.Serialization.OnDeserialized>
        Private Sub Deserialization(context As Runtime.Serialization.StreamingContext)
            'Restore values from serialization
            If Location = FileLocation.Local And Provider Is Nothing Then
                Provider = New IOMediaProvider(Path)
            End If
        End Sub
#End Region

        Public Enum FileLocation
            Local
            Remote
            Undefined
        End Enum
    End Class
End Namespace