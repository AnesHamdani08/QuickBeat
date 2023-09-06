Imports QuickBeat.Player
Namespace Dialogs
    Public Class TagEditor
        Private _Path As String

        Shared ReadOnly Property MetadataProperty As DependencyProperty = DependencyProperty.Register("FileTag", GetType(FileTag), GetType(TagEditor))
        Property FileTag As FileTag
            Get
                Return GetValue(MetadataProperty)
            End Get
            Set(value As FileTag)
                SetValue(MetadataProperty, value)
            End Set
        End Property

        Sub New(Path As String)

            ' This call is required by the designer.
            InitializeComponent()

            ' Add any initialization after the InitializeComponent() call.

            WindowStartupLocation = WindowStartupLocation.CenterScreen
            _Path = Path
        End Sub

        Sub New(Owner As System.Windows.Window, Path As String)

            ' This call is required by the designer.
            InitializeComponent()

            ' Add any initialization after the InitializeComponent() call.

            Me.Owner = Owner
            _Path = Path
        End Sub

        Private Sub TagEditor_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
            FileTag = New FileTag With {.Path = _Path}
        End Sub

        Private Async Sub Button_Save_Click(sender As Object, e As RoutedEventArgs)
            Dim info As New IO.FileInfo(_Path)
            Dim _WasReadOnly As Boolean
            If info.IsReadOnly Then
                If HandyControl.Controls.MessageBox.Ask(Utilities.ResourceResolver.Strings.QUERY_FILEUNLOCK) = MessageBoxResult.OK Then
                    info.IsReadOnly = False
                    _WasReadOnly = True
                Else
                    Return
                End If
            End If
            Dim _WasPlaying As Boolean
            Dim _Position As Double
            Dim _WasActivelyPlaying As Boolean
            If Utilities.SharedProperties.Instance.Player.Path = _Path Then
                _WasPlaying = True
                _Position = Utilities.SharedProperties.Instance.Player.Position
                _WasActivelyPlaying = Utilities.SharedProperties.Instance.Player.IsPlaying
                Utilities.SharedProperties.Instance.Player.Stop()
            End If
            Try
                FileTag.Save()
            Catch ex As Exception
                Utilities.DebugMode.Instance.Log(Of TagEditor)(ex.ToString)
                If _WasReadOnly Then info.IsReadOnly = True
                Dim errorCode As Integer = Runtime.InteropServices.Marshal.GetHRForException(ex) And ((1 << 16) - 1)
                If (errorCode = 32 OrElse errorCode = 33) Then
                    HandyControl.Controls.MessageBox.Error(Utilities.ResourceResolver.Strings.QUERY_FILEACCESSDENIED & Environment.NewLine & ex.Message)
                    Return
                End If
            End Try
            If _WasPlaying Then
                Await Utilities.SharedProperties.Instance.Player.LoadSong(Utilities.SharedProperties.Instance.Player.StreamMetadata)
                Utilities.SharedProperties.Instance.Player.Position = _Position
                If _WasActivelyPlaying Then Utilities.SharedProperties.Instance.Player.PlayCommand.Execute(Utilities.SharedProperties.Instance.Player.Stream)
            End If
            If _WasReadOnly Then
                info.IsReadOnly = True
            End If
            'Update Library
            Dim Meta = Player.Metadata.FromFile(_Path, True, True)
            If Meta Is Nothing Then
                'Check Player
                If Utilities.SharedProperties.Instance.Player.StreamMetadata?.Path = _Path Then
                    Utilities.SharedProperties.Instance.Player.StreamMetadata.Covers = Nothing
                    Utilities.SharedProperties.Instance.Player.StreamMetadata.RefreshTagsFromFile_ThreadSafe()
                    Utilities.SharedProperties.Instance.Player.StreamMetadata.EnsureCovers()
                End If
            Else
                Meta.RefreshTagsFromFile_ThreadSafe(True)
            End If
            DialogResult = True
        End Sub

        Private Sub Button_Close_Click(sender As Object, e As RoutedEventArgs)
            DialogResult = True
            Close()
        End Sub

        Private _IsFetching As Boolean = False
        Private Sub Commands_Fetch_CanExecute(sender As Object, e As CanExecuteRoutedEventArgs)
            e.CanExecute = Not _IsFetching
        End Sub

        Private Async Sub Commands_Fetch_Executed(sender As Object, e As ExecutedRoutedEventArgs)
            'release:=album, recording:=track
            'special characters:= + - && || ! ( ) { } [ ] ^ " ~ * ? : \/
            _IsFetching = True
            Dim SpecialCharacters As String() = New String() {"+", "-", "&&", "||", "!", "(", ")", "{", "}", "[", "]", "^", """", "~", "*", "?", ":", "\/"}
            Dim SearchQuery As String = ""
            Dim IB As New Dialogs.InputBox() With {.Owner = Me, .WindowStartupLocation = WindowStartupLocation.CenterOwner}
            IB.AddTextBox("Title", True, FileTag?.Title)
            IB.AddTextBox("Artist", True, FileTag?.Artists.Split(";").FirstOrDefault)
            If Not IB.ShowDialog Then
                _IsFetching = False
                Return
            End If
            Dim ETitle = IB.Value("Title").Replace("+", "\+").Replace("-", "\-").Replace("&&", "\&&").Replace("||", "\||").Replace("!", "\!").Replace("(", "\(").Replace(")", "\)").Replace("{", "\{").Replace("}", "\}").Replace("[", "\[").Replace("]", "\]").Replace("^", "\^").Replace("""", "\""").Replace("~", "\~").Replace("*", "\*").Replace("?", "\?").Replace(":", "\:").Replace("\/", "\\/")
            Dim EArtist = IB.Value("Artist").Replace("+", "\+").Replace("-", "\-").Replace("&&", "\&&").Replace("||", "\||").Replace("!", "\!").Replace("(", "\(").Replace(")", "\)").Replace("{", "\{").Replace("}", "\}").Replace("[", "\[").Replace("]", "\]").Replace("^", "\^").Replace("""", "\""").Replace("~", "\~").Replace("*", "\*").Replace("?", "\?").Replace(":", "\:").Replace("\/", "\\/")
            SearchQuery = $"Title:{ETitle} AND Artist:{EArtist}"
            Dim Query As New MetaBrainz.MusicBrainz.Query("QuickBeat", New Version(1, 0, 0, 0))
            Dim Result = Query.FindRecordings(SearchQuery, 1)
            Dim FResult = Result.Results.FirstOrDefault
            If FResult Is Nothing Then
                _IsFetching = False
                HandyControl.Controls.MessageBox.Error(Utilities.ResourceResolver.Strings.QUERY_NOMATCHES)
                Return
            End If
            FileTag.Title = FResult.Item.Title
            FileTag.Artists = String.Join(";", FResult.Item.ArtistCredit.Select(Of String)(Function(k) k.Name))
            FileTag.Year = FResult.Item.FirstReleaseDate.Year

            If FResult.Item.Releases.Count = 0 Then
                _IsFetching = False
                Return
            End If
            FileTag.Album = FResult.Item.Releases.FirstOrDefault?.Title
            Dim CA As New MetaBrainz.MusicBrainz.CoverArt.CoverArt("QuickBeat", New Version(1, 0, 0, 0), MetaBrainz.MusicBrainz.CoverArt.CoverArt.DefaultContactInfo)
            Dim FrontCover = Await CA.FetchFrontAsync(FResult.Item.Releases.FirstOrDefault.Id, MetaBrainz.MusicBrainz.CoverArt.CoverArtImageSize.Original)
            FrontCover.Data.Seek(0, IO.SeekOrigin.Begin)
            Try
                Dim ImgStream As New IO.MemoryStream
                Dim Enco As New PngBitmapEncoder()
                Enco.Frames.Add(BitmapFrame.Create(FrontCover.Data))
                Enco.Save(ImgStream)
                Dim Pic As New TagLib.Picture(New TagLib.ByteVector(ImgStream.ToArray))
                Pic.Type = TagLib.PictureType.FrontCover
                If FileTag.Covers.Count > 0 Then
                    If HandyControl.Controls.MessageBox.Ask(Utilities.ResourceResolver.Strings.QUERY_KEEPPICTURES) <> MessageBoxResult.OK Then
                        FileTag.Covers?.Clear()
                    End If
                End If
                FileTag.Covers.Add(Pic)
            Catch ex As Exception
                Utilities.DebugMode.Instance.Log(Of TagEditor)(ex.ToString)
            End Try
            _IsFetching = False
        End Sub
    End Class
End Namespace