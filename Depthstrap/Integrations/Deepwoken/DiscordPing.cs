// SPDX-FileCopyrightText: 2026 Depthstrap
//
// SPDX-License-Identifier: MPL-2.0

using System.IO.Compression;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text.Json;

namespace Depthstrap.Integrations.Deepwoken
{
    /// <summary>
    /// Sends a message (and optionally a screenshot of the Roblox window) to a Discord webhook,
    /// e.g. to call friends for help. Triggered by the macro hotkey or the test button.
    /// </summary>
    [SupportedOSPlatform("windows")]
    internal static class DiscordPing
    {
        public static bool IsValidWebhook(string? url) =>
            Uri.TryCreate(url, UriKind.Absolute, out var uri)
            && uri.Scheme == Uri.UriSchemeHttps
            && (uri.Host is "discord.com" or "discordapp.com" or "canary.discord.com" or "ptb.discord.com")
            && uri.AbsolutePath.StartsWith("/api/webhooks/", StringComparison.Ordinal);

        public static async Task<bool> SendAsync(int? robloxPid, ActivityWatcher? activityWatcher, CancellationToken token = default)
        {
            var settings = App.Settings.Prop;

            if (!IsValidWebhook(settings.DiscordPingWebhookUrl))
                return false;

            string message = settings.DiscordPingMessage;

            var data = activityWatcher?.InGame == true ? activityWatcher.Data : null;
            if (data is not null && DeepwokenPlaces.GetLocation(data.UniverseId, data.PlaceId) is string location)
                message += $"\n📍 {location}";

            try
            {
                using var form = new MultipartFormDataContent();
                string payload = JsonSerializer.Serialize(message, DiscordPingJsonContext.Default.String);
                form.Add(new StringContent($"{{\"content\":{payload}}}", System.Text.Encoding.UTF8, "application/json"), "payload_json");

                if (settings.DiscordPingScreenshot && robloxPid is int pid && CaptureWindowPng(pid) is byte[] png)
                {
                    var image = new ByteArrayContent(png);
                    image.Headers.ContentType = new MediaTypeHeaderValue("image/png");
                    form.Add(image, "files[0]", "screenshot.png");
                }

                using var response = await App.HttpClient.PostAsync(new Uri(settings.DiscordPingWebhookUrl), form, token);

                if (!response.IsSuccessStatusCode)
                {
                    App.Logger.Error($"Discord ping failed with status {(int)response.StatusCode}");
                    return false;
                }

                App.Logger.Info("Discord ping sent");
                return true;
            }
            catch (Exception ex)
            {
                App.Logger.Error($"Discord ping failed: {ex.Message}");
                return false;
            }
        }

        private static byte[]? CaptureWindowPng(int pid)
        {
            try
            {
                using var process = Process.GetProcessById(pid);
                IntPtr hwnd = process.MainWindowHandle;

                if (hwnd == IntPtr.Zero || !GetClientRect(hwnd, out RECT rect))
                    return null;

                var origin = new POINT();
                if (!ClientToScreen(hwnd, ref origin))
                    return null;

                int width = rect.Right - rect.Left;
                int height = rect.Bottom - rect.Top;
                if (width <= 0 || height <= 0)
                    return null;

                IntPtr screenDc = GetDC(IntPtr.Zero);
                IntPtr memDc = CreateCompatibleDC(screenDc);
                IntPtr bitmap = CreateCompatibleBitmap(screenDc, width, height);
                IntPtr old = SelectObject(memDc, bitmap);

                try
                {
                    if (!BitBlt(memDc, 0, 0, width, height, screenDc, origin.X, origin.Y, SRCCOPY))
                        return null;

                    var info = new BITMAPINFOHEADER
                    {
                        biSize = (uint)Marshal.SizeOf<BITMAPINFOHEADER>(),
                        biWidth = width,
                        biHeight = -height, // top-down rows
                        biPlanes = 1,
                        biBitCount = 32
                    };

                    byte[] bgra = new byte[width * height * 4];
                    if (GetDIBits(memDc, bitmap, 0, (uint)height, bgra, ref info, 0) == 0)
                        return null;

                    return EncodePng(bgra, width, height);
                }
                finally
                {
                    SelectObject(memDc, old);
                    DeleteObject(bitmap);
                    DeleteDC(memDc);
                    _ = ReleaseDC(IntPtr.Zero, screenDc);
                }
            }
            catch (Exception ex)
            {
                App.Logger.Error($"Failed to capture Roblox window: {ex.Message}");
                return null;
            }
        }

        // Minimal RGB PNG encoder so we don't depend on a UI thread or imaging library
        private static byte[] EncodePng(byte[] bgra, int width, int height)
        {
            using var raw = new MemoryStream();
            using (var zlib = new ZLibStream(raw, CompressionLevel.Fastest, leaveOpen: true))
            {
                byte[] row = new byte[1 + width * 3];
                for (int y = 0; y < height; y++)
                {
                    row[0] = 0; // no filter
                    int src = y * width * 4;
                    for (int x = 0; x < width; x++, src += 4)
                    {
                        row[1 + x * 3] = bgra[src + 2];
                        row[2 + x * 3] = bgra[src + 1];
                        row[3 + x * 3] = bgra[src];
                    }
                    zlib.Write(row);
                }
            }

            using var png = new MemoryStream();
            png.Write([0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A]);

            byte[] header = new byte[13];
            WriteBigEndian(header, 0, (uint)width);
            WriteBigEndian(header, 4, (uint)height);
            header[8] = 8; // bit depth
            header[9] = 2; // truecolour RGB
            WriteChunk(png, "IHDR", header);
            WriteChunk(png, "IDAT", raw.ToArray());
            WriteChunk(png, "IEND", []);

            return png.ToArray();
        }

        private static void WriteChunk(Stream stream, string type, byte[] data)
        {
            byte[] buffer = new byte[4];
            WriteBigEndian(buffer, 0, (uint)data.Length);
            stream.Write(buffer);

            byte[] typeAndData = new byte[4 + data.Length];
            System.Text.Encoding.ASCII.GetBytes(type, 0, 4, typeAndData, 0);
            data.CopyTo(typeAndData, 4);
            stream.Write(typeAndData);

            WriteBigEndian(buffer, 0, Crc32(typeAndData));
            stream.Write(buffer);
        }

        private static readonly uint[] CrcTable = Enumerable.Range(0, 256).Select(n =>
        {
            uint c = (uint)n;
            for (int k = 0; k < 8; k++)
                c = (c & 1) != 0 ? 0xEDB88320 ^ (c >> 1) : c >> 1;
            return c;
        }).ToArray();

        private static uint Crc32(byte[] data)
        {
            uint crc = 0xFFFFFFFF;
            foreach (byte b in data)
                crc = CrcTable[(crc ^ b) & 0xFF] ^ (crc >> 8);
            return crc ^ 0xFFFFFFFF;
        }

        private static void WriteBigEndian(byte[] buffer, int offset, uint value)
        {
            buffer[offset] = (byte)(value >> 24);
            buffer[offset + 1] = (byte)(value >> 16);
            buffer[offset + 2] = (byte)(value >> 8);
            buffer[offset + 3] = (byte)value;
        }

        private const uint SRCCOPY = 0x00CC0020;

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT { public int Left, Top, Right, Bottom; }

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT { public int X, Y; }

        [StructLayout(LayoutKind.Sequential)]
        private struct BITMAPINFOHEADER
        {
            public uint biSize;
            public int biWidth;
            public int biHeight;
            public ushort biPlanes;
            public ushort biBitCount;
            public uint biCompression;
            public uint biSizeImage;
            public int biXPelsPerMeter;
            public int biYPelsPerMeter;
            public uint biClrUsed;
            public uint biClrImportant;
        }

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetClientRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool ClientToScreen(IntPtr hWnd, ref POINT lpPoint);

        [DllImport("user32.dll")]
        private static extern IntPtr GetDC(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

        [DllImport("gdi32.dll")]
        private static extern IntPtr CreateCompatibleDC(IntPtr hdc);

        [DllImport("gdi32.dll")]
        private static extern IntPtr CreateCompatibleBitmap(IntPtr hdc, int width, int height);

        [DllImport("gdi32.dll")]
        private static extern IntPtr SelectObject(IntPtr hdc, IntPtr h);

        [DllImport("gdi32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool BitBlt(IntPtr hdc, int x, int y, int cx, int cy, IntPtr hdcSrc, int x1, int y1, uint rop);

        [DllImport("gdi32.dll")]
        private static extern int GetDIBits(IntPtr hdc, IntPtr hbm, uint start, uint cLines, [Out] byte[] lpvBits, ref BITMAPINFOHEADER lpbmi, uint usage);

        [DllImport("gdi32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool DeleteObject(IntPtr ho);

        [DllImport("gdi32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool DeleteDC(IntPtr hdc);
    }

    [System.Text.Json.Serialization.JsonSerializable(typeof(string))]
    internal sealed partial class DiscordPingJsonContext : System.Text.Json.Serialization.JsonSerializerContext;
}
