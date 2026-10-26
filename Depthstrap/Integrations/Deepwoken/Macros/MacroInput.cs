// SPDX-FileCopyrightText: 2026 Depthstrap
//
// SPDX-License-Identifier: MPL-2.0

using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace Depthstrap.Integrations.Deepwoken.Macros
{
    /// <summary>
    /// Sends keyboard and mouse input with SendInput. Keys go out as scan codes because
    /// Roblox ignores plain virtual-key events.
    /// </summary>
    [SupportedOSPlatform("windows")]
    internal static class MacroInput
    {
        private const uint INPUT_MOUSE = 0;
        private const uint INPUT_KEYBOARD = 1;

        private const uint KEYEVENTF_KEYUP = 0x0002;
        private const uint KEYEVENTF_UNICODE = 0x0004;
        private const uint KEYEVENTF_SCANCODE = 0x0008;

        private const uint MOUSEEVENTF_MOVE = 0x0001;
        private const uint MOUSEEVENTF_LEFTDOWN = 0x0002;
        private const uint MOUSEEVENTF_LEFTUP = 0x0004;
        private const uint MOUSEEVENTF_RIGHTDOWN = 0x0008;
        private const uint MOUSEEVENTF_RIGHTUP = 0x0010;
        private const uint MOUSEEVENTF_VIRTUALDESK = 0x4000;
        private const uint MOUSEEVENTF_ABSOLUTE = 0x8000;

        private const uint MAPVK_VK_TO_VSC = 0;

        private const int SM_SWAPBUTTON = 23;
        private const int SM_XVIRTUALSCREEN = 76;
        private const int SM_YVIRTUALSCREEN = 77;
        private const int SM_CXVIRTUALSCREEN = 78;
        private const int SM_CYVIRTUALSCREEN = 79;

        public const ushort VK_TAB = 0x09;
        public const ushort VK_RETURN = 0x0D;

        public static void KeyDown(ushort vk) => SendKey(vk, false);

        public static void KeyUp(ushort vk) => SendKey(vk, true);

        public static void Tap(ushort vk)
        {
            KeyDown(vk);
            Thread.Sleep(20);
            KeyUp(vk);
        }

        // Letter and digit virtual-key codes match their uppercase ASCII values
        public static void Tap(char key) => Tap((ushort)char.ToUpperInvariant(key));

        public static void KeyDown(char key) => KeyDown((ushort)char.ToUpperInvariant(key));

        public static void KeyUp(char key) => KeyUp((ushort)char.ToUpperInvariant(key));

        public static void TypeText(string text)
        {
            foreach (char c in text)
            {
                Send(KeyboardInput(0, c, KEYEVENTF_UNICODE), KeyboardInput(0, c, KEYEVENTF_UNICODE | KEYEVENTF_KEYUP));
                Thread.Sleep(5);
            }
        }

        public static void MoveTo(MacroPoint point)
        {
            int left = GetSystemMetrics(SM_XVIRTUALSCREEN);
            int top = GetSystemMetrics(SM_YVIRTUALSCREEN);
            int width = Math.Max(1, GetSystemMetrics(SM_CXVIRTUALSCREEN) - 1);
            int height = Math.Max(1, GetSystemMetrics(SM_CYVIRTUALSCREEN) - 1);

            int x = (point.X - left) * 65535 / width;
            int y = (point.Y - top) * 65535 / height;

            Send(MouseInput(x, y, MOUSEEVENTF_MOVE | MOUSEEVENTF_ABSOLUTE | MOUSEEVENTF_VIRTUALDESK));

            // Roblox only registers hover after a relative movement
            Send(MouseInput(0, 5, MOUSEEVENTF_MOVE));
            Send(MouseInput(0, -5, MOUSEEVENTF_MOVE));
        }

        public static void MouseDown() => Send(MouseInput(0, 0, ButtonsSwapped ? MOUSEEVENTF_RIGHTDOWN : MOUSEEVENTF_LEFTDOWN));

        public static void MouseUp() => Send(MouseInput(0, 0, ButtonsSwapped ? MOUSEEVENTF_RIGHTUP : MOUSEEVENTF_LEFTUP));

        public static void Click()
        {
            MouseDown();
            Thread.Sleep(15);
            MouseUp();
        }

        public static void ClickAt(MacroPoint point)
        {
            MoveTo(point);
            Thread.Sleep(15);
            Click();
        }

        public static MacroPoint ScreenCenter => new(GetSystemMetrics(0) / 2, GetSystemMetrics(1) / 2);

        public static MacroPoint CursorPosition => GetCursorPos(out POINT p) ? new MacroPoint(p.X, p.Y) : default;

        private static bool ButtonsSwapped => GetSystemMetrics(SM_SWAPBUTTON) != 0;

        private static void SendKey(ushort vk, bool up)
        {
            ushort scan = (ushort)MapVirtualKey(vk, MAPVK_VK_TO_VSC);
            Send(KeyboardInput(0, scan, KEYEVENTF_SCANCODE | (up ? KEYEVENTF_KEYUP : 0)));
        }

        private static void Send(params INPUT[] inputs) => _ = SendInput((uint)inputs.Length, inputs, Marshal.SizeOf<INPUT>());

        private static INPUT KeyboardInput(ushort vk, ushort scan, uint flags) => new()
        {
            type = INPUT_KEYBOARD,
            u = new InputUnion { ki = new KEYBDINPUT { wVk = vk, wScan = scan, dwFlags = flags } }
        };

        private static INPUT MouseInput(int dx, int dy, uint flags) => new()
        {
            type = INPUT_MOUSE,
            u = new InputUnion { mi = new MOUSEINPUT { dx = dx, dy = dy, dwFlags = flags } }
        };

        [StructLayout(LayoutKind.Sequential)]
        private struct INPUT
        {
            public uint type;
            public InputUnion u;
        }

        [StructLayout(LayoutKind.Explicit)]
        private struct InputUnion
        {
            [FieldOffset(0)] public MOUSEINPUT mi;
            [FieldOffset(0)] public KEYBDINPUT ki;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MOUSEINPUT
        {
            public int dx;
            public int dy;
            public uint mouseData;
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct KEYBDINPUT
        {
            public ushort wVk;
            public ushort wScan;
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT { public int X, Y; }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);

        [DllImport("user32.dll")]
        private static extern uint MapVirtualKey(uint uCode, uint uMapType);

        [DllImport("user32.dll")]
        private static extern int GetSystemMetrics(int nIndex);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetCursorPos(out POINT lpPoint);
    }

    /// <summary>A screen position in pixels, stored in settings as "X Y".</summary>
    internal readonly record struct MacroPoint(int X, int Y)
    {
        public bool IsSet => X != 0 || Y != 0;

        public override string ToString() => $"{X} {Y}";

        public static MacroPoint Parse(string? value)
        {
            var parts = (value ?? "").Split([' ', ','], StringSplitOptions.RemoveEmptyEntries);

            return parts.Length == 2
                && int.TryParse(parts[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out int x)
                && int.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out int y)
                ? new MacroPoint(x, y)
                : default;
        }
    }
}
