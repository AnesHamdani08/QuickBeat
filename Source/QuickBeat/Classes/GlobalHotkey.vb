Imports System.Runtime.InteropServices
Namespace Hotkeys
    Public Class GlobalHotkey
        Private modifier As Integer
        Private key As Integer
        Private hWnd As IntPtr
        Private id As Integer

        ''' <summary>
        ''' Initializes a new instance of <see cref="GlobalHotkey"/>.
        ''' </summary>
        ''' <param name="modifier">One of the predefined modifiers, defined in <see cref="Utilities.CommonFunctions"/></param>
        ''' <param name="key">One of the predefined keys in <see cref="Input.Key"/>.</param>
        ''' <param name="handle">The handle of the window.</param>
        ''' <param name="id">The ID of the hotkey.</param>
        Public Sub New(ByVal modifier As Integer, ByVal key As Key, ByVal handle As IntPtr, id As Integer)
            Me.modifier = modifier
            Me.key = CInt(KeyInterop.VirtualKeyFromKey(key))
            Me.hWnd = handle
            Me.id = id
        End Sub
        Public Function Register() As Boolean
            Return RegisterHotKey(hWnd, id, modifier, key)
        End Function

        Public Function Unregister() As Boolean
            Return UnregisterHotKey(hWnd, id)
        End Function

        Public Overrides Function GetHashCode() As Integer
            Return modifier Xor key Xor hWnd.ToInt32()
        End Function

        <DllImport("user32.dll")>
        Private Shared Function RegisterHotKey(ByVal hWnd As IntPtr, ByVal id As Integer, ByVal fsModifiers As Integer, ByVal vk As Integer) As Boolean
        End Function

        <DllImport("user32.dll")>
        Private Shared Function UnregisterHotKey(ByVal hWnd As IntPtr, ByVal id As Integer) As Boolean
        End Function
    End Class
End Namespace