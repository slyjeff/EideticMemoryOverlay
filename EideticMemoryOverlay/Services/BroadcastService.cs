using Emo.Common.Services;
using Emo.Common.Tcp;
using Emo.Common.Tcp.Requests;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Timers;

namespace Emo.Services {
    interface IBroadcastService {
        void SendRequest(Request request);

        /// <summary>
        /// Add a port that will be sent broadcast messsages
        /// </summary>
        /// <param name="port">send broadcast messages to this port</param>
        /// <remarks>this will only add a port one time; it will somply return if the port has already been added</remarks>
        void AddPort(int port);
    }

    public class BroadcastService : IBroadcastService {
        private readonly IList<int> _ports = new List<int>();
        private readonly LoggingService _logger;
        private readonly Timer _connectionIsAliveTimer = new Timer(1000 * 5); //send a connection is alive notification every 5 seconds

        private readonly object _portsLock = new object();

        public BroadcastService(LoggingService loggingService, ICrossAppEventBus crossAppEventBus) {
            _logger = loggingService;

            _connectionIsAliveTimer.Elapsed += (s, e) => {
                SendRequest(new ConnectIsAliveRequest());
            };

            crossAppEventBus.SendMessage += (request) => {
                SendRequest(request);
            };
        }

        /// <summary>
        /// Add a port that will be sent broadcast messsages
        /// </summary>
        /// <param name="port">send broadcast messages to this port</param>
        public void AddPort(int port) {
            lock (_portsLock) {
                if (_ports.Contains(port)) {
                    return;
                }

                _ports.Add(port);
                _connectionIsAliveTimer.Enabled = true;
            }
        }

        public void SendRequest(Request request) {
            //if no one is listening, don't speak!
            if (!_ports.Any()) {
                _logger.LogMessage("No ports to send status to.");
                _connectionIsAliveTimer.Enabled = false;
                return;
            }

            var worker = new BackgroundWorker();
            worker.DoWork += (x, y) => {
                var portsToRemove = new List<int>();
                lock (_portsLock) {
                    foreach (var port in _ports) {
                        if (SendSocketService.SendRequest(request, port) == null) {
                            //the sender isn't there- stop trying
                            portsToRemove.Add(port);
                        }

                        _logger.LogMessage($"Sent request to port {port}.");
                    }

                    foreach (var portToRemove in portsToRemove) {
                        _ports.Remove(portToRemove);
                    }
                }
            };
            worker.RunWorkerAsync();
        }
    }
}
