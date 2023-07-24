Imports System
Imports System.Collections.Generic
Imports System.IO
Imports System.Linq
Imports System.Threading.Tasks

Namespace SubtitlesParser.Classes.Writers
	''' <summary>
	''' A writer for the SubRip subtitles format.
	''' See https://en.wikipedia.org/wiki/SubRip for complete explanations.
	''' Example output:
	''' 1
	''' 00:18:03,875 --> 00:18:04,231
	''' Oh?
	''' 
	''' 2
	''' 00:18:05,194 --> 00:18:05,905
	''' What was that?
	''' </summary>
	Public Class SrtWriter
		Implements ISubtitlesWriter

		''' <summary>
		''' Converts a subtitle item into the lines for an SRT subtitle entry
		''' </summary>
		''' <param name="subtitleItem">The SubtitleItem to convert</param>
		''' <param name="subtitleEntryNumber">The subtitle number for the entry (increments sequentially from 1)</param>
		''' <param name="includeFormatting">if formatting codes should be included when writing the subtitle item lines. Each subtitle item must have the PlaintextLines property set.</param>
		''' <returns>A list of strings to write as an SRT subtitle entry</returns>
		Private Function SubtitleItemToSubtitleEntry(ByVal subtitleItem As SubtitleItem, ByVal subtitleEntryNumber As Integer, ByVal includeFormatting As Boolean) As IEnumerable(Of String)
            ' take the start and end timestamps and format it as a timecode line


            Dim lines As New List(Of String)()
            lines.Add(subtitleEntryNumber.ToString())
            lines.Add(formatTimecodeLine(subtitleItem))
            ' check if we should be including formatting or not (default to use formatting if plaintextlines isn't set) 
            If includeFormatting = False AndAlso subtitleItem.PlaintextLines IsNot Nothing Then
				lines.AddRange(subtitleItem.PlaintextLines)
			Else
				lines.AddRange(subtitleItem.Lines)
			End If

			Return lines
		End Function

        Private Function formatTimecodeLine(subtitleitem As SubtitleItem) As String

            Dim start As TimeSpan = TimeSpan.FromMilliseconds(subtitleitem.StartTime)
            Dim [end] As TimeSpan = TimeSpan.FromMilliseconds(subtitleitem.EndTime)
            Return String.Format("{0,fff} --> {1,fff}", start.ToString("hh\\:mm\\:ss\\"), [end].ToString("hh\\:mm\\:ss\\"))
        End Function
        ''' <summary>
        ''' Write a list of subtitle items to a stream in the SubRip (SRT) format synchronously 
        ''' </summary>
        ''' <param name="stream">The stream to write to</param>
        ''' <param name="subtitleItems">The subtitle items to write</param>
        ''' <param name="includeFormatting">if formatting codes should be included when writing the subtitle item lines. Each subtitle item must have the PlaintextLines property set.</param>
        Public Sub WriteStream(ByVal stream As Stream, ByVal subtitleItems As IEnumerable(Of SubtitleItem), Optional ByVal includeFormatting As Boolean = True) Implements ISubtitlesWriter.WriteStream
            Using writer As TextWriter = New StreamWriter(stream)

                Dim items As List(Of SubtitleItem) = subtitleItems.ToList() ' avoid multiple enumeration since we're using a for instead of foreach
                For i As Integer = 0 To items.Count - 1
                    Dim subtitleItem As SubtitleItem = items(i)
                    Dim lines As IEnumerable(Of String) = SubtitleItemToSubtitleEntry(subtitleItem, i + 1, includeFormatting) ' add one because subtitle entry numbers start at 1 instead of 0
                    For Each line As String In lines
                        writer.WriteLine(line)
                    Next line

                    writer.WriteLine() ' empty line between subtitle entries
                Next i
            End Using
        End Sub

        ''' <summary>
        ''' Write a list of subtitle items to a stream in the SubRip (SRT) format asynchronously 
        ''' </summary>
        ''' <param name="stream">The stream to write to</param>
        ''' <param name="subtitleItems">The subtitle items to write</param>
        ''' <param name="includeFormatting">if formatting codes should be included when writing the subtitle item lines. Each subtitle item must have the PlaintextLines property set.</param>
        Public Async Function WriteStreamAsync(ByVal stream As Stream, ByVal subtitleItems As IEnumerable(Of SubtitleItem), Optional ByVal includeFormatting As Boolean = True) As Task Implements ISubtitlesWriter.WriteStreamAsync
            Using writer As New StreamWriter(stream)

                Dim items As List(Of SubtitleItem) = subtitleItems.ToList() ' avoid multiple enumeration since we're using a for instead of foreach
                For i As Integer = 0 To items.Count - 1
                    Dim subtitleItem As SubtitleItem = items(i)
                    Dim lines As IEnumerable(Of String) = SubtitleItemToSubtitleEntry(subtitleItem, i + 1, includeFormatting) ' add one because subtitle entry numbers start at 1 instead of 0
                    For Each line As String In lines
                        Await writer.WriteLineAsync(line)
                    Next line

                    Await writer.WriteLineAsync() ' empty line between subtitle entries
                Next i
            End Using
        End Function
    End Class
End Namespace