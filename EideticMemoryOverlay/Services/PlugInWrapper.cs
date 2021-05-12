using EideticMemoryOverlay.PluginApi;
using EideticMemoryOverlay.PluginApi.Buttons;
using EideticMemoryOverlay.PluginApi.Interfaces;
using EideticMemoryOverlay.PluginApi.LocalCards;
using Emo.Common.Enums;
using StructureMap;
using System;
using System.Collections.Generic;

namespace Emo.Services {
    internal class PlugInWrapper : IPlugIn {
        private PlugIn _plugIn;
        internal void SetPlugIn(PlugIn plugIn) {
            _plugIn = plugIn;
        }

        public void SetUp(IContainer container) {
            if (_plugIn == default) {
                return;
            }

            _plugIn.SetUp(container);
        }

        public string Name => _plugIn == default ? string.Empty : _plugIn.Name;

        public Type LocalCardType => _plugIn == default ? typeof(LocalCard) : _plugIn.LocalCardType;

        public IList<Pack> Packs => _plugIn == default ? new List<Pack>() : _plugIn.Packs;

        public string LocalImagesDirectory {
            get => _plugIn == default ? string.Empty : _plugIn.LocalImagesDirectory;
            set {
                if (_plugIn != null) {
                    _plugIn.LocalImagesDirectory = value;
                }
            }
        }

        public CardInfoButton CreateCardInfoButton(CardInfo cardInfo, CardGroupId cardGroupId) {
            if (_plugIn == default) {
                return default;
            }

            return _plugIn.CreateCardInfoButton(cardInfo, cardGroupId);
        }

        public EditableLocalCard CreateEditableLocalCard() {
            if (_plugIn == default) {
                return default;
            }

            return _plugIn.CreateEditableLocalCard();
        }

        public Player CreatePlayer(ICardGroup cardGroup) {
            if (_plugIn == default) {
                return default;
            }

            return _plugIn.CreatePlayer(cardGroup);
        }

        public void LoadAllPlayerCards() {
            if (_plugIn == default) {
                return;
            }

            _plugIn.LoadAllPlayerCards();
        }

        public void LoadEncounterCards() {
            if (_plugIn == default) {
                return;
            }

            _plugIn.LoadEncounterCards();
        }

        public void LoadPlayer(Player player) {
            if (_plugIn == default) {
                return;
            }

            _plugIn.LoadPlayer(player);
        }

        public void LoadPlayerCards(Player player) {
            if (_plugIn == default) {
                return;
            }

            _plugIn.LoadPlayerCards(player);
        }

        public ILocalCardEditor CreateLocalCardEditor() {
            if (_plugIn == default) {
                return default;
            }

            return _plugIn.CreateLocalCardEditor();
        }
    }
}
