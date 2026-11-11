Imports inoMaps.Models

Namespace Services

    Public Class BilinearConverter

        Public Shared Function PixelToGeo(
            map As MapDefinition,
            pixelX As Double,
            pixelY As Double,
            imageWidth As Double,
            imageHeight As Double) As GeoPoint

            'Dim u As Double = pixelX / imageWidth
            'Dim v As Double = pixelY / imageHeight

            Dim u As Double =
                (pixelX - map.TopLeftPixel.X) /
                (map.TopRightPixel.X -
                 map.TopLeftPixel.X)

            Dim v As Double =
                (pixelY - map.TopLeftPixel.Y) /
                (map.BottomLeftPixel.Y -
                 map.TopLeftPixel.Y)

            Dim lon As Double =
                    (1 - u) * (1 - v) * map.TopLeft.Longitude +
                    u * (1 - v) * map.TopRight.Longitude +
                    (1 - u) * v * map.BottomLeft.Longitude +
                    u * v * map.BottomRight.Longitude

            Dim lat As Double =
                    (1 - u) * (1 - v) * map.TopLeft.Latitude +
                    u * (1 - v) * map.TopRight.Latitude +
                    (1 - u) * v * map.BottomLeft.Latitude +
                    u * v * map.BottomRight.Latitude

            If map.CoordinateSystem.Equals(
                "Ferro",
                StringComparison.OrdinalIgnoreCase) Then

                lon -= 17.6666667

            End If

            Return New GeoPoint With {
                .Longitude = lon,
                .Latitude = lat
            }

        End Function

    End Class

End Namespace
