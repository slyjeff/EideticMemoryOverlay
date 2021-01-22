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
                _requestUpdatesTimer.Interval = 1000 * 30; //we've gotten communication, set the polling to 30 seconds
                _requestHandler.RequestReceivedRecently = false;
                return;
            }

            _requestUpdatesTimer.Interval = 1000 * 5; //we haven't heard lately, so start requesting more frequently

            var request = new RegisterForUpdatesRequest { Port = StreamDeckTcpInfo.Port };
            var response = StreamDeckSendSocketService.SendRequest<OkResponse>(request);
            if (response.Ok) {
                _requestUpdatesTimer.Interval = 1000 * 30; //once we are connected, set the polling to 30 seconds
            }
        }

        internal void RegisterForUpdates() {
            _requestUpdatesTimer.Enabled = true;    
        }
    }
}
