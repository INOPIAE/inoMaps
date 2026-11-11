Imports System.IO
Imports System.Text.Json
Imports inoMaps.Models

Namespace Services

    Public Class MapLoader

        Public Shared Function Load(
            jsonFile As String) As MapDefinition

            Dim json =
                File.ReadAllText(jsonFile)

            Return JsonSerializer.Deserialize(
                    Of MapDefinition)(json)

        End Function

        Public Shared Function GetImagePath(
            jsonFile As String,
            map As MapDefinition) As String

            Dim folder =
                Path.GetDirectoryName(jsonFile)

            Return Path.Combine(
                folder,
                map.ImageFile)

        End Function

    End Class

End Namespace