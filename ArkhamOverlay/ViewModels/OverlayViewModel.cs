using ArkhamOverlay.Data;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace ArkhamOverlay.ViewModels {
    public class OverlayViewModel : INotifyPropertyChanged {
        public OverlayViewModel(AppData appData) {
            Configuration = appData.Configuration;
            ActAgendaCards = new ObservableCollection<CardViewModel>();
            EncounterCards = new ObservableCollection<CardViewModel>();
            PlayerCards = new ObservableCollection<CardViewModel>();

            appData.Configuration.PropertyChanged += (s, e) => {
                if (e.PropertyName == nameof(Configuration.UseActAgendaBar)) {
                    MoveActAgendaCards(appData.Configuration.UseActAgendaBar);
                }
            };
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string propertyName) {
            var handler = PropertyChanged;
            if (handler == null) {
                return;
            }
            handler(this, new PropertyChangedEventArgs(propertyName));
        }

        public Configuration Configuration { get; }
        public ObservableCollection<CardViewModel> ActAgendaCards { get; set; }
        public ObservableCollection<CardViewModel> EncounterCards { get; set; }
        public ObservableCollection<CardViewModel> PlayerCards { get; set; }

        private bool _showActAgendaBar;
        public bool ShowActAgendaBar { get => _showActAgendaBar; 
            set { 
                _showActAgendaBar = value;
                OnPropertyChanged(nameof(ShowActAgendaBar));
            }
        }

        private void MoveActAgendaCards(bool useActAgendaBar) {
            var sourceCards = ActAgendaCards;
            var destinationCards = EncounterCards;
            if (useActAgendaBar) {
                sourceCards = EncounterCards;
                destinationCards = ActAgendaCards;
            }

            var cardsToMove = new List<CardViewModel>();
            foreach (var cardViewModel in sourceCards) {
                cardsToMove.Add(cardViewModel);
            }

            foreach (var cardToMove in cardsToMove) {
                sourceCards.Remove(cardToMove);
                destinationCards.Add(cardToMove);
            }
        }
    }
}