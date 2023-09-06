Imports System.ComponentModel
Imports System.Runtime.InteropServices
Imports UPNPLib
Imports QuickBeat.Interfaces
Imports QuickBeat.Classes
Imports QuickBeat.UPnP.ContentDirectory

Namespace UPnP
    Public Class UPnPProvider
        Implements ComponentModel.INotifyPropertyChanged, IStartupItem

        Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged
        Public Sub OnPropertyChanged(<Runtime.CompilerServices.CallerMemberName> Optional CallerName As String = Nothing)
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(CallerName))
        End Sub

        Private _IsServersBusy As Boolean
        Property IsServersBusy As Boolean
            Get
                Return _IsServersBusy
            End Get
            Set(value As Boolean)
                _IsServersBusy = value
                OnPropertyChanged()
            End Set
        End Property

        Private _IsRenderersBusy As Boolean
        Property IsRenderersBusy As Boolean
            Get
                Return _IsRenderersBusy
            End Get
            Set(value As Boolean)
                _IsRenderersBusy = value
                OnPropertyChanged()
            End Set
        End Property

        Private _IsRootBusy As Boolean
        Property IsRootBusy As Boolean
            Get
                Return _IsRootBusy
            End Get
            Set(value As Boolean)
                _IsRootBusy = value
                OnPropertyChanged()
            End Set
        End Property

        Private _IsBrowseBusy As Boolean
        Property IsBrowseBusy As Boolean
            Get
                Return _IsBrowseBusy
            End Get
            Set(value As Boolean)
                _IsBrowseBusy = value
                OnPropertyChanged()
            End Set
        End Property

        Private _RootDevices As New ObjectModel.ObservableCollection(Of UPnPDeviceListItemWrapper)
        ReadOnly Property RootDevices As ObjectModel.ObservableCollection(Of UPnPDeviceListItemWrapper)
            Get
                Return _RootDevices
            End Get
        End Property

        Private _Servers As New ObjectModel.ObservableCollection(Of UPnPDeviceListItemWrapper)
        ReadOnly Property Servers As ObjectModel.ObservableCollection(Of UPnPDeviceListItemWrapper)
            Get
                Return _Servers
            End Get
        End Property

        Private _Renderers As New ObjectModel.ObservableCollection(Of UPnPDeviceListItemWrapper)
        ReadOnly Property Renderers As ObjectModel.ObservableCollection(Of UPnPDeviceListItemWrapper)
            Get
                Return _Renderers
            End Get
        End Property

        Private _SelectedServerIndex As Integer = -1
        Property SelectedServerIndex As Integer
            Get
                Return _SelectedServerIndex
            End Get
            Set(value As Integer)
                If value >= 0 AndAlso value < Servers.Count Then
                    _SelectedServerIndex = value
                    OnPropertyChanged()
                    SelectServer(value)
                End If
            End Set
        End Property

        Private _SelectedRendererIndex As Integer = -1
        Property SelectedRendererIndex As Integer
            Get
                Return _SelectedRendererIndex
            End Get
            Set(value As Integer)
                If value >= 0 AndAlso value < Renderers.Count Then
                    Dim selRenderer As MediaRenderer = Nothing
                    Try
                        selRenderer = Renderers(value).CreateRenderer
                    Catch ex As Exception
                        Utilities.DebugMode.Instance.Log(Of UPnPProvider)(ex.ToString)
                    End Try
                    If selRenderer Is Nothing Then
                        Renderers.RemoveAt(value)
                    Else
                        SelectedRenderer = selRenderer
                        _SelectedRendererIndex = value
                        OnPropertyChanged()
                    End If
                ElseIf value = -1 Then
                    _SelectedRendererIndex = -1
                    OnPropertyChanged()
                    SelectedRenderer = Nothing
                End If
            End Set
        End Property

        Private _IsUsingRenderer As Boolean
        Property IsUsingRenderer As Boolean
            Get
                Return _IsUsingRenderer
            End Get
            Set(value As Boolean)
                _IsUsingRenderer = value
                OnPropertyChanged()
            End Set
        End Property

        Private _SelectedServer As UPnPItem
        Property SelectedServer As UPnPItem
            Get
                Return _SelectedServer
            End Get
            Set(value As UPnPItem)
                _SelectedServer = value
                LoadChildren(value)
                OnPropertyChanged()
            End Set
        End Property

        Private _SelectedRenderer As MediaRenderer
        Property SelectedRenderer As MediaRenderer
            Get
                Return _SelectedRenderer
            End Get
            Set(value As MediaRenderer)
                If _SelectedRenderer IsNot Nothing Then
                    _SelectedRenderer.StopMonitoring()
                End If
                _SelectedRenderer = value
                OnPropertyChanged()
                _SelectedRenderer?.StartMonitoring()
                _SelectedRenderer?.ForceInfoPass()
                IsUsingRenderer = SelectedRenderer IsNot Nothing
            End Set
        End Property

        Public Property Configuration As New StartupItemConfiguration("UPnP") Implements IStartupItem.Configuration

        ReadOnly Property SearchCommand As New DelegateSearchCommand(Me)
        ReadOnly Property StopStreamCommand As New DelegateStopStreamCommand(Me)
#Region "Fields"
        Private MediaServer As UPnPDevice = Nothing  'The device in which we browse or search for music   
        Private ContentDirectory As UPnPService = Nothing 'The service that handles the browsing and searching actions
        Private AllDevices As UPnPDevices = Nothing 'A list with all discovered upnp-devices
        Private DeviceFinder As New UPnPDeviceFinder
        Private WithEvents myDeviceFinderCallback As New myUPnPDeviceFinderCallback
        Private hServerFind As Integer = Nothing
        Private hRendererFind As Integer = Nothing
        Private hRootDeviceFind As Integer = Nothing
        Private TotalItemDuration As DateTime
        Private SelectedItemInPlaylist As Integer = -1
        Private myMediaRenderer As MediaRenderer = Nothing
#End Region
#Region "Classes"
        <ComVisible(True), ComImport(), Guid("415A984A-88B3-49F3-92AF-0508BEDF0D6C"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)> Interface IUPnPDeviceFinderCallback
            <PreserveSig()> Function DeviceAdded(ByVal lFindData As Integer, ByVal pDevice As UPNPLib.IUPnPDevice) As Integer
            <PreserveSig()> Function DeviceRemoved(ByVal lFindData As Integer, ByVal bstrUDN As String) As Integer
            <PreserveSig()> Function SearchComplete(ByVal lFindData As Integer) As Integer
        End Interface
        Public Class UPnPDeviceListItemWrapper
            Implements ComponentModel.INotifyPropertyChanged

            Public UPnPDevice As UPnPDevice = Nothing

            Private _Info As UPnPInfo
            Overridable Property Info As UPnPInfo
                Get
                    Return _Info
                End Get
                Set(value As UPnPInfo)
                    _Info = value
                    OnPropertyChanged()
                End Set
            End Property

            Public Sub New(ByVal UPnPDevice As UPnPDevice)
                If UPnPDevice IsNot Nothing Then
                    Me.UPnPDevice = UPnPDevice
                    Info = New UPnPInfo(UPnPDevice)
                End If
            End Sub

            Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged
            Public Sub OnPropertyChanged(<Runtime.CompilerServices.CallerMemberName> Optional CallerName As String = Nothing)
                RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(CallerName))
            End Sub
            Public Overridable Function CreateRenderer() As MediaRenderer
                Return New MediaRenderer(UPnPDevice)
            End Function
            Public Overrides Function ToString() As String
                If Me.UPnPDevice IsNot Nothing Then Return Me.UPnPDevice.FriendlyName Else Return "N/A"
            End Function
        End Class
        Public Class myUPnPDeviceFinderCallback
            Implements IUPnPDeviceFinderCallback

            Public Event DeviceFound(ByVal lFindData As Integer, ByVal pDevice As UPNPLib.IUPnPDevice)
            Public Event DeviceLost(ByVal lFindData As Integer, ByVal bstrUDN As String)
            Public Event SearchOperationCompleted(ByVal lFindData As Integer)

            Public Function DeviceAdded(ByVal lFindData As Integer, ByVal pDevice As UPNPLib.IUPnPDevice) As Integer Implements IUPnPDeviceFinderCallback.DeviceAdded
                RaiseEvent DeviceFound(lFindData, pDevice)
                Return lFindData
            End Function

            Public Function DeviceRemoved(ByVal lFindData As Integer, ByVal bstrUDN As String) As Integer Implements IUPnPDeviceFinderCallback.DeviceRemoved
                RaiseEvent DeviceLost(lFindData, bstrUDN)
                Return lFindData
            End Function

            Public Function SearchComplete(ByVal lFindData As Integer) As Integer Implements IUPnPDeviceFinderCallback.SearchComplete
                RaiseEvent SearchOperationCompleted(lFindData)
                Return lFindData
            End Function
        End Class
#End Region
        Sub New()
            Init()
        End Sub
        Public Sub Init() Implements IStartupItem.Init
            Utilities.SharedProperties.Instance.ItemsConfiguration.Add(Configuration)
            Configuration.SetStatus("All Good", 100)
        End Sub

        Public Sub Search()
            OnPropertyChanged(NameOf(IsUsingRenderer))
            DeviceFinderCategoryCount = 0
            If Not IsServersBusy Then
                Servers.Clear()
                hServerFind = DeviceFinder.CreateAsyncFind("urn:schemas-upnp-org:device:MediaServer:1", 0, myDeviceFinderCallback)
                DeviceFinderCategoryCount += 1
                DeviceFinder.StartAsyncFind(hServerFind)
                IsServersBusy = True
            End If
            If Not IsRenderersBusy Then
                Renderers.Clear()
                Renderers.Add(New LocalMediaRenderer.LocalRendererUPnPDeviceListItemWrapper)
                hRendererFind = DeviceFinder.CreateAsyncFind("urn:schemas-upnp-org:device:MediaRenderer:1", 0, myDeviceFinderCallback)
                DeviceFinderCategoryCount += 1
                DeviceFinder.StartAsyncFind(hRendererFind)
                IsRenderersBusy = True
            End If
            If Not IsRootBusy Then
                RootDevices.Clear()
                hRootDeviceFind = DeviceFinder.CreateAsyncFind("upnp:rootdevice", 0, myDeviceFinderCallback)
                DeviceFinderCategoryCount += 1
                DeviceFinder.StartAsyncFind(hRootDeviceFind)
                IsRootBusy = True
            End If
            Configuration.SetStatus("Searching for devices...", 55)
        End Sub
#Region "Finder"
        Dim DeviceFinderCategoryCount = 0
        Private Sub myDeviceFinderCallback_DeviceFound(lFindData As Integer, pDevice As IUPnPDevice) Handles myDeviceFinderCallback.DeviceFound
            Select Case lFindData
                Case hServerFind
                    Servers.Add(New UPnPDeviceListItemWrapper(pDevice))
                Case hRendererFind
                    Renderers.Add(New UPnPDeviceListItemWrapper(pDevice))
                Case hRootDeviceFind
                    RootDevices.Add(New UPnPDeviceListItemWrapper(pDevice))
            End Select
        End Sub

        Private Sub myDeviceFinderCallback_SearchOperationCompleted(lFindData As Integer) Handles myDeviceFinderCallback.SearchOperationCompleted
            DeviceFinder.CancelAsyncFind(lFindData)
            Select Case lFindData
                Case hServerFind
                    hServerFind = 0
                    IsServersBusy = False
                Case hRendererFind
                    hRendererFind = 0
                    IsRenderersBusy = False
                Case hRootDeviceFind
                    hRootDeviceFind = 0
                    IsRootBusy = False
            End Select
            DeviceFinderCategoryCount -= 1
            If DeviceFinderCategoryCount <= 0 Then
                Configuration.SetStatus($"Done, {Servers.Count + Renderers.Count + RootDevices.Count} Device(s) found", 100)
            End If
        End Sub
#End Region
#Region "Navigation"
        Public Sub SelectServer(index As Integer)
            MediaServer = Servers(index).UPnPDevice
            Dim udsEnu = MediaServer.Services.GetEnumerator
            Do While udsEnu.MoveNext
                If TryCast(udsEnu.Current, UPnPService)?.ServiceTypeIdentifier = "urn:schemas-upnp-org:service:ContentDirectory:1" Then
                    ContentDirectory = TryCast(udsEnu.Current, UPnPService)
                    SelectedServer = New UPnPItem(MediaServer)
                    Exit Do
                End If
            Loop
        End Sub
        Sub LoadChildren(parentTreeNode As UPnPItem)
            If parentTreeNode Is Nothing Then Return
            IsBrowseBusy = True
            If TypeOf parentTreeNode.Tag Is CD_Item Then
                Return
            End If
            Dim myObjects() As CD_Object = Nothing

            If parentTreeNode.Tag Is Nothing Then '=the root-node of the treeview
                myObjects = BrowseContainer("0")
            ElseIf TypeOf parentTreeNode.Tag Is CD_Object Then
                myObjects = BrowseContainer(DirectCast(parentTreeNode.Tag, CD_Object).ID)
            End If

            If myObjects Is Nothing Then Return

            parentTreeNode.Items.Clear()

            For Each myObject As CD_Object In myObjects
                Dim myTreeNode As New UPnPItem(myObject.Title, Nothing) With {.Parent = Me, .Tag = myObject}

                parentTreeNode.Items.Add(myTreeNode)

                If TypeOf myObject Is CD_Container Then
                    myTreeNode.Icon = Utilities.CommonFunctions.GenerateCoverImage(Utilities.ResourceResolver.Images.FOLDER)
                    If Not myObject.ClassName = "object.container.playlistContainer" Then myTreeNode.Items.Add(New UPnPItem("Loading...", Utilities.CommonFunctions.GenerateCoverImage(Utilities.ResourceResolver.Images.CLOCK)))
                Else
                    Dim image = myObject.Resource?.FirstOrDefault(Function(k) TryGetMimeTypeFromDLNAProtocole(k.protocolInfo) = "image/png")
                    If image Is Nothing Then image = myObject.Resource?.FirstOrDefault(Function(k) TryGetMimeTypeFromDLNAProtocole(k.protocolInfo) = "image/jpeg")
                    If image Is Nothing Then
                        myTreeNode.Icon = Utilities.CommonFunctions.GenerateCoverImage(Utilities.ResourceResolver.Images.MUSICNOTE)
                    Else
                        Dim bi As New BitmapImage
                        bi.BeginInit()
                        bi.UriSource = image.URI
                        bi.EndInit()
                        myTreeNode.Icon = bi
                    End If
                End If
            Next
            IsBrowseBusy = False
        End Sub
        Public Function GetCDObjectInformation(ByVal objectID As String) As CD_Object
            If ContentDirectory Is Nothing Then Return Nothing

            Dim myItem As CD_Object = Nothing
            Dim myInObject(5) As Object
            Dim myOutObject(3) As Object

            myInObject.SetValue(objectID, 0) '; //UPnP Object to browse, check SDK for values of the WMV hierarchy
            myInObject.SetValue("BrowseMetadata", 1) '; //SortOfInformation
            myInObject.SetValue("upnp:album,upnp:artist,upnp:genre,upnp:title,res@size,res@duration,res@bitrate,res@sampleFrequency,res@bitsPerSample,res@nrAudioChannels,res@protocolInfo,res@protection,res@importUri", 2) ' Filter
            myInObject.SetValue(0, 3) ' Start Index
            myInObject.SetValue(1, 4) ' Max result count
            myInObject.SetValue("", 5) ' sort Criteria

            ContentDirectory.InvokeAction("Browse", myInObject, myOutObject)

            Dim myXMLDoc As XDocument

            myXMLDoc = XDocument.Parse(CStr(myOutObject(0)))
            'myXMLDoc.LoadXml(CStr(myOutObject(0)).Replace("&", "&amp;"))
            Dim element = myXMLDoc.Root.Elements.FirstOrDefault
            myItem = If(element Is Nothing, Nothing, CD_Object.Create_CD_Object(element))

            Return myItem
        End Function
        Private Function BrowseContainer(ByVal objectID As String) As CD_Object()
            If ContentDirectory Is Nothing Then Return Nothing
            Dim myObjects() As CD_Object = Nothing
            Dim myInObject(5) As Object
            Dim myOutObject(3) As Object

            myInObject.SetValue(objectID, 0) '; //UPnP Object to browse, check SDK for values of the WMV hierarchy
            myInObject.SetValue("BrowseDirectChildren", 1) '; //SortOfInformation
            myInObject.SetValue("", 2) ' Filter
            myInObject.SetValue(0, 3) ' Start Index
            myInObject.SetValue(0, 4) ' Max result count
            myInObject.SetValue("", 5) ' sort Criteria

            Dim myResponse As Object = ContentDirectory.InvokeAction("Browse", myInObject, myOutObject)
            If myOutObject(0) Is Nothing Then Return Nothing
            Dim myXMLDoc As XDocument = XDocument.Parse(CStr(myOutObject(0)).Replace("&", "&amp;"))
            'myXMLDoc.LoadXml(CStr(myOutObject(0)).Replace("&", "&amp;"))                
            For Each xmlNode As XNode In myXMLDoc.Root.Elements
                If myObjects Is Nothing Then
                    ReDim Preserve myObjects(0) 'first time we use the myObjects array, it is undefined, so we make it the size of 1 element
                Else
                    ReDim Preserve myObjects(myObjects.Length) 'each time we increase the size of the array and remember the elements that were already in it
                End If
                myObjects(myObjects.Length - 1) = CD_Object.Create_CD_Object(xmlNode)
                'Debug.WriteLine("Create Object:" & myObjects(myObjects.Length - 1).ClassName & " " & myObjects(myObjects.Length - 1).ID)
            Next
            'Debug.WriteLine("BrowseContainer: Stop")
            Return myObjects
        End Function
#End Region
#Region "Helpers"
        Public Shared Function TryGetMimeTypeFromDLNAProtocole(Protocol As String) As String
            'Str Protocol ref:"http-get:*:audio/mpeg:DLNA.ORG_PN=MP3;DLNA.ORG_OP=01;DLNA.ORG_FLAGS=01700000000000000000000000000000"
            Dim HttpLessProtocol = Protocol.Replace("http-get:*:", "")
            Dim ix0 = HttpLessProtocol.IndexOf(":")
            Dim ix1 = HttpLessProtocol.IndexOf(";")
            If ix1 < ix0 Then
                Return HttpLessProtocol.Substring(0, ix1)
            Else
                Return HttpLessProtocol.Substring(0, ix0)
            End If
        End Function
#End Region
#Region "WPF Support"
        Public Class DelegateSearchCommand
            Implements ICommand

            Public Event CanExecuteChanged As EventHandler Implements ICommand.CanExecuteChanged
            Private _Provider As UPnPProvider

            Sub New(Renderer As UPnPProvider)
                _Provider = Renderer
                AddHandler CommandManager.RequerySuggested, New EventHandler(Sub(sender As Object, e As EventArgs)
                                                                                 RaiseEvent CanExecuteChanged(Me, New EventArgs())
                                                                             End Sub)
            End Sub

            Public Sub Execute(parameter As Object) Implements ICommand.Execute
                _Provider?.Search()
            End Sub

            Public Function CanExecute(parameter As Object) As Boolean Implements ICommand.CanExecute
                Return _Provider IsNot Nothing AndAlso Not _Provider.IsServersBusy AndAlso Not _Provider.IsRenderersBusy AndAlso Not _Provider.IsRootBusy
            End Function
        End Class
        Public Class DelegateStopStreamCommand
            Implements ICommand

            Public Event CanExecuteChanged As EventHandler Implements ICommand.CanExecuteChanged
            Private _Provider As UPnPProvider

            Sub New(Renderer As UPnPProvider)
                _Provider = Renderer
                AddHandler CommandManager.RequerySuggested, New EventHandler(Sub(sender As Object, e As EventArgs)
                                                                                 RaiseEvent CanExecuteChanged(Me, New EventArgs())
                                                                             End Sub)
            End Sub

            Public Sub Execute(parameter As Object) Implements ICommand.Execute
                If _Provider IsNot Nothing Then _Provider.SelectedRendererIndex = -1
            End Sub

            Public Function CanExecute(parameter As Object) As Boolean Implements ICommand.CanExecute
                Return _Provider IsNot Nothing
            End Function
        End Class
#End Region
    End Class
End Namespace