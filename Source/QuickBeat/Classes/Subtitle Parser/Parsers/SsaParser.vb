Option Infer On

Imports System
Imports System.Collections.Generic
Imports System.IO
Imports System.Linq
Imports System.Text
Imports System.Text.RegularExpressions
Imports QuickBeat.SubtitlesParser.Classes.Utils

Namespace SubtitlesParser.Classes.Parsers
    ''' <summary>
    ''' A parser for the SubStation Alpha subtitles format.
    ''' See http://en.wikipedia.org/wiki/SubStation_Alpha for complete explanations.
    ''' Ex:
    ''' [Script Info]
    ''' ; This is a Sub Station Alpha v4 script.
    ''' ; For Sub Station Alpha info and downloads,
    ''' ; go to http://www.eswat.demon.co.uk/
    ''' Title: Neon Genesis Evangelion - Episode 26 (neutral Spanish)
    ''' Original Script: RoRo
    ''' Script Updated By: version 2.8.01
    ''' ScriptType: v4.00
    ''' Collisions: Normal
    ''' PlayResY: 600
    ''' PlayDepth: 0
    ''' Timer: 100,0000
    '''  
    ''' [V4 Styles]
    ''' Format: Name, Fontname, Fontsize, PrimaryColour, SecondaryColour, TertiaryColour, BackColour, Bold, Italic, BorderStyle, Outline, Shadow, Alignment, MarginL, MarginR, MarginV, AlphaLevel, Encoding
    ''' Style: DefaultVCD, Arial,28,11861244,11861244,11861244,-2147483640,-1,0,1,1,2,2,30,30,30,0,0
    '''   
    ''' [Events]
    ''' Format: Marked, Start, End, Style, Name, MarginL, MarginR, MarginV, Effect, Text
    ''' Dialogue: Marked=0,0:00:01.18,0:00:06.85,DefaultVCD, NTP,0000,0000,0000,,{\pos(400,570)}Like an angel with pity on nobody
    ''' </summary>
    Public Class SsaParser
        Implements ISubtitlesParser

        ' Methods ------------------------------------------------------------------

        Public Function ParseStream(ByVal ssaStream As Stream, ByVal encoding As Encoding) As List(Of SubtitleItem) Implements ISubtitlesParser.ParseStream
            ' test if stream if readable and seekable (just a check, should be good)
            If Not ssaStream.CanRead OrElse Not ssaStream.CanSeek Then
                Dim message = String.Format("Stream must be seekable and readable in a subtitles parser. " & "Operation interrupted; isSeekable: {0} - isReadable: {1}", ssaStream.CanSeek, ssaStream.CanRead)
                Throw New ArgumentException(message)
            End If

            ' seek the beginning of the stream
            ssaStream.Position = 0

            Dim reader = New StreamReader(ssaStream, encoding, True)

            ' default wrap style to none if the header section doesn't contain a wrap style definition (very possible since it wasn't present in SSA, only ASS) 
            Dim wrapStyle As SsaWrapStyle = SsaWrapStyle.None

            Dim line = reader.ReadLine()
            Dim lineNumber = 1
            ' read the line until the [Events] section
            Do While line IsNot Nothing AndAlso line <> SsaFormatConstants.EVENT_LINE
                If line.StartsWith(SsaFormatConstants.WRAP_STYLE_PREFIX) Then
                    ' get the wrap style
                    ' the raw string is the second array item after splitting the line at `:` (which we know will be present since it's
                    ' included in the `WRAP_STYLE_PREFIX` const), so trim the space off the beginning of that item, and parse that string into the enum 
                    wrapStyle = line.Split(":"c)(1).TrimStart().FromString()
                End If

                line = reader.ReadLine()
                lineNumber += 1
            Loop

            If line IsNot Nothing Then
                ' we are at the event section
                Dim headerLine = reader.ReadLine()
                If Not String.IsNullOrEmpty(headerLine) Then
                    Dim columnHeaders = headerLine.Split(SsaFormatConstants.SEPARATOR).Select(Function(head) head.Trim()).ToList()

                    Dim startIndexColumn = columnHeaders.IndexOf(SsaFormatConstants.START_COLUMN)
                    Dim endIndexColumn = columnHeaders.IndexOf(SsaFormatConstants.END_COLUMN)
                    Dim textIndexColumn = columnHeaders.IndexOf(SsaFormatConstants.TEXT_COLUMN)

                    If startIndexColumn > 0 AndAlso endIndexColumn > 0 AndAlso textIndexColumn > 0 Then
                        Dim items = New List(Of SubtitleItem)()

                        line = reader.ReadLine()
                        Do While line IsNot Nothing
                            If Not String.IsNullOrEmpty(line) Then
                                Dim columns = line.Split(SsaFormatConstants.SEPARATOR)
                                Dim startText = columns(startIndexColumn)
                                Dim endText = columns(endIndexColumn)


                                Dim textLine = String.Join(",", columns.Skip(textIndexColumn))

                                Dim start = ParseSsaTimecode(startText)
                                Dim [end] = ParseSsaTimecode(endText)

                                If start > 0 AndAlso [end] > 0 AndAlso Not String.IsNullOrEmpty(textLine) Then
                                    Dim lines As List(Of String)
                                    Select Case wrapStyle
                                        Case SsaWrapStyle.Smart, SsaWrapStyle.SmartWideLowerLine, SsaWrapStyle.EndOfLine
                                            ' according to the spec doc: 
                                            ' `\n` is ignored by SSA if smart-wrapping (and therefore smart with wider lower line) is enabled
                                            ' end-of-line word wrapping: only `\N` breaks
                                            lines = textLine.Split("\N").ToList()
                                        Case SsaWrapStyle.None
                                            ' the default value of the variable is None, which breaks on either `\n` or `\N`

                                            ' according to the spec doc: 
                                            ' no word wrapping: `\n` `\N` both breaks
                                            lines = Regex.Split(textLine, "(?:\\n)|(?:\\N)").ToList() ' regex because there isn't an overload to take an array of strings to split on
                                        Case Else
                                            Throw New ArgumentOutOfRangeException()
                                    End Select

                                    ' trim any spaces from the start of a line (happens when a subtitler includes a space after a newline char ie `this is\N two lines` instead of `this is\Ntwo lines`)
                                    ' this doesn't actually matter for the SSA/ASS format, however if you were to want to convert from SSA/ASS to a format like SRT, it could lead to spaces preceding the second line, which looks funny 
                                    lines = lines.Select(Function(_line) _line.TrimStart()).ToList()

                                    Dim item = New SubtitleItem() With {
                                        .StartTime = start,
                                        .EndTime = [end],
                                        .Lines = lines,
                                        .PlaintextLines = lines.Select(Function(subtitleLine) Regex.Replace(subtitleLine, "\{.*?\}", String.Empty)).ToList(),
                                        .Index = items.Count
                                    }
                                    items.Add(item)
                                End If
                            End If
                            line = reader.ReadLine()
                        Loop

                        If items.Any() Then
                            Return items
                        Else
                            Throw New ArgumentException("Stream is not in a valid Ssa format")
                        End If
                    Else
                        Dim message = String.Format("Couldn't find all the necessary columns " & "headers ({0}, {1}, {2}) in header line {3}", SsaFormatConstants.START_COLUMN, SsaFormatConstants.END_COLUMN, SsaFormatConstants.TEXT_COLUMN, headerLine)
                        Throw New ArgumentException(message)
                    End If
                Else
                    Dim message = String.Format("The header line after the line '{0}' was null -> " & "no need to continue parsing", line)
                    Throw New ArgumentException(message)
                End If
            Else
                Dim message = String.Format("We reached line '{0}' with line number #{1} without finding to " & "Event section ({2})", line, lineNumber, SsaFormatConstants.EVENT_LINE)
                Throw New ArgumentException(message)
            End If
        End Function

        ''' <summary>
        ''' Takes an SRT timecode as a string and parses it into a double (in seconds). A SRT timecode reads as follows: 
        ''' 00:00:20,000
        ''' </summary>
        ''' <param name="s">The timecode to parse</param>
        ''' <returns>The parsed timecode as a TimeSpan instance. If the parsing was unsuccessful, -1 is returned (subtitles should never show)</returns>
        Private Function ParseSsaTimecode(ByVal s As String) As Integer
            Dim result As TimeSpan = Nothing

            If TimeSpan.TryParse(s, result) Then
                Dim nbOfMs = CInt(Math.Truncate(result.TotalMilliseconds))
                Return nbOfMs
            Else
                Return -1
            End If
        End Function
    End Class
End Namespace