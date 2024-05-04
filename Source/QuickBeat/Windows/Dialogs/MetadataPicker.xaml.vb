Imports System.Collections.ObjectModel
Imports System.ComponentModel
Imports HandyControl.Data
Imports QuickBeat.Player

Namespace Dialogs
    Public Class MetadataPicker
        Private _DialogMetadataResult As Metadata
        ReadOnly Property DialogMetadataResult As Metadata
            Get
                Return _DialogMetadataResult
            End Get
        End Property

        Private _SafeClose As Boolean = False

        Private _ItemsSource As New ObservableCollection(Of Metadata)
        ReadOnly Property ItemsSource As ObservableCollection(Of Metadata)
            Get
                Return _ItemsSource
            End Get
        End Property

        Private Sub ListBox_Main_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
            If ListBox_Main.SelectedItem Is Nothing Then Return
            _DialogMetadataResult = ListBox_Main.SelectedItem
            _SafeClose = True
            DialogResult = True
        End Sub

        Public Sub Populate(Metadatas As IEnumerable(Of Metadata))
            For Each item In Metadatas
                ItemsSource.Add(item)
            Next
        End Sub

        Private Sub PlaylistPicker_Closing(sender As Object, e As CancelEventArgs) Handles Me.Closing
            If Not _SafeClose Then
                DialogResult = False
            Else
                If DialogMetadataResult Is Nothing Then DialogResult = False
            End If
        End Sub

        Private Sub MetadataPicker_Initialized(sender As Object, e As EventArgs) Handles Me.Initialized
            ListBox_Main.ItemsSource = ItemsSource
        End Sub

        Private Sub SearchBar_Main_SearchStarted(sender As Object, e As FunctionEventArgs(Of String)) Handles SearchBar_Main.SearchStarted
            Dim i = ItemsSource.FirstOrDefault(Function(k) k.Title.ToLower.Contains(e.Info) OrElse k.DefaultArtist?.ToLower.Contains(e.Info) OrElse If(String.IsNullOrEmpty(k.Path), False, k.Path.ToLower.Contains(e.Info)))
            If i IsNot Nothing Then ListBox_Main.ScrollIntoView(i)
        End Sub

        Private Sub MetadataPicker_PreviewKeyUp(sender As Object, e As KeyEventArgs) Handles Me.PreviewKeyUp
            If e.Key = Key.Escape Then Close()
        End Sub
    End Class
End Namespace