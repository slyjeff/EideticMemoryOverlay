using StreamDeckPlugin.Events;
using StreamDeckPlugin.Utils;
using System;
using System.Collections.Generic;

namespace StreamDeckPlugin.Services {
    public interface IImageService {
        event Action<IDynamicActionInfo> ImageLoaded;
        string GetImage(IDynamicActionInfo dynamicActionInfo);
        bool HasImage(string imageId);
        void UpdateButtonImage(string name, byte[] bytes);
    }

    public class ImageService : IImageService {
        private static IDictionary<string, byte[]> _imageCache = new Dictionary<string, byte[]>();

        private readonly IEventBus _eventBus;

        public event Action<IDynamicActionInfo> ImageLoaded;

        public ImageService(IEventBus eventBus) {
            _eventBus = eventBus;
            eventBus.OnDynamicActionInfoChanged(DynamicActionChanged);
        }

        private void DynamicActionChanged(IDynamicActionInfo dynamicActionInfo) {
            if (!dynamicActionInfo.IsImageAvailable) {
                return;
            }
            
            if (HasImage(dynamicActionInfo.ImageId)) {
                return;
            }

            _eventBus.GetButtonImage(dynamicActionInfo.Deck, dynamicActionInfo.Index, dynamicActionInfo.Mode);
        }

        public void UpdateButtonImage(string imageId, byte[] bytes) {
            _imageCache[imageId] = bytes;
            _eventBus.ImageLoaded(imageId);
        }

        public string GetImage(IDynamicActionInfo dynamicAction) {
            var imageBytes = dynamicAction.ImageId != null && _imageCache.ContainsKey(dynamicAction.ImageId) ? _imageCache[dynamicAction.ImageId] : null;

            return ImageUtils.CreateStreamdeckImage(imageBytes, dynamicAction.IsToggled);
        }

        public bool HasImage(string imageId) {
            //we "have" a null image because we'll just make a blank bitmap;
            if (imageId == null) {
                return true;
            }

            return _imageCache.ContainsKey(imageId);
        }
    }
}
