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

        public override Player CreatePlayer(ICardGroup cardGroup) {
            return new MarvelPlayer(cardGroup);
        }

        public override void LoadPlayer(Player player) {
        }

        public override void LoadPlayerCards(Player player) {
        }

        public override void LoadAllPlayerCards() {
        }

        public override void LoadEncounterCards() {
        }
    }
}
