using System.Windows.Media;

namespace EideticMemoryOverlay.PluginApi {
    public class DeckListItem {
        private CardInfo _card;
        private string _name;

        public DeckListItem(CardInfo card) {
            _card = card;
        }

        public DeckListItem(string name) {
            _name = name;
        }

        public string Name {
            get {
                if (_card == null) {
                    return _name;
                }

                return _card.DeckListName;
            }
        }

        public Brush Foreground {
            get {
                if (_card == null) {
                    return new SolidColorBrush(Colors.Black);
                }

                return _card.DeckListColor;
            }
        }
    }

}
