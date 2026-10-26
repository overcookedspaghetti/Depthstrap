// SPDX-FileCopyrightText: 2026 Depthstrap
//
// SPDX-License-Identifier: MPL-2.0

using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace Depthstrap.Integrations.Deepwoken.Macros
{
    /// <summary>
    /// Watches macro hotkeys while Roblox is focused. Pressing a macro's hotkey starts it,
    /// pressing it again stops it, and starting another macro stops the current one.
    /// A rising beep means a macro started, a falling beep means it stopped.
    /// </summary>
    [SupportedOSPlatform("windows")]
    internal sealed class MacroManager : IDisposable
    {
        private static readonly TimeSpan PingCooldown = TimeSpan.FromSeconds(5);
        private const ushort VK_M = 'M';

        private readonly int _robloxPid;
        private readonly ActivityWatcher? _activityWatcher;
        private readonly CancellationTokenSource _shutdown = new();
        private readonly Dictionary<MacroHotkey, (Action<MacroContext> Run, bool Loops)> _macros = [];
        private readonly CharismaHelper? _charisma;
        private readonly MacroHotkey _charismaHotkey;
        private readonly MacroHotkey _pingHotkey;
        private readonly bool _flashMap;
        private readonly Lock _lock = new();

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2213", Justification = "Owned and disposed by the macro task in Run")]
        private CancellationTokenSource? _current;
        private MacroHotkey _currentKey;
        private DateTime _lastPing = DateTime.MinValue;
        private DateTime _ignoreMapKeyUntil = DateTime.MinValue;

        public MacroManager(int robloxPid, ActivityWatcher? activityWatcher)
        {
            _robloxPid = robloxPid;
            _activityWatcher = activityWatcher;

            var s = App.Settings.Prop;

            Register(s.MacroAgilityHotkey, DeepwokenMacros.Agility, true);
            Register(s.MacroFortitudeHotkey, DeepwokenMacros.Fortitude, true);
            Register(s.MacroWillpowerHotkey, DeepwokenMacros.Willpower, true);
            Register(s.MacroEatHotkey, DeepwokenMacros.Eat, false);
            Register(s.MacroSellHotkey, DeepwokenMacros.Sell, false);
            Register(s.MacroBuyHotkey, DeepwokenMacros.Buy, false);
            Register(s.MacroBankHotkey, DeepwokenMacros.Bank, false);
            Register(s.MacroNotesHotkey, DeepwokenMacros.DropNotes, false);

            _pingHotkey = DiscordPing.IsValidWebhook(s.DiscordPingWebhookUrl) ? s.DiscordPingHotkey : MacroHotkey.None;
            _charismaHotkey = s.MacroCharismaHotkey;
            _flashMap = s.MacroFlashMap;

            if (_charismaHotkey != MacroHotkey.None)
                _charisma = new CharismaHelper(IsRobloxFocused);

            new Thread(PollKeys) { IsBackground = true, Name = "Deepwoken Macros" }.Start();

            App.Logger.Info($"Macros ready ({_macros.Count} hotkeys, charisma: {_charisma != null}, flash map: {_flashMap}, ping: {_pingHotkey != MacroHotkey.None})");
        }

        private void Register(MacroHotkey key, Action<MacroContext> run, bool loops)
        {
            if (key == MacroHotkey.None)
                return;

            if (!_macros.TryAdd(key, (run, loops)))
                App.Logger.Warn($"Hotkey {key} is assigned to more than one macro, keeping the first");
        }

        private bool IsRobloxFocused()
        {
            _ = GetWindowThreadProcessId(GetForegroundWindow(), out uint pid);
            return pid == _robloxPid;
        }

        private static bool IsDown(int vk) => (GetAsyncKeyState(vk) & 0x8000) != 0;

        private void PollKeys()
        {
            var keys = _macros.Keys.Append(_charismaHotkey).Append(_pingHotkey)
                .Where(k => k != MacroHotkey.None).Distinct().ToArray();
            var wasDown = new Dictionary<MacroHotkey, bool>();
            bool mapWasDown = false;

            while (!_shutdown.IsCancellationRequested)
            {
                bool focused = IsRobloxFocused();

                foreach (var key in keys)
                {
                    bool down = IsDown((int)key);
                    if (down && !wasDown.GetValueOrDefault(key) && focused)
                        OnHotkey(key);
                    wasDown[key] = down;
                }

                if (_flashMap)
                {
                    bool mapDown = IsDown(VK_M);
                    if (mapWasDown && !mapDown && focused && DateTime.UtcNow > _ignoreMapKeyUntil)
                    {
                        // M released: close the map again
                        _ignoreMapKeyUntil = DateTime.UtcNow.AddMilliseconds(500);
                        _ = Task.Run(() =>
                        {
                            Thread.Sleep(200);
                            if (IsRobloxFocused())
                                MacroInput.Tap(VK_M);
                        });
                    }
                    mapWasDown = mapDown;
                }

                Thread.Sleep(15);
            }
        }

        private void OnHotkey(MacroHotkey key)
        {
            if (key == _pingHotkey)
            {
                if (DateTime.UtcNow - _lastPing > PingCooldown)
                {
                    _lastPing = DateTime.UtcNow;
                    _ = DiscordPing.SendAsync(_robloxPid, _activityWatcher, _shutdown.Token);
                }
                return;
            }

            if (key == _charismaHotkey && _charisma != null)
            {
                _charisma.Enabled = !_charisma.Enabled;
                Beep(_charisma.Enabled);
                App.Logger.Info($"Charisma helper {(_charisma.Enabled ? "enabled" : "disabled")}");
                return;
            }

            if (!_macros.TryGetValue(key, out var macro))
                return;

            lock (_lock)
            {
                bool wasRunningThis = _current != null && _currentKey == key;
                StopCurrent();

                if (wasRunningThis)
                {
                    Beep(false);
                    return;
                }

                var cts = CancellationTokenSource.CreateLinkedTokenSource(_shutdown.Token);
                _current = cts;
                _currentKey = key;
                Beep(true);

                _ = Task.Run(() => Run(key, macro.Run, macro.Loops, cts));
            }
        }

        private void Run(MacroHotkey key, Action<MacroContext> run, bool loops, CancellationTokenSource cts)
        {
            var ctx = new MacroContext(IsRobloxFocused, loops, cts.Token);

            try
            {
                App.Logger.Info($"Macro on {key} started");
                run(ctx);
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                App.Logger.Error($"Macro on {key} failed: {ex.Message}");
            }
            finally
            {
                ctx.ReleaseAll();

                lock (_lock)
                {
                    if (_current == cts)
                    {
                        _current = null;
                        _currentKey = MacroHotkey.None;
                    }
                }

                cts.Dispose();
                App.Logger.Info($"Macro on {key} finished");
            }
        }

        private void StopCurrent()
        {
            _current?.Cancel();
            _current = null;
            _currentKey = MacroHotkey.None;
        }

        private static void Beep(bool started) => MessageBeep(started ? 0x40u : 0x30u);

        public void Dispose()
        {
            lock (_lock)
                StopCurrent();

            _shutdown.Cancel();
            _charisma?.Dispose();
            _shutdown.Dispose();
        }

        [DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(int vKey);

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool MessageBeep(uint uType);
    }
}
