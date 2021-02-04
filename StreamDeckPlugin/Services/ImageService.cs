using ArkhamOverlay.TcpUtils.Requests;
using ArkhamOverlay.TcpUtils.Responses;
using StreamDeckPlugin.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace StreamDeckPlugin.Services {
    public interface IImageService {
        event Action<IDynamicActionInfo> ImageLoaded;
        string GetImage(IDynamicActionInfo dynamicActionInfo);
        bool HasImage(IDynamicActionInfo dynamicActionInfo);
    }

    public class ImageService : IImageService{
        public static IDictionary<string, byte[]> ImageCache = new Dictionary<string, byte[]>();
        private readonly ISendSocketService _sendSocketService;

        public static IImageService Service { get; private set; }
        public event Action<IDynamicActionInfo> ImageLoaded;

        public ImageService(IDynamicActionInfoStore dynamicActionService, ISendSocketService sendSocketService) {
            if (Service != null) {
                throw new Exception("Only one instance of Service may be created");
            }

            Service = this;

            dynamicActionService.DynamicActionChanged += DynamicActionChanged;
            _sendSocketService = sendSocketService;
        }

        private void DynamicActionChanged(IDynamicActionInfo dynamicActionInfo) {
            if (!dynamicActionInfo.IsImageAvailable) {
                return;
            }
            
            if (HasImage(dynamicActionInfo)) {
                return;
            }

            var worker = new BackgroundWorker();
            worker.DoWork += (x, y) => {
                var request = new ButtonImageRequest { Deck = dynamicActionInfo.Deck, Index = dynamicActionInfo.Index, FromCardSet = dynamicActionInfo.Mode == DynamicActionMode.Set };
                var response = _sendSocketService.SendRequest<ButtonImageResponse>(request);
                ImageCache[response.Name] = response.Bytes;
                ImageLoaded?.Invoke(dynamicActionInfo);
            };
            worker.RunWorkerAsync();
        }

        public string GetImage(IDynamicActionInfo dynamicAction) {
            var imageBytes = dynamicAction.ImageId != null && ImageCache.ContainsKey(dynamicAction.ImageId) ? ImageCache[dynamicAction.ImageId] : null;

            return ImageUtils.CreateStreamdeckImage(imageBytes, dynamicAction.IsToggled);
        }

        public bool HasImage(IDynamicActionInfo dynamicActionInfo) {
            //we "have" a null image because we'll just make a blank bitmap;
            if (dynamicActionInfo.ImageId == null) {
                return true;
            }

            return ImageCache.ContainsKey(dynamicActionInfo.ImageId);
        }
    }
}
