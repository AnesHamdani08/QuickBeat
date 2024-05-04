Imports System.Collections.ObjectModel
Imports System.ComponentModel
Imports System.Reflection

Namespace Player
    <Serializable>
    Public Class Profile
        Inherits ObservableCollection(Of AudioEffect)
        Implements IDisposable

        <Serializable>
        MustInherit Class AudioEffect
            Implements INotifyPropertyChanged

            <AttributeUsage(AttributeTargets.Property, AllowMultiple:=False)>
            Public Class FieldAttribute
                Inherits Attribute
                Property DisplayName As String
                ''' <summary>
                ''' The field unit if available
                ''' </summary>
                ''' <returns></returns>
                Property Unit As String
                Property UnitShortForm As String
                Property ValueType As Type
                Property Description As String
                ''' <summary>
                ''' Must be the same as <see cref="MethodGroupAttribute.DisplayName"/>
                ''' </summary>
                ''' <returns></returns>
                Property Group As String
                ''' <summary>
                ''' Ignored if <see cref="ValueType"/> is not a numeric.
                ''' </summary>
                ''' <returns></returns>
                Property Minimum As Double = 0.0001
                ''' <summary>
                ''' Ignored if <see cref="ValueType"/> is not a numeric.
                ''' </summary>
                ''' <returns></returns>
                Property Maximum As Double = Double.MaxValue

                ''' <summary>
                ''' Whether or not to snap to fixed values based on interval (<see cref="Frequency"/>)
                ''' </summary>
                ''' <returns></returns>
                Property SnapToTicks As Boolean = False

                ''' <summary>
                ''' Ignored if <see cref="SnapToTicks"/> is false
                ''' </summary>
                ''' <returns></returns>
                Property Frequency As Double = 1

                Sub New(DisplayName As String, ValueType As Type)
                    Me.DisplayName = DisplayName
                    Me.ValueType = ValueType
                End Sub

                Sub New(DisplayName As String, ValueType As Type, Unit As String, UnitShortForm As String)
                    Me.DisplayName = DisplayName
                    Me.ValueType = ValueType
                    Me.Unit = Unit
                    Me.UnitShortForm = UnitShortForm
                End Sub
            End Class

            <AttributeUsage(AttributeTargets.Method)>
            Public Class MethodAttribute
                Inherits Attribute
                Property DisplayName As String
                Property Description As String
                ''' <summary>
                ''' Must be the same as <see cref="MethodGroupAttribute.DisplayName"/>
                ''' </summary>
                ''' <returns></returns>
                Property Group As String
                Property StackWrap As Boolean
                Sub New(DisplayName As String, Description As String)
                    Me.DisplayName = DisplayName
                    Me.Description = Description
                End Sub
            End Class

            ''' <summary>
            ''' Use to setup a display group.            
            ''' </summary>
            <AttributeUsage(AttributeTargets.Property)>
            Public Class MethodGroupAttribute
                Inherits Attribute
                Property Orientation As Orientation
                ''' <summary>
                ''' Default to Grid, True to stack, False to wrap.
                ''' If using Grid, use <see cref="MarginAttribute"/> on a property of choice to set the location of the control.
                ''' </summary>
                ''' <returns></returns>
                Property GridStackWrap As TriState = TriState.UseDefault
                Property Height As Double = Double.NaN
                Property Width As Double = Double.NaN
                Property Scroll As Boolean = False
                ''' <summary>
                ''' Default <see cref="ScrollBarVisibility.Disabled"/>
                ''' </summary>
                ''' <returns></returns>
                Property VerticalScrollBarVisibility As ScrollBarVisibility = ScrollBarVisibility.Disabled
                ''' <summary>
                ''' Default <see cref="ScrollBarVisibility.Auto"/>
                ''' </summary>
                ''' <returns></returns>
                Property HorizontalScrollBarVisibility As ScrollBarVisibility = ScrollBarVisibility.Auto
                Property DisplayName As String
                Property Description As String
                Property HorizontalItemSpacing As Double
                Property VerticalItemSpacing As Double
            End Class

            <AttributeUsage(AttributeTargets.Property)>
            Public Class MarginAttribute
                Inherits Attribute
                Property Margin As Thickness
                Property HorizontalAlignment As HorizontalAlignment = HorizontalAlignment.Stretch
                Property VerticalAlignment As VerticalAlignment = VerticalAlignment.Stretch
            End Class

            ''' <summary>
            ''' Use to ignore default generated config window and point to a custom config method.
            ''' </summary>
            <AttributeUsage(AttributeTargets.Method, AllowMultiple:=False)>
            Public Class ConfigAttribute
                Inherits Attribute
            End Class

            <NonSerialized> Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged
            Protected Sub OnPropertyChanged(<Runtime.CompilerServices.CallerMemberName> Optional CallerName As String = Nothing)
                RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(CallerName))
            End Sub

            MustOverride Property Name As String
            MustOverride Property Description As String
            MustOverride Property ShortName As String
            MustOverride Property Category As String
            MustOverride Property IsEnabled As Boolean
            MustOverride Property HEffect As Integer
            MustOverride Property HStream As Integer
            MustOverride Property Priority As Integer
            MustOverride Sub Apply(Optional Force As Boolean = False)
            Overridable Sub Clean()

            End Sub
        End Class

        <NonSerialized> Private WithEvents _Parent As Player
        Property Parent As Player
            Get
                Return _Parent
            End Get
            Set(value As Player)
                _Parent = value
                OnPropertyChanged(New PropertyChangedEventArgs(NameOf(Parent)))
            End Set
        End Property

        Private _Name As String
        Property Name As String
            Get
                Return _Name
            End Get
            Set(value As String)
                _Name = value
                OnPropertyChanged(New PropertyChangedEventArgs(NameOf(Name)))
            End Set
        End Property

        Private _Description As String
        Private disposedValue As Boolean

        Property Description As String
            Get
                Return _Description
            End Get
            Set(value As String)
                _Description = value
                OnPropertyChanged(New PropertyChangedEventArgs(NameOf(Description)))
            End Set
        End Property

        Protected Overrides Sub InsertItem(index As Integer, item As AudioEffect)
            If Parent IsNot Nothing Then
                item.HStream = Parent?.Stream
                item.Priority = index
                item.Apply()
            End If
            MyBase.InsertItem(index, item)
        End Sub

        Protected Overrides Sub RemoveItem(index As Integer)
            If Me(index) IsNot Nothing Then Me(index).IsEnabled = False
            Un4seen.Bass.Bass.BASS_ChannelRemoveFX(If(Parent IsNot Nothing, Parent.Stream, Me(index)?.HStream), Me(index)?.HEffect)
            Me(index)?.Clean()
            MyBase.RemoveItem(index)
        End Sub

        Protected Overrides Sub ClearItems()
            For Each _Item In Me
                If _Item Is Nothing Then Continue For
                _Item.IsEnabled = False
                _Item.Clean()
            Next

            MyBase.ClearItems()
        End Sub

        Sub New(Parent As Player)
            Me.Parent = Parent
        End Sub

        Private Sub _Parent_MediaLoaded(OldValue As Integer, NewValue As Integer) Handles _Parent.MediaLoaded
            For Each _Item In Me
                If _Item Is Nothing Then Continue For
                Dim Result = Un4seen.Bass.Bass.BASS_ChannelRemoveFX(OldValue, _Item?.HEffect)
                _Item.HStream = NewValue
                _Item.HEffect = 0 'TODO subject to change
                If _Item.IsEnabled Then _Item.Apply(True)
            Next
        End Sub

        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not disposedValue Then
                If disposing Then
                    ' TODO: dispose managed state (managed objects)                    
                    For Each fx In Me
                        fx.IsEnabled = False
                    Next
                    ClearItems()
                End If

                ' TODO: free unmanaged resources (unmanaged objects) and override finalizer
                ' TODO: set large fields to null
                disposedValue = True
            End If
        End Sub

        ' ' TODO: override finalizer only if 'Dispose(disposing As Boolean)' has code to free unmanaged resources
        ' Protected Overrides Sub Finalize()
        '     ' Do not change this code. Put cleanup code in 'Dispose(disposing As Boolean)' method
        '     Dispose(disposing:=False)
        '     MyBase.Finalize()
        ' End Sub

        Public Sub Dispose() Implements IDisposable.Dispose
            ' Do not change this code. Put cleanup code in 'Dispose(disposing As Boolean)' method
            Dispose(disposing:=True)
            GC.SuppressFinalize(Me)
        End Sub
    End Class
End Namespace
