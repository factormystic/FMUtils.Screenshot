using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microsoft.Win32;
using FMUtils.WinApi;

namespace FMUtils.Screenshot
{
    internal static class Core
    {
        internal static string DebugPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "prosnap-debug");

        internal static Bitmap ScreenshotWindow(IntPtr h)
        {
            var WindowArea = Helper.IsWindowMazimized(h) ? Screen.FromHandle(h).WorkingArea : Helper.GetWindowRectangle(h);
            return ScreenshotArea(WindowArea);
        }

        internal static Bitmap ScreenshotArea(Rectangle r)
        {
            var bitmap = new Bitmap(r.Width, r.Height);

            using (var graphics = Graphics.FromImage(bitmap))
            {
                graphics.CopyFromScreen(r.X, r.Y, 0, 0, bitmap.Size);
            }

            return bitmap;
        }

        internal static Color GetWindowGlassColor()
        {
            try
            {
                Trace.WriteLine("Attempting to get DWM color info...", string.Format("Screenshot.Core [{0}]", System.Threading.Thread.CurrentThread.Name));

                object RegColor = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\DWM").GetValue("ColorizationColor");
                object RegBalance = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\DWM").GetValue("ColorizationColorBalance");

                uint RawColor = (uint)(int)RegColor; //http://stackoverflow.com/questions/1085097/why-cant-i-unbox-an-int-as-a-decimal

                Color glass = Color.FromArgb((byte)((RawColor & 0x00FF0000) >> 16), (byte)((RawColor & 0x0000FF00) >> 8), (byte)(RawColor & 0x000000FF));

                double h, s, v;
                ColorExtensions.ColorToHSV(glass, out h, out s, out v);

                Color better = ColorExtensions.ColorFromHSV(h, s * ((double)(int)RegBalance) / 85, 1 - ((double)(int)RegBalance / 170));

                return better;
            }
            catch (NullReferenceException nre)
            {
                Trace.WriteLine(string.Format("That failed: {0}", nre.GetBaseException()), string.Format("Screenshot.Core [{0}]", System.Threading.Thread.CurrentThread.Name));
                Trace.WriteLine("Could not extract DWM color info from the registry, trying a direct API call...", string.Format("Screenshot.Core [{0}]", System.Threading.Thread.CurrentThread.Name));

                try
                {
                    uint RawColor;
                    bool opaque;
                    DWM.DwmGetColorizationColor(out RawColor, out opaque);

                    Color glass = Color.FromArgb((byte)((RawColor & 0x00FF0000) >> 16), (byte)((RawColor & 0x0000FF00) >> 8), (byte)(RawColor & 0x000000FF));

                    double h, s, v;
                    ColorExtensions.ColorToHSV(glass, out h, out s, out v);

                    return glass;
                }
                catch (Exception e)
                {
                    Trace.WriteLine(string.Format("That failed too: {0}", e.GetBaseException()), string.Format("Screenshot.Core [{0}]", System.Threading.Thread.CurrentThread.Name));
                    return Color.Transparent;
                }
            }
        }

        //cf. http://stackoverflow.com/a/1020819/1569
        internal static Bitmap GetCursor()
        {
            Windowing.CURSORINFO cursorInfo = new Windowing.CURSORINFO();
            cursorInfo.cbSize = Marshal.SizeOf(cursorInfo);
            if (!Windowing.GetCursorInfo(out cursorInfo))
                return null;

            if (cursorInfo.flags != Windowing.CURSOR_SHOWING)
                return null;

            IntPtr hicon = Windowing.CopyIcon(cursorInfo.hCursor);
            if (hicon == IntPtr.Zero)
                return null;

            Windowing.ICONINFO iconInfo;
            if (!Windowing.GetIconInfo(hicon, out iconInfo))
                return null;

            int x = cursorInfo.ptScreenPos.X - ((int)iconInfo.xHotspot);
            int y = cursorInfo.ptScreenPos.Y - ((int)iconInfo.yHotspot);

            using (Bitmap maskBitmap = Bitmap.FromHbitmap(iconInfo.hbmMask))
            {
                // Is this a monochrome cursor?
                if (maskBitmap.Height == maskBitmap.Width * 2)
                {
                    Bitmap resultBitmap = new Bitmap(maskBitmap.Width, maskBitmap.Width);

                    Graphics desktopGraphics = Graphics.FromHwnd(Windowing.GetDesktopWindow());
                    IntPtr desktopHdc = desktopGraphics.GetHdc();

                    IntPtr maskHdc = Windowing.CreateCompatibleDC(desktopHdc);
                    IntPtr oldPtr = Windowing.SelectObject(maskHdc, maskBitmap.GetHbitmap());

                    using (Graphics resultGraphics = Graphics.FromImage(resultBitmap))
                    {
                        IntPtr resultHdc = resultGraphics.GetHdc();

                        // These two operation will result in a black cursor over a white background.
                        // Later in the code, a call to MakeTransparent() will get rid of the white background.
                        Windowing.BitBlt(resultHdc, 0, 0, 32, 32, maskHdc, 0, 32, Windowing.TernaryRasterOperations.SRCCOPY);
                        Windowing.BitBlt(resultHdc, 0, 0, 32, 32, maskHdc, 0, 0, Windowing.TernaryRasterOperations.SRCINVERT);

                        resultGraphics.ReleaseHdc(resultHdc);
                    }

                    IntPtr newPtr = Windowing.SelectObject(maskHdc, oldPtr);
                    Windowing.DeleteDC(newPtr);
                    Windowing.DeleteDC(maskHdc);
                    desktopGraphics.ReleaseHdc(desktopHdc);

                    // Remove the white background from the BitBlt calls,
                    // resulting in a black cursor over a transparent background.
                    resultBitmap.MakeTransparent(Color.White);
                    return resultBitmap;
                }
            }

            Icon icon = Icon.FromHandle(hicon);
            return icon.ToBitmap();
        }
    }
}
