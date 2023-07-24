Imports System.ComponentModel

Namespace Classes
    Public Class Themes

        <Serializable>
        Public MustInherit Class Theme
            Implements INotifyPropertyChanged

            <NonSerialized> Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged
            Public Event PresetChanged(sender As Theme)

            Protected Sub OnPropertyChanged(<Runtime.CompilerServices.CallerMemberName> Optional CallerName As String = Nothing)
                RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(CallerName))
            End Sub

            MustOverride Property Name As String
            MustOverride Property ColorPallete As Color()

            Private Protected Sub OnPresetChanged()

            End Sub

            MustOverride Function GetPreset() As HandyControl.Themes.PresetManager.Preset

            MustOverride Sub OnMediaChanged(media As Player.Metadata)
            MustOverride Sub Dispose()
        End Class

        Public Class [Default]
            Inherits Theme

            Private _Name As String
            Public Overrides Property Name As String
                Get
                    Return _Name
                End Get
                Set(value As String)
                    _Name = value
                    OnPresetChanged()
                End Set
            End Property

            Private _ColorPallete As Color() = New Color() {Color.FromRgb(50, 108, 243), Color.FromRgb(10, 10, 10), Colors.White}
            Public Overrides Property ColorPallete As Color()
                Get
                    Return _ColorPallete
                End Get
                Set(value As Color())
                    _ColorPallete = value
                    OnPropertyChanged()
                End Set
            End Property

            Private _Preset As New HandyControl.Themes.PresetManager.Preset() With {.AssemblyName = Reflection.Assembly.GetExecutingAssembly.GetName.Name, .ColorPreset = "Resources\Themes\Default"}

            Public Overrides Sub OnMediaChanged(media As Player.Metadata)
            End Sub

            Public Overrides Function GetPreset() As HandyControl.Themes.PresetManager.Preset
                Return _Preset
            End Function

            Public Overrides Sub Dispose()
            End Sub
        End Class

        Public Class Neon
            Inherits Theme

            Private _Name As String
            Public Overrides Property Name As String
                Get
                    Return _Name
                End Get
                Set(value As String)
                    _Name = value
                    OnPresetChanged()
                End Set
            End Property

            Private _ColorPallete As Color() = New Color() {Color.FromRgb(243, 73, 113), Color.FromRgb(255, 147, 130), Color.FromRgb(249, 100, 0)}
            Public Overrides Property ColorPallete As Color()
                Get
                    Return _ColorPallete
                End Get
                Set(value As Color())
                    _ColorPallete = value
                    OnPropertyChanged()
                End Set
            End Property

            Private _Preset As New HandyControl.Themes.PresetManager.Preset() With {.AssemblyName = Reflection.Assembly.GetExecutingAssembly.GetName.Name, .ColorPreset = "Resources\Themes\Neon"}

            Public Overrides Sub OnMediaChanged(media As Player.Metadata)
            End Sub

            Public Overrides Function GetPreset() As HandyControl.Themes.PresetManager.Preset
                Return _Preset
            End Function

            Public Overrides Sub Dispose()
            End Sub
        End Class

    End Class

End Namespace
