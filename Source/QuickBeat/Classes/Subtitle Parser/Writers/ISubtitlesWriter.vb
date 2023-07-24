Imports System.Collections.Generic
Imports System.IO
Imports System.Threading.Tasks

Namespace SubtitlesParser.Classes.Writers
	''' <summary>
	''' Interface specifying the required method for a SubWriter
	''' </summary>
	Public Interface ISubtitlesWriter
		''' <summary>
		''' Writes a list of SubtitleItems into a stream 
		''' </summary>
		''' <param name="stream">the stream to write to</param>
		''' <param name="subtitleItems">the SubtitleItems to write</param>
		''' <param name="includeFormatting">if formatting codes should be included when writing the subtitle item lines. Each subtitle item must have the PlaintextLines property set.</param>
		Sub WriteStream(ByVal stream As Stream, ByVal subtitleItems As IEnumerable(Of SubtitleItem), Optional ByVal includeFormatting As Boolean = True)

		''' <summary>
		''' Asynchronously writes a list of SubtitleItems into a stream 
		''' </summary>
		''' <param name="stream">the stream to write to</param>
		''' <param name="subtitleItems">the SubtitleItems to write</param>
		''' <param name="includeFormatting">if formatting codes should be included when writing the subtitle item lines. Each subtitle item must have the PlaintextLines property set.</param>
		Function WriteStreamAsync(ByVal stream As Stream, ByVal subtitleItems As IEnumerable(Of SubtitleItem), Optional ByVal includeFormatting As Boolean = True) As Task
	End Interface
End Namespace