Namespace Interfaces
    ''' <summary>
    ''' Provides link-refresh methods
    ''' </summary>
    Public Interface IMediaProvider
        Property Name As String
        Property Author As String
        Property Version As Version
        Property Token As String

        ''' <summary>
        ''' Returns a refreshed url
        ''' </summary>        
        ''' <returns></returns>
        Function Fetch() As Task(Of String)
        Function FetchThumbnail() As Task(Of ImageSource)
    End Interface
End Namespace
