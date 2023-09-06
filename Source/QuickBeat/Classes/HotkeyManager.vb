Imports System.ComponentModel
Imports System.Diagnostics.Tracing
Imports System.Runtime.CompilerServices
Imports System.Threading
Imports QuickBeat.Classes
Imports QuickBeat.Hotkeys.WPF.Commands
Imports QuickBeat.Interfaces
Imports QuickBeat.Utilities

Namespace Hotkeys
    <Serializable>
    Public Class HotkeyManager
        Inherits ObjectModel.ObservableCollection(Of Hotkey)
        Implements Interfaces.IStartupItem

#Region "Constants"
        'this constant represents which command. Sort of like the function in user32.dll we are calling.
        Private Const WM_APPCOMMAND = &H319
        'this declares the user32.dll call to SendMessageW we are making
        Declare Auto Function SendMessageW Lib "user32.dll" Alias "SendMessageW" (
    ByVal hWnd As Integer,
    ByVal Msg As Integer,
    ByVal wParam As Integer,
    ByVal lParam As Integer) As Integer
#End Region
#Region "Classes"
        <Serializable>
        Public Class Hotkey
            Implements ComponentModel.INotifyPropertyChanged

            <NonSerialized> Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged
            Protected Sub OnPropertyChanged(<Runtime.CompilerServices.CallerMemberName> Optional CallerName As String = Nothing)
                RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(CallerName))
            End Sub

            <NonSerialized> Private _Parent As HotkeyManager
            Property Parent As HotkeyManager
                Get
                    Return _Parent
                End Get
                Set(value As HotkeyManager)
                    _Parent = value
                    OnPropertyChanged()
                End Set
            End Property

            Private _Key As Key
            Property Key As Key
                Get
                    Return _Key
                End Get
                Set(value As Key)
                    _Key = value
                    OnPropertyChanged()
                End Set
            End Property

            Private _Modifier As Integer
            Property Modifier As Integer
                Get
                    Return _Modifier
                End Get
                Set(value As Integer)
                    _Modifier = value
                    OnPropertyChanged()
                    OnPropertyChanged(NameOf(ModifierString))
                End Set
            End Property

            ReadOnly Property ModifierString As String
                Get
                    Select Case Modifier
                        Case NOMOD
                            Return ""
                        Case SHIFT
                            Return "Shift"
                        Case CTRL
                            Return "Ctrl"
                        Case ALT
                            Return "Alt"
                        Case WIN
                            Return "Windows"
                    End Select
                    Return Modifier
                End Get
            End Property

            Private _Message As Messages
            Property Message As Messages
                Get
                    Return _Message
                End Get
                Set(value As Messages)
                    _Message = value
                    OnPropertyChanged()
                End Set
            End Property

            <NonSerialized> Private _IsEnabled As Boolean
            ''' <summary>
            ''' Don't modify directly, Please use <see cref="HotkeyManager.Enable(Hotkey)"/> or <see cref="HotkeyManager.Disable(Hotkey)"/>.
            ''' </summary>
            ''' <returns></returns>
            Property IsEnabled As Boolean
                Get
                    Return _IsEnabled
                End Get
                Set(value As Boolean)
                    _IsEnabled = value
                    OnPropertyChanged()
                End Set
            End Property

        End Class
        Public Enum Messages
            WIN_APPCOMMAND_BROWSER_BACKWARD = 1
            WIN_APPCOMMAND_BROWSER_FORWARD = 2
            WIN_APPCOMMAND_BROWSER_REFRESH = 3
            WIN_APPCOMMAND_BROWSER_STOP = 4
            WIN_APPCOMMAND_BROWSER_SEARCH = 5
            WIN_APPCOMMAND_BROWSER_FAVORITES = 6
            WIN_APPCOMMAND_BROWSER_HOME = 7
            WIN_APPCOMMAND_VOLUME_MUTE = 8
            WIN_APPCOMMAND_VOLUME_DOWN = 9
            WIN_APPCOMMAND_VOLUME_UP = 10
            WIN_APPCOMMAND_MEDIA_NEXTTRACK = 11
            WIN_APPCOMMAND_MEDIA_PREVIOUSTRACK = 12
            WIN_APPCOMMAND_MEDIA_STOP = 13
            WIN_APPCOMMAND_MEDIA_PLAY_PAUSE = 14
            WIN_APPCOMMAND_LAUNCH_MAIL = 15
            WIN_APPCOMMAND_LAUNCH_MEDIA_SELECT = 16
            WIN_APPCOMMAND_LAUNCH_APP1 = 17
            WIN_APPCOMMAND_LAUNCH_APP2 = 18
            WIN_APPCOMMAND_BASS_DOWN = 19
            WIN_APPCOMMAND_BASS_BOOST = 20
            WIN_APPCOMMAND_BASS_UP = 21
            WIN_APPCOMMAND_TREBLE_DOWN = 22
            WIN_APPCOMMAND_TREBLE_UP = 23
            WIN_APPCOMMAND_MICROPHONE_VOLUME_MUTE = 24
            WIN_APPCOMMAND_MICROPHONE_VOLUME_DOWN = 25
            WIN_APPCOMMAND_MICROPHONE_VOLUME_UP = 26
            WIN_APPCOMMAND_HELP = 27
            WIN_APPCOMMAND_FIND = 28
            WIN_APPCOMMAND_NEW = 29
            WIN_APPCOMMAND_OPEN = 30
            WIN_APPCOMMAND_CLOSE = 31
            WIN_APPCOMMAND_SAVE = 32
            WIN_APPCOMMAND_PRINT = 33
            WIN_APPCOMMAND_UNDO = 34
            WIN_APPCOMMAND_REDO = 35
            WIN_APPCOMMAND_COPY = 36
            WIN_APPCOMMAND_CUT = 37
            WIN_APPCOMMAND_PASTE = 38
            WIN_APPCOMMAND_REPLY_TO_MAIL = 39
            WIN_APPCOMMAND_FORWARD_MAIL = 40
            WIN_APPCOMMAND_SEND_MAIL = 41
            WIN_APPCOMMAND_SPELL_CHECK = 42
            WIN_APPCOMMAND_DICTATE_OR_COMMAND_CONTROL_TOGGLE = 43
            WIN_APPCOMMAND_MIC_ON_OFF_TOGGLE = 44
            WIN_APPCOMMAND_CORRECTION_LIST = 45
            WIN_APPCOMMAND_MEDIA_PLAY = 46
            WIN_APPCOMMAND_MEDIA_PAUSE = 47
            WIN_APPCOMMAND_MEDIA_RECORD = 48
            WIN_APPCOMMAND_MEDIA_FAST_FORWARD = 49
            WIN_APPCOMMAND_MEDIA_REWIND = 50
            WIN_APPCOMMAND_MEDIA_CHANNEL_UP = 51
            WIN_APPCOMMAND_MEDIA_CHANNEL_DOWN = 52
        End Enum
#End Region
#Region "Properties"
        <NonSerialized> Private _Hwnd As IntPtr
        ReadOnly Property Hwnd As IntPtr
            Get
                Return _Hwnd
            End Get
        End Property

        ReadOnly Property HasItems As Boolean
            Get
                Return (Count > 0)
            End Get
        End Property

        Private _Configuration As New StartupItemConfiguration("Hotkey Manager")
        Public Property Configuration As StartupItemConfiguration Implements IStartupItem.Configuration
            Get
                Return _Configuration
            End Get
            Set(value As StartupItemConfiguration)
                _Configuration = value
                OnPropertyChanged()
            End Set
        End Property
#End Region
#Region "Ctor"
        Sub New(Hwnd As IntPtr)
            _Hwnd = Hwnd
        End Sub
#End Region
#Region "Methods"
        Protected Function WndProc(hwnd As IntPtr, msg As Integer, wParam As IntPtr, lParam As IntPtr, ByRef handled As Boolean) As IntPtr
            'wParam = Hotkey.ID
            If msg = Utilities.CommonFunctions.WM_HOTKEY_MSG_ID Then
                DispatchMessage(wParam)
                handled = True
            End If
            Return IntPtr.Zero
        End Function

        Shadows Function Add(item As Hotkey) As Boolean
            Dim cItem = New Hotkey With {.Key = item.Key, .Message = item.Message, .Modifier = item.Modifier} 'Use a clone to avoid binding to a reference
            If cItem Is Nothing Then Return False
            If Not [Enum].IsDefined(GetType(Messages), cItem.Message) Then Return False
            If Not Configuration.IsLoaded Then Throw New InvalidOperationException("Make sure ""Init"" is called before adding cItems.")
            Dim GH As New GlobalHotkey(cItem.Modifier, cItem.Key, Hwnd, cItem.Message)
            cItem.IsEnabled = GH.Register
            cItem.Parent = Me
            MyBase.Add(cItem)
            OnPropertyChanged(NameOf(Count))
            OnPropertyChanged(NameOf(HasItems))
            Return True
        End Function

        Shadows Function Remove(item As Hotkey) As Boolean
            If item Is Nothing Then Return False
            Dim Hotkey = Me.FirstOrDefault(Function(k) k.Key = item.Key AndAlso k.Message = item.Message AndAlso k.Modifier = item.Modifier)
            If Hotkey Is Nothing Then Return False
            Dim GH As New GlobalHotkey(Hotkey.Modifier, Hotkey.Key, Hwnd, Hotkey.Message)
            Dim Result = GH.Unregister()
            If Not Result Then Return False
            Dim r = MyBase.Remove(item)
            OnPropertyChanged(NameOf(Count))
            OnPropertyChanged(NameOf(HasItems))
            Return r
        End Function

        Shadows Sub RemoveAt(index As Integer)
            If index < 0 OrElse index >= Count Then Return
            Dim Hotkey = Item(index)
            Dim GH As New GlobalHotkey(Hotkey.Modifier, Hotkey.Key, Hwnd, Hotkey.Modifier)
            Dim Result = GH.Unregister()
            If Not Result Then Return
            MyBase.RemoveAt(index)
            OnPropertyChanged(NameOf(Count))
            OnPropertyChanged(NameOf(HasItems))
        End Sub

        Shadows Sub Clear()
            For Each Hotkey In Me
                Dim GH As New GlobalHotkey(Hotkey.Modifier, Hotkey.Key, Hwnd, Hotkey.Message)
                If GH.Unregister Then Hotkey.IsEnabled = False
            Next
            MyBase.Clear()
            OnPropertyChanged(NameOf(Count))
            OnPropertyChanged(NameOf(HasItems))
        End Sub

        ''' <summary>
        ''' Attempts to disable <see cref="Hotkey"/> and returns the result of the operation. Note: the new <see cref="Hotkey"/> is stored in <see cref="Hotkey.IsEnabled"/>.
        ''' </summary>
        ''' <param name="item">Must be a child of the current instance.</param>
        ''' <returns>A <see cref="Boolean"/> indicating the result of the operation</returns>
        Function Disable(item As Hotkey) As Boolean
            Dim hk = Me.FirstOrDefault(Function(k) k.Key = item.Key AndAlso k.Modifier = item.Modifier AndAlso k.Message = item.Message)
            If hk.IsEnabled = False Then Return True
            If hk IsNot Nothing Then
                Dim GH As New GlobalHotkey(item.Modifier, item.Key, Hwnd, item.Message)
                If GH.Unregister Then
                    hk.IsEnabled = False
                    Return True
                End If
            End If
            Return False
        End Function

        ''' <summary>
        ''' Attempts to enable <see cref="Hotkey"/> and returns the result of the operation. Note: the new <see cref="Hotkey"/> is stored in <see cref="Hotkey.IsEnabled"/>.
        ''' </summary>
        ''' <param name="item">Must be a child of the current instance.</param>
        ''' <returns>A <see cref="Boolean"/> indicating the result of the operation</returns>
        Function Enable(item As Hotkey) As Boolean
            Dim hk = Me.FirstOrDefault(Function(k) k.Key = item.Key AndAlso k.Modifier = item.Modifier AndAlso k.Message = item.Message)
            If hk.IsEnabled Then Return True
            If hk IsNot Nothing Then
                Dim GH As New GlobalHotkey(item.Modifier, item.Key, Hwnd, item.Message)
                If GH.Register Then
                    hk.IsEnabled = True
                    Return True
                End If
            End If
            Return False
        End Function

        ''' <summary>
        ''' Call after deserialization. Updates all <see cref="Hotkey.Parent"/>.
        ''' </summary>
        Sub Update()
            For Each hk In Me ';)
                hk.Parent = Me
            Next
        End Sub

        Public Function DispatchMessage(message As Messages) As Integer
            'call the SendMessage function with the current window handle, the command we want to use, same handle, and the button we want to press
            Return SendMessageW(Hwnd, WM_APPCOMMAND, Hwnd, message << 16)
        End Function

        Public Shadows Sub OnPropertyChanged(<CallerMemberName> Optional CallerName As String = Nothing)
            MyBase.OnPropertyChanged(New PropertyChangedEventArgs(CallerName))
        End Sub

        ''' <summary>
        ''' If <see cref="Hwnd"/> is not set. Will wait async. for an available window to hook to.
        ''' </summary>
        Public Sub Init() Implements IStartupItem.Init
            If Hwnd = IntPtr.Zero Then
                Configuration.SetError(True, New Exception("Waiting for an available window..."))
                Application.Current.Dispatcher.InvokeAsync(Async Function()
                                                               Do While Hwnd = IntPtr.Zero
                                                                   If Application.Current.MainWindow IsNot Nothing Then
                                                                       Dim helper As New Interop.WindowInteropHelper(Application.Current.MainWindow)
                                                                       _Hwnd = helper.EnsureHandle
                                                                       Interop.HwndSource.FromHwnd(Hwnd).AddHook(New Interop.HwndSourceHook(AddressOf WndProc))
                                                                       Configuration.IsErrored = False
                                                                       Configuration.SetStatus("Hooked to " & Application.Current.MainWindow.Title, 100)
                                                                   Else
                                                                       Await Task.Delay(1000)
                                                                   End If
                                                               Loop
                                                           End Function)
            Else
                Interop.HwndSource.FromHwnd(Hwnd).AddHook(New Interop.HwndSourceHook(AddressOf WndProc))
            End If
        End Sub

        'Sub Load(data As Specialized.StringCollection)
        '    Utilities.DebugMode.Instance.Log(Of HotkeyManager)("Attempting to load data, Count:=" & data?.Count)
        '    If data IsNot Nothing AndAlso data.Count > 0 Then
        '        Configuration.SetStatus("Loading Data...", 0)
        '        Dim BinF As New System.Runtime.Serialization.Formatters.Binary.BinaryFormatter()
        '        Dim ProgStep = If(data.Count = 0, 100, 100 / data.Count)
        '        Dim i = 1
        '        For Each bin In data
        '            Dim Hotkey = BinF.Deserialize(New IO.MemoryStream(Convert.FromBase64String(bin)))
        '            Add(Hotkey)
        '            Configuration.SetStatus($"{i}/{data.Count}", Configuration.Progress + ProgStep)
        '            i += 1
        '        Next
        '    Else
        '        Configuration.SetStatus("No data to be loaded", 100)
        '    End If
        '    Utilities.DebugMode.Instance.Log(Of HotkeyManager)("Done loading data.")
        '    Configuration.SetStatus(Configuration.Status, 100)
        'End Sub

        'Iterator Function Save() As IEnumerable(Of String)
        '    Utilities.DebugMode.Instance.Log(Of HotkeyManager)("Attempting to save data")
        '    Dim BinF As New System.Runtime.Serialization.Formatters.Binary.BinaryFormatter
        '    For Each hotkey In Me
        '        Dim mem As New IO.MemoryStream
        '        BinF.Serialize(mem, hotkey)
        '        Yield Convert.ToBase64String(mem.ToArray)
        '    Next
        '    Utilities.DebugMode.Instance.Log(Of HotkeyManager)("Done saving data.")
        'End Function

        Function Save() As IO.MemoryStream
            Dim BinF As New System.Runtime.Serialization.Formatters.Binary.BinaryFormatter
            Dim HkMem As New IO.MemoryStream
            BinF.Serialize(HkMem, Me)
            Return HkMem
        End Function

        ''' <summary>
        ''' The inverse function of <see cref="Save()"/>.Automatically calls <see cref="Init()"/> after deserialization.
        ''' </summary>
        ''' <param name="Data"></param>
        ''' <returns></returns>
        Shared Async Function Load(Data As IO.MemoryStream) As Task(Of HotkeyManager)
            Dim BinF As New System.Runtime.Serialization.Formatters.Binary.BinaryFormatter
            Dim OldHk As HotkeyManager = CType(BinF.Deserialize(Data), HotkeyManager)
            Dim Hk As New HotkeyManager(IntPtr.Zero)
            Hk.Init()
            Do While Not Hk.Configuration.IsLoaded
                Await Task.Delay(100)
            Loop
            For Each hotkey In OldHk
                Hk.Add(hotkey)
            Next
            OldHk.Clear()
            OldHk = Nothing
            Return Hk
        End Function
#End Region
#Region "WPF"
        <NonSerialized> Public ReadOnly _AddCommand As New AddHotkeyCommand(Me)
        ReadOnly Property AddCommand As AddHotkeyCommand
            Get
                Return _AddCommand
            End Get
        End Property
        <NonSerialized> Public ReadOnly _RemoveCommand As New RemoveHotkeyCommand(Me)
        ReadOnly Property RemoveCommand As RemoveHotkeyCommand
            Get
                Return _RemoveCommand
            End Get
        End Property
        <NonSerialized> Public ReadOnly _ClearCommand As New ClearHotkeysCommand(Me)
        ReadOnly Property ClearCommand As ClearHotkeysCommand
            Get
                Return _ClearCommand
            End Get
        End Property
        <NonSerialized> Public ReadOnly _EnableCommand As New EnableHotkeyCommand(Me)
        ReadOnly Property EnableCommand As EnableHotkeyCommand
            Get
                Return _EnableCommand
            End Get
        End Property
        <NonSerialized> Public ReadOnly _DisableCommand As New DisableHotkeyCommand(Me)
        ReadOnly Property DisableCommand As DisableHotkeyCommand
            Get
                Return _DisableCommand
            End Get
        End Property
#End Region

    End Class
End Namespace