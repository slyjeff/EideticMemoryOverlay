using EideticMemoryOverlay.PluginApi;
using Emo.Common.Enums;
using StructureMap;

namespace MarvelChampionsLcg {
    public class MarvelChampionsLcg : PlugIn {
        public MarvelChampionsLcg() : base("Marvel Champions") {
        }

        public override string LocalImagesDirectory { get; set; }

        public override void SetUp(IContainer container) {
        }

        public override Player CreatePlayer(CardGroupId playerId) {
            return new MarvelPlayer(playerId, this);
        }

        public override void LoadPlayer(Player player) {
        }
    }
}
