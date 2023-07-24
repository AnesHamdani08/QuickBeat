﻿Imports System
Imports System.Collections.Generic
Imports System.IO
Imports System.Linq
Imports System.Text
Imports System.Threading.Tasks
Imports QuickBeat.SubtitlesParser.Classes.Utils

Namespace SubtitlesParser.Classes.Writers
	''' <summary>
	''' A writer for the Substation Alpha subtitles format.
	''' See http://en.wikipedia.org/wiki/SubStation_Alpha for complete explanations.
	''' Example output:
	''' [Script Info]
	''' ; Script generated by SubtitlesParser v1.4.9.0
	''' ; https://github.com/AlexPoint/SubtitlesParser
	''' ScriptType: v4.00
	''' WrapStyle: 3
	'''
	''' [Events]
	''' Format: Layer, Start, End, Style, Name, MarginL, MarginR, MarginV, Effect, Text
	''' Dialogue: 0,0:18:03.87,0:18:04.23,,,0,0,0,,Oh?
	''' Dialogue: 0,0:18:05.19,0:18:05.90,,,0,0,0,,What was that?
	''' </summary>
	Public Class SsaWriter
		Implements ISubtitlesWriter

		''' <summary>
		''' Write the SSA file header to a text writer 
		''' </summary>
		''' <param name="writer">The TextWriter to write to</param>
		''' <param name="wrapStyle">The <see cref="SsaWrapStyle">wrap style</see> the player should use</param>
		Private Sub WriteHeader(ByVal writer As TextWriter, Optional ByVal wrapStyle As SsaWrapStyle = SsaWrapStyle.None)
			writer.WriteLine(SsaFormatConstants.SCRIPT_INFO_LINE)
			writer.WriteLine($"{SsaFormatConstants.COMMENT} Script generated by SubtitlesParser v{Me.GetType().Assembly.GetName().Version}")
			writer.WriteLine($"{SsaFormatConstants.COMMENT} https://github.com/AlexPoint/SubtitlesParser")
			writer.WriteLine("ScriptType: v4.00") ' the SSA format
			writer.WriteLine($"{SsaFormatConstants.WRAP_STYLE_PREFIX}{wrapStyle}")
			writer.WriteLine() ' blank line between sections

			writer.Flush()
		End Sub

        ''' <summary>
        ''' Asynchronously write the SSA file header to a text writer 
        ''' </summary>
        ''' <param name="writer">The TextWriter to write to</param>
        ''' <param name="wrapStyle">The <see cref="SsaWrapStyle">wrap style</see> the player should use</param>
        Private Async Function WriteHeaderAsync(ByVal writer As TextWriter, Optional ByVal wrapStyle As SsaWrapStyle = SsaWrapStyle.None) As Task
            Await writer.WriteLineAsync(SsaFormatConstants.SCRIPT_INFO_LINE)
            Await writer.WriteLineAsync($"{SsaFormatConstants.COMMENT} Script generated by SubtitlesParser v{Me.GetType().Assembly.GetName().Version}")
            Await writer.WriteLineAsync($"{SsaFormatConstants.COMMENT} https://github.com/AlexPoint/SubtitlesParser")
            Await writer.WriteLineAsync("ScriptType: v4.00") ' the SSA format
            Await writer.WriteLineAsync($"{SsaFormatConstants.WRAP_STYLE_PREFIX}{wrapStyle}")
            Await writer.WriteLineAsync() ' blank line between sections

            Await writer.FlushAsync()
        End Function

        ''' <summary>
        ''' Converts a subtitle item into an SSA formatted dialogue line
        ''' </summary>
        ''' <param name="subtitleItem">The SubtitleItem to convert</param>
        ''' <param name="includeFormatting">if formatting codes should be included when writing the subtitle item lines. Each subtitle item must have the PlaintextLines property set.</param>
        ''' <returns>The full dialogue line</returns>
        Private Function SubtitleItemToDialogueLine(ByVal subtitleItem As SubtitleItem, ByVal includeFormatting As Boolean) As String
			Dim fields(9) As String ' style, name, and effect fields are left blank
			fields(0) = "0" ' layer
			fields(1) = TimeSpan.FromMilliseconds(subtitleItem.StartTime).ToString("h\:mm\:ss\.fff") ' start
			fields(2) = TimeSpan.FromMilliseconds(subtitleItem.EndTime).ToString("h\:mm\:ss\.fff") ' end
			fields(5) = "0" ' left margin
			fields(6) = "0" ' right margin
			fields(7) = "0" ' vertical margin

			' combine all items in the `Lines` property into a single string, with each item being seperated by an SSA newline (\N)
			' check if we should be including formatting or not (default to use formatting if plaintextlines isn't set) 
			Dim lines As List(Of String) = If(includeFormatting = False AndAlso subtitleItem.PlaintextLines IsNot Nothing, subtitleItem.PlaintextLines, subtitleItem.Lines)
			fields(9) = lines.Aggregate(String.Empty, Function(current, line) current & $"{line}\N").TrimEnd("\"c, "N"c)


			Dim builder As New StringBuilder(SsaFormatConstants.DIALOGUE_PREFIX)
            Return builder.Append(String.Join(SsaFormatConstants.SEPARATOR, fields)).ToString()
        End Function

        ''' <summary>
        ''' Write a list of subtitle items to a stream in the SSA/ASS format synchronously
        ''' </summary>
        ''' <param name="stream">The stream to write to</param>
        ''' <param name="subtitleItems">The subtitle items to write</param>
        ''' <param name="includeFormatting">if formatting codes should be included when writing the subtitle item lines. Each subtitle item must have the PlaintextLines property set.</param>
        Public Sub WriteStream(ByVal stream As Stream, ByVal subtitleItems As IEnumerable(Of SubtitleItem), Optional ByVal includeFormatting As Boolean = True) Implements ISubtitlesWriter.WriteStream
            Using writer As TextWriter = New StreamWriter(stream)
                WriteHeader(writer)

                writer.WriteLine(SsaFormatConstants.EVENT_LINE)
                writer.WriteLine("Format: Layer, Start, End, Style, Name, MarginL, MarginR, MarginV, Effect, Text") ' column headers
                For Each item As SubtitleItem In subtitleItems
                    writer.WriteLine(SubtitleItemToDialogueLine(item, includeFormatting))
                Next item
            End Using
        End Sub

        ''' <summary>
        ''' Write a list of subtitle items to a stream in the SSA/ASS format asynchronously
        ''' </summary>
        ''' <param name="stream">The stream to write to</param>
        ''' <param name="subtitleItems">The subtitle items to write</param>
        ''' <param name="includeFormatting">if formatting codes should be included when writing the subtitle item lines. Each subtitle item must have the PlaintextLines property set.</param>
        Public Async Function WriteStreamAsync(ByVal stream As Stream, ByVal subtitleItems As IEnumerable(Of SubtitleItem), Optional ByVal includeFormatting As Boolean = True) As Task Implements ISubtitlesWriter.WriteStreamAsync
            Using writer As New StreamWriter(stream)
                Await WriteHeaderAsync(writer)

                Await writer.WriteLineAsync(SsaFormatConstants.EVENT_LINE)
                Await writer.WriteLineAsync("Format: Layer, Start, End, Style, Name, MarginL, MarginR, MarginV, Effect, Text") ' column headers
                For Each item As SubtitleItem In subtitleItems
                    Await writer.WriteLineAsync(SubtitleItemToDialogueLine(item, includeFormatting))
                Next item
            End Using
        End Function
    End Class
End Namespace