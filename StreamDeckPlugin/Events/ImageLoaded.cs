using StreamDeckPlugin.Services;
using System;

namespace StreamDeckPlugin.Events {
    public class ImageLoaded : IEvent {
        public ImageLoaded(string imageId) {
            ImageId = imageId;
        }

        public string ImageId { get; }
    }

    public static class ImageLoadedExtensions {
        public static void ImageLoaded(this IEventBus eventBus, string imageId) {
            eventBus.Publish(new ImageLoaded(imageId));
        }

        public static void OnImageLoaded(this IEventBus eventBus, Action<string> action) {
            eventBus.Subscribe<ImageLoaded>(x => action?.Invoke(x.ImageId));
        }
    }
}
