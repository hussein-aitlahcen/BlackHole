using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows.Media.Imaging;

namespace BlackHole.Common.Helpers
{
    public static class ImageHelpers
    {
        /// <summary>
        /// </summary>
        /// <param name="rawData"></param>
        /// <returns></returns>
        public static BitmapImage BitmapToImageSource(byte[] rawData)
        {
            using (var stream = new MemoryStream(rawData))
            {
                var bitmapimage = new BitmapImage();
                bitmapimage.BeginInit();
                bitmapimage.StreamSource = stream;
                bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapimage.EndInit();
                return bitmapimage;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="image"></param>
        /// <param name="imageQuality"></param>
        /// <returns></returns>
        public static byte[] CompressImage(Bitmap image, int imageQuality)
        {
            var imageQualitysParameter = new EncoderParameter(Encoder.Quality, imageQuality);
            var codecParameter = new EncoderParameters(1) { Param = { [0] = imageQualitysParameter } };
            var jpegCodec = ImageCodecInfo.GetImageEncoders().First(codec => codec.MimeType == "image/jpeg");
            
            using (var stream = new MemoryStream())
            {
#warning using the jpg compression causes the webcam frames to bug FIX THIS
                //image.Save(stream, jpegCodec, codecParameter);
                image.Save(stream, ImageFormat.Bmp);

                return stream.ToArray();
            }
        }
    }
}