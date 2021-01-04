using System.Collections.Generic;
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
    }
}
