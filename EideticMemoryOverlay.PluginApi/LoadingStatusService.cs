using System;
using System.Text;

namespace EideticMemoryOverlay.PluginApi {
    public enum Status {
        Finished,
        LoadingDeck,
        LoadingCards,
        Error,
    }

    public class LoadingStatusService {
        private const int ENCOUNTER_DECK = -1;

        private object syncObject = new object();

        private AppData _appData;

        private Tuple<int, Status> _lastStatus;

        public LoadingStatusService(AppData appData) {
            _appData = appData;
        }

        public void ReportPlayerStatus(int id, Status status) {
            lock (syncObject) {
                ReportStatus(id, status);
            }
        }

        public void ReportEncounterCardsStatus(Status status) {
            lock (syncObject) {
                ReportStatus(ENCOUNTER_DECK, status);
            }
        }

        private void ReportStatus(int id, Status newStatus) {
            string statusString = null;
            if (_lastStatus == null) {
                statusString = CreateMessage(id, newStatus);
            } else {
                var lastUpdated = _lastStatus.Item1;
                var lastStatus = _lastStatus.Item2;
                if (newStatus == lastStatus) {
                    // If multiple players have the same status, aggregate the message
                    statusString = CreateAggregateMessage(newStatus);
                } else if (id == lastUpdated || newStatus > lastStatus) {
                    // If the status is an update on the last updated player
                    // or if the new status has a higher priority, update the message.
                    statusString = CreateMessage(id, newStatus);
                }
            }

            if (!string.IsNullOrWhiteSpace(statusString)) {
                _appData.Status = statusString;
            }

            _lastStatus = new Tuple<int, Status>(id, newStatus);
        }

        private string CreateMessage(int id, Status status) {
            var sb = new StringBuilder();

            switch (status) {
                case Status.LoadingDeck:
                case Status.LoadingCards:
                    sb.Append("Loading");
                    break;
                case Status.Finished:
                    sb.Append("Finished loading");
                    break;
                case Status.Error:
                    sb.Append("Error loading");
                    break;
                default:
                    return string.Empty;
            }

            switch (id) {
                case ENCOUNTER_DECK:
                    if (status == Status.LoadingDeck) {
                        sb.Append(" encounter sets.");
                    } else {
                        sb.Append(" encounter cards.");
                    }
                    break;
                default:
                    if (status == Status.LoadingDeck) {
                        sb.Append($" player {id} deck.");
                    } else {
                        sb.Append($" player {id} cards.");
                    }
                    break;
            }

            return sb.ToString();
        }

        private string CreateAggregateMessage(Status status) {
            switch (status) {
                case Status.LoadingDeck:
                case Status.LoadingCards:
                    return "Loading player and/or encounter cards.";
                case Status.Finished:
                    return "Finished loading cards.";
                case Status.Error:
                    return "Error loading player and/or encounter cards.";
                default:
                    return string.Empty;
            }
        }
    }
}
