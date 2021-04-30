using EideticMemoryOverlay.PluginApi;
using Emo.Common.Services;
using System;

namespace Emo.Events {
    public class ClearAllCardsForCardGroupRequest : IEvent {
        public ClearAllCardsForCardGroupRequest(ICardGroup cardGroup) {
            CardGroup = cardGroup;
        }

        public ICardGroup CardGroup { get; }
    }

    public static class ClearAllCardsForCardGroupRequestExtensions {
        public static void PublishClearAllCardsForCardGroupRequest(this IEventBus eventBus, ICardGroup cardGroup) {
            eventBus.Publish(new ClearAllCardsForCardGroupRequest(cardGroup));
        }

        public static void SubscribeToClearAllCardsForCardGroupRequest(this IEventBus eventBus, Action<ClearAllCardsForCardGroupRequest> callback) {
            eventBus.Subscribe(callback);
        }

        public static void UnsubscribeFromClearAllCardsForCardGroupRequest(this IEventBus eventBus, Action<ClearAllCardsForCardGroupRequest> callback) {
            eventBus.Unsubscribe(callback);
        }
    }
}
