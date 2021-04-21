using Emo.Common.Enums;
using SharpDeck;
using SharpDeck.Manifest;

namespace StreamDeckPlugin.Actions {

    [StreamDeckAction("Track Clues", "emo.trackclues")]
    public class TrackCluesAction : TrackStatAction {
        public TrackCluesAction() : base(StatType.Clues) {

        }
    }
}