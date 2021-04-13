using Emo.Common;
using Emo.Common.Enums;
using Emo.Common.Services;
using System;
using System.Collections.Generic;

namespace Emo.Events {
    public class CardGroupChanged : ICrossAppEvent, ICardGroupInfo {
        public CardGroupChanged(CardGroupId cardGroupId, string name, bool isImageAvailable, string imageId, IList<string> zones) {
            CardGroupId = cardGroupId;
            Name = name;
            IsImageAvailable = isImageAvailable;
            ImageId = imageId;
            Zones = zones;
        }

        public CardGroupId CardGroupId { get; }
        public string Name { get; }
        public bool IsImageAvailable { get; }
        public string ImageId { get; }
        public IList<string> Zones { get; }
    }

    public static class CardGroupChangedExtensions {
        public static void PublishCardGroupChanged(this IEventBus eventBus, CardGroupId cardGroupId, string name, bool isImageAvailable, string imageId, IList<string> zones) {
            eventBus.Publish(new CardGroupChanged(cardGroupId, name, isImageAvailable, imageId, zones));
        }

        public static void SubscribeToCardGroupChanged(this IEventBus eventBus, Action<CardGroupChanged> callback) {
            eventBus.Subscribe(callback);
        }
        public static void UnsubscribeFromCardGroupChanged(this IEventBus eventBus, Action<CardGroupChanged> callback) {
            eventBus.Unsubscribe(callback);
        }
    }
}
