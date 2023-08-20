Imports System.ComponentModel
Imports Un4seen.Bass
Imports System.Text.RegularExpressions
Imports QuickBeat.Utilities.CommonFunctions
Namespace QScript
    Public Class DeveloperConsole
        Class Palette
            Property Keyword As Brush
            Property Text As Brush
            Property Argument As Brush
            Property Type As Brush
            Property Method As Brush
            Property [String] As Brush
            Property [Enum] As Brush
            Property [Error] As Brush
            Enum KeywordType
                Text
                Method
                Keyword
                [String]
                Type
                Argument
                [Enum]
                [Error]
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

        Public Shared SuggestionsControlsProperty As DependencyProperty = DependencyProperty.Register("SuggestionsControls", GetType(ObjectModel.ObservableCollection(Of FrameworkElement)), GetType(DeveloperConsole), New UIPropertyMetadata(New ObjectModel.ObservableCollection(Of FrameworkElement)))
        Property SuggestionsControls As ObjectModel.ObservableCollection(Of FrameworkElement)
            Get
                Return GetValue(SuggestionsControlsProperty)
            End Get
            Set(value As ObjectModel.ObservableCollection(Of FrameworkElement))
                SetValue(SuggestionsControlsProperty, value)
            End Set
        End Property

        Property UIPalette As New Palette With {.Keyword = Brushes.LightBlue, .Method = Brushes.White, .String = Brushes.Orange, .Type = Brushes.DodgerBlue, .Argument = Brushes.White, .Text = Brushes.White, .Enum = Brushes.GreenYellow, .[Error] = Brushes.Red}
        Property MainFontFamily As FontFamily = New FontFamily("Courier New")
        Property AvailableMethods As New Dictionary(Of String, Reflection.MethodInfo())
        Property LockedMethod As Reflection.MethodInfo
        Property LockedMethods As KeyValuePair(Of String, Reflection.MethodInfo())
        Private Property AllowClosing As Boolean = False

        Private ReadOnly Property Aqua As Aqua.Aqua
            Get
                Return Utilities.SharedProperties.Instance.Aqua
            End Get
        End Property

        Private Async Sub ConsoleIn_TB_KeyUp(sender As Object, e As KeyEventArgs) Handles ConsoleIn_TB.KeyUp
            If e.Key = Key.Enter Then
                Try
                    Dim Result = (Await Aqua.ExecuteProxy(ConsoleIn_TB.Text))?.ToString
                    ConsoleIn_TB.Clear()
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

        Private Sub DeveloperConsole_Initialized(sender As Object, e As EventArgs) Handles Me.Initialized
            AddHandler Aqua.IsRunningChanged, AddressOf Aqua_IsRunningChanged
            AddHandler Aqua.RunningLineChanged, AddressOf Aqua_RunningLineChanged
            AddHandler Aqua.ErrorOccured, AddressOf Aqua_ErrorOccured

            AvailableMethods.Add("Aqua", Aqua.GetType.GetMethods())
            AvailableMethods.Add("Player", GetType(Player.Player).GetMethods())
            AvailableMethods.Add("Playlist", GetType(Player.Player).GetMethods())
            AvailableMethods.Add("HotkeyManager", GetType(Hotkeys.HotkeyManager).GetMethods())
            AvailableMethods.Add("Library", GetType(Library.Library).GetMethods())
            AvailableMethods.Add("SharedProperties", GetType(Utilities.SharedProperties).GetMethods())
            AvailableMethods.Add("SettingsBrowser", GetType(QScript.SettingsBrowser).GetMethods)
            AvailableMethods.Add("NamedPipeManager", GetType(Classes.NamedPipeManager).GetMethods)
            AvailableMethods.Add("YoutubeDL", GetType(Youtube.YoutubeDL).GetMethods)
        End Sub

        Private Sub ConsoleIn_TB_TextChanged(sender As Object, e As TextChangedEventArgs) Handles ConsoleIn_TB.TextChanged
            PrepareAndGenerateSuggestions(ConsoleIn_TB.Text)
        End Sub

        Private Sub PrepareAndGenerateSuggestions(command As String)
            SuggestionsControls.Clear()
            Dim sCom = command.Split(New Char() {" "}, StringSplitOptions.RemoveEmptyEntries)
            Select Case sCom.Length
                Case 0 'Generate all available references and variables
                    Dim _TB As New TextBlock With {.FontFamily = MainFontFamily}
                    _TB.Inlines.Add(New Run With {.Text = "Aqua", .Foreground = UIPalette.Type})
                    SuggestionsControls.Add(_TB)
                    For Each ref In Aqua.ReferenceBinder?.GetAvailableReferencesNames
                        Dim TB As New TextBlock With {.FontFamily = MainFontFamily}
                        TB.Inlines.Add(New Run With {.Text = "#", .Foreground = UIPalette.Keyword})
                        TB.Inlines.Add(New Run With {.Text = ref, .Foreground = UIPalette.Text})
                        TB.Inlines.Add(New Run With {.Text = " As ", .Foreground = UIPalette.Keyword})
                        TB.Inlines.Add(New Run With {.Text = If(Aqua.ReferenceBinder?.GetReference(ref)?.GetType.Name, "?"), .Foreground = UIPalette.Type})
                        SuggestionsControls.Add(TB)
                    Next
                    Dim WEnu = Aqua.Workspace.GetEnumerator()
                    Do While WEnu.MoveNext
                        If WEnu.Current.Value Is Nothing Then
                            Continue Do
                        End If
                        Dim TB As New TextBlock With {.FontFamily = MainFontFamily}
                        TB.Inlines.Add(New Run With {.Text = WEnu.Current.Key, .Foreground = UIPalette.Text})
                        TB.Inlines.Add(New Run With {.Text = " As ", .Foreground = UIPalette.Keyword})
                        TB.Inlines.Add(New Run With {.Text = WEnu.Current.Value.GetType.Name, .Foreground = UIPalette.Type})
                        SuggestionsControls.Add(TB)
                    Loop
                Case 1 'Generate specific reference or variable
                    If "Aqua".StartsWith(sCom.FirstOrDefault) Then
                        Dim _TB As New TextBlock With {.FontFamily = MainFontFamily}
                        _TB.Inlines.Add(New Run With {.Text = "Aqua", .Foreground = UIPalette.Type})
                        SuggestionsControls.Add(_TB)
                    End If
                    For Each ref In Aqua.ReferenceBinder?.GetAvailableReferencesNames
                        If sCom.FirstOrDefault <> "#" Then
                            If Not ref.StartsWith(sCom.FirstOrDefault?.Substring(1)) Then Continue For
                        End If
                        Dim TB As New TextBlock With {.FontFamily = MainFontFamily}
                        TB.Inlines.Add(New Run With {.Text = "#", .Foreground = UIPalette.Keyword})
                        TB.Inlines.Add(New Run With {.Text = ref, .Foreground = UIPalette.Text})
                        TB.Inlines.Add(New Run With {.Text = " As ", .Foreground = UIPalette.Keyword})
                        TB.Inlines.Add(New Run With {.Text = If(Aqua.ReferenceBinder?.GetReference(ref)?.GetType.Name, "?"), .Foreground = UIPalette.Type})
                        SuggestionsControls.Add(TB)
                    Next
                    Dim WEnu = Aqua.Workspace.GetEnumerator()
                    Do While WEnu.MoveNext
                        If WEnu.Current.Value Is Nothing Then
                            Continue Do
                        End If
                        If Not WEnu.Current.Key?.StartsWith(sCom.FirstOrDefault) Then Continue Do
                        Dim TB As New TextBlock With {.FontFamily = MainFontFamily}
                        TB.Inlines.Add(New Run With {.Text = WEnu.Current.Key, .Foreground = UIPalette.Text})
                        TB.Inlines.Add(New Run With {.Text = " As ", .Foreground = UIPalette.Keyword})
                        TB.Inlines.Add(New Run With {.Text = WEnu.Current.Value.GetType.Name, .Foreground = UIPalette.Type})
                        SuggestionsControls.Add(TB)
                    Loop
                Case 2 'Generate operators
                    For Each op In Aqua.Operators
                        Dim TB As New TextBlock With {.FontFamily = MainFontFamily, .Foreground = UIPalette.Keyword}
                        TB.Text = op
                        SuggestionsControls.Add(TB)
                    Next
                Case 3 'Generate all methods
                    If sCom(1) = "->" Then
                        If Not sCom(2).StartsWith("""") Then
                            Dim TB As New TextBlock With {.FontFamily = MainFontFamily, .Foreground = UIPalette.Error}
                            TB.Inlines.Add(New Run("Expected """))
                            SuggestionsControls.Add(TB)
                            Return
                        End If
                        If sCom(2).Length < 2 Then
                            Dim DeterminedMets As Reflection.MethodInfo() = Nothing
                            If sCom.FirstOrDefault = "Aqua" Then
                                DeterminedMets = AvailableMethods("Aqua")
                            ElseIf Aqua.HasVariable(sCom.FirstOrDefault) Then
                                Dim DeterminedType = Aqua.GetVariable(sCom.FirstOrDefault)?.GetType
                                If AvailableMethods.ContainsKey(DeterminedType.Name) Then
                                    DeterminedMets = AvailableMethods(DeterminedType.Name)
                                Else
                                    Dim Mets = DeterminedType.GetMethods
                                    AvailableMethods.Add(DeterminedType.Name, Mets)
                                    DeterminedMets = Mets
                                End If
                            ElseIf sCom.FirstOrDefault?.StartsWith("#") Then
                                Dim Ref = Aqua.ReferenceBinder?.GetReference(sCom.FirstOrDefault.Substring(1, sCom.FirstOrDefault.Length - 1))
                                If Ref IsNot Nothing Then
                                    Dim DeterminedType = Ref.GetType
                                    If AvailableMethods.ContainsKey(DeterminedType.Name) Then
                                        DeterminedMets = AvailableMethods(DeterminedType.Name)
                                    Else
                                        Dim Mets = DeterminedType.GetMethods
                                        AvailableMethods.Add(DeterminedType.Name, Mets)
                                        DeterminedMets = Mets
                                    End If
                                End If
                            End If
                            GenerateSuggestions(sCom(2).Replace("""", ""), DeterminedMets)
                            Return
                        End If
                        Dim MetName = If(sCom(2).EndsWith(""""), sCom(2).Remove(0, 1).Remove(sCom(2).Length - 2, 1), sCom(2).Remove(0, 1))
                        Select Case sCom.FirstOrDefault
                            Case "Aqua"
                                GenerateSuggestions(MetName, AvailableMethods("Aqua"))
                            Case Else
                                If Aqua.HasVariable(sCom.FirstOrDefault) Then
                                    Dim var = Aqua.GetVariable(sCom.FirstOrDefault)
                                    If var.GetType.Name = LockedMethods.Key Then
                                        GenerateSuggestions(MetName, LockedMethods.Value)
                                    Else
                                        Dim DeterminedType = var.GetType
                                        Dim DeterminedMets As Reflection.MethodInfo()
                                        If AvailableMethods.ContainsKey(DeterminedType.Name) Then
                                            DeterminedMets = AvailableMethods(DeterminedType.Name)
                                        Else
                                            Dim Mets = DeterminedType.GetMethods
                                            AvailableMethods.Add(DeterminedType.Name, Mets)
                                            DeterminedMets = Mets
                                        End If
                                        LockMethods(DeterminedType.Name, DeterminedMets)
                                    End If
                                Else
                                    Dim Ref = Aqua.ReferenceBinder?.GetReference(sCom.FirstOrDefault.Substring(1, sCom.FirstOrDefault.Length - 1))
                                    If Ref IsNot Nothing Then
                                        If Ref.GetType.Name = LockedMethods.Key Then
                                            GenerateSuggestions(MetName, LockedMethods.Value)
                                        Else
                                            Dim DeterminedType = Ref.GetType
                                            Dim DeterminedMets As Reflection.MethodInfo()
                                            If AvailableMethods.ContainsKey(DeterminedType.Name) Then
                                                DeterminedMets = AvailableMethods(DeterminedType.Name)
                                            Else
                                                Dim Mets = DeterminedType.GetMethods
                                                AvailableMethods.Add(DeterminedType.Name, Mets)
                                                DeterminedMets = Mets
                                            End If
                                            LockMethods(DeterminedType.Name, DeterminedMets)
                                        End If
                                    End If
                                End If
                        End Select
                    End If
                Case > 3 'Generate locked method info and params
                    Select Case sCom.FirstOrDefault
                        Case "Aqua"
                            If Aqua.GetSetting("auto") Then
                                Dim Met = AvailableMethods("Aqua").FirstOrDefault(Function(k) k.Name = sCom(2).Remove(0, 1).Remove(sCom(2).Length - 2, 1))
                                If Met Is Nothing Then Return
                                Dim _TB As New TextBlock With {.FontFamily = MainFontFamily}
                                _TB.Inlines.Add(New Run With {.Text = If(Met.ReturnType Is GetType(System.Void), "Sub ", "Function "), .Foreground = UIPalette.Keyword})
                                _TB.Inlines.Add(New Run With {.Text = Met.Name, .Foreground = UIPalette.Method})
                                _TB.Inlines.Add(New Run With {.Text = "(", .Foreground = UIPalette.Text})
                                Dim _i = 0
                                For Each param In Met.GetParameters
                                    If _i > 0 Then _TB.Inlines.Add(New Run With {.Text = ", ", .Foreground = UIPalette.Text})
                                    If param.IsOptional Then _TB.Inlines.Add(New Run With {.Text = "Optional ", .Foreground = UIPalette.Keyword})
                                    _TB.Inlines.Add(New Run With {.Text = param.Name, .Foreground = UIPalette.Text})
                                    _TB.Inlines.Add(New Run With {.Text = " As ", .Foreground = UIPalette.Keyword})
                                    _TB.Inlines.Add(New Run With {.Text = param.ParameterType.Name, .Foreground = UIPalette.Type})
                                    _i += 1
                                Next
                                _TB.Inlines.Add(New Run With {.Text = ")", .Foreground = UIPalette.Text})
                                If Met.ReturnType IsNot GetType(System.Void) Then
                                    _TB.Inlines.Add(New Run With {.Text = " As ", .Foreground = UIPalette.Keyword})
                                    _TB.Inlines.Add(New Run With {.Text = Met.ReturnType.Name, .Foreground = UIPalette.Type})
                                End If
                                SuggestionsControls.Add(_TB)
                                If Met IsNot Nothing Then
                                    Dim Params = Met.GetParameters()
                                    Dim i = sCom.Length - 3
                                    If i <= Params.Length Then
                                        Dim Param = Params(i - 1)
                                        Dim TB As New TextBlock With {.FontFamily = MainFontFamily}
                                        If Param.IsOptional Then TB.Inlines.Add(New Run With {.Text = "Optional ", .Foreground = UIPalette.Keyword})
                                        TB.Inlines.Add(New Run With {.Text = Param.Name, .Foreground = UIPalette.Text})
                                        TB.Inlines.Add(New Run With {.Text = " As ", .Foreground = UIPalette.Keyword})
                                        TB.Inlines.Add(New Run With {.Text = Param.ParameterType.Name, .Foreground = UIPalette.Type})
                                        SuggestionsControls.Add(TB)
                                    End If
                                End If
                            Else
                                If sCom.Length = 4 Then
                                    For Each op In New String() {"get", "set", "let", "method"}
                                        Dim TB As New TextBlock With {.FontFamily = MainFontFamily, .Foreground = UIPalette.Keyword}
                                        TB.Text = op
                                        SuggestionsControls.Add(TB)
                                    Next
                                End If
                                If sCom.Length < 5 Then Return
                                Dim Met = AvailableMethods("Aqua").FirstOrDefault(Function(k) k.Name = sCom(2).Remove(0, 1).Remove(sCom(2).Length - 2, 1))
                                If Met IsNot Nothing Then
                                    Dim _TB As New TextBlock With {.FontFamily = MainFontFamily}
                                    _TB.Inlines.Add(New Run With {.Text = If(Met.ReturnType Is GetType(System.Void), "Sub ", "Function "), .Foreground = UIPalette.Keyword})
                                    _TB.Inlines.Add(New Run With {.Text = Met.Name, .Foreground = UIPalette.Method})
                                    _TB.Inlines.Add(New Run With {.Text = "(", .Foreground = UIPalette.Text})
                                    Dim _i = 0
                                    For Each param In Met.GetParameters
                                        If _i > 0 Then _TB.Inlines.Add(New Run With {.Text = ", ", .Foreground = UIPalette.Text})
                                        If param.IsOptional Then _TB.Inlines.Add(New Run With {.Text = "Optional ", .Foreground = UIPalette.Keyword})
                                        _TB.Inlines.Add(New Run With {.Text = param.Name, .Foreground = UIPalette.Text})
                                        _TB.Inlines.Add(New Run With {.Text = " As ", .Foreground = UIPalette.Keyword})
                                        _TB.Inlines.Add(New Run With {.Text = param.ParameterType.Name, .Foreground = UIPalette.Type})
                                        _i += 1
                                    Next
                                    _TB.Inlines.Add(New Run With {.Text = ")", .Foreground = UIPalette.Text})
                                    If Met.ReturnType IsNot GetType(System.Void) Then
                                        _TB.Inlines.Add(New Run With {.Text = " As ", .Foreground = UIPalette.Keyword})
                                        _TB.Inlines.Add(New Run With {.Text = Met.ReturnType.Name, .Foreground = UIPalette.Type})
                                    End If
                                    SuggestionsControls.Add(_TB)
                                    Dim Params = Met.GetParameters()
                                    Dim i = sCom.Length - 4
                                    If i <= Params.Length Then
                                        Dim Param = Params(i - 1)
                                        Dim TB As New TextBlock With {.FontFamily = MainFontFamily}
                                        If Param.IsOptional Then TB.Inlines.Add(New Run With {.Text = "Optional ", .Foreground = UIPalette.Keyword})
                                        TB.Inlines.Add(New Run With {.Text = Param.Name, .Foreground = UIPalette.Text})
                                        TB.Inlines.Add(New Run With {.Text = " As ", .Foreground = UIPalette.Keyword})
                                        TB.Inlines.Add(New Run With {.Text = Param.ParameterType.Name, .Foreground = UIPalette.Type})
                                        SuggestionsControls.Add(TB)
                                    End If
                                End If
                            End If
                        Case Else
                            Dim Ref = Aqua.ReferenceBinder?.GetReference(sCom.FirstOrDefault.Substring(1, sCom.FirstOrDefault.Length - 1))
                            If Aqua.GetSetting("auto") Then
                                Dim Met = If(Ref, Aqua.GetVariable(sCom.FirstOrDefault))?.GetType.GetMethods.FirstOrDefault(Function(k) k.Name = sCom(2).Remove(0, 1).Remove(sCom(2).Length - 2, 1))
                                If Met IsNot Nothing Then
                                    Dim _TB As New TextBlock With {.FontFamily = MainFontFamily}
                                    _TB.Inlines.Add(New Run With {.Text = If(Met.ReturnType Is GetType(System.Void), "Sub ", "Function "), .Foreground = UIPalette.Keyword})
                                    _TB.Inlines.Add(New Run With {.Text = Met.Name, .Foreground = UIPalette.Method})
                                    _TB.Inlines.Add(New Run With {.Text = "(", .Foreground = UIPalette.Text})
                                    Dim _i = 0
                                    For Each param In Met.GetParameters
                                        If _i > 0 Then _TB.Inlines.Add(New Run With {.Text = ", ", .Foreground = UIPalette.Text})
                                        If param.IsOptional Then _TB.Inlines.Add(New Run With {.Text = "Optional ", .Foreground = UIPalette.Keyword})
                                        _TB.Inlines.Add(New Run With {.Text = param.Name, .Foreground = UIPalette.Text})
                                        _TB.Inlines.Add(New Run With {.Text = " As ", .Foreground = UIPalette.Keyword})
                                        _TB.Inlines.Add(New Run With {.Text = param.ParameterType.Name, .Foreground = UIPalette.Type})
                                        _i += 1
                                    Next
                                    _TB.Inlines.Add(New Run With {.Text = ")", .Foreground = UIPalette.Text})
                                    If Met.ReturnType IsNot GetType(System.Void) Then
                                        _TB.Inlines.Add(New Run With {.Text = " As ", .Foreground = UIPalette.Keyword})
                                        _TB.Inlines.Add(New Run With {.Text = Met.ReturnType.Name, .Foreground = UIPalette.Type})
                                    End If
                                    SuggestionsControls.Add(_TB)
                                    Dim Params = Met.GetParameters()
                                    Dim i = sCom.Length - 3
                                    If i <= Params.Length Then
                                        Dim Param = Params(i - 1)
                                        Dim TB As New TextBlock With {.FontFamily = MainFontFamily}
                                        If Param.IsOptional Then TB.Inlines.Add(New Run With {.Text = "Optional ", .Foreground = UIPalette.Keyword})
                                        TB.Inlines.Add(New Run With {.Text = Param.Name, .Foreground = UIPalette.Text})
                                        TB.Inlines.Add(New Run With {.Text = " As ", .Foreground = UIPalette.Keyword})
                                        TB.Inlines.Add(New Run With {.Text = Param.ParameterType.Name, .Foreground = UIPalette.Type})
                                        SuggestionsControls.Add(TB)
                                    End If
                                End If
                            Else
                                If sCom.Length < 5 Then Return
                                Dim Met = If(Ref, Aqua.GetVariable(sCom.FirstOrDefault))?.GetType.GetMethods.FirstOrDefault(Function(k) k.Name = sCom(2).Remove(0, 1).Remove(sCom(2).Length - 2, 1))
                                If Met IsNot Nothing Then
                                    Dim _TB As New TextBlock With {.FontFamily = MainFontFamily}
                                    _TB.Inlines.Add(New Run With {.Text = If(Met.ReturnType Is GetType(System.Void), "Sub ", "Function "), .Foreground = UIPalette.Keyword})
                                    _TB.Inlines.Add(New Run With {.Text = Met.Name, .Foreground = UIPalette.Method})
                                    _TB.Inlines.Add(New Run With {.Text = "(", .Foreground = UIPalette.Text})
                                    Dim _i = 0
                                    For Each param In Met.GetParameters
                                        If _i > 0 Then _TB.Inlines.Add(New Run With {.Text = ", ", .Foreground = UIPalette.Text})
                                        If param.IsOptional Then _TB.Inlines.Add(New Run With {.Text = "Optional ", .Foreground = UIPalette.Keyword})
                                        _TB.Inlines.Add(New Run With {.Text = param.Name, .Foreground = UIPalette.Text})
                                        _TB.Inlines.Add(New Run With {.Text = " As ", .Foreground = UIPalette.Keyword})
                                        _TB.Inlines.Add(New Run With {.Text = param.ParameterType.Name, .Foreground = UIPalette.Type})
                                        _i += 1
                                    Next
                                    _TB.Inlines.Add(New Run With {.Text = ")", .Foreground = UIPalette.Text})
                                    If Met.ReturnType IsNot GetType(System.Void) Then
                                        _TB.Inlines.Add(New Run With {.Text = " As ", .Foreground = UIPalette.Keyword})
                                        _TB.Inlines.Add(New Run With {.Text = Met.ReturnType.Name, .Foreground = UIPalette.Type})
                                    End If
                                    SuggestionsControls.Add(_TB)
                                    Dim Params = Met.GetParameters()
                                    Dim i = sCom.Length - 4
                                    If i <= Params.Length Then
                                        Dim Param = Params(i - 1)
                                        Dim TB As New TextBlock With {.FontFamily = MainFontFamily}
                                        If Param.IsOptional Then TB.Inlines.Add(New Run With {.Text = "Optional ", .Foreground = UIPalette.Keyword})
                                        TB.Inlines.Add(New Run With {.Text = Param.Name, .Foreground = UIPalette.Text})
                                        TB.Inlines.Add(New Run With {.Text = " As ", .Foreground = UIPalette.Keyword})
                                        TB.Inlines.Add(New Run With {.Text = Param.ParameterType.Name, .Foreground = UIPalette.Type})
                                        SuggestionsControls.Add(TB)
                                    End If
                                End If
                            End If
                    End Select
            End Select
        End Sub

        Private Sub GenerateSuggestions(query As String, method As Reflection.MethodInfo())
            SuggestionsControls.Clear()
            If method Is Nothing Then Return
            Dim _Mets = method.Where(Function(k) k.Name.StartsWith(query))
            For Each _Met In _Mets
                If _Met IsNot Nothing Then
                    Dim TB As New TextBlock With {.FontFamily = MainFontFamily}
                    TB.Inlines.Add(New Run With {.Text = If(_Met.ReturnType Is GetType(System.Void), "Sub ", "Function "), .Foreground = UIPalette.Keyword})
                    TB.Inlines.Add(New Run With {.Text = _Met.Name, .Foreground = UIPalette.Method})
                    TB.Inlines.Add(New Run With {.Text = "(", .Foreground = UIPalette.Text})
                    Dim _i = 0
                    For Each param In _Met.GetParameters
                        If _i > 0 Then TB.Inlines.Add(New Run With {.Text = ", ", .Foreground = UIPalette.Text})
                        If param.IsOptional Then TB.Inlines.Add(New Run With {.Text = "Optional ", .Foreground = UIPalette.Keyword})
                        TB.Inlines.Add(New Run With {.Text = param.Name, .Foreground = UIPalette.Text})
                        TB.Inlines.Add(New Run With {.Text = " As ", .Foreground = UIPalette.Keyword})
                        TB.Inlines.Add(New Run With {.Text = param.ParameterType.Name, .Foreground = UIPalette.Type})
                        _i += 1
                    Next
                    TB.Inlines.Add(New Run With {.Text = ")", .Foreground = UIPalette.Text})
                    If _Met.ReturnType IsNot GetType(System.Void) Then
                        TB.Inlines.Add(New Run With {.Text = " As ", .Foreground = UIPalette.Keyword})
                        TB.Inlines.Add(New Run With {.Text = _Met.ReturnType.Name, .Foreground = UIPalette.Type})
                    End If
                    SuggestionsControls.Add(TB)
                End If
            Next
        End Sub
        Private Sub LockMethod(method As Reflection.MethodInfo)
            LockedMethod = method
        End Sub
        Private Sub LockMethods(typename As String, methods As Reflection.MethodInfo())
            LockedMethods = New KeyValuePair(Of String, Reflection.MethodInfo())(typename, methods)
        End Sub

        Private Sub Aqua_IsRunningChanged(NewValue As Boolean)
            Application.Current.Dispatcher.Invoke(Sub()
                                                      IDE_AquaIsRunning = NewValue
                                                  End Sub)
        End Sub

        Private Sub Aqua_RunningLineChanged(OldValue As String, NewValue As String)
            Application.Current.Dispatcher.Invoke(Sub()
                                                      IDE_AquaRunningLine = NewValue
                                                  End Sub)
        End Sub

        Private Sub Aqua_ErrorOccured(ex As Exception)
            Utilities.DebugMode.Instance.Log(Of Aqua.Aqua)(ex.ToString)
        End Sub
#Region "IDE"
        Public Shared IDE_AquaIsRunningProperty As DependencyProperty = DependencyProperty.Register("IDE_AquaIsRunning", GetType(Boolean), GetType(DeveloperConsole), New UIPropertyMetadata(False))
        Property IDE_AquaIsRunning As Boolean
            Get
                Return GetValue(IDE_AquaIsRunningProperty)
            End Get
            Set(value As Boolean)
                SetValue(IDE_AquaIsRunningProperty, value)
            End Set
        End Property

        Public Shared IDE_AquaRunningLineProperty As DependencyProperty = DependencyProperty.Register("IDE_AquaRunningLine", GetType(String), GetType(DeveloperConsole))
        Property IDE_AquaRunningLine As String
            Get
                Return GetValue(IDE_AquaRunningLineProperty)
            End Get
            Set(value As String)
                SetValue(IDE_AquaRunningLineProperty, value)
            End Set
        End Property

        Public Shared IDE_FilePathProperty As DependencyProperty = DependencyProperty.Register("IDE_FilePath", GetType(String), GetType(DeveloperConsole))
        Property IDE_FilePath As String
            Get
                Return GetValue(IDE_FilePathProperty)
            End Get
            Set(value As String)
                SetValue(IDE_FilePathProperty, value)
            End Set
        End Property

        Public Shared IDE_FileTextProperty As DependencyProperty = DependencyProperty.Register("IDE_FileText", GetType(String), GetType(DeveloperConsole))
        Property IDE_FileText As String
            Get
                Return GetValue(IDE_FileTextProperty)
            End Get
            Set(value As String)
                SetValue(IDE_FileTextProperty, value)
            End Set
        End Property

        Public Shared IDE_FileNameProperty As DependencyProperty = DependencyProperty.Register("IDE_FileName", GetType(String), GetType(DeveloperConsole))
        Property IDE_FileName As String
            Get
                Return GetValue(IDE_FileNameProperty)
            End Get
            Set(value As String)
                SetValue(IDE_FileNameProperty, value)
            End Set
        End Property

        Public Shared IDE_FileDirtyProperty As DependencyProperty = DependencyProperty.Register("IDE_FileDirty", GetType(Boolean), GetType(DeveloperConsole))
        Property IDE_FileDirty As Boolean
            Get
                Return GetValue(IDE_FileDirtyProperty)
            End Get
            Set(value As Boolean)
                SetValue(IDE_FileDirtyProperty, value)
            End Set
        End Property

        Public Shared IDE_FoundErrorsProperty As DependencyProperty = DependencyProperty.Register("IDE_FoundErrors", GetType(ObjectModel.ObservableCollection(Of String)), GetType(DeveloperConsole), New UIPropertyMetadata(New ObjectModel.ObservableCollection(Of String)))
        Property IDE_FoundErrors As ObjectModel.ObservableCollection(Of String)
            Get
                Return GetValue(IDE_FoundErrorsProperty)
            End Get
            Set(value As ObjectModel.ObservableCollection(Of String))
                SetValue(IDE_FoundErrorsProperty, value)
            End Set
        End Property

        Private Sub MenuItem_File_New_Click(sender As Object, e As RoutedEventArgs)
            IDE_FileName = "Untitled"
            IDE_FilePath = ""
            IDE_FileText = "Not Saved"
            IDE_FileDirty = True
        End Sub

        Private Sub MenuItem_File_Open_Click(sender As Object, e As RoutedEventArgs)
            Dim OFD As New Microsoft.Win32.OpenFileDialog With {.CheckFileExists = True, .Filter = "QScript|*.qs", .Title = "Select a file"}
            If OFD.ShowDialog Then
                Try
                    IDE_FileText = IO.File.ReadAllText(OFD.FileName)
                    IDE_FileName = IO.Path.GetFileNameWithoutExtension(OFD.FileName)
                    IDE_FilePath = OFD.FileName
                    IDE_FileDirty = False
                Catch ex As Exception
                    HandyControl.Controls.MessageBox.Error("Couldn't open file." & Environment.NewLine & ex.Message)
                End Try
            End If
        End Sub

        Private Sub MenuItem_File_Save_Click(sender As Object, e As RoutedEventArgs)
            Dim SFP As String = IDE_FilePath
            If Not IO.File.Exists(SFP) Then
                Dim SFD As New Microsoft.Win32.SaveFileDialog With {.Filter = "QScript|*.qs"}
                If SFD.ShowDialog Then
                    SFP = SFD.FileName
                    IDE_FileName = SFD.SafeFileName
                    IDE_FilePath = SFP
                Else
                    Return
                End If
            End If
            Try
                IO.File.WriteAllText(SFP, IDE_FileText)
                IDE_FileDirty = False
            Catch ex As Exception
                HandyControl.Controls.MessageBox.Error("Couldn't save file." & Environment.NewLine & ex.Message)
            End Try
        End Sub

        Private Sub MenuItem_File_SaveAs_Click(sender As Object, e As RoutedEventArgs)
            Dim SFD As New Microsoft.Win32.SaveFileDialog With {.FileName = "QScript|*.qs"}
            If SFD.ShowDialog Then
                Try
                    IO.File.WriteAllText(SFD.FileName, IDE_FileText)
                Catch ex As Exception
                    HandyControl.Controls.MessageBox.Error("Couldn't save file." & Environment.NewLine & ex.Message)
                End Try
            Else
                Return
            End If
        End Sub

        Private Sub MenuItem_File_Run_Click(sender As Object, e As RoutedEventArgs)
            Aqua.Reset()
            Aqua.RunScriptProxy(IDE_FileText)
        End Sub

        Private Sub IDEIn_TB_TextChanged(sender As Object, e As TextChangedEventArgs) Handles IDEIn_TB.TextChanged
            IDE_FileDirty = True
        End Sub

        'Private Sub IDEIn_TB_SelectionChanged(sender As Object, e As RoutedEventArgs) Handles IDEIn_TB.SelectionChanged

        'End Sub

        Private Sub IDEIn_TB_PreviewKeyUp(sender As Object, e As KeyEventArgs) Handles IDEIn_TB.PreviewKeyUp
            If e.Key = Key.Enter Then
                'IDE_FormatText()
                'IDEIn_TB.Select(IDEIn_TB.Text.Length, 0)
                PrepareAndGenerateSuggestions(IDEIn_TB.GetLineText(IDEIn_TB.GetLineIndexFromCharacterIndex(IDEIn_TB.CaretIndex)).Trim)
            End If
        End Sub

        Private Sub IDE_FormatText(Optional AddNewLineAtEnd As Boolean = True, Optional AddSpaceAtEnd As Boolean = False)
            Dim FormattedText As String = ""
            Dim NestedBlockCount As Integer = 0
            IDE_FoundErrors.Clear()
            Dim enu = IDE_FileText.Split(New Char() {vbCr, vbLf, vbCrLf}, StringSplitOptions.RemoveEmptyEntries).GetEnumerator()
            Dim CLineI As Integer = 0
            Do While enu.MoveNext
                Dim CLine = enu.Current?.ToString.Trim(New Char() {" "})
                CLineI += 1
                Dim FLine As String = ""
                If CLine.ToLower.StartsWith("end if") Then NestedBlockCount -= 1
                FLine &= If(CLine.ToLower.StartsWith("elseif"), "", Space(NestedBlockCount * 4))
                If CLine.ToLower.StartsWith("if") Then NestedBlockCount += 1
                Dim SCline = CLine.Split(New Char() {" "}, StringSplitOptions.RemoveEmptyEntries)
                For i As Integer = 0 To SCline.Length - 1
                    Dim Done As Boolean = True
                    Select Case i
                        Case 0
                            Select Case SCline(i).ToLower
                                Case "if"
                                    FLine &= "If"
                                Case "elseif"
                                    FLine &= "ElseIf"
                                Case "else"
                                    FLine &= "Else"
                                Case "end"
                                    FLine &= "End"
                                Case Else
                                    If SCline.Length >= 2 Then
                                        If SCline(1).ToLower <> "as" AndAlso SCline(1).ToLower <> "=" Then
                                            Select Case SCline(i).ToLower
                                                Case "aqua"
                                                    FLine &= "Aqua"
                                                Case Else
                                                    Dim FRef = Aqua.ReferenceBinder.GetAvailableReferencesNames.Select(Of String)(Function(k) k.ToLower).FirstOrDefault(Function(l) l.ToLower = SCline(1).ToLower)
                                                    If String.IsNullOrEmpty(FRef) Then
                                                        Dim FKey = Aqua.FindMatchingKey(SCline(i))
                                                        If String.IsNullOrEmpty(FKey) Then
                                                            IDE_FoundErrors.Add("Couldn't find the reference """ & SCline(i) & """. At L:" & CLineI & ", C:" & i)
                                                            Done = False
                                                        Else
                                                            FLine &= FKey
                                                        End If
                                                    Else
                                                        FLine &= FRef
                                                    End If
                                            End Select
                                        Else
                                            Done = False
                                        End If
                                    Else
                                        FLine &= SCline(i)
                                    End If
                            End Select
                        Case 1
                            If Not Aqua.Operators.Contains(SCline(i)) Then
                                IDE_FoundErrors.Add("Expected one of the operators. At L:" & CLineI & ", C:" & i)
                                Done = False
                            Else
                                FLine &= SCline(i)
                            End If
                        Case Else
                            Done = False
                    End Select
                    If Not Done Then
                        FLine &= SCline(i)
                    End If
                    FLine &= " "
                Next
                FormattedText &= FLine & If(AddNewLineAtEnd, Environment.NewLine, "")
            Loop
            IDE_FileText = FormattedText
        End Sub

        Private Sub Thumb_SideBar_Right_DragDelta(sender As Object, e As Primitives.DragDeltaEventArgs)
            Dim Change = (0 - e.HorizontalChange)
            Dim AddChange = Grid_SideBar_Right.Width + Change
            Grid_SideBar_Right.Width = If(AddChange > Me.Width / 2, Me.Width / 2, If(AddChange < 150, 150, AddChange))
        End Sub

        Private Sub Thumb_ConsoleOut_DragDelta(sender As Object, e As Primitives.DragDeltaEventArgs)
            Dim Change = (0 - e.VerticalChange)
            Dim AddChange = ConsoleOut_TB_SV.Height + Change
            ConsoleOut_TB_SV.Height = If(AddChange > Me.Height / 2, Me.Height / 2, If(AddChange < 150, 150, AddChange))
        End Sub
#End Region
    End Class
End Namespace