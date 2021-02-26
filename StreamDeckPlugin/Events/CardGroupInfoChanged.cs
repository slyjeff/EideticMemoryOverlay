using ArkhamOverlay.Common;
using ArkhamOverlay.Common.Services;
using System;

namespace StreamDeckPlugin.Events {
    public class CardGroupInfoChanged : IEvent {
        public CardGroupInfoChanged(ICardGroupInfo cardGroupInfo) {
            CardGroupInfo = cardGroupInfo;
        }

        public ICardGroupInfo CardGroupInfo { get; }
    }

    public static class CardGroupInfoChangedExtensions {
        public static void PublishStreamDeckCardGroupInfoChanged(this IEventBus eventBus, ICardGroupInfo cardGroupInfo) {
            eventBus.Publish(new CardGroupInfoChanged(cardGroupInfo));
        }

        public static void SubscribeToStreamDeckCardGroupInfoChanged(this IEventBus eventBus, Action<CardGroupInfoChanged> action) {
            eventBus.Subscribe(action);
        }

        public static void UnsubscribeFromStreamDeckCardGroupInfoChanged(this IEventBus eventBus, Action<CardGroupInfoChanged> callback) {
            eventBus.Unsubscribe(callback);
        }
    }
}
