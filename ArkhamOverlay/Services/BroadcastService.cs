using ArkhamOverlay.Common.Services;
using ArkhamOverlay.Common.Tcp;
using ArkhamOverlay.Common.Tcp.Requests;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace ArkhamOverlay.Services {
    interface IBroadcastService {
        void SendRequest(Request request);
        
        IList<int> Ports { get; }
    }

    public class BroadcastService : IBroadcastService {
        private readonly LoggingService _logger;

        public BroadcastService(LoggingService loggingService, ICrossAppEventBus crossAppEventBus, IEventBus eventBus) {
            Ports = new List<int>();

            _logger = loggingService;
            crossAppEventBus.SendMessage += (request) => {
                SendRequest(request);
            };
        }

        public IList<int> Ports { get; }

        public void SendRequest(Request request) {
            //if no one is listening, don't speak!
            if (!Ports.Any()) {
                _logger.LogMessage("No ports to send status to.");
                return;
            }

            var worker = new BackgroundWorker();
            worker.DoWork += (x, y) => {
                var portsToRemove = new List<int>();
                foreach (var port in Ports) {
                    try {
                        SendSocketService.SendRequest(request, port);
                        _logger.LogMessage($"Sent request to port {port}.");
                    } catch (Exception ex) {
                        //this clearly is not cool- stop trying to talk
                        _logger.LogException(ex, $"Error sending status to port {port}.");
                        portsToRemove.Add(port);
                    }
                }

                foreach (var portToRemove in portsToRemove) {
                    Ports.Remove(portToRemove);
                }
            };
            worker.RunWorkerAsync();
        }
    }
}
