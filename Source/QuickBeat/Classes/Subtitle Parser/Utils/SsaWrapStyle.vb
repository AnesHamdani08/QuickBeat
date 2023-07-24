Namespace SubtitlesParser.Classes.Utils
	' Note: the spec doc doesn't actually specify a name, just a number and a description, so I took some creative liberties
	''' <summary>
	''' Represents a Wrap Style used by Advanced SSA
	''' </summary>
	Public Enum SsaWrapStyle
		''' <summary>
		''' Smart wrapping, lines are evenly broken
		''' </summary>
		Smart = 0
		''' <summary>
		''' End-of-line word wrapping, only \N breaks
		''' </summary>
		EndOfLine = 1
		''' <summary>
		''' No word wrapping, \n \N both breaks
		''' </summary>
		None = 2
		''' <summary>
		''' Same as Smart, but the lower line gets wider
		''' </summary>
		SmartWideLowerLine = 3
	End Enum

	''' <summary>
	'''  Extension methods for parsing to a wrap style
	''' </summary>
	Public Module SsaWrapStyleExtensions
		''' <summary>
		''' Parse a string into a wrap style
		'''
		''' Invalid input strings will return `SsaWrapStyle.None`
		''' </summary>
		''' <param name="rawString">A string representation of a wrap style value</param>
		''' <returns>A SsaWrapStyle corresponding to the value parsed from the input string</returns>
		<System.Runtime.CompilerServices.Extension>
		Public Function FromString(ByVal rawString As String) As SsaWrapStyle
			Dim rawInt As Integer
			Return If(Integer.TryParse(rawString, rawInt) = False, SsaWrapStyle.None, FromInt(rawInt))
		End Function

		''' <summary>
		''' Parse an integer into a wrap style
		'''
		''' Integers outside the range of valid wrap styles will default to `SsaWrapStyle.None`
		''' </summary>
		''' <param name="rawInt">An integer inside the range of values representing a wrap style</param>
		''' <returns>A SsaWrapStyle corresponding to the integer value specified</returns> 
		Public Function FromInt(rawint As Integer) As SsaWrapStyle
			Select Case rawint
				Case 0
					Return SsaWrapStyle.Smart
				Case 1
					Return SsaWrapStyle.EndOfLine
				Case 2
					Return SsaWrapStyle.None
				Case 3
					Return SsaWrapStyle.SmartWideLowerLine
				Case Else
					Return SsaWrapStyle.None
			End Select
		End Function
	End Module
End Namespace