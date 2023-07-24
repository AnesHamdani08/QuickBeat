Imports System.ComponentModel
Imports Un4seen.Bass
Imports System.Text.RegularExpressions
Imports QuickBeat.Utilities.CommonFunctions
Namespace QScript
    Public Class DeveloperConsole
        Class Palette
            Property Method As Brush
            Property Keyword As Brush
            Property [Class] As Brush
            Property [String] As Brush
            Property Type As Brush
            Property Argument As Brush
            Property [Enum] As Brush
            Property Text As Brush
            Enum KeywordType
                Text
                Method
                Keyword
                [Class]
                [String]
                Type
                Argument
                [Enum]
            End Enum
        End Class
        Class EditableKeyValuePair(Of TKey, TValue)
            Property Key As TKey
            Property Value As TValue
            Sub New(key As TKey, value As TValue)
                Me.Key = key
                Me.Value = value
            End Sub
        End Class
        Property UIPalette As New Palette With {.Class = Brushes.LightGreen, .Keyword = Brushes.LightBlue, .Method = Brushes.White, .String = Brushes.Orange, .Type = Brushes.DodgerBlue, .Argument = Brushes.White, .Text = Brushes.White, .Enum = Brushes.GreenYellow}
        Private Property AllowClosing As Boolean = False

        Private ReadOnly Property Aqua As New Aqua.Aqua()

        Private Sub ConsoleIn_TB_KeyUp(sender As Object, e As KeyEventArgs) Handles ConsoleIn_TB.KeyUp
            If e.Key = Key.Enter Then
                Try
                    Dim Result = Aqua.ExecuteProxy(ConsoleIn_TB.Text)?.ToString
                    If Result Is Nothing Then Exit Try
                    ConsoleOut_TB.AppendText(Result & Environment.NewLine)
                Catch ex As Exception
                    Utilities.DebugMode.Instance.Log(Of DeveloperConsole)(ex.ToString)
                End Try
            End If
        End Sub

        Private Sub ConsoleOut_TB_TextChanged(sender As Object, e As TextChangedEventArgs) Handles ConsoleOut_TB.TextChanged
            ConsoleOut_TB_SV.ScrollToEnd()
        End Sub

        Private Sub DeveloperConsole_Closing(sender As Object, e As CancelEventArgs) Handles Me.Closing
            If Not AllowClosing Then
                Hide()
                e.Cancel = True
            End If
        End Sub

        Private Sub DeveloperConsole_Activated(sender As Object, e As EventArgs) Handles Me.Activated
            ConsoleIn_TB.Focus()
        End Sub

        Private Sub ConsoleOut_Compiler_TB_TextChanged(sender As Object, e As TextChangedEventArgs) Handles ConsoleOut_Compiler_TB.TextChanged
            ConsoleOut_Compiler_TB.ScrollToEnd()
        End Sub
    End Class
End Namespace