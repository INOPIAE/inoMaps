Namespace Models

    Public Class MapDefinition

        Public Property Name As String

        Public Property ImageFile As String

        Public Property CoordinateSystem As String

        Public Property TopLeftPixel As PixelPoint
        Public Property TopRightPixel As PixelPoint
        Public Property BottomLeftPixel As PixelPoint
        Public Property BottomRightPixel As PixelPoint

        Public Property TopLeft As GeoPoint
        Public Property TopRight As GeoPoint
        Public Property BottomLeft As GeoPoint
        Public Property BottomRight As GeoPoint

    End Class

End Namespace