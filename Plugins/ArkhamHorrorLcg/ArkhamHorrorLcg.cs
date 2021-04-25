using ArkhamHorrorLcg.ArkhamDb;
using EideticMemoryOverlay.PluginApi;
using EideticMemoryOverlay.PluginApi.Buttons;
using Emo.Common.Enums;
using StructureMap;
using System;
using System.Reflection;

namespace ArkhamHorrorLcg {
    public class ArkhamHorrorLcg : PlugIn {
        public static string PlugInName = Assembly.GetExecutingAssembly().GetName().Name;
        private IContainer _container;
        private IArkhamConfiguration _configuration;

        public ArkhamHorrorLcg() : base ("Arkham Horror: The Card Game") {
        }

        public override void SetUp(IContainer container) {
            _container = container;

            container.Configure(x => {
                x.For<IArkhamConfiguration>().Use<ArkhamConfiguration>().Singleton();
                x.For<IArkhamDbService>().Use<ArkhamDbService>();
                x.For<ICardLoadService>().Use<CardLoadService>();
                x.For<IPackLoader>().Use<PackLoader>();
            });

            _configuration = container.GetInstance<IArkhamConfiguration>();

            var packLoader = container.GetInstance<IPackLoader>();
            packLoader.FindMissingEncounterSets();
        }

        public override CardInfoButton CreateCardInfoButton(CardInfo cardInfo, CardGroupId cardGroupId) {
            return new ArkhamCardInfoButton (cardInfo as ArkhamCardInfo, cardGroupId);
        }

        public override Player CreatePlayer(CardGroupId playerId) {
            return new ArkhamPlayer(playerId, this);
        }

        public override void LoadPlayer(Player player) {
            var arkhamPlayer = player as ArkhamPlayer;
            if (arkhamPlayer == default) {
                return;
            }

            var cardLoadService = _container.GetInstance<ICardLoadService>();
            cardLoadService.LoadPlayer(arkhamPlayer);
        }

        public override Type LocalCardType { get { return typeof(ArkhamLocalCard); } }

        public override string LocalImagesDirectory {
            get {
                return _configuration.LocalImagesDirectory;
            }
            set {
                _configuration.LocalImagesDirectory = value;
                _configuration.Save();
            }
        }
    }
}
