Option Infer On

Imports System
Imports System.Collections.Generic
Imports System.Globalization
Imports System.IO
Imports System.Linq
Imports System.Text
Imports System.Text.RegularExpressions

Namespace SubtitlesParser.Classes.Parsers
	''' <summary>
	''' Parser for MicroDVD .sub subtitles files
	''' 
	''' A .sub file looks like this:
	''' {1}{1}29.970
	''' {0}{180}PIRATES OF THE CARIBBEAN|English subtitlez by tHe.b0dY
	''' {509}{629}Drink up me 'earties yo ho!
	''' {635}{755}We kidnap and ravage and don't give a hoot.
	''' 
	''' We need the video frame rate to extract .sub files -> careful when using it
	''' 
	''' see https://en.wikipedia.org/wiki/MicroDVD
	''' </summary>
	Public Class MicroDvdParser
		Implements ISubtitlesParser

		' Properties -----------------------------------------------------------------------

		Private ReadOnly defaultFrameRate As Single = 25
		Private ReadOnly _lineSeparators() As Char = {"|"c}


		' Constructors --------------------------------------------------------------------

		Public Sub New()
		End Sub

		Public Sub New(ByVal defaultFrameRate As Single)
			Me.defaultFrameRate = defaultFrameRate
		End Sub


        ' Methods -------------------------------------------------------------------------

        Public Function ParseStream(ByVal subStream As Stream, ByVal encoding As Encoding) As List(Of SubtitleItem) Implements ISubtitlesParser.ParseStream
            ' test if stream if readable and seekable (just a check, should be good)
            If Not subStream.CanRead OrElse Not subStream.CanSeek Then
                Dim message = String.Format("Stream must be seekable and readable in a subtitles parser. " & "Operation interrupted; isSeekable: {0} - isReadable: {1}", subStream.CanSeek, subStream.CanSeek)
                Throw New ArgumentException(message)
            End If

            ' seek the beginning of the stream
            subStream.Position = 0
            Dim reader = New StreamReader(subStream, encoding, True)

            Dim items = New List(Of SubtitleItem)()
            Dim line = reader.ReadLine()
            ' find the first relevant line
            Do While line IsNot Nothing AndAlso Not IsMicroDvdLine(line)
                line = reader.ReadLine()
            Loop

            If line IsNot Nothing Then
                Dim frameRate As Single = Nothing
                ' try to extract the framerate from the first line
                Dim firstItem = ParseLine(line, defaultFrameRate)
                If firstItem.Lines IsNot Nothing AndAlso firstItem.Lines.Any() Then
                    Dim success = TryExtractFrameRate(firstItem.Lines(0), frameRate)
                    If Not success Then
                        Console.WriteLine("Couldn't extract frame rate of sub file with first line {0}. " & "We use the default frame rate: {1}", line, defaultFrameRate)
                        frameRate = defaultFrameRate

                        ' treat it as a regular line
                        firstItem.Index = items.Count
                        items.Add(firstItem)
                    End If
                Else
                    frameRate = defaultFrameRate
                End If

                ' parse other lines
                line = reader.ReadLine()
                Do While line IsNot Nothing
                    If Not String.IsNullOrEmpty(line) Then
                        Dim item = ParseLine(line, frameRate)
                        item.Index = items.Count
                        items.Add(item)
                    End If
                    line = reader.ReadLine()
                Loop
            End If

            If items.Any() Then
                Return items
            Else
                Throw New ArgumentException("Stream is not in a valid MicroDVD format")
            End If
        End Function

        Private Const LineRegex As String = "^[{\[](-?\d+)[}\]][{\[](-?\d+)[}\]](.*)"

		Private Function IsMicroDvdLine(ByVal line As String) As Boolean
			Return Regex.IsMatch(line, LineRegex)
		End Function

		''' <summary>
		''' Parses one line of the .sub file
		''' 
		''' ex:
		''' {0}{180}PIRATES OF THE CARIBBEAN|English subtitlez by tHe.b0dY
		''' </summary>
		''' <param name="line">The .sub file line</param>
		''' <param name="frameRate">The frame rate with which the .sub file was created</param>
		''' <returns>The corresponding SubtitleItem</returns>
		Private Function ParseLine(ByVal line As String, ByVal frameRate As Single) As SubtitleItem
			Dim match = Regex.Match(line, LineRegex)
			If match.Success AndAlso match.Groups.Count > 2 Then
				Dim startFrame = match.Groups(1).Value
				Dim start = CInt(Math.Truncate(1000 * Double.Parse(startFrame) / frameRate))
				Dim endTime = match.Groups(2).Value
				Dim [end] = CInt(Math.Truncate(1000 * Double.Parse(endTime) / frameRate))
				Dim text = match.Groups(match.Groups.Count - 1).Value
				Dim lines = text.Split(_lineSeparators)
				Dim nonEmptyLines = lines.Where(Function(l) Not String.IsNullOrEmpty(l)).ToList()
				Dim item = New SubtitleItem With {
					.Lines = nonEmptyLines,
					.StartTime = start,
					.EndTime = [end]
				}

				Return item
			Else
				Dim message = String.Format("The subtitle file line {0} is " & "not in the micro dvd format. We stop the process.", line)
				Throw New InvalidDataException(message)
			End If
		End Function

		''' <summary>
		''' Tries to extract the frame rate from a subtitle file line.
		''' 
		''' Supported formats are:
		''' - {x}{y}25
		''' - {x}{y}{...}23.976
		''' </summary>
		''' <param name="text">The subtitle file line</param>
		''' <param name="frameRate">The frame rate if we can parse it</param>
		''' <returns>True if the parsing was successful, false otherwise</returns>
		Private Function TryExtractFrameRate(ByVal text As String, ByRef frameRate As Single) As Boolean
			If Not String.IsNullOrEmpty(text) Then
				Dim success = Single.TryParse(text, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, frameRate)
				Return success
			Else
				frameRate = defaultFrameRate
				Return False
			End If
		End Function

	End Class
End Namespace