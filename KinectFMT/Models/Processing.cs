using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Printing;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Printing;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using KinectFMT.MVVModels;
using KinectFMT.Properties;
using Image = System.Drawing.Image;
using Point = System.Drawing.Point;

namespace KinectFMT.Models
{
    public class Processing
    {
        public static Bitmap ResizeImage(Image image, int width, int height)
        {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height, PixelFormat.Format32bppArgb);
            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);
            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                using var wrapMode = new ImageAttributes();
                wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
            }
            return destImage;
        }
        public static byte[] GetBytes(Bitmap bmp)
        {
            Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
            BitmapData data = bmp.LockBits(rect,
                ImageLockMode.ReadOnly, bmp.PixelFormat);
            // coloca os bytes da imagem em uma matriz
            int bytesCount = data.Stride * data.Height;
            byte[] result = new byte[bytesCount];
            Marshal.Copy(data.Scan0, result, 0, bytesCount);
            bmp.UnlockBits(data);
            return result;
        }
        public static byte[] ProcessPhoto(Bitmap bitmap, string overlayPath, string savePath)
        {
            if (bitmap.Height != 1080 || bitmap.Width != 1920)
                bitmap = ResizeImage(bitmap, 1920, 1080);
            //bitmap = bitmap.Clone(new Rectangle(150, 0, 1620, 1080), bitmap.PixelFormat);
            if (Activation.IsTrial)
            {
                var gra = Graphics.FromImage(bitmap);
                gra.DrawString("DEMO", new Font(FontFamily.GenericSansSerif, 100), new SolidBrush(Color.Brown), 500, 400);
                gra.DrawString("DEMO", new Font(FontFamily.GenericSansSerif, 100), new SolidBrush(Color.Brown), 100, 100);
                gra.DrawString("DEMO", new Font(FontFamily.GenericSansSerif, 100), new SolidBrush(Color.Brown), 1000, 700);
                gra.DrawString("DEMO", new Font(FontFamily.GenericSansSerif, 100), new SolidBrush(Color.Brown), 1400, 900);
                gra.Save();
                gra.Dispose();
            }

            Bitmap resultBitmap;
            if (string.IsNullOrEmpty(overlayPath))
            {
                //photo.ROI = new Rectangle(new Point(200, 0), new Size(needWidth, needHeight));
                resultBitmap = bitmap.Clone(new Rectangle(150, 0, 1620, 1080), bitmap.PixelFormat);
                resultBitmap.Save(savePath);
                return GetBytes(bitmap);
            }
            try
            {
                Bitmap overlay = new Bitmap(overlayPath);
                Overlay(bitmap, overlay);
            }
            catch (Exception)
            {
                //photo.ROI = new Rectangle(new Point(200, 0), new Size(needWidth, needHeight));
                return GetBytes(bitmap);
            }
            //photo.ROI= new Rectangle(new Point(200,0),new Size(needWidth,needHeight) );
            resultBitmap = bitmap.Clone(new Rectangle(150, 0, 1620, 1080), bitmap.PixelFormat);
            resultBitmap.Save(savePath);
            return GetBytes(bitmap);
        }
        public static void Overlay(Bitmap target, Bitmap overlay)
        {
            Graphics gra = Graphics.FromImage(target);
            ResizeImage(overlay, target.Width, target.Height);
            overlay.SetResolution(target.HorizontalResolution, target.VerticalResolution);
            gra.CompositingQuality = CompositingQuality.HighQuality;
            gra.CompositingMode = CompositingMode.SourceOver;
            gra.DrawImage(overlay, new Point(0, 0));
            gra.Save();
            gra.Dispose();
        }
        public static void Print(string filename)
        {
            PrintDialog test = new PrintDialog {PrintQueue = LocalPrintServer.GetDefaultPrintQueue()};
            using var pd = new PrintDocument();
            pd.PrintPage += (o, e) =>
            {
                var img = Image.FromFile(filename);
                e.Graphics.DrawImage(img, e.Graphics.VisibleClipBounds);
            };
            pd.Print();
        }
        public static bool SendEmail(string smtpServer, string email, string password, int port, string to, string message, string title, string attachmentPath)
        {
            StreamWriter writer = new StreamWriter(Settings.Default.SavedImagesPath + "\\Email.txt", true);
            try
            {
                var myEmail = new MailAddress(email, "Freeze My Time");
                var recipient = new MailAddress(to);
                var emailMessage = new MailMessage(myEmail, recipient);
                if (!string.IsNullOrEmpty(attachmentPath))
                    emailMessage.Attachments.Add(new Attachment(attachmentPath));
                emailMessage.Body = message;
                emailMessage.Subject = title;
                var client = new SmtpClient(smtpServer, port);
                client.Credentials = new NetworkCredential(email, password);
                client.EnableSsl = true;
                try
                {
                    client.SendMailAsync(emailMessage);
                     writer.WriteLine(to + " " + attachmentPath?.Substring(attachmentPath.Length - 18, 18) + " " + "true");
                     writer.Close();
                     writer.Dispose();
                     return true;
                }
                catch (SmtpException)
                {
                    //   SendResult = Properties.Resources.SendNegativeResult;
                    writer.WriteLine(to + " " + attachmentPath?.Substring(attachmentPath.Length - 18, 18)+" "+"false");
                    writer.Close();
                    writer.Dispose();
                    return false;
                }
            }
            catch (Exception)
            {
                //  SendResult = Properties.Resources.SendNegativeResult;
                writer.WriteLine(to + " " + attachmentPath?.Substring(attachmentPath.Length - 18, 18) + " "+ "false");
                writer.Close();
                writer.Dispose();
                return false;
            }
        }
        public static string FindStringResource(string needKey)
        {
            var dictionary = Application.Current.Resources.MergedDictionaries[0];
            var keys = dictionary.Keys;
            var enumeratorKeys = keys.GetEnumerator();
            var values = dictionary.Values;
            var enumeratorValues = values.GetEnumerator();
            int i = 0;
            while (i < keys.Count)
            {
                enumeratorKeys.MoveNext();
                enumeratorValues.MoveNext();
                i++;
                if (enumeratorKeys.Current?.ToString() == needKey)
                    break;
            }
            var result = enumeratorValues.Current?.ToString();
            return result;
        }
    }
}
