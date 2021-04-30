using EideticMemoryOverlay.PluginApi.Buttons;
using Emo.Common.Enums;
using Emo.Common.Services;
using Emo.Common.Utils;
using Emo.Events;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace EideticMemoryOverlay.PluginApi {
    public interface ICardGroup {
        CardGroupId Id { get; }
        CardGroupType Type { get; }
        string Name { get; set; }
        List<IButton> CardButtons { get; }
        IList<CardZone> CardZones { get; }
        IEnumerable<CardInfo> CardPool { get; }
        bool Loading { get; set; }

        /// <summary>
        /// Notify listeners that the card group has changed
        /// </summary>
        void PublishCardGroupChanged();

        /// <summary>
        /// Remove all CardInfos from the pool and all card zones
        /// </summary>
        void ClearCards();

        /// <summary>
        /// Replace the cards in this group with a new set of cards
        /// </summary>
        /// <param name="cards">new cards to use</param>
        void LoadCards(IEnumerable<CardInfo> cards);

        /// <summary>
        /// Image associated with this card group
        /// </summary>
        byte[] ButtonImageAsBytes { get; set; }

        /// <summary>
        /// Add a Card Zone to this Card Group
        /// </summary>
        /// <param name="cardZone">Card Zone to add</param>
        void AddCardZone(CardZone cardZone);

        /// <summary>
        /// Retrieve a Card Zone by index
        /// </summary>
        /// <param name="index"></param>
        /// <returns>Card Zone at the index- default(CardZone) if does not exist</returns>
        CardZone GetCardZone(int index);

        /// <summary>
        /// Get a list of all buttons in this card group 
        /// </summary>
        /// <returns>list of button info for every button in this card group</returns>
        /// <remark>used for sending update events</remark>
        IList<ButtonInfo> GetButtonInfo();

        /// <summary>
        /// Find a button
        /// </summary>
        /// <param name="context">Information to find the button</param>
        /// <returns>The button identified by the context- default if not found</returns>
        IButton GetButton(IButtonContext context);

        /// <summary>
        /// Find a button
        /// </summary>
        /// <param name="buttonMode">Look in the pool or in card zones</param>
        /// <param name="zoneIndex">index of zone the button- does not apply for pool</param>
        /// <param name="index">index of the button</param>
        /// <returns>The button identified by the parameters- default if not found</returns>
        IButton GetButton(ButtonMode buttonMode, int zoneIndex, int index);

        /// <summary>
        /// Look through all card zones to find this button and remove it
        /// </summary>
        /// <param name="button">The button to remove</param>
        void RemoveCard(CardButton button);
    }
}
