using ArkhamOverlay.TcpUtils;
using ArkhamOverlay.TcpUtils.Requests;
using ArkhamOverlay.TcpUtils.Responses;
using StreamDeckPlugin.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace StreamDeckPlugin.Services {
    public interface IImageService {
        event Action<IDynamicAction> ImageLoaded;
        string GetImage(IDynamicAction dynamicAction);
        bool HasImage(IDynamicAction dynamicAction);
    }

    public class ImageService : IImageService{
        public static IDictionary<string, byte[]> ImageCache = new Dictionary<string, byte[]>();
        private readonly ISendSocketService _sendSocketService;

        public static IImageService Service { get; private set; }
        public event Action<IDynamicAction> ImageLoaded;

        public ImageService(IDynamicActionService dynamicActionService, ISendSocketService sendSocketService) {
            if (Service != null) {
                throw new Exception("Only one instance of Service may be created");
            }

            Service = this;

            dynamicActionService.DynamicActionChanged += DynamicActionChanged;
            _sendSocketService = sendSocketService;
        }

        private void DynamicActionChanged(IDynamicAction dynamicAction) {
            if (!dynamicAction.IsImageAvailable) {
                return;
            }
            
            if (HasImage(dynamicAction)) {
                return;
            }

            var worker = new BackgroundWorker();
            worker.DoWork += (x, y) => {
                var request = new ButtonImageRequest { Deck = dynamicAction.Deck, Index = dynamicAction.Index, FromCardSet = dynamicAction.Mode == DynamicActionMode.Set };
                var response = _sendSocketService.SendRequest<ButtonImageResponse>(request);
                ImageCache[response.Name] = response.Bytes;
                ImageLoaded?.Invoke(dynamicAction);
            };
            worker.RunWorkerAsync();
        }

        public string GetImage(IDynamicAction dynamicAction) {
            var imageBytes = dynamicAction.ImageId != null && ImageCache.ContainsKey(dynamicAction.ImageId) ? ImageCache[dynamicAction.ImageId] : null;

            return ImageUtils.CreateStreamdeckImage(imageBytes, dynamicAction.IsToggled);
        }

        public bool HasImage(IDynamicAction dynamicAction) {
            //we "have" a null image because we'll just make a blank bitmap;
            if (dynamicAction.ImageId == null) {
                return true;
            }

            return ImageCache.ContainsKey(dynamicAction.ImageId);
        }
    }
}
