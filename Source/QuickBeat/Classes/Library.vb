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
        ''' <summary>
        ''' Occures when a <see cref="Player.Metadata.IsFavorite"/> changes value
        ''' </summary>
        ''' <param name="sender"></param>
        Public Event FavoriteChanged(sender As Player.Metadata)

#Region "Raisers"
        Public Sub OnFavoriteChanged(sender As Player.Metadata)
            RaiseEvent FavoriteChanged(sender)
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

        Private _SearchResult As New ObjectModel.ObservableCollection(Of Metadata)
        ReadOnly Property SearchResult As ObjectModel.ObservableCollection(Of Metadata)
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
                Utilities.SharedProperties.Instance.Player?.LoadSong(SearchResult?(value))
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
                    _MostPlayed.IsInUse = False
                    If Not _MostPlayed.IsInUse Then _MostPlayed.FreeCovers()
                End If
                _MostPlayed = value
                If _MostPlayed IsNot Nothing Then
                    _MostPlayed.EnsureCovers()
                    _MostPlayed.IsInUse = True
                End If
                OnPropertyChanged()
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
                        If SharedProperties.Instance.IsLogging Then Utilities.DebugMode.Instance.Log(Of Library)("Hit timeout on random group.")
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

        'Used for search engine indexing, provides faster search operations        
        Private Lookup_Path As ILookup(Of String, Metadata)
        Private Lookup_Title As ILookup(Of String, Metadata)
        Private Lookup_Artist As ILookup(Of String, Metadata)
        Private Lookup_Album As ILookup(Of String, Metadata)

#End Region
#Region "Methods"
        Async Sub Search(Optional SearchFilePathOnly As Boolean = False)
            If String.IsNullOrEmpty(SearchQuery) Then Return
            If IsSearching Then Return
            If SharedProperties.Instance.IsLogging Then Utilities.DebugMode.Instance.Log(Of Library)("Attempting to search, Query:=" & SearchQuery & "...")
            IsSearching = True
            SearchResult.Clear()
            Await Task.Run(Sub()
                               Dim sw As New Stopwatch
                               sw.Start()

                               If NeedsCacheRefresh Then RefreshSearchCache()

                               Dim LowerQuery = SearchQuery.ToLower
                               Dim QueryPath = IO.Path.GetDirectoryName(LowerQuery)
                               Dim fItem = If(SearchFilePathOnly, Nothing, If(String.IsNullOrEmpty(QueryPath), Nothing, Lookup_Path?.Where(Function(k) k.Key.ToLower.Contains(QueryPath))))
                               Dim fItemFullPath = Me.Where(Function(k) k.Path.ToLower.Contains(LowerQuery))
                               Dim fItemTitle = If(SearchFilePathOnly, Nothing, Lookup_Title?.FirstOrDefault(Function(k) If(String.IsNullOrEmpty(k.Key), "", k.Key) = LowerQuery.FirstOrDefault))
                               Dim FilteredfItemTitle = If(SearchFilePathOnly, Nothing, fItemTitle?.Where(Function(k) k.Title.ToLower.Contains(LowerQuery)))
                               Dim fItemArtist = If(SearchFilePathOnly, Nothing, Lookup_Artist?.Where(Function(k) If(k.Key, "").Contains(LowerQuery)))
                               Dim fItemAlbum = If(SearchFilePathOnly, Nothing, Lookup_Album?.Where(Function(k) If(k.Key, "").Contains(LowerQuery)))

                               If fItem IsNot Nothing Then
                                   For Each fResult In fItem
                                       For Each _fResult In fResult
                                           Application.Current.Dispatcher.Invoke(Sub()
                                                                                     If SearchResult.Contains(_fResult) Then Return
                                                                                     SearchResult.Add(_fResult)
                                                                                 End Sub)
                                       Next
                                   Next
                               End If

                               If fItemFullPath IsNot Nothing Then
                                   For Each fResult In fItemFullPath
                                       Application.Current.Dispatcher.Invoke(Sub()
                                                                                 If SearchResult.Contains(fResult) Then Return
                                                                                 SearchResult.Add(fResult)
                                                                             End Sub)
                                   Next
                               End If

                               If FilteredfItemTitle IsNot Nothing Then
                                   For Each fResult In FilteredfItemTitle
                                       Application.Current.Dispatcher.Invoke(Sub()
                                                                                 If SearchResult.Contains(fResult) Then Return
                                                                                 SearchResult.Add(fResult)
                                                                             End Sub)
                                   Next
                               End If

                               If fItemArtist IsNot Nothing Then
                                   For Each fResult In fItemArtist
                                       For Each _fResult In fResult
                                           Application.Current.Dispatcher.Invoke(Sub()
                                                                                     If SearchResult.Contains(_fResult) Then Return
                                                                                     SearchResult.Add(_fResult)
                                                                                 End Sub)
                                       Next
                                   Next
                               End If

                               If fItemAlbum IsNot Nothing Then
                                   For Each fResult In fItemAlbum
                                       For Each _fResult In fResult
                                           Application.Current.Dispatcher.Invoke(Sub()
                                                                                     If SearchResult.Contains(_fResult) Then Return
                                                                                     SearchResult.Add(_fResult)
                                                                                 End Sub)
                                       Next
                                   Next
                               End If

                               sw.Stop()
                               SearchTookTime = Math.Round(sw.Elapsed.TotalSeconds, 3)

                               'Cleanup, will be done automaticaly even if left alone ;)
                               fItem = Nothing
                               fItemTitle = Nothing
                               FilteredfItemTitle = Nothing
                               fItemArtist = Nothing
                               fItemAlbum = Nothing

                               'Return
                               'For Each metadata In Me.Where(Function(k) k.HasValue(SearchQuery))
                               '    Application.Current.Dispatcher.Invoke(Sub()
                               '                                              SearchResult.Add(metadata)
                               '                                          End Sub)
                               'Next
                               'Old Code
                           End Sub)
            IsSearching = False
            If SharedProperties.Instance.IsLogging Then Utilities.DebugMode.Instance.Log(Of Library)("Done searching.")
        End Sub

        Public Function SearchSync(Query As String, Optional CaseSensitive As Boolean = True, Optional SearchFilePathOnly As Boolean = False) As List(Of Metadata)
            If String.IsNullOrEmpty(Query) Then Return New List(Of Metadata)
            Dim SearchResult As New List(Of Metadata) 'Not to be confused with Me.SearchResult
            If SharedProperties.Instance.IsLogging Then Utilities.DebugMode.Instance.Log(Of Library)("Attempting to search sync, Query:=" & Query & "...")
            Dim sw As New Stopwatch
            sw.Start()

            If NeedsCacheRefresh Then RefreshSearchCache()

            Dim LowerQuery = If(CaseSensitive, Query, Query.ToLower)
            Dim QueryPath = IO.Path.GetDirectoryName(LowerQuery)
            Dim fItem = If(SearchFilePathOnly, Nothing, If(String.IsNullOrEmpty(QueryPath), Nothing, Lookup_Path?.Where(Function(k) If(CaseSensitive, k.Key, k.Key.ToLower).Contains(QueryPath))))
            Dim fItemFullPath = Me.Where(Function(k) If(CaseSensitive, k.Path, k.Path?.ToLower).Contains(LowerQuery))
            Dim fItemTitle = If(SearchFilePathOnly, Nothing, Lookup_Title?.FirstOrDefault(Function(k) If(String.IsNullOrEmpty(k.Key), "", If(CaseSensitive, k.Key, k.Key.ToLower)) = LowerQuery.FirstOrDefault))
            Dim FilteredfItemTitle = If(SearchFilePathOnly, Nothing, fItemTitle?.Where(Function(k) If(CaseSensitive, k.Title, k.Title.ToLower).Contains(LowerQuery)))
            Dim fItemArtist = If(SearchFilePathOnly, Nothing, Lookup_Artist?.Where(Function(k) If(If(CaseSensitive, k.Key, k.Key?.ToLower), "").Contains(LowerQuery)))
            Dim fItemAlbum = If(SearchFilePathOnly, Nothing, Lookup_Album?.Where(Function(k) If(If(CaseSensitive, k.Key, k.Key?.ToLower), "").Contains(LowerQuery)))

            If fItem IsNot Nothing Then
                For Each fResult In fItem
                    For Each _fResult In fResult
                        If SearchResult.Contains(_fResult) Then Continue For
                        SearchResult.Add(_fResult)
                    Next
                Next
            End If

            If fItemFullPath IsNot Nothing Then
                For Each fResult In fItemFullPath
                    If SearchResult.Contains(fResult) Then Continue For
                    SearchResult.Add(fResult)
                Next
            End If

            If FilteredfItemTitle IsNot Nothing Then
                For Each fResult In FilteredfItemTitle
                    If SearchResult.Contains(fResult) Then Continue For
                    SearchResult.Add(fResult)
                Next
            End If

            If fItemArtist IsNot Nothing Then
                For Each fResult In fItemArtist
                    For Each _fResult In fResult
                        If SearchResult.Contains(_fResult) Then Continue For
                        SearchResult.Add(_fResult)
                    Next
                Next
            End If

            If fItemAlbum IsNot Nothing Then
                For Each fResult In fItemAlbum
                    For Each _fResult In fResult
                        If SearchResult.Contains(_fResult) Then Continue For
                        SearchResult.Add(_fResult)
                    Next
                Next
            End If

            'Cleanup, will be done automaticaly even if left alone ;)
            fItem = Nothing
            fItemTitle = Nothing
            FilteredfItemTitle = Nothing
            fItemArtist = Nothing
            fItemAlbum = Nothing

            If SharedProperties.Instance.IsLogging Then Utilities.DebugMode.Instance.Log(Of Library)("Done searching sync.")

            Return SearchResult
        End Function

        Sub Load(data As Specialized.StringCollection)
            If SharedProperties.Instance.IsLogging Then Utilities.DebugMode.Instance.Log(Of Library)("Attempting to load data, Count:=" & data?.Count)
            If data IsNot Nothing AndAlso data.Count > 0 Then
                Configuration.SetStatus("Loading Data...", 0)
                Dim BinF As New System.Runtime.Serialization.Formatters.Binary.BinaryFormatter()
                Dim ProgStep = If(data.Count = 0, 100, 100 / data.Count)
                Dim i = 1
                For Each bin In data
                    Dim Metadata = BinF.Deserialize(New IO.MemoryStream(Convert.FromBase64String(bin)))
                    Add(Metadata, False)
                    Configuration.SetStatus($"{i}/{data.Count}", Configuration.Progress + ProgStep)
                    i += 1
                Next
                'MostPlayed = Me.OrderByDescending(Of Integer)(Function(k) k.PlayCount).FirstOrDefault
                EnsureMostPlayed(Nothing, False)
            Else
                Configuration.SetStatus("No data to be loaded", 100)
            End If
            Configuration.SetStatus("Search Indexing...", 100)

            RefreshSearchCache()

            If SharedProperties.Instance.IsLogging Then Utilities.DebugMode.Instance.Log(Of Library)("Done loading data.")
            Configuration.SetStatus($"Done indexing, loaded {Count} song", 100)
        End Sub

        Iterator Function Save() As IEnumerable(Of String)
            If SharedProperties.Instance.IsLogging Then Utilities.DebugMode.Instance.Log(Of Library)("Attempting to save data")
            Dim BinF As New System.Runtime.Serialization.Formatters.Binary.BinaryFormatter
            For Each metadata In Me
                Dim mem As New IO.MemoryStream
                BinF.Serialize(mem, metadata)
                Yield Convert.ToBase64String(mem.ToArray)
            Next
            If SharedProperties.Instance.IsLogging Then Utilities.DebugMode.Instance.Log(Of Library)("Done saving data.")
        End Function

        Private Sub RefreshSearchCache()
            Dim _State = Configuration.Status

            Configuration.SetStatus("Caching...", 0)
            Lookup_Path = Me.ToLookup(Of String)(Function(k) IO.Path.GetDirectoryName(k.Path).ToLower)
            Configuration.SetStatus("Caching...", 25)
            Lookup_Title = Me.ToLookup(Of String)(Function(k) k.Title?.ToLower.FirstOrDefault)
            Configuration.SetStatus("Caching...", 50)
            Lookup_Artist = Me.ToLookup(Of String)(Function(k) k.DefaultArtist?.ToLower)
            Configuration.SetStatus("Caching...", 75)
            Lookup_Album = Me.ToLookup(Of String)(Function(k) k.Album?.ToLower)
            Configuration.SetStatus(_State, 100)

            NeedsCacheRefresh = False
        End Sub

        Private Sub ClearSearchCache()
            Lookup_Path = Nothing
            Lookup_Title = Nothing
            Lookup_Artist = Nothing
            Lookup_Album = Nothing

            NeedsCacheRefresh = True
        End Sub

        Function IncreasePlaycount(path As String, increment As Integer) As Boolean
            If SharedProperties.Instance.IsLogging Then Utilities.DebugMode.Instance.Log(Of Library)("Attempting to increase playcount, Path:=" & path & ", Increment:=" & increment)
            Dim meta = Me.FirstOrDefault(Function(k) k.Path = path)
            If meta Is Nothing Then
                If SharedProperties.Instance.IsLogging Then Utilities.DebugMode.Instance.Log(Of Library)("Couldn't find metadata.")
                Return False
            Else
                meta.PlayCount += increment
                If meta.PlayCount > MostPlayed?.PlayCount Then
                    MostPlayed = meta
                End If
                If SharedProperties.Instance.IsLogging Then Utilities.DebugMode.Instance.Log(Of Library)("Done increasing playcount.")
                Return False
            End If
        End Function

        Async Function IncreasePlaycountAsync(path As String, increment As Integer) As Task(Of Boolean)
            Return Await Task.Run(Function()
                                      If SharedProperties.Instance.IsLogging Then Utilities.DebugMode.Instance.Log(Of Library)("Attempting to increase playcount, Path:=" & path & ", Increment:=" & increment)
                                      Dim meta = Me.FirstOrDefault(Function(k) k.Path = path)
                                      If meta Is Nothing Then
                                          If SharedProperties.Instance.IsLogging Then Utilities.DebugMode.Instance.Log(Of Library)("Couldn't find metadata.")
                                          Return False
                                      Else
                                          meta.PlayCount += increment
                                          'Can be replaced with EnsureMostPlayed if not used in UI thread
                                          If meta.PlayCount > MostPlayed?.PlayCount Then
                                              Application.Current.Dispatcher.Invoke(Sub()
                                                                                        MostPlayed = meta
                                                                                    End Sub)
                                          End If
                                          If SharedProperties.Instance.IsLogging Then Utilities.DebugMode.Instance.Log(Of Library)("Done increasing playcount.")
                                          Return False
                                      End If
                                  End Function)
        End Function

        Sub FocusArtistGroup(artist As String)
            If FocusedGroup IsNot Nothing Then
                FocusedGroup.IsInUse = False
                FocusedGroup.FreeCovers()
            End If

            Dim metadatas = Me.Where(Function(k) k.Artists?.Any(Function(l) l.ToLower = artist.ToLower))
            If metadatas.Count = 0 Then Return
            Dim artistgroup As New MetadataGroup() With {.Name = artist, .Category = "Artist"}
            For Each metadata In metadatas
                artistgroup.Add(metadata)
            Next
            artistgroup.UpdateValues()

            artistgroup.IsInUse = True
            FocusedGroup = artistgroup

            RaiseEvent ArtistFocused(FocusedGroup)
        End Sub

        Sub FocusAlbumGroup(album As String)
            If FocusedGroup IsNot Nothing Then
                FocusedGroup.IsInUse = False
                FocusedGroup.FreeCovers()
            End If

            Dim metadatas = Me.Where(Function(k) k.Album?.ToLower = album.ToLower)
            If metadatas.Count = 0 Then Return
            Dim albumgroup As New MetadataGroup() With {.Name = album, .Category = "Album"}
            For Each metadata In metadatas
                albumgroup.Add(metadata)
            Next
            albumgroup.UpdateValues()

            albumgroup.IsInUse = True
            FocusedGroup = albumgroup

            RaiseEvent AlbumFocused(FocusedGroup)
        End Sub

        Sub FocusGroup(playlist As Playlist)
            If playlist Is Nothing Then Return
            If FocusedGroup IsNot Nothing Then
                FocusedGroup.IsInUse = False
                FocusedGroup.FreeCovers()
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
        End Sub

        Public Shadows Sub Add(item As Metadata, Optional UpdateMostPlayed As Boolean = True)
            MyBase.Add(item)
            NeedsCacheRefresh = True
            OnPropertyChanged(NameOf(Count))
            OnPropertyChanged(NameOf(HasItems))
            If UpdateMostPlayed Then
                EnsureMostPlayed(item)
            End If
        End Sub

        Public Shadows Function Remove(item As Metadata) As Boolean
            Dim r = MyBase.Remove(item)
            If r Then
                NeedsCacheRefresh = True
                OnPropertyChanged(NameOf(Count))
                OnPropertyChanged(NameOf(HasItems))
            End If
            Return r
        End Function

        Public Shadows Sub RemoveAt(index As Integer)
            MyBase.RemoveAt(index)
            NeedsCacheRefresh = True
            OnPropertyChanged(NameOf(Count))
            OnPropertyChanged(NameOf(HasItems))
        End Sub

        Public Shadows Sub Clear()
            MyBase.Clear()
            ClearSearchCache
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
            If MostPlayed Is Nothing Or (MostPlayed IsNot Nothing AndAlso Item Is Nothing) Then
                MostPlayed = Me.OrderByDescending(Of Integer)(Function(k) k.PlayCount).FirstOrDefault
            Else
                If Item?.PlayCount > MostPlayed.PlayCount Then
                    MostPlayed = Item
                End If
            End If

            If SkipArtists Then Return

            Dim Groups As IOrderedEnumerable(Of IGrouping(Of String, Metadata)) = Nothing

            If MostPlayedArtist Is Nothing Then
                'Must be calculated only when necessary
                Groups = Me.GroupBy(Function(k) k.Artists.FirstOrDefault).OrderByDescending(Function(l) l.Sum(Function(m) m.PlayCount))
                If Groups.Count < 1 Then Return
                Dim mpa = New MetadataGroup() With {.Name = Groups.FirstOrDefault?.Key, .Category = "Artist"}
                For Each Item In Groups.FirstOrDefault
                    mpa.Add(Item)
                Next
                mpa.IsInUse = True
                Dim hascovermeta = mpa.FirstOrDefault(Function(k) k.HasCover)
                hascovermeta.EnsureCovers()
                mpa.Cover = New TransformedBitmap(hascovermeta.DefaultCover, New ScaleTransform(150 / hascovermeta.DefaultCover.Width, 150 / hascovermeta.DefaultCover.Height))
                If Not hascovermeta.IsInUse Then hascovermeta.FreeCovers()
                MostPlayedArtist = mpa
                Groups = Nothing
            End If

            If MostPlayedArtist2 Is Nothing Then
                'Must be calculated only when necessary
                If Groups Is Nothing Then Groups = Me.GroupBy(Function(k) k.Artists.FirstOrDefault).OrderByDescending(Function(l) l.Sum(Function(m) m.PlayCount))
                If Groups.Count < 2 Then Return
                Dim mpa = New MetadataGroup() With {.Name = Groups.Skip(1).FirstOrDefault?.Key, .Category = "Artist"}
                For Each Item In Groups.Skip(1).FirstOrDefault
                    mpa.Add(Item)
                Next
                mpa.IsInUse = True
                Dim hascovermeta = mpa.FirstOrDefault(Function(k) k.HasCover)
                hascovermeta.EnsureCovers()
                mpa.Cover = New TransformedBitmap(hascovermeta.DefaultCover, New ScaleTransform(150 / hascovermeta.DefaultCover.Width, 150 / hascovermeta.DefaultCover.Height))
                If Not hascovermeta.IsInUse Then hascovermeta.FreeCovers()
                MostPlayedArtist2 = mpa
                Groups = Nothing
            End If

            If MostPlayedArtist3 Is Nothing Then
                'Must be calculated only when necessary
                If Groups Is Nothing Then Groups = Me.GroupBy(Function(k) k.Artists.FirstOrDefault).OrderByDescending(Function(l) l.Sum(Function(m) m.PlayCount))
                If Groups.Count < 3 Then Return
                Dim mpa = New MetadataGroup() With {.Name = Groups.Skip(2).FirstOrDefault?.Key, .Category = "Artist"}
                For Each Item In Groups.Skip(2).FirstOrDefault
                    mpa.Add(Item)
                Next
                mpa.IsInUse = True
                Dim hascovermeta = mpa.FirstOrDefault(Function(k) k.HasCover)
                hascovermeta.EnsureCovers()
                mpa.Cover = New TransformedBitmap(hascovermeta.DefaultCover, New ScaleTransform(150 / hascovermeta.DefaultCover.Width, 150 / hascovermeta.DefaultCover.Height))
                If Not hascovermeta.IsInUse Then hascovermeta.FreeCovers()
                MostPlayedArtist3 = mpa
                Groups = Nothing
            End If

            OnPropertyChanged(NameOf(RandomGroup))
        End Sub
#End Region
    End Class
End Namespace