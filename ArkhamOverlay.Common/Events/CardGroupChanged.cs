using ArkhamOverlay.Common;
using ArkhamOverlay.Common.Enums;
using ArkhamOverlay.Common.Services;
using System;

namespace ArkhamOverlay.Events {
    public class CardGroupChanged : ICrossAppEvent, ICardGroupInfo {
        public CardGroupChanged(CardGroupId cardGroupId, string name, bool isImageAvailable, string imageId) {
            CardGroupId = cardGroupId;
            Name = name;
            IsImageAvailable = isImageAvailable;
            ImageId = imageId;
        }

        public CardGroupId CardGroupId { get; }
        public string Name { get; }
        public bool IsImageAvailable { get; }
        public string ImageId { get; }
    }

    public static class CardGroupChangedExtensions {
        public static void PublishCardGroupChanged(this IEventBus eventBus, CardGroupId cardGroupId, string name, bool isImageAvailable, string imageId) {
            eventBus.Publish(new CardGroupChanged(cardGroupId, name, isImageAvailable, imageId));
        }

        public static void SubscribeToCardGroupChanged(this IEventBus eventBus, Action<CardGroupChanged> callback) {
            eventBus.Subscribe(callback);
        }
        public static void UnsubscribeFromCardGroupChanged(this IEventBus eventBus, Action<CardGroupChanged> callback) {
            eventBus.Unsubscribe(callback);
        }
    }
}
