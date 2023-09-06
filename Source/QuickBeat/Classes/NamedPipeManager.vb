Imports System.IO
Imports System.IO.Pipes
Imports System.Threading
Imports QuickBeat.Interfaces
Imports System.ComponentModel

Namespace Classes
    Public Class NamedPipeManager
        Implements IDisposable, Interfaces.IStartupItem, INotifyPropertyChanged

        Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged

        Public Sub OnPropertyChanged(<Runtime.CompilerServices.CallerMemberName> Optional CallerName As String = Nothing)
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(CallerName))
        End Sub

        Private _Configuration As New StartupItemConfiguration("Named Pipe")
        Public Property Configuration As StartupItemConfiguration Implements Interfaces.IStartupItem.Configuration
            Get
                Return _Configuration
            End Get
            Set(value As StartupItemConfiguration)
                _Configuration = value
                OnPropertyChanged()
            End Set
        End Property

        Private _TotalReceived As Integer
        Property TotalReceived As Integer
            Get
                Return _TotalReceived
            End Get
            Set(value As Integer)
                _TotalReceived = value
                OnPropertyChanged()
            End Set
        End Property

        Private _NamedPipeName As String = "GenericNamedPipe"
        Property NamedPipeName As String
            Get
                Return _NamedPipeName
            End Get
            Set(value As String)
                _NamedPipeName = value
                OnPropertyChanged()
            End Set
        End Property

        Private _IsRunning As Boolean = False
        Public Property IsRunning As Boolean
            Get
                Return _IsRunning
            End Get
            Set(value As Boolean)
                _IsRunning = value
                OnPropertyChanged()
            End Set
        End Property

        Public Property MaxServerInstances As Integer = NamedPipeServerStream.MaxAllowedServerInstances

        Public Event ReceiveString(sender As NamedPipeManager, msg As String)

        Public Event OnException(ex As Exception)

        Private Const EXIT_STRING As String = "__EXIT__"

        Private PipeThread As Thread
        Private PipeThreadCT As New CancellationTokenSource()
        Private ServerPipeDirection As PipeDirection = PipeDirection.InOut

        Public Sub New(ByVal name As String, Direction As PipeDirection)
            NamedPipeName = name
            ServerPipeDirection = Direction
            Configuration.Name = name
        End Sub

        Public Sub Init() Implements IStartupItem.Init
            Configuration.SetStatus("Set-up, Waiting for start signal...", 75)
        End Sub

        Public Sub StartServer()
            PipeThread = New Thread(Async Sub(pipeName)
                                        IsRunning = True
                                        While True
                                            Try
                                                Dim text As String
                                                Using server = New NamedPipeServerStream(TryCast(pipeName, String), ServerPipeDirection, MaxServerInstances)
                                                    Configuration.SetStatus("Started, All Good", 100)
                                                    Await server.WaitForConnectionAsync(PipeThreadCT.Token)

                                                    Using reader As StreamReader = New StreamReader(server)
                                                        text = reader.ReadToEnd()
                                                    End Using
                                                End Using
                                                If text = EXIT_STRING Then Exit While
                                                TotalReceived += 1
                                                OnReceiveString(text)
                                                If IsRunning = False Then Exit While
                                            Catch ex As Exception
                                                Configuration.SetError(True, ex)
                                                Configuration.Status = "X_X"
                                                RaiseEvent OnException(ex)
                                                Exit While
                                            End Try
                                        End While
                                        Configuration.SetStatus("Server not running.", 50)
                                    End Sub)
            PipeThread.Name = NamedPipeName
            PipeThread.Start(NamedPipeName)
        End Sub

        Protected Overridable Sub OnReceiveString(ByVal text As String)
            Application.Current.Dispatcher.Invoke(Sub()
                                                      RaiseEvent ReceiveString(Me, text)
                                                  End Sub)
        End Sub

        Public Sub StopServer()
            IsRunning = False
            Write(EXIT_STRING)
            Thread.Sleep(30)
        End Sub

        Public Function Write(ByVal text As String, ByVal Optional connectTimeout As Integer = 300) As Boolean
            Using client = New NamedPipeClientStream(NamedPipeName)

                Try
                    client.Connect(connectTimeout)
                Catch ex As Exception
                    Return False
                End Try

                If Not client.IsConnected Then Return False

                Using writer As StreamWriter = New StreamWriter(client)
                    writer.Write(text)
                    writer.Flush()
                End Using
            End Using

            Return True
        End Function

        Public Overrides Function ToString() As String
            Return "MaxServerInstances=" & MaxServerInstances & ";NamedPipeName=" & NamedPipeName & ";IsRunning=" & IsRunning & ";ServerPipeDirection=" & ServerPipeDirection.ToString & "[" & ServerPipeDirection & "]"
        End Function

#Region "IDisposable Support"

        Private disposedValue As Boolean ' To detect redundant calls

        ' IDisposable
        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not disposedValue Then
                If disposing Then

                    StopServer()
                    PipeThreadCT.Cancel()
                End If

            End If
            disposedValue = True
        End Sub

        'Protected Overrides Sub Finalize()
        '    ' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
        '    Dispose(False)
        '    MyBase.Finalize()
        'End Sub

        ' This code added by Visual Basic to correctly implement the disposable pattern.
        Public Sub Dispose() Implements IDisposable.Dispose
            ' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
            Dispose(True)

            ' GC.SuppressFinalize(Me)
        End Sub

#End Region

    End Class
End Namespace