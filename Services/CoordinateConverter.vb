Imports inoMaps.Models

Namespace Services

    Public Class CoordinateConverter

        Public Shared Function PixelToGeo(
                map As MapDefinition,
                imageWidth As Double,
                imageHeight As Double,
                pixelX As Double,
                pixelY As Double) As GeoCoordinate

            Dim relX As Double =
                pixelX / imageWidth

            Dim relY As Double =
                pixelY / imageHeight

            Dim lon As Double =
                map.TopLeft.Longitude +
                relX *
                (map.TopRight.Longitude -
                 map.TopLeft.Longitude)

            Dim lat As Double =
                map.TopLeft.Latitude -
                relY *
                (map.TopLeft.Latitude -
                 map.BottomLeft.Latitude)

            If map.CoordinateSystem.Equals(
                "Ferro",
                StringComparison.OrdinalIgnoreCase) Then

                lon -= 17.6666667

            End If

            Return New GeoCoordinate With {
                .Longitude = lon,
                .Latitude = lat
            }

        End Function

        Public Shared Function GreenwichToFerro(
                longitude As Double) As Double

            Return longitude + 17.6666667

        End Function

    End Class

End Namespace
