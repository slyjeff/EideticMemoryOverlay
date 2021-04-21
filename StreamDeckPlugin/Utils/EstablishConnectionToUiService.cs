using Emo.Common.Services;
using StreamDeckPlugin.Events;
using System.Timers;

namespace StreamDeckPlugin.Utils {
    public interface IEstablishConnectionToUiService {
        void AttemptToEstablishConnection();
    }

    public class EstablishConnectionToUiService : IEstablishConnectionToUiService {
        private readonly Timer _establishConnectionTimer = new Timer(1000 * 5); //request every 5 seconds
        private readonly ITcpRequestHandler _requestHandler;
        private readonly IEventBus _eventBus;

        public EstablishConnectionToUiService(ITcpRequestHandler requestHandler, IEventBus eventBus) {
            _requestHandler = requestHandler;
            _eventBus = eventBus;
            _establishConnectionTimer.Enabled = false;
            _establishConnectionTimer.Elapsed += SendRequest;
        }

        private void SendRequest(object sender, ElapsedEventArgs e) {
            if (_requestHandler.RequestReceivedRecently) {
                _establishConnectionTimer.Interval = 1000 * 10; //don't check for another 10 seconds once we've established connection
                _requestHandler.RequestReceivedRecently = false;
                return;
            }

            _establishConnectionTimer.Interval = 1000 * 5; //we don't have a connection, check every 5 seconds

            _eventBus.PublishEstablishConnectionToUiRequest();
        }

        public void AttemptToEstablishConnection() {
            _establishConnectionTimer.Enabled = true;    
        }
    }
}
