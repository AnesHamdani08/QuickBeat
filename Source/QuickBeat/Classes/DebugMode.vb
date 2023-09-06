Imports System.ComponentModel

Namespace Utilities
    Public Class DebugMode
        Implements ComponentModel.INotifyPropertyChanged

        Public Shared Instance As New DebugMode
        Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged

        Public Sub OnPropertyChanged(<Runtime.CompilerServices.CallerMemberName> Optional CallerName As String = Nothing)
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(CallerName))
        End Sub

        'Variables

        Public Sub Start()
            If IsEnabled Then Return
            IsEnabled = True
            Utilities.SharedProperties.Instance.IsLogging = True
            Dim info = My.Application.Info
            Utilities.SharedProperties.Instance.Notification("QuickBeat Debug Kit",
                                                         $"App: {info.AssemblyName}" & Environment.NewLine &
                                                         $"Version: {info.Version}" & Environment.NewLine &
                                                         $"Debug Mode: Enabled",
                                                          HandyControl.Data.NotifyIconInfoType.Info)
        End Sub

        Public Sub Log(Of TClass As Class)(message As String, <Runtime.CompilerServices.CallerLineNumber> Optional CallerLine As String = Nothing, <Runtime.CompilerServices.CallerMemberName> Optional CallerName As String = Nothing)
            If SharedProperties.Instance?.IsLogging Then Debug.WriteLine($"[{Now.ToString("HH:mm:ss")}][{GetType(TClass).FullName}.{CallerName},{CallerLine}]: {message}")
        End Sub

#Region "WPF Support"
        Private _IsEnabled As Boolean = False
        Property IsEnabled As Boolean
            Get
                Return _IsEnabled
            End Get
            Set(value As Boolean)
                _IsEnabled = value
                OnPropertyChanged()
            End Set
        End Property

        Public ReadOnly Property StartCommand As New DelegateStartCommand(Me)
#Region "Classes"
        Public Class DelegateStartCommand
            Implements ICommand

            Public Event CanExecuteChanged As EventHandler Implements ICommand.CanExecuteChanged
            Private Parent As DebugMode
            Sub New(parent As DebugMode)
                Me.Parent = parent
                AddHandler CommandManager.RequerySuggested, New EventHandler(Sub(sender As Object, e As EventArgs)
                                                                                 RaiseEvent CanExecuteChanged(Me, New EventArgs())
                                                                             End Sub)
            End Sub
            Public Sub Execute(parameter As Object) Implements ICommand.Execute
                Parent?.Start()
            End Sub

            Public Function CanExecute(parameter As Object) As Boolean Implements ICommand.CanExecute
                Return Not Parent?.IsEnabled
            End Function
        End Class
#End Region
#End Region
    End Class
End Namespace