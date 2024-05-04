Imports System.Xml

Namespace UPnP
    Namespace ContentDirectory
        Public Class Resource
            Public size As ULong
            Public duration As String
            Public bitrate As UInteger
            Public sampleFrequency As UInteger
            Public bitsPerSample As UInteger
            Public nrAudioChannels As UInteger
            Public protocolInfo As String
            Public protection As String
            Public importUri As String
            Public URI As Uri

            Public Sub New(ByVal xmlNode As XElement)
                'Read the 'DIDL-Lite' attributes of the Object
                For Each tmpAttr As XAttribute In xmlNode.Attributes
                    Select Case tmpAttr.Name.LocalName.ToLower
                        Case "duration"
                            Me.duration = CStr(tmpAttr.Value)
                        Case "protocolinfo"
                            Me.protocolInfo = CStr(tmpAttr.Value)
                        Case "size"
                            Me.size = CULng(tmpAttr.Value)
                        Case "bitrate"
                            Me.bitrate = CUInt(tmpAttr.Value)
                        Case "samplefrequency"
                            Me.sampleFrequency = CUInt(tmpAttr.Value)
                        Case "bitspersample"
                            Me.bitsPerSample = CUInt(tmpAttr.Value)
                        Case "nrAudioChannels"
                            Me.nrAudioChannels = CUInt(tmpAttr.Value)
                        Case "protection"
                            Me.protection = CStr(tmpAttr.Value)
                        Case "importUri"
                            Me.importUri = CStr(tmpAttr.Value)
                    End Select
                Next

                Me.URI = New Uri(Xml.XmlConvert.DecodeName(System.Web.HttpUtility.UrlDecode(xmlNode.Value))) 'the url which we wil send to the media renderer to play it                
            End Sub

            Public Overrides Function ToString() As String
                Return Me.URI.ToString
            End Function
        End Class

        Public Class CD_Object
            Public Enum CD_Object_WriteStatus
                WS_Writable
                WS_Protected
                WS_NotWritable
                WS_Unknown
                WS_Mixed
            End Enum

            Dim _ID As String 'Required=yes, MultipleValues=no
            Dim _ParentID As String 'Required=yes, MultipleValues=no
            Dim _Title As String 'Required=yes, MultipleValues=no
            Dim _Creator As String = Nothing 'Required=no, MultipleValues=no
            Dim _Resource() As Resource 'Required=no, MultipleValues=yes
            Dim _Class As String 'Required=yes, MultipleValues=no
            Dim _Restricted As Boolean 'Required=yes, MultipleValues=no
            Dim _WriteStatus As CD_Object_WriteStatus = Nothing 'Required=no, MultipleValues=no
            Dim _XMLDump As String 'Didl info to send to the media renderer when playing this item

            Public Sub New(ByVal xmlNode As XElement)
                Me._XMLDump = xmlNode.ToString
                'Read the 'DIDL-Lite' attributes of the Object
                For Each tmpAttr As XAttribute In xmlNode.Attributes
                    Select Case tmpAttr.Name.LocalName.ToLower
                        Case "id"
                            Me._ID = CStr(tmpAttr.Value)
                        Case "parentid"
                            Me._ParentID = CStr(tmpAttr.Value)
                        Case "restricted"
                            Me._Restricted = CBool(tmpAttr.Value)
                    End Select
                Next

                'Read the 'Dublin Core' & 'UPnP' attributes of the Object
                For Each childNode As XElement In xmlNode.Elements
                    Select Case childNode.Name.LocalName.ToLower
                        Case "title"
                            Me._Title = CStr(childNode.Value)
                        Case "creator"
                            Me._Creator = CStr(childNode.Value)
                        Case "class"
                            Me._Class = CStr(childNode.Value)
                        Case "res"
                            If Me._Resource Is Nothing Then
                                ReDim Preserve _Resource(0)
                                _Resource(0) = New Resource(childNode)
                            Else
                                ReDim Preserve _Resource(_Resource.Length)
                                _Resource(_Resource.Length - 1) = New Resource(childNode)
                            End If
                        Case "writestatus"
                            Select Case CStr(childNode.Value).ToUpper
                                Case "WRITABLE"
                                    Me._WriteStatus = CD_Object_WriteStatus.WS_Writable
                                Case "PROTECTED"
                                    Me._WriteStatus = CD_Object_WriteStatus.WS_Protected
                                Case "NOT_WRITABLE"
                                    Me._WriteStatus = CD_Object_WriteStatus.WS_Writable
                                Case "UNKNOWN"
                                    Me._WriteStatus = CD_Object_WriteStatus.WS_Unknown
                                Case "MIXED"
                                    Me._WriteStatus = CD_Object_WriteStatus.WS_Mixed
                            End Select
                    End Select
                Next
            End Sub

            Public Shared Function Create_CD_Object(ByVal xmlNode As XElement)
                Dim newCDObject As CD_Object = Nothing
                Select Case xmlNode.Name.LocalName.ToLower
                    Case "item"
                        newCDObject = New CD_Item(xmlNode)
                    Case "container"
                        newCDObject = New CD_Container(xmlNode)
                End Select
                Return newCDObject
            End Function

            Public ReadOnly Property ID() As String
                Get
                    Return Me._ID
                End Get
            End Property

            Public ReadOnly Property ParentID() As String
                Get
                    Return Me._ParentID
                End Get
            End Property

            Public ReadOnly Property Title() As String
                Get
                    Return Me._Title
                End Get
            End Property

            Public ReadOnly Property Creator() As String
                Get
                    Return Me._Creator
                End Get
            End Property

            Public ReadOnly Property Resource() As Resource()
                Get
                    Return Me._Resource
                End Get
            End Property

            Public ReadOnly Property ClassName() As String
                Get
                    Return Me._Class
                End Get
            End Property

            Public ReadOnly Property Restricted() As Boolean
                Get
                    Return Me._Restricted
                End Get
            End Property

            Public ReadOnly Property WriteStatus() As CD_Object_WriteStatus
                Get
                    Return Me._WriteStatus
                End Get
            End Property

            Public ReadOnly Property XMLDump() As String
                Get
                    Return Me._XMLDump
                End Get
            End Property

            Public Overrides Function ToString() As String
                Return $"{ClassName}: {Title}"
            End Function
        End Class

        Public Class CD_Item
            Inherits CD_Object

            Dim _RefID As String = Nothing 'Required=no, MultipleValues=no
            Dim _Artist As String = ""
            Dim _Album As String = ""
            Dim _Genre As String = ""

            Public Sub New(ByVal xmlNode As XElement)
                MyBase.New(xmlNode)

                'Read the 'DIDL-Lite' attributes of the Object
                For Each tmpAttr As XAttribute In xmlNode.Attributes
                    Select Case tmpAttr.Name.LocalName.ToLower
                        Case "refid"
                            Me._RefID = CStr(tmpAttr.Value)
                    End Select
                Next

                'Read the 'Dublin Core' & 'UPnP' attributes of the Object
                For Each childNode As XElement In xmlNode.Elements
                    Select Case childNode.Name.LocalName.ToLower
                        Case "artist"
                            Me._Artist = CStr(childNode.Value)
                        Case "album"
                            Me._Album = CStr(childNode.Value)
                        Case "genre"
                            Me._Genre = CStr(childNode.Value)
                    End Select
                Next
            End Sub

            Public ReadOnly Property RefID() As String
                Get
                    Return Me._RefID
                End Get
            End Property

            Public ReadOnly Property Artist() As String
                Get
                    Return Me._Artist
                End Get
            End Property

            Public ReadOnly Property Album() As String
                Get
                    Return Me._Album
                End Get
            End Property

            Public ReadOnly Property Genre() As String
                Get
                    Return Me._Genre
                End Get
            End Property
        End Class

        Public Class CD_Container
            Inherits CD_Object

            Dim _ChildCount As Integer = Nothing 'Required=no, MultipleValues=no
            'Dim _CreateClass() As String = Nothing 'Required=no, MultipleValues=yes
            'Dim _SearchClass() As String = Nothing 'Required=no, MultipleValues=yes
            Dim _Searchable As Boolean = False 'Required=no, MultipleValues=no

            Public Sub New(ByVal xmlNode As XElement)
                MyBase.New(xmlNode)

                'Read the 'DIDL-Lite' attributes of the Object
                For Each tmpAttr As XAttribute In xmlNode.Attributes
                    Select Case tmpAttr.Name.LocalName.ToLower
                        Case "searchable"
                            Me._Searchable = CBool(tmpAttr.Value)
                        Case "childcount"
                            Me._ChildCount = CInt(tmpAttr.Value)
                    End Select
                Next
            End Sub

            Public ReadOnly Property ChildCount() As Integer
                Get
                    Return Me._ChildCount
                End Get
            End Property

            Public ReadOnly Property Searchable() As Boolean
                Get
                    Return Me._Searchable
                End Get
            End Property
        End Class
    End Namespace
End Namespace
