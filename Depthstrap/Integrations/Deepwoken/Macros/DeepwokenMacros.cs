// SPDX-FileCopyrightText: 2026 Depthstrap
//
// SPDX-License-Identifier: MPL-2.0

using System.Runtime.Versioning;

namespace Depthstrap.Integrations.Deepwoken.Macros
{
    /// <summary>
    /// The individual macro routines. Training macros loop until stopped; the rest run once.
    /// Timings follow the in-game training and menu animations.
    /// </summary>
    [SupportedOSPlatform("windows")]
    internal static class DeepwokenMacros
    {
        private static Settings Settings => App.Settings.Prop;

        // Agility: sprint on the treadmill, tapping Q to keep stamina going
        public static void Agility(MacroContext ctx)
        {
            while (true)
            {
                ctx.Click();
                ctx.Wait(50);
                ctx.Hold('W');

                for (int i = 0; i < 11; i++)
                {
                    ctx.Tap('Q');
                    ctx.Wait(2500);
                }

                ctx.Release('W');
                ctx.Wait(1000);
            }
        }

        // Fortitude (boulder): start lifting, then put down and pick up again
        public static void Fortitude(MacroContext ctx)
        {
            while (true)
            {
                ctx.Click();
                ctx.Wait(25000);
                ctx.Tap('E');
                ctx.Wait(30000);
                ctx.Tap('E');
                ctx.Wait(500);
            }
        }

        // Willpower: click to keep reading
        public static void Willpower(MacroContext ctx)
        {
            while (true)
            {
                ctx.Click();
                ctx.Wait(45000);
            }
        }

        public static void Eat(MacroContext ctx)
        {
            var search = MacroPoint.Parse(Settings.MacroEatSearchPos);
            var food = MacroPoint.Parse(Settings.MacroEatFoodPos);
            if (!Require(search, food) || string.IsNullOrWhiteSpace(Settings.MacroEatFood))
                return;

            ctx.Tap(MacroInput.VK_TAB);
            ctx.Wait(50);
            ctx.ClickAt(search);
            ctx.Type(Settings.MacroEatFood);
            ctx.Wait(30);
            ctx.ClickAt(food);
            ctx.Wait(30);
            ctx.ClickAt(MacroInput.ScreenCenter);
        }

        public static void Sell(MacroContext ctx)
        {
            var item = MacroPoint.Parse(Settings.MacroSellItemPos);
            var confirm = MacroPoint.Parse(Settings.MacroSellConfirmPos);
            if (!Require(item, confirm))
                return;

            for (int i = 0; i < Math.Max(1, Settings.MacroSellRepeats); i++)
            {
                ctx.ClickAt(item);
                ctx.Wait(20);
                ctx.ClickAt(confirm);
                ctx.Wait(150);
                ctx.Click();
                ctx.Wait(150);
                ctx.Click();
                ctx.Wait(250);
            }
        }

        public static void Buy(MacroContext ctx)
        {
            var sliderStart = MacroPoint.Parse(Settings.MacroBuySliderStartPos);
            var sliderEnd = MacroPoint.Parse(Settings.MacroBuySliderEndPos);
            var submit = MacroPoint.Parse(Settings.MacroBuySubmitPos);
            if (!Require(sliderStart, sliderEnd, submit))
                return;

            for (int i = 0; i < Math.Max(1, Settings.MacroBuyRepeats); i++)
            {
                ctx.Tap('E');
                ctx.Wait(30);
                Drag(ctx, sliderStart, sliderEnd);
                ctx.Wait(75);
                ctx.Click();
                ctx.Wait(75);
                ctx.ClickAt(submit);
                ctx.Wait(250);
            }
        }

        public static void Bank(MacroContext ctx)
        {
            var search = MacroPoint.Parse(Settings.MacroBankSearchPos);
            var item = MacroPoint.Parse(Settings.MacroBankItemPos);
            var vow = MacroPoint.Parse(Settings.MacroBankVowPos);
            if (!Require(search, item) || string.IsNullOrWhiteSpace(Settings.MacroBankItem))
                return;

            for (int i = 0; i < Math.Max(1, Settings.MacroBankRepeats); i++)
            {
                ctx.ClickAt(search);
                ctx.Type(Settings.MacroBankItem);
                ctx.Wait(30);
                Drag(ctx, item, MacroInput.ScreenCenter);

                if (vow.IsSet)
                {
                    ctx.Wait(500);
                    ctx.ClickAt(vow);
                }

                ctx.Wait(2000);
            }
        }

        public static void DropNotes(MacroContext ctx)
        {
            var note = MacroPoint.Parse(Settings.MacroNotesNotePos);
            var sliderStart = MacroPoint.Parse(Settings.MacroNotesSliderStartPos);
            var sliderEnd = MacroPoint.Parse(Settings.MacroNotesSliderEndPos);
            var confirm = MacroPoint.Parse(Settings.MacroNotesConfirmPos);
            if (!Require(note, sliderStart, sliderEnd, confirm))
                return;

            for (int i = 0; i < Math.Max(1, Settings.MacroNotesRepeats); i++)
            {
                ctx.ClickAt(note);
                ctx.Wait(20);
                Drag(ctx, sliderStart, sliderEnd);
                ctx.Wait(100);
                ctx.ClickAt(confirm);
                ctx.Wait(100);
            }
        }

        private static void Drag(MacroContext ctx, MacroPoint from, MacroPoint to)
        {
            ctx.MoveTo(from);
            ctx.Wait(50);
            ctx.MouseDown();

            try
            {
                ctx.Wait(75);
                ctx.MoveTo(to);
                ctx.Wait(20);
            }
            finally
            {
                MacroContext.MouseUp();
            }
        }

        private static bool Require(params MacroPoint[] points)
        {
            if (points.All(p => p.IsSet))
                return true;

            App.Logger.Warn("Macro skipped: not all screen positions are set");
            return false;
        }
    }
}
