using System.Drawing;
using FMUtils.Screenshot.Properties;
using FMUtils.WinApi;

namespace FMUtils.Screenshot
{
    internal static class CompositionHelper
    {
        private static int ShadowMarginLeft = 14;
        private static int ShadowMarginRight = 18;
        private static int ShadowMarginTop = 13;
        private static int ShadowMarginBottom = 20;

        private static int MenuShadowMarginLeft = 0;
        private static int MenuShadowMarginRight = 6;
        private static int MenuShadowMarginTop = 0;
        private static int MenuShadowMarginBottom = 6;

        internal static Bitmap GetImageWithShadow(Bitmap img)
        {
            int CompositeWidth = img.Width + ShadowMarginLeft + ShadowMarginRight;
            int CompositeHeight = img.Height + ShadowMarginTop + ShadowMarginBottom;

            Bitmap bitmap = new Bitmap(CompositeWidth, CompositeHeight);
            using (Graphics canvas = Graphics.FromImage(bitmap))
            {
                canvas.DrawImage(Resources.active_top_left, new Rectangle(1, 1, Resources.active_top_left.Width, Resources.active_top_left.Height), new Rectangle(0, 0, Resources.active_top_left.Width, Resources.active_top_left.Height), GraphicsUnit.Pixel);
                //canvas.DrawImageUnscaled(Resources.active_top_left, 1, 1);

                canvas.DrawImage(Resources.active_top_right, new Rectangle(CompositeWidth - Resources.active_top_right.Width, 1, Resources.active_top_right.Width, Resources.active_top_right.Height), new Rectangle(0, 0, Resources.active_top_right.Width, Resources.active_top_right.Height), GraphicsUnit.Pixel);
                //canvas.DrawImageUnscaled(Resources.active_top_right, CompositeWidth - Resources.active_top_right.Width, 1);

                //canvas.DrawImage(Resources.active_bottom_right, new Rectangle(CompositeWidth - Resources.active_bottom_right.Width - 1, CompositeHeight - Resources.active_bottom_right.Height - 1, Resources.active_bottom_right.Width, Resources.active_bottom_right.Height), new Rectangle(0, 0, Resources.active_bottom_right.Width, Resources.active_bottom_right.Height), GraphicsUnit.Pixel);
                canvas.DrawImage(Resources.active_bottom_right, new Rectangle(CompositeWidth - Resources.active_bottom_right.Width, CompositeHeight - Resources.active_bottom_right.Height - 1, Resources.active_bottom_right.Width, Resources.active_bottom_right.Height), new Rectangle(0, 0, Resources.active_bottom_right.Width, Resources.active_bottom_right.Height), GraphicsUnit.Pixel);
                //canvas.DrawImageUnscaled(Resources.active_bottom_right, CompositeWidth - Resources.active_bottom_right.Width, CompositeHeight - Resources.active_bottom_right.Height - 1);

                canvas.DrawImage(Resources.active_bottom_left_2, new Rectangle(1, CompositeHeight - Resources.active_bottom_left_2.Height - 1, Resources.active_bottom_left_2.Width, Resources.active_bottom_left_2.Height), new Rectangle(0, 0, Resources.active_bottom_left_2.Width, Resources.active_bottom_left_2.Height), GraphicsUnit.Pixel);
                //canvas.DrawImageUnscaled(Resources.active_bottom_left_2, 1, CompositeHeight - Resources.active_bottom_left_2.Height - 1);


                int y1 = Resources.active_top_left.Height + 1;
                int y2 = CompositeHeight - (ShadowMarginTop + ShadowMarginBottom) + 1;
                for (int x = 0; x < Resources.active_left.Width; x++)
                    canvas.DrawLine(new Pen(Resources.active_left.GetPixel(x, Resources.active_left.Height - 1), 1f), x + 1, y1, x + 1, y2);

                for (int x = 0; x < Resources.active_right.Width; x++)
                    canvas.DrawLine(new Pen(Resources.active_right.GetPixel(x, Resources.active_right.Height - 1), 1f), x + CompositeWidth - Resources.active_right.Width - 1, y1, x + CompositeWidth - Resources.active_right.Width - 1, y2);


                int x1 = Resources.active_top_left.Width + 1;
                int x2 = CompositeWidth - Resources.active_top_right.Width - 1;
                for (int y = 0; y < Resources.active_top.Height; y++)
                    canvas.DrawLine(new Pen(Resources.active_top.GetPixel(Resources.active_top.Width - 1, y), 1f), x1, y + 1, x2, y + 1);

                for (int y = 0; y < Resources.active_bottom.Height; y++)
                    canvas.DrawLine(new Pen(Resources.active_bottom.GetPixel(Resources.active_bottom.Width - 1, y), 1f), x1, y + CompositeHeight - Resources.active_bottom.Height - 1, x2, y + CompositeHeight - Resources.active_bottom.Height - 1);

                //Draw the screenshot onto the composite, starting at the shadow offsets but with the original width and height, so it's not stretched
                canvas.DrawImageUnscaled(img, ShadowMarginLeft, ShadowMarginTop, img.Width, img.Height);

                canvas.Save();
            }

            return bitmap;
        }

        internal static Bitmap GetImageWithRoundedBorders(Bitmap img)
        {
            Bitmap ResultImage = img.Clone() as Bitmap;
            var First = new Rectangle(new Point(0), img.Size);

            if (Helper.VisualStyle == Helper.VisualStyles.Aero || Helper.VisualStyle == Helper.VisualStyles.Basic)
            {
                Point TopLeft = new Point(First.Left, First.Top);

                SetAlpha(ResultImage, 0, TopLeft.X + 0, TopLeft.Y + 0);
                SetAlpha(ResultImage, 0, TopLeft.X + 1, TopLeft.Y + 0);
                SetAlpha(ResultImage, 0, TopLeft.X + 2, TopLeft.Y + 0);
                SetAlpha(ResultImage, 0, TopLeft.X + 3, TopLeft.Y + 0);
                SetAlpha(ResultImage, 0, TopLeft.X + 4, TopLeft.Y + 0);

                SetAlpha(ResultImage, 0, TopLeft.X + 0, TopLeft.Y + 1);
                SetAlpha(ResultImage, 0, TopLeft.X + 1, TopLeft.Y + 1);
                SetAlpha(ResultImage, 0, TopLeft.X + 2, TopLeft.Y + 1);

                SetAlpha(ResultImage, 0, TopLeft.X + 0, TopLeft.Y + 2);
                SetAlpha(ResultImage, 0, TopLeft.X + 1, TopLeft.Y + 2);

                SetAlpha(ResultImage, 0, TopLeft.X + 0, TopLeft.Y + 3);

                SetAlpha(ResultImage, 0, TopLeft.X + 0, TopLeft.Y + 4);




                Point TopRight = new Point(First.Right, First.Top);

                SetAlpha(ResultImage, 0, TopRight.X - 1, TopRight.Y + 0);
                SetAlpha(ResultImage, 0, TopRight.X - 2, TopRight.Y + 0);
                SetAlpha(ResultImage, 0, TopRight.X - 3, TopRight.Y + 0);
                SetAlpha(ResultImage, 0, TopRight.X - 4, TopRight.Y + 0);
                SetAlpha(ResultImage, 0, TopRight.X - 5, TopRight.Y + 0);

                SetAlpha(ResultImage, 0, TopRight.X - 1, TopRight.Y + 1);
                SetAlpha(ResultImage, 0, TopRight.X - 2, TopRight.Y + 1);
                SetAlpha(ResultImage, 0, TopRight.X - 3, TopRight.Y + 1);

                SetAlpha(ResultImage, 0, TopRight.X - 1, TopRight.Y + 2);
                SetAlpha(ResultImage, 0, TopRight.X - 2, TopRight.Y + 2);

                SetAlpha(ResultImage, 0, TopRight.X - 1, TopRight.Y + 3);

                SetAlpha(ResultImage, 0, TopRight.X - 1, TopRight.Y + 4);
            }

            //Don't round the bottom corners of the window if it's basic
            if (Helper.VisualStyle == Helper.VisualStyles.Aero)
            {
                Point BottomLeft = new Point(First.Left, First.Bottom);

                SetAlpha(ResultImage, 0, BottomLeft.X + 0, BottomLeft.Y - 5);

                SetAlpha(ResultImage, 0, BottomLeft.X + 0, BottomLeft.Y - 4);

                SetAlpha(ResultImage, 0, BottomLeft.X + 0, BottomLeft.Y - 3);
                SetAlpha(ResultImage, 0, BottomLeft.X + 1, BottomLeft.Y - 3);

                SetAlpha(ResultImage, 0, BottomLeft.X + 0, BottomLeft.Y - 2);
                SetAlpha(ResultImage, 0, BottomLeft.X + 1, BottomLeft.Y - 2);
                SetAlpha(ResultImage, 0, BottomLeft.X + 2, BottomLeft.Y - 2);

                SetAlpha(ResultImage, 0, BottomLeft.X + 0, BottomLeft.Y - 1);
                SetAlpha(ResultImage, 0, BottomLeft.X + 1, BottomLeft.Y - 1);
                SetAlpha(ResultImage, 0, BottomLeft.X + 2, BottomLeft.Y - 1);
                SetAlpha(ResultImage, 0, BottomLeft.X + 3, BottomLeft.Y - 1);
                SetAlpha(ResultImage, 0, BottomLeft.X + 4, BottomLeft.Y - 1);



                Point BottomRight = new Point(First.Right, First.Bottom);

                SetAlpha(ResultImage, 0, BottomRight.X - 1, BottomRight.Y - 5);

                SetAlpha(ResultImage, 0, BottomRight.X - 1, BottomRight.Y - 4);

                SetAlpha(ResultImage, 0, BottomRight.X - 1, BottomRight.Y - 3);
                SetAlpha(ResultImage, 0, BottomRight.X - 2, BottomRight.Y - 3);

                SetAlpha(ResultImage, 0, BottomRight.X - 1, BottomRight.Y - 2);
                SetAlpha(ResultImage, 0, BottomRight.X - 2, BottomRight.Y - 2);
                SetAlpha(ResultImage, 0, BottomRight.X - 3, BottomRight.Y - 2);

                SetAlpha(ResultImage, 0, BottomRight.X - 1, BottomRight.Y - 1);
                SetAlpha(ResultImage, 0, BottomRight.X - 2, BottomRight.Y - 1);
                SetAlpha(ResultImage, 0, BottomRight.X - 3, BottomRight.Y - 1);
                SetAlpha(ResultImage, 0, BottomRight.X - 4, BottomRight.Y - 1);
                SetAlpha(ResultImage, 0, BottomRight.X - 5, BottomRight.Y - 1);
            }

            return ResultImage;
        }

        internal static Bitmap GetImageWithMenuShadow(Bitmap img, Rectangle occlusion, bool clipOcclusion)
        {
            occlusion = new Rectangle(occlusion.X, occlusion.Y, occlusion.Right == img.Width ? occlusion.Width + MenuShadowMarginRight : occlusion.Width, occlusion.Bottom == img.Height ? occlusion.Height + MenuShadowMarginBottom : occlusion.Height);

            int CompositeWidth = img.Width + MenuShadowMarginLeft + MenuShadowMarginRight;
            int CompositeHeight = img.Height + MenuShadowMarginTop + MenuShadowMarginBottom;

            Bitmap bitmap = new Bitmap(CompositeWidth, CompositeHeight);
            using (Graphics canvas = Graphics.FromImage(bitmap))
            {
                if (clipOcclusion)
                    canvas.ExcludeClip(occlusion);
                //canvas.FillRectangle((new SolidBrush(Color.Red), occlusion);

                var top_right_dest_rect = new Rectangle(MenuShadowMarginLeft + img.Width, MenuShadowMarginTop + 5, Resources.context_top_right.Width, Resources.context_top_right.Height);
                canvas.DrawImage(Resources.context_top_right, top_right_dest_rect, new Rectangle(0, 0, Resources.context_top_right.Width, Resources.context_top_right.Height), GraphicsUnit.Pixel);

                canvas.DrawImage(Resources.context_bottom_right, new Rectangle(MenuShadowMarginLeft + img.Width, MenuShadowMarginTop + img.Height, Resources.context_bottom_right.Width, Resources.context_bottom_right.Height), new Rectangle(0, 0, Resources.context_bottom_right.Width, Resources.context_bottom_right.Height), GraphicsUnit.Pixel);
                canvas.DrawImage(Resources.context_bottom_left, new Rectangle(MenuShadowMarginLeft + 4, MenuShadowMarginTop + img.Height, Resources.context_bottom_left.Width, Resources.context_bottom_left.Height - 1), new Rectangle(0, 0, Resources.context_bottom_left.Width, Resources.context_bottom_left.Height), GraphicsUnit.Pixel);

                int y1 = 5 + Resources.context_top_right.Height;
                int y2 = CompositeHeight - MenuShadowMarginTop - MenuShadowMarginBottom - 1;
                for (int x = 0; x < Resources.context_right.Width; x++)
                    canvas.DrawLine(new Pen(Resources.context_right.GetPixel(x, Resources.context_right.Height - 1), 1f), x + CompositeWidth - MenuShadowMarginRight, y1, x + CompositeWidth - MenuShadowMarginRight, y2);

                int x1 = 4 + Resources.context_bottom_left.Width;
                int x2 = CompositeWidth - MenuShadowMarginLeft - MenuShadowMarginRight - 1;
                for (int y = 0; y < Resources.context_bottom.Height; y++)
                    canvas.DrawLine(new Pen(Resources.context_bottom.GetPixel(Resources.context_bottom.Width - 1, y), 1f), x1, y + CompositeHeight - MenuShadowMarginBottom, x2, y + CompositeHeight - MenuShadowMarginBottom);

                canvas.DrawImageUnscaled(img, new Point(MenuShadowMarginLeft, MenuShadowMarginTop));
                canvas.Save();
            }

            return bitmap;
        }

        private static void SetAlpha(Bitmap Image, int a, int x, int y)
        {
            Image.SetPixel(x, y, Color.FromArgb(a, Image.GetPixel(x, y)));
        }

        internal static Rectangle GetTargetRectWithShadow(Rectangle r)
        {
            return new Rectangle(r.Location.X - ShadowMarginLeft, r.Location.Y - ShadowMarginTop, r.Width + ShadowMarginLeft + ShadowMarginRight, r.Height + ShadowMarginTop + ShadowMarginBottom);
        }

        internal static Point GetBaseLayerDeltaOffsetWithShadow()
        {
            return new Point(ShadowMarginLeft, ShadowMarginTop);
        }

        internal static Rectangle GetTargetRectWithMenuShadow(Rectangle r)
        {
            return new Rectangle(r.Location.X - MenuShadowMarginLeft, r.Location.Y - MenuShadowMarginTop, r.Width + MenuShadowMarginLeft + MenuShadowMarginRight, r.Height + MenuShadowMarginTop + MenuShadowMarginBottom);
        }
    }
}
