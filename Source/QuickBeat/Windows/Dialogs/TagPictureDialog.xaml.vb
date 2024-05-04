Imports System.ComponentModel
Namespace Dialogs
    Public Class TagPictureDialog
        Private _AllowClosing As Boolean = False
        Public ReadOnly Property Result As TagLib.Picture
            Get
                Return TempResult
            End Get
        End Property
        Private Property TempResult As TagLib.Picture

        Private _ImageResult As ImageSource
        Property ImageResult As ImageSource
            Get
                Return _ImageResult
            End Get
            Set(value As ImageSource)
                _ImageResult = value
            End Set
        End Property

        Sub New()

            ' This call is required by the designer.
            InitializeComponent()

            ' Add any initialization after the InitializeComponent() call.

        End Sub

        Public Sub New(Wnd As System.Windows.Window)

            ' This call is required by the designer.
            InitializeComponent()

            ' Add any initialization after the InitializeComponent() call.
            Owner = Wnd
            WindowStartupLocation = WindowStartupLocation.CenterOwner
        End Sub

        Private Sub Tag_Add_MouseLeftButtonUp(sender As Object, e As MouseButtonEventArgs) Handles Tag_Add.MouseLeftButtonUp
            Dim OFD As New Forms.OpenFileDialog With {.Filter = "Supported Images|*.png;*.jpg;*.jpeg|All Files|*.*"}
            If OFD.ShowDialog <> Forms.DialogResult.Cancel Then
                Try
                    Dim Pic As New TagLib.Picture(OFD.FileName)
                    Tag_Description.Text = Pic.Description
                    Tag_MIME_Type.Text = Pic.MimeType
                    Tag_Type.SelectedIndex = If(Pic.Type = TagLib.PictureType.NotAPicture, Tag_Type.Items.Count - 1, Pic.Type)
                    TempResult = Pic
                    _ImageResult = New BitmapImage(New Uri(OFD.FileName))
                    Tag_Picture.Source = ImageResult
                Catch
                End Try
            End If
        End Sub

        Private Sub Tag_Paste_MouseLeftButtonUp(sender As Object, e As MouseButtonEventArgs) Handles Tag_Paste.MouseLeftButtonUp
            Try
                Dim Img = Clipboard.GetImage
                Dim ImgStream As New IO.MemoryStream
                Dim Enco As New PngBitmapEncoder()
                Enco.Frames.Add(BitmapFrame.Create(Img))
                Enco.Save(ImgStream)
                Dim Pic As New TagLib.Picture(New TagLib.ByteVector(ImgStream.ToArray))
                Tag_Description.Text = Pic.Description
                Tag_MIME_Type.Text = Pic.MimeType
                Tag_Type.SelectedIndex = If(Pic.Type = TagLib.PictureType.NotAPicture, Tag_Type.Items.Count - 1, Pic.Type)
                TempResult = Pic
                _ImageResult = Img
                Tag_Picture.Source = Img
            Catch
            End Try
        End Sub

        Private Sub Tag_Save_MouseLeftButtonUp(sender As Object, e As MouseButtonEventArgs) Handles Tag_Save.MouseLeftButtonUp
            TempResult.Description = Tag_Description.Text
            TempResult.Type = If(Tag_Type.SelectedIndex = (Tag_Type.Items.Count - 1), TagLib.PictureType.NotAPicture, Tag_Type.SelectedIndex)
            _AllowClosing = True
            DialogResult = True
        End Sub

        Private Sub TagPictureDialog_Closing(sender As Object, e As CancelEventArgs) Handles Me.Closing
            If _AllowClosing = False Then
                DialogResult = False
            End If
        End Sub

        Private Sub TagPictureDialog_PreviewKeyUp(sender As Object, e As KeyEventArgs) Handles Me.PreviewKeyUp
            If e.Key = Key.Escape Then Close()
        End Sub
    End Class
End Namespace