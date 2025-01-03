using System.Windows;
using System.Windows.Input;
using AppDownloader.Apps;
using System.Windows.Controls;

namespace AppDownloader;

public partial class MainWindow : Window
{
    // Initialize apps
    private readonly Discord _discord;
    private readonly Telegram _telegram;
    private readonly Steam _steam;
    private readonly EpicGames _epicGames;
    private readonly DeepL _deepL;
    private readonly NekoBox _nekoBox;
    private readonly PowerToys _powerToys;

    public MainWindow()
    {
        // Initialize components
        InitializeComponent();
        _discord = new Discord();
        _telegram = new Telegram();
        _steam = new Steam();
        _epicGames = new EpicGames();
        _deepL = new DeepL();
        _nekoBox = new NekoBox();
        _powerToys = new PowerToys();
    }
    // Main function
    // DragZone
    private void DragZone_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        DragMove();
    }

    // MaximizeButton
    private void MaximizeButton_Click(object sender, RoutedEventArgs e)
    {
        WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
    }

    // CloseButton
    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        this.Close();
    }
    
    // Other functions
    // CheckStatus
    private async void CheckStatus_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var tasks = new[]
            {
                Task.Run(() =>
                {
                    Dispatcher.Invoke(() => DiscordStatus.Text = "Checking... 0%");
                    var result = _discord.FindProgramExecutable("Discord.exe",
                        progress => Dispatcher.Invoke(() => DiscordStatus.Text = $"Checking... {progress}%"));
                    Dispatcher.Invoke(() => DiscordStatus.Text = result ? "Installed" : "Not Installed");
                }),
                
                Task.Run(() =>
                {
                    Dispatcher.Invoke(() => TelegramStatus.Text = "Checking... 0%");
                    var result = _telegram.FindProgramExecutable("Telegram.exe",
                        progress => Dispatcher.Invoke(() => TelegramStatus.Text = $"Checking... {progress}%"));
                    Dispatcher.Invoke(() => TelegramStatus.Text = result ? "Installed" : "Not Installed");
                }),
                
                Task.Run(() =>
                {
                    Dispatcher.Invoke(() => SteamStatus.Text = "Checking... 0%");
                    var result = _steam.FindProgramExecutable("Steam.exe",
                        progress => Dispatcher.Invoke(() => SteamStatus.Text = $"Checking... {progress}%"));
                    Dispatcher.Invoke(() => SteamStatus.Text = result ? "Installed" : "Not Installed");
                }),
                
                Task.Run(() =>
                {
                    Dispatcher.Invoke(() => EpicStatus.Text = "Checking... 0%");
                    var result = _epicGames.FindProgramExecutable("EpicGamesLauncher.exe",
                        progress => Dispatcher.Invoke(() => EpicStatus.Text = $"Checking... {progress}%"));
                    Dispatcher.Invoke(() => EpicStatus.Text = result ? "Installed" : "Not Installed");
                }),
                
                Task.Run(() =>
                {
                    Dispatcher.Invoke(() => DeepLStatus.Text = "Checking... 0%");
                    var result = _deepL.FindProgramExecutable("DeepL.exe",
                        progress => Dispatcher.Invoke(() => DeepLStatus.Text = $"Checking... {progress}%"));
                    Dispatcher.Invoke(() => DeepLStatus.Text = result ? "Installed" : "Not Installed");
                }),
                
                Task.Run(() =>
                {
                    Dispatcher.Invoke(() => NekoBoxStatus.Text = "Checking... 0%");
                    var result = _nekoBox.FindProgramExecutable("nekobox.exe",
                        progress => Dispatcher.Invoke(() => NekoBoxStatus.Text = $"Checking... {progress}%"));
                    Dispatcher.Invoke(() => NekoBoxStatus.Text = result ? "Installed" : "Not Installed");
                }),
                
                Task.Run(() =>
                {
                    Dispatcher.Invoke(() => PowerToysStatus.Text = "Checking... 0%");
                    var result = _powerToys.FindProgramExecutable("PowerToys.exe",
                        progress => Dispatcher.Invoke(() => PowerToysStatus.Text = $"Checking... {progress}%"));
                    Dispatcher.Invoke(() => PowerToysStatus.Text = result ? "Installed" : "Not Installed");
                })
            };

            await Task.WhenAll(tasks);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error checking status: {ex.Message}");
        }
    }
    
    // InstallSelected
    private async void InstallSelected_Click(object sender, RoutedEventArgs e)
    {
        if (DiscordCheckBox.IsChecked == true && !_discord.IsInstalled())
        {
            try
            {
                DiscordStatus.Text = "Installing...";
                await Task.Run(() =>
                {
                    _discord.Download();
                    _discord.Install();
                });
                DiscordStatus.Text = "Installed";
            }
            catch (Exception ex)
            {
                DiscordStatus.Text = $"Error: {ex.Message}";
            }
        }
        
        if (TelegramCheckBox.IsChecked == true && !_telegram.IsInstalled())
        {
            try
            {
                TelegramStatus.Text = "Installing...";
                await Task.Run(() =>
                {
                    _telegram.Download();
                    _telegram.Install();
                });
                TelegramStatus.Text = "Installed";
            }
            catch (Exception ex)
            {
                TelegramStatus.Text = $"Error: {ex.Message}";
            }
        }

        if (SteamCheckBox.IsChecked == true && !_steam.IsInstalled())
        {
            try
            {
                SteamStatus.Text = "Installing...";
                await Task.Run(() =>
                {
                    _steam.Download();
                    _steam.Install();
                });
                SteamStatus.Text = "Installed";
            }
            catch (Exception ex)
            {
                SteamStatus.Text = $"Error: {ex.Message}";
            }
        }

        if (EpicCheckBox.IsChecked == true && !_epicGames.IsInstalled())
        {
            try
            {
                EpicStatus.Text = "Installing...";
                await Task.Run(() =>
                {
                    _epicGames.Download();
                    _epicGames.Install();
                });
                EpicStatus.Text = "Installed";
            }
            catch (Exception ex)
            {
                EpicStatus.Text = $"Error: {ex.Message}";
            }
        }

        if (DeepLCheckBox.IsChecked == true && !_deepL.IsInstalled())
        {
            try
            {
                DeepLStatus.Text = "Installing...";
                await Task.Run(() =>
                {
                    _deepL.Download();
                    _deepL.Install();
                });
                DeepLStatus.Text = "Installed";
            }
            catch (Exception ex)
            {
                DeepLStatus.Text = $"Error: {ex.Message}";
            }
        }

        if (NekoBoxCheckBox.IsChecked == true && !_nekoBox.IsInstalled())
        {
            try
            {
                NekoBoxStatus.Text = "Installing...";
                await Task.Run(() =>
                {
                    _nekoBox.Download();
                    _nekoBox.Install();
                });
                NekoBoxStatus.Text = "Installed";
            }
            catch (Exception ex)
            {
                NekoBoxStatus.Text = $"Error: {ex.Message}";
            }
        }

        if (PowerToysCheckBox.IsChecked == true && !_powerToys.IsInstalled())
        {
            try
            {
                PowerToysStatus.Text = "Installing...";
                await Task.Run(() =>
                {
                    _powerToys.Download();
                    _powerToys.Install();
                });
                PowerToysStatus.Text = "Installed";
            }
            catch (Exception ex)
            {
                PowerToysStatus.Text = $"Error: {ex.Message}";
            }
        }
    }

    // SelectNotInstalled
    private void SelectNotInstalled_Click(object sender, RoutedEventArgs e)
    {
        if (DiscordStatus.Text == "Not Installed")
            DiscordCheckBox.IsChecked = true;
        
        if (TelegramStatus.Text == "Not Installed")
            TelegramCheckBox.IsChecked = true;
        
        if (SteamStatus.Text == "Not Installed")
            SteamCheckBox.IsChecked = true;
        
        if (EpicStatus.Text == "Not Installed")
            EpicCheckBox.IsChecked = true;
        
        if (DeepLStatus.Text == "Not Installed")
            DeepLCheckBox.IsChecked = true;
        
        if (NekoBoxStatus.Text == "Not Installed")
            NekoBoxCheckBox.IsChecked = true;
        
        if (PowerToysStatus.Text == "Not Installed")
            PowerToysCheckBox.IsChecked = true;
    }
}