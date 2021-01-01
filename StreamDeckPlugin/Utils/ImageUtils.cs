using ArkhamOverlay.TcpUtils;
using ArkhamOverlay.TcpUtils.Responses;
using System;
using System.Drawing;
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

        public static string AsImage(this CardInfoResponse cardInfoResponse) {
            var bitmap = string.IsNullOrEmpty(cardInfoResponse.ImageSource)
                       ? CreateSolidBackgroundBitmap(cardInfoResponse.CardButtonType)
                       : CreateCardArtBitmap(cardInfoResponse);


            var converter = new ImageConverter();
            var converted = (byte[])converter.ConvertTo(bitmap, typeof(byte[]));
            var imageString = Convert.ToBase64String(converted);

            return "data:image/png;base64," + imageString;
        }


        private static Bitmap CreateSolidBackgroundBitmap(CardButtonType cardButtonType) {
            var bitmap = new Bitmap(72, 72);
            using (var gfx = Graphics.FromImage(bitmap)) {
                var brush = new SolidBrush(cardButtonType.AsColor());
                gfx.FillRectangle(brush, 0, 0, 72, 72);
            }

            return bitmap;
        }

        public static Bitmap CreateCardArtBitmap(CardInfoResponse cardInfoResponse) {
            var imageRequest = WebRequest.Create("https://arkhamdb.com/" + cardInfoResponse.ImageSource);
            var imageResponse = imageRequest.GetResponse();
            var responseStream = imageResponse.GetResponseStream();
            var bitmap = new Bitmap(responseStream);

            var cropRect = new Rectangle(CreateStartingPointBasedOnCardType(cardInfoResponse.CardButtonType), new System.Drawing.Size(220, 200));
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
