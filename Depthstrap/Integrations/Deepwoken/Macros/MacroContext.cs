// SPDX-FileCopyrightText: 2026 Depthstrap
//
// SPDX-License-Identifier: MPL-2.0

using System.Runtime.Versioning;

namespace Depthstrap.Integrations.Deepwoken.Macros
{
    /// <summary>
    /// Wraps input for a running macro so it can never type into another window.
    /// Looping macros pause (releasing held keys) while Roblox is unfocused; one-shot macros stop.
    /// </summary>
    [SupportedOSPlatform("windows")]
    internal sealed class MacroContext(Func<bool> isRobloxFocused, bool pauseWhenUnfocused, CancellationToken token)
    {
        private readonly HashSet<ushort> _heldKeys = [];

        public CancellationToken Token { get; } = token;

        public void Wait(int milliseconds)
        {
            var until = DateTime.UtcNow.AddMilliseconds(milliseconds);

            while (DateTime.UtcNow < until)
            {
                EnsureFocus();
                Token.WaitHandle.WaitOne(Math.Min(50, Math.Max(1, (int)(until - DateTime.UtcNow).TotalMilliseconds)));
                Token.ThrowIfCancellationRequested();
            }
        }

        public void Tap(char key)
        {
            EnsureFocus();
            MacroInput.Tap(key);
        }

        public void Tap(ushort vk)
        {
            EnsureFocus();
            MacroInput.Tap(vk);
        }

        public void Hold(char key)
        {
            EnsureFocus();
            ushort vk = char.ToUpperInvariant(key);
            MacroInput.KeyDown(vk);
            _heldKeys.Add(vk);
        }

        public void Release(char key)
        {
            ushort vk = char.ToUpperInvariant(key);
            MacroInput.KeyUp(vk);
            _heldKeys.Remove(vk);
        }

        public void Type(string text)
        {
            EnsureFocus();
            MacroInput.TypeText(text);
        }

        public void Click()
        {
            EnsureFocus();
            MacroInput.Click();
        }

        public void ClickAt(MacroPoint point)
        {
            EnsureFocus();
            MacroInput.ClickAt(point);
        }

        public void MoveTo(MacroPoint point)
        {
            EnsureFocus();
            MacroInput.MoveTo(point);
        }

        public void MouseDown()
        {
            EnsureFocus();
            MacroInput.MouseDown();
        }

        // Always allowed so a drag never leaves the button stuck down
        public static void MouseUp() => MacroInput.MouseUp();

        public void ReleaseAll()
        {
            foreach (ushort vk in _heldKeys)
                MacroInput.KeyUp(vk);

            _heldKeys.Clear();
        }

        private void EnsureFocus()
        {
            Token.ThrowIfCancellationRequested();

            if (isRobloxFocused())
                return;

            if (!pauseWhenUnfocused)
                throw new OperationCanceledException("Roblox lost focus");

            // Let go of held keys so they don't stick in other windows, then hold them again on return
            var held = _heldKeys.ToArray();
            foreach (ushort vk in held)
                MacroInput.KeyUp(vk);

            while (!isRobloxFocused())
            {
                Token.WaitHandle.WaitOne(250);
                Token.ThrowIfCancellationRequested();
            }

            foreach (ushort vk in held)
                MacroInput.KeyDown(vk);
        }
    }
}
