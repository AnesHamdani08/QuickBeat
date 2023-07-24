Imports System.ComponentModel
Namespace QScript
    Public Class ListEditor
        Dim LetExit As Boolean = False
        Property _ItemsSource As ObjectModel.ObservableCollection(Of String)
        Public Property ItemsSource As ObjectModel.ObservableCollection(Of String)
            Get
                Return _ItemsSource
            End Get
            Set(value As ObjectModel.ObservableCollection(Of String))
                _ItemsSource = value
                Main_ListBox.ItemsSource = value
            End Set
        End Property
        Public Sub SpecializedToItemsSource(Col As Specialized.StringCollection)
            Dim ItS As New ObjectModel.ObservableCollection(Of String)
            ItemsSource = ItS
            For Each item In Col
                ItS.Add(item)
            Next
        End Sub

        Public Function ItemsSourceToSpecialized() As Specialized.StringCollection
            Dim Col As New Specialized.StringCollection
            For Each item In ItemsSource
                Col.Add(item)
            Next
            Return Col
        End Function

        Private Sub CTRL_Add_MouseLeftButtonUp(sender As Object, e As MouseButtonEventArgs) Handles CTRL_Add.MouseLeftButtonUp
            Dim IB = InputBox("Value")
            If Not String.IsNullOrEmpty(IB) Then
                ItemsSource.Add(IB)
            End If
        End Sub

        Private Sub CTRL_Clear_MouseLeftButtonUp(sender As Object, e As MouseButtonEventArgs) Handles CTRL_Clear.MouseLeftButtonUp
            Dim IB = HandyControl.Controls.MessageBox.Ask("Clear?")
            If IB = MessageBoxResult.Yes Then
                ItemsSource.Clear()
            End If
        End Sub

        Private Sub CTRL_Import_MouseLeftButtonUp(sender As Object, e As MouseButtonEventArgs) Handles CTRL_Import.MouseLeftButtonUp
            Dim opf As New Microsoft.Win32.OpenFileDialog With {.CheckFileExists = True}
            If opf.ShowDialog Then
                For Each line In IO.File.ReadAllLines(opf.FileName)
                    ItemsSource.Add(line)
                Next
            End If
        End Sub

        Private Sub CTRL_MoveDown_MouseLeftButtonUp(sender As Object, e As MouseButtonEventArgs) Handles CTRL_MoveDown.MouseLeftButtonUp
            If Main_ListBox.SelectedIndex <> -1 Then ItemsSource.Move(Main_ListBox.SelectedIndex, Main_ListBox.SelectedIndex + 1)
        End Sub

        Private Sub CTRL_MoveUp_MouseLeftButtonUp(sender As Object, e As MouseButtonEventArgs) Handles CTRL_MoveUp.MouseLeftButtonUp
            If Main_ListBox.SelectedIndex <> -1 Then ItemsSource.Move(Main_ListBox.SelectedIndex, Main_ListBox.SelectedIndex - 1)
        End Sub

        Private Sub CTRL_Remove_MouseLeftButtonUp(sender As Object, e As MouseButtonEventArgs) Handles CTRL_Remove.MouseLeftButtonUp
            If Main_ListBox.SelectedIndex <> -1 Then ItemsSource.RemoveAt(Main_ListBox.SelectedIndex)
        End Sub

        Private Sub CTRL_Save_MouseLeftButtonUp(sender As Object, e As MouseButtonEventArgs) Handles CTRL_Save.MouseLeftButtonUp
            Dim sfd As New Microsoft.Win32.SaveFileDialog With {.Filter = "Text|*.txt"}
            If sfd.ShowDialog Then
                Dim FS As New IO.FileStream(sfd.FileName, IO.FileMode.OpenOrCreate, IO.FileAccess.Write, IO.FileShare.Read)
                Dim SW As New IO.StreamWriter(FS)
                For Each item In ItemsSource
                    SW.WriteLine(item)
                Next
                SW.Flush()
                FS.Flush()
                FS.Close()
            End If
        End Sub

        Private Sub CTRL_MoveTo_MouseLeftButtonUp(sender As Object, e As MouseButtonEventArgs) Handles CTRL_MoveTo.MouseLeftButtonUp
            Dim IB = InputBox($"To <0;{ItemsSource.Count - 1}>")
            Dim iIB As Integer
            If Integer.TryParse(IB, iIB) Then
                ItemsSource.Move(Main_ListBox.SelectedIndex, iIB)
            End If
        End Sub

        Private Sub ListEditor_Closing(sender As Object, e As CancelEventArgs) Handles Me.Closing
            If Not LetExit Then DialogResult = False
        End Sub

        Private Sub TitleBar_Done_Click(sender As Object, e As RoutedEventArgs) Handles TitleBar_Done.Click
            LetExit = True
            DialogResult = True
        End Sub

        Private Sub Main_ListBox_MouseDoubleClick(sender As Object, e As MouseButtonEventArgs) Handles Main_ListBox.MouseDoubleClick
            If Main_ListBox.SelectedIndex <> -1 Then
                Dim IB = InputBox($"{Main_ListBox.SelectedIndex}. {ItemsSource(Main_ListBox.SelectedIndex)}")
                If Not String.IsNullOrEmpty(IB) Then
                    Try
                        ItemsSource.Item(Main_ListBox.SelectedIndex) = IB
                    Catch
                    End Try
                End If
            End If
        End Sub
    End Class
End Namespace