Imports System.ComponentModel
Imports QuickBeat.Classes
Imports QuickBeat.Interfaces

Namespace Player
    Public MustInherit Class EngineModule
        Implements ComponentModel.INotifyPropertyChanged, Interfaces.IStartupItem

        Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged
        Protected Overridable Sub OnPropertyChanged(<Runtime.CompilerServices.CallerMemberName> Optional CallerName As String = Nothing)
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(CallerName))
        End Sub

        Public MustOverride ReadOnly Property Name As String
        Public MustOverride ReadOnly Property Description As String
        Public MustOverride ReadOnly Property Category As String
        Public MustOverride Property Info As String
        Public MustOverride Property IsEnabled As Boolean
        Public Overridable Property Icon As ImageSource

        Public MustOverride Property Configuration As StartupItemConfiguration Implements IStartupItem.Configuration

        Public MustOverride Sub Init() Implements IStartupItem.Init
        Overridable Sub Config()

        End Sub

    End Class
End Namespace
