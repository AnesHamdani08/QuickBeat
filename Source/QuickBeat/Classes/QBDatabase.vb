Imports System.Collections.Specialized
Imports System.ComponentModel

Namespace Classes
    ''' <summary>
    ''' Used to save items to a local file(database)
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    Public Class QBDatabase(Of T)
        Implements ComponentModel.INotifyPropertyChanged

        ''' <summary>
        ''' Used to locate cached items in database
        ''' </summary>
        <Serializable>
        Private Class CacheItem
            Public Property UID As Guid
            Public Property ID As String
            Public Property StartIndex As UInteger
            Public Property Length As UInteger
            Public Property Location As Integer

            Public Overrides Function ToString() As String
                Return $"{ID}[{StartIndex}-{Length}]@{Location}"
            End Function
        End Class

        Public Const ITEMSPERDB As Integer = 10

        Private _Name As String
        ReadOnly Property Name As String
            Get
                Return _Name
            End Get
        End Property

        ReadOnly Property Count As Integer
            Get
                Return CachedItems.Count
            End Get
        End Property

        Private _TotalSize As ULong
        ReadOnly Property TotalSize As ULong
            Get
                Return _TotalSize
            End Get
        End Property

        Private WithEvents _Size As New ObjectModel.ObservableCollection(Of KeyValuePair(Of UInteger, Long))
        ReadOnly Property Size As ObjectModel.ObservableCollection(Of KeyValuePair(Of UInteger, Long))
            Get
                Return _Size
            End Get
        End Property

        ReadOnly Property HasItems As Boolean
            Get
                Return Count > 0
            End Get
        End Property

        Private CachedItems As New List(Of CacheItem)

        Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged

#Region "WPF Support"
        ReadOnly Property ClearCommand As New Database.WPF.DelegateClearCommand
        ReadOnly Property DeleteCluster As New Database.WPF.DelegateDeleteClusterCommand(Of T)(Me)
        ReadOnly Property ReloadCommand As New Database.WPF.DelegateReloadCommand
#End Region
#Region "Ctor"
        Public Sub New(DBName As String)
            If Not IO.File.Exists(GetDBPath(DBName, True)) Then
                Throw New InvalidOperationException("Couldn't find a database hint file")
            Else
                _Name = DBName
                Dim BinF As New System.Runtime.Serialization.Formatters.Binary.BinaryFormatter()
                Dim Metadata = TryCast(BinF.Deserialize(New IO.MemoryStream(IO.File.ReadAllBytes(GetDBPath(DBName, True)))), List(Of CacheItem))
                If Metadata IsNot Nothing Then
                    CachedItems = Metadata
                    RefreshSize()
                End If
            End If
        End Sub

        Private Sub New(DBName As String, Bypass As Boolean)
            _Name = DBName
        End Sub

        Public Shared Function Create(DBName As String) As QBDatabase(Of T)
            Dim BinF As New System.Runtime.Serialization.Formatters.Binary.BinaryFormatter()
            IO.File.OpenWrite(GetDBPath(DBName & 0)).Close()
            Using fs As New IO.FileStream(GetDBPath(DBName, True), IO.FileMode.Create, IO.FileAccess.Write)
                Dim CI As New List(Of CacheItem)
                BinF.Serialize(fs, CI)
            End Using
            Return New QBDatabase(Of T)(DBName, True)
        End Function

        Public Shared Function LoadOrCreate(DBName As String) As QBDatabase(Of T)
            If IO.File.Exists(GetDBPath(DBName & 0)) AndAlso IO.File.Exists(GetDBPath(DBName, True)) Then
                Return New QBDatabase(Of T)(DBName)
            Else
                Dim BinF As New System.Runtime.Serialization.Formatters.Binary.BinaryFormatter()
                IO.File.OpenWrite(GetDBPath(DBName & 0)).Close()
                Using fs As New IO.FileStream(GetDBPath(DBName, True), IO.FileMode.Create, IO.FileAccess.Write)
                    Dim CI As New List(Of CacheItem)
                    BinF.Serialize(fs, CI)
                End Using
                Return New QBDatabase(Of T)(DBName, True)
            End If
        End Function
#End Region
#Region "Navigation"
        Public Sub Reload()
            If Not IO.File.Exists(GetDBPath(Name, True)) Then
                Throw New InvalidOperationException("Couldn't find a database hint file")
            Else
                Dim BinF As New System.Runtime.Serialization.Formatters.Binary.BinaryFormatter()
                Dim Metadata = TryCast(BinF.Deserialize(New IO.MemoryStream(IO.File.ReadAllBytes(GetDBPath(Name, True)))), List(Of CacheItem))
                If Metadata IsNot Nothing Then
                    CachedItems = Metadata
                    RefreshSize()
                End If
            End If
        End Sub

        Public Function GetItem(UID As Guid) As T
            If UID = Guid.Empty Then Return Nothing
            Dim sUID = UID.ToString
            Dim CI As CacheItem = CachedItems.FirstOrDefault(Function(k) k.UID.ToString = sUID)
            If CI Is Nothing Then Return Nothing
            Dim BinF As New System.Runtime.Serialization.Formatters.Binary.BinaryFormatter()
            Dim buffer(CI.Length - 1) As Byte
            Using fs = IO.File.OpenRead(GetDBPath(Name & CI.Location))
                fs.Seek(CI.StartIndex, IO.SeekOrigin.Begin)
                fs.Read(buffer, CI.StartIndex, buffer.Length)
            End Using
            Dim obj = BinF.Deserialize(New IO.MemoryStream(buffer))
            If TypeOf obj Is T Then
                Return CType(obj, T)
            Else
                Return Nothing
            End If
        End Function

        Public Async Function GetItemAsync(UID As Guid) As Task(Of T)
            If UID = Guid.Empty Then Return Nothing
            Dim sUID = UID.ToString
            Dim CI As CacheItem = CachedItems.FirstOrDefault(Function(k) k.UID.ToString = sUID)
            If CI Is Nothing Then Return Nothing
            Dim BinF As New System.Runtime.Serialization.Formatters.Binary.BinaryFormatter()
            Dim buffer(CI.Length - 1) As Byte
            Using fs = IO.File.OpenRead(GetDBPath(Name & CI.Location))
                fs.Seek(CI.StartIndex, IO.SeekOrigin.Begin)
                Await fs.ReadAsync(buffer, CI.StartIndex, buffer.Length)
            End Using
            Dim obj = BinF.Deserialize(New IO.MemoryStream(buffer))
            If TypeOf obj Is T Then
                Return CType(obj, T)
            Else
                Return Nothing
            End If
        End Function

        Public Function GetItem(ID As String) As T
            If String.IsNullOrEmpty(ID) Then Return Nothing
            Dim CI As CacheItem = CachedItems.FirstOrDefault(Function(k) k.ID = ID)
            If CI Is Nothing Then Return Nothing
            Dim BinF As New System.Runtime.Serialization.Formatters.Binary.BinaryFormatter()
            Dim buffer(CI.Length - 1) As Byte
            Using fs = IO.File.OpenRead(GetDBPath(Name & CI.Location))
                fs.Seek(CI.StartIndex, IO.SeekOrigin.Begin)
                fs.Read(buffer, 0, CI.Length)
            End Using
            Dim obj = BinF.Deserialize(New IO.MemoryStream(buffer))
            If TypeOf obj Is T Then
                Return CType(obj, T)
            Else
                Return Nothing
            End If
        End Function

        Public Async Function GetItemAsync(ID As String) As Task(Of T)
            If String.IsNullOrEmpty(ID) Then Return Nothing
            Dim CI As CacheItem = CachedItems.FirstOrDefault(Function(k) k.ID = ID)
            If CI Is Nothing Then Return Nothing
            Dim BinF As New System.Runtime.Serialization.Formatters.Binary.BinaryFormatter()
            Dim buffer(CI.Length - 1) As Byte
            Using fs = IO.File.OpenRead(GetDBPath(Name & CI.Location))
                fs.Seek(CI.StartIndex, IO.SeekOrigin.Begin)
                Await fs.ReadAsync(buffer, 0, CI.Length)
            End Using
            Dim obj = BinF.Deserialize(New IO.MemoryStream(buffer))
            If TypeOf obj Is T Then
                Return CType(obj, T)
            Else
                Return Nothing
            End If
        End Function

        Private Function GetItem(CI As CacheItem) As T
            If CI Is Nothing Then Return Nothing
            Dim BinF As New System.Runtime.Serialization.Formatters.Binary.BinaryFormatter()
            Dim buffer(CI.Length - 1) As Byte
            Using fs = IO.File.OpenRead(GetDBPath(Name & CI.Location))
                fs.Seek(CI.StartIndex, IO.SeekOrigin.Begin)
                fs.Read(buffer, 0, CI.Length)
            End Using
            Dim obj = BinF.Deserialize(New IO.MemoryStream(buffer))
            If TypeOf obj Is T Then
                Return CType(obj, T)
            Else
                Return Nothing
            End If
        End Function

        Public Iterator Function GetItemsFromLocation(Location As Integer) As IEnumerable(Of T)
            Dim fCI = CachedItems.Where(Function(k) k.Location = Location)
            If fCI.Any Then
                If fCI.FirstOrDefault?.StartIndex = 0 Then 'blind scan
                    Using fs = IO.File.OpenRead(GetDBPath(Name & Location))
                        For Each item In CachedItems
                            Dim BinF As New System.Runtime.Serialization.Formatters.Binary.BinaryFormatter()
                            Dim buffer(item.Length - 1) As Byte
                            fs.Read(buffer, 0, item.Length)
                            Dim obj = BinF.Deserialize(New IO.MemoryStream(buffer))
                            If TypeOf obj Is T Then
                                Yield CType(obj, T)
                            Else
                                Yield Nothing
                            End If
                        Next
                    End Using
                Else 'check scan
                    Using fs = IO.File.OpenRead(GetDBPath(Name & Location))
                        For Each item In CachedItems
                            Dim BinF As New System.Runtime.Serialization.Formatters.Binary.BinaryFormatter()
                            Dim buffer(item.Length - 1) As Byte
                            fs.Seek(item.StartIndex, IO.SeekOrigin.Begin)
                            fs.Read(buffer, 0, item.Length)
                            Dim obj = BinF.Deserialize(New IO.MemoryStream(buffer))
                            If TypeOf obj Is T Then
                                Yield CType(obj, T)
                            Else
                                Yield Nothing
                            End If
                        Next
                    End Using
                End If
            End If
        End Function

        Public Iterator Function GetItems() As IEnumerable(Of T)
            Dim g = CachedItems.GroupBy(Of Integer)(Function(k) k.Location)
            For Each group In g
                If group.FirstOrDefault?.StartIndex = 0 Then 'blind scan                                    
                    Using fs = IO.File.OpenRead(GetDBPath(Name & group.Key))
                        For Each item In group
                            Dim BinF As New System.Runtime.Serialization.Formatters.Binary.BinaryFormatter()
                            Dim buffer(item.Length - 1) As Byte
                            fs.Read(buffer, 0, item.Length)
                            Dim obj = BinF.Deserialize(New IO.MemoryStream(buffer))
                            If TypeOf obj Is T Then
                                Yield CType(obj, T)
                            Else
                                Yield Nothing
                            End If
                        Next
                    End Using
                Else 'check scan                                    
                    Using fs = IO.File.OpenRead(GetDBPath(Name & group.Key))
                        For Each item In group
                            Dim BinF As New System.Runtime.Serialization.Formatters.Binary.BinaryFormatter()
                            Dim buffer(item.Length - 1) As Byte
                            fs.Seek(item.StartIndex, IO.SeekOrigin.Begin)
                            fs.Read(buffer, 0, item.Length)
                            Dim obj = BinF.Deserialize(New IO.MemoryStream(buffer))
                            If TypeOf obj Is T Then
                                Yield CType(obj, T)
                            Else
                                Yield Nothing
                            End If
                        Next
                    End Using
                End If
            Next
        End Function

        Public Async Function GetItemsAsync() As Task(Of T())
            Dim Items(CachedItems.Count - 1) As T
            Dim tracker As Integer = 0
            Dim g = CachedItems.GroupBy(Of Integer)(Function(k) k.Location)
            For Each group In g
                If group.FirstOrDefault?.StartIndex = 0 Then 'blind scan                                    
                    Using fs = IO.File.OpenRead(GetDBPath(Name & group.Key))
                        For i As Integer = 0 To group.Count - 1 'CachedItems.Count - 1                            
                            Dim item = group(i)
                            Dim BinF As New System.Runtime.Serialization.Formatters.Binary.BinaryFormatter()
                            Dim buffer(item.Length - 1) As Byte
                            Await fs.ReadAsync(buffer, 0, item.Length)
                            Dim obj = BinF.Deserialize(New IO.MemoryStream(buffer))
                            If TypeOf obj Is T Then
                                Items(tracker) = CType(obj, T)
                                tracker += 1
                            End If
                        Next
                    End Using
                Else 'check scan
                    Using fs = IO.File.OpenRead(GetDBPath(Name & group.Key))
                        For i As Integer = 0 To group.Count - 1 'CachedItems.Count - 1
                            Dim item = group(i)
                            Dim BinF As New System.Runtime.Serialization.Formatters.Binary.BinaryFormatter()
                            Dim buffer(item.Length - 1) As Byte
                            fs.Seek(item.StartIndex, IO.SeekOrigin.Begin)
                            Await fs.ReadAsync(buffer, 0, item.Length)
                            Dim obj = BinF.Deserialize(New IO.MemoryStream(buffer))
                            If TypeOf obj Is T Then
                                Items(tracker) = CType(obj, T)
                                tracker += 1
                            End If
                        Next
                    End Using
                End If
            Next
            Return Items
        End Function

        Public Function GetItems(IDs As String()) As T()
            Dim Items(IDs.Length - 1) As T
            Dim tracker = 0
            Dim g = CachedItems.Where(Function(k) IDs.Contains(k.ID)).GroupBy(Of Integer)(Function(k) k.Location)
            For Each group In g
                Using fs = IO.File.OpenRead(GetDBPath(Name & group.Key))
                    For Each item In group
                        Dim BinF As New System.Runtime.Serialization.Formatters.Binary.BinaryFormatter()
                        Dim buffer(item.Length - 1) As Byte
                        fs.Seek(item.StartIndex, IO.SeekOrigin.Begin)
                        fs.Read(buffer, 0, item.Length)
                        Dim obj = BinF.Deserialize(New IO.MemoryStream(buffer))
                        If TypeOf obj Is T Then
                            Items(tracker) = CType(obj, T)
                            tracker += 1
                        Else
                            Items(tracker) = Nothing
                            tracker += 1
                        End If
                    Next
                End Using
            Next
            Return Items
        End Function

        Public Function GetIDs() As String()
            Return CachedItems.Select(Of String)(Function(k) k.ID).ToArray
        End Function

        Public Async Function AddItemAsync(Item As T, ID As String) As Task(Of Guid)
            Dim BinF As New System.Runtime.Serialization.Formatters.Binary.BinaryFormatter()
            Dim mem As New IO.MemoryStream()
            BinF.Serialize(mem, Item)
            Dim uid As Guid = Guid.NewGuid()
            Dim loc = PickLessFilled()
            Using fs = IO.File.OpenWrite(GetDBPath(Name & loc))
                fs.Seek(0, IO.SeekOrigin.End)
                CachedItems.Add(New CacheItem() With {.UID = uid, .ID = ID, .StartIndex = fs.Length, .Length = mem.Length, .Location = loc})
                Await fs.WriteAsync(mem.ToArray, 0, mem.Length)
            End Using
            mem.Dispose()
            Using fs As New IO.FileStream(GetDBPath(Name, True), IO.FileMode.OpenOrCreate, IO.FileAccess.Write)
                BinF.Serialize(fs, CachedItems)
            End Using
            RefreshSize()
            Return uid
        End Function

        Public Function AddItem(Item As T, ID As String) As Guid
            Dim BinF As New System.Runtime.Serialization.Formatters.Binary.BinaryFormatter()
            Dim mem As New IO.MemoryStream()
            BinF.Serialize(mem, Item)
            Dim uid As Guid = Guid.NewGuid()
            Dim loc = PickLessFilled()
            Using fs = IO.File.OpenWrite(GetDBPath(Name & loc))
                fs.Seek(0, IO.SeekOrigin.End)
                CachedItems.Add(New CacheItem() With {.UID = uid, .ID = ID, .StartIndex = fs.Length, .Length = mem.Length, .Location = loc})
                fs.Write(mem.ToArray, 0, mem.Length)
            End Using
            mem.Dispose()
            Using fs As New IO.FileStream(GetDBPath(Name, True), IO.FileMode.OpenOrCreate, IO.FileAccess.Write)
                BinF.Serialize(fs, CachedItems)
            End Using
            RefreshSize()
            Return uid
        End Function

        Public Function AddItems(Items As T(), IDs As String()) As Guid()
            If Items.Length <> IDs.Length Then Throw New ArgumentOutOfRangeException("Item or IDs", "Length must be matched")
            Dim GuidS(Items.Length - 1) As Guid
            Dim BinF As New System.Runtime.Serialization.Formatters.Binary.BinaryFormatter()
            Dim mem As New IO.MemoryStream()
            Dim loc = PickLessFilled()
            Dim fs = IO.File.OpenWrite(GetDBPath(Name & loc))
            fs.Seek(0, IO.SeekOrigin.End)
            For i As Integer = 0 To Items.Length - 1
                mem.SetLength(0)
                BinF.Serialize(mem, Items(i))
                Dim uid As Guid = Guid.NewGuid()
                CachedItems.Add(New CacheItem() With {.UID = uid, .ID = IDs(i), .StartIndex = fs.Length, .Length = mem.Length, .Location = loc})
                fs.Write(mem.ToArray, 0, mem.Length)
                GuidS(i) = uid
                Dim iloc = PickLessFilled()
                If loc <> iloc Then
                    fs.Dispose()
                    fs = IO.File.OpenWrite(GetDBPath(Name & iloc))
                    loc = iloc
                End If
            Next
            fs.Dispose()
            mem.Dispose()
            Using fsi As New IO.FileStream(GetDBPath(Name, True), IO.FileMode.OpenOrCreate, IO.FileAccess.Write)
                BinF.Serialize(fsi, CachedItems)
            End Using
            RefreshSize()
            Return GuidS
        End Function

        Public Function AddItems(Items As T(), IDSelect As Func(Of T, String)) As Guid()
            Dim GuidS(Items.Length - 1) As Guid
            Dim BinF As New System.Runtime.Serialization.Formatters.Binary.BinaryFormatter()
            Dim mem As New IO.MemoryStream()
            Dim loc = PickLessFilled()
            Dim fs = IO.File.OpenWrite(GetDBPath(Name & loc))
            Dim i = 0
            fs.Seek(0, IO.SeekOrigin.End)
            For Each item In Items
                mem.SetLength(0)
                BinF.Serialize(mem, item)
                Dim uid As Guid = Guid.NewGuid()
                CachedItems.Add(New CacheItem() With {.UID = uid, .ID = IDSelect.Invoke(item), .StartIndex = fs.Length, .Length = mem.Length, .Location = loc})
                fs.Write(mem.ToArray, 0, mem.Length)
                GuidS(i) = uid
                i += 1
                Dim iloc = PickLessFilled()
                If loc <> iloc Then
                    fs.Dispose()
                    fs = IO.File.OpenWrite(GetDBPath(Name & iloc))
                    loc = iloc
                End If
            Next
            fs.Dispose()
            mem.Dispose()
            Using fsi As New IO.FileStream(GetDBPath(Name, True), IO.FileMode.OpenOrCreate, IO.FileAccess.Write) '🏎 🏁
                BinF.Serialize(fsi, CachedItems)
            End Using
            Return GuidS
            RefreshSize()
        End Function

#Region "Remove Item Proxy"
        Public Function RemoveItem(ID As String) As Boolean
            If String.IsNullOrEmpty(ID) Then Return False
            Return RemoveItemInternal(CachedItems.FirstOrDefault(Function(k) k.ID = ID))
        End Function
        Public Function RemoveItem(UID As Guid) As Boolean
            If UID = Guid.Empty Then Return False
            Dim sUID = UID.ToString
            Return RemoveItemInternal(CachedItems.FirstOrDefault(Function(k) k.UID.ToString = sUID))
        End Function
#End Region

        Private Function RemoveItemInternal(Item As CacheItem) As Boolean
            If Item Is Nothing Then Return False
            Dim BinF As New System.Runtime.Serialization.Formatters.Binary.BinaryFormatter()
            Using fs = IO.File.Open(GetDBPath(Name & Item.Location), IO.FileMode.Open)
                Dim fbuffer(Item.StartIndex - 1) As Byte
                fs.Read(fbuffer, 0, Item.StartIndex)
                fs.Seek(Item.StartIndex + Item.Length, IO.SeekOrigin.Begin)
                Dim lbuffer(fs.Length - fs.Position) As Byte
                fs.Read(lbuffer, 0, lbuffer.Length)
                fs.SetLength(Item.StartIndex)
                fs.Write(lbuffer, 0, lbuffer.Length)
            End Using
            Dim offset = Item.Length
            Dim fCachedItems = CachedItems.Where(Function(k) k.Location = Item.Location)
            For Each fItem In fCachedItems
                If fItem.StartIndex < Item.StartIndex Then Continue For
                fItem.StartIndex -= offset
            Next
            Dim i = CachedItems.IndexOf(Item)
            If i = -1 Then CachedItems.Remove(Item) Else CachedItems.RemoveAt(i)
            Using fs As New IO.FileStream(GetDBPath(Name, True), IO.FileMode.OpenOrCreate, IO.FileAccess.Write)
                BinF.Serialize(fs, CachedItems)
            End Using
            RefreshSize()
            Return True
        End Function

        Public Sub RemoveCluster(index As Integer)
            If Not IO.File.Exists(GetDBPath(Name & index)) Then Return
            Try
                IO.File.Delete(GetDBPath(Name & index))
            Catch ex As Exception
                Return 'if fails keep data
            End Try
            Dim i = 0
            Do While i < CachedItems.Count
                If CachedItems(i).Location = index Then
                    CachedItems.RemoveAt(i)
                Else
                    i += 1
                End If
            Loop
            Dim BinF As New System.Runtime.Serialization.Formatters.Binary.BinaryFormatter()
            Using fs As New IO.FileStream(GetDBPath(Name, True), IO.FileMode.OpenOrCreate, IO.FileAccess.Write)
                BinF.Serialize(fs, CachedItems)
            End Using
            RefreshSize()
        End Sub

        Public Sub Clear()
            For Each group In CachedItems.GroupBy(Of Integer)(Function(k) k.Location)
                IO.File.Delete(GetDBPath(Name & group.Key))
            Next
            Using fs = IO.File.OpenWrite(GetDBPath(Name & 0))
                fs.SetLength(0)
            End Using
            CachedItems.Clear()
            Dim BinF As New System.Runtime.Serialization.Formatters.Binary.BinaryFormatter()
            Using fs As New IO.FileStream(GetDBPath(Name, True), IO.FileMode.Create, IO.FileAccess.Write)
                BinF.Serialize(fs, CachedItems)
            End Using
            RefreshSize()
        End Sub
#End Region
#Region "Methods"
        Public Function ContainsID(ID As String) As Boolean
            Return CachedItems.FirstOrDefault(Function(k) k.ID = ID) IsNot Nothing
        End Function

        Public Function ContainsUID(UID As Guid) As Boolean
            Dim sUID = UID.ToString
            Return CachedItems.FirstOrDefault(Function(k) k.UID.ToString = sUID) IsNot Nothing
        End Function
#End Region
#Region "Helpers"
        Private Sub RefreshSize()
            Application.Current.Dispatcher.Invoke(Sub()
                                                      _TotalSize = 0
                                                      Size.Clear()
                                                      For Each group In CachedItems.GroupBy(Of Integer)(Function(k) k.Location)
                                                          Dim info As New IO.FileInfo(GetDBPath(Name & group.Key))
                                                          _TotalSize += info.Length
                                                          Size.Add(New KeyValuePair(Of UInteger, Long)(group.Key, info.Length))
                                                      Next
                                                      RefreshCount()
                                                  End Sub)
        End Sub
        Private Sub RefreshCount()
            OnPropertyChanged(NameOf(Count))
            OnPropertyChanged(NameOf(HasItems))
        End Sub
        Protected Overridable Sub OnPropertyChanged(<Runtime.CompilerServices.CallerMemberName> Optional CallerName As String = Nothing)
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(CallerName))
        End Sub

        Private Shared Function GetDBPath(FileName As String, Optional IsHint As Boolean = False) As String
            If Not IO.Directory.Exists(IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "QuickBeat", "Shared")) Then
                IO.Directory.CreateDirectory(IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "QuickBeat", "Shared"))
            End If
            If IsHint Then
                Return IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "QuickBeat", "Shared", FileName & ".qbdh")
            Else
                Return IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "QuickBeat", "Shared", FileName & ".qbd")
            End If
        End Function

        Private Function PickLessFilled() As Integer
            If CachedItems.Count = 0 Then
                Return 0
            Else
                Dim g = CachedItems.GroupBy(Of Integer)(Function(k) k.Location)
                Dim lf = g.FirstOrDefault(Function(k) k.Count < ITEMSPERDB)
                If lf Is Nothing Then
                    IO.File.OpenWrite(GetDBPath(Name & g.Last.Key + 1)).Close()
                    Return g.Last.Key + 1
                Else
                    Return lf.Key
                End If
            End If
        End Function

        Private Sub _Size_CollectionChanged(sender As Object, e As NotifyCollectionChangedEventArgs) Handles _Size.CollectionChanged
            OnPropertyChanged(NameOf(TotalSize))
        End Sub
#End Region
    End Class
End Namespace
Namespace Classes.Database.WPF
    Public Class DelegateClearCommand
        Implements ICommand

        Public Event CanExecuteChanged As EventHandler Implements ICommand.CanExecuteChanged

        Sub New()
            AddHandler CommandManager.RequerySuggested, New EventHandler(Sub(sender As Object, e As EventArgs)
                                                                             RaiseEvent CanExecuteChanged(Me, New EventArgs())
                                                                         End Sub)
        End Sub

        Public Function CanExecute(parameter As Object) As Boolean Implements ICommand.CanExecute
            Return parameter IsNot Nothing AndAlso parameter.GetType.Name = "QBDatabase`1" '.IsGenericType AndAlso t.IsGenericTypeDefinition AndAlso t.GetGenericTypeDefinition Is GetType(QBDatabase(Of))
        End Function

        Public Sub Execute(parameter As Object) Implements ICommand.Execute
            If HandyControl.Controls.MessageBox.Show(Application.Current.MainWindow, Utilities.ResourceResolver.Strings.QUERY_CONFIRM, Utilities.ResourceResolver.Strings.APPNAME, MessageBoxButton.YesNo, MessageBoxImage.Question) <> MessageBoxResult.Yes Then
                Return
            End If
            parameter?.Clear()
        End Sub
    End Class

    Public Class DelegateDeleteClusterCommand(Of T)
        Implements ICommand

        Public Event CanExecuteChanged As EventHandler Implements ICommand.CanExecuteChanged
        Private _Parent As QBDatabase(Of T)

        Sub New(parent As QBDatabase(Of T))
            _Parent = parent
            AddHandler CommandManager.RequerySuggested, New EventHandler(Sub(sender As Object, e As EventArgs)
                                                                             RaiseEvent CanExecuteChanged(Me, New EventArgs())
                                                                         End Sub)
        End Sub

        Public Function CanExecute(parameter As Object) As Boolean Implements ICommand.CanExecute
            Dim i = 0
            Return _Parent IsNot Nothing AndAlso Integer.TryParse(parameter?.ToString, i)
        End Function

        Public Sub Execute(parameter As Object) Implements ICommand.Execute
            If HandyControl.Controls.MessageBox.Show(Application.Current.MainWindow, Utilities.ResourceResolver.Strings.QUERY_CONFIRM, Utilities.ResourceResolver.Strings.APPNAME, MessageBoxButton.YesNo, MessageBoxImage.Question) <> MessageBoxResult.Yes Then
                Return
            End If
            _Parent.RemoveCluster(CInt(parameter))
        End Sub
    End Class

    Public Class DelegateReloadCommand
        Implements ICommand

        Public Event CanExecuteChanged As EventHandler Implements ICommand.CanExecuteChanged

        Sub New()
            AddHandler CommandManager.RequerySuggested, New EventHandler(Sub(sender As Object, e As EventArgs)
                                                                             RaiseEvent CanExecuteChanged(Me, New EventArgs())
                                                                         End Sub)
        End Sub

        Public Function CanExecute(parameter As Object) As Boolean Implements ICommand.CanExecute
            Return parameter IsNot Nothing AndAlso parameter.GetType.Name = "QBDatabase`1"
        End Function

        Public Sub Execute(parameter As Object) Implements ICommand.Execute
            parameter?.Reload()
        End Sub
    End Class
End Namespace