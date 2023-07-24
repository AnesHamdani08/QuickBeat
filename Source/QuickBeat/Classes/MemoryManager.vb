Imports System.ComponentModel

Namespace Classes
    Public Class MemoryManager
        Implements ComponentModel.INotifyPropertyChanged

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

        Private WithEvents PeriodicCleaner As New Forms.Timer With {.Interval = Period.TotalMilliseconds}

        Public Sub Clean()
            For Each meta In Utilities.SharedProperties.Instance.Library?.Where(Function(k) k.IsInUse = False AndAlso k.IsCoverLocked = False AndAlso k.Covers IsNot Nothing)
                meta.FreeCovers()
                TotalCleaned += 1
            Next
        End Sub

        Private Sub PeriodicCleaner_Tick(sender As Object, e As EventArgs) Handles PeriodicCleaner.Tick
            Clean()
        End Sub

    End Class
End Namespace