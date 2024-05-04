Imports EDeezer = E.Deezer
Imports System.ComponentModel
Imports QuickBeat.Interfaces
Imports QuickBeat.Classes
Imports QuickBeat.Player

Namespace Utilities
    Public Class DeezerProvider
        Inherits DependencyObject
        Implements IStartupItem

        Public Event FetchCompleted(sender As DeezerProvider)

        Public BlockAutoFetch As Boolean = True
        Private _RedundancyCounter As Integer = 0
        Private _RedundancyIsCounting As Boolean = False
        Private ReadOnly _PlayerMediaLoadedHandler As New Player.Player.MetadataChangedEventHandler(Sub()
                                                                                                        Fetch()
                                                                                                    End Sub)

        Private _BindFetchToDefaultPlayerMediaLoaded As Boolean = False
        ''' <summary>
        ''' Not a <see cref="DependencyProperty"/>
        ''' </summary>
        ''' <returns></returns>
        Public Property BindFetchToDefaultPlayerMediaLoaded As Boolean
            Get
                Return _BindFetchToDefaultPlayerMediaLoaded
            End Get
            Set(value As Boolean)
                If SharedProperties.Instance?.Player IsNot Nothing Then
                    If value Then
                        AddHandler SharedProperties.Instance.Player.MetadataChanged, _PlayerMediaLoadedHandler
                    Else
                        RemoveHandler SharedProperties.Instance.Player.MetadataChanged, _PlayerMediaLoadedHandler
                    End If
                    _BindFetchToDefaultPlayerMediaLoaded = value
                End If
            End Set
        End Property

        Public Shared ArtistProperty = DependencyProperty.Register("Artist", GetType(Player.MetadataGroup), GetType(DeezerProvider))
        Property Artist As Player.MetadataGroup
            Get
                Return GetValue(ArtistProperty)
            End Get
            Set(value As Player.MetadataGroup)
                If Artist IsNot Nothing AndAlso Artist.IsInUse = False Then
                    Artist.FreeCovers()
                    For Each meta In Artist
                        If Not meta.IsInUse Then meta.FreeCovers()
                    Next
                End If
                SetValue(ArtistProperty, value)
            End Set
        End Property

        Public Shared RelatedArtistProperty = DependencyProperty.Register("RelatedArtist", GetType(Player.MetadataGroup), GetType(DeezerProvider))
        Property RelatedArtist As Player.MetadataGroup
            Get
                Return GetValue(RelatedArtistProperty)
            End Get
            Set(value As Player.MetadataGroup)
                If RelatedArtist IsNot Nothing AndAlso RelatedArtist.IsInUse = False Then
                    RelatedArtist.FreeCovers()
                    For Each meta In RelatedArtist
                        If Not meta.IsInUse Then meta.FreeCovers()
                    Next
                End If
                SetValue(RelatedArtistProperty, value)
            End Set
        End Property

        Public Shared TrackProperty = DependencyProperty.Register("Track", GetType(Player.Metadata), GetType(DeezerProvider))
        Property Track As Player.Metadata
            Get
                Return GetValue(TrackProperty)
            End Get
            Set(value As Player.Metadata)
                If Track IsNot Nothing Then
                    If Not Track.IsInUse Then Track.FreeCovers()
                End If
                SetValue(TrackProperty, value)
            End Set
        End Property

        Public Shared UseDefaultQueryProperty = DependencyProperty.Register("UseDefaultQuery", GetType(Boolean), GetType(DeezerProvider), New UIPropertyMetadata(False, New PropertyChangedCallback(Sub(d, e)
                                                                                                                                                                                                        Dim ref = TryCast(d, DeezerProvider)
                                                                                                                                                                                                        If ref IsNot Nothing AndAlso e.NewValue Then
                                                                                                                                                                                                            Dim _BlockAF As Boolean = ref.BlockAutoFetch
                                                                                                                                                                                                            ref.BlockAutoFetch = False
                                                                                                                                                                                                            ref.ArtistQuery = "REOL" '49469512
                                                                                                                                                                                                            ref.TrackQuery = "Q?" '2127495917
                                                                                                                                                                                                            ref.BlockAutoFetch = _BlockAF
                                                                                                                                                                                                        End If
                                                                                                                                                                                                    End Sub)))
        ''' <summary>
        ''' Uses the dev(me :)) query
        ''' </summary>
        ''' <returns></returns>
        Property UseDefaultQuery As Boolean
            Get
                Return GetValue(UseDefaultQueryProperty)
            End Get
            Set(value As Boolean)
                SetValue(UseDefaultQueryProperty, value)
            End Set
        End Property

        Public Shared FallBackToDefaultQueryProperty = DependencyProperty.Register("FallBackToDefaultQuery", GetType(Boolean), GetType(DeezerProvider))
        Property FallBackToDefaultQuery As Boolean
            Get
                Return GetValue(FallBackToDefaultQueryProperty)
            End Get
            Set(value As Boolean)
                SetValue(FallBackToDefaultQueryProperty, value)
            End Set
        End Property
        Public Shared DecodeToThumbnailsProperty = DependencyProperty.Register("DecodeToThumbnails", GetType(Boolean), GetType(DeezerProvider))
        Property DecodeToThumbnails As Boolean
            Get
                Return GetValue(DecodeToThumbnailsProperty)
            End Get
            Set(value As Boolean)
                SetValue(DecodeToThumbnailsProperty, value)
            End Set
        End Property
        Public Shared DecodeWidthProperty = DependencyProperty.Register("DecodeWidth", GetType(Integer), GetType(DeezerProvider), New PropertyMetadata(150))
        Property DecodeWidth As Integer
            Get
                Return GetValue(DecodeWidthProperty)
            End Get
            Set(value As Integer)
                SetValue(DecodeWidthProperty, value)
            End Set
        End Property
        Public Shared DecodeHeightProperty = DependencyProperty.Register("DecodeHeight", GetType(Integer), GetType(DeezerProvider), New PropertyMetadata(150))
        Property DecodeHeight As Integer
            Get
                Return GetValue(DecodeHeightProperty)
            End Get
            Set(value As Integer)
                SetValue(DecodeHeightProperty, value)
            End Set
        End Property

        Public Shared FetchPropProperty = DependencyProperty.Register("FetchProp", GetType(Boolean), GetType(DeezerProvider), New UIPropertyMetadata(False, New PropertyChangedCallback(Sub(d, e)
                                                                                                                                                                                            If e.NewValue Then
                                                                                                                                                                                                Dim ref = TryCast(d, DeezerProvider)
                                                                                                                                                                                                If ref IsNot Nothing Then
                                                                                                                                                                                                    ref.Fetch()
                                                                                                                                                                                                    ref.SetValue(e.Property, False)
                                                                                                                                                                                                End If
                                                                                                                                                                                            End If
                                                                                                                                                                                        End Sub)))
        ''' <summary>
        ''' Helper property to fetch from xaml
        ''' </summary>
        ''' <returns></returns>
        Property FetchProp As Boolean
            Get
                Return GetValue(FetchPropProperty)
            End Get
            Set(value As Boolean)
                SetValue(FetchPropProperty, value)
            End Set
        End Property

        Public Shared CombineQueriesProperty = DependencyProperty.Register("CombineQueries", GetType(Boolean), GetType(DeezerProvider))
        ''' <summary>
        ''' Determines whether or not to lock <see cref="Track"/> to <see cref="Artist"/> top track
        ''' </summary>
        ''' <remarks>
        ''' Doesn't update, call <see cref="Fetch()"/> manually!
        ''' </remarks>
        ''' <returns></returns>
        Property CombineQueries As Boolean
            Get
                Return GetValue(CombineQueriesProperty)
            End Get
            Set(value As Boolean)
                SetValue(CombineQueriesProperty, value)
            End Set
        End Property

        Public Shared ArtistQueryProperty = DependencyProperty.Register("ArtistQuery", GetType(String), GetType(DeezerProvider), New UIPropertyMetadata(New PropertyChangedCallback(Sub(d, e)
                                                                                                                                                                                        Dim ref = TryCast(d, DeezerProvider)
                                                                                                                                                                                        If ref IsNot Nothing Then
                                                                                                                                                                                            If Not ref.BlockAutoFetch Then ref.Fetch()
                                                                                                                                                                                        End If
                                                                                                                                                                                    End Sub)))
        Property ArtistQuery As String
            Get
                Return GetValue(ArtistQueryProperty)
            End Get
            Set(value As String)
                SetValue(ArtistQueryProperty, value)
            End Set
        End Property

        Public Shared TrackQueryProperty = DependencyProperty.Register("TrackQuery", GetType(String), GetType(DeezerProvider), New UIPropertyMetadata(New PropertyChangedCallback(Sub(d, e)
                                                                                                                                                                                      Dim ref = TryCast(d, DeezerProvider)
                                                                                                                                                                                      If ref IsNot Nothing Then
                                                                                                                                                                                          If Not ref.BlockAutoFetch Then ref.Fetch()
                                                                                                                                                                                      End If
                                                                                                                                                                                  End Sub)))
        Property TrackQuery As String
            Get
                Return GetValue(TrackQueryProperty)
            End Get
            Set(value As String)
                SetValue(TrackQueryProperty, value)
            End Set
        End Property

        Public Shared ArtistInfoOnlyProperty = DependencyProperty.Register("ArtistInfoOnly", GetType(Boolean), GetType(DeezerProvider), New PropertyMetadata(True))
        ''' <summary>
        ''' Determines whether or not fill <see cref="Artist"/> Items
        ''' </summary>        
        ''' <returns></returns>
        Property ArtistInfoOnly As Boolean
            Get
                Return GetValue(ArtistInfoOnlyProperty)
            End Get
            Set(value As Boolean)
                SetValue(ArtistInfoOnlyProperty, value)
            End Set
        End Property

        Public Shared IsBusyProperty = DependencyProperty.Register("IsBusy", GetType(Boolean), GetType(DeezerProvider))
        Property IsBusy As Boolean
            Get
                Return GetValue(IsBusyProperty)
            End Get
            Set(value As Boolean)
                SetValue(IsBusyProperty, value)
            End Set
        End Property

        Public Shared TagProperty = DependencyProperty.Register("Tag", GetType(Object), GetType(DeezerProvider))
        Property Tag As Object
            Get
                Return GetValue(TagProperty)
            End Get
            Set(value As Object)
                SetValue(TagProperty, value)
            End Set
        End Property

        Private LocalMetadataGroupDB As Classes.QBDatabase(Of MetadataGroup)
        Private LocalThumbnailDB As Classes.QBDatabase(Of String)
        Private _IsInitialized As Boolean = False

        ''' <summary>
        ''' Has a redundancy detection, in a series of calls on the last call is executed after 1 sec delay
        ''' </summary>
        Public Async Sub Fetch()
            If Not _IsInitialized Then
                Configuration.Name &= " " & Tag.ToString
                Utilities.SharedProperties.Instance?.ItemsConfiguration.Add(Configuration)
                Init()
                _IsInitialized = True
            End If
            If IsBusy Then Return
            If Not Utilities.SharedProperties.Instance.IsInternetConnected Then Return
            If String.IsNullOrEmpty(ArtistQuery) AndAlso String.IsNullOrEmpty(TrackQuery) Then Return
            If _RedundancyIsCounting Then
                _RedundancyCounter = 0
                Return
            Else
                _RedundancyIsCounting = True
            End If
            Do While _RedundancyIsCounting
                Await Task.Delay(10)
                _RedundancyCounter += 1
                If _RedundancyCounter = 100 Then '1 sec has passed
                    _RedundancyCounter = 0
                    _RedundancyIsCounting = False
                    Exit Do
                End If
            Loop
            Configuration.SetStatus("Acquiring data...", 0)
            Dim Session As EDeezer.Deezer
            Try
                Session = EDeezer.DeezerSession.CreateNew()
            Catch ex As Exception
                Utilities.DebugMode.Instance?.Log(Of DeezerProvider)(ex.ToString)
                Configuration.SetStatus("All Good", 100)
                Configuration.SetError(True, ex)
                Return
            End Try
            IsBusy = True
            Dim aID As UInteger = 0
            Dim tID As UInteger = 0
            Dim SkipArtist, SkipTrack, FoundArtistCache As Boolean
            Configuration.SetStatus("Acquiring IDs...", 0)
            Try 'As much i hate multiple try blocks, it's inevitable in this case :'(
                If UseDefaultQuery Then
                    aID = 49469512
                    tID = 2127495917
                Else
                    If If(Artist Is Nothing, "", Artist.Name.ToLower) <> If(ArtistQuery, "").ToLower Then
                        If LocalMetadataGroupDB.ContainsID($"G{ArtistQuery}") Then
                            SkipArtist = True
                            FoundArtistCache = True
                        Else
                            Dim aResult = If(String.IsNullOrEmpty(ArtistQuery), Nothing, Await Session.Search.Artists(ArtistQuery, 0, 1))
                            If aResult Is Nothing OrElse aResult.Count = 0 Then
                                If FallBackToDefaultQuery Then aID = 49469512 'REOL
                            Else
                                aID = If(aResult.FirstOrDefault?.Id, If(FallBackToDefaultQuery, 49469512, 0))
                            End If
                        End If
                    Else
                        SkipArtist = True
                    End If
                    If If(Track Is Nothing, "", Track.Title.ToLower) <> If(TrackQuery, "").ToLower Then
                        Dim tResult = If(String.IsNullOrEmpty(TrackQuery), Nothing, Await Session.Search.Tracks(TrackQuery, 0, 1))
                        If tResult Is Nothing OrElse tResult.Count = 0 Then
                            If FallBackToDefaultQuery Then tID = 2127495917 'Q?
                        Else
                            tID = If(tResult.FirstOrDefault?.Id, If(FallBackToDefaultQuery, 2127495917, 0))
                        End If
                    Else
                        SkipTrack = True
                    End If
                End If
            Catch ex As Exception
                Configuration.SetStatus("All Good", 100)
                Configuration.SetError(True, ex)
                Utilities.DebugMode.Instance?.Log(Of DeezerProvider)(ex.ToString)
            End Try
            If Not SkipTrack Then
                Configuration.SetStatus("Decoding Track Data", 50)
                Track = Nothing
                Try
                    If tID <> 0 Then
                        Dim tr = Await Session.Browse.GetTrackById(tID)
                        If tr IsNot Nothing AndAlso Not String.IsNullOrEmpty(tr.Preview) Then
                            Dim MD As New Player.Metadata() With {.Location = Player.Metadata.FileLocation.Remote, .OriginalPath = tr.Link, .Provider = New Deezer.DeezerMediaProvider(tr.Id), .BlockDOWNLOADPROC = True, .Album = tr.AlbumName, .Year = tr.ReleaseDate.Year, .Artists = New String() {tr.ArtistName}, .PlayCount = tr.Rank, .Index = tr.Number, .Title = tr.Title, .Length = tr.Duration, .Path = tr.Preview, .UID = tID}
                            Dim url = If(tr.HasPicture(EDeezer.Api.PictureSize.Medium), tr.GetPicture(EDeezer.Api.PictureSize.Medium), If(tr.HasPicture(EDeezer.Api.PictureSize.Large), tr.GetPicture(EDeezer.Api.PictureSize.Large), If(tr.HasPicture(EDeezer.Api.PictureSize.Small), tr.GetPicture(EDeezer.Api.PictureSize.Small), If(tr.HasPicture(EDeezer.Api.PictureSize.ExtraLarge), tr.GetPicture(EDeezer.Api.PictureSize.ExtraLarge), Nothing))))
                            Dim bi As BitmapImage = Nothing
                            If DecodeToThumbnails Then
                                bi = New Uri(url).ToBitmapSource(DecodeWidth, DecodeHeight)
                            Else
                                bi = New Uri(url).ToBitmapSource
                            End If
                            MD.ThumbnailCoverLink = tr.GetPicture(EDeezer.Api.PictureSize.Medium)
                            MD.CoverLink = url
                            MD.Covers = New ImageSource() {bi}
                            Me.Track = MD
                        Else
                            Me.Track = Nothing
                        End If
                    End If
                Catch ex As Exception
                    Configuration.SetError(True, ex)
                    Utilities.DebugMode.Instance?.Log(Of DeezerProvider)(ex.ToString)
                End Try
            End If
            If Not SkipArtist Then
                Configuration.SetStatus("Decoding Artist Data", 80)
                Artist = Nothing
                Try
                    If aID <> 0 Then
                        'Bind Artist
                        Dim Artist = Await Session.Browse.GetArtistById(aID)
                        If Artist Is Nothing AndAlso FallBackToDefaultQuery Then
                            Artist = Await Session.Browse.GetArtistById(49469512)
                        End If
                        If Artist IsNot Nothing Then
                            If LocalMetadataGroupDB.ContainsID($"G{Artist.Name}") Then
                                Dim mdg = LocalMetadataGroupDB.GetItem($"G{Artist.Name}")
                                Dim thumb = LocalThumbnailDB.GetItem($"G{Artist.Name}")
                                If Not String.IsNullOrEmpty(thumb) Then
                                    Dim mem As New IO.MemoryStream(Convert.FromBase64String(thumb))
                                    Dim pngDeco As New PngBitmapDecoder(mem, BitmapCreateOptions.None, BitmapCacheOption.Default)
                                    mdg.Cover = pngDeco.Frames.FirstOrDefault
                                    Dispatcher.Invoke(Sub() Me.Artist = mdg)
                                Else
                                    Dim bi As New BitmapImage
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
                                    bi.BeginInit()
                                    bi.UriSource = tUri
                                    bi.EndInit()
                                    'Cache Thumb
                                    Dim cmem As New IO.MemoryStream
                                    Dim pngEnco As New PngBitmapEncoder
                                    pngEnco.Frames.Add(BitmapFrame.Create(bi))
                                    pngEnco.Save(cmem)
#Disable Warning
                                    LocalThumbnailDB.AddItemAsync(Convert.ToBase64String(cmem.ToArray), $"G{mdg.Name}")
#Enable Warning
                                    cmem.Dispose()
                                    mdg.Cover = bi
                                    Dispatcher.Invoke(Sub() Me.Artist = mdg)
                                End If
                                Artist = Nothing
                                End If
                            End If
                        If Artist IsNot Nothing Then
                            Dim MDG As New Player.MetadataGroup() With {.Type = MetadataGroup.GroupType.Artist, .Name = Artist.Name, .Category = "Deezer", .PlayCount = Convert.ToInt32(Artist.Fans), .Tag = Artist.Link, .UID = aID}
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
                                Dim bi As BitmapSource = Nothing
                                If DecodeToThumbnails Then
                                    bi = tUri.ToBitmapSource(DecodeWidth, DecodeHeight)
                                Else
                                    bi = tUri.ToBitmapSource
                                End If
                                MDG.Cover = bi
                                MDG.IsCoverLocked = True
                            End If
                            If Not ArtistInfoOnly Then
                                Dim ttl = Await Artist.GetTopTracks(0, 10)
                                If ttl.Count = 0 Then
                                    Dim album = (Await Artist.GetAlbums(0, 1))?.FirstOrDefault
                                    If album Is Nothing Then
                                        ttl = Await Artist.GetSmartRadio(0, 15)
                                    Else
                                        ttl = Await album.GetTracks()
                                    End If
                                End If
                                For Each rTr In ttl
                                    Dim tr = Await Session.Browse.GetTrackById(rTr.Id)
                                    Dim MD As New Player.Metadata() With {.Location = Player.Metadata.FileLocation.Remote, .OriginalPath = tr.Link, .Provider = New Deezer.DeezerMediaProvider(tr.Id), .BlockDOWNLOADPROC = True, .Album = tr.AlbumName, .Year = tr.ReleaseDate.Year, .Artists = New String() {tr.ArtistName}, .PlayCount = tr.Rank, .Index = tr.Number, .Title = tr.Title, .Length = tr.Duration, .Path = tr.Preview}
                                    Dim uri As Uri = Nothing
                                    If tr.HasPicture(EDeezer.Api.PictureSize.Medium) Then
                                        uri = New Uri(tr.GetPicture(EDeezer.Api.PictureSize.Medium))
                                    ElseIf tr.HasPicture(EDeezer.Api.PictureSize.Large) Then
                                        uri = New Uri(tr.GetPicture(EDeezer.Api.PictureSize.Large))
                                    ElseIf tr.HasPicture(EDeezer.Api.PictureSize.Small) Then
                                        uri = New Uri(tr.GetPicture(EDeezer.Api.PictureSize.Small))
                                    ElseIf tr.HasPicture(EDeezer.Api.PictureSize.ExtraLarge) Then
                                        uri = New Uri(tr.GetPicture(EDeezer.Api.PictureSize.ExtraLarge))
                                    End If
                                    If uri IsNot Nothing Then
                                        Dim bi As BitmapImage = Nothing
                                        If DecodeToThumbnails Then
                                            bi = uri.ToBitmapSource(DecodeWidth, DecodeHeight)
                                        Else
                                            bi = uri.ToBitmapSource
                                        End If
                                        MD.ThumbnailCoverLink = uri.ToString 'tr.GetPicture(EDeezer.Api.PictureSize.Medium)
                                        'MD.CoverLink = uri.ToString
                                        'MD.Covers = New ImageSource() {bi} 'Removed due to the app being able to get cover on demand and link is same anyways and... RAM
                                    End If
                                    MDG.Add(MD)
                                Next
                            End If
                            If Me.Artist IsNot Nothing Then
                                Me.Artist.IsCoverLocked = False
                                Me.Artist.FreeCovers()
                            End If
                            Me.Artist = MDG
                            'Cache                            
                            Await LocalMetadataGroupDB.AddItemAsync(MDG, $"G{MDG.Name}")
                            If tUri IsNot Nothing Then
                                If TryCast(MDG.Cover, BitmapSource)?.IsDownloading Then
                                    Dim name = Me.Artist.Name 'temp var
                                    AddHandler CType(MDG.Cover, BitmapSource).DownloadCompleted, New EventHandler(Async Sub(s, e)
                                                                                                                      'Dim mem As New IO.MemoryStream
                                                                                                                      'Dim pngEnco As New PngBitmapEncoder()
                                                                                                                      'pngEnco.Frames.Add(BitmapFrame.Create(s)) 'BitmapFrame.Create(tUri, BitmapCreateOptions.None, BitmapCacheOption.OnLoad))
                                                                                                                      'pngEnco.Save(mem)
                                                                                                                      Dim mem = Save(s)
                                                                                                                      Await LocalThumbnailDB.AddItemAsync(Convert.ToBase64String(mem.ToArray), $"G{name}")
                                                                                                                      mem.Dispose()
                                                                                                                  End Sub)
                                Else
                                    'Dim mem As New IO.MemoryStream
                                    'Dim pngEnco As New PngBitmapEncoder()
                                    'pngEnco.Frames.Add(BitmapFrame.Create(Me.Artist.Cover)) 'BitmapFrame.Create(tUri, BitmapCreateOptions.None, BitmapCacheOption.OnLoad))
                                    'pngEnco.Save(mem)
                                    Dim mem = CType(Me.Artist.Cover, BitmapSource).Save
                                    Await LocalThumbnailDB.AddItemAsync(Convert.ToBase64String(mem.ToArray), $"G{Me.Artist.Name}")
                                    mem.Dispose()
                                End if
                            End If
                            'Related
                            Dim Related = (Await Artist.GetRelated(0, 1))?.FirstOrDefault
                                If Related IsNot Nothing Then
                                    Dim RMDG As New Player.MetadataGroup() With {.Type = MetadataGroup.GroupType.Artist, .Name = Related.Name, .Category = "Deezer", .PlayCount = Convert.ToInt32(Related.Fans), .Tag = Related.Link}
                                    'Picture                    
                                    Dim rtUri As Uri = Nothing
                                    If Related.HasPicture(EDeezer.Api.PictureSize.Medium) Then
                                        rtUri = New Uri(Related.GetPicture(EDeezer.Api.PictureSize.Medium))
                                    ElseIf Related.HasPicture(EDeezer.Api.PictureSize.Large) Then
                                        rtUri = New Uri(Related.GetPicture(EDeezer.Api.PictureSize.Large))
                                    ElseIf Related.HasPicture(EDeezer.Api.PictureSize.Small) Then
                                        rtUri = New Uri(Related.GetPicture(EDeezer.Api.PictureSize.Small))
                                    ElseIf Related.HasPicture(EDeezer.Api.PictureSize.ExtraLarge) Then
                                        rtUri = New Uri(Related.GetPicture(EDeezer.Api.PictureSize.ExtraLarge))
                                    End If
                                    If rtUri IsNot Nothing Then
                                        Dim bi As BitmapImage = Nothing
                                        If DecodeToThumbnails Then
                                            bi = rtUri.ToBitmapSource(DecodeWidth, DecodeHeight)
                                        Else
                                            bi = rtUri.ToBitmapSource
                                        End If
                                        RMDG.Cover = bi
                                    End If
                                    If Not ArtistInfoOnly Then
                                        Dim rttl = Await Related.GetTopTracks(0, 10)
                                        If rttl.Count = 0 Then
                                            Dim album = (Await Related.GetAlbums(0, 1))?.FirstOrDefault
                                            If album Is Nothing Then
                                                rttl = Await Related.GetSmartRadio(0, 15)
                                            Else
                                                rttl = Await album.GetTracks()
                                            End If
                                        End If
                                        For Each rTr In rttl
                                            Dim tr = Await Session.Browse.GetTrackById(rTr.Id)
                                            Dim MD As New Player.Metadata() With {.Location = Player.Metadata.FileLocation.Remote, .OriginalPath = tr.Link, .Provider = New Deezer.DeezerMediaProvider(tr.Id), .BlockDOWNLOADPROC = True, .Album = tr.AlbumName, .Year = tr.ReleaseDate.Year, .Artists = New String() {tr.ArtistName}, .PlayCount = tr.Rank, .Index = tr.Number, .Title = tr.Title, .Length = tr.Duration, .Path = tr.Preview}
                                            Dim uri As Uri = Nothing
                                            If tr.HasPicture(EDeezer.Api.PictureSize.Medium) Then
                                                uri = New Uri(tr.GetPicture(EDeezer.Api.PictureSize.Medium))
                                            ElseIf tr.HasPicture(EDeezer.Api.PictureSize.Large) Then
                                                uri = New Uri(tr.GetPicture(EDeezer.Api.PictureSize.Large))
                                            ElseIf tr.HasPicture(EDeezer.Api.PictureSize.Small) Then
                                                uri = New Uri(tr.GetPicture(EDeezer.Api.PictureSize.Small))
                                            ElseIf tr.HasPicture(EDeezer.Api.PictureSize.ExtraLarge) Then
                                                uri = New Uri(tr.GetPicture(EDeezer.Api.PictureSize.ExtraLarge))
                                            End If
                                            If uri IsNot Nothing Then
                                                Dim bi As BitmapImage = Nothing
                                                If DecodeToThumbnails Then
                                                    bi = uri.ToBitmapSource(DecodeWidth, DecodeHeight)
                                                Else
                                                    bi = uri.ToBitmapSource
                                                End If
                                                MD.ThumbnailCoverLink = tr.GetPicture(EDeezer.Api.PictureSize.Medium)
                                                MD.CoverLink = uri.ToString
                                                MD.Covers = New ImageSource() {bi}
                                            End If
                                            RMDG.Add(MD)
                                        Next
                                    End If
                                    If Me.RelatedArtist IsNot Nothing Then
                                        Me.RelatedArtist.IsCoverLocked = False
                                        Me.RelatedArtist.FreeCovers()
                                    End If
                                    Me.RelatedArtist = RMDG
                                End If
                            End If
                        If CombineQueries Then
                            Track = Me.Artist?.Item(0)
                        ElseIf tID = 0 AndAlso Artist IsNot Nothing Then
                            Dim tr = (Await Artist.GetTopTracks(0, 1))?.FirstOrDefault 'Await Session.Browse.GetTrackById(tID)                    
                            If tr Is Nothing Then
                                tr = (Await Artist.GetSmartRadio(0, 1))?.FirstOrDefault
                            End If
                            If tr IsNot Nothing Then
                                If tr IsNot Nothing Then
                                    Dim MD As New Player.Metadata() With {.Location = Player.Metadata.FileLocation.Remote, .OriginalPath = tr.Link, .Provider = New Deezer.DeezerMediaProvider(tr.Id), .BlockDOWNLOADPROC = True, .Album = tr.AlbumName, .Year = tr.ReleaseDate.Year, .Artists = New String() {tr.ArtistName}, .PlayCount = tr.Rank, .Index = tr.Number, .Title = tr.Title, .Length = tr.Duration, .Path = tr.Preview, .UID = tr.Id}
                                    Dim url = If(tr.HasPicture(EDeezer.Api.PictureSize.Medium), tr.GetPicture(EDeezer.Api.PictureSize.Medium), If(tr.HasPicture(EDeezer.Api.PictureSize.Large), tr.GetPicture(EDeezer.Api.PictureSize.Large), If(tr.HasPicture(EDeezer.Api.PictureSize.Small), tr.GetPicture(EDeezer.Api.PictureSize.Small), If(tr.HasPicture(EDeezer.Api.PictureSize.ExtraLarge), tr.GetPicture(EDeezer.Api.PictureSize.ExtraLarge), Nothing))))
                                    Dim bi As BitmapImage = Nothing
                                    If DecodeToThumbnails Then
                                        bi = New Uri(url).ToBitmapSource(DecodeWidth, DecodeHeight)
                                    Else
                                        bi = New Uri(url).ToBitmapSource
                                    End If
                                    MD.ThumbnailCoverLink = tr.GetPicture(EDeezer.Api.PictureSize.Medium)
                                    MD.CoverLink = url
                                    MD.Covers = New ImageSource() {bi}
                                    Me.Track = MD
                                End If
                            End If
                        End If
                    End If
                Catch ex As Exception
                    Configuration.SetError(True, ex)
                    Utilities.DebugMode.Instance?.Log(Of DeezerProvider)(ex.ToString)
                End Try
            End If
            If FoundArtistCache Then
                Dim mdg = LocalMetadataGroupDB.GetItem($"G{ArtistQuery}")
                Dim thumb = LocalThumbnailDB.GetItem($"G{ArtistQuery}")
                Dim mem As New IO.MemoryStream(Convert.FromBase64String(thumb))
                Dim pngDeco As New PngBitmapDecoder(mem, BitmapCreateOptions.None, BitmapCacheOption.Default)
                mdg.Cover = pngDeco.Frames.FirstOrDefault
                Dispatcher.Invoke(Sub() Me.Artist = mdg)
            End If
            Configuration.SetStatus("All Good", 100)
            Session?.Dispose()
            IsBusy = False
            RaiseEvent FetchCompleted(Me)
        End Sub

        ''' <summary>
        ''' Doesn't have a redundancy detection, please take time to set up one or use <see cref="Fetch()"/> and monitor is busy or handle <see cref="FetchCompleted"/>
        ''' </summary>
        ''' <returns></returns>
        Public Async Function FetchAsync() As Task
            If Not _IsInitialized Then
                Configuration.Name &= " " & Tag?.ToString
                Utilities.SharedProperties.Instance?.ItemsConfiguration.Add(Configuration)
                Init()
                _IsInitialized = True
            End If
            If IsBusy Then Return
            If Not Utilities.SharedProperties.Instance.IsInternetConnected Then Return
            If String.IsNullOrEmpty(ArtistQuery) AndAlso String.IsNullOrEmpty(TrackQuery) Then Return
            Configuration.SetStatus("Acquiring data...", 0)
            Dim Session As EDeezer.Deezer
            Try
                Session = EDeezer.DeezerSession.CreateNew()
            Catch ex As Exception
                Utilities.DebugMode.Instance?.Log(Of DeezerProvider)(ex.ToString)
                Configuration.SetStatus("All Good", 100)
                Configuration.SetError(True, ex)
                Return
            End Try
            IsBusy = True
            Dim aID As UInteger = 0 '2127495917
            Dim tID As UInteger = 0 '49469512
            Dim SkipArtist, SkipTrack, FoundArtistCache As Boolean
            Configuration.SetStatus("Acquiring IDs...", 0)
            Try 'As much i hate multiple try blocks, it's inevitable in this case :'(
                If UseDefaultQuery Then
                    aID = 49469512
                    tID = 2127495917
                Else
                    If If(Artist Is Nothing, "", Artist.Name.ToLower) <> If(ArtistQuery, "").ToLower Then
                        If LocalMetadataGroupDB.ContainsID($"G{ArtistQuery}") Then
                            SkipArtist = True
                            FoundArtistCache = True
                        Else
                            Dim aResult = If(String.IsNullOrEmpty(ArtistQuery), Nothing, Await Session.Search.Artists(ArtistQuery, 0, 1))
                            If aResult Is Nothing OrElse aResult.Count = 0 Then
                                If FallBackToDefaultQuery Then aID = 49469512 'REOL
                            Else
                                aID = If(aResult.FirstOrDefault?.Id, If(FallBackToDefaultQuery, 49469512, 0))
                            End If
                        End If
                    Else
                        SkipArtist = True
                    End If
                    If If(Track Is Nothing, "", Track.Title.ToLower) <> If(TrackQuery, "").ToLower Then
                        Dim tResult = If(String.IsNullOrEmpty(TrackQuery), Nothing, Await Session.Search.Tracks(TrackQuery, 0, 1))
                        If tResult Is Nothing OrElse tResult.Count = 0 Then
                            If FallBackToDefaultQuery Then tID = 2127495917 'Q?
                        Else
                            tID = If(tResult.FirstOrDefault?.Id, If(FallBackToDefaultQuery, 2127495917, 0))
                        End If
                    Else
                        SkipTrack = True
                    End If
                End If
            Catch ex As Exception
                Configuration.SetStatus("All Good", 100)
                Configuration.SetError(True, ex)
                Utilities.DebugMode.Instance?.Log(Of DeezerProvider)(ex.ToString)
            End Try
            Configuration.SetStatus("Decoding Track Data", 50)
            If Not SkipTrack Then
                Configuration.SetStatus("Decoding Track Data", 50)
                Track = Nothing
                Try
                If tID <> 0 Then
                    Dim tr = Await Session.Browse.GetTrackById(tID)
                    If tr IsNot Nothing AndAlso Not String.IsNullOrEmpty(tr.Preview) Then
                            Dim MD As New Player.Metadata() With {.Location = Player.Metadata.FileLocation.Remote, .OriginalPath = tr.Link, .Provider = New Deezer.DeezerMediaProvider(tr.Id), .BlockDOWNLOADPROC = True, .Album = tr.AlbumName, .Year = tr.ReleaseDate.Year, .Artists = New String() {tr.ArtistName}, .PlayCount = tr.Rank, .Index = tr.Number, .Title = tr.Title, .Length = tr.Duration, .Path = tr.Preview, .UID = tID}
                            Dim url = If(tr.HasPicture(EDeezer.Api.PictureSize.Medium), tr.GetPicture(EDeezer.Api.PictureSize.Medium), If(tr.HasPicture(EDeezer.Api.PictureSize.Large), tr.GetPicture(EDeezer.Api.PictureSize.Large), If(tr.HasPicture(EDeezer.Api.PictureSize.Small), tr.GetPicture(EDeezer.Api.PictureSize.Small), If(tr.HasPicture(EDeezer.Api.PictureSize.ExtraLarge), tr.GetPicture(EDeezer.Api.PictureSize.ExtraLarge), Nothing))))
                            Dim bi As BitmapImage = Nothing
                            If DecodeToThumbnails Then
                                bi = New Uri(url).ToBitmapSource(DecodeWidth, DecodeHeight)
                            Else
                                bi = New Uri(url).ToBitmapSource
                            End If
                            MD.ThumbnailCoverLink = tr.GetPicture(EDeezer.Api.PictureSize.Medium)
                            MD.CoverLink = url
                            MD.Covers = New ImageSource() {bi}
                            Me.Track = MD
                        Else
                        Me.Track = Nothing
                    End If
                End If
            Catch ex As Exception
                Configuration.SetError(True, ex)
                Utilities.DebugMode.Instance?.Log(Of DeezerProvider)(ex.ToString)
            End Try
            End If
            Configuration.SetStatus("Decoding Artist Data", 80)
            If Not SkipArtist Then
                Configuration.SetStatus("Decoding Track Data", 50)
                Artist = Nothing
                Try
                    If aID <> 0 Then
                        'Bind Artist
                        Dim Artist = Await Session.Browse.GetArtistById(aID)
                        If Artist Is Nothing AndAlso FallBackToDefaultQuery Then
                            Artist = Await Session.Browse.GetArtistById(49469512)
                        End If
                        If Artist IsNot Nothing Then
                            If LocalMetadataGroupDB.ContainsID($"G{Artist.Name}") Then
                                Dim mdg = LocalMetadataGroupDB.GetItem($"G{Artist.Name}")
                                Dim thumb = LocalThumbnailDB.GetItem($"G{Artist.Name}")
                                Dim mem As IO.MemoryStream = Nothing
                                If String.IsNullOrEmpty(thumb) Then
                                    mem = Nothing
                                Else
                                    Try
                                        mem = New IO.MemoryStream(Convert.FromBase64String(thumb))
                                    Catch
                                    End Try
                                End If
                                Dim bi As New BitmapImage
                                If mem Is Nothing Then
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
                                    bi.BeginInit()
                                    bi.UriSource = tUri
                                    bi.EndInit()
                                    'Cache Thumb
                                    Dim cmem As New IO.MemoryStream
                                    Dim pngEnco As New PngBitmapEncoder
                                    pngEnco.Frames.Add(BitmapFrame.Create(bi))
                                    pngEnco.Save(cmem)
#Disable Warning
                                    LocalThumbnailDB.AddItemAsync(Convert.ToBase64String(cmem.ToArray), $"G{mdg.Name}")
#Enable Warning
                                    cmem.Dispose()
                                Else
                                    bi.BeginInit()
                                    bi.StreamSource = mem
                                    bi.EndInit()
                                End If
                                mdg.Cover = bi
                                Dispatcher.Invoke(Sub() Me.Artist = mdg)
                                Artist = Nothing
                            End If
                        End If
                        If Artist IsNot Nothing Then
                            Dim MDG As New Player.MetadataGroup() With {.Type = MetadataGroup.GroupType.Artist, .Name = Artist.Name, .Category = "Deezer", .PlayCount = Convert.ToInt32(Artist.Fans), .Tag = Artist.Link, .UID = aID}
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
                                Dim bi As BitmapSource = Nothing
                                If DecodeToThumbnails Then
                                    bi = tUri.ToBitmapSource(DecodeWidth, DecodeHeight)
                                Else
                                    bi = tUri.ToBitmapSource
                                End If
                                MDG.Cover = bi
                                MDG.IsCoverLocked = True
                            End If
                            If Not ArtistInfoOnly Then
                                Dim ttl = Await Artist.GetTopTracks(0, 10)
                                If ttl.Count = 0 Then
                                    Dim album = (Await Artist.GetAlbums(0, 1))?.FirstOrDefault
                                    If album Is Nothing Then
                                        ttl = Await Artist.GetSmartRadio(0, 15)
                                    Else
                                        ttl = Await album.GetTracks()
                                    End If
                                End If
                                For Each rTr In ttl
                                    Dim tr = Await Session.Browse.GetTrackById(rTr.Id)
                                    Dim MD As New Player.Metadata() With {.Location = Player.Metadata.FileLocation.Remote, .OriginalPath = tr.Link, .Provider = New Deezer.DeezerMediaProvider(tr.Id), .BlockDOWNLOADPROC = True, .Album = tr.AlbumName, .Year = tr.ReleaseDate.Year, .Artists = New String() {tr.ArtistName}, .PlayCount = tr.Rank, .Index = tr.Number, .Title = tr.Title, .Length = tr.Duration, .Path = tr.Preview}
                                    Dim uri As Uri = Nothing
                                    If tr.HasPicture(EDeezer.Api.PictureSize.Medium) Then
                                        uri = New Uri(tr.GetPicture(EDeezer.Api.PictureSize.Medium))
                                    ElseIf tr.HasPicture(EDeezer.Api.PictureSize.Large) Then
                                        uri = New Uri(tr.GetPicture(EDeezer.Api.PictureSize.Large))
                                    ElseIf tr.HasPicture(EDeezer.Api.PictureSize.Small) Then
                                        uri = New Uri(tr.GetPicture(EDeezer.Api.PictureSize.Small))
                                    ElseIf tr.HasPicture(EDeezer.Api.PictureSize.ExtraLarge) Then
                                        uri = New Uri(tr.GetPicture(EDeezer.Api.PictureSize.ExtraLarge))
                                    End If
                                    If uri IsNot Nothing Then
                                        Dim bi As BitmapImage = Nothing
                                        If DecodeToThumbnails Then
                                            bi = uri.ToBitmapSource(DecodeWidth, DecodeHeight)
                                        Else
                                            bi = uri.ToBitmapSource
                                        End If
                                        MD.ThumbnailCoverLink = uri.ToString 'see fetch for explanation on why the latter is removed
                                        'MD.CoverLink = uri.ToString
                                        'MD.Covers = New ImageSource() {bi}
                                    End If
                                    MDG.Add(MD)
                                Next
                            End If
                            If Me.Artist IsNot Nothing Then
                                Me.Artist.IsCoverLocked = False
                                Me.Artist.FreeCovers()
                            End If
                            Me.Artist = MDG
                            'Cache
#Disable Warning
                            LocalMetadataGroupDB.AddItemAsync(MDG, $"G{MDG.Name}")
#Enable Warning
                            If tUri IsNot Nothing Then
                                If TryCast(MDG.Cover, BitmapSource)?.IsDownloading Then
                                    Dim name = Me.Artist.Name 'temp var
                                    AddHandler CType(MDG.Cover, BitmapSource).DownloadCompleted, New EventHandler(Async Sub(s, e)
                                                                                                                      'Dim mem As New IO.MemoryStream
                                                                                                                      'Dim pngEnco As New PngBitmapEncoder()
                                                                                                                      'pngEnco.Frames.Add(BitmapFrame.Create(s)) 'BitmapFrame.Create(tUri, BitmapCreateOptions.None, BitmapCacheOption.OnLoad))
                                                                                                                      'pngEnco.Save(mem)
                                                                                                                      Dim mem = Save(s)
                                                                                                                      Await LocalThumbnailDB.AddItemAsync(Convert.ToBase64String(mem.ToArray), $"G{name}")
                                                                                                                      mem.Dispose()
                                                                                                                  End Sub)
                                Else
                                    'Dim mem As New IO.MemoryStream
                                    'Dim pngEnco As New PngBitmapEncoder()
                                    'pngEnco.Frames.Add(BitmapFrame.Create(Me.Artist.Cover)) 'BitmapFrame.Create(tUri, BitmapCreateOptions.None, BitmapCacheOption.OnLoad))
                                    'pngEnco.Save(mem)
                                    Dim mem = CType(Me.Artist.Cover, BitmapSource).Save
                                    Await LocalThumbnailDB.AddItemAsync(Convert.ToBase64String(mem.ToArray), $"G{Me.Artist.Name}")
                                    mem.Dispose()
                                End If
                            End If
                            'Related
                            Dim Related = (Await Artist.GetRelated(0, 1))?.FirstOrDefault
                            If Related IsNot Nothing Then
                                Dim RMDG As New Player.MetadataGroup() With {.Type = MetadataGroup.GroupType.Artist, .Name = Related.Name, .Category = "Deezer", .PlayCount = Convert.ToInt32(Related.Fans), .Tag = Related.Link, .UID = Related.Id}
                                'Picture                    
                                Dim rtUri As Uri = Nothing
                                If Related.HasPicture(EDeezer.Api.PictureSize.Medium) Then
                                    rtUri = New Uri(Related.GetPicture(EDeezer.Api.PictureSize.Medium))
                                ElseIf Related.HasPicture(EDeezer.Api.PictureSize.Large) Then
                                    rtUri = New Uri(Related.GetPicture(EDeezer.Api.PictureSize.Large))
                                ElseIf Related.HasPicture(EDeezer.Api.PictureSize.Small) Then
                                    rtUri = New Uri(Related.GetPicture(EDeezer.Api.PictureSize.Small))
                                ElseIf Related.HasPicture(EDeezer.Api.PictureSize.ExtraLarge) Then
                                    rtUri = New Uri(Related.GetPicture(EDeezer.Api.PictureSize.ExtraLarge))
                                End If
                                If rtUri IsNot Nothing Then
                                    Dim bi As BitmapImage = Nothing
                                    If DecodeToThumbnails Then
                                        bi = rtUri.ToBitmapSource(DecodeWidth, DecodeHeight)
                                    Else
                                        bi = rtUri.ToBitmapSource
                                    End If
                                    RMDG.Cover = bi
                                End If
                                If Not ArtistInfoOnly Then
                                    Dim rttl = Await Related.GetTopTracks(0, 10)
                                    If rttl.Count = 0 Then
                                        Dim album = (Await Related.GetAlbums(0, 1))?.FirstOrDefault
                                        If album Is Nothing Then
                                            rttl = Await Related.GetSmartRadio(0, 15)
                                        Else
                                            rttl = Await album.GetTracks()
                                        End If
                                    End If
                                    For Each rTr In rttl
                                        Dim tr = Await Session.Browse.GetTrackById(rTr.Id)
                                        Dim MD As New Player.Metadata() With {.Location = Player.Metadata.FileLocation.Remote, .OriginalPath = tr.Link, .Provider = New Deezer.DeezerMediaProvider(tr.Id), .BlockDOWNLOADPROC = True, .Album = tr.AlbumName, .Year = tr.ReleaseDate.Year, .Artists = New String() {tr.ArtistName}, .PlayCount = tr.Rank, .Index = tr.Number, .Title = tr.Title, .Length = tr.Duration, .Path = tr.Preview}
                                        Dim uri As Uri = Nothing
                                        If tr.HasPicture(EDeezer.Api.PictureSize.Medium) Then
                                            uri = New Uri(tr.GetPicture(EDeezer.Api.PictureSize.Medium))
                                        ElseIf tr.HasPicture(EDeezer.Api.PictureSize.Large) Then
                                            uri = New Uri(tr.GetPicture(EDeezer.Api.PictureSize.Large))
                                        ElseIf tr.HasPicture(EDeezer.Api.PictureSize.Small) Then
                                            uri = New Uri(tr.GetPicture(EDeezer.Api.PictureSize.Small))
                                        ElseIf tr.HasPicture(EDeezer.Api.PictureSize.ExtraLarge) Then
                                            uri = New Uri(tr.GetPicture(EDeezer.Api.PictureSize.ExtraLarge))
                                        End If
                                        If uri IsNot Nothing Then
                                            Dim bi As BitmapImage = Nothing
                                            If DecodeToThumbnails Then
                                                bi = uri.ToBitmapSource(DecodeWidth, DecodeHeight)
                                            Else
                                                bi = uri.ToBitmapSource
                                            End If
                                            MD.ThumbnailCoverLink = tr.GetPicture(EDeezer.Api.PictureSize.Medium)
                                            MD.CoverLink = uri.ToString
                                            MD.Covers = New ImageSource() {bi}
                                        End If
                                        RMDG.Add(MD)
                                    Next
                                End If
                                If Me.RelatedArtist IsNot Nothing Then
                                    Me.RelatedArtist.IsCoverLocked = False
                                    Me.RelatedArtist.FreeCovers()
                                End If
                                Me.RelatedArtist = RMDG
                            End If
                        End If
                        If CombineQueries Then
                            Track = Me.Artist?.Item(0)
                        ElseIf tID = 0 Then
                            Dim tr = (Await Artist.GetTopTracks(0, 1))?.FirstOrDefault 'Await Session.Browse.GetTrackById(tID)                    
                            If tr Is Nothing Then
                                tr = (Await Artist.GetSmartRadio(0, 1))?.FirstOrDefault
                            End If
                            If tr IsNot Nothing Then
                                If tr IsNot Nothing Then
                                    Dim MD As New Player.Metadata() With {.Location = Player.Metadata.FileLocation.Remote, .OriginalPath = tr.Link, .Provider = New Deezer.DeezerMediaProvider(tr.Id), .BlockDOWNLOADPROC = True, .Album = tr.AlbumName, .Year = tr.ReleaseDate.Year, .Artists = New String() {tr.ArtistName}, .PlayCount = tr.Rank, .Index = tr.Number, .Title = tr.Title, .Length = tr.Duration, .Path = tr.Preview, .UID = tr.Id}
                                    Dim url = If(tr.HasPicture(EDeezer.Api.PictureSize.Medium), tr.GetPicture(EDeezer.Api.PictureSize.Medium), If(tr.HasPicture(EDeezer.Api.PictureSize.Large), tr.GetPicture(EDeezer.Api.PictureSize.Large), If(tr.HasPicture(EDeezer.Api.PictureSize.Small), tr.GetPicture(EDeezer.Api.PictureSize.Small), If(tr.HasPicture(EDeezer.Api.PictureSize.ExtraLarge), tr.GetPicture(EDeezer.Api.PictureSize.ExtraLarge), Nothing))))
                                    Dim bi As BitmapImage = Nothing
                                    If DecodeToThumbnails Then
                                        bi = New Uri(url).ToBitmapSource(DecodeWidth, DecodeHeight)
                                    Else
                                        bi = New Uri(url).ToBitmapSource
                                    End If
                                    MD.ThumbnailCoverLink = tr.GetPicture(EDeezer.Api.PictureSize.Medium)
                                    MD.CoverLink = url
                                    MD.Covers = New ImageSource() {bi}
                                    Me.Track = MD
                                End If
                            End If
                        End If
                    End If
                Catch ex As Exception
                    Configuration.SetError(True, ex)
                    Utilities.DebugMode.Instance?.Log(Of DeezerProvider)(ex.ToString)
                End Try
            End If
            If FoundArtistCache Then
                Dim mdg = LocalMetadataGroupDB.GetItem($"G{ArtistQuery}")
                Dim thumb = LocalThumbnailDB.GetItem($"G{ArtistQuery}")
                Dim mem As New IO.MemoryStream(Convert.FromBase64String(thumb))
                Dim bi As New BitmapImage
                bi.BeginInit()
                bi.StreamSource = mem
                bi.EndInit()
                mdg.Cover = bi
                Dispatcher.Invoke(Sub() Me.Artist = mdg)
            End If
            Configuration.SetStatus("All Good", 100)
            Session?.Dispose()
            IsBusy = False
            RaiseEvent FetchCompleted(Me)
        End Function

        Public Sub Init() Implements IStartupItem.Init
            LocalMetadataGroupDB = Classes.QBDatabase(Of MetadataGroup).LoadOrCreate("DeezerMetadataGroup")
            LocalThumbnailDB = Classes.QBDatabase(Of String).LoadOrCreate("DeezerThumbnails")
            Configuration.SetStatus("All Good", 100)
        End Sub

        Public ReadOnly Property FetchCommand As New FetchDelegateCommand(Me)

        Public Property Configuration As New StartupItemConfiguration("Deezer") Implements IStartupItem.Configuration

        Public Class FetchDelegateCommand
            Implements ICommand

            Public Event CanExecuteChanged As EventHandler Implements ICommand.CanExecuteChanged
            Private ReadOnly _Parent As DeezerProvider

            Sub New(parent As DeezerProvider)
                _Parent = parent
                AddHandler CommandManager.RequerySuggested, New EventHandler(Sub(sender As Object, e As EventArgs)
                                                                                 RaiseEvent CanExecuteChanged(Me, New EventArgs())
                                                                             End Sub)
            End Sub

            Public Sub Execute(parameter As Object) Implements ICommand.Execute
                _Parent?.Fetch()
            End Sub

            Public Function CanExecute(parameter As Object) As Boolean Implements ICommand.CanExecute
                Return Not _Parent?.IsBusy
            End Function
        End Class

    End Class
End Namespace
