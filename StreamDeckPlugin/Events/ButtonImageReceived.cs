using StreamDeckPlugin.Services;
using System;

namespace StreamDeckPlugin.Events {
    public class ButtonImageReceived : IEvent {
        public ButtonImageReceived(string imageId, byte[] bytes) {
            ImageId = imageId;
            Bytes = bytes;
        }

        public string ImageId{ get; }
        public byte[] Bytes { get; }
    }

    public static class ButtonImageReceivedExtensions {
        public static void ButtonImageReceived(this IEventBus eventBus, string imageId, byte[] bytes) {
            eventBus.Publish(new ButtonImageReceived(imageId, bytes));
        }

        public static void OnButtonImageReceived(this IEventBus eventBus, Action<string, byte[]> action) {
            eventBus.Subscribe<ButtonImageReceived>(x => action?.Invoke(x.ImageId, x.Bytes));
        }
    }
}
