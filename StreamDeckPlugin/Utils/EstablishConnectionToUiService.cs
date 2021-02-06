using StreamDeckPlugin.Events;
using StreamDeckPlugin.Services;
using System.Timers;

namespace StreamDeckPlugin.Utils {
    public interface IEstablishConnectionToUiService {
        void AttemptToEstablishConnection();
    }

    public class EstablishConnectionToUiService : IEstablishConnectionToUiService {
        private readonly Timer _establishConnectionTimer = new Timer(1000 * 5); //request every 5 seconds
        private readonly TcpRequestHandler _requestHandler;
        private readonly IEventBus _eventBus;

        public EstablishConnectionToUiService(TcpRequestHandler requestHandler, IEventBus eventBus) {
            _requestHandler = requestHandler;
            _eventBus = eventBus;
            _establishConnectionTimer.Enabled = false;
            _establishConnectionTimer.Elapsed += SendRequest;
        }

        private void SendRequest(object sender, ElapsedEventArgs e) {
            if (_requestHandler.RequestReceivedRecently) {
                _requestHandler.RequestReceivedRecently = false;
                return;
            }

            _eventBus.PublishEstablishConnectionToUiRequest();
        }

        public void AttemptToEstablishConnection() {
            _establishConnectionTimer.Enabled = true;    
        }
    }
}
