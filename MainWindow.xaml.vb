Imports System.IO
Imports System.Windows.Input
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

        If e.Delta > 0 Then
            CurrentZoom *= 1.1
        Else
            CurrentZoom /= 1.1
        End If

        ZoomTransform.ScaleX = CurrentZoom

        ZoomTransform.ScaleY = CurrentZoom

    End Sub

    Private Sub MapImage_MouseMove(sender As Object, e As MouseEventArgs)

        If CurrentMap Is Nothing Then Exit Sub

        Dim p = e.GetPosition(MapImage)

        Dim scaledX = p.X * (ImagePixelWidth / MapImage.ActualWidth)

        Dim scaledY = p.Y * (ImagePixelHeight / MapImage.ActualHeight)

        Dim geo = BilinearConverter.PixelToGeo(
                CurrentMap,
                scaledX,
                scaledY,
                ImagePixelWidth,
                ImagePixelHeight)


        txtPosition.Text = $"X={scaledX:0} Y={scaledY:0} " & $"Lon={geo.Longitude:F6} " & $"Lat={geo.Latitude:F6}"

    End Sub

    Private Sub MapImage_MouseLeftButtonDown(sender As Object, e As MouseButtonEventArgs)

        If CurrentMap Is Nothing Then Exit Sub

        Dim p = e.GetPosition(MapImage)

        Dim scaledX = p.X

        Dim scaledY = p.Y

        Dim geo = BilinearConverter.PixelToGeo(
                CurrentMap,
                scaledX,
                scaledY,
                ImagePixelWidth,
                ImagePixelHeight)

        txtLongitude.Text = geo.Longitude.ToString("F6")

        txtLatitude.Text = geo.Latitude.ToString("F6")

    End Sub


    Private Sub LoadImage_Click(sender As Object, e As RoutedEventArgs)

        Dim dlg As New OpenFileDialog

        dlg.Filter = "Bilddateien|*.jpg;*.jpeg;*.png;*.bmp;*.tif"

        If dlg.ShowDialog <> True Then Return

        CurrentImageFile = dlg.FileName

        MapImage.Source = New BitmapImage(New Uri(CurrentImageFile))

    End Sub

    Private Sub Calibrate_Click(sender As Object, e As RoutedEventArgs)

        If String.IsNullOrEmpty(CurrentImageFile) Then

            MessageBox.Show("Zuerst eine Bilddatei laden.")

            Return

        End If

        Dim dlg As New Dialogs.CalibrationWindow(CurrentImageFile)

        dlg.ShowDialog()

    End Sub

    Private Sub MainWindow_Initialized(sender As Object, e As EventArgs) Handles Me.Initialized
        BuildRecentMapMenu()
        If My.Settings.LastMap <> "" Then
            If File.Exists(My.Settings.LastMap) Then
                LoadMapData(My.Settings.LastMap)
            End If
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
End Class