Imports System.IO
Imports System.Text
Imports System.Text.RegularExpressions
'OG Code
Namespace SubtitlesParser.Classes.Parsers
    Public Class LRCParser
        Implements ISubtitlesParser

        Public Enum MetadataType
            Undefined
            Artist
            Album
            Title
            Length
            Version
            Software
            LyricsAuthor
            FileAuthor
            Offset
        End Enum

        Private _Metadata As New List(Of KeyValuePair(Of String, String))
        ''' <summary>
        ''' To be loaded using <see cref="ParseStream(Stream, Encoding)"/>
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Metadata As List(Of KeyValuePair(Of String, String))
            Get
                Return _Metadata
            End Get
        End Property

        Public Function ParseStream(lrcstream As Stream, encoding As Encoding) As List(Of SubtitleItem) Implements ISubtitlesParser.ParseStream
            ' test if stream if readable and seekable (just a check, should be good)
            If Not lrcstream.CanRead OrElse Not lrcstream.CanSeek Then
                Dim message = String.Format("Stream must be seekable and readable in a subtitles parser. " & "Operation interrupted; isSeekable: {0} - isReadable: {1}", lrcstream.CanSeek, lrcstream.CanSeek)
                Throw New ArgumentException(message)
            End If

            ' seek the beginning of the stream
            lrcstream.Position = 0

            Dim reader = New StreamReader(lrcstream, encoding, True)
            Dim items = New List(Of SubtitleItem)()
            Dim helper As New QuickBeat.Utilities.SettingsHelper()
            Dim line As String
            line = reader.ReadLine()

            Do While line IsNot Nothing
                If String.IsNullOrEmpty(line.Trim) Then
                    line = reader.ReadLine()
                    Continue Do
                End If
                Console.WriteLine("Parsing: " & line)
                'Check and parse metadata or section header
                If line.StartsWith("[") AndAlso line.EndsWith("]") Then
                    Dim tLine = line.TrimStart(New Char() {"["}).TrimEnd(New Char() {"]"})
                    Dim sLine = tLine.Split(":")
                    If sLine.Length > 2 Then 'probably the latter text contains a ":"
                        sLine = New String() {sLine(0), String.Join(":", sLine.Skip(1))}
                    End If
                    'Found metadata
                    Dim type = ParseMetadataType(sLine.FirstOrDefault)
                    If type = MetadataType.Undefined Then 'section start
                        helper.StartSection(tLine)
                    Else 'metadata
                        helper.Metadata.Add(type.ToString, sLine.LastOrDefault)
                    End If
                    line = reader.ReadLine()
                    Continue Do
                End If
                'Parse lyric                
                Dim timestamp = line.Substring(0, line.LastIndexOf("]") + 1)
                Dim lyric = line.Substring(line.LastIndexOf("]") + 1)
                Dim timestamps = Regex.Matches(timestamp, "\[(.*?)\]") 'in case of repeating lyric                
                If timestamps.Count = 1 Then
                    helper.AddItem(timestamps.Item(0).Value, lyric)
                Else
                    Dim sItems As New List(Of KeyValuePair(Of String, String)) 'check list for pass-thru , incase there is sections header after start time value
                    For i As Integer = 0 To timestamps.Count - 1
                        If Not (timestamps.Item(i).Value.Contains(":") AndAlso timestamps.Item(i).Value.Contains(".")) Then 'section header
                            sItems.Insert(0, New KeyValuePair(Of String, String)(timestamps.Item(i).Value.TrimStart(New Char() {"["}).TrimEnd(New Char() {"]"}), lyric))
                            Continue For
                        End If
                        helper.AddItem(timestamps(i).Value, lyric)
                    Next
                    If Not sItems.FirstOrDefault.Key.Contains("[") Then 'found section header
                        helper.StartSection(sItems.FirstOrDefault.Key)
                        For i As Integer = 1 To sItems.Count - 1
                            helper.AddItem(sItems(i).Key, lyric)
                        Next
                    End If
                End If
                line = reader.ReadLine()
            Loop
            helper.EndSection() 'flush
            'helper now contains data

            Metadata.Clear()
            For Each meta In helper.Metadata
                Metadata.Add(New KeyValuePair(Of String, String)(meta.Key, meta.Value))
            Next
            Dim ix = 0
            For Each item In helper
                Dim sTt = item.Key.TrimStart(New Char() {"["}).TrimEnd(New Char() {"]"}).Split(":")
                Dim sTime = (New TimeSpan(0, 0, sTt(0), sTt(1).Split(".")(0), sTt(1).Split(".")(1))).TotalMilliseconds
                Dim iSub As New SubtitleItem With {.Index = ix, .StartTime = sTime, .EndTime = sTime}
                iSub.Lines.Add(item.Value)
                iSub.PlaintextLines.Add(item.Value)
                items.Add(iSub)
                ix += 1
            Next
            For Each section In helper.RawSections
                For Each item In section.Value
                    Dim sTt = item.Key.TrimStart(New Char() {"["}).TrimEnd(New Char() {"]"}).Split(":")
                    Dim sTime = (New TimeSpan(0, 0, sTt(0), sTt(1).Split(".")(0), sTt(1).Split(".")(1))).TotalMilliseconds
                    Dim iSub As New SubtitleItem With {.Index = ix, .StartTime = sTime, .EndTime = sTime}
                    iSub.Singer = section.Key
                    iSub.Lines.Add(item.Value)
                    iSub.PlaintextLines.Add(item.Value)
                    items.Add(iSub)
                    ix += 1
                Next
            Next

            If items.Any() Then
                Return items
            Else
                Throw New ArgumentException("Stream is not in a valid Srt format")
            End If
        End Function

        ''' <summary>
        ''' Attempts to read the metadata of the LRC file and stops when reaching the first lyric.
        ''' The <paramref name="reader"/> will contain the first lyric
        ''' </summary>
        ''' <param name="reader"></param>
        ''' <returns>
        ''' A <see cref="IEnumerable"/> Of <see cref="KeyValuePair(Of TKey, TValue)"/> Containing Metadata Type as Key and Metadata Value as Value
        ''' </returns>
        Private Iterator Function GetLRCMetadata(ByVal reader As TextReader) As IEnumerable(Of KeyValuePair(Of String, String))
            Dim line As String

            line = reader.ReadLine()

            Do While line IsNot Nothing
                If String.IsNullOrEmpty(line.Trim) Then Continue Do
                If line.StartsWith("[") AndAlso line.EndsWith("]") Then
                    Dim sLine = line.TrimStart(New Char() {"["}).TrimEnd(New Char() {"]"}).Split(":")
                    If sLine.Length > 2 Then 'probably the latter text contains a ":"
                        sLine = New String() {sLine(0), String.Join(":", sLine.Skip(1))}
                    End If
                    Yield New KeyValuePair(Of String, String)(sLine.FirstOrDefault, sLine.LastOrDefault)
                End If
                If line.StartsWith("[") AndAlso Not line.EndsWith("]") Then Exit Do 'Reached lyrics data
                line = reader.ReadLine()
            Loop
        End Function


        Private Function ParseMetadataType(type As String) As MetadataType
            Select Case type
                Case "ar"
                    Return MetadataType.Artist
                Case "al"
                    Return MetadataType.Album
                Case "ti"
                    Return MetadataType.Title
                Case "au"
                    Return MetadataType.LyricsAuthor
                Case "length"
                    Return MetadataType.Length
                Case "by"
                    Return MetadataType.FileAuthor
                Case "offset"
                    Return MetadataType.Offset
                Case "re"
                    Return MetadataType.Software
                Case "ve"
                    Return MetadataType.Version
                Case Else
                    Return MetadataType.Undefined
            End Select
        End Function

    End Class
End Namespace
