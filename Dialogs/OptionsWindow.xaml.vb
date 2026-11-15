Imports System.Windows.Media

Namespace Dialogs
    Public Class OptionsWindow

        Private SelectedFillColor As Color
        Private SelectedStrokeColor As Color

        Public Sub New()

            InitializeComponent()

            LoadSettings()

        End Sub

        Private Sub LoadSettings()

            chkDecimalDisplay.IsChecked =
            My.Settings.DecimalDisplay

            txtMarkerDiameter.Text =
                My.Settings.SearchMarkerDiameter.ToString()

            SelectedFillColor =
                My.Settings.SearchMarkerFillColor

            SelectedStrokeColor =
                My.Settings.SearchMarkerStrokeColor

            txtMarkerStrokeThickness.Text =
                My.Settings.SearchMarkerStrokeThickness.ToString()

            UpdateColorPreviews()

        End Sub

        Private Sub UpdateColorPreviews()

            borderFillColor.Background =
                New SolidColorBrush(SelectedFillColor)

            borderStrokeColor.Background =
                New SolidColorBrush(SelectedStrokeColor)

        End Sub

        Private Sub btnChooseFillColor_Click(
                sender As Object,
                e As RoutedEventArgs)

            Dim chosen = ShowColorDialog(SelectedFillColor)

            If chosen.HasValue Then

                SelectedFillColor = chosen.Value

                UpdateColorPreviews()

            End If

        End Sub

        Private Sub btnChooseStrokeColor_Click(
                sender As Object,
                e As RoutedEventArgs)

            Dim chosen = ShowColorDialog(SelectedStrokeColor)

            If chosen.HasValue Then

                SelectedStrokeColor = chosen.Value

                UpdateColorPreviews()

            End If

        End Sub

        Private Function ShowColorDialog(initialColor As Color) As Color?

            Using dlg As New System.Windows.Forms.ColorDialog()

                dlg.FullOpen = True

                dlg.Color = System.Drawing.Color.FromArgb(
                    initialColor.A,
                    initialColor.R,
                    initialColor.G,
                    initialColor.B)

                If dlg.ShowDialog() = System.Windows.Forms.DialogResult.OK Then

                    Return Color.FromArgb(
                        dlg.Color.A,
                        dlg.Color.R,
                        dlg.Color.G,
                        dlg.Color.B)

                End If

            End Using

            Return Nothing

        End Function

        Private Function ValidateSettings() As Boolean

            Dim diameter As Double
            Dim strokeThickness As Double

            If Not Double.TryParse(txtMarkerDiameter.Text, diameter) OrElse diameter <= 0 Then

                MessageBox.Show("Bitte einen gültigen Durchmesser für den SearchMarker eingeben.")
                Return False

            End If

            If Not Double.TryParse(txtMarkerStrokeThickness.Text, strokeThickness) OrElse strokeThickness < 0 Then

                MessageBox.Show("Bitte eine gültige Umrissbreite für den SearchMarker eingeben.")
                Return False

            End If

            Return True

        End Function

        Private Sub SaveSettings()

            My.Settings.DecimalDisplay =
            chkDecimalDisplay.IsChecked.GetValueOrDefault

            My.Settings.SearchMarkerDiameter =
                Double.Parse(txtMarkerDiameter.Text)

            My.Settings.SearchMarkerFillColor =
                SelectedFillColor

            My.Settings.SearchMarkerStrokeColor =
                SelectedStrokeColor

            My.Settings.SearchMarkerStrokeThickness =
                Double.Parse(txtMarkerStrokeThickness.Text)

            My.Settings.Save()

        End Sub

        Private Sub btnSave_Click(
        sender As Object,
        e As RoutedEventArgs)

            If Not ValidateSettings() Then Return

            SaveSettings()

            MessageBox.Show(
            "Einstellungen gespeichert.",
            "Optionen",
            MessageBoxButton.OK,
            MessageBoxImage.Information)

        End Sub

        Private Sub btnSaveClose_Click(
        sender As Object,
        e As RoutedEventArgs)

            If Not ValidateSettings() Then Return

            SaveSettings()

            DialogResult = True

            Close()

        End Sub

        Private Sub btnCancel_Click(
        sender As Object,
        e As RoutedEventArgs)

            Close()

        End Sub

    End Class
End Namespace