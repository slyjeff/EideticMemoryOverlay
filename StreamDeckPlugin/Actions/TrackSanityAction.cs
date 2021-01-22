using SharpDeck;
using SharpDeck.Manifest;
using ArkhamOverlay.TcpUtils;

namespace StreamDeckPlugin.Actions {

    [StreamDeckAction("Track Sanity", "arkhamoverlay.tracksanity")]
    public class TrackSanityAction : TrackStatAction {
        public TrackSanityAction() : base(StatType.Sanity) {
        }
    }
}