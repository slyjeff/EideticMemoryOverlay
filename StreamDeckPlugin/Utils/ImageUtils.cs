using ArkhamOverlay.TcpUtils;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Net;

namespace StreamDeckPlugin.Utils {
    public static class ImageUtils {
        public static string BlankImage() {
            var bitmap = CreateSolidBackgroundBitmap(CardButtonType.Action);

            var converter = new ImageConverter();
            var converted = (byte[])converter.ConvertTo(bitmap, typeof(byte[]));
            var imageString = Convert.ToBase64String(converted);

            return "data:image/png;base64," + imageString;
        }

        public static string AsImage(this ICardInfo cardInfo) {
            var bitmap = string.IsNullOrEmpty(cardInfo.ImageSource)
                       ? CreateSolidBackgroundBitmap(cardInfo.CardButtonType)
                       : CreateCardArtBitmap(cardInfo);

            if (cardInfo.IsVisible) {
                DrawSelected(bitmap);
            }

            var converter = new ImageConverter();
            var converted = (byte[])converter.ConvertTo(bitmap, typeof(byte[]));
            var imageString = Convert.ToBase64String(converted);

            return "data:image/png;base64," + imageString;
        }

        private static void DrawSelected(Bitmap bitmap) {
            var pen = new Pen(Color.Goldenrod, 16);
            using (Graphics G = Graphics.FromImage(bitmap)) {
                G.DrawRoundedRectangle(pen, new Rectangle(0, 0, 220, 220), 30);
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


        private static Bitmap CreateSolidBackgroundBitmap(CardButtonType cardButtonType) {
            var bitmap = new Bitmap(72, 72);
            using (var gfx = Graphics.FromImage(bitmap)) {
                var brush = new SolidBrush(cardButtonType.AsColor());
                gfx.FillRectangle(brush, 0, 0, 72, 72);
            }

            return bitmap;
        }

        public static Bitmap CreateCardArtBitmap(ICardInfo cardInfo) {
            var imageRequest = WebRequest.Create("https://arkhamdb.com/" + cardInfo.ImageSource);
            var imageResponse = imageRequest.GetResponse();
            var responseStream = imageResponse.GetResponseStream();
            var bitmap = new Bitmap(responseStream);

            var cropRect = new Rectangle(CreateStartingPointBasedOnCardType(cardInfo.CardButtonType), new Size(220, 220));
            var croppedBitmap = new Bitmap(cropRect.Width, cropRect.Height);

            using (Graphics g = Graphics.FromImage(croppedBitmap)) {
                g.DrawImage(bitmap, new Rectangle(0, 0, croppedBitmap.Width, croppedBitmap.Height), cropRect, GraphicsUnit.Pixel);
            }

            return croppedBitmap;
        }

        private static Point CreateStartingPointBasedOnCardType(CardButtonType cardButtonType) {
            switch (cardButtonType) {
                case CardButtonType.Scenario:
                    return new Point(40, 60);
                case CardButtonType.Agenda:
                    return new Point(10, 40);
                case CardButtonType.Act:
                    return new Point(190, 40);
                case CardButtonType.Location:
                    return new Point(40, 40);
                case CardButtonType.Enemy:
                    return new Point(40, 200);
                case CardButtonType.Treachery:
                    return new Point(40, 0);
                case CardButtonType.Asset:
                    return new Point(40, 40);
                case CardButtonType.Event:
                    return new Point(40, 0);
                case CardButtonType.Skill:
                    return new Point(40, 40);
                case CardButtonType.Unknown:
                    return new Point(40, 40);
                default:
                    return new Point(40, 40);
            }
        }
    }
}
