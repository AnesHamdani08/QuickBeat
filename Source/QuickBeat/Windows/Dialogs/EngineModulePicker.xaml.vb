Imports System.ComponentModel
Namespace Dialogs
    Public Class EngineModulePicker
        Private _DialogEngineModuleResult As Type
        ReadOnly Property DialogEngineModuleResult As Type
            Get
                Return _DialogEngineModuleResult
            End Get
        End Property

        Private _SafeClose As Boolean = False

        Private Sub ListBox_Main_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
            Try
                _DialogEngineModuleResult = TryCast(TryCast(ListBox_Main.SelectedItem, ListBoxItem)?.Tag, Type)
            Catch ex As Exception
                _DialogEngineModuleResult = Nothing
            End Try
            _SafeClose = True
            DialogResult = True
        End Sub

        Private Sub PlaylistPicker_Initialized(sender As Object, e As EventArgs) Handles Me.Initialized
            Dim AudioEffectType = GetType(QuickBeat.Player.EngineModule)
            Dim AudioEffectsList = GetType(QuickBeat.Player.EngineModules).GetNestedTypes().Where(Function(k) AudioEffectType.IsAssignableFrom(k))
            For Each ae In AudioEffectsList
                Dim tempAE As Player.EngineModule = Activator.CreateInstance(ae)
                Dim LBI As New ListBoxItem() With {.Content = $"{tempAE.Category}/{tempAE.Name}", .ToolTip = tempAE.Description}
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
                If DialogEngineModuleResult Is Nothing Then DialogResult = False
            End If
        End Sub
    End Class
End Namespace