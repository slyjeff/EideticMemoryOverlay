using EideticMemoryOverlay.PluginApi;
using Emo.Common.Enums;
using StructureMap;

namespace Gloomhaven {
    public class Gloomhaven : PlugIn {
        public Gloomhaven() : base("Gloomhaven") {
        }

        public override void SetUp(IContainer container) {
        }

        public override Player CreatePlayer(CardGroupId playerId) {
            return new GloomhavenPlayer(playerId, this);
        }

        public override void LoadPlayer(Player player) {
        }
    }
}
