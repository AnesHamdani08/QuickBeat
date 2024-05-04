Imports System.IO
Imports System.Runtime.InteropServices
Imports QuickBeat.Utilities

Namespace Player
    <Serializable>
    Public Class CachedMetadata
        Inherits Metadata
        Public Overrides Property Location As FileLocation
            Get
                Return FileLocation.Cached
            End Get
            Set(value As FileLocation)
            End Set
        End Property

        Public Overrides Property IsFavorite As Boolean
            Get
                Return MyBase.IsFavorite
            End Get
            Set(value As Boolean)
                MyBase.IsFavorite = value
                SyncProperties(True)
            End Set
        End Property

        Public Overrides Property PlayCount As Integer
            Get
                Return MyBase.PlayCount
            End Get
            Set(value As Integer)
                MyBase.PlayCount = value
                SyncProperties(True)
            End Set
        End Property

        Private _Data As IO.MemoryStream 'serialized
        ReadOnly Property Data As IO.MemoryStream
            Get
                Return _Data
            End Get
        End Property

        Sub New(UID As String, buffer As Byte())
            MyBase.New()

            Me.UID = UID
            SyncProperties()
            _Data = New IO.MemoryStream(buffer)
            OnPropertyChanged(NameOf(Data))
        End Sub

        'Sub New(buffer As Byte())
        '    MyBase.New()

        '    _Data = New IO.MemoryStream(buffer)
        '    OnPropertyChanged(NameOf(Data))
        'End Sub

        Public Overrides Function CreateStream() As Integer
            If Utilities.SharedProperties.Instance.RemoteLibrary.ContainsID(UID) Then
                Try
                    Data.Dispose()
                Catch
                End Try
                Dim cm = Utilities.SharedProperties.Instance.RemoteLibrary.GetItem(UID)
                If cm IsNot Nothing Then _Data = New IO.MemoryStream(cm.Data?.ToArray)
            End If

            If Data Is Nothing Then
                Return 0
            End If

            SyncProperties()

            If Data.CanSeek Then Data.Seek(0, SeekOrigin.Begin)

            ' creating the user file callback delegates
            _myStreamCreateUser = New Un4seen.Bass.BASS_FILEPROCS(
            New Un4seen.Bass.FILECLOSEPROC(AddressOf MyFileProcUserClose),
            New Un4seen.Bass.FILELENPROC(AddressOf MyFileProcUserLength),
            New Un4seen.Bass.FILEREADPROC(AddressOf MyFileProcUserRead),
            New Un4seen.Bass.FILESEEKPROC(AddressOf MyFileProcUserSeek))

            Return Un4seen.Bass.Bass.BASS_StreamCreateFileUser(Un4seen.Bass.BASSStreamSystem.STREAMFILE_NOBUFFER, Un4seen.Bass.BASSFlag.BASS_STREAM_PRESCAN Or Un4seen.Bass.BASSFlag.BASS_STREAM_AUTOFREE, _myStreamCreateUser, IntPtr.Zero)
        End Function

        Public Overrides Sub RefreshTagsFromFile(Optional SkipCover As Boolean = False)
            If Data Is Nothing Then Return
            Utilities.DebugMode.Instance.Log(Of Metadata)("Attempting to refresh a metadata from internal file, Path:=" & Path)
            Utilities.DebugMode.Instance.Log(Of Metadata)("Refreshing metadata tags...")
            Dim Tags As TagLib.File = Nothing
            Try
                Dim fname = If(String.IsNullOrEmpty(Extension), IO.Path.GetFileName(Path), IO.Path.GetFileNameWithoutExtension(Path) & "." & Extension)
                Tags = TagLib.File.Create(New Classes.FileBytesAbstraction(fname, New IO.MemoryStream(Data.ToArray)), If(SkipCover, TagLib.ReadStyle.Average Or TagLib.ReadStyle.PictureLazy, TagLib.ReadStyle.Average))
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
            If Data Is Nothing Then Return
            Utilities.DebugMode.Instance.Log(Of Metadata)("Attempting to refresh a metadata from internal file, Path:=" & Path)
            Utilities.DebugMode.Instance.Log(Of Metadata)("Refreshing metadata tags...")
            Dim Tags As TagLib.File = Nothing
            Try
                Dim fname = If(String.IsNullOrEmpty(Extension), IO.Path.GetFileName(Path), IO.Path.GetFileNameWithoutExtension(Path) & "." & Extension)
                Tags = TagLib.File.Create(New Classes.FileBytesAbstraction(fname, New IO.MemoryStream(Data.ToArray)), If(SkipCover, TagLib.ReadStyle.Average Or TagLib.ReadStyle.PictureLazy, TagLib.ReadStyle.Average))
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
            If Data IsNot Nothing Then
                Dim Tags As TagLib.File = Nothing
                Try
                    Dim fname = If(String.IsNullOrEmpty(Extension), IO.Path.GetFileName(Path), IO.Path.GetFileNameWithoutExtension(Path) & "." & Extension)
                    Tags = TagLib.File.Create(New Classes.FileBytesAbstraction(fname, New IO.MemoryStream(Data.ToArray)), TagLib.ReadStyle.Average)
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
                ElseIf Not String.IsNullOrEmpty(Me.CoverLink) Then
                    Me.Covers = New ImageSource() {New Uri(CoverLink).ToBitmapSource()}
                ElseIf Not String.IsNullOrEmpty(Me.ThumbnailCoverLink) Then
                    Me.Covers = New ImageSource() {New Uri(ThumbnailCoverLink).ToBitmapSource()}
                End If
            ElseIf (Covers Is Nothing OrElse Covers.Count = 0) AndAlso Location = FileLocation.Cached AndAlso Provider IsNot Nothing Then
                Dim Thumb = Await Provider.FetchThumbnail
                If Thumb IsNot Nothing Then
                    Me.Covers = New ImageSource() {Thumb}
                End If
            End If
        End Sub

        Private Sub SyncProperties(Optional OneWayToSource As Boolean = False)
            If OneWayToSource Then
                SharedProperties.Instance.UpdateOrCreateRemoteFileProperty(New Tuple(Of Integer, Boolean)(PlayCount, IsFavorite), UID)
            Else
                Dim props = SharedProperties.Instance.GetRemoteFileProperties(UID)
                If props Is Nothing Then Return
                MyBase.PlayCount = props.Item2
                MyBase.IsFavorite = props.Item3
            End If
        End Sub

        Public Shared Function FromRemoteMetadata(Meta As Metadata, Buffer As Byte()) As CachedMetadata
            If Meta.Location <> FileLocation.Remote Then Throw New ArgumentOutOfRangeException("Meta", "Only Remote Metadata Is Support")
            Dim cMeta As New CachedMetadata(URLtoUID(Meta.OriginalPath), Buffer)
            cMeta.Provider = New CacheMediaProvider(cMeta.UID)
            Meta.CopyAllTo(cMeta)
            Return cMeta
        End Function

        Public Shared Function CacheFromFile(Path As String) As CachedMetadata
            Dim buffer = IO.File.ReadAllBytes(Path)
            Dim lMeta = Metadata.FromFile(Path, True)
            Dim cMeta As New CachedMetadata(URLtoUID(lMeta.OriginalPath), buffer)
            cMeta.Provider = New CacheMediaProvider(cMeta.UID)
            lMeta.CopyTo(cMeta)
            Return cMeta
        End Function

#Region "Cache"
        Private _myStreamCreateUser As Un4seen.Bass.BASS_FILEPROCS

        Private Sub MyFileProcUserClose(user As IntPtr)
            If Data Is Nothing Then
                Return
            End If
            If Utilities.SharedProperties.Instance.RemoteLibrary.ContainsID(UID) Then
                Data.Dispose()
            End If
        End Sub

        Private Function MyFileProcUserLength(user As IntPtr) As Long
            If Data Is Nothing Then
                Return 0L
            End If
            Return Data.Length
        End Function

        Private Function MyFileProcUserRead(buffer As IntPtr, length As Integer, user As IntPtr) As Integer
            If Data Is Nothing Then
                Return 0
            End If
            Try
                ' at first we need to create a byte[] with the size of the requested length
                Dim tdata(length - 1) As Byte
                ' read the file into data
                Dim bytesread As Integer = Me.Data.Read(tdata, 0, length)
                ' and now we need to copy the data to the buffer
                ' we write as many bytes as we read via the file operation
                Marshal.Copy(tdata, 0, buffer, bytesread)
                Return bytesread
            Catch
                Return 0
            End Try
        End Function

        Private Function MyFileProcUserSeek(offset As Long, user As IntPtr) As Boolean
            If Data Is Nothing Then
                Return False
            End If
            Try
                Dim pos As Long = Data.Seek(offset, SeekOrigin.Begin)
                Return True
            Catch
                Return False
            End Try
        End Function
#End Region
    End Class
End Namespace