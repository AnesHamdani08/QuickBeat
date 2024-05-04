Imports System.Collections.ObjectModel
Imports System.Globalization

Namespace Converters
    Public Class CollectionCountToVisibilityConverter
        Implements IValueConverter

        Public Function Convert(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.Convert
            'Collection.GetType().IsGenericType &&           Collection.GetType().GetGenericTypeDefinition() == TypeOf (ObservableCollection <>);
            If value Is Nothing Then Return Visibility.Collapsed
            If value.GetType.IsGenericType AndAlso (value.GetType.GetGenericTypeDefinition Is GetType(ObservableCollection(Of )) OrElse value.GetType.GetGenericTypeDefinition Is GetType(Collection(Of )) OrElse value.GetType.GetGenericTypeDefinition Is GetType(List(Of ))) Then Return If(value.Count = 0, Visibility.Collapsed, Visibility.Visible)
            Return Visibility.Collapsed
        End Function

        Public Function ConvertBack(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.ConvertBack
            Return value
        End Function
    End Class
End Namespace