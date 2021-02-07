using ArkhamOverlay.Common.Enums;
using SharpDeck;
using SharpDeck.Manifest;

namespace StreamDeckPlugin.Actions {

    [StreamDeckAction("Track Health", "arkhamoverlay.trackhealth")]
    public class TrackHealthAction : TrackStatAction {
        public TrackHealthAction() : base(StatType.Health) {
        }
    }
}