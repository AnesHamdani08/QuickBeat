Imports System.ComponentModel
Imports QuickBeat.Utilities

Namespace UPnP
    Public Class UPnPInfo
        Implements ComponentModel.INotifyPropertyChanged

        Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged
        Public Sub OnPropertyChanged(<Runtime.CompilerServices.CallerMemberName> Optional CallerName As String = Nothing)
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(CallerName))
        End Sub

        Private _Name As String
        Public Overloads Property Name As String
            Get
                Return _Name
            End Get
            Set(value As String)
                _Name = value
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

        Private _Description As String
        Property Description As String
            Get
                Return _Description
            End Get
            Set(value As String)
                _Description = value
                OnPropertyChanged()
            End Set
        End Property

        Private _ManufacturerName As String
        Property ManufacturerName As String
            Get
                Return _ManufacturerName
            End Get
            Set(value As String)
                _ManufacturerName = value
                OnPropertyChanged()
            End Set
        End Property

        Private _ManufacturerURL As String
        Property ManufacturerURL As String
            Get
                Return _ManufacturerURL
            End Get
            Set(value As String)
                _ManufacturerURL = value
                OnPropertyChanged()
            End Set
        End Property

        Private _ModelName As String
        Property ModelName As String
            Get
                Return _ModelName
            End Get
            Set(value As String)
                _ModelName = value
                OnPropertyChanged()
            End Set
        End Property

        Private _ModelNumber As String
        Property ModelNumber As String
            Get
                Return _ModelNumber
            End Get
            Set(value As String)
                _ModelNumber = value
                OnPropertyChanged()
            End Set
        End Property

        Private _ModelURL As String
        Property ModelURL As String
            Get
                Return _ModelURL
            End Get
            Set(value As String)
                _ModelURL = value
                OnPropertyChanged()
            End Set
        End Property

        Sub New()

        End Sub
        Sub New(Device As UPNPLib.UPnPDevice)
            Me.Name = Device.FriendlyName
            Me.Description = Device.Description
            Dim IconURL As String = Device.IconURL("image/png", 32, 32, 16)
            If Not String.IsNullOrEmpty(IconURL) Then
                Me.Icon = If(New Uri(IconURL, UriKind.Absolute).ToBitmapSource, New BitmapImage(New Uri("Resources/CircledPlay.png", UriKind.Relative)))
            End If
            Me.ManufacturerName = Device.ManufacturerName
            Me.ManufacturerURL = Device.ManufacturerURL
            Me.ModelName = Device.ModelName
            Me.ModelNumber = Device.ModelNumber
            Me.ModelURL = Device.ModelURL
        End Sub

        Public Overrides Function ToString() As String
            Return $"{Me.Name}[{ModelName}.{ModelNumber}]"
        End Function
    End Class
End Namespace