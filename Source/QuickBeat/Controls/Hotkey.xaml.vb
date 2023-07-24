Imports QuickBeat.Utilities

Namespace Controls
    Public Class Hotkey
        Shared ReadOnly Property HotkeyProperty As DependencyProperty = DependencyProperty.Register("Hotkey", GetType(Hotkeys.HotkeyManager.Hotkey), GetType(Hotkey), New UIPropertyMetadata(New Hotkeys.HotkeyManager.Hotkey))
        Property Hotkey As Hotkeys.HotkeyManager.Hotkey
            Get
                Return GetValue(HotkeyProperty)
            End Get
            Set(value As Hotkeys.HotkeyManager.Hotkey)
                SetValue(HotkeyProperty, value)
            End Set
        End Property

        Private Sub FocusCommand_CanExecute(sender As Object, e As CanExecuteRoutedEventArgs)
            e.CanExecute = True
            e.Handled = True
        End Sub

        Private Sub FocusCommand_Executed(sender As Object, e As ExecutedRoutedEventArgs)
            Focus()
            e.Handled = True
        End Sub

        Private Sub Hotkey_KeyDown(sender As Object, e As KeyEventArgs) Handles Me.KeyDown
            Select Case Keyboard.Modifiers
                Case ModifierKeys.None
                    'Hotkey = New Hotkeys.HotkeyManager.Hotkey() With {.Key = e.Key, .Modifier = CommonFunctions.NOMOD}
                    Hotkey.Key = e.Key
                    Hotkey.Modifier = CommonFunctions.NOMOD
                Case ModifierKeys.Control
                    'Hotkey = New Hotkeys.HotkeyManager.Hotkey() With {.Key = e.Key, .Modifier = CommonFunctions.CTRL}
                    Hotkey.Key = e.Key
                    Hotkey.Modifier = CommonFunctions.CTRL
                Case ModifierKeys.Alt
                    'Hotkey = New Hotkeys.HotkeyManager.Hotkey() With {.Key = e.Key, .Modifier = CommonFunctions.ALT}
                    Hotkey.Key = e.Key
                    Hotkey.Modifier = CommonFunctions.ALT
                Case ModifierKeys.Shift
                    'Hotkey = New Hotkeys.HotkeyManager.Hotkey() With {.Key = e.Key, .Modifier = CommonFunctions.SHIFT}
                    Hotkey.Key = e.Key
                    Hotkey.Modifier = CommonFunctions.SHIFT
                Case ModifierKeys.Windows
                    'Hotkey = New Hotkeys.HotkeyManager.Hotkey() With {.Key = e.Key, .Modifier = CommonFunctions.WIN}
                    Hotkey.Key = e.Key
                    Hotkey.Modifier = CommonFunctions.WIN
            End Select
        End Sub

    End Class
End Namespace