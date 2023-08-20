Imports System.Runtime.CompilerServices

Namespace Utilities
    Module CommonFunctions
        'modifiers
        Public Const NOMOD = &H0

        Public Const ALT = &H1
        Public Const CTRL = &H2
        Public Const SHIFT = &H4
        Public Const WIN = &H8

        'windows message id for hotkey
        Public Const WM_HOTKEY_MSG_ID = &H312

        Public Async Function SaveCoverToFile(path As String, data As Byte()) As Task
            Using fs As New IO.FileStream(path, IO.FileMode.OpenOrCreate, IO.FileAccess.Write)
                Dim Enco As New PngBitmapEncoder
                Enco.Frames.Add(BitmapFrame.Create(New IO.MemoryStream(data)))
                Enco.Save(fs)
                Await fs.FlushAsync
                fs.Close()
            End Using
        End Function

        <Extension>
        Public Function ToCoverImage(text As String) As DrawingImage
            Dim dv As New DrawingVisual()
            Dim dc = dv.RenderOpen
            Dim recBrush As New LinearGradientBrush(New GradientStopCollection From {New GradientStop(Color.FromRgb(38, 50, 56), 0), New GradientStop(Color.FromRgb(69, 90, 100), 1)}, 45)
            dc.DrawRoundedRectangle(recBrush, Nothing, New Rect(0, 0, 100, 100), 15, 15)
#Disable Warning
            Dim txt = New FormattedText(text.FirstOrDefault & text.LastOrDefault, Globalization.CultureInfo.CurrentCulture, FlowDirection.LeftToRight, New Typeface(New FontFamily("Arial"), FontStyles.Normal, FontWeights.DemiBold, FontStretches.Normal), 132 / 2, New SolidColorBrush(Color.FromRgb(255, 193, 7))) '2 for text length                        
#Enable Warning
            dc.DrawText(txt, New Point((100 - txt.Width) / 2, (100 - txt.Height) / 2))
            dc.Close()
            Return New DrawingImage(dv.Drawing)
        End Function

        Public Function GenerateCoverImage(geometry As Geometry) As DrawingImage
            Dim dv As New DrawingVisual()
            Dim dc = dv.RenderOpen
            Dim recBrush As New LinearGradientBrush(New GradientStopCollection From {New GradientStop(Color.FromRgb(38, 50, 56), 0), New GradientStop(Color.FromRgb(69, 90, 100), 1)}, 45)
            dc.DrawRoundedRectangle(recBrush, Nothing, New Rect(0, 0, 100, 100), 15, 15)
            dc.DrawImage(geometry.ToImageSource(New SolidColorBrush(Color.FromRgb(255, 193, 7)), Nothing), New Rect(10, 10, 80, 80))
            dc.Close()
            Return New DrawingImage(dv.Drawing)
        End Function

        Public Function GetFiles(Path As String, Optional Filters As String = "*.mp3|*.m4a|*.mp4|*.wav|*.aiff|*.mp2|*.mp1|*.ogg|*.wma|*.flac|*.alac|*.webm|*.midi|*.mid") As IEnumerable(Of String)
            If Not IO.Directory.Exists(Path) Then Return Nothing
            Return Filters.Split("|"c).SelectMany(Function(filter) System.IO.Directory.GetFiles(Path, filter, IO.SearchOption.AllDirectories)).ToArray()
        End Function

        Public Function GetFiles(Paths As IEnumerable(Of String), Optional Filters As String = "*.mp3|*.m4a|*.mp4|*.wav|*.aiff|*.mp2|*.mp1|*.ogg|*.wma|*.flac|*.alac|*.webm|*.midi|*.mid") As IEnumerable(Of String)
            Return Paths.SelectMany(Function(Path) Filters.Split("|"c).SelectMany(Function(filter) If(IO.Directory.Exists(Path), System.IO.Directory.GetFiles(Path, filter, IO.SearchOption.AllDirectories).ToArray(), New String() {})))
        End Function

        <Extension>
        Public Function GetImageStats(ByVal bitmap As BitmapSource) As List(Of KeyValuePair(Of Color, Integer))
            If bitmap Is Nothing Then Return New List(Of KeyValuePair(Of Color, Integer)) From {New KeyValuePair(Of Color, Integer)(Colors.Black, 1)}
            Dim format = bitmap.Format
            If format <> PixelFormats.Bgr24 AndAlso format <> PixelFormats.Bgr32 AndAlso format <> PixelFormats.Bgra32 AndAlso format <> PixelFormats.Pbgra32 Then
                Throw New InvalidOperationException("BitmapSource must have Bgr24, Bgr32, Bgra32 Or Pbgra32 format")
                Return Nothing
            End If

            Dim width = bitmap.PixelWidth
            Dim height = bitmap.PixelHeight
            Dim numPixels = width * height
            Dim bytesPerPixel = format.BitsPerPixel / 8
            Dim pixelBuffer = New Byte(numPixels * bytesPerPixel - 1) {}
            bitmap.CopyPixels(pixelBuffer, width * bytesPerPixel, 0)
            Dim pixels As New Dictionary(Of Color, Integer)
            Dim i As Integer = 0
            Dim _i As Integer = 0

            While i < pixelBuffer.Length
                Dim _color = Color.FromRgb(pixelBuffer(i), pixelBuffer(i + 1), pixelBuffer(i + 2))
                If pixels.ContainsKey(_color) Then
                    pixels(_color) = pixels(_color) + 1
                Else
                    pixels.Add(_color, 1)
                End If
                i += bytesPerPixel
                _i += 1
            End While

            Dim OrderedResult = pixels.OrderByDescending(Function(k) CInt(k.Value)).ToList
            Return OrderedResult
        End Function

        ''' <summary>
        ''' Determins most dominant colors from image histogram
        ''' </summary>
        ''' <param name="ImageStats">Result of <see cref="GetImageStats(BitmapSource)"/></param>
        ''' <returns></returns>
        Public Function Get3DominantColors(ImageStats As List(Of KeyValuePair(Of Color, Integer))) As Color()
            Dim DomiColors(2) As Color
            Dim i As Integer = 1
            DomiColors(0) = ImageStats.FirstOrDefault.Key

            For Each pixel In ImageStats
                Select Case i
                    Case 1
                        If Math.Abs(pixel.Key.AverageRGB - DomiColors(0).AverageRGB) >= 30 Then
                            DomiColors(1) = pixel.Key
                            i += 1
                        End If
                    Case 2
                        If (Math.Abs(pixel.Key.AverageRGB - DomiColors(0).AverageRGB) >= 30) AndAlso (Math.Abs(pixel.Key.AverageRGB - DomiColors(1).AverageRGB) >= 30) Then
                            DomiColors(2) = pixel.Key
                            i += 1
                        End If
                End Select
                If i = 3 Then Exit For
            Next

            Return DomiColors
        End Function

        ''' <summary>
        ''' The resulting luma value range is 0..255, where 0 is the darkest and 255 is the lightest. Values greater than 128 are considered light.
        ''' </summary>
        ''' <param name="color">The color to calculate luminance for</param>
        ''' <returns></returns>
        <Extension>
        Public Function Luminance(color As Color) As Double
            Return (0.299 * color.R + 0.587 * color.G + 0.114 * color.B)
        End Function

        ''' <summary>
        ''' If an exception occures, returns a black transparent color (0,0,0,0)
        ''' </summary>
        ''' <param name="bitmap">The image to calculate its average color</param>
        ''' <param name="Opacity">The return color's opacity</param>
        ''' <returns></returns>
        <Extension>
        Public Function GetAverageColor(ByVal bitmap As BitmapSource, Optional Opacity As Integer = 255) As System.Windows.Media.Color
            If bitmap Is Nothing Then Return Colors.Black
            Dim format As PixelFormat
            Try
                format = bitmap.Format
            Catch ex As Exception
                If SharedProperties.Instance.IsLogging Then Utilities.DebugMode.Instance.Log(Of Application)(ex.ToString)
                Return Color.FromArgb(0, 0, 0, 0)
            End Try
            If format <> PixelFormats.Bgr24 AndAlso format <> PixelFormats.Bgr32 AndAlso format <> PixelFormats.Bgra32 AndAlso format <> PixelFormats.Pbgra32 Then
                'Throw New InvalidOperationException("BitmapSource must have Bgr24, Bgr32, Bgra32 Or Pbgra32 format")
                Return Nothing
            End If

            Dim width = bitmap.PixelWidth
            Dim height = bitmap.PixelHeight
            Dim numPixels = width * height
            Dim bytesPerPixel = format.BitsPerPixel / 8
            Dim pixelBuffer = New Byte(numPixels * bytesPerPixel - 1) {}
            bitmap.CopyPixels(pixelBuffer, width * bytesPerPixel, 0)
            Dim blue As Long = 0
            Dim green As Long = 0
            Dim red As Long = 0
            Dim i As Integer = 0

            While i < pixelBuffer.Length
                blue += pixelBuffer(i)
                green += pixelBuffer(i + 1)
                red += pixelBuffer(i + 2)
                i += bytesPerPixel
            End While

            Return System.Windows.Media.Color.FromArgb(CByte(Opacity), CByte((red / numPixels)), CByte((green / numPixels)), CByte((blue / numPixels)))
        End Function

        <Extension>
        Public Function GetInverseColor(Color As System.Windows.Media.Color, Optional Opacity As Integer = 255) As System.Windows.Media.Color
            Dim R = Color.R
            Dim G = Color.G
            Dim B = Color.B
            Dim newR = 255 - R
            Dim newG = 255 - G
            Dim newB = 255 - B
            Return System.Windows.Media.Color.FromArgb(Opacity, newR, newG, newB)
        End Function

        <Extension>
        Public Function ToWPFColor(GDIColor As System.Drawing.Color) As System.Windows.Media.Color
            Return Color.FromArgb(GDIColor.A, GDIColor.R, GDIColor.G, GDIColor.B)
        End Function
        <Extension>
        Public Function ToGDIColor(Color As Color) As System.Drawing.Color
            Return System.Drawing.Color.FromArgb(Color.A, Color.R, Color.G, Color.B)
        End Function
        <Extension>
        Public Function AverageRGB(Color As Color) As Double
            Return (CDbl(Color.R) + CDbl(Color.G) + CDbl(Color.B)) / 3
        End Function

        <System.Runtime.InteropServices.DllImport("wininet.dll")>
        Private Function InternetGetConnectedState(ByRef Description As Integer, ByVal ReservedValue As Integer) As Boolean
        End Function
        ''' <summary>
        ''' Uses P/Invoke to check for active internet connection
        ''' </summary>
        ''' <returns></returns>
        Public Function CheckInternetConnection() As Boolean
            Try
                Dim ConnDesc As Integer
                Return InternetGetConnectedState(ConnDesc, 0)
            Catch
                Return False
            End Try
        End Function

        Public Function CheckInternet() As Boolean
            Return HandyControl.Tools.ApplicationHelper.IsConnectedToInternet
        End Function

        <Extension>
        Function ReplaceExcludingBetween(input As String, oldChar As Char, newChar As Char, exclusionstart As Char, exclusionend As Char) As String
            Dim newstring As String = ""
            Dim IsSkipping As Boolean = False
            For Each _char In input
                If IsSkipping Then
                    If _char <> exclusionend Then
                        newstring &= _char
                        Continue For
                    Else
                        IsSkipping = False
                        newstring &= _char
                        Continue For
                    End If
                End If
                If _char = exclusionstart Then
                    newstring &= _char
                    IsSkipping = True
                    Continue For
                End If
                If _char = oldChar Then
                    newstring &= newChar
                Else
                    newstring &= _char
                End If
            Next
            Return newstring
        End Function

        ''' <summary>
        ''' Determines if a type Is numeric.  Nullable numeric types are considered numeric.
        ''' </summary>
        ''' <remarks>
        ''' Boolean Is Not considered numeric.
        ''' </remarks>
        <Extension>
        Public Function IsNumericType(ByVal type As Type) As Boolean
            If type Is Nothing Then
                Return False
            End If

            Select Case Type.GetTypeCode(type)
                Case TypeCode.Byte, TypeCode.Decimal, TypeCode.Double, TypeCode.Int16, TypeCode.Int32, TypeCode.Int64, TypeCode.SByte, TypeCode.Single, TypeCode.UInt16, TypeCode.UInt32, TypeCode.UInt64
                    Return True
                Case TypeCode.Object

                    If type.IsGenericType AndAlso type.GetGenericTypeDefinition() = GetType(Nullable(Of)) Then
                        Return IsNumericType(Nullable.GetUnderlyingType(type))
                    End If

                    Return False
            End Select

            Return False
        End Function

        Public Function SizeConverter(bytes As ULong) As String
            Try
                Select Case bytes
                    Case Is >= 1099511627776
                        Return FormatNumber(CDbl(bytes / 1099511627776), 2) & " TB"
                    Case 1073741824 To 1099511627775
                        Return FormatNumber(CDbl(bytes / 1073741824), 2) & " GB"
                    Case 1048576 To 1073741823
                        Return FormatNumber(CDbl(bytes / 1048576), 2) & " MB"
                    Case 1024 To 1048575
                        Return FormatNumber(CDbl(bytes / 1024), 2) & " KB"
                    Case 0 To 1023
                        Return FormatNumber(bytes, 2) & " Bytes"
                    Case Else
                        Return "ERROR"
                End Select
            Catch
                Return "ERROR"
            End Try
        End Function

        ''' <summary>
        ''' Converts <see cref="IEnumerable(Of QuickBear.Player.Metadata)"/> to <see cref="QuickBeat.Player.Playlist"/>
        ''' </summary>
        ''' <param name="list"></param>
        ''' <remarks>Retains the same reference of the object copied</remarks>
        ''' <returns></returns>
        <Extension>
        Public Function ToPlaylist(list As IEnumerable(Of Player.Metadata)) As Player.Playlist
            Dim x As New Player.Playlist
            For Each item In list
                x.Add(item)
            Next
            Return x
        End Function

        <Runtime.InteropServices.DllImport("gdi32.dll", EntryPoint:="DeleteObject")>
        Public Function DeleteObject(<Runtime.InteropServices.[In]> ByVal hObject As IntPtr) As Boolean
        End Function

        <Extension>
        Public Function ToImageSource(ByVal bmp As System.Drawing.Bitmap, Optional ChangeRes As Boolean = False, Optional ResX As Integer = 0, Optional ResY As Integer = 0) As BitmapSource
            If ChangeRes = False Then
                Try
                    Dim handle = bmp.GetHbitmap()
                    Try
                        Return System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(handle, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions())
                    Finally
                        DeleteObject(handle)
                    End Try
                Catch ex As Exception
                    Return Nothing
                End Try
            Else
                Try
                    Dim ResBmp As New System.Drawing.Bitmap(bmp, ResX, ResY)
                    Dim handle = ResBmp.GetHbitmap()
                    Try
                        Return System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(handle, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions())
                    Finally
                        DeleteObject(handle)
                    End Try
                Catch ex As Exception
                    Return Nothing
                End Try
            End If
        End Function

        ''' <summary>
        ''' Calculates the moving average with the given range over the given array and returns the result.
        ''' </summary>
        ''' <param name="arr">The array to use for calculation.</param>
        ''' <param name="Range">The number of previous and following values to average to the resulting value.</param>        
        <Extension>
        Public Function CalcMovAvg(ByVal arr As Double(), ByVal Range As Byte) As Double()
            'Create an array with the same size as the given one
            Dim ret(UBound(arr)) As Double

            'The indizes between which the values will be averaged; change with each iteration
            Dim FromIndex, ToIndex As Integer
            'Buffer for average calculation; changes with each iteration
            Dim TempAvg As Double

            'Iterate through every element in the given array
            For i As Integer = 0 To UBound(arr)
                'Set start and end indizes (keep array bounds in mind)
                FromIndex = If(i < Range, 0, i - Range)
                ToIndex = If((UBound(arr) - i) < Range, UBound(arr), i + Range)

                'Clear buffer from previous calculations
                TempAvg = 0

                'Calculate the average from arr(FromIndex) to arr(ToIndex)
                For j As Integer = FromIndex To ToIndex
                    TempAvg += arr(j) / (ToIndex + 1 - FromIndex)
                Next

                'Save average in resulting array
                ret(i) = TempAvg
            Next

            'Return result
            Return ret
        End Function
        ''' <summary>
        ''' Calculates the moving average with the given range over the given array and returns the result.
        ''' </summary>
        ''' <param name="arr">The array to use for calculation.</param>
        ''' <param name="Range">The number of previous and following values to average to the resulting value.</param>
        <Extension>
        Public Function CalcMovAvg(ByVal arr As Single(), ByVal Range As Byte) As Single()
            'Create an array with the same size as the given one
            Dim ret(UBound(arr)) As Single

            'The indizes between which the values will be averaged; change with each iteration
            Dim FromIndex, ToIndex As Integer
            'Buffer for average calculation; changes with each iteration
            Dim TempAvg As Single

            'Iterate through every element in the given array
            For i As Integer = 0 To UBound(arr)
                'Set start and end indizes (keep array bounds in mind)
                FromIndex = If(i < Range, 0, i - Range)
                ToIndex = If((UBound(arr) - i) < Range, UBound(arr), i + Range)

                'Clear buffer from previous calculations
                TempAvg = 0

                'Calculate the average from arr(FromIndex) to arr(ToIndex)
                For j As Integer = FromIndex To ToIndex
                    TempAvg += arr(j) / (ToIndex + 1 - FromIndex)
                Next

                'Save average in resulting array
                ret(i) = TempAvg
            Next

            'Return result
            Return ret
        End Function

        Public Function GeometryDrawingFromPath(path As String, brush As System.Windows.Media.Brush, pen As System.Windows.Media.Pen) As System.Windows.Media.DrawingImage
            Return New System.Windows.Media.DrawingImage(New System.Windows.Media.GeometryDrawing(brush, pen, System.Windows.Media.Geometry.Parse(path)))
        End Function
        <Extension>
        Public Function ToImageSource(geomerty As System.Windows.Media.Geometry, brush As System.Windows.Media.Brush, pen As System.Windows.Media.Pen) As System.Windows.Media.DrawingImage
            Return New System.Windows.Media.DrawingImage(New System.Windows.Media.GeometryDrawing(brush, pen, geomerty))
        End Function

        <Extension>
        Public Function ToPlaylist(group As Player.MetadataGroup, Optional cover As ImageSource = Nothing) As Player.Playlist
            Dim x As New Player.Playlist() With {.Category = group.Category, .Name = group.Name, .TotalDuration = group.TotalDuration, .SongAddBehaviour = Player.Playlist.AddBehaviour.Last}
            If cover IsNot Nothing Then x.Cover = cover
            For Each meta In group
                x.Add(meta)
            Next
            Return x
        End Function

        Public Structure EditableKeyValuePair(Of Tkey, Tvalue)
            Public Property Key As Tkey
            Public Property Value As Tvalue

            Public Sub New(key As Tkey, value As Tvalue)
                Me.Key = key
                Me.Value = value
            End Sub

        End Structure

        Public Class MetadataEqualityComparer
            Implements IEqualityComparer(Of Player.Metadata)

            Public Shadows Function Equals(x As Player.Metadata, y As Player.Metadata) As Boolean Implements IEqualityComparer(Of Player.Metadata).Equals
                Return (x.Path = y.Path)
            End Function

            Public Shadows Function GetHashCode(obj As Player.Metadata) As Integer Implements IEqualityComparer(Of Player.Metadata).GetHashCode
                Return $"{obj.Title}{obj.DefaultArtist}{obj.Album}{obj.Path}".GetHashCode
            End Function
        End Class

        Public Enum ClosingBehaviour
            Close
            Minimize
            SystemTray
        End Enum
        Public Enum StartupBehaviour
            Normal
            Minimize
        End Enum
    End Module
End Namespace