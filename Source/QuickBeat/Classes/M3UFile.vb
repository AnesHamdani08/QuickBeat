Namespace Utilities
    Public Class M3UFile
        Inherits ObjectModel.ObservableCollection(Of Player.Metadata)

        Private Sub _OnPropertyChanged(<Runtime.CompilerServices.CallerMemberName> Optional CallerName As String = Nothing)
            OnPropertyChanged(New ComponentModel.PropertyChangedEventArgs(CallerName))
        End Sub

        Private _Name As String
        Property Name As String
            Get
                Return _Name
            End Get
            Set(value As String)
                _Name = value
                _OnPropertyChanged()
            End Set
        End Property

        Private _Path As String
        Property Path As String
            Get
                Return _Path
            End Get
            Set(value As String)
                _Path = value
                _OnPropertyChanged()
            End Set
        End Property

        Sub New(Path As String)
            Me.Path = Path
        End Sub

        ''' <summary>
        ''' Loads the M3U file content from <see cref="Path"/>
        ''' </summary>
        ''' <remarks>
        ''' Supports extended M3U: #PLAYLIST, #EXTINF with format Duration, Artist - Title
        ''' </remarks>
        Public Sub Load()
            If Not IO.File.Exists(Path) Then Return
            Using fs As New IO.FileStream(Path, IO.FileMode.Open, IO.FileAccess.Read)
                Using sr As New IO.StreamReader(fs)
                    Dim cDir = Environment.CurrentDirectory
                    Environment.CurrentDirectory = IO.Path.GetDirectoryName(Path)
                    Dim lastinfo As String = ""
                    Do While Not sr.EndOfStream
                        Dim line = sr.ReadLine
                        If line.StartsWith("#EXTM3U") Then Continue Do
                        If line.StartsWith("#PLAYLIST:") Then Name = line.Substring("#PLAYLIST:".Length)
                        If line.StartsWith("#EXTINF:") Then lastinfo = line.Substring("#EXTINF:".Length) : Continue Do
                        If line.StartsWith("#") Then Continue Do
                        Dim filepath = IO.Path.GetFullPath(line)
                        If Not IO.File.Exists(filepath) Then Continue Do
                        Dim meta = Player.Metadata.FromFile(filepath, True)
                        If Not String.IsNullOrEmpty(lastinfo) Then
                            Dim splitinfo = lastinfo.Split(",").LastOrDefault.Split("-")
                            If splitinfo.Length >= 2 Then
                                If String.IsNullOrEmpty(meta.Title) Then meta.Title = splitinfo.LastOrDefault
                                If String.IsNullOrEmpty(meta.DefaultArtist) Then meta.Artists = New String() {splitinfo.FirstOrDefault}
                            End If
                            lastinfo = ""
                        End If
                        Add(meta)
                    Loop
                    Environment.CurrentDirectory = cDir
                    sr.Close()
                End Using
            End Using
        End Sub

        ''' <summary>
        ''' Saves the content of the current instance to a file
        ''' </summary>
        ''' <param name="path">The full path including file name and extension</param>
        ''' <remarks>
        ''' Uses default encoding
        ''' </remarks>
        Public Sub Save(path As String, Optional ExtendedM3U As Boolean = True)
            Using fs As New IO.FileStream(path, IO.FileMode.Create, IO.FileAccess.Write)
                Using sw As New IO.StreamWriter(fs)
                    If ExtendedM3U Then sw.WriteLine("#EXTM3U")
                    If ExtendedM3U AndAlso Not String.IsNullOrEmpty(Name) Then sw.WriteLine("#PLAYLIST:" & Name)
                    For Each meta In Me
                        If ExtendedM3U Then sw.WriteLine($"#EXTINF:{Math.Round(meta.Length)},{meta.DefaultArtist} - {meta.Title}")
                        sw.WriteLine(meta.OriginalPath)
                    Next
                    sw.Flush()
                End Using
            End Using
        End Sub
        ''' <summary>
        ''' Saves the content of the current instance to a file using a specific encoding
        ''' </summary>
        ''' <param name="path">The full path including file name and extension</param>        
        ''' <param name="encoding">The character encoding to use</param>
        Public Sub Save(path As String, encoding As Text.Encoding, Optional ExtendedM3U As Boolean = True)
            Using fs As New IO.FileStream(path, IO.FileMode.Create, IO.FileAccess.Write)
                Using sw As New IO.StreamWriter(fs, encoding)
                    If ExtendedM3U Then sw.WriteLine("#EXTM3U")
                    If ExtendedM3U AndAlso Not String.IsNullOrEmpty(Name) Then sw.WriteLine("#PLAYLIST:" & Name)
                    For Each meta In Me
                        If ExtendedM3U Then sw.WriteLine($"#EXTINF:{Math.Round(meta.Length)},{meta.DefaultArtist} - {meta.Title}")
                        sw.WriteLine(meta.OriginalPath)
                    Next
                    sw.Flush()
                End Using
                fs.Flush()
                End Using
        End Sub
    End Class
End Namespace