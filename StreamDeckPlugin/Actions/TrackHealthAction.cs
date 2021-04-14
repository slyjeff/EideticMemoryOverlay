using Emo.Common.Enums;
using SharpDeck;
using SharpDeck.Manifest;

namespace StreamDeckPlugin.Actions {

    [StreamDeckAction("Track Health", "emo.trackhealth")]
    public class TrackHealthAction : TrackStatAction {
        public TrackHealthAction() : base(StatType.Health) {
        }
    }
}