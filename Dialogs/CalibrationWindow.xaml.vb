Imports inoMaps.Models
Imports Microsoft.VisualBasic
Imports System.Text.Json
Imports System.Windows
Imports System.Windows.Input
Imports System.Windows.Media.Imaging
Imports Microsoft.Win32

Namespace Dialogs
    Public Class CalibrationWindow

        Private _imageFile As String

        Private CurrentStep As Integer = 0

        Private MapData As New MapDefinition

        Private CurrentZoom As Double = 1.0

        Public Sub New(Optional imageFile As String = Nothing)

            InitializeComponent()

            AddHandler Me.MouseWheel,
                AddressOf Window_MouseWheel

            AddHandler Me.Loaded,
            AddressOf CalibrationWindow_Loaded

            If Not String.IsNullOrEmpty(imageFile) Then

                LoadImage(imageFile)

            End If

        End Sub

        Private Sub CalibrationWindow_Loaded(sender As Object, e As RoutedEventArgs)

            If String.IsNullOrEmpty(_imageFile) Then

                If Not PromptLoadImage() Then

                    Me.Close()

                End If

            End If

        End Sub

        Private Function PromptLoadImage() As Boolean

            Dim dlg As New OpenFileDialog

            dlg.Filter = "Bilddateien|*.jpg;*.jpeg;*.png;*.bmp;*.tif"

            If dlg.ShowDialog <> True Then Return False

            LoadImage(dlg.FileName)

            Return True

        End Function

        Private Sub LoadImage(imageFile As String)

            _imageFile = imageFile

            imgMap.Source =
            New BitmapImage(
                New Uri(imageFile))

            MapData.ImageFile =
            IO.Path.GetFileName(imageFile)

            CurrentStep = 0

            UpdateStep()

        End Sub

        Private Sub LoadImage_Click(
            sender As Object,
            e As RoutedEventArgs)

            PromptLoadImage()

        End Sub

        Private Sub UpdateStep()

            Select Case CurrentStep

                Case 0
                    txtStep.Text =
                    "Obere linke Ecke anklicken"

                Case 1
                    txtStep.Text =
                    "Obere rechte Ecke anklicken"

                Case 2
                    txtStep.Text =
                    "Untere linke Ecke anklicken"

                Case 3
                    txtStep.Text =
                    "Untere rechte Ecke anklicken"

                Case Else
                    txtStep.Text =
                    "Kalibrierung abgeschlossen"

            End Select

        End Sub

        Private Sub imgMap_MouseLeftButtonDown(sender As Object, e As MouseButtonEventArgs)

            If imgMap.Source Is Nothing Then Return

            If CurrentStep > 3 Then Return

            Dim p =
            e.GetPosition(imgMap)

            Dim lonInput = InputBox("Längengrad eingeben")

            Dim lon As Double

            If Not Double.TryParse(lonInput, lon) Then
                MessageBox.Show("Ungültiger Längengrad.")
                Return
            End If

            Dim latInput = InputBox("Breitengrad eingeben")

            Dim lat As Double

            If Not Double.TryParse(latInput, lat) Then
                MessageBox.Show("Ungültiger Breitengrad.")
                Return
            End If

            Dim geo As New GeoPoint With {
            .Longitude = lon,
            .Latitude = lat
        }

            Dim pix As New PixelPoint With {
            .X = p.X,
            .Y = p.Y
        }

            Select Case CurrentStep

                Case 0
                    MapData.TopLeft = geo
                    MapData.TopLeftPixel = pix

                Case 1
                    MapData.TopRight = geo
                    MapData.TopRightPixel = pix

                Case 2
                    MapData.BottomLeft = geo
                    MapData.BottomLeftPixel = pix

                Case 3
                    MapData.BottomRight = geo
                    MapData.BottomRightPixel = pix

            End Select

            CurrentStep += 1

            UpdateStep()

        End Sub

        Private Sub SaveCalibration_Click(sender As Object, e As RoutedEventArgs)

            If CurrentStep <= 3 Then
                MessageBox.Show("Bitte zuerst alle vier Eckpunkte kalibrieren.")
                Return
            End If

            Dim saveDlg As New Microsoft.Win32.SaveFileDialog

            saveDlg.Filter = "Json|*.json"

            If saveDlg.ShowDialog <> True Then Return

            MapData.Name = InputBox("Kartenname")

            MapData.CoordinateSystem = InputBox("Koordinatensystem (Greenwich/Ferro)", , "Greenwich")

            Dim options As New JsonSerializerOptions

            options.WriteIndented = True

            Dim json = JsonSerializer.Serialize(MapData, options)

            IO.File.WriteAllText(saveDlg.FileName, json)

            MessageBox.Show("Kalibrierung gespeichert.")

            DialogResult = True

        End Sub

        Private Sub ZoomIn_Click(sender As Object, e As RoutedEventArgs)

            CurrentZoom *= 1.2

            ZoomTransform.ScaleX = CurrentZoom
            ZoomTransform.ScaleY = CurrentZoom

        End Sub

        Private Sub ZoomOut_Click(sender As Object, e As RoutedEventArgs)

            CurrentZoom /= 1.2

            ZoomTransform.ScaleX = CurrentZoom
            ZoomTransform.ScaleY = CurrentZoom

        End Sub

        Private Sub Window_MouseWheel(sender As Object, e As MouseWheelEventArgs)

            If e.Delta > 0 Then
                CurrentZoom *= 1.1
            Else
                CurrentZoom /= 1.1
            End If

            ZoomTransform.ScaleX = CurrentZoom
            ZoomTransform.ScaleY = CurrentZoom

        End Sub

        Private Sub imgMap_MouseMove(sender As Object, e As MouseEventArgs)

            Dim p = e.GetPosition(imgMap)

            txtMousePosition.Text =
            $"X={p.X:0}  Y={p.Y:0}"

        End Sub
    End Class
End Namespace