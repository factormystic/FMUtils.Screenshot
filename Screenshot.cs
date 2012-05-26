using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using FMUtils.WinApi;

namespace FMUtils.Screenshot
{
    public enum ScreenshotMethod { Auto, GDI, DWM };

    public class Screenshot
    {
        public IntPtr TargetHandle { get; private set; }

        public Bitmap BaseScreenshotImage;

        public Rectangle CaptureRect { get; private set; }
        public Rectangle TargetRect { get; private set; }

        public bool isMaximized { get; private set; }
        public bool isRounded { get; private set; }

        public string WindowClass { get; private set; }
        public string WindowTitle { get; private set; }
        public DateTime Date { get; private set; }

        public Screen TargetScreen { get; private set; }

        public ScreenshotMethod Method { get; private set; }

        public Screenshot(IntPtr targetWindow, ScreenshotMethod method, bool withSolidGlass)
        {
            this.TargetHandle = targetWindow == IntPtr.Zero ? Windowing.GetForegroundWindow() : targetWindow;
            this.WindowClass = Windowing.GetWindowClass(TargetHandle);

            Trace.WriteLine(string.Format("Requested method is '{0}'", method), string.Format("Screenshot.ctor [{0}]", System.Threading.Thread.CurrentThread.Name));

            this.Method = method == ScreenshotMethod.Auto ? ScreenshotMethod.DWM : method;
            this.Method = FMUtils.WinApi.Helper.VisualStyle != Helper.VisualStyles.Aero ? ScreenshotMethod.GDI : this.Method;
            
            //I cannot recall why I uncommented the above and used this instead, undoing until I remember
            //this.Method = method;

            Trace.WriteLine(string.Format("Actual method is '{0}'", this.Method), string.Format("Screenshot.ctor [{0}]", System.Threading.Thread.CurrentThread.Name));

            //this.TargetRect = isMaximized ? Screen.FromHandle(TargetHandle).WorkingArea : Helper.GetWindowRectangle(TargetHandle);
            this.TargetRect = Helper.GetWindowRectangleDPI(TargetHandle);
            this.isMaximized = Helper.IsWindowMazimized(TargetHandle);
            this.TargetScreen = Screen.FromHandle(TargetHandle);

            this.BaseScreenshotImage = this.Method == ScreenshotMethod.DWM ? ScreenshotWindowDWM(TargetHandle, withSolidGlass) : ScreenshotWindowGDI(TargetHandle, withSolidGlass);

            this.isRounded = !Helper.IsSquareWindowEdge(TargetHandle);

            this.WindowTitle = WinApi.Helper.GetWindowText(TargetHandle, true);
            this.Date = DateTime.Now;
        }

        public Screenshot(Rectangle r)
        {
            this.Method = ScreenshotMethod.GDI;
            
            this.TargetRect = r;
            this.isMaximized = false;
            this.TargetScreen = Screen.FromRectangle(r);

            this.BaseScreenshotImage = Core.ScreenshotArea(r);

            this.isRounded = false;

            this.WindowTitle = "Screenshot (" + r.ToString() + ")";
            this.Date = DateTime.Now;
        }

        private Screenshot() { }

        private Bitmap ScreenshotWindowGDI(IntPtr targetWindow, bool withOpaqueGlass)
        {
            Trace.WriteLine(string.Format("Creating screenshot of window {0}: '{1}'...", this.WindowClass, targetWindow), string.Format("Screenshot.ScreenshotWindowGDI [{0}]", System.Threading.Thread.CurrentThread.Name));

            Bitmap ss = null;

            isMaximized = Helper.IsWindowMazimized(targetWindow);

            //var transparent = new Unmanaged.DWM_COLORIZATION_PARAMS(0x664b968a, 0x664b968a, 0x5, 0x2d, 0x32, 0x32, 0x0);
            //var opaque = new Unmanaged.DWM_COLORIZATION_PARAMS(0x664b968a, 0x664b968a, 0x28, 0x255, 0x32, 0x32, 0x1);

            CaptureRect = isMaximized ? this.TargetScreen.WorkingArea : TargetRect; //: BackingWindow.RectangleToScreen(ThumbRect);

            try
            {
                DWM.DWM_COLORIZATION_PARAMS OriginalColorization = Helper.GetColorization();
                if (withOpaqueGlass && Helper.isCompositionEnabled && OriginalColorization.OpaqueBlend == 0)
                {
                    DWM.DWM_COLORIZATION_PARAMS OpaqueColorization = OriginalColorization;

                    OpaqueColorization.OpaqueBlend = 1;
                    OpaqueColorization.ColorBalance = Math.Min(255, OpaqueColorization.ColorBalance * 8);
                    OpaqueColorization.AfterglowBalance = (uint)(OpaqueColorization.AfterglowBalance / 4.5);

                    DWM.DwmSetColorizationParameters(ref OpaqueColorization, 0);
                    ss = Core.ScreenshotWindow(targetWindow);
                    DWM.DwmSetColorizationParameters(ref OriginalColorization, 0);
                }
                else
                {
                    ss = Core.ScreenshotWindow(targetWindow);
                }
            }
            catch (ArgumentException ae)
            {
                Trace.WriteLine("Argument Exception: " + ae.GetBaseException().Message);
            }
            catch (NullReferenceException nre)
            {
                Trace.WriteLine("Null Reference Exception: " + nre.GetBaseException().Message);
            }
            finally
            {
                ss = ss ?? Core.ScreenshotWindow(targetWindow);
            }

            Trace.WriteLine("Done.", string.Format("Screenshot.ScreenshotWindowGDI [{0}]", System.Threading.Thread.CurrentThread.Name));

            var debug_path = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), @"prosnap-debug");
            if (System.IO.Directory.Exists(debug_path))
                ss.Save(System.IO.Path.Combine(debug_path, "raw.png"), System.Drawing.Imaging.ImageFormat.Png);

            return ss;
        }

        private Bitmap ScreenshotWindowDWM(IntPtr targetWindow, bool withOpaqueGlass)
        {
            if (!withOpaqueGlass)
                return Core.ScreenshotWindow(targetWindow);

            Trace.WriteLine(string.Format("Creating screenshot of window {0}: '{1}'...", this.WindowClass, targetWindow), string.Format("Screenshot.ScreenshotWindowDWM [{0}]", System.Threading.Thread.CurrentThread.Name));

            ////todo- don't reposition if there's another monitor
            //var CurrentScreen = Screen.FromHandle(TargetHandle);
            //TargetWindowRect.X = Math.Max(TargetWindowRect.X, CurrentScreen.WorkingArea.Left);
            //TargetWindowRect.Y = Math.Max(TargetWindowRect.Y, CurrentScreen.WorkingArea.Top);

            var DPIScaleFactor = Helper.GetDPIScaleFactor();
            var WindowBorderWidth = Helper.GetWindowBorderWidth() * DPIScaleFactor;

            var BackingPositionRect = isMaximized ? new Rectangle(TargetRect.Left, TargetRect.Top, TargetRect.Width, TargetRect.Height) : new Rectangle(TargetRect.Left, TargetRect.Top, TargetRect.Width, TargetRect.Height);
            if (Environment.OSVersion.Version.CompareTo(new Version(6, 2)) > 0)
            {
                if (Helper.VisualStyle == Helper.VisualStyles.Aero)
                {
                    BackingPositionRect.X += isMaximized && DPIScaleFactor == 1 ? 0 : 2;
                    BackingPositionRect.Height -= 2;
                    BackingPositionRect.Width -= 4;
                }
            }

            var ThumbRect = new Windowing.RECT(0, 0, BackingPositionRect.Width, BackingPositionRect.Height);
            if (Environment.OSVersion.Version.CompareTo(new Version(6, 2)) > 0)
            {
                if (Helper.VisualStyle == Helper.VisualStyles.Aero)
                {
                    ThumbRect.Left -= 2;
                    ThumbRect.Right += 2;
                    ThumbRect.Bottom += 2;
                }
            }

            Bitmap screenshot = new Bitmap(1, 1);
            using (var BackingWindow = new DWMBackingForm(Core.GetWindowGlassColor(), BackingPositionRect))
            {
                try
                {
                    IntPtr ThumbHandle;
                    var ThumbProps = new DWM.DWM_THUMBNAIL_PROPERTIES()
                    {
                        dwFlags = DWM.DWM_TNP_VISIBLE | DWM.DWM_TNP_RECTDESTINATION | DWM.DWM_TNP_OPACITY,
                        fVisible = true,
                        fSourceClientAreaOnly = true,
                        opacity = (byte)255,
                        rcDestination = ThumbRect
                    };
                    DWM.DwmRegisterThumbnail(BackingWindow.Handle, targetWindow, out ThumbHandle);
                    DWM.DwmUpdateThumbnailProperties(ThumbHandle, ref ThumbProps);
                    BackingWindow.Disposed += (s, e) => DWM.DwmUnregisterThumbnail(ThumbHandle);

                    BackingWindow.Visible = true;

                    ////Discovered by experimentation
                    //double offset = Math.Floor(7.0 * DPIScaleFactor);
                    //double extra_offset_left = 0;
                    //double extra_offset_top = 0;
                    //switch (Helper.OperatingSystem)
                    //{
                    //    case Helper.OperatingSystems.Win8:
                    //        offset = Math.Floor(9.0 * DPIScaleFactor);
                    //        extra_offset_left = -3.3 * DPIScaleFactor + (Helper.VisualStyle == Helper.VisualStyles.Aero ? -2 : 0);
                    //        extra_offset_top = -3.3 * DPIScaleFactor;
                    //        break;
                    //}

                    CaptureRect = isMaximized ? this.TargetScreen.WorkingArea : BackingPositionRect; //: BackingWindow.RectangleToScreen(ThumbRect);
                    //CaptureRect = isMaximized && Helper.HasNonClientBorder(TargetHandle) ? new Rectangle((int)(BackingPositionRect.Left + WindowBorderWidth + offset + extra_offset_left), (int)(BackingPositionRect.Top + WindowBorderWidth + offset + extra_offset_top), (int)(BackingPositionRect.Width - WindowBorderWidth * 2 - offset * 2 - extra_offset_left * 2 - (2 - Math.Floor(DPIScaleFactor))), (int)(BackingPositionRect.Height - WindowBorderWidth * 2 - offset * 2 - extra_offset_top * 2 - (2 - Math.Floor(DPIScaleFactor)))) : BackingPositionRect;

                    screenshot = Core.ScreenshotArea(CaptureRect);
                    BackingWindow.Close();
                }
                catch (Exception e)
                {
                    //mystery gdi exception when closing the backing form... sometimes...
                }
            }

            Trace.WriteLine("Done.", string.Format("Screenshot.ScreenshotWindowDWM [{0}]", System.Threading.Thread.CurrentThread.Name));

            return screenshot;
        }

        protected static Screenshot FromBitmapRect(Bitmap MouseTargetScreenshot, Windowing.RECT MouseTargetWindowRect)
        {
            return new Screenshot()
            {
                BaseScreenshotImage = MouseTargetScreenshot,
                TargetRect = MouseTargetWindowRect,
                CaptureRect = MouseTargetWindowRect
            };
        }

        public void ReplaceWithBitmap(Bitmap replacement)
        {
            this.BaseScreenshotImage = replacement;
            this.TargetRect = new Rectangle(this.TargetRect.Location, replacement.Size);
            this.CaptureRect = this.TargetRect;
        }
    }
}