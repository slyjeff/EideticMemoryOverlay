using ArkhamOverlay.Common.Enums;
using ArkhamOverlay.Common.Services;
using ArkhamOverlay.Common.Utils;
using StreamDeckPlugin.Events;
using StreamDeckPlugin.Utils;
using System;
using System.Collections.Generic;

namespace StreamDeckPlugin.Services {
    public interface IImageService {
        event Action<IDynamicActionInfo> ImageLoaded;
        string GetImage(string imageId);
        string GetImage(IDynamicActionInfo dynamicActionInfo);
        bool HasImage(string imageId);
        void UpdateButtonImage(string imageId, byte[] bytes);

        /// <summary>
        /// Check to see if this image is stored- if it is not, retrieve it
        /// </summary>
        /// <param name="imageId">Id of the image in the cache- used to store and retrieve it</param>
        /// <param name="cardGroupId">Card Group for the image</param>
        /// <param name="buttonMode">ButtonMode for the image- may be null if the image is for the Card Group </param>
        /// <param name="index">Index of the image- may be null if the image is for the Card Group</param>
        void LoadImage(string imageId, CardGroupId cardGroupId, ButtonMode? buttonMode = null, int? index = null);
    }

    public class ImageService : IImageService {
        private static IDictionary<string, byte[]> _imageCache = new Dictionary<string, byte[]>();

        private readonly IEventBus _eventBus;

        public event Action<IDynamicActionInfo> ImageLoaded;

        public ImageService(IEventBus eventBus) {
            _eventBus = eventBus;
        }

        public void LoadImage(string imageId, CardGroupId cardGroupId, ButtonMode? buttonMode = null, int? index = null) {
            if (string.IsNullOrEmpty(imageId)) {
                return;
            }

            if (HasImage(imageId)) {
                return;
            }

            _eventBus.PublishGetButtonImageRequest(cardGroupId, buttonMode, index);
        }

        public void UpdateButtonImage(string imageId, byte[] bytes) {
            _imageCache[imageId] = bytes;
            _eventBus.PublishImageLoadedEvent(imageId);
        }

        public string GetImage(string imageId) {
            var imageBytes = imageId != null && _imageCache.ContainsKey(imageId) ? _imageCache[imageId] : null;

            return ImageUtils.CreateStreamdeckImage(imageBytes);
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
