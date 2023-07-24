Imports System.ComponentModel
Imports System.Globalization

Namespace Classes
    Public Class SleepTimer
        Implements INotifyPropertyChanged

        Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged
        Public Event Finished(ByRef handled As Boolean)

        Protected Sub OnPropertyChanged(<Runtime.CompilerServices.CallerMemberName> Optional CallerName As String = Nothing)
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(CallerName))
        End Sub

        Private WithEvents _Timer As New Forms.Timer With {.Interval = 1000, .Enabled = IsEnabled}
        Private Sub _Timer_Tick() Handles _Timer.Tick
            If UpdateUIConstantly Then
                OnPropertyChanged(NameOf(TimeLeft))
                OnPropertyChanged(NameOf(Progress))
            End If
            If TimeLeft.TotalSeconds < 0 Then
                IsEnabled = False
                IsFinished = True
                Dim handled As Boolean = False
                RaiseEvent Finished(handled)
                If handled Then Return
                Select Case Action
                    Case TimerAction.Notify
                        Utilities.SharedProperties.Instance.Notification(Utilities.ResourceResolver.Strings.APPNAME, Utilities.ResourceResolver.Strings.QUERY_TIMEREND, HandyControl.Data.NotifyIconInfoType.Info)
                    Case TimerAction.Pause
                        Utilities.SharedProperties.Instance.Player?.Pause()
                    Case TimerAction.Exit
                        Application.Current.Shutdown(0)
                End Select
            End If
        End Sub

        Private _UpdateUIConstantly As Boolean = True
        Property UpdateUIConstantly As Boolean
            Get
                Return _UpdateUIConstantly
            End Get
            Set(value As Boolean)
                _UpdateUIConstantly = value
                OnPropertyChanged()
            End Set
        End Property

        Private _SelectedDate As Date = Now
        Property SelectedDate As Date
            Get
                Return _SelectedDate
            End Get
            Set(value As Date)
                _SelectedDate = value
                OnPropertyChanged()
            End Set
        End Property

        Private _Hours As Integer
        Property Hours As Integer
            Get
                Return _Hours
            End Get
            Set(value As Integer)
                _Hours = value
                UpdateSelectedDate()
                OnPropertyChanged()
            End Set
        End Property

        Private _Minutes As Integer
        Property Minutes As Integer
            Get
                Return _Minutes
            End Get
            Set(value As Integer)
                _Minutes = value
                UpdateSelectedDate()
                OnPropertyChanged()
            End Set
        End Property

        Private _Seconds As Integer
        Property Seconds As Integer
            Get
                Return _Seconds
            End Get
            Set(value As Integer)
                _Seconds = value
                UpdateSelectedDate()
                OnPropertyChanged()
            End Set
        End Property

        ReadOnly Property Progress As Integer
            Get
                If IsEnabled AndAlso TimeOfStart <> Date.MinValue Then
                    Dim range = SelectedDate.Subtract(TimeOfStart)
                    Dim percentage = (TimeLeft.TotalSeconds / range.TotalSeconds) * 100
                    Return If(Double.IsInfinity(percentage), 0, 100 - percentage)
                Else
                    Return 0
                End If
            End Get
        End Property

        Private _TimeOfStart As Date = Date.MinValue
        Property TimeOfStart As Date
            Get
                Return _TimeOfStart
            End Get
            Set(value As Date)
                _TimeOfStart = value
                OnPropertyChanged()
            End Set
        End Property

        ReadOnly Property TimeLeft As TimeSpan
            Get
                Return SelectedDate.Subtract(Now)
            End Get
        End Property

        Private _IsEnabled As Boolean
        Property IsEnabled As Boolean
            Get
                Return _IsEnabled
            End Get
            Set(value As Boolean)
                If value <> _IsEnabled Then
                    IsFinished = False
                    If value Then
                        TimeOfStart = Now
                        UpdateSelectedDate()
                    End If
                End If
                _IsEnabled = value
                _Timer.Enabled = value
                OnPropertyChanged()
            End Set
        End Property

        Private _IsFinished As Boolean = True
        Property IsFinished As Boolean
            Get
                Return _IsFinished
            End Get
            Set(value As Boolean)
                _IsFinished = value
                If value Then
                    _IsEnabled = False
                    OnPropertyChanged(NameOf(IsEnabled))
                End If
                OnPropertyChanged()
            End Set
        End Property

        Private _Action As TimerAction
        Property Action As TimerAction
            Get
                Return _Action
            End Get
            Set(value As TimerAction)
                _Action = value
                OnPropertyChanged()
            End Set
        End Property

        Private _StartCommand As New StartCommandDelegate
        ReadOnly Property StartCommand As StartCommandDelegate
            Get
                Return _StartCommand
            End Get
        End Property

        Private _StopCommand As New StopCommandDelegate
        ReadOnly Property StopCommand As StopCommandDelegate
            Get
                Return _StopCommand
            End Get
        End Property

        Private Sub UpdateSelectedDate()
            SelectedDate = Now.Add(New TimeSpan(Hours, Minutes, Seconds))
        End Sub
#Region "Classes"
        Public Enum TimerAction
            None
            Notify
            Pause
            [Exit]
        End Enum
#End Region
#Region "WPF Support"
        Public Class StartCommandDelegate
            Implements ICommand

            Public Event CanExecuteChanged As EventHandler Implements ICommand.CanExecuteChanged
            Sub New()
                AddHandler CommandManager.RequerySuggested, New EventHandler(Sub(sender As Object, e As EventArgs)
                                                                                 RaiseEvent CanExecuteChanged(Me, New EventArgs())
                                                                             End Sub)
            End Sub

            Public Sub Execute(parameter As Object) Implements ICommand.Execute
                With CType(parameter, SleepTimer)
                    If .TimeLeft.TotalSeconds = 0 Then Return
                    .IsEnabled = True
                End With
            End Sub

            Public Function CanExecute(parameter As Object) As Boolean Implements ICommand.CanExecute
                Return (parameter IsNot Nothing) AndAlso (TypeOf parameter Is SleepTimer)
            End Function
        End Class

        Public Class StopCommandDelegate
            Implements ICommand

            Public Event CanExecuteChanged As EventHandler Implements ICommand.CanExecuteChanged
            Sub New()
                AddHandler CommandManager.RequerySuggested, New EventHandler(Sub(sender As Object, e As EventArgs)
                                                                                 RaiseEvent CanExecuteChanged(Me, New EventArgs())
                                                                             End Sub)
            End Sub

            Public Sub Execute(parameter As Object) Implements ICommand.Execute
                CType(parameter, SleepTimer).IsEnabled = False
            End Sub

            Public Function CanExecute(parameter As Object) As Boolean Implements ICommand.CanExecute
                Return (parameter IsNot Nothing) AndAlso (TypeOf parameter Is SleepTimer)
            End Function
        End Class
#End Region
    End Class
End Namespace
Namespace Converters
    Public Class TimerActionToIntegerConverter
        Implements IValueConverter

        Public Function Convert(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.Convert
            Dim _v As Integer
            If Integer.TryParse(value, _v) Then
                Return _v
            Else
                Return value
            End If
        End Function

        Public Function ConvertBack(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.ConvertBack
            Return CType(value, Classes.SleepTimer.TimerAction)
        End Function
    End Class
End Namespace