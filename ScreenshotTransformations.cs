using System.Drawing;
using FMUtils.Screenshot.Properties;
using FMUtils.WinApi;

namespace FMUtils.Screenshot
{
    public class ScreenshotTransformations
    {
        Size BaseSize;
        Rectangle First;
        Rectangle Second;
        Rectangle CompositeArea;
        Bitmap SecondScreenshotImage;

        public ScreenshotTransformations(Size baseSize, Rectangle first, Rectangle second, Rectangle compositeArea, Bitmap secondScreenshotImage)
        {
            this.BaseSize = baseSize;
            this.First = first;
            this.Second = second;
            this.CompositeArea = compositeArea;
            this.SecondScreenshotImage = secondScreenshotImage;
        }
        
        private static Bitmap GetImageWithShadow(Bitmap img, Bitmap SecondScreenshotImage, Rectangle First, Rectangle Second, Rectangle CompositeArea)
        {
            int ShadowMarginLeft = 14;
            int ShadowMarginRight = 18;
            int ShadowMarginTop = 13;
            int ShadowMarginBottom = 20;


            int CompositeWidth = First.Width + ShadowMarginLeft + ShadowMarginRight;
            int CompositeHeight = First.Height + ShadowMarginTop + ShadowMarginBottom;

            //int CompositeWidth = ScreenshotImage.Width + ShadowMarginLeft + ShadowMarginRight;
            //int CompositeHeight = ScreenshotImage.Height + ShadowMarginTop + ShadowMarginBottom;

            Bitmap bitmap = CompositeArea == Rectangle.Empty ? new Bitmap(CompositeWidth, CompositeHeight) : new Bitmap(CompositeArea.Width + ShadowMarginLeft + ShadowMarginRight, CompositeArea.Height + ShadowMarginTop + ShadowMarginBottom);

            using (Graphics canvas = Graphics.FromImage(bitmap))
            {
                canvas.DrawImage(Resources.active_top_left, new Rectangle(1, 1, Resources.active_top_left.Width, Resources.active_top_left.Height), new Rectangle(0, 0, Resources.active_top_left.Width, Resources.active_top_left.Height), GraphicsUnit.Pixel);
                //canvas.DrawImageUnscaled(Resources.active_top_left, 1, 1);

                canvas.DrawImage(Resources.active_top_right, new Rectangle(CompositeWidth - Resources.active_top_right.Width, 1, Resources.active_top_right.Width, Resources.active_top_right.Height), new Rectangle(0, 0, Resources.active_top_right.Width, Resources.active_top_right.Height), GraphicsUnit.Pixel);
                //canvas.DrawImageUnscaled(Resources.active_top_right, CompositeWidth - Resources.active_top_right.Width, 1);

                //canvas.DrawImage(Resources.active_bottom_right, new Rectangle(CompositeWidth - Resources.active_bottom_right.Width - 1, CompositeHeight - Resources.active_bottom_right.Height - 1, Resources.active_bottom_right.Width, Resources.active_bottom_right.Height), new Rectangle(0, 0, Resources.active_bottom_right.Width, Resources.active_bottom_right.Height), GraphicsUnit.Pixel);
                canvas.DrawImage(Resources.active_bottom_right, new Rectangle(CompositeWidth - Resources.active_bottom_right.Width, CompositeHeight - Resources.active_bottom_right.Height - 1, Resources.active_bottom_right.Width, Resources.active_bottom_right.Height), new Rectangle(0, 0, Resources.active_bottom_right.Width, Resources.active_bottom_right.Height), GraphicsUnit.Pixel);
                //canvas.DrawImageUnscaled(Resources.active_bottom_right, CompositeWidth - Resources.active_bottom_right.Width, CompositeHeight - Resources.active_bottom_right.Height - 1);

                canvas.DrawImage(Resources.active_bottom_left_2, new Rectangle(1, CompositeHeight - Resources.active_bottom_left_2.Height - 1, Resources.active_bottom_left_2.Width, Resources.active_bottom_left_2.Height - 1), new Rectangle(0, 0, Resources.active_bottom_left_2.Width, Resources.active_bottom_left_2.Height), GraphicsUnit.Pixel);
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

                if (Second != Rectangle.Empty)
                {
                    canvas.DrawImage(Resources.context_top_right, new Rectangle(ShadowMarginLeft + Second.Right, ShadowMarginTop + Second.Top + 5, Resources.context_top_right.Width, Resources.context_top_right.Height), new Rectangle(0, 0, Resources.context_top_right.Width, Resources.context_top_right.Height), GraphicsUnit.Pixel);
                    canvas.DrawImage(Resources.context_bottom_right, new Rectangle(ShadowMarginLeft + Second.Right, ShadowMarginTop + Second.Bottom, Resources.context_bottom_right.Width, Resources.context_bottom_right.Height), new Rectangle(0, 0, Resources.context_bottom_right.Width, Resources.context_bottom_right.Height), GraphicsUnit.Pixel);
                    canvas.DrawImage(Resources.context_bottom_left, new Rectangle(ShadowMarginLeft + Second.Left + 4, ShadowMarginTop + Second.Bottom, Resources.context_bottom_left.Width, Resources.context_bottom_left.Height - 1), new Rectangle(0, 0, Resources.context_bottom_left.Width, Resources.context_bottom_left.Height), GraphicsUnit.Pixel);


                    if (First.Bottom < Second.Bottom)
                    {
                        y1 = First.Bottom + ShadowMarginTop;
                        y2 = Second.Bottom + ShadowMarginTop - 1;
                    }
                    else
                    {
                        y1 = ShadowMarginTop + Second.Top + 5 + Resources.context_top_right.Height;
                        y2 = y1 + Second.Height - 5 - Resources.context_top_right.Height - 1;
                    }

                    for (int x = 0; x < Resources.context_right.Width; x++)
                        canvas.DrawLine(new Pen(Resources.context_right.GetPixel(x, Resources.context_right.Height - 1), 1f), x + ShadowMarginLeft + Second.Right, y1, x + ShadowMarginLeft + Second.Right, y2);


                    if (First.Right < Second.Right)
                    {
                        x1 = First.Right + ShadowMarginLeft;
                        x2 = Second.Right + ShadowMarginLeft - 1;
                    }
                    else
                    {
                        x1 = ShadowMarginLeft + Second.Left + Resources.context_bottom_left.Width + 4;
                        x2 = ShadowMarginLeft + Second.Right - 1;
                    }

                    for (int y = 0; y < Resources.context_bottom.Height; y++)
                        canvas.DrawLine(new Pen(Resources.context_bottom.GetPixel(Resources.context_bottom.Width - 1, y), 1f), x1, y + ShadowMarginTop + Second.Bottom, x2, y + ShadowMarginTop + Second.Bottom);

                    canvas.DrawImageUnscaled(SecondScreenshotImage, new Point(Second.X + ShadowMarginLeft, Second.Y + ShadowMarginTop));
                }

                canvas.Save();
            }

            return bitmap;
        }

        private static void SetAlpha(Bitmap Image, int a, int x, int y)
        {
            Image.SetPixel(x, y, Color.FromArgb(a, Image.GetPixel(x, y)));
        }
    }
}
