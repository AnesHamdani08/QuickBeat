﻿Imports QuickBeat.Player.Profile.AudioEffect
Imports Un4seen.Bass
Imports Un4seen.Bass.Misc
Imports Un4seen.Bass.AddOn.WaDsp

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

        '<NonSerialized> Private FXItem As New Un4seen.Bass.

        'Sub New()
        '    FXItem.Preset_Default()
        'End Sub

        'Public Overrides Sub Apply(Optional Force as Boolean = False)
        '    If Not Force Then If HEffect <> 0 Then Return
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
        '#Region "Serialization"
        '        Private SerializationData As String

        '        <Runtime.Serialization.OnDeserialized>
        '        Private Sub Deserialization(context As Runtime.Serialization.StreamingContext)
        '            'Restore values from serialization
        '            Dim SData = SerializationData.Split(";")
        '            FXItem = 
        '            End Sub

        '        <Runtime.Serialization.OnSerializing>
        '        Private Sub Serializing(context As Runtime.Serialization.StreamingContext)
        '            'Save values for deserialization
        '             SerializationData = FXItem. & ";"
        '             HEffect = 0
        '        End Sub
        '#End Region
        'End Class

        <Serializable>
        Public Class Chorus
            Inherits Profile.AudioEffect

            Public Overrides Property Name As String = "Chorus"

            Public Overrides Property Description As String = "Audio effect that occurs when individual sounds with approximately the same time, and very similar pitches, converge."

            Public Overrides Property ShortName As String = "CRS"

            Public Overrides Property Category As String = "DX8;DSP"

            Private _IsEnabled As Boolean
            Public Overrides Property IsEnabled As Boolean
                Get
                    Return _IsEnabled
                End Get
                Set(value As Boolean)
                    If value = _IsEnabled Then Return
                    If value Then
                        HEffect = Bass.BASS_ChannelSetFX(HStream, BASSFXType.BASS_FX_DX8_CHORUS, Priority)
                        _IsEnabled = Bass.BASS_FXSetParameters(HEffect, FXItem)
                    Else
                        _IsEnabled = Not Bass.BASS_ChannelRemoveFX(HStream, HEffect)
                        If Not _IsEnabled Then
                            HEffect = 0
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

            Private _Priority As Integer
            Public Overrides Property Priority As Integer
                Get
                    Return _Priority
                End Get
                Set(value As Integer)
                    Bass.BASS_FXSetPriority(HEffect, value)
                    _Priority = value
                    OnPropertyChanged()
                End Set
            End Property

            <Field("Delay", GetType(Single), "Milliseconds", "ms", Minimum:=0, Maximum:=20, Description:="Number of milliseconds the input is delayed before it is played back.")>
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
            <Field("Depth", GetType(Single), "Percentage", "%", Minimum:=0, Maximum:=100, Description:="Percentage by which the delay time is modulated by the low-frequency oscillator, in hundredths of a percentage point.")>
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
            <Field("Feedback", GetType(Single), "Percentage", "%", Minimum:=-99, Maximum:=99, Description:="Percentage of output signal to feed back into the effect's input.")>
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
            <Field("Frequency", GetType(Single), "Hertz", "Hz", Minimum:=0, Maximum:=10, Description:="Frequency of the LFO.")>
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
            <Field("Wet/Dry Mix", GetType(Single), "Percentage", "%", Minimum:=0, Maximum:=100, Description:="Ratio of wet (processed) signal to dry (unprocessed) signal.")>
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
            <Field("Phase", GetType(BASSFXPhase), "Degrees", "°", Description:="Phase differential between left and right LFOs.")>
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

            <NonSerialized> Private FXItem As New Un4seen.Bass.BASS_DX8_CHORUS

            Sub New()
                FXItem.Preset_Default()
            End Sub

            Public Overrides Sub Apply(Optional Force As Boolean = False)
                If Not Force Then If HEffect <> 0 Then Return
                HEffect = Bass.BASS_ChannelSetFX(HStream, BASSFXType.BASS_FX_DX8_CHORUS, Priority)
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

#Region "Serialization"
            Private SerializationData As String

            <Runtime.Serialization.OnDeserialized>
            Private Sub Deserialization(context As Runtime.Serialization.StreamingContext)
                'Restore values from serialization
                Dim SData = SerializationData.Split(";")
                FXItem = New BASS_DX8_CHORUS With {.fDelay = SData(0), .fDepth = SData(1), .fFeedback = SData(2), .fFrequency = SData(3), .fWetDryMix = SData(4), .lPhase = SData(5), .lWaveform = SData(6)}
            End Sub

            <Runtime.Serialization.OnSerializing>
            Private Sub Serializing(context As Runtime.Serialization.StreamingContext)
                'Save values for deserialization
                SerializationData = FXItem.fDelay & ";" & FXItem.fDepth & ";" & FXItem.fFeedback & ";" & FXItem.fFrequency & ";" & FXItem.fWetDryMix & ";" & FXItem.lPhase & ";" & FXItem.lWaveform
                HEffect = 0
            End Sub
#End Region
        End Class

        <Serializable>
        Public Class Flanger
            Inherits Profile.AudioEffect

            Public Overrides Property Name As String = "Flanger"

            Public Overrides Property Description As String = "Flanging is an audio effect produced by mixing two identical signals together, one signal delayed by a small and gradually changing period, usually smaller than 20 milliseconds."

            Public Overrides Property ShortName As String = "FLR"

            Public Overrides Property Category As String = "DX8;DSP"

            Private _IsEnabled As Boolean
            Public Overrides Property IsEnabled As Boolean
                Get
                    Return _IsEnabled
                End Get
                Set(value As Boolean)
                    If value = _IsEnabled Then Return
                    If value Then
                        HEffect = Bass.BASS_ChannelSetFX(HStream, BASSFXType.BASS_FX_DX8_FLANGER, Priority)
                        _IsEnabled = Bass.BASS_FXSetParameters(HEffect, FXItem)
                    Else
                        _IsEnabled = Not Bass.BASS_ChannelRemoveFX(HStream, HEffect)
                        If Not _IsEnabled Then
                            HEffect = 0
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

            Private _Priority As Integer
            Public Overrides Property Priority As Integer
                Get
                    Return _Priority
                End Get
                Set(value As Integer)
                    Bass.BASS_FXSetPriority(HEffect, value)
                    _Priority = value
                    OnPropertyChanged()
                End Set
            End Property

            <Field("Delay", GetType(Single), "Milliseconds", "ms", Minimum:=0, Maximum:=4, Description:="Number of milliseconds the input is delayed before it is played back.")>
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
            <Field("Depth", GetType(Single), "Percentage", "%", Minimum:=0, Maximum:=100, Description:="Percentage by which the delay time is modulated by the low-frequency oscillator, in hundredths of a percentage point.")>
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
            <Field("Feedback", GetType(Single), "Percentage", "%", Minimum:=-99, Maximum:=99, Description:="Percentage of output signal to feed back into the effect's input.")>
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
            <Field("Frequency", GetType(Single), "Hertz", "Hz", Minimum:=0, Maximum:=10, Description:="Frequency of the LFO.")>
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
            <Field("Wet/Dry Mix", GetType(Single), "Percentage", "%", Minimum:=0, Maximum:=100, Description:="Ratio of wet (processed) signal to dry (unprocessed) signal.")>
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
            <Field("Phase", GetType(BASSFXPhase), "Degrees", "°", Description:="Phase differential between left and right LFOs.")>
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

            <NonSerialized> Private FXItem As New Un4seen.Bass.BASS_DX8_FLANGER

            Sub New()
                FXItem.Preset_Default()
            End Sub

            Public Overrides Sub Apply(Optional Force As Boolean = False)
                If Not Force Then If HEffect <> 0 Then Return
                HEffect = Bass.BASS_ChannelSetFX(HStream, BASSFXType.BASS_FX_DX8_FLANGER, Priority)
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

#Region "Serialization"
            Private SerializationData As String

            <Runtime.Serialization.OnDeserialized>
            Private Sub Deserialization(context As Runtime.Serialization.StreamingContext)
                'Restore values from serialization
                Dim SData = SerializationData.Split(";")
                FXItem = New BASS_DX8_FLANGER With {.fDelay = SData(0), .fDepth = SData(1), .fFeedback = SData(2), .fFrequency = SData(3), .fWetDryMix = SData(4), .lPhase = SData(5), .lWaveform = SData(6)}
            End Sub

            <Runtime.Serialization.OnSerializing>
            Private Sub Serializing(context As Runtime.Serialization.StreamingContext)
                'Save values for deserialization
                SerializationData = FXItem.fDelay & ";" & FXItem.fDepth & ";" & FXItem.fFeedback & ";" & FXItem.fFrequency & ";" & FXItem.fWetDryMix & ";" & FXItem.lPhase & ";" & FXItem.lWaveform
                HEffect = 0
            End Sub
#End Region
        End Class

        <Serializable>
        Public Class Echo
            Inherits Profile.AudioEffect

            Public Overrides Property Name As String = "Echo"

            Public Overrides Property Description As String = "Repetition or a partial repetition of a sound due to reflection."

            Public Overrides Property ShortName As String = "ECO"

            Public Overrides Property Category As String = "DX8;DSP"

            Private _IsEnabled As Boolean
            Public Overrides Property IsEnabled As Boolean
                Get
                    Return _IsEnabled
                End Get
                Set(value As Boolean)
                    If value = _IsEnabled Then Return
                    If value Then
                        HEffect = Bass.BASS_ChannelSetFX(HStream, BASSFXType.BASS_FX_DX8_ECHO, Priority)
                        _IsEnabled = Bass.BASS_FXSetParameters(HEffect, FXItem)
                    Else
                        _IsEnabled = Not Bass.BASS_ChannelRemoveFX(HStream, HEffect)
                        If Not _IsEnabled Then
                            HEffect = 0
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

            Private _Priority As Integer
            Public Overrides Property Priority As Integer
                Get
                    Return _Priority
                End Get
                Set(value As Integer)
                    Bass.BASS_FXSetPriority(HEffect, value)
                    _Priority = value
                    OnPropertyChanged()
                End Set
            End Property

            <Field("Feedback", GetType(Single), "Percentage", "%", Minimum:=0, Maximum:=100, Description:="Percentage of output fed back into input")>
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

            <Field("Left Delay", GetType(Single), "Milliseconds", "ms", Minimum:=1, Maximum:=2000)>
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

            <Field("Right Delay", GetType(Single), "Milliseconds", "ms", Minimum:=1, Maximum:=2000)>
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

            <Field("Wet/Dry Mix", GetType(Single), "Percentage", "%", Minimum:=0, Maximum:=100, Description:="Ratio of wet (processed) signal to dry (unprocessed) signal.")>
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

            <Field("Pan Delay", GetType(Boolean), "Boolean", "T/F", Description:="Value that specifies whether to swap left and right delays with each successive echo.")>
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

            <NonSerialized> Private FXItem As New Un4seen.Bass.BASS_DX8_ECHO

            Sub New()
                FXItem.Preset_Default()
            End Sub

            Public Overrides Sub Apply(Optional Force As Boolean = False)
                If Not Force Then If HEffect <> 0 Then Return
                HEffect = Bass.BASS_ChannelSetFX(HStream, BASSFXType.BASS_FX_DX8_ECHO, Priority)
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

#Region "Serialization"
            Private SerializationData As String

            <Runtime.Serialization.OnDeserializing>
            Private Sub Deserialization(context As Runtime.Serialization.StreamingContext)
                'Restore values from serialization
                Dim SData = SerializationData.Split(";")
                FXItem = New BASS_DX8_ECHO With {.fFeedback = SData(0), .fLeftDelay = SData(1), .fRightDelay = SData(2), .fWetDryMix = SData(3), .lPanDelay = SData(4)}
            End Sub

            <Runtime.Serialization.OnSerializing>
            Private Sub Serializing(context As Runtime.Serialization.StreamingContext)
                'Save values for deserialization
                SerializationData = FXItem.fFeedback & ";" & FXItem.fLeftDelay & ";" & FXItem.fRightDelay & ";" & FXItem.fWetDryMix & ";" & FXItem.lPanDelay
                HEffect = 0
            End Sub
#End Region
        End Class

        <Serializable>
        Public Class I3DL2Reverb
            Inherits Profile.AudioEffect

            Public Overrides Property Name As String = "Interactive 3D Audio Level 2 Reverb"

            Public Overrides Property Description As String = "An audio effect applied to a sound signal to simulate reverberation."

            Public Overrides Property ShortName As String = "I3DL2"

            Public Overrides Property Category As String = "DX8;DSP"

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

            Private _Priority As Integer
            Public Overrides Property Priority As Integer
                Get
                    Return _Priority
                End Get
                Set(value As Integer)
                    Bass.BASS_FXSetPriority(HEffect, value)
                    _Priority = value
                    OnPropertyChanged()
                End Set
            End Property

            <MethodGroup(DisplayName:="Presets", GridStackWrap:=False, HorizontalItemSpacing:=10, VerticalItemSpacing:=10, Orientation:=Orientation.Horizontal)>
            Property Group0

            <NonSerialized> Private FXItem As Un4seen.Bass.BASS_DX8_I3DL2REVERB

            Public Overrides Sub Apply(Optional Force As Boolean = False)
                If Not Force Then If HEffect <> 0 Then Return
                If FXItem Is Nothing Then
                    FXItem = New Un4seen.Bass.BASS_DX8_I3DL2REVERB
                    FXItem.Preset_Default()
                End If
                HEffect = Bass.BASS_ChannelSetFX(HStream, BASSFXType.BASS_FX_DX8_I3DL2REVERB, Priority)
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

#Region "Serialization"
            Private SerializationData As String

            <Runtime.Serialization.OnDeserialized>
            Private Sub Deserialization(context As Runtime.Serialization.StreamingContext)
                'Restore values from serialization
                Dim SData = SerializationData.Split(";")
                FXItem = New BASS_DX8_I3DL2REVERB With {.flDecayHFRatio = SData(0), .flDecayTime = SData(1), .flDensity = SData(2), .flDiffusion = SData(3),
                    .flHFReference = SData(4), .flReflectionsDelay = SData(5), .flReverbDelay = SData(6), .flRoomRolloffFactor = SData(7),
                    .lReflections = SData(8), .lReverb = SData(9), .lRoom = SData(10), .lRoomHF = SData(11)}
            End Sub

            <Runtime.Serialization.OnSerializing>
            Private Sub Serializing(context As Runtime.Serialization.StreamingContext)
                'Save values for deserialization
                SerializationData = FXItem.flDecayHFRatio & ";" & FXItem.flDecayTime & ";" & FXItem.flDensity & ";" & FXItem.flDiffusion & ";" &
                    FXItem.flHFReference & ";" & FXItem.flReflectionsDelay & ";" & FXItem.flReverbDelay & ";" & FXItem.flRoomRolloffFactor & ";" &
                    FXItem.lReflections & ";" & FXItem.lReverb & ";" & FXItem.lRoom & ";" & FXItem.lRoomHF
                HEffect = 0
            End Sub
#End Region
        End Class

        <Serializable>
        Public Class DynamicAmplification
            Inherits Profile.AudioEffect

            Public Overrides Property Name As String = "Dynamic Amplification"

            Public Overrides Property Description As String = "Dynamic Amplification"

            Public Overrides Property ShortName As String = "DAMP"

            Public Overrides Property Category As String = "BASS_FX;DSP"

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

            Private _Priority As Integer
            Public Overrides Property Priority As Integer
                Get
                    Return _Priority
                End Get
                Set(value As Integer)
                    Bass.BASS_FXSetPriority(HEffect, value)
                    _Priority = value
                    OnPropertyChanged()
                End Set
            End Property

            <MethodGroup(DisplayName:="Presets", GridStackWrap:=False, HorizontalItemSpacing:=10, VerticalItemSpacing:=10, Orientation:=Orientation.Horizontal)>
            Property Group0

            <NonSerialized> Private FXItem As Un4seen.Bass.AddOn.Fx.BASS_BFX_DAMP

            Public Overrides Sub Apply(Optional Force As Boolean = False)
                If Not Force Then If HEffect <> 0 Then Return
                If FXItem Is Nothing Then
                    FXItem = New Un4seen.Bass.AddOn.Fx.BASS_BFX_DAMP
                    FXItem.Preset_Soft()
                End If
                HEffect = Bass.BASS_ChannelSetFX(HStream, BASSFXType.BASS_FX_BFX_DAMP, Priority)
                _IsEnabled = Bass.BASS_FXSetParameters(HEffect, FXItem)
                If HEffect <> 0 Then _IsEnabled = True
                OnPropertyChanged(NameOf(IsEnabled))
                NotifyUI()
            End Sub

            Sub Update()
                If IsEnabled Then
                    Bass.BASS_FXSetParameters(HEffect, FXItem)
                    NotifyUI()
                End If
            End Sub

            <Field("Delay", GetType(Single), "Milliseconds", "ms", Minimum:=0, Maximum:=500, Description:="Delay in seconds before increasing level (0...n, linear). Default = 0.")>
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

            <Field("Gain", GetType(Single), "Level", "Lv.", Minimum:=0, Maximum:=20, Description:="Amplification level (0...1...n, linear). Default = 0.")>
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

            <Field("Quiet", GetType(Single), "Level", "Lv.", Minimum:=0, Maximum:=1, Description:="Quiet volume level (0...1, linear). Default = 0.")>
            Public Property Quiet As Single
                Get
                    Return FXItem.fQuiet
                End Get
                Set(value As Single)
                    FXItem.fQuiet = value
                    If IsEnabled Then
                        If Not Bass.BASS_FXSetParameters(HEffect, FXItem) Then
                            Threading.Thread.Sleep(100)
                            Bass.BASS_FXSetParameters(HEffect, FXItem)
                        End If
                        OnPropertyChanged()
                    End If
                End Set
            End Property

            <Field("Rate", GetType(Single), Minimum:=0, Maximum:=1, Description:="Amplification adjustment rate (0...1, linear), e.g. 0.02. Default = 0.")>
            Public Property Rate As Single
                Get
                    Return FXItem.fRate
                End Get
                Set(value As Single)
                    FXItem.fRate = value
                    If IsEnabled Then
                        If Not Bass.BASS_FXSetParameters(HEffect, FXItem) Then
                            Threading.Thread.Sleep(100)
                            Bass.BASS_FXSetParameters(HEffect, FXItem)
                        End If
                        OnPropertyChanged()
                    End If
                End Set
            End Property

            <Field("Target", GetType(Single), "Decibels", "dB", Minimum:=0, Maximum:=1, Description:="Target volume level (0<...1, linear). Default = 1.0 (0dB).")>
            Public Property Target As Single
                Get
                    Return FXItem.fTarget
                End Get
                Set(value As Single)
                    FXItem.fTarget = value
                    If IsEnabled Then
                        If Not Bass.BASS_FXSetParameters(HEffect, FXItem) Then
                            Threading.Thread.Sleep(100)
                            Bass.BASS_FXSetParameters(HEffect, FXItem)
                        End If
                        OnPropertyChanged()
                    End If
                End Set
            End Property

            <Field("Channel", GetType(AddOn.Fx.BASSFXChan), "Channel", "Ch.", Description:="Define on which channels to apply the effect. Default: -1(BASS_BFX_CHANALL - all channels.)")>
            Public Property Channel As AddOn.Fx.BASSFXChan
                Get
                    Return FXItem.lChannel
                End Get
                Set(value As AddOn.Fx.BASSFXChan)
                    FXItem.lChannel = value
                    If IsEnabled Then
                        If Not Bass.BASS_FXSetParameters(HEffect, FXItem) Then
                            Threading.Thread.Sleep(100)
                            Bass.BASS_FXSetParameters(HEffect, FXItem)
                        End If
                        OnPropertyChanged()
                    End If
                End Set
            End Property

            Private Sub NotifyUI()
                OnPropertyChanged(NameOf(Delay))
                OnPropertyChanged(NameOf(Gain))
                OnPropertyChanged(NameOf(Quiet))
                OnPropertyChanged(NameOf(Rate))
                OnPropertyChanged(NameOf(Target))
                OnPropertyChanged(NameOf(Channel))
            End Sub

            <Method("Default", "Sets the instance members to a preset.", Group:="Presets")>
            Sub Preset_Default()
                FXItem = New AddOn.Fx.BASS_BFX_DAMP
                Update()
            End Sub

            <Method("Soft", "Sets the instance members to a preset.", Group:="Presets")>
            Sub Preset_Soft()
                FXItem.Preset_Soft()
                Update()
            End Sub

            <Method("Medium", "Sets the instance members to a preset.", Group:="Presets")>
            Sub Preset_Medium()
                FXItem.Preset_Medium()
                Update()
            End Sub

            <Method("Hard", "Sets the instance members to a preset.", Group:="Presets")>
            Sub Preset_Hard()
                FXItem.Preset_Hard()
                Update()
            End Sub

#Region "Serialization"
            Private SerializationData As String

            <Runtime.Serialization.OnDeserialized>
            Private Sub Deserialization(context As Runtime.Serialization.StreamingContext)
                'Restore values from serialization
                Dim SData = SerializationData.Split(";")
                FXItem = New AddOn.Fx.BASS_BFX_DAMP With {.fDelay = SData(0), .fGain = SData(1), .fQuiet = SData(2), .fRate = SData(3), .fTarget = SData(4),
                    .lChannel = SData(5)}
            End Sub

            <Runtime.Serialization.OnSerializing>
            Private Sub Serializing(context As Runtime.Serialization.StreamingContext)
                'Save values for deserialization
                SerializationData = FXItem.fDelay & ";" & FXItem.fGain & ";" & FXItem.fQuiet & ";" & FXItem.fRate & ";" & FXItem.fTarget & ";" &
                    FXItem.lChannel
                HEffect = 0
            End Sub
#End Region
        End Class

        <Serializable>
        Public Class Compressor
            Inherits Profile.AudioEffect

            Public Overrides Property Name As String = "Compressor"

            Public Overrides Property Description As String = "A compressor is used to reduce a signal's dynamic range that is, to reduce the difference in level between the loudest and quietest parts of an audio signal."

            Public Overrides Property ShortName As String = "CMP"

            Public Overrides Property Category As String = "DX8;DSP"

            Private _IsEnabled As Boolean

            Public Overrides Property IsEnabled As Boolean
                Get
                    Return _IsEnabled
                End Get
                Set(value As Boolean)
                    If value = _IsEnabled Then Return
                    If value Then
                        HEffect = Bass.BASS_ChannelSetFX(HStream, BASSFXType.BASS_FX_DX8_COMPRESSOR, Priority)
                        _IsEnabled = Bass.BASS_FXSetParameters(HEffect, FXItem)
                    Else
                        _IsEnabled = Not Bass.BASS_ChannelRemoveFX(HStream, HEffect)
                        If Not _IsEnabled Then
                            HEffect = 0
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

            Private _Priority As Integer
            Public Overrides Property Priority As Integer
                Get
                    Return _Priority
                End Get
                Set(value As Integer)
                    Bass.BASS_FXSetPriority(HEffect, value)
                    _Priority = value
                    OnPropertyChanged()
                End Set
            End Property

            <Field("Attack", GetType(Single), "Milliseconds", "ms", Minimum:=0.01, Maximum:=500, Description:="Time in ms before compression reaches its full value.")>
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

            <Field("Gain", GetType(Single), "Decibels", "dB", Minimum:=-60, Maximum:=60, Description:="Output gain of signal in dB after compression.")>
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

            <Field("Predelay", GetType(Single), "Milliseconds", "ms", Minimum:=0, Maximum:=4, Description:="Time in ms after fThreshold is reached before attack phase is started.")>
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

            <Field("Ratio", GetType(Single), "Percentage", "%", Minimum:=1, Maximum:=100, Description:="Compression ratio.")>
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

            <Field("Release", GetType(Single), "Milliseconds", "ms", Minimum:=50, Maximum:=3000, Description:="Time (speed) in ms at which compression is stopped after input drops below fThreshold.")>
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

            <Field("Threshold", GetType(Single), "Decibels", "dB", Minimum:=-60, Maximum:=0, Description:="Point at which compression begins, in dB.")>
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

            <NonSerialized> Private FXItem As New Un4seen.Bass.BASS_DX8_COMPRESSOR

            Sub New()
                FXItem.Preset_Default()
            End Sub

            Public Overrides Sub Apply(Optional Force As Boolean = False)
                If Not Force Then If HEffect <> 0 Then Return
                HEffect = Bass.BASS_ChannelSetFX(HStream, BASSFXType.BASS_FX_DX8_COMPRESSOR, Priority)
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

#Region "Serialization"
            Private SerializationData As String

            <Runtime.Serialization.OnDeserialized>
            Private Sub Deserialization(context As Runtime.Serialization.StreamingContext)
                'Restore values from serialization
                Dim SData = SerializationData.Split(";")
                FXItem = New BASS_DX8_COMPRESSOR With {.fAttack = SData(0), .fGain = SData(1), .fPredelay = SData(2), .fRatio = SData(3), .fRelease = SData(4), .fThreshold = SData(5)}
            End Sub

            <Runtime.Serialization.OnSerializing>
            Private Sub Serializing(context As Runtime.Serialization.StreamingContext)
                'Save values for deserialization
                SerializationData = FXItem.fAttack & ";" & FXItem.fGain & ";" & FXItem.fPredelay & ";" & FXItem.fRatio & ";" & FXItem.fRelease & ";" & FXItem.fThreshold
                HEffect = 0
            End Sub
#End Region
        End Class

        <Serializable>
        Public Class SingleBandEqualizer
            Inherits Profile.AudioEffect

            Public Overrides Property Name As String = "Single Band Equalizer"

            Public Overrides Property Description As String = "1 band equalizer."

            Public Overrides Property ShortName As String = "sEQ"

            Public Overrides Property Category As String = "DX8;DSP"

            Private _IsEnabled As Boolean

            Public Overrides Property IsEnabled As Boolean
                Get
                    Return _IsEnabled
                End Get
                Set(value As Boolean)
                    If value = _IsEnabled Then Return
                    If value Then
                        HEffect = Bass.BASS_ChannelSetFX(HStream, BASSFXType.BASS_FX_DX8_PARAMEQ, Priority)
                        _IsEnabled = Bass.BASS_FXSetParameters(HEffect, FXItem)
                    Else
                        _IsEnabled = Not Bass.BASS_ChannelRemoveFX(HStream, HEffect)
                        If Not _IsEnabled Then
                            HEffect = 0
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

            Private _Priority As Integer
            Public Overrides Property Priority As Integer
                Get
                    Return _Priority
                End Get
                Set(value As Integer)
                    Bass.BASS_FXSetPriority(HEffect, value)
                    _Priority = value
                    OnPropertyChanged()
                End Set
            End Property

            <Field("Center", GetType(Single), "Hertz", "Hz", Description:="Center frequency, in hertz, in the range from 80 to 16000. This value cannot exceed one-third of the frequency of the channel. Default 100 Hz.", Minimum:=80, Maximum:=18000)>
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

            <Field("Bandwidth", GetType(Single), "Semitones", "Sm.", Description:="Bandwidth, in semitones, in the range from 1 to 36. Default 18 semitones.", Minimum:=1, Maximum:=36)>
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

            <Field("Gain", GetType(Single), "Decibels", "dB", Description:="Gain, in the range from -15 to 15. Default 0 dB.", Minimum:=-15, Maximum:=15)>
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


            <NonSerialized> Private FXItem As New Un4seen.Bass.BASS_DX8_PARAMEQ(100, 18, 0)

            Sub New()
                FXItem.Preset_Default()
            End Sub

            Public Overrides Sub Apply(Optional Force As Boolean = False)
                If Not Force Then If HEffect <> 0 Then Return
                HEffect = Bass.BASS_ChannelSetFX(HStream, BASSFXType.BASS_FX_DX8_PARAMEQ, Priority)
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

#Region "Serialization"
            Private SerializationData As String

            <Runtime.Serialization.OnDeserialized>
            Private Sub Deserialization(context As Runtime.Serialization.StreamingContext)
                'Restore values from serialization
                Dim SData = SerializationData.Split(";")
                FXItem = New BASS_DX8_PARAMEQ With {.fBandwidth = SData(0), .fCenter = SData(1), .fGain = SData(2)}
            End Sub

            <Runtime.Serialization.OnSerializing>
            Private Sub Serializing(context As Runtime.Serialization.StreamingContext)
                'Save values for deserialization
                SerializationData = FXItem.fBandwidth & ";" & FXItem.fCenter & ";" & FXItem.fGain
                HEffect = 0
            End Sub
#End Region
        End Class

        <Serializable>
        Public Class MultiBandEqualizer
            Inherits Profile.AudioEffect

            Public Enum MultiBandEqualizerIndex
                Bass
                UpperBass
                MidRange
                UpperMidRange
                HighEnd
            End Enum

            Public Overrides Property Name As String = "Multi Band Equalizer"

            Public Overrides Property Description As String = "5 band equalizer."

            Public Overrides Property ShortName As String = "mEQ"

            Public Overrides Property Category As String = "DX8;DSP"

            Private _AreEnabled(4) As Boolean

            Public Overrides Property IsEnabled As Boolean
                Get
                    Return _AreEnabled.All(Function(k) k)
                End Get
                Set(value As Boolean)
                    If value = IsEnabled Then Return
                    If value Then
                        For i As Integer = 0 To 4
                            _HEffects(i) = Bass.BASS_ChannelSetFX(HStream, BASSFXType.BASS_FX_DX8_PARAMEQ, Priority)
                            _AreEnabled(i) = Bass.BASS_FXSetParameters(_HEffects(i), FXItems(i))
                        Next
                    Else
                        For i As Integer = 0 To 4
                            Dim r = Bass.BASS_ChannelRemoveFX(HStream, _HEffects(i))
                            _AreEnabled(i) = Not r
                            If r Then _HEffects(i) = 0
                        Next
                    End If
                    OnPropertyChanged()
                End Set
            End Property

            Private _HEffects(4) As Integer
            Public Overrides Property HEffect As Integer
                Get
                    Return _HEffects(Index)
                End Get
                Set(value As Integer)
                    _HEffects(Index) = value
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

            Private _Priority As Integer
            Public Overrides Property Priority As Integer
                Get
                    Return _Priority
                End Get
                Set(value As Integer)
                    Bass.BASS_FXSetPriority(HEffect, value)
                    _Priority = value
                    OnPropertyChanged()
                End Set
            End Property

            Private _Index As MultiBandEqualizerIndex
            <Field("Index", GetType(MultiBandEqualizerIndex), Description:="Selected band.")>
            Public Property Index As MultiBandEqualizerIndex
                Get
                    Return _Index
                End Get
                Set(value As MultiBandEqualizerIndex)
                    _Index = value
                    OnPropertyChanged()
                    OnPropertyChanged(NameOf(Center))
                    OnPropertyChanged(NameOf(Bandwidth))
                    OnPropertyChanged(NameOf(Gain))
                End Set
            End Property

            <Field("Center", GetType(Single), "Hertz", "Hz", Description:="Center frequency, in hertz, in the range from 80 to 16000. This value cannot exceed one-third of the frequency of the channel. Default 100 Hz.", Minimum:=80, Maximum:=18000)>
            Public Property Center As Single
                Get
                    Return FXItems(Index).fCenter
                End Get
                Set(value As Single)
                    FXItems(Index).fCenter = value
                    If _AreEnabled(Index) Then
                        If Not Bass.BASS_FXSetParameters(HEffect, FXItems(Index)) Then
                            Threading.Thread.Sleep(100)
                            Bass.BASS_FXSetParameters(HEffect, FXItems(Index))
                        End If
                        OnPropertyChanged()
                    End If
                End Set
            End Property

            <Field("Bandwidth", GetType(Single), "Semitones", "Sm.", Description:="Bandwidth, in semitones, in the range from 1 to 36. Default 18 semitones.", Minimum:=1, Maximum:=36)>
            Public Property Bandwidth As Single
                Get
                    Return FXItems(Index).fBandwidth
                End Get
                Set(value As Single)
                    FXItems(Index).fBandwidth = value
                    If _AreEnabled(Index) Then
                        If Not Bass.BASS_FXSetParameters(HEffect, FXItems(Index)) Then
                            Threading.Thread.Sleep(100)
                            Bass.BASS_FXSetParameters(HEffect, FXItems(Index))
                        End If
                        OnPropertyChanged()
                    End If
                End Set
            End Property

            <Field("Gain", GetType(Single), "Decibels", "dB", Description:="Gain, in the range from -15 to 15. Default 0 dB.", Minimum:=-15, Maximum:=15)>
            Public Property Gain As Single
                Get
                    Return FXItems(Index).fGain
                End Get
                Set(value As Single)
                    FXItems(Index).fGain = value
                    If _AreEnabled(Index) Then
                        If Not Bass.BASS_FXSetParameters(HEffect, FXItems(Index)) Then
                            Threading.Thread.Sleep(100)
                            Bass.BASS_FXSetParameters(HEffect, FXItems(Index))
                        End If
                        OnPropertyChanged()
                    End If
                End Set
            End Property

            <MethodGroup(DisplayName:="Presets", Description:="A set of premade values", GridStackWrap:=TriState.False, Height:=Double.NaN, HorizontalItemSpacing:=10, Orientation:=Orientation.Horizontal, Scroll:=False, VerticalItemSpacing:=10, Width:=Double.NaN)>
            Property Group0


            <NonSerialized> Private FXItems(4) As Un4seen.Bass.BASS_DX8_PARAMEQ

            Sub New()
                FXItems(0) = New BASS_DX8_PARAMEQ(80, 18, 0)
                FXItems(1) = New BASS_DX8_PARAMEQ(200, 18, 0)
                FXItems(2) = New BASS_DX8_PARAMEQ(1000, 18, 0)
                FXItems(3) = New BASS_DX8_PARAMEQ(5000, 18, 0)
                FXItems(4) = New BASS_DX8_PARAMEQ(10000, 18, 0)
            End Sub

            Public Overrides Sub Apply(Optional Force As Boolean = False)
                If Not Force Then If _HEffects.All(Function(k) k <> 0) Then Return
                Dim i = 0
                If Force OrElse _HEffects(i) = 0 Then
                    _HEffects(i) = Bass.BASS_ChannelSetFX(HStream, BASSFXType.BASS_FX_DX8_PARAMEQ, Priority)
                    _AreEnabled(i) = Bass.BASS_FXSetParameters(_HEffects(i), FXItems(i))
                End If
                i = 1
                If Force OrElse _HEffects(i) = 0 Then
                    _HEffects(i) = Bass.BASS_ChannelSetFX(HStream, BASSFXType.BASS_FX_DX8_PARAMEQ, Priority)
                    _AreEnabled(i) = Bass.BASS_FXSetParameters(_HEffects(i), FXItems(i))
                End If
                i = 2
                If Force OrElse _HEffects(i) = 0 Then
                    _HEffects(i) = Bass.BASS_ChannelSetFX(HStream, BASSFXType.BASS_FX_DX8_PARAMEQ, Priority)
                    _AreEnabled(i) = Bass.BASS_FXSetParameters(_HEffects(i), FXItems(i))
                End If
                i = 3
                If Force OrElse _HEffects(i) = 0 Then
                    _HEffects(i) = Bass.BASS_ChannelSetFX(HStream, BASSFXType.BASS_FX_DX8_PARAMEQ, Priority)
                    _AreEnabled(i) = Bass.BASS_FXSetParameters(_HEffects(i), FXItems(i))
                End If
                i = 4
                If Force OrElse _HEffects(i) = 0 Then
                    _HEffects(i) = Bass.BASS_ChannelSetFX(HStream, BASSFXType.BASS_FX_DX8_PARAMEQ, Priority)
                    _AreEnabled(i) = Bass.BASS_FXSetParameters(_HEffects(i), FXItems(i))
                End If
                OnPropertyChanged(NameOf(IsEnabled))
                NotifyUI()
            End Sub

            Private Sub NotifyUI()
                OnPropertyChanged(NameOf(Index))
                OnPropertyChanged(NameOf(Center))
                OnPropertyChanged(NameOf(Gain))
                OnPropertyChanged(NameOf(Bandwidth))
            End Sub

            Sub Update()
                For i As Integer = 0 To 4
                    If _AreEnabled(i) Then
                        Dim r = Bass.BASS_FXSetParameters(_HEffects(i), FXItems(i))
                    End If
                Next
                NotifyUI()
            End Sub

            <Method("Default", "Sets the instance members to a preset.", Group:="Presets")>
            Sub Preset_Default()
                FXItems(0) = New BASS_DX8_PARAMEQ(80, 18, 0)
                FXItems(1) = New BASS_DX8_PARAMEQ(200, 18, 0)
                FXItems(2) = New BASS_DX8_PARAMEQ(1000, 18, 0)
                FXItems(3) = New BASS_DX8_PARAMEQ(5000, 18, 0)
                FXItems(4) = New BASS_DX8_PARAMEQ(10000, 18, 0)
                Update()
            End Sub

            <Method("Bass", "Sets the instance members to a preset.", Group:="Presets")>
            Sub Preset_Bass()
                FXItems(0) = New BASS_DX8_PARAMEQ(80, 18, 4)
                FXItems(1) = New BASS_DX8_PARAMEQ(200, 18, 6)
                FXItems(2) = New BASS_DX8_PARAMEQ(1000, 18, -1)
                FXItems(3) = New BASS_DX8_PARAMEQ(5000, 18, -1)
                FXItems(4) = New BASS_DX8_PARAMEQ(10000, 18, 1)
                Update()
            End Sub

            <Method("Treble", "Sets the instance members to a preset.", Group:="Presets")>
            Sub Preset_Treble()
                FXItems(0) = New BASS_DX8_PARAMEQ(80, 18, 0)
                FXItems(1) = New BASS_DX8_PARAMEQ(200, 18, 1)
                FXItems(2) = New BASS_DX8_PARAMEQ(1000, 18, 1)
                FXItems(3) = New BASS_DX8_PARAMEQ(5000, 18, 6)
                FXItems(4) = New BASS_DX8_PARAMEQ(10000, 18, 4)
                Update()
            End Sub

            <Method("Party", "Sets the instance members to a preset.", Group:="Presets")>
            Sub Preset_Party()
                FXItems(0) = New BASS_DX8_PARAMEQ(80, 18, 5.5)
                FXItems(1) = New BASS_DX8_PARAMEQ(200, 18, 0)
                FXItems(2) = New BASS_DX8_PARAMEQ(1000, 18, 0)
                FXItems(3) = New BASS_DX8_PARAMEQ(5000, 18, 0)
                FXItems(4) = New BASS_DX8_PARAMEQ(10000, 18, 5.5)
                Update()
            End Sub

            <Method("Club", "Sets the instance members to a preset.", Group:="Presets")>
            Sub Preset_Club()
                FXItems(0) = New BASS_DX8_PARAMEQ(80, 18, 0)
                FXItems(1) = New BASS_DX8_PARAMEQ(120, 18, 2)
                FXItems(2) = New BASS_DX8_PARAMEQ(500, 18, 5)
                FXItems(3) = New BASS_DX8_PARAMEQ(2000, 18, 5)
                FXItems(4) = New BASS_DX8_PARAMEQ(10000, 18, 0)
                Update()
            End Sub

#Region "Serialization"
            Private SerializationData As String

            <Runtime.Serialization.OnDeserialized>
            Private Sub Deserialization(context As Runtime.Serialization.StreamingContext)
                'Restore values from serialization
                Dim SData = SerializationData.Split(";")
                FXItems = {Nothing, Nothing, Nothing, Nothing, Nothing}
                Try
                    FXItems(0) = New BASS_DX8_PARAMEQ(SData(0), SData(1), SData(2))
                    FXItems(1) = New BASS_DX8_PARAMEQ(SData(3), SData(4), SData(5))
                    FXItems(2) = New BASS_DX8_PARAMEQ(SData(6), SData(7), SData(8))
                    FXItems(3) = New BASS_DX8_PARAMEQ(SData(9), SData(10), SData(11))
                    FXItems(4) = New BASS_DX8_PARAMEQ(SData(12), SData(13), SData(14))
                Catch ex As Exception
                    Utilities.DebugMode.Instance?.Log(Of MultiBandEqualizer)(ex.ToString)
                    FXItems(0) = New BASS_DX8_PARAMEQ(80, 18, 0)
                    FXItems(1) = New BASS_DX8_PARAMEQ(200, 18, 0)
                    FXItems(2) = New BASS_DX8_PARAMEQ(1000, 18, 0)
                    FXItems(3) = New BASS_DX8_PARAMEQ(5000, 18, 0)
                    FXItems(4) = New BASS_DX8_PARAMEQ(10000, 18, 0)
                End Try
            End Sub

            <Runtime.Serialization.OnSerializing>
            Private Sub Serializing(context As Runtime.Serialization.StreamingContext)
                'Save values for deserialization
                SerializationData = ""
                For i As Integer = 0 To 4
                    SerializationData &= FXItems(i).fBandwidth & ";" & FXItems(i).fCenter & ";" & FXItems(i).fGain & If(i < 4, ";", "")
                Next
                _HEffects = {0, 0, 0, 0, 0}
            End Sub
#End Region
        End Class

        <Serializable>
        Public Class BassBoost
            Inherits Profile.AudioEffect

            Public Overrides Property Name As String = "Bass Boost"

            Public Overrides Property Description As String = "Boosts low frequencies."

            Public Overrides Property ShortName As String = "BASS"

            Public Overrides Property Category As String = "DX8;DSP"

            Private _IsEnabled As Boolean

            Public Overrides Property IsEnabled As Boolean
                Get
                    Return _IsEnabled
                End Get
                Set(value As Boolean)
                    If value = _IsEnabled Then Return
                    If value Then
                        HEffect = Bass.BASS_ChannelSetFX(HStream, BASSFXType.BASS_FX_DX8_PARAMEQ, Priority)
                        _IsEnabled = Bass.BASS_FXSetParameters(HEffect, FXItem)
                    Else
                        _IsEnabled = Not Bass.BASS_ChannelRemoveFX(HStream, HEffect)
                        If Not _IsEnabled Then
                            HEffect = 0
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

            Private _Priority As Integer
            Public Overrides Property Priority As Integer
                Get
                    Return _Priority
                End Get
                Set(value As Integer)
                    Bass.BASS_FXSetPriority(HEffect, value)
                    _Priority = value
                    OnPropertyChanged()
                End Set
            End Property

            <Field("Gain", GetType(Single), "Decibels", "dB", Description:="Gain, in the range from -15 to 15. Default 0 dB.", Minimum:=-15, Maximum:=15)>
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


            <NonSerialized> Private FXItem As New Un4seen.Bass.BASS_DX8_PARAMEQ(90, 36, 0)

            Sub New()
                ' FXItem.Preset_Default()
            End Sub

            Public Overrides Sub Apply(Optional Force As Boolean = False)
                If Not Force Then If HEffect <> 0 Then Return
                HEffect = Bass.BASS_ChannelSetFX(HStream, BASSFXType.BASS_FX_DX8_PARAMEQ, Priority)
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

#Region "Serialization"
            Private SerializationData As String

            <Runtime.Serialization.OnDeserialized>
            Private Sub Deserialization(context As Runtime.Serialization.StreamingContext)
                'Restore values from serialization
                Dim SData = SerializationData.Split(";")
                FXItem = New BASS_DX8_PARAMEQ With {.fBandwidth = SData(0), .fCenter = SData(1), .fGain = SData(2)}
            End Sub

            <Runtime.Serialization.OnSerializing>
            Private Sub Serializing(context As Runtime.Serialization.StreamingContext)
                'Save values for deserialization
                SerializationData = FXItem.fBandwidth & ";" & FXItem.fCenter & ";" & FXItem.fGain
                HEffect = 0
            End Sub
#End Region
        End Class

        <Serializable>
        Public Class TrebleBoost
            Inherits Profile.AudioEffect

            Public Overrides Property Name As String = "Treble Boost"

            Public Overrides Property Description As String = "Boosts high frequencies."

            Public Overrides Property ShortName As String = "TRBL"

            Public Overrides Property Category As String = "DX8;DSP"

            Private _IsEnabled As Boolean

            Public Overrides Property IsEnabled As Boolean
                Get
                    Return _IsEnabled
                End Get
                Set(value As Boolean)
                    If value = _IsEnabled Then Return
                    If value Then
                        HEffect = Bass.BASS_ChannelSetFX(HStream, BASSFXType.BASS_FX_DX8_PARAMEQ, Priority)
                        _IsEnabled = Bass.BASS_FXSetParameters(HEffect, FXItem)
                    Else
                        _IsEnabled = Not Bass.BASS_ChannelRemoveFX(HStream, HEffect)
                        If Not _IsEnabled Then
                            HEffect = 0
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

            Private _Priority As Integer
            Public Overrides Property Priority As Integer
                Get
                    Return _Priority
                End Get
                Set(value As Integer)
                    Bass.BASS_FXSetPriority(HEffect, value)
                    _Priority = value
                    OnPropertyChanged()
                End Set
            End Property

            <Field("Gain", GetType(Single), "Decibels", "dB", Description:="Gain, in the range from -15 to 15. Default 0 dB.", Minimum:=-15, Maximum:=15)>
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


            <NonSerialized> Private FXItem As New Un4seen.Bass.BASS_DX8_PARAMEQ(10000, 36, 0)

            Sub New()

            End Sub

            Public Overrides Sub Apply(Optional Force As Boolean = False)
                If Not Force Then If HEffect <> 0 Then Return
                HEffect = Bass.BASS_ChannelSetFX(HStream, BASSFXType.BASS_FX_DX8_PARAMEQ, Priority)
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

#Region "Serialization"
            Private SerializationData As String

            <Runtime.Serialization.OnDeserialized>
            Private Sub Deserialization(context As Runtime.Serialization.StreamingContext)
                'Restore values from serialization
                Dim SData = SerializationData.Split(";")
                FXItem = New BASS_DX8_PARAMEQ With {.fBandwidth = SData(0), .fCenter = SData(1), .fGain = SData(2)}
            End Sub

            <Runtime.Serialization.OnSerializing>
            Private Sub Serializing(context As Runtime.Serialization.StreamingContext)
                'Save values for deserialization
                SerializationData = FXItem.fBandwidth & ";" & FXItem.fCenter & ";" & FXItem.fGain
                HEffect = 0
            End Sub
#End Region
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

            Public Overrides Property Priority As Integer
                Get
                    Return -1
                End Get
                Set(value As Integer)
                End Set
            End Property

            <Field("Frequency", GetType(Single), "Hertz", "Hz", Description:="The stream's fequency", Minimum:=100, Maximum:=100000)>
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

            Public Overrides Sub Apply(Optional Force As Boolean = False)
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

            Public Overrides Property Priority As Integer
                Get
                    Return -1
                End Get
                Set(value As Integer)
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

            Public Overrides Sub Apply(Optional Force As Boolean = False)
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

        <Serializable>
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

            Public Overrides Property Category As String = "WaDSP;AddOn"

            Private _IsEnabled As Boolean

            Public Overrides Property IsEnabled As Boolean
                Get
                    Return _IsEnabled
                End Get
                Set(value As Boolean)
                    If value = _IsEnabled Then Return
                    If value Then
                        Dim h = BassWaDsp.BASS_WADSP_ChannelSetDSP(HPlugin, HStream, Priority)
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

            Private _Priority As Integer
            Public Overrides Property Priority As Integer
                Get
                    Return _Priority
                End Get
                Set(value As Integer)
                    _Priority = value
                    IsEnabled = Not IsEnabled
                    IsEnabled = Not IsEnabled
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

            Public Overrides Sub Apply(Optional Force As Boolean = False)
                HEffect = BassWaDsp.BASS_WADSP_ChannelSetDSP(HPlugin, HStream, Priority)
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

        <Serializable>
        Public Class Reverb
            Inherits Profile.AudioEffect

            Public Overrides Property Name As String = "Reverb"

            Public Overrides Property Description As String = "Reverberation."

            Public Overrides Property ShortName As String = "RVB"

            Public Overrides Property Category As String = "DX8;DSP"

            Private _IsEnabled As Boolean
            Public Overrides Property IsEnabled As Boolean
                Get
                    Return _IsEnabled
                End Get
                Set(value As Boolean)
                    If value = _IsEnabled Then Return
                    If value Then
                        HEffect = Bass.BASS_ChannelSetFX(HStream, BASSFXType.BASS_FX_DX8_REVERB, Priority)
                        _IsEnabled = Bass.BASS_FXSetParameters(HEffect, FXItem)
                    Else
                        _IsEnabled = Not Bass.BASS_ChannelRemoveFX(HStream, HEffect)
                        If Not _IsEnabled Then
                            HEffect = 0
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

            Private _Priority As Integer
            Public Overrides Property Priority As Integer
                Get
                    Return _Priority
                End Get
                Set(value As Integer)
                    Bass.BASS_FXSetPriority(HEffect, value)
                    _Priority = value
                    OnPropertyChanged()
                End Set
            End Property

            <Field("HighFreqRTRatio", GetType(Single), Minimum:=0.001, Maximum:=0.999, Description:="In the range from 0.001 through 0.999. The default value is 0.001.")>
            Public Property HighFreqRTRatio As Single
                Get
                    Return FXItem.fHighFreqRTRatio
                End Get
                Set(value As Single)
                    FXItem.fHighFreqRTRatio = value
                    If IsEnabled Then
                        If Not Bass.BASS_FXSetParameters(HEffect, FXItem) Then
                            Threading.Thread.Sleep(100)
                            Bass.BASS_FXSetParameters(HEffect, FXItem)
                        End If
                        OnPropertyChanged()
                    End If
                End Set
            End Property
            <Field("Input Gain", GetType(Single), "Decibels", "dB", Minimum:=-96, Maximum:=0, Description:="Input gain of signal, in decibels (dB), in the range from -96 through 0. The default value is 0 dB.")>
            Public Property InGain As Single
                Get
                    Return FXItem.fInGain
                End Get
                Set(value As Single)
                    FXItem.fInGain = value
                    If IsEnabled Then
                        If Not Bass.BASS_FXSetParameters(HEffect, FXItem) Then
                            Threading.Thread.Sleep(100)
                            Bass.BASS_FXSetParameters(HEffect, FXItem)
                        End If
                        OnPropertyChanged()
                    End If
                End Set
            End Property
            <Field("Mix", GetType(Single), "Decibels", "dB", Minimum:=-96, Maximum:=0, Description:="Reverb mix, in dB, in the range from -96 through 0. The default value is 0 dB.")>
            Public Property ReverbMix As Single
                Get
                    Return FXItem.fReverbMix
                End Get
                Set(value As Single)
                    FXItem.fReverbMix = value
                    If IsEnabled Then
                        If Not Bass.BASS_FXSetParameters(HEffect, FXItem) Then
                            Threading.Thread.Sleep(100)
                            Bass.BASS_FXSetParameters(HEffect, FXItem)
                        End If
                        OnPropertyChanged()
                    End If
                End Set
            End Property
            <Field("Time", GetType(Single), "Milliseconds", "ms", Minimum:=0.001, Maximum:=3000, Description:="Reverb time, in milliseconds, in the range from 0.001 through 3000. The default value is 1000.")>
            Public Property ReverbTime As Single
                Get
                    Return FXItem.fReverbTime
                End Get
                Set(value As Single)
                    FXItem.fReverbTime = value
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

            <NonSerialized> Private FXItem As New Un4seen.Bass.BASS_DX8_REVERB

            Sub New()
                FXItem.Preset_Default()
            End Sub

            Public Overrides Sub Apply(Optional Force As Boolean = False)
                If Not Force Then If HEffect <> 0 Then Return
                If FXItem Is Nothing Then
                    FXItem = New BASS_DX8_REVERB
                    FXItem.Preset_Default()
                End If
                HEffect = Bass.BASS_ChannelSetFX(HStream, BASSFXType.BASS_FX_DX8_REVERB, Priority)
                _IsEnabled = Bass.BASS_FXSetParameters(HEffect, FXItem)
                OnPropertyChanged(NameOf(IsEnabled))
                NotifyUI()
            End Sub

            Private Sub NotifyUI()
                OnPropertyChanged(NameOf(HighFreqRTRatio))
                OnPropertyChanged(NameOf(InGain))
                OnPropertyChanged(NameOf(ReverbMix))
                OnPropertyChanged(NameOf(ReverbTime))
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
                FXItem?.Preset_Default()
                Update()
            End Sub

#Region "Serialization"
            Private SerializationData As String

            <Runtime.Serialization.OnDeserialized>
            Private Sub Deserialization(context As Runtime.Serialization.StreamingContext)
                'Restore values from serialization
                Dim SData = SerializationData.Split(";")
                FXItem = New BASS_DX8_REVERB With {.fHighFreqRTRatio = SData(0), .fInGain = SData(1), .fReverbMix = SData(2), .fReverbTime = SData(3)}
            End Sub

            <Runtime.Serialization.OnSerializing>
            Private Sub Serializing(context As Runtime.Serialization.StreamingContext)
                'Save values for deserialization
                SerializationData = FXItem.fHighFreqRTRatio & ";" & FXItem.fInGain & ";" & FXItem.fReverbMix & ";" & FXItem.fReverbTime
                HEffect = 0
            End Sub
#End Region
        End Class

        <Serializable>
        Public Class Mix
            Inherits Profile.AudioEffect

            Public Overrides Property Name As String = "Mix"

            Public Overrides Property Description As String = "Channel Swap/Remap/Downmix"

            Public Overrides Property ShortName As String = "MIX"

            Public Overrides Property Category As String = "BASS_FX;DSP"

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

            Private _Priority As Integer
            Public Overrides Property Priority As Integer
                Get
                    Return _Priority
                End Get
                Set(value As Integer)
                    Bass.BASS_FXSetPriority(HEffect, value)
                    _Priority = value
                    OnPropertyChanged()
                End Set
            End Property

            <MethodGroup(DisplayName:="Presets", GridStackWrap:=False, HorizontalItemSpacing:=10, VerticalItemSpacing:=10, Orientation:=Orientation.Horizontal)>
            Property Group0

            <NonSerialized> Private FXItem As Un4seen.Bass.AddOn.Fx.BASS_BFX_MIX

            Public Overrides Sub Apply(Optional Force As Boolean = False)
                If Not Force Then If HEffect <> 0 Then Return
                If FXItem Is Nothing Then
                    FXItem = New Un4seen.Bass.AddOn.Fx.BASS_BFX_MIX(8)
                End If
                HEffect = Bass.BASS_ChannelSetFX(HStream, BASSFXType.BASS_FX_BFX_MIX, Priority)
                _IsEnabled = Bass.BASS_FXSetParameters(HEffect, FXItem)
                If HEffect <> 0 Then _IsEnabled = True
                OnPropertyChanged(NameOf(IsEnabled))
                NotifyUI()
            End Sub

            Sub Update()
                If IsEnabled Then
                    Dim Result = Bass.BASS_FXSetParameters(HEffect, FXItem)
                    NotifyUI()
                End If
            End Sub

            Private Sub NotifyUI()

            End Sub

            <Method("Default", "Sets the instance members to a preset.", Group:="Presets")>
            Sub Preset_Default()
                FXItem = New AddOn.Fx.BASS_BFX_MIX(8)
                Update()
            End Sub
            <Method("Stereo to Mono", "Sets the instance members to a preset.", Group:="Presets")>
            Sub Preset_DownmixStereoToMono()
                FXItem = New AddOn.Fx.BASS_BFX_MIX(2)
                FXItem.lChannel(0) = AddOn.Fx.BASSFXChan.BASS_BFX_CHAN1 Or AddOn.Fx.BASSFXChan.BASS_BFX_CHAN2
                FXItem.lChannel(1) = AddOn.Fx.BASSFXChan.BASS_BFX_CHAN1 Or AddOn.Fx.BASSFXChan.BASS_BFX_CHAN2
                Update()
            End Sub
            <Method("5.1 to Stereo", "Sets the instance members to a preset.", Group:="Presets")>
            Sub Preset_DownmixFivePointOneToStereo()
                FXItem = New AddOn.Fx.BASS_BFX_MIX(6)
                FXItem.lChannel(0) = AddOn.Fx.BASSFXChan.BASS_BFX_CHAN1 Or AddOn.Fx.BASSFXChan.BASS_BFX_CHAN3 Or AddOn.Fx.BASSFXChan.BASS_BFX_CHAN5
                FXItem.lChannel(1) = AddOn.Fx.BASSFXChan.BASS_BFX_CHAN2 Or AddOn.Fx.BASSFXChan.BASS_BFX_CHAN4 Or AddOn.Fx.BASSFXChan.BASS_BFX_CHAN6
                FXItem.lChannel(2) = AddOn.Fx.BASSFXChan.BASS_BFX_CHANNONE
                FXItem.lChannel(3) = AddOn.Fx.BASSFXChan.BASS_BFX_CHANNONE
                FXItem.lChannel(4) = AddOn.Fx.BASSFXChan.BASS_BFX_CHANNONE
                FXItem.lChannel(5) = AddOn.Fx.BASSFXChan.BASS_BFX_CHANNONE
                Update()
            End Sub
            <Method("7.1 to Stereo", "Sets the instance members to a preset.", Group:="Presets")>
            Sub Preset_DownmixSevenPointOneToStereo()
                FXItem = New AddOn.Fx.BASS_BFX_MIX(8)
                FXItem.lChannel(0) = AddOn.Fx.BASSFXChan.BASS_BFX_CHAN1 Or AddOn.Fx.BASSFXChan.BASS_BFX_CHAN3 Or AddOn.Fx.BASSFXChan.BASS_BFX_CHAN5 Or AddOn.Fx.BASSFXChan.BASS_BFX_CHAN7
                FXItem.lChannel(1) = AddOn.Fx.BASSFXChan.BASS_BFX_CHAN2 Or AddOn.Fx.BASSFXChan.BASS_BFX_CHAN4 Or AddOn.Fx.BASSFXChan.BASS_BFX_CHAN6 Or AddOn.Fx.BASSFXChan.BASS_BFX_CHAN8
                FXItem.lChannel(2) = AddOn.Fx.BASSFXChan.BASS_BFX_CHANNONE
                FXItem.lChannel(3) = AddOn.Fx.BASSFXChan.BASS_BFX_CHANNONE
                FXItem.lChannel(4) = AddOn.Fx.BASSFXChan.BASS_BFX_CHANNONE
                FXItem.lChannel(5) = AddOn.Fx.BASSFXChan.BASS_BFX_CHANNONE
                FXItem.lChannel(6) = AddOn.Fx.BASSFXChan.BASS_BFX_CHANNONE
                FXItem.lChannel(7) = AddOn.Fx.BASSFXChan.BASS_BFX_CHANNONE
                Update()
            End Sub
            <Method("5.1 to Mono", "Sets the instance members to a preset.", Group:="Presets")>
            Sub Preset_DownmixFivePointOneToMono()
                FXItem = New AddOn.Fx.BASS_BFX_MIX(6)
                FXItem.lChannel(0) = AddOn.Fx.BASSFXChan.BASS_BFX_CHANALL 'AddOn.Fx.BASSFXChan.BASS_BFX_CHAN1 Or AddOn.Fx.BASSFXChan.BASS_BFX_CHAN3 Or AddOn.Fx.BASSFXChan.BASS_BFX_CHAN5 Or AddOn.Fx.BASSFXChan.BASS_BFX_CHAN2 Or AddOn.Fx.BASSFXChan.BASS_BFX_CHAN4 Or AddOn.Fx.BASSFXChan.BASS_BFX_CHAN6
                FXItem.lChannel(1) = AddOn.Fx.BASSFXChan.BASS_BFX_CHANALL
                FXItem.lChannel(2) = AddOn.Fx.BASSFXChan.BASS_BFX_CHANALL
                FXItem.lChannel(3) = AddOn.Fx.BASSFXChan.BASS_BFX_CHANALL
                FXItem.lChannel(4) = AddOn.Fx.BASSFXChan.BASS_BFX_CHANALL
                FXItem.lChannel(5) = AddOn.Fx.BASSFXChan.BASS_BFX_CHANALL
                Update()
            End Sub
            <Method("7.1 to Mono", "Sets the instance members to a preset.", Group:="Presets")>
            Sub Preset_DownmixSevenPointOneToMono()
                FXItem = New AddOn.Fx.BASS_BFX_MIX(8)
                FXItem.lChannel(0) = AddOn.Fx.BASSFXChan.BASS_BFX_CHANALL 'AddOn.Fx.BASSFXChan.BASS_BFX_CHAN1 Or AddOn.Fx.BASSFXChan.BASS_BFX_CHAN3 Or AddOn.Fx.BASSFXChan.BASS_BFX_CHAN5 Or AddOn.Fx.BASSFXChan.BASS_BFX_CHAN7 Or AddOn.Fx.BASSFXChan.BASS_BFX_CHAN2 Or AddOn.Fx.BASSFXChan.BASS_BFX_CHAN4 Or AddOn.Fx.BASSFXChan.BASS_BFX_CHAN6 Or AddOn.Fx.BASSFXChan.BASS_BFX_CHAN8
                FXItem.lChannel(1) = AddOn.Fx.BASSFXChan.BASS_BFX_CHANALL
                FXItem.lChannel(2) = AddOn.Fx.BASSFXChan.BASS_BFX_CHANALL
                FXItem.lChannel(3) = AddOn.Fx.BASSFXChan.BASS_BFX_CHANALL
                FXItem.lChannel(4) = AddOn.Fx.BASSFXChan.BASS_BFX_CHANALL
                FXItem.lChannel(5) = AddOn.Fx.BASSFXChan.BASS_BFX_CHANALL
                FXItem.lChannel(6) = AddOn.Fx.BASSFXChan.BASS_BFX_CHANALL
                FXItem.lChannel(7) = AddOn.Fx.BASSFXChan.BASS_BFX_CHANALL
                Update()
            End Sub
            <Method("Swap Stereo", "Sets the instance members to a preset.", Group:="Presets")>
            Sub Preset_SwapStereo()
                FXItem = New AddOn.Fx.BASS_BFX_MIX(2)
                FXItem.lChannel(0) = AddOn.Fx.BASSFXChan.BASS_BFX_CHAN2
                FXItem.lChannel(1) = AddOn.Fx.BASSFXChan.BASS_BFX_CHAN1
                Update()
            End Sub
            <Method("Custom", "Sets the instance members to a preset.", Group:="Presets")>
            Sub Preset_Custom()
                FXItem = New AddOn.Fx.BASS_BFX_MIX(8)
                Dim IB As New Dialogs.InputBox()
                IB.Description =
                    "Mute Channel    NONE" & Environment.NewLine &
                    "Mono	            1=left" & Environment.NewLine &
                    "Stereo	            1=left, 2=right." & Environment.NewLine &
                    "3 channels          1=left-front, 2=right-front, 3=center." & Environment.NewLine &
                    "4 channels          1=left-front, 2=right-front, 3=left-rear/side, 4=right-rear/side." & Environment.NewLine &
                    "6 channels (5.1)  1=left-front, 2=right-front, 3=center, 4=LFE, 5=left-rear/side, 6=right-rear/side." & Environment.NewLine &
                    "8 channels (7.1)  1=left-front, 2=right-front, 3=center, 4=LFE, 5=left-rear/side, 6=right-rear/side, 7=left-rear center, 8=right-rear center."
                IB.Footer = "Use channel index to select a channel, combine with "","""
                IB.AddTextBox("Left-Front", DefaultInput:="NONE")
                IB.AddTextBox("Right-Front", DefaultInput:="NONE")
                IB.AddTextBox("Center", DefaultInput:="NONE")
                IB.AddTextBox("LFE", DefaultInput:="NONE")
                IB.AddTextBox("Left-Rear", DefaultInput:="NONE")
                IB.AddTextBox("Right-Rear", DefaultInput:="NONE")
                IB.AddTextBox("Left-Rear Center", DefaultInput:="NONE")
                IB.AddTextBox("Right-Rear Center", DefaultInput:="NONE")
                If IB.ShowDialog Then
                    Dim Result As AddOn.Fx.BASSFXChan
                    Dim eResult(7) As Boolean
                    'LF
                    If System.Enum.TryParse(Of AddOn.Fx.BASSFXChan)(DelimitedIntegerToFXChanString(IB.Value("Left-Front")), Result) Then
                        FXItem.lChannel(0) = Result
                        eResult(0) = True
                    End If
                    'RF
                    If System.Enum.TryParse(Of AddOn.Fx.BASSFXChan)(DelimitedIntegerToFXChanString(IB.Value("Right-Front")), Result) Then
                        FXItem.lChannel(1) = Result
                        eResult(1) = True
                    End If
                    'Center                    
                    If System.Enum.TryParse(Of AddOn.Fx.BASSFXChan)(DelimitedIntegerToFXChanString(IB.Value("Center")), Result) Then
                        FXItem.lChannel(2) = Result
                        eResult(2) = True
                    End If
                    'LFE
                    If System.Enum.TryParse(Of AddOn.Fx.BASSFXChan)(DelimitedIntegerToFXChanString(IB.Value("LFE")), Result) Then
                        FXItem.lChannel(3) = Result
                        eResult(3) = True
                    End If
                    'LR
                    If System.Enum.TryParse(Of AddOn.Fx.BASSFXChan)(DelimitedIntegerToFXChanString(IB.Value("Left-Rear")), Result) Then
                        FXItem.lChannel(4) = Result
                        eResult(4) = True
                    End If
                    'RR
                    If System.Enum.TryParse(Of AddOn.Fx.BASSFXChan)(DelimitedIntegerToFXChanString(IB.Value("Left-Rear")), Result) Then
                        FXItem.lChannel(5) = Result
                        eResult(5) = True
                    End If
                    'LR-C                    
                    If System.Enum.TryParse(Of AddOn.Fx.BASSFXChan)(DelimitedIntegerToFXChanString(IB.Value("Left-Rear Center")), Result) Then
                        FXItem.lChannel(6) = Result
                        eResult(6) = True
                    End If
                    'RR-C
                    If System.Enum.TryParse(Of AddOn.Fx.BASSFXChan)(DelimitedIntegerToFXChanString(IB.Value("Right-Rear Center")), Result) Then
                        FXItem.lChannel(7) = Result
                        eResult(7) = True
                    End If
                    If eResult.Any(Function(k) k = False) Then
                        HandyControl.Controls.MessageBox.Error(Utilities.ResourceResolver.Strings.QUERY_ERROR_PARSE)
                    End If
                    Update()
                End If
            End Sub
            Private Function DelimitedIntegerToFXChanString(value As String) As String
                Dim result = String.Join(", ", value.Split(New Char() {","}, StringSplitOptions.RemoveEmptyEntries).Select(Of String)(Function(k) "BASS_BFX_CHAN" & k.Trim))
                Return result
            End Function

#Region "Serialization"
            Private SerializationData As String
            '
            <Runtime.Serialization.OnDeserialized>
            Private Sub Deserialization(context As Runtime.Serialization.StreamingContext)
                'Restore values from serialization
                Dim SData = SerializationData.Split(";")
                FXItem = New AddOn.Fx.BASS_BFX_MIX(8)
                If SData.Length > 0 Then FXItem.lChannel(0) = SData(0)
                If SData.Length > 1 Then FXItem.lChannel(1) = SData(1)
                If SData.Length > 2 Then FXItem.lChannel(2) = SData(2)
                If SData.Length > 3 Then FXItem.lChannel(3) = SData(3)
                If SData.Length > 4 Then FXItem.lChannel(4) = SData(4)
                If SData.Length > 5 Then FXItem.lChannel(5) = SData(5)
                If SData.Length > 6 Then FXItem.lChannel(6) = SData(6)
                If SData.Length > 7 Then FXItem.lChannel(7) = SData(7)
            End Sub

            <Runtime.Serialization.OnSerializing>
            Private Sub Serializing(context As Runtime.Serialization.StreamingContext)
                'Save values for deserialization
                SerializationData = String.Join(";", FXItem.lChannel.Select(Of String)(Function(k) CInt(k)))
                HEffect = 0
            End Sub
#End Region
        End Class

        <Serializable>
        Public Class Rotate
            Inherits Profile.AudioEffect

            Public Overrides Property Name As String = "Rotate"

            Public Overrides Property Description As String = "This is a volume rotate effect between even channels, just like 2 channels playing ping-pong between each other."

            Public Overrides Property ShortName As String = "3D"

            Public Overrides Property Category As String = "BASS_FX"

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

            Private _Priority As Integer
            Public Overrides Property Priority As Integer
                Get
                    Return _Priority
                End Get
                Set(value As Integer)
                    Bass.BASS_FXSetPriority(HEffect, value)
                    _Priority = value
                    OnPropertyChanged()
                End Set
            End Property

            <Field("Rate", GetType(Single), "Hertz", "Hz", Description:="Rotation rate/speed in Hz (A negative rate can be used for reverse direction).", Minimum:=-0.5, Maximum:=0.5)>
            Public Property Rate As Single
                Get
                    Return FXItem.fRate
                End Get
                Set(value As Single)
                    FXItem.fRate = value
                    If IsEnabled Then
                        If Not Bass.BASS_FXSetParameters(HEffect, FXItem) Then
                            Threading.Thread.Sleep(100)
                            Bass.BASS_FXSetParameters(HEffect, FXItem)
                        End If
                        OnPropertyChanged()
                    End If
                End Set
            End Property

            <Field("Channel", GetType(AddOn.Fx.BASSFXChan), "Channel", "Ch.", Description:="Define on which channels to apply the effect. Default: -1(BASS_BFX_CHANALL - all channels.)")>
            Public Property Channel As AddOn.Fx.BASSFXChan
                Get
                    Return FXItem.lChannel
                End Get
                Set(value As AddOn.Fx.BASSFXChan)
                    FXItem.lChannel = value
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

            <NonSerialized> Private FXItem As Un4seen.Bass.AddOn.Fx.BASS_BFX_ROTATE

            Public Overrides Sub Apply(Optional Force As Boolean = False)
                If Not Force Then If HEffect <> 0 Then Return
                If FXItem Is Nothing Then
                    FXItem = New Un4seen.Bass.AddOn.Fx.BASS_BFX_ROTATE(0.1, AddOn.Fx.BASSFXChan.BASS_BFX_CHANALL)
                End If
                HEffect = Bass.BASS_ChannelSetFX(HStream, BASSFXType.BASS_FX_BFX_ROTATE, Priority)
                _IsEnabled = Bass.BASS_FXSetParameters(HEffect, FXItem)
                If HEffect <> 0 Then _IsEnabled = True
                OnPropertyChanged(NameOf(IsEnabled))
                NotifyUI()
            End Sub

            Sub Update()
                If IsEnabled Then
                    Dim Result = Bass.BASS_FXSetParameters(HEffect, FXItem)
                    NotifyUI()
                End If
            End Sub

            Private Sub NotifyUI()
                OnPropertyChanged(NameOf(Rate))
                OnPropertyChanged(NameOf(Channel))
            End Sub

            <Method("Default", "Sets the instance members to a preset.", Group:="Presets")>
            Sub Preset_Default()
                FXItem = New AddOn.Fx.BASS_BFX_ROTATE
                Update()
            End Sub

            <Method("Slow", "Sets the instance members to a preset.", Group:="Presets")>
            Sub Preset_Slow()
                FXItem = New AddOn.Fx.BASS_BFX_ROTATE(0.05, AddOn.Fx.BASSFXChan.BASS_BFX_CHANALL)
                Update()
            End Sub

            <Method("Medium", "Sets the instance members to a preset.", Group:="Presets")>
            Sub Preset_Medium()
                FXItem = New AddOn.Fx.BASS_BFX_ROTATE(0.1, AddOn.Fx.BASSFXChan.BASS_BFX_CHANALL)
                Update()
            End Sub

            <Method("Fast", "Sets the instance members to a preset.", Group:="Presets")>
            Sub Preset_Fast()
                FXItem = New AddOn.Fx.BASS_BFX_ROTATE(0.3, AddOn.Fx.BASSFXChan.BASS_BFX_CHANALL)
                Update()
            End Sub

#Region "Serialization"
            Private SerializationData As String
            '
            <Runtime.Serialization.OnDeserialized>
            Private Sub Deserialization(context As Runtime.Serialization.StreamingContext)
                'Restore values from serialization
                Dim SData = SerializationData.Split(";")
                FXItem = New AddOn.Fx.BASS_BFX_ROTATE With {.fRate = SData(0), .lChannel = (1)}
            End Sub

            <Runtime.Serialization.OnSerializing>
            Private Sub Serializing(context As Runtime.Serialization.StreamingContext)
                'Save values for deserialization
                SerializationData = FXItem.fRate & ";" & FXItem.lChannel
                HEffect = 0
            End Sub
#End Region
        End Class

        <Serializable>
        Public Class StreoEnhancer
            Inherits Profile.AudioEffect

            Public Overrides Property Name As String = "Stereo Enhancer"

            Public Overrides Property Description As String = "Stereo Enhancer DSP (actually also removes mono signals)."

            Public Overrides Property ShortName As String = "SEN"

            Public Overrides Property Category As String = "DSP"

            Private _IsEnabled As Boolean

            Public Overrides Property IsEnabled As Boolean
                Get
                    Return _IsEnabled
                End Get
                Set(value As Boolean)
                    If value = _IsEnabled Then Return
                    If value Then
                        FXItem.Start()
                        _IsEnabled = True
                    Else
                        FXItem.Stop()
                        _IsEnabled = False
                    End If
                    OnPropertyChanged()
                End Set
            End Property

            Public Overrides Property HEffect As Integer
                Get
                    Return FXItem.DSPHandle
                End Get
                Set(value As Integer)
                    If value = 0 Then
                        FXItem.Stop()
                        OnPropertyChanged(NameOf(IsEnabled))
                    End If
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
                    FXItem.ChannelHandle = value
                    OnPropertyChanged(NameOf(IsEnabled))
                    OnPropertyChanged()
                End Set
            End Property

            Private _Priority As Integer
            Public Overrides Property Priority As Integer
                Get
                    Return _Priority
                End Get
                Set(value As Integer)
                    FXItem.DSPPriority = value
                    _Priority = value
                    OnPropertyChanged()
                End Set
            End Property

            <Field("Coefficient", GetType(Double), Minimum:=0, Maximum:=10, Description:="The stereo wide (coefficient) controling the stereo enhancement between 0.0 (narrow, mono signal) and 10.0 (wide, enhanced stereo signal) - default is 2.0.")>
            Public Property WideCoeff As Double
                Get
                    Return FXItem.WideCoeff
                End Get
                Set(value As Double)
                    FXItem.WideCoeff = value
                End Set
            End Property

            <Field("Wet/Dry", GetType(Double), Minimum:=0, Maximum:=1, Description:="The Wet/Dry ratio between 0.0 (dry, unprocessed signal only) and 1.0 (wet, processed signal only) - default is 0.5.")>
            Public Property WetDry As Double
                Get
                    Return FXItem.WetDry
                End Get
                Set(value As Double)
                    FXItem.WetDry = value
                End Set
            End Property

            <Field("Dithering Factor", GetType(Double), Maximum:=20, SnapToTicks:=True, Frequency:=0.25, Description:="The dithering bitdepth of the triangular probability density function (TPDF) - default is 0.7.")>
            Public Property DitherFactor As Double
                Get
                    Return FXItem.DitherFactor
                End Get
                Set(value As Double)
                    FXItem.DitherFactor = value
                End Set
            End Property

            <Field("Dithering", GetType(Boolean), Description:="If Dithering should be used (default is false).")>
            Public Property UseDithering As Boolean
                Get
                    Return FXItem.UseDithering
                End Get
                Set(value As Boolean)
                    FXItem.UseDithering = value
                End Set
            End Property

            <MethodGroup(DisplayName:="Presets", Description:="A set of premade values", GridStackWrap:=TriState.False, Height:=Double.NaN, HorizontalItemSpacing:=10, Orientation:=Orientation.Horizontal, Scroll:=False, VerticalItemSpacing:=10, Width:=Double.NaN)>
            Property Group0

            <NonSerialized> Private FXItem As New Un4seen.Bass.Misc.DSP_StereoEnhancer

            Public Overrides Sub Apply(Optional Force As Boolean = False)
                FXItem.Start()
                _IsEnabled = FXItem.IsAssigned
                OnPropertyChanged(NameOf(IsEnabled))
            End Sub

            Private Sub NotifyUI()
                OnPropertyChanged(NameOf(WideCoeff))
                OnPropertyChanged(NameOf(WetDry))
                OnPropertyChanged(NameOf(DitherFactor))
                OnPropertyChanged(NameOf(UseDithering))
                OnPropertyChanged(NameOf(IsEnabled))
            End Sub

            <Method("Default", "Sets the instance members to a preset.", Group:="Presets")>
            Sub Preset_Default()
                FXItem.WideCoeff = 2
                FXItem.UseDithering = False
                FXItem.WetDry = 0.5
                FXItem.DitherFactor = 0.7
                NotifyUI()
            End Sub

            <Method("Narrow (Mono)", "Sets the instance members to a preset.", Group:="Presets")>
            Sub Preset_Narrow()
                FXItem.WideCoeff = 0
                FXItem.UseDithering = False
                FXItem.WetDry = 0.5
                FXItem.DitherFactor = 0.7
                NotifyUI()
            End Sub

            <Method("Wide", "Sets the instance members to a preset.", Group:="Presets")>
            Sub Preset_Wide()
                FXItem.WideCoeff = 5
                FXItem.UseDithering = True
                FXItem.WetDry = 0.7
                FXItem.DitherFactor = 1.2
                NotifyUI()
            End Sub

            <Method("Vocal Remove", "Sets the instance members to a preset.", Group:="Presets")>
            Sub Preset_FullWide()
                FXItem.WideCoeff = 10
                FXItem.UseDithering = True
                FXItem.WetDry = 0.95
                FXItem.DitherFactor = 20
                NotifyUI()
            End Sub
#Region "Serialization"
            Private SerializationData As String

            <Runtime.Serialization.OnDeserialized>
            Private Sub Deserialization(context As Runtime.Serialization.StreamingContext)
                'Restore values from serialization
                Dim SData = SerializationData.Split(";")
                If SData.Length = 4 Then FXItem = New DSP_StereoEnhancer With {.WideCoeff = CDbl(SData(0)), .WetDry = CDbl(SData(1)), .DitherFactor = CDbl(SData(2)), .UseDithering = CBool(SData(3))}
            End Sub

            <Runtime.Serialization.OnSerializing>
            Private Sub Serializing(context As Runtime.Serialization.StreamingContext)
                'Save values for deserialization
                SerializationData = FXItem.WideCoeff & ";" & FXItem.WetDry & ";" & FXItem.DitherFactor & ";" & FXItem.UseDithering
                HEffect = 0
            End Sub
#End Region
        End Class

        <Serializable>
        Public Class AutoWah
            Inherits Profile.AudioEffect

            Public Overrides Property Name As String = "Auto-Wah"

            Public Overrides Property Description As String = "The effect implements the auto-wah by using 4-stage phaser effect which moves a peak in the frequency response up and down the frequency spectrum by amplitude of input signal."

            Public Overrides Property ShortName As String = "ATWH"

            Public Overrides Property Category As String = "BASS_FX;DSP"

            Private _IsEnabled As Boolean
            Public Overrides Property IsEnabled As Boolean
                Get
                    Return _IsEnabled
                End Get
                Set(value As Boolean)
                    If value = _IsEnabled Then Return
                    If value Then
                        HEffect = Bass.BASS_ChannelSetFX(HStream, BASSFXType.BASS_FX_BFX_AUTOWAH, Priority)
                        _IsEnabled = Bass.BASS_FXSetParameters(HEffect, FXItem)
                    Else
                        _IsEnabled = Not Bass.BASS_ChannelRemoveFX(HStream, HEffect)
                        If Not _IsEnabled Then
                            HEffect = 0
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

            Private _Priority As Integer
            Public Overrides Property Priority As Integer
                Get
                    Return _Priority
                End Get
                Set(value As Integer)
                    Bass.BASS_FXSetPriority(HEffect, value)
                    _Priority = value
                    OnPropertyChanged()
                End Set
            End Property

            <Field("Dry Mix", GetType(Single), Minimum:=-2, Maximum:=2, Description:="Dry (unaffected) signal mix (-2...+2). Default = 0.")>
            Public Property DryMix As Single
                Get
                    Return FXItem.fDryMix
                End Get
                Set(value As Single)
                    FXItem.fDryMix = value
                    If IsEnabled Then
                        If Not Bass.BASS_FXSetParameters(HEffect, FXItem) Then
                            Threading.Thread.Sleep(100)
                            Bass.BASS_FXSetParameters(HEffect, FXItem)
                        End If
                        OnPropertyChanged()
                    End If
                End Set
            End Property
            <Field("Feedback", GetType(Single), Minimum:=-1, Maximum:=1, Description:="Feedback (-1...+1). Default = 0.")>
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
            <Field("Frequency", GetType(Single), "Hertz", "Hz", Minimum:=0.1, Maximum:=1000, Description:="Base frequency of sweep range (0<...1000). Default = 0.")>
            Public Property Frequency As Single
                Get
                    Return FXItem.fFreq
                End Get
                Set(value As Single)
                    FXItem.fRange = value
                    If IsEnabled Then
                        If Not Bass.BASS_FXSetParameters(HEffect, FXItem) Then
                            Threading.Thread.Sleep(100)
                            Bass.BASS_FXSetParameters(HEffect, FXItem)
                        End If
                        OnPropertyChanged()
                    End If
                End Set
            End Property
            <Field("Range", GetType(Single), "Octave", "Oct", Minimum:=0.1, Maximum:=9.9, Description:="Sweep range in octaves (0<...<10). Default = 0.")>
            Public Property Range As Single
                Get
                    Return FXItem.fRange
                End Get
                Set(value As Single)
                    FXItem.fRange = value
                    If IsEnabled Then
                        If Not Bass.BASS_FXSetParameters(HEffect, FXItem) Then
                            Threading.Thread.Sleep(100)
                            Bass.BASS_FXSetParameters(HEffect, FXItem)
                        End If
                        OnPropertyChanged()
                    End If
                End Set
            End Property
            <Field("Rate", GetType(Single), "Cycles/Second", "C/s", Minimum:=0.1, Maximum:=9.9, Description:="Rate of sweep in cycles per second (0<...<10). Default = 0.")>
            Public Property Rate As Single
                Get
                    Return FXItem.fRate
                End Get
                Set(value As Single)
                    FXItem.fRate = value
                    If IsEnabled Then
                        If Not Bass.BASS_FXSetParameters(HEffect, FXItem) Then
                            Threading.Thread.Sleep(100)
                            Bass.BASS_FXSetParameters(HEffect, FXItem)
                        End If
                        OnPropertyChanged()
                    End If
                End Set
            End Property
            <Field("Wet Mix", GetType(Single), Minimum:=-2, Maximum:=2, Description:="Wet (affected) signal mix (-2...+2). Default = 0.")>
            Public Property WetMix As Single
                Get
                    Return FXItem.fWetMix
                End Get
                Set(value As Single)
                    FXItem.fWetMix = value
                    If IsEnabled Then
                        If Not Bass.BASS_FXSetParameters(HEffect, FXItem) Then
                            Threading.Thread.Sleep(100)
                            Bass.BASS_FXSetParameters(HEffect, FXItem)
                        End If
                        OnPropertyChanged()
                    End If
                End Set
            End Property
            <Field("Channel", GetType(AddOn.Fx.BASSFXChan), "Channel", "Ch.", Description:="Define on which channels to apply the effect. Default: -1(BASS_BFX_CHANALL - all channels.)")>
            Public Property Channel As AddOn.Fx.BASSFXChan
                Get
                    Return FXItem.lChannel
                End Get
                Set(value As AddOn.Fx.BASSFXChan)
                    FXItem.lChannel = value
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

            <NonSerialized> Private FXItem As New Un4seen.Bass.AddOn.Fx.BASS_BFX_AUTOWAH()

            Sub New()
                FXItem.Preset_Default()
            End Sub

            Public Overrides Sub Apply(Optional Force As Boolean = False)
                If Not Force Then If HEffect <> 0 Then Return
                HEffect = Bass.BASS_ChannelSetFX(HStream, BASSFXType.BASS_FX_BFX_AUTOWAH, Priority)
                _IsEnabled = Bass.BASS_FXSetParameters(HEffect, FXItem)
                OnPropertyChanged(NameOf(IsEnabled))
                NotifyUI()
            End Sub

            Private Sub NotifyUI()
                OnPropertyChanged(NameOf(DryMix))
                OnPropertyChanged(NameOf(Feedback))
                OnPropertyChanged(NameOf(Frequency))
                OnPropertyChanged(NameOf(Range))
                OnPropertyChanged(NameOf(Rate))
                OnPropertyChanged(NameOf(WetMix))
                OnPropertyChanged(NameOf(Channel))
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
            <Method("Fast", "Sets the instance members to a preset.", Group:="Presets")>
            Sub Preset_FastAutoWah()
                FXItem.Preset_FastAutoWah()
                Update()
            End Sub
            <Method("High Fast", "Sets the instance members to a preset.", Group:="Presets")>
            Sub Preset_HiFastAutoWah()
                FXItem.Preset_HiFastAutoWah()
                Update()
            End Sub
            <Method("Slow Fast", "Sets the instance members to a preset.", Group:="Presets")>
            Sub Preset_SlowAutoWah()
                FXItem.Preset_SlowAutoWah()
                Update()
            End Sub

#Region "Serialization"
            Private SerializationData As String

            <Runtime.Serialization.OnDeserialized>
            Private Sub Deserialization(context As Runtime.Serialization.StreamingContext)
                'Restore values from serialization
                Dim SData = SerializationData.Split(";")
                FXItem = New AddOn.Fx.BASS_BFX_AUTOWAH(SData(0), SData(5), SData(1), SData(4), SData(3), SData(2))
                FXItem.lChannel = SData(6)
            End Sub

            <Runtime.Serialization.OnSerializing>
            Private Sub Serializing(context As Runtime.Serialization.StreamingContext)
                'Save values for deserialization
                SerializationData = FXItem.fDryMix & ";" & FXItem.fFeedback & ";" & FXItem.fFreq & ";" & FXItem.fRange & ";" & FXItem.fRate & ";" & FXItem.fWetMix & ";" & CInt(FXItem.lChannel)
                HEffect = 0
            End Sub
#End Region
        End Class

        <Serializable>
        Public Class Phaser
            Inherits Profile.AudioEffect

            Public Overrides Property Name As String = "Phaser"

            Public Overrides Property Description As String = "Phasers use an internal low frequency oscillator to automatically move notches in the frequency response up and down the frequency spectrum."

            Public Overrides Property ShortName As String = "PHSR"

            Public Overrides Property Category As String = "BASS_FX;DSP"

            Private _IsEnabled As Boolean
            Public Overrides Property IsEnabled As Boolean
                Get
                    Return _IsEnabled
                End Get
                Set(value As Boolean)
                    If value = _IsEnabled Then Return
                    If value Then
                        HEffect = Bass.BASS_ChannelSetFX(HStream, BASSFXType.BASS_FX_BFX_PHASER, Priority)
                        _IsEnabled = Bass.BASS_FXSetParameters(HEffect, FXItem)
                    Else
                        _IsEnabled = Not Bass.BASS_ChannelRemoveFX(HStream, HEffect)
                        If Not _IsEnabled Then
                            HEffect = 0
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

            Private _Priority As Integer
            Public Overrides Property Priority As Integer
                Get
                    Return _Priority
                End Get
                Set(value As Integer)
                    Bass.BASS_FXSetPriority(HEffect, value)
                    _Priority = value
                    OnPropertyChanged()
                End Set
            End Property

            <Field("Dry Mix", GetType(Single), Minimum:=-2, Maximum:=2, Description:="Dry (unaffected) signal mix (-2...+2). Default = 0.")>
            Public Property DryMix As Single
                Get
                    Return FXItem.fDryMix
                End Get
                Set(value As Single)
                    FXItem.fDryMix = value
                    If IsEnabled Then
                        If Not Bass.BASS_FXSetParameters(HEffect, FXItem) Then
                            Threading.Thread.Sleep(100)
                            Bass.BASS_FXSetParameters(HEffect, FXItem)
                        End If
                        OnPropertyChanged()
                    End If
                End Set
            End Property
            <Field("Feedback", GetType(Single), Minimum:=-1, Maximum:=1, Description:="Feedback (-1...+1). Default = 0.")>
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
            <Field("Frequency", GetType(Single), "Hertz", "Hz", Minimum:=0.1, Maximum:=1000, Description:="Base frequency of sweep range (0<...1000). Default = 0.")>
            Public Property Frequency As Single
                Get
                    Return FXItem.fFreq
                End Get
                Set(value As Single)
                    FXItem.fRange = value
                    If IsEnabled Then
                        If Not Bass.BASS_FXSetParameters(HEffect, FXItem) Then
                            Threading.Thread.Sleep(100)
                            Bass.BASS_FXSetParameters(HEffect, FXItem)
                        End If
                        OnPropertyChanged()
                    End If
                End Set
            End Property
            <Field("Range", GetType(Single), "Octave", "Oct", Minimum:=0.1, Maximum:=9.9, Description:="Sweep range in octaves (0<...<10). Default = 0.")>
            Public Property Range As Single
                Get
                    Return FXItem.fRange
                End Get
                Set(value As Single)
                    FXItem.fRange = value
                    If IsEnabled Then
                        If Not Bass.BASS_FXSetParameters(HEffect, FXItem) Then
                            Threading.Thread.Sleep(100)
                            Bass.BASS_FXSetParameters(HEffect, FXItem)
                        End If
                        OnPropertyChanged()
                    End If
                End Set
            End Property
            <Field("Rate", GetType(Single), "Cycles/Second", "C/s", Minimum:=0.1, Maximum:=9.9, Description:="Rate of sweep in cycles per second (0<...<10). Default = 0.")>
            Public Property Rate As Single
                Get
                    Return FXItem.fRate
                End Get
                Set(value As Single)
                    FXItem.fRate = value
                    If IsEnabled Then
                        If Not Bass.BASS_FXSetParameters(HEffect, FXItem) Then
                            Threading.Thread.Sleep(100)
                            Bass.BASS_FXSetParameters(HEffect, FXItem)
                        End If
                        OnPropertyChanged()
                    End If
                End Set
            End Property
            <Field("Wet Mix", GetType(Single), Minimum:=-2, Maximum:=2, Description:="Wet (affected) signal mix (-2...+2). Default = 0.")>
            Public Property WetMix As Single
                Get
                    Return FXItem.fWetMix
                End Get
                Set(value As Single)
                    FXItem.fWetMix = value
                    If IsEnabled Then
                        If Not Bass.BASS_FXSetParameters(HEffect, FXItem) Then
                            Dim err = Bass.BASS_ErrorGetCode.ToString
                            Threading.Thread.Sleep(100)
                            Bass.BASS_FXSetParameters(HEffect, FXItem)
                        End If
                        OnPropertyChanged()
                    End If
                End Set
            End Property
            <Field("Channel", GetType(AddOn.Fx.BASSFXChan), "Channel", "Ch.", Description:="Define on which channels to apply the effect. Default: -1(BASS_BFX_CHANALL - all channels.)")>
            Public Property Channel As AddOn.Fx.BASSFXChan
                Get
                    Return FXItem.lChannel
                End Get
                Set(value As AddOn.Fx.BASSFXChan)
                    FXItem.lChannel = value
                    If IsEnabled Then
                        If Not Bass.BASS_FXSetParameters(HEffect, FXItem) Then
                            Dim err = Bass.BASS_ErrorGetCode.ToString
                            Threading.Thread.Sleep(100)
                            Bass.BASS_FXSetParameters(HEffect, FXItem)
                        End If
                        OnPropertyChanged()
                    End If
                End Set
            End Property

            <MethodGroup(DisplayName:="Presets", GridStackWrap:=False, HorizontalItemSpacing:=10, VerticalItemSpacing:=10, Orientation:=Orientation.Horizontal)>
            Property Group0

            <NonSerialized> Private FXItem As New Un4seen.Bass.AddOn.Fx.BASS_BFX_PHASER()

            Sub New()
                FXItem.Preset_Default()
            End Sub

            Public Overrides Sub Apply(Optional Force As Boolean = False)
                If Not Force Then If HEffect <> 0 Then Return
                HEffect = Bass.BASS_ChannelSetFX(HStream, BASSFXType.BASS_FX_BFX_PHASER, Priority)
                _IsEnabled = Bass.BASS_FXSetParameters(HEffect, FXItem)
                OnPropertyChanged(NameOf(IsEnabled))
                NotifyUI()
            End Sub

            Private Sub NotifyUI()
                OnPropertyChanged(NameOf(DryMix))
                OnPropertyChanged(NameOf(Feedback))
                OnPropertyChanged(NameOf(Frequency))
                OnPropertyChanged(NameOf(Range))
                OnPropertyChanged(NameOf(Rate))
                OnPropertyChanged(NameOf(WetMix))
                OnPropertyChanged(NameOf(Channel))
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
            <Method("Basic Phase", "Sets the instance members to a preset.", Group:="Presets")>
            Sub Preset_BasicPhase()
                FXItem.Preset_BasicPhase()
                Update()
            End Sub
            <Method("Fast Phase", "Sets the instance members to a preset.", Group:="Presets")>
            Sub Preset_FastPhase()
                FXItem.Preset_FastPhase()
                Update()
            End Sub
            <Method("Invert With Invert Feedback", "Sets the instance members to a preset.", Group:="Presets")>
            Sub Preset_InvertWithInvertFeedback()
                FXItem.Preset_InvertWithInvertFeedback()
                Update()
            End Sub

            <Method("Medium Phase", "Sets the instance members to a preset.", Group:="Presets")>
            Sub Preset_MediumPhase()
                FXItem.Preset_MediumPhase()
                Update()
            End Sub

            <Method("Phase Shift", "Sets the instance members to a preset.", Group:="Presets")>
            Sub Preset_PhaseShift()
                FXItem.Preset_PhaseShift()
                Update()
            End Sub

            <Method("Phase With Feedback", "Sets the instance members to a preset.", Group:="Presets")>
            Sub Preset_PhaseWithFeedback()
                FXItem.Preset_PhaseWithFeedback()
                Update()
            End Sub

            <Method("Slow Invert Phase Shift With Feedback", "Sets the instance members to a preset.", Group:="Presets")>
            Sub Preset_SlowInvertPhaseShiftWithFeedback()
                FXItem.Preset_SlowInvertPhaseShiftWithFeedback()
                Update()
            End Sub

            <Method("Tremolo Wah", "Sets the instance members to a preset.", Group:="Presets")>
            Sub Preset_TremoloWah()
                FXItem.Preset_TremoloWah()
                Update()
            End Sub

#Region "Serialization"
            Private SerializationData As String

            <Runtime.Serialization.OnDeserialized>
            Private Sub Deserialization(context As Runtime.Serialization.StreamingContext)
                'Restore values from serialization
                Dim SData = SerializationData.Split(";")
                FXItem = New AddOn.Fx.BASS_BFX_PHASER(SData(0), SData(5), SData(1), SData(4), SData(3), SData(2))
                FXItem.lChannel = SData(6)
            End Sub

            <Runtime.Serialization.OnSerializing>
            Private Sub Serializing(context As Runtime.Serialization.StreamingContext)
                'Save values for deserialization
                SerializationData = FXItem.fDryMix & ";" & FXItem.fFeedback & ";" & FXItem.fFreq & ";" & FXItem.fRange & ";" & FXItem.fRate & ";" & FXItem.fWetMix & ";" & CInt(FXItem.lChannel)
                HEffect = 0
            End Sub
#End Region
        End Class

        <Serializable>
        Public Class BiQuadFilter
            Inherits Profile.AudioEffect

            Public Overrides Property Name As String = "BiQuad Filter"

            Public Overrides Property Description As String = "BiQuad filters are second-order recursive linear filters."

            Public Overrides Property ShortName As String = "BQF"

            Public Overrides Property Category As String = "BASS_FX;DSP"

            Private _IsEnabled As Boolean
            Public Overrides Property IsEnabled As Boolean
                Get
                    Return _IsEnabled
                End Get
                Set(value As Boolean)
                    If value = _IsEnabled Then Return
                    If value Then
                        HEffect = Bass.BASS_ChannelSetFX(HStream, BASSFXType.BASS_FX_BFX_BQF, Priority)
                        _IsEnabled = Bass.BASS_FXSetParameters(HEffect, FXItem)
                    Else
                        _IsEnabled = Not Bass.BASS_ChannelRemoveFX(HStream, HEffect)
                        If Not _IsEnabled Then
                            HEffect = 0
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

            Private _Priority As Integer
            Public Overrides Property Priority As Integer
                Get
                    Return _Priority
                End Get
                Set(value As Integer)
                    Bass.BASS_FXSetPriority(HEffect, value)
                    _Priority = value
                    OnPropertyChanged()
                End Set
            End Property


            <Field("Channel", GetType(AddOn.Fx.BASSFXChan), "Channel", "Ch.", Description:="Define on which channels to apply the effect. Default: -1(BASS_BFX_CHANALL - all channels.)")>
            Public Property Channel As AddOn.Fx.BASSFXChan
                Get
                    Return FXItem.lChannel
                End Get
                Set(value As AddOn.Fx.BASSFXChan)
                    FXItem.lChannel = value
                    If IsEnabled Then
                        If Not Bass.BASS_FXSetParameters(HEffect, FXItem) Then
                            Threading.Thread.Sleep(100)
                            Bass.BASS_FXSetParameters(HEffect, FXItem)
                        End If
                        OnPropertyChanged()
                    End If
                End Set
            End Property

            <Field("Filter", GetType(AddOn.Fx.BASSBFXBQF), "Filter", "Filter", Description:="Defines which BiQuad filter should be used.")>
            Public Property Filter As AddOn.Fx.BASSBFXBQF
                Get
                    Return FXItem.lFilter
                End Get
                Set(value As AddOn.Fx.BASSBFXBQF)
                    FXItem.lFilter = value
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

            <NonSerialized> Private FXItem As New Un4seen.Bass.AddOn.Fx.BASS_BFX_BQF()

            Sub New()
                FXItem.lFilter = AddOn.Fx.BASSBFXBQF.BASS_BFX_BQF_ALLPASS
                FXItem.lChannel = AddOn.Fx.BASSFXChan.BASS_BFX_CHANALL
            End Sub

            Public Overrides Sub Apply(Optional Force As Boolean = False)
                If Not Force Then If HEffect <> 0 Then Return
                HEffect = Bass.BASS_ChannelSetFX(HStream, BASSFXType.BASS_FX_BFX_BQF, Priority)
                _IsEnabled = Bass.BASS_FXSetParameters(HEffect, FXItem)
                OnPropertyChanged(NameOf(IsEnabled))
                NotifyUI()
            End Sub

            Private Sub NotifyUI()
                OnPropertyChanged(NameOf(Channel))
                OnPropertyChanged(NameOf(Filter))
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
                FXItem.lFilter = AddOn.Fx.BASSBFXBQF.BASS_BFX_BQF_ALLPASS
                FXItem.lChannel = AddOn.Fx.BASSFXChan.BASS_BFX_CHANALL
                Update()
            End Sub

#Region "Serialization"
            Private SerializationData As String

            <Runtime.Serialization.OnDeserialized>
            Private Sub Deserialization(context As Runtime.Serialization.StreamingContext)
                'Restore values from serialization
                Dim SData = SerializationData.Split(";")
                FXItem = New AddOn.Fx.BASS_BFX_BQF
                FXItem.lFilter = SData(1)
                FXItem.lChannel = SData(0)
            End Sub

            <Runtime.Serialization.OnSerializing>
            Private Sub Serializing(context As Runtime.Serialization.StreamingContext)
                'Save values for deserialization
                SerializationData = CInt(FXItem.lChannel) & ";" & CInt(FXItem.lFilter)
                HEffect = 0
            End Sub
#End Region
        End Class
    End Class
End Namespace
