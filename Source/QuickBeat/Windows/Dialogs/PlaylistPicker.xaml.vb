Imports System.ComponentModel
Namespace Dialogs
    Public Class PlaylistPicker
        Private _DialogPlaylistResult As Player.Playlist
        ReadOnly Property DialogPlaylistResult As Player.Playlist
            Get
                Return _DialogPlaylistResult
            End Get
        End Property

        Private _SafeClose As Boolean = False

        Private Sub ListBox_Main_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
            Try
                If TryCast(ListBox_Main.SelectedItem, QuickBeat.Player.Playlist)?.Tag = "NEW" Then
                    Dim name = Dialogs.InputBox.ShowSingle(Owner, Utilities.ResourceResolver.Strings.NAME)
                    If String.IsNullOrEmpty(name) Then
                        DialogResult = False
                    Else
                        Dim pl As New Player.Playlist With {.Name = name, .Cover = Utilities.ToCoverImage(name)}
                        Utilities.SharedProperties.Instance.CustomPlaylists?.Add(pl)
                        _DialogPlaylistResult = pl
                    End If
                Else
                    _DialogPlaylistResult = ListBox_Main.SelectedItem
                End if
            Catch ex As Exception
                _DialogPlaylistResult = Nothing
            End Try
            _SafeClose = True
            DialogResult = True
        End Sub

        Private Sub PlaylistPicker_Initialized(sender As Object, e As EventArgs) Handles Me.Initialized
            TryCast(Me.Resources.Item("LOP"), Lists.ListOfPlaylist)?.Add(Utilities.SharedProperties.Instance.Player?.Playlist)
            Application.Current.Dispatcher.Invoke(Sub()
                                                      Dim cPL As New QuickBeat.Player.Playlist() With {.Name = Utilities.ResourceResolver.Strings.NEWPLAYLIST, .Description = Utilities.ResourceResolver.Strings.NEWPLAYLIST_HINT, .Category = "Action", .Tag = "NEW", .Cover = Utilities.ToCoverImage("+", True)}
                                                      TryCast(Me.Resources.Item("LOP"), Lists.ListOfPlaylist)?.Add(cPL)
                                                  End Sub)
            For Each plist In Utilities.SharedProperties.Instance.CustomPlaylists
                TryCast(Me.Resources.Item("LOP"), Lists.ListOfPlaylist)?.Add(plist)
            Next
        End Sub

        Private Sub PlaylistPicker_Closing(sender As Object, e As CancelEventArgs) Handles Me.Closing
            If Not _SafeClose Then
                DialogResult = False
            End If
        End Sub

        Private Sub PlaylistPicker_PreviewKeyUp(sender As Object, e As KeyEventArgs) Handles Me.PreviewKeyUp
            If e.Key = Key.Escape Then Close()
        End Sub
    End Class
End Namespace