Namespace Utilities
    Public Class SettingsHelper
        Implements IDictionary(Of String, String)


        Private _Items As New Dictionary(Of String, String)
        Private _Sections As New List(Of KeyValuePair(Of String, List(Of KeyValuePair(Of String, String))))
        ReadOnly Property RawSections As List(Of KeyValuePair(Of String, List(Of KeyValuePair(Of String, String))))
            Get
                Return _Sections
            End Get
        End Property

        ReadOnly Property Metadata As New Dictionary(Of String, String)

        ReadOnly Property SectionsCount As Integer
            Get
                Return _Sections.Count
            End Get
        End Property

        ReadOnly Property Sections As IEnumerable(Of String)
            Get
                Return _Sections.Select(Of String)(Function(k) k.Key)
            End Get
        End Property

        ReadOnly Property ItemsCount As Integer
            Get
                Return _Items.Count
            End Get
        End Property

        ReadOnly Property IsInSection As Boolean
            Get
                Return Not (String.IsNullOrEmpty(_CurrentSectionKey) AndAlso (_CurrentSection Is Nothing))
            End Get
        End Property

        Default Public Property Item(key As String) As String Implements IDictionary(Of String, String).Item
            Get
                Return DirectCast(_Items, IDictionary(Of String, String))(key)
            End Get
            Set(value As String)
                DirectCast(_Items, IDictionary(Of String, String))(key) = value
            End Set
        End Property

        Public ReadOnly Property Keys As ICollection(Of String) Implements IDictionary(Of String, String).Keys
            Get
                Return DirectCast(_Items, IDictionary(Of String, String)).Keys
            End Get
        End Property

        Public ReadOnly Property Values As ICollection(Of String) Implements IDictionary(Of String, String).Values
            Get
                Return DirectCast(_Items, IDictionary(Of String, String)).Values
            End Get
        End Property

        Public ReadOnly Property Count As Integer Implements ICollection(Of KeyValuePair(Of String, String)).Count
            Get
                Return DirectCast(_Items, ICollection(Of KeyValuePair(Of String, String))).Count
            End Get
        End Property

        Public ReadOnly Property IsReadOnly As Boolean Implements ICollection(Of KeyValuePair(Of String, String)).IsReadOnly
            Get
                Return DirectCast(_Items, ICollection(Of KeyValuePair(Of String, String))).IsReadOnly
            End Get
        End Property

        Private _CurrentSectionKey As String
        Private _CurrentSection As List(Of KeyValuePair(Of String, String))

        Sub AddItem(key As String, value As String)
            If Not String.IsNullOrEmpty(_CurrentSectionKey) AndAlso _CurrentSection IsNot Nothing Then
                _CurrentSection.Add(New KeyValuePair(Of String, String)(key, value))
            Else
                If Not _Items.ContainsKey(key) Then _Items.Add(key, value) 'TODO pay this a visit
            End If
        End Sub

        Function RemoveItem(key As String, Optional section As String = Nothing) As Boolean
            If String.IsNullOrEmpty(section) Then
                Return _Items.Remove(key)
            Else
                Return _Sections.Remove(_Sections.FirstOrDefault(Function(k) k.Key = section))
            End If
        End Function

        Sub StartSection(key As String)
            If String.IsNullOrEmpty(key) Then Return
            If key = "Items" Then Throw New ArgumentOutOfRangeException("Please use another section key. ""Items"" is reserved for internal use.")
            If Not String.IsNullOrEmpty(_CurrentSectionKey) AndAlso _CurrentSection IsNot Nothing Then
                EndSection()
            End If
            _CurrentSectionKey = key
            _CurrentSection = New List(Of KeyValuePair(Of String, String))
        End Sub

        Private Sub _StartSection(key As String)
            If Not String.IsNullOrEmpty(_CurrentSectionKey) AndAlso _CurrentSection IsNot Nothing Then
                EndSection()
            End If
            _CurrentSectionKey = key
            _CurrentSection = New List(Of KeyValuePair(Of String, String))
        End Sub

        Sub EndSection()
            If String.IsNullOrEmpty(_CurrentSectionKey) OrElse _CurrentSection Is Nothing Then Return
            If _CurrentSectionKey = "Items" Then
                For Each _item In _CurrentSection
                    _Items.Add(_item.Key, _item.Value)
                Next
            ElseIf _CurrentSectionKey = "Metadata" Then
                For Each _item In _CurrentSection
                    Metadata.Add(_item.Key, _item.Value)
                Next
            Else
                _Sections.Add(New KeyValuePair(Of String, List(Of KeyValuePair(Of String, String)))(_CurrentSectionKey, _CurrentSection))
            End If
        End Sub

        Function RemoveSection(key As String) As Boolean
            Return _Sections.Remove(_Sections.FirstOrDefault(Function(k) k.Key = key))
        End Function

        Sub Load(dump As String)
            Dim Sdump = dump.Split(New Char() {vbCr, vbLf, vbCrLf}, StringSplitOptions.RemoveEmptyEntries)
            For Each line In Sdump
                If line.StartsWith("[") AndAlso line.EndsWith("]") Then 'Section                    
                    _StartSection(line.Replace("[", "").Replace("]", ""))
                Else
                    Dim Sline = line.Split(New Char() {"="}, StringSplitOptions.None)
                    If Sline.Length < 2 Then Throw New ArgumentException("Expected a key and a value on the current line: " & line)
                    'AddItem(Sline.Take(1).FirstOrDefault, String.Join("", Sline.Skip(1).Select(Of String)(Function(k)
                    '                                                                                          If k.Length = 0 Then
                    '                                                                                              k &= "="
                    '                                                                                          End If
                    '                                                                                          Return k
                    '                                                                                      End Function)))
                    AddItem(Sline.Take(1).FirstOrDefault, String.Join("=", Sline.Skip(1)))
                End If
            Next
            EndSection()
        End Sub

        Function Dump() As String
            Dim SB As New Text.StringBuilder
            SB.AppendLine("[Metadata]")
            For Each mdata In Metadata
                SB.AppendLine($"{mdata.Key}={mdata.Value}")
            Next
            SB.AppendLine("[Items]")
            For Each _Item In _Items
                SB.AppendLine($"{_Item.Key}={_Item.Value}")
            Next
            For Each section In _Sections
                SB.AppendLine($"[{section.Key}]")
                For Each _Item In section.Value
                    SB.AppendLine($"{_Item.Key}={_Item.Value}")
                Next
            Next
            Return SB.ToString
        End Function

        Function ContainsSection(section As String) As Boolean
            Return _Sections.Any(Function(k) k.Key = section)
        End Function

        Function GetItem(key As String) As String
            Return _Items.FirstOrDefault(Function(k) k.Key = key).Value
        End Function

        ''' <summary>
        ''' Gets the first occurence of a specific section
        ''' </summary>
        ''' <param name="key">Section key</param>
        ''' <returns></returns>
        Function GetSection(key As String) As List(Of KeyValuePair(Of String, String))
            Return _Sections.FirstOrDefault(Function(k) k.Key = key).Value
        End Function

        Function GetSectionItem(section As String, key As String) As String
            Return _Sections.FirstOrDefault(Function(k) k.Key = key).Value.FirstOrDefault(Function(l) l.Key = key).Value
        End Function

        Public Function ContainsKey(key As String) As Boolean Implements IDictionary(Of String, String).ContainsKey
            Return DirectCast(_Items, IDictionary(Of String, String)).ContainsKey(key)
        End Function

        Public Sub Add(key As String, value As String) Implements IDictionary(Of String, String).Add
            DirectCast(_Items, IDictionary(Of String, String)).Add(key, value)
        End Sub

        Public Function Remove(key As String) As Boolean Implements IDictionary(Of String, String).Remove
            Return DirectCast(_Items, IDictionary(Of String, String)).Remove(key)
        End Function

        Public Function TryGetValue(key As String, ByRef value As String) As Boolean Implements IDictionary(Of String, String).TryGetValue
            Return DirectCast(_Items, IDictionary(Of String, String)).TryGetValue(key, value)
        End Function

        Public Sub Add(item As KeyValuePair(Of String, String)) Implements ICollection(Of KeyValuePair(Of String, String)).Add
            DirectCast(_Items, ICollection(Of KeyValuePair(Of String, String))).Add(item)
        End Sub

        Public Sub Clear() Implements ICollection(Of KeyValuePair(Of String, String)).Clear
            DirectCast(_Items, ICollection(Of KeyValuePair(Of String, String))).Clear()
        End Sub

        Public Function Contains(item As KeyValuePair(Of String, String)) As Boolean Implements ICollection(Of KeyValuePair(Of String, String)).Contains
            Return DirectCast(_Items, ICollection(Of KeyValuePair(Of String, String))).Contains(item)
        End Function

        Public Sub CopyTo(array() As KeyValuePair(Of String, String), arrayIndex As Integer) Implements ICollection(Of KeyValuePair(Of String, String)).CopyTo
            DirectCast(_Items, ICollection(Of KeyValuePair(Of String, String))).CopyTo(array, arrayIndex)
        End Sub

        Public Function Remove(item As KeyValuePair(Of String, String)) As Boolean Implements ICollection(Of KeyValuePair(Of String, String)).Remove
            Return DirectCast(_Items, ICollection(Of KeyValuePair(Of String, String))).Remove(item)
        End Function

        Public Function GetEnumerator() As IEnumerator(Of KeyValuePair(Of String, String)) Implements IEnumerable(Of KeyValuePair(Of String, String)).GetEnumerator
            Return DirectCast(_Items, IEnumerable(Of KeyValuePair(Of String, String))).GetEnumerator()
        End Function

        Private Function IEnumerable_GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
            Return DirectCast(_Items, IEnumerable).GetEnumerator()
        End Function
    End Class
End Namespace
