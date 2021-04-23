using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ArkhamHorrorLcg {
    public static class ArkhamImageUtils {
        public static ImageSource CropBaseStatLine(this ImageSource imageSource) {
            if (imageSource == null) {
                return null;
            }

            var startingPoint = new Point(226, 4);
            return new CroppedBitmap(imageSource as BitmapImage, new Int32Rect(Convert.ToInt32(startingPoint.X), Convert.ToInt32(startingPoint.Y), 190, 40));
        }

        public static ImageSource CropFullInvestigator(this ImageSource imageSource) {
            if (imageSource == null) {
                return null;
            }

            var startingPoint = new Point(40, 50);
            return new CroppedBitmap(imageSource as BitmapImage, new Int32Rect(Convert.ToInt32(startingPoint.X), Convert.ToInt32(startingPoint.Y), 160, 220));
        }
    }
}
