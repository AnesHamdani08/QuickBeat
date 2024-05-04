Imports System.ComponentModel

Namespace Dialogs
    Public Class InputBox

        Private _SafeToClose As Boolean = False

        Shared ReadOnly Property DescriptionProperty As DependencyProperty = DependencyProperty.Register("Description", GetType(String), GetType(InputBox))
        Property Description As String
            Get
                Return GetValue(DescriptionProperty)
            End Get
            Set(value As String)
                SetValue(DescriptionProperty, value)
            End Set
        End Property

        Shared ReadOnly Property FooterProperty As DependencyProperty = DependencyProperty.Register("Footer", GetType(String), GetType(InputBox))
        Property Footer As String
            Get
                Return GetValue(FooterProperty)
            End Get
            Set(value As String)
                SetValue(FooterProperty, value)
            End Set
        End Property

        Default ReadOnly Property Value(name As String) As String
            Get
                Return If(VirtualizingStackPanel_MainContent.Children.OfType(Of TextBox).FirstOrDefault(Function(k) HandyControl.Controls.TitleElement.GetTitle(k) = name)?.Text,
                     VirtualizingStackPanel_MainContent.Children.OfType(Of ComboBox).FirstOrDefault(Function(k) HandyControl.Controls.TitleElement.GetTitle(k) = name)?.SelectedIndex)
            End Get
        End Property

        ReadOnly Property Values() As IEnumerable(Of KeyValuePair(Of String, String))
            Get
                Dim TValues = VirtualizingStackPanel_MainContent.Children.OfType(Of TextBox).Select(Of KeyValuePair(Of String, String))(Function(k) New KeyValuePair(Of String, String)(HandyControl.Controls.TitleElement.GetTitle(k), k.Text))
                Dim CValues = VirtualizingStackPanel_MainContent.Children.OfType(Of ComboBox).Select(Of KeyValuePair(Of String, String))(Function(k) New KeyValuePair(Of String, String)(HandyControl.Controls.TitleElement.GetTitle(k), k.SelectedIndex))
                Return TValues.Concat(CValues)
            End Get
        End Property

        ''' <summary>
        ''' Adds a <see cref="TextBox"/> as the first item of the input box
        ''' </summary>
        ''' <param name="Name">Field name</param>
        ''' <param name="IsNeccessary">Is field neccessary</param>
        ''' <param name="DefaultInput">The text in the field</param>
        Public Sub AddTextBox(Name As String, Optional IsNeccessary As Boolean = True, Optional DefaultInput As String = "", Optional PlaceHolder As String = "")
            Dim TB As New TextBox With {.Text = DefaultInput, .Tag = "Take Me Home!"}
            HandyControl.Controls.TitleElement.SetTitle(TB, Name)
            HandyControl.Controls.InfoElement.SetNecessary(TB, IsNeccessary)
            HandyControl.Controls.InfoElement.SetPlaceholder(TB, PlaceHolder)

            VirtualizingStackPanel_MainContent.Children.Insert(1, TB)
        End Sub

        ''' <summary>
        ''' Adds a <see cref="ComboBox"/> as the first item of the input box
        ''' </summary>
        ''' <param name="Name">Field name</param>
        ''' <param name="IsNeccessary">Is field neccessary</param>
        ''' <param name="DefaultInput">The selected index in the field</param>
        Public Sub AddComboBox(Name As String, Items As IEnumerable(Of Object), Optional IsNeccessary As Boolean = True, Optional PlaceHolder As String = "", Optional DefaultInput As Integer = -1)
            Dim CB As New ComboBox With {.SelectedIndex = DefaultInput, .Tag = "Take Me Home!"}
            HandyControl.Controls.TitleElement.SetTitle(CB, Name)
            HandyControl.Controls.InfoElement.SetNecessary(CB, IsNeccessary)
            HandyControl.Controls.InfoElement.SetPlaceholder(CB, PlaceHolder)
            For Each obj In Items
                CB.Items.Add(obj)
            Next
            VirtualizingStackPanel_MainContent.Children.Insert(1, CB)
        End Sub

        Private Sub InputBox_Closing(sender As Object, e As CancelEventArgs) Handles Me.Closing
            If Not _SafeToClose Then
                DialogResult = False
            End If
        End Sub

        ''' <summary>
        ''' Shows an inputbox with a single input, something like <see cref="Interaction.InputBox(String, String, String, Integer, Integer)"/> with single parameter
        ''' </summary>
        ''' <returns></returns>
        Shared Function ShowSingle(title As String, Optional DefaultResponse As String = "") As String
            Dim ib As New InputBox() With {.Title = title}
            ib.AddTextBox(title, True, DefaultResponse)
            If ib.ShowDialog() Then
                Return ib.Value(title)
            Else
                Return ""
            End If
        End Function

        Shared Function ShowSingle(Owner As System.Windows.Window, title As String, Optional DefaultResponse As String = "") As String
            Dim ib As New InputBox() With {.Owner = Owner, .Title = title, .WindowStartupLocation = WindowStartupLocation.CenterOwner}
            ib.AddTextBox(title, True, DefaultResponse)
            If ib.ShowDialog() Then
                Return ib.Value(title)
            Else
                Return ""
            End If
        End Function

        Shared Function ShowSingle(Owner As System.Windows.Window, title As String, description As String, Optional DefaultResponse As String = "") As String
            Dim ib As New InputBox() With {.Owner = Owner, .Title = title, .Description = description, .WindowStartupLocation = WindowStartupLocation.CenterOwner}
            ib.AddTextBox(title, True, DefaultResponse)
            If ib.ShowDialog() Then
                Return ib.Value(title)
            Else
                Return ""
            End If
        End Function

        Shared Function ShowSingle(Owner As System.Windows.Window, title As String, description As String, footer As String, Optional DefaultResponse As String = "") As String
            Dim ib As New InputBox() With {.Owner = Owner, .Title = title, .Description = description, .Footer = footer, .WindowStartupLocation = WindowStartupLocation.CenterOwner}
            ib.AddTextBox(title, True, DefaultResponse)
            If ib.ShowDialog() Then
                Return ib.Value(title)
            Else
                Return ""
            End If
        End Function

        Private Sub Commands_Continue_CanExecute(sender As Object, e As CanExecuteRoutedEventArgs)
            Dim Res0 = VirtualizingStackPanel_MainContent.Children.OfType(Of TextBox).Any(Function(k) HandyControl.Controls.InfoElement.GetNecessary(k) AndAlso String.IsNullOrEmpty(k.Text))
            Dim Res1 = VirtualizingStackPanel_MainContent.Children.OfType(Of ComboBox).Any(Function(k) HandyControl.Controls.InfoElement.GetNecessary(k) AndAlso k.SelectedIndex = -1)
            e.CanExecute = Not (Res0 OrElse Res1)
        End Sub

        Private Sub Commands_Continue_Executed(sender As Object, e As ExecutedRoutedEventArgs)
            _SafeToClose = True
            DialogResult = True
        End Sub

        Private Sub InputBox_PreviewKeyUp(sender As Object, e As KeyEventArgs) Handles Me.PreviewKeyUp
            Select Case e.Key
                Case Key.Escape
                    Close()
                Case Key.Enter
                    If Button_Continue.Command IsNot Nothing Then
                        If Button_Continue.Command.CanExecute(Nothing) Then Button_Continue.Command.Execute(Nothing)
                    End If
            End Select
        End Sub
    End Class
End Namespace