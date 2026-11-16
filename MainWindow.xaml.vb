Imports System.IO
Imports System.Windows.Input
Imports System.Windows.Media
Imports System.Windows.Media.Imaging
Imports inoMaps.Dialogs

Imports inoMaps.Models
Imports inoMaps.Services
Imports Microsoft.Win32

Class MainWindow

    Private CurrentMap As MapDefinition

    Private CurrentZoom As Double = 1.0

    Private ImagePixelWidth As Double

    Private ImagePixelHeight As Double

    Private CurrentImageFile As String

    Private Const MAX_RECENT_MAPS As Integer = 5

    Private IsDragging As Boolean = False

    Private DragStartPoint As Point

    Private StartHorizontalOffset As Double

    Private StartVerticalOffset As Double

    Private Sub MainWindow_Initialized_Marker()

        ApplySearchMarkerSettings()

    End Sub

    Private Sub ApplySearchMarkerSettings()

        Dim diameter = My.Settings.SearchMarkerDiameter

        SearchMarker.Width = diameter
        SearchMarker.Height = diameter

        SearchMarker.Fill =
            New SolidColorBrush(My.Settings.SearchMarkerFillColor)

        SearchMarker.Stroke =
            New SolidColorBrush(My.Settings.SearchMarkerStrokeColor)

        SearchMarker.StrokeThickness =
            My.Settings.SearchMarkerStrokeThickness

    End Sub

    Private Sub btnLoadMap_Click(sender As Object, e As RoutedEventArgs)

        Dim dlg As New OpenFileDialog()

        dlg.Filter = "Map Definition|*.json"

        If dlg.ShowDialog() <> True Then Exit Sub

        LoadMapData(dlg.FileName)
        AddRecentMap(dlg.FileName)
        My.Settings.LastMap = dlg.FileName
        My.Settings.Save()
    End Sub

    Private Sub LoadMapData(filename As String)
        CurrentMap = MapLoader.Load(filename)

        Dim imagePath = MapLoader.GetImagePath(filename, CurrentMap)

        If Not File.Exists(imagePath) Then

            MessageBox.Show("Bilddatei nicht gefunden :" & vbCrLf & imagePath)

            Return

        End If

        Dim bmp As New BitmapImage()

        bmp.BeginInit()
        bmp.UriSource = New Uri(imagePath)
        bmp.CacheOption =
        BitmapCacheOption.OnLoad
        bmp.EndInit()

        MapImage.Source = bmp

        ImagePixelWidth = bmp.PixelWidth
        ImagePixelHeight = bmp.PixelHeight

        MapCanvas.Width = ImagePixelWidth
        MapCanvas.Height = ImagePixelHeight

        SearchMarker.Visibility = Visibility.Collapsed

        Me.Title = "inoMaps - " & CurrentMap.Name


    End Sub

    Private Sub btnZoomIn_Click(sender As Object, e As RoutedEventArgs)

        CurrentZoom *= 1.2

        ZoomTransform.ScaleX = CurrentZoom

        ZoomTransform.ScaleY = CurrentZoom

    End Sub

    Private Sub btnZoomOut_Click(sender As Object, e As RoutedEventArgs)

        CurrentZoom /= 1.2

        ZoomTransform.ScaleX = CurrentZoom

        ZoomTransform.ScaleY = CurrentZoom

    End Sub

    Private Sub MapImage_MouseWheel(sender As Object, e As MouseWheelEventArgs)

        If Keyboard.Modifiers = ModifierKeys.Shift Then

            If e.Delta > 0 Then
                CurrentZoom *= 1.1
            Else
                CurrentZoom /= 1.1
            End If

            ZoomTransform.ScaleX = CurrentZoom

            ZoomTransform.ScaleY = CurrentZoom

            e.Handled = True

        Else

            Dim offset = MapScrollViewer.VerticalOffset - e.Delta

            MapScrollViewer.ScrollToVerticalOffset(offset)

            e.Handled = True

        End If

    End Sub

    Private Sub MapImage_MouseMove(sender As Object, e As MouseEventArgs)

        If CurrentMap Is Nothing Then Exit Sub

        If IsDragging Then

            Dim currentPoint =
        e.GetPosition(Me)

            Dim deltaX =
        currentPoint.X - DragStartPoint.X

            Dim deltaY =
        currentPoint.Y - DragStartPoint.Y

            MapScrollViewer.ScrollToHorizontalOffset(
        StartHorizontalOffset - deltaX)

            MapScrollViewer.ScrollToVerticalOffset(
        StartVerticalOffset - deltaY)

            Return

        End If


        Dim p = e.GetPosition(MapImage)

        Dim scaledX = p.X

        Dim scaledY = p.Y

        Dim geo = BilinearConverter.PixelToGeo(
                CurrentMap,
                scaledX,
                scaledY,
                ImagePixelWidth,
                ImagePixelHeight)


        txtPosition.Text = $"Lat={geo.Latitude:F6} Lon={geo.Longitude:F6}"
    End Sub

    Private Sub MapImage_MouseLeftButtonDown(sender As Object, e As MouseButtonEventArgs)
        If CurrentMap Is Nothing Then Exit Sub

        IsDragging = True

        DragStartPoint =
            e.GetPosition(Me)

        StartHorizontalOffset =
            MapScrollViewer.HorizontalOffset

        StartVerticalOffset =
            MapScrollViewer.VerticalOffset

        MapImage.CaptureMouse()

        Cursor = Cursors.SizeAll



        Dim p = e.GetPosition(MapImage)

        Dim scaledX = p.X

        Dim scaledY = p.Y

        Dim geo = BilinearConverter.PixelToGeo(
                CurrentMap,
                scaledX,
                scaledY,
                ImagePixelWidth,
                ImagePixelHeight)

        If My.Settings.DecimalDisplay Then

            txtLongitude.Text = geo.Longitude.ToString("F6").Replace(",", ".")

            txtLatitude.Text = geo.Latitude.ToString("F6").Replace(",", ".")

            txtFerro.Text = (geo.Longitude + 17.666667).ToString("F6").Replace(",", ".")
        Else
            txtLongitude.Text = geo.Longitude.ToString("F6")

            txtLatitude.Text = geo.Latitude.ToString("F6")

            txtFerro.Text = (geo.Longitude + 17.666667).ToString("F6")
        End If
    End Sub

    Private Sub MapImage_MouseLeftButtonUp(
    sender As Object,
    e As MouseButtonEventArgs)

        IsDragging = False

        MapImage.ReleaseMouseCapture()

        Cursor = Cursors.Arrow

    End Sub

    Private Sub LoadImage_Click(sender As Object, e As RoutedEventArgs)

        Dim dlg As New OpenFileDialog

        dlg.Filter = "Bilddateien|*.jpg;*.jpeg;*.png;*.bmp;*.tif"

        If dlg.ShowDialog <> True Then Return

        CurrentImageFile = dlg.FileName

        MapImage.Source = New BitmapImage(New Uri(CurrentImageFile))

    End Sub

    Private Sub Calibrate_Click(sender As Object, e As RoutedEventArgs)

        Dim dlg As New Dialogs.CalibrationWindow(CurrentImageFile)

        dlg.ShowDialog()

    End Sub

    Private Sub MainWindow_Initialized(sender As Object, e As EventArgs) Handles Me.Initialized
        If My.Settings.NeedsUpgrade Then
            My.Settings.Upgrade()
            My.Settings.NeedsUpgrade = False
            My.Settings.Save()
        End If

        ApplySearchMarkerSettings()

        BuildRecentMapMenu()
        If My.Settings.LastMap <> "" Then
            If File.Exists(My.Settings.LastMap) Then
                LoadMapData(My.Settings.LastMap)
            End If
        End If
    End Sub

    Private Sub Options_Click(sender As Object, e As RoutedEventArgs)

        Dim dlg As New Dialogs.OptionsWindow

        dlg.Owner = Me

        If dlg.ShowDialog() = True Then

            ApplySearchMarkerSettings()

        End If

    End Sub

    Private Sub Info_Click(sender As Object, e As RoutedEventArgs)

        Dim dlg As New AboutWindow()

        dlg.Owner = Me

        dlg.ShowDialog()

    End Sub
    Private Sub Close_Click(sender As Object, e As RoutedEventArgs)

        Application.Current.Shutdown()

    End Sub

    Private Sub AddRecentMap(filename As String)

        Dim maps As New List(Of String)

        Dim value =
            My.Settings.RecentMaps

        If Not String.IsNullOrWhiteSpace(value) Then

            maps =
                value.Split("|"c).ToList()

        End If

        maps.Remove(filename)

        maps.Insert(0, filename)

        While maps.Count > MAX_RECENT_MAPS
            maps.RemoveAt(maps.Count - 1)
        End While

        My.Settings.RecentMaps =
            String.Join("|", maps)

        My.Settings.Save()

        BuildRecentMapMenu()

    End Sub

    Private Sub BuildRecentMapMenu()

        mnuRecentMaps.Items.Clear()

        Dim value =
            My.Settings.RecentMaps

        If String.IsNullOrWhiteSpace(value) Then

            Dim item As New MenuItem()

            item.Header = "(Keine Einträge)"
            item.IsEnabled = False

            mnuRecentMaps.Items.Add(item)

            Return

        End If

        Dim maps =
            value.Split("|"c)

        For Each mapFile In maps

            If IO.File.Exists(mapFile) Then

                Dim item As New MenuItem()

                item.Header =
                    IO.Path.GetFileNameWithoutExtension(mapFile)

                item.Tag = mapFile

                AddHandler item.Click,
                    AddressOf RecentMap_Click

                mnuRecentMaps.Items.Add(item)

            End If

        Next

    End Sub

    Private Sub RecentMap_Click(
    sender As Object,
    e As RoutedEventArgs)

        Dim item =
            CType(sender, MenuItem)

        Dim filename =
            item.Tag.ToString()

        LoadMapData(filename)
        My.Settings.LastMap = filename
        My.Settings.Save()

    End Sub

    Private Sub btnSearch_Click(sender As Object, e As RoutedEventArgs)

        ' Evtl. vorhandenen Marker zurücksetzen
        SearchMarker.Visibility = Visibility.Collapsed

        If CurrentMap Is Nothing Then Exit Sub

        Dim lon As Double
        Dim lat As Double

        If Not Double.TryParse(txtLongitude.Text.Replace(".", ","), lon) Then Exit Sub
        If Not Double.TryParse(txtLatitude.Text.Replace(".", ","), lat) Then Exit Sub

        ' Geokoordinate -> Pixel
        Dim pixel = BilinearConverter.GeoToPixel(
        CurrentMap,
        lon,
        lat,
        ImagePixelWidth,
        ImagePixelHeight)

        ' Marker auf Zielpunkt setzen (Pixelkoordinaten des Bildes,
        ' die Skalierung übernimmt der ZoomTransform des Canvas)
        Canvas.SetLeft(SearchMarker, pixel.X - (SearchMarker.Width / 2))
        Canvas.SetTop(SearchMarker, pixel.Y - (SearchMarker.Height / 2))

        SearchMarker.Visibility = Visibility.Visible

        ' Position im gezoomten Bild
        Dim zoomedX = pixel.X * CurrentZoom
        Dim zoomedY = pixel.Y * CurrentZoom

        ' Punkt in die Mitte des sichtbaren Bereichs setzen
        Dim targetH =
        zoomedX - (MapScrollViewer.ViewportWidth / 2)

        Dim targetV =
        zoomedY - (MapScrollViewer.ViewportHeight / 2)

        MapScrollViewer.ScrollToHorizontalOffset(targetH)
        MapScrollViewer.ScrollToVerticalOffset(targetV)

    End Sub

    Private Sub btnDeleteMarker_Click(sender As Object, e As RoutedEventArgs)

        SearchMarker.Visibility = Visibility.Collapsed

    End Sub

    Private Sub OnlineHelp_Click(sender As Object, e As RoutedEventArgs)

        Dim url As String = "https://github.com/INOPIAE/inoMaps/blob/master/documentation/benutzung.md"

        Try
            Process.Start(New ProcessStartInfo(url) With {
                .UseShellExecute = True
            })
        Catch ex As Exception
            MessageBox.Show(
                "Die Online-Hilfe konnte nicht geöffnet werden." &
                Environment.NewLine &
                ex.Message,
                "Fehler",
                MessageBoxButton.OK,
                MessageBoxImage.Error)
        End Try

    End Sub

End Class