Namespace Dialogs
    Public Class LicenseWindow

        Public Sub New()

            InitializeComponent()

            LoadLicenseText()

        End Sub

        Private Sub LoadLicenseText()

            Dim licensePath =
                IO.Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    "LICENSE")

            If IO.File.Exists(licensePath) Then

                txtLicense.Text =
                    IO.File.ReadAllText(licensePath)

            Else

                txtLicense.Text =
                    "Lizenzdatei (LICENSE) wurde nicht gefunden."

            End If

        End Sub

        Private Sub Close_Click(
                sender As Object,
                e As RoutedEventArgs)

            Me.Close()

        End Sub

    End Class
End Namespace
