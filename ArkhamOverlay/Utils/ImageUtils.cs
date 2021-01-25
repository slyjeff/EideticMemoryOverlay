using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ArkhamOverlay.Utils {
    public static class ImageUtils {
        public static ImageSource CreateSolidColorImage(Color color) {
            int width = 128;
            int height = width;
            int stride = width / 8;
            byte[] pixels = new byte[height * stride];

            BitmapPalette myPalette = new BitmapPalette(new List<Color> { color });
            BitmapSource bitmapSource = BitmapSource.Create(
                width,
                height,
                96,
                96,
                PixelFormats.Indexed1,
                myPalette,
                pixels,
                stride);

            return bitmapSource;
        }

        public static ImageSource CropImage (this ImageSource imageSource, CardType cardType) {
            var startingPoint = GetCropStartingPoint(cardType);
            return new CroppedBitmap(imageSource as BitmapImage, new Int32Rect(Convert.ToInt32(startingPoint.X), Convert.ToInt32(startingPoint.Y), 220, 220));
        }

        private static Point GetCropStartingPoint(CardType cardType) {
            switch (cardType) {
                case CardType.Scenario:
                    return new Point(40, 60);
                case CardType.Agenda:
                    return new Point(10, 40);
                case CardType.Investigator:
                    return new Point(10, 50);
                case CardType.Act:
                    return new Point(190, 40);
                case CardType.Location:
                    return new Point(40, 40);
                case CardType.Enemy:
                    return new Point(40, 190);
                case CardType.Treachery:
                    return new Point(40, 0);
                case CardType.Asset:
                    return new Point(40, 40);
                case CardType.Event:
                    return new Point(40, 0);
                case CardType.Skill:
                    return new Point(40, 40);
                default:
                    return new Point(40, 40);
            }
        }

        public static byte[] AsBytes(this ImageSource image) {
            byte[] bytes = null;

            if (image is BitmapSource bitmapSource) {
                var encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bitmapSource));

                using (var stream = new MemoryStream()) {
                    encoder.Save(stream);
                    bytes = stream.ToArray();
                }
            }
            return bytes;
        }

    }
}
