using SharpDeck;
using SharpDeck.Manifest;
using ArkhamOverlay.TcpUtils;

namespace StreamDeckPlugin.Actions {

    [StreamDeckAction("Track Clues", "arkhamoverlay.trackclues")]
    public class TrackCluesAction : TrackStatAction {
        public TrackCluesAction() : base(StatType.Clues) {

        }
    }
}