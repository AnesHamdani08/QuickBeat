Option Infer On

Imports System
Imports System.Collections.Generic

Namespace SubtitlesParser.Classes
	Public Class SubtitleItem

		'Properties------------------------------------------------------------------

		''' <summary>
		''' Start time in milliseconds.
		''' </summary>
		Public Property StartTime() As Integer
		Public ReadOnly Property StartTimeString As String
			Get
				Dim ts = TimeSpan.FromMilliseconds(StartTime)
				Return $"{If(ts.Minutes < 10, 0, "")}{ts.Minutes}:{If(ts.Seconds < 10, 0, "")}{ts.Seconds}"
			End Get
		End Property
		''' <summary>
		''' End time in milliseconds.
		''' </summary>
		Public Property EndTime() As Integer
		''' <summary>
		''' The raw subtitle string from the file
		''' May include formatting
		''' </summary>
		Public Property Lines() As List(Of String)
		''' <summary>
		''' The plain-text string from the file
		''' Does not include formatting
		''' </summary>
		Public Property PlaintextLines() As List(Of String)
		Public ReadOnly Property JoinedLines As String
			Get
				Return String.Join(Environment.NewLine, PlaintextLines)
			End Get
		End Property
		''' <summary>
		''' THe index of the subtitle in the list
		''' </summary>
		''' <returns></returns>
		Public Property Index As Integer

        'Constructors-----------------------------------------------------------------

        ''' <summary>
        ''' The empty constructor
        ''' </summary>
        Public Sub New()
			Me.Lines = New List(Of String)()
			Me.PlaintextLines = New List(Of String)()
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