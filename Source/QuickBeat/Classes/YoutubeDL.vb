Imports System.ComponentModel
Imports System.Globalization
Imports Newtonsoft
Imports QuickBeat.Classes
Imports QuickBeat.Interfaces

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

        Private _VideoQuality As YoutubeVideo.Quality = YoutubeVideo.Quality.best
        Property VideoQuality As YoutubeVideo.Quality
            Get
                Return _VideoQuality
            End Get
            Set(value As YoutubeVideo.Quality)
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

        Public Class YoutubeVideo
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
            Property RequestQuality As Quality
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
            Public Class Thumbnail
                Public Property ID As String
                Public Property URL As String
                Public Property Width As Integer
                Public Property Height As Integer
                Public Property Resolution As String
            End Class
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
            Public Enum Quality
                best
                worst
                bestvideo
                worstvideo
                bestaudio
                worstaudio
            End Enum
            Public Shadows Function ToString() As String
                Return Title & Environment.NewLine & Author & Environment.NewLine & URL & Environment.NewLine & DirectURL & Environment.NewLine & RequestQuality.ToString
            End Function
        End Class
        Public Class YoutubePlaylist
            Inherits ObjectModel.ObservableCollection(Of YoutubeVideo)

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

            Private _Thumbnails As New List(Of YoutubeVideo.Thumbnail)
            Property Thumbnails As List(Of YoutubeVideo.Thumbnail)
                Get
                    Return _Thumbnails
                End Get
                Set(value As List(Of YoutubeVideo.Thumbnail))
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
        Public Enum LinkState
            Free
            ContactingYTDL
            ParsingData
        End Enum

        Public Enum SearchProvider
            Youtube
            SoundCloud
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
        ''' A quick function opposed to <see cref="DumpAndManageVideo(String)"/>, gets only essential info:<see cref="YoutubeVideo.Title"/> and <see cref="YoutubeVideo.DirectURL"/>
        ''' </summary>
        ''' <param name="URL">The video's url</param>        
        ''' <returns></returns>
        Public Async Function GetVideo(URL As String) As Task(Of YoutubeVideo)
            If Not IO.File.Exists(YoutubeDLLocation) Then Return Nothing
            If Utilities.SharedProperties.Instance.IsInternetConnected Then
                IsBusy = True
                SetKnownState(LinkState.ContactingYTDL)
                Dim YoutubeDL As Process = Process.Start(New ProcessStartInfo(YoutubeDLLocation, "-s -e -g -f " & VideoQuality.ToString & Space(1) & URL) With {.RedirectStandardOutput = True, .UseShellExecute = False, .WindowStyle = ProcessWindowStyle.Hidden, .CreateNoWindow = True})
                SetKnownState(LinkState.ParsingData)
                Dim AInfo = Await YoutubeDL.StandardOutput.ReadToEndAsync
                Dim Info = AInfo.Split(New Char() {vbCr, vbCrLf, vbLf})
                If Info.Length >= 2 Then
                    Dim Vid As New YoutubeVideo With {.Title = Info(0), .URL = URL, .DirectURL = Info(1), .RequestQuality = VideoQuality}
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
                    Dim YoutubeDL As Process = Process.Start(New ProcessStartInfo(YoutubeDLLocation, $"""{If(Provider = SearchProvider.Youtube, "yt", "sc")}search{Limit}:{Query}"" --get-id") With {.RedirectStandardOutput = True, .UseShellExecute = False, .WindowStyle = ProcessWindowStyle.Hidden, .CreateNoWindow = True})
                    SetKnownState(LinkState.ParsingData)
                    Dim out = Await YoutubeDL.StandardOutput.ReadToEndAsync
                    IsBusy = False
                    Return out.Split(New Char() {"\n", "\r", "\r\n"})
                End If
            End If
            Return Nothing
        End Function
        Public Async Function SearchVideoAndDump(Query As String, Limit As Integer, Provider As SearchProvider) As Task(Of IEnumerable(Of YoutubeVideo))
            If Not IO.File.Exists(YoutubeDLLocation) Then Return Nothing
            If Limit >= 1 Then
                If Utilities.SharedProperties.Instance.IsInternetConnected Then
                    Try
                        IsBusy = True
                        SetKnownState(LinkState.ContactingYTDL)
                        Dim YoutubeDL As Process = Process.Start(New ProcessStartInfo(YoutubeDLLocation, $"""{If(Provider = SearchProvider.Youtube, "yt", "sc")}search{Limit}:{Query}"" -J") With {.RedirectStandardOutput = True, .UseShellExecute = False, .WindowStyle = ProcessWindowStyle.Hidden, .CreateNoWindow = True})
                        Dim sout = Await YoutubeDL.StandardOutput.ReadToEndAsync '.Split(New Char() {"\n", "\r", "\r\n"})
                        SetKnownState(LinkState.ParsingData)
                        Dim jsout = Json.Linq.JObject.Parse(sout)

                        Dim Videos As New List(Of YoutubeVideo)
                        For Each rawjson In jsout("entries")
                            Videos.Add(Await ParseYoutubeJson(rawjson.ToString))
                        Next
                        IsBusy = False
                        Return Videos
                    Catch ex As Exception
                        IsBusy = False
                        Utilities.DebugMode.Instance.Log(Of Youtube.YoutubeDL)(ex.ToString)
                    End Try
                    Return Nothing
                End If
            End If
            Return Nothing
        End Function
        Public Async Function SearchVideo(Query As String, Provider As SearchProvider) As Task(Of String)
            If Not IO.File.Exists(YoutubeDLLocation) Then Return Nothing
            If Utilities.SharedProperties.Instance.IsInternetConnected Then
                IsBusy = True
                SetKnownState(LinkState.ContactingYTDL)
                Dim YoutubeDL As Process = Process.Start(New ProcessStartInfo(YoutubeDLLocation, $"""{If(Provider = SearchProvider.Youtube, "yt", "sc")}search:{Query}"" --get-id") With {.RedirectStandardOutput = True, .UseShellExecute = False, .WindowStyle = ProcessWindowStyle.Hidden, .CreateNoWindow = True})
                IsBusy = False
                Return Await YoutubeDL.StandardOutput.ReadToEndAsync
            Else
                Return Nothing
            End If
        End Function
        Public Async Function SearchVideoAndDump(Query As String, Provider As SearchProvider) As Task(Of YoutubeVideo)
            If Not IO.File.Exists(YoutubeDLLocation) Then Return Nothing
            If Utilities.SharedProperties.Instance.IsInternetConnected Then
                IsBusy = True
                SetKnownState(LinkState.ContactingYTDL)
                Dim YoutubeDL As Process = Process.Start(New ProcessStartInfo(YoutubeDLLocation, $"""{If(Provider = SearchProvider.Youtube, "yt", "sc")}search:{Query}"" -j") With {.RedirectStandardOutput = True, .UseShellExecute = False, .WindowStyle = ProcessWindowStyle.Hidden, .CreateNoWindow = True})
                Dim Info = Await YoutubeDL.StandardOutput.ReadToEndAsync
                SetKnownState(LinkState.ParsingData)
                Dim vid = Await ParseYoutubeJson(Info)
                IsBusy = False
                Return vid
            Else
                Return Nothing
            End If
        End Function
        Public Async Function DumpAndManagePlaylist(URL As String, Optional limit As Integer = -1) As Task(Of YoutubePlaylist)
            If Not IO.File.Exists(YoutubeDLLocation) Then Return Nothing
            If Utilities.SharedProperties.Instance.IsInternetConnected Then
                IsBusy = True
                SetKnownState(LinkState.ContactingYTDL)
                Dim YoutubeDL As Process = Process.Start(New ProcessStartInfo(YoutubeDLLocation, "-J " & If(limit = -1, "--yes-playlist", "--playlist-end " & limit) & Space(1) & URL) With {.RedirectStandardOutput = True, .UseShellExecute = False, .WindowStyle = ProcessWindowStyle.Hidden, .CreateNoWindow = True})
                Dim Info = Await YoutubeDL.StandardOutput.ReadToEndAsync
                'fix invalid json
                Try
                    SetKnownState(LinkState.ParsingData)
                    Dim jsout = Json.Linq.JObject.Parse(Info)

                    Dim Playlist As New YoutubePlaylist
                    With Playlist
                        .ID = jsout("id")
                        .Title = jsout("title")
                        .Description = jsout("description")
                        Dim RawDate As String = If(jsout("modified_date"), "")
                        Dim _Date As Date
                        If Not String.IsNullOrEmpty(RawDate) AndAlso Date.TryParse($"{RawDate.Substring(0, 4)} {RawDate.Substring(4, 2)} {RawDate.Substring(6, 2)}", _Date) Then
                            .ModifiedDate = _Date
                        End If
                        .ViewCount = If(jsout("view_count"), 0)
                        .ExpectedCount = If(jsout("playlist_count"), 0)
                        .Channel = jsout("channel")?.ToString
                        .ChannelID = jsout("channel_id")
                        .Uploader = jsout("uploader")
                        .UploaderID = jsout("uploader_id")
                        .ChannelURL = jsout("channel_url")
                        .UploaderURL = jsout("uploader_url")
                        Dim RawThumbnails = jsout("thumbnails")
                        Dim Thumbnails As New List(Of YoutubeVideo.Thumbnail)
                        If RawThumbnails IsNot Nothing Then
                            For Each Thumbnail As Json.Linq.JToken In RawThumbnails
                                Try
                                    Dim CThumbnail As New YoutubeVideo.Thumbnail
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
                        .Thumbnails = Thumbnails
                    End With
                    For Each rawjson In jsout("entries")
                        Playlist.Add(Await ParseYoutubeJson(rawjson.ToString))
                    Next
                    IsBusy = False
                    Return Playlist
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
        Public Async Function ParseYoutubeJson(StrJson As String) As Task(Of YoutubeVideo)
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
                                          Dim DirectURLS As New List(Of YoutubeVideo.Video)
                                          Dim AudioOnlyURLS As New List(Of YoutubeVideo.Video)
                                          Dim VideoOnlyURLS As New List(Of YoutubeVideo.Video)
                                          Dim MixedOnlyURLS As New List(Of YoutubeVideo.Video)
                                          For Each RURL As Json.Linq.JToken In RawDirectURLS
                                              Dim CVideo As New YoutubeVideo.Video
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
                                              Dim HTTPHeader As New YoutubeVideo.Video.HTTPHeader
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
                                                  CVideo.Type = YoutubeVideo.Video.FileType.Mixed
                                              ElseIf CVideo.AudioCodec <> "none" AndAlso CVideo.VideoCodec = "none" Then
                                                  CVideo.Type = YoutubeVideo.Video.FileType.Audio
                                              ElseIf CVideo.AudioCodec = "none" AndAlso CVideo.VideoCodec <> "none" Then
                                                  CVideo.Type = YoutubeVideo.Video.FileType.Video
                                              End If
                                              Select Case CVideo.Type
                                                  Case YoutubeVideo.Video.FileType.Mixed
                                                      CVideo.VideoBitrate = RURL("vbr")
                                                      CVideo.AudioSampleRate = RURL("asr")
                                                      CVideo.TotalBitrate = RURL("TotalBitrate")
                                                      CVideo.AudioBitrate = RURL("abr")
                                                      MixedOnlyURLS.Add(CVideo)
                                                  Case YoutubeVideo.Video.FileType.Audio
                                                      CVideo.AudioSampleRate = RURL("asr")
                                                      CVideo.TotalBitrate = RURL("TotalBitrate")
                                                      CVideo.AudioBitrate = RURL("abr")
                                                      AudioOnlyURLS.Add(CVideo)
                                                  Case YoutubeVideo.Video.FileType.Video
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
                                          Dim HQAudio = If(AudioOnlyURLS?.Any, AudioOnlyURLS.Last, New YoutubeVideo.Video)
                                          Dim LQAudio = If(AudioOnlyURLS?.Any, AudioOnlyURLS.First, New YoutubeVideo.Video)
                                          Dim HQVideo = If(VideoOnlyURLS?.Any, VideoOnlyURLS.Last, New YoutubeVideo.Video)
                                          Dim LQVideo = If(VideoOnlyURLS?.Any, VideoOnlyURLS.First, New YoutubeVideo.Video)
                                          Dim HQMixed = If(MixedOnlyURLS?.Any, MixedOnlyURLS.Last, New YoutubeVideo.Video)
                                          Dim LQMixed = If(MixedOnlyURLS?.Any, MixedOnlyURLS.First, New YoutubeVideo.Video)
                                          Dim DislikeCount = ParsedInfo("dislike_count")
                                          Dim Duration = TimeSpan.FromSeconds(If(ParsedInfo("duration"), 0))
                                          Dim FileName = ParsedInfo("_filename")
                                          Dim FPS = ParsedInfo("fps")
                                          Dim LikeCount = ParsedInfo("like_count")
                                          Dim RawSubtitles = ParsedInfo("subtitles")
                                          Dim Subtitles As New List(Of Tuple(Of String, List(Of YoutubeVideo.Subtitle)))
                                          If RawSubtitles IsNot Nothing Then
                                              For Each Subtitle As Json.Linq.JToken In RawSubtitles
                                                  Dim CSubtitle As New List(Of YoutubeVideo.Subtitle)
                                                  For Each _Subtitle As Json.Linq.JToken In Subtitle
                                                      For Each __Subtitle As Json.Linq.JToken In _Subtitle
                                                          Dim CCSubtitle As New YoutubeVideo.Subtitle
                                                          CCSubtitle.Language = CType(Subtitle, Json.Linq.JProperty).Name
                                                          CCSubtitle.Extension = __Subtitle("ext") 'String.Join(Environment.NewLine, _Subtitle("ext")).Split(UniversalSplitter) '_Subtitle("ext")
                                                          CCSubtitle.RawData = __Subtitle("url")
                                                          CSubtitle.Add(CCSubtitle)
                                                      Next
                                                  Next
                                                  Subtitles.Add(New Tuple(Of String, List(Of YoutubeVideo.Subtitle))(If(CSubtitle.FirstOrDefault?.Language, "null"), CSubtitle))
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
                                          Dim Thumbnails As New List(Of YoutubeVideo.Thumbnail)
                                          If RawThumbnails IsNot Nothing Then
                                              For Each Thumbnail As Json.Linq.JToken In RawThumbnails
                                                  Try
                                                      Dim CThumbnail As New YoutubeVideo.Thumbnail
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
                                          Dim HQThumbnail = If(Thumbnails.Any, Thumbnails.Last, New YoutubeVideo.Thumbnail)
                                          Dim LQThumbnail = If(Thumbnails.Any, Thumbnails.First, New YoutubeVideo.Thumbnail)
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
                                          Dim MediaInfo As New YoutubeVideo.MediaProperties
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
                                          Dim PlaylistInfo As New YoutubeVideo.YoutubePlaylist
                                          Dim intPlaylistIndex = 0
                                          PlaylistInfo.Index = If(PlaylistIndex IsNot Nothing, If(Integer.TryParse(PlaylistIndex, intPlaylistIndex), intPlaylistIndex, -1), -1)
                                          PlaylistInfo.Playlist_ID = PlaylistID
                                          PlaylistInfo.Title = PlaylistTitle
                                          PlaylistInfo.Uploader = PlaylistUploader
                                          PlaylistInfo.Uploader_ID = PlaylistUploader_ID
                                          'Chapter info
                                          Dim RawChapters = ParsedInfo("chapters")
                                          Dim Chapters As New List(Of YoutubeVideo.Chapter)
                                          If RawChapters IsNot Nothing Then
                                              For Each Chapter As Json.Linq.JToken In RawChapters
                                                  Try
                                                      If Chapter Is Nothing Then Continue For
                                                  Dim ETime = Chapter("end_time")
                                                  Dim STime = Chapter("start_time")
                                                  Dim CTitle = Chapter("title")
                                                  Dim Chap As New YoutubeVideo.Chapter
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
                                          Dim Video = New YoutubeVideo
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
                                              .LikeCount = LikeCount
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
        Public Async Function DumpAndManageVideo(URL As String) As Task(Of YoutubeVideo)
            If Not IO.File.Exists(YoutubeDLLocation) Then Return Nothing
            If Utilities.SharedProperties.Instance.IsInternetConnected Then
                Try
                    IsBusy = True
                    SetKnownState(LinkState.ContactingYTDL)
                    Dim YoutubeDL As Process = Process.Start(New ProcessStartInfo(YoutubeDLLocation, "-j " & URL) With {.RedirectStandardOutput = True, .UseShellExecute = False, .WindowStyle = ProcessWindowStyle.Hidden, .CreateNoWindow = True})
                    Dim Info = Await YoutubeDL.StandardOutput.ReadToEndAsync
                    SetKnownState(LinkState.ParsingData)
                    Dim vid = Await ParseYoutubeJson(Info)
                    IsBusy = False
                    Return vid
                Catch ex As Exception
                    IsBusy = False
                    Utilities.DebugMode.Instance.Log(Of Youtube.YoutubeDL)(ex.ToString)
                End Try
                Return Nothing
            End If
            Return Nothing
        End Function

        Public Sub Init() Implements IStartupItem.Init

        End Sub

    End Class
    Namespace WPF
        Public Class Int32ToQualityConverter
            Implements IValueConverter

            Public Function Convert(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.Convert
                Return CInt(CType(value, Youtube.YoutubeDL.YoutubeVideo.Quality))
            End Function

            Public Function ConvertBack(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.ConvertBack
                Return CType(CInt(value), Youtube.YoutubeDL.YoutubeVideo.Quality)
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
                If Not Utilities.CommonFunctions.CheckInternet Then
                    HandyControl.Controls.MessageBox.Error(Utilities.ResourceResolver.Strings.QUERY_INTERNET_DISCONNECTED)
                    Return
                End If
                Dim url = Dialogs.InputBox.ShowSingle(Application.Current.MainWindow, "URL")
                Dim video = Await _Client.DumpAndManageVideo(url)
                If video Is Nothing Then Return
                Dim meta As New Player.Metadata With {.Location = Player.Metadata.FileLocation.Remote, .Title = video.Title, .OriginalPath = url, .Provider = New Youtube.YoutubeDLMediaProvider(url), .BlockDOWNLOADPROC = True}
                meta.Artists = If(video.Properties.Artists IsNot Nothing, If(video.Properties.Artists.Length > 0, video.Properties.Artists, New String() {video.Author}), New String() {video.Author})
                meta.Album = video.Properties.Album
                meta.Genres = video.Properties.Genres
                meta.Length = video.Duration.TotalSeconds
                Select Case _Client.VideoQuality
                    Case YoutubeDL.YoutubeVideo.Quality.best
                        meta.Path = video.HQMixed.DirectURL
                    Case YoutubeDL.YoutubeVideo.Quality.bestaudio
                        meta.Path = video.HQAudio.DirectURL
                    Case YoutubeDL.YoutubeVideo.Quality.bestvideo
                        meta.Path = video.HQVideo.DirectURL
                    Case YoutubeDL.YoutubeVideo.Quality.worst
                        meta.Path = video.LQMixed.DirectURL
                    Case YoutubeDL.YoutubeVideo.Quality.worstaudio
                        meta.Path = video.LQAudio.DirectURL
                    Case YoutubeDL.YoutubeVideo.Quality.worstvideo
                        meta.Path = video.LQVideo.DirectURL
                End Select
                If String.IsNullOrEmpty(meta.Path) Then
                    If Not String.IsNullOrEmpty(video.HQMixed.DirectURL) Then
                        meta.Path = video.HQMixed.DirectURL
                    ElseIf Not String.IsNullOrEmpty(video.HQAudio.DirectURL) Then
                        meta.Path = video.HQAudio.DirectURL
                    ElseIf Not String.IsNullOrEmpty(video.LQMixed.DirectURL) Then
                        meta.Path = video.LQMixed.DirectURL
                    ElseIf Not String.IsNullOrEmpty(video.LQAudio.DirectURL) Then
                        meta.Path = video.LQAudio.DirectURL
                    ElseIf Not String.IsNullOrEmpty(video.DirectURL) Then
                        meta.Path = video.DirectURL
                    End If
                End If
                meta.PlayCount = video.ViewCount
                Dim thumb As New BitmapImage
                thumb.BeginInit()
                thumb.UriSource = New Uri(video.ThumbnailURL, UriKind.Absolute)
                thumb.EndInit()
                meta.Covers = New ImageSource() {thumb}
#Disable Warning
                Await TryCast(parameter, Player.Player)?.LoadSong(meta)
#Enable Warning
                Dim Vsub = video.Subtitles.FirstOrDefault(Function(k) k.Item1 = "en")
                If Vsub IsNot Nothing Then
                    Dim _Vsub = Vsub.Item2.FirstOrDefault(Function(k) k.Extension = "vtt")
                    If _Vsub IsNot Nothing Then
                        If TypeOf Application.Current.MainWindow Is MainWindow Then
                            With CType(Application.Current.MainWindow, MainWindow)
                                Dim parser As New SubtitlesParser.Classes.Parsers.SubParser
                                Using httpc As New Net.Http.HttpClient()
                                    Dim data = Await httpc.GetStreamAsync(_Vsub.RawData)
                                    Dim subs = parser.ParseStream(data)
                                    Application.Current.Dispatcher.Invoke(Sub()
                                                                              If .Subtitles Is Nothing Then .Subtitles = New ObjectModel.ObservableCollection(Of SubtitlesParser.Classes.SubtitleItem)
                                                                              For Each _sub In subs
                                                                                  .Subtitles?.Add(_sub)
                                                                                  Dim PosSyncProc As New Un4seen.Bass.SYNCPROC(Sub(h, c, _data, u)
                                                                                                                                   Application.Current.Dispatcher.Invoke(Sub()
                                                                                                                                                                             .InfoText = String.Join(Environment.NewLine, _sub.PlaintextLines)
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
                ElseIf video.Subtitles.Count > 0 Then
                    Dim _Vsub = video.Subtitles.FirstOrDefault(Function(k) k.Item2.FirstOrDefault(Function(l) l.Extension = "vtt") IsNot Nothing)
                    If _Vsub IsNot Nothing Then
                        If TypeOf Application.Current.MainWindow Is MainWindow Then
                            With CType(Application.Current.MainWindow, MainWindow)
                                Dim parser As New SubtitlesParser.Classes.Parsers.SubParser
                                Using httpc As New Net.Http.HttpClient()
                                    Dim data = Await httpc.GetStreamAsync(_Vsub.Item2.FirstOrDefault(Function(k) k.Extension = "vtt")?.RawData)
                                    Dim subs = parser.ParseStream(data)
                                    Application.Current.Dispatcher.Invoke(Sub()
                                                                              If .Subtitles Is Nothing Then .Subtitles = New ObjectModel.ObservableCollection(Of SubtitlesParser.Classes.SubtitleItem)
                                                                              For Each _sub In subs
                                                                                  .Subtitles?.Add(_sub)
                                                                                  Dim PosSyncProc As New Un4seen.Bass.SYNCPROC(Sub(h, c, _data, u)
                                                                                                                                   Application.Current.Dispatcher.Invoke(Sub()
                                                                                                                                                                             .InfoText = String.Join(Environment.NewLine, _sub.PlaintextLines)
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
            End Sub

            Public Function CanExecute(parameter As Object) As Boolean Implements ICommand.CanExecute
                Return Not IsNothing(_Client) AndAlso IO.File.Exists(_Client.YoutubeDLLocation) AndAlso TypeOf parameter Is Player.Player
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
                If Not Utilities.CommonFunctions.CheckInternet Then
                    HandyControl.Controls.MessageBox.Error(Utilities.ResourceResolver.Strings.QUERY_INTERNET_DISCONNECTED)
                    Return
                End If
                Dim url = Dialogs.InputBox.ShowSingle(Application.Current.MainWindow, "URL")
                Dim video = Await _Client.GetVideo(url)
                If video Is Nothing Then Return
                Await TryCast(parameter, Player.Player)?.LoadSong(New Player.Metadata With {.Location = Player.Metadata.FileLocation.Remote, .OriginalPath = url, .Provider = New Youtube.YoutubeDLMediaProvider(url), .Path = video.DirectURL, .Title = video.Title, .BlockDOWNLOADPROC = True})
            End Sub

            Public Function CanExecute(parameter As Object) As Boolean Implements ICommand.CanExecute
                Return Not IsNothing(_Client) AndAlso IO.File.Exists(_Client.YoutubeDLLocation) AndAlso TypeOf parameter Is Player.Player
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
                If Not Utilities.CommonFunctions.CheckInternet Then
                    HandyControl.Controls.MessageBox.Error(Utilities.ResourceResolver.Strings.QUERY_INTERNET_DISCONNECTED)
                    Return
                End If
                Dim query = Dialogs.InputBox.ShowSingle(Application.Current.MainWindow, "Query")
                If String.IsNullOrEmpty(query) Then Return
                Dim videos = Await _Client.SearchVideoAndDump(query, 1, YoutubeDL.SearchProvider.Youtube)
                Dim video = videos.FirstOrDefault
                If video Is Nothing Then Return
                If HandyControl.Controls.MessageBox.Ask(Utilities.ResourceResolver.Strings.QUERY_FILEPLAY.Replace("%f", video.Title & " - " & video.Author)) = MessageBoxResult.Cancel Then
                    Return
                End If
                Dim meta As New Player.Metadata With {.Location = Player.Metadata.FileLocation.Remote, .OriginalPath = video.URL, .Provider = New Youtube.YoutubeDLMediaProvider(video.URL), .Title = video.Title, .BlockDOWNLOADPROC = True}
                meta.Artists = If(video.Properties.Artists IsNot Nothing, If(video.Properties.Artists.Length > 0, video.Properties.Artists, New String() {video.Author}), New String() {video.Author})
                meta.Album = video.Properties.Album
                meta.Genres = video.Properties.Genres
                meta.Length = video.Duration.TotalSeconds
                Select Case _Client.VideoQuality
                    Case YoutubeDL.YoutubeVideo.Quality.best
                        meta.Path = video.HQMixed.DirectURL
                    Case YoutubeDL.YoutubeVideo.Quality.bestaudio
                        meta.Path = video.HQAudio.DirectURL
                    Case YoutubeDL.YoutubeVideo.Quality.bestvideo
                        meta.Path = video.HQVideo.DirectURL
                    Case YoutubeDL.YoutubeVideo.Quality.worst
                        meta.Path = video.LQMixed.DirectURL
                    Case YoutubeDL.YoutubeVideo.Quality.worstaudio
                        meta.Path = video.LQAudio.DirectURL
                    Case YoutubeDL.YoutubeVideo.Quality.worstvideo
                        meta.Path = video.LQVideo.DirectURL
                End Select
                If String.IsNullOrEmpty(meta.Path) Then
                    If Not String.IsNullOrEmpty(video.HQMixed.DirectURL) Then
                        meta.Path = video.HQMixed.DirectURL
                    ElseIf Not String.IsNullOrEmpty(video.HQAudio.DirectURL) Then
                        meta.Path = video.HQAudio.DirectURL
                    ElseIf Not String.IsNullOrEmpty(video.LQMixed.DirectURL) Then
                        meta.Path = video.LQMixed.DirectURL
                    ElseIf Not String.IsNullOrEmpty(video.LQAudio.DirectURL) Then
                        meta.Path = video.LQAudio.DirectURL
                    ElseIf Not String.IsNullOrEmpty(video.DirectURL) Then
                        meta.Path = video.DirectURL
                    End If
                End If
                meta.PlayCount = If(video.ViewCount > Integer.MaxValue, Integer.MaxValue, video.ViewCount)
                Dim thumb As New BitmapImage
                thumb.BeginInit()
                thumb.UriSource = New Uri(video.ThumbnailURL, UriKind.Absolute)
                thumb.EndInit()
                meta.Covers = New ImageSource() {thumb}
#Disable Warning
                TryCast(parameter, Player.Player)?.LoadSong(meta)
#Enable Warning
                Dim Vsub = video.Subtitles.FirstOrDefault(Function(k) k.Item1 = "en")
                If Vsub IsNot Nothing Then
                    Dim _Vsub = Vsub.Item2.FirstOrDefault(Function(k) k.Extension = "vtt")
                    If _Vsub IsNot Nothing Then
                        If TypeOf Application.Current.MainWindow Is MainWindow Then
                            With CType(Application.Current.MainWindow, MainWindow)
                                Dim parser As New SubtitlesParser.Classes.Parsers.SubParser
                                Using httpc As New Net.Http.HttpClient()
                                    Dim data = Await httpc.GetStreamAsync(_Vsub.RawData)
                                    Dim subs = parser.ParseStream(data)
                                    Application.Current.Dispatcher.Invoke(Sub()
                                                                              If .Subtitles Is Nothing Then .Subtitles = New ObjectModel.ObservableCollection(Of SubtitlesParser.Classes.SubtitleItem)
                                                                              For Each _sub In subs
                                                                                  .Subtitles?.Add(_sub)
                                                                                  Dim PosSyncProc As New Un4seen.Bass.SYNCPROC(Sub(h, c, _data, u)
                                                                                                                                   Application.Current.Dispatcher.Invoke(Sub()
                                                                                                                                                                             .InfoText = String.Join(Environment.NewLine, _sub.PlaintextLines)
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
                    Dim _Vsub = video.Subtitles.FirstOrDefault(Function(k) k.Item2.FirstOrDefault(Function(l) l.Extension = "vtt") IsNot Nothing)
                    If _Vsub IsNot Nothing Then
                        If TypeOf Application.Current.MainWindow Is MainWindow Then
                            With CType(Application.Current.MainWindow, MainWindow)
                                Dim parser As New SubtitlesParser.Classes.Parsers.SubParser
                                Using httpc As New Net.Http.HttpClient()
                                    Dim data = Await httpc.GetStreamAsync(_Vsub.Item2.FirstOrDefault(Function(k) k.Extension = "vtt")?.RawData)
                                    Dim subs = parser.ParseStream(data)
                                    Application.Current.Dispatcher.Invoke(Sub()
                                                                              If .Subtitles Is Nothing Then .Subtitles = New ObjectModel.ObservableCollection(Of SubtitlesParser.Classes.SubtitleItem)
                                                                              For Each _sub In subs
                                                                                  .Subtitles?.Add(_sub)
                                                                                  Dim PosSyncProc As New Un4seen.Bass.SYNCPROC(Sub(h, c, _data, u)
                                                                                                                                   Application.Current.Dispatcher.Invoke(Sub()
                                                                                                                                                                             .InfoText = String.Join(Environment.NewLine, _sub.PlaintextLines)
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
            End Sub

            Public Function CanExecute(parameter As Object) As Boolean Implements ICommand.CanExecute
                Return Not IsNothing(_Client) AndAlso IO.File.Exists(_Client.YoutubeDLLocation) AndAlso TypeOf parameter Is Player.Player
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
                If Not Utilities.CommonFunctions.CheckInternet Then
                    HandyControl.Controls.MessageBox.Error(Utilities.ResourceResolver.Strings.QUERY_INTERNET_DISCONNECTED)
                    Return
                End If
                Dim query = Dialogs.InputBox.ShowSingle(Application.Current.MainWindow, "Query")
                If String.IsNullOrEmpty(query) Then Return
                Dim videos = Await _Client.SearchVideoAndDump(query, 1, _Client.DefaultSearchProvider)
                Dim video = videos.FirstOrDefault
                If video Is Nothing Then Return
                If HandyControl.Controls.MessageBox.Ask(Utilities.ResourceResolver.Strings.QUERY_FILEPLAY.Replace("%f", video.Title & " - " & video.Author)) = MessageBoxResult.Cancel Then
                    Return
                End If
                Dim meta As New Player.Metadata With {.Location = Player.Metadata.FileLocation.Remote, .OriginalPath = video.URL, .Provider = New Youtube.YoutubeDLMediaProvider(video.URL), .Title = video.Title, .BlockDOWNLOADPROC = True}
                meta.Artists = If(video.Properties.Artists IsNot Nothing, If(video.Properties.Artists.Length > 0, video.Properties.Artists, New String() {video.Author}), New String() {video.Author})
                meta.Album = video.Properties.Album
                meta.Genres = video.Properties.Genres
                meta.Length = video.Duration.TotalSeconds
                Select Case _Client.VideoQuality
                    Case YoutubeDL.YoutubeVideo.Quality.best
                        meta.Path = video.HQMixed.DirectURL
                    Case YoutubeDL.YoutubeVideo.Quality.bestaudio
                        meta.Path = video.HQAudio.DirectURL
                    Case YoutubeDL.YoutubeVideo.Quality.bestvideo
                        meta.Path = video.HQVideo.DirectURL
                    Case YoutubeDL.YoutubeVideo.Quality.worst
                        meta.Path = video.LQMixed.DirectURL
                    Case YoutubeDL.YoutubeVideo.Quality.worstaudio
                        meta.Path = video.LQAudio.DirectURL
                    Case YoutubeDL.YoutubeVideo.Quality.worstvideo
                        meta.Path = video.LQVideo.DirectURL
                End Select
                If String.IsNullOrEmpty(meta.Path) Then
                    If Not String.IsNullOrEmpty(video.HQMixed.DirectURL) Then
                        meta.Path = video.HQMixed.DirectURL
                    ElseIf Not String.IsNullOrEmpty(video.HQAudio.DirectURL) Then
                        meta.Path = video.HQAudio.DirectURL
                    ElseIf Not String.IsNullOrEmpty(video.LQMixed.DirectURL) Then
                        meta.Path = video.LQMixed.DirectURL
                    ElseIf Not String.IsNullOrEmpty(video.LQAudio.DirectURL) Then
                        meta.Path = video.LQAudio.DirectURL
                    ElseIf Not String.IsNullOrEmpty(video.DirectURL) Then
                        meta.Path = video.DirectURL
                    End If
                End If
                meta.PlayCount = If(video.ViewCount > Integer.MaxValue, Integer.MaxValue, video.ViewCount)
                Dim thumb As New BitmapImage
                thumb.BeginInit()
                thumb.UriSource = New Uri(video.ThumbnailURL, UriKind.Absolute)
                thumb.EndInit()
                meta.Covers = New ImageSource() {thumb}
                Await TryCast(parameter, Player.Player)?.LoadSong(meta)
                Dim Vsub = video.Subtitles.FirstOrDefault(Function(k) k.Item1 = "en")
                If Vsub IsNot Nothing Then
                    Dim _Vsub = Vsub.Item2.FirstOrDefault(Function(k) k.Extension = "vtt")
                    If _Vsub IsNot Nothing Then
                        If TypeOf Application.Current.MainWindow Is MainWindow Then
                            With CType(Application.Current.MainWindow, MainWindow)
                                Dim parser As New SubtitlesParser.Classes.Parsers.SubParser
                                Using httpc As New Net.Http.HttpClient()
                                    Dim data = Await httpc.GetStreamAsync(_Vsub.RawData)
                                    Dim subs = parser.ParseStream(data)
                                    Application.Current.Dispatcher.Invoke(Sub()
                                                                              If .Subtitles Is Nothing Then .Subtitles = New ObjectModel.ObservableCollection(Of SubtitlesParser.Classes.SubtitleItem)
                                                                              For Each _sub In subs
                                                                                  .Subtitles?.Add(_sub)
                                                                                  Dim PosSyncProc As New Un4seen.Bass.SYNCPROC(Sub(h, c, _data, u)
                                                                                                                                   Application.Current.Dispatcher.Invoke(Sub()
                                                                                                                                                                             .InfoText = String.Join(Environment.NewLine, _sub.PlaintextLines)
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
                    Dim _Vsub = video.Subtitles.FirstOrDefault(Function(k) k.Item2.FirstOrDefault(Function(l) l.Extension = "vtt") IsNot Nothing)
                    If _Vsub IsNot Nothing Then
                        If TypeOf Application.Current.MainWindow Is MainWindow Then
                            With CType(Application.Current.MainWindow, MainWindow)
                                Dim parser As New SubtitlesParser.Classes.Parsers.SubParser
                                Using httpc As New Net.Http.HttpClient()
                                    Dim data = Await httpc.GetStreamAsync(_Vsub.Item2.FirstOrDefault(Function(k) k.Extension = "vtt")?.RawData)
                                    Dim subs = parser.ParseStream(data)
                                    Application.Current.Dispatcher.Invoke(Sub()
                                                                              If .Subtitles Is Nothing Then .Subtitles = New ObjectModel.ObservableCollection(Of SubtitlesParser.Classes.SubtitleItem)
                                                                              For Each _sub In subs
                                                                                  .Subtitles?.Add(_sub)
                                                                                  Dim PosSyncProc As New Un4seen.Bass.SYNCPROC(Sub(h, c, _data, u)
                                                                                                                                   Application.Current.Dispatcher.Invoke(Sub()
                                                                                                                                                                             .InfoText = String.Join(Environment.NewLine, _sub.PlaintextLines)
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
            End Sub

            Public Function CanExecute(parameter As Object) As Boolean Implements ICommand.CanExecute
                Return Not IsNothing(_Client) AndAlso IO.File.Exists(_Client.YoutubeDLLocation) AndAlso TypeOf parameter Is Player.Player
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
                If Not Utilities.CommonFunctions.CheckInternet Then
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
                        Dim pThumb As New BitmapImage
                        pThumb.BeginInit()
                        pThumb.UriSource = pThumbURI
                        pThumb.EndInit()
                        pPlaylist.Cover = pThumb
                    End If
                    For Each video In playlist
                        If video Is Nothing Then Continue For
                        Dim meta As New Player.Metadata With {.Location = Player.Metadata.FileLocation.Remote, .Title = video.Title, .OriginalPath = video.URL, .Provider = New Youtube.YoutubeDLMediaProvider(video.URL), .BlockDOWNLOADPROC = True}
                        meta.Artists = If(video.Properties.Artists IsNot Nothing, If(video.Properties.Artists.Length > 0, video.Properties.Artists, New String() {video.Author}), New String() {video.Author})
                        meta.Album = video.Properties.Album
                        meta.Genres = video.Properties.Genres
                        meta.Length = video.Duration.TotalSeconds
                        Select Case _Client.VideoQuality
                            Case YoutubeDL.YoutubeVideo.Quality.best
                                meta.Path = video.HQMixed.DirectURL
                            Case YoutubeDL.YoutubeVideo.Quality.bestaudio
                                meta.Path = video.HQAudio.DirectURL
                            Case YoutubeDL.YoutubeVideo.Quality.bestvideo
                                meta.Path = video.HQVideo.DirectURL
                            Case YoutubeDL.YoutubeVideo.Quality.worst
                                meta.Path = video.LQMixed.DirectURL
                            Case YoutubeDL.YoutubeVideo.Quality.worstaudio
                                meta.Path = video.LQAudio.DirectURL
                            Case YoutubeDL.YoutubeVideo.Quality.worstvideo
                                meta.Path = video.LQVideo.DirectURL
                        End Select
                        If String.IsNullOrEmpty(meta.Path) Then
                            If Not String.IsNullOrEmpty(video.HQMixed.DirectURL) Then
                                meta.Path = video.HQMixed.DirectURL
                            ElseIf Not String.IsNullOrEmpty(video.HQAudio.DirectURL) Then
                                meta.Path = video.HQAudio.DirectURL
                            ElseIf Not String.IsNullOrEmpty(video.LQMixed.DirectURL) Then
                                meta.Path = video.LQMixed.DirectURL
                            ElseIf Not String.IsNullOrEmpty(video.LQAudio.DirectURL) Then
                                meta.Path = video.LQAudio.DirectURL
                            ElseIf Not String.IsNullOrEmpty(video.DirectURL) Then
                                meta.Path = video.DirectURL
                            End If
                        End If
                        meta.PlayCount = video.ViewCount
                        Dim thumb As New BitmapImage
                        thumb.BeginInit()
                        thumb.UriSource = New Uri(video.ThumbnailURL, UriKind.Absolute)
                        thumb.EndInit()
                        meta.Covers = New ImageSource() {thumb}
                        pPlaylist.Add(meta)
                    Next
#Disable Warning
                    TryCast(parameter, Player.Player)?.LoadPlaylist(pPlaylist)
#Enable Warning
                End If
            End Sub

            Public Function CanExecute(parameter As Object) As Boolean Implements ICommand.CanExecute
                Return Not IsNothing(_Client) AndAlso IO.File.Exists(_Client.YoutubeDLLocation) AndAlso TypeOf parameter Is Player.Player
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
End Namespace