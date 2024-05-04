Option Infer On

Imports System
Imports System.Collections.Generic
Imports System.IO
Imports System.Linq
Imports System.Text
Imports System.Text.RegularExpressions

Namespace SubtitlesParser.Classes.Parsers
	''' <summary>
	''' Parser for the .vtt subtitles files. Does not handle formatting tags within the text; that has to be parsed separately.
	''' 
	''' A .vtt file looks like:
	''' WEBVTT
	''' 
	''' CUE - 1
	''' 00:00:10.500 --> 00:00:13.000
	''' Elephant's Dream
	'''
	''' CUE - 2
	''' 00:00:15.000 --> 00:00:18.000
	''' At the left we can see...
	''' </summary>
	Public Class VttParser
		Implements ISubtitlesParser

		' Properties -----------------------------------------------------------------------

		Private ReadOnly _delimiters() As String = { "-->", "- >", "->" }


		' Constructors --------------------------------------------------------------------

		Public Sub New()
		End Sub


        ' Methods -------------------------------------------------------------------------

        Public Function ParseStream(ByVal vttStream As Stream, ByVal encoding As Encoding) As List(Of SubtitleItem) Implements ISubtitlesParser.ParseStream
            ' test if stream if readable and seekable (just a check, should be good)
            If Not vttStream.CanRead OrElse Not vttStream.CanSeek Then
                Dim message = String.Format("Stream must be seekable and readable in a subtitles parser. " & "Operation interrupted; isSeekable: {0} - isReadable: {1}", vttStream.CanSeek, vttStream.CanSeek)
                Throw New ArgumentException(message)
            End If

            ' seek the beginning of the stream
            vttStream.Position = 0

            Dim reader = New StreamReader(vttStream, encoding, True)

            Dim items = New List(Of SubtitleItem)()
            Dim vttSubParts = GetVttSubTitleParts(reader).ToList()
            If vttSubParts.Any() Then
                For Each vttSubPart In vttSubParts
                    Dim lines = vttSubPart.Split(New String() {Environment.NewLine}, StringSplitOptions.None).Select(Function(s) s.Trim()).Where(Function(l) Not String.IsNullOrEmpty(l)).ToList()

                    Dim item = New SubtitleItem()
                    For Each line In lines
                        If item.StartTime = 0 AndAlso item.EndTime = 0 Then
                            ' we look for the timecodes first
                            Dim startTc As Integer = Nothing
                            Dim endTc As Integer = Nothing
                            Dim success = TryParseTimecodeLine(line, startTc, endTc)
                            If success Then
                                item.StartTime = startTc
                                item.EndTime = endTc
                            End If
                        Else
                            ' we found the timecode, now we get the text
                            item.Lines.Add(line)
                        End If
                    Next line

                    If (item.StartTime <> 0 OrElse item.EndTime <> 0) AndAlso item.Lines.Any() Then
                        ' parsing succeeded
                        item.Index = items.Count
                        items.Add(item)
                    End If
                Next vttSubPart

				If items.Count = 0 Then
					Throw New FormatException("Parsing as VTT returned no VTT part.")
				Else
					Return items
				End If
			Else
				Throw New FormatException("Parsing as VTT returned no VTT part.")
			End If
        End Function

        ''' <summary>
        ''' Enumerates the subtitle parts in a VTT file based on the standard line break observed between them. 
        ''' A VTT subtitle part is in the form:
        ''' 
        ''' CUE - 1
        ''' 00:00:20.000 --> 00:00:24.400
        ''' Altocumulus clouds occur between six thousand
        ''' 
        ''' The first line is optional, as well as the hours in the time codes.
        ''' </summary>
        ''' <param name="reader">The textreader associated with the vtt file</param>
        ''' <returns>An IEnumerable(string) object containing all the subtitle parts</returns>
        Private Iterator Function GetVttSubTitleParts(ByVal reader As TextReader) As IEnumerable(Of String)
			Dim line As String
			Dim sb = New StringBuilder()

			line = reader.ReadLine()
'INSTANT VB WARNING: An assignment within expression was extracted from the following statement:
'ORIGINAL LINE: while ((line = reader.ReadLine()) != null)
			Do While line IsNot Nothing
				If String.IsNullOrEmpty(line.Trim()) Then
					' return only if not empty
					Dim res = sb.ToString().TrimEnd()
					If Not String.IsNullOrEmpty(res) Then
						Yield res
					End If
					sb = New StringBuilder()
				Else
					sb.AppendLine(line)
				End If
				line = reader.ReadLine()
			Loop

			If sb.Length > 0 Then
				Yield sb.ToString()
			End If
		End Function

		Private Function TryParseTimecodeLine(ByVal line As String, ByRef startTc As Integer, ByRef endTc As Integer) As Boolean
			Dim parts = line.Split(_delimiters, StringSplitOptions.None)
			If parts.Length <> 2 Then
				' this is not a timecode line
				startTc = -1
				endTc = -1
				Return False
			Else
				startTc = ParseVttTimecode(parts(0))
				endTc = ParseVttTimecode(parts(1))
				Return True
			End If
		End Function

		''' <summary>
		''' Takes an VTT timecode as a string and parses it into a double (in seconds). A VTT timecode reads as follows: 
		''' 00:00:20.000
		''' or
		''' 00:20.000
		''' </summary>
		''' <param name="s">The timecode to parse</param>
		''' <returns>The parsed timecode as a TimeSpan instance. If the parsing was unsuccessful, -1 is returned (subtitles should never show)</returns>
		Private Function ParseVttTimecode(ByVal s As String) As Integer
			Dim hours As Integer = 0
			Dim minutes As Integer = 0
			Dim seconds As Integer = 0
			Dim milliseconds As Integer = -1
			Dim match = Regex.Match(s, "([0-9]+):([0-9]+):([0-9]+)[,\.]([0-9]+)")
			If match.Success Then
				hours = Integer.Parse(match.Groups(1).Value)
				minutes = Integer.Parse(match.Groups(2).Value)
				seconds = Integer.Parse(match.Groups(3).Value)
				milliseconds = Integer.Parse(match.Groups(4).Value)
			Else
				match = Regex.Match(s, "([0-9]+):([0-9]+)[,\.]([0-9]+)")
				If match.Success Then
					minutes = Integer.Parse(match.Groups(1).Value)
					seconds = Integer.Parse(match.Groups(2).Value)
					milliseconds = Integer.Parse(match.Groups(3).Value)
				End If
			End If

			If milliseconds >= 0 Then
				Dim result As New TimeSpan(0, hours, minutes, seconds, milliseconds)
				Dim nbOfMs = CInt(Math.Truncate(result.TotalMilliseconds))
				Return nbOfMs
			End If

			Return -1
		End Function
	End Class
End Namespace