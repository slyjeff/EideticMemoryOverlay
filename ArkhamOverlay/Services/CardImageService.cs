using ArkhamOverlay.CardButtons;
using ArkhamOverlay.Utils;
using System;
using System.Runtime.Caching;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace ArkhamOverlay.Services {
    public class CardImageService {
        private readonly LoggingService _logger;

        private readonly CardImageCache _cache = new CardImageCache();

        public CardImageService(LoggingService loggingService) {
            _logger = loggingService;
        }

        public void LoadImage(IHasImageButton hasImageButton) {
            if (string.IsNullOrEmpty(hasImageButton.ImageSource)) {
                return;
            }

            if (Application.Current.Dispatcher.CheckAccess()) {
                DoLoadImage(hasImageButton);
            } else {
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => {
                    DoLoadImage(hasImageButton);
                }));
            }
        }

        private void DoLoadImage(IHasImageButton hasImageButton) {
            try {
                _logger.LogMessage($"Loading image for button {hasImageButton.Name}");

                if (_cache.IsInCache(hasImageButton.ImageId)) {
                    _logger.LogMessage($"Found image in cache for button {hasImageButton.Name}");
                    hasImageButton.Image = _cache.GetFromCache(hasImageButton.ImageId);
                    if (hasImageButton.Image is BitmapImage bitmapImage && bitmapImage.IsDownloading) {
                        bitmapImage.DownloadCompleted += (s, e) => {
                            CropImage(hasImageButton);
                        };
                    } else {
                        CropImage(hasImageButton);
                    }
                    return;
                }

                var uri = new Uri(hasImageButton.ImageSource, UriKind.Absolute);
                if (uri.IsFile) {
                    var localImage = ImageUtils.LoadLocalImage(uri);
                    hasImageButton.Image = localImage;
                    _cache.SaveTocache(hasImageButton.ImageId, localImage);
                    CropImage(hasImageButton);
                } else {
                    var bitmapImage = new BitmapImage(uri);
                    _cache.SaveTocache(hasImageButton.ImageId, bitmapImage);
                    bitmapImage.DownloadCompleted += (s, e) => {
                        CropImage(hasImageButton);
                    };
                    hasImageButton.Image = bitmapImage;
                }
            } catch (Exception ex) {
                _logger.LogException(ex, $"Error loading image for button {hasImageButton.Name}");
            }
        }

        private void CropImage(IHasImageButton hasImageButton) {
            try {
                hasImageButton.ButtonImage = hasImageButton.Image.CropImage(hasImageButton.ImageCardType);
                hasImageButton.ButtonImageAsBytes = hasImageButton.ButtonImage.AsBytes();
            } catch (Exception ex) {
                _logger.LogException(ex, $"Error cropping image for button {hasImageButton.Name}");
            }
        }

        private class CardImageCache {
            private static readonly CacheItemPolicy DEFAULT_POLICY = new CacheItemPolicy();

            private readonly MemoryCache _memoryCache;

            public CardImageCache() {
                _memoryCache = MemoryCache.Default;
            }

            public bool SaveTocache(string cacheKey, object savedItem) {
                return _memoryCache.Add(cacheKey, savedItem, DEFAULT_POLICY);
            }

            public BitmapImage GetFromCache(string cacheKey) {
                if (!IsInCache(cacheKey)) {
                    return null;
                }
                return _memoryCache[cacheKey] as BitmapImage;
            }

            public void RemoveFromCache(string cacheKey) {
                if (IsInCache(cacheKey)) {
                    _memoryCache.Remove(cacheKey);
                }
            }

            public bool IsInCache(string cacheKey) {
                return _memoryCache.Contains(cacheKey);
            }

        }
    }
}
