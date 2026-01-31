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
        Private _TempBuffer As String
        ''' <summary>
        ''' Used while <see cref="SharedProperties.IsLogging"/> is false
        ''' </summary>
        ''' <returns></returns>
        ReadOnly Property TempBuffer As String
            Get
                Return _TempBuffer
            End Get
        End Property

        Private _Cache As String
        ''' <summary>
        ''' Contains all logs during this session
        ''' </summary>
        ''' <returns></returns>
        ReadOnly Property Cache As String
            Get
                Return _Cache
            End Get
        End Property

        Private _BypassCompilerOptimization As Boolean = False
        ''' <summary>
        ''' Wheter or not to use <see cref="Debug.WriteLine(String)"/> or <see cref="Console.WriteLine()"/>
        ''' </summary>
        ''' <returns></returns>
        Property BypassCompilerOptimization As Boolean
            Get
                Return _BypassCompilerOptimization
            End Get
            Set(value As Boolean)
                _BypassCompilerOptimization = value
                OnPropertyChanged()
            End Set
        End Property

        Public Sub Start()
            If IsEnabled Then Return
            IsEnabled = True
            'Utilities.SharedProperties.Instance.IsLogging = True
            Dim info = My.Application.Info
            Try
                'Console.SetOut(New Utilities.MultiTextWriter.ControlWriter(My.Windows.DeveloperConsole.ConsoleOut_TB, My.Windows.DeveloperConsole.Dispatcher))
                'TODO add somemthing to this
                MsgBox("failed to set out")
            Catch ex As Exception
                HandyControl.Controls.MessageBox.Error("Failed to set console out." & Environment.NewLine & ex.ToString)
            End Try
            Utilities.SharedProperties.Instance.Notification("QuickBeat Debug Kit",
                                                         $"App: {info.AssemblyName}" & Environment.NewLine &
                                                         $"Version: {info.Version}" & Environment.NewLine &
                                                         $"Debug Mode: Unlocked",
                                                          HandyControl.Data.NotifyIconInfoType.Info)
        End Sub

        Public Sub Log(Of TClass As Class)(message As String, <Runtime.CompilerServices.CallerLineNumber> Optional CallerLine As String = Nothing, <Runtime.CompilerServices.CallerMemberName> Optional CallerName As String = Nothing)
            Dim fMessage = $"[{Now.ToString("HH:mm:ss")}][{GetType(TClass).FullName}.{CallerName},{CallerLine}]: {message}"
            _Cache &= fMessage & Environment.NewLine
            If SharedProperties.Instance?.IsLogging Then
                If Not String.IsNullOrEmpty(_TempBuffer) Then
                    If BypassCompilerOptimization Then
                        Console.WriteLine(TempBuffer)
                    Else
                        Debug.WriteLine(_TempBuffer)
                    End If
                    _TempBuffer = ""
                End If
                If BypassCompilerOptimization Then
                    Console.WriteLine(fMessage)
                Else
                    Debug.WriteLine(fMessage)
                End If
            Else
                    _TempBuffer &= fMessage & Environment.NewLine
            End If
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