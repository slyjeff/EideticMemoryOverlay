using ArkhamOverlay.CardButtons;
using ArkhamOverlay.Data;
using PageController;
using System;
using System.Windows;
using System.Windows.Input;

namespace ArkhamOverlay.Pages.SelectCards {
    public class SelectPlayerMenuViewModel : ViewModel {

        public SelectPlayerMenuViewModel(Game game, ShowCardButton showCardButton) {
            AddCardToPlayer1 = new AddCardToPlayerCommand(game.Players[0], showCardButton.Card);
            AddCardToPlayer2 = new AddCardToPlayerCommand(game.Players[1], showCardButton.Card);
            AddCardToPlayer3 = new AddCardToPlayerCommand(game.Players[2], showCardButton.Card);
            AddCardToPlayer4 = new AddCardToPlayerCommand(game.Players[3], showCardButton.Card);
        }

        public ICommand AddCardToPlayer1 { get; }
        public ICommand AddCardToPlayer2 { get; }
        public ICommand AddCardToPlayer3 { get; }
        public ICommand AddCardToPlayer4 { get; }
    }

    public class AddCardToPlayerCommand : ICommand {
        private Player _player;
        private readonly Card _card;

        public AddCardToPlayerCommand(Player player, Card card) {
            _player = player;
            _card = card;
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter) {
            return true;
        }

        public void Execute(object parameter) {
            _player.SelectableCards.CardSet.AddCard(_card);
        }

        public Visibility Visibility { get { return string.IsNullOrEmpty(_player.Name) ? Visibility.Collapsed : Visibility.Visible; } }

        public string Text { get { return "Add card to " + _player.Name; } }
    }
}
