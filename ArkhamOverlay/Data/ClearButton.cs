using System;
using System.Windows.Media;

namespace ArkhamOverlay.Data {
    public class ClearButton : ICardButton {
        public string Name { 
            get {
                return "Clear Cards";        
            } 
        }

        public Brush Background {
            get {
                return new SolidColorBrush(Colors.Black);
            }
        }

        public event Action Clicked;

        public void Click() {
            Clicked?.Invoke();
        }
    }

}
