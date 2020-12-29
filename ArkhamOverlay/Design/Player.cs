using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace ArkhamOverlay.Design {
    /// <summary>
    /// Design-time instance of <see cref="IPlayer"/>.
    /// </summary>
    public class Player: IPlayer {
        public string Investigator => "Lola Hayes";

        public string DeckId => throw new NotImplementedException();

        public BitmapImage InvestigatorImage => throw new NotImplementedException();

        public List<IPlayerButton> PlayerButtons => new List<IPlayerButton> { new ClearButton(), new Card() { Name = "Survivor Card", Faction = "Survivor" }, new Card { Name = "Guardian Card", Faction = "Guardian" } };

        public IEnumerable<string> CardIds => throw new NotImplementedException();

        public bool Loading => true;

        public Visibility LoadedVisiblity => Visibility.Visible;
    }
}
