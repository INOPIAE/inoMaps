Imports inoMaps.Models

Namespace Services

    Public Class BilinearConverter

        Public Shared Function PixelToGeo(map As MapDefinition, pixelX As Double, pixelY As Double, imageWidth As Double, imageHeight As Double) As GeoPoint

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

        Public Shared Function GeoToPixel(map As MapDefinition, longitude As Double, latitude As Double, imageWidth As Double, imageHeight As Double) As PixelPoint

            ' Ziel-Längengrad ggf. von Ferro auf Greenwich zurückrechnen,
            ' da map.TopLeft/... stets in Greenwich vorliegen (siehe PixelToGeo).
            Dim lonTarget As Double = longitude

            If map.CoordinateSystem.Equals(
                "Ferro",
                StringComparison.OrdinalIgnoreCase) Then

                lonTarget += 17.6666667

            End If

            ' Inverse bilineare Transformation per Newton-Iteration:
            ' PixelToGeo berechnet lon/lat als bilineare Funktion von (u, v).
            ' Diese Funktion sucht (u, v), sodass lon(u,v) = lonTarget und
            ' lat(u,v) = latitude gilt, und berechnet daraus die Pixelkoordinaten.

            Dim u As Double = 0.5
            Dim v As Double = 0.5

            Const maxIterations As Integer = 50
            Const tolerance As Double = 0.0000000001

            For i As Integer = 1 To maxIterations

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

                Dim fLon As Double = lon - lonTarget
                Dim fLat As Double = lat - latitude

                If Math.Abs(fLon) < tolerance AndAlso
                   Math.Abs(fLat) < tolerance Then

                    Exit For

                End If

                ' Jacobi-Matrix (partielle Ableitungen nach u und v)
                Dim dLonDu As Double =
                    -(1 - v) * map.TopLeft.Longitude +
                    (1 - v) * map.TopRight.Longitude -
                    v * map.BottomLeft.Longitude +
                    v * map.BottomRight.Longitude

                Dim dLonDv As Double =
                    -(1 - u) * map.TopLeft.Longitude -
                    u * map.TopRight.Longitude +
                    (1 - u) * map.BottomLeft.Longitude +
                    u * map.BottomRight.Longitude

                Dim dLatDu As Double =
                    -(1 - v) * map.TopLeft.Latitude +
                    (1 - v) * map.TopRight.Latitude -
                    v * map.BottomLeft.Latitude +
                    v * map.BottomRight.Latitude

                Dim dLatDv As Double =
                    -(1 - u) * map.TopLeft.Latitude -
                    u * map.TopRight.Latitude +
                    (1 - u) * map.BottomLeft.Latitude +
                    u * map.BottomRight.Latitude

                ' Lineares Gleichungssystem per Cramer-Regel lösen:
                ' [dLonDu dLonDv] [deltaU]   [-fLon]
                ' [dLatDu dLatDv] [deltaV] = [-fLat]

                Dim det As Double =
                    dLonDu * dLatDv - dLonDv * dLatDu

                If Math.Abs(det) < 0.000000000001 Then

                    Exit For

                End If

                Dim deltaU As Double =
                    (-fLon * dLatDv + fLat * dLonDv) / det

                Dim deltaV As Double =
                    (-dLonDu * fLat + dLatDu * fLon) / det

                u += deltaU
                v += deltaV

            Next

            Dim pixelX As Double =
                (1 - u) * (1 - v) * map.TopLeftPixel.X +
                u * (1 - v) * map.TopRightPixel.X +
                (1 - u) * v * map.BottomLeftPixel.X +
                u * v * map.BottomRightPixel.X

            Dim pixelY As Double =
                (1 - u) * (1 - v) * map.TopLeftPixel.Y +
                u * (1 - v) * map.TopRightPixel.Y +
                (1 - u) * v * map.BottomLeftPixel.Y +
                u * v * map.BottomRightPixel.Y

            Return New PixelPoint With {
                .X = pixelX,
                .Y = pixelY
            }

        End Function
    End Class

End Namespace
