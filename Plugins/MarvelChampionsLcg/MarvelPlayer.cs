using EideticMemoryOverlay.PluginApi;
using Emo.Common.Enums;

namespace MarvelChampionsLcg {
    internal class MarvelPlayer : Player {
        public MarvelPlayer(CardGroupId deck, IPlugIn plugIn) : base(deck, plugIn) {
        }
    }
}
