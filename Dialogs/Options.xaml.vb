
Namespace Dialogs
    Public Class OptionsWindow

        Public Sub New()

            InitializeComponent()

            LoadSettings()

        End Sub

        Private Sub LoadSettings()

            chkDecimalDisplay.IsChecked =
            My.Settings.DecimalDisplay

        End Sub

        Private Sub SaveSettings()

            My.Settings.DecimalDisplay =
            chkDecimalDisplay.IsChecked.GetValueOrDefault

            My.Settings.Save()

        End Sub

        Private Sub btnSave_Click(
        sender As Object,
        e As RoutedEventArgs)

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