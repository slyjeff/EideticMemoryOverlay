using ArkhamOverlay.TcpUtils.Requests;
using ArkhamOverlay.TcpUtils.Responses;
using SharpDeck;
using SharpDeck.Events.Received;
using SharpDeck.Manifest;
using StreamDeckPlugin.Services;
using StreamDeckPlugin.Utils;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StreamDeckPlugin.Actions {
    [StreamDeckAction("Toggle Act/Agenda Bar", "arkhamoverlay.toggleactagendabar")]
    public class ToggleActAgendaBarAction : StreamDeckAction {
        public static IList<ToggleActAgendaBarAction> ListOf = new List<ToggleActAgendaBarAction>();
        private readonly ISendSocketService _sendSocketService = ServiceLocator.GetService<ISendSocketService>();

        public ToggleActAgendaBarAction() {
            ListOf.Add(this);
        }

        protected override Task OnWillAppear(ActionEventArgs<AppearancePayload> args) {
            //make sure the overlay app aware of us- otherwise we won't get updates
            var request = new RegisterForUpdatesRequest { Port = StreamDeckTcpInfo.Port };
            _sendSocketService.SendRequest<OkResponse>(request);

            return Task.CompletedTask;
        }

        protected override Task OnKeyUp(ActionEventArgs<KeyPayload> args) {
            _sendSocketService.SendRequest<OkResponse>(new ToggleActAgendaBarRequest());
            return Task.CompletedTask;
        }

        public void SetStatus(bool isVisible) {
            SetStateAsync(isVisible ? 0 : 1);
        }
    }
}