Imports System.ComponentModel
Namespace Dialogs
    Public Class VideoEffectPicker
        Private _DialogVideoEffectResult As Type
        ReadOnly Property DialogVideoEffectResult As Type
            Get
                Return _DialogVideoEffectResult
            End Get
        End Property

        Private _SafeClose As Boolean = False

        Private Sub ListBox_Main_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
            Try
                _DialogVideoEffectResult = TryCast(TryCast(ListBox_Main.SelectedItem, ListBoxItem)?.Tag, Type)
            Catch ex As Exception
                _DialogVideoEffectResult = Nothing
            End Try
            _SafeClose = True
            DialogResult = True
        End Sub

        Private Sub PlaylistPicker_Initialized(sender As Object, e As EventArgs) Handles Me.Initialized
            Dim AudioEffectType = GetType(QuickBeat.Player.VideoEffects.VideoEffect)
            Dim AudioEffectsList = GetType(QuickBeat.Player.VideoEffects).GetNestedTypes().Where(Function(k) Not k.IsAbstract AndAlso AudioEffectType.IsAssignableFrom(k))
            For Each ae In AudioEffectsList
                Dim tempAE As Player.VideoEffects.VideoEffect = Activator.CreateInstance(ae)
                Dim LBI As New ListBoxItem() With {.Content = $"{tempAE.Name}", .ToolTip = ae.Name}
                tempAE = Nothing
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
                If DialogVideoEffectResult Is Nothing Then DialogResult = False
            End If
        End Sub
    End Class
End Namespace