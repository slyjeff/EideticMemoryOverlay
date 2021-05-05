using EideticMemoryOverlay.PluginApi;
using EideticMemoryOverlay.PluginApi.Interfaces;
using Emo.Common.Enums;

namespace Gloomhaven {
    internal class GloomhavenPlayer : Player {
        public GloomhavenPlayer(ICardGroup cardGroup) : base(cardGroup) {
        }
    }
}
