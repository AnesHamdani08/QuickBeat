Namespace UPnP
    Public Class LocalMediaRenderer
        Inherits MediaRenderer

        Public Overrides Property Info As UPnPInfo
            Get
                Return MyBase.Info
            End Get
            Set(value As UPnPInfo)
                MyBase.Info = value
            End Set
        End Property

        Sub New()
            Info = New UPnPInfo With {.Name = "Local Renderer", .Icon = New BitmapImage(New Uri("Resources/MusicRecord.png", UriKind.Relative)), .ModelName = "QuickBeat", .ModelNumber = My.Application.Info.Version.ToString, .ManufacturerName = "Anes08"}
            PlaybackControlCommand = New DelegatePlaybackControlCommand(Me)
            SetAVUriCommand = New DelegateSetAVUriCommand(Me)
            If Utilities.SharedProperties.Instance.Player IsNot Nothing Then AddHandler Utilities.SharedProperties.Instance.Player.MetadataChanged, AddressOf Player_MetadataChanged
        End Sub

        Private Sub Player_MetadataChanged()
            ForceInfoPass()
        End Sub

        Public Overrides Async Sub SetAVTransportURI(InstanceID As UInteger, CurrentURI As String, CurrentURIMetaData As String)
            IsBusy = True
            If String.IsNullOrEmpty(CurrentURIMetaData) OrElse CurrentURIMetaData = "NOT_IMPLEMENTED" Then
                Dim Meta As New Player.Metadata With {.Location = Player.Metadata.FileLocation.Remote, .Path = CurrentURI, .OriginalPath = CurrentURI}
                Await Utilities.SharedProperties.Instance.Player?.LoadSong(Meta)
            Else
                Dim xDoc = XDocument.Parse(CurrentURIMetaData.Replace("&", "&amp;"))
                Dim cdObj = TryCast(UPnP.ContentDirectory.CD_Object.Create_CD_Object(If(xDoc.Root.Name.LocalName = "DIDL-Lite", xDoc.Root.FirstNode, xDoc.Root)), UPnP.ContentDirectory.CD_Item)
                If cdObj IsNot Nothing Then
                    Dim cdObjRes = If(cdObj.ClassName = "object.item.videoItem",
                        cdObj.Resource.FirstOrDefault(Function(k) UPnP.UPnPProvider.TryGetMimeTypeFromDLNAProtocole(k.protocolInfo) = "video/mp4"),
                    cdObj.Resource.FirstOrDefault(Function(k) UPnP.UPnPProvider.TryGetMimeTypeFromDLNAProtocole(k.protocolInfo) = "audio/mpeg"))
                    If cdObjRes IsNot Nothing Then
                        Dim Meta As New Player.Metadata() With {.Location = Player.Metadata.FileLocation.Remote, .Path = CurrentURI, .OriginalPath = cdObjRes.URI.ToString, .Title = cdObj.Title, .Album = cdObj.Album, .Artists = New String() {cdObj.Artist}, .Genres = New String() {cdObj.Genre}, .Length = TimeSpan.Parse(cdObjRes.duration).TotalSeconds} 'path was: cdObjRes.URI.ToString
                        Await Utilities.SharedProperties.Instance.Player?.LoadSong(Meta)
                    End If
                End If
            End If
            IsBusy = False
        End Sub

        Public Overrides Sub Play()
            Utilities.SharedProperties.Instance.Player?.Play()
        End Sub
        Public Overrides Sub Pause()
            Utilities.SharedProperties.Instance.Player?.Pause()
        End Sub
        Public Overrides Sub [Next]()
            Utilities.SharedProperties.Instance.Player?.Next()
        End Sub
        Public Overrides Sub Previous()
            Utilities.SharedProperties.Instance.Player?.Previous()
        End Sub
        Public Overrides Sub [Stop]()
            Utilities.SharedProperties.Instance.Player?.Stop()
        End Sub
        Public Overrides Sub Seek(Position As Integer)
            If Utilities.SharedProperties.Instance.Player IsNot Nothing Then Utilities.SharedProperties.Instance.Player.Position = Position
        End Sub
        Protected Overrides Sub OnProgressRequested()
            SetPosition(Utilities.SharedProperties.Instance.Player?.Position)
            SetDuration(Utilities.SharedProperties.Instance.Player?.Length)
        End Sub
        Protected Overrides Sub OnStateRequested()
            SetIsPlaying(Utilities.SharedProperties.Instance.Player?.IsPlaying)
            SetIsPaused(Utilities.SharedProperties.Instance.Player?.IsPaused)
            SetIsStopped(Utilities.SharedProperties.Instance.Player?.IsStopped)
            CurrentTrackTitle = Utilities.SharedProperties.Instance.Player?.StreamMetadata?.Title
            CurrentTrackArtist = Utilities.SharedProperties.Instance.Player?.StreamMetadata?.DefaultArtist
            CurrentTrackAlbum = Utilities.SharedProperties.Instance.Player?.StreamMetadata?.Album
            CurrentTrackGenre = Utilities.SharedProperties.Instance.Player?.StreamMetadata?.JoinedGenres
            If IsPlaying Then
                StartProgressMonitoring()
            Else
                StopProgressMonitoring()
            End If
        End Sub

        Public Class LocalRendererUPnPDeviceListItemWrapper
            Inherits UPnP.UPnPProvider.UPnPDeviceListItemWrapper

            Private _LocalRenderer As LocalMediaRenderer

            Sub New()
                MyBase.New(Nothing)
                Info = New UPnPInfo With {.Name = "Local Renderer", .Icon = New BitmapImage(New Uri("Resources/MusicRecord.png", UriKind.Relative)), .ModelName = "QuickBeat", .ModelNumber = My.Application.Info.Version.ToString, .ManufacturerName = "Anes08"}
            End Sub

            Public Overrides Function CreateRenderer() As MediaRenderer
                If _LocalRenderer Is Nothing Then _LocalRenderer = New LocalMediaRenderer
                Return _LocalRenderer
            End Function
        End Class
    End Class
End Namespace