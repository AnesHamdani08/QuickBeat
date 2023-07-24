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

            Public Overrides Sub Init()
                If _IsEnabled Then Return

                Dim hDSP = Un4seen.Bass.Bass.BASS_PluginLoad(IO.Path.Combine(My.Application.Info.DirectoryPath, "bass_wadsp.dll"))
                Dim vDSP = BassWaDsp.BASS_WADSP_GetVersion()
                Dim IsInit = BassWaDsp.BASS_WADSP_Init(IntPtr.Zero)

                _IsEnabled = IsInit
                OnPropertyChanged(NameOf(IsEnabled))

                Info = $"Handle: {hDSP}{Environment.NewLine}Version: {vDSP}"
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

            Public Overrides Sub Init()
                If IsEnabled Then Return
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

            Public Overrides Sub Init()
                If _IsEnabled Then Return

                Dim hFLAC = Un4seen.Bass.Bass.BASS_PluginLoad(IO.Path.Combine(My.Application.Info.DirectoryPath, "bassflac.dll"))

                _IsEnabled = True
                OnPropertyChanged(NameOf(IsEnabled))

                Info = $"Handle: {hFLAC}"
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

            Public Overrides Sub Init()
                If _IsEnabled Then Return

                Dim PLUG_FLAC = Un4seen.Bass.Bass.BASS_PluginLoad(IO.Path.Combine(My.Application.Info.DirectoryPath, "bassflac.dll"))
                Dim PLUG_FX = Un4seen.Bass.Bass.BASS_PluginLoad(IO.Path.Combine(My.Application.Info.DirectoryPath, "bass_fx.dll"))
                Dim PLUG_MIDI = Un4seen.Bass.Bass.BASS_PluginLoad(IO.Path.Combine(My.Application.Info.DirectoryPath, "bassmidi.dll"))
                '
                Dim PLUG_WavPack = Un4seen.Bass.Bass.BASS_PluginLoad(IO.Path.Combine(My.Application.Info.DirectoryPath, "basswv.dll"))
                Dim PLUG_Opus = Un4seen.Bass.Bass.BASS_PluginLoad(IO.Path.Combine(My.Application.Info.DirectoryPath, "bassopus.dll"))
                Dim PLUG_DirectStreamDigital = Un4seen.Bass.Bass.BASS_PluginLoad(IO.Path.Combine(My.Application.Info.DirectoryPath, "bassdsd.dll"))
                Dim PLUG_ALAC = Un4seen.Bass.Bass.BASS_PluginLoad(IO.Path.Combine(My.Application.Info.DirectoryPath, "bassalac.dll"))
                Dim PLUG_WebM = Un4seen.Bass.Bass.BASS_PluginLoad(IO.Path.Combine(My.Application.Info.DirectoryPath, "basswebm.dll"))
                Dim PLUG_APE = Un4seen.Bass.Bass.BASS_PluginLoad(IO.Path.Combine(My.Application.Info.DirectoryPath, "bassape.dll"))
                Dim PLUG_Musepack = Un4seen.Bass.Bass.BASS_PluginLoad(IO.Path.Combine(My.Application.Info.DirectoryPath, "bass_mpc.dll"))
                Dim PLUG_TTA = Un4seen.Bass.Bass.BASS_PluginLoad(IO.Path.Combine(My.Application.Info.DirectoryPath, "bass_tta.dll"))
                Dim PLUG_Speex = Un4seen.Bass.Bass.BASS_PluginLoad(IO.Path.Combine(My.Application.Info.DirectoryPath, "bass_spx.dll"))
                Dim PLUG_AC3 = Un4seen.Bass.Bass.BASS_PluginLoad(IO.Path.Combine(My.Application.Info.DirectoryPath, "bass_ac3.dll"))
                Dim PLUG_OptimFROG = Un4seen.Bass.Bass.BASS_PluginLoad(IO.Path.Combine(My.Application.Info.DirectoryPath, "bass_ofr.dll"))
                Dim PLUG_ADX = Un4seen.Bass.Bass.BASS_PluginLoad(IO.Path.Combine(My.Application.Info.DirectoryPath, "bass_adx.dll"))
                Dim PLUG_AIX = Un4seen.Bass.Bass.BASS_PluginLoad(IO.Path.Combine(My.Application.Info.DirectoryPath, "bass_aix.dll"))
                '
                Try
                    PLUG_FX = Un4seen.Bass.AddOn.Fx.BassFx.BASS_FX_GetVersion
                Catch
                    PLUG_FX = 0
                End Try
                '
                Dim PLUG_Count As Integer = 0
                If PLUG_FLAC <> 0 Then PLUG_Count += 1
                If PLUG_MIDI <> 0 Then PLUG_Count += 1
                '
                If PLUG_WavPack <> 0 Then PLUG_Count += 1
                If PLUG_Opus <> 0 Then PLUG_Count += 1
                If PLUG_DirectStreamDigital <> 0 Then PLUG_Count += 1
                If PLUG_ALAC <> 0 Then PLUG_Count += 1
                If PLUG_WebM <> 0 Then PLUG_Count += 1
                If PLUG_APE <> 0 Then PLUG_Count += 1
                If PLUG_Musepack <> 0 Then PLUG_Count += 1
                If PLUG_TTA <> 0 Then PLUG_Count += 1
                If PLUG_Speex <> 0 Then PLUG_Count += 1
                If PLUG_AC3 <> 0 Then PLUG_Count += 1
                If PLUG_OptimFROG <> 0 Then PLUG_Count += 1
                If PLUG_ADX <> 0 Then PLUG_Count += 1
                If PLUG_AIX <> 0 Then PLUG_Count += 1
                '
                If PLUG_FX <> 0 Then PLUG_Count += 1
                '                

                _IsEnabled = True
                OnPropertyChanged(NameOf(IsEnabled))

                Info = $"Loaded {PLUG_Count} Module(s)"
            End Sub
        End Class
    End Class
End Namespace
