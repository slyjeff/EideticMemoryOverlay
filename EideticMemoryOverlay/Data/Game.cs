using EideticMemoryOverlay.PluginApi;
using EideticMemoryOverlay.PluginApi.Buttons;
using Emo.Common.Enums;
using Emo.Common.Services;
using Emo.Common.Utils;
using Emo.Events;
using Emo.Services;
using PageController;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace Emo.Data {
    public class Game : ViewModel, IGame, IGameData {
        private readonly IEventBus _eventBus;
        private readonly LoggingService _logger;

        public Game(IEventBus eventBus, LoggingService logger) {
            _eventBus = eventBus;
            _logger = logger;

            Players = new List<Player>();
            _eventBus.SubscribeToButtonClickRequest(ButtonClickRequestHandler);
        }

        /// <summary>
        /// Setup the game using plugin specific logic
        /// </summary>
        /// <param name="plugIn">The plugin to use</param>
        public void InitializeFromPlugin(IPlugIn plugIn) {
            PlugInName = plugIn.GetType().Assembly.GetName().Name;
            Players = new List<Player> { 
                plugIn.CreatePlayer(new CardGroup(CardGroupId.Player1, plugIn)), 
                plugIn.CreatePlayer(new CardGroup(CardGroupId.Player2, plugIn)),
                plugIn.CreatePlayer(new CardGroup(CardGroupId.Player3, plugIn)), 
                plugIn.CreatePlayer(new CardGroup(CardGroupId.Player4, plugIn))};
            EncounterSets = new List<EncounterSet>();
            LocalPacks = new List<string>();
            ScenarioCards = new CardGroup(CardGroupId.Scenario, plugIn);
            ScenarioCards.AddCardZone(new CardZone("Act/Agenda Bar", CardZoneLocation.Top));
            LocationCards = new CardGroup(CardGroupId.Locations, plugIn);
            EncounterDeckCards = new CardGroup(CardGroupId.EncounterDeck, plugIn);
        }

        public string PlugInName { get; set; }

        public string FileName { get; set; }

        private string _name;
        public string Name { get => _name; 
            set {
                _name = value;
                NotifyPropertyChanged(nameof(Name));
            }
        }

        private string _scenario;
        public string Scenario {
            get => _scenario;
            set {
                _scenario = value;
                NotifyPropertyChanged(nameof(Scenario));
            }
        }
        
        private string _snapshotDirectory;
        public string SnapshotDirectory {
            get => _snapshotDirectory;
            set {
                _snapshotDirectory = value;
                NotifyPropertyChanged(nameof(SnapshotDirectory));
            }
        }

        public IList<EncounterSet> EncounterSets { get; set;  }

        public IList<string> LocalPacks { get; set; }

        public ICardGroup ScenarioCards { get; private set; }

        public ICardGroup LocationCards { get; private set; }

        public ICardGroup EncounterDeckCards { get; private set; }

        public IList<Player> Players { get; protected set; }

        public event Action PlayersChanged;

        public Visibility EncounterCardOptionsVisibility { 
            get {
                return EncounterSets.Any() ? Visibility.Visible : Visibility.Collapsed;
            } 
        }

        public void ClearAllCardsLists() {
            foreach (var selectableCards in AllCardGroups) {
                selectableCards.ClearCards();
            }
        }

        public bool IsEncounterSetSelected(string code) {
            return EncounterSets.Any(x => x.Code == code);
        }

        public IList<ICardGroup> AllCardGroups {
            get {
                var allCardGroups = new List<ICardGroup> {
                    ScenarioCards,
                    LocationCards,
                    EncounterDeckCards
                };
                
                foreach (var player in Players) {
                    allCardGroups.Add(player.CardGroup);
                }
                return allCardGroups;
            }
        }

        /// <summary>
        /// Find the card Group using the ID
        /// </summary>
        /// <param name="cardGroupId">Unique ID for a group</param>
        /// <returns>The group matching the passed in ID</returns>
        public ICardGroup GetCardGroup(CardGroupId cardGroupId) {
            switch (cardGroupId) {
                case CardGroupId.Player1:
                    return Players[0].CardGroup;
                case CardGroupId.Player2:
                    return Players[1].CardGroup;
                case CardGroupId.Player3:
                    return Players[2].CardGroup;
                case CardGroupId.Player4:
                    return Players[3].CardGroup;
                case CardGroupId.Scenario:
                    return ScenarioCards;
                case CardGroupId.Locations:
                    return LocationCards;
                case CardGroupId.EncounterDeck:
                    return EncounterDeckCards;
                default:
                    return ScenarioCards;
            }
        }

        /// <summary>
        /// Add a card (create a button) to a zone
        /// </summary>
        /// <param name="cardGroupId">Add to a zone of this card group</param>
        /// <param name="zoneIndex">Add to this zone</param>
        /// <param name="button">Source button for creating this new button</param>
        public void AddCardToZone(CardGroupId cardGroupId, int zoneIndex, CardImageButton button) {
            var destinationCardGroup = GetCardGroup(cardGroupId);
            var destinationCardZone = destinationCardGroup.GetCardZone(zoneIndex);
            if (destinationCardZone == default) {
                _logger.LogError($"Cannot add card {button.CardInfo.Name} to {destinationCardGroup.Name} because zone with index {zoneIndex} does not exist");
                return;
            }

            _logger.LogMessage($"Adding card {button.CardInfo.Name} to {destinationCardZone.Name} of {destinationCardGroup.Name} ");
            destinationCardZone.CreateCardButton(button, CreateButtonOptions(destinationCardGroup, destinationCardZone, button.CardInfo));
        }

        /// <summary>
        /// When a button is clicked, look at the context, see if it's a button in this card group, and execute the appropriate logic
        /// </summary>
        /// <param name="eventData">Event specific information</param>
        private void ButtonClickRequestHandler(ButtonClickRequest eventData) {
            _logger.LogMessage("Handling button click request");
            try {
                var cardGroup = GetCardGroup(eventData.CardGroupId);

                var button = cardGroup.GetButton(eventData);
                if (button == default(Button)) {
                    return;
                }

                if (eventData.MouseButton == MouseButton.Left) {
                    HandleButtonLeftClick(cardGroup, button);
                } else {
                    HandleButtonRightClick(cardGroup, button as CardImageButton, eventData.ButtonOption);
                }
            } catch (Exception exception) {
                _logger.LogException(exception, "Error handling button click");
            }
        }

        /// <summary>
        /// Execute the appropriate left click logic for a button
        /// </summary>
        /// <param name="cardGroup">The Card Group this card was clicked in</param>
        /// <param name="button">The button clicked</param>
        private void HandleButtonLeftClick(ICardGroup cardGroup, IButton button) {
            if (button is CardImageButton cardImageButton) {
                _logger.LogMessage($"Requesting toggle for {cardImageButton.CardInfo.Name} visibility");
                _eventBus.PublishToggleCardInfoVisibilityRequest(cardImageButton.CardInfo);
                return;
            }

            if (button is ShowCardZoneButton showCardZoneButton) {
                _logger.LogMessage($"Requesting toggle for {cardGroup.Name} {showCardZoneButton.CardZone.Name} visibility");
                _eventBus.PublishToggleCardZoneVisibilityRequest(showCardZoneButton.CardZone);
                return;
            }

            if (button is ClearButton) {
                _logger.LogMessage($"Requesting clear all cards for {cardGroup.Name} visibility");
                _eventBus.PublishClearAllCardsForCardGroupRequest(cardGroup);
                return;
            }
        }

        /// <summary>
        /// Execute the appropriate right click logic for a button
        /// </summary>
        /// <param name="cardGroup">The Card Group this card was clicked in</param>
        /// <param name="button">The button clicked</param>
        /// <param name="buttonOption">Option the user selected from a right click menu, if applicable</param>
        private void HandleButtonRightClick(ICardGroup cardGroup, CardImageButton button, ButtonOption buttonOption) {
            //button should always be set- if it's not, we have an issue
            if (button == null) {
                _logger.LogError($"Right Button Click for {cardGroup.Name} was not a button with an image");
                return;
            }

            //button option should always be set- if it's not, we have an issue
            if (buttonOption == null) {
                _logger.LogError($"Right Button Click for {cardGroup.Name} had no option selected");
                return;
            }

            if (buttonOption.Operation == ButtonOptionOperation.Remove) {
                cardGroup.RemoveCard(button as CardButton);
                return;
            }

            if (buttonOption.Operation == ButtonOptionOperation.Move) {
                cardGroup.RemoveCard(button as CardButton);
            }

            //whether add or move, we need to add the card to the specified zone
            AddCardToZone(buttonOption.CardGroupId, buttonOption.ZoneIndex, button);
        }

        /// <summary>
        /// Create the right click options for a button
        /// </summary>
        /// <param name="cardGroup">Card group that contains the button</param>
        /// <param name="destinationCardZone">Card Zone that contains the button</param>
        /// <param name="cardInfo">Information bout the card for this button</param>
        /// <returns>Options to assign to a right click button</returns>
        private IEnumerable<ButtonOption> CreateButtonOptions(ICardGroup cardGroup, CardZone destinationCardZone, CardInfo cardInfo) {
            var options = new List<ButtonOption>();

            options.Add(new ButtonOption(ButtonOptionOperation.Remove));

            var cardZones = cardGroup.CardZones;
            foreach (var cardZone in cardZones) {
                if (cardZone != destinationCardZone) {
                    options.Add(new ButtonOption(ButtonOptionOperation.Move, cardGroup.Id, cardZones.IndexOf(cardZone)));
                }
            }

            return options;
        }
    }
}
