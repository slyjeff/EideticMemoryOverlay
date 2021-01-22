using SharpDeck;
using SharpDeck.Manifest;
using ArkhamOverlay.TcpUtils;

namespace StreamDeckPlugin.Actions {

    [StreamDeckAction("Track Health", "arkhamoverlay.trackhealth")]
    public class TrackHealthAction : TrackStatAction {
        public TrackHealthAction() : base(StatType.Health) {
        }
    }
}