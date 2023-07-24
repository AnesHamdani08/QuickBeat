Imports System.Dynamic
Imports QuickBeat.Player.Profile.AudioEffect
Imports System.Windows.Media.Effects
Imports TagLib.Ape
Imports Un4seen.Bass
Imports Un4seen.Bass.Misc
Imports Un4seen.Bass.AddOn.WaDsp
Imports Windows
Imports Windows.Perception.Spatial.Preview
Imports Windows.UI.Xaml.Shapes

Namespace Player
    Public Class AudioEffects

        'Snippet
        'Public Class Name
        '    Inherits Profile.AudioEffect

        '    Public Overrides Property Name As String = ""

        '    Public Overrides Property Description As String = ""

        '    Public Overrides Property ShortName As String = ""

        '    Public Overrides Property Category As String = ""

        '    Private _IsEnabled As Boolean

        '    Public Overrides Property IsEnabled As Boolean
        '        Get
        '            Return _IsEnabled
        '        End Get
        '        Set(value As Boolean)
        '            If value = _IsEnabled Then Return
        '            If value Then
        '                HEffect = Bass.BASS_ChannelSetFX(HStream, BASSFXType.BASS_FX_, 0)
        '                _IsEnabled = Bass.BASS_FXSetParameters(HEffect, FXItem)
        '            Else
        '                _IsEnabled = Not Bass.BASS_ChannelRemoveFX(HStream, HEffect)
        '            End If
        '            OnPropertyChanged()
        '        End Set
        '    End Property

        '    Private _HEffect As Integer
        '    Public Overrides Property HEffect As Integer
        '        Get
        '            Return _HEffect
        '        End Get
        '        Set(value As Integer)
        '            _HEffect = value
        '            OnPropertyChanged()
        '        End Set
        '    End Property

        '    Private _HStream As Integer
        '    Public Overrides Property HStream As Integer
        '        Get
        '            Return _HStream
        '        End Get
        '        Set(value As Integer)
        '            _HStream = value
        '            OnPropertyChanged()
        '        End Set
        '    End Property

        '<Field("", GetType())>
        'Public Property   As  
        '    Get
        '        Return FXItem. 
        '    End Get
        '    Set(value As  )
        '        FXItem.  = value
        '        If IsEnabled Then
        '            If Not Bass.BASS_FXSetParameters(HEffect, FXItem) Then
        '                Threading.Thread.Sleep(100)
        '                Bass.BASS_FXSetParameters(HEffect, FXItem)
        '            End If
        '        End If
        '    End Set
        'End Property

        '<MethodGroup(DisplayName:="")>
        'Property Group0

        'Private FXItem As New Un4seen.Bass.

        'Sub New()
        '    FXItem.Preset_Default()
        'End Sub

        'Public Overrides Sub Apply()
        '    HEffect = Bass.BASS_ChannelSetFX(HStream, BASSFXType.BASS_FX_, 0)
        '    _IsEnabled = Bass.BASS_FXSetParameters(HEffect, FXItem)
        '    OnPropertyChanged(NameOf(IsEnabled))
        'End Sub

        'Private Sub NotifyUI()
        '    OnPropertyChanged(NameOf( ))
        'End Sub

        'Sub Update()
        '    If IsEnabled Then
        '        Bass.BASS_FXSetParameters(HEffect, FXItem)
        '    End If
        'End Sub

        '<Method("Default", "Sets the instance members to a preset.", Group:="Group0")>
        'Sub Preset_Default()
        '    FXItem.Preset_Default()
        '    Update()
        'End Sub
        'End Class

        <Serializable>
        Public Class Chorus
            Inherits Profile.AudioEffect

            Public Overrides Property Name As String = "Chorus"

            Public Overrides Property Description As String = "Audio effect that occurs when individual sounds with approximately the same time, and very similar pitches, converge."

            Public Overrides Property ShortName As String = "CRS"

            Public Overrides Property Category As String = "DSP"

            Private _IsEnabled As Boolean
            Public Overrides Property IsEnabled As Boolean
                Get
                    Return _IsEnabled
                End Get
                Set(value As Boolean)
                    If value = _IsEnabled Then Return
                    If value Then
                        HEffect = Bass.BASS_ChannelSetFX(HStream, BASSFXType.BASS_FX_DX8_CHORUS, 0)
                        _IsEnabled = Bass.BASS_FXSetParameters(HEffect, FXItem)
                    Else
                        _IsEnabled = Not Bass.BASS_ChannelRemoveFX(HStream, HEffect)
                    End If
                    OnPropertyChanged()
                End Set
            End Property

            Private _HEffect As Integer
            Public Overrides Property HEffect As Integer
                Get
                    Return _HEffect
                End Get
                Set(value As Integer)
                    _HEffect = value
                    OnPropertyChanged()
                End Set
            End Property

            Private _HStream As Integer
            Public Overrides Property HStream As Integer
                Get
                    Return _HStream
                End Get
                Set(value As Integer)
                    _HStream = value
                    OnPropertyChanged()
                End Set
            End Property

            <Field("Delay", GetType(Single), Minimum:=0, Maximum:=20, Description:="Number of milliseconds the input is delayed before it is played back.")>
            Public Property Delay As Single
                Get
                    Return FXItem.fDelay
                End Get
                Set(value As Single)
                    FXItem.fDelay = value
                    If IsEnabled Then
                        If Not Bass.BASS_FXSetParameters(HEffect, FXItem) Then
                            Threading.Thread.Sleep(100)
                            Bass.BASS_FXSetParameters(HEffect, FXItem)
                        End If
                        OnPropertyChanged()
                    End If
                End Set
            End Property
            <Field("Depth", GetType(Single), Minimum:=0, Maximum:=100, Description:="Percentage by which the delay time is modulated by the low-frequency oscillator, in hundredths of a percentage point.")>
            Public Property Depth As Single
                Get
                    Return FXItem.fDepth
                End Get
                Set(value As Single)
                    FXItem.fDepth = value
                    If IsEnabled Then
                        If Not Bass.BASS_FXSetParameters(HEffect, FXItem) Then
                            Threading.Thread.Sleep(100)
                            Bass.BASS_FXSetParameters(HEffect, FXItem)
                        End If
                        OnPropertyChanged()
                    End If
                End Set
            End Property
            <Field("Feedback", GetType(Single), Minimum:=-99, Maximum:=99, Description:="Percentage of output signal to feed back into the effect's input.")>
            Public Property Feedback As Single
                Get
                    Return FXItem.fFeedback
                End Get
                Set(value As Single)
                    FXItem.fFeedback = value
                    If IsEnabled Then
                        If Not Bass.BASS_FXSetParameters(HEffect, FXItem) Then
                            Threading.Thread.Sleep(100)
                            Bass.BASS_FXSetParameters(HEffect, FXItem)
                        End If
                        OnPropertyChanged()
                    End If
                End Set
            End Property
            <Field("Frequency", GetType(Single), Minimum:=0, Maximum:=10, Description:="Frequency of the LFO.")>
            Public Property Frequency As Single
                Get
                    Return FXItem.fFrequency
                End Get
                Set(value As Single)
                    FXItem.fFrequency = value
                    If IsEnabled Then
                        If Not Bass.BASS_FXSetParameters(HEffect, FXItem) Then
                            Threading.Thread.Sleep(100)
                            Bass.BASS_FXSetParameters(HEffect, FXItem)
                        End If
                        OnPropertyChanged()
                    End If
                End Set
            End Property
            <Field("Wet/Dry Mix", GetType(Single), Minimum:=0, Maximum:=100, Description:="Ratio of wet (processed) signal to dry (unprocessed) signal.")>
            Public Property WetDryMix As Single
                Get
                    Return FXItem.fWetDryMix
                End Get
                Set(value As Single)
                    FXItem.fWetDryMix = value
                    If IsEnabled Then
                        If Not Bass.BASS_FXSetParameters(HEffect, FXItem) Then
                            Threading.Thread.Sleep(100)
                            Bass.BASS_FXSetParameters(HEffect, FXItem)
                        End If
                        OnPropertyChanged()
                    End If
                End Set
            End Property
            <Field("Phase", GetType(BASSFXPhase), Description:="Phase differential between left and right LFOs.")>
            Public Property Phase As BASSFXPhase
                Get
                    Return FXItem.lPhase
                End Get
                Set(value As BASSFXPhase)
                    FXItem.lPhase = value
                    If IsEnabled Then
                        If Not Bass.BASS_FXSetParameters(HEffect, FXItem) Then
                            Threading.Thread.Sleep(100)
                            Bass.BASS_FXSetParameters(HEffect, FXItem)
                        End If
                        OnPropertyChanged()
                    End If
                End Set
            End Property
            <Field("Waveform", GetType(Integer), Minimum:=0, Maximum:=1, Description:="Waveform of the LFO... 0 = triangle, 1 = sine.")>
            Public Property Waveform As Integer
                Get
                    Return FXItem.lWaveform
                End Get
                Set(value As Integer)
                    FXItem.lWaveform = value
                    If IsEnabled Then
                        If Not Bass.BASS_FXSetParameters(HEffect, FXItem) Then
                            Threading.Thread.Sleep(100)
                            Bass.BASS_FXSetParameters(HEffect, FXItem)
                        End If
                        OnPropertyChanged()
                    End If
                End Set
            End Property

            <MethodGroup(DisplayName:="Presets", GridStackWrap:=False, HorizontalItemSpacing:=10, VerticalItemSpacing:=10, Orientation:=Orientation.Horizontal)>
            Property Group0

            Private FXItem As New Un4seen.Bass.BASS_DX8_CHORUS

            Sub New()
                FXItem.Preset_Default()
            End Sub

            Public Overrides Sub Apply()
                HEffect = Bass.BASS_ChannelSetFX(HStream, BASSFXType.BASS_FX_DX8_CHORUS, 0)
                _IsEnabled = Bass.BASS_FXSetParameters(HEffect, FXItem)
                OnPropertyChanged(NameOf(IsEnabled))
                NotifyUI()
            End Sub

            Private Sub NotifyUI()
                OnPropertyChanged(NameOf(Delay))
                OnPropertyChanged(NameOf(Depth))
                OnPropertyChanged(NameOf(Feedback))
                OnPropertyChanged(NameOf(Frequency))
                OnPropertyChanged(NameOf(WetDryMix))
                OnPropertyChanged(NameOf(Phase))
                OnPropertyChanged(NameOf(Waveform))
            End Sub

            Sub Update()
                If IsEnabled Then
                    If Bass.BASS_FXSetParameters(HEffect, FXItem) Then
                        NotifyUI()
                    End If
                End If
            End Sub

            <Method("Default", "Sets the instance members to a preset.", Group:="Presets")>
            Sub Preset_Default()
                FXItem.Preset_Default()
                Update()
            End Sub
            <Method("A", "Sets the instance members to a preset.", Group:="Presets")>
            Sub Preset_A()
                FXItem.Preset_A()
                Update()
            End Sub
            <Method("B", "Sets the instance members to a preset.", Group:="Presets")>
            Sub Preset_B()
                FXItem.Preset_B()
                Update()
            End Sub
        End Class

        <Serializable>
        Public Class Flanger
            Inherits Profile.AudioEffect

            Public Overrides Property Name As String = "Flanger"

            Public Overrides Property Description As String = "Flanging is an audio effect produced by mixing two identical signals together, one signal delayed by a small and gradually changing period, usually smaller than 20 milliseconds."

            Public Overrides Property ShortName As String = "FLR"

            Public Overrides Property Category As String = "DSP"

            Private _IsEnabled As Boolean
            Public Overrides Property IsEnabled As Boolean
                Get
                    Return _IsEnabled
                End Get
                Set(value As Boolean)
                    If value = _IsEnabled Then Return
                    If value Then
                        HEffect = Bass.BASS_ChannelSetFX(HStream, BASSFXType.BASS_FX_DX8_FLANGER, 0)
                        _IsEnabled = Bass.BASS_FXSetParameters(HEffect, FXItem)
                    Else
                        _IsEnabled = Not Bass.BASS_ChannelRemoveFX(HStream, HEffect)
                    End If
                    OnPropertyChanged()
                End Set
            End Property

            Private _HEffect As Integer
            Public Overrides Property HEffect As Integer
                Get
                    Return _HEffect
                End Get
                Set(value As Integer)
                    _HEffect = value
                    OnPropertyChanged()
                End Set
            End Property

            Private _HStream As Integer
            Public Overrides Property HStream As Integer
                Get
                    Return _HStream
                End Get
                Set(value As Integer)
                    _HStream = value
                    OnPropertyChanged()
                End Set
            End Property

            <Field("Delay", GetType(Single), Minimum:=0, Maximum:=4, Description:="Number of milliseconds the input is delayed before it is played back.")>
            Public Property Delay As Single
                Get
                    Return FXItem.fDelay
                End Get
                Set(value As Single)
                    FXItem.fDelay = value
                    If IsEnabled Then
                        If Not Bass.BASS_FXSetParameters(HEffect, FXItem) Then
                            Threading.Thread.Sleep(100)
                            Bass.BASS_FXSetParameters(HEffect, FXItem)
                        End If
                        OnPropertyChanged()
                    End If
                End Set
            End Property
            <Field("Depth", GetType(Single), Minimum:=0, Maximum:=100, Description:="Percentage by which the delay time is modulated by the low-frequency oscillator, in hundredths of a percentage point.")>
            Public Property Depth As Single
                Get
                    Return FXItem.fDepth
                End Get
                Set(value As Single)
                    FXItem.fDepth = value
                    If IsEnabled Then
                        If Not Bass.BASS_FXSetParameters(HEffect, FXItem) Then
                            Threading.Thread.Sleep(100)
                            Bass.BASS_FXSetParameters(HEffect, FXItem)
                        End If
                        OnPropertyChanged()
                    End If
                End Set
            End Property
            <Field("Feedback", GetType(Single), Minimum:=-99, Maximum:=99, Description:="Percentage of output signal to feed back into the effect's input.")>
            Public Property Feedback As Single
                Get
                    Return FXItem.fFeedback
                End Get
                Set(value As Single)
                    FXItem.fFeedback = value
                    If IsEnabled Then
                        If Not Bass.BASS_FXSetParameters(HEffect, FXItem) Then
                            Threading.Thread.Sleep(100)
                            Bass.BASS_FXSetParameters(HEffect, FXItem)
                        End If
                        OnPropertyChanged()
                    End If
                End Set
            End Property
            <Field("Frequency", GetType(Single), Minimum:=0, Maximum:=10, Description:="Frequency of the LFO.")>
            Public Property Frequency As Single
                Get
                    Return FXItem.fFrequency
                End Get
                Set(value As Single)
                    FXItem.fFrequency = value
                    If IsEnabled Then
                        If Not Bass.BASS_FXSetParameters(HEffect, FXItem) Then
                            Threading.Thread.Sleep(100)
                            Bass.BASS_FXSetParameters(HEffect, FXItem)
                        End If
                        OnPropertyChanged()
                    End If
                End Set
            End Property
            <Field("Wet/Dry Mix", GetType(Single), Minimum:=0, Maximum:=100, Description:="Ratio of wet (processed) signal to dry (unprocessed) signal.")>
            Public Property WetDryMix As Single
                Get
                    Return FXItem.fWetDryMix
                End Get
                Set(value As Single)
                    FXItem.fWetDryMix = value
                    If IsEnabled Then
                        If Not Bass.BASS_FXSetParameters(HEffect, FXItem) Then
                            Threading.Thread.Sleep(100)
                            Bass.BASS_FXSetParameters(HEffect, FXItem)
                        End If
                        OnPropertyChanged()
                    End If
                End Set
            End Property
            <Field("Phase", GetType(BASSFXPhase), Description:="Phase differential between left and right LFOs.")>
            Public Property Phase As BASSFXPhase
                Get
                    Return FXItem.lPhase
                End Get
                Set(value As BASSFXPhase)
                    FXItem.lPhase = value
                    If IsEnabled Then
                        If Not Bass.BASS_FXSetParameters(HEffect, FXItem) Then
                            Threading.Thread.Sleep(100)
                            Bass.BASS_FXSetParameters(HEffect, FXItem)
                        End If
                        OnPropertyChanged()
                    End If
                End Set
            End Property
            <Field("Waveform", GetType(Integer), Minimum:=0, Maximum:=1, Description:="Waveform of the LFO... 0 = triangle, 1 = sine.")>
            Public Property Waveform As Integer
                Get
                    Return FXItem.lWaveform
                End Get
                Set(value As Integer)
                    FXItem.lWaveform = value
                    If IsEnabled Then
                        If Not Bass.BASS_FXSetParameters(HEffect, FXItem) Then
                            Threading.Thread.Sleep(100)
                            Bass.BASS_FXSetParameters(HEffect, FXItem)
                        End If
                        OnPropertyChanged()
                    End If
                End Set
            End Property

            <MethodGroup(DisplayName:="Presets", GridStackWrap:=False, HorizontalItemSpacing:=10, VerticalItemSpacing:=10, Orientation:=Orientation.Horizontal)>
            Property Group0

            Private FXItem As New Un4seen.Bass.BASS_DX8_FLANGER

            Sub New()
                FXItem.Preset_Default()
            End Sub

            Public Overrides Sub Apply()
                HEffect = Bass.BASS_ChannelSetFX(HStream, BASSFXType.BASS_FX_DX8_FLANGER, 0)
                _IsEnabled = Bass.BASS_FXSetParameters(HEffect, FXItem)
                OnPropertyChanged(NameOf(IsEnabled))
                NotifyUI()
            End Sub

            Private Sub NotifyUI()
                OnPropertyChanged(NameOf(Delay))
                OnPropertyChanged(NameOf(Depth))
                OnPropertyChanged(NameOf(Feedback))
                OnPropertyChanged(NameOf(Frequency))
                OnPropertyChanged(NameOf(WetDryMix))
                OnPropertyChanged(NameOf(Phase))
                OnPropertyChanged(NameOf(Waveform))
            End Sub

            Sub Update()
                If IsEnabled Then
                    If Bass.BASS_FXSetParameters(HEffect, FXItem) Then
                        NotifyUI()
                    End If
                End If
            End Sub

            <Method("Default", "Sets the instance members to a preset.", Group:="Presets")>
            Sub Preset_Default()
                FXItem.Preset_Default()
                Update()
            End Sub
            <Method("A", "Sets the instance members to a preset.", Group:="Presets")>
            Sub Preset_A()
                FXItem.Preset_A()
                Update()
            End Sub
            <Method("B", "Sets the instance members to a preset.", Group:="Presets")>
            Sub Preset_B()
                FXItem.Preset_B()
                Update()
            End Sub
        End Class

        <Serializable>
        Public Class Echo
            Inherits Profile.AudioEffect

            Public Overrides Property Name As String = "Echo"

            Public Overrides Property Description As String = "Repetition or a partial repetition of a sound due to reflection."

            Public Overrides Property ShortName As String = "ECO"

            Public Overrides Property Category As String = "DSP"

            Private _IsEnabled As Boolean
            Public Overrides Property IsEnabled As Boolean
                Get
                    Return _IsEnabled
                End Get
                Set(value As Boolean)
                    If value = _IsEnabled Then Return
                    If value Then
                        HEffect = Bass.BASS_ChannelSetFX(HStream, BASSFXType.BASS_FX_DX8_ECHO, 0)
                        _IsEnabled = Bass.BASS_FXSetParameters(HEffect, FXItem)
                    Else
                        _IsEnabled = Not Bass.BASS_ChannelRemoveFX(HStream, HEffect)
                    End If
                    OnPropertyChanged()
                End Set
            End Property

            Private _HEffect As Integer
            Public Overrides Property HEffect As Integer
                Get
                    Return _HEffect
                End Get
                Set(value As Integer)
                    _HEffect = value
                    OnPropertyChanged()
                End Set
            End Property

            Private _HStream As Integer
            Public Overrides Property HStream As Integer
                Get
                    Return _HStream
                End Get
                Set(value As Integer)
                    _HStream = value
                    OnPropertyChanged()
                End Set
            End Property

            <Field("Feedback", GetType(Single), Minimum:=0, Maximum:=100, Description:="Percentage of output fed back into input")>
            Property Feedback As Single
                Get
                    Return FXItem.fFeedback
                End Get
                Set(value As Single)
                    FXItem.fFeedback = value
                    If IsEnabled Then
                        If Not Bass.BASS_FXSetParameters(HEffect, FXItem) Then
                            Threading.Thread.Sleep(100)
                            Bass.BASS_FXSetParameters(HEffect, FXItem)
                        End If
                        OnPropertyChanged()
                    End If
                End Set
            End Property

            <Field("Left Delay", GetType(Single), Minimum:=1, Maximum:=2000)>
            Property LeftDelay As Single
                Get
                    Return FXItem.fLeftDelay
                End Get
                Set(value As Single)
                    FXItem.fLeftDelay = value
                    If IsEnabled Then
                        If Not Bass.BASS_FXSetParameters(HEffect, FXItem) Then
                            Threading.Thread.Sleep(100)
                            Bass.BASS_FXSetParameters(HEffect, FXItem)
                        End If
                        OnPropertyChanged()
                    End If
                End Set
            End Property

            <Field("Right Delay", GetType(Single), Minimum:=1, Maximum:=2000)>
            Property RightDelay As Single
                Get
                    Return FXItem.fRightDelay
                End Get
                Set(value As Single)
                    FXItem.fRightDelay = value
                    If IsEnabled Then
                        If Not Bass.BASS_FXSetParameters(HEffect, FXItem) Then
                            Threading.Thread.Sleep(100)
                            Bass.BASS_FXSetParameters(HEffect, FXItem)
                        End If
                        OnPropertyChanged()
                    End If
                End Set
            End Property

            <Field("Wet/Dry Mix", GetType(Single), Minimum:=0, Maximum:=100, Description:="Ratio of wet (processed) signal to dry (unprocessed) signal.")>
            Public Property WetDryMix As Single
                Get
                    Return FXItem.fWetDryMix
                End Get
                Set(value As Single)
                    FXItem.fWetDryMix = value
                    If IsEnabled Then
                        If Not Bass.BASS_FXSetParameters(HEffect, FXItem) Then
                            Threading.Thread.Sleep(100)
                            Bass.BASS_FXSetParameters(HEffect, FXItem)
                        End If
                        OnPropertyChanged()
                    End If
                End Set
            End Property

            <Field("Pan Delay", GetType(Boolean), Description:="Value that specifies whether to swap left and right delays with each successive echo.")>
            Property PanDelay As Boolean
                Get
                    Return FXItem.lPanDelay
                End Get
                Set(value As Boolean)
                    FXItem.lPanDelay = value
                    If IsEnabled Then
                        If Not Bass.BASS_FXSetParameters(HEffect, FXItem) Then
                            Threading.Thread.Sleep(100)
                            Bass.BASS_FXSetParameters(HEffect, FXItem)
                        End If
                        OnPropertyChanged()
                    End If
                End Set
            End Property

            <MethodGroup(DisplayName:="Presets", GridStackWrap:=False, HorizontalItemSpacing:=10, VerticalItemSpacing:=10, Orientation:=Orientation.Horizontal)>
            Property Group0

            Property FXItem As New Un4seen.Bass.BASS_DX8_ECHO

            Sub New()
                FXItem.Preset_Default()
            End Sub

            Public Overrides Sub Apply()
                HEffect = Bass.BASS_ChannelSetFX(HStream, BASSFXType.BASS_FX_DX8_ECHO, 0)
                _IsEnabled = Bass.BASS_FXSetParameters(HEffect, FXItem)
                OnPropertyChanged(NameOf(IsEnabled))
                NotifyUI()
            End Sub


            Private Sub NotifyUI()
                OnPropertyChanged(NameOf(Feedback))
                OnPropertyChanged(NameOf(LeftDelay))
                OnPropertyChanged(NameOf(RightDelay))
                OnPropertyChanged(NameOf(WetDryMix))
                OnPropertyChanged(NameOf(PanDelay))
            End Sub

            Sub Update()
                If IsEnabled Then
                    If Bass.BASS_FXSetParameters(HEffect, FXItem) Then
                        NotifyUI()
                    End If
                End If
            End Sub

            <Method("Default", "Sets the instance members to a preset.", Group:="Presets")>
            Sub Preset_Default()
                FXItem.Preset_Default()
                Update()
            End Sub
            <Method("Long", "Sets the instance members to a preset.", Group:="Presets")>
            Sub Preset_Long()
                FXItem.Preset_Long()
                Update()
            End Sub
            <Method("Small", "Sets the instance members to a preset.", Group:="Presets")>
            Sub Preset_Small()
                FXItem.Preset_Small()
                Update()
            End Sub
        End Class

        <Serializable>
        Public Class I3DL2Reverb
            Inherits Profile.AudioEffect

            Public Overrides Property Name As String = "Interactive 3D Audio Level 2 Reverb"

            Public Overrides Property Description As String = "An audio effect applied to a sound signal to simulate reverberation."

            Public Overrides Property ShortName As String = "I3DL2"

            Public Overrides Property Category As String = "DSP"

            Private _IsEnabled As Boolean
            Public Overrides Property IsEnabled As Boolean
                Get
                    Return _IsEnabled
                End Get
                Set(value As Boolean)
                    If value Then
                        If HEffect = 0 Then
                            Apply()
                        End If
                    Else
                        _IsEnabled = Not Bass.BASS_ChannelRemoveFX(HStream, HEffect)
                        If _IsEnabled = False Then
                            HEffect = 0
                        End If
                        OnPropertyChanged()
                    End If
                End Set
            End Property

            Private _HEffect As Integer
            Public Overrides Property HEffect As Integer
                Get
                    Return _HEffect
                End Get
                Set(value As Integer)
                    _HEffect = value
                    OnPropertyChanged()
                End Set
            End Property

            Private _HStream As Integer
            Public Overrides Property HStream As Integer
                Get
                    Return _HStream
                End Get
                Set(value As Integer)
                    _HStream = value
                    OnPropertyChanged()
                End Set
            End Property

            <MethodGroup(DisplayName:="Presets", GridStackWrap:=False, HorizontalItemSpacing:=10, VerticalItemSpacing:=10, Orientation:=Orientation.Horizontal)>
            Property Group0

            Property FXItem As Un4seen.Bass.BASS_DX8_I3DL2REVERB

            Public Overrides Sub Apply()
                FXItem = New Un4seen.Bass.BASS_DX8_I3DL2REVERB
                FXItem.Preset_Default()
                HEffect = Bass.BASS_ChannelSetFX(HStream, BASSFXType.BASS_FX_DX8_I3DL2REVERB, 0)
                _IsEnabled = Bass.BASS_FXSetParameters(HEffect, FXItem)
                If HEffect <> 0 Then _IsEnabled = True
                OnPropertyChanged(NameOf(IsEnabled))
            End Sub

            Sub Update()
                If IsEnabled Then
                    Bass.BASS_FXSetParameters(HEffect, FXItem)
                End If
            End Sub

            <Method("Default", "Sets the instance members to a preset.", Group:="Presets")>
            Sub Preset_Default()
                FXItem.Preset_Default()
                Update()
            End Sub
        End Class

        <Serializable>
        Public Class Compressor
            Inherits Profile.AudioEffect

            Public Overrides Property Name As String = "Compressor"

            Public Overrides Property Description As String = "A compressor is used to reduce a signal's dynamic range that is, to reduce the difference in level between the loudest and quietest parts of an audio signal."

            Public Overrides Property ShortName As String = "CMP"

            Public Overrides Property Category As String = "DSP"

            Private _IsEnabled As Boolean

            Public Overrides Property IsEnabled As Boolean
                Get
                    Return _IsEnabled
                End Get
                Set(value As Boolean)
                    If value = _IsEnabled Then Return
                    If value Then
                        HEffect = Bass.BASS_ChannelSetFX(HStream, BASSFXType.BASS_FX_DX8_COMPRESSOR, 0)
                        _IsEnabled = Bass.BASS_FXSetParameters(HEffect, FXItem)
                    Else
                        _IsEnabled = Not Bass.BASS_ChannelRemoveFX(HStream, HEffect)
                    End If
                    OnPropertyChanged()
                End Set
            End Property

            Private _HEffect As Integer
            Public Overrides Property HEffect As Integer
                Get
                    Return _HEffect
                End Get
                Set(value As Integer)
                    _HEffect = value
                    OnPropertyChanged()
                End Set
            End Property

            Private _HStream As Integer
            Public Overrides Property HStream As Integer
                Get
                    Return _HStream
                End Get
                Set(value As Integer)
                    _HStream = value
                    OnPropertyChanged()
                End Set
            End Property

            <Field("Attack", GetType(Single), Minimum:=0.01, Maximum:=500, Description:="Time in ms before compression reaches its full value.")>
            Public Property Attack As Single
                Get
                    Return FXItem.fAttack
                End Get
                Set(value As Single)
                    FXItem.fAttack = value
                    If IsEnabled Then
                        If Not Bass.BASS_FXSetParameters(HEffect, FXItem) Then
                            Threading.Thread.Sleep(100)
                            Bass.BASS_FXSetParameters(HEffect, FXItem)
                        End If
                        OnPropertyChanged()
                    End If
                End Set
            End Property

            <Field("Gain", GetType(Single), Minimum:=-60, Maximum:=60, Description:="Output gain of signal in dB after compression.")>
            Public Property Gain As Single
                Get
                    Return FXItem.fGain
                End Get
                Set(value As Single)
                    FXItem.fGain = value
                    If IsEnabled Then
                        If Not Bass.BASS_FXSetParameters(HEffect, FXItem) Then
                            Threading.Thread.Sleep(100)
                            Bass.BASS_FXSetParameters(HEffect, FXItem)
                        End If
                        OnPropertyChanged()
                    End If
                End Set
            End Property

            <Field("Predelay", GetType(Single), Minimum:=0, Maximum:=4, Description:="Time in ms after fThreshold is reached before attack phase is started.")>
            Public Property Predelay As Single
                Get
                    Return FXItem.fPredelay
                End Get
                Set(value As Single)
                    FXItem.fPredelay = value
                    If IsEnabled Then
                        If Not Bass.BASS_FXSetParameters(HEffect, FXItem) Then
                            Threading.Thread.Sleep(100)
                            Bass.BASS_FXSetParameters(HEffect, FXItem)
                        End If
                        OnPropertyChanged()
                    End If
                End Set
            End Property

            <Field("Ratio", GetType(Single), Minimum:=1, Maximum:=100, Description:="Compression ratio.")>
            Public Property Ratio As Single
                Get
                    Return FXItem.fRatio
                End Get
                Set(value As Single)
                    FXItem.fRatio = value
                    If IsEnabled Then
                        If Not Bass.BASS_FXSetParameters(HEffect, FXItem) Then
                            Threading.Thread.Sleep(100)
                            Bass.BASS_FXSetParameters(HEffect, FXItem)
                        End If
                        OnPropertyChanged()
                    End If
                End Set
            End Property

            <Field("Release", GetType(Single), Minimum:=50, Maximum:=3000, Description:="Time (speed) in ms at which compression is stopped after input drops below fThreshold.")>
            Public Property Release As Single
                Get
                    Return FXItem.fRelease
                End Get
                Set(value As Single)
                    FXItem.fRelease = value
                    If IsEnabled Then
                        If Not Bass.BASS_FXSetParameters(HEffect, FXItem) Then
                            Threading.Thread.Sleep(100)
                            Bass.BASS_FXSetParameters(HEffect, FXItem)
                        End If
                        OnPropertyChanged()
                    End If
                End Set
            End Property

            <Field("Threshold", GetType(Single), Minimum:=-60, Maximum:=0, Description:="Point at which compression begins, in dB.")>
            Public Property Threshold As Single
                Get
                    Return FXItem.fThreshold
                End Get
                Set(value As Single)
                    FXItem.fThreshold = value
                    If IsEnabled Then
                        If Not Bass.BASS_FXSetParameters(HEffect, FXItem) Then
                            Threading.Thread.Sleep(100)
                            Bass.BASS_FXSetParameters(HEffect, FXItem)
                        End If
                        OnPropertyChanged()
                    End If
                End Set
            End Property

            <MethodGroup(DisplayName:="Presets", GridStackWrap:=False, HorizontalItemSpacing:=10, VerticalItemSpacing:=10, Orientation:=Orientation.Horizontal)>
            Property Group0

            Private FXItem As New Un4seen.Bass.BASS_DX8_COMPRESSOR

            Sub New()
                FXItem.Preset_Default()
            End Sub

            Public Overrides Sub Apply()
                HEffect = Bass.BASS_ChannelSetFX(HStream, BASSFXType.BASS_FX_DX8_COMPRESSOR, 0)
                _IsEnabled = Bass.BASS_FXSetParameters(HEffect, FXItem)
                OnPropertyChanged(NameOf(IsEnabled))
                NotifyUI()
            End Sub

            Private Sub NotifyUI()
                OnPropertyChanged(NameOf(Attack))
                OnPropertyChanged(NameOf(Gain))
                OnPropertyChanged(NameOf(Predelay))
                OnPropertyChanged(NameOf(Ratio))
                OnPropertyChanged(NameOf(Release))
                OnPropertyChanged(NameOf(Threshold))
            End Sub

            Sub Update()
                If IsEnabled Then
                    Bass.BASS_FXSetParameters(HEffect, FXItem)
                    NotifyUI()
                End If
            End Sub

            <Method("Default", "Sets the instance members to a preset.", Group:="Presets")>
            Sub Preset_Default()
                FXItem.Preset_Default()
                Update()
            End Sub

            <Method("Hard", "Sets the instance members to a preset.", Group:="Presets")>
            Sub Preset_Hard()
                FXItem.Preset_Hard()
                Update()
            End Sub

            <Method("Hard 2", "Sets the instance members to a preset.", Group:="Presets")>
            Sub Preset_Hard2()
                FXItem.Preset_Hard2()
                Update()
            End Sub

            <Method("Hard Commercial", "Sets the instance members to a preset.", Group:="Presets")>
            Sub Preset_HardCommercial()
                FXItem.Preset_HardCommercial()
                Update()
            End Sub

            <Method("Medium", "Sets the instance members to a preset.", Group:="Presets")>
            Sub Preset_Medium()
                FXItem.Preset_Medium()
                Update()
            End Sub

            <Method("Soft", "Sets the instance members to a preset.", Group:="Presets")>
            Sub Preset_Soft()
                FXItem.Preset_Soft()
                Update()
            End Sub

            <Method("Soft 2", "Sets the instance members to a preset.", Group:="Presets")>
            Sub Preset_Soft2()
                FXItem.Preset_Soft2()
                Update()
            End Sub
        End Class

        <Serializable>
        Public Class SignleBandEqualizer
            Inherits Profile.AudioEffect

            Public Overrides Property Name As String = "Single Band Equalizer"

            Public Overrides Property Description As String = "1 band equalizer."

            Public Overrides Property ShortName As String = "sEQ"

            Public Overrides Property Category As String = "DSP"

            Private _IsEnabled As Boolean

            Public Overrides Property IsEnabled As Boolean
                Get
                    Return _IsEnabled
                End Get
                Set(value As Boolean)
                    If value = _IsEnabled Then Return
                    If value Then
                        HEffect = Bass.BASS_ChannelSetFX(HStream, BASSFXType.BASS_FX_DX8_PARAMEQ, 0)
                        _IsEnabled = Bass.BASS_FXSetParameters(HEffect, FXItem)
                    Else
                        _IsEnabled = Not Bass.BASS_ChannelRemoveFX(HStream, HEffect)
                    End If
                    OnPropertyChanged()
                End Set
            End Property

            Private _HEffect As Integer
            Public Overrides Property HEffect As Integer
                Get
                    Return _HEffect
                End Get
                Set(value As Integer)
                    _HEffect = value
                    OnPropertyChanged()
                End Set
            End Property

            Private _HStream As Integer
            Public Overrides Property HStream As Integer
                Get
                    Return _HStream
                End Get
                Set(value As Integer)
                    _HStream = value
                    OnPropertyChanged()
                End Set
            End Property

            <Field("Center", GetType(Single), Description:="Center frequency, in hertz, in the range from 80 to 16000. This value cannot exceed one-third of the frequency of the channel. Default 100 Hz.", Minimum:=80, Maximum:=18000)>
            Public Property Center As Single
                Get
                    Return FXItem.fCenter
                End Get
                Set(value As Single)
                    FXItem.fCenter = value
                    If IsEnabled Then
                        If Not Bass.BASS_FXSetParameters(HEffect, FXItem) Then
                            Threading.Thread.Sleep(100)
                            Bass.BASS_FXSetParameters(HEffect, FXItem)
                        End If
                        OnPropertyChanged()
                    End If
                End Set
            End Property

            <Field("Bandwidth", GetType(Single), Description:="Bandwidth, in semitones, in the range from 1 to 36. Default 18 semitones.", Minimum:=1, Maximum:=36)>
            Public Property Bandwidth As Single
                Get
                    Return FXItem.fBandwidth
                End Get
                Set(value As Single)
                    FXItem.fBandwidth = value
                    If IsEnabled Then
                        If Not Bass.BASS_FXSetParameters(HEffect, FXItem) Then
                            Threading.Thread.Sleep(100)
                            Bass.BASS_FXSetParameters(HEffect, FXItem)
                        End If
                        OnPropertyChanged()
                    End If
                End Set
            End Property

            <Field("Gain", GetType(Single), Description:="Gain, in the range from -15 to 15. Default 0 dB.", Minimum:=-15, Maximum:=15)>
            Public Property Gain As Single
                Get
                    Return FXItem.fGain
                End Get
                Set(value As Single)
                    FXItem.fGain = value
                    If IsEnabled Then
                        If Not Bass.BASS_FXSetParameters(HEffect, FXItem) Then
                            Threading.Thread.Sleep(100)
                            Bass.BASS_FXSetParameters(HEffect, FXItem)
                        End If
                        OnPropertyChanged()
                    End If
                End Set
            End Property

            <MethodGroup(DisplayName:="Presets", Description:="A set of premade values", GridStackWrap:=TriState.False, Height:=Double.NaN, HorizontalItemSpacing:=10, Orientation:=Orientation.Horizontal, Scroll:=False, VerticalItemSpacing:=10, Width:=Double.NaN)>
            Property Group0


            Private FXItem As New Un4seen.Bass.BASS_DX8_PARAMEQ(100, 18, 0)

            Sub New()
                FXItem.Preset_Default()
            End Sub

            Public Overrides Sub Apply()
                HEffect = Bass.BASS_ChannelSetFX(HStream, BASSFXType.BASS_FX_DX8_PARAMEQ, 0)
                _IsEnabled = Bass.BASS_FXSetParameters(HEffect, FXItem)
                OnPropertyChanged(NameOf(IsEnabled))
                NotifyUI()
            End Sub

            Private Sub NotifyUI()
                OnPropertyChanged(NameOf(Center))
                OnPropertyChanged(NameOf(Gain))
                OnPropertyChanged(NameOf(Bandwidth))
            End Sub

            Sub Update()
                If IsEnabled Then
                    If Bass.BASS_FXSetParameters(HEffect, FXItem) Then
                        NotifyUI()
                    End If
                End If
            End Sub

            <Method("Default", "Sets the instance members to a preset.", Group:="Presets")>
            Sub Preset_Default()
                FXItem.Preset_Default()
                Update()
            End Sub

            <Method("Low", "Sets the instance members to a preset.", Group:="Presets")>
            Sub Preset_Low()
                FXItem.Preset_High()
                Update()
            End Sub

            <Method("Mid", "Sets the instance members to a preset.", Group:="Presets")>
            Sub Preset_Mid()
                FXItem.Preset_Mid()
                Update()
            End Sub

            <Method("High", "Sets the instance members to a preset.", Group:="Presets")>
            Sub Preset_High()
                FXItem.Preset_High()
                Update()
            End Sub

            <Method("Mid Bass", "Sets the instance members to a preset.", Group:="Presets")>
            Sub Preset_Bass0()
                Center = 98
                Bandwidth = 24
                Gain = 10
                Update()
            End Sub

            <Method("High Bass", "Sets the instance members to a preset.", Group:="Presets")>
            Sub Preset_Bass1()
                Center = 110
                Bandwidth = 10
                Gain = 5
                Update()
            End Sub

            <Method("Mid Treble", "Sets the instance members to a preset.", Group:="Presets")>
            Sub Preset_Treble1()
                Center = 7500
                Bandwidth = 24
                Gain = 10
                Update()
            End Sub
        End Class

        <Serializable>
        Public Class BassBoost
            Inherits Profile.AudioEffect

            Public Overrides Property Name As String = "Bass Boost"

            Public Overrides Property Description As String = "Boosts low frequencies."

            Public Overrides Property ShortName As String = "BASS"

            Public Overrides Property Category As String = "DSP"

            Private _IsEnabled As Boolean

            Public Overrides Property IsEnabled As Boolean
                Get
                    Return _IsEnabled
                End Get
                Set(value As Boolean)
                    If value = _IsEnabled Then Return
                    If value Then
                        HEffect = Bass.BASS_ChannelSetFX(HStream, BASSFXType.BASS_FX_DX8_PARAMEQ, 0)
                        _IsEnabled = Bass.BASS_FXSetParameters(HEffect, FXItem)
                    Else
                        _IsEnabled = Not Bass.BASS_ChannelRemoveFX(HStream, HEffect)
                    End If
                    OnPropertyChanged()
                End Set
            End Property

            Private _HEffect As Integer
            Public Overrides Property HEffect As Integer
                Get
                    Return _HEffect
                End Get
                Set(value As Integer)
                    _HEffect = value
                    OnPropertyChanged()
                End Set
            End Property

            Private _HStream As Integer
            Public Overrides Property HStream As Integer
                Get
                    Return _HStream
                End Get
                Set(value As Integer)
                    _HStream = value
                    OnPropertyChanged()
                End Set
            End Property

            <Field("Gain", GetType(Single), Description:="Gain, in the range from -15 to 15. Default 0 dB.", Minimum:=-15, Maximum:=15)>
            Public Property Gain As Single
                Get
                    Return FXItem.fGain
                End Get
                Set(value As Single)
                    FXItem.fGain = value
                    If IsEnabled Then
                        If Not Bass.BASS_FXSetParameters(HEffect, FXItem) Then
                            Threading.Thread.Sleep(100)
                            Bass.BASS_FXSetParameters(HEffect, FXItem)
                        End If
                        OnPropertyChanged()
                    End If
                End Set
            End Property

            <MethodGroup(DisplayName:="Presets", Description:="A set of premade values", GridStackWrap:=TriState.False, Height:=Double.NaN, HorizontalItemSpacing:=10, Orientation:=Orientation.Horizontal, Scroll:=False, VerticalItemSpacing:=10, Width:=Double.NaN)>
            Property Group0


            Private FXItem As New Un4seen.Bass.BASS_DX8_PARAMEQ(90, 36, 0)

            Sub New()
                ' FXItem.Preset_Default()
            End Sub

            Public Overrides Sub Apply()
                HEffect = Bass.BASS_ChannelSetFX(HStream, BASSFXType.BASS_FX_DX8_PARAMEQ, 0)
                _IsEnabled = Bass.BASS_FXSetParameters(HEffect, FXItem)
                OnPropertyChanged(NameOf(IsEnabled))
                NotifyUI()
            End Sub

            Private Sub NotifyUI()
                OnPropertyChanged(NameOf(Gain))
            End Sub

            Sub Update()
                If IsEnabled Then
                    If Bass.BASS_FXSetParameters(HEffect, FXItem) Then
                        NotifyUI()
                    End If
                End If
            End Sub

            <Method("Default", "Sets the instance members to a preset.", Group:="Presets")>
            Sub Preset_Default()
                Gain = 0
            End Sub

            <Method("Low", "Sets the instance members to a preset.", Group:="Presets")>
            Sub Preset_Low()
                Gain = 5
            End Sub

            <Method("Mid", "Sets the instance members to a preset.", Group:="Presets")>
            Sub Preset_Mid()
                Gain = 7
            End Sub

            <Method("High", "Sets the instance members to a preset.", Group:="Presets")>
            Sub Preset_High()
                Gain = 10
            End Sub

        End Class

        <Serializable>
        Public Class TrebleBoost
            Inherits Profile.AudioEffect

            Public Overrides Property Name As String = "Treble Boost"

            Public Overrides Property Description As String = "Boosts high frequencies."

            Public Overrides Property ShortName As String = "TRBL"

            Public Overrides Property Category As String = "DSP"

            Private _IsEnabled As Boolean

            Public Overrides Property IsEnabled As Boolean
                Get
                    Return _IsEnabled
                End Get
                Set(value As Boolean)
                    If value = _IsEnabled Then Return
                    If value Then
                        HEffect = Bass.BASS_ChannelSetFX(HStream, BASSFXType.BASS_FX_DX8_PARAMEQ, 0)
                        _IsEnabled = Bass.BASS_FXSetParameters(HEffect, FXItem)
                    Else
                        _IsEnabled = Not Bass.BASS_ChannelRemoveFX(HStream, HEffect)
                    End If
                    OnPropertyChanged()
                End Set
            End Property

            Private _HEffect As Integer
            Public Overrides Property HEffect As Integer
                Get
                    Return _HEffect
                End Get
                Set(value As Integer)
                    _HEffect = value
                    OnPropertyChanged()
                End Set
            End Property

            Private _HStream As Integer
            Public Overrides Property HStream As Integer
                Get
                    Return _HStream
                End Get
                Set(value As Integer)
                    _HStream = value
                    OnPropertyChanged()
                End Set
            End Property

            <Field("Gain", GetType(Single), Description:="Gain, in the range from -15 to 15. Default 0 dB.", Minimum:=-15, Maximum:=15)>
            Public Property Gain As Single
                Get
                    Return FXItem.fGain
                End Get
                Set(value As Single)
                    FXItem.fGain = value
                    If IsEnabled Then
                        If Not Bass.BASS_FXSetParameters(HEffect, FXItem) Then
                            Threading.Thread.Sleep(100)
                            Bass.BASS_FXSetParameters(HEffect, FXItem)
                        End If
                        OnPropertyChanged()
                    End If
                End Set
            End Property

            <MethodGroup(DisplayName:="Presets", Description:="A set of premade values", GridStackWrap:=TriState.False, Height:=Double.NaN, HorizontalItemSpacing:=10, Orientation:=Orientation.Horizontal, Scroll:=False, VerticalItemSpacing:=10, Width:=Double.NaN)>
            Property Group0


            Private FXItem As New Un4seen.Bass.BASS_DX8_PARAMEQ(10000, 36, 0)

            Sub New()

            End Sub

            Public Overrides Sub Apply()
                HEffect = Bass.BASS_ChannelSetFX(HStream, BASSFXType.BASS_FX_DX8_PARAMEQ, 0)
                _IsEnabled = Bass.BASS_FXSetParameters(HEffect, FXItem)
                OnPropertyChanged(NameOf(IsEnabled))
                NotifyUI()
            End Sub

            Private Sub NotifyUI()
                OnPropertyChanged(NameOf(Gain))
            End Sub

            Sub Update()
                If IsEnabled Then
                    If Bass.BASS_FXSetParameters(HEffect, FXItem) Then
                        NotifyUI()
                    End If
                End If
            End Sub

            <Method("Default", "Sets the instance members to a preset.", Group:="Presets")>
            Sub Preset_Default()
                Gain = 0
            End Sub

            <Method("Low", "Sets the instance members to a preset.", Group:="Presets")>
            Sub Preset_Low()
                Gain = 5
            End Sub

            <Method("Mid", "Sets the instance members to a preset.", Group:="Presets")>
            Sub Preset_Mid()
                Gain = 7
            End Sub

            <Method("High", "Sets the instance members to a preset.", Group:="Presets")>
            Sub Preset_High()
                Gain = 10
            End Sub

        End Class

        <Serializable>
        Public Class Frequency
            Inherits Profile.AudioEffect

            Public Overrides Property Name As String = "Frequency"

            Public Overrides Property Description As String = "Change the sample rate of the current stream."

            Public Overrides Property ShortName As String = "FRQ"

            Public Overrides Property Category As String = "DSP"

            Private _IsEnabled As Boolean

            Public Overrides Property IsEnabled As Boolean
                Get
                    Return _IsEnabled
                End Get
                Set(value As Boolean)
                    If value = _IsEnabled Then Return
                    If value Then
                        _IsEnabled = Bass.BASS_ChannelSetAttribute(HStream, BASSAttribute.BASS_ATTRIB_FREQ, FXItem)
                    Else
                        _IsEnabled = Not Bass.BASS_ChannelSetAttribute(HStream, BASSAttribute.BASS_ATTRIB_FREQ, 0)
                    End If
                    OnPropertyChanged()
                End Set
            End Property

            Private _HEffect As Integer
            Public Overrides Property HEffect As Integer
                Get
                    Return _HEffect
                End Get
                Set(value As Integer)
                    _HEffect = value
                    OnPropertyChanged()
                End Set
            End Property

            Private _HStream As Integer
            Public Overrides Property HStream As Integer
                Get
                    Return _HStream
                End Get
                Set(value As Integer)
                    _HStream = value
                    OnPropertyChanged()
                End Set
            End Property

            <Field("Frequency", GetType(Single), Description:="The stream's fequency", Minimum:=100, Maximum:=100000)>
            Public Property Frequency As Single
                Get
                    Return FXItem
                End Get
                Set(value As Single)
                    FXItem = value
                    If IsEnabled Then
                        If Not Bass.BASS_ChannelSetAttribute(HStream, BASSAttribute.BASS_ATTRIB_FREQ, FXItem) Then
                            Threading.Thread.Sleep(100)
                            Bass.BASS_ChannelSetAttribute(HStream, BASSAttribute.BASS_ATTRIB_FREQ, FXItem)
                        End If
                        OnPropertyChanged()
                    End If
                End Set
            End Property

            <MethodGroup(DisplayName:="Presets", GridStackWrap:=TriState.False, HorizontalItemSpacing:=10, Orientation:=Orientation.Horizontal, VerticalItemSpacing:=10)>
            Property Group0

            Private FXItem As Single = 0

            Sub New()
                If HStream = 0 Then Return
                Dim StreamInfo = Bass.BASS_ChannelGetInfo(HStream)
                FXItem = If(StreamInfo Is Nothing, 0, StreamInfo.freq)
            End Sub

            Public Overrides Sub Apply()
                _IsEnabled = Bass.BASS_ChannelSetAttribute(HStream, BASSAttribute.BASS_ATTRIB_FREQ, FXItem)
                OnPropertyChanged(NameOf(IsEnabled))
                NotifyUI()
            End Sub

            Private Sub NotifyUI()
                OnPropertyChanged(NameOf(Frequency))
            End Sub

            Sub Update()
                If IsEnabled Then
                    Bass.BASS_ChannelSetAttribute(HStream, BASSAttribute.BASS_ATTRIB_FREQ, FXItem)
                    NotifyUI()
                End If
            End Sub

            <Method("Nightcore", "Enjoy nightcore in realtime!", Group:="Presets")>
            Sub Preset_Nightcore()
                If HStream = 0 Then Return
                'Nightcore 10%~35% : We'll take 24% since it sounds the best :)
                Dim Freq = Bass.BASS_ChannelGetInfo(HStream).freq
                FXItem = Freq + (Freq * 24 / 100)
                Update()
            End Sub

            <Method("Default", "Sets the instance members to a preset.", Group:="Presets")>
            Sub Preset_Default()
                If HStream = 0 Then Return
                Dim StreamInfo = Bass.BASS_ChannelGetInfo(HStream)
                FXItem = If(StreamInfo Is Nothing, 0, StreamInfo.freq)
                Update()
            End Sub

        End Class

        <Serializable>
        Public Class Balance
            Inherits Profile.AudioEffect

            Public Overrides Property Name As String = "Balance"

            Public Overrides Property Description As String = "Change the channel pan of the current stream."

            Public Overrides Property ShortName As String = "BAL"

            Public Overrides Property Category As String = "DSP"

            Private _IsEnabled As Boolean

            Public Overrides Property IsEnabled As Boolean
                Get
                    Return _IsEnabled
                End Get
                Set(value As Boolean)
                    If value = _IsEnabled Then Return
                    If value Then
                        _IsEnabled = Bass.BASS_ChannelSetAttribute(HStream, BASSAttribute.BASS_ATTRIB_PAN, FXItem)
                    Else
                        _IsEnabled = Not Bass.BASS_ChannelSetAttribute(HStream, BASSAttribute.BASS_ATTRIB_PAN, 0)
                    End If
                    OnPropertyChanged()
                End Set
            End Property

            Private _HEffect As Integer
            Public Overrides Property HEffect As Integer
                Get
                    Return _HEffect
                End Get
                Set(value As Integer)
                    _HEffect = value
                    OnPropertyChanged()
                End Set
            End Property

            Private _HStream As Integer
            Public Overrides Property HStream As Integer
                Get
                    Return _HStream
                End Get
                Set(value As Integer)
                    _HStream = value
                    OnPropertyChanged()
                End Set
            End Property

            <Field("Balance", GetType(Single), Description:="The panning/balance position, -1 (full left) to +1 (full right), 0 = centre.", Minimum:=-1, Maximum:=1)>
            Public Property Balance As Single
                Get
                    Return FXItem
                End Get
                Set(value As Single)
                    FXItem = value
                    If IsEnabled Then
                        If Not Bass.BASS_ChannelSetAttribute(HStream, BASSAttribute.BASS_ATTRIB_PAN, FXItem) Then
                            Threading.Thread.Sleep(100)
                            Bass.BASS_ChannelSetAttribute(HStream, BASSAttribute.BASS_ATTRIB_PAN, FXItem)
                        End If
                        OnPropertyChanged()
                    End If
                End Set
            End Property

            <MethodGroup(DisplayName:="Presets", GridStackWrap:=TriState.False, HorizontalItemSpacing:=10, Orientation:=Orientation.Horizontal, VerticalItemSpacing:=10)>
            Property Group0

            Private FXItem As Single = 0

            Sub New()
                If Not Bass.BASS_ChannelGetAttribute(HStream, BASSAttribute.BASS_ATTRIB_PAN, FXItem) Then FXItem = 0
            End Sub

            Public Overrides Sub Apply()
                _IsEnabled = Bass.BASS_ChannelSetAttribute(HStream, BASSAttribute.BASS_ATTRIB_PAN, FXItem)
                OnPropertyChanged(NameOf(IsEnabled))
                NotifyUI()
            End Sub

            Private Sub NotifyUI()
                OnPropertyChanged(NameOf(Balance))
            End Sub

            Sub Update()
                If IsEnabled Then
                    Bass.BASS_ChannelSetAttribute(HStream, BASSAttribute.BASS_ATTRIB_PAN, FXItem)
                    NotifyUI()
                End If
            End Sub

            <Method("Default", "Sets the instance members to a preset.", Group:="Presets")>
            Sub Preset_Default()
                FXItem = 0
                Update()
            End Sub

            <Method("Left", "Sets the instance members to a preset.", Group:="Presets")>
            Sub Preset_Left()
                Balance = -1
                Update()
            End Sub

            <Method("Right", "Sets the instance members to a preset.", Group:="Presets")>
            Sub Preset_Right()
                Balance = 1
            End Sub

            <Method("Mid Left", "Sets the instance members to a preset.", Group:="Presets")>
            Sub Preset_MidLeft()
                Balance = -0.5
            End Sub

            <Method("Mid Right", "Sets the instance members to a preset.", Group:="Presets")>
            Sub Preset_MidRight()
                Balance = 0.5
            End Sub
        End Class
        Public Class WinampDSP
            Inherits Profile.AudioEffect

            Private _Name As String = "Generic Winamp DSP Plugin"
            Public Overrides Property Name As String
                Get
                    Return _Name
                End Get
                Set(value As String)
                    _Name = value
                    OnPropertyChanged()
                End Set
            End Property

            Public Overrides Property Description As String = "Winamp DSP Plugin Loader."

            Private _ShortName As String
            Public Overrides Property ShortName As String
                Get
                    Return _ShortName
                End Get
                Set(value As String)
                    _ShortName = value
                    OnPropertyChanged()
                End Set
            End Property

            Public Overrides Property Category As String = "AddOn"

            Private _IsEnabled As Boolean

            Public Overrides Property IsEnabled As Boolean
                Get
                    Return _IsEnabled
                End Get
                Set(value As Boolean)
                    If value = _IsEnabled Then Return
                    If value Then
                        Dim h = BassWaDsp.BASS_WADSP_ChannelSetDSP(HPlugin, HStream, 0)
                        If h <> 0 Then
                            HEffect = h
                            _IsEnabled = True
                            OnPropertyChanged()
                        End If
                    Else
                        If BassWaDsp.BASS_WADSP_ChannelRemoveDSP(HPlugin) Then
                            _IsEnabled = False
                            OnPropertyChanged()
                        End If
                    End If
                    OnPropertyChanged()
                End Set
            End Property

            Private _HEffect As Integer
            Public Overrides Property HEffect As Integer
                Get
                    Return _HEffect
                End Get
                Set(value As Integer)
                    _HEffect = value
                    OnPropertyChanged()
                End Set
            End Property

            Private _HPlugin As Integer
            Public Property HPlugin As Integer
                Get
                    Return _HPlugin
                End Get
                Set(value As Integer)
                    _HPlugin = value
                    OnPropertyChanged()
                End Set
            End Property

            Private _HStream As Integer
            Public Overrides Property HStream As Integer
                Get
                    Return _HStream
                End Get
                Set(value As Integer)
                    _HStream = value
                    OnPropertyChanged()
                End Set
            End Property

            Private _PluginPath As String
            Public Property PluginPath As String
                Get
                    Return _PluginPath
                End Get
                Set(value As String)
                    _PluginPath = value
                    OnPropertyChanged()
                End Set
            End Property

            Sub New()
                Dim ofd As New Microsoft.Win32.OpenFileDialog
                If ofd.ShowDialog Then
                    HPlugin = BassWaDsp.BASS_WADSP_Load(ofd.FileName, 5, 5, 100, 100, Nothing)
                    _IsEnabled = (HPlugin <> 0)
                    If HPlugin <> 0 Then
                        Dim IsActive = BassWaDsp.BASS_WADSP_Start(HPlugin, HStream, 0)
                        Name = BassWaDsp.BASS_WADSP_GetName(HPlugin)
                    End If
                Else
                    Name = "Unset Winamp Plugin"
                End If
            End Sub

            Public Overrides Sub Apply()
                HEffect = BassWaDsp.BASS_WADSP_ChannelSetDSP(HPlugin, HStream, 0)
                _IsEnabled = (HEffect <> 0)
                OnPropertyChanged(NameOf(IsEnabled))
            End Sub

            Public Overrides Sub Clean()
                BassWaDsp.BASS_WADSP_Stop(HPlugin)
                BassWaDsp.BASS_WADSP_FreeDSP(HPlugin)
                HPlugin = 0
            End Sub

            <Config>
            Public Sub Config()
                BassWaDsp.BASS_WADSP_Config(HPlugin)
            End Sub

        End Class
    End Class
End Namespace
