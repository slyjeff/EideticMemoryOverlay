using EideticMemoryOverlay.PluginApi.LocalCards;

namespace ArkhamHorrorLcg {
    class ArkhamLocalCard : LocalCard {
        internal string CardType { get; set; }
        internal string ArkhamDbId { get; set; }

        public override void CopyTo(EditableLocalCard editableLocalCard) {
            base.CopyTo(editableLocalCard);

            var arkhamEditableLocalCard = editableLocalCard as ArkhamEditableLocalCard;
            if (arkhamEditableLocalCard == default) {
                return;
            }

            arkhamEditableLocalCard.CardType = CardType;
            arkhamEditableLocalCard.ArkhamDbId = ArkhamDbId;
        }

        public override void CopyFrom(EditableLocalCard editableLocalCard) {
            base.CopyFrom(editableLocalCard);

            var arkhamEditableLocalCard = editableLocalCard as ArkhamEditableLocalCard;
            if (arkhamEditableLocalCard == default) {
                return;
            }

            CardType = arkhamEditableLocalCard.CardType;
            ArkhamDbId = arkhamEditableLocalCard.ArkhamDbId;
        }
    }
}
