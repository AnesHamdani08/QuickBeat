Imports System.Net.Http
Imports Newtonsoft.Json
Namespace Utilities
	Public Class GithubUpdatesHelper
		Const BaseURL As String = "https://api.github.com"
		Public Async Function ListReleases(owner As String, repo As String) As Task(Of Root())
			Dim url As String = $"{BaseURL}/repos/{owner}/{repo}/releases"
			Using hc As New HttpClient
				hc.DefaultRequestHeaders.Accept.Add(New System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"))
				hc.DefaultRequestHeaders.UserAgent.ParseAdd("request")
				Dim data As String
				Try
					data = Await hc.GetStringAsync(url)
				Catch ex As Exception
					Utilities.DebugMode.Instance.Log(Of GithubUpdatesHelper)(ex.ToString)
					Return Nothing
				End Try
				If data.StartsWith("[") Then 'Indicates JArray = Successful
					Dim jdoc = Newtonsoft.Json.Linq.JArray.Parse(data)
					Dim Roots(jdoc.Count - 1) As Root
					For i As Integer = 0 To jdoc.Count - 1
						Roots(i) = JsonConvert.DeserializeObject(Of Root)(jdoc(i).ToString)
					Next
					Return Roots
				Else
					Return Nothing
				End If
			End Using
		End Function

		Public Async Function GetLatestRelease(owner As String, repo As String) As Task(Of Root)
			Dim url As String = $"{BaseURL}/repos/{owner}/{repo}/releases/latest"
			Using hc As New HttpClient
				hc.DefaultRequestHeaders.Accept.Add(New System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"))
				hc.DefaultRequestHeaders.UserAgent.ParseAdd("request")
				Dim data As String
				Try
					data = Await hc.GetStringAsync(url)
				Catch ex As Exception
					Utilities.DebugMode.Instance.Log(Of GithubUpdatesHelper)(ex.ToString)
					Return Nothing
				End Try
				If data.StartsWith("{") Then 'Indicates JToken = Successful
					Dim jdoc = Newtonsoft.Json.Linq.JObject.Parse(data)
					Return JsonConvert.DeserializeObject(Of Root)(jdoc.ToString)
				Else
					Return Nothing
				End If
			End Using
		End Function

		Public Async Function GetReleaseByTagName(owner As String, repo As String, tag As String) As Task(Of Root)
			Dim url As String = $"{BaseURL}/repos/{owner}/{repo}/releases/tags/{tag}"
			Using hc As New HttpClient
				hc.DefaultRequestHeaders.Accept.Add(New System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"))
				hc.DefaultRequestHeaders.UserAgent.ParseAdd("request")
				Dim data As String
				Try
					data = Await hc.GetStringAsync(url)
				Catch ex As Exception
					Utilities.DebugMode.Instance.Log(Of GithubUpdatesHelper)(ex.ToString)
					Return Nothing
				End Try
				If data.StartsWith("{") Then 'Indicates JToken = Successful
					Dim jdoc = Newtonsoft.Json.Linq.JObject.Parse(data)
					Return JsonConvert.DeserializeObject(Of Root)(jdoc.ToString)
				Else
					Return Nothing
				End If
			End Using
		End Function

		Public Async Function GetRelease(owner As String, repo As String, release_id As String) As Task(Of Root)
			Dim url As String = $"{BaseURL}/repos/{owner}/{repo}/releases/{release_id}"
			Using hc As New HttpClient
				hc.DefaultRequestHeaders.Accept.Add(New System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"))
				hc.DefaultRequestHeaders.UserAgent.ParseAdd("request")
				Dim data As String
				Try
					data = Await hc.GetStringAsync(url)
				Catch ex As Exception
					Utilities.DebugMode.Instance.Log(Of GithubUpdatesHelper)(ex.ToString)
					Return Nothing
				End Try
				If data.StartsWith("{") Then 'Indicates JToken = Successful
					Dim jdoc = Newtonsoft.Json.Linq.JObject.Parse(data)
					Return JsonConvert.DeserializeObject(Of Root)(jdoc.ToString)
				Else
					Return Nothing
				End If
			End Using
		End Function
#Region "Classes"
		'Thanks to: https://json2csharp.com/ ; https://jsonformatter.curiousconcept.com/
		' Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
		Public Class Asset
			<JsonProperty("url")>
			Public Property Url() As String

			<JsonProperty("id")>
			Public Property Id() As Integer

			<JsonProperty("node_id")>
			Public Property NodeId() As String

			<JsonProperty("name")>
			Public Property Name() As String

			<JsonProperty("label")>
			Public Property Label() As Object

			<JsonProperty("uploader")>
			Public Property Uploader() As Uploader

			<JsonProperty("content_type")>
			Public Property ContentType() As String

			<JsonProperty("state")>
			Public Property State() As String

			<JsonProperty("size")>
			Public Property Size() As Integer

			<JsonProperty("download_count")>
			Public Property DownloadCount() As Integer

			<JsonProperty("created_at")>
			Public Property CreatedAt() As DateTime

			<JsonProperty("updated_at")>
			Public Property UpdatedAt() As DateTime

			<JsonProperty("browser_download_url")>
			Public Property BrowserDownloadUrl() As String
		End Class
		Public Class Author
			<JsonProperty("login")>
			Public Property Login() As String

			<JsonProperty("id")>
			Public Property Id() As Integer

			<JsonProperty("node_id")>
			Public Property NodeId() As String

			<JsonProperty("avatar_url")>
			Public Property AvatarUrl() As String

			<JsonProperty("gravatar_id")>
			Public Property GravatarId() As String

			<JsonProperty("url")>
			Public Property Url() As String

			<JsonProperty("html_url")>
			Public Property HtmlUrl() As String

			<JsonProperty("followers_url")>
			Public Property FollowersUrl() As String

			<JsonProperty("following_url")>
			Public Property FollowingUrl() As String

			<JsonProperty("gists_url")>
			Public Property GistsUrl() As String

			<JsonProperty("starred_url")>
			Public Property StarredUrl() As String

			<JsonProperty("subscriptions_url")>
			Public Property SubscriptionsUrl() As String

			<JsonProperty("organizations_url")>
			Public Property OrganizationsUrl() As String

			<JsonProperty("repos_url")>
			Public Property ReposUrl() As String

			<JsonProperty("events_url")>
			Public Property EventsUrl() As String

			<JsonProperty("received_events_url")>
			Public Property ReceivedEventsUrl() As String

			<JsonProperty("type")>
			Public Property Type() As String

			<JsonProperty("site_admin")>
			Public Property SiteAdmin() As Boolean
		End Class
		Public Class Root
			<JsonProperty("url")>
			Public Property Url() As String

			<JsonProperty("assets_url")>
			Public Property AssetsUrl() As String

			<JsonProperty("upload_url")>
			Public Property UploadUrl() As String

			<JsonProperty("html_url")>
			Public Property HtmlUrl() As String

			<JsonProperty("id")>
			Public Property Id() As Integer

			<JsonProperty("author")>
			Public Property Author() As Author

			<JsonProperty("node_id")>
			Public Property NodeId() As String

			<JsonProperty("tag_name")>
			Public Property TagName() As String

			<JsonProperty("target_commitish")>
			Public Property TargetCommitish() As String

			<JsonProperty("name")>
			Public Property Name() As String

			<JsonProperty("draft")>
			Public Property Draft() As Boolean

			<JsonProperty("prerelease")>
			Public Property Prerelease() As Boolean

			<JsonProperty("created_at")>
			Public Property CreatedAt() As DateTime

			<JsonProperty("published_at")>
			Public Property PublishedAt() As DateTime

			<JsonProperty("assets")>
			Public Property Assets() As List(Of Asset)

			<JsonProperty("tarball_url")>
			Public Property TarballUrl() As String

			<JsonProperty("zipball_url")>
			Public Property ZipballUrl() As String

			<JsonProperty("body")>
			Public Property Body() As String
		End Class
		Public Class Uploader
			<JsonProperty("login")>
			Public Property Login() As String

			<JsonProperty("id")>
			Public Property Id() As Integer

			<JsonProperty("node_id")>
			Public Property NodeId() As String

			<JsonProperty("avatar_url")>
			Public Property AvatarUrl() As String

			<JsonProperty("gravatar_id")>
			Public Property GravatarId() As String

			<JsonProperty("url")>
			Public Property Url() As String

			<JsonProperty("html_url")>
			Public Property HtmlUrl() As String

			<JsonProperty("followers_url")>
			Public Property FollowersUrl() As String

			<JsonProperty("following_url")>
			Public Property FollowingUrl() As String

			<JsonProperty("gists_url")>
			Public Property GistsUrl() As String

			<JsonProperty("starred_url")>
			Public Property StarredUrl() As String

			<JsonProperty("subscriptions_url")>
			Public Property SubscriptionsUrl() As String

			<JsonProperty("organizations_url")>
			Public Property OrganizationsUrl() As String

			<JsonProperty("repos_url")>
			Public Property ReposUrl() As String

			<JsonProperty("events_url")>
			Public Property EventsUrl() As String

			<JsonProperty("received_events_url")>
			Public Property ReceivedEventsUrl() As String

			<JsonProperty("type")>
			Public Property Type() As String

			<JsonProperty("site_admin")>
			Public Property SiteAdmin() As Boolean
		End Class
#End Region
	End Class
End Namespace