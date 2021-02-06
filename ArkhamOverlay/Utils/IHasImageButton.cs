using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace ArkhamOverlay.Utils {
    public interface IHasImageButton {
        string Name { get; }
        CardType ImageCardType { get; }
        ImageSource Image { get; set; }
        ImageSource ButtonImage { get; set; }
        byte[] ButtonImageAsBytes { get; set; }
    }

    public static class HasImageButtonExtensions {
        private static readonly Dictionary<string, BitmapImage> CardImageCache = new Dictionary<string, BitmapImage>();

        public static void LoadImage(this IHasImageButton hasImageButton, string url) {
            if (string.IsNullOrEmpty(url)) {
                return;
            }

            if (Application.Current.Dispatcher.CheckAccess()) {
                DoLoadImage(hasImageButton, url);
            } else {
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => {
                    DoLoadImage(hasImageButton, url);
                }));
            }
        }

        private static void DoLoadImage(IHasImageButton hasImageButton, string location) {
            try {
                if (CardImageCache.ContainsKey(hasImageButton.Name)) {
                    hasImageButton.Image = CardImageCache[hasImageButton.Name];
                    CropImage(hasImageButton);
                    return;
                }

                var uri = new Uri(location, UriKind.Absolute);
                if (uri.IsFile) {
                    var localImage = ImageUtils.LoadLocalImage(uri);
                    hasImageButton.Image = localImage;
                    CardImageCache[hasImageButton.Name] = localImage;
                    CropImage(hasImageButton);
                } else {
                    var bitmapImage = new BitmapImage(uri);
                    bitmapImage.DownloadCompleted += (s, e) => {
                        CardImageCache[hasImageButton.Name] = bitmapImage;
                        CropImage(hasImageButton);
                    };
                    hasImageButton.Image = bitmapImage;
                }
            } catch {
                //todo: add logging, which means changing this from an extention method into a service
            }
        }

        internal static void CropImage(IHasImageButton hasImageButton) {
            try {
                hasImageButton.ButtonImage = hasImageButton.Image.CropImage(hasImageButton.ImageCardType);
                hasImageButton.ButtonImageAsBytes = hasImageButton.ButtonImage.AsBytes();
            } catch {
                //todo: add logging, which means changing this from an extention method into a service
            }
        }
    }
}
