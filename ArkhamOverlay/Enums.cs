using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArkhamOverlay {
    public enum CardType {
        Player,
        Scenario,
        Agenda,
        Act,
        Enemy,
        Treachery,
        Location,
        Other,
    }

    public enum Faction {
        Guardian,
        Seeker,
        Rogue,
        Mystic,
        Survivor,
        Neutral,
        Other,
    }

    public enum SelectableType {
        Player,
        Scenario,
        Location,
        Encounter,
    }
}
