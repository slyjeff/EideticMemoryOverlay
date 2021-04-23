using EideticMemoryOverlay.PluginApi.Buttons;
using EideticMemoryOverlay.PluginApi.LocalCards;
using Emo.Common.Enums;
using System;

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
        /// Type type used for local cards
        /// </summary>
        Type LocalCardType { get; }

        /// <summary>
        /// Create a card info button using plugin specific logic
        /// </summary>
        /// <param name="cardInfo">card info that this button represents</param>
        /// <param name="cardGroupId">card group that this buttton belongs to- used by plug ins to determine contextual logic for right click options</param>
        /// <returns>Card info button with options determined by the plugin</returns>
        CardInfoButton CreateCardInfoButton(CardInfo cardInfo, CardGroupId cardGroupId);
    }
    
    /// <summary>
    /// Plugins must inherit from this class to provide an entry point to the application
    /// </summary>
    public abstract class PlugIn : IPlugIn {
        protected PlugIn(string name) {
            Name = name;
        }

        /// <summary>
        /// initialize the plugin
        /// </summary>
        public abstract void SetUp();

        /// <summary>
        /// Displayed name of the plug in
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Type type used for local cards
        /// </summary>
        public virtual Type LocalCardType { get { return typeof(LocalCard); } }

        /// <summary>
        /// Create a card info button using plugin specific logic
        /// </summary>
        /// <param name="cardInfo">card info that this button represents</param>
        /// <param name="cardGroupId">card group that this buttton belongs to- used by plug ins to determine contextual logic for right click options</param>
        /// <returns>Card info button with options determined by the plugin</returns>
        public virtual CardInfoButton CreateCardInfoButton(CardInfo cardInfo, CardGroupId cardGroupId) {
            return new CardInfoButton(cardInfo);
        }
    }
}
