using ArkhamOverlay.TcpUtils.Requests;
using ArkhamOverlay.TcpUtils.Responses;
using System.Timers;

namespace StreamDeckPlugin.Utils {
    public class RegisterForUpdatesService {
        private readonly Timer _requestUpdatesTimer = new Timer(1000 * 5); //request every 5 seconds
        private readonly TcpRequestHandler _requestHandler;

        public RegisterForUpdatesService(TcpRequestHandler requestHandler) {
            _requestHandler = requestHandler;
            _requestUpdatesTimer.Enabled = false;
            _requestUpdatesTimer.Elapsed += SendRequest;
        }

        private void SendRequest(object sender, ElapsedEventArgs e) {
            if (_requestHandler.RequestReceivedRecently) {
                _requestHandler.RequestReceivedRecently = false;
                return;
            }

            var request = new RegisterForUpdatesRequest { Port = StreamDeckTcpInfo.Port };
            StreamDeckSendSocketService.SendRequest<OkResponse>(request);
        }

        internal void RegisterForUpdates() {
            _requestUpdatesTimer.Enabled = true;    
        }
    }
}
