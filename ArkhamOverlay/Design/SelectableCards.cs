using ArkhamOverlay.Data;
using System.Collections.Generic;
using System.Windows;

namespace ArkhamOverlay.Design {
    /// <summary>
    /// Design-time instance of <see cref="ISelectableCards"/>.
    /// </summary>
    public class SelectableCards: ISelectableCards {
        public string Name => "Lola Hayes";

        public List<ICardButton> CardButtons => new List<ICardButton> { 
            new ClearButton(), 
            new Card() { Name = "Survivor Card", Faction = "Survivor" }, 
            new Card { Name = "Guardian Card", Faction = "Guardian" } 
        };

        public bool Loading => true;

        public Visibility LoadedVisiblity => Visibility.Visible;
    }
}
