Imports System.Collections.Generic
Imports System.IO
Imports System.Text

Namespace SubtitlesParser.Classes.Parsers
	''' <summary>
	''' Interface specifying the required method for a SubParser.
	''' </summary>
	Public Interface ISubtitlesParser
		''' <summary>
		''' Parses a subtitles file stream in a list of SubtitleItem
		''' </summary>
		''' <param name="stream">The subtitles file stream to parse</param>
		''' <param name="encoding">The stream encoding (if known)</param>
		''' <returns>The corresponding list of SubtitleItems</returns>
		Function ParseStream(ByVal stream As Stream, ByVal encoding As Encoding) As List(Of SubtitleItem)

	End Interface
End Namespace