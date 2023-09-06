Imports EDeezer = E.Deezer
Imports System.ComponentModel
Imports QuickBeat.Interfaces
Imports QuickBeat.Classes

Namespace Utilities
    Public Class DeezerProvider
        Inherits DependencyObject
        Implements IStartupItem

        Public BlockAutoFetch As Boolean = True

        Public Shared ArtistProperty = DependencyProperty.Register("Artist", GetType(Player.MetadataGroup), GetType(DeezerProvider))
        Property Artist As Player.MetadataGroup
            Get
                Return GetValue(ArtistProperty)
            End Get
            Set(value As Player.MetadataGroup)
                SetValue(ArtistProperty, value)
            End Set
        End Property

        Public Shared RelatedArtistProperty = DependencyProperty.Register("RelatedArtist", GetType(Player.MetadataGroup), GetType(DeezerProvider))
        Property RelatedArtist As Player.MetadataGroup
            Get
                Return GetValue(RelatedArtistProperty)
            End Get
            Set(value As Player.MetadataGroup)
                SetValue(RelatedArtistProperty, value)
            End Set
        End Property

        Public Shared TrackProperty = DependencyProperty.Register("Track", GetType(Player.Metadata), GetType(DeezerProvider))
        Property Track As Player.Metadata
            Get
                Return GetValue(TrackProperty)
            End Get
            Set(value As Player.Metadata)
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

        Public Shared IsBusyProperty = DependencyProperty.Register("IsBusy", GetType(Boolean), GetType(DeezerProvider))
        Property IsBusy As Boolean
            Get
                Return GetValue(IsBusyProperty)
            End Get
            Set(value As Boolean)
                SetValue(IsBusyProperty, value)
            End Set
        End Property

        Public Async Sub Fetch()
            If Not SharedProperties.Instance.ItemsConfiguration.Contains(Configuration) Then
                SharedProperties.Instance.ItemsConfiguration.Add(Configuration)
                Init()
            End If
            If IsBusy Then Return
            If Not Utilities.SharedProperties.Instance.IsInternetConnected Then Return
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
            Configuration.SetStatus("Acquiring IDs...", 0)
            Try 'As much i hate multiple try blocks, it's inevitable in this case :'(
                If UseDefaultQuery Then
                    aID = 49469512
                    tID = 2127495917
                Else
                    Dim aResult = Await Session.Search.Artists(ArtistQuery, 0, 1)
                    If aResult Is Nothing OrElse aResult.Count = 0 Then
                        If FallBackToDefaultQuery Then aID = 49469512 'REOL
                    Else
                        aID = If(aResult.FirstOrDefault?.Id, If(FallBackToDefaultQuery, 49469512, 0))
                    End If
                    Dim tResult = Await Session.Search.Tracks(TrackQuery, 0, 1)
                    If tResult Is Nothing OrElse tResult.Count = 0 Then
                        If FallBackToDefaultQuery Then tID = 2127495917 'Q?
                    Else
                        tID = If(tResult.FirstOrDefault?.Id, If(FallBackToDefaultQuery, 2127495917, 0))
                    End If
                End If
            Catch ex As Exception
                Configuration.SetStatus("All Good", 100)
                Configuration.SetError(True, ex)
                Utilities.DebugMode.Instance?.Log(Of DeezerProvider)(ex.ToString)
            End Try
            Configuration.SetStatus("Decoding Track Data", 50)
            Try
                If tID <> 0 Then
                    Dim tr = Await Session.Browse.GetTrackById(tID)
                    If tr IsNot Nothing AndAlso Not String.IsNullOrEmpty(tr.Preview) Then
                        Dim MD As New Player.Metadata() With {.Location = Player.Metadata.FileLocation.Remote, .OriginalPath = tr.Link, .Provider = New Deezer.DeezerMediaProvider(tr.Id), .BlockDOWNLOADPROC = True, .Album = tr.AlbumName, .Artists = New String() {tr.ArtistName}, .PlayCount = tr.Rank, .Index = tr.Number, .Title = tr.Title, .Length = tr.Duration, .Path = tr.Preview}
                        Dim url = If(tr.HasPicture(EDeezer.Api.PictureSize.Medium), tr.GetPicture(EDeezer.Api.PictureSize.Medium), If(tr.HasPicture(EDeezer.Api.PictureSize.Large), tr.GetPicture(EDeezer.Api.PictureSize.Large), If(tr.HasPicture(EDeezer.Api.PictureSize.Small), tr.GetPicture(EDeezer.Api.PictureSize.Small), If(tr.HasPicture(EDeezer.Api.PictureSize.ExtraLarge), tr.GetPicture(EDeezer.Api.PictureSize.ExtraLarge), Nothing))))
                        Dim bi As New BitmapImage
                        bi.BeginInit()
                        bi.UriSource = New Uri(url)
                        bi.EndInit()
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
            Configuration.SetStatus("Decoding Artist Data", 80)
            Try
                If aID <> 0 Then
                    'Bind Artist
                    Dim Artist = Await Session.Browse.GetArtistById(aID)
                    If Artist Is Nothing AndAlso FallBackToDefaultQuery Then
                        Artist = Await Session.Browse.GetArtistById(49469512)
                    End If
                    If Artist IsNot Nothing Then
                        Dim MDG As New Player.MetadataGroup() With {.Name = Artist.Name, .Category = "Deezer", .PlayCount = Convert.ToInt32(Artist.Fans)}
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
                            Dim bi As New BitmapImage
                            bi.BeginInit()
                            bi.UriSource = tUri
                            bi.EndInit()
                            MDG.Cover = bi
                            MDG.IsCoverLocked = True
                        End If
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
                            Dim MD As New Player.Metadata() With {.Location = Player.Metadata.FileLocation.Remote, .OriginalPath = tr.Link, .Provider = New Deezer.DeezerMediaProvider(tr.Id), .BlockDOWNLOADPROC = True, .Album = tr.AlbumName, .Artists = New String() {tr.ArtistName}, .PlayCount = tr.Rank, .Index = tr.Number, .Title = tr.Title, .Length = tr.Duration, .Path = tr.Preview}
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
                                Dim bi As New BitmapImage
                                bi.BeginInit()
                                bi.UriSource = uri
                                bi.EndInit()
                                MD.Covers = New ImageSource() {bi}
                            End If
                            MDG.Add(MD)
                        Next
                        If Me.Artist IsNot Nothing Then
                            Me.Artist.IsCoverLocked = False
                            Me.Artist.FreeCovers()
                        End If
                        Me.Artist = MDG
                        'Related
                        Dim Related = (Await Artist.GetRelated(0, 1))?.FirstOrDefault
                        If Related IsNot Nothing Then
                            Dim RMDG As New Player.MetadataGroup() With {.Name = Related.Name, .Category = "Deezer", .PlayCount = Convert.ToInt32(Related.Fans)}
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
                                Dim bi As New BitmapImage
                                bi.BeginInit()
                                bi.UriSource = rtUri
                                bi.EndInit()
                                RMDG.Cover = bi
                            End If
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
                                Dim MD As New Player.Metadata() With {.Location = Player.Metadata.FileLocation.Remote, .OriginalPath = tr.Link, .Provider = New Deezer.DeezerMediaProvider(tr.Id), .BlockDOWNLOADPROC = True, .Album = tr.AlbumName, .Artists = New String() {tr.ArtistName}, .PlayCount = tr.Rank, .Index = tr.Number, .Title = tr.Title, .Length = tr.Duration, .Path = tr.Preview}
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
                                    Dim bi As New BitmapImage
                                    bi.BeginInit()
                                    bi.UriSource = uri
                                    bi.EndInit()
                                    MD.Covers = New ImageSource() {bi}
                                End If
                                RMDG.Add(MD)
                            Next
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
                                Dim MD As New Player.Metadata() With {.Location = Player.Metadata.FileLocation.Remote, .OriginalPath = tr.Link, .Provider = New Deezer.DeezerMediaProvider(tr.Id), .BlockDOWNLOADPROC = True, .Album = tr.AlbumName, .Artists = New String() {tr.ArtistName}, .PlayCount = tr.Rank, .Index = tr.Number, .Title = tr.Title, .Length = tr.Duration, .Path = tr.Preview}
                                Dim url = If(tr.HasPicture(EDeezer.Api.PictureSize.Medium), tr.GetPicture(EDeezer.Api.PictureSize.Medium), If(tr.HasPicture(EDeezer.Api.PictureSize.Large), tr.GetPicture(EDeezer.Api.PictureSize.Large), If(tr.HasPicture(EDeezer.Api.PictureSize.Small), tr.GetPicture(EDeezer.Api.PictureSize.Small), If(tr.HasPicture(EDeezer.Api.PictureSize.ExtraLarge), tr.GetPicture(EDeezer.Api.PictureSize.ExtraLarge), Nothing))))
                                Dim bi As New BitmapImage
                                bi.BeginInit()
                                bi.UriSource = New Uri(url)
                                bi.EndInit()
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
            Configuration.SetStatus("All Good", 100)
            Session?.Dispose()
            IsBusy = False
        End Sub

        Public Sub Init() Implements IStartupItem.Init
            Configuration.SetStatus("All Good", 100)
        End Sub

        Public ReadOnly Property FetchCommand As New FetchDelegateCommand(Me)

        Public Property Configuration As New StartupItemConfiguration("Deezer") Implements IStartupItem.Configuration

        Public Class FetchDelegateCommand
            Implements ICommand

            Public Event CanExecuteChanged As EventHandler Implements ICommand.CanExecuteChanged
            Private _Parent As DeezerProvider

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
