Option Infer On

Imports Microsoft.VisualBasic
Imports System
Imports System.Collections.Generic
Imports System.IO
Imports System.Linq
Imports System.Text
Imports System.Text.RegularExpressions

Namespace SubtitlesParser.Classes.Parsers
	Public Class SubParser
		' Properties -----------------------------------------------------------------------

		Private ReadOnly _subFormatToParser As New Dictionary(Of SubtitlesFormat, ISubtitlesParser) From {
			{SubtitlesFormat.SubRipFormat, New SrtParser()},
			{SubtitlesFormat.MicroDvdFormat, New MicroDvdParser()},
			{SubtitlesFormat.SubViewerFormat, New SubViewerParser()},
			{SubtitlesFormat.SubStationAlphaFormat, New SsaParser()},
			{SubtitlesFormat.TtmlFormat, New TtmlParser()},
			{SubtitlesFormat.WebVttFormat, New VttParser()},
			{SubtitlesFormat.YoutubeXmlFormat, New YtXmlFormatParser()},
			{SubtitlesFormat.LRCFormat, New LRCParser()}
		}


		' Constructors --------------------------------------------------------------------

		Public Sub New()
		End Sub


		' Methods -------------------------------------------------------------------------		
		''' <summary>
		''' Parses the lyrics based on file extension
		''' </summary>
		''' <param name="path">Subtitles file path</param>
		''' <param name="stream">Stream containing the file data</param>
		''' <returns>
		''' Nothing if failed
		''' </returns>
		Public Function ParseStream(path As String, stream As Stream) As List(Of SubtitleItem)
			Dim parser = _subFormatToParser.FirstOrDefault(Function(k) k.Key.Extension = "\" & IO.Path.GetExtension(path))
			If parser.Key Is Nothing Then Return Nothing
			Return parser.Value.ParseStream(stream, Encoding.UTF8)
		End Function

		''' <summary>
		''' Gets the most likely format of the subtitle file based on its filename.
		''' Most likely because .sub are sometimes srt files for example.
		''' </summary>
		''' <param name="fileName">The subtitle file name</param>
		''' <returns>The most likely subtitles format</returns>
		Public Function GetMostLikelyFormat(ByVal fileName As String) As SubtitlesFormat
			Dim extension = Path.GetExtension(fileName)

			If Not String.IsNullOrEmpty(extension) Then
                For Each SubFormat In SubtitlesFormat.SupportedSubtitlesFormats
                    If SubFormat.Extension IsNot Nothing AndAlso Regex.IsMatch(extension, SubFormat.Extension, RegexOptions.IgnoreCase) Then
                        Return SubFormat
                    End If
                Next SubFormat
            End If

			Return Nothing
		End Function

		''' <summary>
		''' Parses a subtitles file stream
		''' </summary>
		''' <param name="stream">The subtitles file stream</param>
		''' <returns>The corresponding list of SubtitleItems</returns>
		Public Function ParseStream(ByVal stream As Stream) As List(Of SubtitleItem)
			' we default encoding to UTF-8
			Return ParseStream(stream, Encoding.UTF8)
		End Function

		''' <summary>
		''' Parses a subtitle file stream.
		''' We try all the parsers registered in the _subFormatToParser dictionary
		''' </summary>
		''' <param name="stream">The subtitle file stream</param>
		''' <param name="encoding">The stream encoding</param>
		''' <param name="subFormat">The preferred subFormat to try first (if we have a clue with the subtitle file name for example)</param>
		''' <returns>The corresponding list of SubtitleItem, null if parsing failed</returns>
		Public Function ParseStream(ByVal stream As Stream, ByVal encoding As Encoding, Optional ByVal subFormat As SubtitlesFormat = Nothing) As List(Of SubtitleItem)
			Dim dictionary = If(subFormat IsNot Nothing, _subFormatToParser.OrderBy(Function(dic) Math.Abs(String.Compare(dic.Key.Name, subFormat.Name, StringComparison.Ordinal))).ToDictionary(Function(entry) entry.Key, Function(entry) entry.Value), _subFormatToParser)

			Return ParseStream(stream, encoding, dictionary)
		End Function

		''' <summary>
		''' Parses a subtitle file stream.
		''' We try all the parsers registered in the _subFormatToParser dictionary
		''' </summary>
		''' <param name="stream">The subtitle file stream</param>
		''' <param name="encoding">The stream encoding</param>
		''' <param name="subFormatDictionary">The dictionary of the subtitles parser (ordered) to try</param>
		''' <returns>The corresponding list of SubtitleItem, null if parsing failed</returns>
		Public Function ParseStream(ByVal stream As Stream, ByVal encoding As Encoding, ByVal subFormatDictionary As Dictionary(Of SubtitlesFormat, ISubtitlesParser)) As List(Of SubtitleItem)
			' test if stream if readable
			If Not stream.CanRead Then
				Throw New ArgumentException("Cannot parse a non-readable stream")
			End If

			' copy the stream if not seekable
			Dim seekableStream = stream
			If Not stream.CanSeek Then
				seekableStream = StreamHelpers.CopyStream(stream)
				seekableStream.Seek(0, SeekOrigin.Begin)
			End If

			' if dictionary is null, use the default one
			subFormatDictionary = If(subFormatDictionary, _subFormatToParser)

            For Each EsubtitlesParser In subFormatDictionary
                Try
                    Dim parser = EsubtitlesParser.Value
                    Dim items = parser.ParseStream(seekableStream, encoding)
                    Return items
                Catch ex As Exception
                    Continue For ' Let's try the next parser...
                    'Console.WriteLine(ex);
                End Try
            Next ESubtitlesParser

            ' all the parsers failed
            Dim firstCharsOfFile = LogFirstCharactersOfStream(stream, 500, encoding)
			Dim message = String.Format("All the subtitles parsers failed to parse the following stream:{0}", firstCharsOfFile)
			Throw New ArgumentException(message)
		End Function

		''' <summary>
		''' Logs the first characters of a stream for debug
		''' </summary>
		''' <param name="stream">The file stream</param>
		''' <param name="nbOfCharactersToPrint">The number of caracters to print</param>
		''' <param name="encoding">The stream encoding</param>
		''' <returns>The first characters of the stream</returns>
		Private Function LogFirstCharactersOfStream(ByVal stream As Stream, ByVal nbOfCharactersToPrint As Integer, ByVal encoding As Encoding) As String
			Dim message = ""
			' print the first 500 characters
			If stream.CanRead Then
				If stream.CanSeek Then
					stream.Position = 0
				End If

				Dim reader = New StreamReader(stream, encoding, True)

				Dim buffer = New Char(nbOfCharactersToPrint - 1){}
				reader.ReadBlock(buffer, 0, nbOfCharactersToPrint)
				message = String.Format("Parsing of subtitle stream failed. Beginning of sub stream:" & vbLf & "{0}", String.Join("", buffer))
			Else
				message = String.Format("Tried to log the first {0} characters of a closed stream", nbOfCharactersToPrint)
			End If
			Return message
		End Function

	End Class
End Namespace