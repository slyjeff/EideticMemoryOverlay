using ArkhamOverlay.Common.Enums;
using ArkhamOverlay.Common.Services;
using System;

namespace StreamDeckPlugin.Events {
    public class DynamicButtonClickRequest : IEvent, IButtonContext {
        public DynamicButtonClickRequest(CardGroupId cardGroup, int cardZoneIndex, int index, bool isLeftClick) {
            CardGroup = cardGroup;
            CardZoneIndex = cardZoneIndex;
            Index = index;
            IsLeftClick = isLeftClick;
        }

        public CardGroupId CardGroup { get; }
        public int CardZoneIndex { get; }
        public int Index { get; }
        public bool IsLeftClick { get; }
    }

    public static class DynamicButtonClickExtensions {
        public static void PublishDynamicButtonClickRequest(this IEventBus eventBus, CardGroupId cardGroup, int cardZoneIndex, int index, bool isLeftClick) {
            eventBus.Publish(new DynamicButtonClickRequest(cardGroup, cardZoneIndex, index, isLeftClick));
        }

        public static void SubscribeToDynamicButtonClickRequest(this IEventBus eventBus, Action<DynamicButtonClickRequest> callback) {
            eventBus.Subscribe(callback);
        }
    }
}
