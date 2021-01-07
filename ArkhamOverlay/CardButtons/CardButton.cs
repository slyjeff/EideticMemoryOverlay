using ArkhamOverlay.Data;
using PageController;
using System.Windows.Media;

namespace ArkhamOverlay.CardButtons {
    public interface ICardButton {
        string Name { get; }

        void LeftClick();

        void RightClick();
    }

    public abstract class CardButton : ViewModel, ICardButton {
        public string Name { get; set; }

        public Brush Background { get { return new SolidColorBrush(Colors.Black); } }

        public virtual Brush BorderBrush {get { return Background; } }

        public SelectableCards SelectableCards { get; set; }

        public abstract void LeftClick();

        public virtual void RightClick() {
            //by default, do nothing
        }
    }
}
