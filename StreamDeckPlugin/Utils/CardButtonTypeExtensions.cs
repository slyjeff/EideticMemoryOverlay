using ArkhamOverlay.TcpUtils;
using System;
using System.Drawing;

namespace StreamDeckPlugin.Utils {
    public static class CardButtonTypeExtensions {
        public static string AsImage(this CardButtonType cardButtonType) {
            var bmp = new Bitmap(72, 72);
            using (var gfx = Graphics.FromImage(bmp)) {
                var brush = new SolidBrush(cardButtonType.AsColor());
                gfx.FillRectangle(brush, 0, 0, 72, 72);
            }

            ImageConverter converter = new ImageConverter();
            var converted = (byte[])converter.ConvertTo(bmp, typeof(byte[]));
            var imageString = Convert.ToBase64String(converted);

            return "data:image/png;base64," + imageString;
        }
    }
}
