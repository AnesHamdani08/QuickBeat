Imports System.ComponentModel
Imports System.Globalization
Imports Newtonsoft
Imports Newtonsoft.Json
Imports QuickBeat.Classes
Imports QuickBeat.Interfaces
Imports QuickBeat.Utilities
Imports QuickBeat.Youtube.Classes

Namespace Youtube
    <Serializable>
    Public Class YoutubeDL
        Implements QuickBeat.Interfaces.IStartupItem, ComponentModel.INotifyPropertyChanged

        <NonSerialized> Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged

        Public Sub OnPropertyChanged(<Runtime.CompilerServices.CallerMemberName> Optional CallerName As String = Nothing)
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(CallerName))
        End Sub

        Private _YouTubeDLPath As String
        Public Property YoutubeDLLocation As String
            Get
                Return _YouTubeDLPath
            End Get
            Set(value As String)
                _YouTubeDLPath = value
                If Configuration.Status = "Waiting for file path" Then
                    Configuration.SetStatus("All Good", 100)
                End If
                OnPropertyChanged()
            End Set
        End Property

        <NonSerialized> Private _IsBusy As Boolean
        Public Property IsBusy As Boolean
            Get
                Return _IsBusy
            End Get
            Set(value As Boolean)
                _IsBusy = value
                If Not value Then
                    Configuration.IsLoaded = True
                    Configuration.Progress = 100
                    SetKnownState(LinkState.Free)
                Else
                    Configuration.IsLoaded = False
                    Configuration.Progress = 0
                End If
                OnPropertyChanged()
            End Set
        End Property

        Private ReadOnly Property UniversalSplitter As Char() = New Char() {"\r", "\n", "\r\n"}

        Private _Configuration As New StartupItemConfiguration("Youtube-DL")
        Public Property Configuration As StartupItemConfiguration Implements IStartupItem.Configuration
            Get
                Return _Configuration
            End Get
            Set(value As StartupItemConfiguration)
                _Configuration = value
                OnPropertyChanged()
            End Set
        End Property

        Private _VideoQuality As VideoQuality = VideoQuality.best
        Property VideoQuality As VideoQuality
            Get
                Return _VideoQuality
            End Get
            Set(value As VideoQuality)
                _VideoQuality = value
                OnPropertyChanged()
            End Set
        End Property

        Private _DefaultSearchProvider As SearchProvider = SearchProvider.Youtube
        Property DefaultSearchProvider As SearchProvider
            Get
                Return _DefaultSearchProvider
            End Get
            Set(value As SearchProvider)
                _DefaultSearchProvider = value
                OnPropertyChanged()
            End Set
        End Property

#Region "WPF Support"
        <NonSerialized> Private _LoadCommand As New WPF.LoadCommand(Me)
        Public ReadOnly Property LoadCommand As WPF.LoadCommand
            Get
                Return _LoadCommand
            End Get
        End Property
        <NonSerialized> Private _LoadPlaylistCommand As New WPF.LoadPlaylistCommand(Me)
        Public ReadOnly Property LoadPlaylistCommand As WPF.LoadPlaylistCommand
            Get
                Return _LoadPlaylistCommand
            End Get
        End Property
        <NonSerialized> Private _QuickLoadCommand As New WPF.QuickLoadCommand(Me)
        Public ReadOnly Property QuickLoadCommand As WPF.QuickLoadCommand
            Get
                Return _QuickLoadCommand
            End Get
        End Property
        <NonSerialized> Private _SearchAndLoadCommand As New WPF.SearchAndLoadCommand(Me)
        Public ReadOnly Property SearchAndLoadCommand As WPF.SearchAndLoadCommand
            Get
                Return _SearchAndLoadCommand
            End Get
        End Property
        <NonSerialized> Private _SearchAndLoadDefaultCommand As New WPF.SearchAndLoadDefaultCommand(Me)
        Public ReadOnly Property SearchAndLoadDefaultCommand As WPF.SearchAndLoadDefaultCommand
            Get
                Return _SearchAndLoadDefaultCommand
            End Get
        End Property
        <NonSerialized> Private _SetPathCommand As New WPF.SetPathCommand(Me)
        Public ReadOnly Property SetPathCommand As WPF.SetPathCommand
            Get
                Return _SetPathCommand
            End Get
        End Property
#End Region

        Public Enum LinkState
            Free
            ContactingYTDL
            ParsingData
        End Enum

        Public Enum SearchProvider
            Youtube
            SoundCloud
            YoutubeMusic
        End Enum

        ''' <summary>
        ''' Initialize a new instance of <see cref="YoutubeDL"/>
        ''' </summary>
        ''' <remarks>
        ''' Set <see cref="YoutubeDLLocation"/> after calling this constructor
        ''' </remarks>
        Public Sub New()
            Configuration.SetStatus("", 100)
            Configuration.SetStatus("Waiting for file path", 0, 10000)
        End Sub
        ''' <summary>
        '''Initialize a new instance of <see cref="YoutubeDL"/>
        ''' </summary>        
        ''' <exception cref="ArgumentException"/>
        ''' <param name="YTDLPath">Youtube-DL path</param>
        Public Sub New(YTDLPath As String)
            If IO.File.Exists(YTDLPath) Then
                YoutubeDLLocation = YTDLPath
                Configuration.SetStatus("All Good", 100)
            Else
                Configuration.SetStatus("Please provide a valid path.", 0, 5000)
                Configuration.SetError(True, New ArgumentNullException("YoutubeDL doesn't exists in the given path."))
            End If
        End Sub
        Public Shared Function IsYoutubeLink(URL As String) As Boolean
            'https://www.youtube.com/watch?v=0xSiBpUdW4E&list=RDCLAK5uy_kvhjcPWzH7xZL-WnqGbiA_euQGy5_cbHI&index=19
            If URL.Contains("youtube.com/watch?v=") Then Return True
            'youtu.be
            If URL.ToLower.Contains("youtu.be") Then Return True 'https://youtu.be/PKfxmFU3lWY?list=RDPKfxmFU3lWY
            Return False
        End Function
        Public Shared Function IsYoutubePlaylistLink(URL As String) As Boolean
            'https://www.youtube.com/playlist?list=PLzSjbEiFKZ_w9zWXjVSTLi5FUlcPHwQCc
            If URL.Contains("youtube.com/playlist?list=") Then Return True
            Return False
        End Function
        ''' <summary>
        ''' A quick function opposed to <see cref="DumpAndManageVideo(String)"/>, gets only essential info:<see cref="YoutubeVideo.Title"/>, Direct Link is Stored In <see cref="YoutubeVideo.WebpageURL"/>
        ''' </summary>
        ''' <param name="URL">The video's url</param>        
        ''' <returns></returns>
        Public Async Function GetVideo(URL As String) As Task(Of YoutubeVideo)
            If Not IO.File.Exists(YoutubeDLLocation) Then Return Nothing
            If Utilities.SharedProperties.Instance.IsInternetConnected Then
                IsBusy = True
                SetKnownState(LinkState.ContactingYTDL)
                Dim YoutubeDL As Process = Process.Start(New ProcessStartInfo(YoutubeDLLocation, $"-s -e -g -f {VideoQuality.ToString} ""{URL.Split("&")(0)}""") With {.RedirectStandardOutput = True, .UseShellExecute = False, .WindowStyle = ProcessWindowStyle.Hidden, .CreateNoWindow = True})
                SetKnownState(LinkState.ParsingData)
                Dim AInfo = Await YoutubeDL.StandardOutput.ReadToEndAsync
                Dim Info = AInfo.Split(New Char() {vbCr, vbCrLf, vbLf})
                If Info.Length >= 2 Then
                    Dim Vid As New YoutubeVideo With {.Title = Info(0), .OriginalURL = URL, .URL = Info(1), .WebpageURL = URL, .Type = VideoQuality.ToString}
                    IsBusy = False
                    Return Vid
                Else
                    IsBusy = False
                    Return Nothing
                End If
            Else
                Return Nothing
            End If
        End Function
        Public Async Function SearchVideo(Query As String, Limit As Integer, Provider As SearchProvider) As Task(Of String())
            If Not IO.File.Exists(YoutubeDLLocation) Then Return Nothing
            If Limit >= 1 Then
                If Utilities.SharedProperties.Instance.IsInternetConnected Then
                    IsBusy = True
                    SetKnownState(LinkState.ContactingYTDL)
                    Dim YoutubeDL As Process = Process.Start(New ProcessStartInfo(YoutubeDLLocation, If(Provider = SearchProvider.YoutubeMusic,
                                                                                  $"""https://music.youtube.com/search?q={System.Web.HttpUtility.UrlEncode(Query)}"" --playlist-end {Limit} --get-id",
                                                                                  $"""{If(Provider = SearchProvider.Youtube, "yt", "sc")}search{Limit}:{Query}"" --get-id")) With {.RedirectStandardOutput = True, .UseShellExecute = False, .WindowStyle = ProcessWindowStyle.Hidden, .CreateNoWindow = True})
                    SetKnownState(LinkState.ParsingData)
                    Dim out = Await YoutubeDL.StandardOutput.ReadToEndAsync
                    IsBusy = False
                    Return out.Split(New Char() {"\n", "\r", "\r\n"})
                End If
            End If
            Return Nothing
        End Function
        ''' <summary>
        ''' Searches and Returns the First Video Reseult
        ''' </summary>
        ''' <param name="Query">Search Query</param>        
        ''' <param name="Provider">One of the predefined providers</param>
        ''' <returns></returns>
        Public Async Function SearchVideoAndDump(Query As String, Provider As SearchProvider) As Task(Of IEnumerable(Of YoutubeVideo))
            If Not IO.File.Exists(YoutubeDLLocation) Then Return Nothing
            Dim Limit = 1
            If Limit >= 1 Then
                If Utilities.SharedProperties.Instance.IsInternetConnected Then
                    Try
                        IsBusy = True
                        SetKnownState(LinkState.ContactingYTDL)
                        Dim YoutubeDL As Process = Process.Start(New ProcessStartInfo(YoutubeDLLocation, If(Provider = SearchProvider.YoutubeMusic,
                                                                                      $"""https://music.youtube.com/search?q={System.Web.HttpUtility.UrlEncode(Query)}"" --playlist-end {Limit} -j -s -f {VideoQuality.ToString}",
                                                                                      $"""{If(Provider = SearchProvider.Youtube, "yt", "sc")}search{Limit}:{Query}"" -j -s -f {VideoQuality.ToString}")) With {.RedirectStandardOutput = True, .UseShellExecute = False, .WindowStyle = ProcessWindowStyle.Hidden, .CreateNoWindow = True})
                        Dim sout = Await YoutubeDL.StandardOutput.ReadToEndAsync '.Split(New Char() {"\n", "\r", "\r\n"})                        
                        SetKnownState(LinkState.ParsingData)
                        IsBusy = False
                        Dim mVid = JsonConvert.DeserializeObject(Of YoutubeVideo)(sout)
                        Return {mVid}
                    Catch ex As Exception
                        IsBusy = False
                        Utilities.DebugMode.Instance.Log(Of Youtube.YoutubeDL)(ex.ToString)
                    End Try
                    Return Nothing
                End If
            End If
            Return Nothing
        End Function
        Public Async Function DumpAndManagePlaylist(URL As String, Optional limit As Integer = -1) As Task(Of YoutubePlaylist)
            If Not IO.File.Exists(YoutubeDLLocation) Then Return Nothing
            If Utilities.SharedProperties.Instance.IsInternetConnected Then
                IsBusy = True
                SetKnownState(LinkState.ContactingYTDL)
                Dim YoutubeDL As Process = Process.Start(New ProcessStartInfo(YoutubeDLLocation, "-j -s -f " & VideoQuality.ToString & If(limit = -1, " --yes-playlist ", " --playlist-end " & limit) & $" ""{URL}""") With {.RedirectStandardOutput = True, .UseShellExecute = False, .WindowStyle = ProcessWindowStyle.Hidden, .CreateNoWindow = True})
                Dim Info = Await YoutubeDL.StandardOutput.ReadToEndAsync
                'fix invalid json
                Try
                    SetKnownState(LinkState.ParsingData)
                    If Info.ToString.StartsWith("{") Then 'Indicates JToken = Successful                                
                        Dim mPlaylist = JsonConvert.DeserializeObject(Of YoutubePlaylist)(Info)
                        IsBusy = False
                        Return mPlaylist
                    End If
                Catch ex As Exception
                    IsBusy = False
                    Utilities.DebugMode.Instance.Log(Of YoutubeDL)(ex.ToString)
                    Return Nothing
                End Try
            End If
            Return Nothing
        End Function
        Public Sub SetKnownState(state As LinkState)
            Select Case state
                Case LinkState.Free
                    Configuration.SetStatus("...", Configuration.Progress)
                Case LinkState.ContactingYTDL
                    Configuration.SetStatus("Connecting to Youtube-DL", Configuration.Progress)
                Case LinkState.ParsingData
                    Configuration.SetStatus("Parsing Data...", Configuration.Progress)
            End Select
        End Sub
        Public Async Function ParseYoutubeJson(StrJson As String) As Task(Of YoutubeVideoEX)
            Return Await Task.Run(Function()
                                      Try
                                          Dim Info = StrJson
                                          If String.IsNullOrEmpty(Info.Trim) Then Throw New InvalidOperationException
                                          'Acquiring Info                                                  
                                          Dim ParsedInfo = Json.Linq.JObject.Parse(Info)
                                          Dim WebPage = ParsedInfo("webpage_url")
                                          Dim FullTitle = ParsedInfo("fulltitle")
                                          Dim AltTitle = ParsedInfo("alt_title")
                                          Dim AudioCodec = ParsedInfo("acodec")
                                          Dim Author = If(ParsedInfo("channel"), ParsedInfo("uploader"))
                                          Dim AuhorID = If(ParsedInfo("channel_id"), ParsedInfo("uploader_id"))
                                          Dim AuthorURL = If(ParsedInfo("channel_url"), ParsedInfo("uploader_url"))
                                          Dim RawCategories = ParsedInfo("categories")
                                          Dim Categories = If(RawCategories Is Nothing, New String() {""}, String.Join(Environment.NewLine, RawCategories).Split(UniversalSplitter))
                                          Dim Creator = ParsedInfo("creator")
                                          Dim Description = ParsedInfo("description")
                                          Dim RawDirectURLS = ParsedInfo("formats")
                                          'Dim RawDirectURLS = ParsedInfo("requested_formats")
                                          Dim DirectURLS As New List(Of YoutubeVideoEX.Video)
                                          Dim AudioOnlyURLS As New List(Of YoutubeVideoEX.Video)
                                          Dim VideoOnlyURLS As New List(Of YoutubeVideoEX.Video)
                                          Dim MixedOnlyURLS As New List(Of YoutubeVideoEX.Video)
                                          For Each RURL As Json.Linq.JToken In RawDirectURLS
                                              Dim CVideo As New YoutubeVideoEX.Video
                                              CVideo.AudioCodec = RURL("acodec")
                                              CVideo.Container = RURL("container")
                                              CVideo.DirectURL = RURL("url")
                                              CVideo.Extension = RURL("ext")
                                              CVideo.FileSize = RURL("filesize")
                                              CVideo.Format = RURL("format")
                                              CVideo.FormatID = RURL("format_id")
                                              CVideo.FormatNote = RURL("format_note")
                                              CVideo.FPS = RURL("fps")
                                              CVideo.Height = RURL("height")
                                              Dim RawHTTPHeader = RURL("http_headers")
                                              Dim HTTPHeader As New YoutubeVideoEX.Video.HTTPHeader
                                              HTTPHeader.Accept = RawHTTPHeader("Accept")
                                              HTTPHeader.AcceptCharset = RawHTTPHeader("Accept-Charset")
                                              HTTPHeader.AcceptEncoding = RawHTTPHeader("Accept-Encoding")
                                              HTTPHeader.AcceptLanguage = RawHTTPHeader("Accept-Language")
                                              HTTPHeader.UserAgent = RawHTTPHeader("User-Agent")
                                              CVideo.HTTPHeaders = HTTPHeader
                                              CVideo.Protocol = RURL("protocol")
                                              CVideo.Quality = RURL("quality")
                                              CVideo.VideoCodec = RURL("vcodec")
                                              CVideo.Width = RURL("width")
                                              If CVideo.AudioCodec <> "none" AndAlso CVideo.VideoCodec <> "none" Then
                                                  CVideo.Type = YoutubeVideoEX.Video.FileType.Mixed
                                              ElseIf CVideo.AudioCodec <> "none" AndAlso CVideo.VideoCodec = "none" Then
                                                  CVideo.Type = YoutubeVideoEX.Video.FileType.Audio
                                              ElseIf CVideo.AudioCodec = "none" AndAlso CVideo.VideoCodec <> "none" Then
                                                  CVideo.Type = YoutubeVideoEX.Video.FileType.Video
                                              End If
                                              Select Case CVideo.Type
                                                  Case YoutubeVideoEX.Video.FileType.Mixed
                                                      CVideo.VideoBitrate = RURL("vbr")
                                                      CVideo.AudioSampleRate = RURL("asr")
                                                      CVideo.TotalBitrate = RURL("TotalBitrate")
                                                      CVideo.AudioBitrate = RURL("abr")
                                                      MixedOnlyURLS.Add(CVideo)
                                                  Case YoutubeVideoEX.Video.FileType.Audio
                                                      CVideo.AudioSampleRate = RURL("asr")
                                                      CVideo.TotalBitrate = RURL("TotalBitrate")
                                                      CVideo.AudioBitrate = RURL("abr")
                                                      AudioOnlyURLS.Add(CVideo)
                                                  Case YoutubeVideoEX.Video.FileType.Video
                                                      CVideo.VideoBitrate = RURL("vbr")
                                                      CVideo.AudioSampleRate = RURL("asr")
                                                      CVideo.TotalBitrate = RURL("TotalBitrate")
                                                      VideoOnlyURLS.Add(CVideo)
                                              End Select
                                              DirectURLS.Add(CVideo)
                                          Next
                                          'AudioOnlyURLS.OrderBy(Function(k) k.FileSize)
                                          'VideoOnlyURLS.OrderBy(Function(k) k.Width)
                                          'MixedOnlyURLS.OrderBy(Function(k) k.Width)
                                          Dim HQAudio = If(AudioOnlyURLS?.Any, AudioOnlyURLS.Last, New YoutubeVideoEX.Video)
                                          Dim LQAudio = If(AudioOnlyURLS?.Any, AudioOnlyURLS.First, New YoutubeVideoEX.Video)
                                          Dim HQVideo = If(VideoOnlyURLS?.Any, VideoOnlyURLS.Last, New YoutubeVideoEX.Video)
                                          Dim LQVideo = If(VideoOnlyURLS?.Any, VideoOnlyURLS.First, New YoutubeVideoEX.Video)
                                          Dim HQMixed = If(MixedOnlyURLS?.Any, MixedOnlyURLS.Last, New YoutubeVideoEX.Video)
                                          Dim LQMixed = If(MixedOnlyURLS?.Any, MixedOnlyURLS.First, New YoutubeVideoEX.Video)
                                          Dim DislikeCount = ParsedInfo("dislike_count")
                                          Dim Duration = TimeSpan.FromSeconds(If(ParsedInfo("duration"), 0))
                                          Dim FileName = ParsedInfo("_filename")
                                          Dim FPS = ParsedInfo("fps")
                                          Dim LikeCount = ParsedInfo("like_count")
                                          Dim RawSubtitles = ParsedInfo("subtitles")
                                          Dim Subtitles As New List(Of Tuple(Of String, List(Of YoutubeVideoEX.Subtitle)))
                                          If RawSubtitles IsNot Nothing Then
                                              For Each Subtitle As Json.Linq.JToken In RawSubtitles
                                                  Dim CSubtitle As New List(Of YoutubeVideoEX.Subtitle)
                                                  For Each _Subtitle As Json.Linq.JToken In Subtitle
                                                      For Each __Subtitle As Json.Linq.JToken In _Subtitle
                                                          Dim CCSubtitle As New YoutubeVideoEX.Subtitle
                                                          CCSubtitle.Language = CType(Subtitle, Json.Linq.JProperty).Name
                                                          CCSubtitle.Extension = __Subtitle("ext") 'String.Join(Environment.NewLine, _Subtitle("ext")).Split(UniversalSplitter) '_Subtitle("ext")
                                                          CCSubtitle.RawData = __Subtitle("url")
                                                          CSubtitle.Add(CCSubtitle)
                                                      Next
                                                  Next
                                                  Subtitles.Add(New Tuple(Of String, List(Of YoutubeVideoEX.Subtitle))(If(CSubtitle.FirstOrDefault?.Language, "null"), CSubtitle))
                                              Next
                                          End If
                                          Dim RawTags = ParsedInfo("tags")
                                          Dim Tags As New List(Of String)
                                          If RawTags IsNot Nothing Then
                                              For Each Tag In RawTags
                                                  Tags.Add(TryCast(Tag, Json.Linq.JValue)?.Value)
                                              Next
                                          End If
                                          Dim RawThumbnails = ParsedInfo("thumbnails")
                                          Dim Thumbnails As New List(Of Thumbnail)
                                          If RawThumbnails IsNot Nothing Then
                                              For Each Thumbnail As Json.Linq.JToken In RawThumbnails
                                                  Try
                                                      Dim CThumbnail As New Thumbnail
                                                      CThumbnail.ID = Thumbnail("id")
                                                      CThumbnail.Resolution = Thumbnail("resolution")
                                                      CThumbnail.URL = Thumbnail("url")
                                                      CThumbnail.Height = If(Thumbnail("height"), 0)
                                                      CThumbnail.Width = If(Thumbnail("width"), 0)
                                                      Thumbnails.Add(CThumbnail)
                                                  Catch ex As Exception
                                                      Utilities.DebugMode.Instance.Log(Of YoutubeDL)(ex.ToString)
                                                  End Try
                                              Next
                                          End If
                                          Dim HQThumbnail = If(Thumbnails.Any, Thumbnails.Last, New Thumbnail)
                                          Dim LQThumbnail = If(Thumbnails.Any, Thumbnails.First, New Thumbnail)
                                          Dim ThumbnailURL = ParsedInfo("thumbnail")
                                          'Media Properties
                                          Dim Track = ParsedInfo("track")
                                          Dim Artist = ParsedInfo("artist")
                                          Dim Artists As String() = If(Artist?.ToString.Split(","), {})
                                          Dim TrackNumber = ParsedInfo("track_number")
                                          Dim TrackID = ParsedInfo("track_id")
                                          Dim Genre = ParsedInfo("genre")
                                          Dim Genres As String() = If(Genre?.ToString.Split(New Char() {",", "/"}), {})
                                          Dim Album = ParsedInfo("album")
                                          Dim AlbumType = ParsedInfo("album_type")
                                          Dim AlbumArtist = ParsedInfo("album_artist")
                                          Dim DiscNumber = ParsedInfo("disc_number")
                                          Dim ReleaseYear = ParsedInfo("release_year")
                                          Dim MediaInfo As New YoutubeVideoEX.MediaProperties
                                          MediaInfo.Album = Album
                                          MediaInfo.AlbumArtist = AlbumArtist
                                          MediaInfo.AlbumType = AlbumType
                                          MediaInfo.Artists = Artists
                                          MediaInfo.DiscNumber = If(DiscNumber, 0)
                                          MediaInfo.Genres = Genres
                                          Dim iReleaseYear As Integer
                                          Integer.TryParse(ReleaseYear?.ToString, iReleaseYear)
                                          MediaInfo.ReleaseYear = iReleaseYear
                                          MediaInfo.Track = Track
                                          MediaInfo.TrackNumber = If(TrackNumber, 0)
                                          MediaInfo.Track_ID = TrackID
                                          'Playlist Properties
                                          Dim Playlist = ParsedInfo("playlist")
                                          Dim PlaylistIndex = ParsedInfo("playlist_index")
                                          Dim PlaylistID = ParsedInfo("playlist_id")
                                          Dim PlaylistTitle = ParsedInfo("playlist_title")
                                          Dim PlaylistUploader = ParsedInfo("playlist_uploader")
                                          Dim PlaylistUploader_ID = ParsedInfo("playlist_uploader_id")
                                          Dim PlaylistInfo As New YoutubeVideoEX.YoutubePlaylist
                                          Dim intPlaylistIndex = 0
                                          PlaylistInfo.Index = If(PlaylistIndex IsNot Nothing, If(Integer.TryParse(PlaylistIndex, intPlaylistIndex), intPlaylistIndex, -1), -1)
                                          PlaylistInfo.Playlist_ID = PlaylistID
                                          PlaylistInfo.Title = PlaylistTitle
                                          PlaylistInfo.Uploader = PlaylistUploader
                                          PlaylistInfo.Uploader_ID = PlaylistUploader_ID
                                          'Chapter info
                                          Dim RawChapters = ParsedInfo("chapters")
                                          Dim Chapters As New List(Of YoutubeVideoEX.Chapter)
                                          If RawChapters IsNot Nothing Then
                                              For Each Chapter As Json.Linq.JToken In RawChapters
                                                  Try
                                                      If Chapter Is Nothing Then Continue For
                                                      Dim ETime = Chapter("end_time")
                                                      Dim STime = Chapter("start_time")
                                                      Dim CTitle = Chapter("title")
                                                      Dim Chap As New YoutubeVideoEX.Chapter
                                                      Chap.EndTime = If(ETime IsNot Nothing, TimeSpan.FromSeconds(ETime), Nothing)
                                                      Chap.StarTime = If(STime IsNot Nothing, TimeSpan.FromSeconds(STime), Nothing)
                                                      Chap.Title = CTitle
                                                      Chapters.Add(Chap)
                                                  Catch ex As Exception
                                                      Utilities.DebugMode.Instance.Log(Of YoutubeDL)(ex.ToString)
                                                  End Try
                                              Next
                                          End If
                                          Dim Title = ParsedInfo("title")
                                          Dim RawUploadDate = ParsedInfo("upload_date")
                                          Dim UploadDate As New Date(RawUploadDate.ToString.Substring(0, 4), RawUploadDate.ToString.Substring(4, 2), RawUploadDate.ToString.Substring(6, 2))
                                          Dim VideoCodec = ParsedInfo("vcodec")
                                          Dim VideoExtension = ParsedInfo("ext")
                                          Dim ViewCount = ParsedInfo("view_count")
                                          'Returning Info
                                          Dim Video = New YoutubeVideoEX
                                          With Video
                                              .URL = WebPage
                                              .JSONDump = Info
                                              .AlternateTitle = AltTitle
                                              .ThumbnailURL = ThumbnailURL
                                              .AudioCodec = AudioCodec
                                              .Author = Author
                                              .AuthorID = AuhorID
                                              .AuthorURL = AuthorURL
                                              .Categories = Categories
                                              .Creator = Creator
                                              .Description = Description
                                              .DislikeCount = DislikeCount?.ToString
                                              .Duration = Duration
                                              .FileName = FileName
                                              .FPS = If(FPS, 0)
                                              .FullTitle = FullTitle
                                              .LikeCount = If(LikeCount, 0)
                                              .Subtitles = Subtitles
                                              .Tags = Tags
                                              .Thumbnails = Thumbnails
                                              .Title = Title
                                              .UploadDate = UploadDate
                                              .VideoCodec = VideoCodec
                                              .VideoExtension = VideoExtension
                                              .ViewCount = ViewCount
                                              .Videos = DirectURLS
                                              .HQAudio = HQAudio
                                              .HQMixed = HQMixed
                                              .HQThumbnail = HQThumbnail
                                              .HQVideo = HQVideo
                                              .LQAudio = LQAudio
                                              .LQMixed = LQMixed
                                              .LQThumbnail = LQThumbnail
                                              .LQVideo = LQVideo
                                              .AudioOnly = AudioOnlyURLS
                                              .VideoOnly = VideoOnlyURLS
                                              .MixedOnly = MixedOnlyURLS
                                              .Chapters = Chapters
                                              .Properties = MediaInfo
                                              .Playlist = PlaylistInfo
                                              'Damn debugging
                                          End With
                                          Return Video
                                      Catch ex As Exception
                                          Utilities.DebugMode.Instance.Log(Of Youtube.YoutubeDL)(ex.ToString)
                                      End Try
                                      Return Nothing
                                  End Function)
        End Function
        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="URL">Youtube-DL Compatible URL</param>
        ''' <returns></returns>
        Public Async Function DumpAndManageVideo(URL As String) As Task(Of YoutubeVideo)
            If Not IO.File.Exists(YoutubeDLLocation) Then Return Nothing
            If Utilities.SharedProperties.Instance.IsInternetConnected Then
                Try
                    IsBusy = True
                    SetKnownState(LinkState.ContactingYTDL)
                    Dim YoutubeDL As Process = Process.Start(New ProcessStartInfo(YoutubeDLLocation, $"-j  -s -f {VideoQuality.ToString} ""{URL.Split("&")(0)}""") With {.RedirectStandardOutput = True, .UseShellExecute = False, .WindowStyle = ProcessWindowStyle.Hidden, .CreateNoWindow = True})
                    Dim Info = Await YoutubeDL.StandardOutput.ReadToEndAsync
                    SetKnownState(LinkState.ParsingData)
                    If Info.StartsWith("{") Then 'Indicates JToken = Successful
                        Dim jdoc = Newtonsoft.Json.Linq.JObject.Parse(Info)
                        Dim mVid = JsonConvert.DeserializeObject(Of YoutubeVideo)(jdoc.ToString)
                        IsBusy = False
                        Return mVid
                    Else
                        IsBusy = False
                        Return Nothing
                    End If
                Catch ex As Exception
                    IsBusy = False
                    Utilities.DebugMode.Instance.Log(Of Youtube.YoutubeDL)(ex.ToString)
                End Try
                Return Nothing
            End If
            Return Nothing
        End Function

        Public Sub Init() Implements IStartupItem.Init
            Utilities.SharedProperties.Instance.ItemsConfiguration.Add(Configuration)
        End Sub

    End Class
    Namespace WPF
        Public Class Int32ToQualityConverter
            Implements IValueConverter

            Public Function Convert(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.Convert
                Return CInt(CType(value, Youtube.Classes.VideoQuality))
            End Function

            Public Function ConvertBack(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.ConvertBack
                Return CType(CInt(value), Youtube.Classes.VideoQuality)
            End Function
        End Class

        Public Class Int32ToSearchProviderConverter
            Implements IValueConverter

            Public Function Convert(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.Convert
                Return CInt(CType(value, Youtube.YoutubeDL.SearchProvider))
            End Function

            Public Function ConvertBack(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.ConvertBack
                Return CType(CInt(value), Youtube.YoutubeDL.SearchProvider)
            End Function
        End Class

        Public Class LoadCommand
            Implements ICommand

            Public Event CanExecuteChanged As EventHandler Implements ICommand.CanExecuteChanged
            Private _Client As YoutubeDL

            Sub New(client As YoutubeDL)
                _Client = client
                AddHandler CommandManager.RequerySuggested, New EventHandler(Sub(sender As Object, e As EventArgs)
                                                                                 RaiseEvent CanExecuteChanged(Me, New EventArgs())
                                                                             End Sub)
            End Sub

            Public Async Sub Execute(parameter As Object) Implements ICommand.Execute
                If Not Utilities.SharedProperties.Instance.IsInternetConnected Then
                    HandyControl.Controls.MessageBox.Show(Application.Current.MainWindow, Utilities.ResourceResolver.Strings.QUERY_INTERNET_DISCONNECTED, icon:=MessageBoxImage.Error)
                    Return
                End If
                Dim url = Dialogs.InputBox.ShowSingle(Application.Current.MainWindow, "URL")
                If String.IsNullOrEmpty(url) Then Return
                Dim video = Await _Client.DumpAndManageVideo(url)
                If video Is Nothing Then Return
                Dim meta = video.AsMetadata(_Client.VideoQuality)
                Dim result = Await TryCast(parameter, Player.Player)?.LoadSong(meta)
                If Not result Then
                    Dim ib As New Dialogs.InputBox()
                    ib.AddComboBox("Quality", video.Formats.Select(Of String)(Function(k) $"{k.VideoExtension}[{k.VideoCodec}];{k.AudioExtension}[{k.AudioExtension}] @{If(k.Resolution, "0x0")}"))
                    If Not ib.ShowDialog Then
                        Return
                    End If
                    Dim format = video.Formats.Item(CInt(ib.Value("Quality")))
                    meta.Path = format.URL
                    If Not Await TryCast(parameter, Player.Player)?.LoadSong(meta) Then
                        Return
                    End If
                End If
                Dim Vsub = If(If(video.Subtitles.en, If(video.Subtitles.ja, video.Subtitles.ar)),
                    If(video.AutomaticCaptions.en, If(video.AutomaticCaptions.ja, video.AutomaticCaptions.ar)))
                If Vsub IsNot Nothing Then
                    Dim _Vsub = Vsub.FirstOrDefault(Function(k) k.Extension = "vtt")
                    If _Vsub IsNot Nothing Then
                        If TypeOf Application.Current.MainWindow Is MainWindow Then
                            With CType(Application.Current.MainWindow, MainWindow)
                                Dim parser As New SubtitlesParser.Classes.Parsers.SubParser
                                Using httpc As New Net.Http.HttpClient()
                                    Dim data = Await httpc.GetStreamAsync(_Vsub.URL)
                                    Dim subs = parser.ParseStream(data)
                                    Application.Current.Dispatcher.Invoke(Sub()
                                                                              If .Subtitles Is Nothing Then .Subtitles = New ObjectModel.ObservableCollection(Of SubtitlesParser.Classes.SubtitleItem)
                                                                              For Each _sub In subs
                                                                                  .Subtitles?.Add(_sub)
                                                                                  Dim PosSyncProc As New Un4seen.Bass.SYNCPROC(Sub(h, c, _data, u)
                                                                                                                                   Application.Current.Dispatcher.Invoke(Sub()
                                                                                                                                                                             '.InfoText = String.Join(Environment.NewLine, _sub.PlaintextLines)
                                                                                                                                                                             .CurrentSubtitle = _sub
                                                                                                                                                                         End Sub)
                                                                                                                               End Sub)
                                                                                  Dim stream = TryCast(parameter, Player.Player)?.Stream
                                                                                  Dim PosSyncProcHandle = Un4seen.Bass.Bass.BASS_ChannelSetSync(stream, Un4seen.Bass.BASSSync.BASS_SYNC_POS, Un4seen.Bass.Bass.BASS_ChannelSeconds2Bytes(stream, TimeSpan.FromMilliseconds(_sub.StartTime).TotalSeconds), PosSyncProc, IntPtr.Zero)
                                                                                  ._SubtilesPositionSyncProcs.Add(New Tuple(Of Integer, Integer, Un4seen.Bass.SYNCPROC)(stream, PosSyncProcHandle, PosSyncProc))
                                                                              Next
                                                                              .IsUsingSubtitles = True
                                                                          End Sub)
                                End Using
                            End With
                        End If
                    End If
                Else
                    For Each cap In video.AutomaticCaptions.AsArray
                        If cap IsNot Nothing Then
                            Dim _Vsub = cap.FirstOrDefault(Function(k) k.Extension = "vtt")
                            If _Vsub IsNot Nothing Then
                                If TypeOf Application.Current.MainWindow Is MainWindow Then
                                    With CType(Application.Current.MainWindow, MainWindow)
                                        Dim parser As New SubtitlesParser.Classes.Parsers.SubParser
                                        Using httpc As New Net.Http.HttpClient()
                                            Dim data = Await httpc.GetStreamAsync(_Vsub.URL)
                                            Dim subs = parser.ParseStream(data)
                                            Application.Current.Dispatcher.Invoke(Sub()
                                                                                      If .Subtitles Is Nothing Then .Subtitles = New ObjectModel.ObservableCollection(Of SubtitlesParser.Classes.SubtitleItem)
                                                                                      For Each _sub In subs
                                                                                          .Subtitles?.Add(_sub)
                                                                                          Dim PosSyncProc As New Un4seen.Bass.SYNCPROC(Sub(h, c, _data, u)
                                                                                                                                           Application.Current.Dispatcher.Invoke(Sub()
                                                                                                                                                                                     '.InfoText = String.Join(Environment.NewLine, _sub.PlaintextLines)
                                                                                                                                                                                     .CurrentSubtitle = _sub
                                                                                                                                                                                 End Sub)
                                                                                                                                       End Sub)
                                                                                          Dim stream = TryCast(parameter, Player.Player)?.Stream
                                                                                          Dim PosSyncProcHandle = Un4seen.Bass.Bass.BASS_ChannelSetSync(stream, Un4seen.Bass.BASSSync.BASS_SYNC_POS, Un4seen.Bass.Bass.BASS_ChannelSeconds2Bytes(stream, TimeSpan.FromMilliseconds(_sub.StartTime).TotalSeconds), PosSyncProc, IntPtr.Zero)
                                                                                          ._SubtilesPositionSyncProcs.Add(New Tuple(Of Integer, Integer, Un4seen.Bass.SYNCPROC)(stream, PosSyncProcHandle, PosSyncProc))
                                                                                      Next
                                                                                      .IsUsingSubtitles = True
                                                                                  End Sub)
                                        End Using
                                    End With
                                End If
                            End If
                        End If
                    Next
                End If
            End Sub

            Public Function CanExecute(parameter As Object) As Boolean Implements ICommand.CanExecute
                Return SharedProperties.Instance.IsInternetConnected AndAlso Not IsNothing(_Client) AndAlso IO.File.Exists(_Client.YoutubeDLLocation) AndAlso TypeOf parameter Is Player.Player
            End Function
        End Class
        Public Class QuickLoadCommand
            Implements ICommand

            Public Event CanExecuteChanged As EventHandler Implements ICommand.CanExecuteChanged
            Private _Client As YoutubeDL

            Sub New(client As YoutubeDL)
                _Client = client
                AddHandler CommandManager.RequerySuggested, New EventHandler(Sub(sender As Object, e As EventArgs)
                                                                                 RaiseEvent CanExecuteChanged(Me, New EventArgs())
                                                                             End Sub)
            End Sub

            Public Async Sub Execute(parameter As Object) Implements ICommand.Execute
                If Not Utilities.SharedProperties.Instance.IsInternetConnected Then
                    HandyControl.Controls.MessageBox.Error(Utilities.ResourceResolver.Strings.QUERY_INTERNET_DISCONNECTED)
                    Return
                End If
                Dim url = Dialogs.InputBox.ShowSingle(Application.Current.MainWindow, "URL")
                If String.IsNullOrEmpty(url) Then Return
                Dim video = Await _Client.GetVideo(url)
                If video Is Nothing Then Return
                Await TryCast(parameter, Player.Player)?.LoadSong(New Player.Metadata With {.Location = Player.Metadata.FileLocation.Remote, .OriginalPath = url, .Provider = New Youtube.YoutubeDLMediaProvider(url), .Extension = "mp4", .Path = video.URL, .Title = video.Title})
            End Sub

            Public Function CanExecute(parameter As Object) As Boolean Implements ICommand.CanExecute
                Return SharedProperties.Instance.IsInternetConnected AndAlso Not IsNothing(_Client) AndAlso IO.File.Exists(_Client.YoutubeDLLocation) AndAlso TypeOf parameter Is Player.Player
            End Function
        End Class
        Public Class SearchAndLoadCommand
            Implements ICommand

            Public Event CanExecuteChanged As EventHandler Implements ICommand.CanExecuteChanged
            Private _Client As YoutubeDL

            Sub New(client As YoutubeDL)
                _Client = client
                AddHandler CommandManager.RequerySuggested, New EventHandler(Sub(sender As Object, e As EventArgs)
                                                                                 RaiseEvent CanExecuteChanged(Me, New EventArgs())
                                                                             End Sub)
            End Sub

            Public Async Sub Execute(parameter As Object) Implements ICommand.Execute
                If Not Utilities.SharedProperties.Instance.IsInternetConnected Then
                    HandyControl.Controls.MessageBox.Error(Utilities.ResourceResolver.Strings.QUERY_INTERNET_DISCONNECTED)
                    Return
                End If
                Dim query = Dialogs.InputBox.ShowSingle(Application.Current.MainWindow, "Query")
                If String.IsNullOrEmpty(query) Then Return
                Dim videos = Await _Client.SearchVideoAndDump(query, YoutubeDL.SearchProvider.Youtube)
                Dim video = videos.FirstOrDefault
                If video Is Nothing Then Return
                If HandyControl.Controls.MessageBox.Ask(Utilities.ResourceResolver.Strings.QUERY_FILEPLAY.Replace("%f", video.Title & " - " & video.Channel)) = MessageBoxResult.Cancel Then
                    Return
                End If
                Dim meta = video.AsMetadata(_Client.VideoQuality)
#Disable Warning
                Dim result = Await TryCast(parameter, Player.Player)?.LoadSong(meta)
#Enable Warning
                If Not result Then
                    Dim ib As New Dialogs.InputBox()
                    ib.AddComboBox("Quality", video.Formats.Select(Of String)(Function(k) $"{k.VideoExtension}[{k.VideoCodec}];{k.AudioExtension}[{k.AudioExtension}] In {k.Container} @{If(k.Resolution, "0x0")}"))
                    'If Not ib.ShowDialog Then
                    '    Return
                    'End If
                    Dim succPlay As Boolean = False
                    Dim sDlg As Boolean = ib.ShowDialog
                    If Not sDlg Then Return
                    Do While sDlg
                        Dim format = video.Formats.Item(CInt(ib.Value("Quality")))
                        meta.Path = format.URL
                        If Await TryCast(parameter, Player.Player)?.LoadSong(meta) Then
                            succPlay = True
                            Exit Do
                        End If
                        sDlg = ib.ShowDialog
                    Loop
                    If Not succPlay Then Return
                End If
                Dim Vsub = If(If(video.Subtitles.en, If(video.Subtitles.ja, video.Subtitles.ar)),
                    If(video.AutomaticCaptions.en, If(video.AutomaticCaptions.ja, video.AutomaticCaptions.ar)))
                If Vsub IsNot Nothing Then
                    Dim _Vsub = Vsub.FirstOrDefault(Function(k) k.Extension = "vtt")
                    If _Vsub IsNot Nothing Then
                        If TypeOf Application.Current.MainWindow Is MainWindow Then
                            With CType(Application.Current.MainWindow, MainWindow)
                                Dim parser As New SubtitlesParser.Classes.Parsers.SubParser
                                Using httpc As New Net.Http.HttpClient()
                                    Dim data = Await httpc.GetStreamAsync(_Vsub.URL)
                                    Dim subs = parser.ParseStream(data)
                                    Application.Current.Dispatcher.Invoke(Sub()
                                                                              If .Subtitles Is Nothing Then .Subtitles = New ObjectModel.ObservableCollection(Of SubtitlesParser.Classes.SubtitleItem)
                                                                              For Each _sub In subs
                                                                                  .Subtitles?.Add(_sub)
                                                                                  Dim PosSyncProc As New Un4seen.Bass.SYNCPROC(Sub(h, c, _data, u)
                                                                                                                                   Application.Current.Dispatcher.Invoke(Sub()
                                                                                                                                                                             '.InfoText = String.Join(Environment.NewLine, _sub.PlaintextLines)
                                                                                                                                                                             .CurrentSubtitle = _sub
                                                                                                                                                                         End Sub)
                                                                                                                               End Sub)
                                                                                  Dim stream = TryCast(parameter, Player.Player)?.Stream
                                                                                  Dim PosSyncProcHandle = Un4seen.Bass.Bass.BASS_ChannelSetSync(stream, Un4seen.Bass.BASSSync.BASS_SYNC_POS, Un4seen.Bass.Bass.BASS_ChannelSeconds2Bytes(stream, TimeSpan.FromMilliseconds(_sub.StartTime).TotalSeconds), PosSyncProc, IntPtr.Zero)
                                                                                  ._SubtilesPositionSyncProcs.Add(New Tuple(Of Integer, Integer, Un4seen.Bass.SYNCPROC)(stream, PosSyncProcHandle, PosSyncProc))
                                                                              Next
                                                                              .IsUsingSubtitles = True
                                                                          End Sub)
                                End Using
                            End With
                        End If
                    End If
                Else
                    For Each cap In video.AutomaticCaptions.AsArray
                        If cap IsNot Nothing Then
                            Dim _Vsub = cap.FirstOrDefault(Function(k) k.Extension = "vtt")
                            If _Vsub IsNot Nothing Then
                                If TypeOf Application.Current.MainWindow Is MainWindow Then
                                    With CType(Application.Current.MainWindow, MainWindow)
                                        Dim parser As New SubtitlesParser.Classes.Parsers.SubParser
                                        Using httpc As New Net.Http.HttpClient()
                                            Dim data = Await httpc.GetStreamAsync(_Vsub.URL)
                                            Dim subs = parser.ParseStream(data)
                                            Application.Current.Dispatcher.Invoke(Sub()
                                                                                      If .Subtitles Is Nothing Then .Subtitles = New ObjectModel.ObservableCollection(Of SubtitlesParser.Classes.SubtitleItem)
                                                                                      For Each _sub In subs
                                                                                          .Subtitles?.Add(_sub)
                                                                                          Dim PosSyncProc As New Un4seen.Bass.SYNCPROC(Sub(h, c, _data, u)
                                                                                                                                           Application.Current.Dispatcher.Invoke(Sub()
                                                                                                                                                                                     '.InfoText = String.Join(Environment.NewLine, _sub.PlaintextLines)
                                                                                                                                                                                     .CurrentSubtitle = _sub
                                                                                                                                                                                 End Sub)
                                                                                                                                       End Sub)
                                                                                          Dim stream = TryCast(parameter, Player.Player)?.Stream
                                                                                          Dim PosSyncProcHandle = Un4seen.Bass.Bass.BASS_ChannelSetSync(stream, Un4seen.Bass.BASSSync.BASS_SYNC_POS, Un4seen.Bass.Bass.BASS_ChannelSeconds2Bytes(stream, TimeSpan.FromMilliseconds(_sub.StartTime).TotalSeconds), PosSyncProc, IntPtr.Zero)
                                                                                          ._SubtilesPositionSyncProcs.Add(New Tuple(Of Integer, Integer, Un4seen.Bass.SYNCPROC)(stream, PosSyncProcHandle, PosSyncProc))
                                                                                      Next
                                                                                      .IsUsingSubtitles = True
                                                                                  End Sub)
                                        End Using
                                    End With
                                End If
                            End If
                        End If
                    Next
                End If
            End Sub

            Public Function CanExecute(parameter As Object) As Boolean Implements ICommand.CanExecute
                Return SharedProperties.Instance.IsInternetConnected AndAlso Not IsNothing(_Client) AndAlso IO.File.Exists(_Client.YoutubeDLLocation) AndAlso TypeOf parameter Is Player.Player
            End Function
        End Class
        Public Class SearchAndLoadDefaultCommand
            Implements ICommand

            Public Event CanExecuteChanged As EventHandler Implements ICommand.CanExecuteChanged
            Private _Client As YoutubeDL

            Sub New(client As YoutubeDL)
                _Client = client
                AddHandler CommandManager.RequerySuggested, New EventHandler(Sub(sender As Object, e As EventArgs)
                                                                                 RaiseEvent CanExecuteChanged(Me, New EventArgs())
                                                                             End Sub)
            End Sub

            Public Async Sub Execute(parameter As Object) Implements ICommand.Execute
                If Not Utilities.SharedProperties.Instance.IsInternetConnected Then
                    HandyControl.Controls.MessageBox.Error(Utilities.ResourceResolver.Strings.QUERY_INTERNET_DISCONNECTED)
                    Return
                End If
                Dim query = Dialogs.InputBox.ShowSingle(Application.Current.MainWindow, "Query")
                If String.IsNullOrEmpty(query) Then Return
                Dim videos = Await _Client.SearchVideoAndDump(query, _Client.DefaultSearchProvider)
                Dim video = videos.FirstOrDefault
                If video Is Nothing Then Return
                If HandyControl.Controls.MessageBox.Ask(Utilities.ResourceResolver.Strings.QUERY_FILEPLAY.Replace("%f", video.Title & " - " & video.Channel)) = MessageBoxResult.Cancel Then
                    Return
                End If
                Dim meta = video.AsMetadata(_Client.VideoQuality)
#Disable Warning
                Dim result = Await TryCast(parameter, Player.Player)?.LoadSong(meta)
#Enable Warning
                If Not result Then
                    Dim ib As New Dialogs.InputBox()
                    ib.AddComboBox("Quality", video.Formats.Select(Of String)(Function(k) $"{k.VideoExtension}[{k.VideoCodec}];{k.AudioExtension}[{k.AudioExtension}] In {k.Container} @{If(k.Resolution, "0x0")}"))
                    'If Not ib.ShowDialog Then
                    '    Return
                    'End If
                    Dim succPlay As Boolean = False
                    Dim sDlg As Boolean = ib.ShowDialog
                    If Not sDlg Then Return
                    Do While sDlg
                        Dim format = video.Formats.Item(CInt(ib.Value("Quality")))
                        meta.Path = format.URL
                        If Await TryCast(parameter, Player.Player)?.LoadSong(meta) Then
                            succPlay = True
                            Exit Do
                        End If
                        sDlg = ib.ShowDialog
                    Loop
                    If Not succPlay Then Return
                End If
                Dim Vsub = If(If(video.Subtitles.en, If(video.Subtitles.ja, video.Subtitles.ar)),
                    If(video.AutomaticCaptions.en, If(video.AutomaticCaptions.ja, video.AutomaticCaptions.ar)))
                If Vsub IsNot Nothing Then
                    Dim _Vsub = Vsub.FirstOrDefault(Function(k) k.Extension = "vtt")
                    If _Vsub IsNot Nothing Then
                        If TypeOf Application.Current.MainWindow Is MainWindow Then
                            With CType(Application.Current.MainWindow, MainWindow)
                                Dim parser As New SubtitlesParser.Classes.Parsers.SubParser
                                Using httpc As New Net.Http.HttpClient()
                                    Dim data = Await httpc.GetStreamAsync(_Vsub.URL)
                                    Dim subs = parser.ParseStream(data)
                                    Application.Current.Dispatcher.Invoke(Sub()
                                                                              If .Subtitles Is Nothing Then .Subtitles = New ObjectModel.ObservableCollection(Of SubtitlesParser.Classes.SubtitleItem)
                                                                              For Each _sub In subs
                                                                                  .Subtitles?.Add(_sub)
                                                                                  Dim PosSyncProc As New Un4seen.Bass.SYNCPROC(Sub(h, c, _data, u)
                                                                                                                                   Application.Current.Dispatcher.Invoke(Sub()
                                                                                                                                                                             '.InfoText = String.Join(Environment.NewLine, _sub.PlaintextLines)
                                                                                                                                                                             .CurrentSubtitle = _sub
                                                                                                                                                                         End Sub)
                                                                                                                               End Sub)
                                                                                  Dim stream = TryCast(parameter, Player.Player)?.Stream
                                                                                  Dim PosSyncProcHandle = Un4seen.Bass.Bass.BASS_ChannelSetSync(stream, Un4seen.Bass.BASSSync.BASS_SYNC_POS, Un4seen.Bass.Bass.BASS_ChannelSeconds2Bytes(stream, TimeSpan.FromMilliseconds(_sub.StartTime).TotalSeconds), PosSyncProc, IntPtr.Zero)
                                                                                  ._SubtilesPositionSyncProcs.Add(New Tuple(Of Integer, Integer, Un4seen.Bass.SYNCPROC)(stream, PosSyncProcHandle, PosSyncProc))
                                                                              Next
                                                                              .IsUsingSubtitles = True
                                                                          End Sub)
                                End Using
                            End With
                        End If
                    End If
                Else
                    For Each cap In video.AutomaticCaptions.AsArray
                        If cap IsNot Nothing Then
                            Dim _Vsub = cap.FirstOrDefault(Function(k) k.Extension = "vtt")
                            If _Vsub IsNot Nothing Then
                                If TypeOf Application.Current.MainWindow Is MainWindow Then
                                    With CType(Application.Current.MainWindow, MainWindow)
                                        Dim parser As New SubtitlesParser.Classes.Parsers.SubParser
                                        Using httpc As New Net.Http.HttpClient()
                                            Dim data = Await httpc.GetStreamAsync(_Vsub.URL)
                                            Dim subs = parser.ParseStream(data)
                                            Application.Current.Dispatcher.Invoke(Sub()
                                                                                      If .Subtitles Is Nothing Then .Subtitles = New ObjectModel.ObservableCollection(Of SubtitlesParser.Classes.SubtitleItem)
                                                                                      For Each _sub In subs
                                                                                          .Subtitles?.Add(_sub)
                                                                                          Dim PosSyncProc As New Un4seen.Bass.SYNCPROC(Sub(h, c, _data, u)
                                                                                                                                           Application.Current.Dispatcher.Invoke(Sub()
                                                                                                                                                                                     '.InfoText = String.Join(Environment.NewLine, _sub.PlaintextLines)
                                                                                                                                                                                     .CurrentSubtitle = _sub
                                                                                                                                                                                 End Sub)
                                                                                                                                       End Sub)
                                                                                          Dim stream = TryCast(parameter, Player.Player)?.Stream
                                                                                          Dim PosSyncProcHandle = Un4seen.Bass.Bass.BASS_ChannelSetSync(stream, Un4seen.Bass.BASSSync.BASS_SYNC_POS, Un4seen.Bass.Bass.BASS_ChannelSeconds2Bytes(stream, TimeSpan.FromMilliseconds(_sub.StartTime).TotalSeconds), PosSyncProc, IntPtr.Zero)
                                                                                          ._SubtilesPositionSyncProcs.Add(New Tuple(Of Integer, Integer, Un4seen.Bass.SYNCPROC)(stream, PosSyncProcHandle, PosSyncProc))
                                                                                      Next
                                                                                      .IsUsingSubtitles = True
                                                                                  End Sub)
                                        End Using
                                    End With
                                End If
                            End If
                        End If
                    Next
                End If
            End Sub

            Public Function CanExecute(parameter As Object) As Boolean Implements ICommand.CanExecute
                Return SharedProperties.Instance.IsInternetConnected AndAlso Not IsNothing(_Client) AndAlso IO.File.Exists(_Client.YoutubeDLLocation) AndAlso TypeOf parameter Is Player.Player
            End Function
        End Class
        Public Class LoadPlaylistCommand
            Implements ICommand

            Public Event CanExecuteChanged As EventHandler Implements ICommand.CanExecuteChanged
            Private _Client As YoutubeDL

            Sub New(client As YoutubeDL)
                _Client = client
                AddHandler CommandManager.RequerySuggested, New EventHandler(Sub(sender As Object, e As EventArgs)
                                                                                 RaiseEvent CanExecuteChanged(Me, New EventArgs())
                                                                             End Sub)
            End Sub

            Public Async Sub Execute(parameter As Object) Implements ICommand.Execute
                If Not Utilities.SharedProperties.Instance.IsInternetConnected Then
                    HandyControl.Controls.MessageBox.Error(Utilities.ResourceResolver.Strings.QUERY_INTERNET_DISCONNECTED)
                    Return
                End If
                Dim dialog As New Dialogs.InputBox()
                dialog.AddTextBox(Utilities.ResourceResolver.Strings.COUNT, DefaultInput:="-1")
                dialog.AddTextBox("URL")
                If dialog.ShowDialog Then
                    Dim url = dialog.Value("URL")
                    Dim Count As Integer = -1
                    Integer.TryParse(dialog.Value(Utilities.ResourceResolver.Strings.COUNT), Count)
                    Dim playlist = Await _Client.DumpAndManagePlaylist(url, Count)
                    If playlist Is Nothing Then Return
                    Dim pPlaylist As New Player.Playlist() With {.Category = "Youtube", .Description = playlist.Description, .Name = If(Not String.IsNullOrEmpty(playlist.Uploader), playlist.Uploader & " | ", "") & playlist.Title}
                    Dim pThumbURL = playlist.Thumbnails.FirstOrDefault(Function(k) Not String.IsNullOrEmpty(k.URL))?.URL
                    If Not String.IsNullOrEmpty(pThumbURL) Then
                        Dim pThumbURI = New Uri(pThumbURL, UriKind.Absolute)
                        pPlaylist.Cover = pThumbURI.ToBitmapSource
                    End If
                    For Each video In playlist.Entries
                        If video Is Nothing Then Continue For
                        Dim meta = video.AsMetadata(_Client.VideoQuality)
                        pPlaylist.Add(meta)
                    Next
#Disable Warning
                    TryCast(parameter, Player.Player)?.LoadPlaylist(pPlaylist)
#Enable Warning
                End If
            End Sub

            Public Function CanExecute(parameter As Object) As Boolean Implements ICommand.CanExecute
                Return SharedProperties.Instance.IsInternetConnected AndAlso Not IsNothing(_Client) AndAlso IO.File.Exists(_Client.YoutubeDLLocation) AndAlso TypeOf parameter Is Player.Player
            End Function
        End Class
        Public Class SetPathCommand
            Implements ICommand

            Public Event CanExecuteChanged As EventHandler Implements ICommand.CanExecuteChanged
            Private _Client As YoutubeDL

            Sub New(client As YoutubeDL)
                _Client = client
                AddHandler CommandManager.RequerySuggested, New EventHandler(Sub(sender As Object, e As EventArgs)
                                                                                 RaiseEvent CanExecuteChanged(Me, New EventArgs())
                                                                             End Sub)
            End Sub

            Public Sub Execute(parameter As Object) Implements ICommand.Execute
                Dim ofd As New Microsoft.Win32.OpenFileDialog With {.Title = "Youtube-DL or equivalent path", .Filter = "Executables|*.exe|All Files|*.*"}
                If Not ofd.ShowDialog Then
                    Return
                End If
                _Client.YoutubeDLLocation = ofd.FileName
                My.Settings.APP_YOUTUBEDL_PATH = ofd.FileName
                My.Settings.Save()
            End Sub

            Public Function CanExecute(parameter As Object) As Boolean Implements ICommand.CanExecute
                Return Not IsNothing(_Client)
            End Function
        End Class
    End Namespace
    Namespace Classes
        Public Class YoutubeVideoEX
            Property AlternateTitle As String
            Property AudioCodec As String
            Property AudioOnly As New List(Of Video)
            Property Author As String
            Property AuthorID As String
            Property AuthorURL As String
            Property Categories As String()
            Property Chapters As List(Of Chapter)
            Property Creator As String
            Property Description As String
            Property DirectURL As String
            Property DislikeCount As Integer
            Property Duration As TimeSpan
            Property FileName As String
            Property FPS As Integer
            Property FullTitle As String
            Property HQAudio As Video
            Property HQMixed As Video
            Property HQThumbnail As Thumbnail
            Property HQVideo As Video
            Property JSONDump As String
            Property LikeCount As Integer
            Property LQAudio As Video
            Property LQMixed As Video
            Property LQThumbnail As Thumbnail
            Property LQVideo As Video
            Property MixedOnly As New List(Of Video)
            Property Playlist As YoutubePlaylist
            Property Properties As MediaProperties
            Property RequestQuality As VideoQuality
            Property Subtitles As New List(Of Tuple(Of String, List(Of Subtitle)))
            Property Tags As New List(Of String)
            Property Thumbnails As New List(Of Thumbnail)
            Property ThumbnailURL As String
            Property Title As String
            Property UploadDate As Date
            Property URL As String
            Property VideoCodec As String
            Property VideoExtension As String
            Property VideoOnly As New List(Of Video)
            Property Videos As New List(Of Video)
            Property ViewCount As ULong
            Public Class Subtitle
                Public Property Language As String
                Public Property RawData As String
                Public Property Extension As String
            End Class
            Public Class Video
                Public Property AudioBitrate As String
                Public Property AudioSampleRate As String
                Public Property AudioCodec As String
                Public Property Container As String
                Public Property DirectURL As String
                Public Property Extension As String
                Public Property FileSize As String
                Public Property Format As String
                Public Property FormatID As String
                Public Property FormatNote As String
                Public Property FPS As String
                Public Property Height As String
                Public Property HTTPHeaders As HTTPHeader
                Public Property Protocol As String
                Public Property Quality As String
                Public Property TotalBitrate As String
                Public Property Type As FileType
                Public Property VideoBitrate As String
                Public Property VideoCodec As String
                Public Property Width As String
                Public Enum FileType
                    Audio
                    Video
                    Mixed
                End Enum
                Public Class HTTPHeader
                    Public Property AcceptLanguage As String
                    Public Property AcceptCharset As String
                    Public Property UserAgent As String
                    Public Property AcceptEncoding As String
                    Public Property Accept As String
                End Class
            End Class
            Public Class YoutubePlaylist
                Property Index As Integer
                Property Playlist_ID As String
                Property Title As String
                Property Uploader As String
                Property Uploader_ID As String
            End Class
            Public Class MediaProperties
                Property Album As String
                Property AlbumArtist As String
                Property AlbumType As String
                Property Artists As String()
                Property DiscNumber As Integer
                Property Genres As String()
                Property ReleaseYear As Integer
                Property Track As String
                Property Track_ID As String
                Property TrackNumber As Integer
            End Class
            Public Class Chapter
                Property EndTime As TimeSpan
                Property StarTime As TimeSpan
                Property Title As String
            End Class
            Public Shadows Function ToString() As String
                Return Title & Environment.NewLine & Author & Environment.NewLine & URL & Environment.NewLine & DirectURL & Environment.NewLine & RequestQuality.ToString
            End Function
        End Class
        Public Class YoutubePlaylistEX
            Inherits ObjectModel.ObservableCollection(Of YoutubeVideoEX)

            Private Sub _PropertyChanged(<Runtime.CompilerServices.CallerMemberName> Optional CallerName As String = Nothing)
                OnPropertyChanged(New PropertyChangedEventArgs(CallerName))
            End Sub

            Private _ID As String
            Property ID As String
                Get
                    Return _ID
                End Get
                Set(value As String)
                    _ID = value
                    _PropertyChanged()
                End Set
            End Property

            Private _Title As String
            Property Title As String
                Get
                    Return _Title
                End Get
                Set(value As String)
                    _Title = value
                    _PropertyChanged()
                End Set
            End Property

            Private _Description As String
            Property Description As String
                Get
                    Return _Description
                End Get
                Set(value As String)
                    _Description = value
                    _PropertyChanged()
                End Set
            End Property

            Private _Thumbnails As New List(Of Thumbnail)
            Property Thumbnails As List(Of Thumbnail)
                Get
                    Return _Thumbnails
                End Get
                Set(value As List(Of Thumbnail))
                    _Thumbnails = value
                    _PropertyChanged()
                End Set
            End Property

            Private _ModifiedDate As Date
            Property ModifiedDate As Date
                Get
                    Return _ModifiedDate
                End Get
                Set(value As Date)
                    _ModifiedDate = value
                    _PropertyChanged()
                End Set
            End Property

            Private _ViewCount As Integer
            Property ViewCount As Integer
                Get
                    Return _ViewCount
                End Get
                Set(value As Integer)
                    _ViewCount = value
                    _PropertyChanged()
                End Set
            End Property

            Private _ExpectedCount As Integer
            ''' <summary>
            ''' Retrieved video count, Should match <see cref="Count"/>
            ''' </summary>
            ''' <returns></returns>
            Property ExpectedCount As Integer
                Get
                    Return _ExpectedCount
                End Get
                Set(value As Integer)
                    _ExpectedCount = value
                    _PropertyChanged()
                End Set
            End Property

            Private _Channel As String
            Property Channel As String
                Get
                    Return _Channel
                End Get
                Set(value As String)
                    _Channel = value
                    _PropertyChanged()
                End Set
            End Property

            Private _ChannelID As String
            Property ChannelID As String
                Get
                    Return _Channel
                End Get
                Set(value As String)
                    _Channel = value
                    _PropertyChanged()
                End Set
            End Property

            Private _Uploader As String
            Property Uploader As String
                Get
                    Return _Uploader
                End Get
                Set(value As String)
                    _Uploader = value
                    _PropertyChanged()
                End Set
            End Property

            Private _UploaderID As String
            Property UploaderID As String
                Get
                    Return _UploaderID
                End Get
                Set(value As String)
                    _UploaderID = value
                    _PropertyChanged()
                End Set
            End Property

            Private _ChannelURL As String
            Property ChannelURL As String
                Get
                    Return _ChannelURL
                End Get
                Set(value As String)
                    _ChannelURL = value
                    _PropertyChanged()
                End Set
            End Property

            Private _UploaderURL As String
            Property UploaderURL As String
                Get
                    Return _UploaderURL
                End Get
                Set(value As String)
                    _UploaderURL = value
                    _PropertyChanged()
                End Set
            End Property

        End Class
        Public Class Resolution
            Public Property Height As Integer
            Public Property Width As Integer
            Public Sub New(H As Integer, W As Integer)
                Height = H
                Width = W
            End Sub
        End Class

        Public Enum VideoQuality
            best
            worst
            bestvideo
            worstvideo
            bestaudio
            worstaudio
        End Enum
#Region "JSON"
        ' Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);      

        Public Class DownloaderOptions
            <JsonProperty("http_chunk_size")>
            Public Property http_chunk_size() As Integer?
        End Class

        Public Class Format
            <JsonProperty("format_id")>
            Public Property format_id() As String

            <JsonProperty("format_note")>
            Public Property format_note() As String

            <JsonProperty("ext")>
            Public Property Extension() As String

            <JsonProperty("protocol")>
            Public Property Protocol() As String

            <JsonProperty("acodec")>
            Public Property AudioCodec() As String

            <JsonProperty("vcodec")>
            Public Property VideoCodec() As String

            <JsonProperty("url")>
            Public Property URL() As String

            <JsonProperty("width")>
            Public Property Width() As Integer?

            <JsonProperty("height")>
            Public Property Height() As Integer?

            <JsonProperty("fps")>
            Public Property FPS() As Double?

            <JsonProperty("rows")>
            Public Property Rows() As Integer?

            <JsonProperty("columns")>
            Public Property Columns() As Integer?

            <JsonProperty("fragments")>
            Public Property Fragments() As List(Of Fragment)

            <JsonProperty("resolution")>
            Public Property Resolution() As String

            <JsonProperty("aspect_ratio")>
            Public Property AspectRatio() As Double?

            <JsonProperty("http_headers")>
            Public Property HTTPHeaders() As HttpHeaders

            <JsonProperty("audio_ext")>
            Public Property AudioExtension() As String

            <JsonProperty("video_ext")>
            Public Property VideoExtension() As String

            <JsonProperty("vbr")>
            Public Property VideoBitrate() As Double?

            <JsonProperty("abr")>
            Public Property AudioBitrate() As Double?

            <JsonProperty("tbr")>
            Public Property TotalBitrate() As Double?

            <JsonProperty("format")>
            Public Property Format() As String

            <JsonProperty("format_index")>
            Public Property FormatIndex() As Object

            <JsonProperty("manifest_url")>
            Public Property ManifestURL() As String

            <JsonProperty("language")>
            Public Property Language() As Object

            <JsonProperty("preference")>
            Public Property Preference() As Object

            <JsonProperty("quality")>
            Public Property Quality() As Double?

            <JsonProperty("has_drm")>
            Public Property HasDRM() As Boolean?

            <JsonProperty("source_preference")>
            Public Property SourcePreference() As Integer?

            <JsonProperty("asr")>
            Public Property AudioSampleRate() As Integer?

            <JsonProperty("filesize")>
            Public Property FileSize() As Long?

            <JsonProperty("audio_channels")>
            Public Property AudioChannels() As Integer?

            <JsonProperty("language_preference")>
            Public Property LanguagePreference() As Integer?

            <JsonProperty("dynamic_range")>
            Public Property DynamicRange() As String

            <JsonProperty("container")>
            Public Property Container() As String

            <JsonProperty("downloader_options")>
            Public Property downloader_options() As DownloaderOptions

            <JsonProperty("filesize_approx")>
            Public Property FilesizeApprox() As Integer?
        End Class

        Public Class Fragment
            <JsonProperty("url")>
            Public Property URL() As String

            <JsonProperty("duration")>
            Public Property Duration() As Double?
        End Class

        Public Class HttpHeaders
            <JsonProperty("User-Agent")>
            Public Property UserAgent() As String

            <JsonProperty("Accept")>
            Public Property Accept() As String

            <JsonProperty("Accept-Language")>
            Public Property AcceptLanguage() As String

            <JsonProperty("Sec-Fetch-Mode")>
            Public Property SecFetchMode() As String
        End Class

        Public Class RequestedFormat
            <JsonProperty("asr")>
            Public Property AudioSampleRate() As Integer?

            <JsonProperty("filesize")>
            Public Property FileSize() As Integer?

            <JsonProperty("format_id")>
            Public Property FormatID() As String

            <JsonProperty("format_note")>
            Public Property FormatNote() As String

            <JsonProperty("source_preference")>
            Public Property SourcePreference() As Integer?

            <JsonProperty("fps")>
            Public Property FPS() As Double?

            <JsonProperty("audio_channels")>
            Public Property AudioChannels() As Integer?

            <JsonProperty("height")>
            Public Property Height() As Integer?

            <JsonProperty("quality")>
            Public Property Quality() As Double?

            <JsonProperty("has_drm")>
            Public Property HasDRM() As Boolean?

            <JsonProperty("tbr")>
            Public Property TotalBitrate() As Double?

            <JsonProperty("url")>
            Public Property URL() As String

            <JsonProperty("width")>
            Public Property Width() As Integer?

            <JsonProperty("language")>
            Public Property Language() As Object

            <JsonProperty("language_preference")>
            Public Property LanguagePreference() As Integer?

            <JsonProperty("preference")>
            Public Property Preference() As Object

            <JsonProperty("ext")>
            Public Property Extension() As String

            <JsonProperty("vcodec")>
            Public Property VideoCodec() As String

            <JsonProperty("acodec")>
            Public Property AudioCodec() As String

            <JsonProperty("dynamic_range")>
            Public Property DynamicRange() As String

            <JsonProperty("container")>
            Public Property Container() As String

            <JsonProperty("downloader_options")>
            Public Property downloader_options() As DownloaderOptions

            <JsonProperty("protocol")>
            Public Property Protocol() As String

            <JsonProperty("resolution")>
            Public Property Resolution() As String

            <JsonProperty("aspect_ratio")>
            Public Property AspectRatio() As Double?

            <JsonProperty("http_headers")>
            Public Property HTTPHeaders() As HttpHeaders

            <JsonProperty("video_ext")>
            Public Property VideoExtension() As String

            <JsonProperty("audio_ext")>
            Public Property AudioExtension() As String

            <JsonProperty("abr")>
            Public Property AudioBitrate() As Double?

            <JsonProperty("vbr")>
            Public Property VideoBitrate() As Double?

            <JsonProperty("format")>
            Public Property Format() As String
        End Class

        Public Class Subtitles
            <JsonProperty("ext")>
            Public Property Extension() As String

            <JsonProperty("url")>
            Public Property URL() As String

            <JsonProperty("name")>
            Public Property Name() As String
        End Class

        Public Class AutomaticCaptions

#Region "Languages"
            <JsonProperty("en")>
            Public Property en() As List(Of Subtitles)

            <JsonProperty("fr")>
            Public Property fr() As List(Of Subtitles)

            <JsonProperty("es")>
            Public Property es() As List(Of Subtitles)

            <JsonProperty("af")>
            Public Property af() As List(Of Subtitles)

            <JsonProperty("ak")>
            Public Property ak() As List(Of Subtitles)

            <JsonProperty("sq")>
            Public Property sq() As List(Of Subtitles)

            <JsonProperty("am")>
            Public Property am() As List(Of Subtitles)

            <JsonProperty("ar")>
            Public Property ar() As List(Of Subtitles)

            <JsonProperty("hy")>
            Public Property hy() As List(Of Subtitles)

            <JsonProperty("as")>
            Public Property [as]() As List(Of Subtitles)

            <JsonProperty("ay")>
            Public Property ay() As List(Of Subtitles)

            <JsonProperty("az")>
            Public Property az() As List(Of Subtitles)

            <JsonProperty("bn")>
            Public Property bn() As List(Of Subtitles)

            <JsonProperty("eu")>
            Public Property eu() As List(Of Subtitles)

            <JsonProperty("be")>
            Public Property be() As List(Of Subtitles)

            <JsonProperty("bho")>
            Public Property bho() As List(Of Subtitles)

            <JsonProperty("bs")>
            Public Property bs() As List(Of Subtitles)

            <JsonProperty("bg")>
            Public Property bg() As List(Of Subtitles)

            <JsonProperty("my")>
            Public Property my() As List(Of Subtitles)

            <JsonProperty("ca")>
            Public Property ca() As List(Of Subtitles)

            <JsonProperty("ceb")>
            Public Property ceb() As List(Of Subtitles)

            <JsonProperty("zh-Hans")>
            Public Property zhHans() As List(Of Subtitles)

            <JsonProperty("zh-Hant")>
            Public Property zhHant() As List(Of Subtitles)

            <JsonProperty("co")>
            Public Property co() As List(Of Subtitles)

            <JsonProperty("hr")>
            Public Property hr() As List(Of Subtitles)

            <JsonProperty("cs")>
            Public Property cs() As List(Of Subtitles)

            <JsonProperty("da")>
            Public Property da() As List(Of Subtitles)

            <JsonProperty("dv")>
            Public Property dv() As List(Of Subtitles)

            <JsonProperty("nl")>
            Public Property nl() As List(Of Subtitles)

            <JsonProperty("en-orig")>
            Public Property enorig() As List(Of Subtitles)

            <JsonProperty("eo")>
            Public Property eo() As List(Of Subtitles)

            <JsonProperty("et")>
            Public Property et() As List(Of Subtitles)

            <JsonProperty("ee")>
            Public Property ee() As List(Of Subtitles)

            <JsonProperty("fil")>
            Public Property fil() As List(Of Subtitles)

            <JsonProperty("fi")>
            Public Property fi() As List(Of Subtitles)

            <JsonProperty("gl")>
            Public Property gl() As List(Of Subtitles)

            <JsonProperty("lg")>
            Public Property lg() As List(Of Subtitles)

            <JsonProperty("ka")>
            Public Property ka() As List(Of Subtitles)

            <JsonProperty("de")>
            Public Property de() As List(Of Subtitles)

            <JsonProperty("el")>
            Public Property el() As List(Of Subtitles)

            <JsonProperty("gn")>
            Public Property gn() As List(Of Subtitles)

            <JsonProperty("gu")>
            Public Property gu() As List(Of Subtitles)

            <JsonProperty("ht")>
            Public Property ht() As List(Of Subtitles)

            <JsonProperty("ha")>
            Public Property ha() As List(Of Subtitles)

            <JsonProperty("haw")>
            Public Property haw() As List(Of Subtitles)

            <JsonProperty("iw")>
            Public Property iw() As List(Of Subtitles)

            <JsonProperty("hi")>
            Public Property hi() As List(Of Subtitles)

            <JsonProperty("hmn")>
            Public Property hmn() As List(Of Subtitles)

            <JsonProperty("hu")>
            Public Property hu() As List(Of Subtitles)

            <JsonProperty("is")>
            Public Property [is]() As List(Of Subtitles)

            <JsonProperty("ig")>
            Public Property ig() As List(Of Subtitles)

            <JsonProperty("id")>
            Public Property id() As List(Of Subtitles)

            <JsonProperty("ga")>
            Public Property ga() As List(Of Subtitles)

            <JsonProperty("it")>
            Public Property it() As List(Of Subtitles)

            <JsonProperty("ja")>
            Public Property ja() As List(Of Subtitles)

            <JsonProperty("jv")>
            Public Property jv() As List(Of Subtitles)

            <JsonProperty("kn")>
            Public Property kn() As List(Of Subtitles)

            <JsonProperty("kk")>
            Public Property kk() As List(Of Subtitles)

            <JsonProperty("km")>
            Public Property km() As List(Of Subtitles)

            <JsonProperty("rw")>
            Public Property rw() As List(Of Subtitles)

            <JsonProperty("ko")>
            Public Property ko() As List(Of Subtitles)

            <JsonProperty("kri")>
            Public Property kri() As List(Of Subtitles)

            <JsonProperty("ku")>
            Public Property ku() As List(Of Subtitles)

            <JsonProperty("ky")>
            Public Property ky() As List(Of Subtitles)

            <JsonProperty("lo")>
            Public Property lo() As List(Of Subtitles)

            <JsonProperty("la")>
            Public Property la() As List(Of Subtitles)

            <JsonProperty("lv")>
            Public Property lv() As List(Of Subtitles)

            <JsonProperty("ln")>
            Public Property ln() As List(Of Subtitles)

            <JsonProperty("lt")>
            Public Property lt() As List(Of Subtitles)

            <JsonProperty("lb")>
            Public Property lb() As List(Of Subtitles)

            <JsonProperty("mk")>
            Public Property mk() As List(Of Subtitles)

            <JsonProperty("mg")>
            Public Property mg() As List(Of Subtitles)

            <JsonProperty("ms")>
            Public Property ms() As List(Of Subtitles)

            <JsonProperty("ml")>
            Public Property ml() As List(Of Subtitles)

            <JsonProperty("mt")>
            Public Property mt() As List(Of Subtitles)

            <JsonProperty("mi")>
            Public Property mi() As List(Of Subtitles)

            <JsonProperty("mr")>
            Public Property mr() As List(Of Subtitles)

            <JsonProperty("mn")>
            Public Property mn() As List(Of Subtitles)

            <JsonProperty("ne")>
            Public Property ne() As List(Of Subtitles)

            <JsonProperty("nso")>
            Public Property nso() As List(Of Subtitles)

            <JsonProperty("no")>
            Public Property no() As List(Of Subtitles)

            <JsonProperty("ny")>
            Public Property ny() As List(Of Subtitles)

            <JsonProperty("or")>
            Public Property [or]() As List(Of Subtitles)

            <JsonProperty("om")>
            Public Property om() As List(Of Subtitles)

            <JsonProperty("ps")>
            Public Property ps() As List(Of Subtitles)

            <JsonProperty("fa")>
            Public Property fa() As List(Of Subtitles)

            <JsonProperty("pl")>
            Public Property pl() As List(Of Subtitles)

            <JsonProperty("pt")>
            Public Property pt() As List(Of Subtitles)

            <JsonProperty("pa")>
            Public Property pa() As List(Of Subtitles)

            <JsonProperty("qu")>
            Public Property qu() As List(Of Subtitles)

            <JsonProperty("ro")>
            Public Property ro() As List(Of Subtitles)

            <JsonProperty("ru")>
            Public Property ru() As List(Of Subtitles)

            <JsonProperty("sm")>
            Public Property sm() As List(Of Subtitles)

            <JsonProperty("sa")>
            Public Property sa() As List(Of Subtitles)

            <JsonProperty("gd")>
            Public Property gd() As List(Of Subtitles)

            <JsonProperty("sr")>
            Public Property sr() As List(Of Subtitles)

            <JsonProperty("sn")>
            Public Property sn() As List(Of Subtitles)

            <JsonProperty("sd")>
            Public Property sd() As List(Of Subtitles)

            <JsonProperty("si")>
            Public Property si() As List(Of Subtitles)

            <JsonProperty("sk")>
            Public Property sk() As List(Of Subtitles)

            <JsonProperty("sl")>
            Public Property sl() As List(Of Subtitles)

            <JsonProperty("so")>
            Public Property so() As List(Of Subtitles)

            <JsonProperty("st")>
            Public Property st() As List(Of Subtitles)

            <JsonProperty("su")>
            Public Property su() As List(Of Subtitles)

            <JsonProperty("sw")>
            Public Property sw() As List(Of Subtitles)

            <JsonProperty("sv")>
            Public Property sv() As List(Of Subtitles)

            <JsonProperty("tg")>
            Public Property tg() As List(Of Subtitles)

            <JsonProperty("ta")>
            Public Property ta() As List(Of Subtitles)

            <JsonProperty("tt")>
            Public Property tt() As List(Of Subtitles)

            <JsonProperty("te")>
            Public Property te() As List(Of Subtitles)

            <JsonProperty("th")>
            Public Property th() As List(Of Subtitles)

            <JsonProperty("ti")>
            Public Property ti() As List(Of Subtitles)

            <JsonProperty("ts")>
            Public Property ts() As List(Of Subtitles)

            <JsonProperty("tr")>
            Public Property tr() As List(Of Subtitles)

            <JsonProperty("tk")>
            Public Property tk() As List(Of Subtitles)

            <JsonProperty("uk")>
            Public Property uk() As List(Of Subtitles)

            <JsonProperty("ur")>
            Public Property ur() As List(Of Subtitles)

            <JsonProperty("ug")>
            Public Property ug() As List(Of Subtitles)

            <JsonProperty("uz")>
            Public Property uz() As List(Of Subtitles)

            <JsonProperty("vi")>
            Public Property vi() As List(Of Subtitles)

            <JsonProperty("cy")>
            Public Property cy() As List(Of Subtitles)

            <JsonProperty("fy")>
            Public Property fy() As List(Of Subtitles)

            <JsonProperty("xh")>
            Public Property xh() As List(Of Subtitles)

            <JsonProperty("yi")>
            Public Property yi() As List(Of Subtitles)

            <JsonProperty("yo")>
            Public Property yo() As List(Of Subtitles)

            <JsonProperty("zu")>
            Public Property zu() As List(Of Subtitles)

#End Region

            Public Function AsArray() As List(Of Subtitles)()
                With Me
                    Return New List(Of Subtitles)() { .af, .ak, .am, .ar, .as, .ay, .az, .be, .bg, .bho, .bn, .bs,
                       .ca, .ceb, .co, .cs, .cy, .da, .de, .dv, .ee, .el, .en, .enorig, .eo, .es, .et, .eu,
                       .fa, .fi, .fil, .fr, .fy, .ga, .gd, .gl, .gn, .gu, .ha, .haw, .hi, .hmn, .hr, .ht, .hu, .hy,
                       .id, .ig, .is, .it, .iw, .ja, .jv, .ka, .kk, .km, .kn, .ko, .kri, .ku, .ky, .la, .lb, .lg, .ln, .lo,
                       .lt, .lv, .mg, .mi, .mk, .ml, .mn, .mr, .ms, .mt, .my, .ne, .nl, .no, .nso, .ny,
                       .om, .or, .pa, .pl, .ps, .pt, .qu, .ro, .ru, .rw, .sa, .sd, .si, .sk, .sl, .sm, .sn, .so, .sq, .sr, .st, .su, .sv, .sw,
                       .ta, .te, .tg, .th, .ti, .tk, .tr, .ts, .tt, .ug, .uk, .ur, .uz, .vi, .xh, .yi, .yo, .zhHans, .zhHant, .zu}
                End With
            End Function

            Public Function GetEnumerator() As IEnumerator(Of List(Of Subtitles))
                With Me
                    Return New List(Of Subtitles)() { .af, .ak, .am, .ar, .as, .ay, .az, .be, .bg, .bho, .bn, .bs,
                        .ca, .ceb, .co, .cs, .cy, .da, .de, .dv, .ee, .el, .en, .enorig, .eo, .es, .et, .eu,
                        .fa, .fi, .fil, .fr, .fy, .ga, .gd, .gl, .gn, .gu, .ha, .haw, .hi, .hmn, .hr, .ht, .hu, .hy,
                        .id, .ig, .is, .it, .iw, .ja, .jv, .ka, .kk, .km, .kn, .ko, .kri, .ku, .ky, .la, .lb, .lg, .ln, .lo,
                        .lt, .lv, .mg, .mi, .mk, .ml, .mn, .mr, .ms, .mt, .my, .ne, .nl, .no, .nso, .ny,
                        .om, .or, .pa, .pl, .ps, .pt, .qu, .ro, .ru, .rw, .sa, .sd, .si, .sk, .sl, .sm, .sn, .so, .sq, .sr, .st, .su, .sv, .sw,
                        .ta, .te, .tg, .th, .ti, .tk, .tr, .ts, .tt, .ug, .uk, .ur, .uz, .vi, .xh, .yi, .yo, .zhHans, .zhHant, .zu}.GetEnumerator
                End With
            End Function

            Private Function IEnumerable_GetEnumerator() As IEnumerator
                With Me
                    Return New List(Of Subtitles)() { .af, .ak, .am, .ar, .as, .ay, .az, .be, .bg, .bho, .bn, .bs,
                        .ca, .ceb, .co, .cs, .cy, .da, .de, .dv, .ee, .el, .en, .enorig, .eo, .es, .et, .eu,
                        .fa, .fi, .fil, .fr, .fy, .ga, .gd, .gl, .gn, .gu, .ha, .haw, .hi, .hmn, .hr, .ht, .hu, .hy,
                        .id, .ig, .is, .it, .iw, .ja, .jv, .ka, .kk, .km, .kn, .ko, .kri, .ku, .ky, .la, .lb, .lg, .ln, .lo,
                        .lt, .lv, .mg, .mi, .mk, .ml, .mn, .mr, .ms, .mt, .my, .ne, .nl, .no, .nso, .ny,
                        .om, .or, .pa, .pl, .ps, .pt, .qu, .ro, .ru, .rw, .sa, .sd, .si, .sk, .sl, .sm, .sn, .so, .sq, .sr, .st, .su, .sv, .sw,
                        .ta, .te, .tg, .th, .ti, .tk, .tr, .ts, .tt, .ug, .uk, .ur, .uz, .vi, .xh, .yi, .yo, .zhHans, .zhHant, .zu}.GetEnumerator
                End With
            End Function
        End Class

        Public Class Thumbnail
            <JsonProperty("url")>
            Public Property URL() As String

            <JsonProperty("height")>
            Public Property Height() As Integer?

            <JsonProperty("width")>
            Public Property Width() As Integer?

            <JsonProperty("preference")>
            Public Property Preference() As Integer?

            <JsonProperty("id")>
            Public Property ID() As String

            <JsonProperty("resolution")>
            Public Property Resolution() As String
        End Class

        Public Class Version
            <JsonProperty("version")>
            Public Property Version() As String

            <JsonProperty("current_git_head")>
            Public Property CurrentGitHead() As Object

            <JsonProperty("release_git_head")>
            Public Property ReleaseGitHead() As String

            <JsonProperty("repository")>
            Public Property Repository() As String
        End Class

        Public Class YoutubeVideo
            <JsonProperty("id")>
            Public Property ID() As String

            <JsonProperty("title")>
            Public Property Title() As String

            <JsonProperty("formats")>
            Public Property Formats() As List(Of Format)

            <JsonProperty("thumbnails")>
            Public Property Thumbnails() As List(Of Thumbnail)

            <JsonProperty("thumbnail")>
            Public Property Thumbnail() As String

            <JsonProperty("description")>
            Public Property Description() As String

            <JsonProperty("channel_id")>
            Public Property ChannelID() As String

            <JsonProperty("channel_url")>
            Public Property ChannelURL() As String

            <JsonProperty("duration")>
            Public Property Duration() As Integer?

            <JsonProperty("view_count")>
            Public Property ViewCount() As Integer?

            <JsonProperty("average_rating")>
            Public Property AverageRating() As Object

            <JsonProperty("age_limit")>
            Public Property AgeLimit() As Integer?

            <JsonProperty("webpage_url")>
            Public Property WebpageURL() As String

            <JsonProperty("categories")>
            Public Property Categories() As List(Of String)

            <JsonProperty("tags")>
            Public Property Tags() As List(Of String)

            <JsonProperty("playable_in_embed")>
            Public Property PlayableInEmbed() As Boolean?

            <JsonProperty("live_status")>
            Public Property LiveStatus() As String

            <JsonProperty("release_timestamp")>
            Public Property ReleaseTimestamp() As Object

            <JsonProperty("_format_sort_fields")>
            Public Property FormatSortFields() As List(Of String)

            <JsonProperty("automatic_captions")>
            Public Property AutomaticCaptions() As AutomaticCaptions

            <JsonProperty("subtitles")>
            Public Property Subtitles() As AutomaticCaptions

            <JsonProperty("album")>
            Public Property Album() As String

            <JsonProperty("artist")>
            Public Property Artist() As String

            <JsonProperty("track")>
            Public Property Track() As String

            <JsonProperty("release_date")>
            Public Property ReleaseDate() As Object

            <JsonProperty("release_year")>
            Public Property ReleaseYear() As Object

            <JsonProperty("comment_count")>
            Public Property CommentCount() As Integer?

            <JsonProperty("chapters")>
            Public Property Chapters() As Object

            <JsonProperty("heatmap")>
            Public Property Heatmap() As Object

            <JsonProperty("channel")>
            Public Property Channel() As String

            <JsonProperty("channel_follower_count")>
            Public Property ChannelFollowerCount() As Integer?

            <JsonProperty("channel_is_verified")>
            Public Property ChannelIsVerified() As Boolean?

            <JsonProperty("uploader")>
            Public Property Uploader() As String

            <JsonProperty("uploader_id")>
            Public Property UploaderID() As Object

            <JsonProperty("uploader_url")>
            Public Property UploaderURL() As Object

            <JsonProperty("upload_date")>
            Public Property UploadDate() As String

            <JsonProperty("creator")>
            Public Property Creator() As String

            <JsonProperty("alt_title")>
            Public Property AlternateTitle() As String

            <JsonProperty("availability")>
            Public Property Availability() As String

            <JsonProperty("original_url")>
            Public Property OriginalURL() As String

            <JsonProperty("webpage_url_basename")>
            Public Property webpage_url_basename() As String

            <JsonProperty("webpage_url_domain")>
            Public Property webpage_url_domain() As String

            <JsonProperty("extractor")>
            Public Property extractor() As String

            <JsonProperty("extractor_key")>
            Public Property extractor_key() As String

            <JsonProperty("playlist_count")>
            Public Property PlaylistCount() As Integer?

            <JsonProperty("playlist")>
            Public Property Playlist() As String

            <JsonProperty("playlist_id")>
            Public Property PlaylistID() As String

            <JsonProperty("playlist_title")>
            Public Property PlaylistTitle() As String

            <JsonProperty("playlist_uploader")>
            Public Property PlaylistUploader() As String

            <JsonProperty("playlist_uploader_id")>
            Public Property PlaylistUploaderID() As String

            <JsonProperty("n_entries")>
            Public Property nEntries() As Integer?

            <JsonProperty("playlist_index")>
            Public Property PlaylistIndex() As Integer?

            <JsonProperty("__last_playlist_index")>
            Public Property lastPlaylistIndex() As Integer?

            <JsonProperty("playlist_autonumber")>
            Public Property PlaylistAutonumber() As Integer?

            <JsonProperty("display_id")>
            Public Property DisplayID() As String

            <JsonProperty("fulltitle")>
            Public Property FullTitle() As String

            <JsonProperty("duration_string")>
            Public Property DurationString() As String

            <JsonProperty("is_live")>
            Public Property IsLive() As Boolean?

            <JsonProperty("was_live")>
            Public Property WasLive() As Boolean?

            <JsonProperty("requested_subtitles")>
            Public Property RequestedSubtitles() As Object

            <JsonProperty("_has_drm")>
            Public Property HasDrm() As Object

            <JsonProperty("epoch")>
            Public Property epoch() As Integer?

            <JsonProperty("requested_formats")>
            Public Property RequestedFormats() As List(Of RequestedFormat)

            <JsonProperty("format")>
            Public Property Format() As String

            <JsonProperty("format_id")>
            Public Property FormatID() As String

            <JsonProperty("ext")>
            Public Property Extension() As String

            <JsonProperty("protocol")>
            Public Property Protocol() As String

            <JsonProperty("language")>
            Public Property Language() As Object

            <JsonProperty("format_note")>
            Public Property FormatNote() As String

            <JsonProperty("filesize_approx")>
            Public Property FilesizeApprox() As Long?

            <JsonProperty("tbr")>
            Public Property TotalBitrate() As Double?

            <JsonProperty("url")>
            Public Property URL As String

            <JsonProperty("width")>
            Public Property Width() As Integer?

            <JsonProperty("height")>
            Public Property Height() As Integer?

            <JsonProperty("resolution")>
            Public Property Resolution() As String

            <JsonProperty("fps")>
            Public Property FPS() As Double?

            <JsonProperty("dynamic_range")>
            Public Property DynamicRange() As String

            <JsonProperty("vcodec")>
            Public Property VideoCodec() As String

            <JsonProperty("vbr")>
            Public Property VideoBitrate() As Double?

            <JsonProperty("stretched_ratio")>
            Public Property StretchedRatio() As Object

            <JsonProperty("aspect_ratio")>
            Public Property AspectRatio() As Double?

            <JsonProperty("acodec")>
            Public Property AudioCodec() As String

            <JsonProperty("abr")>
            Public Property AudioBitrate() As Double?

            <JsonProperty("asr")>
            Public Property AudioSamplerate() As Integer?

            <JsonProperty("audio_channels")>
            Public Property AudioChannels() As Integer?

            <JsonProperty("_filename")>
            Public Property Filename0() As String

            <JsonProperty("filename")>
            Public Property Filename() As String

            <JsonProperty("_type")>
            Public Property Type() As String

            <JsonProperty("_version")>
            Public Property Version() As Version

            Public Function AsMetadata(PreferedQuality As VideoQuality) As Player.Metadata
                Dim meta As New Player.Metadata() With {.Location = Player.Metadata.FileLocation.Remote, .OriginalPath = OriginalURL, .Provider = New YoutubeDLMediaProvider(OriginalURL)}
                meta.Title = Title
                meta.Album = Album
                meta.Artists = New String() {Artist}
                meta.Bitrate = If(AudioBitrate, TotalBitrate.GetValueOrDefault)
                meta.Channels = AudioChannels.GetValueOrDefault
                meta.Codecs = New String() {AudioCodec, VideoCodec}
                Dim thumbs = Thumbnails?.Where(Function(k) IO.Path.GetExtension(k.URL) = ".jpg" AndAlso k.Height.HasValue)
                If thumbs IsNot Nothing Then
                    meta.CoverLink = If(If(thumbs.FirstOrDefault(Function(k) k.Height = 480),
                        If(thumbs.FirstOrDefault(Function(k) k.Height = 720),
                        If(thumbs.FirstOrDefault(Function(k) k.Height = 360),
                         If(thumbs.FirstOrDefault(Function(k) k.Height = 1080),
                         thumbs.FirstOrDefault))))?.URL, Thumbnail)
                    meta.ThumbnailCoverLink = meta.CoverLink
                End If
                meta.Genres = Categories
                meta.HasCover = True
                meta.Length = Duration.GetValueOrDefault
                If String.IsNullOrEmpty(URL) Then
                    Dim fFormats = Formats?.Where(Function(k) k.Protocol = "https") 'hls m3u not supported atm
                    Dim sFormat As Format = Nothing
                    If fFormats IsNot Nothing Then
                        Dim AudioOnlyFormats = fFormats.Where(Function(k) k.VideoExtension = "none") 'or resolution = "audio only"
                        Dim VideoOnlyFormats = fFormats.Where(Function(k) k.AudioExtension = "none")
                        Dim MixedOnlyFormats = fFormats.Where(Function(k) k.VideoExtension <> "none" AndAlso k.AudioExtension <> "none")
                        Select Case PreferedQuality
                            Case VideoQuality.best
                                sFormat = MixedOnlyFormats.OrderByDescending(Of Double)(Function(k) k.AudioBitrate.GetValueOrDefault).FirstOrDefault
                            Case VideoQuality.bestaudio
                                sFormat = AudioOnlyFormats.OrderByDescending(Of Double)(Function(k) k.AudioBitrate.GetValueOrDefault).FirstOrDefault
                            Case VideoQuality.bestvideo
                                sFormat = VideoOnlyFormats.OrderByDescending(Of Double)(Function(k) k.AudioBitrate.GetValueOrDefault).FirstOrDefault
                            Case VideoQuality.worst
                                sFormat = MixedOnlyFormats.OrderByDescending(Of Double)(Function(k) k.AudioBitrate.GetValueOrDefault).LastOrDefault
                            Case VideoQuality.worstaudio
                                sFormat = AudioOnlyFormats.OrderByDescending(Of Double)(Function(k) k.AudioBitrate.GetValueOrDefault).LastOrDefault
                            Case VideoQuality.worstvideo
                                sFormat = VideoOnlyFormats.OrderByDescending(Of Double)(Function(k) k.AudioBitrate.GetValueOrDefault).LastOrDefault
                        End Select
                    End If
                    If sFormat IsNot Nothing Then
                        meta.Path = sFormat.Extension
                        meta.Path = sFormat.URL
                        meta.Size = sFormat.FileSize.GetValueOrDefault
                    End If
                Else
                    meta.Extension = Extension
                    meta.Path = URL
                End If
                If meta.Size = 0 Then meta.Size = FilesizeApprox.GetValueOrDefault
                meta.UID = URLtoUID(OriginalURL)
                Return meta
            End Function
        End Class

        ' Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);            
        Public Class YoutubePlaylist
            <JsonProperty("id")>
            Public Property ID() As String

            <JsonProperty("title")>
            Public Property Title() As String

            <JsonProperty("availability")>
            Public Property Availability() As String

            <JsonProperty("channel_follower_count")>
            Public Property ChannelFollowerCount() As Object

            <JsonProperty("description")>
            Public Property Description() As String

            <JsonProperty("tags")>
            Public Property Tags() As List(Of Object)

            <JsonProperty("thumbnails")>
            Public Property Thumbnails() As List(Of Thumbnail)

            <JsonProperty("modified_date")>
            Public Property ModifiedDate() As String

            <JsonProperty("view_count")>
            Public Property ViewCount() As Integer?

            <JsonProperty("playlist_count")>
            Public Property PlaylistCount() As Integer?

            <JsonProperty("channel")>
            Public Property Channel() As String

            <JsonProperty("channel_id")>
            Public Property ChannelID() As String

            <JsonProperty("uploader_id")>
            Public Property UploaderID() As String

            <JsonProperty("uploader")>
            Public Property Uploader() As String

            <JsonProperty("channel_url")>
            Public Property ChannelURL() As String

            <JsonProperty("uploader_url")>
            Public Property UploaderURL() As String

            <JsonProperty("_type")>
            Public Property Type() As String

            <JsonProperty("entries")>
            Public Property Entries() As List(Of YoutubeVideo)

            <JsonProperty("extractor_key")>
            Public Property extractor_key() As String

            <JsonProperty("extractor")>
            Public Property extractor() As String

            <JsonProperty("webpage_url")>
            Public Property WebpageURL() As String

            <JsonProperty("original_url")>
            Public Property OriginalURL() As String

            <JsonProperty("webpage_url_basename")>
            Public Property webpage_url_basename() As String

            <JsonProperty("webpage_url_domain")>
            Public Property webpage_url_domain() As String

            <JsonProperty("requested_entries")>
            Public Property requested_entries() As List(Of Integer?)

            <JsonProperty("epoch")>
            Public Property epoch() As Integer?

            <JsonProperty("__files_to_move")>
            Public Property __files_to_move() As Object

            <JsonProperty("_version")>
            Public Property Version() As Version
        End Class

#End Region
    End Namespace
End Namespace