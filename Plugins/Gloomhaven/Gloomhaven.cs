using EideticMemoryOverlay.PluginApi;
using EideticMemoryOverlay.PluginApi.Interfaces;
using EideticMemoryOverlay.PluginApi.LocalCards;
using Emo.Common.Enums;
using StructureMap;
using System.Windows.Controls;

namespace Gloomhaven {
    public class Gloomhaven : PlugIn {
        public override string LocalImagesDirectory { get; set; }

        public Gloomhaven() : base("Gloomhaven") {
        }

        public override void SetUp(IContainer container) {
        }

        public override Player CreatePlayer(ICardGroup cardGroup) {
            return new GloomhavenPlayer(cardGroup);
        }

        public override void LoadPlayer(Player player) {
        }

        public override void LoadPlayerCards(Player player) {
        }

        public override void LoadAllPlayerCards() {
        }

        public override void LoadEncounterCards() {
        }

        public override ILocalCardEditor CreateLocalCardEditor() {
            return default;
        }
    }
}
