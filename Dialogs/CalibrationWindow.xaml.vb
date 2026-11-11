Imports System.IO
Imports System.Text.Json
Imports System.Windows.Input
Imports System.Windows.Media
Imports System.Windows.Media.Imaging
Imports System.Windows.Shapes
Imports inoMaps.Models
Imports Microsoft.VisualBasic

Namespace Dialogs
    Partial Public Class CalibrationWindow

        Private _imageFile As String

        Private CurrentZoom As Double = 1.0

        Private CurrentStep As Integer = 0

        Private MapData As New MapDefinition

        Public Sub New(imageFile As String)

            InitializeComponent()

            _imageFile = imageFile

            AddHandler Me.Loaded,
                AddressOf CalibrationWindow_Loaded


            AddHandler Me.MouseWheel,
                AddressOf Window_MouseWheel

        End Sub

        Private Async Sub CalibrationWindow_Loaded(sender As Object, e As RoutedEventArgs)

            Await Dispatcher.InvokeAsync(
                Sub()
                    LoadImage()
                End Sub,
                Windows.Threading.DispatcherPriority.ApplicationIdle)

            UpdateStepText()

        End Sub


#Region "Bild laden"

        Private Sub LoadImage()

            Dim bmp As New BitmapImage()

            bmp.BeginInit()
            bmp.CacheOption = BitmapCacheOption.OnLoad
            bmp.UriSource = New Uri(_imageFile)
            bmp.EndInit()

            imgMap.Source = bmp



            MapCanvas.Width = bmp.PixelWidth
            MapCanvas.Height = bmp.PixelHeight

            MapData.ImageFile =
            System.IO.Path.GetFileName(_imageFile)

        End Sub

#End Region

#Region "Kalibrierung"

        Private Sub UpdateStepText()

            Select Case CurrentStep

                Case 0
                    txtStep.Text = "Punkt 1 von 4 : Obere linke Ecke (TL)"

                Case 1
                    txtStep.Text = "Punkt 2 von 4 : Obere rechte Ecke (TR)"

                Case 2
                    txtStep.Text = "Punkt 3 von 4 : Untere linke Ecke (BL)"

                Case 3
                    txtStep.Text = "Punkt 4 von 4 : Untere rechte Ecke (BR)"

                Case Else
                    txtStep.Text = "Kalibrierung abgeschlossen"

            End Select

        End Sub

        Private Sub imgMap_MouseLeftButtonDown(sender As Object, e As MouseButtonEventArgs)

            If CurrentStep > 3 Then Return

            Dim p = e.GetPosition(imgMap)

            Dim imageX = p.X / CurrentZoom

            Dim imageY = p.Y / CurrentZoom

            Dim lonText = InputBox("Längengrad eingeben", "Kalibrierung")

            If String.IsNullOrWhiteSpace(lonText) Then Return

            Dim latText = InputBox("Breitengrad eingeben", "Kalibrierung")

            If String.IsNullOrWhiteSpace(latText) Then Return

            Dim lon As Double
            Dim lat As Double

            If lonText.Contains(" ") Then
                Dim lonp As Double() = lonText.Split(" ").Select(Function(s) Double.Parse(s, Globalization.CultureInfo.InvariantCulture)).ToArray()
                lonText = lonp(0) + lonp(1) / 60.0 '+ lonp(2) / 3600.0
                MessageBox.Show("Lon: " & lonText)
            End If
            If latText.Contains(" ") Then
                Dim latp As Double() = latText.Split(" ").Select(Function(s) Double.Parse(s, Globalization.CultureInfo.InvariantCulture)).ToArray()
                latText = latp(0) + latp(1) / 60.0 ' + latp(2) / 3600.0
                MessageBox.Show("Lat: " & latText)
            End If
            lonText = lonText.Replace(",", ".")
            latText = latText.Replace(",", ".")
            If Not Double.TryParse(
            lonText,
            Globalization.NumberStyles.Any,
            Globalization.CultureInfo.InvariantCulture,
            lon) Then

                MessageBox.Show("Ungültiger Längengrad")

                Return

            End If

            If Not Double.TryParse(
            latText,
            Globalization.NumberStyles.Any,
            Globalization.CultureInfo.InvariantCulture,
            lat) Then

                MessageBox.Show("Ungültiger Breitengrad")

                Return

            End If

            Dim pixel As New PixelPoint With {.X = imageX, .Y = imageY}
            Dim geo As New GeoPoint With {.Longitude = lon, .Latitude = lat}

            Select Case CurrentStep

                Case 0

                    MapData.TopLeftPixel = pixel
                    MapData.TopLeft = geo

                    DrawMarker(imageX, imageY, "TL")

                Case 1

                    MapData.TopRightPixel = pixel
                    MapData.TopRight = geo

                    DrawMarker(imageX, imageY, "TR")

                Case 2

                    MapData.BottomLeftPixel = pixel
                    MapData.BottomLeft = geo

                    DrawMarker(imageX, imageY, "BL")

                Case 3

                    MapData.BottomRightPixel = pixel
                    MapData.BottomRight = geo

                    DrawMarker(imageX, imageY, "BR")

            End Select

            CurrentStep += 1

            UpdateStepText()

        End Sub

#End Region

#Region "Marker"

        Private Sub DrawMarker(x As Double, y As Double, text As String)

            Dim marker As New Ellipse()

            marker.Width = 14
            marker.Height = 14

            marker.Fill = Brushes.Red
            marker.Stroke = Brushes.White
            marker.StrokeThickness = 2

            Canvas.SetLeft(marker, x - 7)
            Canvas.SetTop(marker, y - 7)

            MapCanvas.Children.Add(marker)

            Dim label As New TextBlock()

            label.Text = text
            label.Foreground = Brushes.Red
            label.FontWeight = FontWeights.Bold

            Canvas.SetLeft(label, x + 10)
            Canvas.SetTop(label, y - 10)

            MapCanvas.Children.Add(label)

        End Sub

#End Region

#Region "Zoom"

        Private Sub ZoomIn_Click(sender As Object, e As RoutedEventArgs)
            CurrentZoom *= 1.2

            ApplyZoom()

        End Sub

        Private Sub ZoomOut_Click(sender As Object, e As RoutedEventArgs)

            CurrentZoom /= 1.2

            ApplyZoom()

        End Sub

        Private Sub Window_MouseWheel(sender As Object, e As MouseWheelEventArgs)

            If e.Delta > 0 Then
                CurrentZoom *= 1.1
            Else
                CurrentZoom /= 1.1
            End If

            ApplyZoom()

        End Sub

        Private Sub ApplyZoom()

            ZoomTransform.ScaleX = CurrentZoom
            ZoomTransform.ScaleY = CurrentZoom

            txtZoom.Text =
            $"{CurrentZoom * 100:F0} %"

        End Sub

#End Region

#Region "Mausposition"

        Private Sub imgMap_MouseMove(sender As Object, e As MouseEventArgs)

            Dim p = e.GetPosition(imgMap)

            Dim imageX = p.X / CurrentZoom

            Dim imageY = p.Y / CurrentZoom

            txtMousePosition.Text = $"X={imageX:F6}   Y={imageY:F6}"

        End Sub

#End Region

#Region "Speichern"

        Private Sub SaveCalibration_Click(
        sender As Object,
        e As RoutedEventArgs)

            If CurrentStep < 4 Then

                MessageBox.Show("Bitte zuerst alle vier Kalibrierpunkte setzen.")

                Return

            End If

            Dim mapName = InputBox("Name der Karte", "Kartendefinition")

            If String.IsNullOrWhiteSpace(mapName) Then
                Return
            End If

            MapData.Name = mapName

            Dim cs = InputBox("Koordinatensystem (Greenwich oder Ferro)", "Koordinatensystem", "Greenwich")

            If String.IsNullOrWhiteSpace(cs) Then
                cs = "Greenwich"
            End If

            MapData.CoordinateSystem = cs

            Dim dlg As New Microsoft.Win32.SaveFileDialog()

            dlg.Filter = "JSON-Dateien (*.json)|*.json"

            dlg.FileName = mapName.Replace(" ", "_") & ".json"

            If dlg.ShowDialog() <> True Then
                Return
            End If

            Dim options As New JsonSerializerOptions()

            options.WriteIndented = True

            Dim json = JsonSerializer.Serialize(MapData, options)

            File.WriteAllText(dlg.FileName, json)

            MessageBox.Show("Kalibrierung gespeichert.")

            DialogResult = True

            Close()

        End Sub

#End Region

    End Class
End Namespace