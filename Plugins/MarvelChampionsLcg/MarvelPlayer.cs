using EideticMemoryOverlay.PluginApi;
using EideticMemoryOverlay.PluginApi.Interfaces;
using Emo.Common.Enums;

namespace MarvelChampionsLcg {
    internal class MarvelPlayer : Player {
        public MarvelPlayer(ICardGroup cardGroup) : base(cardGroup) {
        }
    }
}
