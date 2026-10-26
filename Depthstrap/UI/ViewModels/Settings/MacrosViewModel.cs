// SPDX-FileCopyrightText: 2026 Depthstrap
//
// SPDX-License-Identifier: MPL-2.0

using CommunityToolkit.Mvvm.Input;
using Depthstrap.Integrations.Deepwoken;
using Depthstrap.Integrations.Deepwoken.Macros;

namespace Depthstrap.UI.ViewModels.Settings
{
    internal class MacrosViewModel : NotifyPropertyChangedViewModel
    {
        private const int PickCountdownSeconds = 5;

        private static Models.Persistable.Settings S => App.Settings.Prop;

        public IAsyncRelayCommand<string> PickPositionCommand => new AsyncRelayCommand<string>(PickPosition);

        public IAsyncRelayCommand SendTestPingCommand => new AsyncRelayCommand(SendTestPing);

        public IReadOnlyList<MacroHotkey> HotkeyOptions { get; } = Enum.GetValues<MacroHotkey>();

        private string _status = "";
        public string Status
        {
            get => _status;
            private set => SetProperty(ref _status, value);
        }

        private bool _isPicking;
        public bool IsNotPicking => !_isPicking;

        private async Task PickPosition(string? target)
        {
            if (target is null || !OperatingSystem.IsWindows() || _isPicking)
                return;

            _isPicking = true;
            OnPropertyChanged(nameof(IsNotPicking));

            try
            {
                for (int i = PickCountdownSeconds; i > 0; i--)
                {
                    Status = $"Switch to Roblox and hover over the spot... {i}";
                    await Task.Delay(1000);
                }

                string position = MacroInput.CursorPosition.ToString();
                SetPosition(target, position);
                Status = $"Saved position {position}";
            }
            finally
            {
                _isPicking = false;
                OnPropertyChanged(nameof(IsNotPicking));
            }
        }

        private void SetPosition(string target, string position)
        {
            switch (target)
            {
                case nameof(EatSearchPos): EatSearchPos = position; break;
                case nameof(EatFoodPos): EatFoodPos = position; break;
                case nameof(SellItemPos): SellItemPos = position; break;
                case nameof(SellConfirmPos): SellConfirmPos = position; break;
                case nameof(BuySliderStartPos): BuySliderStartPos = position; break;
                case nameof(BuySliderEndPos): BuySliderEndPos = position; break;
                case nameof(BuySubmitPos): BuySubmitPos = position; break;
                case nameof(BankSearchPos): BankSearchPos = position; break;
                case nameof(BankItemPos): BankItemPos = position; break;
                case nameof(BankVowPos): BankVowPos = position; break;
                case nameof(NotesNotePos): NotesNotePos = position; break;
                case nameof(NotesSliderStartPos): NotesSliderStartPos = position; break;
                case nameof(NotesSliderEndPos): NotesSliderEndPos = position; break;
                case nameof(NotesConfirmPos): NotesConfirmPos = position; break;
            }
        }

        private async Task SendTestPing()
        {
            if (!OperatingSystem.IsWindows())
                return;

            if (!DiscordPing.IsValidWebhook(PingWebhookUrl))
            {
                Status = "That doesn't look like a Discord webhook URL (https://discord.com/api/webhooks/...)";
                return;
            }

            Status = "Sending test ping...";
            Status = await DiscordPing.SendAsync(null, null) ? "Test ping sent, check your Discord channel" : "Test ping failed, check the webhook URL";
        }

        public bool MacrosEnabled
        {
            get => S.MacrosEnabled;
            set { S.MacrosEnabled = value; OnPropertyChanged(); }
        }

        // Discord ping

        public static MacroHotkey PingHotkey { get => S.DiscordPingHotkey; set => S.DiscordPingHotkey = value; }
        public static string PingWebhookUrl { get => S.DiscordPingWebhookUrl; set => S.DiscordPingWebhookUrl = value.Trim(); }
        public static string PingMessage { get => S.DiscordPingMessage; set => S.DiscordPingMessage = value; }
        public static bool PingScreenshot { get => S.DiscordPingScreenshot; set => S.DiscordPingScreenshot = value; }

        // Training

        public static MacroHotkey AgilityHotkey { get => S.MacroAgilityHotkey; set => S.MacroAgilityHotkey = value; }
        public static MacroHotkey FortitudeHotkey { get => S.MacroFortitudeHotkey; set => S.MacroFortitudeHotkey = value; }
        public static MacroHotkey WillpowerHotkey { get => S.MacroWillpowerHotkey; set => S.MacroWillpowerHotkey = value; }
        public static MacroHotkey CharismaHotkey { get => S.MacroCharismaHotkey; set => S.MacroCharismaHotkey = value; }
        public static bool FlashMap { get => S.MacroFlashMap; set => S.MacroFlashMap = value; }

        // Eat

        public static MacroHotkey EatHotkey { get => S.MacroEatHotkey; set => S.MacroEatHotkey = value; }
        public static string EatFood { get => S.MacroEatFood; set => S.MacroEatFood = value; }
        public string EatSearchPos { get => S.MacroEatSearchPos; set { S.MacroEatSearchPos = value; OnPropertyChanged(); } }
        public string EatFoodPos { get => S.MacroEatFoodPos; set { S.MacroEatFoodPos = value; OnPropertyChanged(); } }

        // Sell

        public static MacroHotkey SellHotkey { get => S.MacroSellHotkey; set => S.MacroSellHotkey = value; }
        public static decimal SellRepeats { get => S.MacroSellRepeats; set => S.MacroSellRepeats = (int)value; }
        public string SellItemPos { get => S.MacroSellItemPos; set { S.MacroSellItemPos = value; OnPropertyChanged(); } }
        public string SellConfirmPos { get => S.MacroSellConfirmPos; set { S.MacroSellConfirmPos = value; OnPropertyChanged(); } }

        // Buy

        public static MacroHotkey BuyHotkey { get => S.MacroBuyHotkey; set => S.MacroBuyHotkey = value; }
        public static decimal BuyRepeats { get => S.MacroBuyRepeats; set => S.MacroBuyRepeats = (int)value; }
        public string BuySliderStartPos { get => S.MacroBuySliderStartPos; set { S.MacroBuySliderStartPos = value; OnPropertyChanged(); } }
        public string BuySliderEndPos { get => S.MacroBuySliderEndPos; set { S.MacroBuySliderEndPos = value; OnPropertyChanged(); } }
        public string BuySubmitPos { get => S.MacroBuySubmitPos; set { S.MacroBuySubmitPos = value; OnPropertyChanged(); } }

        // Bank

        public static MacroHotkey BankHotkey { get => S.MacroBankHotkey; set => S.MacroBankHotkey = value; }
        public static string BankItem { get => S.MacroBankItem; set => S.MacroBankItem = value; }
        public static decimal BankRepeats { get => S.MacroBankRepeats; set => S.MacroBankRepeats = (int)value; }
        public string BankSearchPos { get => S.MacroBankSearchPos; set { S.MacroBankSearchPos = value; OnPropertyChanged(); } }
        public string BankItemPos { get => S.MacroBankItemPos; set { S.MacroBankItemPos = value; OnPropertyChanged(); } }
        public string BankVowPos { get => S.MacroBankVowPos; set { S.MacroBankVowPos = value; OnPropertyChanged(); } }

        // Drop notes

        public static MacroHotkey NotesHotkey { get => S.MacroNotesHotkey; set => S.MacroNotesHotkey = value; }
        public static decimal NotesRepeats { get => S.MacroNotesRepeats; set => S.MacroNotesRepeats = (int)value; }
        public string NotesNotePos { get => S.MacroNotesNotePos; set { S.MacroNotesNotePos = value; OnPropertyChanged(); } }
        public string NotesSliderStartPos { get => S.MacroNotesSliderStartPos; set { S.MacroNotesSliderStartPos = value; OnPropertyChanged(); } }
        public string NotesSliderEndPos { get => S.MacroNotesSliderEndPos; set { S.MacroNotesSliderEndPos = value; OnPropertyChanged(); } }
        public string NotesConfirmPos { get => S.MacroNotesConfirmPos; set { S.MacroNotesConfirmPos = value; OnPropertyChanged(); } }
    }
}
