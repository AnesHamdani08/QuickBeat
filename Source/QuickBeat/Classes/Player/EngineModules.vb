Imports Un4seen.Bass.AddOn.WaDsp
Namespace Player
    Public Class EngineModules
        Public Class BASS_WaDSP
            Inherits EngineModule

            Public Overrides ReadOnly Property Name As String = "BASS WaDSP"

            Public Overrides ReadOnly Property Description As String = "An extension enabling the use of Winamp DSP plugins with BASS."

            Public Overrides ReadOnly Property Category As String = "DSP"

            Private _Info As String
            Public Overrides Property Info As String
                Get
                    Return _Info
                End Get
                Set(value As String)
                    _Info = value
                    OnPropertyChanged()
                End Set
            End Property

            Private _IsEnabled As Boolean
            Public Overrides Property IsEnabled As Boolean
                Get
                    Return _IsEnabled
                End Get
                Set(value As Boolean)
                    OnPropertyChanged()
                End Set
            End Property

            Public Overrides Property Configuration As New Classes.StartupItemConfiguration("BASS WaDSP")

            Private _Handle As Integer = 0
            Public Overrides Property Handle As Integer
                Get
                    Return _Handle
                End Get
                Set(value As Integer)
                    _Handle = value
                    OnPropertyChanged()
                End Set
            End Property

            Private _Path As String
            Public Overrides Property Path As String
                Get
                    Return _Path
                End Get
                Set(value As String)
                    _Path = value
                    OnPropertyChanged()
                End Set
            End Property

            Public Overrides Sub Init()
                If _IsEnabled Then Return

                Utilities.SharedProperties.Instance?.ItemsConfiguration.Add(Configuration)
                Path = IO.Path.Combine(My.Application.Info.DirectoryPath, "bass_wadsp.dll")
                Handle = Un4seen.Bass.Bass.BASS_PluginLoad(Path)
                Dim vDSP = BassWaDsp.BASS_WADSP_GetVersion()
                Dim IsInit = BassWaDsp.BASS_WADSP_Init(IntPtr.Zero)

                _IsEnabled = IsInit
                OnPropertyChanged(NameOf(IsEnabled))

                Info = $"Handle: {Handle}{Environment.NewLine}Version: {vDSP}"
                Configuration.SetStatus($"{If(IsInit, "Loaded", "Error")}: {Info}", If(IsInit, 100, 0))
                If Not IsInit Then Configuration.SetError(True, New InvalidOperationException("Unknown error occured"))
            End Sub

            Public Overrides Sub Free()
                BassWaDsp.BASS_WADSP_Free()
                Utilities.SharedProperties.Instance.ItemsConfiguration.Remove(Configuration)
            End Sub
        End Class

        Public Class BASS_SFX
            Inherits EngineModule

            Public Overrides ReadOnly Property Name As String = "BASS SFX"

            Public Overrides ReadOnly Property Description As String = "An extension allowing the use of Sonique, Winamp, Windows Media Player, and BassBox visual plugins with BASS."

            Public Overrides ReadOnly Property Category As String = "Video Effects"

            Private _Info As String
            Public Overrides Property Info As String
                Get
                    Return _Info
                End Get
                Set(value As String)
                    _Info = value
                    OnPropertyChanged()
                End Set
            End Property

            Private _IsEnabled As Boolean
            Public Overrides Property IsEnabled As Boolean
                Get
                    Return _IsEnabled
                End Get
                Set(value As Boolean)
                    OnPropertyChanged()
                End Set
            End Property

            Public Overrides Property Configuration As New Classes.StartupItemConfiguration("BASS SFX")

            Private _Handle As Integer = 0
            Public Overrides Property Handle As Integer
                Get
                    Return _Handle
                End Get
                Set(value As Integer)
                    _Handle = value
                    OnPropertyChanged()
                End Set
            End Property

            Private _Path As String
            Public Overrides Property Path As String
                Get
                    Return _Path
                End Get
                Set(value As String)
                    _Path = value
                    OnPropertyChanged()
                End Set
            End Property

            Public Overrides Sub Init()
                If IsEnabled Then Return
                Utilities.SharedProperties.Instance?.ItemsConfiguration.Add(Configuration)
                Path = IO.Path.Combine(My.Application.Info.DirectoryPath, "BASS_SFX.dll")
                Handle = 0
                Dim helper As New Interop.WindowInteropHelper(Application.Current.MainWindow)
                Dim _Hwnd = helper.EnsureHandle
                If Un4seen.Bass.AddOn.Sfx.BassSfx.BASS_SFX_Init(System.Diagnostics.Process.GetCurrentProcess().Handle, _Hwnd) Then
                    _IsEnabled = True
                    Info = Un4seen.Bass.AddOn.Sfx.BassSfx.BASS_SFX_GetVersion
                    Configuration.SetStatus("All Good, " & Configuration.Status, 100)
                Else
                    Dim err = Un4seen.Bass.AddOn.Sfx.BassSfx.BASS_SFX_ErrorGetCode
                    If err = Un4seen.Bass.AddOn.Sfx.BASSSFXError.BASS_SFX_ERROR_ALREADY Then _IsEnabled = True : Return
                    _IsEnabled = False
                    Configuration.SetError(True, New InvalidOperationException("Couldn't find a valid window"))
                End If
                OnPropertyChanged(NameOf(IsEnabled))
            End Sub
            Public Overrides Sub Free()
                Un4seen.Bass.AddOn.Sfx.BassSfx.BASS_SFX_Free()
                Utilities.SharedProperties.Instance.ItemsConfiguration.Remove(Configuration)
            End Sub
        End Class

        Public Class BASS_FLAC
            Inherits EngineModule

            Public Overrides ReadOnly Property Name As String = "BASS FLAC"

            Public Overrides ReadOnly Property Description As String = "An extension enabling the playback of FLAC (including Ogg FLAC) encoded files and streams."

            Public Overrides ReadOnly Property Category As String = "Decoder"

            Private _Info As String
            Public Overrides Property Info As String
                Get
                    Return _Info
                End Get
                Set(value As String)
                    _Info = value
                    OnPropertyChanged()
                End Set
            End Property

            Private _IsEnabled As Boolean
            Public Overrides Property IsEnabled As Boolean
                Get
                    Return _IsEnabled
                End Get
                Set(value As Boolean)
                    OnPropertyChanged()
                End Set
            End Property

            Public Overrides Property Configuration As New Classes.StartupItemConfiguration("BASS FLAC")

            Private _Handle As Integer = 0
            Public Overrides Property Handle As Integer
                Get
                    Return _Handle
                End Get
                Set(value As Integer)
                    _Handle = value
                    OnPropertyChanged()
                End Set
            End Property

            Private _Path As String
            Public Overrides Property Path As String
                Get
                    Return _Path
                End Get
                Set(value As String)
                    _Path = value
                    OnPropertyChanged()
                End Set
            End Property

            Public Overrides Sub Init()
                If _IsEnabled Then Return

                Utilities.SharedProperties.Instance?.ItemsConfiguration.Add(Configuration)
                Path = IO.Path.Combine(My.Application.Info.DirectoryPath, "bassflac.dll")
                Handle = Un4seen.Bass.Bass.BASS_PluginLoad(Path)

                _IsEnabled = True
                OnPropertyChanged(NameOf(IsEnabled))

                Info = $"Handle: {Handle}"
                Configuration.SetStatus("Loaded: " & Info, 100)
            End Sub
            Public Overrides Sub Free()
                Utilities.SharedProperties.Instance.ItemsConfiguration.Remove(Configuration)
            End Sub
        End Class

        Public Class BASS_FX
            Inherits EngineModule

            Public Overrides ReadOnly Property Name As String = "BASS FX"

            Public Overrides ReadOnly Property Description As String = "An extension providing several effects, including reverse playback and tempo & pitch control."

            Public Overrides ReadOnly Property Category As String = "Audio Effects"

            Private _Info As String
            Public Overrides Property Info As String
                Get
                    Return _Info
                End Get
                Set(value As String)
                    _Info = value
                    OnPropertyChanged()
                End Set
            End Property

            Private _IsEnabled As Boolean
            Public Overrides Property IsEnabled As Boolean
                Get
                    Return _IsEnabled
                End Get
                Set(value As Boolean)
                    OnPropertyChanged()
                End Set
            End Property

            Public Overrides Property Configuration As New Classes.StartupItemConfiguration("BASS FX")

            Private _Handle As Integer = 0
            Public Overrides Property Handle As Integer
                Get
                    Return _Handle
                End Get
                Set(value As Integer)
                    _Handle = value
                    OnPropertyChanged()
                End Set
            End Property

            Private _Path As String
            Public Overrides Property Path As String
                Get
                    Return _Path
                End Get
                Set(value As String)
                    _Path = value
                    OnPropertyChanged()
                End Set
            End Property

            Public Overrides Sub Init()
                If _IsEnabled Then Return

                Utilities.SharedProperties.Instance?.ItemsConfiguration.Add(Configuration)
                Configuration.SetStatus("Loading...", 0)
                Path = IO.Path.Combine(My.Application.Info.DirectoryPath, "bass_fx.dll")
                Handle = Un4seen.Bass.Bass.BASS_PluginLoad(Path)
                Configuration.SetStatus("Loaded", 100)
                _IsEnabled = True
                OnPropertyChanged(NameOf(IsEnabled))

                Info = $"Handle: {Un4seen.Bass.AddOn.Fx.BassFx.BASS_FX_GetVersion}"
            End Sub

            Public Overrides Sub Free()
                Utilities.SharedProperties.Instance.ItemsConfiguration.Remove(Configuration)
            End Sub
        End Class

        Public Class AllDecoders
            Inherits EngineModule

            Public Overrides ReadOnly Property Name As String = "All Decoders"

            Public Overrides ReadOnly Property Description As String = "Automatically loads all available decoders."

            Public Overrides ReadOnly Property Category As String = "Decoder"

            Private _Info As String
            Public Overrides Property Info As String
                Get
                    Return _Info
                End Get
                Set(value As String)
                    _Info = value
                    OnPropertyChanged()
                End Set
            End Property

            Private _IsEnabled As Boolean
            Public Overrides Property IsEnabled As Boolean
                Get
                    Return _IsEnabled
                End Get
                Set(value As Boolean)
                    OnPropertyChanged()
                End Set
            End Property

            Public Overrides Property Configuration As New Classes.StartupItemConfiguration("All Decoders")

            Private _Handles As New List(Of Integer)

            Private _Handle As Integer = 0
            Public Overrides Property Handle As Integer
                Get
                    Return _Handle
                End Get
                Set(value As Integer)
                    _Handle = value
                    OnPropertyChanged()
                End Set
            End Property

            Private _Path As String
            Public Overrides Property Path As String
                Get
                    Return _Path
                End Get
                Set(value As String)
                    _Path = value
                    OnPropertyChanged()
                End Set
            End Property

            Private _LoadedModules As New List(Of String)

            Public Overrides Sub Init()
                If _IsEnabled Then Return

                Path = "Mutliple"
                Handle = 7721

                Dim PLUG_FLAC = Un4seen.Bass.Bass.BASS_PluginLoad(IO.Path.Combine(My.Application.Info.DirectoryPath, "bassflac.dll"))
                Dim PLUG_MIDI = Un4seen.Bass.Bass.BASS_PluginLoad(IO.Path.Combine(My.Application.Info.DirectoryPath, "bassmidi.dll"))
                '
                Dim PLUG_WavPack = Un4seen.Bass.Bass.BASS_PluginLoad(IO.Path.Combine(My.Application.Info.DirectoryPath, "basswv.dll"))
                Dim PLUG_Opus = Un4seen.Bass.Bass.BASS_PluginLoad(IO.Path.Combine(My.Application.Info.DirectoryPath, "bassopus.dll"))
                Dim PLUG_DirectStreamDigital = Un4seen.Bass.Bass.BASS_PluginLoad(IO.Path.Combine(My.Application.Info.DirectoryPath, "bassdsd.dll"))
                Dim PLUG_ALAC = Un4seen.Bass.Bass.BASS_PluginLoad(IO.Path.Combine(My.Application.Info.DirectoryPath, "bassalac.dll"))
                Dim PLUG_WebM = Un4seen.Bass.Bass.BASS_PluginLoad(IO.Path.Combine(My.Application.Info.DirectoryPath, "basswebm.dll"))
                Dim PLUG_HLS = Un4seen.Bass.Bass.BASS_PluginLoad(IO.Path.Combine(My.Application.Info.DirectoryPath, "basshls.dll"))
                Dim PLUG_APE = Un4seen.Bass.Bass.BASS_PluginLoad(IO.Path.Combine(My.Application.Info.DirectoryPath, "bassape.dll"))
                Dim PLUG_Musepack = Un4seen.Bass.Bass.BASS_PluginLoad(IO.Path.Combine(My.Application.Info.DirectoryPath, "bass_mpc.dll"))
                Dim PLUG_SSL = Un4seen.Bass.Bass.BASS_PluginLoad(IO.Path.Combine(My.Application.Info.DirectoryPath, "bass_ssl.dll"))
                Dim PLUG_TTA = Un4seen.Bass.Bass.BASS_PluginLoad(IO.Path.Combine(My.Application.Info.DirectoryPath, "bass_tta.dll"))
                Dim PLUG_Speex = Un4seen.Bass.Bass.BASS_PluginLoad(IO.Path.Combine(My.Application.Info.DirectoryPath, "bass_spx.dll"))
                Dim PLUG_AAC = Un4seen.Bass.Bass.BASS_PluginLoad(IO.Path.Combine(My.Application.Info.DirectoryPath, "bass_aac.dll"))
                Dim PLUG_AC3 = Un4seen.Bass.Bass.BASS_PluginLoad(IO.Path.Combine(My.Application.Info.DirectoryPath, "bass_ac3.dll"))
                Dim PLUG_OptimFROG = Un4seen.Bass.Bass.BASS_PluginLoad(IO.Path.Combine(My.Application.Info.DirectoryPath, "bass_ofr.dll"))
                Dim PLUG_ZXTUNE = Un4seen.Bass.Bass.BASS_PluginLoad(IO.Path.Combine(My.Application.Info.DirectoryPath, "basszxtune.dll"))
                Dim PLUG_ADX = Un4seen.Bass.Bass.BASS_PluginLoad(IO.Path.Combine(My.Application.Info.DirectoryPath, "bass_adx.dll"))
                Dim PLUG_AIX = Un4seen.Bass.Bass.BASS_PluginLoad(IO.Path.Combine(My.Application.Info.DirectoryPath, "bass_aix.dll"))
                '
                Dim PLUG_Count As Integer = 0
                If PLUG_FLAC <> 0 Then PLUG_Count += 1 : _LoadedModules.Add("FLAC") : _Handles.Add(PLUG_FLAC)
                If PLUG_MIDI <> 0 Then PLUG_Count += 1 : _LoadedModules.Add("MIDI") : _Handles.Add(PLUG_MIDI)
                '
                If PLUG_WavPack <> 0 Then PLUG_Count += 1 : _LoadedModules.Add("Wav Pack") : _Handles.Add(PLUG_WavPack)
                If PLUG_Opus <> 0 Then PLUG_Count += 1 : _LoadedModules.Add("Opus") : _Handles.Add(PLUG_Opus)
                If PLUG_DirectStreamDigital <> 0 Then PLUG_Count += 1 : _LoadedModules.Add("Direct Stream Digital") : _Handles.Add(PLUG_DirectStreamDigital)
                If PLUG_ALAC <> 0 Then PLUG_Count += 1 : _LoadedModules.Add("Apple Lossless Audio Codec (ALAC)") : _Handles.Add(PLUG_ALAC)
                If PLUG_WebM <> 0 Then PLUG_Count += 1 : _LoadedModules.Add("WebM") : _Handles.Add(PLUG_WebM)
                If PLUG_HLS <> 0 Then PLUG_Count += 1 : _LoadedModules.Add("HLS") : _Handles.Add(PLUG_HLS)
                If PLUG_APE <> 0 Then PLUG_Count += 1 : _LoadedModules.Add("Ape") : _Handles.Add(PLUG_APE)
                If PLUG_Musepack <> 0 Then PLUG_Count += 1 : _LoadedModules.Add("Musepack") : _Handles.Add(PLUG_Musepack)
                If PLUG_SSL <> 0 Then PLUG_Count += 1 : _LoadedModules.Add("SSL") : _Handles.Add(PLUG_SSL)
                If PLUG_TTA <> 0 Then PLUG_Count += 1 : _LoadedModules.Add("TTA") : _Handles.Add(PLUG_TTA)
                If PLUG_Speex <> 0 Then PLUG_Count += 1 : _LoadedModules.Add("Speex") : _Handles.Add(PLUG_Speex)
                If PLUG_AAC <> 0 Then PLUG_Count += 1 : _LoadedModules.Add("AAC") : _Handles.Add(PLUG_AAC)
                If PLUG_AC3 <> 0 Then PLUG_Count += 1 : _LoadedModules.Add("AC3") : _Handles.Add(PLUG_AC3)
                If PLUG_OptimFROG <> 0 Then PLUG_Count += 1 : _LoadedModules.Add("OptimFROG") : _Handles.Add(PLUG_OptimFROG)
                If PLUG_ZXTUNE <> 0 Then PLUG_Count += 1 : _LoadedModules.Add("ZXTune (Chiptune/Tracker)") : _Handles.Add(PLUG_ZXTUNE)
                If PLUG_ADX <> 0 Then PLUG_Count += 1 : _LoadedModules.Add("ADX") : _Handles.Add(PLUG_ADX)
                If PLUG_AIX <> 0 Then PLUG_Count += 1 : _LoadedModules.Add("AIX") : _Handles.Add(PLUG_AIX)
                '                                
                _IsEnabled = True
                OnPropertyChanged(NameOf(IsEnabled))

                Info = $"Loaded {PLUG_Count} Module(s)"
                Utilities.SharedProperties.Instance?.ItemsConfiguration.Add(Configuration)
                Configuration.SetStatus(Info, 100)
            End Sub

            Public Overrides Sub Free()
                Utilities.SharedProperties.Instance.ItemsConfiguration.Remove(Configuration)
            End Sub

            Public Overrides Sub Config()
                Dim sb As New Text.StringBuilder
                sb.AppendLine($"Loaded {_LoadedModules.Count} Module(s)")
                For Each plug In _LoadedModules
                    sb.AppendLine("Name: " & IO.Path.GetFileNameWithoutExtension(plug))
                    Dim pInfo = Un4seen.Bass.Bass.BASS_PluginGetInfo(Handle)
                    If pInfo Is Nothing Then Continue For
                    sb.AppendLine("Version: " & pInfo.version)
                    sb.AppendLine("Supported Formats:")
                    For Each ext In pInfo.formats
                        sb.AppendLine("Format Name: " & ext.name)
                        sb.AppendLine("Format Extensions: " & ext.exts)
                    Next
                Next
                HandyControl.Controls.MessageBox.Show(Application.Current.MainWindow, sb.ToString, icon:=MessageBoxImage.Information)
            End Sub
        End Class


        Public Class Custom
            Inherits EngineModule

            Private _Name As String = "Custom Module"
            Public Overrides ReadOnly Property Name As String
                Get
                    Return _Name
                End Get
            End Property

            Public Overrides ReadOnly Property Description As String = "Load a Custom BASS Plugin."

            Public Overrides ReadOnly Property Category As String = "Helper"

            Private _Info As String
            Public Overrides Property Info As String
                Get
                    Return _Info
                End Get
                Set(value As String)
                    _Info = value
                    OnPropertyChanged()
                End Set
            End Property

            Private _IsEnabled As Boolean
            Public Overrides Property IsEnabled As Boolean
                Get
                    Return _IsEnabled
                End Get
                Set(value As Boolean)
                    OnPropertyChanged()
                End Set
            End Property

            Public Overrides Property Configuration As New Classes.StartupItemConfiguration(Name)

            Private _Handle As Integer = 0
            Public Overrides Property Handle As Integer
                Get
                    Return _Handle
                End Get
                Set(value As Integer)
                    _Handle = value
                    OnPropertyChanged()
                End Set
            End Property

            Private _Path As String
            Public Overrides Property Path As String
                Get
                    Return _Path
                End Get
                Set(value As String)
                    _Path = value
                    OnPropertyChanged()
                End Set
            End Property

            Public Overrides Sub Init()
                If _IsEnabled Then Return
                Path = SavedInfo
                If String.IsNullOrWhiteSpace(Path) Then
                    Dim ofd As New Microsoft.Win32.OpenFileDialog With {.Filter = "Dynamic-link library|*.dll"}
                    If Not ofd.ShowDialog Then _Name = "Unconfigured" : Path = Nothing : Return
                    Path = ofd.FileName
                    MyBase.SavedInfo = Path
                End If

                _Name = IO.Path.GetFileNameWithoutExtension(Path)
                Configuration.Name = Name

                Utilities.SharedProperties.Instance?.ItemsConfiguration.Add(Configuration)

                Handle = Un4seen.Bass.Bass.BASS_PluginLoad(Path)
                Dim pInfo = Un4seen.Bass.Bass.BASS_PluginGetInfo(Handle)
                Dim IsInit = Handle <> 0

                _IsEnabled = IsInit
                OnPropertyChanged(NameOf(IsEnabled))
                Info = $"Handle: {Handle}{Environment.NewLine}Version: {pInfo.version}"
                Configuration.SetStatus($"{If(IsInit, "Loaded", "Error")}: {Info}", If(IsInit, 100, 0))
                If Not IsInit Then Configuration.SetError(True, New InvalidOperationException("Unknown error occured"))
            End Sub

            Public Overrides Sub Free()
                BassWaDsp.BASS_WADSP_Free()
                Utilities.SharedProperties.Instance.ItemsConfiguration.Remove(Configuration)
            End Sub

            Public Overrides Sub Config()
                Dim pInfo = Un4seen.Bass.Bass.BASS_PluginGetInfo(Handle)
                HandyControl.Controls.MessageBox.Show(Application.Current.MainWindow, $"Name: {Name}{Environment.NewLine}Version: {pInfo.version}{Environment.NewLine}Supported Formats:{Environment.NewLine}{String.Join(Environment.NewLine, pInfo.formats.Select(Of String)(Function(k) k.name & $"[{k.ctype.ToString}]: {k.exts}"))}", icon:=MessageBoxImage.Information)
            End Sub
        End Class
    End Class
End Namespace
