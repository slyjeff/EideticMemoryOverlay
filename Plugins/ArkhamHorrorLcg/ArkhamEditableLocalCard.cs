namespace EideticMemoryOverlay.PluginApi.LocalCards {
    public class ArkhamEditableLocalCard : EditableLocalCard {
        private string _cardType;
        public virtual string CardType {
            get => _cardType;
            set {
                _cardType = value;
                NotifyPropertyChanged(nameof(CardType));
            }
        }

        private string _arkhamDbId;
        public virtual string ArkhamDbId {
            get => _arkhamDbId;
            set {
                _arkhamDbId = value;
                NotifyPropertyChanged(nameof(ArkhamDbId));
            }
        }
    }

}
