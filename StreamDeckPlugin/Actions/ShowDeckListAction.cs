using ArkhamOverlay.TcpUtils;
using ArkhamOverlay.TcpUtils.Requests;
using Newtonsoft.Json.Linq;
using SharpDeck;
using SharpDeck.Events.Received;
using SharpDeck.Manifest;
using StreamDeckPlugin.Events;
using StreamDeckPlugin.Services;
using StreamDeckPlugin.Utils;
using System;
using System.Threading.Tasks;

namespace StreamDeckPlugin.Actions {
    public class ShowDeckSettings {
        public string Deck { get; set; }
    }


    [StreamDeckAction("Show Deck List", "arkhamoverlay.showdecklist")]
    public class ShowDeckListAction : StreamDeckAction {
        private readonly IEventBus _eventBus = ServiceLocator.GetService<IEventBus>();
        private ActionWithDeckSettings _settings = new ActionWithDeckSettings();

        public ShowDeckListAction() {
            _eventBus.SubscribeToInvestigatorImageUpdatedEvent(InvestigatorImageUpdated);
        }

        private void InvestigatorImageUpdated(InvestigatorImageUpdatedEvent investigatorImageUpdatedEvent) {
            if (investigatorImageUpdatedEvent.Deck == Deck) {
                SetImageAsync(ImageUtils.CreateStreamdeckImage(investigatorImageUpdatedEvent.Bytes));
            }
        }

        public Deck Deck {
            get {
                if (_settings == null) {
                    return Deck.Player1;
                }

                return _settings.Deck.AsDeck();
            }
        }

        protected override Task OnWillAppear(ActionEventArgs<AppearancePayload> args) {
            _settings = args.Payload.GetSettings<ActionWithDeckSettings>();

            _eventBus.PublishGetInvestigatorImageRequest(Deck);

            return Task.CompletedTask;
        }

        protected override Task OnKeyUp(ActionEventArgs<KeyPayload> args) {
            _eventBus.PublishShowDeckListRequest(Deck);
            return Task.CompletedTask;
        }

        protected async override Task OnSendToPlugin(ActionEventArgs<JObject> args) {
            _settings.Deck = args.Payload["deck"].Value<string>();

            _eventBus.PublishGetInvestigatorImageRequest(Deck);

            await SetSettingsAsync(_settings);
        }
    }
}