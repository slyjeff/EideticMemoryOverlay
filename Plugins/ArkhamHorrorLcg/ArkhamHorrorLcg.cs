using EideticMemoryOverlay.PluginApi;
using Emo.Common.Enums;

namespace ArkhamHorrorLcg {
    public class ArkhamHorrorLcg : PlugIn {
        public ArkhamHorrorLcg() : base ("Arkham Horror: The Card Game") {
        }

        public override void SetUp() {
        }

        public override CardInfoButton CreateCardInfoButton(CardInfo cardInfo, CardGroupId cardGroupId) {
            return new ArkhamCardInfoButton (cardInfo as ArkhamCardInfo, cardGroupId);
        }
    }
}
