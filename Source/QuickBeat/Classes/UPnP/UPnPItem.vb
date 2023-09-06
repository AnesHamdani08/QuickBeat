Imports System.ComponentModel

Namespace UPnP
    Public Class UPnPItem
        Implements ComponentModel.INotifyPropertyChanged

        Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged
        Public Sub OnPropertyChanged(<Runtime.CompilerServices.CallerMemberName> Optional CallerName As String = Nothing)
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(CallerName))
        End Sub

        Public Property Parent As UPnPProvider
        Public Property Tag As ContentDirectory.CD_Object

        Public Overloads Property Items As New ObjectModel.ObservableCollection(Of UPnPItem)

        Private _Header As String
        Public Overloads Property Header As String
            Get
                Return _Header
            End Get
            Set(value As String)
                _Header = value
                OnPropertyChanged()
            End Set
        End Property

        Private _Artist As String
        Public Overloads Property Artist As String
            Get
                Return _Artist
            End Get
            Set(value As String)
                _Artist = value
                OnPropertyChanged()
            End Set
        End Property

        Private _Album As String
        Public Overloads Property Album As String
            Get
                Return _Album
            End Get
            Set(value As String)
                _Album = value
                OnPropertyChanged()
            End Set
        End Property

        Private _Icon As ImageSource
        Public Property Icon As ImageSource
            Get
                Return _Icon
            End Get
            Set(value As ImageSource)
                _Icon = value
                OnPropertyChanged()
            End Set
        End Property

        Private _Info As UPnPInfo
        Property Info As UPnPInfo
            Get
                Return _Info
            End Get
            Set(value As UPnPInfo)
                _Info = value
                OnPropertyChanged()
            End Set
        End Property

        Public Sub New()

        End Sub
        Public Sub New(Device As UPNPLib.UPnPDevice)
            Info = New UPnPInfo(Device)
        End Sub
        Public Sub New(Header As String, Icon As ImageSource)
            Me.Header = Header
            Me.Icon = Icon
            Info = New UPnPInfo With {.Name = Header, .Icon = Icon}
        End Sub
        Public Sub New(Header As String, Icon As ImageSource, Info As ContentDirectory.CD_Item)
            Me.Header = Header
            Me.Icon = Icon
            Me.Artist = Info.Artist
            Me.Album = Info.Album
        End Sub

        Public Overrides Function ToString() As String
            Return $"{Header}: {Items.Count} Items"
        End Function
    End Class
End Namespace