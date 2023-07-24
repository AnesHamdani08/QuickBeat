Imports System.IO
Namespace Classes
    Public Class FileBytesAbstraction
        Implements TagLib.File.IFileAbstraction

        Public Sub New(ByVal name As String, ByVal bytes() As Byte)
            Me.Name = name
            Dim stream = New MemoryStream(bytes)
            ReadStream = stream
            WriteStream = stream
        End Sub

        Public Sub New(ByVal name As String, ByVal data As IO.MemoryStream)
            Me.Name = name
            Dim stream = New MemoryStream
            data.CopyTo(stream)
            ReadStream = stream
            WriteStream = stream
        End Sub

        Public Sub CloseStream(ByVal stream As Stream) Implements TagLib.File.IFileAbstraction.CloseStream
            stream.Dispose()
        End Sub

        Private privateName As String
        Public Property Name() As String Implements TagLib.File.IFileAbstraction.Name
            Get
                Return privateName
            End Get
            Private Set(ByVal value As String)
                privateName = value
            End Set
        End Property

        Private privateReadStream As Stream
        Public Property ReadStream() As Stream Implements TagLib.File.IFileAbstraction.ReadStream
            Get
                Return privateReadStream
            End Get
            Private Set(ByVal value As Stream)
                privateReadStream = value
            End Set
        End Property

        Private privateWriteStream As Stream
        Public Property WriteStream() As Stream Implements TagLib.File.IFileAbstraction.WriteStream
            Get
                Return privateWriteStream
            End Get
            Private Set(ByVal value As Stream)
                privateWriteStream = value
            End Set
        End Property
    End Class
End Namespace