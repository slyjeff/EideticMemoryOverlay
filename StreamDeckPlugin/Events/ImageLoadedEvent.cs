using Emo.Common.Services;
using System;

namespace StreamDeckPlugin.Events {
    public class ImageLoadedEvent : IEvent {
        public ImageLoadedEvent(string imageId) {
            ImageId = imageId;
        }

        public string ImageId { get; }
    }

    public static class ImageLoadedEventExtensions {
        public static void PublishImageLoadedEvent(this IEventBus eventBus, string imageId) {
            eventBus.Publish(new ImageLoadedEvent(imageId));
        }

        public static void SubscribeToImageLoadedEvent(this IEventBus eventBus, Action<ImageLoadedEvent> callback) {
            eventBus.Subscribe(callback);
        }
    }
}
