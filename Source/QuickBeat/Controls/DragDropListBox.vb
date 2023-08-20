Namespace Controls
    Public Class DragDropListBox
        Inherits ListBox

        Shared ReadOnly Property IsDragDropModeProperty As DependencyProperty = DependencyProperty.Register("IsDragDropMode", GetType(Boolean), GetType(DragDropListBox))

        Public Property IsDragDropMode As Boolean
            Get
                Return GetValue(IsDragDropModeProperty)
            End Get
            Set(value As Boolean)
                SetValue(IsDragDropModeProperty, value)
            End Set
        End Property

        Private Sub DragDropListBox_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
            Dim ICS As New Style(GetType(ListBoxItem))
            ICS.BasedOn = ItemContainerStyle
            ICS.Setters.Add(New Setter(ListBoxItem.AllowDropProperty, True))
            ICS.Setters.Add(New EventSetter(ListBoxItem.PreviewMouseLeftButtonDownEvent, New MouseButtonEventHandler(AddressOf ListBox_Playlist_PreviewMouseLeftButtonDown)))
            ICS.Setters.Add(New EventSetter(ListBoxItem.DropEvent, New DragEventHandler(AddressOf ListBox_Playlist_Drop)))
            Me.ItemContainerStyle = ICS
        End Sub

        Private Sub ListBox_Playlist_PreviewMouseLeftButtonDown(sender As Object, e As MouseButtonEventArgs)
            If IsDragDropMode Then
                If e.LeftButton = MouseButtonState.Pressed Then
                    Dim dragged As ListBoxItem = TryCast(sender, ListBoxItem)
                    DragDrop.DoDragDrop(dragged, New Object() {dragged, dragged.DataContext}, DragDropEffects.Move)
                    e.Handled = True
                End If
            End If
        End Sub

        Private Sub ListBox_Playlist_Drop(sender As Object, e As DragEventArgs)
            Dim dataS As Object() = e.Data.GetData(GetType(Object()))
            If dataS Is Nothing Then Return
            Dim data As Player.Metadata = dataS(1)
            Dim i_data = ItemsControl.ItemsControlFromItemContainer(dataS(0))?.ItemContainerGenerator.IndexFromContainer(dataS(0))
            Dim target As Player.Metadata = TryCast(sender, ListBoxItem).DataContext
            Dim i_target = ItemsControl.ItemsControlFromItemContainer(TryCast(sender, ListBoxItem)).ItemContainerGenerator.IndexFromContainer(TryCast(sender, ListBoxItem))
            If (i_data = i_target) Is Nothing OrElse i_data = i_target Then Return
            Dim iType = ItemsSource.GetType
            If iType.BaseType.IsGenericType AndAlso iType.BaseType.GetGenericTypeDefinition Is GetType(ObjectModel.ObservableCollection(Of )) Then
                TryCast(ItemsSource, Object).Move(i_data, i_target)
            End If
        End Sub
    End Class
End Namespace