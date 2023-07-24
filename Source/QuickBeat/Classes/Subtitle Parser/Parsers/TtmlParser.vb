Option Infer On

Imports System
Imports System.Collections.Generic
Imports System.Globalization
Imports System.IO
Imports System.Linq
Imports System.Text
Imports System.Xml
Imports System.Xml.Linq

Namespace SubtitlesParser.Classes.Parsers
	Public Class TtmlParser
		Implements ISubtitlesParser

        Public Function ParseStream(ByVal xmlStream As Stream, ByVal encoding As Encoding) As List(Of SubtitleItem) Implements ISubtitlesParser.ParseStream
            ' rewind the stream
            xmlStream.Position = 0
            Dim items = New List(Of SubtitleItem)()

            ' parse xml stream
            Dim xDoc = XElement.Load(xmlStream)
            Dim tt = If(xDoc.GetNamespaceOfPrefix("tt"), xDoc.GetDefaultNamespace())

            Dim nodeList = xDoc.Descendants(tt.NamespaceName & "p").ToList()
            For Each node In nodeList
                Try
                    Dim reader = node.CreateReader()
                    reader.MoveToContent()
                    Dim beginString = node.Attribute("begin").Value.Replace("t", "")
                    Dim startTicks = ParseTimecode(beginString)
                    Dim endString = node.Attribute("end").Value.Replace("t", "")
                    Dim endTicks = ParseTimecode(endString)
                    Dim text = reader.ReadInnerXml().Replace("<tt:", "<").Replace("</tt:", "</").Replace(String.Format(" xmlns:tt=""{0}""", tt), "").Replace(String.Format(" xmlns=""{0}""", tt), "")

                    items.Add(New SubtitleItem() With {
                        .StartTime = CInt(startTicks),
                        .EndTime = CInt(endTicks),
                        .Lines = New List(Of String)() From {text},
                        .Index = items.Count
                    })
                Catch ex As Exception
                    Console.WriteLine("Exception raised when parsing xml node {0}: {1}", node, ex)
                End Try
            Next node

            If items.Any() Then
                Return items
            End If
            Throw New ArgumentException("Stream is not in a valid TTML format, or represents empty subtitles")
        End Function

        ''' <summary>
        ''' Takes an SRT timecode as a string and parses it into a double (in seconds). A SRT timecode reads as follows: 
        ''' 00:00:20,000
        ''' </summary>
        ''' <param name="s">The timecode to parse</param>
        ''' <returns>The parsed timecode as a TimeSpan instance. If the parsing was unsuccessful, -1 is returned (subtitles should never show)</returns>
        Private Shared Function ParseTimecode(ByVal s As String) As Long
			Dim result As TimeSpan = Nothing
			If TimeSpan.TryParse(s, result) Then
				Return CLng(Math.Truncate(result.TotalMilliseconds))
			End If
			' Netflix subtitles have a weird format: timecodes are specified as ticks. Ex: begin="79249170t"
			Dim ticks As Long = Nothing
			If Long.TryParse(s.TrimEnd("t"c), ticks) Then
				Return ticks\10000
			End If
			Return -1
		End Function
	End Class
End Namespace