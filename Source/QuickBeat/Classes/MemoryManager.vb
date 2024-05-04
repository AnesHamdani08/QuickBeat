Imports System.ComponentModel

Namespace Classes
    Public Class MemoryManager
        Implements ComponentModel.INotifyPropertyChanged

        Public Interface ICleanableItem
            Property CleanableConfiguration As CleanConfiguration
        End Interface
        Public Class CleanConfiguration
            Public ReadOnly Property IsBeingUsed As Boolean
                Get
                    Return Users.Count > 0
                End Get
            End Property

            ''' <summary>
            ''' Property of <see cref="Classes.MemoryManager"/>, don't touch!
            ''' </summary>
            ''' <returns></returns>
            ReadOnly Property Users As New List(Of Tuple(Of String, TimeSpan))

            Public Sub Lock(<Runtime.CompilerServices.CallerMemberName> Optional CallerName As String = Nothing)
                Users.Add(New Tuple(Of String, TimeSpan)(CallerName, Now.TimeOfDay))
            End Sub

            Public Sub Unlock(<Runtime.CompilerServices.CallerMemberName> Optional CallerName As String = Nothing)
                Users.Remove(Users.FirstOrDefault(Function(k) k.Item1 = CallerName))
            End Sub
        End Class

        Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged
        Protected Sub OnPropertyChanged(<Runtime.CompilerServices.CallerMemberName> Optional ByVal CallerName As String = Nothing)
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(CallerName))
        End Sub

        Private _IsPeriodicCleaning As Boolean = False
        Property IsPeriodicCleaning As Boolean
            Get
                Return _IsPeriodicCleaning
            End Get
            Set(value As Boolean)
                _IsPeriodicCleaning = value
                PeriodicCleaner.Enabled = value
                OnPropertyChanged()
            End Set
        End Property

        Private _Period As TimeSpan = TimeSpan.FromSeconds(5)
        Property Period As TimeSpan
            Get
                Return _Period
            End Get
            Set(value As TimeSpan)
                _Period = value
                OnPropertyChanged()
            End Set
        End Property

        Private _TotalCleaned As Integer
        Property TotalCleaned As Integer
            Get
                Return _TotalCleaned
            End Get
            Set(value As Integer)
                _TotalCleaned = value
                OnPropertyChanged()
            End Set
        End Property

        Private _ItemTimeout As TimeSpan = TimeSpan.FromSeconds(5)
        Property ItemTimeout As TimeSpan
            Get
                Return _ItemTimeout
            End Get
            Set(value As TimeSpan)
                _ItemTimeout = value
                OnPropertyChanged()
            End Set
        End Property

        Private WithEvents PeriodicCleaner As New Forms.Timer With {.Interval = Period.TotalMilliseconds}
        Private _DoCancel As Boolean = False

        Public Sub Clean()
            Dim nw = Now.TimeOfDay
            For Each meta In Utilities.SharedProperties.Instance.Library?.Where(Function(k) If(k Is Nothing, False, Not k.CleanableConfiguration.Users.Any(Function(l) nw.Subtract(l.Item2) < ItemTimeout) AndAlso k.IsCoverLocked = False AndAlso k.Covers IsNot Nothing))
                If _DoCancel Then _DoCancel = False : Return
                meta.FreeCovers()
                TotalCleaned += 1
            Next
            If Utilities.SharedProperties.Instance.GlobalTagCache.Count > 0 Then 'has items
                Dim tsNow = Now.TimeOfDay
                Dim i = 0
                If _DoCancel Then _DoCancel = False : Return
                Do While i < Utilities.SharedProperties.Instance.GlobalTagCache.Count
                    If _DoCancel Then _DoCancel = False : Return
                    If tsNow.Subtract(Utilities.SharedProperties.Instance.GlobalTagCache.Keys(i).Item1).TotalMilliseconds > My.Settings.APP_TAG_HOLDTIME Then
                        If _DoCancel Then _DoCancel = False : Return
                        Utilities.SharedProperties.Instance.GlobalTagCache.Remove(Utilities.SharedProperties.Instance.GlobalTagCache.Keys(i))
                        TotalCleaned += 1
                    Else
                        i += 1
                    End If
                Loop
            End If
        End Sub

        Private Sub PeriodicCleaner_Tick(sender As Object, e As EventArgs) Handles PeriodicCleaner.Tick
            Clean()
        End Sub

        ''' <summary>
        ''' Cancels the current running operation
        ''' </summary>
        Public Sub Cancel()
            _DoCancel = True
        End Sub

    End Class
End Namespace