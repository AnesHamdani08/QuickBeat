Namespace Utilities
    ''' <summary>
    ''' Provides methods to override apps properties
    ''' </summary>
    Public Class OverrideProperties
        Public Enum LockType
            Metadata_DecodeToThumbnail
            Metadata_DecodePixelWidth
            Metadata_DecodePixelHeight
            CommonFunctions_ToBitmapSource_DecodePixelWidth
            CommonFunctions_ToBitmapSource_DecodePixelHeight
            CommonFunctions_ToImageSource_ChangeResolution
            CommonFunctions_ToImageSource_DecodePixelWidth
            CommonFunctions_ToImageSource_DecodePixelHeight
            SharedProperties_IsInternetConnected
        End Enum
        Public Shared ReadOnly Instance As New OverrideProperties

        Property Metadata_DecodeToThumbnail As New Tuple(Of Boolean, Boolean)(False, False)
        Property Metadata_DecodePixelWidth As New Tuple(Of Boolean, Integer)(False, 0)
        Property Metadata_DecodePixelHeight As New Tuple(Of Boolean, Integer)(False, 0)
        Property CommonFunctions_ToBitmapSource_DecodePixelWidth As New Tuple(Of Boolean, Integer)(False, 0)
        Property CommonFunctions_ToBitmapSource_DecodePixelHeight As New Tuple(Of Boolean, Integer)(False, 0)
        Property CommonFunctions_ToImageSource_ChangeResolution As New Tuple(Of Boolean, Boolean)(False, False)
        Property CommonFunctions_ToImageSource_DecodePixelWidth As New Tuple(Of Boolean, Integer)(False, 0)
        Property CommonFunctions_ToImageSource_DecodePixelHeight As New Tuple(Of Boolean, Integer)(False, 0)
        Property SharedProperties_IsInternetConnected As New Tuple(Of Boolean, Boolean)(False, False)

        Public ReadOnly Property Locked(lock As LockType) As Boolean
            Get
                Select Case lock
                    Case LockType.Metadata_DecodeToThumbnail
                        Return Metadata_DecodeToThumbnail?.Item1
                    Case LockType.Metadata_DecodePixelWidth
                        Return Metadata_DecodePixelWidth?.Item1
                    Case LockType.Metadata_DecodePixelHeight
                        Return Metadata_DecodePixelHeight?.Item1
                    Case LockType.CommonFunctions_ToBitmapSource_DecodePixelWidth
                        Return CommonFunctions_ToBitmapSource_DecodePixelWidth?.Item1
                    Case LockType.CommonFunctions_ToBitmapSource_DecodePixelHeight
                        Return CommonFunctions_ToBitmapSource_DecodePixelHeight?.Item1
                    Case LockType.CommonFunctions_ToImageSource_ChangeResolution
                        Return CommonFunctions_ToImageSource_ChangeResolution?.Item1
                    Case LockType.CommonFunctions_ToImageSource_DecodePixelWidth
                        Return CommonFunctions_ToImageSource_DecodePixelWidth?.Item1
                    Case LockType.CommonFunctions_ToImageSource_DecodePixelHeight
                        Return CommonFunctions_ToImageSource_DecodePixelHeight?.Item1
                    Case LockType.SharedProperties_IsInternetConnected
                        Return SharedProperties_IsInternetConnected?.Item1
                End Select
                Return False
            End Get
        End Property
        Public ReadOnly Property Value(lock As LockType) As Object
            Get
                Select Case lock
                    Case LockType.Metadata_DecodeToThumbnail
                        Return Metadata_DecodeToThumbnail?.Item2
                    Case LockType.Metadata_DecodePixelWidth
                        Return Metadata_DecodePixelWidth?.Item2
                    Case LockType.Metadata_DecodePixelHeight
                        Return Metadata_DecodePixelHeight?.Item2
                    Case LockType.CommonFunctions_ToBitmapSource_DecodePixelWidth
                        Return CommonFunctions_ToBitmapSource_DecodePixelWidth?.Item2
                    Case LockType.CommonFunctions_ToBitmapSource_DecodePixelHeight
                        Return CommonFunctions_ToBitmapSource_DecodePixelHeight?.Item2
                    Case LockType.CommonFunctions_ToImageSource_ChangeResolution
                        Return CommonFunctions_ToImageSource_ChangeResolution?.Item2
                    Case LockType.CommonFunctions_ToImageSource_DecodePixelWidth
                        Return CommonFunctions_ToImageSource_DecodePixelWidth?.Item2
                    Case LockType.CommonFunctions_ToImageSource_DecodePixelHeight
                        Return CommonFunctions_ToImageSource_DecodePixelHeight?.Item2
                    Case LockType.SharedProperties_IsInternetConnected
                        Return SharedProperties_IsInternetConnected?.Item2
                End Select
                Return False
            End Get
        End Property

    End Class
End Namespace
