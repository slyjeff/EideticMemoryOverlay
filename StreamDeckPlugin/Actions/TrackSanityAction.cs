using Emo.Common.Enums;
using SharpDeck;
using SharpDeck.Manifest;

namespace StreamDeckPlugin.Actions {

    [StreamDeckAction("Track Sanity", "arkhamoverlay.tracksanity")]
    public class TrackSanityAction : TrackStatAction {
        public TrackSanityAction() : base(StatType.Sanity) {
        }
    }
}