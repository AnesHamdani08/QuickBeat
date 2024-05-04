Imports System.ComponentModel
Namespace Dialogs
    Public Class AudioEffectPicker
        Private _DialogAudioEffectResult As Type
        ReadOnly Property DialogAudioEffectResult As Type
            Get
                Return _DialogAudioEffectResult
            End Get
        End Property

        Private _SafeClose As Boolean = False

        Private Sub ListBox_Main_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
            Try
                _DialogAudioEffectResult = TryCast(TryCast(ListBox_Main.SelectedItem, ListBoxItem)?.Tag, Type)
            Catch ex As Exception
                _DialogAudioEffectResult = Nothing
            End Try
            _SafeClose = True
            DialogResult = True
        End Sub

        Private Sub PlaylistPicker_Initialized(sender As Object, e As EventArgs) Handles Me.Initialized
            Dim AudioEffectType = GetType(QuickBeat.Player.Profile.AudioEffect)
            Dim AudioEffectsList = GetType(QuickBeat.Player.AudioEffects).GetNestedTypes().Where(Function(k) AudioEffectType.IsAssignableFrom(k))
            For Each ae In AudioEffectsList
                'Dim tempAE As Player.Profile.AudioEffect = Activator.CreateInstance(ae)
                'Dim LBI As New ListBoxItem() With {.Content = $"{tempAE.Category}/{tempAE.Name}", .ToolTip = tempAE.Description}
                'tempAE = Nothing
                Dim LBI As New ListBoxItem With {.Content = ae.Name}
                LBI.Tag = ae
                ListBox_Main.Items.Add(LBI)
            Next
            Owner = If(Owner, Application.Current.MainWindow)
            Me.WindowStartupLocation = WindowStartupLocation.CenterOwner
        End Sub

        Private Sub PlaylistPicker_Closing(sender As Object, e As CancelEventArgs) Handles Me.Closing
            If Not _SafeClose Then
                DialogResult = False
            Else
                If DialogAudioEffectResult Is Nothing Then DialogResult = False
            End If
        End Sub

        Private Sub AudioEffectPicker_PreviewKeyUp(sender As Object, e As KeyEventArgs) Handles Me.PreviewKeyUp
            If e.Key = Key.Escape Then Close()
        End Sub
    End Class
End Namespace