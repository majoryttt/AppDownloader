using System.Windows.Controls;

namespace AppDownloader.Controls
{
    public partial class AppsList : UserControl
    {
        public AppsList()
        {
            InitializeComponent();
        }

        // Public properties with distinct names
        public TextBlock ChromeStatusBlock => ChromeStatus;
        public CheckBox ChromeCheckBoxControl => ChromeCheckBox;
        public TextBlock DiscordStatusBlock => DiscordStatus;
        public CheckBox DiscordCheckBoxControl => DiscordCheckBox;
        public TextBlock TelegramStatusBlock => TelegramStatus;
        public CheckBox TelegramCheckBoxControl => TelegramCheckBox;
        public TextBlock SteamStatusBlock => SteamStatus;
        public CheckBox SteamCheckBoxControl => SteamCheckBox;
        public TextBlock EpicStatusBlock => EpicStatus;
        public CheckBox EpicCheckBoxControl => EpicCheckBox;
        public TextBlock DeepLStatusBlock => DeepLStatus;
        public CheckBox DeepLCheckBoxControl => DeepLCheckBox;
        public TextBlock NekoBoxStatusBlock => NekoBoxStatus;
        public CheckBox NekoBoxCheckBoxControl => NekoBoxCheckBox;
        public TextBlock PowerToysStatusBlock => PowerToysStatus;
        public CheckBox PowerToysCheckBoxControl => PowerToysCheckBox;
        public TextBlock JetBrainsToolboxStatusBlock => JetBrainsToolboxStatus;
        public CheckBox JetBrainsToolboxCheckBoxControl => JetBrainsToolboxCheckBox;
        public TextBlock SteelSeriesGgStatusBlock => SteelSeriesGgStatus;
        public CheckBox SteelSeriesGgCheckBoxControl => SteelSeriesGgCheckBox;
        public TextBlock MaterialgramStatusBlock => MaterialgramStatus;
        public CheckBox MaterialgramCheckBoxControl => MaterialgramCheckBox;
    }
}