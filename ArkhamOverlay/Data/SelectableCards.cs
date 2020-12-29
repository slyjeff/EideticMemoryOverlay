using System.Collections.Generic;
using System.ComponentModel;

namespace ArkhamOverlay.Data {
    public interface ISelectableCards {
        string Name { get; }

        List<ICardButton> CardButtons { get; }

        bool Loading { get; }
    }

    public class SelectableCards : ISelectableCards, INotifyPropertyChanged {
        public SelectableCards() {
            CardButtons = new List<ICardButton>();
        }

        public string Name { get; set; }

        public List<ICardButton> CardButtons { get; set; }

        public bool Loading { get; internal set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string propertyName) {
            var handler = PropertyChanged;
            if (handler == null) {
                return;
            }
            handler(this, new PropertyChangedEventArgs(propertyName));
        }

        public void OnCardButtonsChanged() {
            OnPropertyChanged(nameof(CardButtons));
        }

        internal void Load(IEnumerable<Card> cards) {
            var playerButtons = new List<ICardButton> { new ClearButton() };
            playerButtons.AddRange(cards);
            CardButtons = playerButtons;
            OnCardButtonsChanged();
        }
    }
}
