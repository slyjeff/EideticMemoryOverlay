using EideticMemoryOverlay.PluginApi;
using Emo.Common.Enums;

namespace Gloomhaven {
    internal class GloomhavenPlayer : Player {
        public GloomhavenPlayer(CardGroupId deck, IPlugIn plugIn) : base(deck, plugIn) {
        }
    }
}
