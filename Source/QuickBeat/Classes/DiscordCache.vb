Imports QuickBeat.Classes
Imports QuickBeat.Interfaces
Imports QuickBeat.Player
Namespace Classes
    Public Class DiscordCache
        Inherits ObjectModel.ObservableCollection(Of CacheItem) 'Query:TitleArtist,SongLink,SongLinkProvider,CoverLink
        Implements Interfaces.IStartupItem

        Public Property Configuration As New StartupItemConfiguration("Discord Cache") Implements IStartupItem.Configuration

        Public Sub Init() Implements IStartupItem.Init
            Utilities.SharedProperties.Instance?.ItemsConfiguration.Add(Configuration)
            Configuration.SetStatus("Loading Cache...", 0)
            Dim data = My.Settings.APP_INTEGRATIONS_DISCORD_CACHE
            Dim BinF As New System.Runtime.Serialization.Formatters.Binary.BinaryFormatter()
            Dim ProgStep = If(data.Count = 0, 100, 100 / data.Count)
            Dim i = 1
            For Each bin In data 'separated by ||                                
                Dim Metadata = TryCast(BinF.Deserialize(New IO.MemoryStream(Convert.FromBase64String(bin))), CacheItem)
                If Metadata Is Nothing Then Continue For
                MyBase.Add(Metadata)
                Configuration.SetStatus($"{i}/{data.Count}", Configuration.Progress + ProgStep)
                i += 1
            Next
            Configuration.SetStatus("All Good", 100)
        End Sub

        Public Shadows Sub Add(Query As String, Link As String, Provider As String, CoverLink As String, Album As String)
            MyBase.Add(New CacheItem(Query, Link, Provider, CoverLink, Album))
        End Sub

        Public Function GetValue(query As String) As CacheItem
            Return Me.FirstOrDefault(Function(k) k.Query.ToLower = query.ToLower)
        End Function

        ''' <summary>
        ''' Replaces Current Setting: APP_INTEGRATIONS_DISCORD_CACHE in <see cref="My.Settings"/> with current instance data
        ''' </summary>
        ''' <remarks>
        ''' My.Settings.Save is not called
        ''' </remarks>
        Public Sub Save()
            Dim BinF As New System.Runtime.Serialization.Formatters.Binary.BinaryFormatter
            My.Settings.APP_INTEGRATIONS_DISCORD_CACHE.Clear()
            For Each cItem In Me
                Dim mem As New IO.MemoryStream
                BinF.Serialize(mem, cItem)
                My.Settings.APP_INTEGRATIONS_DISCORD_CACHE.Add(Convert.ToBase64String(mem.ToArray))
            Next
        End Sub

        <Serializable>
        Public Class CacheItem
            Public Property Query As String
            Public Property Album As String
            Public Property Link As String
            Public Property Provider As String
            Public Property CoverLink As String

            Sub New(query As String, link As String, provider As String, coverlink As String, album As String)
                Me.Query = query
                Me.Link = link
                Me.Provider = provider
                Me.CoverLink = coverlink
                Me.Album = album
            End Sub

            Public Overrides Function ToString() As String
                Return $"{Query} - {If(Album, "N/A")}"
            End Function
        End Class
    End Class
End Namespace