Imports System.ComponentModel
Namespace Classes
    <Serializable>
    Public Class StartupItemConfiguration
        Implements INotifyPropertyChanged

        Private _Name As String
        Property Name As String
            Get
                Return _Name
            End Get
            Set(value As String)
                _Name = value
                OnPropertyChanged()
            End Set
        End Property

        Private _Status As String
        Property Status As String
            Get
                Return _Status
            End Get
            Set(value As String)
                _Status = value
                OnPropertyChanged()
            End Set
        End Property

        Private _Progress As Double
        Property Progress As Double
            Get
                Return _Progress
            End Get
            Set(value As Double)
                _Progress = value
                OnPropertyChanged()
            End Set
        End Property

        Private _IsLoading As Boolean
        Property IsLoading As Boolean
            Get
                Return _IsLoading
            End Get
            Set(value As Boolean)
                _IsLoading = value
                OnPropertyChanged()
            End Set
        End Property

        Private _IsLoaded As Boolean
        Property IsLoaded As Boolean
            Get
                Return _IsLoaded
            End Get
            Set(value As Boolean)
                _IsLoaded = value
                OnPropertyChanged()
            End Set
        End Property

        Private _IsErrored As Boolean
        Property IsErrored As Boolean
            Get
                Return _IsErrored
            End Get
            Set(value As Boolean)
                _IsErrored = value
                OnPropertyChanged()
                OnPropertyChanged(NameOf(NotIsErrored))
            End Set
        End Property

        ReadOnly Property NotIsErrored As Boolean
            Get
                Return Not IsErrored
            End Get
        End Property

        Private _Exception As Exception
        Property Exception As Exception
            Get
                Return _Exception
            End Get
            Set(value As Exception)
                _Exception = value
                OnPropertyChanged()
            End Set
        End Property

        Private _IsAddedToList As Boolean = False

        <NonSerialized> Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged

        Sub New(Name As String)
            Me.Name = Name
        End Sub

        Sub OnPropertyChanged(<Runtime.CompilerServices.CallerMemberName> Optional CallerName As String = Nothing)
            If CallerName = NameOf(IsLoaded) Then
                Utilities.SharedProperties.Instance?.RecheckAllInitialized()
            End If
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(CallerName))
        End Sub

        Sub SetStatus(Status As String, Progress As Double)
            If _IsWaiting Then CancelTemporaryStatus()
            Me.Status = Status
            Me.Progress = Progress
            Me.IsLoading = (Progress < 100)
            Me.IsLoaded = (Progress = 100)
            If Progress = 100 Then
                Me.IsErrored = False
            End If
        End Sub
        <NonSerialized> Private _IsWaiting As Boolean = False
        <NonSerialized> Private _WaitingCTS As Threading.CancellationTokenSource
        ''' <summary>
        ''' Set a temporary status and clears after a timeout
        ''' </summary>
        ''' <param name="Status"></param>
        ''' <param name="Progress"></param>
        ''' <param name="Timeout">Delay in milliseconds</param>
        Async Sub SetStatus(Status As String, Progress As Double, Timeout As Double)
            If _IsWaiting Then CancelTemporaryStatus()
            _IsWaiting = True
            _WaitingCTS = New System.Threading.CancellationTokenSource()
            Dim oStatus = _Status
            Dim oProgress = _Progress
            Me.Status = Status
            Me.Progress = Progress
            Me.IsLoading = (Progress < 100)
            Me.IsLoaded = (Progress = 100)
            If Progress = 100 Then
                Me.IsErrored = False
            End If
            Try
                Await Task.Delay(Timeout, _WaitingCTS.Token)
            Catch
            End Try
            _WaitingCTS?.Dispose() : _WaitingCTS = Nothing
            _IsWaiting = False
            SetStatus(oStatus, oProgress)
        End Sub

        ''' <summary>
        ''' Set a temporary status and clears after a timeout
        ''' </summary>
        ''' <param name="Status"></param>
        ''' <param name="Progress"></param>
        ''' <param name="Timeout">Delay in milliseconds</param>
        ''' <param name="NewStatus">The status to set after timeout</param>
        ''' <param name="NewProgress">The progress to set after timeout</param>
        Async Sub SetStatus(Status As String, Progress As Double, Timeout As Double, NewStatus As String, NewProgress As Double)
            If _IsWaiting Then CancelTemporaryStatus()
            _IsWaiting = True
            _WaitingCTS = New System.Threading.CancellationTokenSource()
            Me.Status = Status
            Me.Progress = Progress
            Me.IsLoading = (Progress < 100)
            Me.IsLoaded = (Progress = 100)
            If Progress = 100 Then
                Me.IsErrored = False
            End If
            Try
                Await Task.Delay(Timeout, _WaitingCTS.Token)
            Catch
            End Try
            _WaitingCTS?.Dispose() : _WaitingCTS = Nothing
            _IsWaiting = False
            SetStatus(NewStatus, NewProgress)
        End Sub

        Public Sub CancelTemporaryStatus()
            _WaitingCTS?.Cancel()
            _WaitingCTS?.Dispose() : _WaitingCTS = Nothing
        End Sub
        Sub SetError(IsErrored As Boolean, Exception As Exception)
            Me.IsErrored = IsErrored
            Me.Exception = Exception
            Me.IsLoaded = Not IsErrored
        End Sub

        Public Overrides Function ToString() As String
            Return $"{Name}: {Status}[{IsLoading}]: {Progress}, Error[{IsErrored}]: {If(Exception Is Nothing, "None", Exception.Message)}"
        End Function
    End Class
End Namespace