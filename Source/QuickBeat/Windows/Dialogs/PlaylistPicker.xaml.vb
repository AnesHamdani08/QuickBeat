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
                _DialogPlaylistResult = ListBox_Main.SelectedItem
            Catch ex As Exception
                _DialogPlaylistResult = Nothing
            End Try
            _SafeClose = True
            DialogResult = True
        End Sub

        Private Sub PlaylistPicker_Initialized(sender As Object, e As EventArgs) Handles Me.Initialized
            TryCast(Me.Resources.Item("LOP"), Lists.ListOfPlaylist)?.Add(Utilities.SharedProperties.Instance.Player?.Playlist)
            For Each plist In Utilities.SharedProperties.Instance.CustomPlaylists
                TryCast(Me.Resources.Item("LOP"), Lists.ListOfPlaylist)?.Add(plist)
            Next
        End Sub

        Private Sub PlaylistPicker_Closing(sender As Object, e As CancelEventArgs) Handles Me.Closing
            If Not _SafeClose Then
                DialogResult = False
            End If
        End Sub
    End Class
End Namespace