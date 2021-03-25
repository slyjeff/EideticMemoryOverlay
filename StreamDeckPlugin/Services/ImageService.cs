using ArkhamOverlay.Common.Enums;
using ArkhamOverlay.Common.Services;
using ArkhamOverlay.Common.Utils;
using StreamDeckPlugin.Events;
using StreamDeckPlugin.Utils;
using System;
using System.Collections.Generic;

namespace StreamDeckPlugin.Services {
    public interface IImageService {
        /// <summary>
        /// Event raised when an image has been received from the UI and is now available for use
        /// </summary>
        event Action<IDynamicActionInfo> ImageLoaded;
        
        /// <summary>
        /// Get an image using by using image ID
        /// </summary>
        /// <param name="imageId">An ID used to store and retrieve images</param>
        /// <returns>A string formatted for sending images to the StreamDeck device</returns>
        string GetImage(string imageId);

        /// <summary>
        /// Get an image using the dynamic action info
        /// </summary>
        /// <param name="dynamicActionInfo">Information about a dynamic action so the proper image can be retreieved and drawn properly</param>
        /// <returns>A string formatted for sending images to the StreamDeck device</returns>
        /// <remards>Will note the 'toggled' state of the dynamic action info, and draw a gold border if it is toggled</remards>
        string GetImage(IDynamicActionInfo dynamicActionInfo);

        /// <summary>
        /// Whether or not an image for this imageID is stored
        /// </summary>
        /// <param name="imageId">An ID used to store and retrieve images</param>
        /// <returns>True if an image exists for this ID; False if it does not</returns>
        bool HasImage(string imageId);

        /// <summary>
        /// Store an image
        /// </summary>
        /// <param name="imageId">ID of the image to store</param>
        /// <param name="bytes">Content of the image</param>
        void UpdateButtonImage(string imageId, byte[] bytes);

        /// <summary>
        /// Check to see if this image is stored- if it is not, retrieve it
        /// </summary>
        /// <param name="imageId">Id of the image in the cache- used to store and retrieve it</param>
        /// <param name="cardGroupId">Card Group for the image</param>
        /// <param name="buttonMode">ButtonMode for the image- may be null if the image is for the Card Group </param>
        /// <param name="zoneIndex">Zone Index of the image- may be null if the image is for the Card Group</param>
        /// <param name="index">Index of the image- may be null if the image is for the Card Group</param>
        void LoadImage(string imageId, CardGroupId cardGroupId, ButtonMode? buttonMode = null, int? zoneIndex = null, int? index = null);
    }

    public class ImageService : IImageService {
        private static IDictionary<string, byte[]> _imageCache = new Dictionary<string, byte[]>();

        private readonly IEventBus _eventBus;

        public event Action<IDynamicActionInfo> ImageLoaded;

        public ImageService(IEventBus eventBus) {
            _eventBus = eventBus;
        }

        /// <summary>
        /// Check to see if this image is stored- if it is not, retrieve it
        /// </summary>
        /// <param name="imageId">Id of the image in the cache- used to store and retrieve it</param>
        /// <param name="cardGroupId">Card Group for the image</param>
        /// <param name="buttonMode">ButtonMode for the image- may be null if the image is for the Card Group </param>
        /// <param name="zoneIndex">Zone Index of the image- may be null if the image is for the Card Group</param>
        /// <param name="index">Index of the image- may be null if the image is for the Card Group</param>
        public void LoadImage(string imageId, CardGroupId cardGroupId, ButtonMode? buttonMode = null, int? zoneIndex = null, int? index = null) {
            if (string.IsNullOrEmpty(imageId)) {
                return;
            }

            if (HasImage(imageId)) {
                return;
            }

            _eventBus.PublishGetButtonImageRequest(cardGroupId, buttonMode, zoneIndex, index);
        }

        /// <summary>
        /// Store an image
        /// </summary>
        /// <param name="imageId">ID of the image to store</param>
        /// <param name="bytes">Content of the image</param>
        public void UpdateButtonImage(string imageId, byte[] bytes) {
            _imageCache[imageId] = bytes;
            _eventBus.PublishImageLoadedEvent(imageId);
        }

        /// <summary>
        /// Get an image using by using image ID
        /// </summary>
        /// <param name="imageId">An ID used to store and retrieve images</param>
        /// <returns>A string formatted for sending images to the StreamDeck device</returns>
        public string GetImage(string imageId) {
            var imageBytes = imageId != null && _imageCache.ContainsKey(imageId) ? _imageCache[imageId] : null;

            return ImageUtils.CreateStreamdeckImage(imageBytes);
        }

        /// <summary>
        /// Get an image using the dynamic action info
        /// </summary>
        /// <param name="dynamicActionInfo">Information about a dynamic action so the proper image can be retreieved and drawn properly</param>
        /// <returns>A string formatted for sending images to the StreamDeck device</returns>
        /// <remards>Will note the 'toggled' state of the dynamic action info, and draw a gold border if it is toggled</remards>
        public string GetImage(IDynamicActionInfo dynamicAction) {
            var imageBytes = dynamicAction.ImageId != null && _imageCache.ContainsKey(dynamicAction.ImageId) ? _imageCache[dynamicAction.ImageId] : null;

            return ImageUtils.CreateStreamdeckImage(imageBytes, dynamicAction.IsToggled);
        }

        /// <summary>
        /// Whether or not an image for this imageID is stored
        /// </summary>
        /// <param name="imageId">An ID used to store and retrieve images</param>
        /// <returns>True if an image exists for this ID; False if it does not</returns>
        public bool HasImage(string imageId) {
            //we "have" a null image because we'll just make a blank bitmap;
            if (imageId == null) {
                return true;
            }

            return _imageCache.ContainsKey(imageId);
        }
    }
}
