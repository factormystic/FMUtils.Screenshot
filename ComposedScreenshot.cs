using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using FMUtils.WinApi;

namespace FMUtils.Screenshot
{
    public class ComposedScreenshot : Screenshot
    {
        public bool withBorderRounding
        {
            get
            {
                return _withborderrounding;
            }
            set
            {
                if (_withborderrounding != value)
                {
                    _composedScreenshotImage = null;
                    _composedScreenshotThumbnail = null;
                }

                _withborderrounding = value;
            }
        }
        bool _withborderrounding = false;

        public bool withBorderShadow
        {
            get
            {
                return _withbordershadow;
            }
            set
            {
                if (_withbordershadow != value)
                {
                    _composedScreenshotImage = null;
                    _composedScreenshotThumbnail = null;
                }

                _withbordershadow = value;
            }
        }
        bool _withbordershadow = false;

        public bool withCursor
        {
            get
            {
                return _withcursor;
            }
            set
            {
                if (_withcursor != value)
                {
                    _composedScreenshotImage = null;
                    _composedScreenshotThumbnail = null;
                }

                _withcursor = value;
            }
        }
        bool _withcursor = false;

        public Point CursorLocation { get; private set; }
        private Bitmap CursorImage;

        public List<Screenshot> CompositionStack = new List<Screenshot>();

        public Rectangle CompositionRect
        {
            get
            {
                if (CompositionStack.Count == 0)
                    return Rectangle.Empty;

                var StackAggregate = CompositionStack.Select((ss, i) =>
                    {
                        if (!withBorderShadow)
                            return ss.CaptureRect.IsEmpty ? ss.TargetRect : ss.CaptureRect;
                        else
                            return i == 0 ? CompositionHelper.GetTargetRectWithShadow(ss.CaptureRect) : CompositionHelper.GetTargetRectWithMenuShadow(ss.TargetRect);
                    }).Aggregate((result, r) => Rectangle.Union(result, r));

                return withCursor ? Rectangle.Union(StackAggregate, new Rectangle(CursorLocation, CursorImage.Size)) : StackAggregate;
            }
        }

        public Point CompositionOrigin
        {
            get
            {
                Point result = Point.Empty;

                foreach (var ss in CompositionStack)
                {
                    if (CompositionStack.IndexOf(ss) == 0)
                    {
                        result = ss.TargetRect.Location;
                    }
                    else
                    {
                        result.X = ss.TargetRect.Location.X < result.X ? ss.TargetRect.Location.X : result.X;
                        result.Y = ss.TargetRect.Location.Y < result.Y ? ss.TargetRect.Location.Y : result.Y;
                    }
                }

                return result;
            }
        }

        public Bitmap ComposedScreenshotImage
        {
            get
            {
                if (_composedScreenshotImage == null)
                    _composedScreenshotImage = GetComposedScreenshotImage();

                return _composedScreenshotImage;
            }
        }
        Bitmap _composedScreenshotImage = null;

        public Bitmap ComposedScreenshotThumbnail
        {
            get
            {
                if (_composedScreenshotThumbnail == null)
                    _composedScreenshotThumbnail = GetComposedScreenshotThumbnail();

                return _composedScreenshotThumbnail;
            }
        }
        Bitmap _composedScreenshotThumbnail = null;
        private Rectangle r;

        public ComposedScreenshot(ScreenshotMethod method = ScreenshotMethod.DWM, bool withSolidGlass = true)
            : this(IntPtr.Zero, method, withSolidGlass) { }

        public ComposedScreenshot(IntPtr targetWindow, ScreenshotMethod method = ScreenshotMethod.DWM, bool withSolidGlass = true)
            : base(targetWindow, method, withSolidGlass)
        {
            Trace.WriteLine("Creating composed screenshot...", string.Format("ComposedScreenshot.ctor [{0}]", System.Threading.Thread.CurrentThread.Name));

            CursorLocation = Cursor.Position;
            CursorImage = Core.GetCursor();

            //#32768 is the Windows Menu Class
            // || wc.StartsWith("WindowsForms10.Window.20808");
            var ContextMenus = Windowing.GetChildWindows(Windowing.GetDesktopWindow()).Where(h => Windowing.GetWindowClass(h) == "#32768").ToList();

            if (ContextMenus.Any(h => CaptureRect.IntersectsWith(Helper.GetWindowRectangle(h))))
            {
                Trace.WriteLine(string.Format("Found Win32 {0} menus...", ContextMenus.Count), string.Format("ComposedScreenshot.ctor [{0}]", System.Threading.Thread.CurrentThread.Name));

                CompositionStack.AddRange(ContextMenus.Select(h => Screenshot.FromBitmapRect(Core.ScreenshotWindow(h), Helper.GetWindowRectangle((h)))));
            }
            else
            {
                //If there are no Win32 popup context menus found, but the mouse is over something who's root is the target rect, walk that tree for composition items

                //Check if the thing the mouse is over is different (eg, an overhanging menu)
                IntPtr MouseTargetHandle = Windowing.WindowFromPoint(Cursor.Position);

                //We don't want to do this when we're trying to get popup context menus since they are children of the desktop window,
                //not the target window. However that situation is handled separately above.
                //What we're doing here is looking for overlapping windows, like GTK context menus
                if (Windowing.GetAncestor(MouseTargetHandle, Windowing.GA_ROOTOWNER) == this.TargetHandle) // && !this.TargetRect.Contains(Cursor.Position))
                {
                    Trace.WriteLine(string.Format("Mouse over something else, handle {0}...", MouseTargetHandle), string.Format("ComposedScreenshot.ctor [{0}]", System.Threading.Thread.CurrentThread.Name));

                    while (MouseTargetHandle != TargetHandle && MouseTargetHandle != IntPtr.Zero)
                    {
                        //var MouseTargetWindowRect = Helper.GetWindowRectangle(MouseTargetHandle);
                        //var MouseTargetScreenshot = Core.ScreenshotArea(MouseTargetHandle, MouseTargetWindowRect);

                        //GTK menus seem to have virtually identical parents of themselves, so I initially tried to circumvent this
                        //with a check to prevent any two same rects from being added
                        //though, it's obsolete with the more robust removal below

                        //if (!CompositionStack.Any(ss => ss.TargetRect == MouseTargetWindowRect))
                        //var layer = Screenshot.FromBitmapRect(MouseTargetScreenshot, MouseTargetWindowRect);
                        var layer = new Screenshot(MouseTargetHandle, ScreenshotMethod.GDI, false);
                        CompositionStack.Add(layer);

                        Trace.WriteLine("Added " + layer.TargetHandle.ToString() + "; Class: " + layer.WindowClass, "ComposedScreenshot.ctor");

                        MouseTargetHandle = Windowing.GetParent(MouseTargetHandle);
                    }

                    Trace.WriteLine(string.Format("Added {0} items...", CompositionStack.Count), string.Format("ComposedScreenshot.ctor [{0}]", System.Threading.Thread.CurrentThread.Name));

                    //Remove any rect which is completely inside of another. This is mainly an effort to avoid extra layers,
                    //such as those generated by GTK menus, where some elements appear to be duplicated.
                    var toRemove = CompositionStack.Where(cs => CompositionStack.Any(a => cs != a && a.TargetRect.Contains(cs.TargetRect))).ToList();

                    Trace.WriteLine(string.Format("Removing {0} items...", toRemove.Count), string.Format("ComposedScreenshot.ctor [{0}]", System.Threading.Thread.CurrentThread.Name));

                    //In a case where there's one menu and two identical rects, don't remove both of them, or we'll have nothing to compose
                    if (toRemove.Count == CompositionStack.Count && toRemove.Count > 0)
                        toRemove.RemoveAt(toRemove.Count - 1);

                    CompositionStack.RemoveAll(r => toRemove.Contains(r));

                    //If they're all fully inside the window bounds, remove them- probably just regular window elements
                    if (CompositionStack.All(cs => this.TargetRect.Contains(cs.TargetRect)))
                    {
                        Trace.WriteLine(string.Format("Clearing {0} items...", toRemove.Count), string.Format("ComposedScreenshot.ctor [{0}]", System.Threading.Thread.CurrentThread.Name));
                        //CompositionStack.RemoveAll(cs => !cs.WindowClass.StartsWith("WindowsForms10.Window.20808"));
                        CompositionStack.Clear();
                    }
                }
            }

            //Since we compose where first is bottom and last is top, we'll need to reverse what we have so far since we enumerate the other direction
            CompositionStack.Reverse();

            //Base window is always the bottom layer of the composition stack
            CompositionStack.Insert(0, this);

            Trace.WriteLine("Done.", string.Format("ComposedScreenshot.ctor [{0}]", System.Threading.Thread.CurrentThread.Name));
        }

        public ComposedScreenshot(Rectangle r)
            : base(r)
        {
            Trace.WriteLine("Creating region screenshot...", string.Format("ComposedScreenshot.ctor [{0}]", System.Threading.Thread.CurrentThread.Name));

            CursorLocation = Cursor.Position;
            CursorImage = Core.GetCursor();

            //CompositionStack.Add(new Screenshot(r));
            CompositionStack.Add(Screenshot.FromBitmapRect(Core.ScreenshotArea(r), r));

            Trace.WriteLine("Done.", string.Format("ComposedScreenshot.ctor [{0}]", System.Threading.Thread.CurrentThread.Name));
        }

        private Bitmap GetComposedScreenshotImage()
        {
            Trace.WriteLine(string.Format("Composing {0} images...", CompositionStack.Count), string.Format("ComposedScreenshot.GetComposedScreenshotImage [{0}]", System.Threading.Thread.CurrentThread.Name));

            Bitmap Composite = new Bitmap(CompositionRect.Width, CompositionRect.Height);
            using (var g = Graphics.FromImage(Composite))
            {
                //todo:
                //1- capture handle's classes
                //2- in here, apply rounding and shadowing as applicable per flags & known class (eg, for windows, the window shadow. for menu's the menu shadow. else, none)

                var ShadowOffset = withBorderShadow ? CompositionHelper.GetBaseLayerDeltaOffsetWithShadow() : Point.Empty;

                for (int i = 0; i < CompositionStack.Count; i++)
                {
                    var WorkingBitmap = CompositionStack[i].BaseScreenshotImage.Clone() as Bitmap;

                    if (i == 0)
                    {
                        if (withBorderRounding && !isMaximized)
                            WorkingBitmap = CompositionHelper.GetImageWithRoundedBorders(WorkingBitmap);

                        if (withBorderShadow)
                            WorkingBitmap = CompositionHelper.GetImageWithShadow(WorkingBitmap);

                        g.DrawImageUnscaled(WorkingBitmap, new Point(CompositionStack[i].TargetRect.Location.X - CompositionOrigin.X, CompositionStack[i].TargetRect.Location.Y - CompositionOrigin.Y));
                    }
                    else
                    {
                        if (withBorderShadow)
                        {
                            var r = CompositionStack[i].TargetRect;
                            r.Intersect(CompositionStack[0].TargetRect);

                            //We only need to clip the occlusion if the screenshot method grabbed any overlaying menus as part of the base screenshot
                            //This doesn't happen when we create it via DWM since then, we solely grab the window
                            //But it ALSO doesn't happen when the visual theme is Basic/Classic. Why? No idea! The shadow is painted on by some mechanism that doesn't capture
                            WorkingBitmap = CompositionHelper.GetImageWithMenuShadow(WorkingBitmap, new Rectangle(r.X - CompositionStack[i].TargetRect.X, r.Y - CompositionStack[i].TargetRect.Y, r.Width, r.Height), this.Method != ScreenshotMethod.DWM && FMUtils.WinApi.Helper.VisualStyle == Helper.VisualStyles.Aero);
                        }

                        g.DrawImageUnscaled(WorkingBitmap, new Point(CompositionStack[i].TargetRect.Location.X - CompositionOrigin.X + ShadowOffset.X, CompositionStack[i].TargetRect.Location.Y - CompositionOrigin.Y + ShadowOffset.Y));
                    }

                    if (Directory.Exists(Core.DebugPath))
                    {
                        g.Flush();
                        g.Save();

                        WorkingBitmap.Save(Path.Combine(Core.DebugPath, "item-" + i.ToString() + ".png"), System.Drawing.Imaging.ImageFormat.Png);
                        Composite.Save(Path.Combine(Core.DebugPath, "comp-" + i.ToString() + ".png"), System.Drawing.Imaging.ImageFormat.Png);
                    }
                }

                if (withCursor)
                    g.DrawImageUnscaled(CursorImage, new Point(CursorLocation.X - CaptureRect.Location.X + ShadowOffset.X, CursorLocation.Y - CaptureRect.Location.Y + ShadowOffset.Y));

                if (Directory.Exists(Core.DebugPath))
                {
                    Composite.Save(Path.Combine(Core.DebugPath, "final.png"), System.Drawing.Imaging.ImageFormat.Png);
                }
            }

            Trace.WriteLine("Done.", string.Format("ComposedScreenshot.GetComposedScreenshotImage [{0}]", System.Threading.Thread.CurrentThread.Name));

            return Composite;
        }

        private Bitmap GetComposedScreenshotThumbnail()
        {
            Trace.WriteLine("Creating thumbnail...", string.Format("ComposedScreenshot.GetComposedScreenshotThumbnail [{0}]", System.Threading.Thread.CurrentThread.Name));

            float ImageAspectRatio = (float)ComposedScreenshotImage.Width / (float)ComposedScreenshotImage.Height;

            float TargetWidth = Math.Min(Screen.PrimaryScreen.WorkingArea.Width / 4, ComposedScreenshotImage.Width);
            float TargetHeight = TargetWidth / ImageAspectRatio;
            
            if (TargetHeight >= Screen.PrimaryScreen.WorkingArea.Height-20)
            {
                //resulting scaled image would be too tall
            }

            Bitmap result = ComposedScreenshotImage.GetThumbnailImage((int)TargetWidth, (int)TargetHeight, null, IntPtr.Zero) as Bitmap;

            Trace.WriteLine("Done.", string.Format("ComposedScreenshot.GetComposedScreenshotThumbnail [{0}]", System.Threading.Thread.CurrentThread.Name));
            return result;
        }

        public void SetOverrideImageData(Bitmap b)
        {
            _composedScreenshotImage = b;
        }
    }
}
