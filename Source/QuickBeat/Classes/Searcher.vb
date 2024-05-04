Imports System.Collections.ObjectModel
Imports System.ComponentModel
Imports Lucene.Net.Analysis.Standard
Imports Lucene.Net.Documents
Imports Lucene.Net.Index
Imports Lucene.Net.QueryParsers
Imports Lucene.Net.Search
Imports Lucene.Net.Store
Imports QuickBeat.Library.Library
Imports QuickBeat.Player
Imports QuickBeat.Utilities

Namespace Library.Search
    Public Class Searcher
        Implements INotifyPropertyChanged, IDisposable

        Public Const PERCAT_RESULT_LIMIT As Integer = 0 '0 = not used

        Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged

        Public Sub OnPropertyChanged(<Runtime.CompilerServices.CallerMemberName> Optional CallerName As String = Nothing)
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(CallerName))
        End Sub

        Private _Path As String
        ''' <summary>
        ''' Lucene search index location
        ''' </summary>
        ''' <returns></returns>
        ReadOnly Property Path As String
            Get
                Return _Path
            End Get
        End Property

        Private _SearchQuery As String
        Property SearchQuery As String
            Get
                Return _SearchQuery
            End Get
            Set(value As String)
                _SearchQuery = value
                OnPropertyChanged()
            End Set
        End Property

        Private _SearchResult As New AdvancedSearchResult
        ReadOnly Property SearchResult As AdvancedSearchResult
            Get
                Return _SearchResult
            End Get
        End Property

        Private _RequiresPopulate As Boolean
        ReadOnly Property RequiresPopulate As Boolean
            Get
                Return _RequiresPopulate
            End Get
        End Property

        Private _Count As Integer
        ReadOnly Property Count As Integer
            Get
                Return _Count
            End Get
        End Property

        Private _IsLoaded As Boolean = False
        Private _Library As Library = Nothing
        Private _Analyzer As New StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_30)
        Private _Directory As FSDirectory
        Private _Reader As IndexReader
        Private disposedValue As Boolean

        Sub New(Path As String, Library As Library)
            _Path = Path
            _Library = Library
        End Sub
        'Call Flow: Init -> Populate(Optional, First time only) -> Load
        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="ForceClear"></param>
        ''' <remarks>Must call <see cref="Load()"/> to reload directory</remarks>
        Sub Init(Optional ForceClear As Boolean = False)
            If _IsLoaded Then
                _IsLoaded = False
                _Directory.Dispose()
                _Reader.Dispose()
            End If
            If ForceClear OrElse (Not IO.Directory.Exists(_Path) OrElse IO.Directory.GetFiles(_Path).Length = 0) Then
                IO.Directory.CreateDirectory(_Path)
                _Directory = FSDirectory.Open(_Path)
                Dim Writer As New IndexWriter(_Directory, _Analyzer, IndexWriter.MaxFieldLength.UNLIMITED)
                Writer.Commit()
                Writer.Dispose()
                _RequiresPopulate = True
            Else
                _Directory = FSDirectory.Open(_Path)
            End If
        End Sub

        Sub Load()
            If Not IO.Directory.Exists(Path) Then Return
            If _Directory Is Nothing Then _Directory = FSDirectory.Open(_Path)
            _Reader = IndexReader.Open(_Directory, True)
            _Count = _Reader.NumDocs
            OnPropertyChanged(NameOf(Count))
            _IsLoaded = True
        End Sub

        ''' <summary>
        ''' Generates the search cache
        ''' </summary>
        Sub Populate()
            If Not _Library?.HasItems Then Return

            If _IsLoaded Then
                _Reader.Dispose()
            End If

            Dim Writer As New IndexWriter(_Directory, _Analyzer, IndexWriter.MaxFieldLength.UNLIMITED)

            For Each meta In _Library
                Dim doc As New Document()
                If Not String.IsNullOrEmpty(meta.Path) Then doc.Add(New Field("path", meta.Path, Field.Store.YES, Field.Index.ANALYZED))
                If Not String.IsNullOrEmpty(meta.Title) Then doc.Add(New Field("title", meta.Title, Field.Store.YES, Field.Index.ANALYZED))
                If Not String.IsNullOrEmpty(meta.JoinedArtists) Then doc.Add(New Field("artist", meta.JoinedArtists, Field.Store.YES, Field.Index.ANALYZED))
                If Not String.IsNullOrEmpty(meta.Album) Then doc.Add(New Field("album", meta.Album, Field.Store.YES, Field.Index.ANALYZED))
                If Not String.IsNullOrEmpty(meta.JoinedGenres) Then doc.Add(New Field("genre", meta.JoinedGenres, Field.Store.YES, Field.Index.ANALYZED))
                If Not String.IsNullOrEmpty(meta.Year.ToString) Then doc.Add(New Field("year", meta.Year, Field.Store.YES, Field.Index.ANALYZED))
                doc.Add(New Field("location", meta.Location.ToString, Field.Store.YES, Field.Index.ANALYZED))
                Writer.AddDocument(doc)
            Next
            Writer.Commit()
            Writer.Dispose()

            If _IsLoaded Then
                _Reader = IndexReader.Open(_Directory, True)
                _Count = _Reader.NumDocs
                OnPropertyChanged(NameOf(Count))
            End If
        End Sub

        Sub Add(Meta As Metadata)
            If _IsLoaded Then
                _Reader.Dispose()
            End If

            Dim Writer As New IndexWriter(_Directory, _Analyzer, IndexWriter.MaxFieldLength.UNLIMITED)

            Dim doc As New Document()
            If Not String.IsNullOrEmpty(Meta.Path) Then doc.Add(New Field("path", Meta.Path, Field.Store.YES, Field.Index.ANALYZED))
            If Not String.IsNullOrEmpty(Meta.Title) Then doc.Add(New Field("title", Meta.Title, Field.Store.YES, Field.Index.ANALYZED))
            If Not String.IsNullOrEmpty(Meta.JoinedArtists) Then doc.Add(New Field("artist", Meta.JoinedArtists, Field.Store.YES, Field.Index.ANALYZED))
            If Not String.IsNullOrEmpty(Meta.Album) Then doc.Add(New Field("album", Meta.Album, Field.Store.YES, Field.Index.ANALYZED))
            If Not String.IsNullOrEmpty(Meta.JoinedGenres) Then doc.Add(New Field("genre", Meta.JoinedGenres, Field.Store.YES, Field.Index.ANALYZED))
            If Not String.IsNullOrEmpty(Meta.Year.ToString) Then doc.Add(New Field("year", Meta.Year, Field.Store.YES, Field.Index.ANALYZED))
            doc.Add(New Field("location", Meta.Location.ToString, Field.Store.YES, Field.Index.ANALYZED))
            Writer.AddDocument(doc)

            Writer.Commit()
            Writer.Dispose()

            If _IsLoaded Then
                _Reader = IndexReader.Open(_Directory, True)
                _Count += 1
                OnPropertyChanged(NameOf(Count))
            End If
        End Sub

        Sub Delete(Meta As Metadata)
            If _IsLoaded Then
                _Reader.Dispose()
            End If

            Dim Writer As New IndexWriter(_Directory, _Analyzer, IndexWriter.MaxFieldLength.UNLIMITED)

            Dim qParser As New MultiFieldQueryParser(Lucene.Net.Util.Version.LUCENE_30, New String() {"path", "title", "artist", "album", "genre", "year"}, _Analyzer)
            Dim pQuery = qParser.Parse($"path:""{Meta.Path}"" AND title:""{Meta.Title}"" AND artist:{Meta.DefaultArtist} AND album:""{Meta.Album}""".Replace("+", "\+").Replace("-", "\-").Replace("&&", "\&&").Replace("||", "\||").Replace("!", "\!").Replace("(", "\(").Replace(")", "\)").Replace("{", "\{").Replace("}", "\}").Replace("[", "\[").Replace("]", "\]").Replace("^", "\^").Replace("""", "\""").Replace("~", "\~").Replace("*", "\*").Replace("?", "\?").Replace(":", "\:").Replace("\/", "\\/"))

            Writer.DeleteDocuments(pQuery)

            Writer.Commit()
            Writer.Dispose()

            If _IsLoaded Then
                _Reader = IndexReader.Open(_Directory, True)
                _Count -= 1
                OnPropertyChanged(NameOf(Count))
            End If
        End Sub

        Sub Search(query As String, Optional limit As Integer = 10000)
            If _Reader Is Nothing Then _Reader = IndexReader.Open(_Directory, True)

            Dim Searcher As New IndexSearcher(_Reader)
            Dim MasterQuery As New BooleanQuery()
            Dim qParser As New MultiFieldQueryParser(Lucene.Net.Util.Version.LUCENE_30, New String() {"path", "title", "genre", "year"}, _Analyzer)
            Dim pQuery = qParser.Parse(query)

            Dim Results = Searcher.Search(pQuery, n:=limit)

            Dim TrackMatches As New ObservableCollection(Of Tuple(Of Double, Metadata))
            Dim GroupMatches As New ObservableCollection(Of Tuple(Of Double, MetadataGroup))

            If Results IsNot Nothing OrElse Results.TotalHits = 0 Then
                For i As Integer = 0 To If(PERCAT_RESULT_LIMIT = 0 OrElse PERCAT_RESULT_LIMIT >= Results.TotalHits, Results.TotalHits - 1, PERCAT_RESULT_LIMIT)
                    Dim doc = Searcher.Doc(Results.ScoreDocs(i).Doc)
                    If TrackMatches.FirstOrDefault(Function(k) k.Item2.Path = doc.Get("path")) IsNot Nothing Then Continue For
                    Dim meta = Metadata.FromFile(doc.Get("path"), True)
                    If meta Is Nothing Then Continue For
                    TrackMatches.Add(New Tuple(Of Double, Metadata)(Results.ScoreDocs(i).Score, meta))
                Next
            End If

            Dim SqParser = New QueryParser(Lucene.Net.Util.Version.LUCENE_30, "artist", _Analyzer)
            Dim SpQuery = SqParser.Parse(query)

            Results = Searcher.Search(SpQuery, n:=limit)

            If Results IsNot Nothing OrElse Results.TotalHits = 0 Then
                For i As Integer = 0 To If(PERCAT_RESULT_LIMIT = 0 OrElse PERCAT_RESULT_LIMIT >= Results.TotalHits, Results.TotalHits - 1, PERCAT_RESULT_LIMIT)
                    Dim doc = Searcher.Doc(Results.ScoreDocs(i).Doc)
                    If GroupMatches.FirstOrDefault(Function(k) k.Item2 IsNot Nothing AndAlso k.Item2.Type = MetadataGroup.GroupType.Artist AndAlso k.Item2.Name.ToLower = doc.Get("artist").ToLower) IsNot Nothing Then Continue For
                    Dim artist = _Library.GetArtistGroup(doc.Get("artist"))
                    If artist Is Nothing Then Continue For
                    GroupMatches.Add(New Tuple(Of Double, MetadataGroup)(Results.ScoreDocs(i).Score, artist))
                Next
            End If

            SqParser = New QueryParser(Lucene.Net.Util.Version.LUCENE_30, "album", _Analyzer)
            SpQuery = SqParser.Parse(query)

            Results = Searcher.Search(SpQuery, n:=limit)

            If Results IsNot Nothing OrElse Results.TotalHits = 0 Then
                For i As Integer = 0 To If(PERCAT_RESULT_LIMIT = 0 OrElse PERCAT_RESULT_LIMIT >= Results.TotalHits, Results.TotalHits - 1, PERCAT_RESULT_LIMIT)
                    Dim doc = Searcher.Doc(Results.ScoreDocs(i).Doc)
                    If GroupMatches.FirstOrDefault(Function(k) k.Item2 IsNot Nothing AndAlso k.Item2.Type = MetadataGroup.GroupType.Album AndAlso k.Item2.Name.ToLower = doc.Get("album").ToLower) IsNot Nothing Then Continue For
                    Dim album = _Library.GetAlbumGroup(doc.Get("album"))
                    If album Is Nothing Then Continue For
                    GroupMatches.Add(New Tuple(Of Double, MetadataGroup)(Results.ScoreDocs(i).Score, album))
                Next
            End If

            TrackMatches.FirstOrDefault?.Item2?.EnsureCovers(True)

            _SearchResult = New AdvancedSearchResult(TrackMatches.FirstOrDefault, GroupMatches.FirstOrDefault, TrackMatches, GroupMatches)
            OnPropertyChanged(NameOf(SearchResult))
        End Sub

        Async Function SearchAsync(query As String, Optional limit As Integer = 10000, Optional SkipCover As Boolean = False, Optional GeneratePlaylists As Boolean = True) As Task
            If _Reader Is Nothing Then _Reader = IndexReader.Open(_Directory, True)

            Dim Searcher As New IndexSearcher(_Reader)
            Dim MasterQuery As New BooleanQuery()
            Dim qParser As New MultiFieldQueryParser(Lucene.Net.Util.Version.LUCENE_30, New String() {"path", "title", "genre", "year"}, _Analyzer)
            Dim pQuery = qParser.Parse(query)

            Dim Results As TopDocs = Nothing
            Await Task.Run(Sub()
                               Try
                                   Results = Searcher.Search(pQuery, n:=limit)
                               Catch ex As Exception
                                   Utilities.DebugMode.Instance.Log(Of Searcher)(ex.ToString)
                               End Try
                           End Sub)

            Dim TrackMatches As New ObservableCollection(Of Tuple(Of Double, Metadata))
            Dim GroupMatches As New ObservableCollection(Of Tuple(Of Double, MetadataGroup))

            If Results IsNot Nothing OrElse Results.TotalHits = 0 Then
                For i As Integer = 0 To If(PERCAT_RESULT_LIMIT = 0 OrElse PERCAT_RESULT_LIMIT >= Results.ScoreDocs.Length, Results.ScoreDocs.Length - 1, PERCAT_RESULT_LIMIT)
                    Dim doc = Searcher.Doc(Results.ScoreDocs(i).Doc)
                    If TrackMatches.FirstOrDefault(Function(k) k.Item2?.Path = doc.Get("path")) IsNot Nothing Then Continue For
                    Select Case doc.Get("location")
                        Case "Local"
                            Dim meta = Metadata.FromFile(doc.Get("path"), True)
                            If meta Is Nothing Then Continue For
                            TrackMatches.Add(New Tuple(Of Double, Metadata)(Results.ScoreDocs(i).Score, meta))
                        Case "Internal"
                            Dim meta As New InternalMetadata(doc.Get("path")) : meta.RefreshTagsFromFile(True)
                            TrackMatches.Add(New Tuple(Of Double, Metadata)(Results.ScoreDocs(i).Score, meta))
                    End Select
                Next
            End If

            Dim SqParser = New QueryParser(Lucene.Net.Util.Version.LUCENE_30, "artist", _Analyzer)
            Dim SpQuery = SqParser.Parse(query)

            Await Task.Run(Sub()
                               Results = Searcher.Search(SpQuery, n:=limit)
                           End Sub)

            If Results IsNot Nothing OrElse Results.ScoreDocs.Length = 0 Then
                For i As Integer = 0 To If(PERCAT_RESULT_LIMIT = 0 OrElse PERCAT_RESULT_LIMIT >= Results.ScoreDocs.Length, Results.ScoreDocs.Length - 1, PERCAT_RESULT_LIMIT)
                    Dim doc = Searcher.Doc(Results.ScoreDocs(i).Doc)
                    If GroupMatches.FirstOrDefault(Function(k) k.Item2 IsNot Nothing AndAlso k.Item2.Type = MetadataGroup.GroupType.Artist AndAlso k.Item2.Name.ToLower = doc.Get("artist").ToLower) IsNot Nothing Then Continue For
                    Dim artist = _Library.GetArtistGroup(doc.Get("artist"))
                    If artist Is Nothing Then Continue For
                    GroupMatches.Add(New Tuple(Of Double, MetadataGroup)(Results.ScoreDocs(i).Score, artist))
                Next
            End If

            SqParser = New QueryParser(Lucene.Net.Util.Version.LUCENE_30, "album", _Analyzer)
            SpQuery = SqParser.Parse(query)
            Await Task.Run(Sub()
                               Results = Searcher.Search(SpQuery, n:=limit)
                           End Sub)
            If Results IsNot Nothing OrElse Results.ScoreDocs.Length = 0 Then
                For i As Integer = 0 To If(PERCAT_RESULT_LIMIT = 0 OrElse PERCAT_RESULT_LIMIT >= Results.ScoreDocs.Length, Results.ScoreDocs.Length - 1, PERCAT_RESULT_LIMIT)
                    Dim doc = Searcher.Doc(Results.ScoreDocs(i).Doc)
                    If GroupMatches.FirstOrDefault(Function(k) k.Item2 IsNot Nothing AndAlso k.Item2.Type = MetadataGroup.GroupType.Album AndAlso k.Item2.Name.ToLower = doc.Get("album").ToLower) IsNot Nothing Then Continue For
                    Dim album = _Library.GetAlbumGroup(doc.Get("album"))
                    If album Is Nothing Then Continue For
                    GroupMatches.Add(New Tuple(Of Double, MetadataGroup)(Results.ScoreDocs(i).Score, album))
                Next
            End If
            If Not SkipCover Then TrackMatches.FirstOrDefault?.Item2?.EnsureCovers(True)

            _SearchResult = New AdvancedSearchResult(TrackMatches.FirstOrDefault, GroupMatches.FirstOrDefault, TrackMatches, GroupMatches, GeneratePlaylists)
            OnPropertyChanged(NameOf(SearchResult))
        End Function

        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not disposedValue Then
                If disposing Then
                    ' TODO: dispose managed state (managed objects)
                    _IsLoaded = False
                    _Analyzer?.Dispose()
                    _Directory?.Dispose()
                    _Reader?.Dispose()
                End If

                ' TODO: free unmanaged resources (unmanaged objects) and override finalizer
                ' TODO: set large fields to null
                disposedValue = True
            End If
        End Sub

        ' ' TODO: override finalizer only if 'Dispose(disposing As Boolean)' has code to free unmanaged resources
        ' Protected Overrides Sub Finalize()
        '     ' Do not change this code. Put cleanup code in 'Dispose(disposing As Boolean)' method
        '     Dispose(disposing:=False)
        '     MyBase.Finalize()
        ' End Sub

        Public Sub Dispose() Implements IDisposable.Dispose
            ' Do not change this code. Put cleanup code in 'Dispose(disposing As Boolean)' method
            Dispose(disposing:=True)
            GC.SuppressFinalize(Me)
        End Sub
    End Class
End Namespace