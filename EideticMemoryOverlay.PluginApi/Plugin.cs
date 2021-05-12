using EideticMemoryOverlay.PluginApi.Buttons;
using EideticMemoryOverlay.PluginApi.Interfaces;
using EideticMemoryOverlay.PluginApi.LocalCards;
using Emo.Common.Enums;
using StructureMap;
using System;
using System.Collections.Generic;
using System.Windows.Controls;

namespace EideticMemoryOverlay.PluginApi {
    /// <summary>
    /// Plugin Specific logic available to the application
    /// </summary>
    public interface IPlugIn {
        /// <summary>
        /// Displayed name of the plug in
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Do any initialization the plugin requires
        /// </summary>
        /// <param name="Container">Structure Map container for dependency injection</param>
        void SetUp(IContainer container);

        /// <summary>
        /// Type type used for local cards
        /// </summary>
        Type LocalCardType { get; }

        /// <summary>
        /// Create an editor for cards using custom plugin logic
        /// </summary>
        /// <returns>User control to show on local card page</returns>
        ILocalCardEditor CreateLocalCardEditor();

        /// <summary>
        /// create a new instance of an editable local card, containing all properties defined by the plugin
        /// </summary>
        /// <returns>editable local card that contains all properties defined by the plugin</returns>
        EditableLocalCard CreateEditableLocalCard();

        /// <summary>
        /// List of packs (that container selectable encounter sets) that can be used for this plug in
        /// </summary>
        IList<Pack> Packs { get; }

        /// <summary>
        /// Directory where local images are stores for this plug in
        /// </summary>
        string LocalImagesDirectory { get; set; }

        // <summary>
        /// Create a player using the plugin, containing any plugin necessary logic
        /// </summary>
        /// <param name="cardGroup">card group for the player</param>
        /// <returns>A player created by the plugin</returns>
        Player CreatePlayer(ICardGroup cardGroup);

        /// <summary>
        /// Create a card info button using plugin specific logic
        /// </summary>
        /// <param name="cardInfo">card info that this button represents</param>
        /// <param name="cardGroupId">card group that this buttton belongs to- used by plug ins to determine contextual logic for right click options</param>
        /// <returns>Card info button with options determined by the plugin</returns>
        CardInfoButton CreateCardInfoButton(CardInfo cardInfo, CardGroupId cardGroupId);
        
        /// <summary>
        /// Load information about a player
        /// </summary>
        /// <param name="player">the player to load</param>
        void LoadPlayer(Player player);

        /// <summary>
        /// Load the cards for a player
        /// </summary>
        /// <param name="player"></param>
        void LoadPlayerCards(Player player);

        /// <summary>
        /// Load cards for all players
        /// </summary>
        void LoadAllPlayerCards();

        /// <summary>
        /// Load all encounter cards
        /// </summary>
        void LoadEncounterCards();
    }

    /// <summary>
    /// Plugins must inherit from this class to provide an entry point to the application
    /// </summary>
    public abstract class PlugIn {
        protected PlugIn(string name) {
            Name = name;
        }

        /// <summary>
        /// Do any initialization the plugin requires
        /// </summary>
        /// <param name="Container">Structure Map container for dependency injection</param>
        public abstract void SetUp(IContainer container);

        /// <summary>
        /// Displayed name of the plug in
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Type type used for local cards
        /// </summary>
        public virtual Type LocalCardType { get { return typeof(LocalCard); } }

        /// <summary>
        /// create a new instance of an editable local card, containing all properties defined by the plugin
        /// </summary>
        /// <returns>editable local card that contains all properties defined by the plugin</returns>
        public virtual EditableLocalCard CreateEditableLocalCard() {
            return new EditableLocalCard();
        }

        /// <summary>
        /// List of packs (that container selectable encounter sets) that can be used for this plug in
        /// </summary>
        public virtual IList<Pack> Packs { get { return new List<Pack>(); } }

        /// <summary>
        /// Create a card info button using plugin specific logic
        /// </summary>
        /// <param name="cardInfo">card info that this button represents</param>
        /// <param name="cardGroupId">card group that this buttton belongs to- used by plug ins to determine contextual logic for right click options</param>
        /// <returns>Card info button with options determined by the plugin</returns>
        public virtual CardInfoButton CreateCardInfoButton(CardInfo cardInfo, CardGroupId cardGroupId) {
            return new CardInfoButton(cardInfo);
        }

        /// <summary>
        /// Create a player using the plugin, containing any plugin necessary logic
        /// </summary>
        /// <param name="cardGroup">card group for the player</param>
        /// <returns>A player created by the plugin</returns>
        public abstract Player CreatePlayer(ICardGroup cardGroup);

        /// <summary>
        /// Load information about a player
        /// </summary>
        /// <param name="player">the player to load</param>
        public abstract void LoadPlayer(Player player);

        /// <summary>
        /// Load the cards for a player
        /// </summary>
        /// <param name="player"></param>
        public abstract void LoadPlayerCards(Player player);

        /// <summary>
        /// Load cards for all players
        /// </summary>
        public abstract void LoadAllPlayerCards();

        /// <summary>
        /// Load all encounter cards
        /// </summary>
        public abstract void LoadEncounterCards();

        /// <summary>
        /// Directory where local images are stores for this plug in
        /// </summary>
        public abstract string LocalImagesDirectory { get; set; }

        /// <summary>
        /// Create an editor for cards using custom plugin logic
        /// </summary>
        /// <returns>User control to show on local card page</returns>
        public abstract ILocalCardEditor CreateLocalCardEditor();
    }
}
