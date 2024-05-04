Imports System.Threading
Imports System.Web.UI.WebControls.WebParts
Imports QuickBeat.Player
Imports QuickBeat.Utilities

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

        Public Shared IsFetchingProperty As DependencyProperty = DependencyProperty.Register("IsFetching", GetType(Boolean), GetType(TagEditor))
        Property IsFetching As Boolean
            Get
                Return GetValue(IsFetchingProperty)
            End Get
            Set(value As Boolean)
                SetValue(IsFetchingProperty, value)
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
            Try
                DialogResult = True
            Catch ex As Exception
                Close()
            End Try
        End Sub

        Private Sub Button_Close_Click(sender As Object, e As RoutedEventArgs)
            'DialogResult = True
            If cToken IsNot Nothing Then cToken.Cancel()
            Close()
        End Sub

        Private Sub Commands_CancelFetch_CanExecute(sender As Object, e As CanExecuteRoutedEventArgs)
            e.CanExecute = IsFetching
        End Sub

        Private Sub Commands_CancelFetch_Executed(sender As Object, e As ExecutedRoutedEventArgs)
            cToken?.Cancel()
        End Sub

        Private Sub Commands_Fetch_CanExecute(sender As Object, e As CanExecuteRoutedEventArgs)
            e.CanExecute = SharedProperties.Instance.IsInternetConnected AndAlso Not IsFetching
        End Sub

        Private cToken As CancellationTokenSource
        Private Async Sub Commands_Fetch_Executed(sender As Object, e As ExecutedRoutedEventArgs)
            'release:=album, recording:=track
            'special characters:= + - && || ! ( ) { } [ ] ^ " ~ * ? : \/
            IsFetching = True
            Dim SpecialCharacters As String() = New String() {"+", "-", "&&", "||", "!", "(", ")", "{", "}", "[", "]", "^", """", "~", "*", "?", ":", "\/"}
            Dim SearchQuery As String = ""
            Dim IB As New Dialogs.InputBox() With {.Owner = Me, .WindowStartupLocation = WindowStartupLocation.CenterOwner}
            IB.Footer = "Use either Lucene or Title and Artist, Lucene is Prioritized"
            IB.AddTextBox("Title", True, FileTag?.Title)
            IB.AddTextBox("Artist", True, FileTag?.Artists.Split(";").FirstOrDefault)
            IB.AddTextBox("Lucene", False)
            IB.AddTextBox("Limit", False, 10)
            If Not IB.ShowDialog Then
                IsFetching = False
                Return
            End If
            Dim ELimit As Integer = 10
            Integer.TryParse(IB.Value("Limit"), ELimit)
            Dim ELucene = IB.Value("Lucene")?.Trim
            If String.IsNullOrEmpty(ELucene) Then
                Dim ETitle = IB.Value("Title").Replace("+", "\+").Replace("-", "\-").Replace("&&", "\&&").Replace("||", "\||").Replace("!", "\!").Replace("(", "\(").Replace(")", "\)").Replace("{", "\{").Replace("}", "\}").Replace("[", "\[").Replace("]", "\]").Replace("^", "\^").Replace("""", "\""").Replace("~", "\~").Replace("*", "\*").Replace("?", "\?").Replace(":", "\:").Replace("\/", "\\/")
                Dim EArtist = IB.Value("Artist").Replace("+", "\+").Replace("-", "\-").Replace("&&", "\&&").Replace("||", "\||").Replace("!", "\!").Replace("(", "\(").Replace(")", "\)").Replace("{", "\{").Replace("}", "\}").Replace("[", "\[").Replace("]", "\]").Replace("^", "\^").Replace("""", "\""").Replace("~", "\~").Replace("*", "\*").Replace("?", "\?").Replace(":", "\:").Replace("\/", "\\/")
                SearchQuery = $"Title:{ETitle} AND Artist:{EArtist}"
            Else
                SearchQuery = ELucene
            End If
            Dim Query As New MetaBrainz.MusicBrainz.Query("QuickBeat", New Version(1, 0, 0, 0))
            Dim Result = Await Query.FindRecordingsAsync(SearchQuery, ELimit)
            If Result.Results.Count = 0 Then
                IsFetching = False
                HandyControl.Controls.MessageBox.Error(Utilities.ResourceResolver.Strings.QUERY_NOMATCHES)
                Return
            End If
            Dim MP As New Dialogs.MetadataPicker() With {.Owner = Me, .WindowStartupLocation = WindowStartupLocation.CenterOwner}
            For i As Integer = 0 To Result.Results.Count - 1
                Dim k = Result.Results.Item(i)
                Dim Meta As New Metadata With {.PlayCount = k.Score, .Title = k.Item.Title}
                If k.Item.ArtistCredit IsNot Nothing Then Meta.Artists = k.Item.ArtistCredit.Select(Of String)(Function(l) l.Name).ToArray
                If k.Item.Releases IsNot Nothing AndAlso k.Item.Releases.Count > 0 Then
                    'Meta.Album = k.Item.Releases(0).Title : Meta.Tag = k.Item.Releases(0).Id
                    Meta.Album = "Appears on " & k.Item.Releases.Count & " Releases"
                    Meta.Tag = i
                End If
                If k.Item.FirstReleaseDate IsNot Nothing Then Meta.Year = k.Item.FirstReleaseDate.Year
                If k.Item.Genres IsNot Nothing AndAlso k.Item.Genres.Count > 0 Then Meta.Genres = k.Item.Genres.Select(Of String)(Function(l) l.Name).ToList
                MP.ItemsSource.Add(Meta)
            Next
            If Not MP.ShowDialog() Then
                IsFetching = False
                Return
            End If
            Dim FResult = MP.DialogMetadataResult
            If FResult.Tag IsNot Nothing Then
                Dim aMP As New Dialogs.MetadataPicker() With {.Owner = Me, .WindowStartupLocation = WindowStartupLocation.CenterOwner}
                For Each release In Result.Results.Item(FResult.Tag).Item.Releases
                    Dim Meta As New Metadata With {.Title = FResult.Title}
                    If release.ArtistCredit IsNot Nothing Then Meta.Artists = release.ArtistCredit.Select(Of String)(Function(l) l.Name).ToArray
                    Meta.Album = release.Title
                    Meta.Tag = release.Id
                    aMP.ItemsSource.Add(Meta)
                Next
                If aMP.ShowDialog Then
                    FResult.Album = aMP.DialogMetadataResult.Album
                    FResult.Tag = aMP.DialogMetadataResult.Tag
                Else
                    FResult.Tag = Result.Results.Item(FResult.Tag).Item.Id
                End If
            End If
            FileTag.Title = FResult.Title
            FileTag.Artists = FResult.JoinedArtists
            FileTag.Year = FResult.Year
            FileTag.Genres = FResult.JoinedGenres
            If FResult.Tag Is Nothing Then
                IsFetching = False
                Return
            End If
            FileTag.Album = FResult.Album
            If FResult.Tag IsNot Nothing AndAlso TypeOf FResult.Tag Is Guid Then
                Dim CA As New MetaBrainz.MusicBrainz.CoverArt.CoverArt("QuickBeat", New Version(1, 0, 0, 0), MetaBrainz.MusicBrainz.CoverArt.CoverArt.DefaultContactInfo)
                cToken = New CancellationTokenSource
                Dim FrontCover As MetaBrainz.MusicBrainz.CoverArt.CoverArtImage = Nothing
                Try
                    FrontCover = Await CA.FetchFrontAsync(CType(FResult.Tag, Guid), MetaBrainz.MusicBrainz.CoverArt.CoverArtImageSize.Original, cToken.Token)
                Catch ex As TaskCanceledException
                    IsFetching = False
                    Return
                End Try
                    FrontCover.Data.Seek(0, IO.SeekOrigin.Begin)
                Try
                    Dim ImgStream As New IO.MemoryStream
                    Dim Enco As New PngBitmapEncoder()
                    Enco.Frames.Add(BitmapFrame.Create(FrontCover.Data))
                    Enco.Save(ImgStream)
                    Dim Pic As New TagLib.Picture(New TagLib.ByteVector(ImgStream.ToArray))
                    Pic.Type = TagLib.PictureType.FrontCover
                    If FileTag.Covers.Count > 0 Then
                        If HandyControl.Controls.MessageBox.Show(Me, Utilities.ResourceResolver.Strings.QUERY_KEEPPICTURES, button:=MessageBoxButton.YesNo, icon:=MessageBoxImage.Question) <> MessageBoxResult.Yes Then
                            FileTag.Covers?.Clear()
                        End If
                    End If
                    FileTag.Covers.Add(Pic)
                Catch ex As Exception
                    Utilities.DebugMode.Instance.Log(Of TagEditor)(ex.ToString)
                End Try
            End If
            IsFetching = False
        End Sub

        Private Sub Button_CopyFrom_Click(sender As Object, e As RoutedEventArgs)
            HandyControl.Controls.MessageBox.Show(Me, Utilities.ResourceResolver.Strings.HINT_CALLREDIRECTION, icon:=MessageBoxImage.Information)
            Dim Handler As New Action(Of Player.Metadata)(Sub(meta)
                                                              FileTag.Title = meta.Title
                                                              FileTag.Artists = meta.JoinedArtists
                                                              FileTag.Genres = meta.JoinedGenres
                                                              FileTag.Album = meta.Album
                                                              FileTag.Year = meta.Year
                                                              If meta.HasCover AndAlso FileTag.Covers.Count > 0 Then
                                                                  If HandyControl.Controls.MessageBox.Show(Me, Utilities.ResourceResolver.Strings.QUERY_KEEPPICTURES, button:=MessageBoxButton.YesNo, icon:=MessageBoxImage.Question) <> MessageBoxResult.Yes Then
                                                                      FileTag.Covers?.Clear()
                                                                  End If
                                                              End If
                                                              meta.EnsureCovers()
                                                              For Each cover In meta.Covers
                                                                  Dim ImgStream As New IO.MemoryStream
                                                                  Dim Enco As New PngBitmapEncoder()
                                                                  Enco.Frames.Add(BitmapFrame.Create(cover))
                                                                  Enco.Save(ImgStream)
                                                                  Dim Pic As New TagLib.Picture(New TagLib.ByteVector(ImgStream.ToArray))
                                                                  Pic.Type = TagLib.PictureType.FrontCover
                                                                  FileTag.Covers.Add(Pic)
                                                              Next
                                                              If Not meta.IsInUse Then 'Release resources
                                                                  meta.FreeCovers()
                                                              End If
                                                          End Sub)
            SharedProperties.Instance.Player?.CaptureNextPlayCall(Handler)
        End Sub

        Private Sub TagEditor_PreviewKeyUp(sender As Object, e As KeyEventArgs) Handles Me.PreviewKeyUp
            If e.Key = Key.Escape Then Close()
        End Sub
    End Class
End Namespace