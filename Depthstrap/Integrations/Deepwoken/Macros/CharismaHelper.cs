// SPDX-FileCopyrightText: 2026 Depthstrap
//
// SPDX-License-Identifier: MPL-2.0

using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace Depthstrap.Integrations.Deepwoken.Macros
{
    /// <summary>
    /// Charisma training: when you start typing one of the training lines in chat,
    /// finishes the line, sends it and clicks to continue the conversation.
    /// </summary>
    [SupportedOSPlatform("windows")]
    internal sealed class CharismaHelper : IDisposable
    {
        // Typed prefix -> full line the training book asks for (chat is case-insensitive)
        private static readonly (string Prefix, string Line)[] Lines =
        [
            ("somew", "someweatherwerehavinghuh"),
            ("somet", "sometimesihavereallydeepthoughtsaboutlifeandstuff"),
            ("soh", "sohowswork"),
            ("sow", "sowhatskeepingyoubusythesedays"),
            ("wow", "wowthisbreezeisgreatright"),
            ("hey", "heyhivekincanibugyouforamoment"),
            ("mew", "mewowisthatthelatestfelinorfashion"),
            ("you", "youeverbeentoacanorrestaurantthefoodsprettyhowlright"),
        ];

        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private const int LLKHF_INJECTED = 0x10;
        private const uint WM_QUIT = 0x0012;
        private const uint VK_BACK = 0x08;
        private const uint VK_ESCAPE = 0x1B;

        private readonly Func<bool> _isRobloxFocused;
        private readonly StringBuilder _typed = new();
        private Thread? _hookThread;
        private uint _hookThreadId;
        private IntPtr _hookHandle;
        private LowLevelKeyboardProc? _hookProc;
        private volatile bool _typing;

        public bool Enabled { get; set; }

        public CharismaHelper(Func<bool> isRobloxFocused)
        {
            _isRobloxFocused = isRobloxFocused;

            _hookThread = new Thread(HookThreadProc) { IsBackground = true, Name = "Charisma Hook" };
            _hookThread.Start();
        }

        private void HookThreadProc()
        {
            _hookThreadId = GetCurrentThreadId();
            _hookProc = HookCallback;
            _hookHandle = SetWindowsHookEx(WH_KEYBOARD_LL, _hookProc, IntPtr.Zero, 0);

            if (_hookHandle == IntPtr.Zero)
            {
                App.Logger.Error($"Failed to install charisma keyboard hook, error {Marshal.GetLastWin32Error()}");
                return;
            }

            while (GetMessage(out MSG msg, IntPtr.Zero, 0, 0) > 0)
            {
                TranslateMessage(ref msg);
                DispatchMessage(ref msg);
            }
        }

        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode < 0 || !Enabled || wParam.ToInt32() != WM_KEYDOWN)
                return CallNextHookEx(_hookHandle, nCode, wParam, lParam);

            var kb = Marshal.PtrToStructure<KBDLLHOOKSTRUCT>(lParam);

            if ((kb.flags & LLKHF_INJECTED) != 0)
                return CallNextHookEx(_hookHandle, nCode, wParam, lParam);

            // Swallow real keypresses while we're typing the line so they don't get mixed in
            if (_typing)
                return (IntPtr)1;

            if (!_isRobloxFocused())
            {
                _typed.Clear();
                return CallNextHookEx(_hookHandle, nCode, wParam, lParam);
            }

            if (kb.vkCode is >= 'A' and <= 'Z')
            {
                _typed.Append((char)(kb.vkCode + 32));
                if (_typed.Length > 64)
                    _typed.Remove(0, _typed.Length - 64);

                string typed = _typed.ToString();

                foreach (var (prefix, line) in Lines)
                {
                    if (!typed.EndsWith(prefix, StringComparison.Ordinal))
                        continue;

                    _typed.Clear();
                    _typing = true;
                    _ = Task.Run(() => Complete(line[prefix.Length..]));
                    break;
                }
            }
            else if (kb.vkCode == VK_BACK)
            {
                if (_typed.Length > 0)
                    _typed.Length--;
            }
            else if (kb.vkCode is MacroInput.VK_RETURN or VK_ESCAPE)
            {
                _typed.Clear();
            }

            return CallNextHookEx(_hookHandle, nCode, wParam, lParam);
        }

        private void Complete(string rest)
        {
            try
            {
                // Let the key that matched reach the chat box first
                Thread.Sleep(30);
                MacroInput.TypeText(rest);
                Thread.Sleep(100);
                MacroInput.Tap(MacroInput.VK_RETURN);
                Thread.Sleep(250);
                MacroInput.Click();
            }
            catch (Exception ex)
            {
                App.Logger.Error($"Charisma helper failed: {ex.Message}");
            }
            finally
            {
                _typing = false;
            }
        }

        public void Dispose()
        {
            Enabled = false;

            if (_hookHandle != IntPtr.Zero)
            {
                UnhookWindowsHookEx(_hookHandle);
                _hookHandle = IntPtr.Zero;
            }

            if (_hookThreadId != 0)
                PostThreadMessage(_hookThreadId, WM_QUIT, IntPtr.Zero, IntPtr.Zero);

            _hookThread = null;
        }

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        [StructLayout(LayoutKind.Sequential)]
        private struct KBDLLHOOKSTRUCT
        {
            public uint vkCode;
            public uint scanCode;
            public uint flags;
            public uint time;
            public nuint dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MSG
        {
            public IntPtr hwnd;
            public uint message;
            public IntPtr wParam;
            public IntPtr lParam;
            public uint time;
            public int ptX;
            public int ptY;
        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll")]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern int GetMessage(out MSG lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool TranslateMessage(ref MSG lpMsg);

        [DllImport("user32.dll")]
        private static extern IntPtr DispatchMessage(ref MSG lpMsg);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool PostThreadMessage(uint idThread, uint msg, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll")]
        private static extern uint GetCurrentThreadId();
    }
}
