Imports System.Reflection
Imports System.Windows.Media.Imaging
Imports System.Windows.Navigation
Imports System.Diagnostics

Namespace Dialogs
    Public Class AboutWindow

        Public Sub New()

            InitializeComponent()

            LoadVersion()
            LoadAboutText()

            imgLogo.Source =
            New BitmapImage(New Uri("pack://application:,,,/Assets/inoMaps_logo.png"))

        End Sub

        Private Sub LoadVersion()

            Dim version =
            Assembly.GetExecutingAssembly().
            GetName().
            Version


            txtVersion.Text =String.Format("Version {0}", version)


        End Sub

        Private Sub LoadAboutText()

            Dim lines As New List(Of String) From {
                "inoMaps",
                "",
                "Werkzeug zur Georeferenzierung historischer Karten sowie zur Auswertung genealogischer Ortsdaten.",
                "",
                String.Format("© 2026 - {0} INOPIAE GbR", Year(Now)),
                "",
                "Alle Rechte vorbehalten.",
                "",
                "Verwendete Technologien:",
                "- .NET 9",
                "- WPF",
                "- JSON",
                "",
                "Diese Software ist freie Software und steht unter den Bedingungen der",
                "GNU Affero General Public License, Version 3.0 (AGPL-3.0).",
                "Den vollständigen Lizenztext kannst du über die Schaltfläche ""AGPL 3.0"" einsehen."
            }

            txtAbout.Text = String.Join(Environment.NewLine, lines)

        End Sub

        Private Sub Hyperlink_RequestNavigate(
                sender As Object,
                e As RequestNavigateEventArgs)

            Dim psi As New ProcessStartInfo(e.Uri.AbsoluteUri)

            psi.UseShellExecute = True

            Process.Start(psi)

            e.Handled = True

        End Sub

        Private Sub ShowLicense_Click(
                sender As Object,
                e As RoutedEventArgs)

            Dim dlg As New LicenseWindow()

            dlg.Owner = Me

            dlg.ShowDialog()

        End Sub

        Private Sub Close_Click(
        sender As Object,
        e As RoutedEventArgs)

            Me.Close()

        End Sub

    End Class
End Namespace