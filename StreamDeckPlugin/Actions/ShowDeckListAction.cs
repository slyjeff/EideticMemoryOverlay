using ArkhamOverlay.TcpUtils;
using ArkhamOverlay.TcpUtils.Requests;
using Newtonsoft.Json.Linq;
using SharpDeck;
using SharpDeck.Events.Received;
using SharpDeck.Manifest;
using StreamDeckPlugin.Utils;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StreamDeckPlugin.Actions {
    public class ShowDeckSettings {
        public string Deck { get; set; }
    }


    [StreamDeckAction("Show Deck List", "arkhamoverlay.showdecklist")]
    public class ShowDeckListAction : StreamDeckAction {
        public static IList<ShowDeckListAction> ListOf = new List<ShowDeckListAction>();

        private ShowDeckSettings _settings = new ShowDeckSettings();

        public ShowDeckListAction() {
            ListOf.Add(this);
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
            _settings = args.Payload.GetSettings<ShowDeckSettings>();
            return Task.CompletedTask;
        }

        protected override Task OnKeyUp(ActionEventArgs<KeyPayload> args) {
            StreamDeckSendSocketService.SendRequest(new ShowDeckListRequest { Deck = Deck } );
            return Task.CompletedTask;
        }

        protected async override Task OnSendToPlugin(ActionEventArgs<JObject> args) {
            _settings.Deck = args.Payload["deck"].Value<string>();

            await SetSettingsAsync(_settings);
        }
    }
}