using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;

namespace StreamDeckPlugin.Utils {
    public static class ImageUtils {
        const int ImageHeightAndWidth = 220;

        public static string BlankImage() {
            var bitmap = CreateSolidBackgroundBitmap(Color.Black);

            var converter = new ImageConverter();
            var converted = (byte[])converter.ConvertTo(bitmap, typeof(byte[]));
            var imageString = Convert.ToBase64String(converted);

            return "data:image/png;base64," + imageString;
        }

        public static string CreateStreamdeckImage(byte[] bytes, bool isToggled = false) {
            var bitmap = (bytes == null)
                ? CreateSolidBackgroundBitmap(Color.Black)
                : CreateImageFromBytes(bytes);

            if (isToggled) {
                ImageUtils.DrawSelected(bitmap);
            }

            var converter = new ImageConverter();
            var converted = (byte[])converter.ConvertTo(bitmap, typeof(byte[]));
            var imageString = Convert.ToBase64String(converted);

            return "data:image/png;base64," + imageString;
        }

        public static void DrawSelected(Bitmap bitmap) {
            var pen = new Pen(Color.Goldenrod, 16);
            using (Graphics G = Graphics.FromImage(bitmap)) {
                G.DrawRoundedRectangle(pen, new Rectangle(0, 0, ImageHeightAndWidth, ImageHeightAndWidth), 30);
            }
        }

        public static GraphicsPath RoundedRect(Rectangle bounds, int radius) {
            int diameter = radius * 2;
            Size size = new Size(diameter, diameter);
            Rectangle arc = new Rectangle(bounds.Location, size);
            GraphicsPath path = new GraphicsPath();

            if (radius == 0) {
                path.AddRectangle(bounds);
                return path;
            }

            // top left arc  
            path.AddArc(arc, 180, 90);

            // top right arc  
            arc.X = bounds.Right - diameter;
            path.AddArc(arc, 270, 90);

            // bottom right arc  
            arc.Y = bounds.Bottom - diameter;
            path.AddArc(arc, 0, 90);

            // bottom left arc 
            arc.X = bounds.Left;
            path.AddArc(arc, 90, 90);

            path.CloseFigure();
            return path;
        }

        public static void DrawRoundedRectangle(this Graphics graphics, Pen pen, Rectangle bounds, int cornerRadius) {
            using (GraphicsPath path = RoundedRect(bounds, cornerRadius)) {
                graphics.DrawPath(pen, path);
            }
        }


        private static Bitmap CreateSolidBackgroundBitmap(Color color) {
            var bitmap = new Bitmap(ImageHeightAndWidth, ImageHeightAndWidth);
            using (var gfx = Graphics.FromImage(bitmap)) {
                var brush = new SolidBrush(color);
                gfx.FillRectangle(brush, 0, 0, ImageHeightAndWidth, ImageHeightAndWidth);
            }

            return bitmap;
        }

        private static Bitmap CreateImageFromBytes(byte[] bytes) {
            if (bytes == null) {
                return CreateSolidBackgroundBitmap(Color.Black);
            }

            Bitmap bitmap;
            using (var stream = new MemoryStream(bytes)) {
                bitmap = new Bitmap(stream);
            }

            return bitmap;
        }
    }
}
