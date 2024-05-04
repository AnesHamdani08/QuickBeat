Imports System.ComponentModel
Imports System.IO
Imports System.Net
Imports QuickBeat.Classes
Imports QuickBeat.Interfaces
Imports QuickBeat.Utilities

Namespace UPnP
    Public Class QuickLink
        Implements ComponentModel.INotifyPropertyChanged, Interfaces.IStartupItem
#Region "Classes"
        Public Class ChatInfo
            Public Property Message() As Object
            Public Property SenderId() As String
            Public Property Role() As HandyControl.Data.ChatRoleType
            Public Property Type() As HandyControl.Data.ChatMessageType
            Public Property Data() As Object
        End Class
#End Region
#Region "WPF Support"
        Public Class DelegateUnlinkCommand
            Implements ICommand

            Public Event CanExecuteChanged As EventHandler Implements ICommand.CanExecuteChanged

            Sub New()
                AddHandler CommandManager.RequerySuggested, New EventHandler(Sub(sender As Object, e As EventArgs)
                                                                                 RaiseEvent CanExecuteChanged(Me, New EventArgs())
                                                                             End Sub)
            End Sub

            Public Sub Execute(parameter As Object) Implements ICommand.Execute
                TryCast(parameter, QuickLink)?.UnLink()
            End Sub

            Public Function CanExecute(parameter As Object) As Boolean Implements ICommand.CanExecute
                Return parameter IsNot Nothing AndAlso TypeOf parameter Is QuickLink AndAlso CType(parameter, QuickLink).Status = LinkStatus.Linked
            End Function
        End Class
        Public Class DelegateScanCommand
            Implements ICommand

            Public Event CanExecuteChanged As EventHandler Implements ICommand.CanExecuteChanged

            Sub New()
                AddHandler CommandManager.RequerySuggested, New EventHandler(Sub(sender As Object, e As EventArgs)
                                                                                 RaiseEvent CanExecuteChanged(Me, New EventArgs())
                                                                             End Sub)
            End Sub

            Public Sub Execute(parameter As Object) Implements ICommand.Execute
                TryCast(parameter, QuickLink)?.Scan()
            End Sub

            Public Function CanExecute(parameter As Object) As Boolean Implements ICommand.CanExecute
                Return parameter IsNot Nothing AndAlso TypeOf parameter Is QuickLink AndAlso Not CType(parameter, QuickLink).IsScanning
            End Function
        End Class
        Public Class DelegateScanCancelCommand
            Implements ICommand

            Public Event CanExecuteChanged As EventHandler Implements ICommand.CanExecuteChanged

            Sub New()
                AddHandler CommandManager.RequerySuggested, New EventHandler(Sub(sender As Object, e As EventArgs)
                                                                                 RaiseEvent CanExecuteChanged(Me, New EventArgs())
                                                                             End Sub)
            End Sub

            Public Sub Execute(parameter As Object) Implements ICommand.Execute
                TryCast(parameter, QuickLink).StopScan = True
            End Sub

            Public Function CanExecute(parameter As Object) As Boolean Implements ICommand.CanExecute
                Return parameter IsNot Nothing AndAlso TypeOf parameter Is QuickLink AndAlso CType(parameter, QuickLink).IsScanning
            End Function
        End Class
        Public Class DelegateLinkCommand
            Implements ICommand

            Public Event CanExecuteChanged As EventHandler Implements ICommand.CanExecuteChanged
            Private _Parent As QuickLink

            Sub New(Parent As QuickLink)
                AddHandler CommandManager.RequerySuggested, New EventHandler(Sub(sender As Object, e As EventArgs)
                                                                                 RaiseEvent CanExecuteChanged(Me, New EventArgs())
                                                                             End Sub)
                _Parent = Parent
            End Sub

            Public Sub Execute(parameter As Object) Implements ICommand.Execute
                Dim sParam = parameter?.ToString
                sParam = sParam.Replace("http://", "")
                _Parent.Link(sParam.Substring(0, sParam.LastIndexOf(":")), sParam.Substring(sParam.LastIndexOf(":") + 1), True)
            End Sub

            Public Function CanExecute(parameter As Object) As Boolean Implements ICommand.CanExecute
                Return parameter IsNot Nothing
            End Function
        End Class
        Public Class DelegateClearNewChatCommand
            Implements ICommand

            Public Event CanExecuteChanged As EventHandler Implements ICommand.CanExecuteChanged

            Sub New()
                AddHandler CommandManager.RequerySuggested, New EventHandler(Sub(sender As Object, e As EventArgs)
                                                                                 RaiseEvent CanExecuteChanged(Me, New EventArgs())
                                                                             End Sub)
            End Sub

            Public Sub Execute(parameter As Object) Implements ICommand.Execute
                TryCast(parameter, QuickLink)?.ClearNewChat()
            End Sub

            Public Function CanExecute(parameter As Object) As Boolean Implements ICommand.CanExecute
                Return True
            End Function
        End Class
        Public Class DelegateSendMessageCommand
            Implements ICommand

            Public Event CanExecuteChanged As EventHandler Implements ICommand.CanExecuteChanged
            Private _Parent As QuickLink

            Sub New(Parent As QuickLink)
                AddHandler CommandManager.RequerySuggested, New EventHandler(Sub(sender As Object, e As EventArgs)
                                                                                 RaiseEvent CanExecuteChanged(Me, New EventArgs())
                                                                             End Sub)
                _Parent = Parent
            End Sub

            Public Sub Execute(parameter As Object) Implements ICommand.Execute
                _Parent.SendMessage(parameter)
            End Sub

            Public Function CanExecute(parameter As Object) As Boolean Implements ICommand.CanExecute
                Return _Parent.Status = LinkStatus.Linked
            End Function
        End Class
        Public Class DelegateSendAudioMessageCommand
            Implements ICommand

            Public Event CanExecuteChanged As EventHandler Implements ICommand.CanExecuteChanged

            Sub New()
                AddHandler CommandManager.RequerySuggested, New EventHandler(Sub(sender As Object, e As EventArgs)
                                                                                 RaiseEvent CanExecuteChanged(Me, New EventArgs())
                                                                             End Sub)
            End Sub

            Public Sub Execute(parameter As Object) Implements ICommand.Execute
                TryCast(parameter, QuickLink)?.SendAudio()
            End Sub

            Public Function CanExecute(parameter As Object) As Boolean Implements ICommand.CanExecute
                Return parameter IsNot Nothing AndAlso TypeOf parameter Is QuickLink AndAlso CType(parameter, QuickLink).Status = LinkStatus.Linked
            End Function
        End Class
#End Region

        Public Event ChatRecieved(Sender As IPAddress, Message As ChatInfo)
        Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged

        Public Sub OnPropertyChanged(<Runtime.CompilerServices.CallerMemberName> Optional CallerName As String = Nothing)
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(CallerName))
        End Sub
#Region "Properties"

        Private _Status As LinkStatus
        Property Status As LinkStatus
            Get
                Return _Status
            End Get
            Set(value As LinkStatus)
                _Status = value
                OnPropertyChanged()
                OnPropertyChanged(NameOf(IsLinked))
                OnPropertyChanged(NameOf(IsControlling))
                OnPropertyChanged(NameOf(IsControlled))
            End Set
        End Property

        Private _LinkedTo As String
        Property LinkedTo As String
            Get
                Return _LinkedTo
            End Get
            Set(value As String)
                _LinkedTo = value
                OnPropertyChanged()
                OnPropertyChanged(NameOf(IsControlling))
                OnPropertyChanged(NameOf(IsControlled))
            End Set
        End Property

        Private _LinkedToAddress As String
        Property LinkedToAddress As String
            Get
                Return _LinkedToAddress
            End Get
            Set(value As String)
                _LinkedToAddress = value
                OnPropertyChanged()
                OnPropertyChanged(NameOf(IsControlling))
                OnPropertyChanged(NameOf(IsControlled))
            End Set
        End Property

        ReadOnly Property IsLinked As Boolean
            Get
                Return Status = LinkStatus.Linked
            End Get
        End Property

        ReadOnly Property IsControlling As Boolean
            Get
                Return _LinkDirectionIsForward = TriState.True
            End Get
        End Property

        ReadOnly Property IsControlled As Boolean
            Get
                Return _LinkDirectionIsForward = TriState.False
            End Get
        End Property

        Private _IsScanning As Boolean
        ReadOnly Property IsScanning As Boolean
            Get
                Return _IsScanning
            End Get
        End Property

        Private _StopScan As Boolean
        Property StopScan As Boolean
            Get
                Return _StopScan
            End Get
            Set(value As Boolean)
                _StopScan = value
                OnPropertyChanged()
            End Set
        End Property

        Private _ScanTimeout As Integer = 10
        Property ScanTimeout As Integer
            Get
                Return _ScanTimeout
            End Get
            Set(value As Integer)
                _ScanTimeout = value
                OnPropertyChanged()
            End Set
        End Property

        Private _NewChat As Boolean = False
        ReadOnly Property NewChat As Boolean
            Get
                Return _NewChat
            End Get
        End Property

        Private _LockChatRead As Boolean
        ''' <summary>
        ''' Keeps <see cref="NewChat"/> set to False and marks every new chat as read
        ''' </summary>
        ''' <returns></returns>
        Property LockChatRead As Boolean
            Get
                Return _LockChatRead
            End Get
            Set(value As Boolean)
                _LockChatRead = value
                OnPropertyChanged()
            End Set
        End Property

        ''' <summary>
        ''' Suspend sync events
        ''' </summary>
        Private _LockSyncEvents As Boolean
        Property LockSyncEvents As Boolean
            Get
                Return _LockSyncEvents
            End Get
            Set(value As Boolean)
                _LockSyncEvents = value
                OnPropertyChanged()
            End Set
        End Property

        ReadOnly Property FoundServers As New ObjectModel.ObservableCollection(Of Tuple(Of String, String))
        ReadOnly Property Chat As New ObjectModel.ObservableCollection(Of ChatInfo)

        ReadOnly Property UnlinkCommand As New DelegateUnlinkCommand
        ReadOnly Property ScanCommand As New DelegateScanCommand
        ReadOnly Property ScanCancelCommand As New DelegateScanCancelCommand
        ReadOnly Property LinkCommand As New DelegateLinkCommand(Me)
        ReadOnly Property ClearNewChatCommand As New DelegateClearNewChatCommand
        ReadOnly Property SendMessageCommand As New DelegateSendMessageCommand(Me)
        ReadOnly Property SendAudioMessageCommand As New DelegateSendAudioMessageCommand

        Public Property Configuration As New StartupItemConfiguration("QuickLink") Implements IStartupItem.Configuration

        Private WithEvents _Player As Player.Player = Nothing
        Private WithEvents _PingTimer As New Forms.Timer With {.Interval = 2000} 'monitors connection
        Private _IsBusy As Boolean
        Private _LinkToken As Guid
        Private _LinkDirectionIsForward As TriState = TriState.UseDefault '<-False | Default | True->
#End Region
        Public Sub Init() Implements IStartupItem.Init
            _Player = SharedProperties.Instance.Player
            Utilities.SharedProperties.Instance?.ItemsConfiguration.Add(Configuration)
            Configuration.SetStatus("Up and Running", 100)
        End Sub

        Public Async Sub Scan()
            If _IsScanning Then Return
            _IsScanning = True
            OnPropertyChanged(NameOf(IsScanning))
            FoundServers.Clear()
            Using hc As New Net.Http.HttpClient With {.Timeout = TimeSpan.FromMilliseconds(ScanTimeout)}
                For i As Integer = 1 To 254
                    For j As Integer = 0 To 2
                        If _StopScan Then
                            StopScan = False
                            _IsScanning = False
                            OnPropertyChanged(NameOf(IsScanning))
                            Return
                        End If
                        Dim baseURL As String = $"http://192.168.1.{i}:{If(j = 0, "80", If(j = 1, "443", "8080"))}"
                        If baseURL = "http://" & SharedProperties.Instance.HTTPFileServer.URL & ":" & SharedProperties.Instance.HTTPFileServer.Port Then Continue For
                        Dim qlURI As New Uri($"{baseURL}/api/quicklink/hello")
                        Try
                            Dim Hresult = Await hc.GetAsync(qlURI)
                            If Hresult.StatusCode = HttpStatusCode.OK Then
                                Dim result = Await Hresult.Content.ReadAsStringAsync
                                If result.StartsWith("{") Then
                                    Dim r = System.Text.Json.JsonDocument.Parse(result)
                                    FoundServers.Add(New Tuple(Of String, String)(r.RootElement.GetProperty("name").GetString, baseURL))
                                End If
                            End If
                        Catch ex As Exception
                        End Try
                    Next
                Next
            End Using
            _IsScanning = False
            OnPropertyChanged(NameOf(IsScanning))
        End Sub

        Public Async Sub Link(Address As String, Port As String, Optional Control As Boolean = False)
            If _IsBusy OrElse Status = LinkStatus.Linking Then Return
            If Status = LinkStatus.Linked Then UnLink()
            _IsBusy = True : Status = LinkStatus.Linking
            Using hc As New Net.Http.HttpClient()
                Dim qlURI As New Uri($"http://{Address}:{Port}/api/quicklink/{If(Control, "linkcontrol", "link")}?name={If(My.Computer.Name, Utilities.SharedProperties.AppName)}&version={My.Application.Info.Version.ToString}")
                hc.DefaultRequestHeaders.Add("endpoint", SharedProperties.Instance.HTTPFileServer.URL & ":" & SharedProperties.Instance.HTTPFileServer.Port) 'bypass NAT
                Dim Hresult = Await hc.GetAsync(qlURI)
                If Hresult.StatusCode = HttpStatusCode.BadRequest OrElse Hresult.StatusCode = HttpStatusCode.Forbidden OrElse Hresult.StatusCode = HttpStatusCode.NotFound Then
                    Utilities.DebugMode.Instance.Log(Of QuickLink)("Couldn't Link to Address:=" & Address & ",Port:=" & Port & ",Code:=" & Hresult.StatusCode.ToString)
                    Return
                End If
                Dim result = Await Hresult.Content.ReadAsStringAsync()
                If Not result.StartsWith("{") Then
                    _IsBusy = False
                    _LinkDirectionIsForward = TriState.UseDefault
                    Status = LinkStatus.Unlinked
                    LinkedTo = ""
                    LinkedToAddress = ""
                    _LinkToken = Nothing
                    Return
                End If
                Dim j = System.Text.Json.JsonDocument.Parse(result)
                Dim jResult = j.RootElement.GetProperty("result")
                If jResult.GetBoolean Then
                    Dim jName = j.RootElement.GetProperty("name")
                    Dim jVersion = j.RootElement.GetProperty("version")
                    Dim jEndpoint = j.RootElement.GetProperty("endpoint")
                    Dim jToken = j.RootElement.GetProperty("token")
                    _LinkDirectionIsForward = If(Control, TriState.True, TriState.False)
                    Status = LinkStatus.Linked
                    LinkedTo = jName.GetString
                    LinkedToAddress = "http://" & jEndpoint.GetString '$"http://{Address}:{Port}"
                    _LinkToken = jToken.GetGuid
                    _PingTimer.Start()
                    Utilities.DebugMode.Instance.Log(Of QuickLink)($"Successfuly Linked. To:={LinkedTo},Address:={LinkedToAddress},Direction:={_LinkDirectionIsForward.ToString}")
                Else
                    _LinkDirectionIsForward = TriState.UseDefault
                    Status = LinkStatus.Unlinked
                    LinkedTo = ""
                    LinkedToAddress = ""
                    _LinkToken = Nothing
                End If
            End Using
            _IsBusy = False
        End Sub

        Public Async Sub UnLink()
            If String.IsNullOrEmpty(LinkedToAddress) Then
                Status = LinkStatus.Unlinked
                Return
            End If
            If Status <> LinkStatus.Linked Then Return
            Using hc As New Net.Http.HttpClient()
                Dim qlURI As New Uri($"{LinkedToAddress}/api/quicklink/unlink")
                hc.DefaultRequestHeaders.Authorization = New Http.Headers.AuthenticationHeaderValue("Bearer", _LinkToken.ToString)
                Try
                    Dim result = Await hc.GetStringAsync(qlURI)
                    If Not result.StartsWith("{") Then
                        _LinkDirectionIsForward = TriState.UseDefault
                        Status = LinkStatus.Unlinked
                        LinkedTo = ""
                        LinkedToAddress = ""
                        _LinkToken = Nothing
                        Utilities.DebugMode.Instance.Log(Of QuickLink)($"Failure. Unlinked. To:={LinkedTo},Address:={LinkedToAddress},Response:={result}")
                        Return
                    End If
                    Dim j = System.Text.Json.JsonDocument.Parse(result)
                    Dim jResult = j.RootElement.GetProperty("result")
                    If jResult.GetString = "good bye" Then 'success
                        _LinkDirectionIsForward = TriState.UseDefault
                        Status = LinkStatus.Unlinked
                        LinkedTo = ""
                        LinkedToAddress = ""
                        _LinkToken = Nothing
                        LockChatRead = False
                        Application.Current.Dispatcher.Invoke(Sub() Chat.Clear())
                        Utilities.DebugMode.Instance.Log(Of QuickLink)($"Successfuly UnLinked.")
                    End If
                Catch
                    _LinkDirectionIsForward = TriState.UseDefault
                    Status = LinkStatus.Unlinked
                    LinkedTo = ""
                    LinkedToAddress = ""
                    _LinkToken = Nothing
                    LockChatRead = False
                    Application.Current.Dispatcher.Invoke(Sub() Chat.Clear())
                    Utilities.DebugMode.Instance.Log(Of QuickLink)($"Failure. Unlinked. To:={LinkedTo},Address:={LinkedToAddress}")
                End Try
            End Using
            _PingTimer.Stop()
        End Sub

        Public Async Sub SendMessage(Message As String)
            If Status <> LinkStatus.Linked Then Return
            Using hc As New Net.Http.HttpClient()
                Dim qlURI As New Uri($"{LinkedToAddress}/api/quicklink/chat")
                hc.DefaultRequestHeaders.Authorization = New Http.Headers.AuthenticationHeaderValue("Bearer", _LinkToken.ToString)
                If String.IsNullOrEmpty(Message) Then
                    Dim ofd As New Microsoft.Win32.OpenFileDialog With {.Filter = "Image Files|*.bmp;*.jpg;*.jpeg;*.png;*.tiff;*.ico;*.wmp"}
                    If Not ofd.ShowDialog Then Return
                    Dim mem As New IO.MemoryStream
                    Dim enco As New PngBitmapEncoder()
                    enco.Frames.Add(BitmapFrame.Create(New Uri(ofd.FileName)))
                    enco.Save(mem)
                    'hc.DefaultRequestHeaders.Add("data", Convert.ToBase64String(mem.ToArray))
                    'hc.DefaultRequestHeaders.Add("message", ofd.FileName)
                    'hc.DefaultRequestHeaders.Add("type", CInt(HandyControl.Data.ChatMessageType.Image))
                    'Dim bData = mem.ToArray
                    'mem.Close()
                    'Dim data As New Http.ByteArrayContent(bData) 'Http.StreamContent(mem)
                    'Data.Headers.ContentLength = bData.Length
                    'Data.Headers.ContentType = New Http.Headers.MediaTypeHeaderValue(HttpFileServer.getcontentType(".png"))
                    'Data.Headers.ContentDisposition = Http.Headers.ContentDispositionHeaderValue.Parse($"attachment; filename=""{ofd.FileName}""")                    
                    Dim data As New Http.StringContent(ConstructMessageBody(Nothing, mem.ToArray, HandyControl.Data.ChatMessageType.Image), Text.Encoding.UTF8, HttpFileServer.getcontentType(".xml"))
                    Dim result = Await hc.PostAsync(qlURI, data)
                    If result.IsSuccessStatusCode Then
                        Application.Current.Dispatcher.Invoke(Sub() Chat.Add(New ChatInfo() With {.Message = BitmapFrame.Create(New Uri(ofd.FileName)),
                                                                             .Role = HandyControl.Data.ChatRoleType.Sender, .SenderId = My.Computer.Name,
                                                                             .Type = HandyControl.Data.ChatMessageType.Image,
                                                                             .Data = ofd.FileName}))
                    End If
                Else
                    'hc.DefaultRequestHeaders.Add("message", Message)
                    'hc.DefaultRequestHeaders.Add("type", CInt(HandyControl.Data.ChatMessageType.String))
                    Dim data As New Http.StringContent(ConstructMessageBody(Message, Nothing, HandyControl.Data.ChatMessageType.String), Text.Encoding.UTF8, HttpFileServer.getcontentType(".xml"))
                    Dim result = Await hc.PostAsync(qlURI, data)
                    If result.IsSuccessStatusCode Then
                        Application.Current.Dispatcher.Invoke(Sub() Chat.Add(New ChatInfo() With {.Message = Message, .Role = HandyControl.Data.ChatRoleType.Sender, .SenderId = My.Computer.Name, .Type = HandyControl.Data.ChatMessageType.String}))
                    End If
                End If
            End Using
        End Sub

        Public Async Sub SendAudio()
            If Status <> LinkStatus.Linked Then Return
            Using hc As New Net.Http.HttpClient()
                Dim qlURI As New Uri($"{LinkedToAddress}/api/quicklink/chat")
                hc.DefaultRequestHeaders.Authorization = New Http.Headers.AuthenticationHeaderValue("Bearer", _LinkToken.ToString)
                Dim ofd As New Dialogs.MetadataPicker
                ofd.Populate(SharedProperties.Instance.Library)
                If Not ofd.ShowDialog Then Return
                Dim content As New Http.StringContent(ConstructMessageBody(Nothing, ofd.DialogMetadataResult, HandyControl.Data.ChatMessageType.Audio), Text.Encoding.UTF8, HttpFileServer.getcontentType(".xml"))
                Dim result = Await hc.PostAsync(qlURI, content)
                If result.IsSuccessStatusCode Then
                    Application.Current.Dispatcher.Invoke(Sub() Chat.Add(New ChatInfo() With {.Message = $"{ofd.DialogMetadataResult.Title} - {ofd.DialogMetadataResult.DefaultArtist} ({ofd.DialogMetadataResult.LengthString})",
                                                                             .Role = HandyControl.Data.ChatRoleType.Sender, .SenderId = My.Computer.Name,
                                                                             .Type = HandyControl.Data.ChatMessageType.Audio,
                                                                             .Data = ofd.DialogMetadataResult.Path}))
                End If
            End Using
        End Sub

        Public Sub ClearNewChat()
            _NewChat = False
            OnPropertyChanged(NameOf(NewChat))
        End Sub

        Public Function GetLinkToken() As Guid
            Return _LinkToken
        End Function

        Public Function ConstructMessageBody(Message As String, Data As Object, Type As HandyControl.Data.ChatMessageType) As String
            Dim XDoc As New XDocument(New XDeclaration("1.0", "utf-8", Nothing))
            Dim XRoot As New XElement("Message")
            XRoot.SetAttributeValue("Type", CInt(Type))
            Dim XContent As New XElement("Content")
            XContent.Value = If(String.IsNullOrEmpty(Message), "", Xml.XmlConvert.EncodeName(Message))
            Dim XData As New XElement("Data")
            Select Case Type
                Case HandyControl.Data.ChatMessageType.Image
                    XData.Value = Convert.ToBase64String(Data) 'no need to convert , base64 string only uses alphanumeric , + , / and =
                Case HandyControl.Data.ChatMessageType.Audio
                    Dim BinF As New System.Runtime.Serialization.Formatters.Binary.BinaryFormatter
                    Dim mStream As New IO.MemoryStream
                    BinF.Serialize(mStream, CType(Data, Player.Metadata))
                    XData.Value = Convert.ToBase64String(mStream.ToArray)
            End Select
            XRoot.Add(XContent)
            XRoot.Add(XData)
            XDoc.Add(XRoot)
            Return XDoc.ToString
        End Function

        Public Function DeconstructMessageBody(Data As String, SenderID As String, Optional Role As HandyControl.Data.ChatRoleType = HandyControl.Data.ChatRoleType.Receiver) As ChatInfo
            If String.IsNullOrEmpty(Data) Then Return Nothing
            Dim XDoc = XDocument.Parse(Data)
            Dim CI As New ChatInfo() With {.SenderId = SenderID, .Role = Role}
            CI.Type = XDoc.Root.Attribute("Type").Value
            Select Case CI.Type
                Case HandyControl.Data.ChatMessageType.Image
                    Dim bData = Convert.FromBase64String(XDoc.Root.Element("Data").Value)
                    Dim dStream As New IO.MemoryStream(bData)
                    Dim deco As New PngBitmapDecoder(dStream, BitmapCreateOptions.None, BitmapCacheOption.Default)
                    Dim frm = deco.Frames.FirstOrDefault.Clone
                    CI.Message = frm
                Case HandyControl.Data.ChatMessageType.Audio
                    Dim BinF As New System.Runtime.Serialization.Formatters.Binary.BinaryFormatter
                    Dim bData = Convert.FromBase64String(XDoc.Root.Element("Data").Value)
                    Dim dStream As New IO.MemoryStream(bData)
                    Dim Meta As Player.Metadata = BinF.Deserialize(dStream)
                    CI.Message = $"{Meta.Title} - {Meta.DefaultArtist} ({Meta.LengthString})"
                    CI.Data = Meta
                Case HandyControl.Data.ChatMessageType.String
                    CI.Message = Xml.XmlConvert.DecodeName(XDoc.Root.Element("Content").Value)
            End Select
            Return CI
        End Function
#Region "Sync"
        Public Async Sub Sync()
            If Status <> LinkStatus.Linked Then Return
            If _LinkDirectionIsForward = TriState.True Then Return
            Using hc As New Net.Http.HttpClient()
                Dim qlURI As New Uri($"{LinkedToAddress}/api/quicklink/status")
                hc.DefaultRequestHeaders.Authorization = New Http.Headers.AuthenticationHeaderValue("Bearer", _LinkToken.ToString)
                Dim result = Await hc.GetStringAsync(qlURI)
                Dim j = System.Text.Json.JsonDocument.Parse(result)
                Dim jResult = j.RootElement.GetProperty("result")
                Dim jPosition = j.RootElement.GetProperty("position")
                Dim jUID = j.RootElement.GetProperty("UID")
                If SharedProperties.Instance.Player.StreamMetadata.UID <> jUID.GetString Then
                    'load file
                    Dim hMeta = Await hc.GetStringAsync($"{LinkedToAddress}/api/quicklink/control?action=currentitem")
                    Dim jMeta = System.Text.Json.JsonDocument.Parse(hMeta)
                    Dim meta As New Player.Metadata With {.Location = Player.Metadata.FileLocation.Remote, .LockUID = True, .BlockDOWNLOADPROC = True, .IsCoverLocked = True} 'shouldn't be cached
                    With meta
                        .Title = jMeta.RootElement.GetProperty("title").GetString
                        .Artists = jMeta.RootElement.GetProperty("artist").GetString?.Split(";")
                        .Album = jMeta.RootElement.GetProperty("album").GetString
                        .Genres = jMeta.RootElement.GetProperty("genres").GetString?.Split(";")
                        .Year = jMeta.RootElement.GetProperty("year").GetUInt32
                        .HasCover = jMeta.RootElement.GetProperty("hascover").GetBoolean
                        .Length = jMeta.RootElement.GetProperty("duration").GetDouble
                        .OriginalPath = "QuickLink://" & jUID.GetString
                        .Path = $"{LinkedToAddress}/api/quicklink/data"
                        .UID = jUID.GetString
                    End With
                    Await Application.Current.Dispatcher.Invoke(Async Function()
                                                                    Dim dCover As New BitmapImage
                                                                    Dim hCover = Await hc.GetAsync($"{LinkedToAddress}/api/quicklink/cover")
                                                                    Dim bCover = Await hCover.Content.ReadAsStreamAsync 'jpg encoded
                                                                    Dim jpgDeco As New JpegBitmapDecoder(bCover, BitmapCreateOptions.None, BitmapCacheOption.OnLoad)
                                                                    meta.Covers = New ImageSource() {jpgDeco.Frames.FirstOrDefault?.Clone()}
                                                                End Function)
                    Await Application.Current.Dispatcher.Invoke(Async Function() Await SharedProperties.Instance.Player.LoadSong(meta, New String() {"Authorization: Bearer " & _LinkToken.ToString}))
                End If
                Select Case jResult.GetString
                    Case "playing"
                        SharedProperties.Instance.Player.Play()
                        SharedProperties.Instance.Player.Position = jPosition.GetDouble
                    Case "paused", "stopped", "stalled"
                        SharedProperties.Instance.Player.Pause()
                    Case "transistion", "unknown"
                End Select
            End Using
        End Sub

        Public Async Sub SyncMedia()
            If Status <> LinkStatus.Linked Then Return
            If _LinkDirectionIsForward = TriState.True Then Return
            Using hc As New Net.Http.HttpClient()
                Dim qlURI As New Uri($"{LinkedToAddress}/api/quicklink/status")
                hc.DefaultRequestHeaders.Authorization = New Http.Headers.AuthenticationHeaderValue("Bearer", _LinkToken.ToString)
                Dim result = Await hc.GetStringAsync(qlURI)
                Dim j = System.Text.Json.JsonDocument.Parse(result)
                Dim jResult = j.RootElement.GetProperty("result")
                Dim jPosition = j.RootElement.GetProperty("position")
                Dim jUID = j.RootElement.GetProperty("UID")
                If SharedProperties.Instance.Player.StreamMetadata.UID <> jUID.GetString Then
                    'load file
                    Dim hMeta = Await hc.GetStringAsync($"{LinkedToAddress}/api/quicklink/control?action=currentitem")
                    Dim jMeta = System.Text.Json.JsonDocument.Parse(hMeta)
                    Dim meta As New Player.Metadata With {.Location = Player.Metadata.FileLocation.Remote, .LockUID = True, .BlockDOWNLOADPROC = True, .IsCoverLocked = True}
                    With meta
                        .Title = jMeta.RootElement.GetProperty("title").GetString
                        .Artists = jMeta.RootElement.GetProperty("artist").GetString?.Split(";")
                        .Album = jMeta.RootElement.GetProperty("album").GetString
                        .Genres = jMeta.RootElement.GetProperty("genres").GetString?.Split(";")
                        .Year = jMeta.RootElement.GetProperty("year").GetUInt32
                        .HasCover = jMeta.RootElement.GetProperty("hascover").GetBoolean
                        .Length = jMeta.RootElement.GetProperty("duration").GetDouble
                        .OriginalPath = "QuickLink://" & jUID.GetString
                        .Path = $"{LinkedToAddress}/api/quicklink/data"
                        .UID = jUID.GetString
                    End With
                    Await Application.Current.Dispatcher.Invoke(Async Function()
                                                                    Dim dCover As New BitmapImage
                                                                    Dim hCover = Await hc.GetAsync($"{LinkedToAddress}/api/quicklink/cover")
                                                                    Dim bCover = Await hCover.Content.ReadAsStreamAsync
                                                                    Try
                                                                        Dim jpgDeco As New JpegBitmapDecoder(bCover, BitmapCreateOptions.None, BitmapCacheOption.OnLoad)
                                                                        meta.Covers = New ImageSource() {jpgDeco.Frames.FirstOrDefault?.Clone()}
                                                                    Catch
                                                                    End Try
                                                                End Function)
                    Await Application.Current.Dispatcher.Invoke(Async Function() Await SharedProperties.Instance.Player.LoadSong(meta, New String() {"Authorization: Bearer " & _LinkToken.ToString}))
                End If
            End Using
        End Sub

        Public Async Sub SyncState()
            If Status <> LinkStatus.Linked Then Return
            If _LinkDirectionIsForward = TriState.True Then Return
            Using hc As New Net.Http.HttpClient()
                Dim qlURI As New Uri($"{LinkedToAddress}/api/quicklink/status")
                hc.DefaultRequestHeaders.Authorization = New Http.Headers.AuthenticationHeaderValue("Bearer", _LinkToken.ToString)
                Dim result = Await hc.GetStringAsync(qlURI)
                Dim j = System.Text.Json.JsonDocument.Parse(result)
                Dim jResult = j.RootElement.GetProperty("result")
                Dim jPosition = j.RootElement.GetProperty("position")
                Dim jUID = j.RootElement.GetProperty("UID")
                Select Case jResult.GetString
                    Case "playing"
                        SharedProperties.Instance.Player.Play()
                        SharedProperties.Instance.Player.Position = jPosition.GetDouble
                    Case "paused", "stopped", "stalled"
                        SharedProperties.Instance.Player.Pause()
                    Case "transistion", "unknown"
                End Select
            End Using
        End Sub

        Public Async Sub SyncPosition()
            If Status <> LinkStatus.Linked Then Return
            If _LinkDirectionIsForward = TriState.True Then Return
            Using hc As New Net.Http.HttpClient()
                Dim qlURI As New Uri($"{LinkedToAddress}/api/quicklink/status")
                hc.DefaultRequestHeaders.Authorization = New Http.Headers.AuthenticationHeaderValue("Bearer", _LinkToken.ToString)
                Dim result = Await hc.GetStringAsync(qlURI)
                Dim j = System.Text.Json.JsonDocument.Parse(result)
                Dim jResult = j.RootElement.GetProperty("result")
                Dim jPosition = j.RootElement.GetProperty("position")
                Dim jUID = j.RootElement.GetProperty("UID")
                SharedProperties.Instance.Player.Position = jPosition.GetDouble
            End Using
        End Sub
#End Region

#Region "Request Handling"
        Public Sub ReturnRequest(ByVal context As HttpListenerContext, tunedUrl As String, params As Dictionary(Of String, String))
            context.Response.ContentType = "application/json"
            context.Response.ContentEncoding = Text.Encoding.UTF8
            Dim sReq = tunedUrl.Remove(0, "api\quicklink\".Length).Split("\")
            Dim Bearer = context.Request.Headers.Get("Authorization")
            Dim Resp As New IO.MemoryStream()
            Using jsw As New System.Text.Json.Utf8JsonWriter(Resp)
                jsw.WriteStartObject()
                If sReq.FirstOrDefault.ToLower = "link" Then '1 endpoint total
                    Dim remoteEP = context.Request.Headers.Get("endpoint")
                    If String.IsNullOrEmpty(remoteEP) OrElse Not params.ContainsKey("name") OrElse Not params.ContainsKey("version") Then
                        context.Response.StatusCode = 400 'bad request
                        jsw.WriteString("result", "missing params")
                    Else
                        If Status = LinkStatus.Linked OrElse Status = LinkStatus.Linking Then
                            context.Response.StatusCode = 503 'service unavailable
                            jsw.WriteBoolean("result", False)
                        Else
                            jsw.WriteBoolean("result", True)
                            jsw.WriteString("name", If(My.Computer.Name, Utilities.SharedProperties.AppName))
                            jsw.WriteString("version", My.Application.Info.Version.ToString)
                            jsw.WriteString("endpoint", SharedProperties.Instance.HTTPFileServer.URL & ":" & SharedProperties.Instance.HTTPFileServer.Port)
                            _LinkDirectionIsForward = TriState.True
                            Status = LinkStatus.Linked
                            _LinkToken = Guid.NewGuid
                            LinkedTo = params.Item("name")
                            LinkedToAddress = "http://" & remoteEP 'context.Request.RemoteEndPoint.Port
                            _PingTimer.Start()
                            jsw.WriteString("token", _LinkToken)
                        End If
                    End If
                ElseIf sReq.FirstOrDefault.ToLower = "linkcontrol" Then '1 endpoint total
                    Dim remoteEP = context.Request.Headers.Get("endpoint")
                    If String.IsNullOrEmpty(remoteEP) OrElse Not params.ContainsKey("name") OrElse Not params.ContainsKey("version") Then
                        context.Response.StatusCode = 400 'bad request
                        jsw.WriteString("result", "missing params")
                    Else
                        If Status = LinkStatus.Linked OrElse Status = LinkStatus.Linking Then
                            context.Response.StatusCode = 503 'service unavailable
                            jsw.WriteBoolean("result", False)
                        Else
                            jsw.WriteBoolean("result", True)
                            jsw.WriteString("name", If(My.Computer.Name, Utilities.SharedProperties.AppName))
                            jsw.WriteString("version", My.Application.Info.Version.ToString)
                            jsw.WriteString("endpoint", SharedProperties.Instance.HTTPFileServer.URL & ":" & SharedProperties.Instance.HTTPFileServer.Port)
                            _LinkDirectionIsForward = TriState.False
                            Status = LinkStatus.Linked
                            _LinkToken = Guid.NewGuid
                            LinkedTo = params.Item("name")
                            LinkedToAddress = "http://" & remoteEP 'context.Request.RemoteEndPoint.Port
                            _PingTimer.Start()
                            jsw.WriteString("token", _LinkToken)
                        End If
                    End If
                ElseIf sReq.FirstOrDefault.ToLower = "hello" Then
                    jsw.WriteString("name", If(My.Computer.Name, Utilities.SharedProperties.AppName))
                Else
                    Dim BearerGUID As Guid = Guid.Empty
                    If Not String.IsNullOrEmpty(Bearer) Then Guid.TryParse(Bearer.Remove(0, "Bearer ".Length), BearerGUID)
                    If Not _LinkToken.Equals(BearerGUID) OrElse Status <> LinkStatus.Linked Then
                        context.Response.StatusCode = 401 'Unauthorized                                                
                        context.Response.AddHeader("WWW-Authenticate", "Bearer")
                    Else
                        Select Case sReq.FirstOrDefault.ToLower '10 endpoint total                                                
                            Case "unlink"
                                _LinkDirectionIsForward = TriState.UseDefault
                                Status = LinkStatus.Unlinked
                                LinkedTo = ""
                                LinkedToAddress = ""
                                _LinkToken = Nothing
                                LockChatRead = False
                                Application.Current.Dispatcher.Invoke(Sub() Chat.Clear())
                                jsw.WriteString("result", "good bye")
                            Case "data"
                                Select Case Utilities.SharedProperties.Instance.Player.StreamMetadata?.Location
                                    Case Player.Metadata.FileLocation.Local
                                        ReturnFile(context, Utilities.SharedProperties.Instance.Player.StreamMetadata.Path)
                                        Return
                                    Case Player.Metadata.FileLocation.Internal, Player.Metadata.FileLocation.Undefined
                                        context.Response.StatusCode = 500
                                    Case Player.Metadata.FileLocation.Cached
                                        Dim data = TryCast(SharedProperties.Instance.Player.StreamMetadata, Player.CachedMetadata)?.Data
                                        If data Is Nothing Then
                                            context.Response.StatusCode = 500
                                        Else
                                            Dim cData As New IO.MemoryStream(data.ToArray)
                                            ReturnFile(context, cData, If(SharedProperties.Instance.Player.StreamMetadata.Title, "output"), SharedProperties.Instance.Player.StreamMetadata.Extension)
                                            Return
                                        End If
                                    Case Player.Metadata.FileLocation.Remote
                                        ReturnFile(context, SharedProperties.Instance.Player.DumpDownloadedStream, If(SharedProperties.Instance.Player.StreamMetadata.Title, "output"), SharedProperties.Instance.Player.StreamMetadata.Extension)
                                        Return
                                End Select
                            Case "cover"
                                Dim fallback As Boolean = True
                                If params.ContainsKey("fallback") Then
                                    If params.Item("fallback").ToLower = "false" OrElse params.Item("fallback") = 0 OrElse params.Item("fallback").ToLower = "n" Then
                                        fallback = False
                                    End If
                                End If
                                If params.ContainsKey("disposition") Then
                                    Select Case params.Item("disposition")
                                        Case "inline"
                                            ReturnCover(context, IsInline:=True, FallBack:=fallback)
                                        Case "attachment" 'idk why i added this
                                            ReturnCover(context, FallBack:=fallback)
                                    End Select
                                Else
                                    ReturnCover(context)
                                End If
                                Return
                            Case "sync"
                                If sReq.Length < 2 Then
                                    context.Response.StatusCode = 401
                                Else
                                    Select Case sReq(1)
                                        Case "all"
                                            Sync()
                                        Case "media"
                                            SyncMedia()
                                        Case "state"
                                            SyncState()
                                        Case "position"
                                            SyncPosition()
                                    End Select
                                End If
                            Case "status"
                                jsw.WriteString("result", If(SharedProperties.Instance.Player.IsPlaying, "playing",
                                                             If(SharedProperties.Instance.Player.IsPaused, "paused",
                                                             If(SharedProperties.Instance.Player.IsStopped, "stopped",
                                                             If(SharedProperties.Instance.Player.IsStalled, "stalled",
                                                             If(SharedProperties.Instance.Player.IsTransitioning, "transistion", "unknown"))))))
                                jsw.WriteNumber("position", SharedProperties.Instance.Player.Position)
                                jsw.WriteString("UID", SharedProperties.Instance.Player.StreamMetadata.UID) 'used for testing if same file is playing
                            Case "control"
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
                                        Case "currentitem"
                                            If SharedProperties.Instance.Player.StreamMetadata Is Nothing Then
                                                jsw.WriteString("result", "current item is nothing for some reason, probably tampering with dev console")
                                            Else
                                                jsw.WriteString("title", SharedProperties.Instance.Player.StreamMetadata.Title)
                                                jsw.WriteString("artist", SharedProperties.Instance.Player.StreamMetadata.JoinedArtists)
                                                jsw.WriteString("album", SharedProperties.Instance.Player.StreamMetadata.Album)
                                                jsw.WriteString("genres", SharedProperties.Instance.Player.StreamMetadata.JoinedGenres)
                                                jsw.WriteNumber("year", SharedProperties.Instance.Player.StreamMetadata.Year)
                                                jsw.WriteBoolean("hascover", SharedProperties.Instance.Player.StreamMetadata.HasCover)
                                                jsw.WriteBoolean("isfavorite", SharedProperties.Instance.Player.StreamMetadata.IsFavorite)
                                                jsw.WriteNumber("duration", SharedProperties.Instance.Player.StreamMetadata.Length)
                                                jsw.WriteString("durationstring", SharedProperties.Instance.Player.StreamMetadata.LengthString)
                                                jsw.WriteString("location", SharedProperties.Instance.Player.StreamMetadata.Location.ToString)
                                                jsw.WriteString("path", SharedProperties.Instance.Player.StreamMetadata.Path)
                                                jsw.WriteNumber("playcount", SharedProperties.Instance.Player.StreamMetadata.PlayCount)
                                                jsw.WriteString("provider", SharedProperties.Instance.Player.StreamMetadata.Provider?.Name)
                                            End If
                                        Case Else
                                            context.Response.StatusCode = 404
                                            jsw.WriteString("result", "not_implemented")
                                    End Select
                                Else
                                    context.Response.StatusCode = 404
                                    jsw.WriteString("result", "not_implemented")
                                End If
                            Case "chat"
                                'Dim msg = context.Request.Headers.Get("message")
                                'Dim type As HandyControl.Data.ChatMessageType = HandyControl.Data.ChatMessageType.String
                                'Dim dStream As New IO.MemoryStream()
                                'context.Request.InputStream.CopyTo(dStream)
                                Using sr As New IO.StreamReader(context.Request.InputStream, Text.Encoding.UTF8)
                                    Dim xmlData = sr.ReadToEnd
                                    Application.Current.Dispatcher.Invoke(Sub()
                                                                              Dim CI = DeconstructMessageBody(xmlData, LinkedTo)
                                                                              Chat.Add(CI)
                                                                              'Chat.Add(New ChatInfo With {.Message = msg, .Type = Type, .Role = HandyControl.Data.ChatRoleType.Receiver, .SenderId = LinkedTo})
                                                                              If Not LockChatRead Then
                                                                                  _NewChat = True
                                                                                  OnPropertyChanged(NameOf(NewChat))
                                                                              End If
                                                                          End Sub)
                                End Using
                                'Integer.TryParse(context.Request.Headers.Get("type"), type)
                                'If type = HandyControl.Data.ChatMessageType.Audio Then
                                '    Dim data = context.Request.Headers.Get("data")
                                '    Application.Current.Dispatcher.Invoke(Sub()
                                '                                              Chat.Add(New ChatInfo With {.Message = msg, .Type = type, .Role = HandyControl.Data.ChatRoleType.Receiver, .SenderId = LinkedTo, .Data = data})
                                '                                              If Not LockChatUnread Then
                                '                                                  _NewChat = True
                                '                                                  OnPropertyChanged(NameOf(NewChat))
                                '                                              End If
                                '                                          End Sub)
                                'ElseIf type = HandyControl.Data.ChatMessageType.Image Then
                                '    Dim disp = context.Request.Headers.Get("Content-Disposition").Remove(0, "attachment; filename=""".Length).TrimEnd(""""c)
                                '    Application.Current.Dispatcher.Invoke(Sub()
                                '                                              If dStream Is Nothing Then Return
                                '                                              Dim deco As New PngBitmapDecoder(dStream, BitmapCreateOptions.None, BitmapCacheOption.Default)
                                '                                              Dim frm = deco.Frames.FirstOrDefault.Clone
                                '                                              Chat.Add(New ChatInfo With {.Message = frm, .Type = type, .Role = HandyControl.Data.ChatRoleType.Receiver, .SenderId = LinkedTo, .Data = disp})
                                '                                              If Not LockChatUnread Then
                                '                                                  _NewChat = True
                                '                                                  OnPropertyChanged(NameOf(NewChat))
                                '                                              End If
                                '                                          End Sub)
                                'Else

                                'End If
                            Case Else
                                context.Response.StatusCode = 404
                                jsw.WriteString("result", "not_implemented")
                        End Select
                    End If
                End If
                jsw.WriteNumber("status", context.Response.StatusCode)
                jsw.WriteEndObject()
            End Using
            context.Response.ContentLength64 = Resp.Length
            Using sw = New IO.StreamWriter(context.Response.OutputStream)
                Try
                    sw.Write(System.Text.Encoding.UTF8.GetString(Resp.ToArray))
                Catch ex As Exception
                    context.Response.Abort()
                    Utilities.DebugMode.Instance.Log(Of HttpFileServer)(ex.ToString)
                End Try
            End Using
            context.Response.Close()
        End Sub

        Private Sub ReturnFile(ByVal context As HttpListenerContext, ByVal filePath As String)
            context.Response.ContentType = HttpFileServer.getcontentType(IO.Path.GetExtension(filePath))
            context.Response.AppendHeader("X-Content-Type-Options", "nosniff")
            context.Response.AppendHeader("Content-Disposition", $"attachment; filename=""{System.Web.HttpUtility.UrlEncode(IO.Path.GetFileName(filePath))}""")
            Dim buffer As Byte() ' = New Byte(bufferSize - 1) {}
            Using fs = IO.File.OpenRead(filePath)
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

        Private Sub ReturnFile(ByVal context As HttpListenerContext, ByVal filedata As IO.Stream, safefilename As String, extension As String)
            context.Response.ContentType = HttpFileServer.getcontentType(IO.Path.GetExtension(extension))
            context.Response.AppendHeader("X-Content-Type-Options", "nosniff")
            context.Response.AppendHeader("Content-Disposition", $"attachment; filename=""{System.Web.HttpUtility.UrlEncode($"{safefilename}{extension}")}""")
            Dim buffer As Byte() ' = New Byte(bufferSize - 1) {}
            Using fs = filedata
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

        Private Sub ReturnCover(ByVal context As HttpListenerContext, Optional IsInline As Boolean = False, Optional FallBack As Boolean = True)
            Dim WasLocked As Boolean = False
            Application.Current.Dispatcher.Invoke(Sub()
                                                      WasLocked = SharedProperties.Instance.Player.StreamMetadata.IsCoverLocked
                                                      SharedProperties.Instance.Player.StreamMetadata.IsCoverLocked = True
                                                      SharedProperties.Instance.Player.StreamMetadata.EnsureCovers()
                                                  End Sub)
            If SharedProperties.Instance.Player.StreamMetadata.Covers Is Nothing OrElse Not SharedProperties.Instance.Player.StreamMetadata.Covers.Any Then
                If FallBack Then
                    ReturnLogo(context, IsInline) 'add inline
                Else
                    context.Response.StatusCode = 404
                    context.Response.OutputStream.Close()
                End If
                Return
            End If
            context.Response.ContentType = HttpFileServer.getcontentType(".jpg")
            context.Response.AppendHeader("X-Content-Type-Options", "nosniff")
            context.Response.AppendHeader("Content-Disposition", If(IsInline, "inline", "attachment; filename=""cover.jpg"""))
            Dim mem As New IO.MemoryStream
            Application.Current.Dispatcher.Invoke(Sub()
                                                      Dim jpgEnco As New JpegBitmapEncoder() 'encode as jpg                                                      
                                                      Dim frame = BitmapFrame.Create(SharedProperties.Instance.Player.StreamMetadata.Covers?.FirstOrDefault)
                                                      jpgEnco.Frames.Add(frame)
                                                      jpgEnco.Save(mem)
                                                      If Not WasLocked Then
                                                          SharedProperties.Instance.Player.StreamMetadata.IsCoverLocked = False
                                                      End If
                                                  End Sub)
            context.Response.ContentLength64 = mem.Length
            Try
                context.Response.OutputStream.Write(mem.ToArray, 0, mem.Length)
            Catch ex As Exception
                context.Response.Abort()
                Utilities.DebugMode.Instance.Log(Of HttpFileServer)(ex.ToString)
            End Try
            context.Response.OutputStream.Close()
        End Sub

        Private Sub ReturnLogo(ByVal context As HttpListenerContext, Optional IsInline As Boolean = False)
            context.Response.ContentType = HttpFileServer.getcontentType(".png")
            context.Response.AppendHeader("X-Content-Type-Options", "nosniff")
            context.Response.AppendHeader("Content-Disposition", If(IsInline, "inline", "attachment; filename=""logo.png"""))
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
#End Region
#Disable Warning
#Region "Player Events (Sync)"
        Private Sub _Player_MetadataChanged() Handles _Player.MetadataChanged
            If LockSyncEvents OrElse _Player.IsPreviewing Then Return
            RaiseSyncEvent("media")
        End Sub

        Private Sub _Player_PositionChanged(newPosition As Double) Handles _Player.PositionChanged
            If LockSyncEvents OrElse _Player.IsPreviewing Then Return
            RaiseSyncEvent("position")
        End Sub

        Private Sub _Player_PlaystateChanged(sender As Player.Player) Handles _Player.PlaystateChanged
            If LockSyncEvents OrElse _Player.IsPreviewing Then Return
            RaiseSyncEvent("state")
        End Sub

        Private Sub _Player_DownloadFinished(data As MemoryStream) Handles _Player.DownloadFinished
            If LockSyncEvents OrElse _Player.IsPreviewing Then Return
            RaiseSyncEvent("all")
        End Sub
        Private Sub _Player_PreviewStarted() Handles _Player.PreviewStarted
            LockSyncEvents = True
        End Sub

        Private Sub _Player_PreviewEnded() Handles _Player.PreviewEnded
            _LockSyncEvents = False
        End Sub
#End Region
#Enable Warning

        'Private _RedundancyCounter As Integer = 0
        'Private _RedundancyIsCounting As Boolean = False

        Private Async Function RaiseSyncEvent(type As String) As Task
            'If _RedundancyIsCounting Then
            '    _RedundancyCounter = 0
            '    Return
            'Else
            '    _RedundancyIsCounting = True
            'End If
            'Do While _RedundancyIsCounting
            '    Await Task.Delay(10)
            '    _RedundancyCounter += 1
            '    If _RedundancyCounter = 10 Then '100 ms has passed
            '        _RedundancyCounter = 0
            '        _RedundancyIsCounting = False
            '        Exit Do
            '    End If
            'Loop
            If Status <> LinkStatus.Linked Then Return
            If _LinkDirectionIsForward = TriState.False Then Return
            Using hc As New Net.Http.HttpClient()
                Dim qlURI As New Uri($"{LinkedToAddress}/api/quicklink/sync/" & type)
                hc.DefaultRequestHeaders.Authorization = New Http.Headers.AuthenticationHeaderValue("Bearer", _LinkToken.ToString)
                Try
                    Dim result = Await hc.GetStringAsync(qlURI)
                Catch
                    _LinkDirectionIsForward = TriState.UseDefault
                    Status = LinkStatus.Unlinked
                    LinkedTo = ""
                    LinkedToAddress = ""
                    _LinkToken = Nothing
                End Try
            End Using
        End Function

        Private Async Sub _PingTimer_Tick(sender As Object, e As EventArgs) Handles _PingTimer.Tick
            If Status = LinkStatus.Unlinked Then
                _PingTimer.Stop()
                Return
            End If
            Try
                Using hc As New Http.HttpClient
                    hc.DefaultRequestHeaders.Authorization = New Http.Headers.AuthenticationHeaderValue("Bearer", _LinkToken.ToString)
                    Dim Hresult = Await hc.GetAsync($"{LinkedToAddress}/api/quicklink/hello")
                    If Hresult.StatusCode <> HttpStatusCode.OK Then
                        _LinkDirectionIsForward = TriState.UseDefault
                        Status = LinkStatus.Unlinked
                        LinkedTo = ""
                        LinkedToAddress = ""
                        _LinkToken = Nothing
                    End If
                End Using
            Catch
                _LinkDirectionIsForward = TriState.UseDefault
                Status = LinkStatus.Unlinked
                LinkedTo = ""
                LinkedToAddress = ""
                _LinkToken = Nothing
            End Try
        End Sub

        Public Enum LinkStatus
            Unlinked
            Linking
            Linked
        End Enum
    End Class
End Namespace