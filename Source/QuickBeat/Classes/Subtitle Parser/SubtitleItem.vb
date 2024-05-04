Option Infer On

Imports System
Imports System.Collections.Generic
Imports System.ComponentModel

Namespace SubtitlesParser.Classes
	Public Class SubtitleItem
		Implements ComponentModel.INotifyPropertyChanged

		'Properties------------------------------------------------------------------
		Private _StartItem As UInteger
		''' <summary>
		''' Start time in milliseconds.
		''' </summary>
		Public Property StartTime As UInteger
			Get
				Return _StartItem
			End Get
			Set(value As UInteger)
				_StartItem = value
				OnPropertyChanged()
				OnPropertyChanged(NameOf(StartTimeString))
			End Set
		End Property
		Public ReadOnly Property StartTimeString As String
			Get
				Dim ts = TimeSpan.FromMilliseconds(StartTime)
				Return $"{If(ts.Minutes < 10, 0, "")}{ts.Minutes}:{If(ts.Seconds < 10, 0, "")}{ts.Seconds}"
			End Get
		End Property

		Private _EndTime As UInteger
		''' <summary>
		''' End time in milliseconds.
		''' </summary>
		Public Property EndTime As UInteger
			Get
				Return _EndTime
			End Get
			Set(value As UInteger)
				_EndTime = value
				OnPropertyChanged()
			End Set
		End Property

		Private _Lines As List(Of String)
		''' <summary>
		''' The raw subtitle string from the file
		''' May include formatting
		''' </summary>
		Public Property Lines() As List(Of String)
			Get
				Return _Lines
			End Get
			Set(value As List(Of String))
				_Lines = value
				OnPropertyChanged()
			End Set
		End Property

		Private _PlaintextLines As List(Of String)
		''' <summary>
		''' The plain-text string from the file
		''' Does not include formatting
		''' </summary>
		Public Property PlaintextLines As List(Of String)
			Get
				Return _PlaintextLines
			End Get
			Set(value As List(Of String))
				_PlaintextLines = value
				OnPropertyChanged()
				OnPropertyChanged(NameOf(JoinedLines))
			End Set
		End Property

		Public ReadOnly Property JoinedLines As String
			Get
				Return String.Join(Environment.NewLine, PlaintextLines)
			End Get
		End Property

		Private _Index As Integer
		''' <summary>
		''' The index of the subtitle in the list
		''' </summary>
		''' <returns></returns>
		Public Property Index As Integer
			Get
				Return _Index
			End Get
			Set(value As Integer)
				_Index = value
				OnPropertyChanged()
			End Set
		End Property

		Private _Singer As String
		''' <summary>
		''' The name of the singer if available (part of LRC Support)
		''' </summary>
		''' <returns></returns>
		Public Property Singer As String
			Get
				Return _Singer
			End Get
			Set(value As String)
				_Singer = value
				OnPropertyChanged()
			End Set
		End Property

		Private _IsCurrent As Boolean
		Public Property IsCurrent As Boolean
			Get
				Return _IsCurrent
			End Get
			Set(value As Boolean)
				_IsCurrent = value
				OnPropertyChanged()
			End Set
		End Property

		'Constructors-----------------------------------------------------------------

		''' <summary>
		''' The empty constructor
		''' </summary>
		Public Sub New()
			Me.Lines = New List(Of String)()
			Me.PlaintextLines = New List(Of String)()
		End Sub

		Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged
		Public Sub OnPropertyChanged(<Runtime.CompilerServices.CallerMemberName> Optional CallerName As String = Nothing)
			RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(CallerName))
		End Sub

		' Methods --------------------------------------------------------------------------

		Public Overrides Function ToString() As String
			Dim startTs = New TimeSpan(0, 0, 0, 0, StartTime)
			Dim endTs = New TimeSpan(0, 0, 0, 0, EndTime)

			Dim res = String.Format("{0} --> {1}: {2}", startTs.ToString("G"), endTs.ToString("G"), String.Join(Environment.NewLine, Lines))
			Return res
		End Function

	End Class
End Namespace