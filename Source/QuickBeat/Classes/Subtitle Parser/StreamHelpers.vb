Option Infer On

Imports System.IO

Namespace SubtitlesParser.Classes
	Friend Module StreamHelpers
		''' <summary>
		''' Copies a stream to another stream.
		''' This method is useful in particular when the inputStream is not seekable.
		''' </summary>
		''' <param name="inputStream">The stream to copy</param>
		''' <returns>A copy of the input Stream</returns>
		Public Function CopyStream(ByVal inputStream As Stream) As Stream
			Dim outputStream = New MemoryStream()
			Dim count As Integer
			Do
				Dim buf = New Byte(1023){}
				count = inputStream.Read(buf, 0, 1024)
				outputStream.Write(buf, 0, count)
			Loop While inputStream.CanRead AndAlso count > 0
			outputStream.ToArray()

			Return outputStream
		End Function
	End Module
End Namespace