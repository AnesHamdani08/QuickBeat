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
#Region "File Associations"
        '----------Imported directly from AUDX-------------------
        ' Create the new file association
        '
        ' Extension is the extension to be registered (eg ".cad"
        ' ClassName is the name of the associated class (eg "CADDoc")
        ' Description is the textual description (eg "CAD Document"
        ' ExeProgram is the app that manages that extension (eg "c:\Cad\MyCad.exe")

        <System.Runtime.InteropServices.DllImport("shell32.dll")> Sub _
     SHChangeNotify(ByVal wEventId As Integer, ByVal uFlags As Integer, ByVal dwItem1 As Integer, ByVal dwItem2 As Integer)
        End Sub

        ''' <summary>
        ''' Create the new file association
        ''' </summary>
        ''' <param name="extension">Extension is the extension to be registered (eg ".cad")</param>
        ''' <param name="className">ClassName is the name of the associated class (eg "CADDoc")</param>
        ''' <param name="description">Description is the textual description (eg "CAD Document"</param>
        ''' <param name="exeProgram">ExeProgram is the app that manages that extension (eg "c:\Cad\MyCad.exe")</param>
        ''' <returns></returns>
        Public Function CreateFileAssociation(ByVal extension As String,
    ByVal className As String, ByVal description As String,
    ByVal exeProgram As String) As Exception
            Const SHCNE_ASSOCCHANGED = &H8000000
            Const SHCNF_IDLIST = 0

            ' ensure that there is a leading dot
            If extension.Substring(0, 1) <> "." Then
                extension = "." & extension
            End If

            Dim key1, key2, key3 As Microsoft.Win32.RegistryKey
            Try
                ' create a value for this key that contains the classname
                key1 = Microsoft.Win32.Registry.ClassesRoot.CreateSubKey(extension)
                key1.SetValue("", className)
                ' create a new key for the Class name
                key2 = Microsoft.Win32.Registry.ClassesRoot.CreateSubKey(className)
                key2.SetValue("", description)
                ' associate the program to open the files with this extension
                key3 = Microsoft.Win32.Registry.ClassesRoot.CreateSubKey(className &
            "\Shell\Open\Command")
                key3.SetValue("", exeProgram & " ""%1""")
            Catch ex As Exception
                Return ex
            Finally
#Disable Warning
                If Not key1 Is Nothing Then key1.Close()
                If Not key2 Is Nothing Then key2.Close()
                If Not key3 Is Nothing Then key3.Close()
#Enable Warning
            End Try

            ' notify Windows that file associations have changed
            SHChangeNotify(SHCNE_ASSOCCHANGED, SHCNF_IDLIST, 0, 0)
            Return Nothing
        End Function
        ''' <summary>
        ''' Delete the exisiting file association
        ''' </summary>
        ''' <param name="extension">Extension is the extension to be registered (eg ".cad")</param>
        ''' <param name="className">ClassName is the name of the associated class (eg "CADDoc")</param>        
        ''' <returns></returns>
        Public Function DeleteFileAssociation(ByVal extension As String,
   ByVal className As String) As Exception
            Const SHCNE_ASSOCCHANGED = &H8000000
            Const SHCNF_IDLIST = 0

            ' ensure that there is a leading dot
            If extension.Substring(0, 1) <> "." Then
                extension = extension.Insert(0, ".")
            End If
            Try
                ' delete the key for the Class name
                Microsoft.Win32.Registry.ClassesRoot.DeleteSubKeyTree(className)
            Catch ex As Exception
                Return ex
            End Try

            ' notify Windows that file associations have changed
            SHChangeNotify(SHCNE_ASSOCCHANGED, SHCNF_IDLIST, 0, 0)
            Return Nothing
        End Function
        ''' <summary>
        ''' Check for the file association
        ''' </summary>
        ''' <param name="extension">Extension is the extension to be registered (eg ".cad")</param>
        ''' <param name="className">ClassName is the name of the associated class (eg "CADDoc")</param>        
        ''' <param name="exeProgram">ExeProgram is the app that manages that extension (eg "c:\Cad\MyCad.exe")</param>
        ''' <returns></returns>
        Public Function CheckFileAssociation(ByVal extension As String,
    ByVal className As String, ByVal exeProgram As String) As Boolean

            ' ensure that there is a leading dot
            If extension.Substring(0, 1) <> "." Then
                extension = extension.Insert(0, ".")
            End If

            Dim key2 As Microsoft.Win32.RegistryKey = Nothing
            Try
                ' open a new key for the Class name
                key2 = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(className &
            "\Shell\Open\Command")
                If key2.GetValue("") = exeProgram & " ""%1""" Then
                    Return True
                End If
            Catch
                Return False
            Finally
                If key2 IsNot Nothing Then key2.Close()
            End Try

            Return True
        End Function
        Public Function IsUserAdministrator() As Boolean
            Dim isAdmin As Boolean
            Try
                Dim principal As New Security.Principal.WindowsPrincipal(Security.Principal.WindowsIdentity.GetCurrent())
                isAdmin = principal.IsInRole(Security.Principal.WindowsBuiltInRole.Administrator)
            Catch
                isAdmin = False
            End Try
            Return isAdmin
        End Function
        '-----------------------------------------------------
        'For Convenience
        ''' <summary>
        ''' Method to manage all supported files at once
        ''' </summary>
        ''' <param name="type">
        ''' 0: Create
        ''' 1: Delete
        ''' 2: Check
        ''' </param>
        Public Function ManageAssociations(type As Integer) As Boolean()
            Select Case type
                Case 0
                    Return New Boolean() {
                    CreateFileAssociation(".mp1", "QuickBeat.mp1", "MPEG-1 Audio Layer I", IO.Path.Combine(My.Application.Info.DirectoryPath, "QuickBeat.exe")) Is Nothing,
                    CreateFileAssociation(".mp2", "QuickBeat.mp2", "MPEG-1 Audio Layer II", IO.Path.Combine(My.Application.Info.DirectoryPath, "QuickBeat.exe")) Is Nothing,
                    CreateFileAssociation(".mp3", "QuickBeat.mp3", "MPEG-1 Audio Layer 3", IO.Path.Combine(My.Application.Info.DirectoryPath, "QuickBeat.exe")) Is Nothing,
                    CreateFileAssociation(".mp4", "QuickBeat.mp4", "MPEG-4 Part 14", IO.Path.Combine(My.Application.Info.DirectoryPath, "QuickBeat.exe")) Is Nothing,
                    CreateFileAssociation(".m4a", "QuickBeat.m4a", "MPEG 4 Audio", IO.Path.Combine(My.Application.Info.DirectoryPath, "QuickBeat.exe")) Is Nothing,
                    CreateFileAssociation(".wav", "QuickBeat.wav", "Waveform Audio File Format", IO.Path.Combine(My.Application.Info.DirectoryPath, "QuickBeat.exe")) Is Nothing,
                    CreateFileAssociation(".aiff", "QuickBeat.aiff", "Audio Interchange File Format", IO.Path.Combine(My.Application.Info.DirectoryPath, "QuickBeat.exe")) Is Nothing,
                    CreateFileAssociation(".ogg", "QuickBeat.ogg", "Vorbis", IO.Path.Combine(My.Application.Info.DirectoryPath, "QuickBeat.exe")) Is Nothing,
                    CreateFileAssociation(".wma", "QuickBeat.wma", "Windows Media Audio", IO.Path.Combine(My.Application.Info.DirectoryPath, "QuickBeat.exe")) Is Nothing,
                    CreateFileAssociation(".flac", "QuickBeat.flac", "Free Lossless Audio Codec", IO.Path.Combine(My.Application.Info.DirectoryPath, "QuickBeat.exe")) Is Nothing,
                    CreateFileAssociation(".alac", "QuickBeat.alac", "Apple Lossless Audio Codec", IO.Path.Combine(My.Application.Info.DirectoryPath, "QuickBeat.exe")) Is Nothing,
                    CreateFileAssociation(".webm", "QuickBeat.webm", "WebM", IO.Path.Combine(My.Application.Info.DirectoryPath, "QuickBeat.exe")) Is Nothing,
                    CreateFileAssociation(".midi", "QuickBeat.midi", "Musical Instrument Digital Interface", IO.Path.Combine(My.Application.Info.DirectoryPath, "QuickBeat.exe")) Is Nothing,
                    CreateFileAssociation(".mid", "QuickBeat.mid", "Musical Instrument Digital Interface", IO.Path.Combine(My.Application.Info.DirectoryPath, "QuickBeat.exe")) Is Nothing
                   }
                Case 1
                    Return New Boolean() {
                    DeleteFileAssociation(".mp1", "QuickBeat.mp1") Is Nothing,
                    DeleteFileAssociation(".mp2", "QuickBeat.mp2") Is Nothing,
                    DeleteFileAssociation(".mp3", "QuickBeat.mp3") Is Nothing,
                    DeleteFileAssociation(".mp4", "QuickBeat.mp4") Is Nothing,
                    DeleteFileAssociation(".m4a", "QuickBeat.m4a") Is Nothing,
                    DeleteFileAssociation(".wav", "QuickBeat.wav") Is Nothing,
                    DeleteFileAssociation(".aiff", "QuickBeat.aiff") Is Nothing,
                    DeleteFileAssociation(".ogg", "QuickBeat.ogg") Is Nothing,
                    DeleteFileAssociation(".wma", "QuickBeat.wma") Is Nothing,
                    DeleteFileAssociation(".flac", "QuickBeat.flac") Is Nothing,
                    DeleteFileAssociation(".alac", "QuickBeat.alac") Is Nothing,
                    DeleteFileAssociation(".webm", "QuickBeat.webm") Is Nothing,
                    DeleteFileAssociation(".midi", "QuickBeat.midi") Is Nothing,
                    DeleteFileAssociation(".mid", "QuickBeat.mid") Is Nothing
                   }
                Case 2
                    Return New Boolean() {
                    CheckFileAssociation(".mp1", "QuickBeat.mp1", "MPEG-1 Audio Layer I"),
                    CheckFileAssociation(".mp2", "QuickBeat.mp2", "MPEG-1 Audio Layer II"),
                    CheckFileAssociation(".mp3", "QuickBeat.mp3", "MPEG-1 Audio Layer 3"),
                    CheckFileAssociation(".mp4", "QuickBeat.mp4", "MPEG-4 Part 14"),
                    CheckFileAssociation(".m4a", "QuickBeat.m4a", "MPEG 4 Audio"),
                    CheckFileAssociation(".wav", "QuickBeat.wav", "Waveform Audio File Format"),
                    CheckFileAssociation(".aiff", "QuickBeat.aiff", "Audio Interchange File Format"),
                    CheckFileAssociation(".ogg", "QuickBeat.ogg", "Vorbis"),
                    CheckFileAssociation(".wma", "QuickBeat.wma", "Windows Media Audio"),
                    CheckFileAssociation(".flac", "QuickBeat.flac", "Free Lossless Audio Codec"),
                    CheckFileAssociation(".alac", "QuickBeat.alac", "Apple Lossless Audio Codec"),
                    CheckFileAssociation(".webm", "QuickBeat.webm", "WebM"),
                    CheckFileAssociation(".midi", "QuickBeat.midi", "Musical Instrument Digital Interface"),
                    CheckFileAssociation(".mid", "QuickBeat.mid", "Musical Instrument Digital Interface")
                    }
                Case Else
                    Return Nothing
            End Select
        End Function
        '-----------------------------------------------------
#End Region
        ''' <summary>
        ''' Returns a path specific app data path
        ''' </summary>
        ''' <returns></returns>        
        Public Function GetEncodedSpecialAppName() As String
            Return $"QuickBeat_{My.Application.Info.Version.Major}_{My.Application.Info.Version.Minor}_{My.Application.Info.Version.Build}_{My.Application.Info.Version.Revision}_{My.Application.Info.DirectoryPath.GetHashCode}"
        End Function
        ''' <summary>
        ''' Returns a HostFileName joined string from provided <paramref name="URL"/>, if failed return the <paramref name="URL"/>.
        ''' </summary>
        ''' <param name="URL">The URL to generate UID from.</param>
        ''' <returns></returns>
        Public Function URLtoUID(URL As String) As String
            'e.g. https://free-mp3-download.net/tmp/8e25573a6b06ee94a1eb92ad52f43130/Reol%20-%20Agitate.mp3
            Try
                Dim uri As New Uri(URL)
                If URL.Contains("youtube.com/watch?v=") OrElse URL.ToLower.Contains("youtu.be") Then
                    Return uri.Host & IO.Path.GetFileName(uri.LocalPath) & uri.Query
                Else
                    Return uri.Host & IO.Path.GetFileName(uri.LocalPath)
                End If
            Catch ex As Exception 'failed
                Return URL
            End Try
        End Function
        Public Function CheckUID(UID As String, URL As String) As Boolean
            Dim uri As New Uri(URL)
            Return (uri.Host & IO.Path.GetFileName(uri.LocalPath)) = UID
        End Function
        Public Function GetInternalFileContent(Path As String) As String
            Dim ResStream = Application.GetResourceStream(New Uri(Path, UriKind.Relative))
            If ResStream Is Nothing Then Return Nothing
            Dim sData As String
            Using _sr As New IO.StreamReader(ResStream.Stream)
                sData = _sr.ReadToEnd()
            End Using
            Return sData
        End Function
        Public Async Function GetInternalFileContentAsync(Path As String) As Task(Of String)
            Dim ResStream = Application.GetResourceStream(New Uri(Path, UriKind.Relative))
            If ResStream Is Nothing Then Return Nothing
            Dim sData As String
            Using _sr As New IO.StreamReader(ResStream.Stream)
                sData = Await _sr.ReadToEndAsync
            End Using
            Return sData
        End Function
        Public Function GenerateDLNAMetadata(metadata As Player.Metadata, uri As String) As String
            Dim XDoc As New XDocument
            Dim NSurn As XNamespace = "urn:schemas-upnp-org:metadata-1-0/DIDL-Lite/"
            Dim Root As New XElement(NSurn + "item", New XAttribute("xmlns", "urn:schemas-upnp-org:metadata-1-0/DIDL-Lite/"))
            Dim NSdc As XNamespace = "http://purl.org/dc/elements/1.1/"
            Root.Add(New XElement(NSdc + "title", New XAttribute(XNamespace.Xmlns + "dc", NSdc)) With {.Value = (metadata.Title)})
            Dim NSupnp As XNamespace = "urn:schemas-upnp-org:metadata-1-0/upnp/"
            Root.Add(New XElement(NSupnp + "class", New XAttribute(XNamespace.Xmlns + "upnp", NSupnp)) With {.Value = "object.item.audioItem.musicTrack"})
            Root.Add(New XElement(NSupnp + "genre", New XAttribute(XNamespace.Xmlns + "upnp", NSupnp)) With {.Value = (metadata.JoinedGenres)})
            Root.Add(New XElement(NSupnp + "artist", New XAttribute(XNamespace.Xmlns + "upnp", NSupnp)) With {.Value = (metadata.JoinedArtists)})
            Root.Add(New XElement(NSupnp + "album", New XAttribute(XNamespace.Xmlns + "upnp", NSupnp)) With {.Value = (metadata.Album)})
            Dim iFile = New IO.FileInfo(metadata.Path)
            Dim tag = Utilities.SharedProperties.Instance.RequestTags(metadata.Path, TagLib.ReadStyle.PictureLazy)
            Dim res As New XElement(XName.Get("res")) With {.Value = System.Web.HttpUtility.UrlEncode(If(uri.StartsWith("http"), "", "http://") & (uri.Replace("\"c, "/"c)))} 'Xml.XmlConvert.EncodeName
            res.SetAttributeValue(XName.Get("nrAudioChannels"), If(tag.Properties?.AudioChannels, metadata.Channels))
            res.SetAttributeValue(XName.Get("bitsPerSample"), tag.Properties?.BitsPerSample)
            res.SetAttributeValue(XName.Get("sampleFrequency"), tag.Properties?.AudioSampleRate)
            res.SetAttributeValue(XName.Get("protocolInfo"), $"http-get:*:audio/mpeg:DLNA.ORG_PN=MP3;DLNA.ORG_OP=01;DLNA.ORG_FLAGS=01700000000000000000000000000000")
            res.SetAttributeValue(XName.Get("bitrate"), If(tag.Properties?.AudioBitrate, metadata.Bitrate) * 100)
            res.SetAttributeValue(XName.Get("duration"), If(tag.Properties?.Duration.ToString("g"), metadata.LengthTS.ToString("g")))
            res.SetAttributeValue(XName.Get("size"), iFile.Length)
            Root.Add(res)
            XDoc.Add(Root)
            Return XDoc.ToString
        End Function
        Public Async Function RunProccessAsAdminAndWait(path As String, args As String, Optional timeout As Integer = 0) As Task
            Dim PSI As New ProcessStartInfo With {.FileName = path, .UseShellExecute = True, .Verb = "runas", .Arguments = args}
            Await Task.Run(Sub()
                               If timeout > 0 Then Process.Start(PSI).WaitForExit(timeout) Else Process.Start(PSI).WaitForExit()
                           End Sub)
        End Function
        Public Async Function SaveCoverToFile(path As String, data As Byte()) As Task
            Using fs As New IO.FileStream(path, IO.FileMode.OpenOrCreate, IO.FileAccess.Write)
                Dim Enco As New PngBitmapEncoder
                Enco.Frames.Add(BitmapFrame.Create(New IO.MemoryStream(data)))
                Enco.Save(fs)
                Await fs.FlushAsync
                fs.Close()
            End Using
        End Function

        Public Function SaveCoverToData(bitmap As BitmapSource) As IO.MemoryStream
            Dim mem As New IO.MemoryStream
            Dim Enco As New PngBitmapEncoder
            Enco.Frames.Add(BitmapFrame.Create(bitmap))
            Enco.Save(mem)
            Return mem
        End Function

        <Extension>
        Public Function IsURL(text As String) As Boolean
            Return text.StartsWith("http://") OrElse text.StartsWith("https://") OrElse text.StartsWith("ftp://")
        End Function

        <Extension>
        Public Function ToCoverImage(text As String, Optional FullText As Boolean = False) As DrawingImage
            Dim dv As New DrawingVisual()
            Dim dc = dv.RenderOpen
            Dim recBrush As New LinearGradientBrush(New GradientStopCollection From {New GradientStop(Color.FromRgb(38, 50, 56), 0), New GradientStop(Color.FromRgb(69, 90, 100), 1)}, 45)
#Disable Warning
            Dim txt = New FormattedText(If(FullText, text, text.FirstOrDefault & text.LastOrDefault), Globalization.CultureInfo.CurrentCulture, FlowDirection.LeftToRight, New Typeface(New FontFamily("Arial"), FontStyles.Normal, FontWeights.DemiBold, FontStretches.Normal), 132 / 2, New SolidColorBrush(Color.FromRgb(255, 193, 7))) '2 for text length                        
#Enable Warning
            dc.DrawRoundedRectangle(recBrush, Nothing, New Rect(0, 0, If(FullText, txt.Width + 50, 100), 100), 15, 15)
            dc.DrawText(txt, New Point((If(FullText, txt.Width + 50, 100) - txt.Width) / 2, (100 - txt.Height) / 2))
            dc.Close()
            Return New DrawingImage(dv.Drawing)
        End Function

        Public Function GenerateCoverImage(geometry As Geometry) As DrawingImage
            Dim dv As New DrawingVisual()
            Dim dc = dv.RenderOpen
            Dim recBrush As New LinearGradientBrush(New GradientStopCollection From {New GradientStop(Color.FromRgb(38, 50, 56), 0), New GradientStop(Color.FromRgb(69, 90, 100), 1)}, 45)
            dc.DrawRoundedRectangle(recBrush, Nothing, New Rect(0, 0, 100, 100), 15, 15)
            'dc.DrawGeometry(New SolidColorBrush(Color.FromRgb(255, 193, 7)), Nothing, geometry)
            dc.DrawImage(geometry.ToImageSource(New SolidColorBrush(Color.FromRgb(255, 193, 7)), Nothing), New Rect(10, 10, 80, 80))
            dc.Close()
            Return New DrawingImage(dv.Drawing)
        End Function

        Public Function GetFiles(Path As String, Optional Filters As String = "*.mp3|*.m4a|*.mp4|*.wav|*.aiff|*.mp2|*.mp1|*.ogg|*.wma|*.flac|*.alac|*.webm|*.midi|*.mid") As IEnumerable(Of String)
            If Not IO.Directory.Exists(Path) Then Return Nothing
            Return Filters.Split("|"c).SelectMany(Function(filter) System.IO.Directory.GetFiles(Path, filter, IO.SearchOption.AllDirectories))
        End Function

        Public Function GetFiles(Paths As IEnumerable(Of String), Optional Filters As String = "*.mp3|*.m4a|*.mp4|*.wav|*.aiff|*.mp2|*.mp1|*.ogg|*.wma|*.flac|*.alac|*.webm|*.midi|*.mid") As IEnumerable(Of String)
            Return Paths.SelectMany(Function(Path) Filters.Split("|"c).SelectMany(Function(filter) If(IO.Directory.Exists(Path), System.IO.Directory.GetFiles(Path, filter, IO.SearchOption.AllDirectories), New String() {})))
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

        <Extension>
        Public Function ToBitmapSource(StreamSource As IO.Stream) As BitmapSource
            Dim bi As New BitmapImage
            bi.BeginInit()
            bi.CacheOption = BitmapCacheOption.OnDemand
            If OverrideProperties.Instance.Locked(OverrideProperties.LockType.CommonFunctions_ToBitmapSource_DecodePixelWidth) Then bi.DecodePixelWidth = OverrideProperties.Instance.Value(OverrideProperties.LockType.CommonFunctions_ToBitmapSource_DecodePixelWidth)
            If OverrideProperties.Instance.Locked(OverrideProperties.LockType.CommonFunctions_ToBitmapSource_DecodePixelHeight) Then bi.DecodePixelHeight = OverrideProperties.Instance.Value(OverrideProperties.LockType.CommonFunctions_ToBitmapSource_DecodePixelHeight)
            bi.StreamSource = StreamSource
            bi.EndInit()
            Return bi
        End Function
        <Extension>
        Public Function ToBitmapSource(StreamSource As IO.Stream, PxWidth As Integer, PxHeight As Integer) As BitmapSource
            Dim bi As New BitmapImage
            bi.BeginInit()
            bi.CacheOption = BitmapCacheOption.OnDemand
            bi.DecodePixelWidth = If(OverrideProperties.Instance.Locked(OverrideProperties.LockType.CommonFunctions_ToBitmapSource_DecodePixelWidth), OverrideProperties.Instance.Value(OverrideProperties.LockType.CommonFunctions_ToBitmapSource_DecodePixelWidth), PxWidth)
            bi.DecodePixelHeight = If(OverrideProperties.Instance.Locked(OverrideProperties.LockType.CommonFunctions_ToBitmapSource_DecodePixelHeight), OverrideProperties.Instance.Value(OverrideProperties.LockType.CommonFunctions_ToBitmapSource_DecodePixelHeight), PxHeight)
            bi.StreamSource = StreamSource
            bi.EndInit()
            Return bi
        End Function
        <Extension>
        Public Function ToBitmapSource(URISource As Uri) As BitmapSource
            Dim bi As New BitmapImage
            bi.BeginInit()
            bi.CacheOption = BitmapCacheOption.OnDemand
            If OverrideProperties.Instance.Locked(OverrideProperties.LockType.CommonFunctions_ToBitmapSource_DecodePixelWidth) Then bi.DecodePixelWidth = OverrideProperties.Instance.Value(OverrideProperties.LockType.CommonFunctions_ToBitmapSource_DecodePixelWidth)
            If OverrideProperties.Instance.Locked(OverrideProperties.LockType.CommonFunctions_ToBitmapSource_DecodePixelHeight) Then bi.DecodePixelHeight = OverrideProperties.Instance.Value(OverrideProperties.LockType.CommonFunctions_ToBitmapSource_DecodePixelHeight)
            bi.UriSource = URISource
            bi.EndInit()
            Return bi
        End Function
        <Extension>
        Public Function ToBitmapSource(URISource As Uri, PxWidth As Integer, PxHeight As Integer) As BitmapSource
            Dim bi As New BitmapImage
            bi.BeginInit()
            bi.CacheOption = BitmapCacheOption.OnDemand
            bi.DecodePixelWidth = If(OverrideProperties.Instance.Locked(OverrideProperties.LockType.CommonFunctions_ToBitmapSource_DecodePixelWidth), OverrideProperties.Instance.Value(OverrideProperties.LockType.CommonFunctions_ToBitmapSource_DecodePixelWidth), PxWidth)
            bi.DecodePixelHeight = If(OverrideProperties.Instance.Locked(OverrideProperties.LockType.CommonFunctions_ToBitmapSource_DecodePixelHeight), OverrideProperties.Instance.Value(OverrideProperties.LockType.CommonFunctions_ToBitmapSource_DecodePixelHeight), PxHeight)
            bi.UriSource = URISource
            bi.EndInit()
            Return bi
        End Function

        <Extension>
        Public Function Save(Image As BitmapSource) As IO.MemoryStream
            Dim mem As New IO.MemoryStream
            Dim pngEnco As New PngBitmapEncoder()
            pngEnco.Frames.Add(BitmapFrame.Create(Image))
            pngEnco.Save(mem)
            Return mem
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
                Utilities.DebugMode.Instance.Log(Of Application)(ex.ToString)
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
            If bmp Is Nothing Then Return Nothing
            If OverrideProperties.Instance.Locked(OverrideProperties.LockType.CommonFunctions_ToImageSource_ChangeResolution) Then
                ChangeRes = True
                ResX = OverrideProperties.Instance.Value(OverrideProperties.LockType.CommonFunctions_ToImageSource_DecodePixelWidth)
                ResY = OverrideProperties.Instance.Value(OverrideProperties.LockType.CommonFunctions_ToImageSource_DecodePixelHeight)
            End If
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

        ''' <summary>
        ''' Check matching percentage between two strings
        ''' </summary>
        ''' <param name="arg0">Base string</param>
        ''' <param name="arg1">Compate string</param>
        ''' <returns>a single (0->1) indicating how much similarity between the two arguments</returns>
        <Extension>
        Public Function GetSimilarity(arg0 As String, arg1 As String) As Single
            If String.IsNullOrEmpty(arg0) OrElse String.IsNullOrEmpty(arg1) Then Return 0
            Dim string1 = arg0.ToLower : Dim string2 = arg1.ToLower
            Dim dis As Single = ComputeDistance(string1, string2)
            Dim maxLen As Single = string1.Length
            If maxLen < string2.Length Then
                maxLen = string2.Length
            End If
            If maxLen = 0.0F Then
                Return 1.0F
            Else
                Return 1.0F - dis / maxLen
            End If
        End Function

        ''' <summary>
        ''' Compares every part of <paramref name="sArg0"/> and <paramref name="sArg1"/> split by a space and sums the result
        ''' </summary>
        ''' <param name="sArg0"></param>
        ''' <param name="sArg1"></param>
        ''' <returns></returns>
        <Extension>
        Public Function GetSplitSimilarity(sArg0 As String, sArg1 As String) As Single
            If String.IsNullOrEmpty(sArg0) OrElse String.IsNullOrEmpty(sArg1) Then Return 0
            If sArg0.Split(New Char() {" "}, StringSplitOptions.RemoveEmptyEntries).Length = 1 AndAlso sArg1.Split(New Char() {" "}, StringSplitOptions.RemoveEmptyEntries).Length = 1 Then
                Return GetSimilarity(sArg0, sArg1)
            End If
            Dim r As Single = 0
            For Each string1 In sArg0.Split(New Char() {" "}, StringSplitOptions.RemoveEmptyEntries)
                Dim string2 = sArg1.ToLower
                Dim dis As Single = ComputeDistance(string1, string2)
                Dim maxLen As Single = string1.Length
                If maxLen < string2.Length Then
                    maxLen = string2.Length
                End If
                If maxLen = 0.0F Then
                    r += 1.0F
                Else
                    r += 1.0F - dis / maxLen
                End If
            Next
            For Each string2 In sArg1.Split(New Char() {" "}, StringSplitOptions.RemoveEmptyEntries)
                Dim string1 = sArg0.ToLower
                Dim dis As Single = ComputeDistance(string1, string2)
                Dim maxLen As Single = string1.Length
                If maxLen < string2.Length Then
                    maxLen = string2.Length
                End If
                If maxLen = 0.0F Then
                    r += 1.0F
                Else
                    r += 1.0F - dis / maxLen
                End If
            Next
            Return r
        End Function

        ''' <summary>
        ''' Limits the <see cref="String"/> to a specific byte count in UTF-8
        ''' </summary>
        ''' <param name="str">Item</param>
        ''' <param name="Limit">Bytes Count Limit</param>
        ''' <param name="AddTrailingDots">Wheter or not to add 3 trailing dots to a limited string. e.g: "Hello Wo..."</param>
        ''' <returns></returns>
        <Extension>
        Public Function LimitBytes(str As String, Limit As Integer, Optional AddTrailingDots As Boolean = False) As String
            If str Is Nothing Then Return str
            Dim sBytes = Text.Encoding.UTF8.GetBytes(str)
            If sBytes.Length > Limit Then
                If AddTrailingDots Then
                    Dim dot As Byte = 46
                    sBytes(Limit - 4) = dot
                    sBytes(Limit - 3) = dot
                    sBytes(Limit - 2) = dot
                End If
                Array.Resize(sBytes, Limit - 1)
                Return Text.Encoding.UTF8.GetString(sBytes)
            Else
                Return str
            End If
        End Function

        ''' <summary>
        ''' Limits the <see cref="String"/> to a specific byte count in the specified encoding
        ''' </summary>
        ''' <param name="str">Item</param>
        ''' <param name="Limit">Bytes Count Limit</param>
        ''' <param name="AddTrailingDots">Wheter or not to add 3 trailing dots to a limited string. e.g: "Hello Wo..."</param>
        ''' <returns></returns>
        <Extension>
        Public Function LimitBytes(str As String, Limit As Integer, Encoding As Text.Encoding, Optional AddTrailingDots As Boolean = False) As String
            Dim sBytes = Encoding.GetBytes(str)
            If sBytes.Length > Limit Then
                If AddTrailingDots Then
                    Dim dot As Byte = 46
                    sBytes(Limit - 4) = dot
                    sBytes(Limit - 3) = dot
                    sBytes(Limit - 2) = dot
                End If
                Array.Resize(sBytes, Limit - 1)
                Return Encoding.GetString(sBytes)
            Else
                Return str
            End If
        End Function

        Private Function ComputeDistance(s As String, t As String) As Integer
            Dim n As Integer = s.Length
            Dim m As Integer = t.Length
            Dim distance As Integer(,) = New Integer(n, m) {}
            ' matrix
            Dim cost As Integer
            If n = 0 Then
                Return m
            End If
            If m = 0 Then
                Return n
            End If
            'init1

            Dim i As Integer = 0
            While i <= n
                distance(i, 0) = System.Math.Min(System.Threading.Interlocked.Increment(i), i - 1)
            End While
            Dim j As Integer = 0
            While j <= m
                distance(0, j) = System.Math.Min(System.Threading.Interlocked.Increment(j), j - 1)
            End While
            'find min distance

            For i = 1 To n
                For j = 1 To m
                    cost = (If(t.Substring(j - 1, 1) = s.Substring(i - 1, 1), 0, 1))
                    distance(i, j) = Math.Min(distance(i - 1, j) + 1, Math.Min(distance(i, j - 1) + 1, distance(i - 1, j - 1) + cost))
                Next
            Next
            Return distance(n, m)
        End Function

        Public Function RemoteFileExists(ByVal url As String) As Boolean
            Try
                Dim request As System.Net.HttpWebRequest = TryCast(System.Net.WebRequest.Create(url), System.Net.HttpWebRequest)
                request.Method = "HEAD"
                Dim response As System.Net.HttpWebResponse = TryCast(request.GetResponse(), System.Net.HttpWebResponse)
                response.Close()
                Return (response.StatusCode = System.Net.HttpStatusCode.OK)
            Catch
                Return False
            End Try
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