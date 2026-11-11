Imports System.Reflection
Imports System.Windows.Media.Imaging

Namespace Dialogs
    Public Class AboutWindow

        Public Sub New()

            InitializeComponent()

            LoadVersion()

            imgLogo.Source =
            New BitmapImage(
                New Uri(
                    "pack://application:,,,/Assets/inoMaps_logo.png"))

        End Sub

        Private Sub LoadVersion()

            Dim version =
            Assembly.GetExecutingAssembly().
            GetName().
            Version

            txtVersion.Text =
            $"Version {version}"

        End Sub

        Private Sub Close_Click(
        sender As Object,
        e As RoutedEventArgs)

            Me.Close()

        End Sub

    End Class
End Namespace