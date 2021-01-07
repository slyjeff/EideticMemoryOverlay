using ArkhamOverlay.CardButtons;
using ArkhamOverlay.Data;
using System.Collections.Generic;
using System.Windows;

namespace ArkhamOverlay.Design {
    /// <summary>
    /// Design-time instance of <see cref="ISelectableCards"/>.
    /// </summary>
    public class SelectableCards: ISelectableCards {
        public string OwnerId => throw new System.NotImplementedException();

        public SelectableType Type => SelectableType.Player;

        public string Name => "Lola Hayes";

        List<ICardButton> ISelectableCards.CardButtons => new List<ICardButton> { 
            new ClearButton(), 
            new Card() { Name = "Survivor Card", Faction = Faction.Survivor }, 
            new Card { Name = "Guardian Card", Faction = Faction.Guardian } 
        };

        public bool Loading => true;

        public Visibility LoadedVisiblity => Visibility.Visible;
    }
}
