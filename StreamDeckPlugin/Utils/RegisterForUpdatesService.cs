using ArkhamOverlay.TcpUtils.Requests;
using ArkhamOverlay.TcpUtils.Responses;
using StreamDeckPlugin.Events;
using StreamDeckPlugin.Services;
using System.Timers;

namespace StreamDeckPlugin.Utils {
    public interface IRegisterForUpdatesService {
        void RegisterForUpdates();
    }

    public class RegisterForUpdatesService : IRegisterForUpdatesService {
        private readonly Timer _requestUpdatesTimer = new Timer(1000 * 5); //request every 5 seconds
        private readonly TcpRequestHandler _requestHandler;
        private readonly IEventBus _eventBus;

        public RegisterForUpdatesService(TcpRequestHandler requestHandler, IEventBus eventBus) {
            _requestHandler = requestHandler;
            _eventBus = eventBus;
            _requestUpdatesTimer.Enabled = false;
            _requestUpdatesTimer.Elapsed += SendRequest;
        }

        private void SendRequest(object sender, ElapsedEventArgs e) {
            if (_requestHandler.RequestReceivedRecently) {
                _requestHandler.RequestReceivedRecently = false;
                return;
            }

            _eventBus.PublishRegisterForUpdatesRequest();
        }

        public void RegisterForUpdates() {
            _requestUpdatesTimer.Enabled = true;    
        }
    }
}
