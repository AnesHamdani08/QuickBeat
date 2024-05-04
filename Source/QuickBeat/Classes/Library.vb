Imports System.Collections.ObjectModel
Imports System.ComponentModel
Imports System.Runtime.CompilerServices
Imports QuickBeat.Classes
Imports QuickBeat.Hotkeys
Imports QuickBeat.Interfaces
Imports QuickBeat.Player
Imports QuickBeat.Utilities

Namespace Library
    Public Class Library
        Inherits ObjectModel.ObservableCollection(Of Metadata)
        Implements Interfaces.IStartupItem

        Public Event ArtistFocused(Group As MetadataGroup)
        Public Event AlbumFocused(Group As MetadataGroup)
        Public Event GroupFocused(Group As MetadataGroup)
        Public Event MostPlayedChanged(Sender As Library)
        ''' <summary>
        ''' Occures when a <see cref="Player.Metadata.IsFavorite"/> changes value
        ''' </summary>
        ''' <param name="sender"></param>
        Public Event FavoriteChanged(sender As Player.Metadata)
        ''' <summary>
        ''' Occures when a <see cref="Player.MetadataGroup.IsFavorite"/> changes value
        ''' </summary>
        ''' <param name="sender"></param>
        Public Event GroupFavoriteChanged(sender As Player.MetadataGroup)
#Region "Constants"
        Const SEARCH_TITLE_TOLERANCE = 0.8F
        Const SEARCH_F_TITLE_TOLERANCE = 0.5F
        Const SEARCH_ARTIST_TOLERANCE = 0.8F
        Const SEARCH_ALBUM_TOLERANCE = 0.8F
        Const SEARCH_LIMIT_ARTIST = 5
        Const SEARCH_LIMIT_ALBUM = 5
#End Region
#Region "Raisers"
        Public Sub OnFavoriteChanged(sender As Player.Metadata)
            RaiseEvent FavoriteChanged(sender)
        End Sub
        Public Sub OnGroupFavoriteChanged(sender As Player.MetadataGroup)
            RaiseEvent GroupFavoriteChanged(sender)
        End Sub
#End Region
#Region "Properties"
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

        ReadOnly Property SearchCommand As New QuickBeat.Player.WPF.Commands.SearchCommand(Me)
        ReadOnly Property FocusArtistCommand As New QuickBeat.Player.WPF.Commands.FocusArtistGroupCommand(Me)
        ReadOnly Property FocusAlbumCommand As New QuickBeat.Player.WPF.Commands.FocusAlbumGroupCommand(Me)
        ReadOnly Property FocusGroupCommand As New QuickBeat.Player.WPF.Commands.FocusGroupCommand(Me)

        Private _SearchIndex As Integer
        ''' <summary>
        ''' A helper property for binding with an <see cref="ItemsControl"/>
        ''' </summary>
        ''' <returns></returns>
        Property SearchIndex As Integer
            Get
                Return _SearchIndex
            End Get
            Set(value As Integer)
                _SearchIndex = value
                If value < 0 Then Return
#Disable Warning
                Utilities.SharedProperties.Instance.Player?.LoadSong(SearchResult?.TrackMatches(value).Item2)
#Enable Warning
            End Set
        End Property

        Private _FocusedGroup As MetadataGroup
        Property FocusedGroup As MetadataGroup
            Get
                Return _FocusedGroup
            End Get
            Set(value As MetadataGroup)
                _FocusedGroup = value
                OnPropertyChanged()
            End Set
        End Property

        ReadOnly Property HasItems As Boolean
            Get
                Return Count > 0
            End Get
        End Property

        Public Property Configuration As New StartupItemConfiguration("Library") Implements IStartupItem.Configuration

        Private _MostPlayed As Metadata
        Property MostPlayed As Metadata
            Get
                Return _MostPlayed
            End Get
            Set(value As Metadata)
                If value Is _MostPlayed Then Return
                If _MostPlayed IsNot Nothing Then
                    _MostPlayed.CleanableConfiguration.Unlock() 'IsInUse = False
                    If Not _MostPlayed.IsInUse Then _MostPlayed.FreeCovers()
                End If
                _MostPlayed = value
                If _MostPlayed IsNot Nothing Then
                    _MostPlayed.EnsureCovers()
                    _MostPlayed.CleanableConfiguration.Lock() 'IsInUse = True
                End If
                OnPropertyChanged()
                RaiseEvent MostPlayedChanged(Me)
            End Set
        End Property

        Private _MostPlayedArtist As MetadataGroup
        Property MostPlayedArtist As MetadataGroup
            Get
                Return _MostPlayedArtist
            End Get
            Set(value As MetadataGroup)
                If value Is _MostPlayedArtist Then Return
                If _MostPlayedArtist IsNot Nothing Then
                    _MostPlayedArtist.IsInUse = False
                    _MostPlayedArtist.FreeCovers()
                End If
                _MostPlayedArtist = value
                If _MostPlayedArtist IsNot Nothing Then
                    _MostPlayedArtist.IsInUse = True
                    _MostPlayedArtist.UpdateValues()
                End If
                OnPropertyChanged()
                RaiseEvent MostPlayedChanged(Me)
            End Set
        End Property

        Private _MostPlayedArtist2 As MetadataGroup
        Property MostPlayedArtist2 As MetadataGroup
            Get
                Return _MostPlayedArtist2
            End Get
            Set(value As MetadataGroup)
                If value Is _MostPlayedArtist2 Then Return
                If _MostPlayedArtist2 IsNot Nothing Then
                    _MostPlayedArtist2.IsInUse = False
                    _MostPlayedArtist2.FreeCovers()
                End If
                _MostPlayedArtist2 = value
                If _MostPlayedArtist2 IsNot Nothing Then
                    _MostPlayedArtist2.IsInUse = True
                    _MostPlayedArtist2.UpdateValues()
                End If
                OnPropertyChanged()
                RaiseEvent MostPlayedChanged(Me)
            End Set
        End Property

        Private _MostPlayedArtist3 As MetadataGroup
        Property MostPlayedArtist3 As MetadataGroup
            Get
                Return _MostPlayedArtist3
            End Get
            Set(value As MetadataGroup)
                If value Is _MostPlayedArtist3 Then Return
                If _MostPlayedArtist3 IsNot Nothing Then
                    _MostPlayedArtist3.IsInUse = False
                    _MostPlayedArtist3.FreeCovers()
                End If
                _MostPlayedArtist3 = value
                If _MostPlayedArtist3 IsNot Nothing Then
                    _MostPlayedArtist3.IsInUse = True
                    _MostPlayedArtist3.UpdateValues()
                End If
                OnPropertyChanged()
                RaiseEvent MostPlayedChanged(Me)
            End Set
        End Property

        ReadOnly Property RandomGroup As MetadataGroup
            Get
                If Count = 0 Then Return Nothing
                Dim rnd As New Random()
                Dim group As New MetadataGroup With {.Name = "Random", .Category = "Custom", .Cover = CommonFunctions.GenerateCoverImage(Utilities.ResourceResolver.Images.RANDOM), .IsCoverLocked = True}
                Dim PickedNumbers As New List(Of Integer)
                Dim i = 25
                Dim TimeOut As Integer = 0
                Do While i > 0
                    If TimeOut = 100 Then
                        Utilities.DebugMode.Instance.Log(Of Library)("Hit timeout on random group.")
                        Exit Do
                    End If
                    TimeOut += 1
                    Dim _i = rnd.Next(0, Count)
                    If PickedNumbers.Contains(_i) Then
                        Continue Do
                    End If
                    PickedNumbers.Add(_i)
                    group.Add(Me(_i))
                    i -= 1
                Loop
                Return group
            End Get
        End Property

        Private _IsSearching As Boolean
        Property IsSearching As Boolean
            Get
                Return _IsSearching
            End Get
            Set(value As Boolean)
                _IsSearching = value
                OnPropertyChanged()
            End Set
        End Property

        Private _SearchTookTime As String = "0"
        Property SearchTookTime As String
            Get
                Return _SearchTookTime
            End Get
            Set(value As String)
                _SearchTookTime = value
                OnPropertyChanged()
            End Set
        End Property

        Private _NeedsCacheRefresh As Boolean
        Property NeedsCacheRefresh As Boolean
            Get
                Return _NeedsCacheRefresh
            End Get
            Set(value As Boolean)
                _NeedsCacheRefresh = value
                OnPropertyChanged()
            End Set
        End Property

        Private _IndexedCount As Integer
        ''' <summary>
        ''' Returns the number of <see cref="Lucene.Net.Documents.Document"/> in the current <see cref="Lucene.Net.Store.FSDirectory"/>
        ''' </summary>
        ''' <returns></returns>
        ReadOnly Property IndexedCount As Integer
            Get
                Return _IndexedCount
            End Get
        End Property

        'Used for search engine indexing, provides faster search operations
        Private Searcher As New Search.Searcher(IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "QuickBeat", GetEncodedSpecialAppName, "global_index"), Me)
        Private Lookup_Path As ILookup(Of String, Metadata)
        Private Lookup_Title As ILookup(Of String, Metadata)
        Private Lookup_Artist As ILookup(Of String, Metadata)
        Private Lookup_Album As ILookup(Of String, Metadata)
        Private Lookup_Year As ILookup(Of Integer, Metadata)

        Private _MostPlayedRefreshCounter As Integer = 0

#End Region
#Region "Methods"
        Async Sub Search()
            If IsSearching Then Return
            IsSearching = True
            If NeedsCacheRefresh Then
                Searcher.Init()
                Searcher.Load()
                _IndexedCount = Searcher.Count
                OnPropertyChanged(NameOf(IndexedCount))
            End If
            If Searcher.Count = 0 AndAlso Me.HasItems Then RebuildSearchCache()
            Dim sw As New Stopwatch
            sw.Start()
            Try
                Await Searcher.SearchAsync(SearchQuery)
            Catch ex As Exception
                Utilities.DebugMode.Instance.Log(Of Library)(ex.ToString)
            End Try
            sw.Stop()
            SearchTookTime = Math.Round(sw.Elapsed.TotalSeconds, 3)
            _SearchResult = Searcher.SearchResult
            OnPropertyChanged(NameOf(SearchResult))
            IsSearching = False
        End Sub

        Private _IsSearchingReturn As Boolean = False
        ''' <summary>
        ''' Returns search result for given query while skipping covers and not generating <see cref="AdvancedSearchResult.TopMatchRelatedPlaylists"/>
        ''' </summary>
        ''' <param name="query">Search query</param>
        ''' <returns></returns>
        Async Function SearchReturn(query As String) As Task(Of AdvancedSearchResult)
            If _IsSearchingReturn Then Return Nothing
            _IsSearchingReturn = True
            Try
                Await Searcher.SearchAsync(query, SkipCover:=True, GeneratePlaylists:=False)
            Catch ex As Exception
                Utilities.DebugMode.Instance.Log(Of Library)(ex.ToString)
            End Try
            _IsSearchingReturn = False
            Return Searcher.SearchResult
        End Function

        Async Sub SearchEX(Optional SearchFilePathOnly As Boolean = False)
            If String.IsNullOrEmpty(SearchQuery.Trim) Then Return
            If IsSearching Then Return
            Utilities.DebugMode.Instance.Log(Of Library)("Attempting to search, Query:=" & SearchQuery & "...")
            IsSearching = True
            SearchResult?.TrackMatches?.Clear()
            SearchResult?.GroupMatches?.Clear()
            _SearchResult = New AdvancedSearchResult
            Await Task.Run(Sub()
                               Dim sw As New Stopwatch
                               sw.Start()

                               If NeedsCacheRefresh Then RefreshSearchCacheEX(True)

                               Dim LowerQuery = SearchQuery.ToLower
                               Dim SplitQuery = SearchQuery.Split(New Char() {" "}, StringSplitOptions.RemoveEmptyEntries)
                               Dim QueryPath = IO.Path.GetDirectoryName(LowerQuery)
                               Dim fItem = If(SearchFilePathOnly, Nothing, If(String.IsNullOrEmpty(QueryPath), Nothing, Lookup_Path?.Where(Function(k) k.Key.ToLower.Contains(QueryPath))))
                               Dim fItemFullPath = Me.Where(Function(k) k.Path.ToLower.Contains(LowerQuery))
                               'Compare first letter of each split query to the cached first letter of metadata titles
                               'Only matching % of 0.8 and higher are taken
                               'Ordered by matching percentage 0->1                                                                                                                        
                               Dim fItemTitle = If(SearchFilePathOnly, Nothing, Lookup_Title?.Select(Function(k) New Tuple(Of Single, IGrouping(Of String, Metadata))(SplitQuery.Select(Of Single)(Function(l) l.FirstOrDefault.ToString.GetSimilarity(k.Key)).Sum, k))).Where(Function(k) k.Item1 > SEARCH_TITLE_TOLERANCE).OrderByDescending(Of Single)(Function(k) k.Item1)
                               'Filter the matching items from Lookup_Title
                               'Only matching % of 0.5/0.8 and higher are taken
                               'Ordered by matching percentage 0->1
                               Dim FilteredfItemTitle = If(SearchFilePathOnly, Nothing, fItemTitle?.SelectMany(Of Tuple(Of Single, Metadata))(Function(k) k.Item2.Select(Of Tuple(Of Single, Metadata))(Function(l) New Tuple(Of Single, Metadata)(LowerQuery.GetSplitSimilarity(l.Title), l))).Where(Function(k) k.Item1 >= SEARCH_F_TITLE_TOLERANCE AndAlso k.Item2?.Title.Split(" ").Length < SplitQuery.Length + 3).OrderByDescending(Of Single)(Function(k) k.Item1))
                               Dim fItemArtist = If(SearchFilePathOnly, Nothing, Lookup_Artist?.Select(Of Tuple(Of Single, IGrouping(Of String, Metadata)))(Function(k) New Tuple(Of Single, IGrouping(Of String, Metadata))(LowerQuery.GetSplitSimilarity(k.Key), k)).Where(Function(k) k.Item1 >= SEARCH_ARTIST_TOLERANCE AndAlso k.Item2.Key?.Split(" ").Length < SplitQuery.Length + 3).OrderByDescending(Of Single)(Function(k) k.Item1))
                               Dim fItemAlbum = If(SearchFilePathOnly, Nothing, Lookup_Album?.Select(Of Tuple(Of Single, IGrouping(Of String, Metadata)))(Function(k) New Tuple(Of Single, IGrouping(Of String, Metadata))(LowerQuery.GetSplitSimilarity(k.Key), k)).Where(Function(k) k.Item1 >= SEARCH_ALBUM_TOLERANCE AndAlso k.Item2.Key?.Split(" ").Length < SplitQuery.Length + 3).OrderByDescending(Of Single)(Function(k) k.Item1))

                               'Provide equal opportunities, beacause some artist like to have a full article as a song title and thus the similarity function will keep incrementing the last value by that article length and thus will result in wrong results
                               'It will impact perforamnce,but hey, quality over quantity right?
                               Dim MaxTitleSplitLength, MaxArtistSplitLength, MaxAlbumSplitLength As Integer
                               If FilteredfItemTitle IsNot Nothing Then
                                   MaxTitleSplitLength = FilteredfItemTitle.Select(Of Integer)(Function(k) k.Item2?.Title.Split(" ").Length).OrderByDescending(Of Integer)(Function(l) l).FirstOrDefault
                                   If MaxTitleSplitLength > 1 Then
                                       FilteredfItemTitle = FilteredfItemTitle.Select(Function(k)
                                                                                          Dim preV = (MaxTitleSplitLength - k.Item2?.Title.Split(" ").Length) + 1
                                                                                          Dim V = preV * k.Item1
                                                                                          Return New Tuple(Of Single, Metadata)(V, k.Item2)
                                                                                      End Function).OrderByDescending(Of Single)(Function(k) k.Item1)
                                   End If
                               End If
                               If fItemArtist IsNot Nothing Then
                                   MaxArtistSplitLength = fItemArtist.Select(Of Integer)(Function(k) k.Item2?.Key?.Split(" ").Length).OrderByDescending(Of Integer)(Function(l) l).FirstOrDefault
                                   If MaxArtistSplitLength > 1 Then
                                       fItemArtist = fItemArtist.Select(Function(k)
                                                                            Dim preV = (MaxArtistSplitLength - k.Item2?.Key?.Split(" ").Length) + 1
                                                                            Dim V = preV * k.Item1
                                                                            Return New Tuple(Of Single, IGrouping(Of String, Metadata))(V, k.Item2)
                                                                        End Function).OrderByDescending(Of Single)(Function(k) k.Item1)
                                   End If
                               End If
                               If fItemAlbum IsNot Nothing Then
                                   MaxAlbumSplitLength = fItemAlbum.Select(Of Integer)(Function(k) k.Item2?.Key?.Split(" ").Length).OrderByDescending(Of Integer)(Function(l) l).FirstOrDefault
                                   If MaxAlbumSplitLength > 1 Then
                                       fItemAlbum = fItemAlbum.Select(Function(k)
                                                                          Dim preV = (MaxAlbumSplitLength - k.Item2?.Key?.Split(" ").Length) + 1
                                                                          Dim V = preV * k.Item1
                                                                          Return New Tuple(Of Single, IGrouping(Of String, Metadata))(V, k.Item2)
                                                                      End Function).OrderByDescending(Of Single)(Function(k) k.Item1)
                                   End If
                               End If

                               Dim TrackMatches As New ObservableCollection(Of Tuple(Of Double, Metadata))
                               Dim ArtistMatches As New ObservableCollection(Of Tuple(Of Double, MetadataGroup))
                               Dim AlbumMatches As New ObservableCollection(Of Tuple(Of Double, MetadataGroup))

                               If fItem IsNot Nothing Then
                                   For Each fResult In fItem
                                       For Each _fResult In fResult
                                           If TrackMatches.FirstOrDefault(Function(k) k.Item2 Is _fResult) IsNot Nothing Then Continue For
                                           TrackMatches.Add(New Tuple(Of Double, Metadata)(100, _fResult))
                                       Next
                                   Next
                               End If

                               If fItemFullPath IsNot Nothing Then
                                   For Each fResult In fItemFullPath
                                       If TrackMatches.FirstOrDefault(Function(k) k.Item2 Is fResult) IsNot Nothing Then Continue For
                                       TrackMatches.Add(New Tuple(Of Double, Metadata)(100, fResult))
                                   Next
                               End If

                               If FilteredfItemTitle IsNot Nothing Then
                                   For Each fResult In FilteredfItemTitle
                                       If TrackMatches.FirstOrDefault(Function(k) k.Item2 Is fResult.Item2) IsNot Nothing Then Continue For
                                       TrackMatches.Add(New Tuple(Of Double, Metadata)(fResult.Item1 * 100, fResult.Item2))
                                   Next
                               End If

                               If fItemArtist IsNot Nothing Then
                                   For Each fResult In fItemArtist
                                       Dim MDG As New MetadataGroup() With {.Type = MetadataGroup.GroupType.Artist, .Name = StrConv(fResult.Item2.Key, VbStrConv.ProperCase)}
                                       For Each sItem In fResult.Item2
                                           MDG.Add(sItem)
                                       Next
                                       ArtistMatches.Add(New Tuple(Of Double, MetadataGroup)(fResult.Item1 * 100, MDG))
                                   Next
                               End If

                               If fItemAlbum IsNot Nothing Then
                                   For Each fResult In fItemAlbum
                                       Dim MDG As New MetadataGroup() With {.Type = MetadataGroup.GroupType.Album, .Name = StrConv(fResult.Item2.Key, VbStrConv.ProperCase)}
                                       For Each sItem In fResult.Item2
                                           MDG.Add(sItem)
                                       Next
                                       AlbumMatches.Add(New Tuple(Of Double, MetadataGroup)(fResult.Item1 * 100, MDG))
                                   Next
                               End If

                               Dim qYear As Integer = -1
                               Dim rYear As Tuple(Of Double, MetadataGroup) = Nothing
                               If Not SearchFilePathOnly AndAlso Integer.TryParse(SearchQuery, qYear) Then
                                   If qYear >= 1900 Then
                                       Dim fItemYear = Lookup_Year.FirstOrDefault(Function(k) k.Key = qYear)
                                       If fItemYear IsNot Nothing Then
                                           Dim MDG As New MetadataGroup() With {.Type = MetadataGroup.GroupType.Artist, .Name = qYear}
                                           For Each sItem In fItemYear
                                               MDG.Add(sItem)
                                           Next
                                           rYear = New Tuple(Of Double, MetadataGroup)(100, MDG)
                                       End If
                                   End If
                               End If

                               sw.Stop()
                               SearchTookTime = Math.Round(sw.Elapsed.TotalSeconds, 3)

                               Dim TopArtistMatch = fItemArtist.FirstOrDefault
                               Dim TopAlbumMatch = fItemAlbum.FirstOrDefault
                               Dim TopTrackMatch = FilteredfItemTitle.FirstOrDefault

                               'Cleanup, will be done automaticaly even if left alone ;)
                               fItem = Nothing
                               fItemTitle = Nothing
                               FilteredfItemTitle = Nothing
                               fItemArtist = Nothing
                               fItemAlbum = Nothing

                               TopTrackMatch?.Item2?.EnsureCovers(FreezeThumbs:=True)
                               Dim r_TopTrackMatch As New Tuple(Of Double, Metadata)(If(TopTrackMatch?.Item1 * 100, 0), TopTrackMatch?.Item2)
                               Dim r_TopGroupMatch As Tuple(Of Double, MetadataGroup)
                               If qYear >= 1900 AndAlso rYear IsNot Nothing Then
                                   r_TopGroupMatch = rYear
                               Else
                                   If (TopArtistMatch IsNot Nothing AndAlso TopAlbumMatch IsNot Nothing AndAlso TopArtistMatch.Item1 > TopAlbumMatch.Item1) OrElse TopArtistMatch IsNot Nothing Then
                                       Dim MDG As New MetadataGroup With {.Type = MetadataGroup.GroupType.Artist, .Name = StrConv(TopArtistMatch.Item2.Key, VbStrConv.ProperCase)}
                                       For Each meta In TopArtistMatch.Item2
                                           MDG.Add(meta)
                                       Next
                                       If MDG.Cover Is Nothing Then
                                           Dim mMDG = MDG.FirstOrDefault(Function(k) k.HasCover)
                                           Dim Freeze As Boolean = False
                                           If mMDG.Covers Is Nothing Then Freeze = True
                                           mMDG?.EnsureCovers(True)
                                           MDG.Cover = mMDG?.DefaultCover
                                           If Freeze Then MDG.Cover.Freeze()
                                       End If
                                       r_TopGroupMatch = New Tuple(Of Double, MetadataGroup)(TopArtistMatch.Item1 * 100, MDG)
                                   ElseIf TopAlbumMatch IsNot Nothing Then
                                       Dim MDG As New MetadataGroup With {.Type = MetadataGroup.GroupType.Album, .Name = StrConv(TopAlbumMatch.Item2.Key, VbStrConv.ProperCase)}
                                       For Each meta In TopAlbumMatch.Item2
                                           MDG.Add(meta)
                                       Next
                                       If MDG.Cover Is Nothing Then
                                           Dim mMDG = MDG.FirstOrDefault(Function(k) k.HasCover)
                                           Dim Freeze As Boolean = False
                                           If mMDG.Covers Is Nothing Then Freeze = True
                                           mMDG?.EnsureCovers(True)
                                           MDG.Cover = mMDG?.DefaultCover
                                           If Freeze Then MDG.Cover.Freeze()
                                       End If
                                       r_TopGroupMatch = New Tuple(Of Double, MetadataGroup)(TopAlbumMatch.Item1 * 100, MDG)
                                   Else
                                       r_TopGroupMatch = New Tuple(Of Double, MetadataGroup)(0, Nothing)
                                   End If
                               End If
                               Application.Current.Dispatcher.Invoke(Sub()
                                                                         'Clone values to UI thread
                                                                         _SearchResult.TopTrackMatch = New Tuple(Of Double, Metadata)(r_TopTrackMatch.Item1, r_TopTrackMatch.Item2?.Clone())
                                                                         _SearchResult.TopGroupMatch = New Tuple(Of Double, MetadataGroup)(r_TopGroupMatch.Item1, r_TopGroupMatch.Item2?.Clone)
                                                                         If _SearchResult.TopGroupMatch.Item2 IsNot Nothing AndAlso _SearchResult.TopGroupMatch.Item2?.Cover Is Nothing Then
                                                                             _SearchResult.TopGroupMatch.Item2.Cover = CommonFunctions.ToCoverImage(String.Join("", _SearchResult.TopGroupMatch.Item2.Name.Skip(2)))
                                                                         End If
                                                                         For Each match In TrackMatches
                                                                             _SearchResult.TrackMatches.Add(match)
                                                                         Next
                                                                         If ArtistMatches.Count > 0 Then
                                                                             For i As Integer = 0 To If(ArtistMatches.Count > SEARCH_LIMIT_ARTIST, SEARCH_LIMIT_ARTIST - 1, ArtistMatches.Count - 1)
                                                                                 _SearchResult.GroupMatches.Add(ArtistMatches(i))
                                                                             Next
                                                                         End If
                                                                         If AlbumMatches.Count > 0 Then
                                                                             For i As Integer = 0 To If(AlbumMatches.Count > SEARCH_LIMIT_ALBUM, SEARCH_LIMIT_ALBUM - 1, AlbumMatches.Count - 1)
                                                                                 _SearchResult.GroupMatches.Add(AlbumMatches(i))
                                                                             Next
                                                                         End If
                                                                         _SearchResult.GenerateRelatedPlaylists()
                                                                         '_SearchResult = New AdvancedSearchResult(r_TopTrackMatch, r_TopGroupMatch, TrackMatches, GroupMatches)
                                                                         OnPropertyChanged(NameOf(SearchResult))
                                                                     End Sub)
                           End Sub)
            IsSearching = False
            Utilities.DebugMode.Instance.Log(Of Library)("Done searching.")
        End Sub

        Public Function SearchSyncEX(Query As String, Optional CaseSensitive As Boolean = True, Optional SearchFilePathOnly As Boolean = False) As AdvancedSearchResult
            If String.IsNullOrEmpty(SearchQuery.Trim) Then Return Nothing
            If IsSearching Then Return Nothing
            Utilities.DebugMode.Instance.Log(Of Library)("Attempting to search sync, Query:=" & SearchQuery & "...")
            IsSearching = True
            Dim sw As New Stopwatch
            sw.Start()

            If NeedsCacheRefresh Then RefreshSearchCacheEX(True)

            Dim LowerQuery = SearchQuery.ToLower
            Dim SplitQuery = SearchQuery.Split(New Char() {" "}, StringSplitOptions.RemoveEmptyEntries)
            Dim QueryPath = IO.Path.GetDirectoryName(LowerQuery)
            Dim fItem = If(SearchFilePathOnly, Nothing, If(String.IsNullOrEmpty(QueryPath), Nothing, Lookup_Path?.Where(Function(k) k.Key.ToLower.Contains(QueryPath))))
            Dim fItemFullPath = Me.Where(Function(k) k.Path.ToLower.Contains(LowerQuery))
            'Compare first letter of each split query to the cached first letter of metadata titles
            'Only matching % of 0.8 and higher are taken
            'Ordered by matching percentage 0->1                                                                                                                        
            Dim fItemTitle = If(SearchFilePathOnly, Nothing, Lookup_Title?.Select(Function(k) New Tuple(Of Single, IGrouping(Of String, Metadata))(SplitQuery.Select(Of Single)(Function(l) l.FirstOrDefault.ToString.GetSimilarity(k.Key)).Sum, k))).Where(Function(k) k.Item1 > SEARCH_TITLE_TOLERANCE).OrderByDescending(Of Single)(Function(k) k.Item1)
            'Filter the matching items from Lookup_Title
            'Only matching % of 0.5/0.8 and higher are taken
            'Ordered by matching percentage 0->1
            Dim FilteredfItemTitle = If(SearchFilePathOnly, Nothing, fItemTitle?.SelectMany(Of Tuple(Of Single, Metadata))(Function(k) k.Item2.Select(Of Tuple(Of Single, Metadata))(Function(l) New Tuple(Of Single, Metadata)(LowerQuery.GetSplitSimilarity(l.Title), l))).Where(Function(k) k.Item1 >= SEARCH_F_TITLE_TOLERANCE AndAlso k.Item2?.Title.Split(" ").Length < SplitQuery.Length + 3).OrderByDescending(Of Single)(Function(k) k.Item1))
            Dim fItemArtist = If(SearchFilePathOnly, Nothing, Lookup_Artist?.Select(Of Tuple(Of Single, IGrouping(Of String, Metadata)))(Function(k) New Tuple(Of Single, IGrouping(Of String, Metadata))(LowerQuery.GetSplitSimilarity(k.Key), k)).Where(Function(k) k.Item1 >= SEARCH_ARTIST_TOLERANCE AndAlso k.Item2.Key?.Split(" ").Length < SplitQuery.Length + 3).OrderByDescending(Of Single)(Function(k) k.Item1))
            Dim fItemAlbum = If(SearchFilePathOnly, Nothing, Lookup_Album?.Select(Of Tuple(Of Single, IGrouping(Of String, Metadata)))(Function(k) New Tuple(Of Single, IGrouping(Of String, Metadata))(LowerQuery.GetSplitSimilarity(k.Key), k)).Where(Function(k) k.Item1 >= SEARCH_ALBUM_TOLERANCE AndAlso k.Item2.Key?.Split(" ").Length < SplitQuery.Length + 3).OrderByDescending(Of Single)(Function(k) k.Item1))

            'Provide equal opportunities, beacause some artist like to have a full article as a song title and thus the similarity function will keep incrementing the last value by that article length and thus will result in wrong results
            'It will impact perforamnce,but hey, quality over quantity right?
            Dim MaxTitleSplitLength, MaxArtistSplitLength, MaxAlbumSplitLength As Integer
            If FilteredfItemTitle IsNot Nothing Then
                MaxTitleSplitLength = FilteredfItemTitle.Select(Of Integer)(Function(k) k.Item2?.Title.Split(" ").Length).OrderByDescending(Of Integer)(Function(l) l).FirstOrDefault
                If MaxTitleSplitLength > 1 Then
                    FilteredfItemTitle = FilteredfItemTitle.Select(Function(k)
                                                                       Dim preV = (MaxTitleSplitLength - k.Item2?.Title.Split(" ").Length) + 1
                                                                       Dim V = preV * k.Item1
                                                                       Return New Tuple(Of Single, Metadata)(V, k.Item2)
                                                                   End Function).OrderByDescending(Of Single)(Function(k) k.Item1)
                End If
            End If
            If fItemArtist IsNot Nothing Then
                MaxArtistSplitLength = fItemArtist.Select(Of Integer)(Function(k) k.Item2?.Key?.Split(" ").Length).OrderByDescending(Of Integer)(Function(l) l).FirstOrDefault
                If MaxArtistSplitLength > 1 Then
                    fItemArtist = fItemArtist.Select(Function(k)
                                                         Dim preV = (MaxArtistSplitLength - k.Item2?.Key?.Split(" ").Length) + 1
                                                         Dim V = preV * k.Item1
                                                         Return New Tuple(Of Single, IGrouping(Of String, Metadata))(V, k.Item2)
                                                     End Function).OrderByDescending(Of Single)(Function(k) k.Item1)
                End If
            End If
            If fItemAlbum IsNot Nothing Then
                MaxAlbumSplitLength = fItemAlbum.Select(Of Integer)(Function(k) k.Item2?.Key?.Split(" ").Length).OrderByDescending(Of Integer)(Function(l) l).FirstOrDefault
                If MaxAlbumSplitLength > 1 Then
                    fItemAlbum = fItemAlbum.Select(Function(k)
                                                       Dim preV = (MaxAlbumSplitLength - k.Item2?.Key?.Split(" ").Length) + 1
                                                       Dim V = preV * k.Item1
                                                       Return New Tuple(Of Single, IGrouping(Of String, Metadata))(V, k.Item2)
                                                   End Function).OrderByDescending(Of Single)(Function(k) k.Item1)
                End If
            End If

            Dim TrackMatches As New ObservableCollection(Of Tuple(Of Double, Metadata))
            Dim ArtistMatches As New ObservableCollection(Of Tuple(Of Double, MetadataGroup))
            Dim AlbumMatches As New ObservableCollection(Of Tuple(Of Double, MetadataGroup))

            If fItem IsNot Nothing Then
                For Each fResult In fItem
                    For Each _fResult In fResult
                        If TrackMatches.FirstOrDefault(Function(k) k.Item2 Is _fResult) IsNot Nothing Then Continue For
                        TrackMatches.Add(New Tuple(Of Double, Metadata)(100, _fResult))
                    Next
                Next
            End If

            If fItemFullPath IsNot Nothing Then
                For Each fResult In fItemFullPath
                    If TrackMatches.FirstOrDefault(Function(k) k.Item2 Is fResult) IsNot Nothing Then Continue For
                    TrackMatches.Add(New Tuple(Of Double, Metadata)(100, fResult))
                Next
            End If

            If FilteredfItemTitle IsNot Nothing Then
                For Each fResult In FilteredfItemTitle
                    If TrackMatches.FirstOrDefault(Function(k) k.Item2 Is fResult.Item2) IsNot Nothing Then Continue For
                    TrackMatches.Add(New Tuple(Of Double, Metadata)(fResult.Item1 * 100, fResult.Item2))
                Next
            End If

            If fItemArtist IsNot Nothing Then
                For Each fResult In fItemArtist
                    Dim MDG As New MetadataGroup() With {.Type = MetadataGroup.GroupType.Artist, .Name = StrConv(fResult.Item2.Key, VbStrConv.ProperCase)}
                    For Each sItem In fResult.Item2
                        MDG.Add(sItem)
                    Next
                    ArtistMatches.Add(New Tuple(Of Double, MetadataGroup)(fResult.Item1 * 100, MDG))
                Next
            End If

            If fItemAlbum IsNot Nothing Then
                For Each fResult In fItemAlbum
                    Dim MDG As New MetadataGroup() With {.Type = MetadataGroup.GroupType.Album, .Name = StrConv(fResult.Item2.Key, VbStrConv.ProperCase)}
                    For Each sItem In fResult.Item2
                        MDG.Add(sItem)
                    Next
                    AlbumMatches.Add(New Tuple(Of Double, MetadataGroup)(fResult.Item1 * 100, MDG))
                Next
            End If


            sw.Stop()
            SearchTookTime = Math.Round(sw.Elapsed.TotalSeconds, 3)

            Dim TopArtistMatch = fItemArtist.FirstOrDefault
            Dim TopAlbumMatch = fItemAlbum.FirstOrDefault
            Dim TopTrackMatch = FilteredfItemTitle.FirstOrDefault

            'Cleanup, will be done automaticaly even if left alone ;)
            fItem = Nothing
            fItemTitle = Nothing
            FilteredfItemTitle = Nothing
            fItemArtist = Nothing
            fItemAlbum = Nothing

            TopTrackMatch?.Item2?.EnsureCovers(FreezeThumbs:=True)
            Dim r_TopTrackMatch As New Tuple(Of Double, Metadata)(If(TopTrackMatch?.Item1 * 100, 0), TopTrackMatch?.Item2)
            Dim r_TopGroupMatch As Tuple(Of Double, MetadataGroup)
            If (TopArtistMatch IsNot Nothing AndAlso TopAlbumMatch IsNot Nothing AndAlso TopArtistMatch.Item1 > TopAlbumMatch.Item1) OrElse TopArtistMatch IsNot Nothing Then
                Dim MDG As New MetadataGroup With {.Type = MetadataGroup.GroupType.Artist, .Name = StrConv(TopArtistMatch.Item2.Key, VbStrConv.ProperCase)}
                For Each meta In TopArtistMatch.Item2
                    MDG.Add(meta)
                Next
                Dim mMDG = MDG.FirstOrDefault(Function(k) k.HasCover)
                Dim Freeze As Boolean = False
                If mMDG.Covers Is Nothing Then Freeze = True
                mMDG?.EnsureCovers(True)
                MDG.Cover = mMDG?.DefaultCover
                If Freeze Then MDG.Cover.Freeze()
                r_TopGroupMatch = New Tuple(Of Double, MetadataGroup)(TopArtistMatch.Item1 * 100, MDG)
            ElseIf TopAlbumMatch IsNot Nothing Then
                Dim MDG As New MetadataGroup With {.Type = MetadataGroup.GroupType.Album, .Name = StrConv(TopAlbumMatch.Item2.Key, VbStrConv.ProperCase)}
                For Each meta In TopAlbumMatch.Item2
                    MDG.Add(meta)
                Next
                Dim mMDG = MDG.FirstOrDefault(Function(k) k.HasCover)
                Dim Freeze As Boolean = False
                If mMDG.Covers Is Nothing Then Freeze = True
                mMDG?.EnsureCovers(True)
                MDG.Cover = mMDG?.DefaultCover
                If Freeze Then MDG.Cover.Freeze()
                r_TopGroupMatch = New Tuple(Of Double, MetadataGroup)(TopAlbumMatch.Item1 * 100, MDG)
            Else
                r_TopGroupMatch = New Tuple(Of Double, MetadataGroup)(0, Nothing)
            End If
            Dim GroupMatches As New ObservableCollection(Of Tuple(Of Double, MetadataGroup))
            For Each match In ArtistMatches
                GroupMatches.Add(match)
            Next
            For Each match In AlbumMatches
                GroupMatches.Add(match)
            Next
            Return New AdvancedSearchResult(r_TopTrackMatch, r_TopGroupMatch, TrackMatches, GroupMatches)
            IsSearching = False
            Utilities.DebugMode.Instance.Log(Of Library)("Done searching.")
        End Function

        Sub Load(data As Specialized.StringCollection)
            Utilities.DebugMode.Instance.Log(Of Library)("Attempting to load data, Count:=" & data?.Count)
            If data IsNot Nothing AndAlso data.Count > 0 Then
                Configuration.SetStatus("Loading Data...", 0)
                Dim BinF As New System.Runtime.Serialization.Formatters.Binary.BinaryFormatter()
                Dim ProgStep = If(data.Count = 0, 100, 100 / data.Count)
                Dim i = 1
                For Each bin In data
                    Dim Metadata = TryCast(BinF.Deserialize(New IO.MemoryStream(Convert.FromBase64String(bin))), Metadata)
                    'If Metadata Is Nothing OrElse Metadata.Location = Metadata.FileLocation.Internal Then Continue For
                    Add(Metadata, False, False)
                    Configuration.SetStatus($"{i}/{data.Count}", Configuration.Progress + ProgStep)
                    i += 1
                Next
                'MostPlayed = Me.OrderByDescending(Of Integer)(Function(k) k.PlayCount).FirstOrDefault
                EnsureMostPlayed(SkipArtists:=False)
            Else
                Configuration.SetStatus("No data to be loaded", 100)
            End If
            Configuration.SetStatus("Search Indexing...", 100)

            Searcher.Init()
            If Searcher.RequiresPopulate Then Searcher.Populate()
            Searcher.Load()
            _IndexedCount = Searcher.Count
            OnPropertyChanged(NameOf(IndexedCount))

            Utilities.DebugMode.Instance.Log(Of Library)("Done loading data.")
            Configuration.SetStatus($"Done indexing, loaded {Count} song", 100)
        End Sub

        Sub Load(data As IEnumerable(Of String))
            Utilities.DebugMode.Instance.Log(Of Library)("Attempting to load data from IEnumerable, Count:=?")
            If data IsNot Nothing Then
                Configuration.SetStatus("Loading Data...", 0)
                Dim BinF As New System.Runtime.Serialization.Formatters.Binary.BinaryFormatter()
                Dim i = 1
                For Each bin In data
                    Dim Metadata = TryCast(BinF.Deserialize(New IO.MemoryStream(Convert.FromBase64String(bin))), Metadata)
                    'If Metadata Is Nothing OrElse Metadata.Location = Metadata.FileLocation.Internal Then Continue For
                    Add(Metadata, False, False)
                    Configuration.SetStatus($"Loading...{i}", 50)
                    i += 1
                Next
                'MostPlayed = Me.OrderByDescending(Of Integer)(Function(k) k.PlayCount).FirstOrDefault
                EnsureMostPlayed(SkipArtists:=False)
                If Me.HasItems Then Configuration.SetStatus("No data to be loaded", 100)
            Else
                Configuration.SetStatus("No data to be loaded", 100)
            End If
            Configuration.SetStatus("Search Indexing...", 100)

            Searcher.Init()
            If Searcher.RequiresPopulate Then Searcher.Populate()
            Searcher.Load()
            _IndexedCount = Searcher.Count
            OnPropertyChanged(NameOf(IndexedCount))

            Utilities.DebugMode.Instance.Log(Of Library)("Done loading data.")
            Configuration.SetStatus($"Done indexing, loaded {Count} song", 100)
        End Sub

        Iterator Function Save() As IEnumerable(Of String)
            Utilities.DebugMode.Instance.Log(Of Library)("Attempting to save data")
            Dim BinF As New System.Runtime.Serialization.Formatters.Binary.BinaryFormatter
            For Each metadata In Me
                Dim mem As New IO.MemoryStream
                BinF.Serialize(mem, metadata)
                Yield Convert.ToBase64String(mem.ToArray)
            Next
            Utilities.DebugMode.Instance.Log(Of Library)("Done saving data.")
        End Function

        Async Sub LoadFrom(fs As IO.FileStream)
            Utilities.DebugMode.Instance.Log(Of Library)("Attempting to load data from IEnumerable, Count:=?")
            If fs IsNot Nothing Then
                Configuration.SetStatus("Loading Data...", 0)
                Dim BinF As New System.Runtime.Serialization.Formatters.Binary.BinaryFormatter()
                Dim i = 1
                Using sw As New IO.StreamReader(fs)
                    Do While Not sw.EndOfStream
                        Dim bin = Await sw.ReadLineAsync
                        Dim Metadata = TryCast(BinF.Deserialize(New IO.MemoryStream(Convert.FromBase64String(bin))), Metadata)
                        'If Metadata Is Nothing OrElse Metadata.Location = Metadata.FileLocation.Internal Then Continue For
                        Add(Metadata, False, False)
                        Configuration.SetStatus($"Loading...{i}", 50)
                        i += 1
                    Loop
                End Using
                'MostPlayed = Me.OrderByDescending(Of Integer)(Function(k) k.PlayCount).FirstOrDefault
                EnsureMostPlayed(SkipArtists:=False)
                If Me.HasItems Then Configuration.SetStatus("No data to be loaded", 100)
            Else
                Configuration.SetStatus("No data to be loaded", 100)
            End If
            Configuration.SetStatus("Search Indexing...", 100)

            Searcher.Init()
            If Searcher.RequiresPopulate Then Searcher.Populate()
            Searcher.Load()
            _IndexedCount = Searcher.Count
            OnPropertyChanged(NameOf(IndexedCount))

            Utilities.DebugMode.Instance.Log(Of Library)("Done loading data.")
            Configuration.SetStatus($"Done indexing, loaded {Count} song", 100)
        End Sub

        ''' <summary>
        ''' Writes instance data and closes the stream.
        ''' </summary>
        ''' <param name="fs">Target file stream</param>
        ''' <returns></returns>
        Async Function SaveTo(fs As IO.FileStream) As Task
            Utilities.DebugMode.Instance.Log(Of Library)("Attempting to save data to IO.FileStream")
            Dim BinF As New System.Runtime.Serialization.Formatters.Binary.BinaryFormatter
            Using sw As New IO.StreamWriter(fs)
                For Each metadata In Me
                    Dim mem As New IO.MemoryStream
                    BinF.Serialize(mem, metadata)
                    Await sw.WriteLineAsync(Convert.ToBase64String(mem.ToArray))
                Next
            End Using
            Utilities.DebugMode.Instance.Log(Of Library)("Done saving data to IO.FileStream.")
        End Function

        Private Sub RefreshSearchCacheEX(Optional PopulateEX As Boolean = False)
            If PopulateEX Then
                Dim _State = Configuration.Status

                Configuration.SetStatus("Caching...", 0)
                Lookup_Path = Me.ToLookup(Of String)(Function(k) IO.Path.GetDirectoryName(k.Path).ToLower)
                Configuration.SetStatus("Caching...", 20)
                Lookup_Title = Me.ToLookup(Of String)(Function(k) k.Title?.ToLower.FirstOrDefault)
                Configuration.SetStatus("Caching...", 40)
                Lookup_Artist = Me.ToLookup(Of String)(Function(k) k.DefaultArtist?.ToLower)
                Configuration.SetStatus("Caching...", 60)
                Lookup_Album = Me.ToLookup(Of String)(Function(k) k.Album?.ToLower)
                Configuration.SetStatus("Caching...", 80)
                Lookup_Year = Me.ToLookup(Of Integer)(Function(k) k.Year)
                Configuration.SetStatus(_State, 100)

                NeedsCacheRefresh = False
            Else
                Dim _State = Configuration.Status
                Configuration.SetStatus("Caching...", 50)
                Searcher.Init()
                Searcher.Populate()
                Searcher.Load()
                _IndexedCount = Searcher.Count
                OnPropertyChanged(NameOf(IndexedCount))
                Configuration.SetStatus(_State, 100)
                NeedsCacheRefresh = False
            End If
        End Sub

        Public Sub RebuildSearchCache()
            Dim _State = Configuration.Status
            Configuration.SetStatus("Caching...", 50)
            Searcher.Dispose()
            Dim _path = IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "QuickBeat", GetEncodedSpecialAppName, "global_index")
            IO.Directory.Delete(_path, True)
            Searcher = New Search.Searcher(_path, Me)
            Try
                Searcher.Init(True)
                Searcher.Populate()
                Searcher.Load()
            Catch ex As Exception
                Configuration.SetStatus(_State, 100)
                Throw ex
            End Try
            _IndexedCount = Searcher.Count
            OnPropertyChanged(NameOf(IndexedCount))
            Configuration.SetStatus(_State, 100)
            NeedsCacheRefresh = False
        End Sub

        Private Sub ClearSearchCacheEX()
            Lookup_Path = Nothing
            Lookup_Title = Nothing
            Lookup_Artist = Nothing
            Lookup_Album = Nothing

            NeedsCacheRefresh = True
        End Sub

        Function IncreasePlaycount(path As String, increment As Integer) As Boolean
            Utilities.DebugMode.Instance.Log(Of Library)("Attempting to increase playcount, Path:=" & path & ", Increment:=" & increment)
            Dim meta = Me.FirstOrDefault(Function(k) k.Path = path)
            If meta Is Nothing Then
                Utilities.DebugMode.Instance.Log(Of Library)("Couldn't find metadata.")
                Return False
            Else
                meta.PlayCount += increment
                _MostPlayedRefreshCounter += 1
                If _MostPlayedRefreshCounter >= 5 Then
                    EnsureMostPlayed()
                    _MostPlayedRefreshCounter = 0
                Else
                    If meta.PlayCount > MostPlayed?.PlayCount Then
                        MostPlayed = meta
                    End If
                End If
                Utilities.DebugMode.Instance.Log(Of Library)("Done increasing playcount.")
                Return False
            End If
        End Function

        Async Function IncreasePlaycountAsync(path As String, increment As Integer) As Task(Of Boolean)
            Return Await Task.Run(Function()
                                      Utilities.DebugMode.Instance.Log(Of Library)("Attempting to increase playcount, Path:=" & path & ", Increment:=" & increment)
                                      Dim meta = Me.FirstOrDefault(Function(k) k.Path = path)
                                      If meta Is Nothing Then
                                          Utilities.DebugMode.Instance.Log(Of Library)("Couldn't find metadata.")
                                          Return False
                                      Else
                                          meta.PlayCount += increment
                                          'Can be replaced with EnsureMostPlayed if not used in UI thread
                                          _MostPlayedRefreshCounter += 1
                                          If _MostPlayedRefreshCounter >= 5 Then
                                              Application.Current.Dispatcher.Invoke(Sub()
                                                                                        EnsureMostPlayed()
                                                                                    End Sub)
                                              _MostPlayedRefreshCounter = 0
                                          Else
                                              If meta.PlayCount > MostPlayed?.PlayCount Then
                                                  Application.Current.Dispatcher.Invoke(Sub()
                                                                                            MostPlayed = meta
                                                                                        End Sub)
                                              End If
                                          End If
                                          Utilities.DebugMode.Instance.Log(Of Library)("Done increasing playcount.")
                                          Return False
                                      End If
                                  End Function)
        End Function

        Sub FocusArtistGroup(artist As String)
            If FocusedGroup IsNot Nothing Then
                FocusedGroup.IsInUse = False
                FocusedGroup.FreeCovers()
                For Each meta In FocusedGroup
                    If Not meta.IsInUse Then meta.FreeCovers()
                Next
            End If

            If Not String.IsNullOrEmpty(artist) Then
                Dim metadatas = Me.Where(Function(k) If(k.Artists Is Nothing, False, k.Artists.Any(Function(l) l.ToLower = artist.ToLower)))
                If metadatas.Count = 0 Then Return
                Dim artistgroup As New MetadataGroup() With {.Type = MetadataGroup.GroupType.Artist, .Name = artist, .Category = "Artist"}
                For Each metadata In metadatas
                    artistgroup.Add(metadata)
                Next
                artistgroup.UpdateValues()

                artistgroup.IsInUse = True
                FocusedGroup = artistgroup

                RaiseEvent ArtistFocused(FocusedGroup)
            End If
        End Sub

        Sub FocusAlbumGroup(album As String)
            If FocusedGroup IsNot Nothing Then
                FocusedGroup.IsInUse = False
                FocusedGroup.FreeCovers()
                For Each meta In FocusedGroup
                    If Not meta.IsInUse Then meta.FreeCovers()
                Next
            End If

            If Not String.IsNullOrEmpty(album) Then
                Dim metadatas = Me.Where(Function(k) k.Album?.ToLower = album.ToLower)
                If metadatas.Count = 0 Then Return
                Dim albumgroup As New MetadataGroup() With {.Type = MetadataGroup.GroupType.Album, .Name = album, .Category = "Album"}
                For Each metadata In metadatas
                    albumgroup.Add(metadata)
                Next
                albumgroup.UpdateValues()

                albumgroup.IsInUse = True
                FocusedGroup = albumgroup

                RaiseEvent AlbumFocused(FocusedGroup)
            End If
        End Sub

        Function GetArtistGroup(artist As String) As MetadataGroup
            If Not String.IsNullOrEmpty(artist) Then
                Dim metadatas = Me.Where(Function(k) If(k.Artists Is Nothing, False, k.Artists.Any(Function(l) l.ToLower = artist.ToLower)))
                If metadatas.Count = 0 Then Return Nothing
                Dim artistgroup As New MetadataGroup() With {.Type = MetadataGroup.GroupType.Artist, .Name = artist, .Category = "Artist"}
                For Each metadata In metadatas
                    artistgroup.Add(metadata)
                Next
                artistgroup.UpdateValues()

                Return artistgroup
            End If
            Return Nothing
        End Function

        Function GetAlbumGroup(album As String) As MetadataGroup
            If Not String.IsNullOrEmpty(album) Then
                Dim metadatas = Me.Where(Function(k) k.Album?.ToLower = album.ToLower)
                If metadatas.Count = 0 Then Return Nothing
                Dim albumgroup As New MetadataGroup() With {.Type = MetadataGroup.GroupType.Album, .Name = album, .Category = "Album"}
                For Each metadata In metadatas
                    albumgroup.Add(metadata)
                Next
                albumgroup.UpdateValues()

                Return albumgroup
            End If
            Return Nothing
        End Function

        Sub FocusGroup(playlist As Playlist)
            If playlist Is Nothing Then Return
            If FocusedGroup IsNot Nothing Then
                FocusedGroup.IsInUse = False
                FocusedGroup.FreeCovers()
                For Each meta In FocusedGroup
                    If Not meta.IsInUse Then meta.FreeCovers()
                Next
            End If

            Dim group As New MetadataGroup() With {.Name = playlist.Name, .Category = playlist.Category}
            For Each metadata In playlist
                group.Add(metadata)
            Next

            group.UpdateValues()

            group.IsInUse = True
            FocusedGroup = group

            RaiseEvent GroupFocused(group)
        End Sub

        Sub FocusGroup(group As MetadataGroup)
            If group Is Nothing Then Return
            If FocusedGroup IsNot Nothing Then
                FocusedGroup.IsInUse = False
                FocusedGroup.FreeCovers()
                For Each meta In FocusedGroup
                    If Not meta.IsInUse Then meta.FreeCovers()
                Next
            End If

            group.UpdateValues()

            group.IsInUse = True
            FocusedGroup = group

            RaiseEvent GroupFocused(group)
        End Sub

        Public Shadows Sub OnPropertyChanged(<CallerMemberName> Optional CallerName As String = Nothing)
            MyBase.OnPropertyChanged(New PropertyChangedEventArgs(CallerName))
        End Sub

        Public Sub Init() Implements IStartupItem.Init
            Configuration.SetStatus("", 0)
            Utilities.SharedProperties.Instance?.ItemsConfiguration.Add(Configuration)
        End Sub

        Public Shadows Sub Add(item As Metadata, Optional UpdateMostPlayed As Boolean = True, Optional NotifySearchCache As Boolean = True)
            If item Is Nothing Then Return
            MyBase.Add(item)
            'NeedsCacheRefresh = True
            If NotifySearchCache Then
                Searcher?.Add(item)
                _IndexedCount += 1
                OnPropertyChanged(NameOf(IndexedCount))
            End If
            OnPropertyChanged(NameOf(Count))
            OnPropertyChanged(NameOf(HasItems))
            If UpdateMostPlayed Then
                EnsureMostPlayed(item)
            End If
        End Sub

        Public Shadows Function Remove(item As Metadata, Optional NotifySearchCache As Boolean = True) As Boolean
            Dim r = MyBase.Remove(item)
            If r Then
                'NeedsCacheRefresh = True
                If NotifySearchCache Then
                    Searcher?.Delete(item)
                    _IndexedCount -= 1
                    OnPropertyChanged(NameOf(IndexedCount))
                End If
                OnPropertyChanged(NameOf(Count))
                OnPropertyChanged(NameOf(HasItems))
            End If
            Return r
        End Function

        Public Shadows Sub RemoveAt(index As Integer, Optional NotifySearchCache As Boolean = True)
            Dim tMeta = Me(index)
            MyBase.RemoveAt(index)
            'NeedsCacheRefresh = True
            If NotifySearchCache Then
                Searcher?.Delete(tMeta)
                _IndexedCount -= 1
                OnPropertyChanged(NameOf(IndexedCount))
            End If
            tMeta = Nothing
            OnPropertyChanged(NameOf(Count))
            OnPropertyChanged(NameOf(HasItems))
        End Sub

        Public Shadows Sub Clear(Optional NotifySearchCache As Boolean = True)
            MyBase.Clear()
            ClearSearchCacheEX()
            If NotifySearchCache Then
                Searcher?.Init(True)
                Searcher?.Load()
                _IndexedCount = 0
                OnPropertyChanged(NameOf(IndexedCount))
            End If
            Dim meta As New Player.InternalMetadata("Resources/Neffex - Fight Back.mp3") : meta.RefreshTagsFromFile(True)
            Add(meta, NotifySearchCache:=False)
            OnPropertyChanged(NameOf(Count))
            OnPropertyChanged(NameOf(HasItems))
        End Sub

        ''' <summary>
        ''' Validates <see cref="MostPlayed"/> ,<see cref="MostPlayedArtist"/> and <see cref="MostPlayedArtist2"/>
        ''' </summary>
        ''' <remarks>
        ''' Only call when necessary!</remarks>
        ''' <param name="Item"></param>
        Public Sub EnsureMostPlayed(Optional Item As Metadata = Nothing, Optional SkipArtists As Boolean = True)
            If Not Me.HasItems Then Return

            If MostPlayed Is Nothing Or (MostPlayed IsNot Nothing AndAlso Item Is Nothing) Then
                MostPlayed = Me.OrderByDescending(Of Integer)(Function(k) k?.PlayCount).FirstOrDefault
            Else
                If Item?.PlayCount > MostPlayed.PlayCount Then
                    MostPlayed = Item
                End If
            End If

            If SkipArtists Then Return

            Dim Groups As IOrderedEnumerable(Of IGrouping(Of String, Metadata)) = Nothing

            If MostPlayedArtist Is Nothing Then
                'Must be calculated only when necessary
                Groups = Me.GroupBy(Function(k) k.DefaultArtist).OrderByDescending(Function(l) l.Sum(Function(m) m.PlayCount))
                If Groups.Count < 1 Then Return
                Dim mpa = New MetadataGroup() With {.Type = MetadataGroup.GroupType.Artist, .Name = Groups.FirstOrDefault?.Key, .Category = "Artist"}
                For Each Item In Groups.FirstOrDefault
                    mpa.Add(Item)
                Next
                mpa.IsInUse = True
                Dim hascovermeta = mpa.FirstOrDefault(Function(k) k.HasCover)
                If hascovermeta IsNot Nothing Then
                    hascovermeta.EnsureCovers()
                    If hascovermeta.IsCoverAvailable Then mpa.Cover = New TransformedBitmap(hascovermeta.DefaultCover, New ScaleTransform(150 / hascovermeta.DefaultCover.Width, 150 / hascovermeta.DefaultCover.Height))
                    If Not hascovermeta.IsInUse Then hascovermeta.FreeCovers()
                End If
                MostPlayedArtist = mpa
                Groups = Nothing
            End If

            If MostPlayedArtist2 Is Nothing Then
                'Must be calculated only when necessary
                If Groups Is Nothing Then Groups = Me.GroupBy(Function(k) k.DefaultArtist).OrderByDescending(Function(l) l.Sum(Function(m) m.PlayCount))
                If Groups.Count < 2 Then Return
                Dim mpa = New MetadataGroup() With {.Type = MetadataGroup.GroupType.Artist, .Name = Groups.Skip(1).FirstOrDefault?.Key, .Category = "Artist"}
                For Each Item In Groups.Skip(1).FirstOrDefault
                    mpa.Add(Item)
                Next
                mpa.IsInUse = True
                Dim hascovermeta = mpa.FirstOrDefault(Function(k) k.HasCover)
                If hascovermeta IsNot Nothing Then
                    hascovermeta.EnsureCovers()
                    If hascovermeta.IsCoverAvailable Then mpa.Cover = New TransformedBitmap(hascovermeta.DefaultCover, New ScaleTransform(150 / hascovermeta.DefaultCover.Width, 150 / hascovermeta.DefaultCover.Height))
                    If Not hascovermeta.IsInUse Then hascovermeta.FreeCovers()
                End If
                MostPlayedArtist2 = mpa
                Groups = Nothing
            End If

            If MostPlayedArtist3 Is Nothing Then
                'Must be calculated only when necessary
                If Groups Is Nothing Then Groups = Me.GroupBy(Function(k) k.DefaultArtist).OrderByDescending(Function(l) l.Sum(Function(m) m.PlayCount))
                If Groups.Count < 3 Then Return
                Dim mpa = New MetadataGroup() With {.Type = MetadataGroup.GroupType.Artist, .Name = Groups.Skip(2).FirstOrDefault?.Key, .Category = "Artist"}
                For Each Item In Groups.Skip(2).FirstOrDefault
                    mpa.Add(Item)
                Next
                mpa.IsInUse = True
                Dim hascovermeta = mpa.FirstOrDefault(Function(k) k.HasCover)
                If hascovermeta IsNot Nothing Then
                    hascovermeta.EnsureCovers()
                    If hascovermeta.IsCoverAvailable Then mpa.Cover = New TransformedBitmap(hascovermeta.DefaultCover, New ScaleTransform(150 / hascovermeta.DefaultCover.Width, 150 / hascovermeta.DefaultCover.Height))
                    If Not hascovermeta.IsInUse Then hascovermeta.FreeCovers()
                End If
                MostPlayedArtist3 = mpa
                Groups = Nothing
            End If

            OnPropertyChanged(NameOf(RandomGroup))
        End Sub
#End Region
#Region "Classes"
        Public Class AdvancedSearchResult
            Public Property TopTrackMatch As Tuple(Of Double, Metadata)
            Public Property TopGroupMatch As Tuple(Of Double, MetadataGroup)
            Public Property TrackMatches As ObservableCollection(Of Tuple(Of Double, Metadata))
            Public Property GroupMatches As ObservableCollection(Of Tuple(Of Double, MetadataGroup))
            Public Property TopMatchRelatedPlaylists As New ObservableCollection(Of MetadataGroup)

            Sub New()
                TopTrackMatch = New Tuple(Of Double, Metadata)(0, Nothing)
                TopGroupMatch = New Tuple(Of Double, MetadataGroup)(0, Nothing)
                TrackMatches = New ObservableCollection(Of Tuple(Of Double, Metadata))
                GroupMatches = New ObservableCollection(Of Tuple(Of Double, MetadataGroup))
            End Sub

            Sub New(TopTrackMatch As Tuple(Of Double, Metadata), TopGroupMatch As Tuple(Of Double, MetadataGroup), TrackMatches As ObservableCollection(Of Tuple(Of Double, Metadata)), GroupMatches As ObservableCollection(Of Tuple(Of Double, MetadataGroup)), Optional CreatePlaylists As Boolean = True)
                Me.TopTrackMatch = TopTrackMatch
                Me.TopGroupMatch = TopGroupMatch
                Me.TrackMatches = TrackMatches
                Me.GroupMatches = GroupMatches
                If CreatePlaylists Then
                    If TopTrackMatch IsNot Nothing AndAlso TopTrackMatch.Item2 IsNot Nothing Then
                        For Each playlist In SharedProperties.Instance.CustomPlaylists.Where(Function(k) k.FirstOrDefault(Function(l) l.Path = TopTrackMatch.Item2?.Path) IsNot Nothing)
                            Dim MDG As New MetadataGroup With {.Type = MetadataGroup.GroupType.Playlist, .Name = playlist.Name, .Tag = playlist}
                            For Each meta In playlist
                                MDG.Add(meta)
                            Next
                            If playlist.Cover Is Nothing Then
                                Dim mMDG = MDG.FirstOrDefault(Function(k) k.HasCover)
                                mMDG?.EnsureCovers(True)
                                MDG.Cover = mMDG?.DefaultCover
                            Else
                                MDG.Cover = playlist.Cover
                                MDG.IsCoverLocked = True
                            End If
                            TopMatchRelatedPlaylists.Add(MDG)
                        Next
                    End If
                    If TopGroupMatch IsNot Nothing AndAlso TopGroupMatch.Item2 IsNot Nothing Then
                        For Each playlist In SharedProperties.Instance.CustomPlaylists.Where(Function(k) k.FirstOrDefault(Function(l) l.DefaultArtist = TopGroupMatch.Item2?.Name) IsNot Nothing)
                            If TopMatchRelatedPlaylists.FirstOrDefault(Function(k) k.Tag Is playlist) IsNot Nothing Then Continue For
                            Dim MDG As New MetadataGroup With {.Type = MetadataGroup.GroupType.Playlist, .Name = playlist.Name}
                            For Each meta In playlist
                                MDG.Add(meta)
                            Next
                            If playlist.Cover Is Nothing Then
                                Dim mMDG = MDG.FirstOrDefault(Function(k) k.HasCover)
                                mMDG?.EnsureCovers(True)
                                MDG.Cover = mMDG?.DefaultCover
                            Else
                                MDG.Cover = playlist.Cover
                                MDG.IsCoverLocked = True
                            End If
                            TopMatchRelatedPlaylists.Add(MDG)
                        Next
                    End If
                    'Reference cleanup
                    For Each group In TopMatchRelatedPlaylists
                        group.Tag = Nothing
                    Next
                End If
            End Sub

            ''' <summary>
            ''' Same function as the constructor, used when initializing with the parameter-less constructor
            ''' </summary>
            Sub GenerateRelatedPlaylists()
                If TopTrackMatch IsNot Nothing AndAlso TopTrackMatch.Item2 IsNot Nothing Then
                    If TopMatchRelatedPlaylists Is Nothing Then TopMatchRelatedPlaylists = New ObservableCollection(Of MetadataGroup)
                    For Each playlist In SharedProperties.Instance.CustomPlaylists.Concat(SharedProperties.Instance.RecommendedPlaylists).Where(Function(k) k.FirstOrDefault(Function(l) l.Path = TopTrackMatch.Item2?.Path) IsNot Nothing)
                        Dim MDG As New MetadataGroup With {.Type = MetadataGroup.GroupType.Playlist, .Name = playlist.Name, .Tag = playlist}
                        For Each meta In playlist
                            MDG.Add(meta)
                        Next
                        If playlist.Cover Is Nothing Then
                            Dim mMDG = MDG.FirstOrDefault(Function(k) k.HasCover)
                            mMDG?.EnsureCovers(True)
                            MDG.Cover = mMDG?.DefaultCover
                        Else
                            MDG.Cover = playlist.Cover
                            MDG.IsCoverLocked = True
                        End If
                        TopMatchRelatedPlaylists.Add(MDG)
                    Next
                End If
                If TopGroupMatch IsNot Nothing AndAlso TopGroupMatch.Item2 IsNot Nothing Then
                    If TopMatchRelatedPlaylists Is Nothing Then TopMatchRelatedPlaylists = New ObservableCollection(Of MetadataGroup)
                    For Each playlist In SharedProperties.Instance.CustomPlaylists.Concat(SharedProperties.Instance.RecommendedPlaylists).Where(Function(k) k.FirstOrDefault(Function(l) l.DefaultArtist = TopGroupMatch.Item2?.Name OrElse l.Album = TopGroupMatch.Item2?.Name) IsNot Nothing)
                        If TopMatchRelatedPlaylists.FirstOrDefault(Function(k) k.Tag Is playlist) IsNot Nothing Then Continue For
                        Dim MDG As New MetadataGroup With {.Type = MetadataGroup.GroupType.Playlist, .Name = playlist.Name}
                        For Each meta In playlist
                            MDG.Add(meta)
                        Next
                        If playlist.Cover Is Nothing Then
                            Dim mMDG = MDG.FirstOrDefault(Function(k) k.HasCover)
                            mMDG?.EnsureCovers(True)
                            MDG.Cover = mMDG?.DefaultCover
                        Else
                            MDG.Cover = playlist.Cover
                            MDG.IsCoverLocked = True
                        End If
                        TopMatchRelatedPlaylists.Add(MDG)
                    Next
                End If
                'Reference cleanup
                If TopMatchRelatedPlaylists Is Nothing Then Return
                For Each group In TopMatchRelatedPlaylists
                    group.Tag = Nothing
                Next
            End Sub

            Public Enum SearchMatchType
                Track
                Group
            End Enum
        End Class
#End Region
    End Class
End Namespace