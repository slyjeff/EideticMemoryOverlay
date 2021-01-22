using SharpDeck;
using SharpDeck.Manifest;
using ArkhamOverlay.TcpUtils;

namespace StreamDeckPlugin.Actions {

    [StreamDeckAction("Track Resources", "arkhamoverlay.trackresources")]
    public class TrackResourcesAction : TrackStatAction {
        public TrackResourcesAction() : base(StatType.Resources) {

        }
    }
}