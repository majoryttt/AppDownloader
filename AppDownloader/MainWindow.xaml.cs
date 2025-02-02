using System.Windows;
using System.Windows.Input;
using AppDownloader.Apps;
using System.Windows.Controls;

namespace AppDownloader
{
    public partial class MainWindow : Window
    {
        private readonly Dictionary<string, (IApp App, TextBlock Status, CheckBox Checkbox)> _apps;

        public MainWindow()
        {
            InitializeComponent();
            _apps = new Dictionary<string, (IApp, TextBlock, CheckBox)>
            {
                { "Chrome", (new Chrome(), AppsList.ChromeStatusBlock, AppsList.ChromeCheckBoxControl) },
                { "Discord", (new Discord(), AppsList.DiscordStatusBlock, AppsList.DiscordCheckBoxControl) },
                { "Telegram", (new Telegram(), AppsList.TelegramStatusBlock, AppsList.TelegramCheckBoxControl) },
                { "Steam", (new Steam(), AppsList.SteamStatusBlock, AppsList.SteamCheckBoxControl) },
                { "Epic Games", (new EpicGames(), AppsList.EpicStatusBlock, AppsList.EpicCheckBoxControl) },
                { "DeepL", (new DeepL(), AppsList.DeepLStatusBlock, AppsList.DeepLCheckBoxControl) },
                { "NekoBox", (new NekoBox(), AppsList.NekoBoxStatusBlock, AppsList.NekoBoxCheckBoxControl) },
                { "PowerToys", (new PowerToys(), AppsList.PowerToysStatusBlock, AppsList.PowerToysCheckBoxControl) },
                { "JetBrains Toolbox", (new JetBrainsToolbox(), AppsList.JetBrainsToolboxStatusBlock, AppsList.JetBrainsToolboxCheckBoxControl) },
                { "SteelSeries GG", (new SteelSeriesGG(), AppsList.SteelSeriesGgStatusBlock, AppsList.SteelSeriesGgCheckBoxControl) },
                { "Materialgram", (new Materialgram(), AppsList.MaterialgramStatusBlock, AppsList.MaterialgramCheckBoxControl) }
            };
        }

        private void DragZone_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void MaximizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private async void CheckStatus_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var statusTasks = _apps.Select(app => Task.Run(() =>
                {
                    Dispatcher.Invoke(() => app.Value.Status.Text = "Checking... 0%");
                    var result = app.Value.App.FindProgramExecutable($"{app.Key}.exe",
                        progress => Dispatcher.Invoke(() => app.Value.Status.Text = $"Checking... {progress}%"));
                    Dispatcher.Invoke(() => app.Value.Status.Text = result ? "Installed" : "Not Installed");
                }));

                await Task.WhenAll(statusTasks);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error checking status: {ex.Message}");
            }
        }

        private async void InstallSelected_Click(object sender, RoutedEventArgs e)
        {
            var installTasks = _apps
                .Where(x => x.Value.Checkbox.IsChecked == true && !x.Value.App.IsInstalled())
                .Select(app => InstallApp(app.Value.App, app.Value.Status));

            await Task.WhenAll(installTasks);
        }

        private async Task InstallApp(IApp app, TextBlock status)
        {
            try
            {
                status.Text = "Installing...";
                await Task.Run(() =>
                {
                    app.Download();
                    app.Install();
                });
                status.Text = "Installed";
            }
            catch (Exception ex)
            {
                status.Text = $"Error: {ex.Message}";
            }
        }

        private void SelectNotInstalled_Click(object sender, RoutedEventArgs e)
        {
            foreach (var app in _apps)
            {
                if (app.Value.Status.Text == "Not Installed")
                {
                    app.Value.Checkbox.IsChecked = true;
                }
            }
        }
    }
}
