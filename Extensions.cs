using System.Drawing.Imaging;
using System.IO;
using System.Drawing;

namespace FMUtils.Screenshot
{
    public static class Extensions
    {
        public static MemoryStream ToMemoryStream(this Bitmap b, ImageFormat format)
        {
            var result = new MemoryStream();
            b.Save(result, format);
            return result;
        }
    }
}
