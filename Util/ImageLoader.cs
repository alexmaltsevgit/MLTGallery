using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace MLTGallery.Util
{
  class ImageLoader
  {
    public static BitmapImage GetCompressedBitmapImage(string path, int quality)
    {
      using (Image image = Image.FromFile(path))
      using (Image memBitmap = new Bitmap(image, image.Width*quality/100, image.Height*quality/100))
      {
        ImageCodecInfo imageCodecInfo = GetEncoderInfo(GetMimeType(image));
        System.Drawing.Imaging.Encoder encoder = System.Drawing.Imaging.Encoder.Quality;
        EncoderParameter encoderParameter = new EncoderParameter(encoder, quality);
        EncoderParameters encoderParameters = new EncoderParameters(1);
        encoderParameters.Param[0] = encoderParameter;

        MemoryStream memoryStream = new MemoryStream();
        memBitmap.Save(memoryStream, imageCodecInfo, encoderParameters);
        Image newImage = Image.FromStream(memoryStream);
        ImageAttributes imageAttributes = new ImageAttributes();
        using (Graphics graphics = Graphics.FromImage(newImage))
        {
          graphics.InterpolationMode =
            System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
          graphics.DrawImage(newImage, new Rectangle(System.Drawing.Point.Empty, newImage.Size), 0, 0,
            newImage.Width, newImage.Height, GraphicsUnit.Pixel, imageAttributes);
        }

        BitmapImage compressed = ToBitmapImage(new Bitmap(newImage));
        compressed.Freeze();
        return compressed;
      }
    }

    public static BitmapImage GetBitmapImage(Uri uri)
    {
      BitmapImage bi = new BitmapImage(uri);
      bi.Freeze();
      return bi;
    }

    public static BitmapImage ToBitmapImage(Bitmap bitmap)
    {
      using (MemoryStream memory = new MemoryStream())
      {
        bitmap.Save(memory, ImageFormat.Png);
        memory.Position = 0;
        BitmapImage bitmapImage = new BitmapImage();
        bitmapImage.BeginInit();
        bitmapImage.StreamSource = memory;
        bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
        bitmapImage.EndInit();

        return bitmapImage;
      }
    }

    private static ImageCodecInfo GetEncoderInfo(String mimeType)
    {
      ImageCodecInfo[] encoders;
      encoders = ImageCodecInfo.GetImageEncoders();
      foreach (ImageCodecInfo ici in encoders)
        if (ici.MimeType == mimeType) return ici;

      return null;
    }

    private static string GetMimeType(Image img)
    {
      var imgguid = img.RawFormat.Guid;
      foreach (ImageCodecInfo codec in ImageCodecInfo.GetImageDecoders())
      {
        if (codec.FormatID == imgguid)
          return codec.MimeType;
      }

      return "image/unknown";
    }
  }
}
