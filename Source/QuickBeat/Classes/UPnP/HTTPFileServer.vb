Imports System
Imports System.ComponentModel
Imports System.IO
Imports System.Net
Imports System.Text
Imports System.Threading
Imports System.Web
Imports QuickBeat.Classes
Imports QuickBeat.Interfaces
Imports QuickBeat.Player
Imports QuickBeat.Utilities

Namespace UPnP
    Public Class HttpFileServer
        Implements IDisposable, IStartupItem, ComponentModel.INotifyPropertyChanged

#Region "WPF Support"
        Public Class DelegateStartCommand
            Implements ICommand

            Public Event CanExecuteChanged As EventHandler Implements ICommand.CanExecuteChanged

            Sub New()
                AddHandler CommandManager.RequerySuggested, New EventHandler(Sub(sender As Object, e As EventArgs)
                                                                                 RaiseEvent CanExecuteChanged(Me, New EventArgs())
                                                                             End Sub)
            End Sub

            Public Sub Execute(parameter As Object) Implements ICommand.Execute
                TryCast(parameter, HttpFileServer)?.Start()
            End Sub

            Public Function CanExecute(parameter As Object) As Boolean Implements ICommand.CanExecute
                Return parameter IsNot Nothing AndAlso TypeOf parameter Is HttpFileServer AndAlso Not CType(parameter, HttpFileServer).IsRunning
            End Function
        End Class
        Public Class DelegateStopCommand
            Implements ICommand

            Public Event CanExecuteChanged As EventHandler Implements ICommand.CanExecuteChanged

            Sub New()
                AddHandler CommandManager.RequerySuggested, New EventHandler(Sub(sender As Object, e As EventArgs)
                                                                                 RaiseEvent CanExecuteChanged(Me, New EventArgs())
                                                                             End Sub)
            End Sub

            Public Sub Execute(parameter As Object) Implements ICommand.Execute
                TryCast(parameter, HttpFileServer)?.Dispose() 'Same as stop
            End Sub

            Public Function CanExecute(parameter As Object) As Boolean Implements ICommand.CanExecute
                Return parameter IsNot Nothing AndAlso TypeOf parameter Is HttpFileServer
            End Function
        End Class
        Public Class DelegateOpenInBrowserCommand
            Implements ICommand

            Public Event CanExecuteChanged As EventHandler Implements ICommand.CanExecuteChanged

            Sub New()
                AddHandler CommandManager.RequerySuggested, New EventHandler(Sub(sender As Object, e As EventArgs)
                                                                                 RaiseEvent CanExecuteChanged(Me, New EventArgs())
                                                                             End Sub)
            End Sub

            Public Sub Execute(parameter As Object) Implements ICommand.Execute
                Process.Start("http://" & TryCast(parameter, HttpFileServer)?.URL & ":" & TryCast(parameter, HttpFileServer)?.Port)
            End Sub

            Public Function CanExecute(parameter As Object) As Boolean Implements ICommand.CanExecute
                Return parameter IsNot Nothing AndAlso TypeOf parameter Is HttpFileServer AndAlso Not String.IsNullOrEmpty(CType(parameter, HttpFileServer).URL)
            End Function
        End Class
        Public Class DelegatePushToRendererCommand
            Implements ICommand

            Public Event CanExecuteChanged As EventHandler Implements ICommand.CanExecuteChanged
            Private _Server As HttpFileServer
            Sub New(Server As HttpFileServer)
                _Server = Server
                AddHandler CommandManager.RequerySuggested, New EventHandler(Sub(sender As Object, e As EventArgs)
                                                                                 RaiseEvent CanExecuteChanged(Me, New EventArgs())
                                                                             End Sub)
            End Sub

            Public Sub Execute(parameter As Object) Implements ICommand.Execute
                With TryCast(parameter, UPnP.UPnPProvider)
                    If .SelectedRenderer IsNot Nothing Then
                        Dim encoURL = If(_Server.URL.StartsWith("http"), "", "http://") & _Server.URL & ":" & _Server.Port & "/" & System.Web.HttpUtility.UrlEncode(SharedProperties.Instance.Player.Path.Replace("\"c, "/"c))
                        .SelectedRenderer.SetAVTransportURI(0, encoURL, GenerateDLNAMetadata(SharedProperties.Instance.Player.StreamMetadata, _Server.URL & ":" & _Server.Port & "/" & SharedProperties.Instance.Player.Path))
                        .SelectedRenderer.Play()
                    End If
                End With
            End Sub

            Public Function CanExecute(parameter As Object) As Boolean Implements ICommand.CanExecute
                Return _Server?.IsRunning AndAlso IO.File.Exists(SharedProperties.Instance.Player.Path) AndAlso parameter IsNot Nothing AndAlso TypeOf parameter Is UPnP.UPnPProvider AndAlso CType(parameter, UPnP.UPnPProvider).SelectedRenderer IsNot Nothing
            End Function
        End Class
        ''' <summary>
        ''' Use: add or delete as command parameter
        ''' </summary>
        Public Class DelegateFirewallControlRuleCommand
            Implements ICommand

            Public Event CanExecuteChanged As EventHandler Implements ICommand.CanExecuteChanged

            Sub New()
                AddHandler CommandManager.RequerySuggested, New EventHandler(Sub(sender As Object, e As EventArgs)
                                                                                 RaiseEvent CanExecuteChanged(Me, New EventArgs())
                                                                             End Sub)
            End Sub
            'add rule: netsh advfirewall firewall add rule name="QuickBeat Music Discovery" dir=in action=allow protocol=TCP localport=80
            'delete rule: netsh advfirewall firewall delete rule name="QuickBeat Music Discovery"
            Public Sub Execute(parameter As Object) Implements ICommand.Execute
                Dim PSI As New ProcessStartInfo With {.FileName = "cmd", .UseShellExecute = True, .Verb = "runas"}
                Select Case parameter?.ToString
                    Case "add"
                        If SharedProperties.Instance.HTTPFileServer Is Nothing Then
                            PSI.Arguments = $"{If(My.Computer.Keyboard.CtrlKeyDown, "/k", "/c")} ""netsh advfirewall firewall add rule name=""QuickBeat Music Discovery"" dir=in action=allow protocol=TCP localport=8080"""
                        Else
                            PSI.Arguments = $"{If(My.Computer.Keyboard.CtrlKeyDown, "/k", "/c")} ""netsh advfirewall firewall add rule name=""QuickBeat Music Discovery"" dir=in action=allow protocol=TCP localport={SharedProperties.Instance.HTTPFileServer.Port}"""
                        End If
                    Case "delete"
                        PSI.Arguments = $"{If(My.Computer.Keyboard.CtrlKeyDown, "/k", "/c")} ""netsh advfirewall firewall delete rule name=""QuickBeat Music Discovery"""""
                End Select
                Try
                    Process.Start(PSI).WaitForExit(3000)
                Catch _ex As Win32Exception
                    If _ex.NativeErrorCode = 1223 Then
                        HandyControl.Controls.MessageBox.Show("The Command Requires Admin Privilge, Please Allow it In the UAC prompt.", "Really ?", icon:=MessageBoxImage.Error)
                        Return
                    End If
                End Try
            End Sub

            Public Function CanExecute(parameter As Object) As Boolean Implements ICommand.CanExecute
                Return parameter IsNot Nothing AndAlso (parameter.ToString = "add" OrElse parameter.ToString = "delete")
            End Function
        End Class
        Public Class DelegateRefreshCommand
            Implements ICommand

            Public Event CanExecuteChanged As EventHandler Implements ICommand.CanExecuteChanged

            Sub New()
                AddHandler CommandManager.RequerySuggested, New EventHandler(Sub(sender As Object, e As EventArgs)
                                                                                 RaiseEvent CanExecuteChanged(Me, New EventArgs())
                                                                             End Sub)
            End Sub

            Public Sub Execute(parameter As Object) Implements ICommand.Execute
                TryCast(parameter, HttpFileServer)?.Cache(True)
            End Sub

            Public Function CanExecute(parameter As Object) As Boolean Implements ICommand.CanExecute
                Return parameter IsNot Nothing AndAlso TypeOf parameter Is HttpFileServer
            End Function
        End Class

        ReadOnly Property StartCommand As New DelegateStartCommand
        ReadOnly Property StopCommand As New DelegateStopCommand
        ReadOnly Property OpenInBrowserCommand As New DelegateOpenInBrowserCommand
        ReadOnly Property PushToRendererCommand As New DelegatePushToRendererCommand(Me)
        ReadOnly Property FirewallControlRuleCommand As New DelegateFirewallControlRuleCommand
        ReadOnly Property RefreshCommand As New DelegateRefreshCommand
#End Region

        Private _RootPaths As String()
        Public ReadOnly Property RootPaths As String()
            Get
                Return _RootPaths
            End Get
        End Property

        Private _IsErrored As Boolean
        Public ReadOnly Property IsErrored As Boolean
            Get
                Return _IsErrored
            End Get
        End Property

        Private _URL As String
        Public ReadOnly Property URL As String
            Get
                Return _URL
            End Get
        End Property

        Public ReadOnly Property IsRunning As Boolean
            Get
                Return If(http Is Nothing, False, http.IsListening)
            End Get
        End Property

        Private _API As Boolean
        Public Property API As Boolean
            Get
                Return _API
            End Get
            Set(value As Boolean)
                _API = value
                OnPropertyChanged()
            End Set
        End Property

        Private _IsBusy As Boolean = False
        Public Property IsBusy As Boolean
            Get
                Return _IsBusy
            End Get
            Set(value As Boolean)
                _IsBusy = value
                OnPropertyChanged()
            End Set
        End Property

        Private _Port As Integer = 80
        Public Property Port As String
            Get
                Return _Port
            End Get
            Set(value As String)
                _Port = value
                OnPropertyChanged()
            End Set
        End Property

        Private _QuickLink As New QuickLink
        Public ReadOnly Property QuickLink As QuickLink
            Get
                Return _QuickLink
            End Get
        End Property


        Private _ContentCache As New Dictionary(Of String, String)
        Private _NeedCache As Boolean = True
        Public Property Configuration As New StartupItemConfiguration("LAN Music Discovery") Implements IStartupItem.Configuration

        'Private Const bufferSize As Integer = 1024 * 512
        '512KB
        Private http As HttpListener
        Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged

        Protected Overridable Sub OnPropertyChanged(<Runtime.CompilerServices.CallerMemberName> Optional ByVal CallerName As String = Nothing)
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(CallerName))
        End Sub

        Public Sub New(ByVal rootPaths As String())
            _RootPaths = rootPaths
        End Sub

        Public Sub New(ByVal rootPaths As Specialized.StringCollection)
            Dim paths(rootPaths.Count - 1) As String
            rootPaths.CopyTo(paths, 0)
            _RootPaths = paths
        End Sub
        Public Sub Start()
            Configuration.SetStatus("Starting...", 25)
            If http IsNot Nothing Then
                http.Stop()
            End If
            If _NeedCache Then
                Configuration.SetStatus("Caching...", 50)
                Cache()
            End If
            Try
                http = New HttpListener()
                'firewall rule command: netsh advfirewall firewall add rule name="QuickBeat Music Discovery" dir=in action=allow protocol=TCP localport=80
                'netsh http add urlacl url=http://+:80/ user=everyone
                'to delete: 'netsh http delete urlacl url=http://+:80/
                http.Prefixes.Add($"http://+:{Port}/")
                http.Start()
                OnPropertyChanged(NameOf(IsRunning))
                _URL = Dns.GetHostEntry(My.Computer.Name).AddressList.LastOrDefault(Function(k) k.AddressFamily = Sockets.AddressFamily.InterNetwork AndAlso k.ToString.StartsWith("192.168"))?.ToString
                OnPropertyChanged(NameOf(URL))
            Catch ex As HttpListenerException
                Utilities.DebugMode.Instance.Log(Of HttpFileServer)(ex.ToString)
                OnPropertyChanged(NameOf(IsRunning))
                _IsErrored = True
                OnPropertyChanged(NameOf(IsErrored))
                If ex.ErrorCode = 5 Then
                    Configuration.SetError(True, New UnauthorizedAccessException("Access Denied, Please Reserve a URL."))
                    If HandyControl.Controls.MessageBox.Show("A Valid URL Reservation Must Be Added in Order for QuickBeat Music Discovery to Run Corretly", "Fatal Error", button:=MessageBoxButton.YesNo, icon:=MessageBoxImage.Error) = MessageBoxResult.Yes Then
                        Dim PSI As New ProcessStartInfo With {.FileName = "cmd", .UseShellExecute = True, .Verb = "runas", .Arguments = $"{If(My.Computer.Keyboard.CtrlKeyDown, "/k", "/c")} ""netsh http add urlacl url=http://+:{Port}/ user=everyone"""}
                        Try
                            Process.Start(PSI).WaitForExit(3000)
                        Catch _ex As Win32Exception
                            Utilities.DebugMode.Instance.Log(Of HttpFileServer)(ex.ToString)
                            If _ex.NativeErrorCode = 1223 Then
                                OnPropertyChanged(NameOf(IsRunning))
                                HandyControl.Controls.MessageBox.Show("The Command Requires Admin Privilge, Please Allow it In the UAC prompt.", "Really ?", icon:=MessageBoxImage.Error)
                                Return
                            End If
                        End Try
                        http = New HttpListener()
                        http.Prefixes.Add($"http://+:{Port}/")
                        http.Start()
                        OnPropertyChanged(NameOf(IsRunning))
                        _URL = Dns.GetHostEntry(My.Computer.Name).AddressList.FirstOrDefault(Function(k) k.ToString.StartsWith("192.168"))?.ToString
                        OnPropertyChanged(NameOf(URL))
                        _IsErrored = False
                        OnPropertyChanged(NameOf(IsErrored))
                        Configuration.SetError(False, Nothing)
                    Else
                        Return
                    End If
                End If
            End Try
            http.BeginGetContext(AddressOf requestWait, Nothing)
            Configuration.SetStatus("Listening on port 80...", 100)
        End Sub
        Public Sub Dispose() Implements System.IDisposable.Dispose
            Configuration.SetStatus("Stopping...", 25)
            http?.[Stop]()
            http = Nothing
            OnPropertyChanged(NameOf(IsRunning))
            Configuration.SetStatus("Stopped successfully", 100)
        End Sub
        Public Async Sub Cache(Optional Force As Boolean = False)
            If IsBusy Then Return
            If Not Force AndAlso Not _NeedCache Then Return
            IsBusy = True
            _ContentCache.Clear()
            Await Task.Run(Sub()
                               _ContentCache.Add("PlayPage", CommonFunctions.GetInternalFileContent("Classes/UPnP/HTML/PlayPage.html"))
                               _ContentCache.Add("Blank", CommonFunctions.GetInternalFileContent("Classes/UPnP/HTML/EmptyPage.html"))
                               _ContentCache.Add("BlankTablePage", CommonFunctions.GetInternalFileContent("Classes/UPnP/HTML/EmptyTablePage.html"))
                               _ContentCache.Add("AudioCardBody", CommonFunctions.GetInternalFileContent("Classes/UPnP/HTML/AudioCardBody.html"))
                               _ContentCache.Add("AudioCardStyle", CommonFunctions.GetInternalFileContent("Classes/UPnP/HTML/AudioCardStyle.html"))
                               _ContentCache.Add("LandingPage", CommonFunctions.GetInternalFileContent("Classes/UPnP/HTML/LandingPage.html"))
                               For Each _dir In Me.RootPaths
                                   _ContentCache.Add(_dir, GenerateDirHTML(_dir))
                                   For Each _sdir In IO.Directory.GetDirectories(_dir, "*", SearchOption.AllDirectories)
                                       _ContentCache.Add(_sdir, GenerateDirHTML(_sdir))
                                   Next
                               Next
                           End Sub)
            IsBusy = False
            _NeedCache = False
        End Sub
        ''' <summary>
        ''' Returns the body of a premade audio card
        ''' </summary>
        ''' <param name="meta"></param>
        ''' <param name="TopLeftText"></param>
        ''' <param name="BottomLeftText"></param>
        ''' <remarks>
        ''' Corresponding style is required!
        ''' </remarks>
        ''' <returns></returns>
        Private Function GenerateCardHTML(meta As Metadata, Optional TopLeftText As String = "", Optional BottomLeftText As String = "") As String
            Return _ContentCache.Item("AudioCardBody").Replace("AUDIO_TITLE", meta.Title).Replace("AUDIO_ARTIST", meta.DefaultArtist).Replace("AUDIO_ALBUM", meta.Album).Replace("AUDIO_LENGTH", meta.LengthString).Replace("AUDIO_PLAYCOUNT", meta.PlayCount).Replace("AUDIO_GENRE", meta.JoinedGenres).Replace("AUDIO_YEAR", meta.Year).Replace("AUDIO_SRC", PathConvert(meta.Path) & "?play=true").Replace("IMG_SRC", PathConvert(meta.Path) & "?cover=true").Replace("LEFT_TOP_TEXT", TopLeftText).Replace("LEFT_BOTTOM_TEXT", BottomLeftText)
        End Function
        Private Function GenerateDirHTML(dirpath) As String
            Dim btp = _ContentCache.Item("BlankTablePage").Replace("PAGE_TITLE", "QuickBeat | " & dirpath).Replace("ROOT_URL", PathConvert(RootPaths.FirstOrDefault(Function(k) dirpath.StartsWith(k)))).Replace("ROOT_NAME", "Root")
            Dim _sw As New IO.StringWriter
            Dim _sdirs = IO.Directory.GetDirectories(dirpath, "*", SearchOption.TopDirectoryOnly)
            If _sdirs.Length > 0 Then
                _sw.WriteLine("<table>")
                _sw.WriteLine("<tr>")
                _sw.WriteLine("<th>Parent</th>")
                _sw.WriteLine("<th>Name</th>")
                _sw.WriteLine("</tr>")
                For Each path In _sdirs
                    _sw.WriteLine("<tr>")
                    _sw.WriteLine($"<td>{IO.Path.GetDirectoryName(path)}</td>")
                    _sw.WriteLine($"<td><a href=""/{ PathConvert(path)}"">{IO.Path.GetFileName(path)}</a></td>")
                    _sw.WriteLine("</tr>")
                Next
                _sw.WriteLine("</table>")
            End If
            Dim _files As IEnumerable(Of String) = CommonFunctions.GetFiles(dirpath)
            If _files IsNot Nothing AndAlso _files.Count > 0 Then
                _sw.WriteLine("<h2>Files</h2>")
                _sw.WriteLine("<table>")
                _sw.WriteLine("<tr>")
                _sw.WriteLine("<th>Title</th>")
                _sw.WriteLine("<th>Artist</th>")
                _sw.WriteLine("<th>Album</th>")
                _sw.WriteLine("<th>Link</th>")
                _sw.WriteLine("<th>Play in Browser</th>")
                _sw.WriteLine("</tr>")
                For Each file In _files
                    _sw.WriteLine("<tr>")
                    Dim meta = Player.Metadata.FromFile(file, True, True)
                    If meta Is Nothing Then
                        _sw.WriteLine($"<td>{IO.Path.GetFileName(file)}</td>")
                        _sw.WriteLine("<td>N/A</td>")
                        _sw.WriteLine("<td>N/A</td>")
                        _sw.WriteLine($"<td><a href=""/{ PathConvert(file)}"">➡</a></td>")
                    Else
                        _sw.WriteLine($"<td>{meta.Title}</td>")
                        _sw.WriteLine($"<td>{meta.JoinedArtists}</td>")
                        _sw.WriteLine($"<td>{meta.Album}</td>")
                        _sw.WriteLine($"<td><a href=""/{ PathConvert(file)}"">➡</a></td>")
                        _sw.WriteLine($"<td><a href=""/{ PathConvert(file)}?play=true"">🎵</a></td>")
                    End If
                    _sw.WriteLine("</tr>")
                Next
                _sw.WriteLine("</table>")
            End If
            Return btp.Insert(btp.IndexOf("</body>"), _sw.ToString)
        End Function
        Private Sub requestWait(ByVal ar As IAsyncResult)
            If http Is Nothing OrElse Not http.IsListening Then
                Return
            End If
            Dim c = http.EndGetContext(ar)
            http.BeginGetContext(AddressOf requestWait, Nothing)
            Utilities.DebugMode.Instance.Log(Of HttpFileServer)($"Incoming Request:  RemoteEndpoint:={c.Request.RemoteEndPoint.ToString},IsLocal:={c.Request.IsLocal},Method:={c.Request.HttpMethod},URL:={HttpUtility.UrlDecode(c.Request.RawUrl)}")
            Dim url, query As String
            If c.Request.Url.LocalPath.EndsWith("/") AndAlso Not String.IsNullOrWhiteSpace(c.Request.Url.Fragment) Then 'fragemented because of a file starting with "#"                
                url = tuneUrl(c.Request.Url.LocalPath & c.Request.Url.Fragment).Split("?").FirstOrDefault
                query = If(c.Request.Url.ToString.Contains("?"), c.Request.Url.ToString.Split("?").LastOrDefault, Nothing)
                If Not String.IsNullOrEmpty(query) Then query = query.Insert(0, "?")
            Else
                url = tuneUrl(c.Request.Url.LocalPath)
                query = c.Request.Url.Query
            End If
            'Dim fullPath = IIf(String.IsNullOrEmpty(url), RootPath, Path.Combine(RootPath, url))
            If String.IsNullOrEmpty(url) Then
                    returnHomePage(c)
                ElseIf url.StartsWith("api\quicklink\") Then
                    Dim params = If(String.IsNullOrEmpty(query), New Dictionary(Of String, String), query.ToLower.Remove(0, 1).Split("&").ToDictionary(Of String, String)(Function(k As String) k.Split("=").FirstOrDefault, Function(l As String) l.Split("=").LastOrDefault))
                    QuickLink.returnRequest(c, url, params)
                ElseIf url.StartsWith("api\") Then
                    If API Then
                        Dim params = query.ToLower.Remove(0, 1).Split("&").ToDictionary(Of String, String)(Function(k As String) k.Split("=").FirstOrDefault, Function(l As String) l.Split("=").LastOrDefault)
                        returnAPI(c, url, params)
                    Else
                        return403(c)
                    End If
                ElseIf url = "favicon.ico" Then
                    returnFavicon(c)
                ElseIf url = "logo.png" Then
                    returnLogo(c)
                ElseIf url = "search" Then
                    Dim params = query.ToLower.Remove(0, 1).Split("&").ToDictionary(Of String, String)(Function(k As String) k.Split("=").FirstOrDefault, Function(l As String) l.Split("=").LastOrDefault)
                    Dim qrt = If(params.ContainsKey("q"), params.Item("q"), If(params.ContainsKey("query"), params.Item("query"), Nothing))
                    If String.IsNullOrEmpty(qrt) Then
                        return404(c)
                    Else
                        returnSearch(c, HttpUtility.UrlDecode(qrt))
                    End If
                ElseIf String.IsNullOrEmpty(query) Then
                    If Directory.Exists(url) Then
                        Dim dir = RootPaths.FirstOrDefault(Function(k) url.StartsWith(k))
                        If dir Is Nothing Then
                            return403(c)
                        Else
                            returnDirContents(c, url)
                        End If
                    ElseIf File.Exists(url) Then
                        Dim dir = RootPaths.FirstOrDefault(Function(k) url.StartsWith(k))
                        If dir Is Nothing Then
                            return403(c)
                        Else
                            returnFile(c, url)
                        End If
                    End If
                ElseIf File.Exists(url) Then
                    Dim params = query.ToLower.Remove(0, 1).Split("&").ToDictionary(Of String, String)(Function(k As String) k.Split("=").FirstOrDefault, Function(l As String) l.Split("=").LastOrDefault)
                    If params.ContainsKey("play") Then
                        returnPlayPage(c, url)
                    ElseIf params.ContainsKey("cover") Then
                        returnCover(c, url, If(params.ContainsKey("index"), params.Item("index"), 0))
                    Else
                        return404(c)
                    End If
                Else
                    Utilities.DebugMode.Instance.Log(Of HttpFileServer)("Couldn't process request, returning 404")
                    return404(c)
                End If
        End Sub
        Private Sub returnDirContents(ByVal context As HttpListenerContext, ByVal dirPath As String)
            context.Response.ContentType = "text/html"
            context.Response.ContentEncoding = Encoding.Unicode
            Dim _dir = If(_ContentCache.ContainsKey(dirPath), _ContentCache.Item(dirPath), Nothing)
            If _dir Is Nothing Then
                Dim dHTML = GenerateDirHTML(dirPath)
                _ContentCache.Add(dirPath, dHTML)
                _dir = dHTML
            End If
            If Not String.IsNullOrEmpty(_dir) Then
                Using sw = New StreamWriter(context.Response.OutputStream)
                    Try
                        sw.Write(_dir)
                    Catch ex As Exception
                        context.Response.Abort()
                        Utilities.DebugMode.Instance.Log(Of HttpFileServer)(ex.ToString)
                    End Try
                End Using
            End If
            context.Response.OutputStream.Close()
        End Sub
        Private Async Sub returnSearch(ByVal context As HttpListenerContext, ByVal query As String)
            context.Response.ContentType = "text/html"
            context.Response.ContentEncoding = Encoding.Unicode
            Dim result = Await SharedProperties.Instance.Library.SearchReturn(query)
            If result Is Nothing Then 'busy
                context.Response.StatusCode = 500
            Else
                Dim btp = _ContentCache.Item("BlankTablePage").Replace("PAGE_TITLE", "QuickBeat | Search - " & query).Replace("ROOT_URL", "#").Replace("ROOT_NAME", "Root")
                btp = btp.Insert(btp.IndexOf("</style>"), _ContentCache.Item("AudioCardStyle"))
                Dim _sw As New IO.StringWriter
                _sw.WriteLine($"<i>Showing result for: {query}</i>")
                'top match
                If result.TopTrackMatch?.Item2 IsNot Nothing Then
                    '_sw.WriteLine("<h2>Top Match</h2>")
                    _sw.WriteLine(GenerateCardHTML(result.TopTrackMatch.Item2, "Top Match 🏆", Math.Round(result.TopTrackMatch.Item1, 2)))
                    If False Then
                        _sw.WriteLine("<table>")
                        _sw.WriteLine("<tr>")
                        _sw.WriteLine("<th>Score</th>")
                        _sw.WriteLine("<th>Title</th>")
                        _sw.WriteLine("<th>Artist</th>")
                        _sw.WriteLine("<th>Album</th>")
                        _sw.WriteLine("<th>Link</th>")
                        _sw.WriteLine("<th>Play in Browser</th>")
                        _sw.WriteLine("</tr>")
                        _sw.WriteLine("<tr>")
                        _sw.WriteLine($"<td>{Math.Round(result.TopTrackMatch.Item1, 2)}</td>")
                        _sw.WriteLine($"<td>{result.TopTrackMatch.Item2.Title}</td>")
                        _sw.WriteLine($"<td>{result.TopTrackMatch.Item2.JoinedArtists}</td>")
                        _sw.WriteLine($"<td>{result.TopTrackMatch.Item2.Album}</td>")
                        _sw.WriteLine($"<td><a href=""/{ PathConvert(result.TopTrackMatch.Item2.Path)}"">➡</a></td>")
                        _sw.WriteLine($"<td><a href=""/{ PathConvert(result.TopTrackMatch.Item2.Path)}?play=true"">🎵</a></td>")
                        _sw.WriteLine("</tr>")
                        _sw.WriteLine("</table>")
                    End If
                End If
                'other
                If result.TrackMatches.Count > 0 Then
                    _sw.WriteLine("<h2>Result</h2>")
                    _sw.WriteLine("<table>")
                    _sw.WriteLine("<tr>")
                    _sw.WriteLine("<th>Score</th>")
                    _sw.WriteLine("<th>Title</th>")
                    _sw.WriteLine("<th>Artist</th>")
                    _sw.WriteLine("<th>Album</th>")
                    _sw.WriteLine("<th>Link</th>")
                    _sw.WriteLine("<th>Play in Browser</th>")
                    _sw.WriteLine("</tr>")
                    For Each file In result.TrackMatches
                        If file.Item2 Is Nothing Then Continue For
                        _sw.WriteLine("<tr>")
                        _sw.WriteLine($"<td>{Math.Round(file.Item1, 2)}</td>")
                        _sw.WriteLine($"<td>{file.Item2.Title}</td>")
                        _sw.WriteLine($"<td>{file.Item2.JoinedArtists}</td>")
                        _sw.WriteLine($"<td>{file.Item2.Album}</td>")
                        _sw.WriteLine($"<td><a href=""/{ PathConvert(file.Item2.Path)}"">➡</a></td>")
                        _sw.WriteLine($"<td><a href=""/{ PathConvert(file.Item2.Path)}?play=true"">🎵</a></td>")
                        _sw.WriteLine("</tr>")
                    Next
                    _sw.WriteLine("</table>")
                Else
                    _sw.WriteLine("<h2>Nothing Here!</h2>")
                End If
                btp = btp.Insert(btp.IndexOf("</body>"), _sw.ToString)
                Using sw As New StreamWriter(context.Response.OutputStream)
                    Try
                        sw.Write(btp)
                    Catch ex As Exception
                        context.Response.Abort()
                        Utilities.DebugMode.Instance.Log(Of HttpFileServer)(ex.ToString)
                    End Try
                End Using
            End If
            context.Response.OutputStream.Close()
        End Sub
        Private Sub returnCover(ByVal context As HttpListenerContext, ByVal filePath As String, Optional index As Integer = 0)
            Dim tag = Utilities.SharedProperties.Instance.RequestTags(filePath)
            If tag.Tag.Pictures.Length = 0 Then
                context.Response.ContentType = getcontentType(".html")
                context.Response.StatusCode = 404
                context.Response.OutputStream.Close()
                Return
            ElseIf index >= tag.Tag.Pictures.Length Then
                context.Response.ContentType = getcontentType(".html")
                context.Response.StatusCode = 400
                context.Response.OutputStream.Close()
                Return
            End If
            Dim pic = tag.Tag.Pictures(index)
            context.Response.ContentType = pic.MimeType
            context.Response.AppendHeader("X-Content-Type-Options", "nosniff")
            context.Response.AppendHeader("Content-Disposition", $"attachment; filename=""cover.{pic.MimeType.Split("/").LastOrDefault}""")
            context.Response.ContentLength64 = pic.Data.Count
            Try
                context.Response.OutputStream.Write(pic.Data.Data, 0, pic.Data.Count)
            Catch ex As Exception
                context.Response.Abort()
                Utilities.DebugMode.Instance.Log(Of HttpFileServer)(ex.ToString)
            End Try
            context.Response.OutputStream.Close()
        End Sub
        Private Sub returnFile(ByVal context As HttpListenerContext, ByVal filePath As String)
            context.Response.ContentType = getcontentType(Path.GetExtension(filePath))
            context.Response.AppendHeader("X-Content-Type-Options", "nosniff")
            context.Response.AppendHeader("Content-Disposition", $"attachment; filename=""{System.Web.HttpUtility.UrlEncode(IO.Path.GetFileName(filePath))}""")
            Dim buffer As Byte() ' = New Byte(bufferSize - 1) {}
            Using fs = File.OpenRead(filePath)
                Dim Range = context.Request.Headers.Item("Range")
                If Range IsNot Nothing Then
                    Dim sI, eI As Integer : sI = -1 : eI = -1
                    Dim sRange = Range.Remove(0, "bytes=".Length).Split(New Char() {"-"}, StringSplitOptions.RemoveEmptyEntries)
                    If sRange.Length >= 1 Then sI = sRange(0)
                    If sRange.Length >= 2 Then eI = sRange(1)
                    If eI = -1 Then eI = fs.Length - 1
                    buffer = New Byte(eI - sI) {}
                    fs.Position = sI
                    context.Response.StatusCode = 206
                    context.Response.KeepAlive = True
                    context.Response.SendChunked = True
                    context.Response.ContentLength64 = eI - sI + 1
                    context.Response.AppendHeader("Accept-Ranges", "bytes")
                    context.Response.AppendHeader("Content-Range", $"bytes {sI}-{eI}/{fs.Length}")
                Else
                    buffer = New Byte(fs.Length - 1) {}
                    context.Response.ContentLength64 = buffer.Length
                End If
                If buffer.Length > 0 Then
                    Try
                        Dim read = fs.Read(buffer, 0, buffer.Length)
                        context.Response.OutputStream.Write(buffer, 0, read)
                    Catch ex As Exception
                        context.Response.Abort()
                        Return
                    End Try
                End If
            End Using
            context.Response.OutputStream.Close()
        End Sub
        Private Sub returnFavicon(ByVal context As HttpListenerContext)
            context.Response.ContentType = getcontentType(".ico")
            context.Response.AppendHeader("X-Content-Type-Options", "nosniff")
            context.Response.AppendHeader("Content-Disposition", $"attachment; filename=""favicon.ico""")
            Dim ResStream = Application.GetResourceStream(New Uri("Resources/MusicRecordIcon.ico", UriKind.Relative))
            If ResStream Is Nothing Then
                context.Response.StatusCode = 404
                Return
            End If
            Dim buffer As Byte()
            Using fs = ResStream.Stream
                Dim Range = context.Request.Headers.Item("Range")
                If Range IsNot Nothing Then
                    Dim sI, eI As Integer : sI = -1 : eI = -1
                    Dim sRange = Range.Remove(0, "bytes=".Length).Split(New Char() {"-"}, StringSplitOptions.RemoveEmptyEntries)
                    If sRange.Length >= 1 Then sI = sRange(0)
                    If sRange.Length >= 2 Then eI = sRange(1)
                    If eI = -1 Then eI = fs.Length - 1
                    buffer = New Byte(eI - sI) {}
                    fs.Position = sI
                    context.Response.StatusCode = 206
                    context.Response.KeepAlive = True
                    context.Response.SendChunked = True
                    context.Response.ContentLength64 = eI - sI + 1
                    context.Response.AppendHeader("Accept-Ranges", "bytes")
                    context.Response.AppendHeader("Content-Range", $"bytes {sI}-{eI}/{fs.Length}")
                Else
                    buffer = New Byte(fs.Length - 1) {}
                    context.Response.ContentLength64 = buffer.Length
                End If
                If buffer.Length > 0 Then
                    Try
                        Dim read = fs.Read(buffer, 0, buffer.Length)
                        context.Response.OutputStream.Write(buffer, 0, read)
                    Catch ex As Exception
                        context.Response.Abort()
                        Return
                    End Try
                End If
            End Using
            context.Response.OutputStream.Close()
        End Sub
        Private Sub returnLogo(ByVal context As HttpListenerContext)
            context.Response.ContentType = getcontentType(".png")
            context.Response.AppendHeader("X-Content-Type-Options", "nosniff")
            context.Response.AppendHeader("Content-Disposition", "attachment; filename=""logo.png""")
            Dim ResStream = Application.GetResourceStream(New Uri("Resources/MusicRecord.png", UriKind.Relative))
            If ResStream Is Nothing Then
                context.Response.StatusCode = 404
                Return
            End If
            Dim buffer As Byte()
            Using fs = ResStream.Stream
                Dim Range = context.Request.Headers.Item("Range")
                If Range IsNot Nothing Then
                    Dim sI, eI As Integer : sI = -1 : eI = -1
                    Dim sRange = Range.Remove(0, "bytes=".Length).Split(New Char() {"-"}, StringSplitOptions.RemoveEmptyEntries)
                    If sRange.Length >= 1 Then sI = sRange(0)
                    If sRange.Length >= 2 Then eI = sRange(1)
                    If eI = -1 Then eI = fs.Length - 1
                    buffer = New Byte(eI - sI) {}
                    fs.Position = sI
                    context.Response.StatusCode = 206
                    context.Response.KeepAlive = True
                    context.Response.SendChunked = True
                    context.Response.ContentLength64 = eI - sI + 1
                    context.Response.AppendHeader("Accept-Ranges", "bytes")
                    context.Response.AppendHeader("Content-Range", $"bytes {sI}-{eI}/{fs.Length}")
                Else
                    buffer = New Byte(fs.Length - 1) {}
                    context.Response.ContentLength64 = buffer.Length
                End If
                If buffer.Length > 0 Then
                    Try
                        Dim read = fs.Read(buffer, 0, buffer.Length)
                        context.Response.OutputStream.Write(buffer, 0, read)
                    Catch ex As Exception
                        context.Response.Abort()
                        Return
                    End Try
                End If
            End Using
            context.Response.OutputStream.Close()
        End Sub
        Private Sub returnPlayPage(ByVal context As HttpListenerContext, path As String)
            context.Response.ContentType = "text/html"
            context.Response.ContentEncoding = Encoding.Unicode
            Dim meta = Player.Metadata.FromFile(path, True, True)
            Dim dirPath = IO.Path.GetDirectoryName(path)
            Dim RootPath = If(RootPaths.Contains(dirPath), "#", PathConvert(RootPaths.FirstOrDefault(Function(k) dirPath.StartsWith(k))))
            Using sw As New StreamWriter(context.Response.OutputStream)
                Try
                    sw.Write(_ContentCache.Item("PlayPage").Replace("PAGE_TITLE", $"{meta.Title} - {meta.DefaultArtist} - QuickBeat").Replace("ROOT_URL", RootPath).Replace("ROOT_NAME", "Root").Replace("AUDIO_SRC", PathConvert(path)).Replace("AUDIO_MIME", getcontentType(IO.Path.GetExtension(path))).Replace("IMG_SRC", PathConvert(path) & "?cover=true").Replace("AUDIO_TITLE", meta.Title).Replace("AUDIO_ARTIST", meta.DefaultArtist).Replace("AUDIO_ALBUM", meta.Album))
                Catch ex As Exception
                    context.Response.Abort()
                    Utilities.DebugMode.Instance.Log(Of HttpFileServer)(ex.ToString)
                End Try
            End Using
            context.Response.OutputStream.Close()
        End Sub
        Private Sub returnAPI(ByVal context As HttpListenerContext, tunedUrl As String, params As Dictionary(Of String, String))
            context.Response.ContentType = getcontentType(".json")
            context.Response.ContentEncoding = Encoding.UTF8
            Dim sReq = tunedUrl.Remove(0, "api\".Length).Split("\")
            Dim Resp As New IO.MemoryStream()
            Using jsw As New System.Text.Json.Utf8JsonWriter(Resp)
                jsw.WriteStartObject()
                Select Case sReq.FirstOrDefault.ToLower '44 endpoint total
                    Case "info"
                        jsw.WriteString("AppName", SharedProperties.AppName)
                        jsw.WriteString("AppCodeName", SharedProperties.AppCodeName)
                        jsw.WriteString("AppVersion", My.Application.Info.Version.ToString)
                        jsw.WriteString("AppCopyright", SharedProperties.AppCopyright)
                        jsw.WriteString("AppAuthor", SharedProperties.AppAuthor)
                        jsw.WriteString("AppAuthorGithub", SharedProperties.AppAuthorGithub)
                        jsw.WriteString("AppAuthorDiscord", SharedProperties.AppAuthorDiscord)
                        jsw.WriteString("AppAuthorMessage", SharedProperties.AppAuthorMessage)
                    Case "player"
                        Select Case sReq(1)
                            Case "controls" '9 endpoints
                                If params.ContainsKey("action") Then
                                    Select Case params.Item("action")
                                        Case "play"
                                            SharedProperties.Instance.Player.Play()
                                            context.Response.StatusCode = 204 'success, no content
                                        Case "pause"
                                            SharedProperties.Instance.Player.Pause()
                                            context.Response.StatusCode = 204
                                        Case "toggleplaypause"
                                            SharedProperties.Instance.Player.PlayPause()
                                            context.Response.StatusCode = 204
                                        Case "mute"
                                            SharedProperties.Instance.Player.IsMuted = True
                                            context.Response.StatusCode = 204
                                        Case "unmute"
                                            SharedProperties.Instance.Player.IsMuted = False
                                            context.Response.StatusCode = 204
                                        Case "togglemute"
                                            SharedProperties.Instance.Player.IsMuted = Not SharedProperties.Instance.Player.IsMuted
                                            context.Response.StatusCode = 204
                                        Case "volume"
                                            If params.ContainsKey("param") Then
                                                Dim dVol As Double
                                                If Double.TryParse(params.Item("param"), dVol) Then
                                                    Dim vol = SharedProperties.Instance.Player.Volume
                                                    SharedProperties.Instance.Player.Volume = params.Item("param")
                                                    If SharedProperties.Instance.Player.Volume = vol Then 'ill param
                                                        jsw.WriteString("result", "ill param, must be between 0 and 100")
                                                        context.Response.StatusCode = 400 'bad request
                                                    Else
                                                        context.Response.StatusCode = 204
                                                    End If
                                                Else
                                                    jsw.WriteString("result", "ill param, must be a double(float) between 0 and 100")
                                                    context.Response.StatusCode = 400
                                                End If
                                            Else
                                                jsw.WriteString("result", SharedProperties.Instance.Player.Volume.ToString)
                                            End If
                                        Case "loop"
                                            If params.ContainsKey("param") Then
                                                Dim res As Boolean
                                                If Boolean.TryParse(params.Item("param"), res) Then
                                                    SharedProperties.Instance.Player.IsLooping = res
                                                    context.Response.StatusCode = 204
                                                Else
                                                    jsw.WriteString("result", "ill param, must be of a boolean type")
                                                    context.Response.StatusCode = 400
                                                End If
                                            Else
                                                jsw.WriteString("result", SharedProperties.Instance.Player.IsLooping.ToString)
                                            End If
                                        Case "seek"
                                            If params.ContainsKey("param") Then
                                                Dim pos As Double
                                                If Double.TryParse(params.Item("param"), pos) Then
                                                    SharedProperties.Instance.Player.Position = pos
                                                    context.Response.StatusCode = 204
                                                Else
                                                    jsw.WriteString("result", "ill param, must be of a double(float) type")
                                                    context.Response.StatusCode = 400
                                                End If
                                            Else
                                                jsw.WriteString("result", SharedProperties.Instance.Player.Position.ToString)
                                            End If
                                        Case Else
                                            context.Response.StatusCode = 404
                                            jsw.WriteString("result", "not_implemented")
                                    End Select
                                Else
                                    context.Response.StatusCode = 404
                                    jsw.WriteString("result", "not_implemented")
                                End If
                            Case "property" '5 endpoints
                                If params.ContainsKey("type") Then
                                    Select Case params.Item("type")
                                        Case "state"
                                            jsw.WriteString("result", If(SharedProperties.Instance.Player.IsPlaying, "playing",
                                                            If(SharedProperties.Instance.Player.IsPaused, "paused",
                                                            If(SharedProperties.Instance.Player.IsStopped, "stopped",
                                                            If(SharedProperties.Instance.Player.IsStalled, "stalled",
                                                            If(SharedProperties.Instance.Player.IsTransitioning, "transistion", "unknown"))))))
                                        Case "position"
                                            jsw.WriteString("result", SharedProperties.Instance.Player.Position.ToString)
                                        Case "positionstring"
                                            jsw.WriteString("result", SharedProperties.Instance.Player.PositionString)
                                        Case "duration"
                                            jsw.WriteString("result", SharedProperties.Instance.Player.Length.ToString)
                                        Case "durationstring"
                                            jsw.WriteString("result", If(SharedProperties.Instance.Player.StreamMetadata?.LengthString, "not_available"))
                                        Case Else
                                            context.Response.StatusCode = 404
                                            jsw.WriteString("result", "not_implemented")
                                    End Select
                                Else
                                    context.Response.StatusCode = 404
                                    jsw.WriteString("result", "not_implemented")
                                End If
                            Case Else
                                context.Response.StatusCode = 404
                                jsw.WriteString("result", "not_implemented")
                        End Select
                    Case "playlist"
                        Select Case sReq(1)
                            Case "controls" '9 endpoints
                                If params.ContainsKey("action") Then
                                    Select Case params.Item("action")
                                        Case "add"
                                            If params.ContainsKey("param") Then
                                                Dim path As String = HttpUtility.UrlDecode(params.Item("param"))
                                                If IO.File.Exists(path) Then
                                                    If params.ContainsKey("index") Then
                                                        Dim i As Integer
                                                        If Integer.TryParse(params.Item("index"), i) Then
                                                            If i < 0 OrElse i > SharedProperties.Instance.Player.Playlist.Count Then
                                                                jsw.WriteString("result", "ill param, must be a zero-based integer value >= 0 and <= playlist count")
                                                                context.Response.StatusCode = 400
                                                            Else
                                                                Application.Current.Dispatcher.Invoke(Sub()
                                                                                                          SharedProperties.Instance.Player.Playlist.Insert(i, Player.Metadata.FromFile(path))
                                                                                                      End Sub)
                                                                context.Response.StatusCode = 204
                                                            End If
                                                        Else
                                                            jsw.WriteString("result", "ill param, index must be an integer value")
                                                            context.Response.StatusCode = 400
                                                        End If
                                                    Else
                                                        Application.Current.Dispatcher.Invoke(Sub()
                                                                                                  SharedProperties.Instance.Player.Playlist.Add(Player.Metadata.FromFile(path))
                                                                                              End Sub)
                                                    End If
                                                    context.Response.StatusCode = 204
                                                Else
                                                    jsw.WriteString("result", "ill param, file doesn't exist")
                                                    context.Response.StatusCode = 400
                                                End If
                                            Else
                                                jsw.WriteString("result", "param required, must be of type string and a valid file path, you can use index (integer) to control where the item is added")
                                            End If
                                        Case "move"
                                            If params.ContainsKey("param") Then
                                                Dim iFrom, iTo As Double
                                                If Integer.TryParse(params.Item("param").Split(",").FirstOrDefault, iFrom) AndAlso Integer.TryParse(params.Item("param").Split(",").LastOrDefault, iTo) Then
                                                    Application.Current.Dispatcher.Invoke(Sub()
                                                                                              SharedProperties.Instance.Player.Playlist.Move(iFrom, iTo)
                                                                                          End Sub)
                                                    context.Response.StatusCode = 204
                                                Else
                                                    jsw.WriteString("result", "ill param, must be two integer values separated by a comma "",""")
                                                    context.Response.StatusCode = 400
                                                End If
                                            Else
                                                jsw.WriteString("result", "param required, must be two integer values separated by a comma "",""")
                                            End If
                                        Case "removeat"
                                            If params.ContainsKey("param") Then
                                                Dim i As Integer
                                                If Integer.TryParse(params.Item("param"), i) Then
                                                    If i < 0 OrElse i >= SharedProperties.Instance.Player.Playlist.Count Then
                                                        jsw.WriteString("result", "ill param, must be a zero-based integer value >= 0 and < playlist count")
                                                        context.Response.StatusCode = 400
                                                    Else
                                                        Application.Current.Dispatcher.Invoke(Sub()
                                                                                                  SharedProperties.Instance.Player.Playlist.RemoveAt(i)
                                                                                              End Sub)
                                                        context.Response.StatusCode = 204
                                                    End If
                                                Else
                                                    jsw.WriteString("result", "ill param, index must be an integer value")
                                                    context.Response.StatusCode = 400
                                                End If
                                            Else
                                                jsw.WriteString("result", "param required, must be two integer values separated by a comma "",""")
                                            End If
                                        Case "index"
                                            If params.ContainsKey("param") Then
                                                Dim i As Integer
                                                If Integer.TryParse(params.Item("param"), i) Then
                                                    If i < 0 OrElse i >= SharedProperties.Instance.Player.Playlist.Count Then
                                                        jsw.WriteString("result", "ill param, must be a zero-based integer value >= 0 and < playlist count")
                                                        context.Response.StatusCode = 400
                                                    Else
                                                        Application.Current.Dispatcher.Invoke(Sub()
                                                                                                  SharedProperties.Instance.Player.Playlist.Index = i
                                                                                              End Sub)
                                                        context.Response.StatusCode = 204
                                                    End If
                                                Else
                                                    jsw.WriteString("result", "ill param, index must be an integer value")
                                                    context.Response.StatusCode = 400
                                                End If
                                            Else
                                                jsw.WriteString("result", SharedProperties.Instance.Player.Playlist.Index.ToString)
                                            End If
                                        Case "clear"
                                            Application.Current.Dispatcher.Invoke(Sub()
                                                                                      SharedProperties.Instance.Player.Playlist.Clear()
                                                                                  End Sub)
                                            context.Response.StatusCode = 204
                                        Case "next"
                                            Application.Current.Dispatcher.Invoke(Sub()
                                                                                      SharedProperties.Instance.Player.Playlist.Next()
                                                                                  End Sub)
                                            context.Response.StatusCode = 204
                                        Case "previous"
                                            Application.Current.Dispatcher.Invoke(Sub()
                                                                                      SharedProperties.Instance.Player.Playlist.Previous()
                                                                                  End Sub)
                                            context.Response.StatusCode = 204
                                        Case "isshuffling"
                                            If params.ContainsKey("param") Then
                                                Dim bVal As Boolean
                                                If Boolean.TryParse(params.Item("param"), bVal) Then
                                                    SharedProperties.Instance.Player.Playlist.IsShuffling = bVal
                                                    context.Response.StatusCode = 204
                                                Else
                                                    jsw.WriteString("result", "ill param, must be boolean value")
                                                    context.Response.StatusCode = 400
                                                End If
                                            Else
                                                jsw.WriteString("result", SharedProperties.Instance.Player.Playlist.IsShuffling.ToString)
                                            End If
                                        Case "islooping"
                                            If params.ContainsKey("param") Then
                                                Dim bVal As Boolean
                                                If Boolean.TryParse(params.Item("param"), bVal) Then
                                                    SharedProperties.Instance.Player.Playlist.IsLooping = bVal
                                                    context.Response.StatusCode = 204
                                                Else
                                                    jsw.WriteString("result", "ill param, must be boolean value")
                                                    context.Response.StatusCode = 400
                                                End If
                                            Else
                                                jsw.WriteString("result", SharedProperties.Instance.Player.Playlist.IsLooping.ToString)
                                            End If
                                        Case Else
                                            context.Response.StatusCode = 404
                                            jsw.WriteString("result", "not_implemented")
                                    End Select
                                Else
                                    context.Response.StatusCode = 404
                                    jsw.WriteString("result", "not_implemented")
                                End If
                            Case "property" '11 endpoints
                                If params.ContainsKey("type") Then
                                    Select Case params.Item("type")
                                        Case "category"
                                            jsw.WriteString("result", SharedProperties.Instance.Player.Playlist.Category)
                                        Case "count"
                                            jsw.WriteString("result", SharedProperties.Instance.Player.Playlist.Count.ToString)
                                        Case "currentitem"
                                            If SharedProperties.Instance.Player.Playlist.CurrentItem Is Nothing Then
                                                jsw.WriteString("result", "current item is nothing for some reason, probably tampering with dev console")
                                            Else
                                                jsw.WriteString("title", SharedProperties.Instance.Player.Playlist.CurrentItem.Title)
                                                jsw.WriteString("artist", SharedProperties.Instance.Player.Playlist.CurrentItem.JoinedArtists)
                                                jsw.WriteString("album", SharedProperties.Instance.Player.Playlist.CurrentItem.Album)
                                                jsw.WriteString("genres", SharedProperties.Instance.Player.Playlist.CurrentItem.JoinedGenres)
                                                jsw.WriteString("year", SharedProperties.Instance.Player.Playlist.CurrentItem.Year.ToString)
                                                jsw.WriteString("hascover", SharedProperties.Instance.Player.Playlist.CurrentItem.HasCover.ToString)
                                                jsw.WriteString("isfavorite", SharedProperties.Instance.Player.Playlist.CurrentItem.IsFavorite.ToString)
                                                jsw.WriteString("duration", SharedProperties.Instance.Player.Playlist.CurrentItem.Length.ToString)
                                                jsw.WriteString("durationstring", SharedProperties.Instance.Player.Playlist.CurrentItem.LengthString)
                                                jsw.WriteString("location", SharedProperties.Instance.Player.Playlist.CurrentItem.Location.ToString)
                                                jsw.WriteString("path", SharedProperties.Instance.Player.Playlist.CurrentItem.Path)
                                                jsw.WriteString("playcount", SharedProperties.Instance.Player.Playlist.CurrentItem.PlayCount.ToString)
                                                jsw.WriteString("provider", SharedProperties.Instance.Player.Playlist.CurrentItem.Provider?.Name)
                                            End If
                                        Case "nextitem"
                                            If SharedProperties.Instance.Player.Playlist.NextItem Is Nothing Then
                                                jsw.WriteString("result", "next item is nothing, probably at end of playlist and loop is off")
                                            Else
                                                jsw.WriteString("title", SharedProperties.Instance.Player.Playlist.NextItem.Title)
                                                jsw.WriteString("artist", SharedProperties.Instance.Player.Playlist.NextItem.JoinedArtists)
                                                jsw.WriteString("album", SharedProperties.Instance.Player.Playlist.NextItem.Album)
                                                jsw.WriteString("genres", SharedProperties.Instance.Player.Playlist.NextItem.JoinedGenres)
                                                jsw.WriteString("year", SharedProperties.Instance.Player.Playlist.NextItem.Year.ToString)
                                                jsw.WriteString("hascover", SharedProperties.Instance.Player.Playlist.NextItem.HasCover.ToString)
                                                jsw.WriteString("isfavorite", SharedProperties.Instance.Player.Playlist.NextItem.IsFavorite.ToString)
                                                jsw.WriteString("duration", SharedProperties.Instance.Player.Playlist.NextItem.Length.ToString)
                                                jsw.WriteString("durationstring", SharedProperties.Instance.Player.Playlist.NextItem.LengthString)
                                                jsw.WriteString("location", SharedProperties.Instance.Player.Playlist.NextItem.Location.ToString)
                                                jsw.WriteString("path", SharedProperties.Instance.Player.Playlist.NextItem.Path)
                                                jsw.WriteString("playcount", SharedProperties.Instance.Player.Playlist.NextItem.PlayCount.ToString)
                                                jsw.WriteString("provider", SharedProperties.Instance.Player.Playlist.NextItem.Provider?.Name)
                                            End If
                                        Case "previousitem"
                                            If SharedProperties.Instance.Player.Playlist.PreviousItem Is Nothing Then
                                                jsw.WriteString("result", "previous item is nothing, probably at start of playlist and loop is off")
                                            Else
                                                jsw.WriteString("title", SharedProperties.Instance.Player.Playlist.PreviousItem.Title)
                                                jsw.WriteString("artist", SharedProperties.Instance.Player.Playlist.PreviousItem.JoinedArtists)
                                                jsw.WriteString("album", SharedProperties.Instance.Player.Playlist.PreviousItem.Album)
                                                jsw.WriteString("genres", SharedProperties.Instance.Player.Playlist.PreviousItem.JoinedGenres)
                                                jsw.WriteString("year", SharedProperties.Instance.Player.Playlist.PreviousItem.Year.ToString)
                                                jsw.WriteString("hascover", SharedProperties.Instance.Player.Playlist.PreviousItem.HasCover.ToString)
                                                jsw.WriteString("isfavorite", SharedProperties.Instance.Player.Playlist.PreviousItem.IsFavorite.ToString)
                                                jsw.WriteString("duration", SharedProperties.Instance.Player.Playlist.PreviousItem.Length.ToString)
                                                jsw.WriteString("durationstring", SharedProperties.Instance.Player.Playlist.PreviousItem.LengthString)
                                                jsw.WriteString("location", SharedProperties.Instance.Player.Playlist.PreviousItem.Location.ToString)
                                                jsw.WriteString("path", SharedProperties.Instance.Player.Playlist.PreviousItem.Path)
                                                jsw.WriteString("playcount", SharedProperties.Instance.Player.Playlist.PreviousItem.PlayCount.ToString)
                                                jsw.WriteString("provider", SharedProperties.Instance.Player.Playlist.PreviousItem.Provider?.Name)
                                            End If
                                        Case "description"
                                            jsw.WriteString("result", SharedProperties.Instance.Player.Playlist.Description)
                                        Case "hasitems"
                                            jsw.WriteString("result", SharedProperties.Instance.Player.Playlist.HasItems.ToString)
                                        Case "hasqueueitems"
                                            jsw.WriteString("result", SharedProperties.Instance.Player.Playlist.HasQueueItems.ToString)
                                        Case "name"
                                            jsw.WriteString("result", SharedProperties.Instance.Player.Playlist.Name)
                                        Case "totalduration"
                                            jsw.WriteString("result", SharedProperties.Instance.Player.Playlist.TotalDuration.TotalSeconds.ToString)
                                        Case "totaldurationstring"
                                            jsw.WriteString("result", SharedProperties.Instance.Player.Playlist.TotalDurationString)
                                        Case Else
                                            context.Response.StatusCode = 404
                                            jsw.WriteString("result", "not_implemented")
                                    End Select
                                Else
                                    context.Response.StatusCode = 404
                                    jsw.WriteString("result", "not_implemented")
                                End If
                            Case Else
                                context.Response.StatusCode = 404
                                jsw.WriteString("result", "not_implemented")
                        End Select
                    Case Else
                        context.Response.StatusCode = 404
                        jsw.WriteString("result", "not_implemented")
                End Select
                jsw.WriteEndObject()
            End Using
            context.Response.ContentLength64 = Resp.Length
            Using sw = New StreamWriter(context.Response.OutputStream)
                Try
                    sw.Write(System.Text.Encoding.UTF8.GetString(Resp.ToArray))
                Catch ex As Exception
                    context.Response.Abort()
                    Utilities.DebugMode.Instance.Log(Of HttpFileServer)(ex.ToString)
                End Try
            End Using
            context.Response.Close()
        End Sub
        Private Sub returnHomePage(ByVal context As HttpListenerContext)
            context.Response.ContentType = "text/html"
            context.Response.ContentEncoding = Encoding.Unicode
            Dim HomePage = _ContentCache.Item("LandingPage").Replace("PAGE_TITLE", "QuickBeat | Home")
            Dim _sw As New IO.StringWriter
            For Each path In RootPaths
                _sw.WriteLine("<tr>")
                _sw.WriteLine($"<td>{IO.Path.GetDirectoryName(path)}</td>")
                _sw.WriteLine($"<td><a href=""/{ PathConvert(path)}"">{IO.Path.GetFileName(path)}</a></td>")
                _sw.WriteLine("</tr>")
            Next
            HomePage = HomePage.Insert(HomePage.IndexOf("</table>"), _sw.ToString)
            Using sw = New StreamWriter(context.Response.OutputStream)
                Try
                    sw.Write(HomePage)
                Catch ex As Exception
                    context.Response.Abort()
                    Utilities.DebugMode.Instance.Log(Of HttpFileServer)(ex.ToString)
                End Try
            End Using
            context.Response.Close()
        End Sub
        Private Sub return404(ByVal context As HttpListenerContext)
            context.Response.StatusCode = 404
            context.Response.Close()
        End Sub
        Private Sub return403(ByVal context As HttpListenerContext)
            context.Response.StatusCode = 403
            context.Response.Close()
        End Sub
#Region "EX"
        Private Sub returnDirContentsEX(ByVal context As HttpListenerContext, ByVal dirPath As String)
            context.Response.ContentType = "text/html"
            context.Response.ContentEncoding = Encoding.Unicode
            Using sw = New StreamWriter(context.Response.OutputStream)
                sw.WriteLine("<html>")
                sw.WriteLine("<head>")
                sw.WriteLine("<meta charset=""utf-16"">")
                sw.WriteLine("<title>QuickBeat Music Discovery</title>")
                'sw.WriteLine("<style>")
                'sw.WriteLine("table, th, td {")
                'sw.WriteLine("border: 1px solid;")
                'sw.WriteLine("}")
                'sw.WriteLine("</style>")
                Dim style = CommonFunctions.GetInternalFileContent("Classes/UPnP/HTML/TableStyle.html")
                sw.WriteLine("<style>")
                sw.WriteLine("html *
{   
   font-family: Verdana;
}")
                sw.WriteLine(style)
                sw.WriteLine("</style>")
                sw.WriteLine("<body>")
                sw.WriteLine("<h1>QuickBeat</h1>")
                sw.WriteLine($"<h3><a href=""/"">Home</a><br>")
                If Not RootPaths.Contains(dirPath) Then
                    sw.WriteLine($"<a href=""/{ PathConvert(RootPaths.FirstOrDefault(Function(k) dirPath.StartsWith(k)))}"">Root</a><br>")
                    If RootPaths.Contains(IO.Path.GetDirectoryName(dirPath)) Then
                        sw.WriteLine("</h3><br>")
                    Else
                        Dim DirName = IO.Path.GetDirectoryName(dirPath)
                        If DirName IsNot Nothing Then sw.WriteLine($"<a href=""/{ PathConvert(DirName)}"">Go Back</a></h3><br>")
                    End If
                End If
                Dim dirs = Directory.GetDirectories(dirPath)
                If dirs.Length > 0 Then
                    sw.WriteLine("<h2>Directories</h2>")
                    sw.WriteLine("<table>")
                    sw.WriteLine("<tr>")
                    sw.WriteLine("<th style=""text-align:left"">Parent</th>")
                    sw.WriteLine("<th style=""text-align:left"">Name</th>")
                    sw.WriteLine("</tr>")
                    For Each directory In dirs
                        'sw.WriteLine($"<a href=""/{directory.Replace("\"c, "/"c)}"">{IO.Path.GetFileName(directory)}</a><br>")
                        sw.WriteLine("<tr>")
                        sw.WriteLine($"<td><a href=""/{ PathConvert(directory)}"">{IO.Path.GetFileName(directory)}</a></td>")
                        sw.WriteLine($"<td>{IO.Path.GetDirectoryName(directory)}</td>")
                        sw.WriteLine("</tr>")
                    Next
                    sw.WriteLine("</table>")
                End If
                Dim files = GetFiles(dirPath) 'Directory.GetFiles(dirPath)                
                sw.WriteLine("<h2>Files</h2>")
                sw.WriteLine("<table>")
                sw.WriteLine("<tr>")
                sw.WriteLine("<th style=""text-align:left"">Title</th>")
                sw.WriteLine("<th style=""text-align:left"">Artist</th>")
                sw.WriteLine("<th style=""text-align:left"">Album</th>")
                sw.WriteLine("<th style=""text-align:left"">Link</th>")
                sw.WriteLine("<th style=""text-align:left"">Play in Browser</th>")
                sw.WriteLine("</tr>")
                For Each file In files
                    sw.WriteLine("<tr>")
                    'sw.WriteLine($"<a href=""/{file.Replace("\"c, "/"c)}"">{IO.Path.GetFileName(file)}</a><br>")
                    Dim meta = Player.Metadata.FromFile(file, True, True)
                    If meta Is Nothing Then
                        sw.WriteLine($"<td>{IO.Path.GetFileName(file)}</td>")
                        sw.WriteLine("<td>N/A</td>")
                        sw.WriteLine("<td>N/A</td>")
                        sw.WriteLine($"<td><a href=""/{ PathConvert(file)}"">➡</a></td>")
                    Else
                        sw.WriteLine($"<td>{meta.Title}</td>")
                        sw.WriteLine($"<td>{meta.JoinedArtists}</td>")
                        sw.WriteLine($"<td>{meta.Album}</td>")
                        sw.WriteLine($"<td><a href=""/{ PathConvert(file)}"">➡</a></td>")
                        sw.WriteLine($"<td><a href=""/{ PathConvert(file)}?play=true"">🎵</a></td>")
                    End If
                    sw.WriteLine("</tr>")
                Next
                sw.WriteLine("</table>")
                sw.WriteLine("</body>")
                sw.WriteLine("</html>")
            End Using
            context.Response.OutputStream.Close()
        End Sub
        Private Sub returnPlayPageEX(ByVal context As HttpListenerContext, path As String)
            context.Response.ContentType = "text/html"
            context.Response.ContentEncoding = Encoding.Unicode
            Using sw = New StreamWriter(context.Response.OutputStream)
                sw.WriteLine("<html>")
                sw.WriteLine("<head>")
                sw.WriteLine("<meta charset=""utf-16"">")
                sw.WriteLine("<title>QuickBeat Music Discovery</title>")
                sw.WriteLine("<body>")
                sw.WriteLine("<h1>QuickBeat</h1>")
                sw.WriteLine($"<h3><a href=""/"">Home</a><br>")
                sw.WriteLine("<table>")
                sw.WriteLine("<tr>")
                sw.WriteLine("<th style=""text-align:left"">Title</th>")
                sw.WriteLine("<th style=""text-align:left"">Artist</th>")
                sw.WriteLine("<th style=""text-align:left"">Album</th>")
                sw.WriteLine("<tr>")
                'sw.WriteLine($"<a href=""/{file.Replace("\"c, "/"c)}"">{IO.Path.GetFileName(file)}</a><br>")
                Dim meta = Player.Metadata.FromFile(path, True, True)
                If meta Is Nothing Then
                    sw.WriteLine($"<td>{IO.Path.GetFileName(path)}</td>")
                    sw.WriteLine("<td>N/A</td>")
                    sw.WriteLine("<td>N/A</td>")
                    sw.WriteLine($"<td><a href=""/{ PathConvert(path)}"">➡</a></td>")
                Else
                    sw.WriteLine($"<td>{meta.Title}</td>")
                    sw.WriteLine($"<td>{meta.JoinedArtists}</td>")
                    sw.WriteLine($"<td>{meta.Album}</td>")
                End If
                sw.WriteLine("</tr>")
                sw.WriteLine("</table>")
                sw.WriteLine("<audio controls autoplay>")
                sw.WriteLine($"<source src=""{PathConvert(path)}"" type=""{getcontentType(IO.Path.GetExtension(path))}""")
                sw.WriteLine("</audio>")
                sw.WriteLine("</body>")
                sw.WriteLine("</html>")
            End Using
            context.Response.OutputStream.Close()
        End Sub
        Private Sub returnHomePageEX(ByVal context As HttpListenerContext)
            context.Response.ContentType = "text/html"
            context.Response.ContentEncoding = Encoding.Unicode
            Using sw = New StreamWriter(context.Response.OutputStream)
                sw.WriteLine("<html>")
                sw.WriteLine("<head>")
                sw.WriteLine("<meta charset=""utf-16"">")
                sw.WriteLine("<title>QuickBeat Music Discovery</title>")
                sw.WriteLine("<body>")
                sw.WriteLine("<h1>QuickBeat</h1>")
                sw.WriteLine("<h1>Home Page</h1>")
                sw.WriteLine("<h2>Directories</h2>")
                sw.WriteLine("<table>")
                sw.WriteLine("<tr>")
                sw.WriteLine("<th style=""text-align:left"">Parent</th>")
                sw.WriteLine("<th style=""text-align:left"">Name</th>")
                sw.WriteLine("</tr>")
                For Each path In RootPaths
                    sw.WriteLine("<tr>")
                    sw.WriteLine($"<td>{IO.Path.GetDirectoryName(path)}</td>")
                    sw.WriteLine($"<td><a href=""/{ PathConvert(path)}"">{IO.Path.GetFileName(path)}</a></td>")
                    sw.WriteLine("</tr>")
                Next
                sw.WriteLine("</table>")
                sw.WriteLine("</body>")
                sw.WriteLine("</html>")
            End Using
            context.Response.OutputStream.Close()
        End Sub
#End Region
        Private Shared Function tuneUrl(ByVal url As String) As String
            url = HttpUtility.UrlDecode(url, Encoding.UTF8)
            url = url.Replace("/"c, "\"c)
            url = url.Substring(1)
            Return url
        End Function
        Private Function PathConvert(ByVal path As String) As String
            Return HttpUtility.UrlEncode(path.Replace("/"c, "\"c))
        End Function
        Public Shared Function getcontentType(ByVal extension As String) As String
            Select Case extension
                Case ".avi"
                    Return "video/x-msvideo"
                Case ".css"
                    Return "text/css"
                Case ".doc"
                    Return "application/msword"
                Case ".gif"
                    Return "image/gif"
                Case ".htm", ".html"
                    Return "text/html"
                Case ".ico"
                    Return "image/x-icon"
                Case ".jpg", ".jpeg"
                    Return "image/jpeg"
                Case ".js"
                    Return "application/x-javascript"
                Case ".mp3"
                    Return "audio/mpeg"
                Case ".flac"
                    Return "audio/flac"
                Case ".png"
                    Return "image/png"
                Case ".pdf"
                    Return "application/pdf"
                Case ".ppt"
                    Return "application/vnd.ms-powerpoint"
                Case ".zip"
                    Return "application/zip"
                Case ".txt"
                    Return "text/plain"
                Case ".json"
                    Return "application/json"
                Case ".xml"
                    Return "application/xml"
                Case Else
                    Return "application/octet-stream"
            End Select
        End Function
        Private Shared Function InlineAssignHelper(Of T)(ByRef target As T, ByVal value As T) As T
            target = value
            Return value
        End Function

        Public Sub Init() Implements IStartupItem.Init
            Utilities.SharedProperties.Instance?.ItemsConfiguration.Add(Configuration)
            Configuration.SetStatus("All Good!", 100)
            QuickLink.Init()
        End Sub
    End Class
End Namespace