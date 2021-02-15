using ArkhamOverlay.CardButtons;
using ArkhamOverlay.Data;
using PageController;
using System;
using System.Windows;
using System.Windows.Input;

namespace ArkhamOverlay.Pages.SelectCards {
    public class SelectPlayerMenuViewModel : ViewModel {

        public SelectPlayerMenuViewModel(Game game, CardTemplateButton cardTemplateButton) {
            AddCardToPlayer1 = new AddCardToPlayerCommand(game.Players[0], cardTemplateButton);
            AddCardToPlayer2 = new AddCardToPlayerCommand(game.Players[1], cardTemplateButton);
            AddCardToPlayer3 = new AddCardToPlayerCommand(game.Players[2], cardTemplateButton);
            AddCardToPlayer4 = new AddCardToPlayerCommand(game.Players[3], cardTemplateButton);
        }

        public ICommand AddCardToPlayer1 { get; }
        public ICommand AddCardToPlayer2 { get; }
        public ICommand AddCardToPlayer3 { get; }
        public ICommand AddCardToPlayer4 { get; }
    }

    public class AddCardToPlayerCommand : ICommand {
        private Player _player;
        private readonly CardTemplateButton _cardTemplateButton;

        public AddCardToPlayerCommand(Player player, CardTemplateButton cardTemplateButton) {
            _player = player;
            _cardTemplateButton = cardTemplateButton;
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter) {
            return true;
        }

        public void Execute(object parameter) {
            _player.CardGroup.CardZone.AddCard(_cardTemplateButton.CardTemplate, _cardTemplateButton.IsToggled);
        }

        public Visibility Visibility { get { return string.IsNullOrEmpty(_player.Name) ? Visibility.Collapsed : Visibility.Visible; } }

        public string Text { get { return "Add to " + _player.Name + "'s hand"; } }
    }
}
