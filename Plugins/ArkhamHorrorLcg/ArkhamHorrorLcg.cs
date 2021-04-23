using EideticMemoryOverlay.PluginApi;
using EideticMemoryOverlay.PluginApi.Buttons;
using Emo.Common.Enums;
using System.Reflection;

namespace ArkhamHorrorLcg {
    public class ArkhamHorrorLcg : PlugIn {
        public static string PlugInName = Assembly.GetExecutingAssembly().GetName().Name;

        public ArkhamHorrorLcg() : base ("Arkham Horror: The Card Game") {
        }

        public override void SetUp() {
        }

        public override CardInfoButton CreateCardInfoButton(CardInfo cardInfo, CardGroupId cardGroupId) {
            return new ArkhamCardInfoButton (cardInfo as ArkhamCardInfo, cardGroupId);
        }
    }
}
