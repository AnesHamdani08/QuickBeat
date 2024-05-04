Imports System.Collections.Generic

Namespace SubtitlesParser.Classes

    Public Class SubtitlesFormat

        ' Properties -----------------------------------------

        Public Property Name As String
        Public Property Extension As String


        ' Private constructor to avoid duplicates ------------

        Private Sub New()

        End Sub


        ' Predefined instances -------------------------------

        Public Shared SubRipFormat As New SubtitlesFormat With
        {
            .Name = "SubRip",
            .Extension = "\.srt"
        }
        Public Shared MicroDvdFormat As New SubtitlesFormat With
        {
            .Name = "MicroDvd",
            .Extension = "\.sub"
        }
        Public Shared SubViewerFormat As New SubtitlesFormat With
        {
            .Name = "SubViewer",
            .Extension = "\.sub"
        }
        Public Shared SubStationAlphaFormat As New SubtitlesFormat With
        {
            .Name = "SubStationAlpha",
            .Extension = "\.ssa"
        }
        Public Shared TtmlFormat As New SubtitlesFormat With
        {
            .Name = "TTML",
            .Extension = "\.ttml"
        }
        Public Shared WebVttFormat As New SubtitlesFormat With
        {
            .Name = "WebVTT",
            .Extension = "\.vtt"
        }
        Public Shared YoutubeXmlFormat As New SubtitlesFormat With
        {
            .Name = "YoutubeXml",
            .Extension = "\.xml"
            }
        Public Shared LRCFormat As New SubtitlesFormat With
        {
            .Name = "LyRiCs",
            .Extension = "\.lrc"
            }

        Public Shared SupportedSubtitlesFormats As New List(Of SubtitlesFormat)(
            {
                SubRipFormat,
                MicroDvdFormat,
                SubViewerFormat,
                SubStationAlphaFormat,
                TtmlFormat,
                WebVttFormat,
                YoutubeXmlFormat,
                LRCFormat
            })

        Public Shared SupportedSubtitlesFormatsFileFilter As String = "Supported Formats|*.srt;*.sub;*.sbv;*.ssa;*.ass;*.ttml;*.vtt;*.xml;*.lrc|All Files|*.*"
        ''' <summary>
        ''' Semi-column separated
        ''' </summary>
        Public Shared SupportedSubtitlesFormatsSCSV As String = ".srt;.sub;.sbv;.ssa;.ass;.ttml;.vtt;.xml;.lrc"
    End Class


End Namespace