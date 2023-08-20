Imports System.ComponentModel
Imports MetaBrainz.MusicBrainz.Interfaces

Namespace Utilities
    Public Class FanArtProvider
        Implements ComponentModel.INotifyPropertyChanged

        Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged

        Protected Sub OnPropertyChanged(<Runtime.CompilerServices.CallerMemberName> Optional CallerName As String = Nothing)
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(CallerName))
        End Sub

        Private _Metadata As Player.MetadataGroup
        Property Metadata As Player.MetadataGroup
            Get
                Return _Metadata
            End Get
            Set(value As Player.MetadataGroup)
                If value Is _Metadata Then Return
                _Metadata = value
                OnPropertyChanged()
                Fetch()
            End Set
        End Property

        Private _Background As ImageSource
        Property Background As ImageSource
            Get
                Return _Background
            End Get
            Set(value As ImageSource)
                _Background = value
                OnPropertyChanged()
            End Set
        End Property

        Private _Thumb As ImageSource
        Property Thumb As ImageSource
            Get
                Return _Thumb
            End Get
            Set(value As ImageSource)
                _Thumb = value
                OnPropertyChanged()
            End Set
        End Property

        Private _BindToDefault As Boolean = False
        Property BindToDefault As Boolean
            Get
                Return _BindToDefault
            End Get
            Set(value As Boolean)
                _BindToDefault = value
                If value Then
                    Metadata = Utilities.SharedProperties.Instance.Library?.MostPlayedArtist
                    If Utilities.SharedProperties.Instance.Library IsNot Nothing Then AddHandler Utilities.SharedProperties.Instance.Library.MostPlayedChanged, _BindHandler
                End If
                    OnPropertyChanged()
            End Set
        End Property

        Private _IsBusy As Boolean
        Property IsBusy As Boolean
            Get
                Return _IsBusy
            End Get
            Set(value As Boolean)
                _IsBusy = value
                OnPropertyChanged()
            End Set
        End Property

        Private _BindHandler As New Library.Library.MostPlayedChangedEventHandler(Sub(s)
                                                                                      Metadata = s.MostPlayedArtist
                                                                                  End Sub)

        Async Sub Fetch()
            IsBusy = True
            'release:=album, recording:=track
            'special characters:= + - && || ! ( ) { } [ ] ^ " ~ * ? : \/                           
            Dim SpecialCharacters As String() = New String() {"+", "-", "&&", "||", "!", "(", ")", "{", "}", "[", "]", "^", """", "~", "*", "?", ":", "\/"}
            Dim SearchQuery As String = ""
            Dim EArtist = Metadata.Name.Replace("+", "\+").Replace("-", "\-").Replace("&&", "\&&").Replace("||", "\||").Replace("!", "\!").Replace("(", "\(").Replace(")", "\)").Replace("{", "\{").Replace("}", "\}").Replace("[", "\[").Replace("]", "\]").Replace("^", "\^").Replace("""", "\""").Replace("~", "\~").Replace("*", "\*").Replace("?", "\?").Replace(":", "\:").Replace("\/", "\\/")
            SearchQuery = $"Artist:{EArtist}"
            Dim Query As New MetaBrainz.MusicBrainz.Query("QuickBeat", New Version(1, 0, 0, 0))
            Dim Result As Searches.ISearchResults(Of Searches.ISearchResult(Of Entities.IArtist)) = Nothing
            Try
                Result = Await Query.FindArtistsAsync(SearchQuery, 1)
            Catch ex As Exception
                Utilities.DebugMode.Instance.Log(Of FanArtProvider)(ex.ToString)
            End Try
            If Result Is Nothing OrElse Result.TotalResults = 0 Then
                FallBack()
                IsBusy = False
                Return
            End If
            Dim FResult = Result?.Results.FirstOrDefault
            If FResult Is Nothing Then
                FallBack()
                IsBusy = False
                Return
            End If
            Dim Artist = New FanartTv.Music.Artist(FResult.Item.Id.ToString)

            Dim BgURL = Artist.List.AImagesrtistbackground?.FirstOrDefault?.Url
            If Not String.IsNullOrEmpty(BgURL) Then
                Using WC As New Net.WebClient()
                    Try
                        Dim Data = Await WC.DownloadDataTaskAsync(BgURL)
                        Dim BI As New BitmapImage
                        BI.BeginInit()
                        BI.StreamSource = New IO.MemoryStream(Data)
                        BI.EndInit()
                        Background = BI
                    Catch ex As Exception
                        Utilities.DebugMode.Instance.Log(Of FanArtProvider)(ex.ToString)
                        FallBack(False)
                    End Try
                End Using
            Else
                FallBack(False, True)
            End If

            Dim ThumbURL = Artist.List.Artistthumb?.FirstOrDefault?.Url
            If Not String.IsNullOrEmpty(ThumbURL) Then
                Using WC As New Net.WebClient()
                    Try
                        Dim Data = Await WC.DownloadDataTaskAsync(ThumbURL)
                        Dim BI As New BitmapImage
                        BI.BeginInit()
                        BI.StreamSource = New IO.MemoryStream(Data)
                        BI.EndInit()
                        Thumb = BI
                    Catch ex As Exception
                        Utilities.DebugMode.Instance.Log(Of FanArtProvider)(ex.ToString)
                        FallBack(Background:=False)
                    End Try
                End Using
            Else
                FallBack(True, False)
            End If
            IsBusy = False
        End Sub

        Private Sub FallBack(Optional Thumb As Boolean = True, Optional Background As Boolean = True)
            If Thumb Then Me.Thumb = Metadata?.Cover
            If Background Then Me.Background = Metadata?.Cover
        End Sub
    End Class
End Namespace