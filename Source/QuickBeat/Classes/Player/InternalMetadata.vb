Imports QuickBeat.Utilities
Imports System.IO
Imports System.Runtime.InteropServices

Namespace Player
    <Serializable>
    Public Class InternalMetadata
        Inherits Metadata
        Public Overrides Property Location As FileLocation
            Get
                Return FileLocation.Internal
            End Get
            Set(value As FileLocation)
            End Set
        End Property

        Public Overrides Function CreateStream() As Integer
            If _fs IsNot Nothing Then
                _fs.Dispose()
            End If
            ' creating the user file callback delegates
            _myStreamCreateUser = New Un4seen.Bass.BASS_FILEPROCS(
            New Un4seen.Bass.FILECLOSEPROC(AddressOf MyFileProcUserClose),
            New Un4seen.Bass.FILELENPROC(AddressOf MyFileProcUserLength),
            New Un4seen.Bass.FILEREADPROC(AddressOf MyFileProcUserRead),
            New Un4seen.Bass.FILESEEKPROC(AddressOf MyFileProcUserSeek))
            Dim MeloStream = Application.GetResourceStream(New Uri(Path, UriKind.Relative))
            If MeloStream Is Nothing Then Return 0
            _fs = MeloStream.Stream
            ' create the stream (the PRESCAN flag shows you what BASS is doing at the beginning to scan the entire file)
            ' if that generates to much output for you, you can simply remove it
            Return Un4seen.Bass.Bass.BASS_StreamCreateFileUser(Un4seen.Bass.BASSStreamSystem.STREAMFILE_NOBUFFER, Un4seen.Bass.BASSFlag.BASS_STREAM_PRESCAN Or Un4seen.Bass.BASSFlag.BASS_STREAM_AUTOFREE, _myStreamCreateUser, IntPtr.Zero)
        End Function

        Public Overrides Sub RefreshTagsFromFile(Optional SkipCover As Boolean = False)
            Dim ResStream = Application.GetResourceStream(New Uri(Path, UriKind.Relative))
            If ResStream Is Nothing Then Return
            Utilities.DebugMode.Instance.Log(Of Metadata)("Attempting to refresh a metadata from internal file, Path:=" & Path)
            Utilities.DebugMode.Instance.Log(Of Metadata)("Refreshing metadata tags...")
            Dim Tags As TagLib.File = Nothing
            Try
                Tags = TagLib.File.Create(New Classes.FileBytesAbstraction(IO.Path.GetFileName(Path), ResStream.Stream), If(SkipCover, TagLib.ReadStyle.Average Or TagLib.ReadStyle.PictureLazy, TagLib.ReadStyle.Average))
            Catch ex As Exception
                Utilities.DebugMode.Instance.Log(Of Metadata)("Error while reading tags: " & ex.ToString)
            End Try
            If Tags IsNot Nothing Then
                Title = Tags.Tag.Title
                Artists = Tags.Tag.Performers
                Album = Tags.Tag.Album
                Year = Tags.Tag.Year
                Length = Tags.Properties.Duration.TotalSeconds
                Size = Tags.Length
                HasCover = (Tags.Tag.Pictures.Length > 0)
            End If
            If Not SkipCover Then
                Dim Covers As New List(Of ImageSource)
                If Tags IsNot Nothing Then
                    For Each picture In Tags?.Tag.Pictures
                        Dim BI As BitmapImage = If(OverrideProperties.Instance.Locked(OverrideProperties.LockType.Metadata_DecodeToThumbnail), New IO.MemoryStream(picture.Data.Data).ToBitmapSource(OverrideProperties.Instance.Value(OverrideProperties.LockType.Metadata_DecodePixelWidth), OverrideProperties.Instance.Value(OverrideProperties.LockType.Metadata_DecodePixelHeight)), New IO.MemoryStream(picture.Data.Data).ToBitmapSource)
                        If BI IsNot Nothing Then Covers.Add(BI)
                    Next
                End If
                Covers = Covers
            End If
            If String.IsNullOrEmpty(Title) Then
                Title = IO.Path.GetFileNameWithoutExtension(Path)
            End If
            Utilities.DebugMode.Instance.Log(Of Metadata)("Done refreshing metadata.")
        End Sub

        Public Overrides Sub RefreshTagsFromFile_ThreadSafe(Optional SkipCover As Boolean = False)
            Dim ResStream = Application.GetResourceStream(New Uri(Path, UriKind.Relative))
            If ResStream Is Nothing Then Return
            Utilities.DebugMode.Instance.Log(Of Metadata)("Attempting to refresh a metadata from internal file, Path:=" & Path)
            Utilities.DebugMode.Instance.Log(Of Metadata)("Refreshing metadata tags...")
            Dim Tags As TagLib.File = Nothing
            Try
                Tags = TagLib.File.Create(New Classes.FileBytesAbstraction(IO.Path.GetFileName(Path), ResStream.Stream), If(SkipCover, TagLib.ReadStyle.Average Or TagLib.ReadStyle.PictureLazy, TagLib.ReadStyle.Average))
            Catch ex As Exception
                Utilities.DebugMode.Instance.Log(Of Metadata)("Error while reading tags: " & ex.ToString)
            End Try
            If Tags IsNot Nothing Then
                Application.Current.Dispatcher.Invoke(Sub()
                                                          Title = Tags.Tag.Title
                                                          Artists = Tags.Tag.Performers
                                                          Album = Tags.Tag.Album
                                                          Year = Tags.Tag.Year
                                                          Length = Tags.Properties.Duration.TotalSeconds
                                                          Size = Tags.Length
                                                          HasCover = (Tags.Tag.Pictures.Length > 0)
                                                      End Sub)
            End If
            If Not SkipCover Then
                Dim Covers As New List(Of ImageSource)
                If Tags IsNot Nothing Then
                    For Each picture In Tags?.Tag.Pictures
                        Dim BI As BitmapImage = If(OverrideProperties.Instance.Locked(OverrideProperties.LockType.Metadata_DecodeToThumbnail), New IO.MemoryStream(picture.Data.Data).ToBitmapSource(OverrideProperties.Instance.Value(OverrideProperties.LockType.Metadata_DecodePixelWidth), OverrideProperties.Instance.Value(OverrideProperties.LockType.Metadata_DecodePixelHeight)), New IO.MemoryStream(picture.Data.Data).ToBitmapSource)
                        If BI IsNot Nothing Then Covers.Add(BI)
                    Next
                End If
                Covers = Covers
            End If
            Application.Current.Dispatcher.Invoke(Sub()
                                                      If String.IsNullOrEmpty(Title) Then
                                                          Title = IO.Path.GetFileNameWithoutExtension(Path)
                                                      End If
                                                  End Sub)
            Utilities.DebugMode.Instance.Log(Of Metadata)("Done refreshing metadata.")
        End Sub

        Public Overrides Async Sub EnsureCovers(Optional DecodeToThumbnail As Boolean = False, Optional ThumbPxWidth As Integer = 150, Optional ThumbPxHeight As Integer = 150, Optional FreezeThumbs As Boolean = False)
            Dim ResStream = Application.GetResourceStream(New Uri(Path, UriKind.Relative))
            If ResStream IsNot Nothing Then
                Dim Tags As TagLib.File = Nothing
                Try
                    Tags = TagLib.File.Create(New Classes.FileBytesAbstraction(IO.Path.GetFileName(Path), ResStream.Stream), TagLib.ReadStyle.Average)
                Catch ex As Exception
                    Utilities.DebugMode.Instance.Log(Of Metadata)("Error while reading tags: " & ex.ToString)
                End Try
                If Tags IsNot Nothing AndAlso Tags.Tag.Pictures.Length > 0 Then
                    HasCover = True
                    Dim Covers As New List(Of ImageSource)
                    For Each picture In Tags.Tag.Pictures
                        Dim BI As BitmapImage = Nothing
                        If OverrideProperties.Instance.Locked(OverrideProperties.LockType.Metadata_DecodeToThumbnail) Then
                            If OverrideProperties.Instance.Value(OverrideProperties.LockType.Metadata_DecodeToThumbnail) Then
                                BI = New IO.MemoryStream(picture.Data.Data).ToBitmapSource(OverrideProperties.Instance.Value(OverrideProperties.LockType.Metadata_DecodePixelWidth), OverrideProperties.Instance.Value(OverrideProperties.LockType.Metadata_DecodePixelHeight))
                            Else
                                BI = New IO.MemoryStream(picture.Data.Data).ToBitmapSource
                            End If
                        Else
                            If DecodeToThumbnail Then
                                BI = New IO.MemoryStream(picture.Data.Data).ToBitmapSource(ThumbPxWidth, ThumbPxHeight)
                            Else
                                BI = New IO.MemoryStream(picture.Data.Data).ToBitmapSource
                            End If
                        End If
                        If FreezeThumbs Then BI.Freeze()
                        Covers.Add(BI)
                    Next
                    Me.Covers = Covers
                    Tags.Dispose()
                End If
            ElseIf (Covers Is Nothing OrElse Covers.Count = 0) AndAlso Location = FileLocation.Internal AndAlso Provider IsNot Nothing Then
                Dim Thumb = Await Provider.FetchThumbnail
                If Thumb IsNot Nothing Then
                    Me.Covers = New ImageSource() {Thumb}
                End If
            End If
        End Sub
#Region "Melody"
        Private _myStreamCreateUser As Un4seen.Bass.BASS_FILEPROCS
        Private _fs As IO.Stream

        Public Sub New(Path As String)
            Me.Path = Path
            Provider = New InternalMediaProvider(Path)
        End Sub

        Private Sub MyFileProcUserClose(user As IntPtr)
            If _fs Is Nothing Then
                Return
            End If
            _fs.Close()
            _fs = Nothing
        End Sub

        Private Function MyFileProcUserLength(user As IntPtr) As Long
            If _fs Is Nothing Then
                Return 0L
            End If
            Return _fs.Length
        End Function

        Private Function MyFileProcUserRead(buffer As IntPtr, length As Integer, user As IntPtr) As Integer
            If _fs Is Nothing Then
                Return 0
            End If
            Try
                ' at first we need to create a byte[] with the size of the requested length
                Dim data(length - 1) As Byte
                ' read the file into data
                Dim bytesread As Integer = _fs.Read(data, 0, length)
                ' and now we need to copy the data to the buffer
                ' we write as many bytes as we read via the file operation
                Marshal.Copy(data, 0, buffer, bytesread)
                Return bytesread
            Catch
                Return 0
            End Try
        End Function

        Private Function MyFileProcUserSeek(offset As Long, user As IntPtr) As Boolean
            If _fs Is Nothing Then
                Return False
            End If
            Try
                Dim pos As Long = _fs.Seek(offset, SeekOrigin.Begin)
                Return True
            Catch
                Return False
            End Try
        End Function
#End Region
    End Class
End Namespace