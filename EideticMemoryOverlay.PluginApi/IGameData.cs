using Emo.Common.Enums;
using System;
using System.Collections.Generic;

namespace EideticMemoryOverlay.PluginApi {
    public interface IGameData {
        string FileName { get; set; }
        
        string SnapshotDirectory { get; set; }

        event Action PlayersChanged;

        event Action EncounterSetsChanged;

        IList<Player> Players { get; }

        ICardGroup ScenarioCards { get; }

        ICardGroup LocationCards { get; }

        ICardGroup EncounterDeckCards { get; }

        bool IsEncounterSetSelected(string code);

        IList<string> LocalPacks { get; set; }
        IList<EncounterSet> EncounterSets { get; set; }

        IList<ICardGroup> AllCardGroups { get; }

        void ClearAllCardsLists();

        void OnEncounterSetsChanged();

        /// <summary>
        /// Setup the game using plugin specific logic
        /// </summary>
        /// <param name="plugIn">The plugin to use</param>
        void InitializeFromPlugin(IPlugIn plugIn);

        /// <summary>
        /// Add a card (create a button) to a zone
        /// </summary>
        /// <param name="cardGroupId">Add to a zone of this card group</param>
        /// <param name="zoneIndex">Add to this zone</param>
        /// <param name="button">Source button for creating this new button</param>
        void AddCardToZone(CardGroupId cardGroupId, int zoneIndex, CardImageButton button);

        /// <summary>
        /// Find the card Group using the ID
        /// </summary>
        /// <param name="cardGroupId">Unique ID for a group</param>
        /// <returns>The group matching the passed in ID</returns>
        ICardGroup GetCardGroup(CardGroupId cardGroupId);
    }
}