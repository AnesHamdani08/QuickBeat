Option Infer On

Imports System
Imports System.Collections.Generic
Imports System.Globalization
Imports System.IO
Imports System.Linq
Imports System.Text
Imports System.Xml

Namespace SubtitlesParser.Classes.Parsers
	Public Class YtXmlFormatParser
		Implements ISubtitlesParser

        Public Function ParseStream(ByVal xmlStream As Stream, ByVal encoding As Encoding) As List(Of SubtitleItem) Implements ISubtitlesParser.ParseStream
            ' rewind the stream
            xmlStream.Position = 0
            Dim items = New List(Of SubtitleItem)()

            ' parse xml stream
            Dim xmlDoc = New XmlDocument()
            xmlDoc.Load(xmlStream)

            If xmlDoc.DocumentElement IsNot Nothing Then
                Dim nodeList = xmlDoc.DocumentElement.SelectNodes("//text")

                If nodeList IsNot Nothing Then
                    For i = 0 To nodeList.Count - 1
                        Dim node = nodeList(i)
                        Try
                            Dim startString = node.Attributes("start").Value
                            Dim start As Single = Single.Parse(startString, CultureInfo.InvariantCulture)
                            Dim durString = node.Attributes("dur").Value
                            Dim duration As Single = Single.Parse(durString, CultureInfo.InvariantCulture)
                            Dim text = node.InnerText

                            items.Add(New SubtitleItem() With {
                                .StartTime = CInt(Math.Truncate(start * 1000)),
                                .EndTime = CInt(Math.Truncate((start + duration) * 1000)),
                                .Lines = New List(Of String)() From {text},
                                .Index = items.Count
                            })
                        Catch ex As Exception
                            Console.WriteLine("Exception raised when parsing xml node {0}: {1}", node, ex)
                        End Try
                    Next i
                End If
            End If

            If items.Any() Then
                Return items
            Else
                Throw New ArgumentException("Stream is not in a valid Youtube XML format")
            End If
        End Function
    End Class
End Namespace