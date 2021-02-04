using ArkhamOverlay.Services.Cache;
using ArkhamOverlay.Utils;
using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace ArkhamOverlay.CardButtons {
    public interface IHasImageButton {
        string Name { get; }
        CardType ImageCardType { get; }
        ImageSource Image { get; set; }
        ImageSource ButtonImage { get; set; }
        byte[] ButtonImageAsBytes { get; set; }
    }

    public abstract class HasImageButtonBase : IHasImageButton {
        public abstract string Name { get; }
        public abstract CardType ImageCardType { get; }
        public abstract ImageSource Image { get; set; }
        public abstract ImageSource ButtonImage { get; set; }
        public abstract byte[] ButtonImageAsBytes { get; set; }

        public void LoadImage(string url) {
            if (Application.Current.Dispatcher.CheckAccess()) {
                DoLoadImage(url);
            } else {
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => {
                    DoLoadImage(url);
                }));
            }
        }

        private void DoLoadImage(string location) {
            try {
                if (CardImageCache.IsIncache(this.Name)) {
                    this.Image = CardImageCache.GetFromCache<ImageSource>(this.Name);
                    CropImage();
                    return;
                }

                var uri = new Uri(location, UriKind.Absolute);
                if (uri.IsFile) {
                    var localImage = ImageUtils.LoadLocalImage(uri);
                    this.Image = localImage;
                    CardImageCache.SaveTocache(this.Name, localImage);
                    CropImage();
                } else {
                    var bitmapImage = new BitmapImage(uri);
                    bitmapImage.DownloadCompleted += (s, e) => {
                        CardImageCache.SaveTocache(this.Name, bitmapImage);
                        CropImage();
                    };
                    this.Image = bitmapImage;
                }
            } catch {
                //todo: add logging, which means changing this from an extention method into a service
            }
        }

        internal void CropImage() {
            try {
                this.ButtonImage = this.Image.CropImage(this.ImageCardType);
                this.ButtonImageAsBytes = this.ButtonImage.AsBytes();
            } catch {
                //todo: add logging, which means changing this from an extention method into a service
            }
        }
    }
}
