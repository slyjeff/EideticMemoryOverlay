using ArkhamOverlay.TcpUtils.Requests;
using ArkhamOverlay.TcpUtils.Responses;
using StreamDeckPlugin.Services;
using System.Timers;

namespace StreamDeckPlugin.Utils {
    public interface IRegisterForUpdatesService {
        void RegisterForUpdates();
    }

    public class RegisterForUpdatesService : IRegisterForUpdatesService {
        private readonly Timer _requestUpdatesTimer = new Timer(1000 * 5); //request every 5 seconds
        private readonly TcpRequestHandler _requestHandler;
        private readonly ISendSocketService _sendSocketService;

        public RegisterForUpdatesService(TcpRequestHandler requestHandler, ISendSocketService sendSocketService) {
            _requestHandler = requestHandler;
            _sendSocketService = sendSocketService;
            _requestUpdatesTimer.Enabled = false;
            _requestUpdatesTimer.Elapsed += SendRequest;
        }

        private void SendRequest(object sender, ElapsedEventArgs e) {
            if (_requestHandler.RequestReceivedRecently) {
                _requestHandler.RequestReceivedRecently = false;
                return;
            }

            var request = new RegisterForUpdatesRequest { Port = StreamDeckTcpInfo.Port };
            _sendSocketService.SendRequest<OkResponse>(request);
        }

        public void RegisterForUpdates() {
            _requestUpdatesTimer.Enabled = true;    
        }
    }
}
