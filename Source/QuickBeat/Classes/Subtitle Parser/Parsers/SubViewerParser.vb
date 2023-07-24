Option Infer On

Imports System
Imports System.Collections.Generic
Imports System.IO
Imports System.Linq
Imports System.Text
Imports System.Text.RegularExpressions

Namespace SubtitlesParser.Classes.Parsers
	''' <summary>
	''' Parser for SubViewer .sub subtitles files
	''' 
	''' [INFORMATION]
	''' ....
	''' 
	''' 00:04:35.03,00:04:38.82
	''' Hello guys... please sit down...
	''' 
	''' 00:05:00.19,00:05:03.47
	''' M. Franklin,[br]are you crazy?
	''' 
	''' see https://en.wikipedia.org/wiki/SubViewer
	''' </summary>
	Public Class SubViewerParser
		Implements ISubtitlesParser

		' Properties ----------------------------------------------------------

		Private Const FirstLine As String = "[INFORMATION]"
		Private Const MaxLineNumberForItems As Short = 20

		Private ReadOnly _timestampRegex As New Regex("\d{2}:\d{2}:\d{2}\.\d{2},\d{2}:\d{2}:\d{2}\.\d{2}", RegexOptions.Compiled)
		Private Const TimecodeSeparator As Char = ","c


        ' Methods -------------------------------------------------------------

        Public Function ParseStream(ByVal subStream As Stream, ByVal encoding As Encoding) As List(Of SubtitleItem) Implements ISubtitlesParser.ParseStream
            ' seek the beginning of the stream
            subStream.Position = 0
            Dim reader = New StreamReader(subStream, encoding, True)

            'INSTANT VB NOTE: The variable firstLine was renamed since Visual Basic does not handle local variables named the same as class members well:
            Dim firstLine_Conflict = reader.ReadLine()
            If firstLine_Conflict = FirstLine Then
                Dim line = reader.ReadLine()
                Dim lineNumber = 2
                Do While line IsNot Nothing AndAlso lineNumber <= MaxLineNumberForItems AndAlso Not IsTimestampLine(line)
                    line = reader.ReadLine()
                    lineNumber += 1
                Loop

                ' first relevant line should be a timecode
                If line IsNot Nothing AndAlso lineNumber <= MaxLineNumberForItems AndAlso IsTimestampLine(line) Then
                    ' we parse all the lines
                    Dim items = New List(Of SubtitleItem)()

                    Dim timeCodeLine = line
                    Dim textLines = New List(Of String)()

                    Do While line IsNot Nothing
                        line = reader.ReadLine()
                        If IsTimestampLine(line) Then
                            ' store previous item
                            Dim timeCodes = ParseTimecodeLine(timeCodeLine)
                            Dim start = timeCodes.Item1
                            Dim [end] = timeCodes.Item2

                            If start > 0 AndAlso [end] > 0 AndAlso textLines.Any() Then
                                items.Add(New SubtitleItem() With {
                                    .StartTime = start,
                                    .EndTime = [end],
                                    .Lines = textLines,
                                    .Index = items.Count
                                })
                            End If

                            ' reset timecode line and text lines
                            timeCodeLine = line
                            textLines = New List(Of String)()
                        ElseIf Not String.IsNullOrEmpty(line) Then
                            ' it's a text line
                            textLines.Add(line)
                        End If
                    Loop

                    ' store last line if necessary
                    Dim lastTimeCodes = ParseTimecodeLine(timeCodeLine)
                    Dim lastStart = lastTimeCodes.Item1
                    Dim lastEnd = lastTimeCodes.Item2
                    If lastStart > 0 AndAlso lastEnd > 0 AndAlso textLines.Any() Then
                        items.Add(New SubtitleItem() With {
                            .StartTime = lastStart,
                            .EndTime = lastEnd,
                            .Lines = textLines
                        })
                    End If

                    If items.Any() Then
                        Return items
                    Else
                        Throw New ArgumentException("Stream is not in a valid SubViewer format")
                    End If
                Else
                    Dim message = String.Format("Couldn't find the first timestamp line in the current sub file. " & "Last line read: '{0}', line number #{1}", line, lineNumber)
                    Throw New ArgumentException(message)
                End If
            Else
                Throw New ArgumentException("Stream is not in a valid SubViewer format")
            End If
        End Function

        Private Function ParseTimecodeLine(ByVal line As String) As Tuple(Of Integer, Integer)
			Dim parts = line.Split(TimecodeSeparator)
			If parts.Length = 2 Then
				Dim start = ParseTimecode(parts(0))
				Dim [end] = ParseTimecode(parts(1))
				Return New Tuple(Of Integer, Integer)(start, [end])
			Else
				Dim message = String.Format("Couldn't parse the timecodes in line '{0}'", line)
				Throw New ArgumentException(message)
			End If
		End Function

		''' <summary>
		''' Takes an SRT timecode as a string and parses it into a double (in seconds). A SRT timecode reads as follows: 
		''' 00:00:20,000
		''' </summary>
		''' <param name="s">The timecode to parse</param>
		''' <returns>The parsed timecode as a TimeSpan instance. If the parsing was unsuccessful, -1 is returned (subtitles should never show)</returns>
		Private Function ParseTimecode(ByVal s As String) As Integer
			Dim result As TimeSpan = Nothing

			If TimeSpan.TryParse(s, result) Then
				Dim nbOfMs = CInt(Math.Truncate(result.TotalMilliseconds))
				Return nbOfMs
			Else
				Return -1
			End If
		End Function

		''' <summary>
		''' Tests if the current line is a timestamp line
		''' </summary>
		''' <param name="line">The subtitle file line</param>
		''' <returns>True if it's a timestamp line, false otherwise</returns>
		Private Function IsTimestampLine(ByVal line As String) As Boolean
			If String.IsNullOrEmpty(line) Then
				Return False
			End If
			Dim isMatch = _timestampRegex.IsMatch(line)
			Return isMatch
		End Function
	End Class
End Namespace