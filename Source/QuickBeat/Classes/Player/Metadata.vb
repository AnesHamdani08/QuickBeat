Imports System.ComponentModel
Imports System.Windows.Markup
Imports QuickBeat.Classes
Imports QuickBeat.Utilities

Namespace Player
    <Serializable>
    Public Class Metadata
        Implements ComponentModel.INotifyPropertyChanged, Classes.MemoryManager.ICleanableItem

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

        Private _Year As UInteger
        Property Year As UInteger
            Get
                Return _Year
            End Get
            Set(value As UInteger)
                _Year = value
                OnPropertyChanged()
            End Set
        End Property

        Private _CoverLink As String
        Property CoverLink As String
            Get
                Return _CoverLink
            End Get
            Set(value As String)
                _CoverLink = value
                OnPropertyChanged()
            End Set
        End Property

        Private _ThumbnailCoverLink As String
        ''' <summary>
        ''' Automatically copies to <see cref="CoverLink"/> if empty
        ''' </summary>
        ''' <returns></returns>
        Property ThumbnailCoverLink As String
            Get
                Return _ThumbnailCoverLink
            End Get
            Set(value As String)
                _ThumbnailCoverLink = value
                If String.IsNullOrEmpty(CoverLink) Then
                    CoverLink = value
                End If
                OnPropertyChanged()
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
        ''' Automatically copies to <see cref="OriginalPath"/> if <see cref="Location"/> is <see cref="FileLocation.Local"/>
        ''' </summary>
        ''' <returns></returns>
        Property Path As String
            Get
                Return _Path
            End Get
            Set(value As String)
                _Path = value
                If Not LockUID Then
                    If Location = FileLocation.Local Then
                        OriginalPath = value
                        UID = value
                    ElseIf Location = FileLocation.Remote Then
                        UID = CommonFunctions.URLtoUID(value)
                    End If
                End If
                If String.IsNullOrEmpty(Extension) Then
                    Extension = IO.Path.GetExtension(value).TrimStart("."c)
                End If
                OnPropertyChanged()
            End Set
        End Property

        Private _PlayCount As Integer
        Overridable Property PlayCount As Integer
            Get
                Return _PlayCount
            End Get
            Set(value As Integer)
                _PlayCount = value
                OnPropertyChanged()
            End Set
        End Property

        Private _IsFavorite As Boolean
        Overridable Property IsFavorite As Boolean
            Get
                Return _IsFavorite
            End Get
            Set(value As Boolean)
                _IsFavorite = value
                OnPropertyChanged()
                SharedProperties.Instance.Library?.OnFavoriteChanged(Me)
            End Set
        End Property

        Private _Size As Long
        Property Size As Long
            Get
                Return _Size
            End Get
            Set(value As Long)
                _Size = value
                OnPropertyChanged()
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

        Private _Location As FileLocation = FileLocation.Undefined
        Overridable Property Location As FileLocation
            Get
                Return _Location
            End Get
            Set(value As FileLocation)
                _Location = value
                OnPropertyChanged()
                OnPropertyChanged(NameOf(IsRemote))
            End Set
        End Property

        Private _Bitrate As Integer
        Property Bitrate As Integer
            Get
                Return _Bitrate
            End Get
            Set(value As Integer)
                _Bitrate = value
                OnPropertyChanged()
            End Set
        End Property

        Private _Channels As Integer
        Property Channels As Integer
            Get
                Return _Channels
            End Get
            Set(value As Integer)
                _Channels = value
                OnPropertyChanged()
            End Set
        End Property

        Private _Codecs As String()
        Property Codecs As String()
            Get
                Return _Codecs
            End Get
            Set(value As String())
                _Codecs = value
                OnPropertyChanged()
            End Set
        End Property

#Region "Read-Only Properties (Helpers)"
        ReadOnly Property DefaultCover As ImageSource
            Get
                Dim dCover = Covers?.FirstOrDefault
                If dCover Is Nothing Then
                    Dim dfCover = New BitmapImage(New Uri("Resources/MusicRecord.png", UriKind.Relative))
                    If dfCover.CanFreeze Then dfCover.Freeze()
                    _IsCoverAvailable = False
                    Return dfCover
                Else
                    _IsCoverAvailable = True
                    Return dCover
                End If
                'Return If(Covers?.FirstOrDefault, New BitmapImage(New Uri("Resources/MusicRecord.png", UriKind.Relative)))
            End Get
        End Property

        Private _IsCoverAvailable As Boolean
        ''' <summary>
        ''' Indicates whether or not <see cref="DefaultCover"/> returned the actual cover or the backup cover (app logo)
        ''' </summary>
        ''' <remarks>
        ''' Value is updated after calling <see cref="DefaultCover"/>
        ''' </remarks>
        ''' <returns></returns>
        ReadOnly Property IsCoverAvailable As Boolean
            Get
                Return _IsCoverAvailable
            End Get
        End Property

        ReadOnly Property DefaultArtist As String
            Get
                Return Artists?.FirstOrDefault
            End Get
        End Property

        ''' <summary>
        ''' Joined using ";"
        ''' </summary>
        ''' <returns></returns>
        ReadOnly Property JoinedArtists As String
            Get
                Return If(Artists Is Nothing, "", String.Join("; ", Artists))
            End Get
        End Property

        ''' <summary>
        ''' Joined using ";"
        ''' </summary>
        ''' <returns></returns>
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

        ReadOnly Property IsRemote As Boolean
            Get
                'Return Location = FileLocation.Remote 'everything has changed baby!
                Return Not (Location = FileLocation.Local OrElse Location = FileLocation.Internal)
            End Get
        End Property

        ''' <summary>
        ''' Joined using ";"
        ''' </summary>
        ''' <returns></returns>
        ReadOnly Property JoinedCodecs As String
            Get
                Return If(Codecs Is Nothing, "", String.Join("; ", Codecs))
            End Get
        End Property

#End Region
        Property Tag As Object

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

        <NonSerialized> Private _IsCoverLocked As Boolean = False
        Property IsCoverLocked As Boolean
            Get
                Return _IsCoverLocked
            End Get
            Set(value As Boolean)
                _IsCoverLocked = value
                OnPropertyChanged()
            End Set
        End Property

        Private _UID As String
        ''' <summary>
        ''' Used to identify songs in library, mainly used in deserialization to map instance to library
        ''' </summary>
        ''' <returns></returns>
        Property UID As String
            Get
                If String.IsNullOrEmpty(_UID) Then
                    If Location = FileLocation.Local Then
                        _UID = Path
                    ElseIf Location = FileLocation.Remote Then
                        _UID = CommonFunctions.URLtoUID(Path)
                    End If
                End If
                Return _UID
            End Get
            Set(value As String)
                _UID = value
                OnPropertyChanged()
            End Set
        End Property

        Private _LockUID As Boolean
        ''' <summary>
        ''' Blocks <see cref="Path"/> from changing <see cref="UID"/>, Restricts <see cref="UID"/> change to its property only.
        ''' </summary>
        ''' <returns></returns>
        Property LockUID As Boolean
            Get
                Return _LockUID
            End Get
            Set(value As Boolean)
                _LockUID = value
                OnPropertyChanged()
            End Set
        End Property

        Private _Extension As String
        ''' <summary>
        ''' Original file extension. eg: mp4,mp3,avi...
        ''' </summary>
        ''' <returns></returns>
        Property Extension As String
            Get
                Return _Extension
            End Get
            Set(value As String)
                _Extension = value
                OnPropertyChanged()
            End Set
        End Property

        Public Sub New()

        End Sub

        ''' <summary>
        ''' Determines whether this instance is being used by an object
        ''' </summary>
        ''' <returns></returns>
        ReadOnly Property IsInUse As Boolean
            Get
                Return CleanableConfiguration.IsBeingUsed
            End Get
        End Property

        <NonSerialized> Private _CleanableConfiguration As New MemoryManager.CleanConfiguration()
        Public Property CleanableConfiguration As MemoryManager.CleanConfiguration Implements MemoryManager.ICleanableItem.CleanableConfiguration
            Get
                If _CleanableConfiguration Is Nothing Then _CleanableConfiguration = New MemoryManager.CleanConfiguration
                Return _CleanableConfiguration
            End Get
            Set(value As MemoryManager.CleanConfiguration)
                _CleanableConfiguration = value
            End Set
        End Property

        <NonSerialized> Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged
        Sub OnPropertyChanged(<Runtime.CompilerServices.CallerMemberName> Optional CallerName As String = Nothing)
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(CallerName))
        End Sub

        Overridable Sub RefreshTagsFromFile(Optional SkipCover As Boolean = False)
            If Not IO.File.Exists(Path) Then Return
            Utilities.DebugMode.Instance.Log(Of Metadata)("Attempting to refresh a metadata from file, Path:=" & Path)
            Utilities.DebugMode.Instance.Log(Of Metadata)("Refreshing metadata tags...")
            Dim Tags As TagLib.File = Nothing
            Try
                Tags = Utilities.SharedProperties.Instance.RequestTags(Path, If(SkipCover, TagLib.ReadStyle.Average Or TagLib.ReadStyle.PictureLazy, TagLib.ReadStyle.Average))
            Catch ex As Exception
                Utilities.DebugMode.Instance.Log(Of Metadata)("Error while reading tags: " & ex.ToString)
            End Try
            If Tags IsNot Nothing Then
                Title = Tags.Tag.Title
                Artists = Tags.Tag.Performers
                Album = Tags.Tag.Album
                Year = Tags.Tag.Year
                Length = Tags.Properties.Duration.TotalSeconds
                Size = Tags.Length
                HasCover = (Tags.Tag.Pictures.Length > 0)
                Bitrate = Tags.Properties.AudioBitrate
                Channels = Tags.Properties.AudioChannels
                If Tags.Properties.Codecs?.Any Then Codecs = Tags.Properties.Codecs.Select(Of String)(Function(k) k.Description).ToArray
            End If
                If Not SkipCover Then
                Dim Covers As New List(Of ImageSource)
                If Tags IsNot Nothing Then
                    For Each picture In Tags?.Tag.Pictures
                        Dim BI As BitmapImage = If(OverrideProperties.Instance.Locked(OverrideProperties.LockType.Metadata_DecodeToThumbnail), New IO.MemoryStream(picture.Data.Data).ToBitmapSource(OverrideProperties.Instance.Value(OverrideProperties.LockType.Metadata_DecodePixelWidth), OverrideProperties.Instance.Value(OverrideProperties.LockType.Metadata_DecodePixelHeight)), New IO.MemoryStream(picture.Data.Data).ToBitmapSource)
                        If BI IsNot Nothing Then Covers.Add(BI)
                    Next
                End If
                Covers = Covers
            End If
            If String.IsNullOrEmpty(Title) Then
                Title = IO.Path.GetFileNameWithoutExtension(Path)
            End If
            Utilities.DebugMode.Instance.Log(Of Metadata)("Done refreshing metadata.")
        End Sub
        Overridable Sub RefreshTagsFromFile_ThreadSafe(Optional SkipCover As Boolean = False)
            If Not IO.File.Exists(Path) Then Return
            Utilities.DebugMode.Instance.Log(Of Metadata)("Attempting to refresh a metadata from file, Path:=" & Path)
            Utilities.DebugMode.Instance.Log(Of Metadata)("Refreshing metadata tags...")
            Dim Tags As TagLib.File = Nothing
            Try
                Tags = Utilities.SharedProperties.Instance.RequestTags(Path, If(SkipCover, TagLib.ReadStyle.Average Or TagLib.ReadStyle.PictureLazy, TagLib.ReadStyle.Average))
            Catch ex As Exception
                Utilities.DebugMode.Instance.Log(Of Metadata)("Error while reading tags: " & ex.ToString)
            End Try
            If Tags IsNot Nothing Then
                Application.Current.Dispatcher.Invoke(Sub()
                                                          Title = Tags.Tag.Title
                                                          Artists = Tags.Tag.Performers
                                                          Album = Tags.Tag.Album
                                                          Year = Tags.Tag.Year
                                                          Length = Tags.Properties.Duration.TotalSeconds
                                                          Size = Tags.Length
                                                          HasCover = (Tags.Tag.Pictures.Length > 0)
                                                          Bitrate = Tags.Properties.AudioBitrate
                                                          Channels = Tags.Properties.AudioChannels
                                                          If Tags.Properties.Codecs?.Any Then Codecs = Tags.Properties.Codecs.Select(Of String)(Function(k) k.Description).ToArray
                                                      End Sub)
            End If
            If Not SkipCover Then
                Dim Covers As New List(Of ImageSource)
                If Tags IsNot Nothing Then
                    For Each picture In Tags?.Tag.Pictures
                        Dim BI As BitmapImage = If(OverrideProperties.Instance.Locked(OverrideProperties.LockType.Metadata_DecodeToThumbnail), New IO.MemoryStream(picture.Data.Data).ToBitmapSource(OverrideProperties.Instance.Value(OverrideProperties.LockType.Metadata_DecodePixelWidth), OverrideProperties.Instance.Value(OverrideProperties.LockType.Metadata_DecodePixelHeight)), New IO.MemoryStream(picture.Data.Data).ToBitmapSource)
                        If BI IsNot Nothing Then Covers.Add(BI)
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
        Overridable Async Sub EnsureCovers(Optional DecodeToThumbnail As Boolean = False, Optional ThumbPxWidth As Integer = 150, Optional ThumbPxHeight As Integer = 150, Optional FreezeThumbs As Boolean = False)
            If (Covers Is Nothing OrElse Covers.Count = 0) AndAlso IO.File.Exists(Path) Then
                Dim Tags As TagLib.File = Nothing
                Try
                    Tags = Utilities.SharedProperties.Instance.RequestTags(Path, TagLib.ReadStyle.Average)
                Catch ex As Exception
                    Utilities.DebugMode.Instance.Log(Of Metadata)("Error while reading tags: " & ex.ToString)
                End Try
                If Tags IsNot Nothing AndAlso Tags.Tag.Pictures.Length > 0 Then
                    HasCover = True
                    Dim Covers As New List(Of ImageSource)
                    For Each picture In Tags.Tag.Pictures
                        Dim BI As BitmapImage
                        If OverrideProperties.Instance.Locked(OverrideProperties.LockType.Metadata_DecodeToThumbnail) Then
                            If OverrideProperties.Instance.Value(OverrideProperties.LockType.Metadata_DecodeToThumbnail) Then
                                BI = New IO.MemoryStream(picture.Data.Data).ToBitmapSource(OverrideProperties.Instance.Value(OverrideProperties.LockType.Metadata_DecodePixelWidth), OverrideProperties.Instance.Value(OverrideProperties.LockType.Metadata_DecodePixelHeight))
                            Else
                                BI = New IO.MemoryStream(picture.Data.Data).ToBitmapSource
                            End If
                        Else
                            If DecodeToThumbnail Then
                                BI = New IO.MemoryStream(picture.Data.Data).ToBitmapSource(ThumbPxWidth, ThumbPxHeight)
                            Else
                                BI = New IO.MemoryStream(picture.Data.Data).ToBitmapSource
                            End If
                        End If
                        If FreezeThumbs Then BI.Freeze()
                        Covers.Add(BI)
                    Next
                    Me.Covers = Covers
                    Tags.Dispose()
                End If
            ElseIf Not String.IsNullOrEmpty(CoverLink) Then
                Me.Covers = New ImageSource() {New Uri(CoverLink).ToBitmapSource()}
            ElseIf Not String.IsNullOrEmpty(ThumbnailCoverLink) Then
                Me.Covers = New ImageSource() {New Uri(ThumbnailCoverLink).ToBitmapSource()}
            ElseIf (Covers Is Nothing OrElse Covers.Count = 0) AndAlso (Location = FileLocation.Remote OrElse Location = FileLocation.Cached) Then
                If Provider IsNot Nothing Then
                    Dim Thumb = Await Provider.FetchThumbnail
                    If Thumb IsNot Nothing Then
                        Me.Covers = New ImageSource() {Thumb}
                    End If
                End If
            End If
        End Sub
        ''' <summary>
        ''' **NOT IMPLEMENTED**
        ''' Creates a ready-to-use HStream, Must be overriden in classes that doesn't point to a local or remote file , and must instead supply the media data.
        ''' The <see cref="Player"/> will automatically use this if <see cref="Location"/> is set to <see cref="FileLocation.Internal"/> or <see cref="FileLocation.Cached"/>
        ''' </summary>
        ''' <returns>An HStream Pointer to the Pre-loaded (or Late-Loaded) stream</returns>
        Public Overridable Function CreateStream() As Integer
            Return 0
        End Function
        Public Sub LoadFromTFile(file As TagLib.File, Optional SkipLength As Boolean = False)
            Title = file.Tag.Title
            Artists = file.Tag.Performers
            Album = file.Tag.Album
            Year = file.Tag.Year
            If Not SkipLength Then Length = file.Properties.Duration.TotalSeconds
            Size = file.Length
            HasCover = (file.Tag.Pictures.Length > 0)
            Bitrate = file.Properties.AudioBitrate
            Channels = file.Properties.AudioChannels
            If file.Properties.Codecs?.Any Then Codecs = file.Properties.Codecs.Select(Of String)(Function(k) k.Description).ToArray
            If HasCover Then
                Dim Covers As New List(Of ImageSource)
                For Each picture In file.Tag.Pictures
                    Application.Current.Dispatcher.Invoke(Sub()
                                                              Dim BI As BitmapImage = If(OverrideProperties.Instance.Locked(OverrideProperties.LockType.Metadata_DecodeToThumbnail), New IO.MemoryStream(picture.Data.Data).ToBitmapSource(OverrideProperties.Instance.Value(OverrideProperties.LockType.Metadata_DecodePixelWidth), OverrideProperties.Instance.Value(OverrideProperties.LockType.Metadata_DecodePixelHeight)), New IO.MemoryStream(picture.Data.Data).ToBitmapSource)
                                                              If BI IsNot Nothing Then Covers.Add(BI)
                                                          End Sub)
                Next
                Me.Covers = Covers
            End If
        End Sub

        Shared Function FromFile(path As String, Optional SkipCover As Boolean = False, Optional SearchLibraryOnly As Boolean = False) As Metadata
            ''Utilities.DebugMode.Instance.Log(Of Metadata)("Attempting to create a metadata from file, Path:=" & path & ", SkipCover:=" & SkipCover & ", SearchLibraryOnly:=" & SearchLibraryOnly)
            If IO.File.Exists(path) Then
                'Check if available in library                
                Dim MData = Utilities.SharedProperties.Instance.Library.FirstOrDefault(Function(k) k.Path = path)
                If MData IsNot Nothing Then
                    ''Utilities.DebugMode.Instance.Log(Of Metadata)("Data found, Returning...")
                    Return MData
                End If
                If SearchLibraryOnly Then Return Nothing
                'Create one
                ''Utilities.DebugMode.Instance.Log(Of Metadata)("Couldn't find metadata, Creating one...")
                Dim Tags As TagLib.File = Nothing
                Try
                    Tags = Utilities.SharedProperties.Instance.RequestTags(path, If(SkipCover, TagLib.ReadStyle.Average Or TagLib.ReadStyle.PictureLazy, TagLib.ReadStyle.Average))
                Catch ex As Exception
                    Utilities.DebugMode.Instance.Log(Of Metadata)("Error while reading tags: " & ex.ToString)
                End Try
                Dim MD As Metadata = If(Tags Is Nothing, New Metadata With {.Location = FileLocation.Local, .Path = path, .OriginalPath = path, .Provider = New IOMediaProvider(path)},
                    New Metadata With {.Location = FileLocation.Local, .OriginalPath = path, .Provider = New IOMediaProvider(path), .Title = Tags.Tag.Title,
                    .Artists = Tags.Tag.Performers, .Album = Tags.Tag.Album, .Year = Tags.Tag.Year, .Path = path, .Length = Tags.Properties.Duration.TotalSeconds,
                    .HasCover = (Tags.Tag.Pictures.Length > 0), .Size = Tags.Length, .Bitrate = Tags.Properties.AudioBitrate, .Channels = Tags.Properties.AudioChannels})
                If MD IsNot Nothing AndAlso Tags?.Properties.Codecs?.Any Then MD.Codecs = Tags.Properties.Codecs.Select(Of String)(Function(k) k?.Description).ToArray
                MD.Extension = IO.Path.GetExtension(path).TrimStart("."c)
                If Not SkipCover Then
                    Dim Covers As New List(Of ImageSource)
                    If Tags IsNot Nothing Then
                        For Each picture In Tags?.Tag.Pictures
                            Dim BI As BitmapImage = New IO.MemoryStream(picture.Data.Data).ToBitmapSource
                            If BI IsNot Nothing Then Covers.Add(BI)
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
        ''' Searches Library and Local Cache for corresponding <see cref="Metadata"/>
        ''' </summary>
        ''' <param name="UID"></param>
        ''' <returns></returns>
        Shared Function FromUID(UID As String) As Metadata
            Utilities.DebugMode.Instance.Log(Of Metadata)("Attempting to create a metadata from UID, UID:=" & UID)
            'Check if available in library                
            Dim MData = Utilities.SharedProperties.Instance.Library.FirstOrDefault(Function(k) k.UID = UID)
            If MData Is Nothing Then
                If Utilities.SharedProperties.Instance.RemoteLibrary.ContainsID(UID) Then
                    Return Utilities.SharedProperties.Instance.RemoteLibrary.GetItem(UID)
                ElseIf io.file.Exists(uid) Then
                    Return Metadata.FromFile(UID, True)
                End If
            Else
                Utilities.DebugMode.Instance.Log(Of Metadata)("Data found, Returning...")
                Return MData
            End If
            Utilities.DebugMode.Instance.Log(Of Metadata)("Couldn't find a metadata .UID:=" & UID)
            Return Nothing
        End Function
        Shared Function FromCache(originalPath As String, Optional IsUID As Boolean = False) As CachedMetadata
            Utilities.DebugMode.Instance.Log(Of Metadata)("Attempting to create a metadata from cache, Path:=" & originalPath)
            'Check if available in library                
            If Utilities.SharedProperties.Instance.RemoteLibrary.ContainsID(If(IsUID, originalPath, CommonFunctions.URLtoUID(originalPath))) Then
                Dim MData = Utilities.SharedProperties.Instance.RemoteLibrary.GetItem(If(IsUID, originalPath, CommonFunctions.URLtoUID(originalPath))) '.FirstOrDefault(Function(k) CommonFunctions.CheckUID(k.UID, originalPath))
                If MData IsNot Nothing Then
                    Utilities.DebugMode.Instance.Log(Of Metadata)("Data found, Returning...")
                    If MData.Provider Is Nothing Then
                        MData.Provider = New CacheMediaProvider(If(IsUID, originalPath, CommonFunctions.URLtoUID(originalPath)))
                    End If
                    Return MData
                End If
            End If
            Utilities.DebugMode.Instance.Log(Of Metadata)("Couldn't create a cached metadata.")
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
                            _IsActive = My.Computer.Network.Ping(Path) 'TODO to be changed to newer implementation
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
            Return $"{Title} By {If(Artists IsNot Nothing, String.Join(";", Artists), "N/A")} From {Album} With Duration {LengthString} At {Location}:{Path}"
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
            Return New Metadata() With {.Location = Location, .Album = Album, .Artists = Artists, .Covers = Covers, .Genres = Genres,
                .Index = Index, .IsFavorite = IsFavorite, .Length = Length, .Size = Size, .Path = Path, .PlayCount = PlayCount, .Title = Title, .BlockDOWNLOADPROC = BlockDOWNLOADPROC,
                .OriginalPath = OriginalPath, .Provider = Provider, .HasCover = HasCover}
        End Function

        ''' <summary>
        ''' Copies this instance primary metadata(only data read from tag) to another reference
        ''' </summary>
        ''' <param name="Metadata"></param>
        Public Sub CopyTo(Metadata As Metadata)
            With Metadata
                .Album = Album
                .Artists = Artists
                .Covers = Covers
                .Genres = Genres
                .Length = Length
                .Size = Size
                .Extension = Extension
                .Path = Path
                .Title = Title
                .OriginalPath = OriginalPath
                .HasCover = HasCover
                .Bitrate = Bitrate
                .Channels = Channels
                .Codecs = Codecs
            End With
        End Sub

        ''' <summary>
        ''' Copies all properties of this instance to another reference except:<see cref="Location"/>, <see cref="Provider"/>
        ''' </summary>
        ''' <param name="Metadata"></param>
        Public Sub CopyAllTo(Metadata As Metadata)
            With Metadata
                .Album = Album
                .Artists = Artists
                .Bitrate = Bitrate
                .Channels = Channels
                .Codecs = Codecs
                .CoverLink = CoverLink
                .Covers = Covers
                .Extension = Extension
                .Genres = Genres
                .Index = Index
                .IsFavorite = IsFavorite
                .Length = Length
                .Size = Size
                .Path = Path
                .Title = Title
                .ThumbnailCoverLink = ThumbnailCoverLink
                .BlockDOWNLOADPROC = BlockDOWNLOADPROC
                .OriginalPath = OriginalPath
                .HasCover = HasCover
                .CoverLink = CoverLink
            End With
        End Sub
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
            Internal
            Cached
        End Enum
    End Class
End Namespace