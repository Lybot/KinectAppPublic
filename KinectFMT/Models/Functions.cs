using System;
using System.Drawing;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Color = System.Drawing.Color;
using FontFamily = System.Drawing.FontFamily;

namespace KinectFMT.Models
{
    public static class Functions
    {
        public static string FindStringResource(string needKey)
        {
            try
            {
                string result = Application.Current.FindResource(needKey) as string;
                return result;
            }
            catch
            {
                return "";
            }
        }
        private static byte[] GetHash(string inputString)
        {
            using HashAlgorithm algorithm = SHA256.Create();
            return algorithm.ComputeHash(Encoding.UTF8.GetBytes(inputString));
        }
        public static string GetHashString(string inputString)
        {

            StringBuilder sb = new StringBuilder();
            foreach (byte b in GetHash(inputString))
                sb.Append(b.ToString("X2"));

            return sb.ToString();
        }
        public static string Encrypt(string text, string key)
        {
            var rc6 = new RC6.RC6(256, Encoding.UTF8.GetBytes(key));
            var result = rc6.EncodeRc6(text);
            return Convert.ToBase64String(result);
        }
        public static string Decrypt(string cryptoText, string key)
        {
            var rc6 = new RC6.RC6(256, Encoding.UTF8.GetBytes(key));
            var result = rc6.DecodeRc6(Convert.FromBase64String(cryptoText));
            return Encoding.UTF8.GetString(result);
        }
        public static ImageSource BitmapToImageSource(Bitmap bitmap, bool disposable)
        {
            try
            {
                using var memory = new MemoryStream();
                bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Png);
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                memory.Position = 0;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                if (disposable)
                    bitmap.Dispose();
                return bitmapImage;
            }
            catch
            {
                using var memory = new MemoryStream();
                var bitmapError = new Bitmap(200, 200);
                Graphics g = Graphics.FromImage(bitmapError);
                g.DrawString(FindStringResource("ErrorSettings"), new Font(FontFamily.GenericSerif, 14), new SolidBrush(Color.Black), bitmapError.Width / 2f, bitmapError.Height / 2f);
                g.Flush();
                bitmapError.Save(memory, System.Drawing.Imaging.ImageFormat.Png);
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                bitmap.Dispose();
                memory.Dispose();
                return bitmapImage;
            }
        }
    }
}
