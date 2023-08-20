Imports System.ComponentModel
Imports System.Runtime.CompilerServices
Namespace QScript
    Public Class SettingsBrowser
        Public Class SettingItem
            Implements INotifyPropertyChanged

            Private _num As Integer
            Private _name As String
            Private _value As String
            Private _type As String
            Public Sub New(pnum As String, pname As String, pvalue As String, ptype As String, <CallerMemberName> ByVal Optional caller As String = Nothing)
                Num = pnum
                Name = pname
                Value = pvalue
                Type = ptype
            End Sub

            Public Property Num As Integer
                Get
                    Return _num
                End Get
                Set(ByVal value As Integer)
                    If _num = value Then Return
                    _num = value
                    OnPropertyChanged()
                End Set
            End Property

            Public Property Name As String
                Get
                    Return _name
                End Get
                Set(ByVal value As String)
                    If _name = value Then Return
                    _name = value
                    OnPropertyChanged()
                End Set
            End Property

            Public Property Value As String
                Get
                    Return _value
                End Get
                Set(ByVal value As String)
                    If _value = value Then Return
                    _value = value
                    OnPropertyChanged()
                End Set
            End Property

            Public Property Type As String
                Get
                    Return _type
                End Get
                Set(ByVal value As String)
                    If _type = value Then Return
                    _type = value
                    OnPropertyChanged()
                End Set
            End Property
            Public Property Tag As System.Configuration.SettingsPropertyValue
            Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged

            Protected Overridable Sub OnPropertyChanged(<CallerMemberName> ByVal Optional propertyName As String = Nothing)
                RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(propertyName))
            End Sub
        End Class

        Private Settings_Source As New ObjectModel.ObservableCollection(Of SettingItem)
        Private _IsBusy As Boolean = False
        Public Property IsBusy As Boolean
            Get
                Return _IsBusy
            End Get
            Set(value As Boolean)
                If value Then
                    Loading_Container.Visibility = Visibility.Visible
                Else
                    Loading_Container.Visibility = Visibility.Collapsed
                End If
                _IsBusy = value
            End Set
        End Property

        Private Sub SettingsBrowser_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
            Main_SettingsView.ItemsSource = Settings_Source
            Dim Dummy = My.Settings.APP_CLOSINGBEHAVIOUR 'just for init. purpose
            Dim i = 0
            Dim templist As New List(Of SettingItem)
            For Each value As System.Configuration.SettingsPropertyValue In My.Settings.PropertyValues
                If value.PropertyValue IsNot Nothing Then
                    If value.PropertyValue.GetType IsNot GetType(Specialized.StringCollection) Then
                        templist.Add(New SettingItem(i + 1, value.Name, value.PropertyValue.ToString, value.PropertyValue.GetType.Name) With {.Tag = value})
                    Else
                        templist.Add(New SettingItem(i + 1, value.Name, TryCast(value.PropertyValue, Specialized.StringCollection).Count & " Item(s)", value.PropertyValue.GetType.Name) With {.Tag = value})
                    End If
                    i += 1
                Else
                    Try
                        templist.Add(New SettingItem(i + 1, value.Name, "Nothing", value.PropertyValue.GetType.Name) With {.Tag = value})
                    Catch ex As Exception
                        templist.Add(New SettingItem(i + 1, value.Name, "Nothing", "N/A") With {.Tag = value})
                    End Try
                    i += 1
                End If
            Next
            templist = templist.OrderBy(Function(k) k.Name).ToList
            For _i As Integer = 0 To templist.Count - 1
                templist(_i).Num = _i + 1
            Next
            For Each si In templist
                Settings_Source.Add(si)
            Next
        End Sub

        Private Sub SettingsBrowser_Closing(sender As Object, e As CancelEventArgs) Handles Me.Closing
            Hide()
            e.Cancel = True
        End Sub

        Private Sub SettingsBrowser_Activated(sender As Object, e As EventArgs) Handles Me.Activated
            If TitleBar_Refresh.IsEnabled = True Then TitleBar_Refresh_Click(Nothing, New RoutedEventArgs)
        End Sub

        Private Sub Main_SettingsView_MouseDoubleClick(sender As Object, e As MouseButtonEventArgs) Handles Main_SettingsView.MouseDoubleClick
            If Main_SettingsView.SelectedIndex = -1 Then Return
            Dim CurSet = Settings_Source(Main_SettingsView.SelectedIndex)?.Tag
            Dim CurType = CurSet?.PropertyValue?.GetType
            If CurType IsNot Nothing Then
                Select Case CurType
                    Case GetType(System.String)
                        Dim IB = InputBox(CurSet.Name & "[" & CurType.FullName & "]" & ",Value=" & CurSet.PropertyValue)
                        If Not String.IsNullOrEmpty(IB) Then
                            CurSet.PropertyValue = IB
                            My.Settings.Save()
                        End If
                    Case GetType(System.Boolean)
                        Dim IB = InputBox(CurSet.Name & "[" & CurType.FullName & "]" & ",Value=" & CurSet.PropertyValue)
                        Dim bIB As Boolean
                        If Boolean.TryParse(IB, bIB) Then
                            CurSet.PropertyValue = bIB
                            My.Settings.Save()
                        End If
                    Case GetType(System.Double)
                        Dim IB = InputBox(CurSet.Name & "[" & CurType.FullName & "]" & ",Value=" & CurSet.PropertyValue)
                        Dim dIB As Double
                        If Double.TryParse(IB, dIB) Then
                            CurSet.PropertyValue = dIB
                            My.Settings.Save()
                        End If
                    Case GetType(System.Int32)
                        Dim IB = InputBox(CurSet.Name & "[" & CurType.FullName & "]" & ",Value=" & CurSet.PropertyValue)
                        Dim iIB As Integer
                        If Integer.TryParse(IB, iIB) Then
                            CurSet.PropertyValue = iIB
                            My.Settings.Save()
                        End If
                    Case GetType(System.Single)
                        Dim IB = InputBox(CurSet.Name & "[" & CurType.FullName & "]" & ",Value=" & CurSet.PropertyValue)
                        Dim sIB As Single
                        If Single.TryParse(IB, sIB) Then
                            CurSet.PropertyValue = sIB
                            My.Settings.Save()
                        End If
                    Case GetType(Specialized.StringCollection)
                        Dim CCol = CType(CurSet.PropertyValue, Specialized.StringCollection)
                        Dim LE As New ListEditor
                        LE.Owner = Me
                        LE.SpecializedToItemsSource(CCol)
                        If LE.ShowDialog() Then
                            CurSet.PropertyValue = LE.ItemsSourceToSpecialized
                            My.Settings.Save()
                        End If
                    Case GetType(System.Uri)
                        Dim IB = InputBox(CurSet.Name & "[" & CurType.FullName & "]" & ",Value=" & CurSet.PropertyValue.ToString & Environment.NewLine & "IsRelative(y or n): " & If(CType(CurSet.PropertyValue, Uri).IsAbsoluteUri, False, True) & Environment.NewLine & "Enter the value and type separated by a "";""")
                        Dim sIB = IB.Split(";")
                        Dim bIB1 As Boolean
                        If Boolean.TryParse(IB, bIB1) Then
                            CurSet.PropertyValue = New Uri(sIB.FirstOrDefault, uriKind:=If(bIB1, UriKind.Relative, UriKind.Absolute))
                            My.Settings.Save()
                        End If
                End Select
            End If
        End Sub

        Private Sub TitleBar_Refresh_Click(sender As Object, e As RoutedEventArgs) Handles TitleBar_Refresh.Click
            IsBusy = True
            TitleBar_Refresh.IsEnabled = False
            Settings_Source.Clear()
            Dim Dummy = My.Settings.APP_CLOSINGBEHAVIOUR 'init. purpose too
            Dim i = 0
            Dim templist As New List(Of SettingItem)
            For Each value As System.Configuration.SettingsPropertyValue In My.Settings.PropertyValues
                If value.PropertyValue IsNot Nothing Then
                    If value.PropertyValue.GetType IsNot GetType(Specialized.StringCollection) Then
                        templist.Add(New SettingItem(i + 1, value.Name, value.PropertyValue.ToString, value.PropertyValue.GetType.Name) With {.Tag = value})
                    Else
                        templist.Add(New SettingItem(i + 1, value.Name, TryCast(value.PropertyValue, Specialized.StringCollection).Count & " Item(s)", value.PropertyValue.GetType.Name) With {.Tag = value})
                    End If
                    i += 1
                Else
                    Try
                        templist.Add(New SettingItem(i + 1, value.Name, "Nothing", value.PropertyValue.GetType.Name) With {.Tag = value})
                    Catch ex As Exception
                        templist.Add(New SettingItem(i + 1, value.Name, "Nothing", "N/A") With {.Tag = value})
                    End Try
                    i += 1
                End If
            Next
            templist = templist.OrderBy(Function(k) k.Name).ToList
            For _i As Integer = 0 To templist.Count - 1
                templist(_i).Num = _i + 1
            Next
            For Each si In templist
                Settings_Source.Add(si)
            Next
            TitleBar_Refresh.IsEnabled = True
            IsBusy = False
        End Sub
    End Class
End Namespace