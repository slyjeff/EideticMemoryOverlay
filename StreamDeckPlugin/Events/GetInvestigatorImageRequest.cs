using ArkhamOverlay.Common.Enums;
using ArkhamOverlay.Common.Services;
using System;

namespace StreamDeckPlugin.Events {
    public class GetInvestigatorImageRequest : IEvent {
        public GetInvestigatorImageRequest(CardGroupId cardGroup) {
            CardGroup = cardGroup;
        }

        public CardGroupId CardGroup { get; }
    }

    public static class GetInvestigatorImageRequestExtensions {
        public static void PublishGetInvestigatorImageRequest(this IEventBus eventBus, CardGroupId cardGroup) {
            eventBus.Publish(new GetInvestigatorImageRequest(cardGroup));
        }

        public static void SubscribeToGetInvestigatorImageRequest(this IEventBus eventBus, Action<GetInvestigatorImageRequest> callback) {
            eventBus.Subscribe(callback);
        }
    }
}
